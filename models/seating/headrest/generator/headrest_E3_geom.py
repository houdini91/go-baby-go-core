# GbG headrest E v0.3: socket inside the backrest top (nothing sticks out behind), slim column, yoke neck that hugs the cradle.
# Joint family copied from GbG_chair_forth: tongue-in-groove + M3 bolts into captive nuts, gussets, teardrop holes.
# Chair coords (mm): x lateral, y rearward, z up. Backrest top z 390, top section y 123.5..144 (20.5 thick, solid).
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType, Mesh
from headrest_E_geom import rounded, ycyl, xcyl, zcyl, hexprism, tris, SEG

SEAT_Z, TOP_Z = 20.0, 390.0
BASE_Z = TOP_Z                         # stack starts on the backrest top edge
BX, SY0, SY1 = 30.0, 123.5, 152.0      # column: 60 x 28.5 (8 mm proud of the backrest plane, above the top edge only)
TEN = (6.0, 127.75, 139.75, 12.0)      # tenon half-width x, y0, y1, length
CL = 0.2                               # snug sliding fit on the P1S (PETG); screws only clamp
ROD_X, ROD_Y, ROD_D, ROD_LOW = 20.0, 133.75, 8.0, 330.0
KNOB_Z = 6.0
NECK_H = 24.0
HEIGHTS = (0, 25, 50, 75, 100, 125, 150)
NECKS = (0, 30, 60)
R_IN, PAD_R, SHELL_T, RIM = 86.0, 74.0, 5.0, 18.0
LIP = 8.0                              # occipital lip: the lower 30 mm of the cup comes 8 mm inward
SPAN = 70.0
CR_Z = (-45.0, 55.0)
YOKE_A, YOKE_T, YOKE_Z = 38.0, 10.0, (-24.0, 22.0)   # yoke half-angle, radial thickness, z range (rel. arm top)
RIB_A, RIB_T, RIB_Z = 34.0, 4.0, (-20.0, 20.0)       # curved tongue on the cradle back
YOKE_BOLTS = (-24.0, 0.0, 24.0)                      # M3 bolt angles, at z = 0
ELL = 1.28                                           # cradle is oval in plan: depth / width (head 192 long x 147 wide)
import os as _os
CHAIR = _os.environ.get("GBG_CHAIR_MESH", _os.path.join(_os.path.dirname(_os.path.abspath(__file__)), "chair_mesh"))

def zp_of(H): return BASE_Z + H + NECK_H
def pad_above_seat(H): return zp_of(H) + 5 - SEAT_Z
def rbox(x0, x1, y0, y1, z0, z1, r=3.0):
    return rounded([(x0, y0), (x1, y0), (x1, y1), (x0, y1)], r).extrude(z1 - z0).translate([0, 0, z0])
def box(x0, x1, y0, y1, z0, z1):
    return M.cube([x1 - x0, y1 - y0, z1 - z0]).translate([x0, y0, z0])
def sector(r0, r1, a0, a1, z0, z1, n=96, r=1.2):
    """ring sector about the z axis, angles in deg from +y toward +x"""
    prof = rounded([(r0, z0), (r1, z0), (r1, z1), (r0, z1)], r)
    return M.revolve(prof, n, a1 - a0).rotate([0, 0, 90 - a1])

# ---------------- joint ----------------
HEEL_T0, HEEL_T1, HEEL_L, HEEL_SCREW = 14.0, 4.0, 36.0, 18.0   # heel thickness at the joint / at the bottom, length, screw depth below the joint
def key_part(zf):
    """printed key (Lego pin): 12 x 12 x 24, half in each part"""
    x, y0, y1, L = TEN
    return rbox(-x, x, y0, y1, zf - L, zf + L, 1.2)
def mortise(zf):
    """socket below a joint face (in the lower part)"""
    x, y0, y1, L = TEN
    return rbox(-x - CL, x + CL, y0 - CL, y1 + CL, zf - L - 0.5, zf + 1, 1.2)
def mortise_up(zf):
    """socket above a joint face (in the upper part)"""
    x, y0, y1, L = TEN
    return rbox(-x - CL, x + CL, y0 - CL, y1 + CL, zf - 1, zf + L + 0.5, 1.2)
