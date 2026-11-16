# W1 pod v1.2 (2026-11-16): the reviewed W1 hull look (convex hull of the PVC collar + a footprint band on the pad), with
#  - footprint grown to carry 2 screw seats (pocket r21, exit r14, screw seats r8),
#  - 2 M3 countersunk from OUTSIDE on flat spot-faces, into hex nuts trapped in the pad's head side (pod -> pad, unchanged),
#  - v1.2: the M3x30 through-bolt PVC clamp (2 bolts, nut traps, lands) is GONE: 3 radial Ø4 holes (top / outward side /
#    60 deg below the side) for ST3.5 x 6.5 pan-head self-tappers that bite the PVC wall; Ø8 spot-faced seats, flared rims,
#  - v1.2: no sharp hull edges: collar Ø28 with both ends rounded r2, band top edge rounded r2 and run 1 mm below the pad
#    surface (so the trim cuts a smooth wall: no loft ripple / sawtooth at the underside edge),
#  - v1.2: PVC bore Ø20.8 slip fit (tolerance block), teardrop truncated (the old Ø20.3 teardrop tip broke through the collar
#    top as a 30 mm slit), 45 deg mouth chamfer,
#  - clean section data for Onshape (exact hull, arc-length resampled, consistent start point).
import math, os, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_F_flow as F, headrest_SW_mount as W, headrest_P2_pad as P, headrest_P3_button as B3, headrest_FS_export as X
import headrest_FITS as FIT
from headrest_E3_geom import place
R, ELL = 86.0, 1.28
SCREWS = [(-14.0, -9.0), (14.0, -12.0)]
SEAT_H = 4.5                    # (v1.1 seat height over rOut; replaced by seat_y(): see there)
POD_SCREW_L = 20.0              # pod -> pad: M3x20 DIN 7991 (v1.1 M3x16 caught only ~0.25 mm of the nut)
RC, LC, RF = 14.0, 30.0, 2.0    # collar radius (v1.1 13.5), length, end round-over
BAND_W0, BAND_W1, BAND_RF = -0.83, 3.0, 2.0  # footprint band from 0.83 mm under the pad surface (-1.0 gave a degenerate hull -> pinched STL) to 3 over it, top edge r2
BORE_DEPTH = 26.0               # blind PVC bore, 4 mm back wall
BORE_CAP = 0.6                  # teardrop truncated 0.6 mm over the bore radius (7.4 mm flat bridge)
BORE_CH = 0.8                   # 45 deg chamfer at the bore mouth (ends at the mouth plane: in the open trough in front of the
                                #   mouth a longer cone would break through the 1.8 mm floor)
# PVC lock: 3 radial wood/self-tapping screws, K_WS mm back from the collar mouth (half the bore depth), at angles in the
# plane normal to the bore: +90 = top, 0 = outward side (away from the pad), -60 = below the side (the collar is still a
# plain Ø28 cylinder there; straight down (-90) runs into the band, 18-19 mm of wall)
K_WS = 13.0
WS_ANG = (90.0, 1.0, -60.0)       # side hole 1 deg up from the outward normal: at exactly 0 a mesh-boolean coincidence pinched the yaw-28 STL
WS_SEAT = 13.3                  # flat seat (pan head) radius from the bore axis: Ø8 floor fully flat on the Ø28 collar
WS_CB = 4.0                     # seat radius (Ø8: Ø7 pan head + driver)
WS_CH = 0.5                     # 45 deg chamfer at the hole mouth in the seat floor
WS_FL = 2.75                    # 45 deg rim flare from WS_SEAT + 0.25 up to WS_SEAT + 3.0: round the Ø8 seat the hull stands
                                #   0.7-2.3 mm over the seat floor within r5.5 (the -60 deg hole at yaw 20/28 up to 2.0-2.3: the first 1.85 mm
                                #   flare left a 0.01-0.02 mm overhanging lip there)
def U(a): return -math.radians(a) * R
def rOut(z, ad): return F.r_face(np.array([z]), np.array([abs(ad)]))[0] + 5 + F.D("F3", np.array([z]), np.array([abs(ad)]))[0]
def footprint():
    c = [CS.circle(21.0, 96).translate([U(46), 1.0]), CS.circle(14.0, 96).translate([U(56), 8.0])]
    for x, z in SCREWS: c.append(CS.circle(8.0, 64).translate([U(46 + math.degrees(x / R)), z]))
    return CS.batch_hull(c)
