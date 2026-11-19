# Arm sections square to the arm path (carrier meshes viewer/r5mesh/nose_arm_r5_{S,L}.stl, LOW, left; HIGH = z-mirror of the
# load), tenon root, with the carrier's print orientation (on the nose's outer face -> layer normal ~ d). Hand beam model.
import numpy as np, math, sys, json, trimesh
sys.path.insert(0, '../../../r5mesh')
from geom import *
import loads as L
from shapely.geometry import Polygon
from shapely.ops import unary_union
nL = normalize = lambda v: v / np.linalg.norm(v)
lean = LD * dirD + LT * dirT
N_LAYER = nL(np.cross(dirT, lean + np.array([0, 0, 1.0])))          # normal of the d' = const plane (bed)
def tsai_hill_X(sinth):
    s2 = sinth ** 2; c2 = 1 - s2; Xa, Xc, S = MAT['Xa'], MAT['Xc'], MAT['S']
    return 1 / math.sqrt(c2 ** 2 / Xa ** 2 + s2 ** 2 / Xc ** 2 + s2 * c2 * (1 / S ** 2 - 1 / Xa ** 2))
def section(mesh, p0, n):
    e2 = np.array([0, 0, 1.0]) - n[2] * n; e2 = nL(e2); e1 = np.cross(e2, n)
    s = mesh.section(plane_origin=p0, plane_normal=n)
    if s is None: return None
    polys = []
    for ent in s.entities:
        P = s.vertices[ent.points]; q = np.c_[(P - p0) @ e1, (P - p0) @ e2]
        if len(q) > 3: polys.append(Polygon(q).buffer(0))
    # keep the loop containing the origin region (the arm) + holes
    outer = [pg for pg in polys if pg.area > 50]
    if not outer: return None
    # even-odd combine
    shape = None
    for pg in sorted(polys, key=lambda g: -g.area):
        shape = pg if shape is None else (shape.difference(pg) if shape.contains(pg) else shape.union(pg) if not shape.intersects(pg) else shape.symmetric_difference(pg))
    # take the component nearest the path point
    comps = [shape] if shape.geom_type == 'Polygon' else list(shape.geoms)
    comp = min(comps, key=lambda g: g.distance(Polygon([(-1, -1), (1, -1), (1, 1), (-1, 1)])))
    return comp, e1, e2
def gprops(g, h=0.1):
    minx, miny, maxx, maxy = g.bounds
    xs = np.arange(minx, maxx, h) + h / 2; ys = np.arange(miny, maxy, h) + h / 2
    X, Y = np.meshgrid(xs, ys, indexing='ij')
    from shapely import contains_xy
    m = contains_xy(g, X, Y)
    A = m.sum() * h * h; cx = X[m].mean(); cy = Y[m].mean()
    Ixx = ((Y[m] - cy) ** 2).sum() * h * h; Iyy = ((X[m] - cx) ** 2).sum() * h * h; Ixy = ((X[m] - cx) * (Y[m] - cy)).sum() * h * h
    return dict(A=A, cx=cx, cy=cy, Ixx=Ixx, Iyy=Iyy, Ixy=Ixy, X=X[m], Y=Y[m], w=maxx - minx, hgt=maxy - miny)
LOADS = []
for cfg in CONFIGS:
    pts = L.pad_points(*cfg)
    for P, nm in ((300.0, 'lean 300'), (600.0, 'proof 600')):
        LOADS.append((nm, cfg, pts['centre'], np.array([P, 0, 0])))
    for k, c in enumerate(pts['corners']):
        for dv in ([1, 0, 0], [-1, 0, 0], [0, 1, 0], [0, -1, 0]):
            LOADS.append(('corner 900', cfg, c, 900.0 * np.array(dv, float)))
    for P in (300.0, 600.0, 900.0):
        LOADS.append((f'pull fwd {P:.0f}', cfg, pts['centre'], np.array([0, -P, 0])))
