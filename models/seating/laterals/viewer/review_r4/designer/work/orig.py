import numpy as np, sys
from load import load, load_orig; from render import render
OUT='/home/mikey/go-baby-go-core/models/seating/laterals/viewer/review_r4/designer/'
tgt=np.array([135,70,270])
def eyeFrom(az,el,dist=900,t=tgt):
    a=np.radians(az); e=np.radians(el); return t+dist*np.array([np.cos(e)*np.cos(a),np.cos(e)*np.sin(a),np.sin(e)])
W=(0.86,0.83,0.77)
k=sys.argv[1]
if k=='a': render([(load_orig('left_wing'),W),(load('armrest'),(0.8,0.8,0.8)),(load('back'),(0.82,0.82,0.82)),(load_orig('support_left_wing'),(0.6,0.6,0.6))],eyeFrom(-35,22),tgt,3.6,fname=OUT+'v7_original_wing_tip.png',title='ORIGINAL wing tip (before v1.1 block), same view as v3')
if k=='b': render([(load_orig('left_wing'),W),(load('armrest'),(0.8,0.8,0.8))],eyeFrom(10,12),tgt,3.6,fname=OUT+'v8_original_wing_out.png',title='ORIGINAL wing tip, outboard side')
