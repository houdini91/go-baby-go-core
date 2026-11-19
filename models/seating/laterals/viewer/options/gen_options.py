# Mount options document (viewer/options/): builds the option-only meshes and packs everything the page needs into
# options/geo.json + options/geodata.json (base64 float32 mm + uint32 index, same format as ../pack.py).
#   (1) tongue + support stay: the r5cad build (viewer/r5cad/fitted, the real Onshape parts) + a 20 x 30 PETG post from the
#       depth-piece foot to the arm rest top, at a short setting (left, long arm LOW, as fitted) and a long one (short arm HIGH:
#       the right-side fitted parts mirrored to the left)
#   (2) thick saddle: on the CLEAN wing (../mock5b/wing_clean.stl), level set of a voxel distance field like ../mock5b/gen_clean.py:
#       outer jaw 12 mm, 100 mm tall (z 181-281), inner jaw 12 mm, 69 mm tall (z 181-250), <= 28 mm front-to-back (S1), tapered
#       edges, 6 mm over the front edge; the round-5 long arm and a flush button; bare wing = clean wing + rounded catch notch
#   (3) tongue as built: the r5cad build alone
# Run: ../.venv/bin/python gen_options.py      (about 8 min; writes *.stl, report.json, sections.json), then gen_prong.py, then pack_options.py
import os, sys, json, math, base64, time, numpy as np, trimesh, manifold3d as mf
from scipy.ndimage import distance_transform_edt
from matplotlib.path import Path
HERE = os.path.dirname(os.path.abspath(__file__)); V = os.path.join(HERE, '..')
sys.path.insert(0, os.path.join(V, 'mock5b')); sys.path.insert(0, os.path.join(V, 'r5mesh'))
from common import load_r5
T0 = time.time()
def log(*a): print(f'[{time.time() - T0:6.1f}s]', *a, flush=True)
g = load_r5(); os.chdir(HERE)
W2S, S2W, tf, box, CS = g['W2S'], g['S2W'], g['tf'], g['box'], g['CS']
C, T_FRONT = g['C'], g['T_FRONT']
def tm2mf(t): return mf.Manifold(mf.Mesh(vert_properties=np.asarray(t.vertices, np.float32), tri_verts=np.asarray(t.faces, np.uint32)))
def mf2tm(m): q = m.to_mesh(); return trimesh.Trimesh(np.asarray(q.vert_properties)[:, :3], np.asarray(q.tri_verts), process=False)
def load(f):
    t = trimesh.load(f); t.merge_vertices(); return t
FIT = os.path.join(V, 'r5cad', 'fitted')
rep = {}

# ------------------------------------------------------------------ (1) stays: rounded 20 x 30 post, foot underside -> arm rest top
AR_TOP = 150.0                                   # arm rest post top under the foot's outboard side (x 137-157, y 25-45), measured
def stay(z_foot, name):
    """swept rounded rectangle (20 across x, 30 along y), from the foot's underside (x 116) curving outboard onto the arm rest top
    (x 147), with flared ends that blend into both parts (family look: one soft body, no box edges)"""
    p0, p3 = np.array([116.0, 34.0, z_foot + 3.0]), np.array([147.0, 35.0, AR_TOP - 2.0])
    p1, p2 = p0 + [0, 0, -0.45 * (z_foot - AR_TOP)], p3 + [-4, 0, 0.45 * (z_foot - AR_TOP) + 6]
    n = 28; parts = []
    for i in range(n + 1):
        u = i / n; P = (1 - u) ** 3 * p0 + 3 * (1 - u) ** 2 * u * p1 + 3 * (1 - u) * u * u * p2 + u ** 3 * p3
        fl = 1.0 + 0.35 * (math.exp(-((u) / 0.12) ** 2) + math.exp(-((1 - u) / 0.12) ** 2))      # flare at both ends
        w, d = 20.0 * fl, 30.0 * fl; r = min(w, d) / 2 - 0.5
        cs = CS.square([w - 2 * r, d - 2 * r], center=True).offset(r, g['JT'], circular_segments=28)
        parts.append(cs.extrude(1.2).translate([P[0], P[1], P[2] - 0.6]))
    hull = [mf.Manifold.batch_hull([parts[i], parts[i + 1]]) for i in range(n)]
    m = mf.Manifold.batch_boolean(hull, mf.OpType.Add)
    # two M5 button heads on the arm rest flange (the arm rest change: threaded inserts)
    for dy in (-9.0, 9.0):
        m = m + mf.Manifold.cylinder(2.2, 4.75, 3.6, 32).translate([p3[0] + 6.0, p3[1] + dy, p3[2] + 2.0 - 2.6]) - mf.Manifold.cylinder(30, 2.7, 2.7, 24).translate([p3[0] + 6.0, p3[1] + dy, p3[2] - 20])
    m = m ^ mf.Manifold.cube([200, 200, z_foot + 1.0 - (AR_TOP - 4.0)]).translate([20, -60, AR_TOP - 4.0])
    mf2tm(m).export(name); return m, float(np.linalg.norm(p3 - p0))
