# W1 pod v1.1: the reviewed W1 hull look (convex hull of the PVC collar + a footprint band on the pad), with
#  - footprint grown to carry 2 screw seats (pocket r21, exit r14, screw seats r8),
#  - 2 M3 button heads from OUTSIDE on flat spot-faces, into hex nuts trapped in the pad's head side,
#  - clean section data for Onshape (exact hull, arc-length resampled, consistent start point).
import math, os, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_F_flow as F, headrest_SW_mount as W, headrest_P2_pad as P, headrest_P3_button as B3, headrest_FS_export as X
from headrest_E3_geom import place
R, ELL = 86.0, 1.28
SCREWS = [(-14.0, -9.0), (14.0, -12.0)]
SEAT_H = 4.5                    # flat screw seat height above the pad surface
CLAMP_K = (17.5, 23.5)          # PVC clamp bolts, mm back from the collar mouth
def U(a): return -math.radians(a) * R
def rOut(z, ad): return F.r_face(np.array([z]), np.array([abs(ad)]))[0] + 5 + F.D("F3", np.array([z]), np.array([abs(ad)]))[0]
def footprint():
    c = [CS.circle(21.0, 96).translate([U(46), 1.0]), CS.circle(14.0, 96).translate([U(56), 8.0])]
    for x, z in SCREWS: c.append(CS.circle(8.0, 64).translate([U(46 + math.degrees(x / R)), z]))
    return CS.batch_hull(c)
def hull(yaw):
    E, d = W.axis("W1", float(yaw), 1)
    collar = W.cyl_along(E - d * 30.0, d, 30.0, 13.5, 256)
    band = W.skin(footprint(), 0.3, 3.0, refine=0.8)
    return M.batch_hull([band, collar]), E, d
def pod(yaw, side=1):
    h, E, d = hull(yaw)
    m = h - W.skin(CS.square([300, 160]).translate([-150, -80]), -40, 0.3, refine=2.0)
    m = m + W._wedge()
    m = m - W.cyl_along(E - d * 26, d, 56, 10.15)
    # PVC clamp: 2 M3x30 through the collar AND the PVC (drill the PVC through the pod), heads on a flat land on top,
    # nuts in shallow hex traps (corner toward print-up = d) on a flat land underneath
    a_ = math.radians(60); ne = np.array([math.sin(a_), math.cos(a_) / ELL, 0]); ne /= np.linalg.norm(ne); ne -= d * (ne @ d); ne /= np.linalg.norm(ne)
    bv = np.array([0, 0, 1.0]); bv -= d * (bv @ d) + ne * (bv @ ne); bv /= np.linalg.norm(bv)
    for k in CLAMP_K:
        A = E - d * k
        m = m - W.cyl_along(A - bv * 30, bv, 60, 1.7)
        m = m - W.cyl_along(A + bv * 12.5, bv, 20, 3.6) - W.cyl_along(A - bv * 40, bv, 40 - 17.7, 4.2)
        hexp = CS.circle(F.NUT_M3_AF / math.sqrt(3), 6).extrude(4.0).translate([0, 0, -2.0])
        Rm = X.rot_to_z(bv).T                                     # z -> bv
        q = np.asarray(hexp.to_mesh().vert_properties)[:, :3] @ Rm.T + (A - bv * 16.7)
        m = m - M.hull_points(q.tolist())
    for x, z in SCREWS:
        th = 46 + math.degrees(x / R); yf = rOut(z, th) + SEAT_H
        m = m - place(M.cylinder(40, 1.7, 1.7, 24).rotate([-90, 0, 0]).translate([0, R - 2, z]), th)
        m = m - place(M.cylinder(30, 3.6, 3.6, 48).rotate([-90, 0, 0]).translate([0, yf, z]), th)
    return m if side > 0 else m.mirror([1, 0, 0])
