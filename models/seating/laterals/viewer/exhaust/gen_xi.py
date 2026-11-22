# X, inside, the best version: the closed exhaust socket (exterior as approved) with every internal feature modelled.
# Chosen internals (see the concept table on the page):
#  * tongue: a 15 x 80 stadium blade (ribbed by its two cam grooves), plug d' -1.5..13.5, mouth to t' 10 (24 deep)
#  * guide channels: a Ø6 boss on each socket wall rides a cam groove in each plug face; the groove is the boss's path in
#    the plug frame for the real motion, so pushing the last 12 mm turns the plug the last 15° (cam track, one motion)
#  * twist: insert tilted 15° nose-up, then push + rotate together (pitch axis at the mouth's top); the beak behind the
#    plug's top rises behind the internal ledge (pull-out); the tapered rear seats on the ceiling wedges
#  * click: a sliding steel-spring catch (11 x 6 block, 3.5 drop) in the plug's rear bottom snaps into a notch in the
#    pocket floor (sharp drop-off, hard stop); it only blocks the rotate-back
#  * release: button (collar, parent side) -> 45° cam -> Ø4 rod along the plug -> 45° cam -> lifts the catch; a steel
#    return spring in the plug's rear (preloaded on the pocket's rear wall) turns it back and nudges it out
#  * tell: an orange ring in the button bore shows whenever the catch is up (button pulled in); flush button = locked
#  * support: the closed ring and the bulge (<= 6 mm per side) make the walls >= 4.4 mm; printed solid there (no ribs)
# Writes xi_*.stl and xi.json.  Run: ../.venv/bin/python gen_xi.py
import os, sys, json, math, numpy as np
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'options'))
from lib5 import *
import shapely.geometry as sg
from scipy.ndimage import gaussian_filter1d
os.chdir(os.path.dirname(os.path.abspath(__file__)))
D0, D1, Z0, Z1, T_END = -1.5, 15.5, 210.0, 300.0, 10.0          # blade 17 x 90 (fine-tune: bigger, robust)
DC, ZCm, A = (D0 + D1) / 2, (Z0 + Z1) / 2, (D1 - D0) / 2
TP, ZP, PHI0, S_CAM = T_FRONT - 1.0, Z1 + 4.0, -15.0, 12.0      # pitch axis (along d'), start tilt, push length of the cam phase
CLR, BULGE = 0.3, 7.5
BOSS = dict(t=6.0, z=255.0, r=3.0, h=2.5)                        # Ø6 boss, 2.5 proud of each wall (4 mm deeper: out of sight from the mouth)
CATCH = dict(d0=DC - 5.5, d1=DC + 5.5, t0=T_END - 8.0, t1=T_END - 2.0, drop=3.0)
def ss(x): x = np.clip(x, 0.0, 1.0); return x * x * x * (x * (6 * x - 15) + 10)
W = wing_full()
sec = [p for p in W.slice(195.0).to_polygons() if len(p) > 2]
pr = mf.CrossSection([np.asarray(p) for p in sec], mf.FillRule.EvenOdd).extrude(84.6).translate([0, 0, 196.7])
W0 = max(((W - box(12.0, 40, -20, 45, 197.0, 281.0)) + (pr ^ box(11.7, 40, -20, 45, 196.7, 281.3))).decompose(), key=lambda m: m.volume())
KLO, KHI = (-15.0, -30.0, 176.0), (40.0, 45.0, 393.0)
G = Grid(W0, (-19.0, -34.0, 174.0), (42.0, 47.0, 395.0), h=0.25, blur=1.5)
wz = np.exp(-((G.Z - 281.0) / 10.0) ** 2); G.F = (G.F * (1 - wz) + gaussian_filter1d(G.F, 6.0 / 0.25, axis=2) * wz).astype(np.float32)
a_ = ss((16.0 - G.T) / 16.0); b_ = ss((G.D - 11.0) / 4.0) * ss((32.0 - G.T) / 16.0)
B = BULGE * ss((G.Z - 189.5) / 24.0) * ss((350.0 - G.Z) / 45.0) * (1 - (1 - a_) * (1 - b_))   # starts above the z 186 hole (top 188.25), faired over 24 mm
FB = G.F + B
Kin = sdf_box(G, KLO, (KHI[0], KHI[1] + 0.3, KHI[2]), 0.0)
WB = W0 - box(KLO[0], KHI[0], KLO[1], KHI[1], KLO[2], KHI[2])
# ---- motion: phase A (tilted, straight in) then phase B (push the last S_CAM while rotating PHI0 -> 0)
def rot(phi, t, z):                      # rotate (t, z) about the pitch axis by phi (deg, + = rear up)
    c, s = math.cos(math.radians(phi)), math.sin(math.radians(phi)); return TP + (t - TP) * c - (z - ZP) * s, ZP + (t - TP) * s + (z - ZP) * c
