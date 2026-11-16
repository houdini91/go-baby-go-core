# GbG headrest: MAST -> HEAD PAD connection options (sketches for review, on the P2 pad; pad-local frame, recline left out).
# Frame: pad oval axis at x=y=0, +y = toward the mast/back of the head, z=0 ~ widest part of the skull.
# The mast column (56 x 26) sits at y 103..129, exactly where it sits behind the backrest for reach n=0 (same as E7).
#   A  CHANNEL : curved tenon on the mast slides UP into an open channel on the pad back (= step 1b), 3x M3 from inside.
#   B  HINGE   : pad HANGS on a bar between two mast cheeks, swings onto a lock bolt; arc slot = tilt -5..+15 deg.
#   C  DOVETAIL: flat TAPERED dovetail on the mast top, pad drops on and wedges tighter under load; 1x M4 cross screw.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_P2_pad as P
import headrest_E3_geom as E
import headrest_E7_joint as J
from headrest_E3_geom import sector, place
from headrest_E_geom import rounded, ycyl, xcyl, hexprism
from headrest_E4_geom import rrbox, round_sector

R, ELL, T = P.R, P.ELL, P.T
CY0, CY1, CX = 103.2, 129.2, 28.0            # mast column (pad frame)
STUB = -115.0                                 # stub bottom (the mast carries on down to the chair)
def oval(m): return m.scale([1, ELL, 1])
def yz(poly, x0, x1, r=0.0):
    """2D outline in (y, z), extruded along x from x0 to x1"""
    cs = CS([poly]); cs = cs if cs.area() > 0 else CS([poly[::-1]])
    if r: cs = cs.offset(-r, JoinType.Round).offset(r, JoinType.Round)
    return cs.extrude(x1 - x0).translate([0, 0, x0]).warp_batch(lambda v: np.asarray(v)[:, [2, 0, 1]])
def yz_cs(cs, x0, x1):
    return cs.extrude(x1 - x0).translate([0, 0, x0]).warp_batch(lambda v: np.asarray(v)[:, [2, 0, 1]])
def circ(r, y, z, n=48): return CS.circle(r, n).translate([y, z])

_H = {}
def headside(back=0.6):
    """everything on the head side of the shell's outer face (minus `back`), following the real pad shape"""
    if back in _H: return _H[back]
    w0 = -40.0; th = 40.0 + T - back
    pl = CS.square([2 * P.arc(95), 200]).translate([-P.arc(95), -95]).extrude(th).refine_to_length(2.0)
    _H[back] = pl.warp_batch(lambda v: P._bend(np.asarray(v), w0)); return _H[back]
def pad(): return P.pad_local()
def foam(): return P.foam_local()
def column(top, y0=CY0, y1=CY1):
    return rrbox(-CX, CX, y0, y1, STUB, top, 6.0)

# ======================= A: curved tenon in an open channel (step 1b on the new pad) =======================
A_Z0 = -42.0                    # channel body starts here (above the column top)
J.SCR_Z = -10.0                 # screw line moved up to where the shell is 5 mm (roll is gone)
def A_pad():
    a = J.TA + J.LIPW + J.SW
    prof = rounded([(60.0, A_Z0), (J.OUT, A_Z0), (J.OUT, J.TZ1 + 7.0), (J.R0 + 1.0, J.TZ1 + 20.0), (60.0, J.TZ1 + 20.0)], 2.0)
    body = oval(M.revolve(prof, 160, 2 * a).rotate([0, 0, 90 - a])) - headside()
    m = pad() + body - oval(J.pocket())
    for s in J.SCR:
        cs = M.cylinder(2.0, 3.0, 1.6, 32).rotate([-90, 0, 0])
        m = m - place(ycyl(3.4, 30, 0, R - 9, J.SCR_Z) + cs.translate([0, R - 1.8, J.SCR_Z]), s)
    return m
