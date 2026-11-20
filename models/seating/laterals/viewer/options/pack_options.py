# Pack the options page's meshes into options/geo.json + options/geodata.json (format of ../pack.py). Run after gen_options.py
# and gen_prong.py:  ../.venv/bin/python pack_options.py   (manifold3d thins the meshes; plain python3 packs them unthinned)
import json, base64, os, struct, numpy as np
HERE = os.path.dirname(os.path.abspath(__file__)); V = os.path.join(HERE, '..'); FIT = os.path.join(V, 'r5cad', 'fitted'); MB = os.path.join(V, 'mock5b')
def stl(f):
    d = open(f, 'rb').read()
    if d[:5] == b'solid' and b'facet' in d[:400]:
        import trimesh; t = trimesh.load(f); a = np.asarray(t.vertices)[np.asarray(t.faces)].reshape(-1, 3)
    else:
        n = struct.unpack('<I', d[80:84])[0]
        a = np.frombuffer(d[84:84 + n * 50], dtype=np.dtype([('n', '<3f4'), ('v', '<9f4'), ('a', '<u2')]))['v'].reshape(-1, 3).astype(np.float64)
    return a * 1000 if a.max() < 5 else a
def raw(k):
    tris = [fc['vertices'] for body in json.load(open(os.path.join(V, 'raw', k + '.json'))) for face in body['faces'] for fc in face['facets']]
    return np.array(tris, dtype=np.float64).reshape(-1, 3) * 1000.0
P = lambda *a: os.path.join(*a)
MESHES = [('armrest', 'raw:ps1_J_D'),
          ('wing_cad', 'wing_cad_fixed.stl'), ('car_cad', P(FIT, 'GBG-SP-AR-L-L-LO-r5.stl')), ('btn_cad', P(FIT, 'GBG-SP-BT-L-r5.stl')),
          ('sleeve_cad', P(FIT, 'GBG-SP-TS-r5_L.stl')), ('depth', P(FIT, 'GBG-SP-DP-D0-T0-r5_L.stl')), ('pad', P(FIT, 'GBG-SP-PD-r5_L.stl')),
          ('ring', P(FIT, 'GBG-SP-RG-r5_L.stl')), ('cover', P(FIT, 'GBG-SP-CV-r5_L.stl'))] + \
         [(n, n + '.stl') for n in ('prong_wing', 'prong_carrier', 'prong_rods', 'tall_wing', 'tall_kit', 'tall_rods', 'tall_btn', 'tall_sleeve', 'tallp_wing', 'tallp_kit', 'tallp_rods')] + \
         [('feat_slits', P(MB, 'feat_slits.stl')), ('feat_tear', P(MB, 'feat_tear.stl')), ('feat_holes', P(MB, 'feat_holes.stl'))]
def thin(a, tol=0.06):
    # simplify with manifold3d when the mesh is a valid solid (keeps the page small); otherwise keep it as it is
    try:
        import manifold3d as mf
    except ImportError:
        return a
    u, inv = np.unique(np.round(a * 1000).astype(np.int64), axis=0, return_inverse=True)
    m = mf.Manifold(mf.Mesh(vert_properties=(u / 1000.0).astype(np.float32), tri_verts=inv.reshape(-1, 3).astype(np.uint32)))
    if m.status().name != 'NoError' or m.is_empty(): return a
    q = m.simplify(tol).to_mesh(); return np.asarray(q.vert_properties)[:, :3][np.asarray(q.tri_verts)].reshape(-1, 3).astype(np.float64)
buf = bytearray(); man = []
for name, f in MESHES:
    if not f.startswith('raw:') and not os.path.exists(f): print('MISSING', name, f); continue
    a = raw(f[4:]) if f.startswith('raw:') else stl(f)
    a = thin(a, 0.02 if name.startswith(('prong_wing', 'prong_carrier', 'tall_wing', 'tall_kit', 'tallp_wing', 'tallp_kit')) else 0.06)
    u, inv = np.unique(np.round(a * 500).astype(np.int64), axis=0, return_inverse=True)
    pos = (u / 500.0).astype(np.float32); idx = inv.reshape(-1).astype(np.uint32)
    o = len(buf); buf += pos.tobytes(); o2 = len(buf); buf += idx.tobytes()
    man.append(dict(name=name, pos=[o, len(pos)], idx=[o2, len(idx)])); print(f'{name:16s} {len(idx) // 3:7d} tris')
json.dump(man, open(P(HERE, 'geo.json'), 'w')); json.dump({'b': base64.b64encode(bytes(buf)).decode()}, open(P(HERE, 'geodata.json'), 'w'))
print(round(os.path.getsize(P(HERE, 'geodata.json')) / 1e6, 2), 'MB')
