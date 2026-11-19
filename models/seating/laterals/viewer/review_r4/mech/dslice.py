from slice import *
W=tri('ps1_RiBH');C=tri('ps1_RUgD');A=tri('ps1_J_D');B=tri('kit11_JzD')
fig,axs=plt.subplots(1,3,figsize=(18,8))
o=P0+2.0*AXH+SH
for ax,d in zip(axs,[0,9.4,21]):
    for T,c in ((W,'k'),(C,'b'),(A,'0.6'),(B,'r')):
        # signed distance to plane d
        s=(T[:,:,:2]-o)@AXH-d
        for t,ss in zip(T,s):
            pts=[]
            for i in range(3):
                j=(i+1)%3
                if (ss[i]>0)!=(ss[j]>0):
                    u=ss[i]/(ss[i]-ss[j]); pts.append(t[i]+(t[j]-t[i])*u)
            if len(pts)==2:
                p=np.array(pts); q=dt(p[:,:2]); ax.plot(q[:,1],p[:,2],c,lw=0.6)
    ax.set_aspect('equal');ax.set_title(f'd={d} (x=t into wing, y=z)');ax.grid(alpha=.3);ax.set_xlim(-70,120);ax.set_ylim(150,400)
plt.savefig('dslices.png',dpi=75)
