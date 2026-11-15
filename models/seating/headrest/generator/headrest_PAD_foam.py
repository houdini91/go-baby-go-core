# GbG headrest F3: the head-side PADDING. Material + how it is cut + how it attaches (clean look).
#   * shell change "V": the 5 through-slots for foam straps are replaced by blind hook-and-loop recesses on the inside -> the
#     back of the shell is clean (no slits), and ANY sheet foam 6-12 mm can be used (it no longer needs tabs).
#   * foam: one piece, cut flat from a 1:1 template (developed from the real 3D surface), bent / heat-formed into the shell.
#   * template: A4 landscape PDF (1:1) + an optional printed 2 mm stencil.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_F_flow as F, headrest_P2_pad as P, headrest_K_options as K
from headrest_E3_geom import place
from headrest_E_geom import ycyl

KP = "F3"; R, ELL, T = F.R, F.ELL, F.T
FOAM_T = 10.0                 # nominal sheet (POC: 10 mm EVA); 6-12 mm all fit (see gaps)
VEL_D = 1.5                   # recess depth for the self-adhesive HOOK patch (low-profile hook+loop ~2.3-3 mm mated)
VEL_GAP = 1.0                 # foam sits this far off the shell (mated hook+loop minus the recess)
# hook-and-loop patches: (angle deg, z, width along the surface, height), 25 mm tape
PATCHES = [(0, 33, 40, 22), (-40, 30, 25, 30), (40, 30, 25, 30), (-28, -26, 30, 20), (28, -26, 30, 20), (-55, 4, 14, 30), (55, 4, 14, 30)]
def U(a): return -math.radians(a) * R
# Mikey 2026-11-15: pre-cut Ø25 hook-and-loop DOTS (peel-and-stick pairs) instead of tape patches -> 8 round pockets Ø26
DOT_D, DOT_POCKET = 25.0, 26.0
DOTS = [(-16, 36), (16, 36), (-42, 32), (42, 32), (-26, 4), (26, 4), (-22, -28), (22, -28)]
ATTACH = "dots"
def _shapes(pocket=True, inset=0.0):
    if ATTACH == "dots":
        d = (DOT_POCKET if pocket else DOT_D) / 2 - inset
        cs = CS.circle(d, 48).translate([U(DOTS[0][0]), DOTS[0][1]])
        for a, z in DOTS[1:]: cs = cs + CS.circle(d, 48).translate([U(a), z])
        return cs
    cs = rrect(U(PATCHES[0][0]), PATCHES[0][1], PATCHES[0][2] - 2 * inset, PATCHES[0][3] - 2 * inset)
    for a, z, w, h in PATCHES[1:]: cs = cs + rrect(U(a), z, w - 2 * inset, h - 2 * inset)
    return cs
def rrect(cx, cz, w, h, r=5.0):
    return CS.square([w - 2 * r, h - 2 * r]).translate([cx - w / 2 + r, cz - h / 2 + r]).offset(r, JoinType.Round, circular_segments=24)
def inner_band(cs, w0, w1):
    """band on the HEAD side of the shell: w = 0 is the inner face, + into the shell"""
    pl = cs.extrude(w1 - w0).translate([0, 0, w0]).refine_to_length(1.5)
    return pl.warp_batch(lambda v: F._bend(KP, np.asarray(v), 0.0, thick=False))
def recesses():
    return inner_band(_shapes(True), -2.0, VEL_D)
def hooks():   # the hook dots sitting in their pockets, 1 mm proud (+ loop dot = foam gap)
    return inner_band(_shapes(False), -VEL_GAP, VEL_D - 0.05)

# ---------------- shell variants ----------------
_C = {}
def pad(var="V"):
    """var 'S' = today's F3 (5 foam-strap slots), 'V' = slots removed + hook recesses"""
    if var in _C: return _C[var]
    if var == "S": m = F.pad(KP)
    else: m = _pad_noslots() - recesses()
    _C[var] = m; return m
def _pad_noslots():
    k = KP
    pl = F._plate(F.outline(k), T, F.RR).refine_to_length(1.0)
    m = pl.warp_batch(lambda v: F._bend(k, np.asarray(v)))
    cuts = []
    for s_ in (1, -1):
        sl = M.batch_hull([M.cylinder(50, 2.25, 2.25, 24).rotate([-90, 0, 0]).translate([0, R - 15, z]) for z in (P.STRAP_Z[0] + 2.25, P.STRAP_Z[1] - 2.25)])
        cuts.append(place(sl, s_ * P.STRAP_ANG))
        cuts.append(place(P.btn_cuts().translate([0, F.BTN_SHIFT, 0]), s_ * P.BTN_ANG))
        cuts.append(place(ycyl(3.4, T + F.BTN_SHIFT + P.BTN_UP + 2, 0, R - 1, F.BTN_Z) + M.cylinder(1.8, 3.1, 1.5, 24).rotate([-90, 0, 0]).translate([0, R - 0.05, F.BTN_Z]), s_ * P.BTN_ANG))
    m = m - M.batch_boolean(cuts, 0)
    m = m - K._taper(K.channel()) - K.col_socket() - K.spine_boss(K.TCL, 30)
    for g in (1, -1): m = m - F.screw_cut_pad(g)
    return m

