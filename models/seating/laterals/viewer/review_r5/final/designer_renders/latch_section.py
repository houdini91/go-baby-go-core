# Designer review r5: plan section of the latch at the notch (S frame d', t') from the ORIGINAL wing mesh + the r5 FS constants.
# Shows the slot, the 3.3 x 45 mouth chamfer, the catch groove, the tooth, the button stack, and the cap's snap bump.
# Run from viewer/r5mesh: ../.venv/bin/python ../review_r5/final/designer_renders/latch_section.py
import sys, numpy as np, trimesh, matplotlib
matplotlib.use('Agg'); import matplotlib.pyplot as plt
sys.path.insert(0, '.')
from frame import WING, M_W2F
LD, LT, ZC = 0.0565, 0.069, 227.0
F2S = np.array([[1, 0, -LD, LD * ZC], [0, 1, -LT, LT * ZC], [0, 0, 1, 0], [0, 0, 0, 1]], float)
W2S = F2S @ M_W2F
OUT = '../review_r5/final/designer_renders/'

w = trimesh.load(WING)
if w.bounds.max() < 5: w.apply_scale(1000)
w.apply_transform(W2S)                       # z unchanged by the shear, so a z plane in S = the same z plane in world

# FS r5 constants (gbg_sidepad_r5.fs)
SLOT = [[3.55, 1.0], [17.65, 1.0], [17.65, 1.6], [17.5, 3.0], [17.335, 9.3], [11.565, 15.118], [9.788, 18.87], [8.835, 22.282], [7.919, 23.746], [3.962, 18.5], [3.7, 8.5], [3.55, 7.7]]
T_BACK, MOUTH, GR_T, GR_D, GR_CH, GK = 2.0, 3.3, 4.5, 22.2, 0.5, np.tan(np.radians(6))
TO_D, BLK_T = (17.15, 22.0), (-4.0, 9.0)
FD, BT_THK, TRAVEL, LIP, LIP_W, FL_W, SL_OUT, SL_IN = 24.6, 4.6, 4.9, 1.6, 13.2, 13.0, 13.8, 12.4
BT_TC, BT_W = -6.3, 12.0
wallT = 17.5 - (GR_T - 3.0) * 0.0262
groove = [[16.85, GR_T - GR_CH - 1.0], [wallT, GR_T - GR_CH - 1.0], [wallT, GR_T - GR_CH], [wallT + GR_CH, GR_T + (wallT + GR_CH - 17.65) * GK], [GR_D, GR_T + (GR_D - 17.65) * GK], [GR_D, BLK_T[1]], [16.85, BLK_T[1]]]
tf1 = GR_T + 0.02 + (TO_D[1] - 17.65) * GK
tooth = [[TO_D[0], GR_T - GR_CH + 0.02], [wallT, GR_T - GR_CH + 0.02], [wallT + GR_CH, GR_T + 0.02 + (wallT + GR_CH - 17.65) * GK], [TO_D[1], tf1], [TO_D[1], 6.3], [TO_D[1] - 2.5, BLK_T[1] - 0.2], [TO_D[0], BLK_T[1] - 0.2]]
bump = [[17.2, 5.0], [19.0, 5.4], [19.0, 6.2], [17.2, 8.2]]           # blank-cap snap bump (cap seated)
finger = [[15.4, 2.6], [17.45, 2.6], [17.45, 3.0], [17.29, 9.0], [15.4, 9.0]]