def to_local(phi, sh, t, z):              # world -> plug frame for pose (phi, slide-out sh along the plug axis)
    c, s = math.cos(math.radians(phi)), math.sin(math.radians(phi))
    t = t + sh * c; z = z + sh * s                                     # undo the slide (axis out = -(cos, sin))
    return rot(-phi, t, z)
POSES_B = [(PHI0 * (1 - k), S_CAM * (1 - k)) for k in np.linspace(0, 1, 22)]
# cam groove = the boss centre's path in the plug frame during phase B (+ flared entry at the plug's rear)
path = np.array([to_local(PHI0 * (1 - k), S_CAM * (1 - k), BOSS['t'], BOSS['z']) for k in np.linspace(0, 1, 160)])
path = np.vstack([path[0] + (path[0] - path[3]) / np.linalg.norm(path[0] - path[3]) * 8.0, path])   # run the groove out past the rear
line = sg.LineString(path)
def groove_local(d, tl, zl):
    # 2D distance from (tl, zl) to the path (vectorized via shapely), the groove depth band on both faces
    from shapely import distance as sdist, points as spoints
    P = np.c_[np.broadcast_to(tl, np.broadcast_shapes(tl.shape, zl.shape)).ravel(), np.broadcast_to(zl, np.broadcast_shapes(tl.shape, zl.shape)).ravel()]
    dist = np.asarray(sdist(line, spoints(P)), np.float32).reshape(np.broadcast_shapes(tl.shape, zl.shape))
    r = BOSS['r'] + 0.25 + 1.2 * ss((tl - (T_END - 4.0)) / 4.0)                 # 0.25 running clearance, flared entry
    g2 = r - dist
    band = np.maximum(np.minimum(d - (D0 - 1.0), (D0 + BOSS['h'] + 0.3) - d), np.minimum(d - (D1 - BOSS['h'] - 0.3), (D1 + 1.0) - d))
    return np.minimum(g2, band)
def plug_local(d, tl, zl, front=-60.0, grooves=True, catch_out=False):
    wedge = 1.5 * ss((tl - (T_END - 6.0)) / 6.0)                               # guide wedge: half-width 7.5 -> 6.0 over the last 6
    st = (A - wedge) - np.sqrt((d - DC) ** 2 + np.maximum(np.abs(zl - ZCm) - (Z1 - Z0) / 2 + A, 0) ** 2)
    body = smin(np.minimum(st, tl - front), T_END - tl, 6.0)                       # rear edge R ~3
    q = [np.abs(d - DC) - 6.5 + 3, np.abs(tl - (T_END + 2.4)) - 3.2 + 3, np.abs(zl - (Z1 - 1.0)) - 7.0 + 3]   # beak 13 wide, t' 9.2-15.6, z 292-306, R3
    beak = -(np.sqrt(sum(np.maximum(x, 0) ** 2 for x in q)) + np.minimum(np.maximum(np.maximum(q[0], q[1]), q[2]), 0) - 3.0)
    f = smax(body, beak, 4.0)                                                  # generous root fillet
    if grooves: f = smin(f, -groove_local(d, tl, zl), 2.0)                     # rounded groove edges
    if catch_out:                                                              # the catch, dropped 3.5 below the plug's floor
        cq = [np.abs(d - DC) - 5.5, np.abs(tl - (CATCH['t0'] + CATCH['t1']) / 2) - 3.0, np.abs(zl - Z0) - CATCH['drop']]
        f = np.maximum(f, -(np.sqrt(sum(np.maximum(x, 0) ** 2 for x in cq)) + np.minimum(np.maximum(np.maximum(cq[0], cq[1]), cq[2]), 0) - 0.0))
    return f
def plug_pose(phi, sh, front=-60.0, **kw):
    tl, zl = to_local(phi, sh, G.T, G.Z)
    return plug_local(G.D, tl, zl, front, **kw)
