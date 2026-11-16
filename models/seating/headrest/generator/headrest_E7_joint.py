# GbG headrest E v0.4.1 + step 1b (SKETCH FOR REVIEW, not merged): mast top = curved TENON that slides UP into an open CHANNEL (inlay, flush back)
# moulded on the cradle's lower back. Replaces the v0.4.1 yoke + tongue/groove; keeps the same 3x M3, now from inside the cradle
# (countersunk, under the foam) into nuts dropped into slots in the tenon. Everything else is the v0.4.1 baseline.
import math, numpy as np
from manifold3d import Manifold as M
import headrest_E3_geom as E
import headrest_E4_geom as G
from headrest_E3_geom import sector, ycyl, zcyl, hexprism, rounded, place
from headrest_E4_geom import rrbox, round_sector, recline, frame, col_top, MX, MY0, MY1, SOCK_Z0, TOP_Z, RODS, ROD_D, cross_cuts

R = E.R_IN; R0 = R + E.SHELL_T              # shell inner 86, outer 91 (circle space; the oval scales y by ELL)
TA = 32.0                                   # tenon half-angle: ~120 mm wide across the back of the head
TCL = 0.25                                  # slide fit (PETG on the P1S)
TT = 9.0                                    # tenon radial thickness (11.5 mm at the back once the oval stretches it)
SK_OUT = R + E.RIM                          # sleeve skin outer = the rim radius -> the back reads flush rim to rim
SW = 5.0                                    # sleeve side-wall, degrees
TZ1 = 0.0                                   # tenon top (cradle frame); pocket top = TZ1 + TCL
TZ0 = E.CR_Z[0] - 14.0                      # tenon root: 14 mm below the cradle's bottom rim, where the mast web joins
SCR = (-20.0, 0.0, 20.0)                    # 3x M3 countersunk, angles
SCR_Z = -22.0

def _S(m, cy, zc): return m.scale([1, E.ELL, 1]).translate([0, cy, zc])

# ---------------- cradle (local frame, centre at origin) ----------------
FOAM_SLOTS = [(0, 34), (40, 34), (-40, 34), (48, -30), (-48, -30)]            # lower centre slot moved off the sleeve
ZIP_HOLES = [(20, 34), (-20, 34), (60, -26), (-60, -26), (62, 8), (-62, 8)]
def _foam_fixings():
    cuts = [M.cube([22, 30, 3.5], True).translate([0, R + 5, z]).rotate([0, 0, -a]) for a, z in FOAM_SLOTS]
    cuts += [M.cube([5, 30, 3], True).translate([0, R + 5, z + dz]).rotate([0, 0, -a]) for a, z in ZIP_HOLES for dz in (-4, 4)]
    return M.batch_boolean(cuts, 0)
LIPW, LIPT = 2.0, 3.5                        # side lips (deg, radial): the tenon's edges hook under them -> cannot lift off backward
OUT = R0 + TCL + TT                         # outer back face: tenon, channel walls and top cap all flush here
def sleeve_body():
    """channel walls + top cap on the shell back, only as high as the tenon (circle space)"""
    a = TA + LIPW + SW; z0 = E.CR_Z[0]
    prof = rounded([(R0 - 1.0, z0 + 2), (OUT, z0 + 2), (OUT, TZ1 + 7.0), (R0 + 1.0, TZ1 + 20.0), (R0 - 1.0, TZ1 + 20.0)], 2.0)
    body = M.revolve(prof, 160, 2 * a).rotate([0, 0, 90 - a])
    for s_ in (1, -1):      # walls fade into the shell sideways over 10 deg
        taper = M.revolve(rounded([(R0 - 1.0, z0 + 4), (R0 + 3.0, z0 + 4), (R0 + 3.0, TZ1 + 6.0), (R0 - 1.0, TZ1 + 14.0)], 1.2), 40, 10).rotate([0, 0, 90 - (a + 10) if s_ > 0 else 90 + a])
        body = body + taper
    return body
def pocket(cl=TCL):
    """open channel: through to the back, closed on top; wider at the bottom of the channel (the lips)"""
    ch = sector(R0 - 0.01, OUT + 20, -TA - cl * 0.7, TA + cl * 0.7, E.CR_Z[0] - 20, TZ1 + cl, r=0.6)
    lip = sector(R0 - 0.01, R0 + LIPT + cl, -TA - LIPW - cl * 0.7, TA + LIPW + cl * 0.7, E.CR_Z[0] - 20, TZ1 + cl, r=0.4)
    return ch + lip