def frame(yaw, side=1):
    """bore axis: mouth E, unit d (out of the mouth), outward side ne, top bv (right-handed, bv up)"""
    E, d = W.axis("W1", float(yaw), side)
    a_ = math.radians(side * 60); ne = np.array([math.sin(a_), math.cos(a_) / ELL, 0]); ne /= np.linalg.norm(ne)
    ne -= d * (ne @ d); ne /= np.linalg.norm(ne)
    bv = np.cross(d, ne); bv *= np.sign(bv[2])
    return E, d, ne, bv
def ws_dirs(yaw, side=1):
    E, d, ne, bv = frame(yaw, side)
    return [(E - d * K_WS, ne * math.cos(math.radians(a)) + bv * math.sin(math.radians(a))) for a in WS_ANG]
def _ring(p, d, r): return W.cyl_along(p - d * 0.005, d, 0.01, r, 256)
def collar(E, d):
    """Ø28 x 30 collar, both end edges rounded r2 (hull of rings)"""
    rings = []
    for th in np.linspace(0, math.pi / 2, 7):
        rr, off = RC - RF * (1 - math.cos(th)), RF * (1 - math.sin(th))
        rings += [_ring(E - d * off, d, rr), _ring(E - d * (LC - off), d, rr)]
    return M.batch_hull(rings)
def band():
    """footprint band on the pad, top edge rounded (hull of inset layers)"""
    fp = footprint(); layers = []
    for th in np.linspace(0, math.pi / 2, 6):
        inset, w = BAND_RF * (1 - math.cos(th)), BAND_W1 - BAND_RF * (1 - math.sin(th))
        layers.append(W.skin(fp.offset(-inset, JoinType.Round, 2.0, 96) if inset > 1e-6 else fp, BAND_W0, w, refine=0.8))
    return M.batch_hull(layers)
_HC = {}
def hull(yaw):
    if yaw not in _HC:
        E, d = W.axis("W1", float(yaw), 1)
        _HC[yaw] = (M.batch_hull([band(), collar(E, d)]), E, d)
    return _HC[yaw]
def prism_along(cs, p0, ax, u, L):
    """2D profile (x toward u, y = ax x u) extruded from p0 along unit ax for L"""
    ax = np.asarray(ax, float); ax /= np.linalg.norm(ax); u = np.asarray(u, float); u = u - ax * (u @ ax); u /= np.linalg.norm(u)
    v = np.cross(ax, u); Mx = np.column_stack([u, v, ax, np.asarray(p0, float)])
    return cs.extrude(L).transform(Mx.tolist())
def td_hole(p0, p1, r, up, cap=None, n=48):
    """= tdHole / tdHoleCap in the .fs: teardrop toward print-up `up` (45 deg roof), tip cut flat at `cap` from the axis;
       round if the hole is within ~17 deg of vertical in print"""
    p0, p1 = np.asarray(p0, float), np.asarray(p1, float); ax = (p1 - p0) / np.linalg.norm(p1 - p0); L = np.linalg.norm(p1 - p0)
    u = np.asarray(up, float) - ax * (np.asarray(up, float) @ ax)
    if np.linalg.norm(u) < 0.3: return W.cyl_along(p0, ax, L, r, n)
    c45 = r * math.sqrt(0.5)
    cs = CS.circle(r, n) + CS.hull(CS([[[c45, c45], [r * math.sqrt(2), 0], [c45, -c45], [0, 0]]]))
    if cap is not None and cap < r * math.sqrt(2): cs = cs ^ CS.square([cap + r, 4 * r]).translate([-r - 0.01, -2 * r])
    return prism_along(cs, p0, ax, u, L)
def cone_along(p0, ax, L, r0, r1, n=48):
    ax = np.asarray(ax, float); ax /= np.linalg.norm(ax)
    return M.cylinder(L, r0, r1, n).transform(np.column_stack([*_basis(ax), np.asarray(p0, float)]).tolist())
def _basis(ax):
    t = np.array([1.0, 0, 0]) if abs(ax[0]) < 0.9 else np.array([0, 1.0, 0]); u = np.cross(ax, t); u /= np.linalg.norm(u)
    return u, np.cross(ax, u), ax
