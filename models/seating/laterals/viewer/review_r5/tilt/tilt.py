# Side-pad TILT study (review r5). Numbers from gbg_sidepad_v1.fs v1.1 constants; frame: LEFT side, x out (+x = toward wing),
# child toward -x, y back (+y = chair back), z up. Tilt theta > 0 = TOP-IN (pad top toward the child), about the y axis.
# Writes tilt_out.txt, tilt.json, tilt_section.svg next to this file. No repo files are touched.
import json, math, os
H = os.path.dirname(os.path.abspath(__file__))
XE = 128.1                 # arm end / sleeve face (FS: armPath end; viewer mesh depth max x 127.8 = XE - JGAP)
TEN_L, TEN_W, TEN_H, CL = 13.0, 13.0, 20.0, 0.2
DEPTHS = {'D0': 104, 'D1': 92, 'D2': 80, 'D3': 68}
FLOOR_T, FOOT = 6.0, (19.0, 46.0, 20.0)          # foot y0, y1, half-height
PAD = (-27.0, 55.0, 40.0)                         # pad y0, y1, half-height
PSY = 36.0; FOAM = 30.0; SHELL = 4.5
ANG = [0, 8, -8, 15, -15]
out = []; J = {}
def p(*a): out.append(' '.join(str(x) for x in a))
s = lambda d: math.sin(math.radians(d)); c = lambda d: math.cos(math.radians(d)); t = lambda d: math.tan(math.radians(d))

p('== 1. Where the angle lives: pad-centre shift (mm) vs pivot ==')
p('A = bend at the sleeve face (x', XE, '); lever = sleeve face -> pad back')
J['A_shift'] = {}
for d, xs in DEPTHS.items():
    L = XE - xs
    J['A_shift'][d] = {a: [round(L * s(a), 1), round(L * (1 - c(a)), 1)] for a in (8, 15)}
    p(f'  {d}: lever {L:.1f}  8deg dz {L*s(8):.1f}  15deg dz {L*s(15):.1f} (dx {L*(1-c(15)):.1f})')
p('B = angled foot, pivot on the pad back at the foot centre (x = xs, z = zp): pad centre does not move;')
p('    foam surface point at zp moves dz = -(FOAM+SHELL)*sin(theta); foam TOP corner moves:')
J['B_top'] = {}
for a in ANG:
    r = FOAM + SHELL
    x = -r * c(a) - PAD[2] * s(a); z = -r * s(a) + PAD[2] * c(a)
    J['B_top'][a] = [round(x + r, 1), round(z - PAD[2], 1)]
    p(f'  {a:+3d}deg: contact centre dz {-r*s(a):+.1f}; foam top corner dx {x + r:+.1f} dz {z - PAD[2]:+.1f}; pad bottom edge dx {PAD[2]*s(a):+.1f}')

p('\n== 2. Angled foot fits even at D0 (shortest)? tenon tip vs tilted hollow floor ==')
J['fit'] = {}
for d, xs in DEPTHS.items():
    tip = XE - TEN_L
    for a in (8, 15):
        hz = TEN_H / 2 + CL
        floor_top = xs + FLOOR_T + hz * t(a)        # highest x of the tilted hollow end, at the hollow's top/bottom corner
        rim = FOOT[2] * t(a)
        J['fit'].setdefault(d, {})[a] = round(tip - floor_top, 1)
        p(f'  {d} {a:2d}deg: tenon tip x {tip:.1f}, tilted floor reaches x {floor_top:.1f}  -> margin {tip - floor_top:.1f}; foot rim moves +-{rim:.1f}')
p('  armrest inner face (viewer mesh, y -9..62, z 120-160): x ~134; pad bottom at 15deg top-in D0: x', round(104 + SHELL + PAD[2]*s(15), 1))

p('\n== 3. Separate wedge (option B-wedge) thickness under the 40-tall foot (min 2.5) ==')
for a in (8, 15):
    p(f'  {a}deg: 2.5 .. {2.5 + 2*FOOT[2]*t(a):.1f}, at the screw {2.5 + FOOT[2]*t(a):.1f}; screw-to-pad-normal misalignment {a}deg')

