# X FINAL internals (gen_xi.py, fine-tuned): blade 17 x 90 to t' 10, beak 11 x 11 on the ledge, catch 11 x 3.0, Ø6 cam bosses at t' 6.
# Derived from x_strength.py: option X (closed "exhaust" socket in the leading edge, insert-and-rotate-in) vs F, on the SAME model as h2h_strength.py:
# the CAD agent's r5b contact solver (loads.py via sf_r5b.py), zero fit gaps, 300 N lean (pad centre) and knock (pad corners),
# all 8 arm/kit/pad settings; arm 1.90 / 1.24 and tenon fuse 2.87 / 1.16 (r5b). Every number is an ESTIMATE.
# X geometry (S frame, seated pose): plug = stadium 13 (d' -1 .. 12) x 80 (z 214-294) from the mouth (t' -14.27) to t' 6;
# a "beak" hook behind its top (t' 6-11, z 286-299) sits behind the internal ledge (pull-out); the rear top seats on the
# pocket ceiling / guide wedges (pitch: gravity-locked); the throat floor carries the front bottom; the collar bears on the
# closed ring round the mouth; the steel-spring catch at the rear bottom blocks the rotate-back only.
# "plain push" = the same plug pushed straight in with the r5b deep catch (no hook, no rotation): full top + bottom contact,
# pull-out on the r5b tooth (2.4 x 32). Prongs (Ø8, inside, from the pocket rear) only fit the plain push: a plug that
# rotates as it seats can't take straight prongs.
# Run: ../.venv/bin/python x_strength.py   (writes x_strength.json)
import os, sys, json, math
HERE = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(HERE, 'h2h_strength.py')).read().split("OUT = {'model'")[0].replace(
    "HERE = os.path.dirname(os.path.abspath(__file__))", "HERE = %r" % HERE)
g = {}; exec(src, g)
base, run_cases, peak, prong_pts, ARM, FUSE, P = (g[k] for k in ('base', 'run_cases', 'peak', 'prong_pts', 'ARM', 'FUSE', 'P'))
D0, D1, Z0, Z1, T_END = -1.5, 15.5, 210.0, 300.0, 10.0
DC = (D0 + D1) / 2
TP, ZP = -15.27, 304.0
CATCH_A = 11.0 * 3.0
def _loc(phi, sh, t, z):    # boss path in the plug frame (gen_xi.py motion): rotate 15° while pushing the last 12 mm
    c, s_ = math.cos(math.radians(phi)), math.sin(math.radians(phi)); t = t + sh * c; z = z + sh * s_
    c2, s2 = math.cos(math.radians(-phi)), math.sin(math.radians(-phi)); return TP + (t - TP) * c2 - (z - ZP) * s2, ZP + (t - TP) * s2 + (z - ZP) * c2