def A_mast():
    top = -46.0
    col = column(top)
    rootb = round_sector(J.R0 + J.TCL, J.R0 + J.TCL + J.TT, -J.TA + 4, J.TA - 4, J.TZ0, E.CR_Z[0] - 1.0, rr=2.5)
    m = col + J.tenon_oval() + M.batch_hull([rrbox(-CX, CX, CY0, CY1, top - 14, top, 6.0), oval(rootb)])
    m = m - oval(M.cylinder(200, J.R0 + J.TCL, J.R0 + J.TCL, 160).translate([0, 0, J.TZ0 - 100]))
    env = sector(R - 20, J.OUT + 1.0, -70, 70, A_Z0 - 0.4, 80, r=0.3) - J.pocket(0)
    m = m - oval(env)                                   # tenon trimmed to the channel inside the pad zone
    return m - J.tenon_cuts()
def A_screws():
    out = []
    for s in J.SCR:
        sh = ycyl(3.0, 16, 0, R - 1.8, J.SCR_Z) + M.cylinder(1.7, 2.9, 1.5, 32).rotate([-90, 0, 0]).translate([0, R - 1.8, J.SCR_Z])
        nut = hexprism(5.5, 2.4, 0, 0, 0).rotate([0, 30, 0]).translate([0, R + J._nut_d(s) - 1.2, J.SCR_Z])
        out.append(place(sh + nut, s))
    return M.batch_boolean(out, 0)
A_OFF = (0, 0, 70)              # apart: pad lifted straight up

# ======================= B: hang-on hinge + tilt lock =======================
BAR = (122.0, -34.0)            # hinge bar axis (y, z), along x
BOLT = (116.0, -52.0)           # lock bolt axis (y, z), along x
CHX = (16.3, 27.0)              # cheek x range (each side)
TILT = (-5.0, 15.0)             # + = top of the pad forward (toward the face)
def _rot_yz(m, deg, c=BAR):
    return m.translate([0, -c[0], -c[1]]).rotate([deg, 0, 0]).translate([0, c[0], c[1]])
def B_lug():
    by, bz = BAR; ly, lz = BOLT
    rb = math.hypot(ly - by, lz - bz); a0 = math.degrees(math.atan2(lz - bz, ly - by))
    body = CS.batch_hull([circ(12.0, by, bz), circ(8.5, ly, lz), CS.square([12, 34]).translate([98, -54])])
    hole = circ(6.25, by, bz)
    mouth = CS.batch_hull([circ(6.25, by, bz), circ(6.25, by + 14, bz - 14)])                  # hook opens DOWN-and-BACK: lower the pad back onto the bar
    slot = CS.batch_hull([circ(3.25, by + rb * math.cos(math.radians(a)), bz + rb * math.sin(math.radians(a))) for a in np.linspace(a0 - TILT[1], a0 - TILT[0], 9)])
    cs = body - hole - mouth - slot
    lug = yz_cs(cs, -15.7, 15.7)
    for s in (1, -1):   # ribs: lug blends into the shell
        lug = lug + M.batch_hull([rrbox(min(s * 15.7, s * 6), max(s * 15.7, s * 6), 99, 103, -46, -22, 1.5), rrbox(min(s * 30, s * 26), max(s * 30, s * 26), 96, 99, -40, -28, 1.0)])
    return lug - headside(1.0)
def B_pad(): return pad() + B_lug()
def B_mast():
    top = -64.0
    col = column(top)
    by, bz = BAR; ly, lz = BOLT
    prof = CS.batch_hull([circ(9.0, by, bz), circ(8.5, ly, lz), CS.square([CY1 - CY0, 6]).translate([CY0, top - 6])])
    m = col
    for s in (1, -1):
        m = m + yz_cs(prof, min(s * CHX[0], s * CHX[1]), max(s * CHX[0], s * CHX[1]))
        m = m + rrbox(min(s * 14, s * CX), max(s * 14, s * CX), CY0, CY1, top - 12, top + 2, 5.0)   # cheek roots
    m = m + xcyl(12.0, 2 * CHX[0] + 1, -CHX[0] - 0.5, by, bz)                              # printed bar Ø12
    m = m - xcyl(6.4, 80, -40, ly, lz)                                                      # M6 lock bolt
    m = m - M.cylinder(4, 5.6, 5.6, 6).rotate([0, 90, 0]).translate([CHX[1] - 3.9, ly, lz])  # hex nut pocket (outside)
    return m