def rod_bores(z0, z1):
    return M.batch_boolean([zcyl(ROD_D + 0.3, z1 - z0, s * ROD_X, ROD_Y, z0) for s in (1, -1)], 0)
def heel_screw_hole(zf):
    """M5 knob through the heel of the upper part into a captive nut in the lower part's back"""
    z = zf - HEEL_SCREW
    return ycyl(5.4, 40, 0, SY1 - 16, z)
def nut_pocket(zf):
    z = zf - HEEL_SCREW
    return hexprism(8.2, 4.6, 0, SY1 - 9.0, z).rotate([0, 0, 0]) + box(-4.1, 4.1, SY1 - 9.0, SY1 - 4.4, z, z + 30)   # slide-in from above, covered by the part above
def heel(zb):
    top = rbox(-BX, BX, SY1 - 1.0, SY1 + HEEL_T0, zb - 2, zb + 10, 3.0)
    bot = rbox(-BX + 3, BX - 3, SY1 - 1.0, SY1 + HEEL_T1, zb - HEEL_L, zb - HEEL_L + 2, 1.2)
    h = M.batch_hull([top, bot])
    h = h - box(-40, 40, 100, SY1 + 0.3, zb - HEEL_L - 5, zb)
    return h - heel_screw_hole(zb)
def knob_hole(zf):                    # kept for the tilt block / chair: same screw position
    return heel_screw_hole(zf)

# ---------------- chair top_back with the socket inside ----------------
def top_back():
    import trimesh
    from mock3d import load_stl
    t = load_stl(CHAIR + "/chair_top_back.stl"); V = t.reshape(-1, 3)
    tm = trimesh.Trimesh(V, np.arange(len(V)).reshape(-1, 3), process=True)
    return M(Mesh(vert_properties=np.asarray(tm.vertices, np.float32), tri_verts=np.asarray(tm.faces, np.uint32)))
def top_back_socket():
    tb = top_back()
    pad = rbox(-BX - 2, BX + 2, 136.0, SY1, BASE_Z - HEEL_L - 4, BASE_Z, 3.0)
    taper = M.batch_hull([rbox(-BX - 2, BX + 2, 136.0, SY1, BASE_Z - HEEL_L - 4, BASE_Z - HEEL_L - 2, 1.0),
                          rbox(-BX + 4, BX - 4, 136.0, 139.0, BASE_Z - HEEL_L - 26, BASE_Z - HEEL_L - 25, 0.8)])
    tb = tb + pad + taper
    return tb - mortise(BASE_Z) - rod_bores(ROD_LOW, BASE_Z + 1) - heel_screw_hole(BASE_Z) - nut_pocket(BASE_Z)

