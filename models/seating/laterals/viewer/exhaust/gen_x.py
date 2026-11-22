# Option X: a CLOSED "exhaust" socket in the wing's leading edge, insert-and-rotate-in (pitch), push-button release.
# The wing stays whole: a stadium mouth on the leading edge with a rolled lip, the wing material a closed ring round it, a
# faired bulge of <= 4.5 mm per side, and behind a short throat the pocket. The pocket is the swept volume of the plug's real
# motion (so every internal face is a guide surface): (1) push in along its own axis while tilted 15° nose-up, (2) rotate
# 15° nose-down about a pitch axis (along d') at the top front of the mouth. At the end of the rotation the beak hook behind
# the plug's top rises behind the internal ledge (pull-out), the tapered rear (guide wedges) seats on the pocket ceiling, the
# collar seats in the lip, and the steel-spring catch at the rear bottom clicks in (it only blocks the rotate-back).
# Plug: stadium 13 (d' -1..12) x 80 (z 214-294), mouth to t' 6 (20 deep), 0.35 sliding clearance in the pocket.
# Kit = the real r5cad arm (unchanged outside, stopped 0.3 short of the leading edge) + the collar bezel + the plug.
# Writes x_wing.stl, x_kit.stl, x_btn.stl, x_rod.stl, x.json.  Run: ../.venv/bin/python gen_x.py
import os, sys, json, math, numpy as np
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'options'))
from lib5 import *
import shapely.geometry as sg
from scipy.ndimage import gaussian_filter1d
os.chdir(os.path.dirname(os.path.abspath(__file__)))
D0, D1, Z0, Z1, T_END = -1.0, 12.0, 214.0, 294.0, 6.0
DC, ZCm, A = (D0 + D1) / 2, (Z0 + Z1) / 2, (D1 - D0) / 2
TP, ZP, PHI0 = T_FRONT - 1.0, Z1 + 4.0, -15.0        # pitch axis (along d'), start tilt (nose up)
CLR, BULGE = 0.35, 4.5
W = wing_full()
sec = [p for p in W.slice(195.0).to_polygons() if len(p) > 2]
pr = mf.CrossSection([np.asarray(p) for p in sec], mf.FillRule.EvenOdd).extrude(84.6).translate([0, 0, 196.7])
W0 = max(((W - box(12.0, 40, -20, 45, 197.0, 281.0)) + (pr ^ box(11.7, 40, -20, 45, 196.7, 281.3))).decompose(), key=lambda m: m.volume())
KLO, KHI = (-14.0, -30.0, 176.0), (40.0, 45.0, 393.0)
G = Grid(W0, (-18.0, -34.0, 174.0), (42.0, 47.0, 395.0), h=0.25, blur=1.5)
wz = np.exp(-((G.Z - 281.0) / 10.0) ** 2); G.F = (G.F * (1 - wz) + gaussian_filter1d(G.F, 6.0 / 0.25, axis=2) * wz).astype(np.float32)      # wide, soft blend where the section changes (z 281)
def ss(x): x = np.clip(x, 0.0, 1.0); return x * x * x * (x * (6 * x - 15) + 10)     # smootherstep (C2)
# faired bulge <= 4.5 per side round the tip (all faces, fading out by t' 12 so slit S1 is untouched) + the outer skin to t' 30
a_ = ss((12.0 - G.T) / 14.0); b_ = ss((G.D - 11.0) / 4.0) * ss((30.0 - G.T) / 16.0)
B = BULGE * ss((G.Z - 165.0) / 40.0) * ss((343.0 - G.Z) / 40.0) * (1 - (1 - a_) * (1 - b_))
FB = G.F + B
Kin = sdf_box(G, KLO, (KHI[0], KHI[1] + 0.3, KHI[2]), 0.0)
WB = W0 - box(KLO[0], KHI[0], KLO[1], KHI[1], KLO[2], KHI[2])
def plug_local(d, t, z, front=-60.0):
    """signed distance (+ inside) of the plug in its own (seated) frame: stadium section, tapered rear (guide wedge), beak"""
    wedge = 1.5 * ss((t - (T_END - 6.0)) / 6.0)                               # half-width 6.5 -> 5.0 over the last 6 mm
    st = (A - wedge) - np.sqrt((d - DC) ** 2 + np.maximum(np.abs(z - ZCm) - (Z1 - Z0) / 2 + A, 0) ** 2)
    body = smin(np.minimum(st, t - front), T_END - t, 3.0)
    q = [np.abs(d - 5.5) - 5.0 + 2, np.abs(t - (T_END + 2.0)) - 2.6 + 2, np.abs(z - (Z1 - 1.0)) - 6.0 + 2]   # beak d' 0.5-10.5, t' 5.4-10.6, z 287-299
    beak = -(np.sqrt(sum(np.maximum(x, 0) ** 2 for x in q)) + np.minimum(np.maximum(np.maximum(q[0], q[1]), q[2]), 0) - 2.0)
    return smax(body, beak, 1.5)
