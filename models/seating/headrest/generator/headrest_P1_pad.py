# GbG headrest: HEAD PAD shape study (review only; not in the assembly yet).
# Standard = "more support" (contoured posterior pad, suboccipital roll, ±50°).  Add-on = "most support" lateral wings (L/R).
# Pad frame: oval axis at x=y=0, +y = toward the back of the head, z=0 ~ the widest part of the skull.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_E3_geom as E
from headrest_E3_geom import sector, ycyl, zcyl, hexprism, rounded, place, ell_pose

R = 86.0; ELL = 1.28; T = 4.5; FOAM = 12.0
SPAN = 50.0                    # standard pad half-angle (circle space): edges come ~39 mm forward of the back of the head
Z0, Z1 = -48.0, 50.0
LIP = 14.0                     # suboccipital roll (shell), the foam follows it
SLOT_ANG = 44.0                # side T-slot: button mount OR lateral wing
LAT = (40.0, 82.0)             # lateral wing span (overlaps the pad edge 40..50)

def r_in(z):
    """head-side shell radius vs height: roll under the occiput, straight middle, top leans back"""
    if z < -36: return R - LIP
    if z < -10: t = (z + 36) / 26.0; return R - LIP * (1 - (3 * t * t - 2 * t * t * t))
    if z < 26: return R
    t = (z - 26) / (Z1 - 26); return R + 7.0 * t * t
ZS = np.linspace(Z0, Z1, 40)
def r_lat(z): return max(R, r_in(z))           # wings: no roll (it would press the neck / under the ear)
def _prof(off_in, off_out, z0=Z0, z1=Z1, solid_back=False, rf=None):
    rf = rf or r_in
    zs = [z for z in ZS if z0 <= z <= z1]
    inner = [(rf(z) + off_in, z) for z in zs]
    if solid_back:
        outer = [(max(R + T, rf(z) + off_out), z) for z in zs]
    else:
        outer = [(rf(z) + off_out, z) for z in zs]
    return inner + outer[::-1]
def _rev(poly, a0, a1, n=120):
    cs = CS([poly[::-1]]) if CS([poly]).area() < 0 else CS([poly])
    if cs.area() <= 0: cs = CS([poly[::-1]])
    return M.revolve(cs, n, a1 - a0).rotate([0, 0, 90 - a1])
def _edge_bead(a, rr=3.2, zlo=Z0 + 3, zhi=Z1 - 3):
    """rounded bead along a vertical edge at angle a (circle space)"""
    pts = []
    t = math.radians(a)
    for z in np.linspace(zlo, zhi, 16):
        r = r_in(z) + T / 2
        pts.append(M.sphere(rr, 20).translate([r * math.sin(t), r * math.cos(t), z]))
    return M.batch_boolean([M.batch_hull([pts[i], pts[i + 1]]) for i in range(len(pts) - 1)], 0)
def _top_bead(a0, a1, z, rr=3.2):
    r = r_in(z) + T / 2
    return M.revolve(CS.circle(rr, 24).translate([r, 0]), 120, a1 - a0).rotate([0, 0, 90 - a1]).translate([0, 0, z])

# ---------------- standard pad ----------------
FOAM_SLOTS = [(0, 36), (32, 36), (-32, 36), (36, -20), (-36, -20)]
def pad_local():
    sh = _rev(_prof(0, T, solid_back=True), -SPAN, SPAN)
    for s_ in (1, -1): sh = sh + _edge_bead(s_ * SPAN)
    sh = sh + _top_bead(-SPAN, SPAN, Z1 - 3.2) + _top_bead(-SPAN, SPAN, Z0 + 3.2)
    for a, z in FOAM_SLOTS:
        sh = sh - M.cube([18, 40, 3.5], True).translate([0, R + 5, z]).rotate([0, 0, -a])
    for s_ in (1, -1):                                       # forehead-strap slots near the top edge
        sh = sh - M.cube([4.5, 40, 26], True).translate([0, R + 3, 30]).rotate([0, 0, -s_ * 46])
    m = sh.scale([1, ELL, 1])
    for s_ in (1, -1):                                       # side T-slot pads (shared by button mount and lateral)
        m = m + place(E.mount_pad(), s_ * SLOT_ANG) - place(E.mount_pad_cuts(), s_ * SLOT_ANG)
    return m
def foam_local():
    f = _rev(_prof(-FOAM, 0.0, Z0 + 3, Z1 - 2), -SPAN + 1.5, SPAN - 1.5)
    return f.scale([1, ELL, 1])

