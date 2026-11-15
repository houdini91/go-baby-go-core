# GbG headrest v1.0 FINAL (locked 2026-11-15): the reference geometry the Onshape FeatureScript port must match.
#   pad   = F3, slit-free, 8 x Ø26x1.5 Velcro-dot pockets, strap slots, triangle button pocket 8 mm deep, K2 tapered channel
#   mast  = M1 stem, B1 receiver R2, relief window, reach n = 0, heights H0/25/50/75/100
#   pod   = W1 at yaw 12/20/28 (default 20), left + right; blank plug
#   chair = top_back with the R2 receiver (chair branch)
import math, os, sys, time, numpy as np
from manifold3d import Manifold as M
import headrest_PAD_foam as Q, headrest_F_flow as F, headrest_M_mast as MM, headrest_MB_base as MB, headrest_SW_mount as W
import headrest_P2_pad as P, headrest_K_options as K, headrest_E4_geom as G
HEIGHTS = (0, 25, 50, 75, 100)
Q.ATTACH = "dots"
_C = {}
def pad():
    if "pad" not in _C: _C["pad"] = Q.pad("V")
    return _C["pad"]
def mast(H): return MB.mast("B1", 0, H, window=True)
def pod(yaw, side): return W._mount_final(yaw, side)
def blank(side): return W.blank(side)
def receiver(): return MM.receiver_chair()
# W uses F.pad(K) (slotted pad) to trim the pod; for the final pad trim against the slit-free pad (same outer surface)
def _mount_final(yaw, side=1):
    W._PADX["W1"] = pad(); return W.mount("W1", yaw, side)
W._mount_final = _mount_final
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "stl")
def save(m, name, xf=None):
    import trimesh
    me = m.to_mesh(); v = np.asarray(me.vert_properties)[:, :3]
    t = trimesh.Trimesh(v, np.asarray(me.tri_verts))
    if xf is not None: t.apply_transform(xf)
    t.apply_translation([-t.bounds[0][0] - (t.bounds[1][0] - t.bounds[0][0]) / 2, -t.bounds[0][1] - (t.bounds[1][1] - t.bounds[0][1]) / 2, -t.bounds[0][2]])
    t.export(os.path.join(OUT, name + ".stl")); return t
if __name__ == "__main__":
    t0 = time.time(); p = pad(); fm = Q.foam()
    print("pad", p.status(), "%.1f cm3" % (p.volume() / 1000), "genus", p.genus(), "foam∩pad %.2f" % (fm ^ p).volume(), flush=True)
    # pocket wall: deepen the pocket by d until it breaks into the head side
    for d in (1.0, 2.0, 3.0, 4.0):
        c = Q.place(P._to_pad(Q.P.B3.B_tri_solid(0.15, P.BTN_UP - P.B3.B_D - d, P.BTN_UP - P.B3.B_D)).translate([0, 0, 0]).translate([0, F.BTN_SHIFT, 0]), P.BTN_ANG)
        print("  pocket +%.0f mm deeper: material removed %.0f mm3 of %.0f (100%% = still inside the wall)" % (d, (c ^ p).volume(), c.volume()))
    for yaw in W.YAWS:
        for s in (1, -1):
            m = pod(yaw, s); r = W.pvc("W1", yaw, s, 140)
            print("pod yaw %g side %+d" % (yaw, s), m.status(), "%.1f cm3" % (m.volume() / 1000), "∩pad %.2f ∩foam %.2f pvc∩pad %.1f" % ((m ^ p).volume(), (m ^ fm).volume(), (r ^ p).volume()), flush=True)
    for s in (1, -1):
        b = blank(s); print("blank", s, b.status(), "%.1f cm3 ∩pad %.2f" % (b.volume() / 1000, (b ^ p).volume()))
    rc = receiver(); print("receiver top_back", rc.status(), "%.0f cm3" % (rc.volume() / 1000), flush=True)
    for H in HEIGHTS:
        m = mast(H); pp = MM.pad(0, H)
        raw = MM.plug() + MM.m1_body(0, H) + MM.L(MM.head_locked(), 0, H)
        print("mast H%d" % H, m.status(), "genus", m.genus(), "%.0f cm3" % (m.volume() / 1000), "| ∩pad %.2f ∩chair %.2f | raw∩chair (what the chair cut trims) %.0f mm3 | rods outside %.1f | window %s" % (
            (m ^ pp).volume(), (m ^ rc).volume(), (raw ^ rc).volume(), (MB.rods("B1", 0, H) - (m + MM.rod_holes(0, H))).volume(), MB.relief(0, H) is not None), "%.0fs" % (time.time() - t0), flush=True)
