# Pad load cases -> resultant at the tongue reference, then a rigid-carrier / unilateral-Winkler contact solve
# against the wing (CAD gaps from gbg_sidepad_r5.fs). Output: contact group forces, peak pressures, tooth load.
import numpy as np, math, json, sys
from geom import *
REF = np.array([10.6, 5.0, C])                     # tongue reference point (F frame ~ S at z 227)
K_BRG = 400.0                                      # N/mm^3 bearing foundation modulus (E 1900 / ~5 mm)

def pad_points(arm, kit, pp):
    zp = zp_of(arm, kit); zc = zp + pp * PAD_OFF
    xm = shellXi(14.0) - 12.0                        # mid foam (x only matters for y/z forces)
    return dict(zp=zp, zc=zc, centre=np.array([xm, 14.0, zc]),
                corners=[np.array([xm, 14.0 - 0.84 * 41.0, zc + s * 0.84 * 40.0]) for s in (+1, -1)] +
                        [np.array([xm, 14.0 + 0.84 * 41.0, zc + s * 0.84 * 40.0]) for s in (+1, -1)])

def wrench(Fw, Pw):
    """world force Fw at world point Pw -> (F, M) in F frame about REF (N, N.mm)"""
    r = toF(Pw) - REF; f = w2f_vec(np.asarray(Fw, float))
    return f, np.cross(r, f)

# ---------------- contact patches on the carrier: points (d', t', z) S frame, force direction on carrier, gap, area
def seg_pts(a, b, t_lo, t_hi, z0, z1, n_force, gap, nz=12, ns=10):
    a = np.array(a, float); b = np.array(b, float); out = []
    for s in np.linspace(0.05, 0.95, ns):
        p = a + s * (b - a)
        if not (t_lo <= p[1] <= t_hi): continue
        for z in np.linspace(z0, z1, nz + 1)[:-1] + (z1 - z0) / nz / 2:
            out.append((p[0], p[1], z, *n_force, gap(p) if callable(gap) else gap, np.linalg.norm(b - a) / ns * (z1 - z0) / nz))
    return out
def area_pts(poly_fn, d0, d1, t0, t1, z, n_force, gap, nd=14, nt=14):
    out = []; dd = (d1 - d0) / nd; dt = (t1 - t0) / nt
    for d in np.linspace(d0 + dd / 2, d1 - dd / 2, nd):
        for t in np.linspace(t0 + dt / 2, t1 - dt / 2, nt):
            g = poly_fn(d, t)
            if g is None: continue
            out.append((d, t, z, *n_force, g, dd * dt))
    return out
def in_poly(P, x, y):
    P = np.asarray(P); n = len(P); ins = False
    for i in range(n):
        x1, y1 = P[i]; x2, y2 = P[(i + 1) % n]
        if (y1 > y) != (y2 > y) and x < x1 + (y - y1) * (x2 - x1) / (y2 - y1): ins = not ins
    return ins
def nrm(a, b, outward_sign):          # unit normal of edge a->b in (d,t); force on carrier = -outward normal of the tongue
    v = np.array(b, float) - np.array(a, float); n = np.array([v[1], -v[0]]) / np.linalg.norm(v)
    return n * outward_sign
