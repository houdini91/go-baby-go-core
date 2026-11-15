# GbG headrest: FLOW options on top of K2 (tapered curved tenon, flush), sketches for review.
# Problem with K2: the back body is a separate revolved block -> hard step/line above the joint ("looks like two parts").
# Here the pad is ONE bent plate whose THICKNESS varies smoothly (a thickness field D(a, z)): it is thick (back face = the tenon's
# oval, flush) where the joint and the button pocket are, and fades to the 5 mm shell with long soft blends. No added blocks.
#   F1  "Soft spine"  : current outline, corners smoothed; thick only along the spine + a soft island round the button pocket.
#   F2  "Flow"        : new continuous outline (chin under the joint, one S-curve up to a softer temple wing) + the same spine/island.
#   F3  "Full back"   : F2 outline; thick across the whole back at button height (car-headrest look), long fade to the top.
# Lock = K2 + nuts: 2x M3x30 from UNDER the chin, angled 35 deg up into captive nuts slid into the tenon's (hidden) side edges.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_P2_pad as P
import headrest_K_options as K
import headrest_P3_button as B3
from headrest_E3_geom import place
from headrest_E_geom import ycyl
NUT_M3_AF = 5.8                              # FINAL 2026-11-15: printed M3 nut trap across flats (nut 5.5; was 5.6 = too tight in PETG)
BTN_Z = 18.0 + P.BTN_DZ                      # button-mount screw height (pad frame)

R, ELL, T, RR, FOAM = P.R, P.ELL, P.T, P.RR, P.FOAM
TO, BOT = K.TO, K.BOT
def sm(t): t = np.clip(t, 0.0, 1.0); return t * t * (3 - 2 * t)
def r_face(z, ad):
    ad = np.abs(ad)
    roll = P.LIP * (1 - sm((z + 36) / 26.0)) * (1 - sm((ad - 30) / 25.0))
    lean = 7.0 * sm((z - 26) / 24.0) * (1 - sm((ad - 40) / 30.0))
    return R - roll + lean

# ---------------- outlines (half, (deg, z)) -> Chaikin-smoothed closed curve ----------------
OUTLINES = dict(
    F1=[(0, -52.5), (44, -52.5), (52, -44), (57, -28), (58, -8), (59, 12), (63, 24), (73, 32), (76, 41), (71, 49), (54, 52), (0, 52.5)],
    F2=[(0, -52.5), (47, -52.5), (57, -40), (60, -16), (62, 8), (68, 26), (75, 38), (72, 50), (50, 55.5), (0, 55.5)])
OUTLINES["F3"] = OUTLINES["F2"]
def chaikin(pts, n=4):
    p = np.asarray(pts, float)
    for _ in range(n):
        q = np.roll(p, -1, axis=0)
        p = np.stack([0.75 * p + 0.25 * q, 0.25 * p + 0.75 * q], axis=1).reshape(-1, 2)
    return p
def outline(k, shrink=0.0):
    half = OUTLINES[k]
    full = [(-a, z) for a, z in half[::-1]] + [(a, z) for a, z in half[1:-1]]
    pts = chaikin([(math.radians(a) * R, z) for a, z in full], 4)
    cs = CS([pts.tolist()]); cs = cs if cs.area() > 0 else CS([pts[::-1].tolist()])
    return cs.offset(-shrink, JoinType.Round) if shrink else cs

# ---------------- thickness fields S(a, z) in [0, 1]  (1 = back face on the tenon oval TO) ----------------
def S_shield(a, z):
    A = 39.0 + 23.0 * sm((z + 40) / 14.0) * (1 - sm((z - 10) / 14.0))     # wide at button height, narrow at the chin
    return (1 - sm((np.abs(a) - A) / 14.0)) * (1 - sm((z - 8) / 38.0))        # fade starts above the button pocket rim
def S_sculpt(a, z):
    body = (1 - sm((np.abs(a) - 39.0) / 13.0)) * (1 - sm((z - 6) / 28.0))
    isl = (1 - sm((np.abs(np.abs(a) - 46.0) - 13.0) / 10.0)) * (1 - sm((np.abs(z - 1.0) - 18.0) / 12.0))
    return 1 - (1 - body) * (1 - isl)