tool = plug_pose(PHI0, S_CAM, grooves=False) + CLR                                # phase A: the tilted plug slid straight in
for ph, sh in POSES_B: tool = np.maximum(tool, plug_pose(ph, sh) + CLR)
tool = np.maximum(tool, plug_pose(0.0, 0.0, catch_out=True) + 0.2)               # the catch notch (0.2 clearance) at the seat
# ---- self-supporting roofs for printing the wing upright: pointed (45°) teardrop tops on the pocket ceiling and the beak recess
SQ2 = math.sqrt(2.0)
zc_top = Z1 - A                                                            # centre of the plug's top round end
ramp = ss((G.T - (T_FRONT + 3.0)) / 6.0) * ss((T_END + 4.0 - G.T) / 3.0)   # none in the throat (the mouth stays a stadium)
td1 = np.minimum((A + CLR) * SQ2 * (0.70 + 0.30 * ramp) - (np.abs(G.D - DC) + (G.Z - zc_top)), np.minimum(G.Z - (zc_top + (A + CLR) / SQ2), ramp - 0.02))   # only above the 45° tangent points
tool = np.maximum(tool, td1)          # (a teardrop over the beak recess broke out of the thin upper wall; pending the final hook form)
# ---- two optional static pins, PARENT side only: M5 DIN 7991 countersunk, flush, on one vertical line at t' -9 (the outer wall
#      there is >= 6 mm; the button's own line is on the collar in front of the leading edge), symmetric about the button's height
Z_BTN = 244.0
PINS = [(-9.0, Z_BTN - 22.0), (-9.0, Z_BTN + 22.0)]
pin_info = []; pinT = None
for (tp_, zp_) in PINS:
    ib2 = np.argmin(abs(G.ax[1] - tp_)); kz2 = np.argmin(abs(G.ax[2] - zp_))
    d_o = float(G.ax[0][np.max(np.nonzero(FB[:, ib2, kz2] > 0)[0])])        # the outer (parent) face at the pin
    rr = np.sqrt((G.T - tp_) ** 2 + (G.Z - zp_) ** 2)
    cone = np.minimum(5.2 - rr - (d_o - G.D), G.D - (d_o - 2.45))              # 90° countersink, Ø10.4 at the face
    f = np.maximum(np.minimum(cone, 30.0), np.minimum(np.minimum(2.75 - rr, (d_o - 2.0) - G.D), G.D - (D1 - 0.5)))
    f = np.maximum(f, np.minimum(5.2 - rr, G.D - d_o))                          # open above the face
    pinT = f if pinT is None else np.maximum(pinT, f)
    pin_info.append((tp_, zp_, d_o))
Res = smin(smin(FB, -tool, 6.0), -pinT, 1.6)                                                       # rolled lip (R ~3); inside: the swept guide faces
wingm = max((WB + G.mesh(np.minimum(Res, Kin))).decompose(), key=lambda m: m.volume())
save_W(wingm, 'xi_wing.stl'); print('wing', flush=True)
# ---- kit: plug (with grooves) + collar bezel + the real arm, minus the internal cavities
plug = plug_pose(0.0, 0.0, T_FRONT - 16.0)
mouth2 = (A + 4.5) - np.sqrt((G.D - DC) ** 2 + np.maximum(np.abs(G.Z - ZCm) - (Z1 - Z0) / 2 + A, 0) ** 2)
bez = np.minimum(np.minimum(mouth2, -Res - 0.25), (T_FRONT + 3.0) - G.T)
bez = smin(bez, G.T - (T_FRONT - 9.0), 4.0)
kitm = max(G.mesh(smax(plug, bez, 2.0)).decompose(), key=lambda m: m.volume())
car = load_fit('GBG-SP-AR-L-L-LO-r5')
arm = (car ^ box(-80, 80, -120, T_FRONT - 0.3, 150, 260)) + (car ^ box(-80, -6.5, -120, T_FRONT + 3.0, 150, 260))
def cyl_t(d, z, r, t0, t1, n=32): return mf.Manifold.cylinder(t1 - t0, r, r, n).rotate([-90, 0, 0]).translate([d, t0, z])
def cyl_d(t, z, r, d0, d1, n=32): return mf.Manifold.cylinder(d1 - d0, r, r, n).rotate([0, 90, 0]).translate([d0, t, z])
def cyl_z(d, t, r, z0, z1, n=32): return mf.Manifold.cylinder(z1 - z0, r, r, n).translate([d, t, z0])
def coil(p0, axis, R, wire, L, turns, n=14):
    axis = np.asarray(axis, float) / np.linalg.norm(axis); u = np.cross(axis, [0, 0, 1.0] if abs(axis[2]) < .9 else [1.0, 0, 0]); u /= np.linalg.norm(u); v = np.cross(axis, u)
    pts = [np.asarray(p0) + axis * L * k / (turns * n) + R * (math.cos(2 * math.pi * k / n) * u + math.sin(2 * math.pi * k / n) * v) for k in range(turns * n + 1)]
    segs = [mf.Manifold.batch_hull([mf.Manifold.sphere(wire / 2, 10).translate(pts[i].tolist()), mf.Manifold.sphere(wire / 2, 10).translate(pts[i + 1].tolist())]) for i in range(len(pts) - 1)]
    return mf.Manifold.batch_boolean(segs, mf.OpType.Add)