st_short, L1 = stay(158.7, 'stay_short.stl')
st_long, L2 = stay(224.7, 'stay_long.stl')
rep['stay'] = dict(section='20 x 30 PETG (flared ends)', short_setting='long arm LOW (foot bottom z 158.7)', long_setting='short arm HIGH (foot bottom z 224.7)',
                   path_length_mm=[round(L1), round(L2)], arm_rest_top_z=AR_TOP, fixing='2 x M5 into the arm rest top (inserts)')
# long setting: right fitted parts mirrored to the left
MX = np.diag([-1.0, 1, 1, 1])
for f, out in (('GBG-SP-AR-R-S-HI-r5', 'hi_carrier'), ('GBG-SP-BT-R-r5', 'hi_button'), ('GBG-SP-TS-r5_R', 'hi_sleeve'), ('GBG-SP-DP-D0-T0-r5_R', 'hi_depth'),
               ('GBG-SP-PD-r5_R', 'hi_pad'), ('GBG-SP-RG-r5_R', 'hi_ring'), ('GBG-SP-FM-20-r5_R', 'hi_foam'), ('GBG-SP-CV-r5_R', 'hi_cover')):
    t = load(os.path.join(FIT, f + '.stl')); t.apply_transform(MX); t.invert(); t.export(out + '.stl')
# clashes of the stays with the arm rest and the kit (rough, mesh booleans)
def raw(k):
    tris = [fc['vertices'] for body in json.load(open(os.path.join(V, 'raw', k + '.json'))) for face in body['faces'] for fc in face['facets']]
    a = np.array(tris).reshape(-1, 3) * 1000.0; t = trimesh.Trimesh(a, np.arange(len(a)).reshape(-1, 3)); t.merge_vertices(digits_vertex=3); return t
armrest = raw('ps1_J_D')
log('stays', rep['stay'])

# ------------------------------------------------------------------ (2) thick saddle on the clean wing
Wc = tm2mf(load(os.path.join(V, 'mock5b', 'wing_clean.stl'))).transform(np.asarray(W2S[:3, :4], float))
H = 0.3
G0 = np.array([-24.0, -28.0, 176.0]); G1 = np.array([36.0, 40.0, 292.0])
NG = np.round((G1 - G0) / H).astype(int) + 1
gd, gt, gz = [G0[i] + H * np.arange(NG[i]) for i in range(3)]
DD, TT = np.meshgrid(gd, gt, indexing='ij'); P2 = np.c_[DD.ravel(), TT.ravel()]
inside = np.zeros(NG, bool)
for k, z in enumerate(gz):
    m = np.zeros(len(P2), bool)
    for c in Wc.slice(float(z)).to_polygons():
        if len(c) > 2: m ^= Path(np.asarray(c)).contains_points(P2)
    inside[:, :, k] = m.reshape(DD.shape)
