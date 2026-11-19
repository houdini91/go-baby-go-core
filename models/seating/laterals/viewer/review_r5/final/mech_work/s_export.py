exec(open('s_walls.py').read().split('rep = {}')[0])
s240 = WS.slice(240.0); s215 = WS.slice(215.2)
fill = (s240 + (s215 ^ W1) + (s215 ^ rect(8.0, 26.0, 11.0, 22.0))) ^ rect(-14, 34, 1.4, 26.0)
out = {}
for z in [227.0, 260.7]:
    sec = (WS.slice(z) + fill) ^ rect(-15, 40, -16, 45)
    out[str(z)] = dict(filled=[np.asarray(p).tolist() for p in sec.to_polygons()],
                       orig=[np.asarray(p).tolist() for p in (WS.slice(z) ^ rect(-15, 40, -16, 45)).to_polygons()])
json.dump(out, open('sections_poly.json', 'w'))
