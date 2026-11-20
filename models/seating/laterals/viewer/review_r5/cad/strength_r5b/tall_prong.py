# Tall tongue (R2 outer root, 2 Ø2 corner rods) + 2 Ø8 prongs: estimate. The prongs take a share s of the yaw and sr of the twist
# off the tongue and the slot walls (conservative shares; the split depends on the rod grade, mount_research.md O1).
import sys, json
exec(open('tall.py').read().split("res = {")[0])
def run2(kt_out, kt_in, rod_Nm, s, sr):
    out = {}
    for P in (300.0,):
        for nm, where in (('lean', 'centre'), ('corner', 'corners')):
            rows = []; po = 0; ft = 0
            for cfg in CONFIGS:
                pts = L.pad_points(*cfg)
                for Pw in ([pts['centre']] if where == 'centre' else pts['corners']):
                    f, M = L.wrench(np.array([P, 0, 0]), Pw)
                    M = M * np.array([1.0, 1.0 - sr, 1.0 - s])
                    M = M - np.sign(M) * np.minimum(np.abs(M), np.array([0, 0, rod_Nm * 1000]))
                    r, u, err, fp = L.solve(f, M, PT); rows.append(fp)
                    po = max(po, r['outer wall']['pmax']); ft = max(ft, r['tooth (latch)']['F'])
            out[nm] = dict(tongue=round(tongue_sf(rows, 2.0, kt_out, kt_in)[0], 2), wall_peak=round(25 / po, 2), tooth=round(25 / (ft / 77.0), 2))
    return out
R = {f'prongs take {int(s*100)} % yaw / {int(sr*100)} % twist': run2(1.4, 1.3, 4.8, s, sr) for s, sr in ((0.35, 0.6), (0.5, 0.7))}
print(json.dumps(R, indent=1)); json.dump(R, open('tall_prong_out.json', 'w'), indent=1)