FIELDS = dict(F1=S_sculpt, F2=S_sculpt, F3=S_shield)
STRAP_PLUS = 2.0                                  # F3 base: shell 5 -> 7 mm round each forehead-strap slot (outside only; foam side unchanged)
def S_strap(a, z):
    return (1 - sm((np.abs(np.abs(a) - P.STRAP_ANG) - 5.0) / 6.0)) * (1 - sm((np.abs(z - sum(P.STRAP_Z) / 2) - 14.0) / 8.0))
def D(k, z, ad):
    d = np.maximum(0.0, TO - r_face(z, ad) - T) * FIELDS[k](ad, z)
    if k == "F3": d = np.maximum(d, STRAP_PLUS * S_strap(ad, z))
    return d

def _bend(k, v, w0=0.0, thick=True):
    u, z, w = v[:, 0], v[:, 1], v[:, 2] + w0
    a = -u / R; ad = np.degrees(a)
    if thick: w = w * (T + D(k, z, ad)) / T
    r = r_face(z, ad) + w
    return np.stack([r * np.sin(a), ELL * r * np.cos(a), z], axis=1)
def _plate(cs, th, rr):
    return cs.offset(-rr, JoinType.Round).extrude(th - 2 * rr).translate([0, 0, rr]).minkowski_sum(M.sphere(rr, 16))

# ---------------- lock: K2 tenon (tapered, lips) + 2 angled screws from below into captive nuts ----------------
SA, SANG, SR0 = 24.0, 35.0, 82.0          # screw plane angle (deg, circle space), tilt from vertical, start radius at the chin
def _scr_local(t0, t1, d):
    """cylinder along the screw axis (local frame at angle 0: +y radial), from parameter t0 to t1 (mm along the axis)"""
    s = math.radians(SANG)
    c = M.cylinder(t1 - t0, d / 2, d / 2, 24).translate([0, 0, t0]).rotate([-SANG, 0, 0])   # tilt toward +y
    return c.translate([0, SR0, BOT - 0.5])
def _xf(m, sgn): return m.rotate([0, 0, -sgn * SA]).scale([1, ELL, 1])
T_NUT = (96.0 - SR0) / math.sin(math.radians(SANG))      # nut centre ~ mid-tenon
def screw_cut_pad(sgn):
    s = math.radians(SANG)
    cone = M.cylinder(1.8, 3.15, 1.5, 24).rotate([-SANG, 0, 0]).translate([0, SR0, BOT - 0.55])
    return _xf(_scr_local(-8, T_NUT + 6, 3.4) + cone, sgn)
def screw_cut_mast(sgn):
    s = math.radians(SANG)
    nut = CS.circle(NUT_M3_AF / math.sqrt(3), 6).extrude(2.7).translate([0, 0, -1.35])
    slot = M.batch_hull([nut, nut.translate([sgn * 30, 0, 0])])                           # slides in from the tenon side edge
    slot = slot.translate([0, 0, T_NUT]).rotate([-SANG, 0, 0]).translate([0, SR0, BOT - 0.5])
    return _xf(_scr_local(0, T_NUT + 8, 3.4) + slot.rotate([0, 0, 0]), sgn)
def screws():
    out = []
    for g in (1, -1):
        sh = _scr_local(0, 30, 3.0) + M.cylinder(1.7, 2.9, 1.5, 24).rotate([-SANG, 0, 0]).translate([0, SR0, BOT - 0.5])
        nut = CS.circle(5.5 / math.sqrt(3), 6).extrude(2.4).translate([0, 0, T_NUT - 1.2]).rotate([-SANG, 0, 0]).translate([0, SR0, BOT - 0.5])
        out.append(_xf(sh + nut, g))
    return M.batch_boolean(out, 0)

