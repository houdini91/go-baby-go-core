# What the main geometric fixes buy (same contact model / section method).
import numpy as np, pickle, math, copy
from geom import *
import loads as L, tongue as T
base = L.patches('zero')
def variant(outer_t0, outer_z1, inner_d=3.75, outer_d=17.45):
    P = dict(base)
    n_o = -L.nrm([outer_d, 3.0], [outer_d - 0.165, 9.3], 1)
    P['outer wall'] = np.array(L.seg_pts([outer_d, 2.0], [outer_d - 0.165 * 1.15, 9.3], outer_t0, 99, TG_Z[0], outer_z1, (*n_o, 0.0), 0.0, nz=12, ns=12), float)
    return P
cases = [('lean 300', [1, 0, 0], 'centre', 300.0), ('proof 600', [1, 0, 0], 'centre', 600.0), ('knock 900 out', [1, 0, 0], 'corners', 900.0)]
for nm, P in (('as built: outer wall t 5.3-9.3, z 214-223', base),
              ('mouth chamfer 0.5 on the outer wall: t 2.5-9.3, z 214-223', variant(2.5, 222.9)),
              ('+ outer wall over the full tongue height (latch out of the tongue)', variant(2.5, TG_Z[1]))):
    area = P['outer wall'][:, 7].sum(); line = f'{nm:70s} outer area {area:5.1f} mm2 |'
    for cn, dw, where, F in cases:
        worst = 0
        for cfg in CONFIGS:
            pts = L.pad_points(*cfg)
            for Pw in ([pts['centre']] if where == 'centre' else pts['corners']):
                f, M = L.wrench(np.array(dw, float) * F, Pw); res, *_ = L.solve(f, M, P)
                worst = max(worst, res['outer wall']['F'] / area)
        line += f' {cn}: p {worst:5.1f} MPa SF {25/worst:4.2f} |'
    print(line)
# tongue section variants at the same worst root moments (seated model, lean 300): Mz -12.2, Md -3.7 N.m, N ~ (ignore)
for nm, lo, hi, gut in (('as built (latch channels in the tongue)', 3.75, 17.45, True), ('solid 13.7 x 32.1 (latch moved out)', 3.75, 17.45, False), ('solid, widened to d 1.5-18.6 (17.1 x 32.1; walls >= 6)', 1.5, 18.6, False)):
    d = np.arange(0, 25, 0.05) + 0.025; z = np.arange(210, 250, 0.05) + 0.025; D, Z = np.meshgrid(d, z, indexing='ij')
    m = (D >= lo) & (D <= hi) & (Z >= TG_Z[0]) & (Z <= TG_Z[1])
    if gut: m &= ~((D >= 7.9) & (Z >= 222.9))
    A, dc, zc, Idd, Izz, Idz = T.props(D, Z, m)
    c = max(hi - dc, dc - lo); s300 = 12.2e3 * c / Izz + 3.7e3 * 16 / Idd
    print(f'{nm:58s} A {A:5.0f}  Izz {Izz:6.0f}  Z_yaw {Izz/c:6.0f} mm3 -> sigma(lean 300, Mz 12.2 + Md 3.7 N.m) {s300:5.1f} MPa, SF {40/s300:4.2f} @300, {40/s300/2:4.2f} @600, {40/s300/4.4:4.2f} @900 corner')
