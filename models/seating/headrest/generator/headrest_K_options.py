# GbG headrest: "A-style" seamless mast->pad joints, FLUSH BOTTOM (sketches for review; P2 pad, pad-local frame, recline left out).
# Common end result: the mast top is a curved TENON (back face = the pad's back surface) + the COLUMN rising 16 mm INTO the pad body.
# The pad body's bottom edge, the tenon's bottom and the column all meet on one line; below the pad only the column shows,
# its back corners rounded to the same oval so it reads as one surface. The shape carries the load; screws only lock.
#   K1  slide down, dovetail lips, 3x M3 from the head side (under the foam)            (= A, bottom fixed)
#   K2  slide down, TAPERED channel (wedges tight, no rattle), 2x M3 flush in the side faces
#   K3  hang on a hidden HINGE KNUCKLE at the top, swing the bottom in, 2x M3 from BELOW (invisible)
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_P2_pad as P
import headrest_J_options as JO
import headrest_E7_joint as J
from headrest_E3_geom import sector, place
from headrest_E_geom import rounded, ycyl, xcyl, hexprism
from headrest_E4_geom import rrbox

R, ELL = P.R, P.ELL
R0, TCL, TT, TA, LIPW, LIPT, SW = 91.0, 0.25, 9.0, 32.0, 2.0, 3.5, 5.0
TI = R0 + TCL; TO = TI + TT; AW = TA + LIPW + SW          # tenon in/out radius (circle space); body half-angle 39
BOT, TZ1 = -52.0, 0.0                                       # common bottom line; tenon top
COL_X, COL_Y0, COL_TOP = 28.0, 103.2, -36.0                 # column (pad frame) rises 16 mm into the pad body
STUB = -115.0
def oval(m): return m.scale([1, ELL, 1])
headside = JO.headside
def ocyl(r, z0, z1): return oval(M.cylinder(z1 - z0, r, r, 180).translate([0, 0, z0]))

# ---------------- common parts ----------------
def body(cap_z=(7.0, 20.0)):
    prof = rounded([(60.0, BOT), (TO, BOT), (TO, cap_z[0]), (R0 + 1.0, cap_z[1]), (60.0, cap_z[1])], 2.0)
    b = oval(M.revolve(prof, 160, 2 * AW).rotate([0, 0, 90 - AW]))
    return b - headside()
def col_socket(cl=TCL):
    return rrbox(-COL_X - cl, COL_X + cl, COL_Y0 - cl, 200, BOT - 30, COL_TOP + cl, 6.0)
def column(top=COL_TOP):
    c = rrbox(-COL_X, COL_X, COL_Y0, 140, STUB, top, 6.0)
    return c ^ ocyl(TO, STUB - 1, 100)                       # back corners on the pad's oval: flush all the way down
def tenon(lips=True):
    t = sector(TI, TO, -TA, TA, BOT, TZ1, r=1.5)
    if lips: t = t + sector(TI, R0 + LIPT, -TA - LIPW + 0.3, TA + LIPW - 0.3, BOT, TZ1, r=0.5)
    return oval(t)
def channel(lips=True, cl=TCL):
    c = sector(R0 - 0.01, TO + 20, -TA - cl * 0.7, TA + cl * 0.7, BOT - 20, TZ1 + cl, r=0.6)
    if lips: c = c + sector(R0 - 0.01, R0 + LIPT + cl, -TA - LIPW - cl * 0.7, TA + LIPW + cl * 0.7, BOT - 20, TZ1 + cl, r=0.4)
    return oval(c)
# ---- carbon SPINE: 2 rods run from the chair receiver up the mast and ~32 mm INTO the pad body (past the joint line) ----
SPINE = ((-16.0, 118.0), (16.0, 118.0))     # (x, y) pad frame = world y 146.8, inside the 56 x 26 column
ROD_D, BOSS_D, ROD_TOP, BOSS_TOP = 8.0, 14.0, -20.0, -16.0
def spine_boss(cl=0.0, down=0.0):
    out = []
    for x, y in SPINE:
        r = BOSS_D / 2 + cl
        out.append(M.batch_hull([M.cylinder(1, r, r, 40).translate([x, y, BOT - 2 - down]), M.sphere(r, 40).translate([x, y, BOSS_TOP - r + cl])]))
    return M.batch_boolean(out, 0)
def rods(z0=STUB):
    return M.batch_boolean([M.cylinder(ROD_TOP - z0, ROD_D / 2, ROD_D / 2, 32).translate([x, y, z0]) for x, y in SPINE], 0)
