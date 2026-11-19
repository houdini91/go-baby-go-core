# Options 2, 4, 5 and 6 on the REAL r5cad wing (repaired), with rounded/flared openings from fine local distance fields.
# Usage: ../.venv/bin/python gen_opts2.py [2] [4] [5] [6]     (no args = all). Writes <name>.stl (world) + opts2_<n>.json
import sys, json, math, numpy as np, trimesh, manifold3d as mf
from lib5 import *
sel = set(sys.argv[1:]) or {'2', '4', '5', '6'}
W = wing()                                           # S frame
car = load_fit('GBG-SP-AR-L-L-LO-r5')
C = 227.0
rep = {}
def sec_polys(m, z): return [np.round(np.asarray(p), 2).tolist() for p in m.slice(z).to_polygons() if len(p) > 2]
def blade_at(m, z, t):
    import shapely.geometry as sg
    ps = [sg.Polygon(p) for p in m.slice(z).to_polygons() if len(p) > 2]
    xs = []
    for p in ps:
        L = p.exterior.intersection(sg.LineString([(-40, t), (60, t)]))
        for g_ in getattr(L, 'geoms', [L]):
            if not g_.is_empty: xs.append(g_.x)
    xs = sorted(xs); return xs[0], xs[-1]

# ------------------------------------------------------------------ option 4: three-prong plug
if '4' in sel:
    P = json.load(open('prong.json'))
    NZ = P['nose_z']; rods = [(P['lower']['axis_d'], P['lower']['z'], P['lower']['bore_t'][1]), (P['upper']['axis_d'], P['upper']['z'], P['upper']['bore_t'][1])]
    lo, hi = (-14.0, -22.0, NZ[0] - 8), (30.0, 24.0, NZ[1] + 8)
    def tool(G):
        T = sdf_box(G, (-60, -60, NZ[0]), (60, 2.0, NZ[1]))
        for d, z, te in rods: T = np.maximum(T, sdf_cyl_t(G, d, z, 4.1, -5.0, te))
        return T
    G = Grid(W, lo, hi, h=0.2, blur=1.5)
    T = tool(G); k = 5.0
    Res = smin(G.F, -T, k)                                   # the bare wing's field: rounded notch rim, flared bore mouths
    Rm = G.mesh(np.minimum(np.minimum(-Res, T + 1.6 * k), G.F + 0.6))
    wing4 = W - Rm
    # nose + tongue + rods: the missing wing (0.25 seam), except the rod bores; the as-built carrier supplies arm, latch, tongue
    noseF = np.minimum(np.minimum(G.F, -Res - 0.25), 2.6 - G.T)
    for d, z, te in rods: noseF = np.minimum(noseF, np.sqrt((G.D - d) ** 2 + (G.Z - z) ** 2) - 4.1)
    nose = G.mesh(noseF)
    rodm = []
    for d, z, te in rods:
        r = mf.Manifold.cylinder(te - 0.6 - (T_FRONT + 3.0), 4.0, 4.0, 48).rotate([-90, 0, 0]).translate([d, T_FRONT + 3.0, z])
        rodm.append(r + mf.Manifold.sphere(4.0, 48).scale([1, 0.3, 1]).translate([d, te - 0.6, z]))
    tb = (car ^ box(-30, 40, 3.0, 40, 200, 260)).bounding_box(); ZT = (tb[2] + tb[5]) / 2
    for dz in (-13.5, 13.5):
        for dd in (tb[0] + 2.5, tb[3] - 2.5):
            rodm.append(mf.Manifold.cylinder(tb[4] - 2.0 - (T_FRONT - 26.0), 1.0, 1.0, 16).rotate([-90, 0, 0]).translate([dd, T_FRONT - 26.0, ZT + dz]))
    rods_m = mf.Manifold.batch_boolean(rodm, mf.OpType.Add)
    car4 = ((car + nose) - rods_m)
    car4 = max(car4.decompose(), key=lambda m: m.volume())
    wing4 = max((wing4 - mf.Manifold.batch_boolean(rodm[:2], mf.OpType.Add)).decompose(), key=lambda m: m.volume())   # only the Ø8 rods reach into the wing
    wing4 = max((wing4 - box(-40, 60, -40, 0.5, NZ[0] + 4.0, NZ[1] - 4.0)).decompose(), key=lambda m: m.volume())   # clear slivers left inside the notch
    save_W(wing4, 'prong_wing.stl'); save_W(car4, 'prong_carrier.stl'); save_W(rods_m, 'prong_rods.stl')
    rep['4'] = dict(k_round=k, clash=round((car4 ^ wing4).volume(), 2), rods_clash=round((rods_m ^ wing4).volume(), 2))
    json.dump(dict(tongue=dict(z=ZT, wing=sec_polys(wing4, ZT), kit=sec_polys(car4, ZT)), rod=dict(z=rods[0][1], rods=sec_polys(rods_m, rods[0][1]))), open('prong_sections2.json', 'w'))
    print('4 done', rep['4'], flush=True)