# ---------------- lateral wing add-on ("most support") ----------------
EAR = (-30.0, 30.0)      # ear relief window in z (front of the wing): pinna roughly z -25..+30 here (estimate for 2-5 y)
def lateral_local(side=1):
    a0, a1 = LAT
    sh = _rev(_prof(T + 3.6, 2 * T + 3.6, -30, Z1 - 4, rf=r_lat), a0, SPAN + 2.0)   # clears the pad's edge bead           # lap: hugs the outside of the pad edge
    wing = M.batch_hull([_rev(_prof(T + 3.6, 2 * T + 3.6, -30, Z1 - 4, rf=r_lat), SPAN + 1.0, SPAN + 2.0, 8), _rev(_prof(0, T, -30, Z1 - 4, rf=r_lat), SPAN + 7.0, SPAN + 8.0, 8)]) + _rev(_prof(0, T, -30, Z1 - 4, rf=r_lat), SPAN + 7.0, a1)
    m = sh + wing + _edge_bead(a1, zlo=-27, zhi=Z1 - 7) + _top_bead(SPAN + 7, a1, Z1 - 7.2)
    # ear relief: scallop the front edge between EAR z
    t0 = a1 - 16
    m = m - sector(R - 30, R + 40, t0, a1 + 5, EAR[0] - 50, EAR[1], r=4.0)
    m = m + _edge_bead(t0, zlo=-27, zhi=EAR[1] + 2)
    m = m.scale([1, ELL, 1])
    # tongue into the pad's side slot + lock screw; its own slot further out for the button mount
    m = m + place(E.shoe_foot(), SLOT_ANG) - place(ycyl(3.4, 40, 0, R - 5, E.SHOE_Z[0] + 10), SLOT_ANG)
    m = m - place(M.cylinder(4, 3.6, 3.6, 24).rotate([-90, 0, 0]).translate([0, R + 2 * T + 0.3 - 1.0 + 4.5, E.SHOE_Z[0] + 10]), SLOT_ANG)   # counterbore
    m = m - (place(E.mount_pad(), SLOT_ANG) - place(E.shoe_slot(0.0), SLOT_ANG))
    m = m + place(E.mount_pad(), 70.0) - place(E.mount_pad_cuts(), 70.0)
    return m if side > 0 else m.mirror([1, 0, 0])
def lateral_foam_local(side=1):
    f = _rev(_prof(-FOAM, 0.0, -27, Z1 - 6, rf=r_lat), SPAN + 4.0, LAT[1] - 1.5)
    f = f - sector(R - 40, R + 40, LAT[1] - 15, LAT[1] + 5, EAR[0] - 50, EAR[1] + 2, r=4.0)
    f = f.scale([1, ELL, 1])
    return f if side > 0 else f.mirror([1, 0, 0])

# ---------------- head models (P95 5 y, P5 2 y) for fit checks ----------------
HEADS = dict(big=(147.0, 192.0, 15.0), small=(125.0, 163.0, 10.0))     # breadth, length, skull-centre z
def head(kind="big"):
    b, l, zc = HEADS[kind]
    sk = M.sphere(1, 64).scale([b / 2, l / 2, b / 2 * 1.05])
    # place: back of skull touches the foam at mid-height (foam face at r R-FOAM, oval -> y = ELL*(R-FOAM))
    yb = ELL * (R - FOAM) - 2.0                                                             # 2 mm foam compression
    sk = sk.translate([0, yb - l / 2, zc])
    neck = M.cylinder(90, b * 0.34, b * 0.36, 48).translate([0, yb - l / 2 + l * 0.18, zc - 125])
    face = M.sphere(1, 48).scale([b * 0.42, l * 0.35, b * 0.55]).translate([0, yb - l * 0.62, zc - 45])
    return sk + neck + face
def ear_points(kind="big"):
    """approx. tragion (ear canal) both sides: ~45 % of head length from the back, ~10 mm below skull centre (estimate)"""
    b, l, zc = HEADS[kind]; yb = ELL * (R - FOAM) - 2.0
    return [(s_ * b / 2, yb - 0.45 * l, zc - 10) for s_ in (1, -1)]

if __name__ == "__main__":
    p = pad_local(); f = foam_local(); L = lateral_local(1); Lf = lateral_foam_local(1)
    for nm, m in (("pad", p), ("foam", f), ("lateral R", L), ("lat foam", Lf)):
        b = m.bounding_box(); print(nm, m.status(), "cm3 %.1f" % (m.volume() / 1000), "size", [round(b[i + 3] - b[i]) for i in range(3)])
    print("lat∩pad", round((L ^ p).volume(), 1), "latfoam∩pad", round((Lf ^ p).volume(), 1), "latfoam∩foam", round((Lf ^ f).volume(), 1), "lat∩foam", round((L ^ f).volume(), 1))
    def gaps(m, k):
        b_, l_, zc = HEADS[k]; yb = ELL * (R - FOAM) - 2.0; c = np.array([0, yb - l_ / 2, zc]); ax = np.array([b_ / 2, l_ / 2, b_ / 2 * 1.05])
        v = np.asarray(m.to_mesh().vert_properties)[:, :3]; q = (v - c) / ax; rn = np.linalg.norm(q, axis=1)
        g = (rn - 1) * np.linalg.norm((v - c) / rn[:, None], axis=1)
        return round(float(g.min()), 1)
    for k in ("big", "small"):
        e = ear_points(k)[0]
        print(k, "gap head->lat foam min %.1f mm" % gaps(Lf, k), "gap head->pad foam min %.1f" % gaps(f, k), "ear point", [round(x) for x in e])
    for k in ("big", "small"):
        h = head(k)
        print(k, "head∩foam %.0f mm3" % (h ^ f).volume(), "head∩pad %.0f" % (h ^ p).volume(), "head∩latfoam %.0f" % (h ^ Lf).volume(), "head∩lat %.0f" % (h ^ L).volume())