def screw_cuts():
    out = []
    for a in SCR:
        cs = M.cylinder(2.0, 3.0, 1.6, 32).rotate([-90, 0, 0])                       # 90 deg countersink, head flush with the inner face
        out.append(place(ycyl(3.4, 30, 0, R - 9, SCR_Z) + cs.translate([0, R - 1.2 - 0.6, SCR_Z]) + ycyl(6.2, 8, 0, R - 9.0, SCR_Z), a))
    return M.batch_boolean(out, 0)
def cradle_local():
    z0, z1 = E.CR_Z
    prof = rounded([(R - E.LIP, z0), (R + E.RIM, z0), (R + E.RIM, z0 + 4), (R + E.SHELL_T, z0 + 4 + 0.01), (R + E.SHELL_T, z1 - E.RIM), (R + E.RIM, z1 - 4),
                    (R + E.RIM, z1), (R, z1), (R, z0 + 30)], 1.6)
    m = M.revolve(prof, 160, 2 * E.SPAN).rotate([0, 0, 90 - E.SPAN])
    m = m + sleeve_body() - pocket()
    for s_ in (1, -1):
        m = m - M.cube([4.5, 30, 28], True).translate([0, R + 2, 40]).rotate([0, 0, -s_ * 54])
    m = m - _foam_fixings()
    m = m.scale([1, E.ELL, 1])
    m = m - screw_cuts()
    for s_ in (1, -1):
        m = m + place(E.mount_pad(), s_ * E.MB_ANG) - place(E.mount_pad_cuts(), s_ * E.MB_ANG)
    m = m - M.cylinder(200, R - 0.01, R - 0.01, 160).translate([0, 0, z0 + 30]).scale([1, E.ELL, 1])
    return m
_C = {}
def cradle(n, H):
    if "c" not in _C: _C["c"] = cradle_local()
    zc, cy = frame(n, H); return recline(_C["c"].translate([0, cy, zc]))

# ---------------- mast: v0.4.1 column / windows / foot / gussets; yoke -> tenon ----------------
def _nut_d(a):
    """distance along the screw line (oval normal at the R point) to the middle of the tenon"""
    t = math.radians(a); rn = R0 + TCL + TT / 2
    return (rn - R) * math.sqrt(math.sin(t) ** 2 + (E.ELL * math.cos(t)) ** 2) / math.sqrt(math.sin(t) ** 2 + math.cos(t) ** 2 / 1.0) * (1.0 if a == 0 else 1.0)
def tenon_cuts():
    """oval space, cradle centre at origin: nut drop-slots from the tenon top down to the screw line + screw clearance"""
    out = []
    for a in SCR:
        d = _nut_d(a)
        slot = M.cube([5.9, 2.8, TZ1 - SCR_Z + 6.5], True).translate([0, R + d, (TZ1 + SCR_Z) / 2 - 0.25])   # open at the top, down to 3.3 below the screw line
        out.append(place(slot + ycyl(3.4, 40, 0, R - 5, SCR_Z), a))
    return M.batch_boolean(out, 0)
def tenon_oval():
    t = round_sector(R0 + TCL, R0 + TCL + TT, -TA, TA, TZ0, TZ1, rr=2.5)
    t = t + sector(R0 + TCL, R0 + LIPT, -TA - LIPW + 0.3, TA + LIPW - 0.3, E.CR_Z[0] + 1, TZ1, r=0.5)   # edge flanges under the lips
    return t.scale([1, E.ELL, 1]) - tenon_cuts()
