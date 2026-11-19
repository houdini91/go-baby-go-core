# Option B (hook-and-drop + keystone) sketch: side elevations (view along +d: t right = into the wing, z up) and plans (d right = outboard, t up).
# Tip frame Q(d,t,z), chair mm. Carrier shown LOW at cells 1-2 (tongue centre c = 239.6), keystone in cell 3.
import numpy as np, matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle, Polygon, FancyBboxPatch, Circle
P = 19.15; Z0 = 201.3; NC = 7; c = Z0 + 2 * P; DROP = 8.0; SEAM = 0.3
LIP_T = (6.5, 12.5); LIP_H = 6.5; LIP_P = 3.0
GRV_T = (6.25, 12.75); CH = 5.075; NOTCH = 14.075; HALF = P            # tongue side pattern (rel. tongue centre)
def rear(d): return 19.1 + 1.31 * (d + 4) - 9.9                       # slot back = S1 front edge - 6 mm normal web
TR = rear(9.4) - 0.5
WING = '#cfcac0'; KIT = '#eef0f2'; KEY = '#ffd9a8'; FIL = '#dfe8f3'; LIP = '#8d8778'; ORG = '#ff6a2a'
def R(ax, x0, x1, y0, y1, fc, ec='k', lw=.6, **k): ax.add_patch(Rectangle((x0, y0), x1 - x0, y1 - y0, fc=fc, ec=ec, lw=lw, **k))
def side(ax, state, title):
    dz = DROP if state in ('up', 'lifted') else 0.0
    key_in = state == 'locked'
    # wing behind the cut face (t > 6): band frame + lips on the outer wall face (seen through the tongue)
    R(ax, 6, rear(17.6), Z0 - 6, Z0 + NC * P + 6, WING, lw=.4)
    ax.plot([6, 6], [Z0 - 6, Z0 + NC * P + 6], 'k', lw=1)
    ax.text(rear(17.6) - 1, Z0 + NC * P + 3, 'wing (outer U-wall\ninner face)', ha='right', va='bottom', fontsize=6)
    R(ax, -20, 6, Z0 - 6, Z0, WING, lw=.4); R(ax, -20, 6, Z0 + NC * P, Z0 + NC * P + 6, WING, lw=.4)
    # fillers: cell 0 below; cells 4-6 above (hang on their lips)
    for j in [0, 4, 5, 6]:
        R(ax, -14, TR, Z0 + j * P + SEAM / 2, Z0 + (j + 1) * P - SEAM / 2, FIL, lw=.5, alpha=.85)
    # keystone (cell 3)
    kz0, kz1 = Z0 + 3 * P + SEAM / 2, Z0 + 4 * P - SEAM / 2
    kt = 0 if key_in else -40
    R(ax, -14 + kt, TR + kt, kz0, kz1, KEY, lw=.7)
    R(ax, -9 + kt, 1 + kt, kz0 + 3.5, kz0 + 15.5, ORG, lw=.4)          # button 10 x 12
    R(ax, 15 + kt, 19 + kt, Z0 + 3.5 * P + 4.6, Z0 + 3.5 * P + 8.1, '#a33', lw=.4)   # bolt
    R(ax, 6.25 + kt, TR + kt, Z0 + 3.5 * P - 3.4, Z0 + 3.5 * P + 3.4, 'none', ec='#555', lw=.4, ls='--')
    ax.text(-14 + kt + 1, kz1 - 2.5, 'keystone' + ('' if key_in else ' (out)'), fontsize=6, va='top')
    # carrier
    zc = c + dz
    R(ax, -14, 6, zc - P + SEAM / 2, zc + P - SEAM / 2, KIT, lw=.9)
    R(ax, 6, TR, zc - P + SEAM / 2, zc + P - SEAM / 2, KIT, lw=.9)
    # arm start (LOW: goes forward and down)
    tt = np.linspace(0, 1, 30); at = -14 - 30 * tt; az = zc - 14 * tt ** 2 * 1.2
    ax.fill_between(at, az - 14 + 2 * tt, az + 14 - 2 * tt, color=KIT, ec='k', lw=.6)
    # side pattern: groove, channels, hook rows
    R(ax, *GRV_T, zc - P, zc + P, '#b9c6d6', lw=.3)
    for (a, b) in [(-CH, CH), (NOTCH, HALF), (-HALF, -NOTCH)]:
        R(ax, GRV_T[1], TR, zc + a, zc + b, '#b9c6d6', lw=.3)
    ax.plot([GRV_T[1]] * 2, [zc + CH, zc + NOTCH], 'r', lw=1.6); ax.plot([GRV_T[1]] * 2, [zc - NOTCH, zc - CH], 'r', lw=1.6)
    for j in range(NC):
        zl = Z0 + P * (j + .5); R(ax, *LIP_T, zl - LIP_H / 2, zl + LIP_H / 2, LIP, lw=.4, zorder=5)
    # Ø6 carbon rod (z c-10)
    ax.plot([-40, TR - 1], [zc - 10, zc - 10], color='#333', lw=2.2, alpha=.5)
    # seat
    if state == 'locked':
        ax.annotate('', (TR + 4, c - P), (TR + 4, c - P - 12), arrowprops=dict(arrowstyle='->', lw=.8))
        ax.text(TR + 5, c - P - 10, 'seat: nose on\nfiller below', fontsize=5.5)
        ax.text(GRV_T[1] + .6, zc - 9.6, 'hook faces\n(red) behind lips', fontsize=5.5, va='center')
    if state == 'up':
        ax.annotate('', (12, zc + 25), (-12, zc + 25), arrowprops=dict(arrowstyle='->', lw=1.2, color='b'))
        ax.annotate('', (0, c + 18), (0, c + 18 + DROP), arrowprops=dict(arrowstyle='->', lw=1.2, color='b'))
        ax.text(-12, zc + 27, '1 push back (raised 8)', fontsize=6, color='b'); ax.text(1, c + 20, '2 drop 8', fontsize=6, color='b')
    if state == 'lifted':
        ax.annotate('', (-45, kz0 + 9), (-10, kz0 + 9), arrowprops=dict(arrowstyle='->', lw=1.2, color='b'))
        ax.text(-60, kz0 + 12, '1 press + pull keystone', fontsize=6, color='b')
        ax.annotate('', (-20, c + DROP + 2), (-20, c - 4), arrowprops=dict(arrowstyle='->', lw=1.2, color='b'))
        ax.text(-34, c - 9, '2 lift 8', fontsize=6, color='b')
        ax.annotate('', (-55, c - 25), (-25, c - 25), arrowprops=dict(arrowstyle='->', lw=1.2, color='b'))
        ax.text(-60, c - 30, '3 pull forward', fontsize=6, color='b')
    for j in range(NC + 1): ax.text(rear(17.6) + 1, Z0 + j * P, f'{Z0 + j * P:.2f}', fontsize=5, va='center')
    ax.set_title(title, fontsize=8); ax.set_xlim(-62, 48); ax.set_ylim(Z0 - 8, Z0 + NC * P + 10); ax.set_aspect('equal')
    ax.set_xlabel('t (mm, + into the wing)', fontsize=6); ax.set_ylabel('z (mm)', fontsize=6); ax.tick_params(labelsize=5)
