from slice import *
from collections import defaultdict
W=tri('ps1_RiBH')
o=P0+2.0*AXH+SH
def loops(segs):
    # chain segments into loops by rounded endpoints
    key=lambda p: (round(p[0],2),round(p[1],2))
    adj=defaultdict(list)
    for i,(a,b) in enumerate(segs): adj[key(a)].append(i); adj[key(b)].append(i)
    used=set(); out=[]
    for i in range(len(segs)):
        if i in used: continue
        used.add(i); a,b=segs[i]; pts=[a,b]; cur=key(b)
        while True:
            nx=[j for j in adj[cur] if j not in used]
            if not nx: break
            j=nx[0]; used.add(j); a2,b2=segs[j]
            nxt=b2 if key(a2)==cur else a2
            pts.append(nxt); cur=key(nxt)
        out.append(np.array(pts))
    return out
for z in [205,215,227,240,250,260,275,292,305,318,330,345]:
    sg=slicez(W,z)
    L=loops([(s[0][:2],s[1][:2]) for s in sg])
    desc=[]
    for l in L:
        q=dt(l); 
        if len(q)<4: continue
        area=0.5*abs(np.sum(q[:-1,0]*q[1:,1]-q[1:,0]*q[:-1,1]))
        desc.append((round(area),q[:,0].min().round(1),q[:,0].max().round(1),q[:,1].min().round(1),q[:,1].max().round(1)))
    desc.sort(reverse=True)
    print(z,desc[:8])