LATCH_Z0 = BLK_Z[0] - 0.3                          # 222.9: channels bch/tch cut the tongue from here to the top
def patches(gap_mode='cad'):
    G = (lambda g: g) if gap_mode == 'cad' else (lambda g: 0.0)
    P = {}
    # inner wall (wing pushes the tongue +d; wedge t' 8.5-18.5 at 0.05, 5.3-8.5 at 0.2 -> 0.05)
    P['inner wall'] = seg_pts([3.75, MOUTH + T_BACK], [3.75, 8.5], 0, 99, *TG_Z, (1.0, 0.0, 0.0), lambda p: G(0.2 - 0.15 * max(0, (p[1] - 7.7) / 0.8)), ns=4) + \
                      seg_pts([3.75, 8.5], [4.012, 18.5], 0, 99, *TG_Z, (*(nrm([3.75, 8.5], [4.012, 18.5], 1)), 0.0), G(0.05))
    # outer wall: only t' 5.3-9.3 (mouth chamfer before, slot diagonal after) and only z below the latch channels
    n_o = -nrm([17.45, 3.0], [17.285, 9.3], 1)
    P['outer wall'] = seg_pts([17.45, 3.0], [17.285, 9.3], MOUTH + T_BACK, 99, TG_Z[0], LATCH_Z0, (*n_o, 0.0), G(0.05), nz=6)
    # tongue rear faces (outer diagonal, back, inner-back diagonal): 0.2 (stop push-in)
    rear = []
    for a, b in zip(TONGUE[3:8], TONGUE[4:9]):
        n = -nrm(a, b, 1); rear += seg_pts(a, b, 0, 99, *TG_Z, (*n, 0.0), G(0.17), ns=5, nz=6)
    P['rear faces'] = rear
    # tooth on the groove's 6 deg face (wing material only behind d' 18.45..22.0 because of the 3.3 mouth chamfer)
    P['tooth (latch)'] = [(d, GR_T + (d - 17.65) * GK, z, 0.0, 1.0, 0.0, G(0.25), (22.0 - 18.45) / 6 * 22.5 / 10)
                          for d in np.linspace(18.45 + 0.3, 22.0 - 0.3, 6) for z in np.linspace(BLK_Z[0] + 1.1, BLK_Z[1] - 1.1, 10)]
    # nose back face on the notch face (t' 1.7 / 2.0): outside the mouth chamfer footprint
    def nose_face(d, t):
        z = t                                          # (reuse: t holds z here)
        inside_blade = (-4.5 <= d <= (24.6 if z > 215.0 else 18.0))
        in_mouth = (3.55 - MOUTH <= d <= 17.65 + MOUTH) and (TG_Z[0] - CLR - MOUTH <= z)
        return G(0.3) if inside_blade and not in_mouth else None
    nf = []
    for (d, z, _, nd_, nt_, nz_, g, a) in area_pts(nose_face, -4.5, 24.6, NZ[0] + SEAM, NZ[1] - SEAM, 0.0, (0, 0, 0), 0, nd=40, nt=40):
        nf.append((d, T_BACK - SEAM, z, 0.0, -1.0, 0.0, g, a))
    P['nose back face'] = nf
    # tongue bottom / top on the slot floor / ceiling (t' >= 5.3), with the 0.3 pad reliefs and the ribs
    def bot(d, t):
        if t < MOUTH + T_BACK or not in_poly(TONGUE, d, t): return None
        if any(abs(t - r) < 0.6 for r in RIBS) and 4.3 <= d <= 9.0: return 0.0
        if PADS[0] <= t <= PADS[1] and d >= ROD[0] + ROD[2] + 1.2 + PAD_REL + 0.5: return G(0.5)
        return G(0.2)
    def top(d, t):
        if t < MOUTH + T_BACK or not in_poly(TONGUE, d, t): return None
        if 7.9 <= d and t <= BLK_T[1] + 0.3: return None   # latch channels open through the top
        if any(abs(t - r) < 0.6 for r in RIBS) and 4.3 <= d <= 8.4: return 0.0
        if PADS[0] <= t <= PADS[1]: return G(0.5)
        return G(0.2)
    P['tongue bottom'] = area_pts(bot, 3.75, 17.45, MOUTH + T_BACK, 23.4, TG_Z[0], (0, 0, 1.0), 0)
    P['tongue top'] = area_pts(top, 3.75, 17.45, MOUTH + T_BACK, 23.4, TG_Z[1], (0, 0, -1.0), 0)
    # nose bottom / top on the notch floor / ceiling (0.3)
    def nb(d, t): return G(0.3) if (-4.5 <= d <= 18.0 and T_FRONT <= t <= T_BACK - SEAM) else None
    def nt(d, t):
        if not (-4.5 <= d <= 24.6 and T_FRONT <= t <= T_BACK - SEAM): return None
        if 7.9 <= d <= 17.45 and -4.3 <= t: return None      # button channel open on the nose top
        if abs(t - BT_TC) <= 6.7 and d >= 8.0: return None   # cap/spring channel
        return G(0.3)
    P['nose bottom'] = area_pts(nb, -4.5, 18.0, T_FRONT, T_BACK - SEAM, NZ[0] + SEAM, (0, 0, 1.0), 0)
    P['nose top'] = area_pts(nt, -4.5, 24.6, T_FRONT, T_BACK - SEAM, NZ[1] - SEAM, (0, 0, -1.0), 0)
    return {k: np.array(v, float) for k, v in P.items()}

