import numpy as np, trimesh, sys
from sec_wing import loadS
W = loadS('wing_r5.stl')
for z in (205, 208, 211, 213, 214.5, 215.5, 217, 220, 224, 240, 246.5, 248, 252):
    s = W.section(plane_origin=[0, 0, z], plane_normal=[0, 0, 1])
    if s is None: print(z, 'none'); continue
    V = s.vertices[:, :2]
    # outer face d' at t' 2..12 band, inner face, front face
    band = V[(V[:, 1] > 2.2) & (V[:, 1] < 12)]
    print(f'z {z:6.1f}: d range at t 2.2-12: {band[:,0].min():6.2f} .. {band[:,0].max():6.2f};  t range at d -3..0: {V[(V[:,0]>-3)&(V[:,0]<0),1].min():6.2f}..{V[(V[:,0]>-3)&(V[:,0]<0),1].max():6.2f}')
