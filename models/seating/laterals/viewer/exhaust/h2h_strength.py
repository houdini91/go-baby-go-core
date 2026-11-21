# Head-to-head strength: F (front-edge tongue, tall 80 / 90, R2 root, optional Ø2 corner rods) vs S (side twist-in bayonet),
# each with and without the optional Ø8 carbon prongs (F: in the wing, as option D; S: as lug cores, see side()).
# ONE model for all: the CAD agent's r5b rigid-carrier / unilateral-Winkler contact solver (review_r5/final/strength_work/loads.py,
# exec'd through review_r5/cad/strength_r5b/sf_r5b.py exactly as tall.py does), 'zero' fit gaps, 300 N lean (pad centre) and
# 300 N knock-out (pad corners), all 8 arm/kit/pad configs. Allowables as r5b: bearing 25 MPa (wing dent onset), Xa 40, S 22.
# The real arm and tenon are unchanged for every candidate: arm SF 1.90 / 1.24, tenon fuse 2.87 / 1.16 (r5b, lean / corner).
# "First gives" = 300 N x the lowest SF over: wing dent (peak bearing on any wing face), the socket part (tongue or plug / teeth /
# cheek), the arm and the tenon fuse. All of it is an ESTIMATE (rigid kit, linear foundation, hand section formulas).
# Prongs are modelled as contact patches too (Ø8, projected-area bearing in the wing bore), not as assumed load shares.
# Run: ../.venv/bin/python h2h_strength.py   (writes h2h_strength.json here)
import os, sys, json, math
HERE = os.path.dirname(os.path.abspath(__file__)); SB = os.path.join(HERE, '..', 'review_r5', 'cad', 'strength_r5b')
os.chdir(SB); sys.argv = ['x', 'zero']
base = open('sf_r5b.py').read().split("if __name__ == '__main__':")[0]
ARM = dict(lean=1.90, corner=1.24); FUSE = dict(lean=2.87, corner=1.16)
P = 300.0

def run_cases(ns, PT):
    L, np = ns['L'], ns['np']; out = {}
    for nm, where in (('lean', 'centre'), ('corner', 'corners')):
        rows = []
        for cfg in ns['CONFIGS']:
            pts = L.pad_points(*cfg)
            for Pw in ([pts['centre']] if where == 'centre' else pts['corners']):
                f, M = L.wrench(np.array([P, 0, 0]), Pw)
                if ns.get('ROD_NM'): M = M - np.sign(M) * np.minimum(np.abs(M), np.array([0, 0, ns['ROD_NM'] * 1000]))
                r, u, err, fp = L.solve(f, M, PT); rows.append((r, fp, f, M))
        out[nm] = rows
    return out

def peak(rows, names):
    return max((r[n]['pmax'] for r, *_ in rows for n in names if n in r), default=1e-9)

def prong_pts(axis, c1, c2, s0, s1, n=12, dirs=None):
    """Ø8 prong along `axis` (0 d, 1 t) at the other two coords (c1, c2), embedded s0..s1 in the wing: bearing points in the
    two perpendicular directions, projected area 8 x ds each (the kit bore is taken as rigid)."""
    pts = []; ds = (s1 - s0) / n
    for s in [s0 + ds * (i + 0.5) for i in range(n)]:
        p = [0, 0, 0]; p[axis] = s
        if axis == 0: p[1], p[2] = c1, c2; dl = [(0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)]
        else: p[0], p[2] = c1, c2; dl = [(1, 0, 0), (-1, 0, 0), (0, 0, 1), (0, 0, -1)]
        for dv in dl: pts.append((*p, *dv, 0.0, 8.0 * ds))
    return pts

