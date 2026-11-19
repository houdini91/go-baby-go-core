# Pack Onshape tessellatedfaces JSON (raw/*.json, metres) into site/geo.json + site/geodata.json (base64 float32 mm + uint32 index).
import json, base64, numpy as np, os
# Everything is the round-5 build from Onshape (r5cad/fitted, the wing repaired to a watertight solid by options/wingrepair.py) plus
# a few helpers from options/gen_main.py and the pad-screw hardware mocks. The old chair mount and the v1.1 kit are not packed.
PARTS = [('armrest', 'ps1_J_D')]
import struct
def stl(f):
    d = open(f, 'rb').read(); n = struct.unpack('<I', d[80:84])[0]
    a = np.frombuffer(d[84:84 + n * 50], dtype=np.dtype([('n', '<3f4'), ('v', '<9f4'), ('a', '<u2')]))
    v = a['v'].reshape(-1, 3).astype(np.float64); return v * 1000 if v.max() < 5 else v
F = 'r5cad/fitted/'
EXTRA = [('wing', 'mock5b/main_wing.stl'), ('carL', F + 'GBG-SP-AR-L-L-LO-r5.stl'), ('btn', F + 'GBG-SP-BT-L-r5.stl'), ('sleeve', F + 'GBG-SP-TS-r5_L.stl'),
         ('depth', F + 'GBG-SP-DP-D0-T0-r5_L.stl'), ('pad', F + 'GBG-SP-PD-r5_L.stl'), ('ring', F + 'GBG-SP-RG-r5_L.stl'), ('foam', F + 'GBG-SP-FM-20-r5_L.stl'),
         ('cover', F + 'GBG-SP-CV-r5_L.stl'), ('carH', 'mock5b/main_carH.stl'), ('flip_over', 'mock5b/main_flip_over.stl'), ('flip_gap', 'mock5b/main_flip_gap.stl'),
         ('cap', 'mock5b/main_cap.stl')]
EXTRA += [(n, f'options/{n}.stl') for n in ('hi_carrier', 'hi_button', 'hi_sleeve', 'hi_depth', 'hi_pad', 'hi_ring', 'hi_cover')]
EXTRA += [('m5b_' + n, f'mock5b/{n}.stl') for n in ('nut_r5', 'screw_r5', 'feat_slits', 'feat_tear', 'feat_holes')]
buf = bytearray(); man = []
for name, f in PARTS + EXTRA:
    if f.endswith('.stl'): a = stl(f)
    else:
        tris = [fc['vertices'] for body in json.load(open(f'raw/{f}.json')) for face in body['faces'] for fc in face['facets']]
        a = np.array(tris, dtype=np.float64).reshape(-1, 3) * 1000.0
    u, inv = np.unique(np.round(a * 500).astype(np.int64), axis=0, return_inverse=True)
    pos = (u / 500.0).astype(np.float32); idx = inv.reshape(-1).astype(np.uint32)
    o = len(buf); buf += pos.tobytes(); o2 = len(buf); buf += idx.tobytes()
    man.append(dict(name=name, pos=[o, len(pos)], idx=[o2, len(idx)]))
    print(f'{name:8s} {len(idx)//3:6d} tris  min {a.min(0).round(1)} max {a.max(0).round(1)}')
json.dump(man, open('site/geo.json', 'w'))
json.dump({'b': base64.b64encode(bytes(buf)).decode()}, open('site/geodata.json', 'w'))
print(round(len(buf) / 1e6, 2), 'MB')
