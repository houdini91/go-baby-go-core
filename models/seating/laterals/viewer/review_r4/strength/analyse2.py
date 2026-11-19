# r4 strength part 2: solid (no bores), alternative rods, bigger button pocket, tenon + pins, wing interface, engagement.
from analyse import *
import sys
def circle(cx, cy, r, n=48):
    a = np.linspace(0, 2 * np.pi, n, endpoint=False); return np.stack([cx + r * np.cos(a), cy + r * np.sin(a)], -1)
def hole(cx, cy, r):
    P = circle(cx, cy, r)[::-1]; pr = props(P)
    if pr[0] > 0: pr = -pr
    return dict(P=P, pr=pr, solid=False, cen=np.array([cx, cy]), area=abs(pr[0]), r_eq=r, depth=1)
def mind(S, x, y):      # min distance from a point to any boundary
    return min(np.min(np.hypot(s['P'][:, 0] - x, s['P'][:, 1] - y)) for s in S)

part = sys.argv[1] if len(sys.argv) > 1 else 'all'
F = lambda r: f"{r['sf']:.2f}"

if part in ('alt', 'all'):
    print('== arm sections: as built / solid-no-bores / Ø4 in both bores / one Ø6 / bond sensitivity ==')
    for u in [0, 2, 12, 13, 14, 15, 16]:
        p0, tn, e1, e2 = frame_arm(u); S = section(T, p0, tn, e1, e2); B = classify(S, p0, e1, e2)
        ids = {id(b['s']) for b in B}
        Sfill = [s for s in S if id(s) not in ids]                       # bores filled with plastic
        out = {}
        for load in ['knock300', 'edge300', 'lean100_60C']:
            r_none = sf_at(S, p0, tn, e1, e2, B, set(), load)
            r_fill = sf_at(Sfill, p0, tn, e1, e2, [], set(), load)
            r_both = sf_at(S, p0, tn, e1, e2, B, {'D4', 'D3'}, load)
            # swap Ø3 -> Ø4
            S44 = list(Sfill); B44 = []
            for b in B:
                h = hole(*b['s']['cen'], 2.15); S44.append(h); B44.append(dict(s=h, d=4.0, name=b['name'], anchor=b['anchor']))
            r_44 = sf_at(S44, p0, tn, e1, e2, B44, {'D4', 'D3'}, load)
            # one Ø6 (bore 6.4) between the two present bores, as far toward -e1 as walls allow (>=1.2 to everything)
            best = None
            for cx in np.arange(-6, 2, 0.25):
                for cy in np.arange(-13, -5, 0.25):
                    if not any(inside((cx, cy), s['P']) for s in Sfill if s['solid']): continue
                    if any(inside((cx, cy), s['P']) for s in Sfill if not s['solid']): continue
                    if mind(Sfill, cx, cy) < 3.2 + 1.2: continue
                    if best is None or cx < best[0]: best = (cx, cy)
            r_6 = None
            if best:
                anc = min(b['anchor'] for b in B) if B else 0
                h = hole(best[0], best[1], 3.2); r_6 = sf_at(Sfill + [h], p0, tn, e1, e2, [dict(s=h, d=6.0, name='D6', anchor=anc)], {'D6'}, load)
            m0 = MAT[LOADS[load][2]]['tb']
            sens = []
            for tb in (2.0, 8.0):
                MAT[LOADS[load][2]]['tb'] = tb; sens.append(F(sf_at(S, p0, tn, e1, e2, B, {'D4', 'D3'}, load)))
            MAT[LOADS[load][2]]['tb'] = m0
            out[load] = (F(r_none), F(r_fill), F(r_both), F(r_44), (F(r_6) + f'@{best[0]:.1f},{best[1]:.1f}') if r_6 else '-', sens)
        print(f'u {u:3}', {k: v for k, v in out.items()})

