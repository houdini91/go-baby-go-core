import sys; sys.path.insert(0,'..')
from frame import *
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt
m=load_wing_frame()
print('W7 hole z186', toDTZ([141.3,88.5,186.6]), 'W2 bolt axes', toDTZ([131.3,80.7,227.8]), toDTZ([132.1,83.4,260.7]))
ZS=[195,200,205,207,212,216,220,227,235,240,247,252,258,262,266,270,275,285]
fig,axs=plt.subplots(3,6,figsize=(36,24))
for ax,z in zip(axs.flat,ZS):
    s=m.section(plane_origin=[0,0,z],plane_normal=[0,0,1])
    for e in s.discrete: ax.plot(e[:,0],e[:,1],'k',lw=.8)
    ax.set_title(f'z {z}');ax.set_aspect('equal');ax.set_xlim(-15,30);ax.set_ylim(-20,40);ax.grid(alpha=.4)
    ax.set_xticks(range(-15,31,2)); ax.set_yticks(range(-20,41,2)); ax.tick_params(labelsize=6)
plt.tight_layout(); plt.savefig('probe2.png',dpi=45)