def wingplan(ax):
    dd = np.linspace(-6, 23.6, 20)
    ax.add_patch(Polygon([(-6, 6), (1.2, 6), (1.2, rear(1.2)), (17.6, rear(17.6)), (17.6, 6), (23.6, 6), (23.6, rear(23.6) + 9.9), (-6, rear(-6) + 9.9)], fc=WING, ec='k', lw=.5))
    ax.plot(dd, 19.1 + 1.31 * (dd + 4), 'r:', lw=.8); ax.text(-5, 19.1 + 1.31 * (-1) + 1, 'S1 front edge', fontsize=5, color='r', rotation=52)
def nose(ax, fc, dt=0, ec='k'):
    ax.add_patch(FancyBboxPatch((-6 + .01, -14 + dt), 29.6 - .02, 20, boxstyle='round,pad=0,rounding_size=6', fc=fc, ec=ec, lw=.8))
def plan_hook(ax):
    wingplan(ax); nose(ax, KIT)
    ax.add_patch(Polygon([(1.4, 6), (17.4, 6), (17.4, rear(17.4) - .5), (1.4, rear(1.4) - .5)], fc=KIT, ec='k', lw=.8))
    for (a, b) in [(1.4, 4.7), (14.1, 17.4)]: R(ax, a, b, GRV_T[0], GRV_T[1], '#b9c6d6', lw=.3)
    for (a, b) in [(1.2, 1.2 + LIP_P), (17.6 - LIP_P, 17.6)]: R(ax, a, b, *LIP_T, LIP, lw=.4)
    ax.plot([1.4, 4.7], [GRV_T[1]] * 2, 'r', lw=1.6); ax.plot([14.1, 17.4], [GRV_T[1]] * 2, 'r', lw=1.6)
    ax.add_patch(Circle((9.4, 0), 3.25, fc='#555', alpha=.5)); ax.plot([9.4, 9.4], [-40, rear(9.4) - 1.5], color='#333', lw=1, ls='--')
    ax.fill_between([0.4, 18.4], [-14, -14], [-40, -40], color=KIT, ec='k', lw=.6)
    ax.text(9.4, -30, 'arm 18 wide', ha='center', fontsize=6); ax.text(9.4, 20, 'tongue', ha='center', fontsize=6)
    ax.text(-5, 3, 'lips 3.0 x 6\n(both walls)', fontsize=5.5); ax.text(10.5, 2.0, 'Ø6.5 rod\n(z c-10)', fontsize=5)
    ax.set_title('(d) plan at the lower hook row z = c - 9.6 (dropped)', fontsize=8)
