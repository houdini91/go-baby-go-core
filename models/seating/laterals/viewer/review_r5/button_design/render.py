# Concept renders for the hook-and-drop button (design only; proportions in the tip frame Q(d,t,z)).
from frame import *
import numpy as np, matplotlib; matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, Rectangle, Polygon, FancyArrowPatch, Circle, Ellipse
W=load_orig('left_wing'); CAR=load('carrier').reshape(-1,3); pc=toDT(CAR[:,:2])
CHAIR='#d8d4cc'; KIT='#ecebe7'; ACC='#ff6a2a'; INK='#2b3036'; MUTE='#8a9097'; BLUE='#2a6df4'
c=240.45; NZ=(c-20,c+20); BAND=(201.3,317.9); FIL=19.15
def segs(z):
    s=slice_z(W,z); p=np.stack([toDT(s[:,0,:2]),toDT(s[:,1,:2])],1); return p
def plot_segs(ax,p,**k):
    for a,b in p:
        if max(a[1],b[1])<45 and min(a[0],b[0])>-25 and max(a[0],b[0])<45: ax.plot([a[0],b[0]],[a[1],b[1]],**k)
def stadium(ax,x0,y0,w,h,**k):
    r=min(w,h)/2; ax.add_patch(FancyBboxPatch((x0-w/2+r*0.0,y0-h/2),w,h,boxstyle=f'round,pad=0,rounding_size={r}',**k))
def arrow(ax,x,y,dx,dy,col,lw=2.2,txt=None,**k):
    ax.add_patch(FancyArrowPatch((x,y),(x+dx,y+dy),arrowstyle='-|>',mutation_scale=14,color=col,lw=lw))
    if txt: ax.text(x+dx*1.05,y+dy*1.05,txt,color=col,fontsize=8,**k)
# x=160 car-side line in (d,t): x = 131.3+SHX+(d+2)*AXH0+t*TH0
def dline(x,t): return (x-P0[0]-SH[0]-t*TH[0])/AXH[0]-2
# arm root outline in (t,z) seen from outside: z extent per t
ts=np.arange(-48,-13.5,1.0); zlo=[];zhi=[]
for t in ts:
    m=np.abs(pc[:,1]-t)<0.8; zlo.append(CAR[m,2].min()); zhi.append(CAR[m,2].max())