p('\n== 4. Screw audit: pad joint (foot 27 x 40 on the pad back, one M5 at y 36, rib has no undercut) ==')
p('Rigid-pad statics: the contact resultant must stay inside the foot rectangle (y 19..46, z -20..20); S = screw tension.')
def S_out(F, y, z):   # outward push F at (y,z): contact C = F + S
    best = 0
    # y limits
    for ylim in (FOOT[0], FOOT[1]):
        # (F*y + S*PSY)/(F+S) within [19,46]
        pass
    need = 0.0
    if y < FOOT[0]: need = max(need, F * (FOOT[0] - y) / (PSY - FOOT[0]))
    if y > FOOT[1]: need = max(need, F * (y - FOOT[1]) / (FOOT[1] - PSY) if FOOT[1] > PSY else 1e9)
    if abs(z) > FOOT[2]: need = max(need, F * (abs(z) - FOOT[2]) / FOOT[2])
    return need
def S_in(F, y, z):    # inward pull F at (y,z): contact C = S - F
    need = F
    # y: (S*PSY - F*y)/(S-F) <= 46 and >= 19
    need = max(need, F * (FOOT[1] - y) / (FOOT[1] - PSY), F * (y - FOOT[0]) / (PSY - FOOT[0]))
    need = max(need, F * (FOOT[2] + abs(z)) / FOOT[2])
    return need
J['pad_screw'] = {}
for nm, y, z in [('pad centre', 14, 0), ('front edge', -27, 0), ('top edge', 14, 40), ('front-top corner', -27, 40), ('over the foot', 32.5, 0)]:
    so, si = S_out(300, y, z), S_in(100, y, z)
    J['pad_screw'][nm] = [round(so), round(si)]
    p(f'  {nm:17s} (y {y}, z {z:+d}): 300 N outward -> screw {so:4.0f} N tension;  100 N inward -> screw {si:4.0f} N tension')

p('\n== 5. Tenon joint axial ==')
p('  hole play (5.5 - 5.0)/2 = 0.25 < hairline JGAP 0.3: an outward 300 N reaches the M5x12 shank (shear) before the faces touch;')
p('  a 100 N inward pull is held ONLY by the M5x12 (shear, cross-pin). Moments -> tenon in sleeve (plastic).')
p('  Proposed ledge: hollow steps 13.4x20.4 -> 11x18 at the tenon tip; bearing area', round((13.4*20.4 - 11*18)), 'mm2 ->', round(300/((13.4*20.4-11*18)), 1), 'MPa at 300 N')

p('\n== 6. Wing joint (no screws) ==')
TH = (-0.6083053, 0.79370313); AX = (0.79577626, 0.60559074)
for nm, F in [('300 N outward (+x)', 300), ('100 N inward (-x)', -100)]:
    ft = F * TH[0]; fd = F * AX[0]
    p(f'  {nm}: along t {ft:+.0f} N ({"pull-out -> printed shuttle catch" if ft < 0 else "push-in -> tongue rear face on slot end (0.5 gap)"}), across d {fd:+.0f} N -> slot walls; moments -> rails + walls')

p('\n== 7. Print counts for 0 / +-8 / +-15, both sides ==')
p('  angled depth piece: 0 universal; each tilted print is handed: A_theta = L top-in = R top-out, B_theta = its mirror.')
p('  per depth: 2x P0 + A8 + B8 + A15 + B15 = 6 prints covers both-in and both-out at each angle (4 new STL per depth, 16 total)')
p('  wedge: 2x W8 + 2x W15 = 4 prints for every depth and every sign (flip about the screw axis), + per-angle screw length')
open(os.path.join(H, 'tilt_out.txt'), 'w').write('\n'.join(out) + '\n')
json.dump(J, open(os.path.join(H, 'tilt.json'), 'w'), indent=1)

