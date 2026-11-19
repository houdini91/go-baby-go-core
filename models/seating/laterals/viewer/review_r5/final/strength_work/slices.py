# 2D plane-stress slices (per mm of z) of the wing tip and the carrier tongue, loaded with the contact-model forces.
import numpy as np, pickle, math, json, sys
from matplotlib.path import Path
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
from geom import *
import fe2d
import loads as L
H = 0.1
def in_any(loops, P):
    cnt = np.zeros(len(P), int)
    for lp in loops: cnt += Path(lp).contains_points(P)
    return cnt % 2 == 1
def wing_mask(latch, x0=-14, x1=30, y0=1.6, y1=40):
    js = json.load(open('wing_sections.json'))['244.0']
    loops = [np.array(l) for l in js]
    xs = np.arange(x0, x1, H) + H / 2; ys = np.arange(y0, y1, H) + H / 2
    X, Y = np.meshgrid(xs, ys, indexing='ij'); P = np.c_[X.ravel(), Y.ravel()]
    m = in_any(loops, P).reshape(X.shape)
    m &= ~Path(np.array(SLOT)).contains_points(P).reshape(X.shape)
    # mouth chamfer: t' < 5.3 -> slot widened by (5.3 - t')
    e = np.clip(MOUTH + T_BACK - Y, 0, None)
    m &= ~((Y < MOUTH + T_BACK) & (X >= 3.55 - e) & (X <= 17.65 + e))
    if latch:
        wallT = 17.5 - (GR_T - 3.0) * 0.0262
        G = [[16.85, 3.0], [wallT, 3.0], [wallT, 4.0], [wallT + 0.5, GR_T + (wallT + 0.5 - 17.65) * GK], [22.2, GR_T + (22.2 - 17.65) * GK], [22.2, 9.0], [16.85, 9.0]]
        m &= ~Path(np.array(G)).contains_points(P).reshape(X.shape)
    m &= Y >= T_BACK
    return m, X, Y, x0, y0
def carrier_mask(latch, x0=-5, x1=26, y0=-14.0, y1=24):
    xs = np.arange(x0, x1, H) + H / 2; ys = np.arange(y0, y1, H) + H / 2
    X, Y = np.meshgrid(xs, ys, indexing='ij'); P = np.c_[X.ravel(), Y.ravel()]
    tg = Path(np.array(TONGUE)).contains_points(P).reshape(X.shape) & (Y >= T_BACK - SEAM)
    nose = (Y < T_BACK - SEAM) & (X >= -4.4) & (X <= 24.5)
    m = tg | nose
    # R3 root fillet: fill the corners at the nose back face (approx: quarter-circle fillets on both tongue sides)
    for xc, s in ((3.75, -1), (17.45, 1)):
        r = 3.0; cx = xc + s * r; cy = T_BACK - SEAM + r
        fil = (Y >= T_BACK - SEAM) & (Y <= cy) & ((X - xc) * s >= 0) & ((X - xc) * s <= r) & ((X - cx) ** 2 + (Y - cy) ** 2 >= r ** 2)
        m |= fil
    if latch:
        m &= ~((X >= 7.9) & (X <= 17.45) & (Y >= -4.3) & (Y <= 9.3))
        m &= ~((X >= 12.05) & (Y >= 2.7) & (Y <= 9.3))
        m &= ~((np.abs(Y - BT_TC) <= 6.7) & (X >= 8.0) & (Y < T_BACK - SEAM))       # cap / connector channel
    return m, X, Y, x0, y0
def band_loads(rows_case, zlo, zhi, sign, groups, tooth_on_carrier=False):
    """per-mm line loads from contact points in z band, worst config row passed in"""
    r = rows_case; f, A, names, grp = r['fpt'], r['A'], r['names'], r['grp']; out = []
    for gi, n in enumerate(names):
        if n not in groups: continue
        s = np.nonzero(grp == gi)[0]
        for k in s:
            a = A[k]
            if not (zlo <= a[2] <= zhi) or f[k] <= 0: continue
            fv = sign * f[k] * a[3:5] / (zhi - zlo)
            p = a[:2]
            if n == 'tooth (latch)' and tooth_on_carrier: p = np.array([np.clip(a[0] - 5.1, 13.2, 17.0), BLK_T[1] + 0.3 + 0.05])
            out.append((p[0], p[1], fv[0], fv[1]))
    return out
def fi_wing(S):   # wing printed z-up: d-t is the layer plane -> in-plane allowables
    sx, sy, txy = S[..., 0], S[..., 1], S[..., 2]
    s1 = (sx + sy) / 2 + np.sqrt(((sx - sy) / 2) ** 2 + txy ** 2); s2 = (sx + sy) / 2 - np.sqrt(((sx - sy) / 2) ** 2 + txy ** 2)
    return np.maximum(np.abs(s1), np.abs(s2)) / MAT['Xa']
def fi_carrier(S):   # carrier printed on the nose outer face: layer normal = d (x here) -> sigma_dd across, tau_dt interlaminar
    sx, sy, txy = S[..., 0], S[..., 1], S[..., 2]
    return np.sqrt((np.clip(sx, 0, None) / MAT['Xc']) ** 2 + (sy / MAT['Xa']) ** 2 + (txy / 18.0) ** 2 + (np.clip(-sx, 0, None) / 40.0) ** 2)
