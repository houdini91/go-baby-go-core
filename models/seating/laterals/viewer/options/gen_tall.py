# Option C (review page): TALL TONGUE. The as-built front-edge tongue (TONGUE polygon of gbg_sidepad_r5.fs, 15.8 wide, 24.5 deep)
# made 64 mm tall inside the old mount's height envelope: nose band z 207-276 (69), tongue z 209.5-273.5, the cheek/swell
# envelope carried up to z 281. Same nose look, push button and latch (the as-built nose with latch A stays at z 207-247; the nose
# above it is the wing's own front edge). The notch rim and the slot mouth get the rounded, flared "exhaust tip" lip.
# Writes tall_wing.stl, tall_kit.stl, tall.json. Run: ../.venv/bin/python gen_tall.py
import sys, json, numpy as np, shapely.geometry as sg
from lib5 import *
VAR = (sys.argv[1] if len(sys.argv) > 1 else 'C').upper()   # C = tall tongue (R2 root + 2 Ø2 corner rods); D = C + 2 Ø8 prongs
PRE = 'tall' if VAR == 'C' else 'tallp'
W = wing_full()
car = load_fit('GBG-SP-AR-L-L-LO-r5'); btn = load_fit('GBG-SP-BT-L-r5')
NZ = (207.0, 281.0) if VAR == 'C' else (196.0, 297.0); TZ = (214.3, 278.3); ZTOP = 293.0; BTN_DZ = 25.0   # ../review_r5/cad/tall_tongue.md
PRONGS = [] if VAR == 'C' else [(5.5, 204.0, 20.0), (5.5, 288.7, 32.0)]          # (d', z, bore end t'): lower 18 deep, upper 30 deep
TONGUE = [[2.0, 1.0], [17.8, 1.0], [17.8, 25.48], [9.0, 25.48], [2.2, 16.57], [2.0, 8.5]]
tg = sg.Polygon(TONGUE); slot2d = tg.buffer(0.2, join_style=2)
lo, hi = (-16.0, -22.0, 188.0), (32.0, 38.0, 306.0)
# 1) carry the cheek/swell envelope up to z 281 (blended into the blade above, R ~ 8)
env = [p for p in W.slice(262.0).to_polygons() if len(p) > 2]
prism = mf.CrossSection([np.asarray(p) for p in env], mf.FillRule.EvenOdd).offset(-0.05, mf.JoinType.Round).extrude(52.0).translate([0, 0, 250.0]) ^ box(-40, 60, -20, 40, 250, 302)
G = Grid(W, lo, hi, h=0.2, blur=1.5)
Gp = Grid(prism, lo, hi, h=0.2, blur=1.5)
cap = np.minimum(np.minimum(Gp.F, ZTOP - G.Z), G.Z - 249.0)
A = np.minimum(np.minimum(smax(G.F, cap, 8.0), 0.6 - G.F), cap + 10.0)
W2 = W + G.mesh(A)
# 2) notch + slot with rounded rims, on the grown wing
G2 = Grid(W2, lo, hi, h=0.2, blur=1.5)
from shapely import distance as sdist, points as spoints
DD, TT = np.meshgrid(G2.ax[0], G2.ax[1], indexing='ij'); P2 = np.c_[DD.ravel(), TT.ravel()]
ins = np.asarray([slot2d.contains(sg.Point(p)) for p in P2[:1]])     # warm-up (shapely)
from shapely import contains_xy
inside = contains_xy(slot2d, P2[:, 0], P2[:, 1]).reshape(DD.shape)
dist = np.asarray(sdist(slot2d.exterior, spoints(P2)), np.float32).reshape(DD.shape)
slotF2 = np.where(inside, dist, -dist)[:, :, None]                  # 2D signed distance of the slot outline (positive inside)
slotF = np.minimum(np.minimum(slotF2, G2.Z - (TZ[0] - 0.2)), (TZ[1] + 0.2) - G2.Z)
notch = sdf_box(G2, (-60, -60, NZ[0]), (60, 2.0, NZ[1]))
tool = np.maximum(notch, np.minimum(slotF, 30.0 - G2.T))
for d_, z_, te in PRONGS: tool = np.maximum(tool, sdf_cyl_t(G2, d_, z_, 4.1, -5.0, te))
k = 4.0
Res = smin(G2.F, -tool, k)
WC = W2 - G2.mesh(np.minimum(np.minimum(-Res, tool + 1.6 * k), G2.F + 0.6))
btnU = btn.translate([0, 0, BTN_DZ])                                  # latch A moved up (button centre z 252)
WC = WC - btnU - btnU.translate([-4.0, 0, 0])                       # the button-catch's tooth path (approximate groove)
WC = max(WC.decompose(), key=lambda m: m.volume())
# 3) carrier: as-built nose (latch inside) for z <= 246.7, the nose above it from the field, the tall tongue, the as-built arm
noseF = np.minimum(np.minimum(G2.F, -Res - 0.25), 2.5 - G2.T)
nose = G2.mesh(noseF)
tongue = mf.CrossSection([np.asarray(TONGUE)]).extrude(TZ[1] - TZ[0]).translate([0, 0, TZ[0]])
arm = car ^ box(-60, 60, -80, T_FRONT + 3.0, 150, 260)                     # the as-built arm (+ 3 mm into the nose so they fuse)
pocket = btnU + btnU.translate([-4.0, 0, 0])                               # button pocket + travel in the nose (approximate)
for d_, z_, te in PRONGS: noseF = np.minimum(noseF, np.sqrt((G2.D - d_) ** 2 + (G2.Z - z_) ** 2) - 4.1)
nose = G2.mesh(noseF) if PRONGS else nose
kit = (nose + tongue + arm) - pocket
# carbon: 2 Ø2 corner rods across the tongue root (16 each side) and, for D, the two Ø8 prongs (rounded tips)
rodl = [mf.Manifold.cylinder(32.0, 1.0, 1.0, 16).rotate([-90, 0, 0]).translate([15.3, -14.0, z_]) for z_ in (TZ[0] + 2.5, TZ[1] - 2.5)]
for d_, z_, te in PRONGS:
    rodl.append(mf.Manifold.cylinder(te - 0.6 - (T_FRONT + 3.0), 4.0, 4.0, 48).rotate([-90, 0, 0]).translate([d_, T_FRONT + 3.0, z_]) + mf.Manifold.sphere(4.0, 48).scale([1, 0.3, 1]).translate([d_, te - 0.6, z_]))
