# Slice the ORIGINAL left wing at several z, express in the wing-tip frame (d, t), keep the front (t <= T_CUT) part as the nose profile.
import numpy as np, struct, json, sys
P0=np.array([131.3,80.7]);AXH=np.array([0.79577626,0.60559074]);TH=np.array([-0.6083053,0.79370313]);SH=np.array([(244.25-227.8)*0.8/32.9,(244.25-227.8)*2.7/32.9])
def to_dt(xy):  # inverse of Q: xy = P0 + (d+2)*AXH + t*TH + SH  (AXH, TH orthonormal)
    r=xy-P0-SH; return np.stack([r@AXH-2, r@TH],-1)
def stl(f):
    d=open(f,'rb').read(); n=struct.unpack('<I',d[80:84])[0]
    a=np.frombuffer(d[84:84+n*50],dtype=np.dtype([('n','<3f4'),('v','<9f4'),('a','<u2')]))
    v=a['v'].reshape(-1,3,3).astype(float); return v*1000 if v.max()<5 else v
W=stl('/home/mikey/research/onshape/gobabygo/chair_mesh/chair_left_wing.stl')
def slice_z(z):
    segs=[]
    for tri in W:
        s=tri[:,2]-z; pts=[]
        for i in range(3):
            a,b=tri[i],tri[(i+1)%3]; sa,sb=s[i],s[(i+1)%3]
            if (sa<0)!=(sb<0): pts.append(a+(b-a)*(sa/(sa-sb)))
        if len(pts)==2: segs.append(pts)
    return np.array(segs)[:,:,:2]
out={}
for z in [190,200,215,240,260,300,320,330]:
    s=slice_z(z); dt=to_dt(s.reshape(-1,2)).reshape(-1,2,2)
    front=dt[(dt[:,:,1].max(1)<10)]
    out[z]=dt.tolist()
    print(z,len(s),'front segs',len(front),'d range',front[:,:,0].min().round(1) if len(front) else '-',front[:,:,0].max().round(1) if len(front) else '-','t min',front[:,:,1].min().round(1) if len(front) else '-')
json.dump(out,open('slices.json','w'))
