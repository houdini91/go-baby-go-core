# Port of armPath/armAt (gbg_sidepad_v1.fs) -> arm centreline in the tip frame (d,t,z); check the space in front of the cells
import numpy as np, math
P0=np.array([131.3,80.7]);AXH=np.array([0.79577626,0.60559074]);TH=np.array([-0.6083053,0.79370313]);SH=np.array([(244.25-227.8)*0.8/32.9,(244.25-227.8)*2.7/32.9])
D_SHIFT=2.0; YEND=35.0
def Q(d,t,z): xy=P0+(d+D_SHIFT)*AXH+t*TH+SH; return np.array([xy[0],xy[1],z])
def to_dt(x,y): r=np.array([x,y])-P0-SH; return r@AXH-D_SHIFT, r@TH
def armPath(c,zp,T0=-14.0):
    st=Q(9.4,T0,c); h0=-TH; th0=math.degrees(math.atan2(h0[1],h0[0]))
    R=(YEND-st[1])/(math.sin(math.radians(th0-90))-1)
    C=[st[0]+R*math.cos(math.radians(th0-90)), st[1]+R*math.sin(math.radians(th0-90))]
    L=R*math.radians(th0+180)  # FS: R*(th0+PI) with th0 in deg -> PI adds 180deg (angle units)
    dz=zp-c; thv=2*math.degrees(math.atan(abs(dz)/L)); Rv=L/(2*math.sin(math.radians(thv)))
    return dict(st=st,th0=th0,R=R,C=C,L=L,dz=dz,Rv=Rv,c=c)
def armAt(p,u):
    th=p['th0']-math.degrees(u/p['R'])
    x=p['C'][0]+p['R']*math.cos(math.radians(th+90)); y=p['C'][1]+p['R']*math.sin(math.radians(th+90))
    if u<=p['L']/2: ph=math.asin(min(u/p['Rv'],1)); zz=p['Rv']*(1-math.cos(ph))
    else: ph=math.asin(min((p['L']-u)/p['Rv'],1)); zz=abs(p['dz'])-p['Rv']*(1-math.cos(ph))
    s=-1 if p['dz']<0 else 1
    return x,y,p['c']+s*zz
c=259.6
for name,zp in (('LOW',c-47.5),('HIGH',c+47.5)):
    p=armPath(c,zp); print(name,'R %.1f L %.1f th0 %.1f'%(p['R'],p['L'],p['th0']))
    for u in np.linspace(0,p['L'],13):
        x,y,z=armAt(p,u); d,t=to_dt(x,y); print('  u %5.1f  d %6.1f t %6.1f z-c %6.1f  x %.1f'%(u,d,t,z-c,x))
