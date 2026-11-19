# Repair the r5cad wing STL (Onshape export, not watertight: a degenerate fan at (148.8, 73.3, 179.5), the same spot as on the
# original chair mesh, plus a few doubled edges at the top and near (118, 117, 195)). Faces on non-manifold edges are dropped,
# small degenerate slivers removed, and every boundary loop is closed with a fan. Returns a manifold3d.Manifold (world mm).
import numpy as np, trimesh, manifold3d as mf
def repaired(path, digits=3, rounds=6):
    t = trimesh.load(path); t.merge_vertices(digits_vertex=digits)
    t.update_faces(t.nondegenerate_faces()); t.update_faces(t.unique_faces()); t.remove_unreferenced_vertices()
    for _ in range(rounds):
        E = np.sort(t.faces[:, [0, 1, 1, 2, 2, 0]].reshape(-1, 2), axis=1)
        u, inv, c = np.unique(E, axis=0, return_inverse=True, return_counts=True)
        badf = np.any((c[inv] > 2).reshape(-1, 3), axis=1)
        if not badf.any(): break
        t.update_faces(~badf); t.remove_unreferenced_vertices()
    # boundary vertices used by more than 2 boundary edges are fan apexes: drop their faces so the holes become simple loops
    for _ in range(4):
        E = np.sort(t.faces[:, [0, 1, 1, 2, 2, 0]].reshape(-1, 2), axis=1); u, c = np.unique(E, axis=0, return_counts=True)
        b = u[c == 1]
        if not len(b): break
        vv, vc = np.unique(b.ravel(), return_counts=True); apex = set(vv[vc > 2].tolist())
        if not apex: break
        t.update_faces(~np.isin(t.faces, list(apex)).any(axis=1)); t.remove_unreferenced_vertices()
    # close boundary loops with fans
    F = t.faces; E = np.sort(F[:, [0, 1, 1, 2, 2, 0]].reshape(-1, 2), axis=1); u, c = np.unique(E, axis=0, return_counts=True)
    bnd = set(map(tuple, u[c == 1])); he = {}
    for f in F:
        for a, b in ((f[0], f[1]), (f[1], f[2]), (f[2], f[0])):
            if (min(a, b), max(a, b)) in bnd: he[b] = a
    Vs = list(t.vertices); NF = list(F); seen = set(); loops = 0
    for s in list(he):
        if s in seen: continue
        loop = [s]; seen.add(s); x = he.get(s)
        while x is not None and x != s and x not in seen: loop.append(x); seen.add(x); x = he.get(x)
        if x != s or len(loop) < 3: continue
        ci = len(Vs); Vs.append(np.mean(t.vertices[loop], 0)); loops += 1
        for i in range(len(loop)): NF.append([loop[i], loop[(i + 1) % len(loop)], ci])
    w = trimesh.Trimesh(np.array(Vs), np.array(NF), process=False)
    M = mf.Manifold(mf.Mesh(vert_properties=np.asarray(w.vertices, np.float32), tri_verts=np.asarray(w.faces, np.uint32)))
    return M, loops
if __name__ == '__main__':
    import sys
    M, n = repaired(sys.argv[1]); print(M.status(), round(M.volume()), 'loops closed', n)
