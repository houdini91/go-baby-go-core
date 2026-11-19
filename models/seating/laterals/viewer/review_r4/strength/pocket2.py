# facet + pocket combinations and arm width, raster pill at u 13 loads (knock 300 N centre; sign: +e1 = pocket side)
from analyse import *
p0, tn, e1, e2 = frame_arm(13); FF, PP, _ = LOADS['knock300']; Mv = np.cross(PP - p0, FF); M1, M2, Tq = Mv @ e1, Mv @ e2, Mv @ tn
sinth = abs(tn @ dirD); g = 0.1
def run(W, cuts, rods, m=MAT['RT'], k=1.0):
    xs = np.arange(-W / 2 + g / 2, W / 2, g); ys = np.arange(-14 + g / 2, 14, g); X, Y = np.meshgrid(xs, ys)
    R = W / 2; st = 14 - R
    msk = (np.abs(Y) <= st) | (np.hypot(X, np.abs(Y) - st) <= R); full = msk.sum()
    for (w, h, dep) in cuts: msk &= ~((X > W / 2 - dep) & (np.abs(Y - 2.0) < h / 2))
    dA = g * g; Ep = m['Ep']
    S = lambda f: Ep * (f * msk).sum() * dA
    EA, E1, E2, E11, E22, E12 = S(1), S(X), S(Y), S(X * X), S(Y * Y), S(X * Y)
    for (x, y, d, eta) in rods:
        A = np.pi * d * d / 4; I = np.pi * d ** 4 / 64
        EA += EC * eta * A; E1 += EC * eta * A * x; E2 += EC * eta * A * y; E11 += EC * eta * A * x * x + EC * I; E22 += EC * eta * A * y * y + EC * I; E12 += EC * eta * A * x * y
    cx, cy = E1 / EA, E2 / EA
    K = np.array([[E11 - EA * cx * cx, E12 - EA * cx * cy], [E12 - EA * cx * cy, E22 - EA * cy * cy]])
    a, b = np.linalg.solve(K, [-M2 * k, M1 * k]); st_ = Ep * (a * (X - cx) + b * (Y - cy))
    sig_t = st_[msk].max(); sig_c = -st_[msk].min()
    fr = msk.sum() / full; tau = abs(Tq * k) / (0.231 * min(W, 28) ** 2 * max(W, 28) * fr ** 2)
    X_ = tsai_hill(sinth, m); Ss = m['S'] * (1 - .35 * sinth)
    sf = lambda s: 1 / math.sqrt((s / X_) ** 2 + (tau / Ss) ** 2)
    return sf(max(sig_t, sig_c / 1.25)), sig_t, sig_c
C = [(-4.0, -10.0, 4.0, 0.35), (0.0, -9.0, 3.0, 0.35)]
C6 = [(-5.2, -5.2, 6.0, 0.35)]
print('M at u13', round(M1), round(M2), round(Tq))
cases = [('plain pill', []), ('current pocket 7.4x9.4x8.4', [(7.4, 9.4, 8.4)]),
         ('facet 12x15x1.5 + pocket 7.4x9.4 (to 9.9)', [(12.4, 15.4, 1.5), (7.4, 9.4, 9.9)]),
         ('facet 12x15x3 + pocket 7.4x9.4 (to 11.4)', [(12.4, 15.4, 3.0), (7.4, 9.4, 11.4)]),
         ('facet 12x15x1.5 + pocket 10.4x14.4 (to 9.9)', [(12.4, 15.4, 1.5), (10.4, 14.4, 9.9)]),
         ('facet 12x15x3 + pocket 10.4x14.4 (to 11.4)', [(12.4, 15.4, 3.0), (10.4, 14.4, 11.4)]),
         ('facet 12x15x3 + pocket 10.4x14.4 to 8.4 (cap thinner)', [(12.4, 15.4, 3.0), (10.4, 14.4, 8.4)]),
         ('pocket 10.4x14.4 depth 6', [(10.4, 14.4, 6.0)])]
for W in [18, 20, 22]:
    for nm, cuts in cases:
        if W != 18 and nm not in ('plain pill', 'current pocket 7.4x9.4x8.4', 'facet 12x15x3 + pocket 10.4x14.4 (to 11.4)', 'facet 12x15x1.5 + pocket 10.4x14.4 (to 9.9)'): continue
        sh = (W - 18) / 2   # widen toward the inside of the bend (-e1): shift rods
        Cw = [(x - sh, y, d, e) for x, y, d, e in C]; C6w = [(x - sh, y, d, e) for x, y, d, e in C6]
        r0 = run(W, cuts, []); r1 = run(W, cuts, Cw); r6 = run(W, cuts, C6w); h0 = run(W, cuts, [], MAT['H60'], 1 / 3); h1 = run(W, cuts, Cw, MAT['H60'], 1 / 3)
        print(f'W{W} {nm:55s} knock noC {r0[0]:.2f} C4+3 {r1[0]:.2f} C6 {r6[0]:.2f} | 60C noC {h0[0]:.2f} C {h1[0]:.2f}  (sig t/c noC {r0[1]:.0f}/{r0[2]:.0f})')
