# Hook-and-drop (B) vs button-is-catch (A) vs steel plunger (C): pull-out element loads incl. the pitch couple, SF.
# Tip frame (d, t, z) about the tongue centre, LOW (pad below). Material allowables as review_r4/strength/analyse.py
# (RT: shear 22, across-layer 18, along 40; 60 C sustained: shear 7, across 5.9) + bearing 25 RT (lock_research.md) / 8 at 60 C.
import numpy as np
AX = np.array([0.79577626, 0.60559074]); TH = np.array([-0.6083053, 0.79370313])
def dt(v): return np.array([v @ AX, v @ TH])            # world xy vector -> (d, t)
rP = np.array([-62.6, -33.3, -47.5])                    # pad centre rel. tongue centre (mech r4)
e = dt(np.array([0, -34.0])); rE = rP + np.array([e[0], e[1], -20.0])   # pad front-bottom corner (strength r4 'edge300')
def F(world_xy, fz=0): f = dt(np.array(world_xy, float)); return np.array([f[0], f[1], fz])
CASES = {'knock 300 N out (RT)': (F([300, 0]), rP, 'RT'), 'corner knock 300 N out (RT)': (F([300, 0]), rE, 'RT'),
         'lean 100 N out, 60 C sustained': (F([100, 0]), rP, 'H60'), 'forward pull 150 N (RT)': (F([0, -150]), rP, 'RT'),
         'inward pull 100 N (RT)': (F([-100, 0]), rP, 'RT'), 'push pad UP 100 N (RT)': (np.array([0, 0, 100.]), rP, 'RT'),
         'push pad DOWN 150 N (RT)': (np.array([0, 0, -150.]), rP, 'RT')}
ALW = {'RT': dict(brg=25., sh=22., ten=18.), 'H60': dict(brg=8., sh=7., ten=5.9)}
ZB = 17.15                                              # nose back face bears on the wing cut face near its top/bottom edge
def solve(Pt, Md, zh_rows):
    """pull-out Pt (>0 = out, -t) and pitch moment Md about d. Forces on the carrier: hook/latch rows (+t) at zh, cut-face bearing (-t) at +-ZB."""
    best = None
    for zh in zh_rows:
        for zb in (ZB, -ZB):
            # H - B = Pt ; Md - zh*H + zb*B = 0
            A = np.array([[1, -1], [-zh, zb]]); b = np.array([Pt, -Md])
            H, B = np.linalg.solve(A, b)
            if H >= -1e-6 and B >= -1e-6 and (best is None or H < best[0]): best = (H, B, zh, zb)
    return best
CONC = {
 'B hook-and-drop (2 rows x 2 walls, lips 3.0 d x 6.5 z x 6 t)': dict(rows=(9.575, -9.575), per=2, brg=3.0 * 6.5, root_sh=6.5 * 6.0, root_Z=6.5 * 6.0 ** 2 / 6, arm=1.5),
 'A button-is-catch (1 tooth 4 d x 15 z, in a wall groove)':   dict(rows=(0.0,), per=1, brg=4.0 * 15, root_sh=5.0 * 15, root_Z=15 * 5.0 ** 2 / 6, arm=2.0),
 'C steel pin Ø6 (8 in tongue, brass liner Ø8 in a 6 wall)':    dict(rows=(0.0,), per=1, brg=6.0 * 8.0, root_sh=None, root_Z=None, arm=None),
}
out = []
for name, cc in CONC.items():
    out.append(f'\n== {name}')
    for cn, (Fv, r, mat) in CASES.items():
        M = np.cross(r, Fv); Pt = -Fv[1]
        if abs(Fv[2]) > 0 and Pt == 0:
            out.append(f'  {cn:34s} vertical: {"keystone above (up) / stack below (down)" if name.startswith("B") else "filler stack (up and down)"} in compression; lift {Fv[2]:+.0f} N, pitch {M[0]/1000:+.1f} roll {M[1]/1000:+.1f} N.m -> U-walls')
            continue
        if Pt <= 0:
            out.append(f'  {cn:34s} push-in {-Pt:.0f} N -> nose back face on the cut face; latch/hooks unloaded'); continue
        H, B, zh, zb = solve(Pt, M[0], cc['rows'])
        a = ALW[mat]; h1 = H / cc['per']
        s_b = h1 / cc['brg']; line = f'  {cn:34s} pull-out {Pt:5.0f} N, pitch {M[0]/1000:+5.1f} N.m -> element {H:5.0f} N (row z {zh:+.1f}, {h1:.0f} N each), cut face {B:4.0f} N | bearing {s_b:4.1f} MPa SF {a["brg"]/s_b:4.1f}'
        if cc['root_sh']:
            tau = h1 / cc['root_sh']; sb = h1 * cc['arm'] / cc['root_Z']
            line += f' | root shear {tau:4.1f} SF {a["sh"]/tau:4.1f} | root bend {sb:4.1f} SF {a["ten"]/sb:4.1f}(across) {(40 if mat=="RT" else 13)/sb:4.1f}(along)'
        else:
            line += f' | steel pin shear {H/(np.pi*9):.0f} MPa (allow ~200)'
        out.append(line)
txt = '\n'.join(out); print(txt)
# moments common to all three (taken by the U-walls), corner knock
for cn in ('knock 300 N out (RT)', 'corner knock 300 N out (RT)'):
    Fv, r, _ = CASES[cn]; M = np.cross(r, Fv); print(f'\n{cn}: F(d,t,z) {Fv.round(0)}  M(d,t,z) {(M/1000).round(1)} N.m (yaw Mz and roll Mt -> U-walls, all concepts)')
# lift latch (B): worst upward
print('\nB lift: child pushes pad up 100 N (2-5 y, both hands) -> carrier lift 100 N into the keystone (compression, ~600 mm2: 0.2 MPa) -> keystone lip channel floor (2 lips x 3 x 6 = 36 mm2: 1.4 MPa, SF 18); 300 N kick-up: SF 6. Keystone bolt sees none of it.')
# friction / self-holding of a 6 deg face
for mu in (0.1, 0.2, 0.3, 0.5):
    print(f'6 deg taper, mu {mu}: friction angle {np.degrees(np.arctan(mu)):.1f} deg -> {"self-holding" if np.degrees(np.arctan(mu)) > 6 else "BACK-DRIVES"}; spring/gravity preload gain cot(6+phi) = {1/np.tan(np.radians(6)+np.arctan(mu)):.1f}')
# spring for the keystone / A button
G = 69e3   # stainless 304
for (d, OD, n, Lf, Li, trav) in ((0.6, 6.0, 6, 14.0, 10.0, 4.0), (0.55, 6.0, 6, 14.0, 10.0, 4.0), (0.5, 5.0, 6, 12.0, 9.0, 3.5)):
    D = OD - d; k = G * d ** 4 / (8 * D ** 3 * n); F0 = k * (Lf - Li); F1 = k * (Lf - Li + trav); C = D / d; Kw = (4 * C - 1) / (4 * C - 4) + 0.615 / C
    tau = Kw * 8 * F1 * D / (np.pi * d ** 3); print(f'spring SS304 d{d} OD{OD} n{n}+2 Lf{Lf}: k {k:.2f} N/mm, solid {(n+2)*d:.1f}, installed {Li} -> {F0:.1f} N, pressed {Li-trav:.1f} -> {F1:.1f} N, tau {tau:.0f} MPa')