# ------------------------------------------------------------------ F: front-edge tongue, tall
def front(H_TALL, prongs, rods=False):
    z0 = 214.3; z1 = z0 + H_TALL; nz = (196.0 if prongs else 207.0, z1 + 2.7)
    src = base.replace("z = np.arange(210, 250, H) + H / 2", "z = np.arange(205, %.1f, H) + H / 2" % (nz[1] + 5))
    ns = {}; exec(src, ns); np = ns['np']
    G = ns['G']; G.TG_Z = (z0, z1); G.NZ = nz
    ns.update(TG_Z=G.TG_Z, NZ=nz, BAR_Z0=z1 - 5.2, TO_Z=(z1 - 32.8, z1 - 0.8))
    PT = ns['patches']('zero')
    if rods: ns['ROD_NM'] = 4.8      # 2 Ø2 corner rods across the tongue root: yaw relief as tall.py (ESTIMATE)
    if prongs:   # as option D: lower prong at z 204 (bore to t' 20), upper at the nose top - 8 (bore to t' 32), d' 5.5
        PT['prongs'] = np.array(prong_pts(1, 5.5, 204.0, 2.0, 20.0) + prong_pts(1, 5.5, nz[1] - 8.3, 2.0, 32.0), float)
    R = run_cases(ns, PT); res = {}
    wing_names = [n for n in PT if n != 'tooth (latch)']
    for nm, rows in R.items():
        sf_t = ns['tongue_sf']([fp for _, fp, *_ in rows], 2.0, 1.4, 1.3)[0]
        sf_w = 25 / peak(rows, wing_names)
        items = {'wing dent': sf_w, 'tongue (kit)': sf_t, 'arm (kit)': ARM[nm], 'tenon fuse (kit)': FUSE[nm]}
        k = min(items, key=items.get)
        res[nm] = dict(first_gives_N=round(P * items[k]), part=k, wing_dent_N=round(P * sf_w), socket_part_N=round(P * sf_t),
                       worst_wing_face=max(wing_names, key=lambda n: max(r[n]['pmax'] for r, *_ in rows)))
    return res