def solve(Fext, Mext, PT, k=K_BRG, iters=200):
    """rigid carrier, unilateral springs with gaps. u = (ud, ut, uz, rd, rt, rz) of the carrier about REF (F frame)."""
    names = list(PT); A = np.concatenate([PT[n] for n in names]); grp = np.concatenate([[i] * len(PT[n]) for i, n in enumerate(names)])
    # S -> F for lever arms (lean)
    pos = A[:, :3].copy(); pos[:, 0] += LD * (pos[:, 2] - ZC); pos[:, 1] += LT * (pos[:, 2] - ZC)
    r = pos - REF; nf = A[:, 3:6]; gap = A[:, 6]; area = A[:, 7]
    # closing motion of point i: m_i = -nf_i (carrier moves against the reaction), delta = m.(u + rot x r) - gap
    m = -nf
    J = np.hstack([m, np.cross(r, m)])           # d(delta)/d(u, theta): m.(theta x r) = theta.(r x m)
    kk = k * area
    act = np.zeros(len(A), bool); u = np.zeros(6)
    F6 = np.r_[Fext, Mext]
    for it in range(iters):
        Kt = (J[act] * kk[act, None]).T @ J[act] + np.eye(6) * 1e-6
        b = F6 + (J[act] * (kk[act] * gap[act])[:, None]).sum(0)
        # equilibrium: sum of reactions = -F6;  reaction_i = -kk delta m  ->  sum J^T kk delta = F6
        u = np.linalg.solve(Kt, b)
        delta = J @ u - gap
        new = delta > 0
        if (new == act).all(): break
        act = new
    delta = J @ u - gap; f = kk * np.clip(delta, 0, None)        # reaction magnitude on carrier (along nf)
    res = {}
    for i, n in enumerate(names):
        s = grp == i
        vec = (f[s, None] * nf[s]).sum(0)
        p = (f[s] / area[s]).max() if s.any() else 0
        res[n] = dict(F=float(f[s].sum()), vec=vec.round(1).tolist(), pmax=float(p), n_act=int((f[s] > 0).sum()))
    eq = (J.T @ f) - F6
    return res, u, float(np.abs(eq).max()), (f, A, names, grp)

CASES = {}
for P in (300.0, 600.0, 900.0):
    CASES[f'lean out {P:.0f} N centre'] = (np.array([P, 0, 0]), 'centre')
    CASES[f'knock out {P:.0f} N corner'] = (np.array([P, 0, 0]), 'corners')
    CASES[f'knock in {P:.0f} N corner'] = (np.array([-P, 0, 0]), 'corners')
    CASES[f'pull fwd {P:.0f} N centre'] = (np.array([0, -P, 0]), 'centre')
    CASES[f'push back {P:.0f} N corner'] = (np.array([0, P, 0]), 'corners')
    CASES[f'down {P:.0f} N centre'] = (np.array([0, 0, -P]), 'centre')
if __name__ == '__main__':
    gm = sys.argv[1] if len(sys.argv) > 1 else 'cad'
    PT = patches(gm)
    print('patch areas mm2:', {k: round(float(v[:, 7].sum()), 1) for k, v in PT.items()})
    out = {}
    for cn, (Fw, where) in CASES.items():
        worst = None
        for cfg in CONFIGS:
            pts = pad_points(*cfg)
            for Pw in ([pts['centre']] if where == 'centre' else pts['corners']):
                f, M = wrench(Fw, Pw)
                res, u, err, _ = solve(f, M, PT)
                key = res['tooth (latch)']['F']
                score = max(res['tooth (latch)']['F'], res['outer wall']['pmax'] * 30, res['inner wall']['pmax'] * 30)
                if worst is None or score > worst[0]:
                    worst = (score, cfg, Pw.round(1).tolist(), f.round(0).tolist(), (M / 1000).round(1).tolist(), res, u.round(4).tolist(), err)
        _, cfg, Pw, f, M, res, u, err = worst
        out[cn] = dict(cfg=cfg, at=Pw, F_dtz=f, M_dtz_Nm=M, res=res, u=u, err=err)
        print(f'\n== {cn}: worst cfg {cfg} at {Pw}  F(d,t,z) {f}  M(d,t,z) {M} N.m  eq.err {err:.2g}')
        for n, r in res.items():
            if r['F'] > 0.5: print(f'   {n:16s} {r["F"]:7.0f} N  vec {r["vec"]}  pmax {r["pmax"]:6.1f} MPa  pts {r["n_act"]}')
    json.dump(out, open(f'loads_{gm}.json', 'w'), indent=1)
