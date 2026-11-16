# Support-contact by SURFACE CLASS for candidate print orientations.
#   A = visible finish surface, B = hidden (under foam / against the pad), M = fit-critical mating surface.
# A face "needs support" if it overhangs > 45 deg and is not on the bed. With supports on the build plate only, the support
# lands on it if the straight-down ray reaches the plate; if the ray hits the part first it is an unsupported overhang (droop).
import math, os, sys, json, numpy as np, trimesh
import headrest_FINAL as FN, headrest_M_mast as MM, headrest_F_flow as F, headrest_K_options as K
R, ELL = 86.0, 1.28
def tm(m): me = m.to_mesh(); return trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts))
def rOut(z, a): return F.r_face(z, a) + 5 + F.D("F3", z, a)
def classify_pad(t):
    c = t.triangles_center; n = t.face_normals
    a = np.degrees(np.arctan2(c[:, 0], c[:, 1] / ELL)); rc = np.hypot(c[:, 0], c[:, 1] / ELL)
    radial = np.column_stack([np.sin(np.radians(a)), np.cos(np.radians(a)) / ELL, np.zeros(len(a))]); radial /= np.linalg.norm(radial, axis=1)[:, None]
    rin = F.r_face(c[:, 2], np.abs(a)); rout = rOut(c[:, 2], np.abs(a))
    cls = np.full(len(c), "A", dtype=object)
    cls[(rc < rin + 1.6) & ((n * radial).sum(1) < 0.3)] = "B"                           # head side (under the foam), dot pockets
    joint = (c[:, 2] < 2) & (np.abs(c[:, 0]) < 60) & (rc > 88) & (rc < rout - 0.4)      # channel / column socket / boss sockets
    pocket = (np.abs(np.abs(a) - 46) < 14) & (c[:, 2] > -18) & (c[:, 2] < 20) & (rc > rin + 1.6) & (rc < rout - 0.4)
    cls[joint | pocket] = "M"
    return cls
def classify_mast(t, H):
    c = t.triangles_center; zc = 451 + H
    cls = np.full(len(c), "A", dtype=object)
    cls[c[:, 2] < MM.TOP + 0.4] = "M"                                                     # plug in the socket
    cls[c[:, 2] > zc + K.BOT - 0.6] = "M"                                                 # tenon / column / bosses inside the pad
    return cls
def classify_pod(t, yaw):
    import headrest_SW_mount as W
    c = t.triangles_center; E, d = W.axis("W1", float(yaw), 1)
    a = np.degrees(np.arctan2(c[:, 0], c[:, 1] / ELL)); rc = np.hypot(c[:, 0], c[:, 1] / ELL)
    cls = np.full(len(c), "A", dtype=object)
    cls[rc < rOut(c[:, 2], np.abs(a)) + 1.0] = "B"                                        # underside on the pad + wedge
    q = c - E; along = q @ d; radial = np.linalg.norm(q - np.outer(along, d), axis=1)
    cls[(radial < 10.5) & (along > -26.5)] = "M"                                          # PVC bore
    return cls
def score(t, cls, T):
    m = t.copy(); m.apply_transform(T); m.apply_translation([0, 0, -m.bounds[0][2]])
    n = m.face_normals; c = m.triangles_center; ar = m.area_faces
    over = (n[:, 2] < -math.cos(math.radians(45))) & (c[:, 2] > 0.4)
    idx = np.where(over)[0]
    hit = m.ray.intersects_any(c[idx] - np.array([0, 0, 1e-3]), np.tile([0, 0, -1.0], (len(idx), 1)))
    out = {}
    for k in "ABM":
        sel = cls[idx] == k
        out[k + "_support_mm2"] = round(float(ar[idx][sel & ~hit].sum()))
        out[k + "_unsupported_mm2"] = round(float(ar[idx][sel & hit].sum()))
    return out
def Rm(axis, deg):
    T = trimesh.transformations.rotation_matrix(math.radians(deg), {"x": [1, 0, 0], "y": [0, 1, 0], "z": [0, 0, 1]}[axis]); return T
CAND = {"upright": np.eye(4), "upside_down": Rm("x", 180), "on_back": Rm("x", -90), "on_front": Rm("x", 90), "side(+x down)": Rm("y", 90), "side(-x down)": Rm("y", -90),
        "upside_down tilt+20": Rm("x", 160), "upside_down tilt-20": Rm("x", 200), "upright tilt+20": Rm("x", 20), "upright tilt-20": Rm("x", -20)}
if __name__ == "__main__":
    import headrest_FS_export as X
    rows = []
    pad = tm(MM.L(FN.pad(), 0, 0)).copy(); pad.apply_translation([0, -28.78, -451])
    cp = classify_pad(pad)
    for on, T in CAND.items(): r = dict(part="pad", orient=on, **score(pad, cp, T)); rows.append(r); print(json.dumps(r), flush=True)
    for H in (50, 100):
        ms = tm(FN.mast(H)); cm = classify_mast(ms, H)
        for on in ("upright", "upside_down", "on_back", "on_front", "side(+x down)", "side(-x down)"):
            r = dict(part=f"mast_H{H}", orient=on, **score(ms, cm, CAND[on])); rows.append(r); print(json.dumps(r), flush=True)
    import headrest_SW_mount as W
    po = tm(FN.pod(20.0, 1)); cpo = classify_pod(po, 20)
    E, d = W.axis("W1", 20.0, 1); Rz = np.eye(4); Rz[:3, :3] = X.rot_to_z(d)
    pc = dict(CAND); pc["bore_up (d up)"] = Rz; pc["bore_down"] = Rm("x", 180) @ Rz
    for on in ("bore_up (d up)", "bore_down", "upright", "upside_down", "on_back", "on_front", "side(+x down)", "side(-x down)"):
        r = dict(part="pod_yaw20", orient=on, **score(po, cpo, pc[on])); rows.append(r); print(json.dumps(r), flush=True)
    os.makedirs(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "review"), exist_ok=True); json.dump(rows, open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "review", "surface_support.json"), "w"), indent=1)
