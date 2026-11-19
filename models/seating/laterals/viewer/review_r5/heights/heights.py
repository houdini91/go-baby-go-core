# Side-pad height plan r5 (scratch). Chair world mm, z up. Seat surface z 20 (chair_base.stl top, 9620 mm2 flat at z 20.0).
# Axilla above seat (sitting), Snyder 1977/1975 via NIST AnthroKids, derived in research/.../laterals/chair_interfaces.md:
#   sitting axilla = sitting height - (stature - standing axilla height) ; cross-check 1975 sitting shoulder - 61..64.
import numpy as np, json, sys
SEAT = 20.0; PAD_H = 80.0
AX = {'2y P5': 220, '2y P50': 248, '5y P50': 310, '5y P95': 350}       # above seat
LO, HI = 25.0, 50.0                                                     # pad top this far below the axilla (ACI 25, Physiopedia 30; 50 = brief)
A = np.arange(SEAT + 220, SEAT + 350 + 0.5, 0.5)                        # axilla z to serve
def serves(c): return (c + PAD_H / 2 + LO, c + PAD_H / 2 + HI)          # axilla z interval a pad centre c serves
def report(name, cs):
    cs = sorted(set(round(c, 2) for c in cs))
    ok = np.zeros_like(A, bool); used = []
    for c in cs:
        lo, hi = serves(c); m = (A >= lo) & (A <= hi); ok |= m
        if m.any(): used.append(c)
    gaps, run = [], None
    for a, g in zip(A, ok):
        if not g and run is None: run = [a, a]
        elif not g: run[1] = a
        elif run is not None: gaps.append(tuple(run)); run = None
    if run: gaps.append(tuple(run))
    excess = [c for c in cs if c not in used]
    worst = max(min([a - (c + PAD_H / 2) for c in cs if a - (c + PAD_H / 2) >= LO] or [999]) for a in A)
    print(f"\n{name}: {len(cs)} centres {cs}")
    print(f"  useful {len(used)}, excess {excess}; uncovered axilla z {[(round(g[0],1), round(g[1],1)) for g in gaps]}; worst pad-top-below-axilla {worst:.1f}")
    return dict(centres=cs, useful=used, excess=excess, gaps=gaps, worst=worst)
for k, v in AX.items():
    z = SEAT + v; print(f"{k}: axilla z {z}, pad centre window z {z - 40 - HI:.0f}..{z - 40 - LO:.0f}")
out = {}
# today (v1.1): 3 tongue positions x LOW/HIGH (+-47.5)
out['today'] = report('today v1.1', [t + s * 47.5 for t in (227, 259.5, 292) for s in (-1, 1)])
# R: round-4 nose 40 tall + 4 identical fillers 19.15 in the 201.3..317.9 band, same flip D 47.5
BAND = (201.3, 317.9); H = 40.0; NF = 4; F = (BAND[1] - BAND[0] - H) / NF
T = [BAND[0] + H / 2 + k * F for k in range(NF + 1)]
print('\nR tongue centres', [round(t, 2) for t in T], 'filler', round(F, 2))
out['R_nose40_4fill'] = report('R nose 40 + 4 fillers, flip 47.5', [t + s * 47.5 for t in T for s in (-1, 1)])
# same nose 40 but only 3 fillers of 25.5 (4 slots)
F3 = (BAND[1] - BAND[0] - H) / 3; T3 = [BAND[0] + H / 2 + k * F3 for k in range(4)]
out['R_nose40_3fill'] = report(f'nose 40 + 3 fillers {F3:.2f}, flip 47.5', [t + s * 47.5 for t in T3 for s in (-1, 1)])
# C: keep r4 51 nose (fillers 32.6) / today's tongue z, add a pad key offset -16.25 on the pad back
out['C_padkey'] = report('today tongues x flip x pad key {0,-16.25}', [t + s * 47.5 + o for t in (227, 259.5, 292) for s in (-1, 1) for o in (0, -16.25)])
# D: two arm drops 47.5 / 31.25
out['D_twodrops'] = report('today tongues x flip x drop {47.5, 31.25}', [t + s * d for t in (227, 259.5, 292) for s in (-1, 1) for d in (47.5, 31.25)])
# nose-height sweep: slots that fit (pitch 20) and coverage with flip 47.5
print('\nnose sweep (fillers sized to fill the band, flip 47.5):')
for h in (30, 36, 40, 45, 51):
    for nf in (3, 4, 5):
        f = (BAND[1] - BAND[0] - h) / nf; tt = [BAND[0] + h / 2 + k * f for k in range(nf + 1)]
        cs = [t + s * 47.5 for t in tt for s in (-1, 1)]
        r = report(f'  h {h} nf {nf} f {f:.1f}', cs)
# roll check: 180 N outward at pad centre, lever 47.5 in z, reacted by the tongue's corners on the 6 mm U-walls
print('\nroll (180 N x 47.5) -> corner force and 6 mm wall stress (wall cantilever 30, 45 deg spread b = h + 60):')
for h in (30, 36, 40, 45, 50):
    M = 180 * 47.5; Fc = M / (h - 5); b = h + 60; s = Fc * 30 / (b * 6 ** 2 / 6)
    print(f'  h {h}: corner force {Fc:.0f} N, wall bending {s:.1f} MPa')
json.dump(out, open(sys.argv[1] if len(sys.argv) > 1 else 'heights.json', 'w'), indent=1, default=float)