# catch (locked position): 11 (d') x 6 (t') x 7.5 (z), 3.0 below the plug floor; a slot with a 45° ramp for the rod tip
ROD_Z = Z0 + 2.5
c_lo = [CATCH['d0'], CATCH['t0'], Z0 - CATCH['drop']]; c_hi = [CATCH['d1'], CATCH['t1'], Z0 + 4.5]
def prism_d(tri, d0, d1):                                                     # a (t', z) polygon extruded along d'
    return tf(mf.CrossSection([np.asarray(tri, float)]).extrude(d1 - d0), np.array([[0, 0, 1, d0], [1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 0, 1]], float))
catch = box(c_lo[0], c_hi[0], c_lo[1], c_hi[1], c_lo[2], c_hi[2]) - box(DC - 2.4, DC + 2.4, c_lo[1] - 1, c_lo[1] + 4.0, ROD_Z - 2.4, ROD_Z + 2.4)
catch = catch + prism_d([[c_lo[1] + 1.0, ROD_Z - 2.4], [c_lo[1] + 4.0, ROD_Z - 2.4], [c_lo[1] + 4.0, ROD_Z + 0.6]], DC - 2.4, DC + 2.4)   # 45° ramp
catch_pocket = box(c_lo[0] - 0.2, c_hi[0] + 0.2, c_lo[1] - 0.2, c_hi[1] + 0.2, Z0 - 6.0, Z0 + 9.5)           # + travel 3.5 up and the spring
catch_spring = coil([DC, (c_lo[1] + c_hi[1]) / 2, Z0 + 4.5], [0, 0, 1], 2.2, 0.5, 4.5, 6)                   # steel, Ø5 x 0.5 wire
# rod: Ø4 along t' from the collar to the catch, 45° tip; button: stadium 8 x 16 cap on the collar's parent side + stem
T_BTN = -27.5                                                  # cap centre t' (on the collar, in front of the leading edge)
rod_t0, rod_t1 = T_BTN, c_lo[1] + 2.5
rod = cyl_t(DC, ROD_Z, 2.0, rod_t0, rod_t1) - prism_d([[rod_t1 - 3.0, ROD_Z + 2.1], [rod_t1 + 0.1, ROD_Z - 1.0], [rod_t1 + 0.1, ROD_Z + 2.1]], DC - 3, DC + 3)   # 45° tip (CCW)
rod_chan = cyl_t(DC, ROD_Z, 2.2, rod_t0 - 1.0, rod_t1 + 4.5)
# ---- the AGREED r5b button (12 x 24 stadium, square push, thumb scoop, 0.8 crown) and its orange tell sleeve, on a pod on
#      the collar's parent side, where the thumb lands; the child-side face of the collar opposite is the pinch reaction
# (T_BTN, Z_BTN set above: the cap sits on the collar's parent side in front of the leading edge, between the two pins' heights)
D_PF = 13.2                                                  # pod face (crown 0.8 proud, as r5b); kept inboard so x <= 165 this far forward
qp = [np.maximum(np.abs(G.T - T_BTN) - (9.5 - 4), 0), np.maximum(np.abs(G.Z - Z_BTN) - (16.0 - 4), 0)]
pod2 = 4.0 - np.sqrt(qp[0] ** 2 + qp[1] ** 2)                 # rounded rectangle 19 x 32 in (t', z), corner R 4
pod = np.minimum(pod2, np.minimum(D_PF - G.D, G.D - (DC - 2.0)))
collar = smax(smax(plug, bez, 2.0), smin(pod, -Res - 0.25, 1.0), 4.0)    # the pod blends into the bezel (R ~2)
X_ = 131.3 + (G.D + 2 + 0.0565 * (G.Z - 227)) * 0.79577626 + (G.T + 0.069 * (G.Z - 227)) * -0.6083053 + 0.4
kitm = max(G.mesh(smin(collar, 164.5 - X_, 4.0)).decompose(), key=lambda m: m.volume())
bt = load_fit('GBG-SP-BT-L-r5') ^ box(21.0, 40, -40, 40, 150, 330); bb = bt.bounding_box()
sv = load_fit('GBG-SP-TS-r5_L')
mv = [D_PF + 0.8 - bb[3], T_BTN - (bb[1] + bb[4]) / 2, Z_BTN - (bb[2] + bb[5]) / 2]
cap = bt.translate(mv); sleeve = sv.translate(mv)
st12 = mf.CrossSection.square([0.01, 12.0], center=True).offset(6.0, mf.JoinType.Round, circular_segments=48)   # 12 (t') x 24 (z)
def along_d(cs, d0, L): return tf(cs.extrude(L), np.array([[0, 0, 1, d0], [1, 0, 0, T_BTN], [0, 1, 0, Z_BTN], [0, 0, 0, 1]], float))
stem = box(DC + 0.5, D_PF - 3.6, T_BTN - 2.5, T_BTN + 2.5, ROD_Z - 2.5, Z_BTN - 6.0)        # plate from the cap down to the rod's 45° cam
button = cap + stem
btn_bore = along_d(st12.offset(0.3, mf.JoinType.Round), D_PF - 8.0, 10.0) + along_d(st12.offset(1.6, mf.JoinType.Round), D_PF - 2.6, 1.0) \
           + box(DC + 0.2, D_PF - 3.0, T_BTN - 2.8, T_BTN + 2.8, ROD_Z - 2.8, Z_BTN - 5.0)
