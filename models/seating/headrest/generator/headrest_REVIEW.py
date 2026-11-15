# Shape review: Onshape (native FeatureScript, review gallery tessellation) vs the reviewed Python model, every part/variant.
# Gallery layout (FS selfTest): rev0 pad @H50 | rev1-5 mast H0..H100 | rev6-8 pod yaw 12/20/28 right | rev9-11 left | rev12-13 blank R/L,
# WORLD placement shifted +400*i in x.
import os, sys, json, math, glob, numpy as np, trimesh
import headrest_FINAL as FN, headrest_M_mast as MM, headrest_E4_geom as G
IN = sys.argv[1]; PFX = sys.argv[2] if len(sys.argv) > 2 else "rev"; OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "review"); os.makedirs(OUT, exist_ok=True)
def tm(m):
    me = m.to_mesh(); return trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts))
def ref(i):
    if i == 0: return "pad F3", G.recline(MM.L(FN.pad(), 0, 50))
    if 1 <= i <= 5: H = (0, 25, 50, 75, 100)[i - 1]; return f"mast H{H}", G.recline(FN.mast(H))
    if 6 <= i <= 11: yaw = (12, 20, 28)[(i - 6) % 3]; s = 1 if i < 9 else -1; return f"pod yaw{yaw} {'right' if s > 0 else 'left'}", G.recline(MM.L(FN.pod(float(yaw), s), 0, 50))
    s = 1 if i == 12 else -1; return f"blank {'right' if s > 0 else 'left'}", G.recline(MM.L(FN.blank(s), 0, 50))
def dist(a, b, n=40000):
    pa, _ = trimesh.sample.sample_surface(a, n, seed=1); pb, _ = trimesh.sample.sample_surface(b, n, seed=2)
    da = np.abs(trimesh.proximity.signed_distance(b, pa)); db = np.abs(trimesh.proximity.signed_distance(a, pb))
    return da, db
def wall(m, n=6000):
    p, fi = trimesh.sample.sample_surface(m, n, seed=3); nrm = m.face_normals[fi]
    loc, ri, _ = m.ray.intersects_location(p - nrm * 1e-3, -nrm, multiple_hits=False)
    d = np.full(n, np.nan); d[ri] = np.linalg.norm(loc - p[ri], axis=1); return d
rows = []
BD = os.path.join(IN, ("g11_" if PFX.startswith("g11") else "") + "bodydetails.json"); faces = json.load(open(BD)) if os.path.exists(BD) else {}
files = glob.glob(os.path.join(IN, PFX + "*rev*.stl"))
key = lambda x: int(os.path.basename(x).split("rev")[1].split("_")[0])
for f in sorted(files, key=key):
    i = key(f)
    on = trimesh.load(f); on.merge_vertices(); on.apply_translation([-400 * i, 0, 0])
    name, rm = ref(i); r = tm(rm)
    da, db = dist(on, r)
    w = wall(on)
    fd = faces.get(os.path.basename(f)[len(PFX):-4] if PFX.startswith("g11") else os.path.basename(f)[:-4], {})
    rows.append(dict(i=i, part=name, onshape_cm3=round(on.volume / 1000, 2), python_cm3=round(r.volume / 1000, 2),
                     dvol_pct=round(100 * (on.volume - r.volume) / r.volume, 2), watertight=bool(on.is_watertight), bodies=len(on.split(only_watertight=False)),
                     dev_mean=round(float(np.mean(np.r_[da, db])), 3), dev_p99=round(float(np.percentile(np.r_[da, db], 99)), 3), dev_max=round(float(max(da.max(), db.max())), 2),
                     wall_min=round(float(np.nanpercentile(w, 0.5)), 2), wall_p5=round(float(np.nanpercentile(w, 5)), 2),
                     bbox_on=np.round(on.extents, 1).tolist(), bbox_py=np.round(r.extents, 1).tolist(), faces=fd))
    # where are the big deviations? save the worst points for the page
    pa, _ = trimesh.sample.sample_surface(on, 40000, seed=1)
    worst = pa[np.argsort(-da)[:5]]
    rows[-1]["worst_at"] = np.round(worst, 1).tolist()
    print(json.dumps({k: v for k, v in rows[-1].items() if k not in ("faces", "worst_at")}), flush=True)
json.dump(rows, open(os.path.join(OUT, "review_%s.json" % PFX), "w"), indent=1)
