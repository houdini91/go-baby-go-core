import numpy as np
def slice_z(tris,z):
    v=tris[:,:,2]-z
    segs=[]
    m=(v.min(1)<0)&(v.max(1)>0)
    for T,s in zip(tris[m],v[m]):
        pts=[]
        for i in range(3):
            j=(i+1)%3
            if (s[i]<0)!=(s[j]<0):
                a=s[i]/(s[i]-s[j]); pts.append(T[i]+a*(T[j]-T[i]))
        if len(pts)==2: segs.append(pts)
    return np.array(segs)
