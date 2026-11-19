# Shared geometry for the mount-option and review pages, built on the REAL Onshape build (viewer/r5cad/fitted).
#   wing(): the r5cad wing (29 mm envelope at the tip, the new swell behind the cheek), repaired to a watertight manifold
#   S frame: d' across the tip (+ outboard), t' into the wing (- = front), lean removed (gen_r5.py / gbg_sidepad_r5.fs)
#   rounded_cut(): a boolean cut whose rim is rounded / flared. The rim comes from a level set of a FINE LOCAL signed-distance field
#     (0.15-0.2 mm voxels, Gaussian-smoothed, only in a small box round the feature); everywhere else the wing stays the exact mesh
#   sweep_pill(): an arm as a real sweep of the r5 pill section (22 x 31, R10.5) along a path, with a rounded root
import os, sys, math, json, numpy as np, trimesh, manifold3d as mf
from scipy.ndimage import distance_transform_edt, gaussian_filter
from matplotlib.path import Path
HERE = os.path.dirname(os.path.abspath(__file__)); V = os.path.join(HERE, '..'); FIT = os.path.join(V, 'r5cad', 'fitted')
sys.path.insert(0, os.path.join(V, 'r5mesh'))
from frame import M_W2F
LD, LT, ZC = 0.0565, 0.069, 227.0
F2S = np.array([[1, 0, -LD, LD * ZC], [0, 1, -LT, LT * ZC], [0, 0, 1, 0], [0, 0, 0, 1]], float)
W2S = F2S @ M_W2F; S2W = np.linalg.inv(W2S)
T_FRONT = -14.27
def tf(m, M): return m.transform(np.asarray(M[:3, :4], float))
def to_mf(t):
    t = t.copy(); t.merge_vertices(digits_vertex=4)
    m = mf.Manifold(mf.Mesh(vert_properties=np.asarray(t.vertices, np.float32), tri_verts=np.asarray(t.faces, np.uint32)))
    assert m.status().name == 'NoError', m.status(); return m
def to_tm(m): q = m.to_mesh(); return trimesh.Trimesh(np.asarray(q.vert_properties)[:, :3], np.asarray(q.tri_verts), process=False)
def save_W(mS, f): to_tm(tf(mS, S2W)).export(f)            # S-frame solid -> world STL
def load_fit(name, S=True):
    m = to_mf(trimesh.load(os.path.join(FIT, name + '.stl'))); return tf(m, W2S) if S else m
def wing(S=True):
    cache = os.path.join(HERE, 'wing_cad_fixed.stl')
    if not os.path.exists(cache):
        from wingrepair import repaired
        m, _ = repaired(os.path.join(FIT, 'wing_L_GBG-SP-WG-L-r5.stl')); to_tm(m).export(cache)
    m = to_mf(trimesh.load(cache)); m = max(m.decompose(), key=lambda x: x.volume())   # drop a tiny inside-out shell (-36 mm3) of the export
    return tf(m, W2S) if S else m
def wing_full(S=True):
    """the r5cad wing with its round-5 notch, slot and groove filled back to the wing's own envelope (section at z 250), for
    mount options that leave the front edge whole (saddle, child-side socket)"""
    cache = os.path.join(HERE, 'wing_full.stl')
    if not os.path.exists(cache):
        Wm = wing()
        env = [p for p in Wm.slice(256.0).to_polygons() if len(p) > 2]   # above the notch's rounded top edge
        cs = mf.CrossSection([np.asarray(p) for p in env], mf.FillRule.EvenOdd).offset(-0.08, mf.JoinType.Round)   # strictly inside the envelope: no coplanar seams
        pr = cs.extrude(42.0).translate([0, 0, 206.0]) ^ box(-40, 60, -30, 32, 206.0, 248.0)
        full = max((Wm + pr).decompose(), key=lambda m: m.volume())
        to_tm(tf(full, S2W)).export(cache)
    m = to_mf(trimesh.load(cache)); return tf(m, W2S) if S else m
def box(d0, d1, t0, t1, z0, z1): return mf.Manifold.cube([d1 - d0, t1 - t0, z1 - z0]).translate([d0, t0, z0])