_a = _loc(-15.0 * 0.03, 12.0 * 0.03, 6.0, 255.0); _b = _loc(0.0, 0.0, 6.0, 255.0)
BT = (_b[0] - _a[0], _b[1] - _a[1]); _n = math.hypot(*BT); BT = (BT[0] / _n, BT[1] / _n)                      # rotation pivot: just in front of the mouth's top (pitch axis along d')
def xmount(twist=True, prongs=False):
    ns = {}; exec(base, ns); np = ns['np']; TF = ns['T_FRONT']
    def area(fn, a0, a1, b0, b1, n0, n1):
        da, db = (a1 - a0) / n0, (b1 - b0) / n1
        return [fn(a0 + da * (i + .5), b0 + db * (j + .5), da * db) for i in range(n0) for j in range(n1)]
    PT = {}
    t0 = TF + 3.0                                                # behind the rolled lip
    PT['inner wall'] = np.array(area(lambda t, z, A: (D0, t, z, 1, 0, 0, 0.0, A), t0, T_END - 1, Z0 + 6, Z1 - 6, 8, 16), float)
    PT['outer wall'] = np.array(area(lambda t, z, A: (D1, t, z, -1, 0, 0, 0.0, A), t0, T_END - 1, Z0 + 6, Z1 - 6, 8, 16), float)
    # top: the seat (rear ceiling + guide wedges) and the throat ceiling; bottom: the throat floor (+ the rear floor if plain push)
    PT['seat + wedges (top)'] = np.array(area(lambda d, t, A: (d, t, Z1, 0, 0, -1, 0.0, A), D0 + 2, D1 - 2, -6.0, T_END, 6, 6), float)
    PT['throat ceiling'] = np.array(area(lambda d, t, A: (d, t, Z1, 0, 0, -1, 0.0, A), D0 + 2, D1 - 2, t0, -6.0, 6, 4), float)
    PT['throat floor'] = np.array(area(lambda d, t, A: (d, t, Z0, 0, 0, 1, 0.0, A), D0 + 2, D1 - 2, t0, (TF + 10.0) if twist else T_END, 6, 4 if twist else 10), float)
    PT['rear face'] = np.array(area(lambda d, z, A: (d, T_END, z, 0, -1, 0, 0.0, A), D0 + 2, D1 - 2, Z0 + 6, Z1 - 12, 6, 12), float)
    ring = []
    for d in np.linspace(-10, 18, 15):
        for z in np.linspace(205, 303, 25):
            dd = max(abs(d - (D0 + D1) / 2) - (D1 - D0) / 2, 0); zz = max(abs(z - (Z0 + Z1) / 2) - (Z1 - Z0) / 2, 0)
            if math.hypot(dd, zz) < 3.5: continue                    # the mouth + its rolled lip
            ring.append((d, TF - 0.3, z, 0, -1, 0, 0.0, 28 / 15 * 98 / 25))
    PT['collar on the closed ring'] = np.array(ring, float)
    if twist:
        # beak hook behind the ledge: front face of the beak (t' 6) bears forward on the ledge's rear face, 10 (d') x 10 (z)
        PT['hook on ledge (pull-out)'] = np.array(area(lambda d, z, A: (d, T_END - 0.3, z, 0, 1, 0, 0.0, A), DC - 5.5, DC + 5.5, Z1 - 6, Z1 + 5, 5, 6), float)
        # steel-spring catch at the rear bottom: blocks the rotate-back (rear moving forward/down about the pivot)
        r = np.array([T_END - 2 - TP, Z0 + 2 - ZP]); m = np.array([r[1], -r[0]]); m = m / np.linalg.norm(m)   # rotate-back motion
        PT['catch (rotate-back)'] = np.array([(d, T_END - 5, Z0 - 1.5, 0, -m[0], -m[1], 0.0, CATCH_A / 4) for d in np.linspace(DC - 4.5, DC + 4.5, 4)], float)   # 11 wide x 3.0 drop
        # cam bosses (Ø6, 2.5 proud) in the plug's grooves, both walls: bear across the groove at the seated end
        PT['cam bosses'] = np.array([(dB, 6.0, 255.0, *nv, 0.0, 6.0 * 2.5) for dB in (D0 + 1.25, D1 - 1.25) for nv in ((0, BT[1], -BT[0]), (0, -BT[1], BT[0]))], float)
    else:
        PT['rear floor'] = np.array(area(lambda d, t, A: (d, t, Z0, 0, 0, 1, 0.0, A), D0 + 2, D1 - 2, -6.0, T_END, 6, 6), float)
        PT['latch tooth (pull-out)'] = np.array(area(lambda d, z, A: (d, T_END - 3, z, 0, 1, 0, 0.0, A), D1 - 2.4, D1, Z1 - 34, Z1 - 2, 3, 12), float)
    if prongs:
        PT['prongs'] = np.array(prong_pts(1, 5.5, 284.0, T_END, 24.0) + prong_pts(1, 5.5, 224.0, T_END, 18.0), float)
    R = run_cases(ns, PT); names = list(PT); res = {}
    for nm, rows in R.items():
        sf_w = 25 / peak(rows, names)
        # plug section at the throat (t' = T_FRONT + 7): stadium 13 x 80 minus the Ø4 button rod; Kt 1.4 (R2 roots)
        a = (D1 - D0) / 2; Ap = (D1 - D0) * (Z1 - Z0) - (4 - math.pi) * a * a - math.pi * 4
        I_z = (D1 - D0) * (Z1 - Z0) ** 3 / 12 * 0.95; I_d = (Z1 - Z0) * (D1 - D0) ** 3 / 12 * 0.9
        cen = np.array([(D0 + D1) / 2, TF + 7.0, (Z0 + Z1) / 2])
        def plug_sf(F, M):
            s = 1.4 * (abs(F[1]) / Ap + abs(M[0]) * ((Z1 - Z0) / 2) / I_z + abs(M[2]) * ((D1 - D0) / 2) / I_d)
            tau = 1.5 * math.hypot(F[0], F[2]) / Ap + abs(M[1]) / (0.29 * (D1 - D0) ** 2 * (Z1 - Z0))
            return 1 / math.sqrt((s / 40) ** 2 + (tau / 22) ** 2)
        worst_plug = 9e9; hookF = 0.0; catchF = 0.0; bossF = 0.0
        for r_, (f, A, nms, grp), *_ in rows:
            Fv = np.zeros(3); Mv = np.zeros(3)
            for gi, n in enumerate(nms):
                if n == 'collar on the closed ring': continue
                s_ = grp == gi
                for fi, row in zip(f[s_], A[s_]):
                    if fi <= 0 or row[1] <= cen[1]: continue
                    v = fi * row[3:6]; Fv += v; Mv += np.cross(row[:3] - cen, v)
            worst_plug = min(worst_plug, plug_sf(Fv, Mv))
            hookF = max(hookF, r_.get('hook on ledge (pull-out)', r_.get('latch tooth (pull-out)', {'F': 0}))['F'])
            if 'catch (rotate-back)' in r_: catchF = max(catchF, r_['catch (rotate-back)']['F'])
            if 'cam bosses' in r_: bossF = max(bossF, r_['cam bosses']['F'])
        # hook (kit, beak 10 wide x 10 tall root, 5 long, printed with the load along the layers): shear + bending, Kt 1.5
        sf_hook = 1 / math.sqrt((1.5 * 6 * hookF * 2.5 / (10 * 10 ** 2) / 40) ** 2 + (1.5 * hookF / (10 * 10) / 22) ** 2) if twist else 25 / max(hookF / (2.4 * 32), 1e-6)
        sf_catch = 25 / max(catchF / CATCH_A, 1e-6) if twist else 9e9
        items = {'wing dent': sf_w, 'plug at the throat (kit)': worst_plug, ('hook (kit)' if twist else 'latch tooth (kit)'): sf_hook,
                 'arm (kit)': ARM[nm], 'tenon fuse (kit)': FUSE[nm]}
        if twist: items['rotate-back catch'] = sf_catch
        k = min(items, key=items.get)
        wf = max(names, key=lambda n: max(r_[n]['pmax'] for r_, *_ in rows))
        res[nm] = dict(first_gives_N=round(P * items[k]), part=k, wing_dent_N=round(P * sf_w), worst_wing_face=wf,
                       plug_N=round(P * worst_plug), hook_or_tooth_load_N=round(hookF), hook_or_tooth_N=round(P * sf_hook),
                       catch_load_N=round(catchF), catch_N=(round(P * sf_catch) if twist else None),
                       boss_load_N=round(bossF), boss_N=round(P * min(25 * 15 / max(bossF / 2, 1e-6), 22 * 28.3 / max(bossF / 2, 1e-6))),
                       ledge_shear_N=round(P * 22 * 11 * 12 / max(hookF, 1e-6)),
                       ring_share_pct=round(100 * max(r_['collar on the closed ring']['F'] for r_, *_ in rows) / P))
    return res
OUT = {'model': 'same as h2h_strength.py / x_strength.py (r5b contact model, zero gaps, 300 N); ESTIMATE'}
OUT['X final'] = xmount(twist=True); print('X final', OUT['X final'], flush=True)
XS = json.load(open(os.path.join(HERE, 'x_strength.json')))
OUT['X v1'] = XS['X twist-in']; OUT['F 80 + rods'] = XS['F 80 + rods']; OUT['F 80 + rods + prongs'] = XS['F 80 + rods + prongs']
json.dump(OUT, open(os.path.join(HERE, 'xi_strength.json'), 'w'), indent=1)