POD_UP = np.array([1.0, 0, 0])  # = podUp(+1) in the .fs: the right pod prints lying on its pad side, +x up
def ws_cuts(yaw):
    """the 3 PVC-lock screw holes (right pod): Ø4 teardrop toward print-up, Ø8 flat seat at WS_SEAT, 45 deg flared rim,
       0.5 mm 45 deg chamfer at the hole mouth"""
    out = []
    for A, v in ws_dirs(yaw):
        out.append(td_hole(A + v * 5.0, A + v * (WS_SEAT + 1.0), FIT.WOOD_SCREW_HOLE / 2, POD_UP))
        out.append(cone_along(A + v * (WS_SEAT - WS_CH - 0.05), v, WS_CH + 0.1, FIT.WOOD_SCREW_HOLE / 2 - 0.05, FIT.WOOD_SCREW_HOLE / 2 + WS_CH + 0.05, 41))   # odd segment counts: no coincident vertices
        out.append(W.cyl_along(A + v * WS_SEAT, v, 0.3, WS_CB, 67))
        out.append(cone_along(A + v * (WS_SEAT + 0.25), v, WS_FL, WS_CB - 0.05, WS_CB - 0.05 + WS_FL, 71))   # 45 deg flared rim, up to WS_SEAT + 3.0
    return out
def bore_cuts(yaw):
    E, d = W.axis("W1", float(yaw), 1); r = FIT.PVC_BORE / 2
    return [td_hole(E - d * BORE_DEPTH, E + d * 30, r, POD_UP, cap=r + BORE_CAP, n=128),
            cone_along(E - d * (BORE_CH + 0.05), d, BORE_CH + 0.35, r - 0.05, r + BORE_CH + 0.3, 97)]   # 45 deg mouth chamfer (ends 0.3 past the mouth plane: no coplanar faces)
def surf_y(th, z, outer=True):
    """local y (along the screw axis = the oval normal at th, place() frame) where the pad's outer / head-side surface is"""
    import headrest_E3_geom as G3
    lo, hi = 70.0, 130.0
    for _ in range(50):
        y = (lo + hi) / 2; p, phi = G3.ell_pose(th, G3.R_IN); f = math.radians(phi)
        w = p + (y - G3.R_IN) * np.array([math.sin(f), math.cos(f), 0])
        ad = math.degrees(math.atan2(w[0], w[1] / ELL)); rc = math.hypot(w[0], w[1] / ELL)
        rs = F.r_face(np.array([z]), np.array([abs(ad)]))[0] + ((5 + F.D("F3", np.array([z]), np.array([abs(ad)]))[0]) if outer else 0)
        if rc > rs: hi = y
        else: lo = y
    return y
SEAT_MIN_PLASTIC = 3.5          # plastic between the pod underside (pad + 0.3) and the pocket mouth, along the screw axis
# v1.2b SCREWS FLIPPED: M3x20 from the pad's head side (head top POD_HEAD_D under the head-side face R), hex nut on a pocket
# floor at POD_NUT_Y in the pod, pocket open to the outside through the round mouth at seat_y() (= the .fs)
POD_HEAD_D, POD_NUT_Y = [1.0, 0.2], [103.9, 103.1]
def seat_y():
    """local y where the nut pocket's round Ø7.2 mouth starts (kept from the v1.2 seat: the hull, rim flare and lip checks are
       unchanged).  v1.2 history: the seat floor for a screw from OUTSIDE, set so that its M3x20 tip ended 0.2 mm short of the pad's head-side face, i.e. 0.2 mm PAST the nut
       (the nut sits 0.4-2.8 mm into its trap), so the screw passes through the whole nut and nothing reaches the foam.
       Plastic under each seat (pad surface measured ALONG the screw axis) must stay >= SEAT_MIN_PLASTIC: 3.5-4.0 mm.
       (The first v1.2 rule kept >= 4.0 mm and left screw 1 with its tip 0.29 mm short of the nut: 2.11 of 2.4 engaged.)
       -> the .fs POD_SEAT_Y"""
    out = []
    for x, z in SCREWS:
        th = 46 + math.degrees(x / R)
        y = round(surf_y(th, z, False) + 0.2 + POD_SCREW_L, 2)
        assert y - (surf_y(th, z) + 0.3) >= SEAT_MIN_PLASTIC - 1e-6, (x, z, y)
        out.append(y)
    return out
SEAT_CB, SEAT_FL = 3.6, 1.5     # pod -> pad M3 seats: counterbore r3.6 over the countersink, 45 deg flare 1.5 at the rim
_SR = {}
def seat_rim(yaw):
    """per pod screw: lowest hull height over the seat floor round the r3.6 counterbore (the downhill side of the flank);
       the 45 deg rim flare starts 0.3 below it, so the downhill rim is chamfered instead of a ~40 deg knife edge"""
    if yaw in _SR: return _SR[yaw]
    h, E, d = hull(yaw); out = []
    for (x, z), yf in zip(SCREWS, seat_y()):
        th = 46 + math.degrees(x / R); hs = []
        for a in np.linspace(0, 2 * math.pi, 36, endpoint=False):
            lo, hi = 0.0, 20.0
            for _ in range(16):
                mid = (lo + hi) / 2
                p = place(M.cube([0.02] * 3, True).translate([(SEAT_CB + 0.05) * math.cos(a), yf + mid, z + (SEAT_CB + 0.05) * math.sin(a)]), th)
                if (p ^ h).volume() > 0: lo = mid
                else: hi = mid
            hs.append(lo)
        out.append(round(max(min(hs) - 0.3, 0.5), 2))
    _SR[yaw] = out; return out
