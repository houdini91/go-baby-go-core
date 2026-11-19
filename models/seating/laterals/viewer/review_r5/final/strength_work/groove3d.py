# 3D voxel FE of the wing around the catch groove (S frame, lean ignored): shoulder in front of the groove, 2.4 skin behind it.
import numpy as np, sys, json, math
from geom import *
import fe3d
h = float(sys.argv[1]) if len(sys.argv) > 1 else 0.35
variant = sys.argv[2] if len(sys.argv) > 2 else 'as_built'
o = np.array([16.0, 2.0, 219.0]); ext = np.array([24.6, 12.8, 251.0])
n = np.ceil((ext - o) / h).astype(int)
c = o + (np.indices(n).transpose(1, 2, 3, 0) + 0.5) * h
d, t, z = c[..., 0], c[..., 1], c[..., 2]
skin_out = 24.6
mouth = MOUTH; gr_d = GR_D
if variant == 'fix':            # proposed: no mouth chamfer on the outer wall (0.5 x 45 instead), groove 1.6 shallower, skin 4.0
    mouth = 0.5; gr_d = 20.6
m = d <= skin_out
in_slot = (d < 17.65 - 0.0262 * (t - 3.0)) & (z >= TG_Z[0] - CLR) & (z <= TG_Z[1] + CLR)
m &= ~in_slot
e = np.clip(T_BACK + mouth - t, 0, None)
m &= ~((t < T_BACK + mouth) & (d <= 17.65 + e) & (z >= TG_Z[0] - CLR - e) & (z <= TG_Z[1] + CLR + e))
wallT = 17.5 - (GR_T - 3.0) * 0.0262
face_t = np.where(d < wallT + GR_CH, GR_T - GR_CH + (d - wallT), GR_T + (d - 17.65) * GK)
groove = (d >= 16.85) & (d <= gr_d) & (t >= face_t) & (t <= 9.0) & (z >= GR_Z[0]) & (z <= GR_Z[1])
m &= ~groove
print('voxels', m.sum(), 'shape', n)
# load: tooth reaction on the groove face, wing-material-backed part, unit 1000 N, direction (+0.105, -0.995) (on the wing)
loads = []
F = 1000.0
ds = np.arange(18.45 if variant == 'as_built' else 17.96, 22.0 if variant == 'as_built' else gr_d - 0.2, h)
zs = np.arange(BLK_Z[0], BLK_Z[1], h)
for dd in ds:
    for zz in zs:
        loads.append((dd, GR_T + (dd - 17.65) * GK, zz, 0.105 * F / (len(ds) * len(zs)), -0.995 * F / (len(ds) * len(zs)), 0))
fixed = lambda X, Y, Z: (Y >= ext[1] - 0.01) | (Z <= o[2] + 0.01) | (Z >= ext[2] - 0.01)
S, info, dt, nv = fe3d.solve(m, o, h, MAT['E'], MAT['nu'], fixed, loads, tol=1e-6, maxiter=8000)
print('cg info', info, 'time', round(dt, 1))
sx, sy, sz, txy, tyz, txz = [S[..., i] for i in range(6)]
s_ip = np.sqrt(((sx - sy) / 2) ** 2 + txy ** 2); p1 = (sx + sy) / 2 + s_ip; p2 = (sx + sy) / 2 - s_ip
FI = np.sqrt((np.maximum(np.abs(p1), np.abs(p2)) / MAT['Xa']) ** 2 + (np.clip(sz, 0, None) / MAT['Xc']) ** 2 + ((np.sqrt(tyz ** 2 + txz ** 2)) / 18.0) ** 2)
def pk(sel, q=99.0):
    v = FI[sel & m]; v = v[np.isfinite(v)]; return np.percentile(v, q), np.nanmax(v)
zmid = (z > 226) & (z < 243)
regions = {
 'shoulder (in front of the groove face)': (t < face_t) & (d > 18.0) & (d < gr_d) & (z > GR_Z[0]) & (z < GR_Z[1]),
 'skin behind the groove': (d > gr_d) & (t > 3.0) & (t < 9.5) & (z > GR_Z[0]) & (z < GR_Z[1]),
 'groove ends (z closures)': (d > 17.5) & (t < 9.5) & ((z < GR_Z[0]) | (z > GR_Z[1])),
}
res = {}
for k, sel in regions.items():
    p99, mx = pk(sel); res[k] = dict(FI99_per_kN=p99, FImax_per_kN=mx)
    print(f'{k:42s} FI(99th pct) per kN tooth load {p99:5.2f}  -> tooth load at FI 1: {1000/p99:6.0f} N   (max {mx:5.2f})')
    # which component dominates
    vv = sel & m
    print('    mean |sigma_ip| %.1f  sigma_zz+ %.1f  tau_il %.1f MPa per kN' % (np.nanpercentile(np.maximum(np.abs(p1), np.abs(p2))[vv], 99), np.nanpercentile(np.clip(sz, 0, None)[vv], 99), np.nanpercentile(np.sqrt(tyz**2+txz**2)[vv], 99)))
json.dump(res, open(f'groove3d_{variant}.json', 'w'), indent=1)