# ---------------- foam ----------------
ZCLIP = -47.0
def foam_cs():
    cs = F.outline(KP, 3.0) ^ CS.square([600, 200]).translate([-300, ZCLIP])
    for s_ in (1, -1):    # strap slot (22.5 x 4.5 in the shell) -> 26 x 7 in the foam, rounded
        a = s_ * P.STRAP_ANG; zc = sum(P.STRAP_Z) / 2; h = P.STRAP_Z[1] - P.STRAP_Z[0] + 3
        cs = cs - rrect(U(a), zc, 7.0, h, 3.4)
        cs = cs - CS.circle(4.5, 32).translate([U(s_ * P.BTN_ANG), F.BTN_Z])     # Ø9 screwdriver hole to the button-mount screw
    return cs
def foam(th=FOAM_T, gap=VEL_GAP):
    pl = F._plate(foam_cs(), th, min(3.0, th / 2 - 0.6)).refine_to_length(1.6)
    return pl.warp_batch(lambda v: F._bend(KP, np.asarray(v), -th - gap, thick=False))

# ---------------- developed (flat) template ----------------
def develop(u, z, depth=FOAM_T / 2 + VEL_GAP):
    """flat coords of plate point (u, z) on the foam's MIDDLE surface: arc length around the oval at height z, and arc length
       down the centre line (the roll makes the centre line longer than z)"""
    u = np.asarray(u, float); z = np.asarray(z, float)
    out = np.zeros((len(u), 2))
    for i, (uu, zz) in enumerate(zip(u, z)):
        a1 = -uu / R; n = max(8, int(abs(a1) / 0.01))
        a = np.linspace(0, a1, n); r = F.r_face(np.full(n, zz), np.degrees(a)) - depth
        x = r * np.sin(a); y = ELL * r * np.cos(a)
        s = np.sum(np.hypot(np.diff(x), np.diff(y)))
        zs = np.linspace(0, zz, max(8, int(abs(zz) / 0.5)))
        rc = F.r_face(zs, np.zeros_like(zs)) - depth
        sz = np.sum(np.hypot(np.diff(zs), ELL * np.diff(rc))) * np.sign(zz)
        out[i] = (-s * np.sign(a1) * -1 if a1 != 0 else 0.0, sz)
    out[:, 0] = np.where(u >= 0, np.abs(out[:, 0]), -np.abs(out[:, 0]))
    return out
def template_polys():
    polys = []
    for p in foam_cs().to_polygons():
        p = np.asarray(p)
        # densify
        q = []
        for a, b in zip(p, np.roll(p, -1, 0)):
            k = max(1, int(np.hypot(*(b - a)) / 1.5))
            q += [a + (b - a) * t for t in np.arange(k) / k]
        q = np.asarray(q); polys.append(develop(q[:, 0], q[:, 1]))
    pats = []
    for p in _shapes(False).to_polygons():
        p = np.asarray(p); pats.append(develop(p[:, 0], p[:, 1]))
    return polys, pats
def dot_centres(): return develop(np.array([U(a) for a, z in DOTS]), np.array([z for a, z in DOTS]))
def punch_centres():
    return develop(np.array([U(s_ * P.BTN_ANG) for s_ in (1, -1)]), np.array([F.BTN_Z, F.BTN_Z]))

if __name__ == "__main__":
    import headrest_P1_pad as P1, time
    t = time.time()
    s, v = pad("S"), pad("V")
    print("shell S %.0f cm3, V %.0f cm3, recess min shell left:" % (s.volume() / 1000, v.volume() / 1000))
    for th in (6.0, 8.0, 10.0, 12.0):
        f = foam(th)
        print("foam %2.0f mm" % th, "∩shell %.1f" % (f ^ v).volume(), "| big head gap min %.1f p10 %.1f | small head gap min %.1f p10 %.1f | head∩foam big %.0f small %.0f" % (
            P.gaps(f, "big").min(), np.percentile(P.gaps(f, "big"), 10), P.gaps(f, "small").min(), np.percentile(P.gaps(f, "small"), 10),
            (P1.head("big") ^ f).volume(), (P1.head("small") ^ f).volume()), flush=True)
    print("hooks∩shell %.1f, hooks∩foam %.1f" % ((hooks() ^ v).volume(), (hooks() ^ foam()).volume()))
    polys, pats = template_polys(); allp = np.concatenate(polys)
    print("template size %.0f x %.0f mm" % (np.ptp(allp[:, 0]), np.ptp(allp[:, 1])), "%.0fs" % (time.time() - t))
