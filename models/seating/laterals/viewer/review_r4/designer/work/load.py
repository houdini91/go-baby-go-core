import json, numpy as np, os
RAW='/home/mikey/go-baby-go-core/models/seating/laterals/viewer/raw/'
NAMES={'wing':'ps1_RiBH','armrest':'ps1_J_D','back':'ps1_RhDH','base':'ps1_J_H','carrier':'ps1_RUgD','button':'kit11_JzD','depth':'kit11_RzCD','pad':'kit11_RcDD','cushion':'kit11_RTED','blankA':'ps1_RkkD','blankB':'ps1_RvkD','x1':'kit11_RhBD','x2':'kit11_RKED'}
CACHE=os.path.dirname(__file__)+'/cache/'
def load(n):
    os.makedirs(CACHE,exist_ok=True)
    c=CACHE+n+'.npy'
    if os.path.exists(c): return np.load(c)
    d=json.load(open(RAW+NAMES[n]+'.json'))
    tris=[]
    for b in d:
        for f in b['faces']:
            for t in f['facets']: tris.append(t['vertices'])
    a=np.array(tris,dtype=np.float32)*1000
    np.save(c,a); return a
if __name__=='__main__':
    for n in NAMES:
        a=load(n); print(n,a.shape[0],a.reshape(-1,3).min(0).round(1),a.reshape(-1,3).max(0).round(1))
def load_stl(path):
    import struct
    b=open(path,'rb').read()
    if b[:5]==b'solid' and b'facet' in b[:300]:
        v=[list(map(float,l.split()[1:])) for l in b.decode().splitlines() if l.strip().startswith('vertex')]
        return np.array(v,np.float32).reshape(-1,3,3)
    n=struct.unpack('<I',b[80:84])[0]
    a=np.frombuffer(b[84:84+n*50],dtype=np.dtype([('n','<3f4'),('v','<9f4'),('a','<u2')]))
    return a['v'].reshape(-1,3,3).astype(np.float32)
def load_orig(n):
    c=CACHE+'orig_'+n+'.npy'
    if os.path.exists(c): return np.load(c)
    a=load_stl('/home/mikey/research/onshape/gobabygo/chair_mesh/chair_'+n+'.stl'); np.save(c,a); return a