tell = sleeve                                                # r5b: 12.6 x 24.6 opening; the sleeve seats under a 1.6 lip
bface = D_PF
# ---- static pins (parts): DIN 7991 M5 x 20 countersunk (flush), real M5 nut captive in a hex pocket inside the plug (slid in
#      from the plug's inner face, inside the socket); the tip ends inside the nut; Ø5.5 clearance: they only lock
hexcs = mf.CrossSection.circle(8.0 / math.sqrt(3), 6)                      # 8 AF hexagon
screws, nuts, caps, nut_pockets, plug_holes = [], [], [], [], []
for (tp_, zp_, d_o) in pin_info:
    head = mf.Manifold.cylinder(2.7, 2.5, 5.0, 48).rotate([0, 90, 0]).translate([d_o - 2.7, tp_, zp_])   # 90° head, Ø10 at the face
    screws.append(head + cyl_d(tp_, zp_, 2.5, d_o - 20.0, d_o - 2.0, 24))
    nd = d_o - 20.0                                                         # nut's inner face = the tip (nothing protrudes)
    nuts.append(tf(hexcs.extrude(4.0), np.array([[0, 0, 1, nd], [1, 0, 0, tp_], [0, 1, 0, zp_], [0, 0, 0, 1]], float)) - cyl_d(tp_, zp_, 2.5, nd - 1, nd + 5))
    nut_pockets.append(tf(hexcs.offset(0.15, mf.JoinType.Miter).extrude(nd + 4.2 - (D0 - 2.0)), np.array([[0, 0, 1, D0 - 2.0], [1, 0, 0, tp_], [0, 1, 0, zp_], [0, 0, 0, 1]], float)))
    plug_holes.append(cyl_d(tp_, zp_, 2.75, nd + 3.5, D1 + 1))
    caps.append(mf.Manifold.cylinder(2.45, 2.75, 5.2, 48).rotate([0, 90, 0]).translate([d_o - 2.45, tp_, zp_]))   # flush printed cone cap
