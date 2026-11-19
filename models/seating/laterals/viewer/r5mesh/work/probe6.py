import sys,json; sys.path.insert(0,'..')
from frame import *
def raw(name):
    B=json.load(open(f'../../raw/{name}.json'))
    a=np.array([fc['vertices'] for body in B for face in body['faces'] for fc in face['facets']],float).reshape(-1,3)*1000
    m=trimesh.Trimesh(a,np.arange(len(a)).reshape(-1,3)); m.merge_vertices(digits_vertex=4); m.apply_transform(M_W2F); return m
m=raw('ps1_RUgD')
parts=m.split(only_watertight=False)
for p in parts: print(len(p.faces), round(p.volume,1), p.bounds.round(1).tolist())
b=raw('kit11_JzD'); print('button', [ (len(p.faces), p.bounds.round(1).tolist()) for p in b.split(only_watertight=False)])
