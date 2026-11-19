# Meshes for the main review page (viewer/site), all from the REAL r5cad build (repaired wing). Writes ../site/main_*.stl
# helpers and ../site/sections.json. Then run ../pack.py.
#   main_wing: the r5cad wing; main_carL: carrier long arm LOW (fitted); main_carH: HIGH = the LOW carrier mirrored about z 227 in
#   the lean-free frame (as the FS builds it); main_flip_over / _gap: where the LOW carrier turned over misses the notch;
#   main_cap: blank cap (approximation: the carrier's nose + the first 9 mm of its tongue; the real cap adds crush ribs + nail notch)
import os, json, numpy as np, trimesh, manifold3d as mf
from lib5 import *
OUT = os.path.join(V, 'site')
W = wing(); car = load_fit('GBG-SP-AR-L-L-LO-r5')
C = 227.0
save_W(W, os.path.join(V, 'mock5b', 'main_wing.stl'))
MzS = np.diag([1.0, 1.0, -1.0, 1.0]); MzS[2, 3] = 2 * C
carH = car.transform(MzS[:3, :4]); save_W(carH, os.path.join(V, 'mock5b', 'main_carH.stl'))
MzW = np.diag([1.0, 1.0, -1.0, 1.0]); MzW[2, 3] = 2 * C
flipS = W2S @ MzW @ S2W
nose = car ^ box(-40, 60, T_FRONT - 0.5, 2.0, 200, 254)
noseF = nose.transform(flipS[:3, :4])
ends = box(-40, 60, -40, 60, 190, C - 8) + box(-40, 60, -40, 60, C + 8, 265)
save_W((noseF - nose) ^ ends, os.path.join(V, 'mock5b', 'main_flip_over.stl')); save_W((nose - noseF) ^ ends, os.path.join(V, 'mock5b', 'main_flip_gap.stl'))
cap = car ^ box(-40, 60, T_FRONT - 0.5, 11.0, 205, 249)
save_W(cap, os.path.join(V, 'mock5b', 'main_cap.stl'))
# latch cutaway sections at z 230 (tip frame = S at this height within 0.2 mm)
def segs(mS, z=230.0):
    out = []
    for p in mS.slice(z).to_polygons():
        p = np.asarray(p)
        for a, b in zip(p, np.roll(p, -1, 0)):
            if -30 < a[0] < 40 and -45 < a[1] < 45: out.append([[round(a[0], 2), round(a[1], 2)], [round(b[0], 2), round(b[1], 2)]])
    return out
btn = load_fit('GBG-SP-BT-L-r5'); slv = load_fit('GBG-SP-TS-r5_L')
json.dump(dict(note='round 5 as built (r5cad), cut at z 230, S frame (d, t) mm', wing=segs(W), carrier=segs(car), button=segs(btn), sleeve=segs(slv)),
          open(os.path.join(OUT, 'sections.json'), 'w'))
bb = btn.bounding_box(); print('button S bounds', np.round(bb, 2))
print('done')
