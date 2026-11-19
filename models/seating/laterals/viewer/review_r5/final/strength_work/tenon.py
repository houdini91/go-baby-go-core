# Tenon root (x = xe) and nut-slot section (x = xe - 6.5), 4 x Ø2 carbon pins bond-limited; dovetail pad mount.
import numpy as np, math
from geom import *
import loads as L
from arm import tsai_hill_X, N_LAYER, LOADS, mirror_z
h = 0.05
def tenon_mask(slot):
    y = np.arange(-6.5, 6.5, h) + h / 2; z = np.arange(-10, 10, h) + h / 2
    Y, Z = np.meshgrid(y, z, indexing='ij')
    r = TEN_R; m = np.ones_like(Y, bool)
    for sy in (-1, 1):
        for sz in (-1, 1):
            cy, cz = sy * (6.5 - r), sz * (10 - r)
            m &= ~((sy * Y > sy * cy) & (sz * Z > sz * cz) & ((Y - cy) ** 2 + (Z - cz) ** 2 > r * r))
    for py, pz in TPIN: m &= ~((Y - py) ** 2 + (Z - pz) ** 2 <= TPIN_R ** 2)
    if slot:
        m &= ~((Y <= -1.0) & (np.abs(Z) <= 4.8)); m &= ~(np.abs(Z) <= M5_CLEAR / 2 if False else (np.abs(Z) <= 2.75))
    return Y, Z, m
M5_CLEAR = 5.5
def props(Y, Z, m):
    A = m.sum() * h * h; cy = Y[m].mean(); cz = Z[m].mean()
    Iyy = ((Z[m] - cz) ** 2).sum() * h * h; Izz = ((Y[m] - cy) ** 2).sum() * h * h
    return A, cy, cz, Iyy, Izz
sinth = abs(np.array([-1.0, 0, 0]) @ N_LAYER); X = tsai_hill_X(sinth)
print(f'tenon axis vs layers: sin(theta) {sinth:.2f} -> allowable along the tenon {X:.1f} MPa (Tsai-Hill; 40 along / 18 across / 22 shear)')
PIN_CAP = math.pi * 2.0 * 8.0 * 4.0                      # bond, 8 mm into the arm, 4 MPa RT -> N per pin
print(f'pin bond cap (8 mm in the arm) {PIN_CAP:.0f} N per pin')
res = {}
for slot, xoff, frac in ((False, 0.0, 1.0), (True, 6.5, 0.5)):
    Y, Z, m = tenon_mask(slot); A, cy, cz, Iyy, Izz = props(Y, Z, m)
    for nm, cfg, Pw, Fw in LOADS:
        zp = zp_of(cfg[0], 'LOW')
        if cfg[1] == 'HIGH': Pw = mirror_z(Pw); Fw = Fw * np.array([1, 1, -1])
        xe = armAt(armPath(C, zp), armPath(C, zp)['L'])[0][0]
        cen = np.array([xe - xoff, YAX, zp])
        M = np.cross(Pw - cen, Fw)                     # world: My (about y) bends in z; Mz bends in y
        My, Mz = M[1] * frac, M[2] * frac
        # pins: tension pair relieves up to 2*cap*lever (about both axes, conservative: take the larger axis)
        relief_y = 2 * PIN_CAP * 14.8 if not slot else 0; relief_z = 2 * PIN_CAP * 7.0 if not slot else 0
        sig = np.abs(Fw[0]) / A + max(abs(My) - relief_y, 0.6 * abs(My)) * 10 / Iyy + max(abs(Mz) - relief_z, 0.6 * abs(Mz)) * 6.5 / Izz
        tau = 1.5 * np.linalg.norm(Fw[1:]) / A + abs(M[0] * frac) / (0.246 * 13 ** 2 * 20)
        SF = 1 / math.sqrt((sig / X) ** 2 + (tau / 18.0) ** 2)
        k = ('root' if not slot else 'nut slot', nm)
        if k not in res or SF < res[k][0]: res[k] = (SF, sig, tau, cfg, (M / 1000).round(1))
    print(f"{'root x=xe' if not slot else 'nut-slot x=xe-6.5'}: A {A:.0f} mm2 Iyy {Iyy:.0f} Izz {Izz:.0f}")
for k, v in sorted(res.items()):
    if k[1] in ('lean 300', 'proof 600', 'corner 900', 'pull fwd 300'):
        print(f'  {k[0]:9s} {k[1]:11s} SF {v[0]:5.2f}  sigma {v[1]:6.1f} tau {v[2]:5.1f}  {v[3]}  M(world) {v[4]} N.m')
# ---------------- dovetail pad mount: rail 9 root / 12 top x 2.5 high, y 22.3..46 (23.7 long), groove closed at y 22 (stop)
print('\nDovetail (pad rail in the foot groove), PETG; pad shell prints child side down (rail in the layer plane ...), foot face down')
Lr = FOOT_Y[1] - (DV_FRONT + 0.3)
for nm, cfg, Pw, Fw in LOADS:
    pass
for P, nm in ((300, 'lean 300'), (600, 'proof 600'), (900, 'corner 900')):
    # worst: outward/inward x force on the pad at the far corner -> rail pulled out of / pushed into the foot; moment about the rail
    zc_off = 40 * 0.84 + PAD_OFF                     # corner z to the rail (rail at the pad centre z, foot axis at zp)
    y_off = 14.0 - 0.84 * 41 - (DV_FRONT + 0.3 + Lr / 2) if nm == 'corner 900' else 14.0 - (DV_FRONT + 0.3 + Lr / 2)
    zoff = 0.84 * 40 if nm == 'corner 900' else 0.0
    # inward pull (pad pulled toward the child, -x): the rail flanks (60 deg) carry it; flank area 2 x 23.7 x 2.9
    flank = 2 * Lr * (DV[2] / math.cos(math.atan((DV[1] - DV[0]) / 2 / DV[2])))
    # tilt moments about y (zoff * P) and about z (y_off * P) go into the rail + foot as a couple across the rail width / length
    Mz_ = abs(P * y_off); My_ = abs(P * zoff)
    # rail root (pad shell side): section 9 wide x 23.7 long, bending about y from My over the 2.5 rail height is negligible;
    # critical = flank/lip of the GROOVE in the foot: lip thickness ~ (FOOT_HZ - 11 - 6) for the outer groove
    lip = FOOT_HZ - PAD_OFF - DV[1] / 2 - DV_CL                   # foot material beyond the outer groove (z)
    # pull-out force on the rail from the tilt couple about z across the rail length (lever ~ Lr*2/3)
    Fp = P * 0.0 + Mz_ / (Lr * 2 / 3) + My_ / (DV[1] / 2 + 1)
    p_fl = Fp / (flank * math.sin(math.radians(60)))
    sig_lip = 6 * (Fp / Lr) * 1.5 / lip ** 2 if lip > 0 else 99
    print(f'  {nm:10s}: lip beyond the outer groove {lip:.2f} mm; rail flank force {Fp:5.0f} N -> flank pressure {p_fl:5.1f} MPa (SF {25/p_fl:4.1f}); '
          f'foot lip bending {sig_lip:5.1f} MPa (SF {40/sig_lip:4.2f} in-plane / {18/sig_lip:4.2f} if across)')
