# usage: python3 hole_audit_render.py <out_dir> <chair_base.stl>   (needs numpy, trimesh, matplotlib)
# Hole audit 2026-11-17: before/after sections of the chair-side pommel mount holes, from the real chair mesh.
import trimesh, numpy as np, matplotlib, sys
matplotlib.use('Agg'); import matplotlib.pyplot as plt
from matplotlib.patches import Polygon, Rectangle
OUT=sys.argv[1]
c = trimesh.load(sys.argv[2])   # chair base STL in chair coordinates (Onshape tessellation of Part Studio 1 'base')
def sect(origin, normal, to2d):
    s = c.section(plane_origin=origin, plane_normal=normal)
    segs=[]
    for e in s.entities:
        p = s.vertices[e.points]; segs.append(np.array([to2d(q) for q in p]))
    return segs
def draw(ax, segs, **kw):
    for g in segs: ax.plot(g[:,0], g[:,1], **kw)
hexpts = lambda af, cx, cy: np.array([[cx+af/np.sqrt(3)*np.cos(np.radians(60*k)), cy+af/np.sqrt(3)*np.sin(np.radians(60*k))] for k in range(7)])

# ---------- 1. nut pocket ----------
fig, axs = plt.subplots(1, 3, figsize=(16, 5.6))
y0=-45
# section at y=-45 (x-z plane): before = real chair mesh
segs = sect([0,y0,0],[0,1,0], lambda q:(q[0],q[2]))
for ax, title, after in [(axs[0],'BEFORE (chair Main V16 mesh): pocket 5.5 AF x 2.4 = bare nut',False),(axs[1],'AFTER (mount re-cut): pocket 5.8 AF x 2.8',True)]:
    draw(ax, segs, color='0.3', lw=1)
    if after:
        R=5.8/np.sqrt(3)
        ax.add_patch(Polygon([[-R,-0.0],[-R,2.8],[-1.75,2.8],[-1.75,18.5],[1.75,18.5],[1.75,2.8],[R,2.8],[R,0]], closed=False, fill=False, ec='tab:green', lw=2, label='new pocket + Ø3.5 hole'))
        nz0=0.4
    else:
        nz0=0.0
    # nut (DIN 934 M3: corners 6.35 across, 2.4 thick) pulled up against the roof
    roof = 2.8 if after else 2.4
    ax.add_patch(Rectangle((-3.175, roof-2.4), 6.35, 2.4, fc='tab:orange', alpha=.5, label='M3 nut (5.5 AF, 2.4)'))
    # screw M3x25 tip at z 0
    ax.add_patch(Rectangle((-1.5, 0), 3.0, 18.5, fc='tab:blue', alpha=.25, label='M3x25 shank, tip z 0'))
    ax.axhline(0, color='k', lw=.6, ls=':')
    ax.set_xlim(-7,7); ax.set_ylim(-1.5,6); ax.set_aspect('equal'); ax.set_title(title, fontsize=9); ax.set_xlabel('x (mm), section y -45'); ax.set_ylabel('z (mm)')
    ax.legend(fontsize=7, loc='upper right')
    ax.text(-6.6,-1.2, 'nut fills pocket exactly: 0 clearance, prints 0.1-0.3 small' if not after else 'nut +0.3 AF, roof 0.4 above it: tip 0.4 past nut, 0 below pan', fontsize=7)
# plan at z 1.2
ax=axs[2]
s=c.section(plane_origin=[0,0,1.2], plane_normal=[0,0,1])
for e in s.entities:
    p=s.vertices[e.points]
    if np.all(np.abs(p[:,0])<8) and np.all(np.abs(p[:,1]-y0)<8): ax.plot(p[:,0],p[:,1],color='0.3',lw=1.5,label='before 5.5 AF (mesh)')