zlo=np.array(zlo)-227+c; zhi=np.array(zhi)-227+c
# ================= FIG 1: placements =================
f,(a1,a2)=plt.subplots(1,2,figsize=(15,7.6),gridspec_kw={'width_ratios':[1,1.15]})
a1.set_title('Plan cut at the button height (z %.0f)  d + outboard, t + into the wing'%c,fontsize=10)
plot_segs(a1,segs(300),color=MUTE,lw=.8); a1.text(-12,34,'grey: original blade, z 300',fontsize=7,color=MUTE)
a1.add_patch(FancyBboxPatch((-6,-14),29,20,boxstyle='round,pad=0,rounding_size=6',fc=KIT,ec=INK,lw=1.2))
a1.add_patch(Rectangle((1.4,6),16,20,fc='none',ec=INK,ls='--',lw=1)); a1.text(3,20,'tongue\n(hooks behind\nlips)',fontsize=7)
a1.add_patch(Rectangle((1.4,-60),16,46,fc=KIT,ec=INK,lw=1)); a1.text(4,-40,'arm',fontsize=9)
a1.add_patch(Rectangle((23,-8),0.9,12,fc=ACC,ec='none')); a1.text(26,-3,'#1 outer side\npress in (-d)',color=ACC,fontsize=8,va='center')
a1.add_patch(Rectangle((-6,-15),0.01,0.01));
a1.text(-23,-20,'#4 front face:\nthe arm root\ncovers d 1..17',fontsize=8,color=MUTE)
tt=np.array([-60,30]); a1.plot(dline(160,tt),tt,color='#b03030',ls=':',lw=1.4); a1.text(dline(160,-55)+0.5,-55,'x = 160\n(car side?\nmeasure)',color='#b03030',fontsize=7)
arrow(a1,40,-2,-15,0,ACC,txt=None); arrow(a1,-22,-2,14,0,BLUE)
a1.text(-24,-6,'fingers under\nthe arm root',fontsize=7,color=BLUE)
a1.set_xlim(-26,48); a1.set_ylim(-62,40); a1.set_aspect('equal'); a1.grid(alpha=.25); a1.set_xlabel('d (mm)'); a1.set_ylabel('t (mm)')
a2.set_title('Outer side elevation (looking -d, from the car side)  nose at centre %.1f, 1 filler below, 3 above'%c,fontsize=10)
a2.add_patch(Rectangle((-14,BAND[0]),20,BAND[1]-BAND[0],fc=CHAIR,ec='none'))
edges=[BAND[0],NZ[0],NZ[1]]+[NZ[1]+FIL*i for i in (1,2,3)]
for e in edges: a2.plot([-14,6],[e,e],color=INK,lw=.6)
a2.add_patch(Rectangle((-14,NZ[0]),20,40,fc=KIT,ec=INK,lw=1.2)); a2.text(-13.3,NZ[1]-4,'nose 40',fontsize=7)
a2.fill_between(ts,zlo,zhi,color=KIT,ec=INK,lw=1); a2.text(-45,c+3,'arm',fontsize=9)
a2.axhline(197.8,xmin=0,xmax=.55,color=MUTE,ls='--',lw=1); a2.text(-60,193,'armrest top z 197.8 (pos-1 gap under the arm only 13.6 mm)',fontsize=7,color=MUTE)
stadium(a2,-2,c,12,24,fc=CHAIR,ec=INK,lw=1.2); a2.text(8,c+6,'#1 thumb button 12 x 24\n(outer side, press -d)',fontsize=8,color=ACC)
a2.add_patch(Ellipse((-17.5,c-17),7,6,fc='none',ec=BLUE,ls='--')); a2.text(-60,c-36,'#2 under-arm trigger\n(press +z = same as lift)',fontsize=8,color=BLUE)
a2.add_patch(Rectangle((-14.4,NZ[1]-1),0.8,1,fc=MUTE)); a2.text(-30,NZ[1]+8,'#3 top of nose: under a filler\n(hidden, and press -z fights the lift)',fontsize=8,color=MUTE)
arrow(a2,8,BAND[1]+6,0,9,INK,txt=" lift 8"); arrow(a2,-14,BAND[1]+10,-14,0,INK,txt="then pull",ha="right")
a2.set_xlim(-62,30); a2.set_ylim(185,340); a2.set_aspect('equal'); a2.grid(alpha=.25); a2.set_xlabel('t (mm)  <- front'); a2.set_ylabel('z (mm)')
f.tight_layout(); f.savefig('fig1_placements.png',dpi=110); plt.close(f)
# ================= FIG 2: favourite spec + sequence =================
f=plt.figure(figsize=(16,8.4)); gs=f.add_gridspec(2,4,height_ratios=[1.25,1])
a=f.add_subplot(gs[0,0]); a.set_title('Button face (outer side, looking -d)',fontsize=10)
a.add_patch(Rectangle((-14,-20),20,40,fc=KIT,ec=INK)); stadium(a,-2,0,12,24,fc=KIT,ec=INK,lw=1.3)
for y in np.linspace(-9,9,7): a.plot([-7.6,3.6],[y,y],color=MUTE,lw=.3)
a.plot([-4.5,-2,0.5],[2.2,5,2.2],color=INK,lw=1.4); a.plot([-4.5,-2,0.5],[-1.3,1.5,-1.3],color=INK,lw=1.4)
a.annotate('',(-8,-14),(4,-14),arrowprops=dict(arrowstyle='<->',lw=.8)); a.text(-2,-17.5,'12 (t -8..+4)',ha='center',fontsize=7)
a.annotate('',(5.5,-12),(5.5,12),arrowprops=dict(arrowstyle='<->',lw=.8)); a.text(4.5,13,'24',fontsize=7)
a.text(-13.5,17,'nose 40 tall, t -14..+6',fontsize=7); a.text(-13.5,-19,'gap 0.3 all round; R6 ends',fontsize=7)
a.text(-2,7.5,'2 engraved\nup-chevrons 0.4 deep',fontsize=6.5,ha='center')
a.set_xlim(-16,9); a.set_ylim(-21,21); a.set_aspect('equal'); a.axis('off')
a=f.add_subplot(gs[0,1]); a.set_title('Section through the button (vertical, d-z)',fontsize=10)
zz=np.linspace(-12,12,50); a.add_patch(Rectangle((-6,-20),29,40,fc=KIT,ec=INK)); a.add_patch(Rectangle((19.4,-12.3),3.6,24.6,fc='white',ec='none'))
scoop=23-0.8*(1-(zz/12)**2)
a.fill_betweenx(zz,19.5,scoop,fc='#f7f6f3',ec=INK,lw=1)
a.add_patch(Rectangle((19.5,-12.3),0.0,0)); a.add_patch(Rectangle((19.1,-12.6),0.4,25.2,fc=ACC,ec='none')); 
a.text(8,-17,'orange tell sleeve 0.6 wall,\nhidden 0.6 below the face\nwhen locked',fontsize=6.5,color=ACC)
a.text(8,15,'scoop 0.8 deep (cyl., axis t):\nlower rim = lift ledge',fontsize=6.5)
a.add_patch(Rectangle((12,-3),7.5,6,fc='#cfcac0',ec=INK)); a.text(9,-1,'spring',fontsize=6.5)
a.annotate('',(23,24),(19.5,24),arrowprops=dict(arrowstyle='<->',lw=.8)); a.text(19.6,25.3,'travel 3.5, hard stop',fontsize=6.5)
arrow(a,30,0,-6.5,0,ACC); a.set_xlim(-7,33); a.set_ylim(-21,28); a.set_aspect('equal'); a.axis('off')
a=f.add_subplot(gs[0,2]); a.set_title('Status: flush = locked, sunk + orange = not',fontsize=10)
for i,(st,dz) in enumerate([('LOCKED\nflush, quiet',0),('NOT DROPPED\nsunk 3.5, orange ring',3.5)]):
    x0=i*24; a.add_patch(Rectangle((x0,-14),18,28,fc=KIT,ec=INK))
    if dz: a.add_patch(FancyBboxPatch((x0+3,-12),12,24,boxstyle='round,pad=0,rounding_size=6',fc=ACC,ec='none'))
    stadium(a,x0+9,0,12-(1.6 if dz else 0),24-(1.6 if dz else 0),fc='#f7f6f3' if not dz else '#e0ded8',ec=INK)
    a.text(x0+9,-19,st,ha='center')