def B_bolt():
    ly, lz = BOLT
    return xcyl(6.0, 2 * CHX[1] + 2, -CHX[1] - 3, ly, lz) + xcyl(13.0, 4.5, -CHX[1] - 7.5, ly, lz)
B_OFF = (0, -36, 36)            # apart: pad lifted forward-up off the bar

# ======================= C: flat tapered dovetail, drops on and wedges =======================
C_BACK = 128.0                  # pad back face over the joint (A's channel back is at 128.3)
C_Z = (-44.0, 2.0)              # dovetail bottom / top on the pad
C_W = (44.0, 30.0)              # neck width at the bottom / top (taper)
C_FL, C_FD, C_ND = 3.0, 4.0, 3.5  # flange overhang each side, flange depth, neck depth
C_CL = 0.25
def _dt(cl, z0, z1, back_extra=0.0):
    """dovetail solid: flange (front) + neck (to the back face), tapered in z"""
    def sl(z):
        t = (z - C_Z[0]) / (C_Z[1] - C_Z[0]); w = C_W[0] + (C_W[1] - C_W[0]) * t
        yb = C_BACK + back_extra; yf = C_BACK - C_ND - C_FD
        fl = M.cube([w + 2 * C_FL + 2 * cl, C_FD + 2 * cl, 0.01]).translate([-w / 2 - C_FL - cl, yf - cl, z])
        nk = M.cube([w + 2 * cl, yb - yf + cl, 0.01]).translate([-w / 2 - cl, yf - cl, z])
        return fl, nk
    a, b = sl(z0), sl(z1)
    return M.batch_hull([a[0], b[0]]) + M.batch_hull([a[1], b[1]])
def C_pad():
    boss = rrbox(-34, 34, 88, C_BACK, C_Z[0] + 2, C_Z[1] + 12, 6.0) - headside()
    m = pad() + boss - _dt(C_CL, C_Z[0] - 30, C_Z[1], 20)
    return m - xcyl(4.4, 90, -45, C_BACK - C_ND - C_FD / 2, -34) - M.cylinder(3.5, 4.2, 4.2, 6).rotate([0, 90, 0]).translate([30.5, C_BACK - C_ND - C_FD / 2, -34])
def C_mast():
    top = -50.0
    m = column(top) + _dt(0.0, top - 1, C_Z[1] - 0.25 - 0.0)
    m = m ^ rrbox(-60, 60, 0, C_BACK, -300, 50, 0.5)          # back flush with the pad
    return m - xcyl(4.4, 90, -45, C_BACK - C_ND - C_FD / 2, -34)
def C_screw():
    y = C_BACK - C_ND - C_FD / 2
    return xcyl(4.0, 66, -35, y, -34) + M.cylinder(3, 3.8, 3.8, 32).rotate([0, 90, 0]).translate([-38, y, -34])
C_OFF = (0, 0, 60)

OPT = dict(A=(A_pad, A_mast, A_screws, A_OFF), B=(B_pad, B_mast, B_bolt, B_OFF), C=(C_pad, C_mast, C_screw, C_OFF))
if __name__ == "__main__":
    import time
    f = foam()
    for k, (fp, fm, fs, off) in OPT.items():
        t = time.time(); p, m, s = fp(), fm(), fs()
        out = [k, "pad %s %.0f cm3" % (p.status(), p.volume() / 1000), "mast %s" % m.status(), "pad∩mast %.1f" % (p ^ m).volume(),
               "screw∩pad %.1f" % (s ^ p).volume(), "screw∩mast %.1f" % (s ^ m).volume(), "mast∩foam %.1f" % (m ^ f).volume(),
               "apart∩ %.1f" % (p.translate(list(off)) ^ m).volume(), "back y %.1f" % p.bounding_box()[4]]
        if k == "B":
            for a in (TILT[0], 5.0, TILT[1]): out.append("tilt %+.0f: pad∩mast %.1f" % (a, (_rot_yz(p, a) ^ m).volume()))
        print(" | ".join(out), "%.0fs" % (time.time() - t))
