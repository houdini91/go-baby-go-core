from slice import *
W=tri('ps1_RiBH'); o=P0+2.0*AXH+SH
fig,axs=plt.subplots(1,5,figsize=(26,8))
for ax,d in zip(axs,[-5,-1,19,22,24.5]):
    s=(W[:,:,:2]-o)@AXH-d
    for t,ss in zip(W,s):
        pts=[]
        for i in range(3):
            j=(i+1)%3
            if (ss[i]>0)!=(ss[j]>0):
                u=ss[i]/(ss[i]-ss[j]); pts.append(t[i]+(t[j]-t[i])*u)
        if len(pts)==2:
            p=np.array(pts); q=dt(p[:,:2]); ax.plot(q[:,1],p[:,2],'k',lw=0.6)
    ax.set_aspect('equal');ax.set_title(f'd={d}');ax.grid(alpha=.3);ax.set_xlim(-25,95);ax.set_ylim(175,395)
    ax.set_xticks(range(-25,96,10));ax.set_yticks(range(175,396,10));ax.tick_params(labelsize=6)
plt.savefig('wing_dsl.png',dpi=70)
