# orientation alternative (layers normal to z), section-width scaling, creep sag at the pad (100 N, 60 C)
from analyse import *
import analyse
us = list(range(0, 60, 4)) + [58]
rows = []
for u in us:
    p0, tn, e1, e2 = frame_arm(u); S = section(T, p0, tn, e1, e2); B = classify(S, p0, e1, e2)
    m = MAT['RT']; F, P, _ = LOADS['knock300']; Mv = np.cross(P - p0, F)
    r = solve(S, B, set(), m, Mv @ e1, Mv @ e2, F @ tn)
    smax = np.abs(r['sig']).max(); tau, J = torsion_tau(S, Mv @ tn)
    sf_d = 1 / math.sqrt((smax / tsai_hill(abs(tn @ dirD), m)) ** 2 + (tau / (m['S'] * (1 - .35 * abs(tn @ dirD)))) ** 2)
    sf_z = 1 / math.sqrt((smax / tsai_hill(abs(tn[2]), m)) ** 2 + (tau / (m['S'] * (1 - .35 * abs(tn[2])))) ** 2)
    # creep sag: unit-load method, 100 N outward at the pad, E creep 60C
    mh = MAT['H60']; Fh = np.array([100., 0, 0]); Mh = np.cross(P - p0, Fh); M1, M2, Tq = Mh @ e1, Mh @ e2, Mh @ tn
    out = []
    for use, bond in [(set(), 'bond'), ({'D4', 'D3'}, 'dowel'), ({'D4', 'D3'}, 'bond')]:
        rr = solve(S, B, use, mh, M1, M2, Fh @ tn, bond=bond); K11, K22, K12 = rr['K']
        Kinv = np.linalg.inv([[K11, K12], [K12, K22]]); v = np.array([-M2, M1])
        out.append(v @ Kinv @ v / 100 + Tq ** 2 / (mh['Ep'] / 2.7 * J) / 100)   # mm per mm of arm
    rows.append((u, sf_d, sf_z, out))
    print(f'u {u:3} sin_d {abs(tn@dirD):.2f} sin_z {abs(tn[2]):.2f}  SF(d-down) {sf_d:.2f}  SF(z-down) {sf_z:.2f}  sag/mm {np.round(out,5)}')
R = np.array([r[3] for r in rows]); uu = np.array([r[0] for r in rows])
sag = np.trapz(R, uu, axis=0)
print('pad sag 100 N, 60 C, creep E 0.8 GPa, arm only (mouth clamped): none / dowel / bonded =', np.round(sag, 2), 'mm')
