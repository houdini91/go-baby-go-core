# Tongue root (slot mouth) section and contact bearing, from the contact model forces.
import numpy as np, pickle, math
from geom import *
import loads as L
H = 0.05
def tongue_section(t_s, with_bore=True):
    """pixel mask of the tongue cross-section (d', z) at t' = t_s (FS geometry): tongue polygon width at t_s,
    minus latch channels (bch d' 7.9-17.45 t' -4.3..9.3; tch d' 12.05-22.6 t' 2.7..9.3) above z 222.9, minus the rod teardrop."""
    d = np.arange(0, 25, H) + H / 2; z = np.arange(210, 250, H) + H / 2
    D, Z = np.meshgrid(d, z, indexing='ij')
    # tongue d-extent at t_s
    lo = 3.75 if t_s <= 8.5 else 3.75 + 0.262 * (t_s - 8.5) / 10
    hi = 17.45 if t_s <= 3 else 17.45 - 0.165 * (t_s - 3) / 6.3
    m = (D >= lo) & (D <= hi) & (Z >= TG_Z[0]) & (Z <= TG_Z[1])
    if -4.3 <= t_s <= 9.3: m &= ~((D >= 7.9) & (D <= 17.45) & (Z >= 222.9))
    if 2.7 <= t_s <= 9.3: m &= ~((D >= 12.05) & (Z >= 222.9))
    if with_bore and -22 <= t_s <= 26:
        r = ROD[2]; dd = D - ROD[0]; zz = Z - ROD[1]
        circ = dd ** 2 + zz ** 2 <= r ** 2
        tip = (dd <= 0) & (-dd <= r * math.sqrt(2) - np.abs(zz)) & (np.abs(zz) <= r * math.sqrt(0.5) * 1.0001 + (-dd - r * math.sqrt(0.5)) * 0 + 99)
        tip &= (np.abs(zz) <= (r * math.sqrt(2) + dd))          # 45 deg flanks toward -d
        m &= ~(circ | tip)
    return D, Z, m
def props(D, Z, m):
    A = m.sum() * H * H; dc = D[m].mean(); zc = Z[m].mean()
    Idd = ((Z[m] - zc) ** 2).sum() * H * H          # about the d axis (bending from z loads / pitch)
    Izz = ((D[m] - dc) ** 2).sum() * H * H          # about the z axis (bending from d loads / yaw)
    Idz = ((D[m] - dc) * (Z[m] - zc)).sum() * H * H
    return A, dc, zc, Idd, Izz, Idz
def section_check(rows, t_s, rod_cap=None):
    """worst over configs: stresses at the tongue section t' = t_s from all tongue contacts beyond it"""
    D, Z, m = tongue_section(t_s); A, dc, zc, Idd, Izz, Idz = props(D, Z, m)
    tongue_groups = ('inner wall', 'outer wall', 'rear faces', 'tooth (latch)', 'tongue bottom', 'tongue top')
    best = None
    for r in rows:
        f, Apts, names, grp = r['fpt'], r['A'], r['names'], r['grp']
        F = np.zeros(3); M = np.zeros(3)
        for gi, n in enumerate(names):
            if n not in tongue_groups: continue
            s = grp == gi
            for fi, a in zip(f[s], Apts[s]):
                if fi <= 0: continue
                p = a[:3].copy()
                if n == 'tooth (latch)': p = np.array([15.1, BLK_T[1] + 0.3, a[2]])   # enters the carrier at the block back face
                if p[1] <= t_s: continue
                fv = fi * a[3:6]; F += fv; M += np.cross(p - np.array([dc, t_s, zc]), fv)
        # section: normal t. N = F_t, V_d, V_z, T = M_t, M_d (bending about d -> sigma from z), M_z (about z -> sigma from d)
        N, Vd, Vz, Md, T, Mz = F[1], F[0], F[2], M[0], M[1], M[2]
        # sigma_t(d, z) = N/A + (Mz-related) ... generalized (unsymmetric) bending
        dd = D[m] - dc; zz = Z[m] - zc
        # stress from moments: sigma = N/A + a*dd + b*zz with [Izz Idz; Idz Idd][a b] = [-Mz, Md] (sign: M = r x F)
        a, b = np.linalg.solve([[Izz, Idz], [Idz, Idd]], [-Mz, Md])
        sig = N / A + a * dd + b * zz
        smax = np.abs(sig).max()
        J = (23.5 * 4.15 ** 3 + 13.7 * 1.2 ** 3 + 5.6 * 8.6 ** 3 + 1.6 * 6.5 ** 3) / 3     # open L: strip + floor + outer block + inner wall
        tau_T = abs(T) * 8.6 / J if t_s < 9.3 else 0
        tau_d = 1.5 * abs(Vd) / A; tau_z = 1.5 * abs(Vz) / A
        FI = (smax / MAT['Xa']) ** 2 + ((tau_d + tau_T) / 18.0) ** 2 + (tau_z / MAT['S']) ** 2
        SF = 1 / math.sqrt(FI)
        if best is None or SF < best['SF']:
            best = dict(SF=SF, smax=smax, tau_d=tau_d, tau_T=tau_T, N=N, Vd=Vd, Vz=Vz, Md=Md / 1000, T=T / 1000, Mz=Mz / 1000, cfg=r['cfg'], corner=r['corner'])
    return best, (A, dc, zc, Idd, Izz)
def bearing(rows, group, area):
    w = None
    for r in rows:
        F = r['res'][group]['F']
        if w is None or F > w[0]: w = (F, r['res'][group]['pmax'], r['cfg'])
    return dict(F=w[0], p_avg=w[0] / area, p_peak=w[1], SF=MAT['brg'] / (w[0] / area) if w[0] > 0 else 99, cfg=w[2])
if __name__ == '__main__':
    for t_s in (2.0, 4.7, 5.3):
        D, Z, m = tongue_section(t_s); A, dc, zc, Idd, Izz, Idz = props(D, Z, m)
        print(f"section t' {t_s}: A {A:.1f} mm2  centroid d' {dc:.2f} z {zc:.2f}  Idd {Idd:.0f}  Izz {Izz:.0f} mm4   (13.7 x 32.1 solid would be A 440, Izz {32.1*13.7**3/12:.0f})")
    for gm in ('zero', 'cad'):
        C_ = pickle.load(open(f'cases_{gm}.pkl', 'rb'))
        print(f'\n===== gaps: {gm}')
        for cn, rows in C_.items():
            b, _ = section_check(rows, 4.7)
            ow = bearing(rows, 'outer wall', 33.7); iw = bearing(rows, 'inner wall', 423.8); nb = bearing(rows, 'nose back face', 384.1)
            tt = bearing(rows, 'tooth (latch)', 80.0)
            print(f"{cn:26s} tongue@t'4.7 SF {b['SF']:4.2f} (sig {b['smax']:5.1f}, tau_d {b['tau_d']:4.1f}, tauT {b['tau_T']:4.1f}; Mz {b['Mz']:5.1f} Md {b['Md']:5.1f} T {b['T']:5.1f} N.m, {b['cfg']}) | "
                  f"outer wall {ow['F']:5.0f} N p {ow['p_avg']:5.1f} SF {ow['SF']:4.2f} | inner {iw['F']:5.0f} N SF {iw['SF']:4.2f} | nose back {nb['F']:5.0f} N SF {nb['SF']:4.2f} | tooth {tt['F']:5.0f} N")