# ---- x-z section sketch (LEFT side, looking from the front): D0 and D3, theta 0 / +15 / -15 ----
def poly(pts, **k):
    a = ' '.join(f'{x:.1f},{y:.1f}' for x, y in pts)
    return f'<polygon points="{a}" ' + ' '.join(f'{kk.replace("_","-")}="{v}"' for kk, v in k.items()) + '/>'
svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="1000" height="560" font-family="sans-serif" font-size="11">']
for row, d in enumerate(('D0', 'D3')):
    xs = DEPTHS[d]
    for col, a in enumerate((15, 0, -15)):
        ox, oy = 30 + col * 325, 170 + row * 240
        S = 1.75
        def T(x, z): return (ox + (x - 25) * S, oy - z * S)
        def R(x, z):     # rotate about the pivot (xs, 0): top-in => top goes -x
            dx, dz = x - xs, z
            return (xs + dx * c(a) - dz * s(a), dx * s(a) + dz * c(a))
        # arm end + tenon (fixed)
        svg.append(poly([T(XE, -14), T(XE + 25, -14), T(XE + 25, 14), T(XE, 14)], fill='#9bb', stroke='#455'))
        svg.append(poly([T(XE - TEN_L, -10), T(XE, -10), T(XE, 10), T(XE - TEN_L, 10)], fill='#7aa', stroke='#344'))
        # depth piece outline: pill at sleeve face, then to the tilted foot
        fr = [R(xs, -FOOT[2]), R(xs, FOOT[2])]
        dp = [T(XE - 0.3, 14), T(xs + 18 if XE - xs > 20 else (XE + xs) / 2, 14), T(*fr[1]), T(*fr[0]), T(xs + 18 if XE - xs > 20 else (XE + xs) / 2, -14), T(XE - 0.3, -14)]
        svg.append(poly(dp, fill='#e6c27a', stroke='#7a5', fill_opacity='0.85'))
        # hollow + ledge + tilted floor
        hf = [R(xs + FLOOR_T, -10.2), R(xs + FLOOR_T, 10.2)]
        svg.append(poly([T(XE - TEN_L - 0.2, 9), T(*hf[1]), T(*hf[0]), T(XE - TEN_L - 0.2, -9)], fill='#fff', stroke='#a85', stroke_dasharray='2,2'))
        # pad shell + foam
        sh = [R(xs, -PAD[2]), R(xs, PAD[2]), R(xs - SHELL, PAD[2]), R(xs - SHELL, -PAD[2])]
        svg.append(poly([T(*q) for q in sh], fill='#555'))
        fo = [R(xs - SHELL, -PAD[2]), R(xs - SHELL, PAD[2]), R(xs - SHELL - FOAM, PAD[2] - 6), R(xs - SHELL - FOAM, -PAD[2] + 6)]
        svg.append(poly([T(*q) for q in fo], fill='#223', fill_opacity='0.55'))
        # pad screw along the pad normal
        s0, s1 = R(xs + FLOOR_T + 2.7, 0), R(xs - SHELL, 0)
        svg.append(f'<line x1="{T(*s0)[0]:.1f}" y1="{T(*s0)[1]:.1f}" x2="{T(*s1)[0]:.1f}" y2="{T(*s1)[1]:.1f}" stroke="#c22" stroke-width="2.5"/>')
        lab = f'{d}  {"+" if a > 0 else ""}{a}° ' + ('top-in' if a > 0 else 'top-out' if a < 0 else '')
        svg.append(f'<text x="{ox + 20}" y="{oy + 95}">{lab}</text>')
svg.append('<text x="40" y="30" font-size="12">LEFT side, front view (x-z section at the joint axis). Angle in the depth-piece foot; pivot = pad back at the foot centre; red = pad M5 along the pad normal.</text>')
svg.append('<text x="40" y="48">grey-blue = arm end + tenon (unchanged); tan = depth piece; dashed = hollow with tenon-tip ledge and tilted floor; dark = shell + foam.</text>')
svg.append('</svg>')
open(os.path.join(H, 'tilt_section.svg'), 'w').write('\n'.join(svg))
print('\n'.join(out))