def peak(FI, m, X, Y, box, q=99.5):
    sel = m & (X >= box[0]) & (X <= box[1]) & (Y >= box[2]) & (Y <= box[3])
    v = FI[sel]; v = v[np.isfinite(v)]
    return np.percentile(v, q) if len(v) else 0.0
def run_case(rows, which, zband, latch, plot=None):
    groups_w = ('inner wall', 'outer wall', 'rear faces', 'tooth (latch)', 'nose back face')
    groups_c = ('inner wall', 'outer wall', 'rear faces', 'tooth (latch)')
    # worst row = max |tooth| + |outer| force
    r = max(rows, key=lambda r: r['res']['outer wall']['F'] + r['res']['tooth (latch)']['F'] + r['res']['inner wall']['F'] * 0.3)
    res = {}
    if which in ('wing', 'both'):
        m, X, Y, x0, y0 = wing_mask(latch)
        ld = band_loads(r, *zband, -1.0, groups_w)
        S, u, Xn, Yn, reac = fe2d.solve(m, x0, y0, H, MAT['E'], MAT['nu'], lambda x, y: y > 36.0, ld)
        FI = fi_wing(S)
        res['wing'] = dict(
            shoulder_skin=peak(FI, m, X, Y, (18.0, 24.7, 1.9, 9.6)),
            outer_wall=peak(FI, m, X, Y, (17.3, 24.7, 2.0, 16)),
            inner_web=peak(FI, m, X, Y, (-5, 4.0, 2.0, 30)),
            blade=peak(FI, m, X, Y, (-14, 30, 16, 34)))
        if plot: _plot(S, FI, m, X, Y, ld, plot + '_wing.png', 'wing slice: failure index (in-plane)')
    if which in ('carrier', 'both'):
        m, X, Y, x0, y0 = carrier_mask(latch)
        ld = band_loads(r, *zband, +1.0, groups_c, tooth_on_carrier=True)
        S, u, Xn, Yn, reac = fe2d.solve(m, x0, y0, H, MAT['E'], MAT['nu'], lambda x, y: y < -10.0, ld)
        FI = fi_carrier(S)
        res['carrier'] = dict(
            tongue_root=peak(FI, m, X, Y, (3.0, 18.5, 0.0, 6.0)),
            rear_wall=peak(FI, m, X, Y, (7.9, 17.5, 9.2, 16)),
            whole=peak(FI, m, X, Y, (-5, 26, -9, 24)))
        if plot: _plot(S, FI, m, X, Y, ld, plot + '_carrier.png', 'carrier slice: failure index (layers normal to d)')
    res['cfg'] = r['cfg']; res['F_tooth'] = r['res']['tooth (latch)']['F']; res['F_outer'] = r['res']['outer wall']['F']
    return res
def _plot(S, FI, m, X, Y, ld, fn, title):
    fig, ax = plt.subplots(figsize=(7, 8))
    F = np.where(m, FI, np.nan)
    im = ax.pcolormesh(X, Y, F, vmin=0, vmax=2, cmap='turbo', shading='auto'); plt.colorbar(im, ax=ax, label='failure index (1 = fail; SF = 1/FI)')
    for (x, y, fx, fy) in ld: ax.arrow(x, y, -fx * 0.004, -fy * 0.004, color='w', width=0.05, length_includes_head=True)
    ax.set_aspect('equal'); ax.set_xlabel("d' (mm, + outboard)"); ax.set_ylabel("t' (mm, + into the wing)"); ax.set_title(title, fontsize=9)
    plt.savefig(fn, dpi=110, bbox_inches='tight'); plt.close(fig)
if __name__ == '__main__':
    gm = sys.argv[1] if len(sys.argv) > 1 else 'zero'
    C_ = pickle.load(open(f'cases_{gm}.pkl', 'rb')); out = {}
    for cn in ('lean 300 (centre)', 'proof 600 (centre)', 'knock 900 out (corner)', 'knock 900 in (corner)', 'pull fwd 300 (centre)'):
        lo = run_case(C_[cn], 'both', (TG_Z[0], 222.9), False, plot=f'fig_{gm}_{cn.split()[0]}{cn.split()[1]}_low' if cn.startswith(('lean', 'knock 900 out')) else None)
        hi = run_case(C_[cn], 'both', (222.9, TG_Z[1]), True, plot=f'fig_{gm}_{cn.split()[0]}{cn.split()[1]}_latch' if cn.startswith(('lean', 'knock 900 out')) else None)
        out[cn] = dict(low=lo, latch=hi)
        print(f"\n{cn}  [{lo['cfg']}]  F_tooth {lo['F_tooth']:.0f}  F_outer {lo['F_outer']:.0f}")
        for nm, d in (('z 214-223 (below latch)', lo), ('z 223-246 (latch zone)', hi)):
            print(f"  {nm}: wing  " + '  '.join(f'{k} SF {1/v:4.2f}' for k, v in d['wing'].items() if v > 0) +
                  "\n                           carrier " + '  '.join(f'{k} SF {1/v:4.2f}' for k, v in d['carrier'].items() if v > 0))
    pickle.dump(out, open(f'slices_{gm}.pkl', 'wb'))
