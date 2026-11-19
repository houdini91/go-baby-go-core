# r5b strength check (builder's), reusing the strength reviewer's contact model and arm/tenon methods from
# ../../final/strength_work (loads.solve / wrench / pad_points, arm.tsai_hill_X, Winkler K 400 N/mm3, allowables 40/18/22/25).
# Geometry = featurescript/gbg_sidepad_r5.fs r5b constants. Run with viewer/.venv python from this folder.
import sys, math, json, types, numpy as np
SW = '../../final/strength_work'; sys.path.insert(0, SW)
import geom as G
# ---- r5b constants (FS)
G.TG_Z = (214.3, 246.4); G.TONGUE = [[2.0, 1.0], [17.8, 1.0], [17.8, 25.48], [9.0, 25.48], [2.2, 16.57], [2.0, 8.5]]
G.ARM_W, G.ARM_H, G.ARM_R, G.GROW, G.ROOT_R = 22.0, 31.0, 10.5, 3.0, 2.5; G.YAX = G.YEND + G.GROW
G.TEN_W, G.TEN_H, G.TEN_L, G.TEN_R, G.JS_DX = 16.0, 24.0, 15.0, 3.0, 9.5
G.TPIN = [[4.5, 9.0], [-4.5, 9.0], [4.5, -9.0], [-4.5, -9.0]]; G.PIN_L = 29.0
G.FOOT_HZ, G.FOOT_R, G.FLARE_L = 24.0, 10.0, 22.0
sys.modules['geom'] = G
from geom import *
import loads as L
import arm as AR
MOUTH_IN, MOUTH_OUT = 3.3, 0.5
BAR_Z0 = 241.2                       # bar channel (open to the outer face) from here up, d' >= 8.8, t' <= 21.1
TCH = (16.7, 21.1); TO_Z = (216.0, 245.5); GR_T = 17.0; TO_D1 = 20.4; WALL = 18.0
def patches(gm='cad'):
    g = (lambda v: v) if gm == 'cad' else (lambda v: 0.0)
    P = {}
    inner = L.seg_pts([2.0, T_BACK + MOUTH_IN], [2.0, 8.5], 0, 99, *TG_Z, (1, 0, 0), g(0.2), ns=4) + \
            L.seg_pts([2.0, 8.5], [2.2, 16.57], 0, 99, *TG_Z, (*L.nrm([2.0, 8.5], [2.2, 16.57], 1), 0), lambda p: 0.0 if min(abs(p[1] - 12), abs(p[1] - 14)) < 0.45 else g(0.2), ns=16)
    P['inner wall'] = inner
    ow = L.seg_pts([17.8, T_BACK + MOUTH_OUT], [17.8, TCH[0]], 0, 99, TG_Z[0], BAR_Z0, (-1, 0, 0), lambda p: 0.0 if min(abs(p[1] - 8), abs(p[1] - 10)) < 0.45 else g(0.2), ns=24, nz=12) + \
         L.seg_pts([17.8, TCH[1]], [17.8, 25.48], 0, 99, TG_Z[0], BAR_Z0, (-1, 0, 0), g(0.2), ns=6, nz=12)
    P['outer wall'] = ow
    rear = L.seg_pts([17.8, 25.48], [9.0, 25.48], 0, 99, *TG_Z, (0, -1, 0), g(0.2), ns=8, nz=8) + \
           L.seg_pts([9.0, 25.48], [2.2, 16.57], 0, 99, *TG_Z, (*(-L.nrm([9.0, 25.48], [2.2, 16.57], 1)), 0), g(0.2), ns=8, nz=8)
    P['rear faces'] = rear
    P['tooth (latch)'] = [(d, GR_T + (d - WALL) * L.GK, z, 0.0, 1.0, 0.0, g(0.12), (TO_D1 - WALL) / 6 * (TO_Z[1] - TO_Z[0]) / 12)
                          for d in np.linspace(WALL + 0.2, TO_D1 - 0.2, 6) for z in np.linspace(TO_Z[0] + 1.2, TO_Z[1] - 1.2, 12)]
    nf = []
    for (d, z) in ((-1.8, 230.0), (21.3, 222.0), (10.0, 209.2)):
        nf.append((d, T_BACK - SEAM, z, 0.0, -1.0, 0.0, g(0.02), 7.07))
    def nose_face(d, z):
        inside = -4.5 <= d <= (24.6 if z > 215.0 else 18.0)
        mouth = (2.0 - MOUTH_IN <= d <= 18.0 + MOUTH_OUT) and (TG_Z[0] - CLR - MOUTH_IN <= z)
        return g(0.3) if inside and not mouth else None
    for (d, z, *_r, gg, a) in L.area_pts(nose_face, -4.5, 24.6, NZ[0] + SEAM, NZ[1] - SEAM, 0.0, (0, 0, 0), 0, nd=40, nt=40):
        nf.append((d, T_BACK - SEAM, z, 0.0, -1.0, 0.0, gg, a))
    P['nose back face'] = nf
    TG = G.TONGUE
    def bot(d, t):
        if t < T_BACK + MOUTH_IN or not L.in_poly(TG, d, t): return None
        if min(abs(t - 14), abs(t - 16)) < 0.5 and 3.0 <= d <= 8.0: return 0.0
        if TCH[0] <= t <= TCH[1] and d >= 8.8: return None            # tooth pocket floor is tongue, but no slot contact under the pocket? keep
        return g(0.5) if 5.5 <= t <= 13.5 else g(0.2)
    def top(d, t):
        if t < T_BACK + MOUTH_IN or not L.in_poly(TG, d, t): return None
        if d >= 8.8 and t <= TCH[1]: return None
        if min(abs(t - 14), abs(t - 16)) < 0.5 and 3.0 <= d <= 8.0: return 0.0
        return g(0.5) if (5.5 <= t <= 13.5 and d <= 8.4) else g(0.2)
    P['tongue bottom'] = L.area_pts(bot, 2.0, 17.8, T_BACK + MOUTH_IN, 25.48, TG_Z[0], (0, 0, 1.0), 0)
    P['tongue top'] = L.area_pts(top, 2.0, 17.8, T_BACK + MOUTH_IN, 25.48, TG_Z[1], (0, 0, -1.0), 0)
    def nb(d, t): return g(0.3) if (-4.5 <= d <= 18.0 and T_FRONT <= t <= T_BACK - SEAM) else None
    def nt(d, t):
        if not (-4.5 <= d <= 24.6 and T_FRONT <= t <= T_BACK - SEAM): return None
        if 8.8 <= d <= 18.5 and -4.3 <= t: return None
        if abs(t - BT_TC) <= 7.0 and d >= 8.0: return None
        return g(0.3)
    P['nose bottom'] = L.area_pts(nb, -4.5, 18.0, T_FRONT, T_BACK - SEAM, NZ[0] + SEAM, (0, 0, 1.0), 0)
    P['nose top'] = L.area_pts(nt, -4.5, 24.6, T_FRONT, T_BACK - SEAM, NZ[1] - SEAM, (0, 0, -1.0), 0)
    return {k: np.array(v, float) for k, v in P.items()}
