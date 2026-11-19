# Run the contact model for the review load set; keep per-point forces of the governing configuration per item.
import numpy as np, json, pickle
from geom import *
import loads as L
SET = {  # name: (world force at pad, where, magnitude)
 'lean 300 (centre)':   ([1, 0, 0], 'centre', 300.0),
 'proof 600 (centre)':  ([1, 0, 0], 'centre', 600.0),
 'knock 900 out (corner)': ([1, 0, 0], 'corners', 900.0),
 'knock 900 in (corner)':  ([-1, 0, 0], 'corners', 900.0),
 'knock 900 back (corner)': ([0, 1, 0], 'corners', 900.0),
 'pull fwd 300 (centre)': ([0, -1, 0], 'centre', 300.0),
 'pull fwd 600 (centre)': ([0, -1, 0], 'centre', 600.0),
 'pull fwd 900 (centre)': ([0, -1, 0], 'centre', 900.0),
}
def run(gap_mode):
    PT = L.patches(gap_mode); out = {}
    for cn, (dirw, where, P) in SET.items():
        rows = []
        for cfg in CONFIGS:
            pts = L.pad_points(*cfg)
            for k, Pw in enumerate([pts['centre']] if where == 'centre' else pts['corners']):
                f, M = L.wrench(np.array(dirw, float) * P, Pw)
                res, u, err, (fpt, A, names, grp) = L.solve(f, M, PT)
                rows.append(dict(cfg=cfg, Pw=Pw, F=f, M=M, res=res, fpt=fpt, A=A, names=names, grp=grp, corner=k))
        out[cn] = rows
    return out
if __name__ == '__main__':
    for gm in ('cad', 'zero'):
        pickle.dump(run(gm), open(f'cases_{gm}.pkl', 'wb'))
    print('ok')