a.set_xlim(-2,44); a.set_ylim(-24,16); a.set_aspect('equal'); a.axis('off')
a=f.add_subplot(gs[0,3]); a.axis('off'); a.set_title('Spec',fontsize=10)
a.text(0,1,'''Outer side of the nose, centred on its 40 mm height
Q(d 23 face, t -2, z = tongue centre); 12 (t) x 24 (z) stadium
Face flat, square to travel -d; 0.8 scoop; 0.4 edge chamfer
Travel 3.5 to a hard stop; ~6 N at rest, ~9 N pressed
Defined click: tooth snaps into its wing notch at the end of
  the drop (lever edge 0.3 mm sharp, PETG on PETG)
Label: 2 engraved up-chevrons = "press, then up"
Colour: chair colour (one piece); orange only as the tell
  sleeve, seen when not locked. Accent cap optional.
Print: button standing (24 along z), layers match the
  nose's outer face; tell sleeve separate, orange.
Toddler: faces the car side, 23 mm outboard of the
  blade, invisible from the seat, needs press + lift 8
  + pull forward, three directions.''',va='top',fontsize=8.2,family='monospace',transform=a.transAxes)
# sequence
def hand(ax,step):
    ax.add_patch(Rectangle((-14,195),20,125,fc=CHAIR,ec='none'))
    dz=0 if step<2 else 8
    ax.add_patch(Rectangle((-14,NZ[0]+dz),20,40,fc=KIT,ec=INK)); ax.fill_between(ts,zlo+dz,zhi+dz,color=KIT,ec=INK,lw=.8)
    if step==3:
        for p in ax.patches[-1:]: pass
    stadium(ax,-2,c+dz,12,24,fc=KIT,ec=INK)
    ax.add_patch(Ellipse((-2,c+dz),11,17,fc='#e8b9a0',ec='#8a5a40',alpha=.8))
    ax.add_patch(Ellipse((-17.5,c-19+dz),7,9,fc='#e8b9a0',ec='#8a5a40',alpha=.85))
    for k in range(3): ax.add_patch(Ellipse((-52,c-28-10*k+dz),7,9,fc='#e8b9a0',ec='#8a5a40',alpha=.85))
    if step==1: ax.text(-16,c-31,'index in the\ncrook under\nthe arm root',fontsize=6.5); ax.text(-61,c-62,'other fingers\nround the arm',fontsize=6.5)
    ax.set_xlim(-62,22); ax.set_ylim(190,300); ax.set_aspect('equal'); ax.axis('off')
