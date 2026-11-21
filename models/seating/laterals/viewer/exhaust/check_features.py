# Check that the protected wing features survive in each generated wing: strap slit S1 stays open (no solid inside its
# outline at three heights) and the z 186 hole stays open (its centre point is outside the solid). Run after the generators.
import os, sys, json, numpy as np
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'options'))
from lib5 import *
import trimesh, shapely.geometry as sg
os.chdir(os.path.dirname(os.path.abspath(__file__)))
s1 = sg.Polygon(json.load(open('../options/sections.json'))['s1'][0]).buffer(-0.3)
out = {}
for f in sys.argv[1:]:
    w = tf(to_mf(trimesh.load(f)), W2S); r = {}
    for z in (210.0, 240.0, 265.0):
        sol = sg.MultiPolygon([sg.Polygon(p) for p in w.slice(z).to_polygons() if len(p) > 2]).buffer(0)
        r[f'S1 solid mm2 at z {z:.0f}'] = round(sol.intersection(s1).area, 2)
    sl = [sg.Polygon(p) for p in w.slice(186.6).to_polygons() if len(p) > 2]
    r['z186 hole open'] = not any(p.buffer(0).contains(sg.Point(5.0, 2.11)) for p in sl) or None
    out[f] = r; print(f, r)
json.dump(out, open('features_check.json', 'w'), indent=1)
