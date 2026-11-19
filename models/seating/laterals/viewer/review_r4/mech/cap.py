from slice import *
B=tri('kit11_JzD'); C=tri('ps1_RUgD')
def frame(T):
    o=P0+2.0*AXH+SH
    q=np.zeros_like(T); r=T[:,:,:2]-o
    q[:,:,0]=r@AXH; q[:,:,1]=r@TH; q[:,:,2]=T[:,:,2]; return q
Bq=frame(B)
n=np.cross(Bq[:,1]-Bq[:,0],Bq[:,2]-Bq[:,0]); A=np.linalg.norm(n,axis=1)/2; nn=n/np.linalg.norm(n,axis=1)[:,None]
# button extents in tip frame
print('button d,t,z min',Bq.reshape(-1,3).min(0).round(1),'max',Bq.reshape(-1,3).max(0).round(1))
# outward-facing faces with -t component: cap
m=(nn[:,1]<-0.2)
cen=Bq.mean(1)
mm=m&(cen[:,1]<cen[:,1].min()+6)
print('cap area',A[mm].sum().round(1),'mean normal',(nn[mm]*A[mm,None]).sum(0)/A[mm].sum())
nm=(nn[mm]*A[mm,None]).sum(0); nm/=np.linalg.norm(nm); print('angle off -t deg',np.degrees(np.arccos(-nm[1])).round(1))
print('cap centroid', (cen[mm]*A[mm,None]).sum(0)/A[mm].sum())
Cq=frame(C).reshape(-1,3); print('carrier d,t,z min',Cq.min(0).round(1),'max',Cq.max(0).round(1))