# ------------------------------------------------------------------ option 2: thick saddle (12 mm jaws), bare wing with a rounded catch notch
if '2' in sel:
    W = wing_full()
    lo, hi = (-26.0, -30.0, 176.0), (42.0, 44.0, 292.0)
    G = Grid(W, lo, hi, h=0.25, blur=1.5)
    import shapely.geometry as sg
    W0 = wing(); s240 = sg.Polygon()
    JAW = 12.0
    outer = G.D > 10.0
    front = sm((-9.0 - G.T) / 4.0)
    t_end = np.where(outer, 34.0, 14.0)
    z1 = np.where(outer, 281.0, 250.0); z0 = 181.0
    taper = sm((t_end - G.T) / 12.0) * sm((G.Z - z0) / 10.0) * sm((z1 - G.Z) / 10.0)
    # strap slit S1 and the z 186 hole (S frame), kept >= 3 mm clear
    S1 = json.load(open('sections.json'))['s1'][0]
    from shapely import distance as sdist, points as spoints
    s1p = sg.Polygon(S1); DD, TT = np.meshgrid(G.ax[0], G.ax[1], indexing='ij')
    DS1 = np.asarray(sdist(s1p, spoints(np.c_[DD.ravel(), TT.ravel()])), np.float32).reshape(DD.shape)[:, :, None]
    DS1 = np.sqrt(DS1 ** 2 + (np.maximum(202.5 - G.Z, 0) + np.maximum(G.Z - 272.0, 0)) ** 2)
    DH = np.sqrt((G.T - 2.11) ** 2 + (G.Z - 186.6) ** 2) - 1.65
    clear = sm((DS1 - 3.0) / 2.0) * sm((DH - 3.0) / 2.0)
    TH = (6.0 * front + JAW * (1 - front) * (0.25 + 0.75 * taper)) * clear * sm((t_end + 2 - G.T) / 2.0) * sm((G.Z - z0 + 2) / 2.0) * sm((z1 + 2 - G.Z) / 2.0)
    dcat = blade_at(W, 227.0, 0.0)[1]
    catchT = sdf_box(G, (dcat - 2.5, -4.0, 219.0), (60, 2.0, 235.0), 1.5)
    Res = smin(G.F, -catchT, 4.0)
    Rm = G.mesh(np.minimum(np.minimum(-Res, catchT + 6.0), G.F + 0.6))
    wing2 = W - Rm
    shell = np.minimum(Res + TH, -Res - 0.1)
    shell = np.maximum(shell, np.minimum(-Res - 0.1, catchT - 0.3))
    kit = max(G.mesh(shell, simp=0.01).decompose(), key=lambda m: m.volume())
    rep['2'] = dict(jaw=JAW, catch_outer_d=round(dcat, 2))
    save_W(wing2, 'saddle2_wing.stl')
    # arm: the as-built carrier's arm, cut where it leaves the nose, joined to the saddle front
    armpart = car ^ box(-60, 60, -80, T_FRONT - 1.0, 120, 260)
    kitall = max(((kit + armpart) - wing2).decompose(), key=lambda m: m.volume())
    save_W(kitall, 'saddle2_kit.stl')
    rep['2']['clash'] = round((kitall ^ wing2).volume(), 2)
    json.dump(dict(z=230.0, wing=sec_polys(wing2, 230.0), kit=sec_polys(kitall, 230.0)), open('saddle2_sections.json', 'w'))
    print('2 done', rep['2'], flush=True)

