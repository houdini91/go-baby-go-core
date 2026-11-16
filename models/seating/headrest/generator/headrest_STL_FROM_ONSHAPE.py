# Final STLs from ONSHAPE geometry (review-gallery tessellation, WORLD placement + 400 mm x shift per part) in print orientation.
#   pad upside down | mast on its side (+x down) | pod lying on its pad-side underside (right: +x up, left: -x up) | blank wedge down
import os, sys, glob, math, numpy as np, trimesh
IN, PFX, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
TH = math.radians(4.645078425891606); PIV = np.array([0, 123.5, 390.0]); CY = 28.78
def unrecline(v):
    q = v - PIV; y = q[:, 1] * math.cos(TH) - q[:, 2] * math.sin(TH); z = q[:, 1] * math.sin(TH) + q[:, 2] * math.cos(TH)
    return np.column_stack([q[:, 0], y, z]) + PIV
def R(axis, deg): return trimesh.transformations.rotation_matrix(math.radians(deg), axis)[:3, :3]
names = {0: "headrest_pad_F3", 1: "headrest_mast_H000", 2: "headrest_mast_H025", 3: "headrest_mast_H050", 4: "headrest_mast_H075", 5: "headrest_mast_H100",
         6: "headrest_button_pod_yaw12_right", 7: "headrest_button_pod_yaw20_right", 8: "headrest_button_pod_yaw28_right",
         9: "headrest_button_pod_yaw12_left", 10: "headrest_button_pod_yaw20_left", 11: "headrest_button_pod_yaw28_left",
         12: "headrest_pocket_blank_right", 13: "headrest_pocket_blank_left"}
os.makedirs(OUT, exist_ok=True)
for f in glob.glob(os.path.join(IN, PFX + "rev*.stl")):
    i = int(os.path.basename(f).split("rev")[1].split("_")[0])
    t = trimesh.load(f); t.merge_vertices(digits_vertex=4)
    v = t.vertices - np.array([400.0 * i, 0, 0]); v = unrecline(v)                 # -> LOCAL chair frame
    if i == 0 or i >= 6: v = v - np.array([0, CY, 451 + 50])                        # pad-frame parts were placed at H50
    if i == 0: v = v @ R([1, 0, 0], 180).T
    elif i <= 5: v = v @ R([0, 1, 0], 90).T
    elif i <= 11: v = v @ R([0, 1, 0], -90 if i <= 8 else 90).T
    else:
        s = 1 if i == 12 else -1; a = math.radians(46 * s); n = np.array([math.sin(a), math.cos(a) / 1.28, 0]); n /= np.linalg.norm(n)
        ax = np.cross(n, [0, 0, 1]); ang = math.degrees(math.acos(np.clip(n[2], -1, 1)))
        v = v @ R(ax / np.linalg.norm(ax), ang).T
    v = v - np.array([(v[:, 0].min() + v[:, 0].max()) / 2, (v[:, 1].min() + v[:, 1].max()) / 2, v[:, 2].min()])
    m = trimesh.Trimesh(v, t.faces); m.export(os.path.join(OUT, names[i] + ".stl"))
    print(names[i], "%.2f cm3" % (abs(m.volume) / 1000), "size", np.round(m.extents, 1), "watertight", m.is_watertight)