h=hexpts(5.8,0,y0); ax.plot(h[:,0],h[:,1],color='tab:green',lw=2,label='after 5.8 AF')
ax.set_aspect('equal'); ax.set_xlim(-5,5); ax.set_ylim(y0-4.5,y0+4.5); ax.set_title('Plan at z 1.2 (pan bottom up)', fontsize=9)
hd,lb=ax.get_legend_handles_labels(); d=dict(zip(lb,hd)); ax.legend(d.values(), d.keys(), fontsize=7)
fig.suptitle('Chair M3 nut pockets (x3, y -45/-60/-75) under the pommel screws', fontsize=11)
fig.tight_layout(); fig.savefig(OUT+'/nut_pocket_before_after.png', dpi=130)

# ---------- 2. rod hole mouth ----------
fig, axs = plt.subplots(1, 2, figsize=(13, 5))
segs = sect([12,0,0],[1,0,0], lambda q:(q[1],q[2]))
for ax, after in [(axs[0],False),(axs[1],True)]:
    draw(ax, segs, color='0.3', lw=1)
    if after:
        # lead-in cone: r 4.75 at y -90.5 -> 3.25 at y -89 (section through the axis)
        for sgn in [1,-1]:
            ax.plot([-90, -89],[7+sgn*4.25, 7+sgn*3.25], color='tab:green', lw=2.5)
        ax.set_title('AFTER: 1.5 x 45 deg lead-in at the real face y -90', fontsize=9)
    else:
        ax.add_patch(Rectangle((-93.5,2), 1.5, 10, fc='tab:red', alpha=.25))
        ax.text(-99.5, 13, 'old lead-in cone at y -93.5..-92:\nin air, 3 mm in front of the face', fontsize=7, color='tab:red')
        ax.set_title('BEFORE (chair Main V16 mesh): sharp mouth, lead-in missed the face', fontsize=9)
    ax.set_xlim(-100,-80); ax.set_ylim(-1,21); ax.set_aspect('equal'); ax.set_xlabel('y (mm), section x +12 (rod axis)'); ax.set_ylabel('z (mm)')
fig.suptitle('Chair carbon-rod hole mouth (2x, x +-12, z 7)', fontsize=11)
fig.tight_layout(); fig.savefig(OUT+'/rod_hole_mouth_before_after.png', dpi=130)

# ---------- 3. finding (not fixed): hook vs real front face ----------
fig, ax = plt.subplots(figsize=(9,6))
segs = sect([20,0,0],[1,0,0], lambda q:(q[1],q[2]))
draw(ax, segs, color='0.3', lw=1.2)
# pommel (recess version) profile near the hook, from the FS: bottom z0 to Y0 -93, inner face to z14, chamfer to (-88.5,18.5), floor
P=np.array([[-110,0],[-93,0],[-93,14],[-88.5,18.5],[-70,18.5],[-70,27],[-99,30],[-110,30]])
ax.add_patch(Polygon(P, closed=True, fc='tab:blue', alpha=.25, ec='tab:blue', label='pommel v0.7.3 (recess) hook, per FS'))
ax.axvline(-96, color='tab:red', ls='--', lw=.8); ax.text(-95.8, 1, 'PAN_FRONT -96 (assumed)', color='tab:red', fontsize=7, rotation=90)
ax.annotate('3 mm gap: hook never\ntouches the seat face', xy=(-91.5,6), xytext=(-108,10), fontsize=8, arrowprops=dict(arrowstyle='->'))
ax.annotate('chamfer overlaps the r4 lip\nby up to 0.58 mm', xy=(-89.2,18.0), xytext=(-86,23), fontsize=8, arrowprops=dict(arrowstyle='->'))
ax.set_xlim(-112,-74); ax.set_ylim(-1,32); ax.set_aspect('equal'); ax.legend(fontsize=8, loc='lower right')
ax.set_title('NOT FIXED (owner decision): hook vs the real seat front (section x 20, chair Main V16 mesh)', fontsize=9)
ax.set_xlabel('y (mm)'); ax.set_ylabel('z (mm)')
fig.tight_layout(); fig.savefig(OUT+'/finding_hook_vs_real_front_face.png', dpi=130)
print('ok')