def seat_cuts(yaw):
    out = []
    for (x, z), hr, yf in zip(SCREWS, seat_rim(yaw), seat_y()):
        th = 46 + math.degrees(x / R)
        def ya(L, r0, r1, y0, n=48): return place(M.cylinder(L, r0, r1, n).rotate([-90, 0, 0]).translate([0, y0, z]), th)
        hexp = CS.circle(FIT.NUT_M3_AF / math.sqrt(3), 6)                     # one corner along +x of the local frame
        yn = POD_NUT_Y[SCREWS.index((x, z))]
        # corner toward print-up (+x world for the right pod): local x of place() at th is ~ the print-up direction projected
        nut = place(hexp.extrude(yf + 0.1 - yn).rotate([-90, 0, 0]).translate([0, yn, z]), th)
        out += [ya(40, FIT.M3_CLEAR / 2, FIT.M3_CLEAR / 2, R - 2, 24), nut,
                ya(hr + 0.05, SEAT_CB, SEAT_CB, yf),
                ya(SEAT_FL, SEAT_CB - 0.02, SEAT_CB + SEAT_FL, yf + hr),
                ya(30, SEAT_CB + SEAT_FL - 0.03, SEAT_CB + SEAT_FL - 0.03, yf + hr + SEAT_FL - 0.05)]
    return out
CSK_M3_R = 3.25
LIFT_STEPS, LIFT_STEP = 4, 0.26
_LC = []
def lift_cuts():
    """v1.2: outside the footprint the underside is lifted to 1.3 mm over the pad in 4 x 0.25 mm steps (= a 45 deg chamfer):
       where the collar / hull flank came down to the pad at a grazing angle the v1.1 trim left a feather lip and a thin rear
       tail; now every free edge is >= 1 mm thick.  Inside the footprint the pod still sits on the pad + 0.3.  (= the .fs)"""
    if not _LC:
        fp = footprint(); bb = fp.bounds(); big = CS.square([bb[2] - bb[0] + 90, bb[3] - bb[1] + 90]).translate([bb[0] - 45, bb[1] - 45])
        # (step 1 bites 0.02 into the band's skirt: coincident walls would leave duplicate vertices)
        _LC.append(M.batch_boolean([W.skin(big - fp.offset(LIFT_STEP * (j - 1) if j > 1 else -0.02, JoinType.Round, 2.0, 96),
                                           -3.0, 0.3 + LIFT_STEP * j, refine=1.0) for j in range(1, LIFT_STEPS + 1)], 0))
    return _LC[0]
def pod(yaw, side=1):
    h, E, d = hull(yaw)
    # trim skin over the pod's side of the pad only, refined to 0.7 mm: at 2 mm its chords sagged ~0.4 mm in the concave
    # strap-slot thickening fade (z 13-20) and the pod met the pad there (2.49 mm3 "overlap" = mesh error, not the fit)
    m = h - W.skin(CS.square([150, 130]).translate([U(91), -65]), -40, 0.3, refine=0.7)
    m = m - lift_cuts()
    m = m + W._wedge()
    cuts = bore_cuts(yaw) + ws_cuts(yaw) + seat_cuts(yaw)
    m = m - M.batch_boolean(cuts, 0)
    ps = sorted(m.decompose(), key=lambda p: -p.volume())        # drop mesh-boolean slivers (< 0.1 mm3, checked)
    assert all(abs(p.volume()) < 0.1 for p in ps[1:]), [p.volume() for p in ps[1:]]
    m = ps[0]
    return m if side > 0 else m.mirror([1, 0, 0])
