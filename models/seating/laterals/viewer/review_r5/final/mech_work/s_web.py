exec(open('s_walls.py').read().split('rep = {}')[0])
s240 = WS.slice(240.0); s215 = WS.slice(215.2)
fill = (s240 + (s215 ^ W1) + (s215 ^ rect(8.0, 26.0, 11.0, 22.0))) ^ rect(-14, 34, 1.4, 26.0)
for z in [215.0, 227.0, 246.5]:
    sec = (WS.slice(z) + fill) ^ rect(-40, 60, -40, 80)
    S = segs(sec); P = densify(SLOT, 100); back = P[P[:, 1] > 9.0]
    a = S[:, :2]; b = S[:, 2:]; ab = b - a; L2 = (ab**2).sum(1) + 1e-12
    best = []
    for p in back:
        t = np.clip(((p - a) * ab).sum(1) / L2, 0, 1); q = a + ab * t[:, None]; dd = np.sqrt(((q - p)**2).sum(1)); i = dd.argmin()
        best.append((dd[i], p, q[i]))
    best.sort(key=lambda x: x[0])
    for d, p, q in best[:3]: print(z, 'web %.2f slot pt d %.2f t %.2f -> boundary d %.2f t %.2f' % (d, p[0], p[1], q[0], q[1]))
print('--- voids near the slot (box d -10..30, t 0..45) in the filled section, and in the original section')
for z in [215.0, 227.0, 240.0, 246.5]:
    sec = (WS.slice(z) + fill) ^ rect(-40, 60, -40, 80)
    for name, cs in [('filled', sec), ('orig', WS.slice(z))]:
        v = rect(-10, 30, 0, 45) - cs
        for c in v.decompose():
            if c.area() < 0.5: continue
            bb = c.bounds(); print(z, name, 'void area %.1f d %.1f..%.1f t %.1f..%.1f' % (c.area(), bb[0], bb[2], bb[1], bb[3]))
