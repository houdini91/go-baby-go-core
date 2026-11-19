import numpy as np, trimesh, json
P0=np.array([131.3,80.7]);AXH=np.array([0.79577626,0.60559074]);TH=np.array([-0.6083053,0.79370313])
SH=np.array([(244.25-227.8)*0.8/32.9,(244.25-227.8)*2.7/32.9])
WING='/home/mikey/research/onshape/gobabygo/chair_mesh/chair_left_wing.stl'
def Q(d,t,z):
    d=np.asarray(d,float);t=np.asarray(t,float);z=np.asarray(z,float)
    x=P0[0]+(d+2)*AXH[0]+t*TH[0]+SH[0]; y=P0[1]+(d+2)*AXH[1]+t*TH[1]+SH[1]
    return np.stack(np.broadcast_arrays(x,y,z),-1)
def toDTZ(p):
    p=np.asarray(p,float); r=p[...,:2]-P0-SH
    return np.stack([r@AXH-2, r@TH, p[...,2]],-1)
# 4x4 matrix world <- frame(d,t,z)
M_F2W=np.eye(4); M_F2W[:3,0]=[AXH[0],AXH[1],0]; M_F2W[:3,1]=[TH[0],TH[1],0]; M_F2W[:3,2]=[0,0,1]
M_F2W[:3,3]=[P0[0]+2*AXH[0]+SH[0], P0[1]+2*AXH[1]+SH[1], 0]
M_W2F=np.linalg.inv(M_F2W)
def load_wing_frame():
    m=trimesh.load(WING); 
    if m.bounds.max()<5: m.apply_scale(1000)
    m.apply_transform(M_W2F); return m
