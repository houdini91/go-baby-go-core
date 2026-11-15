# Simple 3D mock renderer: real chair STLs + primitive shapes (boxes, cylinders, superellipsoids, curved shells), flat-shaded with outlines.
# Painter's algorithm (sorted triangles), per-object colour and alpha. Chair coordinates in mm (-y = front, z up).
import struct, glob, os, numpy as np, matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.collections import PolyCollection

def load_stl(p):
    d = open(p, "rb").read(); n = struct.unpack("<I", d[80:84])[0]
    a = np.frombuffer(d[84:84 + 50 * n], dtype=np.dtype([("n", "<3f4"), ("v", "<9f4"), ("a", "<u2")]))
    return a["v"].reshape(-1, 3, 3).astype(float)

def box(c0, c1):
    x0, y0, z0 = c0; x1, y1, z1 = c1
    V = np.array([[x0, y0, z0], [x1, y0, z0], [x1, y1, z0], [x0, y1, z0], [x0, y0, z1], [x1, y0, z1], [x1, y1, z1], [x0, y1, z1]], float)
    F = [(0, 2, 1), (0, 3, 2), (4, 5, 6), (4, 6, 7), (0, 1, 5), (0, 5, 4), (1, 2, 6), (1, 6, 5), (2, 3, 7), (2, 7, 6), (3, 0, 4), (3, 4, 7)]
    return V[np.array(F)]

def superq(c, r, e=0.35, n=28, m=18):
    """superellipsoid centred c, radii r (rx, ry, rz), squareness e (1 = ellipsoid, 0.2 = rounded box)"""
    def f(w, p):
        return np.sign(np.cos(w)) * np.abs(np.cos(w)) ** p, np.sign(np.sin(w)) * np.abs(np.sin(w)) ** p
    T = []
    us = np.linspace(-np.pi / 2, np.pi / 2, m); vs = np.linspace(-np.pi, np.pi, n)
    P = np.zeros((m, n, 3))
    for i, u in enumerate(us):
        cu, su = f(u, e)
        for j, v in enumerate(vs):
            cv, sv = f(v, e)
            P[i, j] = [c[0] + r[0] * cu * cv, c[1] + r[1] * cu * sv, c[2] + r[2] * su]
    for i in range(m - 1):
        for j in range(n - 1):
            T += [[P[i, j], P[i + 1, j], P[i + 1, j + 1]], [P[i, j], P[i + 1, j + 1], P[i, j + 1]]]
    return np.array(T)

def cyl(a, b, r, n=24, caps=True):
    a = np.array(a, float); b = np.array(b, float); d = b - a; L = np.linalg.norm(d); d /= L
    t = np.cross(d, [0, 0, 1]) if abs(d[2]) < .9 else np.cross(d, [1, 0, 0]); t /= np.linalg.norm(t); s = np.cross(d, t)
    ang = np.linspace(0, 2 * np.pi, n + 1)
    ring = [r * (np.cos(q) * t + np.sin(q) * s) for q in ang]
    T = []
    for i in range(n):
        p0, p1 = ring[i], ring[i + 1]
        T += [[a + p0, b + p0, b + p1], [a + p0, b + p1, a + p1]]
        if caps:
            T += [[a, a + p1, a + p0], [b, b + p0, b + p1]]
    return np.array(T)

def cradle(cx, cy, cz, r_in, thick, h, a0=-105, a1=105, n=30, tilt=0.0):
    """curved C-shell behind a head: centre (cx,cy), opening toward -y, inner radius r_in, angular span a0..a1 deg (0 = straight back, +y)"""
    T = []
    angs = np.radians(np.linspace(a0, a1, n))
    def pt(rr, q, z):
        return np.array([cx + rr * np.sin(q), cy + rr * np.cos(q) + (z - cz) * tilt, z])
    z0, z1 = cz - h / 2, cz + h / 2
    for i in range(n - 1):
        q0, q1 = angs[i], angs[i + 1]
        for rr in (r_in, r_in + thick):
            T += [[pt(rr, q0, z0), pt(rr, q1, z0), pt(rr, q1, z1)], [pt(rr, q0, z0), pt(rr, q1, z1), pt(rr, q0, z1)]]
        for z in (z0, z1):
            T += [[pt(r_in, q0, z), pt(r_in + thick, q0, z), pt(r_in + thick, q1, z)], [pt(r_in, q0, z), pt(r_in + thick, q1, z), pt(r_in, q1, z)]]
    for q in (angs[0], angs[-1]):
        T += [[pt(r_in, q, z0), pt(r_in + thick, q, z0), pt(r_in + thick, q, z1)], [pt(r_in, q, z0), pt(r_in + thick, q, z1), pt(r_in, q, z1)]]
    return np.array(T)