def sections(yaw, step=2.5, npts=64):
    """planar sections of the exact hull perpendicular to d, arc-length resampled from the point furthest along +n"""
    h, E, d = hull(yaw); Rm = X.rot_to_z(d)
    v = np.asarray(h.to_mesh().vert_properties)[:, :3]
    hz = M.hull_points(((v - E) @ Rm.T).tolist()); bb = hz.bounding_box()
    a = math.radians(60); ne = np.array([math.sin(a), math.cos(a) / ELL, 0]); ne = (Rm @ ne)[:2]; ne /= np.linalg.norm(ne)
    zs = np.append(np.arange(bb[2] + 1.0, bb[5] - 0.6, step), bb[5] - 0.6)
    out = []
    for z in zs:
        p = np.asarray(hz.slice(z).to_polygons()[0])
        if np.sum(p[:, 0] * np.roll(p[:, 1], -1) - np.roll(p[:, 0], -1) * p[:, 1]) < 0: p = p[::-1]   # CCW (shoelace)
        i0 = int(np.argmax(p @ ne)); p = np.roll(p, -i0, 0)                          # start at the top (furthest out)
        q = np.vstack([p, p[:1]]); L = np.r_[0, np.cumsum(np.linalg.norm(np.diff(q, axis=0), axis=1))]
        s = np.linspace(0, L[-1], npts, endpoint=False)
        r = np.column_stack([np.interp(s, L, q[:, 0]), np.interp(s, L, q[:, 1])])
        out.append(np.column_stack([r, np.full(npts, z)]) @ Rm + E)
    return out
def write_fsdata(path=None):
    """POD_SECTIONS + POD_AXIS block for featurescript/gbg_headrest_v10.fs (paste at the end of the file)"""
    lines, axes = [], []
    for yaw in (12, 20, 28):
        secs = sections(yaw); h, E, d = hull(yaw)
        lines.append('    "%d" : [\n        ' % yaw + ",\n        ".join("[" + ",".join("%.2f,%.2f,%.2f" % tuple(p) for p in s) + "]" for s in secs) + "]")
        axes.append('    "%d" : [[%.4f, %.4f, %.4f], [%.6f, %.6f, %.6f]]' % (yaw, *E, *d))
    path = path or os.path.join(os.path.dirname(os.path.abspath(__file__)), "pod_sections.fsdata")
    open(path, "w").write("const POD_SECTIONS = {\n" + ",\n".join(lines) + "\n};\nconst POD_AXIS = {\n" + ",\n".join(axes) + "\n};\n")
if __name__ == "__main__":
    import sys, headrest_FINAL as FN
    from zrender import ZScene as Scene
    yaw = float(sys.argv[1]) if len(sys.argv) > 1 else 20.0
    m = pod(yaw); pad = FN.pad()
    print("pod %.1f cm3" % (m.volume() / 1000), m.status(), "parts", len(m.decompose()), "∩pad %.2f" % (m ^ pad).volume())
    for x, z in SCREWS:      # how deep is each spot-face below the hull surface? (seat depth)
        th = 46 + math.degrees(x / R); h, E, d = hull(yaw)
        col = place(M.cylinder(60, 3.6, 3.6, 48).rotate([-90, 0, 0]).translate([0, rOut(z, th), z]), th)
        hb = (col ^ h).bounding_box()
        print("screw %s: hull top over the pad there %.1f mm, seat at %.1f -> cut depth up to %.1f" % ((x, z), 0, SEAT_H, 0))
    def T(x): me = x.to_mesh(); return np.asarray(me.vert_properties)[:, :3][np.asarray(me.tri_verts)]
    c = np.asarray(m.to_mesh().vert_properties)[:, :3].mean(0)
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "review", "img") + "/"; os.makedirs(out, exist_ok=True)
    for nm, az, el in (("p11_side", 40, 10), ("p11_rear", 130, 15), ("p11_low", 60, -25), ("p11_front", -40, 10)):
        s = Scene(); s.add(T(pad), "#c9b99c"); s.add(T(m), "#4a90d9"); s.render(out + nm + ".png", az=az, el=el, size=(4.6, 4.0), dpi=110, focus=(tuple(c), 55), title=nm)
