# Head-to-head option S (side, twist-in bayonet) on the real chair: the r5cad wing with its square mount slab cut back
# (as gen_exh3d.py) and a round "exhaust" swell (axisymmetric about the plug axis, so the kit's collar can turn on it).
# Mouth: stadium 33 x 54 = the Ø33 bore + the two lug entry slots (top, bottom), rolled lip R ~2.5, lead-in flare.
# Inside: bore Ø33 to a floor at d' 5; two lug chambers (d' 5.5-14.3, R 16.5-27) swept 30° (+theta, from +t' toward +z)
# under ledges d' 14.3-19, with a helical ramp on the chamber floor at the entry. Kit (shown LOCKED): the real r5cad arm
# (cut 0.3 short of the wing's front face) + a 14 mm cheek round the front edge + a stadium collar seated on the lip +
# the Ø32.4 plug with two lugs (26° span, 8.5 thick, helical lead face) + the r5b button re-oriented into the collar at R 17.
# The cheek + collar is one smooth mound (refined for looks), its front wrap kept below the axis so the kit can turn.
# Writes h2h_S_wing.stl, h2h_S_kit.stl, h2h_S_btn.stl, h2h_S.json.  Run: ../.venv/bin/python gen_h2h_S.py
import os, sys, json, math, numpy as np
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'options'))
from lib5 import *
import shapely.geometry as sg
os.chdir(os.path.dirname(os.path.abspath(__file__)))
TC, ZC, R0, R1, FLOOR, TURN, SPAN = 7.0, 236.0, 16.5, 26.5, 5.0, 30.0, 26.0
LUG_D = (5.5, 14.3); PROUD = 5.5; SW = 34.0           # swell height and radius (flat top to ~R 30)
W = wing_full()
sec = [p for p in W.slice(195.0).to_polygons() if len(p) > 2]
pr = mf.CrossSection([np.asarray(p) for p in sec], mf.FillRule.EvenOdd).extrude(84.6).translate([0, 0, 196.7])
W0 = max(((W - box(12.0, 40, -20, 45, 197.0, 281.0)) + (pr ^ box(11.7, 40, -20, 45, 196.7, 281.3))).decompose(), key=lambda m: m.volume())
KLO, KHI = (-12.0, -22.0, 176.0), (46.0, 33.5, 393.0)
G = Grid(W0, (-14.0, -24.0, 174.0), (48.0, 36.0, 395.0), h=0.25, blur=1.5)
from scipy.ndimage import gaussian_filter1d
wz = np.exp(-((G.Z - 281.0) / 6.0) ** 2); G.F = (G.F * (1 - wz) + gaussian_filter1d(G.F, 3.0 / 0.25, axis=2) * wz).astype(np.float32)
T_ = G.T - TC; Z_ = G.Z - ZC; Rr = np.sqrt(T_ ** 2 + Z_ ** 2); TH = np.arctan2(Z_, T_)
B = PROUD * np.exp(-(Rr / SW) ** 4) * sm((G.D - 11.0) / 4.0)          # outer skin only (keeps slit S1 open)
FB = G.F + B
Kin = sdf_box(G, KLO, (KHI[0], KHI[1] + 0.3, KHI[2]), 0.0)
WB = W0 - box(KLO[0], KHI[0], KLO[1], KHI[1], KLO[2], KHI[2])
def angd(th, c): return np.abs(np.angle(np.exp(1j * (th - c))))        # angular distance
# ---- tool (positive inside the cavity)
stad = R0 - np.sqrt(T_ ** 2 + np.maximum(np.abs(Z_) - (27.0 - R0), 0) ** 2)     # stadium 33 x 54 (2D, + inside)
bore = R0 - Rr
mouth = np.minimum(stad + 0.0 * G.D, G.D - LUG_D[1])                         # the lug slots run from the ledge up to the face
cyl = np.minimum(bore, G.D - FLOOR)
cham = None
for c in (math.pi / 2, -math.pi / 2):
    a0, a1 = c - math.radians(SPAN / 2 + 1.0), c + math.radians(TURN + SPAN / 2 + 1.0)
    mid = (a0 + a1) / 2; half = (a1 - a0) / 2
    ang = half - angd(TH, mid)                                                # + inside the swept sector
    ramp = (LUG_D[0]) + 2.0 * np.clip((angd(TH, a0) / math.radians(SPAN)), 0, 1)   # helical floor at the entry (2 mm rise)
    ch = np.minimum(np.minimum(np.minimum(ang * Rr, R1 + 0.5 - Rr), Rr - (R0 - 1.0)), np.minimum(G.D - (LUG_D[0] - 0.3), LUG_D[1] - G.D))
    cham = ch if cham is None else np.maximum(cham, ch)
