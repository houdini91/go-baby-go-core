# r4 strength: SF at the critical carrier sections with / without each carbon element.
# Composite beam (transformed section) on mesh slices; carbon bond-limited by anchorage past the section.
from sections import *
import itertools
T = load('ps1_RUgD')
p = armPath(c, zp); xe = armAt(p, p['L'])[0][0]
PAD = np.array([100.0, 14.0, zp])                       # pad centre (load point; x irrelevant for an x load)

# ---- materials (P1S PETG, solid-section basis; printed with >=6 walls + high infill ~0.85 of this) ----
MAT = {
 'RT':  dict(Ep=1900., Xa=40., Xc=18., S=22., tb=4.0),   # along / across layers, shear, epoxy-on-PETG bond MPa
 'H60': dict(Ep=800.,  Xa=13., Xc=5.9, S=7.0, tb=3.0),   # 60 C, hours: creep modulus, creep-rupture allowables (0.65 hot x 0.5 sustained)
}
EC, XCF = 120000., 1200.                                  # pultruded carbon: E, usable strength (flex/compression-limited)

def tsai_hill(sinth, m):           # uniaxial strength at angle theta out of the layer plane
    s2 = sinth ** 2; c2 = 1 - s2
    inv = c2 ** 2 / m['Xa'] ** 2 + s2 ** 2 / m['Xc'] ** 2 + s2 * c2 * (1 / m['S'] ** 2 - 1 / m['Xa'] ** 2)
    return 1 / math.sqrt(inv)

def frame_arm(u):
    if u < 0: p0 = Q(9.4, TAPER_T - u, c); tn = -dirT
    else: p0, tn = armAt(p, u)
    e2 = np.array([0, 0, 1.0]) - tn[2] * tn; e2 /= np.linalg.norm(e2); e1 = np.cross(e2, tn)
    return p0, tn, e1, e2

def classify(S, p0, e1, e2, tenon=False):
    """split loops into PETG (solid + plain holes) and carbon bores [(centre2d, rod d, name, anchor)]"""
    bores = []
    for s in S:
        if s['solid']: continue
        r = s['r_eq']; cen = s['cen']; X = p0 + cen[0] * e1 + cen[1] * e2
        if tenon and 0.9 < r < 1.3:
            bores.append(dict(s=s, d=2.0, name='pin', anchor=min(xe + TPIN_IN - X[0], X[0] - (xe - TEN_L - 0.5))))
            continue
        for i, cf in enumerate(CF):
            # bore centre in the tongue frame
            t_here = (X[:2] - P0 - D_SHIFT * AXH - (ZREF - Z0) * SHEAR) @ TH
            d_here = (X[:2] - P0 - D_SHIFT * AXH - (ZREF - Z0) * SHEAR) @ AXH
            if abs(d_here - cf[0]) < 1.5 and abs(X[2] - (c + cf[1])) < 1.5 and abs(r - cf[2]) < 0.6 * cf[2]:
                bores.append(dict(s=s, d=cf[4], name=f'D{cf[4]:.0f}', anchor=min(CF_T1 - t_here, t_here - cf[3])))
    return bores

def solve(S, bores, use, m, M1, M2, Nax, eta=None, bond='bond'):
    """returns dict with PETG max stress per point set, carbon forces. use: set of rod names to include."""
    el = []   # (EA, Ex1, Ex2, Ex1x1, Ex2x2, Ex1x2)
    pr = sum(s['pr'] for s in S)                           # PETG net (bores are holes)
    rods = [b for b in bores if b['name'] in use]
    if eta is None: eta = [0.0 if bond == 'dowel' else 1.0] * len(rods)
    for it in range(30):
        EA = m['Ep'] * pr[0]; E1 = m['Ep'] * pr[1]; E2 = m['Ep'] * pr[2]
        E11 = m['Ep'] * pr[3]; E22 = m['Ep'] * pr[4]; E12 = m['Ep'] * pr[5]
        for b, k in zip(rods, eta):
            A = math.pi * b['d'] ** 2 / 4; I = math.pi * b['d'] ** 4 / 64; x, y = b['s']['cen']; Ee = EC * k
            Eo = EC                                           # own bending of the rod (dowel action) needs no bond
            EA += Ee * A; E1 += Ee * A * x; E2 += Ee * A * y
            E11 += Ee * A * x * x + Eo * I; E22 += Ee * A * y * y + Eo * I; E12 += Ee * A * x * y
        cx, cy = E1 / EA, E2 / EA
        K11 = E11 - EA * cx * cx; K22 = E22 - EA * cy * cy; K12 = E12 - EA * cx * cy
        # [-M2, M1] = [[K11 K12][K12 K22]] [a b]
        a, b_ = np.linalg.solve([[K11, K12], [K12, K22]], [-M2, M1]); e0 = Nax / EA
        strain = lambda x, y: e0 + a * (x - cx) + b_ * (y - cy)
        if bond in ('full', 'dowel'): break
        new = []; ok = True
        for rod, k in zip(rods, eta):
            x, y = rod['s']['cen']; F = abs(EC * k * strain(x, y)) * math.pi * rod['d'] ** 2 / 4
            Flim = m['tb'] * math.pi * rod['d'] * max(rod['anchor'], 0)
            if F > Flim * 1.001: new.append(k * Flim / F); ok = False
            else: new.append(k)
        if ok: break
        eta = new
    pts = np.vstack([s['P'] for s in S if s['solid']])
    sig = m['Ep'] * np.array([strain(x, y) for x, y in pts])
    rodinfo = []
    for rod, k in zip(rods, eta):
        x, y = rod['s']['cen']; st = EC * strain(x, y)          # fibre stress if fully strained
        rodinfo.append((rod['name'], round(k * st * math.pi * rod['d'] ** 2 / 4), round(abs(st) + EC * (rod['d'] / 2) * math.hypot(a, b_))))
    return dict(sig=sig, pts=pts, K=(K11, K22, K12), rods=rodinfo, eta=eta)