titles=['1  Wrap: fingers under the arm root,\n   thumb lands in the scoop','2  Press in 3.5 (click stop) ...','3  ... and lift 8 with the same grip\n   (thumb on the scoop rim)','4  Pull forward; let go of the button']
for i in range(4):
    a=f.add_subplot(gs[1,i]); hand(a,[1,1,2,3][i]); a.set_title(titles[i],fontsize=9,loc='left')
    if i==1: arrow(a,10,c,-9,0,ACC)
    if i==2: arrow(a,10,c+2,0,10,INK)
    if i==3: arrow(a,-40,c+30,-16,0,INK)
f.tight_layout(); f.savefig('fig2_favourite_spec.png',dpi=110); plt.close(f)
# ================= FIG 3: A / B / C forms =================
f,ax=plt.subplots(1,3,figsize=(15,7.0))
for k,a in enumerate(ax):
    a.add_patch(Rectangle((-14,BAND[0]),40,BAND[1]-BAND[0],fc=CHAIR,ec='none'))
    a.add_patch(Rectangle((-14,NZ[0]),20,40,fc=KIT,ec=INK,lw=1.1)); a.fill_between(ts,zlo,zhi,color=KIT,ec=INK,lw=.8)
    for e in edges: a.plot([-14,6],[e,e],color=INK,lw=.5)
    a.plot([6,6],[BAND[0],BAND[1]],color=INK,lw=.5)
    a.set_xlim(-50,30); a.set_ylim(200,290); a.set_aspect('equal'); a.axis('off')
