# usage: python3 hookfix_render.py <out.png> <chair_base.stl>   (needs numpy, trimesh, shapely, matplotlib)
import sys, numpy as np, trimesh, matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
from shapely.geometry import Polygon, LineString
from shapely.ops import unary_union
c = trimesh.load(sys.argv[2])
def chair_recess(x=20):
    s=c.section(plane_origin=[x,0,0],plane_normal=[1,0,0])
    p2,_=s.to_2D(to_2D=np.array([[0,1,0,0],[0,0,1,0],[1,0,0,-x],[0,0,0,1]],float))
    return unary_union([pg.buffer(0) for pg in p2.polygons_full]).intersection(Polygon([(-100,-1),(-60,-1),(-60,30),(-100,30)]))
def chair_plain():
    arc=[(-86-4*np.cos(t), 16+4*np.sin(t)) for t in np.linspace(0,np.pi/2,60)]
    return Polygon([(-90,0)]+arc+[(-60,20),(-60,0)])
def hook(recess,new):
    F = 18.5 if recess else 20
    if new: Y0=-90.2; hz=17.75; hy=Y0+(F-hz)
    else: Y0=-93 if recess else -96; hz=11+(Y0+96); hy=-96+(F-11)
    HF=Y0-6
    return Polygon([(HF-8,0),(Y0,0),(Y0,hz),(hy,F),(-60,F),(-60,F+8),(HF,30),(HF-8,30)]), Y0, F
fig,axs=plt.subplots(2,2,figsize=(14,10))
for col,new in enumerate([False,True]):
    for row,recess in enumerate([True,False]):
        ax=axs[row][col]; ch = chair_recess() if recess else chair_plain()
        p,Y0,F=hook(recess,new)
        for g in getattr(ch,'geoms',[ch]): ax.fill(*g.exterior.xy, color='0.75', ec='0.3', lw=1)
        ax.fill(*p.exterior.xy, color='tab:blue', alpha=.35, ec='tab:blue', lw=1.2)
        ov=p.intersection(ch)
        for g in getattr(ov,'geoms',[ov]):
            if not g.is_empty and g.area>1e-3: ax.fill(*g.exterior.xy, color='red', alpha=.9)
        hookline = LineString(list(p.exterior.coords)[1:4])
        d = hookline.distance(ch)
        ax.set_xlim(-108,-80); ax.set_ylim(-1,32); ax.set_aspect('equal')
        ax.set_title(f"{'AFTER' if new else 'BEFORE'} - {'recess chair (mount, section x 20, real mesh)' if recess else 'plain chair (no recess)'}", fontsize=9)
        ax.text(-107,26, f"hook face y {Y0:.1f}  gap to seat face {-90-Y0:.1f} mm\noverlap with seat {ov.area:.2f} mm² (section)\nmin clearance hook->seat {d:.2f} mm", fontsize=8, va='top')
        ax.axvline(-90,color='k',lw=.5,ls=':')
        ax.set_xlabel('y (mm)'); ax.set_ylabel('z (mm)')
fig.suptitle('Pommel hook vs the real seat front (y -90, r4 top edge). Blue = pommel (hook part, schematic top), grey = seat, red = overlap', fontsize=10)
fig.tight_layout(); fig.savefig(sys.argv[1], dpi=120); print('ok')
