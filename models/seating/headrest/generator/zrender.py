# Small z-buffer renderer (numba) for concept images: orthographic camera, smooth shading, outlines, one transparent layer.
import numpy as np, numba, matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib import colors as mcolors

@numba.njit(cache=True)
def _raster(P, D, N, objid, W, H, zb, nb, ib, mask_alpha_obj, only_alpha):
    for t in range(P.shape[0]):
        o = objid[t]
        if only_alpha and not mask_alpha_obj[o]:
            continue
        if (not only_alpha) and mask_alpha_obj[o]:
            continue
        x0, y0 = P[t, 0, 0], P[t, 0, 1]; x1, y1 = P[t, 1, 0], P[t, 1, 1]; x2, y2 = P[t, 2, 0], P[t, 2, 1]
        area = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0)
        if abs(area) < 1e-12:
            continue
        xmin = max(int(min(x0, x1, x2)), 0); xmax = min(int(max(x0, x1, x2)) + 1, W - 1)
        ymin = max(int(min(y0, y1, y2)), 0); ymax = min(int(max(y0, y1, y2)) + 1, H - 1)
        for py in range(ymin, ymax + 1):
            for px in range(xmin, xmax + 1):
                cx = px + 0.5; cy = py + 0.5
                w0 = ((x1 - cx) * (y2 - cy) - (x2 - cx) * (y1 - cy)) / area
                w1 = ((x2 - cx) * (y0 - cy) - (x0 - cx) * (y2 - cy)) / area
                w2 = 1.0 - w0 - w1
                if w0 < -1e-6 or w1 < -1e-6 or w2 < -1e-6:
                    continue
                d = w0 * D[t, 0] + w1 * D[t, 1] + w2 * D[t, 2]
                if d > zb[py, px]:
                    zb[py, px] = d
                    for c in range(3):
                        nb[py, px, c] = w0 * N[t, 0, c] + w1 * N[t, 1, c] + w2 * N[t, 2, c]
                    ib[py, px] = o

@numba.njit(cache=True)
def _raster_alpha(P, D, N, objid, W, H, zop, zb, nb, ib, mask_alpha_obj):
    # nearest transparent surface in front of the opaque depth
    for t in range(P.shape[0]):
        o = objid[t]
        if not mask_alpha_obj[o]:
            continue
        x0, y0 = P[t, 0, 0], P[t, 0, 1]; x1, y1 = P[t, 1, 0], P[t, 1, 1]; x2, y2 = P[t, 2, 0], P[t, 2, 1]
        area = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0)
        if abs(area) < 1e-12:
            continue
        xmin = max(int(min(x0, x1, x2)), 0); xmax = min(int(max(x0, x1, x2)) + 1, W - 1)
        ymin = max(int(min(y0, y1, y2)), 0); ymax = min(int(max(y0, y1, y2)) + 1, H - 1)
        for py in range(ymin, ymax + 1):
            for px in range(xmin, xmax + 1):
                cx = px + 0.5; cy = py + 0.5
                w0 = ((x1 - cx) * (y2 - cy) - (x2 - cx) * (y1 - cy)) / area
                w1 = ((x2 - cx) * (y0 - cy) - (x0 - cx) * (y2 - cy)) / area
                w2 = 1.0 - w0 - w1
                if w0 < -1e-6 or w1 < -1e-6 or w2 < -1e-6:
                    continue
                d = w0 * D[t, 0] + w1 * D[t, 1] + w2 * D[t, 2]
                if d > zop[py, px] and d > zb[py, px]:
                    zb[py, px] = d
                    for c in range(3):
                        nb[py, px, c] = w0 * N[t, 0, c] + w1 * N[t, 1, c] + w2 * N[t, 2, c]
                    ib[py, px] = o

def vertex_normals(tris, smooth):
    fn = np.cross(tris[:, 1] - tris[:, 0], tris[:, 2] - tris[:, 0])
    fn /= (np.linalg.norm(fn, axis=1)[:, None] + 1e-12)
    if not smooth:
        return np.repeat(fn[:, None, :], 3, axis=1)
    V = tris.reshape(-1, 3)
    key = np.round(V / 0.05).astype(np.int64)
    _, inv = np.unique(key, axis=0, return_inverse=True)
    inv = inv.ravel()
    acc = np.zeros((inv.max() + 1, 3))
    fnr = np.repeat(fn, 3, axis=0)
    np.add.at(acc, inv, fnr)
    vn = acc[inv]; vn /= (np.linalg.norm(vn, axis=1)[:, None] + 1e-12)
    # keep sharp creases: fall back to face normal where the vertex normal deviates > 40 deg
    dot = np.sum(vn * fnr, axis=1)
    vn[dot < 0.77] = fnr[dot < 0.77]
    return vn.reshape(-1, 3, 3)

