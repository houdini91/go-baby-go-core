import sys; sys.path.insert(0,'..')
from frame import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
m=load_wing_frame()
LD,LT=0.05,0.069
def shr(P,z): P=P.copy(); P[:,0]-=LD*(z-227); P[:,1]-=LT*(z-227); return P
fig,axs=plt.subplots(1,3,figsize=(30,14))
cols=plt.cm.viridis(np.linspace(0,1,8))
for ax,zs in zip(axs,[[200,205,207,209,211,213],[214,216,218,222,227,232],[238,240,245,247,250,255]]):
  for c,z in zip(cols,zs):
    s=m.section(plane_origin=[0,0,z],plane_normal=[0,0,1])
    for e in s.discrete: q=shr(e[:,:2],z); ax.plot(q[:,0],q[:,1],color=c,lw=1,label=str(z))
  dd=np.linspace(-6,20); ax.plot(dd,19.1+1.31*(dd+4),'r:'); ax.plot(dd,19.1+1.31*(dd+4)-9.9,'r--',lw=.6)
  ax.set_aspect('equal');ax.set_xlim(-10,30);ax.set_ylim(-18,40);ax.grid(alpha=.4);ax.set_xticks(range(-10,31,1));ax.set_yticks(range(-18,41,1));ax.tick_params(labelsize=5)
  h,l=ax.get_legend_handles_labels(); d=dict(zip(l,h)); ax.legend(d.values(),d.keys())
plt.tight_layout(); plt.savefig('probe4.png',dpi=50)
# front face t (at d=5) vs z, sheared
for z in range(185,350,10):
    s=m.section(plane_origin=[0,0,z],plane_normal=[0,0,1]); best=None
    for e in s.discrete:
        a,b=e[:-1,:2],e[1:,:2]
        for p,q in zip(a,b):
            if (p[0]-5)*(q[0]-5)<0:
                t=p[1]+(q[1]-p[1])*(5-p[0])/(q[0]-p[0]); best=t if best is None else min(best,t)
    print(z, 'front t at d5', round(best,2), 'sheared', round(best-LT*(z-227),2))
