# Print-orientation study: every part x candidate orientations, real Bambu slices with tree supports (plate only) and
# without; support grams = with - without.
import os, sys, json, math, subprocess, numpy as np, trimesh
import headrest_FINAL as FN, headrest_SW_mount as W, headrest_FS_export as X
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "review", "orient"); os.makedirs(OUT, exist_ok=True)
def Rm(axis, deg):
    a = math.radians(deg); c, s = math.cos(a), math.sin(a)
    M = {"x": [[1, 0, 0], [0, c, -s], [0, s, c]], "y": [[c, 0, s], [0, 1, 0], [-s, 0, c]], "z": [[c, -s, 0], [s, c, 0], [0, 0, 1]]}[axis]
    T = np.eye(4); T[:3, :3] = M; return T
CAND = {"upright": np.eye(4), "upside_down": Rm("x", 180), "on_back(+y down)": Rm("x", -90), "on_front(-y down)": Rm("x", 90), "on_side(+x down)": Rm("y", 90), "on_side(-x down)": Rm("y", -90)}
def tm(m):
    me = m.to_mesh(); return trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts))
def slice_(path, sup, walls, inf):
    sh = os.path.join(os.path.dirname(os.path.abspath(__file__)), "slice_S.sh" if sup else "slice_E.sh")
    r = subprocess.run([sh, path, str(walls), str(inf), "020"], capture_output=True, text=True, env=dict(os.environ))
    for line in r.stdout.splitlines()[::-1]:
        if line.startswith("{"): return json.loads(line)
    return {"result": "fail", "out": r.stdout[-300:] + r.stderr[-300:]}
parts = {"pad": (FN.pad(), 4, 20), "mast_H50": (FN.mast(50), 6, 40), "mast_H100": (FN.mast(100), 6, 40), "pod_yaw20": (FN.pod(20.0, 1), 4, 40)}
only = sys.argv[1:] or list(parts)
res = {}
for nm in only:
    m, walls, inf = parts[nm]; base = tm(m)
    for on, T in CAND.items():
        t = base.copy(); t.apply_transform(T); t.apply_translation(-t.bounds[0] + [0, 0, 0])
        tag = f"{nm}__{on.split('(')[0]}"; p = os.path.join(OUT, tag + ".stl"); t.export(p)
        a = slice_(p, True, walls, inf); b = slice_(p, False, walls, inf)
        row = dict(part=nm, orient=on, h_sup=a.get("time_h"), g_sup=a.get("petg_g"), g_nosup=b.get("petg_g"), support_g=round((a.get("petg_g") or 0) - (b.get("petg_g") or 0), 1), height=round(float(t.extents[2]), 1))
        res[tag] = row; print(json.dumps(row), flush=True)
json.dump(res, open(os.path.join(OUT, "orient_%s.json" % "_".join(only)), "w"), indent=1)