# ---------------- tongue section at the mouth (t' = 2.0, the root behind the nose face)
H = 0.05
def tongue_mask(t_s):
    d = np.arange(0, 25, H) + H / 2; z = np.arange(210, 250, H) + H / 2; D, Z = np.meshgrid(d, z, indexing='ij')
    lo = 2.0 if t_s <= 8.5 else 2.0 + 0.2 * (t_s - 8.5) / 8.07
    m = (D >= lo) & (D <= 17.8) & (Z >= TG_Z[0]) & (Z <= TG_Z[1])
    m &= ~((D >= 8.8) & (Z >= BAR_Z0))
    if TCH[0] <= t_s <= TCH[1]: m &= ~((D >= 8.8) & (Z >= TO_Z[0] - 0.4))
    return D, Z, m
def tongue_sf(res_rows, t_s=2.0, kt_out=1.8, kt=1.3):
    D, Z, m = tongue_mask(t_s); A = m.sum() * H * H; dc = D[m].mean(); zc = Z[m].mean()
    dd = D[m] - dc; zz = Z[m] - zc; Idd = (zz ** 2).sum() * H * H; Izz = (dd ** 2).sum() * H * H; Idz = (dd * zz).sum() * H * H
    worst = None
    for (f, Apts, names, grp) in res_rows:
        F = np.zeros(3); M = np.zeros(3)
        for gi, n in enumerate(names):
            if n not in ('inner wall', 'outer wall', 'rear faces', 'tooth (latch)', 'tongue bottom', 'tongue top'): continue
            s = grp == gi
            for fi, row in zip(f[s], Apts[s]):
                if fi <= 0 or row[1] <= t_s: continue
                v = fi * row[3:6]; r = np.array([row[0] - dc, row[1] - t_s, row[2] - zc])
                F += v; M += np.cross(r, v)
        # section normal = t; bending: M_d (about d) -> sigma ~ z ; M_z (about z) -> sigma ~ d
        Md, Mz = M[0], M[2]
        a_, b_ = np.linalg.solve([[Izz, Idz], [Idz, Idd]], [-Mz, Md])
        sig = F[1] / A + a_ * dd + b_ * zz
        k = np.where(dd > 0, kt_out, kt)
        smax = np.abs(k * sig).max()
        tau = 1.5 * math.hypot(F[0], F[2]) / A + abs(M[1]) / (0.25 * 15.8 ** 2 * 32.1)
        FI = math.sqrt((smax / MAT['Xa']) ** 2 + (tau / MAT['S']) ** 2)
        if worst is None or FI > worst[0]: worst = (FI, smax, tau, M / 1000)
    return 1 / worst[0], worst, A, Izz / max(17.8 - dc, dc - 2.0)
