import sys,json; sys.path.insert(0,'..')
from frame import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
def rawmesh(name):
    B=json.load(open(f'../../raw/{name}.json')); out=[]
    for body in B:
        tris=[fc['vertices'] for face in body['faces'] for fc in face['facets']]
        a=np.array(tris,float).reshape(-1,3,3)*1000; out.append(a)
    return out
bodies=rawmesh('ps1_RUgD'); print(len(bodies),[len(b) for b in bodies])
A=np.concatenate(bodies)
m=trimesh.Trimesh(A.reshape(-1,3),np.arange(len(A)*3).reshape(-1,3)); m.merge_vertices(digits_vertex=4)
print('watertight',m.is_watertight,'bounds world',m.bounds)
m.apply_transform(M_W2F)
LD,LT=0.05,0.069
print('bounds frame',m.bounds)
fig,axs=plt.subplots(2,5,figsize=(30,12))
for ax,t in zip(axs[0],[10,0,-10,-14,-16]):
    s=m.section(plane_origin=[0,t,0],plane_normal=[0,1,0])
    if s is None: continue
    for e in s.discrete: ax.plot(e[:,0],e[:,2],'k')
    ax.set_title(f't={t}');ax.set_aspect('equal');ax.grid(alpha=.3)
for ax,t in zip(axs[1],[-18,-22,-26,-30,-40]):
    s=m.section(plane_origin=[0,t,0],plane_normal=[0,1,0])
    if s is None: continue
    for e in s.discrete: ax.plot(e[:,0],e[:,2],'k')
    ax.set_title(f't={t}');ax.set_aspect('equal');ax.grid(alpha=.3)
plt.savefig('probe5.png',dpi=45)
V=m.vertices; 
for t in [-12,-14,-16,-18,-20,-25,-30,-40,-50]:
    s=V[np.abs(V[:,1]-t)<0.6]
    if len(s): print(t, 'd',s[:,0].min().round(1),s[:,0].max().round(1),'z',s[:,2].min().round(1),s[:,2].max().round(1))
