# Pack Onshape tessellatedfaces JSON (raw/*.json, metres) into site/geo.json + site/geodata.json (base64 float32 mm + uint32 index).
import json, base64, numpy as np, os
PARTS = [('wing', 'ps1_RiBH'), ('armrest', 'ps1_J_D'), ('back', 'ps1_RhDH'), ('base', 'ps1_J_H'),
         ('carrier', 'ps1_RUgD'), ('button', 'kit11_JzD'), ('shuttle', 'kit11_RhBD'), ('depth', 'kit11_RzCD'),
         ('pad', 'kit11_RcDD'), ('ring', 'kit11_RKED'), ('cushion', 'kit11_RTED'), ('blank1', 'ps1_RkkD'), ('blank2', 'ps1_RvkD')]
import struct
def stl(f):
    d = open(f, 'rb').read(); n = struct.unpack('<I', d[80:84])[0]
    a = np.frombuffer(d[84:84 + n * 50], dtype=np.dtype([('n', '<3f4'), ('v', '<9f4'), ('a', '<u2')]))
    v = a['v'].reshape(-1, 3).astype(np.float64); return v * 1000 if v.max() < 5 else v
EXTRA = [('wing0', '/home/mikey/research/onshape/gobabygo/chair_mesh/chair_left_wing.stl')]   # the ORIGINAL wing (before v1.1)
# Round 5 wing-side parts, real solids from r5mesh/gen_r5.py (left side, LOW)
EXTRA += [('wing_r5', 'r5mesh/wing_r5.stl'), ('nosearm_L', 'r5mesh/nose_arm_r5_L.stl'), ('nosearm_S', 'r5mesh/nose_arm_r5_S.stl'),
          ('button_r5', 'r5mesh/button_r5.stl'), ('sleeve_r5', 'r5mesh/sleeve_r5.stl'), ('cap_r5', 'r5mesh/cap_r5.stl')]
# Round 5b mockups (mock5b/gen_mock5b.py): saddle option, LOW/HIGH carriers, button drop-in, pad screw, protected wing features
EXTRA += [('m5b_' + n, f'mock5b/{n}.stl') for n in ('wing_sad', 'saddle_L', 'orig_block', 'carrier_H_L', 'flip_over', 'flip_gap', 'carrier_drop_L',
          'sleeve_split', 'foot_r5', 'pad_r5', 'snap_r5', 'nut_r5', 'screw_r5', 'foam_r5', 'feat_slits', 'feat_tear', 'feat_holes')]
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
