import json,numpy as np,sys
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
R='/home/mikey/go-baby-go-core/models/seating/laterals/viewer/raw/'
P0=np.array([131.3,80.7]);AXH=np.array([0.79577626,0.60559074]);TH=np.array([-0.6083053,0.79370313])
SH=(244.25-227.8)*np.array([0.8/32.9,2.7/32.9])
def tri(f):
    d=json.load(open(R+f+'.json'))
    T=[]
    for b in d:
        for fa in b['faces']:
            for fc in fa['facets']: T.append(fc['vertices'])
    return np.array(T)*1000
def dt(xy):
    o=P0+2.0*AXH+SH  # Q(0,0)
    r=xy-o
    return np.stack([r@AXH, r@TH],-1)  # AXH,TH nearly orthonormal
def slicez(T,z):
    segs=[]
    for t in T:
        s=t[:,2]-z; pts=[]
        for i in range(3):
            a,b=t[i],t[(i+1)%3]
            if (s[i]>0)!=(s[(i+1)%3]>0):
                u=s[i]/(s[i]-s[(i+1)%3]); pts.append(a+(b-a)*u)
        if len(pts)==2: segs.append(pts)
    return np.array(segs)
if __name__=='__main__':
    zs=[float(a) for a in sys.argv[1:]] or [227,260,330,360,380]
    W=tri('ps1_RiBH');C=tri('ps1_RUgD');B=tri('kit11_JzD');S=tri('kit11_RhBD')
    fig,axs=plt.subplots(1,len(zs),figsize=(6*len(zs),6))
    for ax,z in zip(np.atleast_1d(axs),zs):
        for T,c in ((W,'k'),(C,'b'),(B,'r'),(S,'g')):
            sg=slicez(T,z)
            for p in sg:
                q=dt(p[:,:2]); ax.plot(q[:,0],q[:,1],c,lw=0.7)
        ax.set_aspect('equal');ax.set_title(f'z {z}  (x=d, y=t)');ax.grid(alpha=.3)
        ax.set_xlim(-60,60);ax.set_ylim(-60,80)
    plt.savefig(sys.argv[0].replace('slice.py','slices.png'),dpi=80)