def rod_holes(): return M.batch_boolean([M.cylinder(ROD_TOP + 0.5 - STUB + 2, ROD_D / 2 + 0.15, ROD_D / 2 + 0.15, 32).translate([x, y, STUB - 1]) for x, y in SPINE], 0)
def pad(): return P.pad_local()
def foam(): return P.foam_local()

# ---------------- K1: slide down + lips + 3 screws from inside ----------------
J.SCR_Z = -10.0
def K1_pad():
    m = pad() + body() - channel() - col_socket() - spine_boss(TCL, 30)
    for s in J.SCR:
        cs = M.cylinder(2.0, 3.0, 1.6, 32).rotate([-90, 0, 0])
        m = m - place(ycyl(3.4, 30, 0, R - 9, J.SCR_Z) + cs.translate([0, R - 1.8, J.SCR_Z]), s)
    return m
def K1_mast(): return column() + tenon() + spine_boss() - J.tenon_cuts() - rod_holes()
def K1_screws(): return JO.A_screws()

# ---------------- K2: slide down, tapered (wedge) + 2 flush side screws ----------------
TAPER = 3.0                                     # deg narrower at the top than at the bottom (each side)
def _taper(m):
    def f(v):
        v = np.asarray(v).copy(); x, y, z = v[:, 0], v[:, 1] / ELL, v[:, 2]
        a = np.arctan2(x, y); r = np.hypot(x, y)
        k = 1 - (TAPER / TA) * np.clip((z - BOT) / (TZ1 - BOT), 0, 1)
        a = a * k
        return np.stack([r * np.sin(a), ELL * r * np.cos(a), z], axis=1)
    return m.refine_to_length(2.5).warp_batch(f)
SIDE_Z = -24.0
def _side_screw(s, d, L0, L1, head=True):
    """horizontal screw along the oval tangent, entering the body's side face (circle angle s*AW) at mid-tenon radius"""
    a = math.radians(s * (AW - 0.2)); rm = (TI + TO) / 2
    p = np.array([rm * math.sin(a), ELL * rm * math.cos(a), SIDE_Z])
    t = np.array([math.cos(a), -ELL * math.sin(a), 0.0]); t = -s * t / np.linalg.norm(t)    # into the body
    ang = math.degrees(math.atan2(t[1], t[0]))
    c = M.cylinder(L1 - L0, d / 2, d / 2, 24).rotate([0, 90, 0]).translate([L0, 0, 0])
    if head: c = c + M.cylinder(1.8, 3.15, 1.4, 24).rotate([0, 90, 0]).translate([-0.05, 0, 0])
    return c.rotate([0, 0, ang]).translate(p.tolist())
def K2_pad():
    m = pad() + body() - _taper(channel()) - col_socket() - spine_boss(TCL, 30)
    for s in (1, -1): m = m - _side_screw(s, 3.4, -5, 16.5)
    return m
def K2_mast():
    m = column() + _taper(tenon()) + spine_boss() - rod_holes()
    for s in (1, -1): m = m - _side_screw(s, 2.5, 6, 26, head=False)     # pilot for M3x20 (or a heat-set insert)
    return m
def K2_screws(): return M.batch_boolean([_side_screw(s, 3.0, 0, 20) for s in (1, -1)], 0)

# ---------------- K3: hidden hinge knuckle on top, swing in, 2 screws from below ----------------
PV = (121.5, 4.0)                 # knuckle axis (y, z) along x
KR, KW = 4.0, 20.0                # knuckle radius, half-width
SWING = 25.0                      # hang-on angle (bottom of the pad forward); lowered straight down onto the knuckle at this angle
WING_X = 30.0                     # tenon wings outboard of this sit on the pad's ledges
RW = PV[1] - (BOT + 6.0)          # ledge/wing interface: a cylinder about the hinge axis (sliding, never jams)
SCRB = (42.0, -1)                 # bottom screws at x = +-42
def _rot(m, deg): return m.translate([0, -PV[0], -PV[1]]).rotate([deg, 0, 0]).translate([0, PV[0], PV[1]])
def knuckle(r=KR, w=KW):
    return M.batch_hull([M.sphere(r, 32).translate([s * (w - r), PV[0], PV[1]]) for s in (1, -1)]) + xcyl(2 * r, 2 * (w - r), -(w - r), PV[0], PV[1])
def _wing_zone(extra=0.0):
    """outboard of the column, outside the hinge cylinder RW: the ledge lives here"""
    cyl = xcyl(2 * (RW + extra), 300, -150, PV[0], PV[1])
    side = M.cube([300, 400, 300], True).translate([150 + WING_X + extra, 0, 0]) + M.cube([300, 400, 300], True).translate([-150 - WING_X - extra, 0, 0])
    return side - cyl