# ------------------------------------------------------------------ local fine SDF
class Grid:
    """voxel grid over the box lo..hi (S frame) with spacing h; .F = smoothed signed distance of the solid (positive inside)"""
    def __init__(self, solid, lo, hi, h=0.2, blur=1.5):
        self.lo = np.asarray(lo, float); self.h = h
        self.n = np.round((np.asarray(hi, float) - self.lo) / h).astype(int) + 1
        self.ax = [self.lo[i] + h * np.arange(self.n[i]) for i in range(3)]
        DD, TT = np.meshgrid(self.ax[0], self.ax[1], indexing='ij'); P2 = np.c_[DD.ravel(), TT.ravel()]
        ins = np.zeros(self.n, bool)
        for k, z in enumerate(self.ax[2]):
            wn = np.zeros(len(P2), np.int16)           # winding number: robust to overlapping or sliver contours
            for c in solid.slice(float(z)).to_polygons():
                if len(c) < 3: continue
                c = np.asarray(c); a = 0.5 * np.sum(c[:, 0] * np.roll(c[:, 1], -1) - np.roll(c[:, 0], -1) * c[:, 1])
                if abs(a) < 1e-6: continue
                wn += np.where(Path(c).contains_points(P2), 1 if a > 0 else -1, 0).astype(np.int16)
            ins[:, :, k] = (wn > 0).reshape(DD.shape)
        F = (distance_transform_edt(ins) - distance_transform_edt(~ins)) * h
        self.F = gaussian_filter(F, blur).astype(np.float32) if blur else F.astype(np.float32)
        self.D, self.T, self.Z = np.meshgrid(*[a.astype(np.float32) for a in self.ax], indexing='ij', sparse=True)
    def mesh(self, A, simp=0.004, smooth=12):
        """closed mesh of {A > 0} (vectorized marching tetrahedra on this grid, Taubin-smoothed); A is padded with an outside layer"""
        return mt_mesh(A, self.lo, self.h, simp, smooth)