class ZScene:
    def __init__(self):
        self.objs = []
    def add(self, tris, color, alpha=1.0, smooth=True, edge=True):
        self.objs.append((np.asarray(tris, float), mcolors.to_rgb(color), alpha, smooth)); return self

    def camera(self, az, el):
        a, e = np.radians(az), np.radians(el)
        fwd = np.array([np.cos(e) * np.cos(a), np.cos(e) * np.sin(a), np.sin(e)])
        right = np.cross([0, 0, 1], fwd); right /= np.linalg.norm(right); up = np.cross(fwd, right)
        return fwd, right, up

    def render(self, path, az=-135, el=18, size=(6, 6), dpi=110, focus=None, labels=(), title=None, ss=2, light=(0.35, -0.55, 0.75),
               outline=0.55, bg="white", pad=20, crop=None, overlay=None):
        fwd, right, up = self.camera(az, el)
        W0, H0 = int(size[0] * dpi), int(size[1] * dpi); W, H = W0 * ss, H0 * ss
        allT = np.concatenate([o[0] for o in self.objs]); objid = np.concatenate([np.full(len(o[0]), i) for i, o in enumerate(self.objs)])
        N = np.concatenate([vertex_normals(o[0], o[3]) for o in self.objs])
        X = allT @ right; Y = allT @ up; Dp = allT @ fwd
        if focus is not None:
            c = np.array(focus[0], float); r = focus[1]
            cx, cy = c @ right, c @ up; hw = r; hh = r * H0 / W0
            x0, x1, y0, y1 = cx - hw, cx + hw, cy - hh, cy + hh
        else:
            x0, x1, y0, y1 = X.min() - pad, X.max() + pad, Y.min() - pad, Y.max() + pad
            # keep aspect
            sx = (x1 - x0) / W0; sy = (y1 - y0) / H0; s = max(sx, sy)
            cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
            x0, x1, y0, y1 = cx - s * W0 / 2, cx + s * W0 / 2, cy - s * H0 / 2, cy + s * H0 / 2
        sc = W / (x1 - x0)
        P = np.stack([(X - x0) * sc, (y1 - Y) * sc], axis=-1)
        alpha_mask = np.array([o[2] < 1.0 for o in self.objs])
        zb = np.full((H, W), -1e9); nb = np.zeros((H, W, 3)); ib = np.full((H, W), -1, np.int64)
        _raster(P, Dp, N, objid, W, H, zb, nb, ib, alpha_mask, False)
        L = np.array(light, float); L /= np.linalg.norm(L)
        cols = np.array([o[1] for o in self.objs] + [mcolors.to_rgb(bg)])
        def shade(nb, ib):
            n = nb / (np.linalg.norm(nb, axis=-1)[..., None] + 1e-12)
            flip = np.sum(n * fwd, axis=-1) < 0; n[flip] *= -1
            dif = np.clip(n @ L, 0, 1); dif2 = np.clip(n @ (-L * np.array([1, 1, -0.2])), 0, 1)
            h = (L + fwd); h /= np.linalg.norm(h); spec = np.clip(n @ h, 0, 1) ** 40
            base = cols[ib]
            img = base * (0.40 + 0.55 * dif[..., None] + 0.12 * dif2[..., None]) + 0.18 * spec[..., None]
            img[ib < 0] = mcolors.to_rgb(bg)
            return np.clip(img, 0, 1)
        img = shade(nb, ib)
        # outlines: object boundaries and depth jumps
        if outline:
            e = np.zeros((H, W), bool)
            for dy, dx in ((0, 1), (1, 0)):
                a = ib; b = np.roll(ib, (dy, dx), (0, 1))
                za = zb; zbb = np.roll(zb, (dy, dx), (0, 1))
                e |= (a != b) | (np.abs(za - zbb) > 6.0 / sc * 0 + 4.0)
                na = nb / (np.linalg.norm(nb, axis=-1)[..., None] + 1e-12); nbb = np.roll(na, (dy, dx), (0, 1))
                e |= (np.sum(na * nbb, axis=-1) < 0.55) & (a >= 0)
            img[e] = img[e] * (1 - outline)
        # transparent layer
        if alpha_mask.any():
            zt = np.full((H, W), -1e9); nt = np.zeros((H, W, 3)); it = np.full((H, W), -1, np.int64)
            _raster_alpha(P, Dp, N, objid, W, H, zb, zt, nt, it, alpha_mask)
            imt = shade(nt, it)
            alph = np.array([o[2] for o in self.objs] + [0.0])[it]
            alph[it < 0] = 0
            img = img * (1 - alph[..., None]) + imt * alph[..., None]
            et = np.zeros((H, W), bool)
            for dy, dx in ((0, 1), (1, 0)):
                et |= (it != np.roll(it, (dy, dx), (0, 1)))
            img[et] = img[et] * 0.8
        img = img.reshape(H0, ss, W0, ss, 3).mean(axis=(1, 3))
        fig = plt.figure(figsize=(W0 / dpi, H0 / dpi), dpi=dpi); ax = fig.add_axes([0, 0, 1, 1]); ax.imshow(img); ax.axis("off")
        for p3, txt, *off in labels:
            p3 = np.array(p3, float); px = ((p3 @ right) - x0) * sc / ss; py = (y1 - (p3 @ up)) * sc / ss
            o = off[0] if off else (25, -25)
            ax.annotate(txt, xy=(px, py), xytext=(px + o[0], py + o[1]), fontsize=8.5, color="#111",
                        bbox=dict(boxstyle="round,pad=0.3", fc="white", ec="#999", lw=0.6, alpha=0.95),
                        arrowprops=dict(arrowstyle="-", color="#444", lw=0.8))
        if overlay is not None:
            def proj(p3):
                p3 = np.array(p3, float); return (((p3 @ right) - x0) * sc / ss, (y1 - (p3 @ up)) * sc / ss)
            overlay(ax, proj)
        if title:
            ax.text(0.02, 0.975, title, transform=ax.transAxes, fontsize=11, va="top", weight="bold", color="#222")
        fig.savefig(path, facecolor=bg); plt.close(fig)
        self._proj = (right, up, x0, y1, sc / ss)
        return self
