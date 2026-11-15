# GbG headrest F3: INTEGRATED push-button (head switch) mount. Sketches for review.
# The mount anchors in the existing triangle pocket (B: shape carries the load, 1x M3 from the head side through the foam hole),
# its back hugs the pad's outer surface, and it flows forward into a slim collar that holds the 20 mm PVC at one of 3 yaw angles.
#   W1 "Pod"  : pad unchanged; the pod alone carries the rod 12 mm off the surface.
#   W2 "Lane" : a soft half-pipe groove in the pad side (styling line); the rod sits half-sunk in it, the pod is lower.
#   BLANK     : flush plug for the pocket when no button is fitted.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_F_flow as F, headrest_P2_pad as P, headrest_P3_button as B3, headrest_SW_angles as SA
from headrest_E3_geom import place
from headrest_E_geom import ycyl

K = "F3"; R, ELL, T = F.R, F.ELL, F.T
YAWS = (12.0, 20.0, 28.0)          # toward the OUTSIDE from straight forward (see the angle study)
PITCH = -3.0
EXIT_A, EXIT_Z = 60.0, 8.0         # where the rod leaves the pad: front of the side, under the temple wing
OFF = dict(W1=12.5, W2=7.0)        # rod axis distance from the pad's outer surface at the exit
SOCK = 26.0                        # blind PVC socket depth
def surf(a_deg, z):
    a = math.radians(a_deg); r = F.r_face(np.array([z]), np.array([a_deg]))[0] + T + F.D(K, np.array([z]), np.array([a_deg]))[0]
    p = np.array([r * math.sin(a), ELL * r * math.cos(a), z])
    n = np.array([math.sin(a), math.cos(a) / ELL, 0.0]); n /= np.linalg.norm(n); return p, n
def axis(var, yaw, side=1):
    p, n = surf(EXIT_A, EXIT_Z); E = p + n * OFF[var]
    y, pt = math.radians(yaw), math.radians(PITCH)
    d = np.array([math.sin(y) * math.cos(pt), -math.cos(y) * math.cos(pt), math.sin(pt)])
    if side < 0: E = E * np.array([-1, 1, 1]); d = d * np.array([-1, 1, 1])
    return E, d
def cyl_along(p0, d, L, rad, n=48):
    """cylinder of radius rad from p0 along unit d for length L"""
    d = np.asarray(d) / np.linalg.norm(d)
    c = M.cylinder(L, rad, rad, n)
    ax = np.cross([0, 0, 1], d); s = np.linalg.norm(ax); ang = math.degrees(math.atan2(s, d[2]))
    if s < 1e-9: return c.translate(list(p0))
    ax = ax / s
    # rotate about ax by ang: build via two Euler steps (rotate to x-z plane then tilt)
    yaw = math.degrees(math.atan2(d[1], d[0])); tilt = math.degrees(math.acos(np.clip(d[2], -1, 1)))
    return c.rotate([0, tilt, 0]).rotate([0, 0, yaw]).translate(list(p0))
def skin(cs, w0, w1, refine=1.5):
    """band following the pad's outer surface: w = 0 is the surface, + outward"""
    pl = cs.extrude(w1 - w0).translate([0, 0, w0]).refine_to_length(refine)
    def f(v):
        v = np.asarray(v); u, z, w = v[:, 0], v[:, 1], v[:, 2]
        a = -u / R; ad = np.degrees(a); r = F.r_face(z, ad) + T + F.D(K, z, ad) + w
        return np.stack([r * np.sin(a), ELL * r * np.cos(a), z], axis=1)
    return pl.warp_batch(f)
def U(a): return -math.radians(a) * R            # angle (deg, right side +) -> plate u
def footprint(var):
    """teardrop in (u, z): round over the pocket, tapering forward to the exit"""
    pts = [CS.circle(19.0, 48).translate([U(P.BTN_ANG), 1.0]), CS.circle(14.0 if var == "W1" else 12.0, 48).translate([U(EXIT_A - 4), EXIT_Z])]
    return CS.batch_hull(pts)
def _wedge():
    w = B3.B_tri_solid(-0.05, P.BTN_UP - B3.B_D + 0.2, P.BTN_UP + 0.3)
    return place(P._to_pad(w.translate([0, F.BTN_SHIFT, 0])), P.BTN_ANG)
def _screw_nut():
    c = ycyl(3.4, 40, 0, R - 2, F.BTN_Z) + B3.hexnut_y(F.NUT_M3_AF, 25.0, 0, R + T + F.BTN_SHIFT + P.BTN_UP + 1.5, F.BTN_Z)   # FINAL: nut drop channel open to the outside (was a buried 2.6 mm pocket)
    return place(c, P.BTN_ANG)