# ---------------- arm: analytic rounded-rect sections (22 x 31 to mid-length, 30 after)
def rr_props(w, h, r, hh=0.1):
    xs = np.arange(-w / 2, w / 2, hh) + hh / 2; ys = np.arange(-h / 2, h / 2, hh) + hh / 2; X, Y = np.meshgrid(xs, ys, indexing='ij')
    m = np.ones_like(X, bool)
    for sx in (-1, 1):
        for sy in (-1, 1):
            cx, cy = sx * (w / 2 - r), sy * (h / 2 - r)
            m &= ~((sx * X > sx * cx) & (sy * Y > sy * cy) & ((X - cx) ** 2 + (Y - cy) ** 2 > r * r))
    return dict(A=m.sum() * hh * hh, cx=0.0, cy=0.0, Ixx=(Y[m] ** 2).sum() * hh * hh, Iyy=(X[m] ** 2).sum() * hh * hh, Ixy=0.0, X=X[m], Y=Y[m], w=w, hgt=h)
def arm_sf(arm, loads):
    zp = zp_of(arm, 'LOW'); p = armPath(C, zp); out = {}
    for u in [0.3, 4, 10, 20, 30, 40, p['L'] - 0.5]:
        p0, tn = armAt(p, u)
        n2 = np.array([0, 0, 1.0]) - tn[2] * tn; e2 = n2 / np.linalg.norm(n2); e1 = np.cross(e2, tn)
        Gp = rr_props(22.0, 31.0 if u <= p['L'] / 2 else 30.0, 10.5)
        sinth = abs(tn @ AR.N_LAYER); X = AR.tsai_hill_X(sinth)
        b, h_ = 22.0, Gp['hgt']; alpha = 0.208 + (0.246 - 0.208) * min((h_ / b - 1), 1)
        for nm, cfg, Pw, Fw in loads:
            if cfg[0] != arm: continue
            if cfg[1] == 'HIGH': Pw = AR.mirror_z(Pw); Fw = Fw * np.array([1, 1, -1])
            Mv = np.cross(Pw - p0, Fw); Nn = Fw @ tn; M1 = Mv @ e1; M2 = Mv @ e2; T = Mv @ tn; V = np.linalg.norm(Fw - Nn * tn)
            sig = np.abs(Nn / Gp['A'] - M2 * Gp['X'] / Gp['Iyy'] + M1 * Gp['Y'] / Gp['Ixx']).max()
            tau = abs(T) / (alpha * b * b * h_) + 1.5 * V / Gp['A']
            kt = 1.55 if u <= 1.0 else 1.0
            sf = 1 / math.sqrt((kt * sig / X) ** 2 + (tau / MAT['S']) ** 2)
            key = (round(u, 1), nm)
            if key not in out or sf < out[key]: out[key] = sf
    return out
