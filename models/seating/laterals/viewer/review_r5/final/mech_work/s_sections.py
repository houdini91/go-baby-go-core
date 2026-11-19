# Mechanical review r5: section the ORIGINAL left wing (repaired, as gen_r5 uses it) in the S frame and list
#  (a) voids (holes/slits) in the tip box the FS fills (d' -14..34, t' -20..40) per z
#  (b) regions solid in the z-240 section but void at z  -> the FS 'p240' lean-prism would FILL them (z 207-270)
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
B = rect(-14, 34, -20, 40)
def comps(cs):
    out = []
    for c in cs.decompose():
        if c.area() < 0.3: continue
        b = c.bounds()
        out.append(dict(area=round(c.area(), 1), d=[round(b[0], 1), round(b[2], 1)], t=[round(b[1], 1), round(b[3], 1)]))
    return out
def voids(cs):
    # holes = bounding box minus section, components not touching the box border
    hb = (B - cs)
    res = []
    for c in hb.decompose():
        b = c.bounds()
        if c.area() < 0.3: continue
        inner = b[0] > -13.9 and b[2] < 33.9 and b[1] > -19.9 and b[3] < 39.9
        res.append(dict(area=round(c.area(), 1), d=[round(b[0], 1), round(b[2], 1)], t=[round(b[1], 1), round(b[3], 1)], enclosed=inner))
    return res
ref240 = WS.slice(240.0) ^ B
ref215 = WS.slice(215.2) ^ B
res = {}
for z in [186, 200, 204, 207, 210, 213, 215.2, 218, 222, 227, 232, 236, 240, 244, 247, 250, 253, 256, 258, 260.7, 263, 266, 270, 273]:
    S = WS.slice(z) ^ B
    res[z] = dict(area=round(S.area(), 1), voids=voids(S), filled_by_240prism=comps(ref240 - S), filled_by_215prism=comps(ref215 - S))
json.dump(res, open('wing_sections_S.json', 'w'), indent=1)
for z, v in res.items():
    print(z, 'section area', v['area'])
    for h in v['voids']: print('    void   ', h)
    for h in v['filled_by_240prism']: print('    240>z  ', h)
