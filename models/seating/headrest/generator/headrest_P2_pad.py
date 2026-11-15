# GbG headrest: HEAD PAD P2 (review only) - ONE flowing piece, moderate wings built in ("80/20": more support + side hold).
# Built as a flat rounded plate (minkowski with a sphere = round edges everywhere) bent onto the head oval with warp.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_E3_geom as E
from headrest_E3_geom import ycyl, place
import headrest_P1_pad as P1

R = 86.0; ELL = 1.28; T = 5.0; FOAM = 12.0; RR = 2.2          # shell, foam, edge round radius
Z0, Z1 = -48.0, 50.0
LIP = 14.0
STRAP_ANG, STRAP_Z = 57.0, (23.0, 45.5)                     # 22.5 x 4.5 slot for a 20 mm strap
SLOT_ANG = 48.0                                               # (old) button-mount T-slot angle
BUTTON = "B"                                                  # Mikey 2026-11-14: option B, triangle pocket (push in + click + 1 M3)
BTN_ANG, BTN_DZ, BTN_UP = 46.0, -20.0, 3.0                    # pocket centre angle; z shift vs the P3 sketch (triangle z -14..16, below the strap slot); boss stands 3 mm OUT (nothing bumps the foam)
def arc(a): return math.radians(a) * R                        # half-angle (deg) -> arc length at R

# outline: (half-angle deg, z). Narrow at the neck, behind-ear band, temple wings above the ear, rounded top.
OUTLINE = [(36, Z0), (48, -40), (56, -26), (58, -6), (58, 14), (62, 24), (72, 32), (74, 40), (70, 47), (56, Z1), (0, Z1 + 1)]
def outline(shrink=0.0):
    half = [(arc(a), z) for a, z in OUTLINE]                  # bottom centre-right ... up to the top centre
    pts = [(-x, z) for x, z in half[::-1]] + half[:-1]          # top centre -> down the left -> bottom -> up the right
    cs = CS([pts])
    if cs.area() < 0: cs = CS([pts[::-1]])
    cs = cs.offset(-9, JoinType.Round).offset(9, JoinType.Round).offset(7, JoinType.Round).offset(-7, JoinType.Round)   # round every corner
    return cs.offset(-shrink, JoinType.Round) if shrink else cs

def smooth(t): t = min(max(t, 0.0), 1.0); return t * t * (3 - 2 * t)
def r_face(z, a_deg):
    """head-side radius (circle space) at height z and angle a: roll under the occiput fading out on the wings, top leans back"""
    roll = LIP * (1 - smooth((z + 36) / 26.0)) * (1 - smooth((abs(a_deg) - 30) / 25.0))
    lean = 7.0 * smooth((z - 26) / (Z1 - 26)) ** 1.0 * (1 - smooth((abs(a_deg) - 40) / 30.0))
    return R - roll + lean
def _bend(v, w0):
    u, z, w = v[:, 0], v[:, 1], v[:, 2] + w0
    a = -u / R; ad = np.degrees(a)          # minus: keeps the mesh outward-facing after the bend
    r = np.array([r_face(zz, aa) for zz, aa in zip(z, ad)]) + w
    return np.stack([r * np.sin(a), ELL * r * np.cos(a), z], axis=1)
def _plate(cs, thick, rr):
    core = cs.offset(-rr, JoinType.Round).extrude(thick - 2 * rr).translate([0, 0, rr])
    return core.minkowski_sum(M.sphere(rr, 16))
_C = {}
def pad_local():
    if "pad" in _C: return _C["pad"]
    pl = _plate(outline(), T, RR).refine_to_length(1.6)
    m = pl.warp_batch(lambda v: _bend(np.asarray(v), 0.0))
    # solid back behind the roll, centre only (later: the tenon channel goes here)
    # foam fixings + forehead-strap slots (oval space)
    cuts = []
    for a, z in ((0, 36), (30, 36), (-30, 36), (34, -18), (-34, -18), (52, -20), (-52, -20)):
        cuts.append(place(M.cube([16, 40, 3.5], True).translate([0, R + 3, z]), a))
    for s_ in (1, -1):        # forehead strap (only if the child's therapist prescribes it): 20 mm strap, vertical slot in each temple wing
        cuts.append(place(M.batch_hull([M.cylinder(4.5, 2.25, 2.25, 24).rotate([-90, 0, 0]).translate([0, R - 2, z]) for z in (STRAP_Z[0] + 2.25, STRAP_Z[1] - 2.25)]).scale([1, 4.0, 1]), s_ * STRAP_ANG))
    m = m - M.batch_boolean(cuts, 0)
    for s_ in (1, -1):
        if BUTTON == "B":
            m = m + place(btn_boss(), s_ * BTN_ANG) - place(btn_cuts(), s_ * BTN_ANG)
        else:
            m = m + place(E.mount_pad(), s_ * SLOT_ANG) - place(E.mount_pad_cuts(), s_ * SLOT_ANG)
    _C["pad"] = m; return m