# ---------------- extender ----------------
POST_X0 = 10.0                         # posts: x 10..30 each side (rods at x 20); ladder rungs every 25 mm
PLATE, RUNG = 14.0, 5.0
def extender(H, zb=BASE_Z):
    """two posts (each carries a carbon rod) + rungs + solid end plates; printed on its side so bending runs along the layers"""
    z0, z1 = zb, zb + H
    parts = [rbox(s_ * BX if s_ < 0 else POST_X0, -POST_X0 if s_ < 0 else BX, SY0, SY1, z0, z1, 3.0) for s_ in (-1, 1)]
    if H <= 2 * PLATE + 4:
        parts = [rbox(-BX, BX, SY0, SY1, z0, z1, 4.0)]
    else:
        parts += [rbox(-BX, BX, SY0, SY1, z0, z0 + PLATE, 4.0), rbox(-BX, BX, SY0, SY1, z1 - PLATE, z1, 4.0)]
        n = int((H - 2 * PLATE) // 25)
        for i in range(1, n + 1):
            zc = z0 + PLATE + i * (H - 2 * PLATE) / (n + 1)
            parts.append(rbox(-POST_X0 - 1, POST_X0 + 1, SY0, SY1, zc - RUNG / 2, zc + RUNG / 2, 1.5))
    m = M.batch_boolean(parts, 0) + heel(zb)
    m = m - mortise_up(zb) - mortise(z1) - rod_bores(zb - 1, z1 + 1) - heel_screw_hole(z1) - nut_pocket(z1)
    return m

# ---------------- cradle ----------------
def cradle_frame(n, H):
    zp = zp_of(H)
    cy = SY0 + 16.5 - n - ELL * (R_IN + SHELL_T + YOKE_T + 0.3)   # yoke back lands inside the column footprint for N0
    return zp, cy

def ell_pose(theta, r_ref=None):
    """point on the oval at circle-angle theta (deg) for radius r_ref, and the outward normal angle (deg, from +y toward +x)"""
    r_ref = R_IN if r_ref is None else r_ref
    t = math.radians(theta)
    p = np.array([r_ref * math.sin(t), ELL * r_ref * math.cos(t), 0.0])
    phi = math.degrees(math.atan2(math.sin(t) / 1.0, math.cos(t) / ELL))
    return p, phi
def place(m_local, theta, r_ref=None):
    """part built at angle 0 (radial = +y) about radius r_ref -> placed normal to the oval at theta (cradle frame)"""
    r_ref = R_IN if r_ref is None else r_ref
    p, phi = ell_pose(theta, r_ref)
    return m_local.translate([0, -r_ref, 0]).rotate([0, 0, -phi]).translate(p.tolist())
def cradle(n, H):
    zp, cy = cradle_frame(n, H)
    R, z0, z1 = R_IN, CR_Z[0], CR_Z[1]
    prof = rounded([(R - LIP, z0), (R + RIM, z0), (R + RIM, z0 + 4), (R + SHELL_T, z0 + 4 + 0.01), (R + SHELL_T, z1 - RIM), (R + RIM, z1 - 4),
                    (R + RIM, z1), (R, z1), (R, z0 + 30)], 1.6)
    m = M.revolve(prof, 160, 2 * SPAN).rotate([0, 0, 90 - SPAN])
    m = m + sector(R + SHELL_T - 0.5, R + SHELL_T + RIB_T, -RIB_A, RIB_A, RIB_Z[0], RIB_Z[1], r=0.8)
    for s_ in (1, -1):
        m = m - M.cube([4.5, 30, 28], True).translate([0, R + 2, 40]).rotate([0, 0, -s_ * 54])
    m = m - foam_fixings()
    m = m.scale([1, ELL, 1])
    for a in YOKE_BOLTS:
        m = m - place(ycyl(3.4, 40, 0, R - 5, 0) + hexprism(5.6, 2.6, 0, R - 0.01, 0), a)
    for s_ in (1, -1):
        m = m + place(mount_pad(), s_ * MB_ANG)
        m = m - place(mount_pad_cuts(), s_ * MB_ANG)
    inner = M.cylinder(20, R - 0.01, R - 0.01, 160).translate([0, 0, -2]).scale([1, ELL, 1])
    m = m - inner
    return m.translate([0, cy, zp])
FOAM_SLOTS = [(0, 34), (40, 34), (-40, 34), (0, -32), (44, -32), (-44, -32)]     # (angle, z): 22 x 3.5 strap slots
ZIP_HOLES = [(20, 34), (-20, 34), (22, -32), (-22, -32), (62, 8), (-62, 8)]        # (angle, z): pairs of 3 x 5 holes, 8 mm apart
def foam_fixings():
    R = R_IN; cuts = []
    for a, z in FOAM_SLOTS:
        cuts.append(M.cube([22, 30, 3.5], True).translate([0, R + 5, z]).rotate([0, 0, -a]))
    for a, z in ZIP_HOLES:
        for dz in (-4, 4):
            cuts.append(M.cube([5, 30, 3], True).translate([0, R + 5, z + dz]).rotate([0, 0, -a]))
    return M.batch_boolean(cuts, 0)

def foam(n, H):
    zp, cy = cradle_frame(n, H)
    z0 = CR_Z[0]
    prof = rounded([(PAD_R - LIP, z0 + 4), (R_IN - LIP + 0.6, z0 + 4), (R_IN - 0.5, z0 + 30), (R_IN - 0.5, CR_Z[1] - 4), (PAD_R, CR_Z[1] - 4), (PAD_R, z0 + 30)], 4.0)
    return M.revolve(prof, 160, 2 * (SPAN - 2)).rotate([0, 0, 90 - SPAN + 2]).scale([1, ELL, 1]).translate([0, cy, zp])

# ---------------- neck = column top + gussets + yoke ----------------
def neck(n, H):
    zb = BASE_Z + H
    zp, cy = cradle_frame(n, H)
    R0 = R_IN + SHELL_T
    col = rbox(-BX, BX, SY0, SY1, zb, zb + NECK_H, 4.0)
    S = lambda m_: m_.scale([1, ELL, 1]).translate([0, cy, zp])
    yoke = S(sector(R0 + 0.3, R0 + YOKE_T + 0.3, -YOKE_A, YOKE_A, YOKE_Z[0], YOKE_Z[1], r=2.5))
    # gussets: hull of the column top and the yoke's back centre -> flowing wedge, plus two side gussets to the yoke ends
    back = S(sector(R0 + 0.3, R0 + YOKE_T + 0.3, -12, 12, YOKE_Z[0], YOKE_Z[1] - 6, r=2.0))
    web = M.batch_hull([col, back])
    sides = []
    for s in (1, -1):
        a = math.radians(s * (YOKE_A - 6))
        px, py = (R0 + YOKE_T) * math.sin(a), cy + ELL * (R0 + YOKE_T) * math.cos(a)
        tip = zcyl(6, YOKE_Z[1] - YOKE_Z[0] - 10, px, py, zp + YOKE_Z[0] + 2)
        root = rbox(-BX + 2, BX - 2, SY0 + 2, SY1 - 2, zb + 2, zb + NECK_H - 2, 2)
        sides.append(M.batch_hull([tip, root]))
    m = col + heel(zb) + yoke + web + M.batch_boolean(sides, 0)
    # keep the cradle space clear: everything inside R0 (+ clearance) goes
    m = m - S(M.cylinder(200, R0 + 0.3, R0 + 0.3, 160).translate([0, 0, -100]))
    # curved groove for the cradle tongue: open at the top, 4 mm floor at the bottom
    m = m - S(sector(R0 - 0.7, R0 + RIB_T + 0.25, -RIB_A - 0.25, RIB_A + 0.25, RIB_Z[0] - 0.25, YOKE_Z[1] + 5, r=0.5))
    m = m - rod_bores(zb - 1, zb + 20) - mortise_up(zb)
    for a in YOKE_BOLTS:
        h = ycyl(3.4, 60, 0, R_IN - 5, 0)
        m = m - place(h, a).translate([0, cy, zp])
    return m

# ---------------- push-button mount (tongue-in-groove + 2x M3), from v0.2 ----------------
MB_ANG, MB_SPLAY, MB_TILTS = 63.0, 10.0, (0, 20, 40)
PAD_Z, SCREW_Z, SCREW_X = (-20.0, 26.0), -13.0, 5.5
SHOE_Z = (-12.0, 22.0)                 # pad z range on the wing; the slot is open at the top
def mount_pad():
    """low, blended pad on the wing (3.5 mm proud of the shell); the T-slot is sunk INTO the pad + shell"""
    R = R_IN
    core = rounded([(-13, R + 2.0), (13, R + 2.0), (13, R + 8.6), (-13, R + 8.6)], 3.0).extrude(SHOE_Z[1] - SHOE_Z[0]).translate([0, 0, SHOE_Z[0]])
    skirt = rounded([(-9, R + 2.0), (9, R + 2.0), (9, R + 5.5), (-9, R + 5.5)], 1.2).extrude(2).translate([0, 0, SHOE_Z[0] - 12])
    return M.batch_hull([core, skirt])
def shoe_slot(cl=0.0, soft=False):
    R = R_IN
    throat = M.cube([8 + 2 * cl, 2.8 + cl, 40]).translate([-4 - cl, R + 6.0 - cl, SHOE_Z[0] + 4])
    wide = M.cube([14 + 2 * cl, 2.6 + 2 * cl, 40]).translate([-7 - cl, R + 3.4 - cl, SHOE_Z[0] + 4])
    return throat + wide
def mount_pad_cuts():
    R = R_IN
    return shoe_slot(0.2) + ycyl(3.4, 30, 0, R - 5, SHOE_Z[0] + 10) + hexprism(5.6, 2.4, 0, R - 0.01, SHOE_Z[0] + 10)
def shoe_foot():
    """T-tongue on the back of the button mount: slides down inside the wing's sunk slot"""
    R = R_IN
    foot = M.cube([7.6, 3.0, 16]).translate([-3.8, R + 6.0, SHOE_Z[0] + 5]) + M.cube([13.5, 2.2, 16]).translate([-6.75, R + 3.6, SHOE_Z[0] + 5])
    return foot - ycyl(3.4, 30, 0, R, SHOE_Z[0] + 10)
def slot_plug():
    return shoe_foot()
def _align(m, d, p):
    d = np.asarray(d, float); d /= np.linalg.norm(d)
    z = np.array([0, 0, 1.0]); v = np.cross(z, d); c = float(z @ d)
    R3 = np.eye(3) if np.linalg.norm(v) < 1e-9 else (lambda vx: np.eye(3) + vx + vx @ vx * (1 / (1 + c)))(np.array([[0, -v[2], v[1]], [v[2], 0, -v[0]], [-v[1], v[0], 0]]))
    return m.transform(np.hstack([R3, np.asarray(p, float).reshape(3, 1)]).tolist())
def _mount_frame(tilt):
    R = R_IN; p0, phi = ell_pose(MB_ANG); a = math.radians(phi); rad = np.array([math.sin(a), math.cos(a), 0.0])
    p = p0 + rad * 27.0 + np.array([0, 0, 2.0]); t, o = math.radians(tilt), math.radians(MB_SPLAY)
    d = np.array([0.0, -math.sin(t), math.cos(t)]) * math.cos(o) + rad * math.sin(o)
    return p, d / np.linalg.norm(d), rad
MB_YAWS = (-10, -5, 0)                 # three printed versions; - = rod turned away from the face        # rod yaw (deg, + = toward the face), set by the 36-tooth ring
TEETH, ROS_R0, ROS_R1 = 36, 7.0, 17.0
ROD_Z = 24.0                           # rod axis height above the arm top (about temple level)
def _rosette(z0, up=True):
    """ring of 36 tapered radial V-teeth (10 deg pitch, 1.6 mm high): true Hirth-style, so the two rings mesh without overlap"""
    h = 1.6
    def tri(r, w):
        return CS([[(-w / 2, 0), (w / 2, 0), (0, h)]]).extrude(0.2).rotate([90, 0, 0]).rotate([0, 0, 90]).translate([r, 0, 0])
    w0, w1 = 0.8 * 2 * math.pi * ROS_R0 / TEETH, 0.8 * 2 * math.pi * ROS_R1 / TEETH     # 20% backlash for print tolerance
    tooth = M.batch_hull([tri(ROS_R0, w0), tri(ROS_R1 - 0.2, w1)])
    ring = M.batch_boolean([tooth.rotate([0, 0, i * 360 / TEETH]) for i in range(TEETH)], 0)
    return ring.translate([0, 0, z0]) if up else ring.mirror([0, 0, 1]).translate([0, 0, z0])
def _mount_axes(n, H, side=1):
    zp, cy = cradle_frame(n, H)
    p, phi = ell_pose(MB_ANG); a = math.radians(phi)
    nrm = np.array([math.sin(a), math.cos(a), 0.0])
    c = p + nrm * 25.0                       # socket centre (mount sits flush on the wing) (cradle frame, unrotated side = +x)
    return zp, cy, p, nrm, c
def button_base(n, H, side=1):
    zp, cy, p, nrm, c = _mount_axes(n, H, side); R = R_IN
    back = place(rounded([(-11, R + 12.3), (11, R + 12.3), (11, R + 16), (-11, R + 16)], 1.2).extrude(19).translate([0, 0, SHOE_Z[0] + 2]), MB_ANG)
    disc = zcyl(2 * ROS_R1 + 4, 8, c[0], c[1], -4)
    shelf = M.batch_hull([disc, place(M.cube([20, 2, 14]).translate([-10, R + 14, -10]), MB_ANG)])
    m = back + place(shoe_foot(), MB_ANG) + shelf + _rosette(4.0, True).translate([c[0], c[1], 0])
    m = m - zcyl(5.5, 30, c[0], c[1], -15) - zcyl(12, 6, c[0], c[1], -12)
    m = m - place(ycyl(7.0, 20, 0, R + 12.4, SHOE_Z[0] + 10), MB_ANG)                 # access to the shoe lock screw
    m = m.translate([0, cy, zp])
    return m if side > 0 else m.mirror([1, 0, 0])
def button_head(n, H, side=1, yaw=0):
    """sits on the teeth, holds the 20 mm rod horizontally; yaw + = toward the face"""
    zp, cy, p, nrm, c = _mount_axes(n, H, side)
    d = np.array([math.sin(math.radians(-yaw)), -math.cos(math.radians(yaw)), 0.0])     # forward (-y), turned toward -x (the face) for + yaw
    disc = zcyl(2 * ROS_R1 + 4, 8, c[0], c[1], 5.6)
    sock = _align(M.cylinder(30, 14.0, 14.0, 64), d, c + np.array([0, 0, ROD_Z]) - d * 4)
    m = M.batch_hull([disc, sock])
    m = m + _rosette(5.6, False).rotate([0, 0, 180 / TEETH]).translate([c[0], c[1], 0])
    o = c + np.array([0, 0, ROD_Z])
    m = m - _align(M.cylinder(40, 10.2, 10.2, 64), d, o + d * 2)                                     # blind socket: PVC pushes against the back stop
    for k in (10.0, 20.0):                                                                           # 2 pilot holes for small wood screws (kit style)
        m = m - _align(M.cylinder(20, 1.4, 1.4, 16), np.array([0, 0, 1.0]), o + d * k)
        m = m - _align(M.cylinder(3, 1.4, 3.0, 16), np.array([0, 0, 1.0]), o + d * k + np.array([0, 0, 12.0]))
    m = m - zcyl(5.5, 12, c[0], c[1], 3) - hexprism(8.1, 4.4, 0, -2.2, 0).rotate([90, 0, 0]).translate([c[0], c[1], 9.0])   # knob nut
    m = m.translate([0, cy, zp])
    return m if side > 0 else m.mirror([1, 0, 0])
def text_cut(txt, h=5.5, depth=0.8):
    """debossing cutter for a label, in the x-z plane, cutting toward -y from y = 0"""
    from matplotlib.textpath import TextPath
    from matplotlib.font_manager import FontProperties
    tp = TextPath((0, 0), txt, size=h, prop=FontProperties(family="DejaVu Sans", weight="bold"))
    polys = [p for p in tp.to_polygons() if len(p) > 2]
    cs = CS([[(float(x), float(y)) for x, y in p] for p in polys], __import__("manifold3d").FillRule.EvenOdd)
    b = cs.bounds(); cs = cs.translate([-(b[0] + b[2]) / 2, -(b[1] + b[3]) / 2])
    return cs.extrude(depth + 0.5).translate([0, 0, -depth]).rotate([90, 0, 0])
def button_mount(n, H, side=1, tilt=None, yaw=0):
    """one printed part per angle (-10 / 0 / +10 deg), angle debossed on its outer face; slides into the wing shoe, 1 M3 lock"""
    zp, cy, p, nrm, c = _mount_axes(n, H, side); R = R_IN
    back = place(rounded([(-12, R + 8.8), (12, R + 8.8), (12, R + 12.5), (-12, R + 12.5)], 1.5).extrude(28).translate([0, 0, SHOE_Z[0] + 2]), MB_ANG)
    d = np.array([math.sin(math.radians(-yaw)), -math.cos(math.radians(yaw)), 0.0])
    o = c + np.array([0, 0, ROD_Z - 6])
    sock = _align(M.cylinder(32, 13.5, 13.5, 64), d, o - d * 6)
    m = M.batch_hull([back, sock]) + place(shoe_foot(), MB_ANG)
    m = m - _align(M.cylinder(40, 10.2, 10.2, 64), d, o - d * 2)                       # blind socket, back stop
    for k in (6.0, 16.0):
        m = m - _align(M.cylinder(20, 1.4, 1.4, 16), np.array([0, 0, 1.0]), o + d * k)
    m = m - place(ycyl(3.4, 30, 0, R + 5, SHOE_Z[0] + 10) + M.cylinder(1.9, 1.7, 3.4, 16).rotate([90, 0, 0]).translate([0, R + 12.51, SHOE_Z[0] + 10]), MB_ANG)
    # hug the wing: everything inside the shell's outer surface (+0.4) goes, except the tongue in the slot
    keep = sector(R - 12, R + SHELL_T + 0.4, -SPAN - 3, SPAN + 3, -60, 70, r=0.3).scale([1, ELL, 1])
    tongue = place(shoe_foot(), MB_ANG)
    m = (m - keep) + tongue
    m = m - (place(mount_pad(), MB_ANG) - place(shoe_slot(0.0), MB_ANG))
    lab = f"{'R' if side > 0 else 'L'} {'+' if yaw > 0 else ''}{yaw}°"
    LY, LZ = R + 25.0 + 13.5 + 0.01, ROD_Z - 6
    tc = text_cut(lab, 5.0, 1.0).rotate([0, 0, 180]).translate([0, LY, LZ])
    m = m - place(tc, MB_ANG)
    m = m.translate([0, cy, zp])
    if side > 0:
        return m
    # left side: mirror the shape, then re-cut the label so it still reads correctly
    mm = (m + (place(text_cut(lab, 5.0, 1.0).rotate([0, 0, 180]).translate([0, LY, LZ]), MB_ANG).translate([0, cy, zp]) ^ M.batch_hull([m]))).mirror([1, 0, 0])
    tl = place(text_cut(lab, 5.0, 1.0).translate([0, LY, LZ]), MB_ANG).translate([0, cy, zp]).mirror([1, 0, 0])
    return mm - tl
def pvc_button(n, H, side=1, tilt=None, yaw=0, reach=95):
    zp, cy, p, nrm, c = _mount_axes(n, H, side)
    d = np.array([math.sin(math.radians(-yaw)), -math.cos(math.radians(yaw)), 0.0])
    o = c + np.array([0, 0, ROD_Z - 6])
    rod = _align(M.cylinder(reach - 2, 10, 10, 48), d, o - d * 2)
    end = o + d * reach
    inward = np.array([-1.0, 0, 0])
    holder = _align(M.cylinder(24, 13, 13, 32), d, end - d * 12)
    disc = _align(M.cylinder(14, 32, 32, 48), inward, end + inward * 6)
    m = (rod + holder + disc).translate([0, cy, zp])
    return m if side > 0 else m.mirror([1, 0, 0])
def knob_bottom(n, H, side=1):
    zp, cy, p, nrm, c = _mount_axes(n, H, side)
    m = zcyl(5, 22, c[0], c[1], -14) + zcyl(26, 10, c[0], c[1], -24)
    m = m.translate([0, cy, zp]); return m if side > 0 else m.mirror([1, 0, 0])

def strap(n, H, kid_hl=192, kid_yb=112.0):
    zp, cy = cradle_frame(n, H)
    a = math.radians(54); R = R_IN + 2
    sx, sy = R * math.sin(a), cy + ELL * R * math.cos(a); fy = kid_yb - kid_hl - 3; b = sy - fy
    ts = np.linspace(math.pi, 2 * math.pi, 64); pts = [(sx * math.cos(t), sy + b * math.sin(t)) for t in ts]
    zc = lambda t: zp + 40 + 30 * math.sin(t - math.pi) ** 1.5
    out = []
    for i in range(len(pts) - 1):
        (x0, y0), (x1, y1) = pts[i], pts[i + 1]; a0, a1 = zc(ts[i]), zc(ts[i + 1])
        out += [[(x0, y0, a0 - 12), (x1, y1, a1 - 12), (x1, y1, a1 + 12)], [(x0, y0, a0 - 12), (x1, y1, a1 + 12), (x0, y0, a0 + 12)]]
    return np.array(out, float)

def rods(H):
    z1 = BASE_Z + H + 20
    return M.batch_boolean([zcyl(ROD_D, z1 - ROD_LOW, s * ROD_X, ROD_Y, ROD_LOW) for s in (1, -1)], 0), z1 - ROD_LOW
def knobs(H):
    out = []
    for zf in [BASE_Z] + ([BASE_Z + H] if H else []):
        z = zf - HEEL_SCREW
        t = HEEL_T0 - (HEEL_T0 - HEEL_T1) * (HEEL_SCREW / HEEL_L)
        out.append((ycyl(5, 30, 0, SY1 - 9, z), ycyl(26, 10, 0, SY1 + t, z)))
    return out
def yoke_bolts(n, H):
    zp, cy = cradle_frame(n, H)
    out = 0.3 + (R_IN + SHELL_T + YOKE_T) * ELL - R_IN * ELL         # approx head position behind the yoke at the centre
    return M.batch_boolean([place(ycyl(5.5, 2.5, 0, R_IN + (SHELL_T + YOKE_T) * ELL + 0.3, 0), a).translate([0, cy, zp]) for a in YOKE_BOLTS], 0)
TILT_H, TILTS = 30.0, (0, 8, 15)
def _rot_about(m, alpha, pivot):
    return m.translate([-pivot[0], -pivot[1], -pivot[2]]).rotate([alpha, 0, 0]).translate(list(pivot))
def tilt_block(alpha, zb):
    """wedge between the extender and the neck: tilts everything above it by alpha (deg). Turn it round for the other way."""
    pv = (0.0, ROD_Y, zb + TILT_H)
    low = rbox(-BX, BX, SY0, SY1, zb, zb + TILT_H / 2, 4.0)
    up = _rot_about(rbox(-BX, BX, SY0, SY1, zb + TILT_H / 2, zb + TILT_H, 4.0), alpha, pv)
    m = M.batch_hull([low, up]) + heel(zb)
    m = m - rod_bores(zb - 1, zb + 14) - mortise_up(zb)
    top_cuts = mortise(zb + TILT_H) + rod_bores(zb + TILT_H - 13, zb + TILT_H + 1) + heel_screw_hole(zb + TILT_H) + nut_pocket(zb + TILT_H)
    return m - _rot_about(top_cuts, alpha, pv)
def assembly(n, H, tilt=None):
    if tilt is None:
        P = dict(neck=neck(n, H), cradle=cradle(n, H), foam=foam(n, H), rods=rods(H)[0])
        keys = [key_part(BASE_Z)] + ([key_part(BASE_Z + H)] if H else [])
        P["keys"] = M.batch_boolean(keys, 0)
        if H: P["ext"] = extender(H)
        return P
    # extender H, then the tilt block, then the neck & cradle rotated about the block top
    zb = BASE_Z + H; pv = (0.0, ROD_Y, zb + TILT_H); Hn = H + TILT_H
    P = dict(tilt=tilt_block(tilt, zb), rods=M.batch_boolean([zcyl(ROD_D, zb + 13 - ROD_LOW, s_ * ROD_X, ROD_Y, ROD_LOW) for s_ in (1, -1)], 0))
    if H: P["ext"] = extender(H)
    for k, m in (("neck", neck(n, Hn)), ("cradle", cradle(n, Hn)), ("foam", foam(n, Hn))):
        P[k] = _rot_about(m, tilt, pv)
    P["pins"] = _rot_about(M.batch_boolean([zcyl(ROD_D, 33, s_ * ROD_X, ROD_Y, zb + TILT_H - 13) for s_ in (1, -1)], 0), tilt, pv)
    return P

def wing_check(F=150.0, lever=60.0):
    """side knock on a wing: bending of the shell + rims section (hand check)"""
    t, h, rr, rh = SHELL_T, (CR_Z[1] - CR_Z[0]), RIM, 4.0
    A1, c1 = t * h, t / 2; A2, c2 = 2 * rr * rh, rr / 2
    yc = (A1 * c1 + A2 * c2) / (A1 + A2)
    I = h * t ** 3 / 12 + A1 * (c1 - yc) ** 2 + 2 * rh * rr ** 3 / 12 + A2 * (c2 - yc) ** 2
    Z = I / max(yc, rr - yc)
    return F * lever / Z

if __name__ == "__main__":
    for H in HEIGHTS:
        print("H", H, "pad above seat", pad_above_seat(H), "rod", round(rods(H)[1]))
    for nm, m in (("ext50", extender(50)), ("neck0", neck(0, 50)), ("neck30", neck(30, 50)), ("neck60", neck(60, 50)), ("cradle", cradle(0, 50)), ("mount", button_mount(0, 50))):
        print(nm, round(m.volume() / 1000, 1), m.status(), [round(v) for v in m.bounding_box()])
    print("neck∩cradle", round((neck(0, 50) ^ cradle(0, 50)).volume(), 1), "N60", round((neck(60, 50) ^ cradle(60, 50)).volume(), 1))
    print("wing stress at 150 N, 60 mm: %.1f MPa; at 250 N: %.1f" % (wing_check(), wing_check(250)))
