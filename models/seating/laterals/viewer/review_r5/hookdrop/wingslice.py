# Slice the ORIGINAL left wing in the tip frame (d,t) at many z; save plots + a json of segments (front zone only)
import numpy as np, struct, json, matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
P0=np.array([131.3,80.7]);AXH=np.array([0.79577626,0.60559074]);TH=np.array([-0.6083053,0.79370313]);SH=np.array([(244.25-227.8)*0.8/32.9,(244.25-227.8)*2.7/32.9])
def to_dt(xy): r=xy-P0-SH; return np.stack([r@AXH-2, r@TH],-1)
def stl(f):
    d=open(f,'rb').read(); n=struct.unpack('<I',d[80:84])[0]
    a=np.frombuffer(d[84:84+n*50],dtype=np.dtype([('n','<3f4'),('v','<9f4'),('a','<u2')]))
    v=a['v'].reshape(-1,3,3).astype(float); return v*1000 if v.max()<5 else v
W=stl('/home/mikey/research/onshape/gobabygo/chair_mesh/chair_left_wing.stl')
print('wing bbox',W.reshape(-1,3).min(0),W.reshape(-1,3).max(0))
def slice_z(z):
    s=W[:,:,2]-z; out=[]
    for i in range(3):
        pass
    segs=[]
    for tri,ss in zip(W,s):
        pts=[]
        for i in range(3):
            a,b=tri[i],tri[(i+1)%3]; sa,sb=ss[i],ss[(i+1)%3]
            if (sa<0)!=(sb<0): pts.append(a+(b-a)*(sa/(sa-sb)))
        if len(pts)==2: segs.append(pts)
    S=np.array(segs)[:,:,:2]; return to_dt(S.reshape(-1,2)).reshape(-1,2,2)
ZS=[186,192,198,201,205,210,220,230,240,250,260,270,275,280,290,300,310,318,325,335]
res={}
fig,axs=plt.subplots(4,5,figsize=(25,26))
for ax,z in zip(axs.flat,ZS):
    S=slice_z(z); m=(S[:,:,1].min(1)<60)&(S[:,:,0].min(1)>-30)&(S[:,:,0].max(1)<40); S=S[m]; res[z]=S.tolist()
    for (a,b) in S: ax.plot([a[0],b[0]],[a[1],b[1]],'k',lw=.8)
    dd=np.linspace(-10,25,10); ax.plot(dd,19.1+1.31*(dd+4),'r:',lw=.8)
    ax.set_title(f'z {z}'); ax.set_aspect('equal'); ax.set_xlim(-25,35); ax.set_ylim(-20,60); ax.grid(alpha=.3)
plt.savefig('work_wing_slices.png',dpi=60)
json.dump(res,open('work_wing_slices.json','w'))
