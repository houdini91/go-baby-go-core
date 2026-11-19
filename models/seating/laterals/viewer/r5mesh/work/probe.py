import sys; sys.path.insert(0,'..')
from frame import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
m=load_wing_frame()
print(m.bounds, len(m.faces), m.is_watertight, np.linalg.det(M_F2W[:3,:3]))
fig,axs=plt.subplots(2,6,figsize=(36,14))
for ax,d in zip(axs[0],[-6,-2,3,9,13,20]):
    s=m.section(plane_origin=[d,0,0],plane_normal=[1,0,0])
    if s is not None:
        for e in s.discrete: ax.plot(e[:,1],e[:,2],'k',lw=.7)
    ax.set_title(f'd={d} (x=t,y=z)');ax.set_aspect('equal');ax.set_xlim(-25,80);ax.set_ylim(170,360);ax.grid(alpha=.3)
for ax,t in zip(axs[1],[-12,-5,0,5,10,20]):
    s=m.section(plane_origin=[0,t,0],plane_normal=[0,1,0])
    if s is not None:
        for e in s.discrete: ax.plot(e[:,0],e[:,2],'k',lw=.7)
    ax.set_title(f't={t} (x=d,y=z)');ax.set_aspect('equal');ax.set_xlim(-30,35);ax.set_ylim(170,360);ax.grid(alpha=.3)
plt.savefig('probe1.png',dpi=55)