def mt_mesh(A, lo, h, simp=0.004, smooth=12):
    A = np.pad(np.asarray(A, np.float32), 1, constant_values=-1.0); lo = np.asarray(lo, float) - h
    A[A == 0] = 1e-6
    nx, ny, nz = A.shape
    c = [A[dx:nx - 1 + dx, dy:ny - 1 + dy, dz:nz - 1 + dz] for dz in (0, 1) for dy in (0, 1) for dx in (0, 1)]    # corner v = dx + 2dy + 4dz
    pos = np.zeros(c[0].shape, np.int8)
    for q in c: pos += (q > 0)
    I, J, K = np.nonzero((pos > 0) & (pos < 8)); del pos, c
    gid = lambda dx, dy, dz: ((I + dx).astype(np.int64) * ny + (J + dy)) * nz + (K + dz)
    corner = [(v & 1, (v >> 1) & 1, (v >> 2) & 1) for v in range(8)]
    cid = np.stack([gid(*corner[v]) for v in range(8)], 1)
    cval = np.stack([A[I + corner[v][0], J + corner[v][1], K + corner[v][2]] for v in range(8)], 1)
    tets = [[0, 1 << a, (1 << a) | (1 << b), 7] for a, b in ((0, 1), (0, 2), (1, 0), (1, 2), (2, 0), (2, 1))]
    def pt(ids):
        k = ids % nz; j = (ids // nz) % ny; i = ids // (ny * nz); return np.stack([i, j, k], -1) * h + lo
    keys, ea, eb, va, vb, tri_e, inc, outc = [], [], [], [], [], [], [], []
    TRI = []; EA = []; EB = []; VA = []; VB = []; INC = []; OUTC = []
    for t in tets:
        ids = cid[:, t]; vals = cval[:, t]; s = vals > 0; n = s.sum(1)
        for case in (1, 3):
            m = n == case
            if not m.any(): continue
            lone_mask = s[m] if case == 1 else ~s[m]
            L = np.argmax(lone_mask, 1); others = np.argsort(lone_mask, 1, kind='stable')[:, :3]
            ii, vv = ids[m], vals[m]; r = np.arange(len(L))
            for o in range(3):
                EA.append(ii[r, L]); EB.append(ii[r, others[:, o]]); VA.append(vv[r, L]); VB.append(vv[r, others[:, o]])
            TRI.append(('t', len(L)))
            inside = np.where(s[m][:, :, None], pt(ii), 0).sum(1) / np.maximum(s[m].sum(1), 1)[:, None]
            outside = np.where(~s[m][:, :, None], pt(ii), 0).sum(1) / np.maximum((~s[m]).sum(1), 1)[:, None]
            INC.append(inside); OUTC.append(outside)
        m = n == 2
        if m.any():
            ii, vv, ss = ids[m], vals[m], s[m]; order = np.argsort(~ss, 1, kind='stable'); r = np.arange(len(ii))
            a_, b_, c_, d_ = (order[:, q] for q in range(4))
            for (x, y) in ((a_, c_), (a_, d_), (b_, d_), (a_, c_), (b_, d_), (b_, c_)):
                EA.append(ii[r, x]); EB.append(ii[r, y]); VA.append(vv[r, x]); VB.append(vv[r, y])
            TRI.append(('q', len(ii)))
            inside = (pt(ii[r, a_]) + pt(ii[r, b_])) / 2; outside = (pt(ii[r, c_]) + pt(ii[r, d_])) / 2
            INC += [inside, inside]; OUTC += [outside, outside]
    # assemble triangles: each 't' block contributed 3 edge arrays, each 'q' block 6 (two triangles)
    tri_pts_a, tri_pts_b, tri_va, tri_vb = [], [], [], []
    p = 0; tris = []
    for kind, cnt in TRI:
        if kind == 't':
            tris.append(np.stack([np.stack([EA[p + o], EB[p + o], VA[p + o], VB[p + o]], 1) for o in range(3)], 1)); p += 3
        else:
            e = [np.stack([EA[p + o], EB[p + o], VA[p + o], VB[p + o]], 1) for o in range(6)]
            tris.append(np.stack(e[:3], 1)); tris.append(np.stack(e[3:], 1)); p += 6
    T = np.concatenate(tris)                     # (m, 3, 4): ida, idb, va, vb (float array)
    ida = T[:, :, 0].astype(np.int64); idb = T[:, :, 1].astype(np.int64); fa = T[:, :, 2]; fb = T[:, :, 3]
    lo_id = np.minimum(ida, idb); hi_id = np.maximum(ida, idb)
    key = lo_id * (nx * ny * nz) + hi_id
    uk, inv = np.unique(key.ravel(), return_inverse=True)
    wa = fa / (fa - fb)
    P = pt(ida.ravel()) + (pt(idb.ravel()) - pt(ida.ravel())) * wa.ravel()[:, None]
    Vt = np.zeros((len(uk), 3)); Vt[inv] = P
    F = inv.reshape(-1, 3)
    INC = np.concatenate(INC); OUTC = np.concatenate(OUTC)
    nrm = np.cross(Vt[F[:, 1]] - Vt[F[:, 0]], Vt[F[:, 2]] - Vt[F[:, 0]])
    flip = (nrm * (OUTC - INC)).sum(1) < 0
    F[flip] = F[flip][:, ::-1]
    good = (F[:, 0] != F[:, 1]) & (F[:, 1] != F[:, 2]) & (F[:, 0] != F[:, 2])
    F = F[good]
    if smooth:                                    # Taubin smoothing removes the tetrahedral facet noise without shrinking
        tt = trimesh.Trimesh(Vt, F, process=False); tt.merge_vertices()
        trimesh.smoothing.filter_taubin(tt, lamb=0.5, nu=-0.53, iterations=smooth); Vt, F = np.asarray(tt.vertices), np.asarray(tt.faces)
    m = mf.Manifold(mf.Mesh(vert_properties=Vt.astype(np.float32), tri_verts=F.astype(np.uint32)))
    if m.status().name != 'NoError':
        mm = mf.Mesh(vert_properties=Vt.astype(np.float32), tri_verts=F.astype(np.uint32)); mm.merge(); m = mf.Manifold(mm)
    return m.simplify(simp) if simp else m
def smin(a, b, k): hh = np.maximum(k - np.abs(a - b), 0.0); return np.minimum(a, b) - hh * hh / (4 * k)
def smax(a, b, k): hh = np.maximum(k - np.abs(a - b), 0.0); return np.maximum(a, b) + hh * hh / (4 * k)
def sm(x): x = np.clip(x, 0.0, 1.0); return x * x * (3 - 2 * x)
def sdf_box(G, lo, hi, r=0.0):                    # positive inside a rounded box
    c = [(lo[i] + hi[i]) / 2 for i in range(3)]; e = [(hi[i] - lo[i]) / 2 - r for i in range(3)]
    q = [np.abs(A - c[i]) - e[i] for i, A in enumerate((G.D, G.T, G.Z))]
    return -(np.sqrt(sum(np.maximum(x, 0) ** 2 for x in q)) + np.minimum(np.maximum(np.maximum(q[0], q[1]), q[2]), 0) - r)
def sdf_cyl_t(G, d, z, r, t0, t1):                # positive inside a cylinder along t'
    rad = r - np.sqrt((G.D - d) ** 2 + (G.Z - z) ** 2); ax = np.minimum(G.T - t0, t1 - G.T)
    return np.minimum(rad, ax)
def rounded_cut(solid, tool_fn, lo, hi, k=4.0, h=0.18, near=None):
    """solid minus tool, with the rim rounded (radius ~ k/2..k, flared). Only the removal volume R is a level set:
    R = {smin(F_solid, -F_tool, k) < 0} near the tool, so outside R the result is the exact original mesh."""
    G = Grid(solid, lo, hi, h=h, blur=1.0)
    T = tool_fn(G)
    near = near if near is not None else 1.6 * k
    R = np.minimum(np.minimum(-smin(G.F, -T, k), T + near), G.F + 0.6)     # removal: where the rounded result is empty, near the tool
    # keep R away from the grid boundary (except where the tool itself reaches it)
    Rm = G.mesh(R)
    return solid - Rm, Rm

# ------------------------------------------------------------------ arm: sweep of the r5 pill section
def pill_pts(w, h, n):
    r = w / 2; c = h / 2 - r; per = 2 * math.pi * r + 4 * c; s = np.arange(n) / n * per; P = []
    for si in s:
        if si < math.pi * r: a = si / r; P.append([r * math.cos(a), c + r * math.sin(a)])
        elif si < math.pi * r + 2 * c: P.append([-r, c - (si - math.pi * r)])
        elif si < 2 * math.pi * r + 2 * c: a = math.pi + (si - math.pi * r - 2 * c) / r; P.append([r * math.cos(a), -c + r * math.sin(a)])
        else: P.append([r, -c + (si - 2 * math.pi * r - 2 * c)])
    return np.array(P)
def sweep_pill(path_pts, w=22.0, h=31.0, n=96, flare=None):
    """path_pts: (m, 3) world points along the arm centreline; the section's long axis stays vertical-ish (z); optional
    flare(u) -> scale of the section at fraction u (root blends)"""
    P = np.asarray(path_pts, float); m = len(P); S2 = pill_pts(w, h, n); rings = []
    for i in range(m):
        tv = P[min(i + 1, m - 1)] - P[max(i - 1, 0)]; tv /= np.linalg.norm(tv)
        xd = np.cross([0, 0, 1.0], tv); xd /= np.linalg.norm(xd); yd = np.cross(tv, xd)
        s = flare(i / (m - 1)) if flare else 1.0
        rings.append(P[i] + s * (S2[:, :1] * xd + S2[:, 1:] * yd))
    Vv = np.concatenate(rings); F = []
    for i in range(m - 1):
        a, b = i * n, (i + 1) * n
        for j in range(n): j2 = (j + 1) % n; F += [[a + j, b + j, a + j2], [a + j2, b + j, b + j2]]
    c0 = len(Vv); Vv = np.r_[Vv, [rings[0].mean(0)], [rings[-1].mean(0)]]
    for j in range(n): F += [[c0, j, (j + 1) % n], [c0 + 1, (m - 1) * n + (j + 1) % n, (m - 1) * n + j]]
    t = trimesh.Trimesh(Vv, np.array(F), process=False)
    if t.volume < 0: t.invert()
    return mf.Manifold(mf.Mesh(vert_properties=np.asarray(t.vertices, np.float32), tri_verts=np.asarray(t.faces, np.uint32)))
def bezier(p0, p1, p2, p3, n=60):
    u = np.linspace(0, 1, n)[:, None]; return (1 - u) ** 3 * p0 + 3 * (1 - u) ** 2 * u * p1 + 3 * (1 - u) * u * u * p2 + u ** 3 * p3
def QSw(d, t, z):                                   # S-frame point -> world
    return (S2W @ np.array([d, t, z, 1.0]))[:3]
