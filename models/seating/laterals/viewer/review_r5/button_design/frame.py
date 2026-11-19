import sys,numpy as np
sys.path.insert(0,'/home/mikey/go-baby-go-core/models/seating/laterals/viewer/review_r4/designer/work')
from load import load, load_orig
from slice import slice_z
P0=np.array([131.3,80.7]);AXH=np.array([0.79577626,0.60559074]);TH=np.array([-0.6083053,0.79370313])
SH=np.array([(244.25-227.8)*0.8/32.9,(244.25-227.8)*2.7/32.9])
def toDT(xy):
    q=xy-P0-SH; return np.stack([q@AXH-2,q@TH],-1)
def Q(d,t,z): xy=P0+(d+2)*AXH+t*TH+SH; return np.array([xy[0],xy[1],z])