# ---------------- tenon (the fuse) at the root x = xe and at the nut slot x = xe - 9.5
def tenon_sf(loads):
    hh = 0.05; y = np.arange(-8, 8, hh) + hh / 2; z = np.arange(-12, 12, hh) + hh / 2; Y, Z = np.meshgrid(y, z, indexing='ij')
    def mask(slot):
        m = np.ones_like(Y, bool); r = 3.0
        for sy in (-1, 1):
            for sz in (-1, 1):
                cy, cz = sy * (8 - r), sz * (12 - r)
                m &= ~((sy * Y > sy * cy) & (sz * Z > sz * cz) & ((Y - cy) ** 2 + (Z - cz) ** 2 > r * r))
        for py, pz in G.TPIN: m &= ~((Y - py) ** 2 + (Z - pz) ** 2 <= 1.15 ** 2)
        if slot: m &= ~((Y <= 1.5) & (np.abs(Z) <= 4.8)); m &= ~(np.abs(Z) <= 2.75)
        return m
    X = AR.tsai_hill_X(abs(np.array([-1.0, 0, 0]) @ AR.N_LAYER))
    pin_cap = math.pi * 2.0 * 16.0 * 4.0
    out = {}
    for slot, xoff, frac in ((False, 0.0, 1.0), (True, 9.5, 0.5)):
        m = mask(slot); A = m.sum() * hh * hh; cy = Y[m].mean(); cz = Z[m].mean()
        Iyy = ((Z[m] - cz) ** 2).sum() * hh * hh; Izz = ((Y[m] - cy) ** 2).sum() * hh * hh
        for nm, cfg, Pw, Fw in loads:
            zp = zp_of(cfg[0], 'LOW')
            if cfg[1] == 'HIGH': Pw = AR.mirror_z(Pw); Fw = Fw * np.array([1, 1, -1])
            xe = armAt(armPath(C, zp), armPath(C, zp)['L'])[0][0]
            cen = np.array([xe - xoff, YAX, zp]); M = np.cross(Pw - cen, Fw); My, Mz = M[1] * frac, M[2] * frac
            ry = 2 * pin_cap * 18.0 if not slot else 0; rz = 2 * pin_cap * 9.0 if not slot else 0
            sig = abs(Fw[0]) / A + max(abs(My) - ry, 0.6 * abs(My)) * 12 / Iyy + max(abs(Mz) - rz, 0.6 * abs(Mz)) * 8 / Izz
            tau = 1.5 * np.linalg.norm(Fw[1:]) / A + abs(M[0] * frac) / (0.231 * 16 ** 2 * 24)
            sf = 1 / math.sqrt((sig / X) ** 2 + (tau / 18.0) ** 2)
            k = ('tenon root' if not slot else 'tenon at nut slot', nm)
            if k not in out or sf < out[k]: out[k] = sf
    return out
def load_set(P):
    S = []
    for cfg in CONFIGS:
        pts = L.pad_points(*cfg)
        S.append((f'lean {P:.0f} centre', cfg, pts['centre'], np.array([P, 0, 0])))
        for c in pts['corners']:
            S.append((f'knock out {P:.0f} corner', cfg, c, np.array([P, 0, 0])))
            S.append((f'knock in {P:.0f} corner', cfg, c, np.array([-P, 0, 0])))
            S.append((f'knock back {P:.0f} corner', cfg, c, np.array([0, P, 0])))
        S.append((f'pull fwd {P:.0f} centre', cfg, pts['centre'], np.array([0, -P, 0])))
    return S