nut_pocket = mf.Manifold.batch_boolean(nut_pockets, mf.OpType.Add); plug_hole = mf.Manifold.batch_boolean(plug_holes, mf.OpType.Add)
# ---- ONE spring type everywhere: steel, OD 6, wire 0.6, 8 coils (6 active), free 16 (k ~1.18 N/mm). 1x on the catch, 2x for the return.
SPR = dict(od=6.0, wire=0.6, coils=8, free=16.0, k=1.18)
def spring(p0, axis, L): return coil(p0, axis, (SPR['od'] - SPR['wire']) / 2, SPR['wire'], L, SPR['coils'])
tc_ = (c_lo[1] + c_hi[1]) / 2
catch_spring = spring([DC, tc_, Z0 + 4.5], [0, 0, 1], 14.5)                     # installed 14.5 (catch down) -> 11.5 lifted
catch_sp_pocket = cyl_z(DC, tc_, 3.25, Z0 + 4.0, Z0 + 4.5 + 14.5 + 0.3)
RS_Z = [240.0, 254.0]                                                         # the two return springs, stacked in z
ret_springs = [spring([DC, T_END - 11.9, z_], [0, 1, 0], 12.2) for z_ in RS_Z]   # seated 12.2 (3.8 compressed, 4.5 N each)
ret_pocket = mf.Manifold.batch_boolean([cyl_t(DC, z_, 3.25, T_END - 12.2, T_END + 1.0) for z_ in RS_Z], mf.OpType.Add)
kit = (kitm + arm) - catch_pocket - catch_sp_pocket - rod_chan - btn_bore - ret_pocket - nut_pocket - plug_hole
kit = max((kit - wingm).decompose(), key=lambda m: m.volume())
OUTS = [('xi_kit', kit), ('xi_catch', catch), ('xi_rod', rod), ('xi_button', button), ('xi_tell', tell), ('xi_spring_c', catch_spring),
        ('xi_spring_r1', ret_springs[0]), ('xi_spring_r2', ret_springs[1]), ('xi_pin1', screws[0]), ('xi_pin2', screws[1]),
        ('xi_nut1', nuts[0]), ('xi_nut2', nuts[1]), ('xi_cap1', caps[0]), ('xi_cap2', caps[1])]
for nm, m in OUTS:
    save_W(m, nm + '.stl')
# ---- checks and report
s1 = sg.Polygon(json.load(open('../options/sections.json'))['s1'][0])
cav = [c for c in (box(D0 - 1, D1 + 1, T_FRONT + 1.0, 30.0, 205, 310) - wingm).decompose() if c.bounding_box()[1] < T_FRONT + 2]
foot = sg.MultiPolygon([sg.Polygon(p) for c in cav for z in np.arange(203.0, 272.0, 2.0) for p in c.slice(float(z)).to_polygons() if len(p) > 2]).buffer(0)
walls = {}
for z in (214.0, 236.0, 255.0, 275.0, 296.0):
    ws = sg.MultiPolygon([sg.Polygon(p) for p in wingm.slice(z).to_polygons() if len(p) > 2]).buffer(0)
    cv = sg.MultiPolygon([sg.Polygon(p) for c in cav for p in c.slice(z).to_polygons() if len(p) > 2]).buffer(0)
    filled = ws.union(cv.buffer(0.05)).buffer(0)
    ext = sg.MultiLineString([g.exterior for g in getattr(filled, 'geoms', [filled])])
    cvb = cv.intersection(sg.box(-50, T_FRONT + 8, 50, 60))                      # behind the rolled lip
    walls[f'z {z:.0f}'] = round(cvb.distance(ext), 2) if not cvb.is_empty else None
parts = {nm[3:]: [round(float(v), 1) for v in ((np.array(m.bounding_box()[:3]) + np.array(m.bounding_box()[3:])) / 2)] for nm, m in OUTS if nm != 'xi_kit'}
clash = {nm: round((m ^ wingm).volume(), 2) for nm, m in OUTS}
rep = dict(plug=f'stadium {D1 - D0:.0f} x {Z1 - Z0:.0f}, d\' {D0}..{D1}, mouth to t\' {T_END}', engaged_depth=round(T_END - T_FRONT, 1),
           start_tilt_deg=-PHI0, cam_push_mm=S_CAM, pivot_S=[TP, ZP], boss=BOSS, catch=CATCH, bulge_max=BULGE,
           socket_to_S1=round(foot.distance(s1), 2), socket_t_max=round(max(c.bounding_box()[4] for c in cav), 1), thinnest_wall=walls,
           kit_x_max=round(float(to_tm(tf(kit, S2W)).bounds[1][0]), 1), clash=round((kit ^ wingm).volume(), 2),
           button_S=[bface, T_BTN, Z_BTN], pins=[[d_o, tp_, zp_] for (tp_, zp_, d_o) in pin_info], rod=[DC, rod_t0, rod_t1, ROD_Z], ret_springs=RS_Z, spring=SPR, part_clash=clash,
           groove_path=np.round(path[::8], 2).tolist(), parts=parts, plug_dc=DC)
json.dump(rep, open('xi.json', 'w'), indent=1); print(json.dumps(rep, indent=1))
