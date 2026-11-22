# Swept insertion check for X: pose the kit's plug + collar along the WHOLE connect path and measure its overlap with the wing,
# so a socket carved for the seated pose only (the old beak problem) can't recur. Also checks every part in the seated pose.
# Motion (as gen_xi.py): phase A = tilted PHI0 (nose up), slid straight in along the tilted axis from sh 70 to S_CAM;
# phase B = the last S_CAM of push while rotating PHI0 -> 0 about the pitch axis (TP, ZP). Pose maps plug-local -> S:
# rotate (t, z) by phi about (TP, ZP), then slide out by sh along (cos phi, sin phi) reversed (as review_r5c/cad/beak_path.py).
# Run: ../.venv/bin/python insertion_check.py   (writes insertion_check.json)
import os, sys, json, math, numpy as np
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, os.path.join(HERE, '..', 'options'))
from lib5 import to_mf, tf, W2S, box, T_FRONT
import trimesh
os.chdir(HERE)
X = json.load(open('xi.json'))
TP, ZP = X['pivot_S']; PHI0 = -X['start_tilt_deg']; S_CAM = X['cam_push_mm']
L = lambda n: tf(to_mf(trimesh.load(n + '.stl')), W2S)
wing = L('xi_wing'); kit = L('xi_kit')
plug = kit ^ box(-30, 40, T_FRONT - 12.0, 60, 150, 340)               # everything of the kit that can reach the wing
def pose(m, phi, sh):
    c, s = math.cos(math.radians(phi)), math.sin(math.radians(phi))
    R = np.array([[1, 0, 0, 0], [0, c, -s, TP - TP * c + ZP * s], [0, s, c, ZP - TP * s - ZP * c], [0, 0, 0, 1]], float)
    T = np.eye(4); T[1, 3] = -sh * c; T[2, 3] = -sh * s
    return tf(m, T @ R)
TOL = 0.5                                                              # mm3: mesh / boolean noise
out = {'phase_A': [], 'phase_B': []}
for sh in np.arange(70.0, S_CAM - 0.01, -2.0):
    v = (pose(plug, PHI0, sh) ^ wing).volume(); out['phase_A'].append(dict(sh=round(float(sh), 1), overlap_mm3=round(v, 2)))
for k in np.linspace(0, 1, 31):
    phi, sh = PHI0 * (1 - k), S_CAM * (1 - k)
    x = pose(plug, phi, sh) ^ wing; v = x.volume()
    out['phase_B'].append(dict(phi=round(phi, 2), sh=round(sh, 2), overlap_mm3=round(v, 2), box=[round(q, 1) for q in x.bounding_box()] if v > TOL else None))
seated = {}
for n in ('xi_kit', 'xi_catch', 'xi_rod', 'xi_button', 'xi_tell', 'xi_spring_c', 'xi_spring_r1', 'xi_spring_r2', 'xi_pin1', 'xi_pin2', 'xi_nut1', 'xi_nut2', 'xi_cap1', 'xi_cap2'):
    if os.path.exists(n + '.stl'): seated[n] = round((L(n) ^ wing).volume(), 2)
out['seated_overlap_with_wing_mm3'] = seated
worst = max([r['overlap_mm3'] for r in out['phase_A'] + out['phase_B']])
out['worst_path_overlap_mm3'] = worst; out['pass'] = bool(worst <= TOL and max(seated.values()) <= TOL)
json.dump(out, open('insertion_check.json', 'w'), indent=1)
print('worst overlap along the path', worst, 'mm3; seated', seated, '; PASS' if out['pass'] else '; FAIL')
