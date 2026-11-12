import math
# per age: H (HJC spacing) P5/50/95, BKL P5/50/95 (file), Wk skin knee breadth, mid-thigh circ (1975)
s={2:0.844,3:0.928,4:1.0,5:1.067}
bisp={2:(124,143,162),3:(125,146,165),4:(128,148,167),5:(133,158,175)}
BKL={2:(250,272,298),3:(278,300,330),4:(299,327,364),5:(317,350,382)}
mtc={2:(219,252,282),3:(230,267,301),4:(246,279,316),5:(262,293,337)}
for a in (2,3,4,5):
  H=[0.59*b for b in bisp[a]]
  Wk=[x*s[a] for x in (54,62,70)]
  L=[0.69*b for b in BKL[a]]
  dt=[0.85*c/math.pi for c in mtc[a]]   # distal thigh breadth est
  print(f"age {a}: H {[round(x) for x in H]} Wk {[round(x) for x in Wk]} L {[round(x) for x in L]} midthighB {[round(c/math.pi) for c in mtc[a]]} distalB {[round(x) for x in dt]}")
  for i,lab in ((0,'P5'),(1,'P50'),(2,'P95')):
    knee=[H[i]+2*L[i]*math.sin(math.radians(t))-Wk[i] for t in (0,5,10,15,20)]
    d=L[i]-45
    th=[H[i]+2*d*math.sin(math.radians(t))-dt[i] for t in (0,5,10,15,20)]
    hjcx=BKL[a][i]-L[i]-Wk[i]*0.95/2
    print(f"  {lab}: kneeGap {[round(x) for x in knee]}  distalThighGap(d=L-45) {[round(x) for x in th]}  HJCx {round(hjcx)} kneeCentre-from-back {round(BKL[a][i]-Wk[i]*0.95/2)} past front {round(BKL[a][i]-Wk[i]*0.95/2-170)}")
