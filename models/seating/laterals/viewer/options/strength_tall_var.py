# No-prong gains for the push-button tongue: taller tongues (64 / 80 / 90 mm) with the R2 outer root, using the CAD agent's
# contact model exactly as in ../review_r5/cad/strength_r5b/tall.py (sf_r5b.py, r5b patches; only the tongue / notch z change).
# "First gives" = 300 N x the lowest safety factor among: wing slot wall peak bearing (dent onset), tongue (Kt 1.4/1.3),
# and the r5b arm (1.90 / 1.24) and tenon fuse (2.87 / 1.16), which do not change with the tongue.
# Run: ../.venv/bin/python strength_tall_var.py   (writes strength_tall_var.json here)
import os, sys, json
HERE = os.path.dirname(os.path.abspath(__file__)); SB = os.path.join(HERE, '..', 'review_r5', 'cad', 'strength_r5b')
os.chdir(SB); sys.argv = ['x', 'zero']
base = open('sf_r5b.py').read().split("if __name__ == '__main__':")[0]
OUT = {}
for H_TALL in (64.0, 80.0, 90.0):
    z0 = 214.3; z1 = z0 + H_TALL; nz = (207.0, z1 + 2.7)
    src = base.replace("z = np.arange(210, 250, H) + H / 2", "z = np.arange(205, %.1f, H) + H / 2" % (nz[1] + 5))
    # (the torsion term keeps tall.py's 32.1 mm height, as the CAD agent ran it: conservative for the taller tongues)
    ns = {}
    exec(src, ns)
    G = ns['G']; G.TG_Z = (z0, z1); G.NZ = nz
    ns.update(TG_Z=G.TG_Z, NZ=nz, BAR_Z0=z1 - 5.2, TO_Z=(z1 - 32.8, z1 - 0.8))
    PT = ns['patches']('zero'); L = ns['L']; np = ns['np']
    res = {}
    for nm, where in (('lean', 'centre'), ('corner', 'corners')):
        rows = []; po = 0
        for cfg in ns['CONFIGS']:
            pts = L.pad_points(*cfg)
            for Pw in ([pts['centre']] if where == 'centre' else pts['corners']):
                f, M = L.wrench(np.array([300.0, 0, 0]), Pw)
                r, u, err, fp = L.solve(f, M, PT); rows.append(fp); po = max(po, r['outer wall']['pmax'])
        sf_t = ns['tongue_sf'](rows, 2.0, 1.4, 1.3)[0]; sf_w = 25 / po
        arm, fuse = (1.90, 2.87) if nm == 'lean' else (1.24, 1.16)
        items = {'wing dent': sf_w, 'tongue': sf_t, 'arm': arm, 'tenon fuse': fuse}
        k = min(items, key=items.get)
        res[nm] = dict(tongue=round(sf_t, 2), wing_peak=round(sf_w, 2), first_gives_N=round(300 * items[k]), part=k, wing_dent_N=round(300 * sf_w))
    OUT[f'{H_TALL:.0f} mm tall (z {z0}-{z1:.1f}), R2 outer root'] = res
    print(H_TALL, res, flush=True)
json.dump(OUT, open(os.path.join(HERE, 'strength_tall_var.json'), 'w'), indent=1)
