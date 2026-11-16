# GbG headrest: push-button mount JOINT options (concept sketches on a flat patch of the pad shell; review only).
# Local frame: shell outer face at y = 0 (shell is y -5..0, head side = -y), +y out, +z up, the PVC rod points forward = -x.
# Every option: click-in by shape, 1x M3 from the head side (under the foam) just to hold it shut; the PLASTIC takes the push.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
from headrest_E3_geom import rounded

SH = 5.0                                    # pad shell
ROD = (0.0, 18.0, 30.0)                     # PVC socket axis point (x, y, z): 18 mm out from the shell, 30 mm up
def rr(x0, x1, y0, y1, z0, z1, r=2.0):
    r = min(r, (x1 - x0) / 2 - 0.01, (y1 - y0) / 2 - 0.01, (z1 - z0) / 2 - 0.01)
    sp = M.sphere(r, 20)
    return M.batch_hull([sp.translate([x, y, z]) for x in (x0 + r, x1 - r) for y in (y0 + r, y1 - r) for z in (z0 + r, z1 - r)])
def xcyl(d, x0, x1, y, z, n=48): return M.cylinder(x1 - x0, d / 2, d / 2, n).rotate([0, 90, 0]).translate([x0, y, z])
def ycyl(d, y0, y1, x, z, n=48): return M.cylinder(y1 - y0, d / 2, d / 2, n).rotate([-90, 0, 0]).translate([x, y0, z])
def hexnut_y(af, h, x, y0, z):
    r = af / math.sqrt(3)
    return CS([[(r * math.cos(math.radians(a)), r * math.sin(math.radians(a))) for a in range(30, 390, 60)]]).extrude(h).rotate([-90, 0, 0]).translate([x, y0, z])

def patch(z0=-30, z1=56, w=46):
    """piece of the pad shell, top edge rounded (the pad's top bead)"""
    return rr(-w, w, -SH, 0, z0, z1, 2.4)
def screw(z, y_head=-SH, L=16):
    """M3 countersunk from the head side"""
    return ycyl(3.0, y_head, y_head + L, 0, z) + M.cylinder(1.7, 2.9, 1.5, 24).rotate([-90, 0, 0]).translate([0, y_head, z])
def screw_hole(z): return ycyl(3.4, -SH - 1, 2, 0, z) + M.cylinder(1.8, 3.1, 1.5, 24).rotate([-90, 0, 0]).translate([0, -SH - 0.01, z])
def socket_body(y_back):
    """the PVC socket block: rod Ø20 into a blind socket (back stop) + 2 pilot holes for the kit wood screws"""
    x, y, z = ROD
    boss = xcyl(27, x - 20, x + 14, y, z) + rr(-12, 12, y_back, y, z - 12, z + 4, 3.0)
    boss = M.batch_hull([xcyl(27, x - 20, x + 14, y, z), rr(-13, 13, y_back, y_back + 3, z - 16, z + 6, 2.5)])
    boss = boss - xcyl(20.3, x - 21, x + 8, y, z)
    for k in (-14.0, -6.0): boss = boss - M.cylinder(14, 1.4, 1.4, 16).translate([k, y, z + 6])
    return boss
def pvc():
    x, y, z = ROD
    return xcyl(20, x - 70, x + 8, y, z, 40)

# ---------------- A: HINGE HOOK at the bottom + snap at the top ----------------
A_BAR = (0.0, 4.5, -10.0)                    # bar axis (x, y, z): stands 4.5 mm off the shell
def A_pad():
    _, by, bz = A_BAR
    m = patch()
    cheeks = M.batch_boolean([rr(s * 13, s * 21, -1, by + 4.5, bz - 6.5, bz + 6.5, 2.5) if s > 0 else rr(-21, -13, -1, by + 4.5, bz - 6.5, bz + 6.5, 2.5) for s in (1, -1)], 0)
    bar = xcyl(6.0, -16, 16, by, bz)
    seat = M.batch_hull([rr(-13, 13, -1, 3.0, bz - 14, bz - 12, 1.0), rr(-13, 13, -1, 0.6, bz - 6, bz - 5.5, 0.3)])   # 30° ramp under the bar: the heel lands here
    m = m + cheeks + bar
    m = m - rr(-4, 4, -SH - 1, 1, 34, 38, 0.8)           # snap window through the shell
    return m - screw_hole(12)
def A_mount():
    _, by, bz = A_BAR
    plate = rr(-12.6, 12.6, 0.3, 6.0, bz + 3.6, 40, 2.5)
    hook = xcyl(13.0, -12.6, 12.6, by, bz) - xcyl(6.4, -13, 13, by, bz)
    hook = hook - rr(-14, 14, by - 1, by + 9, bz + 0.5, bz + 8, 0.5).rotate([0, 0, 0])          # opening up-and-out: hook on, then swing in
    hook = hook - rr(-14, 14, -9, 0.3, bz - 9, bz + 9, 0.5)                                     # nothing behind the shell face
    finger = rr(-3.5, 3.5, -SH - 0.2, 0.6, 34.3, 37.7, 0.6)                                     # snap finger through the window
    barb = M.batch_hull([rr(-3.5, 3.5, -SH - 1.6, -SH - 0.2, 33.0, 37.7, 0.4), rr(-3.5, 3.5, -SH - 0.6, -SH - 0.2, 37.0, 37.7, 0.2)])
    relief = rr(-6, 6, 2.2, 7, 30, 42, 0.5) - rr(-3.6, 3.6, 0, 8, 29, 43, 0.4)                    # slot round the finger: it flexes
    m = plate + hook + socket_body(5.0) + finger + barb
    m = m - (ycyl(3.4, -1, 9, 0, 12) + hexnut_y(5.6, 2.6, 0, 3.5, 12))
    return m
