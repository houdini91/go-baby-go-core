from sections import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
W = load('ps1_RiBH')
off = P0 + D_SHIFT*AXH + (ZREF-Z0)*SHEAR
def to_dt(xy): r = xy - off; return np.stack([r@AXH, r@TH], -1)
zs = [200, 215, 235, 255, 275, 300, 320, 340]
fig, axs = plt.subplots(2, 4, figsize=(24, 12))
for ax, z in zip(axs.flat, zs):
    S = section(W, np.array([0,0,z+0.01]), np.array([0,0,1.]), np.array([1.,0,0]), np.array([0,1.,0]))
    for s in S:
        P = to_dt(s['P']); P = np.vstack([P, P[:1]])
        ax.plot(P[:,0], P[:,1], 'k-' if s['solid'] else 'r-', lw=.8)
        if not s['solid']:
            q = to_dt(s['P']); print(z, 'hole d', q[:,0].min().round(1), q[:,0].max().round(1), 't', q[:,1].min().round(1), q[:,1].max().round(1))
    for d0,d1,t0,t1,col in [(-7,25,-16,38,'b'),(1.2,17.6,-30,28,'g')]:
        ax.plot([d0,d1,d1,d0,d0],[t0,t0,t1,t1,t0],col+'--',lw=.6)
    ax.set_xlim(-60,60); ax.set_ylim(-40,80); ax.set_aspect('equal'); ax.set_title(f'z {z}'); ax.grid(alpha=.3)
plt.tight_layout(); plt.savefig('wing_z.png', dpi=60)
