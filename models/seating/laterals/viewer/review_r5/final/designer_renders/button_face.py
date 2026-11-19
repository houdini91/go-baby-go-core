# Designer review r5: the button on the nose's outer face (looking -d), with the hidden pockets behind it, from the FS constants;
# plus a side section of a thumb against the flush button (how deep a thumb can follow vs the stroke the latch needs).
# Run: viewer/.venv/bin/python button_face.py
import numpy as np, matplotlib
matplotlib.use('Agg'); import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch
NOSE_T = (-14.27, 1.7); NOSE_Z = (207.3, 246.7); TC, C = -6.3, 227.0
def stadium(ax, w, h, **kw):
    ax.add_patch(FancyBboxPatch((TC - w / 2 + w / 2, C - h / 2 + w / 2), 0, h - w, boxstyle=f'round,pad={w / 2}', **kw))
fig, axs = plt.subplots(1, 2, figsize=(15, 8), gridspec_kw={'width_ratios': [1, 1.3]})
ax = axs[0]
ax.add_patch(plt.Rectangle((NOSE_T[0], NOSE_Z[0]), NOSE_T[1] - NOSE_T[0], NOSE_Z[1] - NOSE_Z[0], fc='#eeeeee', ec='k'))
stadium(ax, 14.0, 26.0, fc='#ffd9b3', ec='#ff7f0e', lw=1, ls='--', label='sleeve counterbore 14.0 x 26.0 (d 20.6..23.0)')
stadium(ax, 13.4, 25.4, fc='none', ec='#1f6fbf', lw=1, ls=':', label='flange pocket 13.4 x 25.4 (d 15.1..20.6)')
stadium(ax, 13.2, 25.2, fc='white', ec='#d62728', lw=1.2, label='lip opening 13.2 x 25.2 (1.6 deep)')
stadium(ax, 12.4, 24.4, fc='#ff7f0e', ec='none', alpha=0.9, label='orange sleeve top face, 1.6 below the face')
stadium(ax, 12.0, 24.0, fc='#c8c8c8', ec='k', lw=1, label='button 12 x 24 (flush = latched)')
ax.add_patch(plt.Rectangle((-13.0, 214.5), 13.4, NOSE_Z[1] - 214.5, fc='none', ec='#2ca02c', lw=1, ls='-.', label='drop-in channel to the nose top (d 15.1..20.3)'))
for x0, x1, y, lab in [(NOSE_T[0], TC - 7.0, 236, '0.97'), (TC + 7.0, NOSE_T[1], 236, '1.00'), (NOSE_T[0], TC - 6.7, 218, '1.27'), (TC + 6.7, NOSE_T[1], 218, '1.30')]:
    ax.annotate('', (x0, y), (x1, y), arrowprops=dict(arrowstyle='<->', lw=0.8)); ax.text((x0 + x1) / 2, y + 0.6, lab, ha='center', fontsize=9, color='#d62728')
ax.text(NOSE_T[0] + 0.2, NOSE_Z[0] + 0.6, 'nose front edge (arm root side)', fontsize=7, rotation=90, va='bottom')
ax.text(NOSE_T[1] - 0.2, NOSE_Z[0] + 0.6, 'seam to the wing (hairline 0.3)', fontsize=7, rotation=90, va='bottom', ha='right')
ax.set_xlim(-17, 4); ax.set_ylim(204, 250); ax.set_aspect('equal'); ax.grid(alpha=0.25)
ax.set_xlabel("t' (mm, - = front)"); ax.set_ylabel('z (mm)'); ax.set_title('Nose outer face: walls round the button stack (red = mm)', fontsize=10)
ax.legend(loc='upper center', bbox_to_anchor=(0.5, -0.08), fontsize=7.5, ncol=1)
# thumb vs stroke
ax = axs[1]
open_w, stroke_need, stroke_hard = 13.2, 4.4, 4.9
ax.fill_between([-20, -open_w / 2], 0, 6, color='#dddddd'); ax.fill_between([open_w / 2, 20], 0, 6, color='#dddddd')
ax.plot([-20, -open_w / 2, -open_w / 2], [6, 6, 0], 'k', lw=1); ax.plot([20, open_w / 2, open_w / 2], [6, 6, 0], 'k', lw=1)
for y, c, lab in [(6 - stroke_need, '#d62728', f'button face at release ({stroke_need} below flush: tooth 4.35 out of the groove)'), (6 - stroke_hard, 'k', f'hard stop ({stroke_hard})'), (6 - 3.0, '#2ca02c', 'typical bare-thumb reach into a 13 mm opening (~2.5-3.5, to verify on the coupon)')]:
    ax.plot([-6, 6], [y, y], color=c, lw=2); ax.text(6.5, y + (0.35 if c == "#d62728" else (-0.35 if c == "k" else 0)), lab, fontsize=8, va='center', color=c)
xx = np.linspace(-10, 10, 200); thumb = 6 + 0.05 * xx ** 2 - 3.0 * np.exp(-(xx / 4.0) ** 2)
ax.plot(xx, np.maximum(thumb, 6 - 3.0), color='#c98e6b', lw=2, label='thumb pad (16-20 wide) bulging into the opening')
ax.set_xlim(-20, 34); ax.set_ylim(-1, 12); ax.set_aspect('equal'); ax.set_xlabel('across the button (mm)'); ax.set_ylabel('d (mm)')
ax.set_title('Release stroke vs what a thumb can follow into a 13.2 opening', fontsize=10); ax.legend(loc='upper left', fontsize=8)
plt.tight_layout(); plt.savefig('D3_button_face_and_stroke.png', dpi=110)