# ---------------- pad / foam / mast ----------------
BTN_SHIFT = 7.6                               # pocket sits in the thick back (found by clash test: mount seats on the curved surface), no boss
_C = {}
def pad(k):
    if k in _C: return _C[k]
    pl = _plate(outline(k), T, RR).refine_to_length(1.0)
    m = pl.warp_batch(lambda v: _bend(k, np.asarray(v)))
    cuts = []
    for a, z in ((0, 36), (30, 36), (-30, 36), (34, -18), (-34, -18)):
        cuts.append(place(M.cube([16, 60, 3.5], True).translate([0, R + 10, z]), a))
    for s_ in (1, -1):        # forehead strap (only if the child's therapist prescribes it): 22.5 x 4.5 through-slot
        sl = M.batch_hull([M.cylinder(50, 2.25, 2.25, 24).rotate([-90, 0, 0]).translate([0, R - 15, z]) for z in (P.STRAP_Z[0] + 2.25, P.STRAP_Z[1] - 2.25)])
        cuts.append(place(sl, s_ * P.STRAP_ANG))
    for s_ in (1, -1):        # button pocket B (no boss: the back is thick here)
        cuts.append(place(P.btn_cuts().translate([0, BTN_SHIFT, 0]), s_ * P.BTN_ANG))
        cuts.append(place(ycyl(3.4, T + BTN_SHIFT + P.BTN_UP + 2, 0, R - 1, BTN_Z) + M.cylinder(1.8, 3.1, 1.5, 24).rotate([-90, 0, 0]).translate([0, R - 0.05, BTN_Z]), s_ * P.BTN_ANG))
    m = m - M.batch_boolean(cuts, 0)
    m = m - K._taper(K.channel()) - K.col_socket() - K.spine_boss(K.TCL, 30)
    for g in (1, -1): m = m - screw_cut_pad(g)
    _C[k] = m; return m
def foam(k):
    key = "foam" + k
    if key in _C: return _C[key]
    cs = outline(k, 3.0) ^ CS.square([600, 200]).translate([-300, -47])
    pl = _plate(cs, FOAM, 3.0).refine_to_length(1.6)
    fm = pl.warp_batch(lambda v: _bend(k, np.asarray(v), -FOAM - 0.01, thick=False))
    fm = fm - M.batch_boolean([place(ycyl(9.0, 40, 0, R - 30, BTN_Z), g * P.BTN_ANG) for g in (1, -1)], 0)   # screwdriver hole to the button-mount screw
    _C[key] = fm; return fm
def mast():
    if "mast" in _C: return _C["mast"]
    m = K.column() + K._taper(K.tenon()) + K.spine_boss() - K.rod_holes()
    for g in (1, -1): m = m - screw_cut_mast(g)
    _C["mast"] = m; return m
def btn_mount(side): return place(P._to_pad(B3.B_mount().translate([0, P.BTN_UP + BTN_SHIFT, 0])).mirror([1, 0, 0]) if side > 0 else P._to_pad(B3.B_mount().translate([0, P.BTN_UP + BTN_SHIFT, 0])), side * P.BTN_ANG)
def btn_screw(side): return place(ycyl(3.0, 23, 0, R - 1.8, BTN_Z) + M.cylinder(1.7, 2.9, 1.5, 24).rotate([-90, 0, 0]).translate([0, R - 1.8, BTN_Z]), side * P.BTN_ANG)

if __name__ == "__main__":
    import sys, time, headrest_P1_pad as P1
    m = mast(); sc = screws(); rods = K.rods()
    print("mast", m.status(), "rods outside mast %.1f" % (rods - (m + K.rod_holes())).volume(), "screw∩mast(thread/nut) %.1f" % (sc ^ m).volume())
    for k in (sys.argv[1:] or ("F1", "F2", "F3")):
        t = time.time(); p = pad(k); f = foam(k); b = p.bounding_box()
        out = [k, str(p.status()), "pad %.0f cm3" % (p.volume() / 1000), "size %s" % [round(b[i + 3] - b[i]) for i in range(3)], "bottom %.1f" % b[2],
               "pad∩mast %.1f" % (p ^ m).volume(), "screws∩pad %.1f" % (sc ^ p).volume(), "foam∩pad %.1f" % (f ^ p).volume(), "foam∩mast %.1f" % (f ^ m).volume(),
               "slide " + " ".join("%g:%.1f" % (dz, (p.translate([0, 0, dz]) ^ m).volume()) for dz in (1, 10, 40, 70))]
        for sd in (1, -1):
            bm = btn_mount(sd); out.append("btn%+d ∩pad %.1f screw∩pad %.1f screw→nut %.1f" % (sd, (bm ^ p).volume(), (btn_screw(sd) ^ p).volume(), (btn_screw(sd) ^ bm).volume()))
        tool = M.batch_boolean([_xf(_scr_local(-60, 0, 7.0), g) for g in (1, -1)], 0)          # screwdriver path below each head
        out.append("driver path∩(pad+foam) %.1f" % (tool ^ (p + f)).volume())
        for h in ("big", "small"): out.append("%s head∩pad %.0f" % (h, (P1.head(h) ^ p).volume()))
        print(" | ".join(out), "%.0fs" % (time.time() - t), flush=True)