def mount_sf(loads, PT):
    out = {}
    rows_by = {}
    for nm, cfg, Pw, Fw in loads:
        f, M = L.wrench(Fw, Pw)
        res, u, err, fp = L.solve(f, M, PT)
        rows_by.setdefault(nm, []).append((res, fp))
    for nm, rr in rows_by.items():
        sf_t, w, A, Zy = tongue_sf([fp for _, fp in rr])
        sf_t1 = tongue_sf([fp for _, fp in rr], 2.0, 1.0, 1.0)[0]
        po = max(r['outer wall']['pmax'] for r, _ in rr); pi = max(r['inner wall']['pmax'] for r, _ in rr)
        ft = max(r['tooth (latch)']['F'] for r, _ in rr)
        ow_F = max(r['outer wall']['F'] for r, _ in rr)
        def mean_p(name):
            best = 0
            for r, (f, A, names, grp) in rr:
                gi = names.index(name); sel = (grp == gi) & (f > 0)
                if sel.any(): best = max(best, f[sel].sum() / A[sel, 7].sum())
            return best
        out[nm] = dict(tongue=sf_t, tongue_kt1=sf_t1, outer_wall_brg=25 / max(po, 1e-6), inner_wall_brg=25 / max(pi, 1e-6), tooth_F=ft,
                       tooth_brg=25 / max(ft / (2.4 * 29.5), 1e-6), shoulder=22 * 15.0 * 30.6 / max(ft, 1e-6),
                       pocket_wall=22 * 172 / max(ft, 1e-6), outer_wall_F=ow_F, outer_wall_mean=25 / max(mean_p('outer wall'), 1e-6), inner_wall_mean=25 / max(mean_p('inner wall'), 1e-6), M=w[3].round(1).tolist())
    return out
if __name__ == '__main__':
    PT = patches(sys.argv[1] if len(sys.argv) > 1 else 'zero')
    print('patch areas mm2', {k: round(float(v[:, 7].sum()), 1) for k, v in PT.items()})
    D, Z, m = tongue_mask(2.0); print('tongue mouth section area %.0f mm2' % (m.sum() * H * H))
    ALL = {}
    for P in (300.0, 450.0, 600.0, 900.0):
        Ls = load_set(P)
        ms = mount_sf(Ls, PT); arm = {}; [arm.update({(a,) + k: v for k, v in arm_sf(a, Ls).items()}) for a in ('S', 'L')]
        ten = tenon_sf(Ls)
        ALL[P] = dict(mount=ms, arm=min(arm.values()), arm_root=min(v for k, v in arm.items() if k[1] <= 1.0),
                      arm_by_case={nm: min(v for k, v in arm.items() if k[2] == nm) for nm in set(k[2] for k in arm)},
                      tenon={f'{k[0]} | {k[1]}': v for k, v in ten.items()})
        print(f'\n===== {P:.0f} N')
        for nm, r in ms.items():
            print(f"{nm:24s} tongue {r['tongue']:5.2f} (Kt 1: {r['tongue_kt1']:4.2f})  outer-wall brg {r['outer_wall_brg']:5.2f}  inner-wall brg {r['inner_wall_brg']:5.2f}  "
                  f"(mean {r['outer_wall_mean']:5.2f} / {r['inner_wall_mean']:5.2f})  tooth F {r['tooth_F']:5.0f} N (brg {r['tooth_brg']:5.1f}, shoulder {r['shoulder']:5.1f}, pocket wall {r['pocket_wall']:5.1f})  "
                  f"arm {ALL[P]['arm_by_case'][nm]:5.2f}  M_root {r['M']} N.m")
        for k, v in sorted(ten.items()): print(f'   {k[0]:18s} {k[1]:24s} SF {v:5.2f}')
    json.dump({str(k): v for k, v in ALL.items()}, open('sf_r5b_' + (sys.argv[1] if len(sys.argv) > 1 else 'zero') + '.json', 'w'), indent=1, default=float)
