# 3D voxel FE of the wing tip around the notch (S frame), loaded with the contact-model forces (wing side), worst config.
import numpy as np, sys, json, pickle
from matplotlib.path import Path
from geom import *
import fe3d, slices
h = float(sys.argv[1]) if len(sys.argv) > 1 else 0.6
case = sys.argv[2] if len(sys.argv) > 2 else 'lean 300 (centre)'
gm = sys.argv[3] if len(sys.argv) > 3 else 'zero'
o = np.array([-6.0, -14.0, 196.0]); ext = np.array([25.0, 30.0, 258.0])
n = np.ceil((ext - o) / h).astype(int)
c = o + (np.indices(n).transpose(1, 2, 3, 0) + 0.5) * h
d, t, z = c[..., 0], c[..., 1], c[..., 2]
js = json.load(open('wing_sections.json'))['244.0']; loops = [np.array(l) for l in js]
P2 = np.c_[d[:, :, 0].ravel(), t[:, :, 0].ravel()]
cnt = np.zeros(len(P2), int)
for lp in loops: cnt += Path(lp).contains_points(P2)
sec = (cnt % 2 == 1).reshape(d.shape[:2])
# in front of t' 2 the blade continues (outside the notch band): extend the section forward to the front face t' -14.27
front = (t[:, :, 0] >= T_FRONT) & (t[:, :, 0] < 2.5) & (d[:, :, 0] >= -4.4) & (d[:, :, 0] <= 24.6)
sec2d = sec | front
m = np.repeat(sec2d[:, :, None], n[2], 2)
m &= d <= 24.6 - np.clip(214.5 - z, 0, None) * 0.9                    # outer chamfer under the cheek
m &= ~((t < T_BACK) & (z >= NZ[0]) & (z <= NZ[1]))                     # notch
P3 = np.c_[d.ravel(), t.ravel()]
slot2d = Path(np.array(SLOT)).contains_points(np.c_[d[:, :, 0].ravel(), t[:, :, 0].ravel()]).reshape(d.shape[:2])
m &= ~(slot2d[:, :, None] & (z >= TG_Z[0] - CLR) & (z <= TG_Z[1] + CLR))
e = np.clip(T_BACK + MOUTH - t, 0, None)
m &= ~((t >= T_BACK - 1) & (t < T_BACK + MOUTH) & (d >= 3.55 - e) & (d <= 17.65 + e) & (z >= TG_Z[0] - CLR - e) & (z <= TG_Z[1] + CLR + e))
wallT = 17.5 - (GR_T - 3.0) * 0.0262
face_t = np.where(d < wallT + GR_CH, GR_T - GR_CH + (d - wallT), GR_T + (d - 17.65) * GK)
m &= ~((d >= 16.85) & (d <= GR_D) & (t >= face_t) & (t <= 9.0) & (z >= GR_Z[0]) & (z <= GR_Z[1]))
print('voxels', int(m.sum()), n)
C_ = pickle.load(open(f'cases_{gm}.pkl', 'rb'))
rows = C_[case]
r = max(rows, key=lambda r: r['res']['outer wall']['F'] + r['res']['tooth (latch)']['F'] + 0.3 * r['res']['inner wall']['F'])
f, A, names, grp = r['fpt'], r['A'], r['names'], r['grp']
loads = []
for gi, nm in enumerate(names):
    if nm in ('tongue bottom', 'tongue top', 'nose bottom', 'nose top', 'inner wall', 'outer wall', 'rear faces', 'tooth (latch)', 'nose back face'):
        for k in np.nonzero(grp == gi)[0]:
            if f[k] <= 0: continue
            a = A[k]; fv = -f[k] * a[3:6]
            p = a[:3].copy()
            # push the application point 0.3 into the wing surface so it lands on a wing node
            p[:2] -= 0.3 * a[3:5]; p[2] -= 0.3 * a[5]
            loads.append((*p, *fv))
fixed = lambda X, Y, Z: (Y >= ext[1] - 0.01) | (Z <= o[2] + 0.01) | (Z >= ext[2] - 0.01)
S, info, dt, nv = fe3d.solve(m, o, h, MAT['E'], MAT['nu'], fixed, loads, tol=1e-6, maxiter=12000)
print('cg', info, round(dt, 1), 's; case', case, r['cfg'], 'tooth', round(r['res']['tooth (latch)']['F']), 'outer', round(r['res']['outer wall']['F']))
sx, sy, sz, txy, tyz, txz = [S[..., i] for i in range(6)]
s_ip = np.sqrt(((sx - sy) / 2) ** 2 + txy ** 2); p1 = (sx + sy) / 2 + s_ip; p2 = (sx + sy) / 2 - s_ip
# bearing (compression) is checked separately; here tension/shear failure index (z-up print)
FI = np.sqrt((np.clip(np.maximum(p1, -p2 * 0.5), 0, None) / MAT['Xa']) ** 2 + (np.clip(sz, 0, None) / MAT['Xc']) ** 2 + (np.sqrt(tyz ** 2 + txz ** 2) / 18.0) ** 2)
reg = {
 'outer wall below the groove (d 17.6-24.6, t 2-14, z 214-223)': (d > 17.6) & (t > 2) & (t < 14) & (z > 214) & (z < 223),
 'shoulder + 2.4 skin (latch zone)': (d > 17.9) & (t > 2) & (t < 10) & (z > 223) & (z < 246.5),
 'inner wall of the slot (d -4.4..3.5, t 2-20)': (d < 3.5) & (t > 2) & (t < 20) & (z > 212) & (z < 249),
 'web to S1 + slot back (t 18-30)': (t > 18) & (t < 29) & (z > 210) & (z < 250),
 'notch floor/ceiling corners (z 205-216 / 244-249, t 0-6)': (t > 1.5) & (t < 6) & (((z > 205) & (z < 216)) | ((z > 244) & (z < 250))),
}
out = {}
for k, sel in reg.items():
    v = FI[sel & m]; v = v[np.isfinite(v)]
    p99 = np.percentile(v, 99); p95 = np.percentile(v, 95)
    out[k] = dict(SF99=1 / p99, SF95=1 / p95)
    print(f'{k:62s} SF(99th) {1/p99:5.2f}  SF(95th) {1/p95:5.2f}')
json.dump(dict(case=case, cfg=r['cfg'], res=out), open(f'wing3d_{gm}_{case.split()[0]}{case.split()[1]}.json', 'w'), indent=1)
