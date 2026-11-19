# Mechanical review r5: walls around the FS slot/groove and the nose extents, on the ORIGINAL wing section (S frame).
# The FS fills the band with the z-240 / z-215.2 sections (lean prisms), so the z-240 section ∪ W1 is the wing at the notch.
import sys, json, numpy as np
sys.path.insert(0, '/home/mikey/go-baby-go-core/models/seating/laterals/viewer/r5mesh')
import manifold3d as mf
from frame import M_W2F
from wingfix import repaired_wing
LD, LT, ZC = 0.0565, 0.069, 227.0
F2S = np.array([[1, 0, -LD, LD*ZC], [0, 1, -LT, LT*ZC], [0, 0, 1, 0], [0, 0, 0, 1]], float)
W, _ = repaired_wing()
WS = W.transform(np.asarray((F2S @ M_W2F)[:3, :4], float))
CS = mf.CrossSection
def rect(x0, x1, y0, y1): return CS([np.array([[x0, y0], [x1, y0], [x1, y1], [x0, y1]], float)])
def segs(cs):
    S = []
    for p in cs.to_polygons():
        p = np.asarray(p); S.append(np.c_[p, np.roll(p, -1, 0)])
    return np.vstack(S)
def dist(pts, S):
    a = S[:, :2]; b = S[:, 2:]; ab = b - a; L2 = (ab**2).sum(1) + 1e-12
    out = []
    for p in pts:
        t = np.clip(((p - a) * ab).sum(1) / L2, 0, 1); q = a + ab * t[:, None]
        out.append(np.sqrt(((q - p)**2).sum(1)).min())
    return np.array(out)
W1 = rect(7.3, 17.9, -14.25, 14.0)
SLOT = np.array([[3.55, 1.0], [17.65, 1.0], [17.65, 1.6], [17.5, 3.0], [17.335, 9.3], [11.565, 15.118], [9.788, 18.87], [8.835, 22.282], [7.919, 23.746], [3.962, 18.5], [3.7, 8.5], [3.55, 7.7]])
def densify(P, n=40):
    out = []
    for i in range(len(P)):
        a, b = P[i], P[(i+1) % len(P)]
        for s in np.linspace(0, 1, n, endpoint=False): out.append(a + (b - a) * s)
    return np.array(out)
rep = {}
for z in [215.0, 220.0, 227.0, 235.0, 240.0, 246.5]:
    # FS band fill (z 215-247): wing(z) + [(sec240 + W1_z215 + sec215^SLIT) ^ box(d' -14..34, t' 1.4..26)]
    s240 = WS.slice(240.0); s215 = WS.slice(215.2)
    fill = (s240 + (s215 ^ W1) + (s215 ^ rect(8.0, 26.0, 11.0, 22.0))) ^ rect(-14, 34, 1.4, 26.0)
    nosef = (s240 + (s215 ^ W1)) ^ rect(-14, 34, -14.67, 2.6)
    sec = (WS.slice(z) + fill + nosef) ^ rect(-40, 60, -40, 80)
    S = segs(sec)
    P = densify(SLOT)
    back = P[P[:, 1] > 9.0]; d_back = dist(back, S)
    outer = P[(P[:, 0] > 17.2) & (P[:, 1] > 2.0)]; d_out = dist(outer, S)
    inner = P[(P[:, 0] < 4.1) & (P[:, 1] > 2.0)]; d_in = dist(inner, S)
    # groove bottom face d' 22.2 at t' 4.98..9 -> skin to the outer face
    g = np.c_[np.full(20, 22.2), np.linspace(5.0, 9.0, 20)]; d_g = dist(g, S)
    # nose: outer face at t' -6.3 (button centre) and the inner face along t' -14..1.7 at the rod height
    xs = sec.bounds()
    rows = {}
    for t in [-13.0, -10.0, -6.3, -2.0, 1.0]:
        line = rect(-40, 60, t - 0.05, t + 0.05) ^ sec
        b = line.bounds(); rows[t] = [round(b[0], 2), round(b[2], 2)]
    rep[z] = dict(S1_web_min=round(d_back.min(), 2), slot_outer_wall_min=round(d_out.min(), 2), slot_inner_wall_min=round(d_in.min(), 2),
                  groove_skin_min=round(d_g.min(), 2), nose_d_extent_by_t=rows)
    print(z, rep[z])
json.dump(rep, open('walls_S.json', 'w'), indent=1)