def torsion_tau(S, T):
    pts = np.vstack([s['P'] for s in S if s['solid']])
    w = np.ptp(pts[:, 0]); h = np.ptp(pts[:, 1]); b, hh = min(w, h), max(w, h)
    r = hh / b; alpha = np.interp(r, [1, 1.5, 2, 3], [0.208, 0.231, 0.246, 0.267]); beta = np.interp(r, [1, 1.5, 2, 3], [0.141, 0.196, 0.229, 0.263])
    A = sum(s['pr'][0] for s in S); Afull = b * hh - (4 - math.pi) * (b / 2) ** 2 if hh > b else b * hh
    k = min(A / (b * hh), 1.0) / 0.95
    return abs(T) / (alpha * b * b * hh * k ** 1.5), beta * b ** 3 * hh * k ** 2

LOADS = {'knock300': (np.array([300., 0, 0]), PAD, 'RT'),
         'edge300':  (np.array([300., 0, 0]), np.array([100., -20., zp - 20]), 'RT'),
         'down150D3': (np.array([0, 0, -150.]), np.array([62., 14., zp]), 'RT'),
         'lean100_60C': (np.array([100., 0, 0]), PAD, 'H60')}

def sf_at(S, p0, tn, e1, e2, bores, use, load, bond='bond'):
    F, P, mk = LOADS[load]; m = MAT[mk]
    Mv = np.cross(P - p0, F); Tq = Mv @ tn; M1 = Mv @ e1; M2 = Mv @ e2; Nax = F @ tn
    r = solve(S, bores, use, m, M1, M2, Nax, bond=bond)
    sinth = abs(tn @ dirD); X = tsai_hill(sinth, m)
    tau, J = torsion_tau(S, Tq)
    smax = np.abs(r['sig']).max()
    # torsion shear: inter-layer if the section plane is not parallel to layers; use S scaled
    Sx = m['S'] * (1 - 0.35 * sinth)
    sf = 1 / math.sqrt((smax / X) ** 2 + (tau / Sx) ** 2)
    return dict(sf=sf, smax=smax, tau=tau, X=X, rods=r['rods'], M=(round(M1), round(M2), round(Tq)), J=J, K=r['K'], sinth=sinth)

CASES = [('none', set()), ('D4', {'D4'}), ('D3', {'D3'}), ('D4+D3', {'D4', 'D3'})]

if __name__ == '__main__':
    out = {}
    for u in [-6, -2, 0, 2, 4, 6, 8, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 26]:
        p0, tn, e1, e2 = frame_arm(u); S = section(T, p0, tn, e1, e2); bores = classify(S, p0, e1, e2)
        row = []
        for load in ['knock300', 'edge300', 'lean100_60C']:
            for name, use in CASES:
                r = sf_at(S, p0, tn, e1, e2, bores, use, load)
                row.append(f"{r['sf']:.2f}")
            row.append(f"{sf_at(S, p0, tn, e1, e2, bores, {'D4', 'D3'}, load, bond='dowel')['sf']:.2f}d")
            row.append(f"{sf_at(S, p0, tn, e1, e2, bores, {'D4', 'D3'}, load, bond='full')['sf']:.2f}f")
        r0 = sf_at(S, p0, tn, e1, e2, bores, set(), 'knock300'); r1 = sf_at(S, p0, tn, e1, e2, bores, {'D4', 'D3'}, 'knock300')
        print(f"u {u:4} A {sum(s['pr'][0] for s in S):5.0f} bores {[(b['name'], round(b['anchor'],1)) for b in bores]} M {r0['M']} sinth {r0['sinth']:.2f}")
        print('     knock none/D4/D3/both', row[0:6], ' edge', row[6:12], ' 60C', row[12:18], ' smax', round(r0['smax'], 1), '->', round(r1['smax'], 1), 'tau', round(r0['tau'], 1), 'rods', r1['rods'])