# ---- button mount B on the pad (P3 local frame: outer shell face y=0, +y out, rod along -x) ----
import headrest_P3_button as B3
def _to_pad(m): return m.translate([0, R + T, BTN_DZ])          # P3 local -> place() local (r_ref = R)
def btn_boss():
    return _to_pad(B3.B_tri_solid(4.0, -2.5, BTN_UP))
def btn_cuts():
    c = B3.B_tri_solid(0.15, BTN_UP - B3.B_D, BTN_UP + 1)
    for k in (1, -1): c = c + M.sphere(1.3, 16).translate([k * 9.3, BTN_UP - B3.B_D / 2, 24])
    return _to_pad(c + B3.screw_hole(18).translate([0, 0, 0]) + B3.ycyl(3.4, -T - 1, BTN_UP, 0, 18))
def btn_mount(side=1):
    """the PVC socket mount, rod pointing FORWARD on either side"""
    m = _to_pad(B3.B_mount().translate([0, BTN_UP, 0]))
    m = m if side < 0 else m.mirror([1, 0, 0])
    return place(m, side * BTN_ANG)
def btn_pvc(side=1):
    m = _to_pad(B3.pvc().translate([0, BTN_UP, 0])); m = m if side < 0 else m.mirror([1, 0, 0]); return place(m, side * BTN_ANG)
def btn_screw(side=1):
    return place(_to_pad(B3.screw(18)), side * BTN_ANG)
def foam_local():
    if "foam" in _C: return _C["foam"]
    pl = _plate(outline(3.0), FOAM, 3.0).refine_to_length(1.6)
    _C["foam"] = pl.warp_batch(lambda v: _bend(np.asarray(v), -FOAM - 0.01)); return _C["foam"]

head, ear_points, HEADS = P1.head, P1.ear_points, P1.HEADS
def gaps(m, k):
    b_, l_, zc = HEADS[k]; yb = ELL * (R - FOAM) - 2.0; c = np.array([0, yb - l_ / 2, zc]); ax = np.array([b_ / 2, l_ / 2, b_ / 2 * 1.05])
    v = np.asarray(m.to_mesh().vert_properties)[:, :3]; q = (v - c) / ax; rn = np.linalg.norm(q, axis=1)
    g = (rn - 1) * np.linalg.norm((v - c) / rn[:, None], axis=1)
    return g
if __name__ == "__main__":
    import time; t = time.time()
    p = pad_local(); f = foam_local()
    for nm, m in (("pad", p), ("foam", f)):
        b = m.bounding_box(); print(nm, m.status(), "cm3 %.1f" % (m.volume() / 1000), "size", [round(b[i + 3] - b[i]) for i in range(3)], "tri", m.num_tri())
    print("pad∩foam %.1f" % (p ^ f).volume(), "%.1fs" % (time.time() - t))
    for sd in (1, -1):
        mt = btn_mount(sd); bb = btn_pvc(sd).bounding_box()
        print("btn side", sd, "mount∩pad %.1f" % (mt ^ p).volume(), "screw∩pad %.1f" % (btn_screw(sd) ^ p).volume(), "mount∩foam %.1f" % (mt ^ f).volume(), "pvc y %.0f..%.0f" % (bb[1], bb[4]), "mount cm3 %.1f" % (mt.volume() / 1000))
    for k in ("big", "small"):
        g = gaps(f, k); h = head(k)
        print(k, "head∩pad %.0f" % (h ^ p).volume(), "foam gap: min %.1f, 10th pct %.1f" % (g.min(), np.percentile(g, 10)), "ears", [[round(x) for x in e] for e in ear_points(k)][:1])
