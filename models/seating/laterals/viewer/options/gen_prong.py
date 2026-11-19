# Option 4, "three-prong plug" (mount_research.md O1 + O3), mocked on the r5cad build (viewer/r5cad/fitted, left, long arm LOW):
#   the solid tongue as built, plus two solid Ø8 carbon rods fixed in a 69 mm nose, 24 mm above and below the tongue centre,
#   sliding front-to-back into Ø8.2 blind bores in the blade; four Ø2 carbon rods in the tongue's corners running into the arm.
#   The wing's notch grows to the 69 mm nose; the three openings get flared, radiused mouths ("exhaust tip").
# Bore depth is set by clearance: >= 3 mm of blade to strap slit S1 and to the blade faces (measured here, written to prong.json).
# Run: ../.venv/bin/python gen_prong.py   (~1 min; then re-run the pack step: ../.venv/bin/python gen_options.py --pack-only)
import os, sys, json, numpy as np, trimesh, manifold3d as mf, shapely.geometry as sg
HERE = os.path.dirname(os.path.abspath(__file__)); V = os.path.join(HERE, '..')
sys.path.insert(0, os.path.join(V, 'mock5b')); sys.path.insert(0, os.path.join(V, 'r5mesh'))
from common import load_r5
g = load_r5(); os.chdir(HERE)
W2S, S2W, tf, box, shp = g['W2S'], g['S2W'], g['tf'], g['box'], g['shp']
T_FRONT, SEAM = g['T_FRONT'], g['SEAM']
def tm2mf(t): t = t.copy(); t.merge_vertices(digits_vertex=3); t.update_faces(t.nondegenerate_faces()); t.update_faces(t.unique_faces()); return mf.Manifold(mf.Mesh(vert_properties=np.asarray(t.vertices, np.float32), tri_verts=np.asarray(t.faces, np.uint32)))
def save(m, f): q = tf(m, S2W).to_mesh(); trimesh.Trimesh(np.asarray(q.vert_properties)[:, :3], np.asarray(q.tri_verts), process=False).export(f)
FIT = os.path.join(V, 'r5cad', 'fitted')
# the r5cad wing STL is not watertight, so rebuild it the way the FS does: original wing + the old pocket closed with the blade's
# own section (z 207-272); the tongue slot and catch groove are then cut by the as-built carrier and button themselves
wing = g['W'] + g['prism_z'](g['env'] ^ g['rect'](-14, 34, T_FRONT + 0.02, 26.0), 207.0, 272.0)
car = tf(tm2mf(trimesh.load(os.path.join(FIT, 'GBG-SP-AR-L-L-LO-r5.stl'))), W2S)
tong = car ^ box(-30, 40, 3.0, 40, 200, 260); tb = tong.bounding_box()
ZT = (tb[2] + tb[5]) / 2                                        # tongue centre height
TOP = tb[4]                                                     # tongue tip t'
Z_UP, Z_LO = ZT + 24.0, ZT - 24.0
NZ = (ZT - 34.5, ZT + 34.5)                                     # 69 mm nose
R_ROD, R_BORE = 4.0, 4.1
# protected features at the rod heights (S1 is a vertical slit z 202.5-272: both rods are inside its height)
W = g['W']; s240, s280 = shp(W.slice(240.0)), shp(W.slice(280.0))
ref = sg.Polygon(max(getattr(s280, 'geoms', [s280]), key=lambda p: p.area).exterior)
s1 = ref.difference(s240).intersection(sg.Polygon([(-4.5, 17.0), (10.5, 37.0), (13.0, 39.5), (8.0, 41.5), (-6.0, 23.5), (-8.0, 20.5)]).buffer(1.0))
s1 = max(getattr(s1, 'geoms', [s1]), key=lambda p: p.area)
M5 = sg.box(-5.8, -6.0, 0.0, 7.0)                               # the filled upper M5 clamp-bolt hole (z 254-266), d' x t'
rep = dict(tongue_S=[round(x, 2) for x in tb], tongue_centre_z=round(ZT, 2), rods_z=[round(Z_LO, 2), round(Z_UP, 2)], nose_z=[round(NZ[0], 2), round(NZ[1], 2)])
rods, bores, flares = [], [], []
for zr, name in ((Z_LO, 'lower'), (Z_UP, 'upper')):
    sec = shp(wing.slice(zr)); blade = [p for p in getattr(sec, 'geoms', [sec]) if p.contains(sg.Point(5.0, 8.0))][0]   # the tip piece (S1 cuts it off)
    # bore axis d': middle of the blade between t' 2 and 20 at this height
    xs = [np.asarray(blade.intersection(sg.LineString([(-40, t), (50, t)])).bounds)[[0, 2]] for t in (4.0, 10.0, 16.0)]
    lo, hi = max(x[0] for x in xs), min(x[1] for x in xs); dr = (lo + hi) / 2
    # deepest t' with >= 3 mm to S1 and >= 3 mm to the faces (2D, at the rod height; the slit is vertical)
    end = 2.0
    for t in np.arange(4.0, 45.0, 0.25):
        cyl = sg.LineString([(dr, 2.0), (dr, t)]).buffer(R_BORE, cap_style=1)
        if cyl.distance(s1) < 3.0 or not blade.buffer(-3.0).contains(cyl.difference(sg.box(-40, -40, 50, 2.0))): break
        end = t
    depth = end - 2.0
    cyl2 = sg.LineString([(dr, 2.0), (dr, end)]).buffer(R_BORE, cap_style=1)
    walls = blade.exterior.distance(sg.Point(dr, (2 + end) / 2)) - R_BORE
    rep[name] = dict(z=round(zr, 2), axis_d=round(dr, 2), bore_t=[2.0, round(end, 2)], bore_depth=round(depth, 1), wall_to_faces=round(walls, 2),
                     clear_to_S1=round(cyl2.distance(s1), 2), clear_to_filled_M5=(round(cyl2.distance(M5), 2) if 254 - 6 <= zr <= 266 + 6 else 'not at this height'),
                     blade_d=[round(lo, 2), round(hi, 2)])
    rod = mf.Manifold.cylinder(end - 0.5 - (T_FRONT + 3.0), R_ROD, R_ROD, 40).rotate([-90, 0, 0]).translate([dr, T_FRONT + 3.0, zr])
    tip = mf.Manifold.sphere(R_ROD, 40).scale([1, 0.35, 1]).translate([dr, end - 0.5, zr])
    rods.append(rod + tip)
    bores.append(mf.Manifold.cylinder(end - 1.0, R_BORE, R_BORE, 40).rotate([-90, 0, 0]).translate([dr, 1.0, zr]))
    flares.append(mf.Manifold.cylinder(3.0, R_BORE + 2.5, R_BORE, 48).rotate([-90, 0, 0]).translate([dr, 1.0, zr]))   # flared mouth