FW = ((distance_transform_edt(inside) - distance_transform_edt(~inside)) * H).astype(np.float32); del inside
D3, T3, Z3 = np.meshgrid(gd, gt, gz, indexing='ij')
log('sdf', NG)
def sm(x): x = np.clip(x, 0.0, 1.0); return x * x * (3 - 2 * x)
def smin(a, b, k): h = np.maximum(k - np.abs(a - b), 0.0); return np.minimum(a, b) - h * h / (4 * k)
def sbox(lo, hi, r=0.0):
    c = [(lo[i] + hi[i]) / 2 for i in range(3)]; e = [(hi[i] - lo[i]) / 2 - r for i in range(3)]
    q = [np.abs(A - c[i]) - e[i] for i, A in enumerate((D3, T3, Z3))]
    return -(np.sqrt(sum(np.maximum(x, 0) ** 2 for x in q)) + np.minimum(np.maximum(np.maximum(q[0], q[1]), q[2]), 0) - r)
def level(F, lo, hi, edge, simp=0.06):
    i0 = np.floor((np.asarray(lo) - G0) / H).astype(int); i1 = np.ceil((np.asarray(hi) - G0) / H).astype(int)
    A = np.ascontiguousarray(F[i0[0]:i1[0] + 1, i0[1]:i1[1] + 1, i0[2]:i1[2] + 1]).astype(np.float64)
    o = G0 + i0 * H; n = np.array(A.shape) - 1; flat = A.ravel(); s1, s2 = A.shape[1] * A.shape[2], A.shape[2]
    def f(x, y, z):
        u = (x - o[0]) / H; v = (y - o[1]) / H; w = (z - o[2]) / H
        if u < 0 or v < 0 or w < 0 or u > n[0] or v > n[1] or w > n[2]: return -1.0
        i = min(int(u), n[0] - 1); j = min(int(v), n[1] - 1); k = min(int(w), n[2] - 1); fu = u - i; fv = v - j; fw = w - k
        p = i * s1 + j * s2 + k
        c00 = flat[p] * (1 - fu) + flat[p + s1] * fu; c01 = flat[p + 1] * (1 - fu) + flat[p + s1 + 1] * fu
        c10 = flat[p + s2] * (1 - fu) + flat[p + s1 + s2] * fu; c11 = flat[p + s2 + 1] * (1 - fu) + flat[p + s1 + s2 + 1] * fu
        return (c00 * (1 - fv) + c10 * fv) * (1 - fw) + (c01 * (1 - fv) + c11 * fv) * fw
    bb = [o[0], o[1], o[2], o[0] + n[0] * H, o[1] + n[1] * H, o[2] + n[2] * H]
    return mf.Manifold.level_set(f, bb, edge).simplify(simp)
