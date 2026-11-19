# Headless renders of the Round 5 wing-side parts: a small numpy z-buffer rasteriser (flat shading + depth/normal edge lines),
# saved as PNG via matplotlib. Run: ../.venv/bin/python render.py
import json, struct, math, numpy as np, trimesh, matplotlib
matplotlib.use('Agg'); import matplotlib.pyplot as plt
from frame import P0, AXH, TH, SH

RAW = '../raw/'
def raw(name):
    B = json.load(open(f'{RAW}{name}.json'))
    a = np.array([fc['vertices'] for body in B for face in body['faces'] for fc in face['facets']], float).reshape(-1, 3) * 1000
    return trimesh.Trimesh(a, np.arange(len(a)).reshape(-1, 3), process=False)
def stl(fn): return trimesh.load(fn, process=False)
def Qw(d, t, z): return np.array([P0[0] + (d + 2) * AXH[0] + t * TH[0] + SH[0], P0[1] + (d + 2) * AXH[1] + t * TH[1] + SH[1], z])
dirD = np.r_[AXH, 0]; dirT = np.r_[TH, 0]

def render(items, eye, target, W=1400, H=1000, fov=26, bg=(0.93, 0.94, 0.945), edges=True):
    eye = np.asarray(eye, float); target = np.asarray(target, float)
    f = target - eye; f /= np.linalg.norm(f); r = np.cross(f, [0, 0, 1]); r /= np.linalg.norm(r); u = np.cross(r, f)
    fl = 0.5 * H / math.tan(math.radians(fov) / 2)
    zbuf = np.full((H, W), np.inf); img = np.ones((H, W, 3)) * bg; nbuf = np.zeros((H, W, 3)); idb = np.full((H, W), -1)
    L1 = -f * 0.55 + u * 0.55 + r * 0.35; L1 /= np.linalg.norm(L1); L2 = -f;
    oid = 0
    for tm, col in items:
        oid += 1
        V = np.asarray(tm.vertices, float); F = np.asarray(tm.faces)
        P = V - eye; x = P @ r; y = P @ u; z = P @ f
        sx = W / 2 + fl * x / z; sy = H / 2 - fl * y / z
        T = V[F]; n = np.cross(T[:, 1] - T[:, 0], T[:, 2] - T[:, 0]); nn = np.linalg.norm(n, axis=1); ok = nn > 1e-12
        n[ok] /= nn[ok, None]
        col = np.asarray(col, float)
        cen = T.mean(1); front = np.einsum('ij,ij->i', n, cen - eye) < 0          # closed solids: back faces never show
        Sx = np.c_[sx[F[:, 0]], sx[F[:, 1]], sx[F[:, 2]]]; Sy = np.c_[sy[F[:, 0]], sy[F[:, 1]], sy[F[:, 2]]]
        zin = (z[F] > 1).all(1)
        ons = (Sx.max(1) >= 0) & (Sx.min(1) < W) & (Sy.max(1) >= 0) & (Sy.min(1) < H)
        keep = ok & front & zin & ons
        small = keep & (Sx.max(1) - Sx.min(1) < 1.2) & (Sy.max(1) - Sy.min(1) < 1.2)
        keep &= ~(small & (np.abs(n @ f) < 0.3)); small &= keep                 # edge-on sub-pixel slivers: not visible
        E = np.stack([np.linalg.norm(T[:, 1] - T[:, 0], axis=1), np.linalg.norm(T[:, 2] - T[:, 1], axis=1), np.linalg.norm(T[:, 0] - T[:, 2], axis=1)], 1)
        hgt = nn / np.maximum(E.max(1), 1e-9)                                     # 2 area / longest edge
        keep &= ~((hgt < 0.12) & (np.abs(n @ f) < 0.5)); small &= keep          # <0.12 mm mesh steps (lean rounding): below print resolution
        # shading for every face (vectorised)
        nv = np.where((n @ f < 0)[:, None], n, -n)
        hv = L1 - f; hv /= np.linalg.norm(hv)
        shade = 0.30 + 0.55 * np.clip(nv @ L1, 0, None) + 0.25 * np.clip(nv @ L2, 0, None)
        spec = 0.10 * np.clip(nv @ hv, 0, None) ** 30
        if small.any():                                   # sub-pixel triangles: splat the centroid
            ii = np.nonzero(small)[0]; px = np.clip(Sx[ii].mean(1).astype(int), 0, W - 1); py = np.clip(Sy[ii].mean(1).astype(int), 0, H - 1)
            dz = (cen[ii] - eye) @ f; o = np.argsort(-dz); ii, px, py, dz = ii[o], px[o], py[o], dz[o]
            lin = py * W + px; zb = zbuf.reshape(-1); okz = dz < zb[lin]; ii, lin, dz = ii[okz], lin[okz], dz[okz]
            zb[lin] = dz
            img.reshape(-1, 3)[lin] = np.clip(col[None] * shade[ii, None] + spec[ii, None], 0, 1)
            nbuf.reshape(-1, 3)[lin] = nv[ii]; idb.reshape(-1)[lin] = oid
        for i in np.nonzero(keep & ~small)[0]:
            a, b, c = F[i]
            xs = np.array([sx[a], sx[b], sx[c]]); ys = np.array([sy[a], sy[b], sy[c]])
            x0 = max(int(math.floor(xs.min())), 0); x1 = min(int(math.ceil(xs.max())), W - 1)
            y0 = max(int(math.floor(ys.min())), 0); y1 = min(int(math.ceil(ys.max())), H - 1)
            if x0 > x1 or y0 > y1: continue
            den = (ys[1] - ys[2]) * (xs[0] - xs[2]) + (xs[2] - xs[1]) * (ys[0] - ys[2])
            if abs(den) < 1e-12: continue
            gx, gy = np.meshgrid(np.arange(x0, x1 + 1) + 0.5, np.arange(y0, y1 + 1) + 0.5)
            w0 = ((ys[1] - ys[2]) * (gx - xs[2]) + (xs[2] - xs[1]) * (gy - ys[2])) / den
            w1 = ((ys[2] - ys[0]) * (gx - xs[2]) + (xs[0] - xs[2]) * (gy - ys[2])) / den
            w2 = 1 - w0 - w1
            m = (w0 >= -1e-6) & (w1 >= -1e-6) & (w2 >= -1e-6)
            if not m.any(): continue
            iz = w0 / z[a] + w1 / z[b] + w2 / z[c]; d = 1 / iz
            sub = zbuf[y0:y1 + 1, x0:x1 + 1]; upd = m & (d < sub)
            if not upd.any(): continue
            sub[upd] = d[upd]
            img[y0:y1 + 1, x0:x1 + 1][upd] = np.clip(col * shade[i] + spec[i], 0, 1)
            nbuf[y0:y1 + 1, x0:x1 + 1][upd] = nv[i]; idb[y0:y1 + 1, x0:x1 + 1][upd] = oid
    if edges:
        fin = np.isfinite(zbuf); zz = np.where(fin, zbuf, 1e9)
        e = np.zeros((H, W), bool)
        for dy, dx in ((0, 1), (1, 0)):
            z2 = np.roll(zz, (-dy, -dx), (0, 1)); n2 = np.roll(nbuf, (-dy, -dx), (0, 1)); i2 = np.roll(idb, (-dy, -dx), (0, 1))
            jump = np.abs(z2 - zz) > 0.004 * np.minimum(zz, z2) + 0.25
            crease = (np.sum(n2 * nbuf, 2) < 0.80) & fin
            e |= jump | crease | (i2 != idb)
        img[e] = img[e] * 0.35
    return img