# wing: notch grown to the 69 mm nose, bores with flared mouths
notch = box(-40, 50, -40, 2.0, NZ[0], NZ[1])
btn = tf(tm2mf(trimesh.load(os.path.join(FIT, 'GBG-SP-BT-L-r5.stl'))), W2S)
wing4 = wing - notch
for b_, f_ in zip(bores, flares): wing4 = wing4 - b_ - f_
wing4 = max(wing4.decompose(), key=lambda m: m.volume())
# carrier: the as-built carrier plus nose extensions above and below (the wing's own front edge), rods and Ø2 spine rods
nose_ext = (wing ^ box(-40, 50, -40, 2.0 - SEAM, NZ[0] + SEAM, NZ[1] - SEAM)) - box(-40, 50, -40, 2.0, 207.0, 247.0)
car4 = car + nose_ext
spine = []
for dz in (-13.5, 13.5):
    for dd in (tb[0] + 1.5 + 1.0, tb[3] - 1.5 - 1.0):
        spine.append(mf.Manifold.cylinder(TOP - 2.0 - (T_FRONT - 26.0), 1.0, 1.0, 16).rotate([-90, 0, 0]).translate([dd, T_FRONT - 26.0, ZT + dz]))
rods_m = mf.Manifold.batch_boolean(rods + spine, mf.OpType.Add)
car4 = car4 - rods_m
wing4 = max((wing4 - car4 - rods_m - btn - btn.translate([-4.0, 0, 0])).decompose(), key=lambda m: m.volume())
save(wing4, 'prong_wing.stl'); save(car4, 'prong_carrier.stl'); save(rods_m, 'prong_rods.stl')
rep['clash_carrier_wing'] = round((car4 ^ wing4).volume(), 2); rep['clash_rods_wing'] = round((rods_m ^ wing4).volume(), 2)
# top-down sections for the drawing: at the tongue (z centre) and at the lower rod
def sec_S(m, z): return [np.round(np.asarray(p), 2).tolist() for p in m.slice(z).to_polygons() if len(p) > 2]
win = box(-30, 40, -40, 45, 150, 300)
json.dump(dict(tongue=dict(z=ZT, wing=sec_S(wing4 ^ win, ZT), kit=sec_S(car4 ^ win, ZT), rods=sec_S(rods_m, ZT + 13.5)),
               rod=dict(z=Z_LO, wing=sec_S(wing4 ^ win, Z_LO), kit=sec_S(car4 ^ win, Z_LO), rods=sec_S(rods_m, Z_LO)),
               s1=[np.round(np.asarray(s1.exterior.coords), 2).tolist()]), open('prong_sections.json', 'w'))
json.dump(rep, open('prong.json', 'w'), indent=1)
print(json.dumps(rep, indent=1))