def lane(side=1):
    """W2: short soft recess under the socket only (all three angles), so the rod sits half-sunk where it leaves the pad"""
    cs = [cyl_along(axis("W2", y, side)[0] - axis("W2", y, side)[1] * (SOCK + 2), axis("W2", y, side)[1], SOCK + 18, 10.6) for y in YAWS]
    return M.batch_hull(cs)
_PADX = {}
def pad(var):
    if var in _PADX: return _PADX[var]
    m = F.pad(K)
    if var == "W2":
        for s in (1, -1): m = m - lane(s)
    _PADX[var] = m; return m
_MC = {}
def mount(var, yaw, side=1):
    key = (var, yaw)
    if key in _MC: return _MC[key] if side > 0 else _MC[key].mirror([1, 0, 0])
    m = _mount(var, yaw); _MC[key] = m
    return m if side > 0 else m.mirror([1, 0, 0])
def _mount(var, yaw, side=1):
    E, d = axis(var, yaw, 1)
    collar_back = E - d * (SOCK + 4.0)
    collar = cyl_along(collar_back, d, SOCK + 4.0, 13.5)
    pod = M.batch_hull([skin(footprint(var), 0.3, 3.0), collar])
    pod = pod + _wedge()
    pod = pod - skin(CS.square([300, 160]).translate([-150, -80]), -40, 0.3, refine=3.0)      # back hugs the pad surface (0.3 gap)
    pod = pod + _wedge()
    pod = pod - cyl_along(E - d * SOCK, d, SOCK + 30, 10.15) - _screw_nut()
    for k in (8.0, 18.0): pod = pod - cyl_along(E - d * k, [0, 0, 1], 20, 1.4)          # 2 kit wood-screw pilots from above, into the PVC
    pod = pod - pad(var)
    return pod if side > 0 else pod.mirror([1, 0, 0])
def blank(side=1, var="W1"):
    tri = place(P._to_pad(B3.B_tri_solid(-0.1, P.BTN_UP - B3.B_D + 0.2, P.BTN_UP + 4).translate([0, F.BTN_SHIFT, 0])), P.BTN_ANG)
    if var == "W2": tri = tri + lane(1)
    b = tri - skin(CS.square([300, 160]).translate([-150, -80]), 0.0, 30, refine=3.0) - _screw_nut()
    if var == "W2": b = b - pad("W2")
    return b if side > 0 else b.mirror([1, 0, 0])
def pvc(var, yaw, side=1, L=None):
    E, d = axis(var, yaw, side)
    S_, n = SA.head_pt("big"); L = L or 110.0
    return cyl_along(E - d * SOCK, d, L + SOCK, 10.0, 40)
def switch_at(var, yaw, side=1, L=110.0):
    """kit 65 mm button on a vertical connector at the rod end, turned to face the head (simplified: disc 65 x 35 + connector)"""
    E, d = axis(var, yaw, side); P_end = E + d * (L - 14)
    inward = np.array([-side, 0, 0.0]); inward = inward - d * np.dot(inward, d); inward /= np.linalg.norm(inward)
    conn = cyl_along(P_end - d * 14, d, 28, 13.5)
    btn = cyl_along(P_end + inward * (SA.V_AX + SA.PLATE_H), inward, SA.STACK, 32.5, 64)
    return conn, btn
if __name__ == "__main__":
    import headrest_P1_pad as P1
    f = F.foam(K)
    for var in ("W1", "W2"):
        p = pad(var)
        print(var, "pad", p.status(), "%.0f cm3" % (p.volume() / 1000))
        for yaw in YAWS:
            for s in (1, -1):
                m = mount(var, yaw, s); r = pvc(var, yaw, s, 140)
                print("  yaw %2.0f side %+d mount %s %.1f cm3 | mount∩pad %.1f | pvc∩pad %.1f | pvc∩foam %.1f | pvc∩mount(socket) %.1f | pvc∩head big %.0f small %.0f" % (
                    yaw, s, m.status(), m.volume() / 1000, (m ^ p).volume(), (r ^ p).volume(), (r ^ f).volume(), (r ^ m).volume(), (r ^ P1.head("big")).volume(), (r ^ P1.head("small")).volume()), flush=True)
    for s in (1, -1):
        b = blank(s); print("blank", s, b.status(), "%.1f cm3" % (b.volume() / 1000), "∩pad %.1f" % (b ^ pad("W1")).volume())