def mast(n, H):
    zt = col_top(H); zc, cy = frame(n, H)
    col = rrbox(-MX, MX, MY0, MY1, SOCK_Z0 + 0.3, zt, 6.0)
    f0, f1 = TOP_Z + 14, zt - 22
    if f1 - f0 > 30:
        nw = int((f1 - f0) // 32); h = (f1 - f0 - (nw - 1) * 7) / nw
        for i in range(nw):
            z0 = f0 + i * (h + 7); col = col - rrbox(-MX + 12, -4, MY0 - 6, MY1 + 6, z0, z0 + h, 4.0) - rrbox(4, MX - 12, MY0 - 6, MY1 + 6, z0, z0 + h, 4.0)
    # tenon (in the cradle's oval frame) + its root band below the cradle, which the web grows out of
    rootb = round_sector(R0 + TCL, R0 + TCL + TT, -TA + 4, TA - 4, TZ0, E.CR_Z[0] - 1.0, rr=2.5)
    top = rrbox(-MX, MX, MY0, MY1, zt - 14, zt, 6.0)
    m = col + tenon_oval().translate([0, cy, zc]) + M.batch_hull([top, _S(rootb, cy, zc)])
    for s_ in (1, -1):      # v0.4.1 side braces, now landing on the tenon root ends
        a = math.radians(s_ * (TA - 6)); rr = R0 + TCL + TT / 2
        tip = M.sphere(6.5, 32).translate([rr * math.sin(a), cy + E.ELL * rr * math.cos(a), zc + TZ0 + 7])
        rz0 = max(zt - 34, TOP_Z + 8); rz1 = max(rz0 + 11, zt - 4)
        root = rrbox(min(s_ * 8, s_ * MX), max(s_ * 8, s_ * MX), MY0 + 1, MY1 - 1, rz0, rz1, 6.0)
        m = m + M.batch_hull([root, tip])
    keep = _S(M.cylinder(200, R0 + TCL, R0 + TCL, 160).translate([0, 0, TZ0 - 100]), cy, zc)          # nothing inside the shell line
    cradle_env = _S(sector(R - E.LIP - 2, OUT + 1.0, -E.SPAN - 1, E.SPAN + 1, E.CR_Z[0] - 0.4, 80, r=0.3) - pocket(0), cy, zc)
    m = m - keep - cradle_env
    m = m - tenon_cuts().translate([0, cy, zc])
    m = m - M.batch_boolean([zcyl(ROD_D + 0.3, zt - 3 - SOCK_Z0 + 1, x, y, SOCK_Z0 - 1) for x, y in RODS], 0)
    m = recline(m)
    foot = rrbox(-56, 56, 124.8, 162.0, TOP_Z + 0.25, TOP_Z + 10, 5.0)
    gus = []
    zg = min(zt - 8, TOP_Z + 80)
    if zg > TOP_Z + 18:
        for s_ in (1, -1):
            low = rrbox(min(s_ * 42, s_ * 56), max(s_ * 42, s_ * 56), 127.0, 159.0, TOP_Z + 0.25, TOP_Z + 10, 4.5)
            high = recline(rrbox(min(s_ * 18, s_ * MX), max(s_ * 18, s_ * MX), MY0 + 1, MY1 - 1, zg - 12, zg, 5.0))
            gus.append(M.batch_hull([low, high]))
    m = m + foot + (M.batch_boolean(gus, 0) if gus else M())
    m = m - recline(keep) - recline(M.batch_boolean([zcyl(ROD_D + 0.3, zt - 3 - SOCK_Z0 + 1, x, y, SOCK_Z0 - 1) for x, y in RODS], 0))
    return m - cross_cuts(head=False)

def screws(n, H):
    """3x M3x16 countersunk (head inside the cradle, under the foam) + M3 nuts in the tenon slots"""
    zc, cy = frame(n, H); out = []
    for a in SCR:
        sh = ycyl(3.0, 16, 0, R - 1.8, SCR_Z) + M.cylinder(1.7, 2.9, 1.5, 32).rotate([-90, 0, 0]).translate([0, R - 1.8, SCR_Z])
        nut = hexprism(5.5, 2.4, 0, 0, 0).rotate([0, 30, 0]).translate([0, R + _nut_d(a) - 1.2, SCR_Z])   # flats on the slot walls: drops in, cannot spin
        out.append(place(sh + nut, a))
    return recline(M.batch_boolean(out, 0).translate([0, cy, zc]))
def foam(n, H): return G.foam(n, H)
def assembly(n, H): return dict(mast=mast(n, H), cradle=cradle(n, H), foam=foam(n, H), rods=G.rods(H))

if __name__ == "__main__":
    r = G.top_back_receiver()
    for H, n in ((0, 0), (0, 45), (0, -15), (75, 15), (125, 0), (125, 45)):
        A = assembly(n, H); mo = G.mast(n, H)
        print(H, n, A["mast"].status(), "mast cm3 %.1f (v0.4.1 %.1f)" % (A["mast"].volume() / 1000, mo.volume() / 1000), "cradle %.1f" % (A["cradle"].volume() / 1000),
              "mast∩cr %.1f" % (A["mast"] ^ A["cradle"]).volume(), "mast∩chair %.1f" % (A["mast"] ^ r).volume(), "cr∩chair %.1f" % (A["cradle"] ^ r).volume(),
              "screws∩mast %.1f" % (screws(n, H) ^ A["mast"]).volume(), "foam∩mast %.1f" % (A["foam"] ^ A["mast"]).volume())
