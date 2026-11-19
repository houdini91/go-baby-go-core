import numpy as np, sys, time
from load import load; from render import render, Q
OUT='/home/mikey/go-baby-go-core/models/seating/laterals/viewer/review_r4/designer/'
C=dict(wing=(0.86,0.83,0.77),armrest=(0.80,0.80,0.80),back=(0.82,0.82,0.82),base=(0.75,0.75,0.75),carrier=(0.96,0.96,0.97),button=(1.0,0.45,0.18),depth=(0.95,0.95,0.96),cushion=(0.20,0.26,0.45),pad=(0.9,0.9,0.92),blankA=(0.3,0.65,0.85),blankB=(0.3,0.65,0.85))
def P(names): return [(load(n),C[n]) for n in names]
kit=['carrier','button','depth','cushion','blankA','blankB']
chair=['wing','armrest','back','base']
which=sys.argv[1:]
tgt=np.array([135,70,260])
def eyeFrom(az,el,dist=900,t=tgt):
    a=np.radians(az); e=np.radians(el)
    return t+dist*np.array([np.cos(e)*np.cos(a),np.cos(e)*np.sin(a),np.sin(e)])
jobs={
 'v1_overall_iso':(chair+kit, eyeFrom(-40,25,1200,np.array([60,40,230])), np.array([60,40,230]),2.6,'Chair + kit (pos 1 LOW, D0), front-outboard iso'),
 'v2_tip_kit':(chair+kit, eyeFrom(-35,22), tgt,3.6,'Wing tip with kit: block + blanks + tongue'),
 'v3_tip_bare':(chair, eyeFrom(-35,22), tgt,3.6,'Wing tip, kit removed: the slot is a deep hole in an added block'),
 'v4_tip_front':(chair+kit, eyeFrom(-90,8), tgt,4.6,'Front view (looking +y)'),
 'v5_tip_top':(chair+kit, eyeFrom(-60,75), tgt,4.6,'From above-front: block footprint vs wing blade'),
 'v6_tip_out':(chair+kit, eyeFrom(0,10), tgt,4.6,'Outboard side view (looking -x)'),
}
for k in (which or jobs):
    n,e,t,s,ti=jobs[k]; t0=time.time()
    render(P(n),e,t,s,fname=OUT+k+'.png',title=ti); print(k,round(time.time()-t0,1))