def plug_pose(phi, front=-60.0):
    c, s = math.cos(math.radians(-phi)), math.sin(math.radians(-phi))       # inverse rotation of the grid into the plug frame
    tl = TP + (G.T - TP) * c - (G.Z - ZP) * s; zl = ZP + (G.T - TP) * s + (G.Z - ZP) * c
    return plug_local(G.D, tl, zl, front)
tool = None
for phi in np.linspace(PHI0, 0.0, 16):
    f = plug_pose(phi) + CLR; tool = f if tool is None else np.maximum(tool, f)
Res = smin(FB, -tool, 6.0)                                                       # rolled lip (R ~3) and rounded internal edges
wingm = max((WB + G.mesh(np.minimum(Res, Kin))).decompose(), key=lambda m: m.volume())
save_W(wingm, 'x_wing.stl'); print('wing', flush=True)
# kit (seated pose): plug + collar bezel seated in the lip (one smooth piece) + the real arm
plug = plug_pose(0.0, T_FRONT - 6.0)
mouth2 = (A + 4.5) - np.sqrt((G.D - DC) ** 2 + np.maximum(np.abs(G.Z - ZCm) - (Z1 - Z0) / 2 + A, 0) ** 2)   # stadium + 4.5
bez = np.minimum(np.minimum(mouth2, -Res - 0.25), (T_FRONT + 3.0) - G.T)
bez = smin(bez, G.T - (T_FRONT - 9.0), 4.0)                                       # 9 mm proud of the leading edge, front rounded
kitF = smax(plug, bez, 2.0)
kitm = max(G.mesh(kitF).decompose(), key=lambda m: m.volume())
car = load_fit('GBG-SP-AR-L-L-LO-r5')
arm = (car ^ box(-80, 80, -120, T_FRONT - 0.3, 150, 260)) + (car ^ box(-80, -6.5, -120, T_FRONT + 3.0, 150, 260))
kit = max(((kitm + arm) - wingm).decompose(), key=lambda m: m.volume())
# push button on the collar's parent-side face, working the catch through a Ø4 rod inside the plug (mock)
st = mf.CrossSection.square([0.01, 10.0], center=True).offset(5.0, mf.JoinType.Round, circular_segments=48)
bz = Z0 + 22.0; bface = float(np.max(G.ax[0][np.any(bez[:, np.argmin(abs(G.ax[1] - (T_FRONT - 4))), :] > 0, axis=1)]))
btn = tf(st.extrude(3.5), np.array([[0, 0, 1, bface - 2.6], [1, 0, 0, T_FRONT - 4.0], [0, 1, 0, bz], [0, 0, 0, 1]], float))
kit = kit - tf(st.offset(0.3, mf.JoinType.Round).extrude(8.0), np.array([[0, 0, 1, bface - 6.0], [1, 0, 0, T_FRONT - 4.0], [0, 1, 0, bz], [0, 0, 0, 1]], float))
rod = mf.Manifold.cylinder(T_END - 2 - (T_FRONT - 4.0), 2.0, 2.0, 24).rotate([-90, 0, 0]).translate([DC, T_FRONT - 4.0, Z0 + 4.0])
save_W(kit, 'x_kit.stl'); save_W(btn, 'x_btn.stl'); save_W(rod, 'x_rod.stl')
s1 = sg.Polygon(json.load(open('../options/sections.json'))['s1'][0])
cav = [c for c in (box(-1.6, 13.0, T_FRONT + 1.0, 28.0, 205, 310) - wingm).decompose() if c.bounding_box()[1] < T_FRONT + 2]   # the socket's own cavity
foot = sg.MultiPolygon([sg.Polygon(p) for c in cav for z in np.arange(203.0, 272.0, 2.0) for p in c.slice(float(z)).to_polygons() if len(p) > 2]).buffer(0)
rep = dict(plug=f'stadium {D1 - D0:.0f} x {Z1 - Z0:.0f}, mouth to t\' {T_END}', engaged_depth=round(T_END - T_FRONT, 1), start_tilt_deg=-PHI0,
           pivot_S=[TP, ZP], pocket_to_S1_in_its_z_range=round(foot.distance(s1), 2), kit_x_max=round(float(to_tm(tf(kit, S2W)).bounds[1][0]), 1),
           clash=round((kit ^ wingm).volume(), 2), bulge_max=BULGE, button_S=[bface, T_FRONT - 4.0, bz])
json.dump(rep, open('x.json', 'w'), indent=1); print(rep)