# protected features
import shapely.geometry as sg
from shapely import distance as sdist, points as spoints
W = g['W']; shp = g['shp']
s240, s280 = shp(W.slice(240.0)), shp(W.slice(280.0))
ref = sg.Polygon(max(getattr(s280, 'geoms', [s280]), key=lambda p: p.area).exterior)
s1p = ref.difference(s240).intersection(sg.Polygon([(-4.5, 17.0), (10.5, 37.0), (13.0, 39.5), (8.0, 41.5), (-6.0, 23.5), (-8.0, 20.5)]).buffer(1.0))
s1p = max(getattr(s1p, 'geoms', [s1p]), key=lambda p: p.area)
DS1_2 = np.asarray(sdist(s1p, spoints(P2)), np.float32).reshape(DD.shape)
DS1 = np.sqrt(DS1_2[:, :, None] ** 2 + (np.maximum(202.5 - Z3, 0) + np.maximum(Z3 - 272.0, 0)) ** 2)
DH = np.sqrt((T3 - 2.11) ** 2 + (Z3 - 186.6) ** 2) - 1.65
JAW = 12.0
outer = D3 > 5.5
front = sm((-8.0 - T3) / 4.0)                                                # the front edge: 6 mm wrap
t_end = np.where(outer, 34.0, 14.0)                                            # outer jaw ~48 mm, inner jaw 28 mm front-to-back
z0, z1 = 181.0, np.where(outer, 281.0, 250.0)                                  # outer jaw 100 mm tall, inner 69 mm
taper = sm((t_end - T3) / 12.0) * sm((Z3 - z0) / 10.0) * sm((z1 - Z3) / 10.0)  # thickness tapers over the last 10-12 mm
clear = sm((DS1 - 3.0) / 2.0) * sm((DH - 3.0) / 2.0)
TH = (6.0 * front + JAW * (1 - front) * (0.25 + 0.75 * taper)) * clear * sm((t_end + 2 - T3) / 2.0) * sm((Z3 - z0 + 2) / 2.0) * sm((z1 + 2 - Z3) / 2.0)
catchF = sbox((15.33 - 2.5, -4.0, 219.0), (40, 2.0, 235.0), 1.5)
F_off = smin(FW, -catchF, 3.0)
shell = np.minimum(F_off + TH, -F_off - 0.05)
shell = np.maximum(shell, np.minimum(-F_off - 0.05, catchF - 0.3))
kit = level(shell, (-24, -28, 178), (36, 40, 289), edge=0.75)
kit = max(kit.decompose(), key=lambda x: x.volume())
arm_w, info = g['build_arm'](44.0)
st = g['stadium'](12.0, 24.0); face = 15.33 + JAW - 1.0
cap = (g['prism_d'](st, face - 2.0, face - 0.4) + g['prism_d'](st, face - 0.4, face, scale_top=(11.2 / 12, 23.2 / 24))).translate([0, -6.3, C])
pocket = g['prism_d'](g['stadium'](12.6, 24.6), face - 2.4, face + 8.0).translate([0, -6.3, C])
wing_bare = tm2mf(load(os.path.join(V, 'mock5b', 'best_TS_wing_off.stl'))).transform(np.asarray(W2S[:3, :4], float))
car = max(((kit + tf(arm_w, W2S)) - pocket - wing_bare).decompose(), key=lambda x: x.volume())
for m, f in ((car, 'saddle_thick.stl'), (cap, 'saddle_thick_btn.stl')): mf2tm(tf(m, S2W)).export(f)
car_w = mf2tm(tf(car, S2W))
surf = (np.abs(FW) < 0.4) & (TH > 0.5)
rep['thick_saddle'] = dict(jaw_mm=JAW, outer_jaw_z=[181, 281], inner_jaw_z=[181, 250], outer_jaw_t_end=34.0, inner_jaw_t_end=14.0, front_wrap_mm=6.0,
                           taper='thickness tapers over the last 10-12 mm', min_clear_S1=round(float(DS1[surf].min()), 2), min_clear_hole186=round(float(DH[surf].min()), 2),
                           kit_x_max=round(float(car_w.bounds[1][0]), 1), clash_wing=round((car ^ wing_bare).volume(), 2),
                           clash_armrest=round((tm2mf(car_w) ^ tm2mf(armrest)).volume(), 1) if armrest.is_volume else 'n/a')
log('thick saddle', rep['thick_saddle'])

# ------------------------------------------------------------------ top-down sections (S frame, d' / t') for the drawings
def sec_S(t, z):
    t2 = t.copy(); t2.apply_transform(W2S); s = t2.section(plane_origin=[0, 0, z], plane_normal=[0, 0, 1])
    if s is None: return []
    return [np.round(e[:, :2], 2).tolist() for e in s.discrete if -40 < e[:, 0].mean() < 50 and -60 < e[:, 1].mean() < 60]
wing_cad = load(os.path.join(FIT, 'wing_L_GBG-SP-WG-L-r5.stl')); car_cad = load(os.path.join(FIT, 'GBG-SP-AR-L-L-LO-r5.stl'))
wclean = load(os.path.join(V, 'mock5b', 'best_TS_wing_off.stl'))
sections = dict(tongue=dict(z=230.0, wing=sec_S(wing_cad, 230.0), kit=sec_S(car_cad, 230.0)),
                saddle=dict(z=230.0, wing=sec_S(wclean, 230.0), kit=sec_S(car_w, 230.0)),
                s1=[np.round(np.asarray(s1p.exterior.coords), 2).tolist()], pad_S=[-78.9, -27.1])
json.dump(sections, open('sections.json', 'w'))
json.dump(rep, open('report.json', 'w'), indent=1)

# packing: pack_options.py (run after this and gen_prong.py)
log('done')