TURN_W = 0.5
def sections(yaw, step=2.5, npts=64):
    """planar sections of the exact hull perpendicular to d, arc-length resampled from the point furthest along +n"""
    h, E, d = hull(yaw); Rm = X.rot_to_z(d)
    v = np.asarray(h.to_mesh().vert_properties)[:, :3]
    hz = M.hull_points(((v - E) @ Rm.T).tolist()); bb = hz.bounding_box()
    a = math.radians(60); ne = np.array([math.sin(a), math.cos(a) / ELL, 0]); ne = (Rm @ ne)[:2]; ne /= np.linalg.norm(ne)
    zs = np.append(np.arange(bb[2] + 1.0, bb[5] - 0.6, step), bb[5] - 0.6)
    extra = np.array([-LC - 0.6, -LC + 0.6, -LC + 1.2, -LC + 1.8, -1.8, -1.2, -0.6, 0.6])       # v1.2: collar end round-overs
    zs = np.sort(np.concatenate([[z for z in zs if np.min(np.abs(extra - z)) > 0.45], extra[(extra > bb[2] + 1.0) & (extra < bb[5] - 0.6)]]))
    out = []
    for z in zs:
        p = np.asarray(hz.slice(z).to_polygons()[0])
        if np.sum(p[:, 0] * np.roll(p[:, 1], -1) - np.roll(p[:, 0], -1) * p[:, 1]) < 0: p = p[::-1]   # CCW (shoelace)
        i0 = int(np.argmax(p @ ne)); p = np.roll(p, -i0, 0)                          # start at the top (furthest out)
        q = np.vstack([p, p[:1]]); L = np.r_[0, np.cumsum(np.linalg.norm(np.diff(q, axis=0), axis=1))]
        # v1.2: half arc length, half turning angle -> points crowd into the tight r2 round-overs (a plain arc-length
        # resampling left the spline 0.6-0.7 mm off the hull there, which showed as ripples / lines along the loft)
        e = np.diff(q, axis=0); ang = np.unwrap(np.arctan2(e[:, 1], e[:, 0])); turn = np.abs(np.diff(np.r_[ang, ang[0] + 2 * math.pi]))
        Th = np.r_[0, np.cumsum(np.r_[0.5 * (turn[-1] + turn[0]), 0.5 * (turn[:-1] + turn[1:])][:len(e)])]
        Th = Th / Th[-1]; t = (1 - TURN_W) * L / L[-1] + TURN_W * Th
        s = np.linspace(0, 1, npts, endpoint=False)
        r = np.column_stack([np.interp(s, t, q[:, 0]), np.interp(s, t, q[:, 1])])
        out.append(np.column_stack([r, np.full(npts, z)]) @ Rm + E)
    return out
def write_fsdata(path=None):
    """POD_SECTIONS + POD_AXIS + POD_SEAT_RIM + POD_SEAT_Y block for featurescript/gbg_headrest_v10.fs (replaces the block at
       the end of the file: see splice_fs())"""
    lines, axes, rims = [], [], []
    for yaw in (12, 20, 28):
        secs = sections(float(yaw)); h, E, d = hull(float(yaw))
        lines.append('    "%d" : [\n        ' % yaw + ",\n        ".join("[" + ",".join("%.2f,%.2f,%.2f" % tuple(p) for p in s) + "]" for s in secs) + "]")
        axes.append('    "%d" : [[%.4f, %.4f, %.4f], [%.6f, %.6f, %.6f]]' % (yaw, *E, *d))
        rims.append('"%d" : [%s]' % (yaw, ", ".join("%.2f" % v for v in seat_rim(float(yaw)))))
    txt = ("const POD_SECTIONS = {\n" + ",\n".join(lines) + "\n};\nconst POD_AXIS = {\n" + ",\n".join(axes) + "\n};\n"
           "// pod -> pad seats (generated, headrest_POD11.py): rim flare start over each seat floor (= lowest flank height there - 0.3)\n"
           "const POD_SEAT_RIM = { " + ", ".join(rims) + " };\n"
           "// seat floor, local y along the screw axis (M3x20 tip 0.2 past the nut, 0.2 short of the head-side face; >= 3.5 mm plastic under the seat)\n"
           "const POD_SEAT_Y = [%s];\n" % ", ".join("%.2f" % v for v in seat_y()))
    path = path or os.path.join(os.path.dirname(os.path.abspath(__file__)), "pod_sections.fsdata")
    open(path, "w").write(txt); return txt
def splice_fs(fs=None):
    """regenerate the generated block at the end of featurescript/gbg_headrest_v10.fs (from 'const POD_SECTIONS' to the end)"""
    fs = fs or os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "featurescript", "gbg_headrest_v10.fs")
    s = open(fs).read(); i = s.index("const POD_SECTIONS = {")
    open(fs, "w").write(s[:i] + write_fsdata(os.devnull))
if __name__ == "__main__":
    import sys, headrest_FINAL as FN
    yaw = float(sys.argv[1]) if len(sys.argv) > 1 else 20.0
    m = pod(yaw); pad = FN.pad()
    print("pod %.1f cm3" % (m.volume() / 1000), m.status(), "parts", len(m.decompose()), "genus", m.genus(), "∩pad %.2f" % (m ^ pad).volume())
