import numpy as np
# cam: rod along t, shuttle along d, slot direction (SR,RT) angle a from t. Return quasi-static force on rod needed
# to move shuttle against resistance Fd (driving direction 'press': rod->shuttle in; 'ret': spring->rod->shuttle out),
# and force on shuttle needed to back-drive rod against Ft ('ins').
def rod_to_sh(RT,SR,mu,Fd):
    a=np.arctan2(SR,RT)               # slot angle from t
    # contact normal makes angle (90-a) with t; pressure angle wrt follower (d) = a
    # follower: N*sin? solve: N(cos a_n_d) - mu N sin... use friction-angle form
    phi=np.arctan(mu)
    p=a                                # pressure angle at follower
    if p+phi>=np.pi/2-1e-9: return np.inf
    # force along t on rod = Fd*tan(p+phi) (contact) ; follower guide friction: shuttle pressed on channel by t-comp -> adds mu*Ft_contact to resistance
    # iterate
    F=Fd
    for _ in range(50):
        Ft=F*np.tan(p+phi); F=Fd+mu*Ft
    Ft=F*np.tan(p+phi)
    return Ft*(1+mu*np.tan(phi))       # rough rod guide friction from the d-component
def sh_to_rod(RT,SR,mu,Ft):
    a=np.arctan2(SR,RT); p=np.pi/2-a; phi=np.arctan(mu)
    if p+phi>=np.pi/2: return np.inf
    return Ft*np.tan(p+phi)*(1+mu)
def drawer(theta,mu,k=1.64):
    e=np.cos(theta)-mu*np.sin(theta)*k
    return np.inf if e<=0 else 1/e
spring=lambda x,F0,k: F0+k*x
for name,RT,SR,stop in (('now RT3.5/SR4.5 (52deg)',3.5,4.5,3.9),('prop RT5/SR5 (45deg)',5.0,5.0,5.5)):
    print('==',name,'ratio SR/RT %.2f'%(SR/RT))
    for mu in (0.2,0.35,0.5):
        drag=1.0   # shuttle drag N (printed, worst)
        ret=rod_to_sh(RT,SR,mu,drag)     # spring force needed to drive shuttle out against drag
        ins=sh_to_rod(RT,SR,mu,1.0)      # shuttle force per 1 N of rod/spring resistance
        print(f' mu {mu}: spring force needed to return shuttle vs 1 N drag = {ret:.1f} N; shuttle push to back-drive per N spring = {ins:.2f}')
# spring candidates: installed length now 9.4 (rod end t10.1 -> seat 19.5)
G=79e3
def k_of(d,OD,n): D=OD-d; return G*d**4/(8*D**3*n), D
for (d,OD,Lf,nt,Li,trav) in ((0.5,6.0,15,9,9.4,3.9),(0.5,6.0,15,9,11.0,5.5),(0.55,6.0,16,9,11.0,5.5),(0.6,6.0,12,8,9.4,3.9)):
    n=nt-2; k,D=k_of(d,OD,n); F0=k*(Lf-Li); F1=k*(Lf-Li+trav); Ls=nt*d
    C=D/d; Kw=(4*C-1)/(4*C-4)+0.615/C; tau=Kw*8*F1*D/(np.pi*d**3)
    print(f'spring d{d} OD{OD} Lf{Lf} coils{nt}: k {k:.2f} N/mm, solid {Ls:.1f}, installed {Li} -> F0 {F0:.1f} N, at stop (+{trav}) {F1:.1f} N, left {Li-trav:.1f} mm, tau {tau:.0f} MPa')
print('-- press force at cap, F_needed = spring_at_stop + 1.5 N drag')
for th in (0,15,30,48):
    for mu in (0.3,0.5):
        print(f' cap/thumb {th} deg off axis, mu {mu}: multiplier {drawer(np.radians(th),mu):.2f}')
# 300 N knock at pad centre, pad centre rel tongue centre in (d,t,z)
r=np.array([-62.6,-33.3,-47.5]); F=300*np.array([0.796,-0.608,0])
print('F (d,t,z)',F.round(0),'M (d,t,z) N.m',(np.cross(r,F)/1000).round(1))
