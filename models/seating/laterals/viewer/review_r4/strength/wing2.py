from sections import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
W = load('ps1_RiBH')
off = P0 + D_SHIFT*AXH + (ZREF-Z0)*SHEAR
def world(d,t): return off + d*AXH + t*TH
fig, axs = plt.subplots(1, 4, figsize=(28, 10))
cfgs = [((-14,58),(0.5,-0.866)), ((-10,52),(0.5,-0.866)), ((0,45),(1,0)), ((9.4,0),(0,1))]
for ax,(pt,dirv) in zip(axs,cfgs):
    a = dirv[0]*AXH + dirv[1]*TH; a = np.array([*a,0]); a/=np.linalg.norm(a)
    n = np.cross(a, [0,0,1.]); p0 = np.array([*world(*pt), 250.])
    S = section(W, p0, n, a, np.array([0,0,1.]))
    for s in S:
        P = np.vstack([s['P'], s['P'][:1]]); ax.plot(P[:,0], P[:,1]+250, 'k-' if s['solid'] else 'r-', lw=.7)
        if not s['solid'] and s['area']>3:
            # convert centre to d,t
            X = p0 + s['cen'][0]*a; r = X[:2]-off
            print(pt, 'hole centre (d,t)', round(r@AXH,1), round(r@TH,1), 'z', round(s['cen'][1]+250,1), 'size', np.ptp(s['P'][:,0]).round(1), 'x', np.ptp(s['P'][:,1]).round(1))
    ax.set_aspect('equal'); ax.grid(alpha=.3); ax.set_title(f'plane through {pt} along {dirv}')
plt.tight_layout(); plt.savefig('wing_face.png', dpi=55)