def plan_key(ax, pressed):
    wingplan(ax); nose(ax, KEY)
    ax.add_patch(Polygon([(1.4, 6), (17.4, 6), (17.4, rear(17.4) - .5), (1.4, rear(1.4) - .5)], fc=KEY, ec='k', lw=.8))
    R(ax, 17.6, 19.8, 14.8, 19.2, 'white', lw=.5)                           # blind pocket in the outer wall
    s = -4.0 if pressed else 0.0
    # slider: button cap (d 23.6, t -9..1) + bar to the bolt (t 15..19)
    R(ax, 19.6 + s, 23.6 + s, -9, 1, ORG, lw=.5); R(ax, 8 + s, 19.6 + s, -6, -2, '#ffb07a', lw=.4)
    R(ax, 14.5 + s, 17.4 + s, -2, 19, '#ffb07a', lw=.4); R(ax, 15 + s, 19.6 + s, 15, 19, '#a33', lw=.4)
    # spring along d (fixed seat at d 2.5)
    xs = np.linspace(2.5, 8 + s, 15); ax.plot(xs, -4 + 1.8 * np.sign(np.sin(np.arange(15) * np.pi / 1.0 + .5)), 'k', lw=.6)
    ax.annotate('', (19.6, -11), (27.6, -11), arrowprops=dict(arrowstyle='->', color=ORG, lw=1.4))
    ax.text(24, -13.5, 'thumb', fontsize=6, color=ORG)
    ax.text(-5, 22, 'bolt 2.0 in pocket' if not pressed else 'bolt clear (-0.5)', fontsize=6)
    ax.set_title('(f) keystone plan at the bolt, PRESSED 4' if pressed else '(e) keystone plan at the bolt z = cell centre + 6.3, LOCKED', fontsize=8)
fig, axs = plt.subplots(2, 3, figsize=(17, 13))
side(axs[0, 0], 'up', '(a) inserted, up: pushed back raised 8 (keystone cell empty)')
side(axs[0, 1], 'locked', '(b) dropped + keystone in (locked)')
side(axs[0, 2], 'lifted', '(c) keystone pressed + out, carrier lifted 8: pull forward')
plan_hook(axs[1, 0]); plan_key(axs[1, 1], False); plan_key(axs[1, 2], True)
for a in axs[1]: a.set_xlim(-12, 32); a.set_ylim(-42, 48); a.set_aspect('equal'); a.set_xlabel('d (mm, + outboard)', fontsize=6); a.set_ylabel('t (mm, + into the wing)', fontsize=6); a.tick_params(labelsize=5)
fig.suptitle('Option B hook-and-drop + keystone (LOW, carrier cells 1-2, c 239.6). Grid 19.15, 7 cells z 201.3-335.35; drop 8; lips 3.0 d x 6.5 z x 6 t at every cell centre, both U-walls (dark)\n'
             'Tongue side faces: groove t 6.25-12.75 x 3.3 deep (full height); channels to the rear face at z c+-5.1 and c+-14.1..19.15; hook rows z c+-(5.1..14.1), hook face t 12.75 (red)', fontsize=9)
plt.tight_layout(rect=(0, 0, 1, .96)); plt.savefig('hookdrop_B_sketch.png', dpi=110)