# ------------------------------------------------------------------ S: twist-in (bayonet) side mount on the parent face
# Round plug R 15 in a bore (axis along d', centre t' TC, z ZC), floor d' 4, outer face at the mouth d' 21.5 (exhaust swell).
# Two lugs (span LUG_SPAN deg, reach 15 -> 23.5, thickness d' 5 -> 14) enter through the stadium mouth's ends (top and bottom),
# then the kit turns TURN deg (+theta, theta from +t' toward +z) so the lugs sit under ledges d' 14 -> 19 (wing plastic).
# Forward turn stops: the lug ends and the arm end landing on the wing's front face. Turn-back: the r5b deep-catch button
# (catch face 2.4 x 12, 6 deg, self-locking) in the collar at r 17 above the axis. The lean pull-out goes lugs -> ledges.
# best that fits the wing tip: bore Ø33 (front wall 4.8 to the front face, 4.5 to strap slit S1 at floor d' 5), mouth 33 x 54
TC, ZC, R0, R1, D_FLOOR, D_FACE_M, D_FACE = 7.0, 236.0, 16.5, 26.5, 5.0, 21.5, 16.0
LUG_D = (5.5, 14.0); TURN = 30.0; LUG_SPAN = 26.0
def side(prongs, lug_th=8.5, cheek_th=14.0):
    ns = {}; exec(base, ns); np = ns['np']
    th0 = [math.radians(90 + TURN), math.radians(270 + TURN)]
    pt = lambda r, th: (TC + r * math.cos(th), ZC + r * math.sin(th))
    tang = lambda th: (0.0, -math.sin(th), math.cos(th))          # +theta direction in (d, t, z)
    PT = {}
    bw = []
    for d in np.linspace(D_FLOOR + 1.0, D_FACE_M - 2.5, 7):
        for j in range(36):
            th = 2 * math.pi * (j + 0.5) / 36
            if d >= LUG_D[1] and any(abs(math.remainder(th - c, 2 * math.pi)) < math.radians(42) for c in (math.pi / 2, -math.pi / 2)): continue   # stadium end slots
            if LUG_D[0] <= d <= LUG_D[1] and any(abs(math.remainder(th - c, 2 * math.pi)) < math.radians(42 + TURN) for c in (math.pi / 2, -math.pi / 2)): continue  # lug chambers
            t, z = pt(R0, th); bw.append((d, t, z, 0, -math.cos(th), -math.sin(th), 0.0, (D_FACE_M - 3.5 - D_FLOOR) / 7 * 2 * math.pi * R0 / 36))
    PT['bore wall'] = np.array(bw, float)
    fl = []
    for r in np.linspace(1, R0 - 1, 6):
        for j in range(16):
            th = 2 * math.pi * (j + 0.5) / 16; t, z = pt(r, th); fl.append((D_FLOOR, t, z, 1, 0, 0, 0.0, math.pi * R0 ** 2 / 96))
    PT['bore floor'] = np.array(fl, float)
    lg = []; ls = []
    for c in th0:
        for r in np.linspace(R0 + 0.5, R1 - 0.5, 5):
            for k in range(6):
                th = c + math.radians(LUG_SPAN) * ((k + 0.5) / 6 - 0.5); t, z = pt(r, th)
                lg.append((LUG_D[1], t, z, -1, 0, 0, 0.0, (R1 - R0) / 5 * r * math.radians(LUG_SPAN) / 6))
        for r in np.linspace(R0 + 0.5, R1 - 0.5, 4):
            for d in np.linspace(LUG_D[0] + 1, LUG_D[1] - 1, 4):
                th = c + math.radians(LUG_SPAN / 2); t, z = pt(r, th); tg = tang(th)
                ls.append((d, t, z, *(-np.array(tg)), 0.0, (R1 - R0) / 4 * (LUG_D[1] - LUG_D[0]) / 4))
    PT['lug ledges'] = np.array(lg, float); PT['lug end stops'] = np.array(ls, float)
    thb = math.radians(90.0); t, z = pt(17.0, thb); tg = tang(thb)
    PT['button catch (turn-back)'] = np.array([(D_FACE_M - 1.2 - 0.6 * i, t + 6 * (j - 0.5) * math.sin(thb), z, *tg, 0.0, 2.4 * 12 / 4) for i in range(2) for j in range(2)], float)
    af = [(d, ns['T_FRONT'] - 0.3, z, 0, -1, 0, 0.0, 15.0 / 8 * 56.0 / 12) for d in np.linspace(-3, 12, 8) for z in np.linspace(200, 256, 12)]
    PT['front face (arm end)'] = np.array(af, float)
    ck = [(D_FACE, t, z, 1, 0, 0, 0.0, (TC - 19 - (-12.0)) / 4 * 60 / 12) for t in np.linspace(-12.0, TC - 19, 4) for z in np.linspace(ZC - 30, ZC + 30, 12)]
    PT['cheek on outer face'] = np.array(ck, float)
    R = run_cases(ns, PT); res = {}; names = list(PT)
    for nm, rows in R.items():
        sf_w = 25 / peak(rows, names)
        # lug root (kit; at r 15, arc span x thickness): shear + bending (lever = half the reach), Kt 1.5 at the root fillet
        lugF = 0.0; catchF = 0.0
        for r, (f, A, nms, grp), *_ in rows:
            gi = nms.index('lug ledges'); sel = grp == gi
            for c in th0:
                m = sel & (np.abs(np.arctan2(A[:, 2] - ZC, A[:, 1] - TC) - math.atan2(math.sin(c), math.cos(c))) < 0.6)
                lugF = max(lugF, f[m].sum())
            catchF = max(catchF, r['button catch (turn-back)']['F'])
        arc = R0 * math.radians(LUG_SPAN); h = lug_th; reach = R1 - R0
        tau = 1.5 * lugF / (arc * h); sig = 1.5 * 6 * lugF * (reach / 2) / (arc * h * h)
        sf_lug = 1 / math.sqrt((sig / 40.0) ** 2 + (tau / 22.0) ** 2)
        if prongs: sf_lug *= 1.6     # Ø8 carbon core through each lug root (kit side): ESTIMATE, bending share only
        sf_catch = 25.0 / max(catchF / (2.4 * 12.0), 1e-6)            # catch face bearing (kit tooth and the wing notch)
        # ledge shear-out in the wing: ledge d' 14 -> 19 above each lug, perimeter (2 x reach + arc at R1) x thickness 5
        ledge_A = (2 * reach + R1 * math.radians(LUG_SPAN)) * (D_FACE_M - 2.5 - LUG_D[1])
        sf_ledge = ledge_A * 22.0 / max(lugF, 1e-6)
        def section(sel_fn, cen, sf_fn):
            worst = 9e9
            for r, (f, A, nms, grp), *_ in rows:
                Fv = np.zeros(3); Mv = np.zeros(3)
                for fi, row in zip(f, A):
                    if fi <= 0 or not sel_fn(row): continue
                    v = fi * row[3:6]; Fv += v; Mv += np.cross(row[:3] - cen, v)
                worst = min(worst, sf_fn(Fv, Mv))
            return worst
        Ap = math.pi * R0 ** 2 - math.pi * 4 ** 2; Ip = math.pi / 4 * (R0 ** 4 - 4 ** 4); Jp = 2 * Ip
        def plug_sf(F, M):
            s = 1.4 * (abs(F[0]) / Ap + math.hypot(M[1], M[2]) * R0 / Ip); t_ = 1.5 * math.hypot(F[1], F[2]) / Ap + abs(M[0]) * R0 / Jp
            return 1 / math.sqrt((s / 40) ** 2 + (t_ / 22) ** 2)
        d_cut = D_FACE_M - 2.5
        sf_plug = section(lambda row: row[0] < d_cut and row[1] > ns['T_FRONT'] + 0.5 and row[0] > D_FACE - 0.5 - 99 and not (abs(row[0] - D_FACE) < 0.01), np.array([d_cut, TC, ZC]), plug_sf)
        t_cut = ns['T_FRONT'] + 2.0; Hc = 62.0; th_ = cheek_th; A_c = th_ * Hc; I_d = th_ * Hc ** 3 / 12; I_zz = Hc * th_ ** 3 / 12
        alpha = 0.333 * (1 - 0.63 * th_ / Hc)
        def cheek_sf(F, M):
            s = 1.4 * (abs(F[1]) / A_c + abs(M[0]) * (Hc / 2) / I_d + abs(M[2]) * (th_ / 2) / I_zz)
            t_ = 1.5 * math.hypot(F[0], F[2]) / A_c + abs(M[1]) / (alpha * th_ * th_ * Hc)
            return 1 / math.sqrt((s / 40) ** 2 + (t_ / 22) ** 2)
        sf_cheek = section(lambda row: row[1] > t_cut, np.array([D_FACE + th_ / 2, t_cut, ZC]), cheek_sf)
        items = {'wing dent': sf_w, 'wing ledge shear-out': sf_ledge, 'lugs (kit)': sf_lug, 'button catch (kit tooth / wing notch)': sf_catch,
                 'plug (kit)': sf_plug, 'cheek round the front edge (kit)': sf_cheek, 'arm (kit)': ARM[nm], 'tenon fuse (kit)': FUSE[nm]}
        k = min(items, key=items.get)
        wf = max(names, key=lambda n: max(r[n]['pmax'] for r, *_ in rows))
        res[nm] = dict(first_gives_N=round(P * items[k]), part=k, wing_dent_N=round(P * sf_w), worst_wing_face=wf,
                       lug_N=round(P * sf_lug), lug_load_N=round(lugF), catch_load_N=round(catchF), catch_N=round(P * sf_catch),
                       ledge_N=round(P * sf_ledge), plug_N=round(P * sf_plug), cheek_N=round(P * sf_cheek))
    return res

OUT = {'model': 'r5b contact model (loads.py via sf_r5b.py), zero gaps, 300 N; arm 1.90/1.24 and tenon fuse 2.87/1.16 as r5b; ESTIMATE'}
for H_ in (80.0, 90.0):
    for pr in (False, True):
        for rd in (False, True):
            k = f'F {H_:.0f}' + (' + rods' if rd else '') + (' + prongs' if pr else ''); OUT[k] = front(H_, pr, rd); print(k, OUT[k], flush=True)
for pr in (False, True):
    k = 'S' + (' + lug cores' if pr else ''); OUT[k] = side(pr); print(k, OUT[k], flush=True)
OUT['S notes'] = ('bigger plugs do not fit: a Ø40 bore (R 20, lugs to R 30, 12 thick) would give first-gives 356 / 254 N (lugs) and wing dent '
                  '568 / 389 N but leaves a 3.3 mm front wall; the lug load per lug stays ~950 (lean) / ~1300 N (corner) at 300 N because '
                  'the pad moment is carried by the lug-floor couple. The cheek round the front edge needs ~14 mm (12 mm: 462 / 313 N).')
json.dump(OUT, open(os.path.join(HERE, 'h2h_strength.json'), 'w'), indent=1)
