# Designer review r5: the depth piece's foot face (y-z, at the pad back) vs the two dovetail grooves at +-11, from the FS constants.
# rrProfile(w 27, h 40, r 14.5) -> r >= min(a,b): STADIUM with semicircular top/bottom (r 13.5 centred at z +-6.5).
# Run: viewer/.venv/bin/python foot_face.py
import numpy as np, matplotlib
matplotlib.use('Agg'); import matplotlib.pyplot as plt
FOOT_Y = (19.0, 46.0); FOOT_HZ = 20.0; DV = (9.0, 12.0, 2.5); DV_CL = 0.15; DV_FRONT = 22.0; PAD_OFF = 11.0; PS_Y = 32.5
fw = FOOT_Y[1] - FOOT_Y[0]; yc = sum(FOOT_Y) / 2; a = fw / 2; b = FOOT_HZ; ra = a
def H(y, hz=b, r=None):                          # half-height of the foot outline at y (stadium, or rounded rect with radius r)
    u = np.abs(y - yc)
    if r is None:
        return np.where(u <= a, (hz - a) + np.sqrt(np.clip(a * a - u * u, 0, None)), np.nan)
    cu = a - r; cz = hz - r
    return np.where(u <= cu, hz, np.where(u <= a, cz + np.sqrt(np.clip(r * r - (u - cu) ** 2, 0, None)), np.nan))
sl = (DV[1] - DV[0]) / (2 * DV[2])
w_m = 2 * (DV[0] / 2 + DV_CL - sl * 1.0); w_b = 2 * (DV[0] / 2 + DV_CL + sl * (DV[2] + 0.1))
yy = np.linspace(FOOT_Y[0], FOOT_Y[1], 400)
fig, axs = plt.subplots(1, 2, figsize=(14, 7))
for ax, (hz, r, ttl) in zip(axs, [(20.0, None, 'AS BUILT: foot 27 x 40 stadium (r 14.5 -> semicircle ends)'), (24.0, 10.0, 'FIX: foot 27 x 48, R10 corners')]):
    h = H(yy, hz, r)
    ax.fill_between(yy, -h, h, color='#d9d9d9', label='foot face at the pad back')
    for s in (1, -1):
        zr = s * PAD_OFF
        for w, c, lab in ((w_b, '#e07b00', 'groove at full depth (12.4 wide)'), (w_m, '#1f6fbf', 'groove mouth (8.1 wide)')):
            ax.add_patch(plt.Rectangle((DV_FRONT, zr - w / 2), FOOT_Y[1] + 5 - DV_FRONT, w, fc='none', ec=c, lw=1.5, label=lab if s > 0 else None))
        ax.add_patch(plt.Circle((PS_Y, zr), 4.8, fc='none', ec='k', lw=0.8, ls=':'))
        # break-out spans: deep groove edge within 1.2 of the outline
        top = abs(zr) + w_b / 2
        bad = h < top + 1.2
        m = bad & (yy >= DV_FRONT); ax.plot(yy, np.where(m, s * h, np.nan), 'r', lw=3, label='groove breaks out / wall < 1.2' if s > 0 else None)
        ok = yy[(~bad) & (yy >= DV_FRONT)]
        if len(ok): ax.text(yc, s * (hz + 2.5), f'full dovetail flank y {ok.min():.1f}..{ok.max():.1f} ({ok.max() - ok.min():.1f} of {FOOT_Y[1] - DV_FRONT:.0f} mm)', ha='center', fontsize=9)
        stop = H(np.linspace(FOOT_Y[0], DV_FRONT, 50), hz, r)
        print(ttl, 'z', zr, ': closed-front stop exists up to |z|', round(float(np.nanmax(stop)), 2), ' rail end spans', round(abs(zr) - w_b / 2, 2), '..', round(abs(zr) + w_b / 2, 2), '| full flank y', (round(ok.min(), 1), round(ok.max(), 1)) if len(ok) else None)
    ax.axvline(DV_FRONT, color='k', lw=0.7, ls='--'); ax.text(DV_FRONT + 0.2, -hz - 3, 'closed front y 22', fontsize=8)
    ax.set_xlim(17, 52); ax.set_ylim(-30, 30); ax.set_aspect('equal'); ax.grid(alpha=0.3)
    ax.set_xlabel('y (mm, + = toward the chair back)'); ax.set_ylabel('z about the arm axis (mm)'); ax.set_title(ttl, fontsize=10)
axs[0].legend(loc='lower left', fontsize=7)
plt.tight_layout(); plt.savefig('D2_foot_face_grooves.png', dpi=110)
