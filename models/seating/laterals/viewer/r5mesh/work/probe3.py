import sys; sys.path.insert(0,'..')
from frame import *
m=load_wing_frame()
import shapely.geometry as sg
def sec(z):
    s=m.section(plane_origin=[0,0,z],plane_normal=[0,0,1]); p2,T=s.to_2D(to_2D=np.eye(4)) if False else (None,None)
    return [e[:,:2] for e in s.discrete]
def intervals(z,t):
    loops=sec(z); xs=[]
    for L in loops:
        a=L[:-1];b=L[1:]
        for p,q in zip(a,b):
            if (p[1]-t)*(q[1]-t)<0: xs.append(p[0]+(q[0]-p[0])*(t-p[1])/(q[1]-p[1]))
    xs=sorted(xs); return [(round(xs[i],1),round(xs[i+1],1)) for i in range(0,len(xs)-1,2) if xs[i]>-20 and xs[i]<40]
for z in [203,205,207,209,211,213,214,215,216,218,222,227,232,238,240,243,245,247,249,252]:
    print(z, {t:intervals(z,t) for t in [-14,-10,0,6,8,10,13,16,20,24,28]})
