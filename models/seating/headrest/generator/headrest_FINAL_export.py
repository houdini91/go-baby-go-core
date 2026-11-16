# Writes the FINAL STLs in PRINT ORIENTATION (part on z = 0, centred), the chair top_back with the receiver (chair coords),
# and the headrest FIT-TEST bar.  Same orientations as the FeatureScript "Print layout".
import os, sys, math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_FINAL as FN, headrest_SW_mount as W, headrest_FS_export as X, headrest_M_mast as MM, headrest_E4_geom as G
import headrest_P3_button as B3, headrest_FITS as FIT
def Ry(d): a = math.radians(d); return np.array([[math.cos(a), 0, math.sin(a), 0], [0, 1, 0, 0], [-math.sin(a), 0, math.cos(a), 0], [0, 0, 0, 1]])
def to_z(v, sign=1):
    Rm = X.rot_to_z(v)
    if sign < 0: Rm = np.diag([1, -1, -1]) @ Rm
    M4 = np.eye(4); M4[:3, :3] = Rm; return M4
MIR = np.diag([-1, 1, 1, 1])
def nrm(s):
    a = math.radians(s * 46); n = np.array([math.sin(a), math.cos(a) / 1.28, 0]); return n / np.linalg.norm(n)
def fit_test():
    """one bar, 8 mm thick, brackets the TOLERANCE BLOCK (headrest_FITS.py = the .fs): rod bores Ø8.3/8.5/8.7 (vertical) |
       M3 nut traps AF 5.6/5.8/6.0 x 2.8 deep (+ Ø3.5 clearance) | M5 nut traps AF 8.1/8.3/8.5 x 4.4 deep (+ Ø5.5) |
       PVC rings Ø20.6/20.8/21.0 | a triangle pocket (+0.15) and a matching wedge (-0.05) to test the button-pod fit"""
    bar = G.rrbox(0, 150, 0, 40, 0, 8, 1.5)
    cuts = []
    for i, d in enumerate((FIT.ROD_HOLE_D - 0.2, FIT.ROD_HOLE_D, FIT.ROD_HOLE_D + 0.2)): cuts.append(M.cylinder(20, d / 2, d / 2, 64).translate([10 + 11 * i, 10, -5]))
    for i, af in enumerate((5.6, 5.8, 6.0)):
        cuts.append(CS.circle(af / math.sqrt(3), 6).extrude(FIT.NUT_M3_TRAP).translate([10 + 11 * i, 28, 8 - FIT.NUT_M3_TRAP])); cuts.append(M.cylinder(20, FIT.M3_CLEAR / 2, FIT.M3_CLEAR / 2, 24).translate([10 + 11 * i, 28, -5]))
    for i, af in enumerate((8.1, 8.3, 8.5)):
        cuts.append(CS.circle(af / math.sqrt(3), 6).extrude(FIT.NUT_M5_TRAP).translate([50 + 13 * i, 28, 8 - FIT.NUT_M5_TRAP])); cuts.append(M.cylinder(20, FIT.M5_CLEAR / 2, FIT.M5_CLEAR / 2, 24).translate([50 + 13 * i, 28, -5]))
    m = bar - M.batch_boolean(cuts, 0)
    rings = M.batch_boolean([M.cylinder(12, d / 2 + 3, d / 2 + 3, 96).translate([0, 0, 0]) - M.cylinder(14, d / 2, d / 2, 96).translate([0, 0, -1]) for d in ()], 0) if False else None
    for i, d in enumerate((FIT.PVC_BORE - 0.2, FIT.PVC_BORE, FIT.PVC_BORE + 0.2)):
        r = M.cylinder(12, d / 2 + 2.5, d / 2 + 2.5, 128) - M.cylinder(14, d / 2, d / 2, 128).translate([0, 0, -1])
        m = m + r.translate([92 + 0 * i, 0, 0]).translate([0, 0, 0]) if False else m + r.translate([100 + 0, 0, 0]).translate([0, 0, 0]) if False else m
        m = m + r.translate([0, 0, 0]).translate([20 + 27 * i, 62, 0])
    # triangle pocket block + wedge
    blk = G.rrbox(95, 140, 45, 85, 0, 10, 1.5)
    tri = B3.B_tri_solid(0.15, 0, 9).rotate([90, 0, 0])
    tb = tri.bounding_box(); tri = tri.translate([117.5 - (tb[0] + tb[3]) / 2, 65 - (tb[1] + tb[4]) / 2, 10 - tb[5] + 0.01 + 0 * 0])
    m = m + (blk - tri.translate([0, 0, 0]))
    w = B3.B_tri_solid(-0.05, 0, 8.0).rotate([90, 0, 0]); wb = w.bounding_box()
    w = w.translate([-(wb[0] + wb[3]) / 2 + 117.5, -(wb[1] + wb[4]) / 2 + 105, -wb[2]])
    return m + w
def pods_only(out=None):
    """the 6 pods only (print orientation), into stl/ or `out`"""
    if out: FN.OUT = out
    os.makedirs(FN.OUT, exist_ok=True)
    for yaw in (12, 20, 28):
        FN.save(FN.pod(float(yaw), 1), f"headrest_button_pod_yaw{yaw}_right", to_z([1.0, 0, 0], 1))
        FN.save(FN.pod(float(yaw), -1), f"headrest_button_pod_yaw{yaw}_left", to_z([-1.0, 0, 0], 1))
def main():
    os.makedirs(FN.OUT, exist_ok=True)
    out = {}
    out["headrest_pad_F3"] = FN.save(FN.pad(), "headrest_pad_F3")
    for H in FN.HEIGHTS: out[f"H{H}"] = FN.save(FN.mast(H), f"headrest_mast_H{H:03d}", Ry(90))
    for yaw in (12, 20, 28):
        E, d = W.axis("W1", float(yaw), 1)
        # print orientation = the .fs podUp(s): lying on the pad-side underside, +x up (right) / -x up (left)
        FN.save(FN.pod(float(yaw), 1), f"headrest_button_pod_yaw{yaw}_right", to_z([1.0, 0, 0], 1))
        FN.save(FN.pod(float(yaw), -1), f"headrest_button_pod_yaw{yaw}_left", to_z([-1.0, 0, 0], 1))
    FN.save(FN.blank(1), "headrest_pocket_blank_right", to_z(nrm(1), 1))
    FN.save(FN.blank(-1), "headrest_pocket_blank_left", to_z(nrm(-1), 1))
    FN.save(fit_test(), "headrest_fit_test")
    # chair part: top_back with the R2 receiver, chair (world) coordinates, NOT re-oriented
    import trimesh
    rc = G.recline(MM.receiver_chair()); me = rc.to_mesh()
    trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts)).export(os.path.join(FN.OUT, "chair_top_back_with_headrest_receiver.stl"))
    print("ok")
if __name__ == "__main__":
    if "pods" in sys.argv[1:]: pods_only()
    elif "fit" in sys.argv[1:]: os.makedirs(FN.OUT, exist_ok=True); FN.save(fit_test(), "headrest_fit_test")
    else: main()