def save(img, fn, title=None):
    h, w = img.shape[:2]; fig = plt.figure(figsize=(w / 100, h / 100), dpi=100); ax = fig.add_axes([0, 0, 1, 1]); ax.imshow(img); ax.axis('off')
    if title: ax.text(14, 30, title, fontsize=15, color='#222', family='sans-serif', weight='bold')
    fig.savefig(fn, dpi=100); plt.close(fig)

if __name__ == '__main__':
    import sys, os
    os.makedirs('renders', exist_ok=True)
    CH, KIT, BTN, ORG, SLV, DARK = (0.84, 0.82, 0.78), (0.95, 0.955, 0.96), (0.86, 0.85, 0.82), (1.0, 0.42, 0.16), (1.0, 0.42, 0.16), (0.14, 0.19, 0.30)
    HL = (0.62, 0.78, 0.95)
    wing = stl('wing_r5.stl'); armL = stl('nose_arm_r5_L.stl'); armS = stl('nose_arm_r5_S.stl'); btn = stl('button_r5.stl')
    slv = stl('sleeve_r5.stl'); cap = stl('cap_r5.stl'); wing0 = stl('work/wing0_fixed.stl')
    rest = raw('ps1_J_D')
    def kitparts(dz):
        out = []
        for k, c in (('kit11_RzCD', KIT), ('kit11_RcDD', KIT), ('kit11_RKED', KIT), ('kit11_RTED', DARK)):
            m = raw(k); m.apply_translation([0, 0, dz]); out.append((m, c))
        return out
    S2W = None
    C = 227.0
    tgt = Qw(10, -6, C)
    V = dict(parent=(tgt + np.array([150, -230, 230]), tgt), front=(Qw(10, -6, C) - dirT * 260 + np.array([0, 0, 40]), Qw(10, -6, C + 5)),
             outer=(Qw(10, -6, C + 10) + dirD * 200 - dirT * 70 + np.array([0, 0, 40]), Qw(10, -6, C + 10)),
             child=(np.array([100, 30, 225]) + np.array([-260, -120, 70]), np.array([118, 55, 225])))
    CLOSE = dict(outer=(Qw(12, -4, C) + dirD * 95 - dirT * 40 + np.array([0, 0, 22]), Qw(12, -4, C)),
                 front=(Qw(8, -14, C) - dirT * 105 + dirD * 25 + np.array([0, 0, 30]), Qw(8, -14, C)),
                 inner=(Qw(-6, -4, C) - dirD * 95 - dirT * 45 + np.array([0, 0, 25]), Qw(-6, -4, C)),
                 top=(Qw(8, -2, C + 20) + np.array([0, 0, 110]) - dirT * 40 + dirD * 10, Qw(8, -2, C + 20)))
    which = sys.argv[1:] or ['all']
    def scene(state, arm='L', hl=False):
        it = [(wing, CH), (rest, CH)]
        if state == 'on':
            a = armL if arm == 'L' else armS
            it += [(a, HL if hl else CH), (btn, ORG if hl else BTN), (slv, SLV)] + kitparts(3.5 if arm == 'L' else 25.5)
        elif state == 'cap': it += [(cap, HL if hl else CH)]
        elif state == 'orig': it = [(wing0, CH), (rest, CH)]
        return it
    jobs = []
    for st in ('on', 'cap', 'open'):
        for v in ('parent', 'front', 'outer'):
            jobs.append((f'{ {"on": "1_kit_on", "cap": "2_kit_off_cap", "open": "3_kit_off_open"}[st]}_{v}.png', scene(st), V[v], 32))
    for v in ('outer', 'front', 'inner', 'top'):
        jobs.append((f'4_close_kit_on_{v}.png', scene('on'), CLOSE[v], 26))
        jobs.append((f'5_close_cap_{v}.png', scene('cap'), CLOSE[v], 26))
        jobs.append((f'6_close_open_{v}.png', scene('open'), CLOSE[v], 26))
        jobs.append((f'0_close_original_{v}.png', scene('orig'), CLOSE[v], 26))
    jobs.append(('7_short_arm_parent.png', scene('on', 'S'), V['parent'], 26))
    jobs.append(('7_short_arm_outer.png', scene('on', 'S'), V['outer'], 26))
    jobs.append(('8_highlight_parent.png', scene('on', hl=True), V['parent'], 26))
    jobs.append(('0_original_parent.png', scene('orig'), V['parent'], 26))
    wopt = stl('option_wing_r5_W1closed.stl')
    for v in ('parent',):
        jobs.append((f'10_option_W1closed_kit_on_{v}.png', [(wopt, CH), (rest, CH), (armL, CH), (btn, BTN), (slv, SLV)] + kitparts(3.5), V[v], 26))
        jobs.append((f'10_option_W1closed_cap_{v}.png', [(wopt, CH), (rest, CH), (cap, CH)], V[v], 26))
    jobs.append(('10_option_W1closed_close_front.png', [(wopt, CH), (rest, CH), (armL, CH), (btn, BTN), (slv, SLV)], CLOSE['front'], 26))
    # pulled out 45 mm forward (along -t): the tongue, button and groove
    def moved(m, v): m2 = m.copy(); m2.apply_translation(v); return m2
    pull = -dirT * 45
    jobs.append(('9_pulled_out_outer.png', [(wing, CH), (rest, CH), (moved(armL, pull), CH), (moved(btn, pull), ORG), (moved(slv, pull), SLV)],
                 (Qw(12, 0, C) + dirD * 150 - dirT * 60 + np.array([0, 0, 60]), Qw(10, -10, C)), 26))
    jobs.append(('9_pulled_out_front.png', [(wing, CH), (rest, CH), (moved(armL, pull), CH), (moved(btn, pull), ORG), (moved(slv, pull), SLV)],
                 (Qw(4, -40, C) - dirT * 150 + dirD * 60 + np.array([0, 0, 70]), Qw(8, -10, C)), 26))
    for fn, items, (eye, tg), fov in jobs:
        if which != ['all'] and not any(w in fn for w in which): continue
        save(render(items, eye, tg, fov=fov), 'renders/' + fn, fn[:-4].replace('_', ' '))
        print('wrote', fn)