class Scene:
    def __init__(self):
        self.objs = []
    def add(self, tris, color, alpha=1.0, edge=True):
        self.objs.append((np.asarray(tris, float), color, alpha, edge)); return self
    def render(self, path, az=-135, el=18, size=(6, 6), dpi=110, xlim=None, title=None, light=(0.4, -0.6, 0.7), focus=None, labels=()):
        a, e = np.radians(az), np.radians(el)
        fwd = np.array([np.cos(e) * np.cos(a), np.cos(e) * np.sin(a), np.sin(e)])   # from target toward camera
        right = np.cross([0, 0, 1], fwd); right /= np.linalg.norm(right); up = np.cross(fwd, right)
        L = np.array(light, float); L /= np.linalg.norm(L)
        polys, cols, depth, edges = [], [], [], []
        for tris, color, alpha, edge in self.objs:
            n = np.cross(tris[:, 1] - tris[:, 0], tris[:, 2] - tris[:, 0]); nl = np.linalg.norm(n, axis=1) + 1e-12; n /= nl[:, None]
            sh = 0.42 + 0.58 * np.abs(n @ L)
            P2 = np.stack([tris @ right, tris @ up], axis=-1)
            dz = (tris @ fwd).mean(axis=1)
            rgb = np.array(matplotlib.colors.to_rgb(color))
            for k in range(len(tris)):
                polys.append(P2[k]); cols.append((*np.clip(rgb * sh[k], 0, 1), alpha)); depth.append(dz[k]); edges.append(edge)
        order = np.argsort(depth)
        fig = plt.figure(figsize=size, dpi=dpi); ax = fig.add_axes([0, 0, 1, 1])
        ax.add_collection(PolyCollection([polys[i] for i in order], facecolors=[cols[i] for i in order], edgecolors=[(0, 0, 0, (0.10 if edges[i] else 0.0) * cols[i][3]) for i in order], linewidths=0.25))
        allp = np.concatenate(polys); ax.set_xlim(allp[:, 0].min() - 20, allp[:, 0].max() + 20); ax.set_ylim(allp[:, 1].min() - 20, allp[:, 1].max() + 20)
        if xlim: ax.set_xlim(*xlim[0]); ax.set_ylim(*xlim[1])
        if focus is not None:
            c = np.array(focus[0], float); r = focus[1]
            cx, cy = c @ right, c @ up
            ax.set_xlim(cx - r, cx + r); ax.set_ylim(cy - r * size[1] / size[0], cy + r * size[1] / size[0])
        for p3, txt in labels:
            p3 = np.array(p3, float)
            ax.annotate(txt, xy=(p3 @ right, p3 @ up), xytext=(12, 10), textcoords="offset points", fontsize=9, color="#111",
                        bbox=dict(boxstyle="round,pad=0.25", fc="white", ec="#888", lw=0.6, alpha=0.9), arrowprops=dict(arrowstyle="-", color="#555", lw=0.8))
        ax.set_aspect("equal"); ax.axis("off")
        if title:
            ax.text(0.02, 0.97, title, transform=ax.transAxes, fontsize=11, va="top", weight="bold", color="#222")
        fig.savefig(path, facecolor="white"); plt.close(fig)

def chair(scene, folder, color="#cdb89c"):
    for f in sorted(glob.glob(os.path.join(folder, "chair_*.stl"))):
        scene.add(load_stl(f), color, 1.0, edge=False)
    return scene

def kid(scene, big=True, gap=0.0, alpha=0.55):
    # seated child (NIST AnthroKids): big = 5 y P95, small = 2 y P5
    top, hh, hl, hb, sh, shb = (685, 193, 192, 147, 453, 303) if big else (517, 158, 163, 125, 327, 213)
    yb = 112 - gap                                   # back of the head at the backrest-top plane (minus forward gap)
    cz = top - hh / 2
    skin = "#e9b48f"
    scene.add(superq((0, yb - hl / 2, cz), (hb / 2, hl / 2, hh / 2), e=1.0, n=30, m=20), skin, alpha)
    scene.add(cyl((0, yb - hl * 0.42, cz - hh * 0.35), (0, yb - hl * 0.42, sh + 5), hb * 0.27), skin, alpha)
    scene.add(superq((0, yb - 72 - gap * 0.3, (sh + 30) / 2), (shb / 2, 72, (sh - 30) / 2 + 10), e=0.55, n=26, m=16), "#7aa6c9", alpha)   # torso, seat to shoulders
    scene.add(superq((0, yb - 200, 70), (shb / 2 - 20, 150, 45), e=0.6, n=22, m=12), "#5d6f8f", alpha)                             # thighs / lap
    return dict(cz=cz, yb=yb, hb=hb, hh=hh, hl=hl)