# ------------------------------------------------------------------ option 5: second slot at z 271, swell carried up to z 291
if '5' in sel:
    import shapely.geometry as sg
    lo, hi = (-14.0, -22.0, 240.0), (32.0, 42.0, 304.0)
    G = Grid(W, lo, hi, h=0.2, blur=1.5)
    # envelope of the cheek/swell: the wing's own section at z 262 (inside the old envelope, swell included), as a prism
    env = [p for p in W.slice(262.0).to_polygons() if len(p) > 2]
    cs = mf.CrossSection([np.asarray(p) for p in env], mf.FillRule.EvenOdd)
    prism = cs.extrude(60.0).translate([0, 0, 240.0]) ^ box(-40, 60, -20, 40, 240, 300)
    Gp = Grid(prism, lo, hi, h=0.2, blur=1.0)
    cap = np.minimum(np.minimum(Gp.F, 296.0 - G.Z), G.Z - 249.0)   # carried up to z ~296 (5 mm above the upper slot), only above the lower notch
    add = smax(G.F, cap, 8.0)                                  # blended into the blade above (R ~ 8)
    A = np.minimum(np.minimum(add, 0.6 - G.F), cap + 10.0)
    Am = G.mesh(A)
    W5 = W + Am
    # the notch/slot/groove void of the as-built mount (z 206.5-247.5), copied 44 up
    lo2, hi2 = (-14.0, -22.0, 200.0), (32.0, 34.0, 254.0)
    env0 = [p for p in W.slice(250.0).to_polygons() if len(p) > 2]
    pr0 = mf.CrossSection([np.asarray(p) for p in env0], mf.FillRule.EvenOdd).extrude(41.0).translate([0, 0, 206.5]) ^ box(-40, 60, -30, 30, 206.5, 247.5)
    void = (pr0 - W) ^ box(-40, 60, -30, 30, 206.6, 247.4)
    W5 = W5 - void.translate([0, 0, 44.0])
    car_up = car.translate([0, 0, 44.0])                       # the same carrier in the upper slot (a 0-drop arm would replace its arm)
    btn0 = load_fit('GBG-SP-BT-L-r5')
    W5 = W5 - car - car_up - btn0 - btn0.translate([0, 0, 44.0])
    W5 = max(W5.decompose(), key=lambda m: m.volume())
    save_W(W5, 'slots2_wing.stl')
    save_W(car_up, 'slots2_car_up.stl')
    rep['5'] = dict(swell_to_z=296.0, upper_slot_centre=271.0, clash_upper=round((car_up ^ W5).volume(), 2), clash_lower=round((car ^ W5).volume(), 2))
    print('5 done', rep['5'], flush=True)

# ------------------------------------------------------------------ option 6: socket in the child-side face
if '6' in sel:
    W = wing_full()
    ZS = (211.0, 243.0); TS6 = (-11.5, 14.0); DEPTH = 14.0
    d_in = blade_at(W, 227.0, 1.0)[0]
    lo, hi = (-26.0, -24.0, 196.0), (20.0, 30.0, 258.0)
    G = Grid(W, lo, hi, h=0.2, blur=1.5)
    T = sdf_box(G, (-60, TS6[0], ZS[0]), (d_in + DEPTH, TS6[1], ZS[1]), 6.0)
    k = 5.0
    Res = smin(G.F, -T, k)
    Rm = G.mesh(np.minimum(np.minimum(-Res, T + 1.6 * k), G.F + 0.6))
    W6 = W - Rm
    # the plug fills the rounded pocket (0.25 clearance); a flange sits 4 mm proud on the inner face round it, feathered out
    plug = np.minimum(np.minimum(-Res - 0.25, G.F + 3.0), T + 3.0)
    inner = G.D < 5.0
    m_out = sdf_box(G, (-60, TS6[0] - 6, ZS[0] - 6), (5, TS6[1] + 5, ZS[1] + 6), 9.0)
    THf = 4.0 * sm(m_out / 5.0) * inner
    flange = np.minimum(Res + THf, -Res - 0.1)
    kitF = smax(plug, flange, 2.0)
    kit = max(G.mesh(kitF, simp=0.01).decompose(), key=lambda m: m.volume())
    # arm: a real sweep of the 22 x 31 pill from the flange to the depth-piece tenon (long arm, LOW: x_e 127.8, y 38, z 183)
    p0 = QSw(d_in - 2.0, 1.25, 227.0); dD = QSw(d_in - 12.0, 1.25, 227.0) - p0
    p3 = np.array([127.8, 38.0, 183.0])
    path = bezier(p0, p0 + dD * 2.2, p3 + np.array([0.0, 30.0, 18.0]), p3, 70)
    arm = tf(sweep_pill(path, 22.0, 31.0, 96, flare=lambda u: 1.0 + 0.25 * math.exp(-(u / 0.08) ** 2)), W2S)
    kit6 = max(((kit + arm) - W6).decompose(), key=lambda m: m.volume())
    save_W(W6, 'socket_wing.stl'); save_W(kit6, 'socket_kit.stl')
    rep['6'] = dict(pocket_t=TS6, pocket_z=ZS, depth=DEPTH, inner_face_d=round(d_in, 2), floor_d=round(d_in + DEPTH, 2), flange_proud=4.0,
                    clash=round((kit6 ^ W6).volume(), 2), proud_world_x_min=round(float(to_tm(tf(kit6, S2W)).bounds[0][0]), 1))
    json.dump(dict(z=227.0, wing=sec_polys(W6, 227.0), kit=sec_polys(kit6, 227.0)), open('socket_sections.json', 'w'))
    print('6 done', rep['6'], flush=True)
old = json.load(open('opts2.json')) if os.path.exists('opts2.json') else {}
old.update(rep); json.dump(old, open('opts2.json', 'w'), indent=1)