stadium(ax[0],-2,c,12,24,fc=KIT,ec=INK); ax[0].plot([-5,1],[c,c],color=INK,lw=1.2)
ax[0].set_title('A  "button is the catch"\n12 x 24 scoop, same face as B; squeeze in (-d)\nagainst fingers on the inner face, pull forward\n(one engraved bar, not an arrow: no lift)',fontsize=9)
stadium(ax[1],-2,c,12,24,fc=KIT,ec=INK); ax[1].plot([-4.5,-2,0.5],[c+2,c+5,c+2],color=INK); ax[1].plot([-4.5,-2,0.5],[c-1.5,c+1.5,c-1.5],color=INK)
ax[1].set_title('B  hook-and-drop (favourite)\n12 x 24 scoop, 2 up-chevrons; press in, lift 8, pull\norange tell visible only when not dropped',fontsize=9)
a=ax[2]; zc=c; a.add_patch(Circle((15,zc),10,fc=CHAIR,ec=INK,lw=.6)); a.add_patch(Circle((15,zc),8.6,fc='#c9c4ba',ec=INK,lw=.8)); a.add_patch(Circle((15,zc),2.2,fc='#9aa0a6',ec=INK,lw=.6))
a.add_patch(Ellipse((15,zc+8.6),9,2.6,fc='white',ec=INK,lw=.6)); a.add_patch(Ellipse((15,zc-8.6),9,2.6,fc='white',ec=INK,lw=.6))
a.set_title('C  steel pin through the wing outer wall\nflush Ø17 pull-disc (chair colour) on the pin, in a Ø20\ncounterbore; 2 thumb-and-finger notches top/bottom;\npull 8 out, twist 90° to park; parked = 8 proud',fontsize=9)
a.text(15,zc-16,'on the wing, t +9,\nbehind the nose seam',fontsize=7,ha='center')
f.tight_layout(); f.subplots_adjust(top=0.80); f.savefig('fig3_ABC_forms.png',dpi=110); plt.close(f)
# ================= FIG 4: the block above the arm =================
f,(a1,a2)=plt.subplots(1,2,figsize=(14,7.4),gridspec_kw={'width_ratios':[1,1.1]})
zs=np.arange(185,352,3); tf=[];dlo=[];dhi=[]
for z in zs:
    s=slice_z(W,z); p=toDT(s.reshape(-1,3)[:,:2]); p=p[(p[:,0]>-12)&(p[:,0]<30)&(p[:,1]<30)]
    t0=p[:,1].min(); tf.append(t0); b=p[p[:,1]<t0+12]; dlo.append(b[:,0].min()); dhi.append(b[:,0].max())
tf=np.array(tf)
a1.plot(tf,zs,color=INK,lw=2,label='original leading edge (rake ~4°)')
a1.plot([-14,-14],[201.3,317.9],color=ACC,lw=2.5,label='draft nose/filler front, vertical t -14')
k=(-7.8+16.2)/(320-200); zz=np.array([201.3,317.9]); a1.plot(-16.2+k*(zz-200)+1.0,zz,color=BLUE,lw=2.5,ls='--',label='suggestion: band raked to the edge')
a1.fill_betweenx(zs,np.minimum(tf,-14),np.where((zs>201)&(zs<318),-14,tf),where=(zs>201)&(zs<318)&(tf>-14),color=ACC,alpha=.25)
a1.text(-13.5,300,'draft stands up to\n6 mm proud of the\noriginal edge here',fontsize=8,color=ACC)
a1.set_title('Leading edge in side view (t vs z)',fontsize=10); a1.legend(fontsize=8,loc='lower right'); a1.grid(alpha=.3); a1.set_xlabel('t (mm, - = front)'); a1.set_ylabel('z'); a1.set_xlim(-20,6)
a2.set_title('Plan sections: original blade vs draft nose band (z 300)',fontsize=10)
plot_segs(a2,segs(300),color=INK,lw=1)
a2.add_patch(FancyBboxPatch((-6,-14),29,20,boxstyle='round,pad=0,rounding_size=6',fc=ACC,alpha=.18,ec=ACC,lw=1.5))
a2.text(-5,-19,'draft band section 29 wide, front t -14',color=ACC)
a2.add_patch(FancyBboxPatch((-3.4,-9.2),22.5,15.2,boxstyle='round,pad=0,rounding_size=7',fc='none',ec=BLUE,lw=1.8,ls='--'))
a2.text(-3,9,'suggestion: blade section 22-24 wide,\nslot 12 (one Ø6 rod), swell only inboard',color=BLUE)
for z,col in [(200,MUTE),(250,MUTE),(330,MUTE)]:
    i=np.argmin(abs(zs-z)); a2.plot([dlo[i],dhi[i]],[tf[i]+3,tf[i]+3],color=col,lw=3,alpha=.6); a2.text(dhi[i]+1,tf[i]+3,'blade near edge z %d: d %.0f..%.0f'%(z,dlo[i],dhi[i]),fontsize=7,color=col)
a2.set_xlim(-14,48); a2.set_ylim(-22,24); a2.set_aspect('equal'); a2.grid(alpha=.3); a2.set_xlabel('d (+ outboard)'); a2.set_ylabel('t')
f.tight_layout(); f.savefig('fig4_block_above_arm.png',dpi=110); plt.close(f)
print('ok')
