# Side-pad v1.1 strength review r4: slice the real carrier mesh (Onshape tessellation, ps1 = P1 LOW, D0)
# square to the arm path, then a hand beam model with / without carbon.  Scratch only.
import json, numpy as np, math, sys
RAW = '/home/mikey/go-baby-go-core/models/seating/laterals/viewer/raw/'
def load(f):
    d = json.load(open(RAW + f + '.json'))
    return np.array([fc['vertices'] for b in d for fa in b['faces'] for fc in fa['facets']], float) * 1000.0

# ---- FS constants (gbg_sidepad_v1.fs) ----
P0 = np.array([131.3, 80.7]); AXH = np.array([0.79577626, 0.60559074]); TH = np.array([-0.6083053, 0.79370313])
D_SHIFT = 2.0; Z0 = 227.8; SHEAR = np.array([0.8 / 32.9, 2.7 / 32.9]); ZREF = 244.25
TG_D = [1.4, 17.4]; TAPER_T = -14.0; YEND = 35.0; ARM_W = 18; TAPER_W = 16
YAX = YEND + (ARM_W - TAPER_W) / 2
CF = [[5.5, -7.0, 2.15, -29.5, 4.0], [9.5, -11.0, 1.65, -31.0, 3.0]]; CF_T1 = 28.5
TPIN = [[3.5, 7.4], [-3.5, 7.4], [3.5, -7.4], [-3.5, -7.4]]; TPIN_IN = 8.0; TEN_L = 13.0
c = 227.0; zp = c - 47.5
def Q(d, t, z):
    dd = d + D_SHIFT
    xy = P0 + dd * AXH + t * TH + (ZREF - Z0) * SHEAR
    return np.array([xy[0], xy[1], z])
dirD = np.array([*AXH, 0]) / np.linalg.norm(AXH); dirT = np.array([*TH, 0]) / np.linalg.norm(TH)

def armPath(c, zp):
    st = Q((TG_D[0] + TG_D[1]) / 2, TAPER_T, c)
    h0 = -dirT; th0 = math.degrees(math.atan2(h0[1], h0[0]))
    R = (YEND - st[1]) / (math.sin(math.radians(th0 - 90)) - 1)
    C = [st[0] + R * math.cos(math.radians(th0 - 90)), st[1] + R * math.sin(math.radians(th0 - 90))]
    L = R * math.radians(th0 + 180)
    dz = zp - c; thv = 2 * math.degrees(math.atan(abs(dz) / L)); Rv = L / (2 * math.sin(math.radians(thv)))
    return dict(st=st, th0=th0, R=R, C=C, L=L, dz=dz, Rv=Rv, c=c)
def armAt(p, u):
    th = p['th0'] - math.degrees(u / p['R'])
    x = p['C'][0] + p['R'] * math.cos(math.radians(th + 90)); y = p['C'][1] + p['R'] * math.sin(math.radians(th + 90))
    if u <= p['L'] / 2:
        ph = math.asin(min(u / p['Rv'], 1)); zz = p['Rv'] * (1 - math.cos(ph)); sl = math.tan(ph)
    else:
        ph = math.asin(min((p['L'] - u) / p['Rv'], 1)); zz = abs(p['dz']) - p['Rv'] * (1 - math.cos(ph)); sl = math.tan(ph)
    s = -1 if p['dz'] < 0 else 1
    t = np.array([math.cos(math.radians(th)), math.sin(math.radians(th)), s * sl]); t /= np.linalg.norm(t)
    return np.array([x, y, p['c'] + s * zz]), t

# ---- plane slicing ----
def slice_mesh(T, p0, n, e1, e2):
    d = (T - p0) @ n                       # (ntri, 3)
    segs = []
    for tri, dd in zip(T, d):
        pts = []
        for i in range(3):
            a, b = i, (i + 1) % 3
            if (dd[a] > 0) != (dd[b] > 0):
                s = dd[a] / (dd[a] - dd[b]); pts.append(tri[a] + s * (tri[b] - tri[a]))
        if len(pts) == 2: segs.append(pts)
    S = np.array(segs)
    if len(S) == 0: return []
    S2 = np.stack([(S - p0) @ e1, (S - p0) @ e2], -1)   # (nseg, 2, 2)
    # chain segments into loops
    key = lambda q: (round(q[0] * 200), round(q[1] * 200))
    adj = {}
    for i, s in enumerate(S2):
        for j in (0, 1): adj.setdefault(key(s[j]), []).append((i, j))
    used = np.zeros(len(S2), bool); loops = []
    for i0 in range(len(S2)):
        if used[i0]: continue
        used[i0] = True; loop = [S2[i0][0], S2[i0][1]]; cur = key(S2[i0][1]); start = key(S2[i0][0])
        while cur != start:
            nxt = [(i, j) for (i, j) in adj.get(cur, []) if not used[i]]
            if not nxt: break
            i, j = nxt[0]; used[i] = True; q = S2[i][1 - j]; loop.append(q); cur = key(q)
        if len(loop) > 3: loops.append(np.array(loop[:-1]))
    return loops

def props(P):        # signed polygon area, first and second moments (about origin)
    x, y = P[:, 0], P[:, 1]; x1, y1 = np.roll(x, -1), np.roll(y, -1); cr = x * y1 - x1 * y
    A = cr.sum() / 2; Sx = ((x + x1) * cr).sum() / 6; Sy = ((y + y1) * cr).sum() / 6
    Iyy = ((x * x + x * x1 + x1 * x1) * cr).sum() / 12     # int x^2
    Ixx = ((y * y + y * y1 + y1 * y1) * cr).sum() / 12     # int y^2
    Ixy = ((x * y1 + 2 * x * y + 2 * x1 * y1 + x1 * y) * cr).sum() / 24
    return np.array([A, Sx, Sy, Iyy, Ixx, Ixy])
def inside(pt, P):
    x, y = pt; xs, ys = P[:, 0], P[:, 1]; xs1, ys1 = np.roll(xs, -1), np.roll(ys, -1)
    m = ((ys > y) != (ys1 > y)) & (x < (xs1 - xs) * (y - ys) / (ys1 - ys + 1e-30) + xs)
    return m.sum() % 2 == 1

def section(T, p0, n, e1, e2):
    loops = slice_mesh(T, p0, n, e1, e2)
    out = []
    for L in loops:
        pr = props(L)
        depth = sum(inside(L[0], M) for M in loops if M is not L)
        sign = 1 if depth % 2 == 0 else -1
        if np.sign(pr[0]) != sign: pr = -pr
        cen = L.mean(0); r_eq = math.sqrt(abs(pr[0]) / math.pi)
        out.append(dict(P=L, pr=pr, solid=sign > 0, cen=pr[1:3] / pr[0], area=abs(pr[0]), r_eq=r_eq, depth=depth))
    return out