def A_open(m):
    _, by, bz = A_BAR
    return m.translate([0, -by, -bz]).rotate([-28, 0, 0]).translate([0, by, bz])

# ---------------- B: TRIANGLE POCKET, push in + click ----------------
B_TRI = [(-15.0, 36.0), (15.0, 36.0), (0.0, 6.0)]     # (x, z) point down; 30 wide, 30 tall
B_D = 8.0                                              # pocket depth (FINAL 2026-11-15: 4 -> 8 mm for the 20-25 Nm switch target)
def _tri(off, d0, d1):
    cs = CS([[(x, z) for x, z in B_TRI]]); cs = cs if cs.area() > 0 else CS([[(x, z) for x, z in B_TRI[::-1]]])
    cs = cs.offset(-5, JoinType.Round).offset(5 + off, JoinType.Round)
    return cs.extrude(d1 - d0).rotate([90, 0, 0]).translate([0, d1, 0]).mirror([0, 0, 0]) if False else cs.extrude(d1 - d0).rotate([-90, 0, 0]).translate([0, d0, 0]).mirror([0, 0, 1]).mirror([0, 0, 1])
def B_tri_solid(off, y0, y1):
    cs = CS([[(x, -z) for x, z in B_TRI]]); cs = cs if cs.area() > 0 else CS([[(x, -z) for x, z in B_TRI[::-1]]])
    cs = cs.offset(-5, JoinType.Round).offset(5 + off, JoinType.Round)
    return cs.extrude(y1 - y0).rotate([-90, 0, 0]).translate([0, y0, 0])     # rotate(-90 about x): (x, z_local) -> (x, ..., -z_local) handled by the -z above
def B_pad():
    m = patch() + B_tri_solid(4.0, -SH - 3.0, 0).translate([0, 0, 0])            # 3 mm boss on the head side behind the pocket (under the foam)
    m = m - B_tri_solid(0.15, -B_D, 1)
    for s in (1, -1):                                                           # click dimples in the two upper side walls
        m = m - M.sphere(1.3, 16).translate([s * 9.3, -B_D / 2, 24])
    return m - screw_hole(18)
def B_mount():
    wedge = B_tri_solid(-0.05, -B_D + 0.2, 0.3)
    plate = M.batch_hull([B_tri_solid(1.0, 0.3, 2.5), rr(-13, 13, 2.0, 5.0, 14, 40, 2.5)])
    m = wedge + plate + socket_body(4.5)
    for s in (1, -1): m = m + M.sphere(1.1, 16).translate([s * 8.9, -B_D / 2, 24])
    return m - (ycyl(3.4, -B_D - 1, 9, 0, 18) + hexnut_y(5.6, 2.6, 0, 1.0, 18))

# ---------------- C: RIM SADDLE - hooks over the pad's top edge, swings down, clicks onto a ledge ----------------
C_TOP = 56.0
def C_pad():
    m = patch()
    ledge = M.batch_hull([rr(-12, 12, -0.5, 4.5, 2.0, 4.0, 1.0), rr(-12, 12, -0.5, 0.8, 9.0, 10.0, 0.4)])    # 45° wedge ledge with a lip
    return m + ledge - screw_hole(26)
def C_mount():
    saddle = rr(-13, 13, -SH - 3.4, 6.0, C_TOP - 2.0, C_TOP + 3.5, 2.0)
    inner = rr(-13, 13, -SH - 3.4, -SH - 0.25, C_TOP - 9, C_TOP + 1, 1.5)         # short lip down the head side (sits above the foam)
    plate = rr(-13, 13, 0.3, 5.0, 4.5, C_TOP + 1, 2.5)
    foot = M.batch_hull([rr(-12, 12, 0.3, 7.2, 4.4, 5.6, 0.5), rr(-12, 12, 0.3, 3.0, 10.5, 11.5, 0.5)])
    foot = foot - M.batch_hull([rr(-13, 13, -0.6, 4.7, 1.8, 4.2, 1.0), rr(-13, 13, -0.6, 0.9, 8.8, 10.2, 0.4)])   # seat that matches the ledge
    m = saddle + inner + plate + socket_body(4.5) + foot
    m = m - rr(-50, 50, -SH - 0.25, 0.25, -40, C_TOP + 0.25, 2.6)                      # clearance round the shell edge it rides on
    m = m - M.batch_hull([rr(-12.3, 12.3, -0.8, 4.75, 1.75, 4.25, 1.0), rr(-12.3, 12.3, -0.8, 1.05, 8.75, 10.25, 0.4)])   # and round the ledge
    return m - (ycyl(3.4, -1, 9, 0, 26) + hexnut_y(5.6, 2.6, 0, 2.2, 26))
def C_open(m):
    return m.translate([0, 0, -C_TOP]).rotate([22, 0, 0]).translate([0, 0, C_TOP])

OPTIONS = dict(A=(A_pad, A_mount, 12), B=(B_pad, B_mount, 18), C=(C_pad, C_mount, 26))
if __name__ == "__main__":
    for k, (fp, fm, sz) in OPTIONS.items():
        p, m = fp(), fm(); s = screw(sz)
        print(k, "pad", p.status(), "mount", m.status(), "cm3 %.1f" % (m.volume() / 1000), "mount∩pad %.1f" % (m ^ p).volume(), "screw∩pad %.1f" % (s ^ p).volume(), "pvc∩mount %.1f" % (pvc() ^ m).volume())
