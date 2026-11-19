# Button tooth (d-t slice, plane stress, per mm of z; button prints standing -> d-t is the layer plane) with / without the 2 slits.
import numpy as np
from matplotlib.path import Path
from geom import *
import fe2d
H = 0.05
wallT = 17.5 - (GR_T - 3.0) * 0.0262
tf1 = GR_T + 0.02 + (TO_D[1] - 17.65) * GK
TOOTH = [[TO_D[0], GR_T - GR_CH + 0.02], [wallT, GR_T - GR_CH + 0.02], [wallT + GR_CH, GR_T + 0.02 + (wallT + GR_CH - 17.65) * GK], [TO_D[1], tf1], [TO_D[1], 6.3], [TO_D[1] - 2.5, BLK_T[1] - 0.2], [TO_D[0], BLK_T[1] - 0.2]]
x0, y0 = 12.5, -4.5
xs = np.arange(x0, 22.5, H) + H / 2; ys = np.arange(y0, 9.5, H) + H / 2
X, Y = np.meshgrid(xs, ys, indexing='ij'); P = np.c_[X.ravel(), Y.ravel()]
base = ((X >= BLK_D[0]) & (X <= BLK_D[1]) & (Y >= BLK_T[0]) & (Y <= BLK_T[1])) | Path(np.array(TOOTH)).contains_points(P).reshape(X.shape)
F_per_mm = 1000.0 / 22.5
loads = []
ds = np.arange(18.45, 22.0, H)
for dd in ds: loads.append((dd, GR_T + 0.02 + (dd - 17.65) * GK, 0.0, F_per_mm / len(ds)))
def fixed(x, y): return (y > BLK_T[1] - 0.06) & (x >= BLK_D[0]) & (x <= BLK_D[1])
for nm, slits in (('solid (no slits)', False), ('as built (2 slits 0.4)', True)):
    m = base.copy()
    if slits:
        for i in range(2): m &= ~((X >= 14.4) & (X <= 19.0) & (Y >= 5.9 + i * 1.3) & (Y <= 6.3 + i * 1.3))
    S, u, Xn, Yn, reac = fe2d.solve(m, x0, y0, H, MAT['E'], MAT['nu'], fixed, loads)
    sx, sy, txy = S[..., 0], S[..., 1], S[..., 2]
    p1 = (sx + sy) / 2 + np.sqrt(((sx - sy) / 2) ** 2 + txy ** 2)
    sel = m & (X > 16.0) & (X < 21.0)
    v = p1[sel]; v = v[np.isfinite(v)]
    print(f'{nm:26s} tooth root region: max principal tension 99th pct {np.percentile(v, 99):5.1f} MPa per kN tooth load (max {v.max():5.1f}); '
          f'tooth load at 40 MPa: {1000 * 40 / np.percentile(v, 99):5.0f} N; tip deflection {np.abs(u[1::2]).max()*1000:.1f} um/kN')
