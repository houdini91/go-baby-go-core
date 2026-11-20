# Hidden-mount study, layout H5: SIDE POCKET with hook-and-drop, on the parent's (outer) side of the wing.
#   The front edge, the child-side face and the top stay whole (old notch filled: wing_full). A rounded pocket is cut into the
#   outer face of the cheek: t' -8..22 (30 long), z 210..252 (42: the plug's 34 + an 8 mm drop), down to d' 6 (10 mm inner wall
#   left), with a rounded, flared "exhaust tip" rim. The carrier's plug goes in sideways (inboard, -d'), then drops 8 mm so a
#   hook ledge at its foot sits under a lip at the pocket floor (plastic carries the outward lean); the push button only blocks
#   the lift. A flange 6 mm proud on the outer face covers the 8 mm drop gap. Arm: a sweep of the r5 22 x 31 section from the
#   flange's front, round the front edge, to the depth-piece tenon (long arm, LOW).
# MOCK FIDELITY: real repaired Onshape wing, real booleans, rounded lips from a fine local distance field; the hook ledge, the
# latch groove and the spring are not modelled. Run: ../.venv/bin/python gen_h5.py   (writes h5_*.stl, h5.json)
import json, math, numpy as np, shapely.geometry as sg
from lib5 import *
W = wing_full()
PT = (-8.0, 22.0); PZ = (210.0, 252.0); PD = 6.0; DROP = 8.0; CLR = 0.25; FLANGE = 6.0
lo, hi = (-10.0, -20.0, 196.0), (44.0, 34.0, 266.0)
G = Grid(W, lo, hi, h=0.2, blur=1.5)
pocket = sdf_box(G, (PD, PT[0], PZ[0]), (80.0, PT[1], PZ[1]), 4.0)
k = 5.0
Res = smin(G.F, -pocket, k)
Rm = G.mesh(np.minimum(np.minimum(-Res, pocket + 1.6 * k), G.F + 0.6))
WH = max((W - Rm - box(PD + 1.0, 80, PT[0] + 2, PT[1] - 2, PZ[0] + 2, PZ[1] - 2)).decompose(), key=lambda m: m.volume())   # clear slivers inside the pocket
save_W(WH, 'h5_wing.stl')
# carrier: plug (seated low: z 210.25 .. 244) + a flange round the pocket mouth, 6 mm proud, feathered into the face
plugT = sdf_box(G, (PD + CLR, PT[0] + CLR, PZ[0] + CLR), (80.0, PT[1] - CLR, PZ[1] - DROP - CLR), 3.75)
plug = np.minimum(plugT, -Res - CLR)
out = G.D > 10.0
around = sdf_box(G, (-60, PT[0] - 7.0, PZ[0] - 7.0), (90, PT[1] + 7.0, PZ[1] + 7.0), 9.0)
TH = FLANGE * sm(around / 4.0) * out
flange = np.minimum(Res + TH, -Res - 0.1)
kitF = smax(plug, flange, 2.0)
kit = max(G.mesh(kitF).decompose(), key=lambda m: m.volume())
# arm: from the flange front (outer side) forward, round the front edge, to the tenon (x_e 127.8, y 38, z 183, heading -x)
d_out = 24.6 + FLANGE - 2.0
p0 = QSw(d_out, PT[0] - 2.0, 232.0); dT = QSw(d_out, PT[0] - 12.0, 232.0) - p0
p3 = np.array([127.8, 38.0, 183.0])
path = bezier(p0, p0 + dT * 3.2, p3 + np.array([8.0, -6.0, 26.0]), p3, 80)   # kept inside the arm rest line (x <= 165)
arm = tf(sweep_pill(path, 22.0, 31.0, 96, flare=lambda u: 1.0 + 0.2 * math.exp(-(u / 0.08) ** 2)), W2S)
car = load_fit('GBG-SP-AR-L-L-LO-r5'); tenon = car ^ box(-80, 80, -120, -40, 150, 215)     # the as-built tenon end (for the depth piece)
kitall = max(((kit + arm + tenon) - WH).decompose(), key=lambda m: m.volume())
save_W(kitall, 'h5_kit.stl')
# push button on the flange's outer face (12 x 24 stadium, crowned), pressed inboard (-d')
st = mf.CrossSection.square([12.0 - 12.0, 24.0 - 12.0], center=True).offset(6.0, mf.JoinType.Round, circular_segments=48)
face = 24.6 + FLANGE
btn = (st.extrude(4.0).scale([1, 1, 1]))
btn = tf(btn, np.array([[0, 0, 1, face - 3.2], [1, 0, 0, 7.0], [0, 1, 0, 231.0], [0, 0, 0, 1]], float))   # extrude along d'
kitall = kitall - tf(st.offset(0.3, mf.JoinType.Round).extrude(6.0), np.array([[0, 0, 1, face - 4.0], [1, 0, 0, 7.0], [0, 1, 0, 231.0], [0, 0, 0, 1]], float))
save_W(kitall, 'h5_kit.stl'); save_W(btn, 'h5_btn.stl')
# clearances to the protected features (S frame, at the pocket heights)
src = json.load(open('sections.json')); s1 = sg.Polygon(src['s1'][0])
pk = sg.box(PD, PT[0], 60, PT[1]).buffer(0)
rep = dict(pocket=dict(t=PT, z=PZ, floor_d=PD, inner_wall=round(PD - (-4.4), 1), front_wall=round(PT[0] - T_FRONT, 1), drop=DROP),
           pocket_to_S1=round(pk.distance(s1), 2), pocket_to_hole_z186=round(PZ[0] - 186.6 - 1.65, 2),
           clash_kit_wing=round((kitall ^ WH).volume(), 2), kit_x_max=round(float(to_tm(tf(kitall, S2W)).bounds[1][0]), 1))
def sec(m, z): return [np.round(np.asarray(p), 2).tolist() for p in m.slice(z).to_polygons() if len(p) > 2]
win = box(-30, 50, -40, 45, 150, 300)
cfg = dict(title='side pocket, hook-and-drop', wing='h5_wing', kit=['h5_kit'], btn='h5_btn', sleeve='h5_btn', press=4.0, btnAxisS=[-1, 0, 0],
           approachS=[70, 0, 8], viewZ=232, cutZ=228, cutView='top', closeCam=[[110, -40, 250], [20, 7, 231]],
           labels=[['plug (hook at its foot)', [20, 7, 247]], ['push button (parent side)', [34, 7, 250]], ['flange', [30, -10, 256]],
                   ['side pocket (rounded mouth)', [26, 7, 258], 'w']],
           guides=[[[6 + 70, 7, 236], [24, 7, 236]]])
section = dict(wing=sec(WH ^ win, 228.0), kit=sec(kitall ^ win, 228.0), forces=[[22, -5, -1, 'front wall'], [8, 20, 1, 'back wall']], lever=[-5, 20, '≈ 25 mm'],
               notes=[[-44, -14, 'plug 30 long front-to-back:', 'tl b'], [-44, -10, 'yaw bends it the strong way', 'tl']])
json.dump(dict(cfg=cfg, section=section, report=rep), open('h5.json', 'w'), indent=1)
print(json.dumps(rep, indent=1))
