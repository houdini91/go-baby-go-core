# Head-to-head option F (front edge, strengthened), refined for looks: the r5b nose / tongue / deep-catch latch made tall
# (tongue 15.8 x 80 z 214.3-294.3, R2 outer root, 2 Ø2 corner rods; nose band z 207-297, or 196-297 with the prongs) on the
# real r5cad arm, the button moved up 33 mm (r5b parts). The wing's square r5 slab is replaced by ONE smooth swell along the
# front edge (8.5 proud at the nose, the old slab's outer face, fading to the wing by t' 33 and over 24 mm above and below),
# so the nose is just the front part of that swell: cut by the notch, rims rolled (R ~1.5), hairline seams when connected.
# One distance field over the whole wing tip (as gen_h2h_S.py). Writes h2h_F[P]_wing/kit/rods/btn/sleeve.stl and h2h_F.json.
# Run: ../.venv/bin/python gen_h2h_F.py
import os, sys, json, numpy as np
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'options'))
from lib5 import *
import shapely.geometry as sg
from shapely import contains_xy, distance as sdist, points as spoints
from scipy.ndimage import gaussian_filter1d
os.chdir(os.path.dirname(os.path.abspath(__file__)))
TZ = (214.3, 294.3); BTN_DZ = 33.0; PROUD = 8.5; KR = 3.0           # KR: rim rounding (smooth-min k; radius ~1.5)
TONGUE = [[2.0, 1.0], [17.8, 1.0], [17.8, 25.48], [9.0, 25.48], [2.2, 16.57], [2.0, 8.5]]
VARS = {'h2h_F': dict(NZ=(207.0, 297.0), PRONGS=[]), 'h2h_FP': dict(NZ=(196.0, 297.0), PRONGS=[(5.5, 204.0, 20.0), (5.5, 288.7, 32.0)])}
W = wing_full()
sec = [p for p in W.slice(195.0).to_polygons() if len(p) > 2]
pr = mf.CrossSection([np.asarray(p) for p in sec], mf.FillRule.EvenOdd).extrude(84.6).translate([0, 0, 196.7])
W0 = max(((W - box(12.0, 40, -20, 45, 197.0, 281.0)) + (pr ^ box(11.7, 40, -20, 45, 196.7, 281.3))).decompose(), key=lambda m: m.volume())
KLO, KHI = (-12.0, -30.0, 176.0), (40.0, 45.0, 393.0)    # back seam at t' 45, on the sloping back face (B = 0 there)
G = Grid(W0, (-14.0, -32.0, 174.0), (42.0, 47.0, 395.0), h=0.25, blur=1.5)
wz = np.exp(-((G.Z - 281.0) / 6.0) ** 2); G.F = (G.F * (1 - wz) + gaussian_filter1d(G.F, 3.0 / 0.25, axis=2) * wz).astype(np.float32)
def ss(x): x = np.clip(x, 0.0, 1.0); return x * x * x * (x * (6 * x - 15) + 10)     # smootherstep (C2)
B = PROUD * ss((44.0 - G.T) / 18.0) * ss((G.Z - 160.0) / 45.0) * ss((345.0 - G.Z) / 45.0) * ss((G.D - 11.0) / 4.0)   # outer skin only (keeps slit S1 open); plateau to t' 26 (covers the tongue's outer wall), long C2 fades
FB = G.F + B
Kin = sdf_box(G, KLO, (KHI[0], KHI[1] + 0.3, KHI[2]), 0.0)
WB = W0 - box(KLO[0], KHI[0], KLO[1], KHI[1], KLO[2], KHI[2])
slot2d = sg.Polygon(TONGUE).buffer(0.2, join_style=2)
DD, TT = np.meshgrid(G.ax[0], G.ax[1], indexing='ij'); P2 = np.c_[DD.ravel(), TT.ravel()]
ins = contains_xy(slot2d, P2[:, 0], P2[:, 1]).reshape(DD.shape)
dist = np.asarray(sdist(slot2d.exterior, spoints(P2)), np.float32).reshape(DD.shape)
slot2 = np.where(ins, dist, -dist)[:, :, None]
slotF = np.minimum(np.minimum(slot2, G.Z - (TZ[0] - 0.2)), (TZ[1] + 0.2) - G.Z)
car = load_fit('GBG-SP-AR-L-L-LO-r5'); btn = load_fit('GBG-SP-BT-L-r5'); slv = load_fit('GBG-SP-TS-r5_L')
btnU = btn.translate([0, 0, BTN_DZ]); pocket = btnU + btnU.translate([-4.0, 0, 0])
cb = btnU.bounding_box()                                  # open the pocket through the skin only at the cap (the bar and tooth
pocket = (pocket ^ box(-60, 21.0, -60, 60, 150, 330)) + (pocket ^ box(21.0, 60, -60, 60, cb[2] - 0.5, cb[5] + 0.5))   # run under it)
arm = (car ^ box(-80, 80, -120, -15.5, 150, 260)) + (car ^ box(-80, 18.0, -120, T_FRONT + 0.5, 150, 260)) + (car ^ box(-80, -6.5, -120, T_FRONT + 3.0, 150, 260))   # the as-built arm; its old flat nose face (d' > 18 at the front) is replaced by the new nose; child-side flare kept
tongue = mf.CrossSection([np.asarray(TONGUE)]).extrude(TZ[1] - TZ[0]).translate([0, 0, TZ[0]])
rep = {}
for pre, V in VARS.items():
    NZ, PRONGS = V['NZ'], V['PRONGS']
    notch = sdf_box(G, (-60, -60, NZ[0]), (60, 2.0, NZ[1]))
    tool = np.maximum(notch, np.minimum(slotF, 30.0 - G.T))
    for d_, z_, te in PRONGS: tool = np.maximum(tool, sdf_cyl_t(G, d_, z_, 4.1, -5.0, te))
    Res = smin(FB, -tool, KR)
    wingm = max((WB + G.mesh(np.minimum(Res, Kin))).decompose(), key=lambda m: m.volume())
    wingm = max((wingm - box(-60, 60, -60, 1.5, NZ[0] + 0.6, NZ[1] - 0.6)).decompose(), key=lambda m: m.volume())   # clear a CAD sliver (an old bore's skin) from the notch
    # nose = the swell envelope inside the notch (0.25 off its faces), rims rolled like the wing's: hairline seams
    noseF = smin(FB, notch - 0.25, KR)
    for d_, z_, te in PRONGS: noseF = np.minimum(noseF, np.sqrt((G.D - d_) ** 2 + (G.Z - z_) ** 2) - 4.1)
    nose = G.mesh(np.minimum(noseF, sdf_box(G, (-60, -60, NZ[0] - 2), (60, 1.9, NZ[1] + 2))))
    kit = ((nose + tongue + arm) - pocket)
    rodl = [mf.Manifold.cylinder(32.0, 1.0, 1.0, 16).rotate([-90, 0, 0]).translate([15.3, -14.0, z_]) for z_ in (TZ[0] + 2.5, TZ[1] - 2.5)]
    for d_, z_, te in PRONGS:
        rodl.append(mf.Manifold.cylinder(te - 0.6 - (T_FRONT + 3.0), 4.0, 4.0, 48).rotate([-90, 0, 0]).translate([d_, T_FRONT + 3.0, z_]) + mf.Manifold.sphere(4.0, 48).scale([1, 0.3, 1]).translate([d_, te - 0.6, z_]))
    rods = mf.Manifold.batch_boolean(rodl, mf.OpType.Add)
    kit = max((((kit - rods) - wingm) ^ box(-80, 80, -120, 60, 167.5, 300)).decompose(), key=lambda m: m.volume())   # drops boolean slivers below the arm
    save_W(wingm, pre + '_wing.stl'); save_W(kit, pre + '_kit.stl'); save_W(rods, pre + '_rods.stl')
    save_W(btnU, pre + '_btn.stl'); save_W(slv.translate([0, 0, BTN_DZ]), pre + '_sleeve.stl')
    s1 = sg.Polygon(json.load(open('../options/sections.json'))['s1'][0])
    rep[pre] = dict(nose_z=NZ, tongue_z=TZ, slot_to_S1=round(slot2d.distance(s1), 2), notch_bottom_to_hole_z186=round(NZ[0] - 186.6 - 1.65, 2),
                    kit_x_max=round(float(to_tm(tf(kit, S2W)).bounds[1][0]), 1), clash=round((kit ^ wingm).volume(), 2), prongs=PRONGS)
    print(pre, rep[pre], flush=True)
json.dump(rep, open('h2h_F.json', 'w'), indent=1)