fig, axs = plt.subplots(1, 2, figsize=(16, 8.5))
for ax, (lo, hi, title) in zip(axs, [((-2, 30), (-20, 26), 'z 235 plan section (S frame): latch, slot, groove'), ((14, 26), (0, 10), 'zoom: the wing shoulder in front of the catch groove')]):
    sec = w.section(plane_origin=[0, 0, 235.0], plane_normal=[0, 0, 1])
    if sec is not None:
        for e in sec.discrete:
            ax.plot(e[:, 0], e[:, 1], color='0.35', lw=1.0)
    ax.fill(*np.array(SLOT + [SLOT[0]]).T, color='#cfe3f7', ec='#1f6fbf', lw=1, label='slot (tongue + 0.2)')
    # mouth chamfer outline (offset = 5.3 - t' for t' 1..5.3)
    tt = np.linspace(1.0, T_BACK + MOUTH, 20)
    ax.plot(17.65 + (T_BACK + MOUTH - tt), tt, color='#d62728', lw=2, label='mouth chamfer 3.3x45 (outer side)')
    ax.plot(3.55 - (T_BACK + MOUTH - tt), tt, color='#d62728', lw=2)
    ax.fill(*np.array(groove + [groove[0]]).T, color='#fde2c3', ec='#e07b00', lw=1, label='catch groove (6 deg face at t\' 4.5)')
    ax.fill(*np.array(tooth + [tooth[0]]).T, color='#9467bd', alpha=0.6, ec='#5b3a8a', lw=1, label='button tooth (latched)')
    ax.plot(*np.array(bump + [bump[0]]).T, color='green', lw=1.5, ls='--', label='blank-cap snap bump (seated)')
    ax.plot(*np.array(finger + [finger[0]]).T, color='green', lw=0.8, ls=':', label='blank-cap finger 2.05 thick')
    ax.axhline(T_BACK, color='k', lw=0.8, ls='--'); ax.text(lo[0] + 0.3, T_BACK + 0.15, "notch face t' 2.0", fontsize=8)
    # button stack at t' BT_TC +- w/2, d' FD - thk .. FD
    ax.add_patch(plt.Rectangle((FD - BT_THK, BT_TC - BT_W / 2), BT_THK, BT_W, fc='#bbbbbb', ec='k', lw=0.8))
    ax.add_patch(plt.Rectangle((FD - LIP, BT_TC - LIP_W / 2), LIP, LIP_W, fc='none', ec='#d62728', lw=0.8, ls=':'))
    ax.add_patch(plt.Rectangle((FD - BT_THK + 0.6, BT_TC - SL_OUT / 2), 2.4, SL_OUT, fc='#ff7f0e', alpha=0.5, ec='#ff7f0e'))
    ax.set_xlim(*lo); ax.set_ylim(*hi); ax.set_aspect('equal'); ax.grid(alpha=0.3)
    ax.set_xlabel("d' (mm, + outboard)"); ax.set_ylabel("t' (mm, + into the wing, - = front)"); ax.set_title(title)
axs[0].legend(loc='lower left', fontsize=8)
# shoulder thickness (t' from the chamfer / notch face to the groove face) across d'
dd = np.linspace(17.65, 22.2, 200)
t0 = np.maximum(T_BACK, T_BACK + MOUTH - (dd - 17.65))
tfac = GR_T + (dd - 17.65) * GK
th = np.clip(tfac - t0, 0, None)
ax = axs[1]
for d_, t_ in [(18.45, None), (19.0, None), (20.0, None), (21.0, None)]:
    i = np.argmin(abs(dd - d_)); ax.annotate(f'{th[i]:.2f}', (d_, (t0[i] + tfac[i]) / 2), fontsize=9, color='#d62728', ha='center')
ax.text(14.3, 0.5, 'red numbers = shoulder thickness (mm) left after the mouth chamfer', fontsize=9, color='#d62728')
plt.tight_layout(); plt.savefig(OUT + 'D1_latch_plan_section_z235.png', dpi=110)

# outer skin at the button: d' of the original wing's outer face at t' -14..2, z 215..240
pts = []
for z in [215, 221, 227, 233, 239, 245]:
    s = w.section(plane_origin=[0, 0, z], plane_normal=[0, 0, 1])
    for e in s.discrete:
        m = (e[:, 1] > -14.5) & (e[:, 1] < 2) & (e[:, 0] > 15)
        if m.any(): pts.append((z, e[m, 0].max(), e[m, 0].min()))
print('outer skin d\' max at t\' -14..2 per z:', [(z, round(a, 2)) for z, a, b in pts])
# ledge bending of the shoulder (cantilever from d' 22.2, loaded on its t' 4.5 face) vs tooth root
F = 900.0; L = 23.1
cnt = (dd >= 18.45) & (dd <= 22.0)
lever = 22.2 - dd[cnt].mean()
for tsh in [2.5, 4.0, 4.5]:
    print(f'shoulder {tsh} thick: ledge bending 6 F e / (L t^2) = {6 * F * lever / (L * tsh ** 2):.0f} MPa (F {F} N, lever {lever:.2f}, L {L})')
print('tooth root (FS calc check): ', round(6 * F * 2.6 / (22.5 * 5.0 ** 2), 1), 'MPa')
print('bearing contact d\' 18.45..22.0 =', round(22.0 - 18.45, 2), 'mm ->', round(F / ((22.0 - 18.45) * 22.5), 1), 'MPa')
# blank-cap finger strain to pass the shoulder (bump d' 19.0 vs chamfered wall at the groove front 18.45)
for a_, dl in [(3.2, 0.55), (6.4, 0.55), (3.2, 1.35)]:
    print(f'cap finger: lever {a_} defl {dl} -> strain {1.5 * 2.05 * dl / a_ ** 2 * 100:.1f} %')
print('cap bump locking face angle from d axis:', round(np.degrees(np.arctan2(0.4, 1.8)), 1), 'deg  (cams out only if mu < tan =', round(0.4 / 1.8, 2), ')')