if part in ('pocket', 'all'):
    print('== bigger button pocket on an 18 x 28 pill at the pocket zone (u 13 loads), raster section ==')
    p0, tn, e1, e2 = frame_arm(13); FF, PP, mk = LOADS['knock300']
    Mv = np.cross(PP - p0, FF); M1, M2, Tq = Mv @ e1, Mv @ e2, Mv @ tn
    g = 0.1; xs = np.arange(-9 + g / 2, 9, g); ys = np.arange(-14 + g / 2, 14, g); X, Y = np.meshgrid(xs, ys)
    pill = (np.abs(Y) <= 5) | (np.hypot(X, np.abs(Y) - 5) <= 9)
    def sf_raster(mask, rods, M1, M2, m, Tq, sinth, Wt_scale):
        dA = g * g; Ep = m['Ep']
        EA = Ep * mask.sum() * dA; E1 = Ep * (X * mask).sum() * dA; E2 = Ep * (Y * mask).sum() * dA
        E11 = Ep * (X * X * mask).sum() * dA; E22 = Ep * (Y * Y * mask).sum() * dA; E12 = Ep * (X * Y * mask).sum() * dA
        for (x, y, d, eta) in rods:
            A = np.pi * d * d / 4; I = np.pi * d ** 4 / 64
            EA += EC * eta * A; E1 += EC * eta * A * x; E2 += EC * eta * A * y
            E11 += EC * eta * A * x * x + EC * I; E22 += EC * eta * A * y * y + EC * I; E12 += EC * eta * A * x * y
        cx, cy = E1 / EA, E2 / EA
        K = np.array([[E11 - EA * cx * cx, E12 - EA * cx * cy], [E12 - EA * cx * cy, E22 - EA * cy * cy]])
        a, b = np.linalg.solve(K, [-M2, M1]); sig = Ep * np.abs(a * (X - cx) + b * (Y - cy))[mask].max()
        tau = abs(Tq) / (0.231 * 18 * 18 * 28 * Wt_scale)
        Xs = tsai_hill(sinth, m); return 1 / math.sqrt((sig / Xs) ** 2 + (tau / (m['S'] * (1 - 0.35 * sinth))) ** 2), sig
    sinth = abs(tn @ dirD)
    print(f'loads u13: M1 {M1:.0f} M2 {M2:.0f} T {Tq:.0f}  sin(theta) {sinth:.2f}')
    rods_now = [(-4.0, -10.0, 4.0, 0.35), (0.0, -9.0, 3.0, 0.35)]    # ~ the bond-limited share seen on the real section
    for (wn, hn) in [(0, 0), (7.4, 9.4), (10.4, 14.4), (12.4, 15.4)]:
        row = []
        for dn in [1.5, 3, 4.5, 6, 8.4]:
            msk = pill.copy()
            if wn: msk &= ~((X > 9 - dn) & (np.abs(Y - 2.0) < hn / 2))
            fr = msk.sum() / pill.sum()
            s0, _ = sf_raster(msk, [], M1, M2, MAT['RT'], Tq, sinth, fr ** 2)
            s1, _ = sf_raster(msk, rods_now, M1, M2, MAT['RT'], Tq, sinth, fr ** 2)
            M1h, M2h, Th = np.array([M1, M2, Tq]) / 3                    # 100 N
            s2, _ = sf_raster(msk, [], M1h, M2h, MAT['H60'], Th, sinth, fr ** 2)
            row.append(f'd{dn}: {s0:.2f}/{s1:.2f}/{s2:.2f}')
            if not wn: break
        print(f'pocket {wn}x{hn} (knock no-C / with-C / 60C no-C):', row)
    # full-depth wall left beside the pocket for the 3 mm facet + pocket
    print('pill 18 wide: with a 3 mm facet + cap 3 + travel 3.9 -> pocket floor at 9.9 deep, wall behind 8.1')

if part in ('tenon', 'all'):
    print('== tenon root (section normal -x), pins Ø2 ==')
    LOADS['down150D3'] = (np.array([0, 0, -150.]), np.array([60., 14., zp]), 'RT')
    LOADS['knock_noface'] = LOADS['knock300']
    for dx in [3.0, 0.6, -0.5, -3.0, -6.5]:
        x = xe + dx; p0 = np.array([x, YAX, zp]); tn = np.array([-1., 0, 0]); e2 = np.array([0, 0, 1.]); e1 = np.cross(e2, tn)
        S = [q for q in section(T, p0, tn, e1, e2) if np.abs(q['P']).max() < 16]; B = classify(S, p0, e1, e2, tenon=True)
        res = []
        for load in ['knock300', 'edge300', 'down150D3', 'lean100_60C']:
            a = sf_at(S, p0, tn, e1, e2, B, set(), load); b = sf_at(S, p0, tn, e1, e2, B, {'pin'}, load)
            ids = {id(q['s']) for q in B}; c_ = sf_at([s for s in S if id(s) not in ids], p0, tn, e1, e2, [], set(), load)
            f = sf_at(S, p0, tn, e1, e2, B, {'pin'}, load, bond='full')
            res.append(f"{load}: {a['sf']:.2f}/{c_['sf']:.2f}/{b['sf']:.2f}/{f['sf']:.2f}f M{a['M']}")
        print(f'x xe{dx:+.1f} A {sum(s["pr"][0] for s in S):.0f} pins {len(B)} anchor {[round(q["anchor"],1) for q in B][:1]} sin {abs(tn@dirD):.2f} | none(holes)/solid/pins/full', res)

if part in ('wing', 'all'):
    print('== wing tip block -> wing (sections normal to t), PETG only; tongue P1 and P3 ==')
    W = load('ps1_RiBH'); off = P0 + D_SHIFT * AXH + (ZREF - Z0) * SHEAR
    for tt in [-10, 0, 20, 33, 40, 46]:
        o = off + tt * TH; p0 = np.array([o[0], o[1], 270.]); tn = dirT; e2 = np.array([0, 0, 1.]); e1 = np.cross(e2, tn)
        S = section(W, p0, tn, e1, e2)
        A = sum(s['pr'][0] for s in S)
        holes = [(round(s['cen'][0], 1), round(s['cen'][1] + 270, 1), round(np.ptp(s['P'][:, 0]), 1), round(np.ptp(s['P'][:, 1]), 1)) for s in S if not s['solid'] and s['area'] > 2]
        res = []
        for (cc, zpp, nm) in [(227.0, 179.5, 'P1 LOW'), (292.0, 339.5, 'P3 HIGH')]:
            Fv = np.array([300., 0, 0]); Pp = np.array([100., 14., zpp]); Mv = np.cross(Pp - p0, Fv)
            r = solve(S, [], set(), MAT['RT'], Mv @ e1, Mv @ e2, Fv @ tn, bond='full')
            res.append(f"{nm}: smax {np.abs(r['sig']).max():.1f} MPa")
        print(f't {tt:4} A {A:6.0f} holes {holes} ', res)
