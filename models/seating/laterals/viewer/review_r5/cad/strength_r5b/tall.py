# Tall tongue (15.8 x 64, z 214.3-278.3) concept check with the r5b contact model (sf_r5b.py), no CAD.
import sys, json
sys.argv = ['x', 'zero']
src = open('sf_r5b.py').read().split("if __name__ == '__main__':")[0]
src = src.replace("z = np.arange(210, 250, H) + H / 2", "z = np.arange(205, 290, H) + H / 2")
exec(src)
G.TG_Z = (214.3, 278.3); G.NZ = (207.0, 281.0); TG_Z = G.TG_Z; NZ = G.NZ
BAR_Z0 = 273.1; TO_Z = (245.5, 277.5)
# the latch globals used by patches()/tongue_mask() are module-level names in this exec'd namespace
globals()['BAR_Z0'] = BAR_Z0; globals()['TO_Z'] = TO_Z; globals()['TG_Z'] = TG_Z; globals()['NZ'] = NZ
PT = patches('zero')
D, Z, m = tongue_mask(2.0); print('tall tongue mouth section %.0f mm2' % (m.sum() * H * H))
def run(kt_out, kt_in, rod_Nm=0.0):
    out = {}
    for P in (300.0, 600.0, 900.0):
        for nm, where, Fd in (('lean', 'centre', [P, 0, 0]), ('corner', 'corners', [P, 0, 0])):
            rows = []; po = 0; pm = 0; ft = 0
            for cfg in CONFIGS:
                pts = L.pad_points(*cfg)
                for Pw in ([pts['centre']] if where == 'centre' else pts['corners']):
                    f, M = L.wrench(np.array(Fd, float), Pw)
                    if rod_Nm: M = M - np.sign(M) * np.minimum(np.abs(M), np.array([0, 0, rod_Nm * 1000]))   # rods relieve yaw (world z ~ S z)
                    r, u, err, fp = L.solve(f, M, PT); rows.append(fp)
                    po = max(po, r['outer wall']['pmax']); ft = max(ft, r['tooth (latch)']['F'])
                    fpt, A, names, grp = fp; gi = names.index('outer wall'); sel = (grp == gi) & (fpt > 0)
                    if sel.any(): pm = max(pm, fpt[sel].sum() / A[sel, 7].sum())
            sf = tongue_sf(rows, 2.0, kt_out, kt_in)[0]
            out[f'{nm} {P:.0f}'] = dict(tongue=round(sf, 2), wall_peak=round(25 / po, 2), wall_mean=round(25 / pm, 2), tooth_F=round(ft))
    return out
res = {'Kt 1.8/1.3 (as r5b roots)': run(1.8, 1.3), 'nominal': run(1.0, 1.0), 'outer root R2 (Kt 1.4/1.3)': run(1.4, 1.3),
       '+ 2 Ø2 corner rods (4.8 N.m yaw relief), Kt 1.8/1.3': run(1.8, 1.3, 4.8)}
for k, v in res.items():
    print('==', k)
    for c, r in v.items(): print('  ', c, r)
json.dump(res, open('tall_out.json', 'w'), indent=1)
