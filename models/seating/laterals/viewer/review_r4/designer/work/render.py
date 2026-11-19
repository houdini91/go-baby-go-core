import numpy as np, sys
sys.path.insert(0, __import__('os').path.dirname(__file__))
from load import load
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt

P0=np.array([131.3,80.7]); AXH=np.array([0.79577626,0.60559074]); TH=np.array([-0.6083053,0.79370313])
SH=np.array([0.8/32.9,2.7/32.9])*(244.25-227.8)
def Q(d,t,z): dd=d+2.0; p=P0+dd*AXH+t*TH+SH; return np.array([p[0],p[1],z])

def cam(eye,target,up=(0,0,1)):
    eye=np.array(eye,float); target=np.array(target,float)
    f=target-eye; f/=np.linalg.norm(f); r=np.cross(f,up); r/=np.linalg.norm(r); u=np.cross(r,f)
    return eye,r,u,f

def render(parts, eye, target, scale, W=1100, H=850, fname=None, title=None, ortho=True, light=None, annotate=None, clipz=None):
    eye,r,u,f=cam(eye,target)
    img=np.ones((H,W,3))*np.array([0.97,0.97,0.975]); zb=np.full((H,W),np.inf)
    L=np.array(light if light is not None else (-0.3*r+0.6*u-0.75*f)); L/=np.linalg.norm(L)
    for tris,col in parts:
        tris=np.asarray(tris,float)
        if clipz is not None:
            keep=(tris[:,:,2].max(1)>=clipz[0])&(tris[:,:,2].min(1)<=clipz[1]); tris=tris[keep]
        rel=tris-eye
        X=rel@r; Y=rel@u; Z=rel@f
        px=W/2+X*scale; py=H/2-Y*scale
        n=np.cross(tris[:,1]-tris[:,0],tris[:,2]-tris[:,0]); nn=np.linalg.norm(n,axis=1); ok=nn>1e-9
        n[ok]/=nn[ok,None]
        # two-sided lighting
        facing=np.sign(-(n@f)); facing[facing==0]=1; n=n*facing[:,None]
        sh=0.35+0.55*np.clip(n@L,0,1)+0.12*np.clip(n@(-f),0,1)
        col=np.array(col)
        for i in np.nonzero(ok)[0]:
            x=px[i]; y=py[i]
            x0=max(int(np.floor(x.min())),0); x1=min(int(np.ceil(x.max())),W-1)
            y0=max(int(np.floor(y.min())),0); y1=min(int(np.ceil(y.max())),H-1)
            if x0>x1 or y0>y1: continue
            gx,gy=np.meshgrid(np.arange(x0,x1+1)+0.5,np.arange(y0,y1+1)+0.5)
            d=(y[1]-y[2])*(x[0]-x[2])+(x[2]-x[1])*(y[0]-y[2])
            if abs(d)<1e-12: continue
            a=((y[1]-y[2])*(gx-x[2])+(x[2]-x[1])*(gy-y[2]))/d
            b=((y[2]-y[0])*(gx-x[2])+(x[0]-x[2])*(gy-y[2]))/d
            c=1-a-b
            m=(a>=-1e-6)&(b>=-1e-6)&(c>=-1e-6)
            if not m.any(): continue
            z=a*Z[i,0]+b*Z[i,1]+c*Z[i,2]
            sub=zb[y0:y1+1,x0:x1+1]
            w=m&(z<sub)
            if not w.any(): continue
            sub[w]=z[w]
            img[y0:y1+1,x0:x1+1][w]=np.clip(col*sh[i],0,1)
    # edge outline from depth discontinuities
    zz=np.where(np.isinf(zb),1e6,zb)
    e=np.zeros_like(zz,bool)
    e[1:,:]|=np.abs(np.diff(zz,axis=0))>3; e[:,1:]|=np.abs(np.diff(zz,axis=1))>3
    img[e]=img[e]*0.35
    fig=plt.figure(figsize=(W/100,H/100),dpi=100); ax=fig.add_axes([0,0,1,1]); ax.imshow(img); ax.axis('off')
    if title: ax.text(12,26,title,fontsize=12,color='#222')
    if annotate:
        for p,txt in annotate:
            rel=np.array(p)-eye; x=W/2+(rel@r)*scale; y=H/2-(rel@u)*scale
            ax.plot([x],[y],'o',color='#d03',ms=4); ax.text(x+6,y-6,txt,fontsize=9,color='#d03')
    if fname: fig.savefig(fname); plt.close(fig)
    return img
