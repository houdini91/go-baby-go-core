# Same arm check with the carrier printed standing (as fitted, z up): layer normal = z.
import numpy as np, trimesh, arm
from geom import *
arm.N_LAYER = np.array([0, 0, 1.0])
for a in ('S', 'L'):
    mesh = trimesh.load(f'../../../r5mesh/nose_arm_r5_{a}.stl'); p = armPath(C, zp_of(a, 'LOW'))
    worst = {}
    for u in [4, 8, 12, 16, 20, 25, 30, 35, 40, 45, 50, 55]:
        r = arm.check_section(mesh, a, u, max(7.2 - u, 0))
        if r is None: continue
        res, G, s = r
        for k, v in res.items():
            if k not in worst or v['SF'] < worst[k][0]: worst[k] = (v['SF'], u, round(s, 2))
    print(a, {k: (round(v[0], 2), v[1], v[2]) for k, v in worst.items() if k in ('lean 300', 'proof 600', 'corner 900', 'pull fwd 300')})
from arm import tsai_hill_X
print('tenon allowable z-up:', tsai_hill_X(0.0), ' vs on the nose face:', tsai_hill_X(0.79))
