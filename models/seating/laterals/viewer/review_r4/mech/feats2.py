from slice import *
W=tri('ps1_RiBH')
o=P0+2.0*AXH+SH
fig,axs=plt.subplots(1,4,figsize=(26,8))
for ax,z in zip(axs,[205,240,275,305]):
    for p in slicez(W,z):
        q=dt(p[:,:2]); ax.plot(q[:,0],q[:,1],'k',lw=0.8)
    ax.set_aspect('equal');ax.grid(alpha=.3);ax.set_xlim(-45,35);ax.set_ylim(-25,95);ax.set_title(f'wing z{z} (x=d,y=t)')
    ax.set_xticks(range(-45,36,5)); ax.set_yticks(range(-25,96,5)); ax.tick_params(labelsize=6)
plt.savefig('wing_zoom.png',dpi=70)
# world-space view from outside: project onto (t,z) all triangles, colour by d
fig,ax=plt.subplots(1,2,figsize=(18,9))
Q=np.zeros_like(W); r=W[:,:,:2]-o; Q[:,:,0]=r@AXH; Q[:,:,1]=r@TH; Q[:,:,2]=W[:,:,2]
n=np.cross(Q[:,1]-Q[:,0],Q[:,2]-Q[:,0]); n/=np.linalg.norm(n,axis=1)[:,None]+1e-12
from matplotlib.collections import PolyCollection
for a,sel,tt in ((ax[0],n[:,0]>0.3,'outer side (+d) faces, t-z'),(ax[1],n[:,0]<-0.3,'inner side (-d) faces, t-z')):
    P=Q[sel][:,:,[1,2]]; c=Q[sel][:,:,0].mean(1)
    pc=PolyCollection(P,array=c,cmap='viridis',edgecolor='none'); a.add_collection(pc); plt.colorbar(pc,ax=a,label='d')
    a.set_xlim(-25,130);a.set_ylim(175,395);a.set_aspect('equal');a.set_title(tt);a.grid(alpha=.3)
plt.savefig('wing_sides.png',dpi=70)
