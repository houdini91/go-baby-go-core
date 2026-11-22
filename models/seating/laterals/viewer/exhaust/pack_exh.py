# Pack the exhaust page's meshes into exhaust/geo.json + geodata.json (format of ../pack.py), thinned with manifold3d.
# Run: ../.venv/bin/python pack_exh.py   (after gen_exh3d.py)
import os, json, base64, struct, numpy as np, manifold3d as mf
HERE = os.path.dirname(os.path.abspath(__file__)); V = os.path.join(HERE, '..'); FIT = os.path.join(V, 'r5cad', 'fitted')
def stl(f):
    d = open(f, 'rb').read(); n = struct.unpack('<I', d[80:84])[0]
    a = np.frombuffer(d[84:84 + n * 50], dtype=np.dtype([('n', '<3f4'), ('v', '<9f4'), ('a', '<u2')]))['v'].reshape(-1, 3).astype(np.float64)
    return a * 1000 if a.max() < 5 else a
def raw(k):
    tris = [fc['vertices'] for body in json.load(open(os.path.join(V, 'raw', k + '.json'))) for face in body['faces'] for fc in face['facets']]
    return np.array(tris, dtype=np.float64).reshape(-1, 3) * 1000.0
def thin(a, tol):
    u, inv = np.unique(np.round(a * 1000).astype(np.int64), axis=0, return_inverse=True)
    m = mf.Manifold(mf.Mesh(vert_properties=(u / 1000.0).astype(np.float32), tri_verts=inv.reshape(-1, 3).astype(np.uint32)))
    if m.status().name != 'NoError' or m.is_empty(): return a
    q = m.simplify(tol).to_mesh(); return np.asarray(q.vert_properties)[:, :3][np.asarray(q.tri_verts)].reshape(-1, 3).astype(np.float64)
P = lambda *a: os.path.join(*a)
MESHES = [('armrest', 'raw:ps1_J_D', 0.06), ('depth', P(FIT, 'GBG-SP-DP-D0-T0-r5_L.stl'), 0.06), ('pad', P(FIT, 'GBG-SP-PD-r5_L.stl'), 0.06),
          ('ring', P(FIT, 'GBG-SP-RG-r5_L.stl'), 0.06), ('cover', P(FIT, 'GBG-SP-CV-r5_L.stl'), 0.06)]
for s in 'ABC': MESHES += [(f'exh_wing_{s}', f'exh_wing_{s}.stl', 0.02), (f'exh_kit_{s}', f'exh_kit_{s}.stl', 0.02), (f'exh_btn_{s}', f'exh_btn_{s}.stl', 0.0)]
for n in ('h2h_F_wing', 'h2h_F_kit', 'h2h_F_rods', 'h2h_F_btn', 'h2h_F_sleeve', 'h2h_FP_wing', 'h2h_FP_kit', 'h2h_FP_rods', 'h2h_S_wing', 'h2h_S_kit', 'h2h_S_btn'):
    MESHES.append((n, n + '.stl', 0.0 if n.endswith(('_btn', '_rods', '_sleeve')) else 0.02))
MESHES += [('x_wing', 'x_wing.stl', 0.02), ('x_kit', 'x_kit.stl', 0.02), ('x_btn', 'x_btn.stl', 0.0), ('x_rod', 'x_rod.stl', 0.0)]
buf = bytearray(); man = []
for name, f, tol in MESHES:
    if not f.startswith("raw:") and not os.path.exists(P(HERE, f) if not os.path.isabs(f) else f): print("skip", name); continue
    a = raw(f[4:]) if f.startswith('raw:') else stl(P(HERE, f) if not os.path.isabs(f) else f)
    if tol: a = thin(a, tol)
    u, inv = np.unique(np.round(a * 500).astype(np.int64), axis=0, return_inverse=True)
    pos = (u / 500.0).astype(np.float32); idx = inv.reshape(-1).astype(np.uint32)
    o = len(buf); buf += pos.tobytes(); o2 = len(buf); buf += idx.tobytes()
    man.append(dict(name=name, pos=[o, len(pos)], idx=[o2, len(idx)])); print(f'{name:14s} {len(idx) // 3:7d} tris')
json.dump(man, open(P(HERE, 'geo.json'), 'w')); json.dump({'b': base64.b64encode(bytes(buf)).decode()}, open(P(HERE, 'geodata.json'), 'w'))
print(round(os.path.getsize(P(HERE, 'geodata.json')) / 1e6, 2), 'MB')