tool = np.maximum(np.maximum(smax(cyl, mouth, 3.0), cham), -9e9)
Res = smin(FB, -tool, 5.0)                                                     # rolled lip and lead-in flare (~R 2.5)
tip = G.mesh(np.minimum(Res, Kin))
wing = max((WB + tip).decompose(), key=lambda m: m.volume())
save_W(wing, 'h2h_S_wing.stl'); print('wing', flush=True)
# ---- kit (locked pose)
face = 16.1 + PROUD                                                            # outer face at the collar (d' ~21.6)
plug = np.minimum(np.minimum(R0 - 0.3 - Rr, G.D - (FLOOR + 0.3)), face + 1.0 - G.D)
lugs = None
for c in (math.pi / 2 + math.radians(TURN), -math.pi / 2 + math.radians(TURN)):
    ang = math.radians(SPAN / 2) - angd(TH, c)
    lead = (G.D - (LUG_D[0] + 0.3)) + 1.2 * (ang * Rr)                        # helical lead face (chamfer on the leading corner)
    lg = np.minimum(np.minimum(np.minimum(ang * Rr, R1 - Rr), Rr - (R0 - 1.0)), np.minimum(lead, (LUG_D[1] - 0.3) - G.D))
    lugs = lg if lugs is None else np.maximum(lugs, lg)
def ss(x): x = np.clip(x, 0.0, 1.0); return x * x * x * (x * (6 * x - 15) + 10)     # smootherstep (C2)
# one smooth mound over the face (no separate collar): a rounded patch in (t', z) from the collar round the front edge,
# 4 mm over the mouth, thickening to 14 mm at the front corner (strength: the wrap carries the whole pad wrench), its rim a
# rolled R ~1.5 lip. The wrap onto the front face stays below the axis (z' <= ZC - 4) so the kit can turn without hitting it.
pr_lo, pr_hi, prr = (-40.0, ZC - 36.0), (TC + 26.0, ZC + 34.0), 14.0
qt = np.maximum(np.abs(G.T - (pr_lo[0] + pr_hi[0]) / 2) - ((pr_hi[0] - pr_lo[0]) / 2 - prr), 0)
qz = np.maximum(np.abs(G.Z - (pr_lo[1] + pr_hi[1]) / 2) - ((pr_hi[1] - pr_lo[1]) / 2 - prr), 0)
patch = prr - np.sqrt(qt ** 2 + qz ** 2)                                         # + inside the rounded patch
front = ss((T_FRONT + 4.0 - G.T) / 4.0)
thick = (1.5 + 2.5 * ss(patch / 8.0) + 10.0 * ss(patch / 14.0) * ss((TC - 12.0 - G.T) / 14.0)) * ss((G.D - 2.0) / 6.0)
thick = thick * (1 - front + front * ss((ZC - 2.0 - G.Z) / 28.0))   # the wrap fades out over 28 mm below the axis
cheek = smin(np.minimum(-FB - 0.3, FB + thick), patch, 3.0)
X = 131.3 + (G.D + 2 + 0.0565 * (G.Z - 227)) * 0.79577626 + (G.T + 0.069 * (G.Z - 227)) * -0.6083053 + 0.4   # world x (approx.)
kitF = smax(np.maximum(plug, lugs), cheek, 2.0)
kitF = smin(kitF, 164.5 - X, 8.0)                                              # inside the arm-rest line x <= 165, rounded
kitm = max(G.mesh(kitF).decompose(), key=lambda m: m.volume())
car = load_fit('GBG-SP-AR-L-L-LO-r5')
arm = car ^ box(-80, 80, -120, T_FRONT - 0.3, 150, 260)                        # the real arm, unchanged, up to the front face
st = mf.CrossSection.square([0.01, 12.0], center=True).offset(6.0, mf.JoinType.Round, circular_segments=48)
bt_c = (TC, ZC + 17.0)
btn = tf(st.extrude(4.0), np.array([[0, 0, 1, face + 4.0 - 3.2], [1, 0, 0, bt_c[0]], [0, 1, 0, bt_c[1]], [0, 0, 0, 1]], float))
kit = (kitm + arm) - tf(st.offset(0.3, mf.JoinType.Round).extrude(9.0), np.array([[0, 0, 1, face - 5.5], [1, 0, 0, bt_c[0]], [0, 1, 0, bt_c[1]], [0, 0, 0, 1]], float))
kit = max((kit - wing).decompose(), key=lambda m: m.volume())
save_W(kit, 'h2h_S_kit.stl'); save_W(btn, 'h2h_S_btn.stl')
s1 = sg.Polygon(json.load(open('../options/sections.json'))['s1'][0])
kx = float(to_tm(tf(kit, S2W)).bounds[1][0])
rep = dict(bore=f'Ø{2 * R0:.0f}, floor d\' {FLOOR}', mouth='stadium 33 x 54', engaged_depth=round(face - 2.5 - FLOOR, 1), turn=TURN, lug_span=SPAN,
           to_S1=round(sg.box(FLOOR, TC - R0, 60, TC + R0).distance(s1), 2), front_wall=round(TC - R0 - T_FRONT, 2),
           mouth_bottom_to_hole_z186=round(ZC - 27.0 - 186.6 - 1.65, 2), kit_x_max=round(kx, 1), clash=round((kit ^ wing).volume(), 2),
           axis_S=[float(face), TC, ZC])
json.dump(rep, open('h2h_S.json', 'w'), indent=1); print(rep)
