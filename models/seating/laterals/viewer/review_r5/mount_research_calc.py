import math
pi=math.pi
M=19000.0  # N.mm yaw @300N lean (brief); knock 27500; strength.md says 24000
Ep=2000.0
w,h,L=15.8,32.0,24.5
It=h*w**3/12; Zt=h*w**2/6
print(f"tongue I={It:.0f} Z={Zt:.0f}  sigma@19={M/Zt:.1f} SF={40/(M/Zt):.2f}  @24 SF={40/(24000/Zt):.2f}")
def rod(d): return pi*d**4/64, pi*d**3/32, pi*d**2/4
for grade,Ec,sf_flex,tau in [("low (EC hobby TDS)",30000,300,30),("mid (brief)",100000,1000,41),("high (RockWest T700)",130000,1800,41)]:
    print("\n==",grade,"E",Ec,"flex",sf_flex,"tau",tau)
    n=Ec/Ep
    # Ø2 corner rods, 4 and 6 rods
    for nr,zs in [(4,[13.5,-13.5]*1),(6,[13.5,0,-13.5])]:
        Ir=pi*1**2*5.4**2*(n-1)* (nr)
        Itr=It+Ir
        sp=M*7.9/Itr; sr=n*M*5.4/Itr; Frod=sr*pi
        print(f" O3 {nr}xØ2 corner rods: I x{Itr/It:.2f}  PETG {sp:.1f}MPa SF {40/sp:.1f}  rod {sr:.0f}MPa SF {sf_flex/sr:.1f}  rod force {Frod:.0f}N bond len@4MPa {Frod/(pi*2*4):.0f}mm")
    # prong plug: two Ø8 solid rods + tongue
    I8,Z8,A8=rod(8)
    kr=2*Ec*I8; kt=Ep*It
    for share_note,kt_eff in [("EI share",kt)]:
        fr=kr/(kr+kt_eff); Mr=M*fr/2; Mt=M*(1-fr)
        srod=Mr/Z8; Fb=Mr/(24.5-6)+300*fr/2; sb=Fb/(8*6)
        print(f" O1 prong plug: rods take {fr*100:.0f}%  rod {srod:.0f}MPa SF {sf_flex/srod:.1f}  bore bearing {sb:.1f}MPa SF {25/sb:.1f}  tongue SF {40/(Mt/Zt):.1f}")
    # cross-rod key, Ø8, lever a=16
    a=16; F=M/a; V=F/2
    print(f" O2 cross-rod key: F per rod {F:.0f}N, rod shear at tongue face {V/A8:.1f}MPa SF {tau/(V/A8):.1f}; rod bend {V*5/Z8:.0f}MPa SF {sf_flex/(V*5/Z8):.1f}; groove bearing {V/(8*10):.1f}MPa SF {25/(V/80):.1f}; shear-out in tongue {F/(2*3.9*32):.1f}MPa SF {18/(F/(2*3.9*32)):.1f}")
    # fixed pin + fork
    a=17;F=M/a
    print(f" O4 pin+fork: F {F:.0f}N pin dbl shear {F/2/A8:.1f}MPa SF {tau/(F/2/A8):.1f}; pin bend ss {F*6.5/Z8:.0f}MPa SF {sf_flex/(F*6.5/Z8):.1f}; tine interlayer shear {F/(3.9*32):.1f}MPa SF {18/(F/(3.9*32)):.1f}")
    # twin Ø8 rods replacing tongue entirely
    Mr=M/2; print(f" twin-rod tongue only: rod {Mr/Z8:.0f}MPa SF {sf_flex/(Mr/Z8):.1f}; bearing {(Mr/18.5+75)/48:.1f}MPa SF {25/((Mr/18.5+75)/48):.1f}")