rods = mf.Manifold.batch_boolean(rodl, mf.OpType.Add)
kit = kit - rods
save_W(rods, PRE + '_rods.stl')
kit = max(kit.decompose(), key=lambda m: m.volume())
WC = max((WC - box(-40, 60, -40, 0.5, NZ[0] + 1.0, NZ[1] - 1.0)).decompose(), key=lambda m: m.volume())   # clear slivers inside the notch
save_W(WC, PRE + '_wing.stl'); save_W(kit, PRE + '_kit.stl')
# clearances to the protected features
src = json.load(open('sections.json'))
s1 = sg.Polygon(src['s1'][0])
W0 = wing()
def holes_at(z):
    ps = [sg.Polygon(p) for p in W0.slice(z).to_polygons() if len(p) > 2]
    return ps
s240 = W0.slice(240.0)
# teardrop: small hole near (d -2, t 39); S2: slit near (-7, 47)
polys = [np.asarray(p) for p in s240.to_polygons() if len(p) > 2]
tear = [sg.Polygon(p) for p in polys if sg.Polygon(p).area < 60 and abs(np.mean(p[:, 1]) - 39) < 6]
rep = dict(nose_z=NZ, tongue_z=TZ, tongue=f'{17.8 - 2.0:.1f} x {TZ[1] - TZ[0]:.0f}', swell_to_z=ZTOP,
           slot_to_S1=round(slot2d.distance(s1), 2), slot_to_teardrop=round(min(slot2d.distance(t) for t in tear), 2) if tear else None,
           notch_bottom_to_hole_z186=round(NZ[0] - 186.6 - 1.65, 2), hole_column_x_gap=round(float(to_tm(tf(kit, S2W)).bounds[0][0]) - 75.5, 1),
           clash=round((kit ^ WC).volume(), 2))
def sec(m, z): return [np.round(np.asarray(p), 2).tolist() for p in m.slice(z).to_polygons() if len(p) > 2]
win = box(-30, 40, -40, 45, 150, 300)
save_W(btnU, 'tall_btn.stl'); save_W(load_fit('GBG-SP-TS-r5_L').translate([0, 0, BTN_DZ]), 'tall_sleeve.stl')
cfg = dict(title='tall tongue' + (' + prongs' if PRONGS else ''), wing=PRE + '_wing', kit=[PRE + '_kit'], rods=PRE + '_rods', btn='tall_btn', sleeve='tall_sleeve', press=4.0, btnAxisS=[-1, 0, 0], approachS=[0, -70, 0], viewZ=244,
           closeCam=[[34, -100, 278], [8, 2, 242]], cutZ=230,
           labels=[['tall tongue, 15.8 × 64', [10, 14, 266]], ['push button', [26, -6, 262]], ['74 mm nose', [8, -14, 278]], ['tall slot (rounded mouth)', [10, 6, 272], 'w']],
           guides=[[[10, 25.5 - 70, 246], [10, 1, 246]]])
section = dict(wing=sec(WC ^ win, 260.0), kit=sec(kit ^ win, 260.0), forces=[[19.5, 5, -1, 'outer wall'], [0.5, 13, 1, 'inner wall']], lever=[5, 13, '≈ 8 mm'],
               notes=[[22, -2, 'tongue', 'tl b'], [-44, -14, 'same lever, twice as tall:', 'tl b'], [-44, -10, 'about 1.65× the strength', 'tl']])
if PRONGS:
    cfg['labels'] += [['Ø8 carbon prong', [5.5, 12, 204]], ['Ø8 carbon prong', [5.5, 14, 288.7]], ['round holes (rounded mouths)', [5.5, 4, 300], 'w']]
    cfg['guides'] += [[[d_, te - 1 - 70, z_], [d_, 2, z_]] for d_, z_, te in PRONGS]
    section['rods'] = sec(rods, 204.0)
    cfg['labels'] = [l if l[0] != '74 mm nose' else ['101 mm nose', [8, -14, 294]] for l in cfg['labels']]
json.dump(dict(cfg=cfg, section=section, report=rep), open(PRE + '.json', 'w'), indent=1)
print(json.dumps(rep, indent=1))
