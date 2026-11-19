import json, numpy as np, matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
D = json.load(open('sections_poly.json'))
GK = 0.10510423526567646; wallT = 17.5 - 1.5 * 0.0262
SLOT = np.array([[3.55, 1.0], [17.65, 1.0], [17.65, 1.6], [17.5, 3.0], [17.335, 9.3], [11.565, 15.118], [9.788, 18.87], [8.835, 22.282], [7.919, 23.746], [3.962, 18.5], [3.7, 8.5], [3.55, 7.7], [3.55, 1.0]])
T = np.array([[3.75, 1.0], [17.45, 1.0], [17.45, 3.0], [17.285, 9.3], [11.4, 15.0], [9.6, 18.8], [8.65, 22.2], [7.9, 23.4], [4.012, 18.5], [3.75, 8.5], [3.75, 1.0]])
G = np.array([[16.85, 3.0], [wallT, 3.0], [wallT, 4.0], [wallT + 0.5, 4.5 + (wallT + 0.5 - 17.65) * GK], [22.2, 4.5 + (22.2 - 17.65) * GK], [22.2, 9.0], [16.85, 9.0], [16.85, 3.0]])
TO = np.array([[17.15, 4.02], [wallT, 4.02], [wallT + 0.5, 4.52 + (wallT + 0.5 - 17.65) * GK], [22.0, 4.52 + 4.35 * GK], [22.0, 6.3], [19.5, 8.8], [17.15, 8.8], [17.15, 4.02]])
fig, axs = plt.subplots(1, 2, figsize=(16, 9))
for ax, z in zip(axs, ['227.0', '260.7']):
    for p in D[z]['filled']:
        p = np.asarray(p); ax.fill(p[:, 0], p[:, 1], color='#ccd', ec='k', lw=0.6)
    for p in D[z]['orig']:
        p = np.asarray(p); ax.plot(np.r_[p[:, 0], p[0, 0]], np.r_[p[:, 1], p[0, 1]], 'g-', lw=0.8)
    if z == '227.0':
        ax.plot(SLOT[:, 0], SLOT[:, 1], 'r-', lw=1.2, label='SLOT (FS)'); ax.plot(T[:, 0], T[:, 1], 'b--', lw=1, label='TONGUE (FS)')
        ax.plot(G[:, 0], G[:, 1], 'm-', lw=1, label='catch groove'); ax.plot(TO[:, 0], TO[:, 1], 'c-', lw=1.2, label='tooth latched')
        ax.plot(TO[:, 0] - 4.9, TO[:, 1], 'c:', lw=1, label='tooth fully pressed (-4.9)')
        ax.plot([21.95, 17.65], [1.0, 5.3], color='orange', lw=1.5, label='mouth chamfer 3.3x45')
        th = np.linspace(0, 2 * np.pi, 80); ax.plot(8.6 + 3.25 * np.cos(th), 0 * th + 0, alpha=0)
        ax.plot([10.66, 15.35], [17.03, 19.26], 'k-', lw=2); ax.text(13, 17.2, '5.19', fontsize=9)
        ax.axhline(2.0, color='gray', lw=0.5, ls=':')
    ax.set_aspect('equal'); ax.set_xlim(-12, 30); ax.set_ylim(-16, 45); ax.grid(alpha=0.3)
    ax.set_xlabel("d' (+ outboard)"); ax.set_ylabel("t' (+ into wing)")
    ax.set_title("S-frame z %s: grey = wing with FS fill, green = original wing" % z, fontsize=9); ax.legend(fontsize=7, loc='upper left')
plt.tight_layout(); plt.savefig('section_slot_S1.png', dpi=110); print('ok')
