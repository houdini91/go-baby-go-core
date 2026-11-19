# Slice the r5 wing mesh in the S frame and overlay the FS (gbg_sidepad_r5.fs) tongue / slot / groove / mouth chamfer.
import sys, numpy as np, trimesh, json
sys.path.insert(0, '../../../r5mesh')
from frame import M_W2F
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
LD, LT, ZC = 0.0565, 0.069, 227.0
F2S = np.array([[1,0,-LD,LD*ZC],[0,1,-LT,LT*ZC],[0,0,1,0],[0,0,0,1]], float)
W2S = F2S @ M_W2F
def loadS(f):
    m = trimesh.load('../../../r5mesh/' + f); m.apply_transform(W2S); return m
TONGUE = [[3.75,1.0],[17.45,1.0],[17.45,3.0],[17.285,9.3],[11.4,15.0],[9.6,18.8],[8.65,22.2],[7.9,23.4],[4.012,18.5],[3.75,8.5]]
SLOT = [[3.55,1.0],[17.65,1.0],[17.65,1.6],[17.5,3.0],[17.335,9.3],[11.565,15.118],[9.788,18.87],[8.835,22.282],[7.919,23.746],[3.962,18.5],[3.7,8.5],[3.55,7.7]]
GK = np.tan(np.radians(6)); GR_T=4.5; wallT = 17.5-(GR_T-3.0)*0.0262
GROOVE = [[16.85,3.0],[wallT,3.0],[wallT,4.0],[wallT+0.5,GR_T+(wallT+0.5-17.65)*GK],[22.2,GR_T+(22.2-17.65)*GK],[22.2,9.0],[16.85,9.0]]
if __name__ == '__main__':
    W = loadS('wing_r5.stl'); W0 = trimesh.load('../../../r5mesh/work/wing0_fixed.stl') if False else None
    out = {}
    fig, axs = plt.subplots(1, 4, figsize=(22, 7))
    for ax, z in zip(axs, (210.0, 218.0, 230.0, 244.0)):
        s = W.section(plane_origin=[0,0,z], plane_normal=[0,0,1])
        p2, _ = s.to_2D(to_2D=np.eye(4)) if False else (None, None)
        segs = []
        for ent in s.entities:
            pts = s.vertices[ent.points][:, :2]; ax.plot(pts[:,0], pts[:,1], 'k-', lw=0.8); segs.append(pts.tolist())
        out[z] = segs
        for P, c in ((TONGUE,'b'), (SLOT,'r'), (GROOVE,'g')):
            P = np.array(P + [P[0]]); ax.plot(P[:,0], P[:,1], c+'-', lw=0.8)
        ax.set_xlim(-15, 35); ax.set_ylim(-20, 40); ax.set_aspect('equal'); ax.grid(alpha=.3); ax.set_title(f'wing_r5.stl S-frame z {z} (d\' x, t\' y)')
    plt.savefig('wing_sections.png', dpi=90, bbox_inches='tight')
    json.dump({str(k): v for k, v in out.items()}, open('wing_sections.json', 'w'))
