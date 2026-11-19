import numpy as np, sys, matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
from load import load, load_orig; from render import render,P0,AXH,TH,SH
OUT='/home/mikey/go-baby-go-core/models/seating/laterals/viewer/review_r4/designer/'
def toF(tr):
    q=tr[...,:2]-P0-SH; return np.stack([q@AXH-2, q@TH, tr[...,2]],-1)
which=sys.argv[1]; side=sys.argv[2]
mesh = load_orig('left_wing') if which=='orig' else load('wing')
F=toF(mesh.astype(float))
W,H,s=1100,900,3.2; tc,zc=40,285
sg=1 if side=='out' else -1
eye=np.array([sg*400,tc,zc]); img=render([(F,(0.86,0.83,0.77))],eye,np.array([0,tc,zc]),s,W=W,H=H)
r = np.cross(np.array([-sg,0,0.]),[0,0,1]); 
ext_t=(tc-W/2/s, tc+W/2/s) if r[1]>0 else (tc+W/2/s, tc-W/2/s)
fig,ax=plt.subplots(figsize=(12,10)); ax.imshow(img,extent=[ext_t[0],ext_t[1],zc-H/2/s,zc+H/2/s])
ax.set_xticks(np.arange(-40,200,10)); ax.set_yticks(np.arange(150,430,10)); ax.grid(alpha=.35,color='b',lw=.5)
ax.set_xlabel('t (mm, + = rearward into wing; front edge at left)' ); ax.set_ylabel('z world (mm)')
ax.set_title(f'{which} wing, {side}board face, orthographic along d (tip frame)  -- red box = v1.1 TIP block t-16..38 z198..347')
ax.plot([-16,38,38,-16,-16],[198,198,347,347,198],'r--',lw=1)
fig.tight_layout(); fig.savefig(OUT+f'f_{which}_{side}.png',dpi=80)
