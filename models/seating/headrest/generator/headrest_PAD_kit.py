# Padding TEST KIT: shell variants for every attachment method + a small TEST COUPON to try them all (and heat forming) cheaply.
#   V-dots    : 8 round pockets Ø26 x 1.5 (pre-cut Velcro dots)                      -> headrest_PAD_foam.pad("V") with ATTACH="dots"
#   V-strips  : 7 rectangular recesses 1.5 deep (pieces cut from 25 mm Velcro tape)
#   plain     : no pockets (self-adhesive EVA stuck straight in)
#   COUPON    : the right wing of the shell (angle 36..80 deg): 2 dot pockets, 1 strip recess, 1 plain zone,
#               + the real strap slot and button pocket. ~1 h print instead of a full shell per method.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_PAD_foam as Q, headrest_F_flow as F

R, ELL = Q.R, Q.ELL
_C = {}
def shell(var):
    if var in _C: return _C[var]
    base = Q._pad_noslots()
    if var == "dots":
        Q.ATTACH = "dots"; m = base - Q.recesses()
    elif var == "strips":
        Q.ATTACH = "strips"; m = base - Q.recesses(); Q.ATTACH = "dots"
    else: m = base
    _C[var] = m; return m

# ---------------- coupon ----------------
CP_A = (36.0, 80.0)
CP_DOTS, CP_PLAIN = [(42.0, 32.0), (48.0, -28.0)], None   # 2 dots (top + low), plain zone for adhesive EVA
CP_STRIP = (55.0, 4.0, 14.0, 30.0)
def wedge(a0, a1, big=400.0):
    pts = [(0.0, 0.0)] + [(big * math.sin(math.radians(a)), big * ELL * math.cos(math.radians(a))) for a in np.linspace(a0, a1, 24)]
    cs = CS([pts]); cs = cs if cs.area() > 0 else CS([pts[::-1]])
    return cs.extrude(300).translate([0, 0, -150])
def coupon():
    if "cp" in _C: return _C["cp"]
    m = Q._pad_noslots() ^ wedge(*CP_A)
    pk = Q.inner_band(CS.circle(Q.DOT_POCKET / 2, 48).translate([Q.U(CP_DOTS[0][0]), CP_DOTS[0][1]]) + CS.circle(Q.DOT_POCKET / 2, 48).translate([Q.U(CP_DOTS[1][0]), CP_DOTS[1][1]]), -2.0, Q.VEL_D)
    st = Q.inner_band(Q.rrect(Q.U(CP_STRIP[0]), CP_STRIP[1], CP_STRIP[2], CP_STRIP[3]), -2.0, Q.VEL_D)
    m = m - pk - st
    _C["cp"] = m; return m

if __name__ == "__main__":
    import trimesh, os, headrest_P2_pad as P
    OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "padding", "test_kit"); os.makedirs(OUT, exist_ok=True)
    def save(m, name):
        me = m.to_mesh(); trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts)).export(os.path.join(OUT, name + ".stl"))
    base = Q._pad_noslots()
    for v in ("dots", "strips", "plain"):
        s = shell(v); save(s, f"shell_F3_{v}")
        print(v, s.status(), "%.0f cm3" % (s.volume() / 1000), flush=True)
    cp = coupon(); save(cp, "coupon_F3_attach_test"); b = cp.bounding_box()
    print("coupon", cp.status(), "%.0f cm3" % (cp.volume() / 1000), "size %.0f x %.0f x %.0f" % (b[3] - b[0], b[4] - b[1], b[5] - b[2]))