def mirror_z(p): q = np.array(p, float).copy(); q[2] = 2 * C - q[2]; return q
def check_section(mesh, arm, u, rodanchor):
    zp_low = zp_of(arm, 'LOW'); p = armPath(C, zp_low); p0, tn = armAt(p, u)
    r = section(mesh, p0, tn)
    if r is None: return None
    g, e1, e2 = r; G = gprops(g)
    sinth = abs(tn @ N_LAYER); X = tsai_hill_X(sinth)
    cen = p0 + G['cx'] * e1 + G['cy'] * e2
    # torsion (Saint-Venant, solid stadium): tau = T / (alpha b^2 h)
    b, h_ = sorted([G['w'], G['hgt']]); alpha = 0.208 + (0.246 - 0.208) * min((h_ / b - 1) / 1.0, 1)
    res = {}
    for nm, cfg, Pw, Fw in LOADS:
        if cfg[0] != arm: continue
        if cfg[1] == 'HIGH': Pw = mirror_z(Pw); Fw = Fw * np.array([1, 1, -1])
        Mv = np.cross(Pw - cen, Fw)            # moment of the pad load about the section centroid (part toward the pad)
        Nn = Fw @ tn; M1 = Mv @ e1; M2 = Mv @ e2; T = Mv @ tn; V = np.linalg.norm(Fw - Nn * tn)
        # sigma = N/A - M1-term ... unsymmetric bending in (e1, e2): sigma = N/A + a*x + b*y with [[Iyy,Ixy],[Ixy,Ixx]][a,b] = [M2? ...]
        # M about e1 bends with y (e2): sigma = M1*y/Ixx ; M about e2 bends with x: sigma = -M2*x/Iyy (principal-axes approx with Ixy)
        a_, b_ = np.linalg.solve([[G['Iyy'], G['Ixy']], [G['Ixy'], G['Ixx']]], [-M2, M1])
        sig = Nn / G['A'] + a_ * (G['X'] - G['cx']) + b_ * (G['Y'] - G['cy'])
        smax = np.abs(sig).max()
        # carbon rod (bond-limited): relieves the PETG by at most F_cap * lever
        if rodanchor > 0:
            Fcap = math.pi * 6.0 * rodanchor * 4.0; lever = 9.0
            smax = max(smax - Fcap * lever / (G['Ixx'] / 14.0), 0.6 * smax)
        tau = abs(T) / (alpha * b * b * h_) + 1.5 * V / G['A']
        kt = 1.45 if u <= 1.0 else 1.0
        FI = math.sqrt((kt * smax / X) ** 2 + (tau / MAT['S']) ** 2)
        key = nm
        if key not in res or 1 / FI < res[key]['SF']:
            res[key] = dict(SF=1 / FI, sig=kt * smax, X=X, tau=tau, cfg=cfg, sinth=sinth, A=G['A'])
    return res, G, sinth
if __name__ == '__main__':
    out = {}
    for arm in ('S', 'L'):
        mesh = trimesh.load(f'../../../r5mesh/nose_arm_r5_{arm}.stl')
        p = armPath(C, zp_of(arm, 'LOW'))
        rod_u_end = 7.2
        print(f'\n== arm {arm}: path length {p["L"]:.1f}')
        for u in [0.3, 2, 4, 6, 8, 10, 12, 14, 16, 20, 25, 30, 35, 40, 45, 50, 55, p['L'] - 0.5]:
            r = check_section(mesh, arm, u, max(rod_u_end - u, 0))
            if r is None: print(f'u {u:5.1f} no section'); continue
            res, G, sinth = r
            out[f'{arm} {u:.1f}'] = {k: {kk: (vv if not isinstance(vv, tuple) else list(vv)) for kk, vv in v.items()} for k, v in res.items()}
            line = f'u {u:5.1f} A {G["A"]:5.0f} sin(th) {sinth:4.2f} X {res["lean 300"]["X"]:4.1f} | ' + ' | '.join(f'{k} SF {v["SF"]:4.2f} (sig {v["sig"]:5.1f}, tau {v["tau"]:4.1f})' for k, v in res.items() if k in ('lean 300', 'proof 600', 'corner 900', 'pull fwd 300'))
            print(line)
    json.dump(out, open('arm_out.json', 'w'), indent=0, default=str)