def _bscrew_xyz(s):
    x = s * SCRB[0]; rm = (TI + TO) / 2; a = math.asin(x / rm)
    return x, ELL * rm * math.cos(a)
def K3_mast():
    m = column() + tenon(lips=False) + knuckle() + spine_boss()
    m = m - (_wing_zone() ^ M.cube([400, 400, 200], True).translate([0, 0, BOT - 100 + 80]))
    for s in (1, -1):
        x, y = _bscrew_xyz(s); m = m - M.cylinder(14, 1.25, 1.25, 20).translate([x, y, BOT + 6 - 2])
    return m - rod_holes()
_K3 = {}
def K3_pad():
    if "p" in _K3: return _K3["p"]
    m = pad() + body(cap_z=(12.0, 24.0)) - channel(lips=False) - col_socket() - spine_boss(TCL, 30)
    m = m + (body() ^ _wing_zone(0.3) ^ M.cube([400, 400, 30], True).translate([0, 0, BOT + 15]))          # ledges under the wings
    m = m - knuckle(KR + 0.25, KW + 0.3) - M.batch_hull([knuckle(KR + 0.25, KW + 0.3), knuckle(KR + 0.25, KW + 0.3).translate([0, 9 * math.sin(math.radians(SWING)), -9 * math.cos(math.radians(SWING))])])   # groove opens straight DOWN when the pad hangs at SWING
    mz = (K3_mast() ^ M.cube([400, 400, 120], True).translate([0, 0, BOT + 50]))
    m = m - M.batch_boolean([_rot(mz, a) for a in np.arange(1.5, SWING + 1.6, 1.5)], 0)                       # swing path is always clear
    for s in (1, -1):
        x, y = _bscrew_xyz(s)
        m = m - M.cylinder(12, 1.7, 1.7, 24).translate([x, y, BOT - 1]) - M.cylinder(1.8, 3.1, 1.5, 24).translate([x, y, BOT - 0.05])
    _K3["p"] = m; return m
def K3_screws():
    out = []
    for s in (1, -1):
        x, y = _bscrew_xyz(s); out.append(M.cylinder(16, 1.5, 1.5, 24).translate([x, y, BOT]) + M.cylinder(1.7, 2.9, 1.5, 24).translate([x, y, BOT]))
    return M.batch_boolean(out, 0)

OPT = dict(K1=(K1_pad, K1_mast, K1_screws, (0, 0, 70)), K2=(K2_pad, K2_mast, K2_screws, (0, 0, 70)), K3=(K3_pad, K3_mast, K3_screws, None))
def K3_apart(m): return _rot(m, -SWING).translate([0, 0, 40])       # tilted (bottom forward) and lifted
if __name__ == "__main__":
    import time, sys
    f = foam()
    for k in (sys.argv[1:] or OPT):
        fp, fm, fs, off = OPT[k]; t = time.time(); p, m, s = fp(), fm(), fs()
        out = [k, "pad %s %.0f cm3" % (p.status(), p.volume() / 1000), "mast %s" % m.status(), "pad∩mast %.1f" % (p ^ m).volume(),
               "screw∩pad %.1f" % (s ^ p).volume(), "screw∩mast %.1f" % (s ^ m).volume(), "mast∩foam %.1f" % (m ^ f).volume(),
               "pad bottom %.1f" % p.bounding_box()[2], "rods∩pad %.1f" % (rods() ^ p).volume(), "rods outside mast %.1f" % (rods() - (m + rod_holes())).volume(), "rods∩mast %.1f" % (rods() ^ m).volume(), "back y %.1f" % p.bounding_box()[4]]
        if k == "K3":
            out.append("swing " + " ".join("%g:%.1f" % (a, (_rot(p, -a) ^ m).volume()) for a in (0.7, 3, 6, 9, 12, 15)))
            out.append("lower " + " ".join("%g:%.1f" % (dz, (_rot(p, -SWING).translate([0, 0, dz]) ^ m).volume()) for dz in (0.3, 1, 2, 3, 5, 8, 12, 20, 40)))
            out.append("swing " + " ".join("%g:%.1f" % (a, (_rot(p, -a) ^ m).volume()) for a in (18, 21, 24)))
        else:
            out.append("slide " + " ".join("%g:%.1f" % (dz, (p.translate([0, 0, dz]) ^ m).volume()) for dz in (1, 10, 30, 55)))
        print(" | ".join(out), "%.0fs" % (time.time() - t), flush=True)
