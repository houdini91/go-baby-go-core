# Repair the original wing STL: a ~1 mm fan of degenerate tris at world (148.85, 73.57, 182.85) (frame d 6.5, t -17.1, z 182.8,
# the underside of the front face, far from the nose) makes 36 non-manifold edges. Drop it and close the hole with a fan.
import numpy as np, trimesh, manifold3d as mf
from frame import WING
def tomf(m): return mf.Manifold(mf.Mesh(vert_properties=np.asarray(m.vertices,np.float32), tri_verts=np.asarray(m.faces,np.uint32)))
def repaired_wing():
    m=trimesh.load(WING); m.merge_vertices(digits_vertex=3); m.update_faces(m.nondegenerate_faces()); m.update_faces(m.unique_faces())
    c=np.array([148.85,73.57,182.85]); bad=np.any(np.linalg.norm(m.vertices[m.faces]-c,axis=2)<0.6,axis=1)
    m.update_faces(~bad); m.remove_unreferenced_vertices()
    E=m.edges_sorted; u,cc=np.unique(E,axis=0,return_counts=True); bnd=set(map(tuple,u[cc==1]))
    # directed boundary half-edges (edges whose reverse is missing)
    F=m.faces; he={}
    for f in F:
        for a,b in ((f[0],f[1]),(f[1],f[2]),(f[2],f[0])):
            if (min(a,b),max(a,b)) in bnd: he[b]=a          # hole loop runs opposite to the face edge
    V=list(m.vertices); newF=list(F); seen=set()
    for s in list(he):
        if s in seen: continue
        loop=[s]; seen.add(s); x=he[s]
        while x!=s: loop.append(x); seen.add(x); x=he[x]
        ci=len(V); V.append(np.mean(m.vertices[loop],0))
        for i in range(len(loop)): newF.append([loop[i],loop[(i+1)%len(loop)],ci])
    w=trimesh.Trimesh(np.array(V),np.array(newF),process=False)
    M=tomf(w); assert M.status().name=='NoError', M.status()
    return M, int(bad.sum())
if __name__=='__main__':
    M,n=repaired_wing(); print('ok', n, M.volume(), M.num_tri())
