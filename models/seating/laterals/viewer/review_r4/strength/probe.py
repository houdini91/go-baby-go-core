from sections import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
T = load('ps1_RUgD')
p = armPath(c, zp); print('arm L', round(p['L'],1), 'R', round(p['R'],1), 'Rv', round(p['Rv'],1), 'st', p['st'].round(1))
us = [-8, -4, 0, 2, 4, 6, 8, 10, 12, 14, 16, 19, 22, 26, 30, 36, 42, 48]
fig, axs = plt.subplots(3, 6, figsize=(24, 12))
for ax, u in zip(axs.flat, us):
    if u < 0:
        p0 = Q(9.4, TAPER_T - u, c); tn = -dirT
    else:
        p0, tn = armAt(p, u)
    e2 = np.array([0, 0, 1.0]) - tn[2] * tn; e2 /= np.linalg.norm(e2); e1 = np.cross(e2, tn)
    S = section(T, p0, tn, e1, e2)
    for s in S:
        P = np.vstack([s['P'], s['P'][:1]]); ax.plot(P[:, 0], P[:, 1], 'k-' if s['solid'] else 'r-', lw=.8)
        if not s['solid']: ax.text(*s['cen'], f"{s['r_eq']:.2f}", fontsize=7, color='r')
    A = sum(s['pr'][0] for s in S)
    ax.set_title(f'u {u}  A {A:.0f}  t-axis.dirD {abs(tn@dirD):.2f}'); ax.set_aspect('equal'); ax.grid(alpha=.3)
    ax.plot(0, 0, 'b+')
plt.tight_layout(); plt.savefig('probe_sections.png', dpi=70)
