# 3D look study: the "exhaust-tip" side mount on the parent's (outer) face of the wing, three mouth shapes.
#   On the repaired Onshape wing with its old notch filled (../options/lib5.wing_full). One fine local distance field:
#   the tip is grown ~6 mm proud of the outer face by a smooth (G2-looking) bump that runs into the front edge; the mouth
#   (A oval, B stadium, C superellipse n 3.6; ~30 t x 50 z, centred t' 7, z 231) is cut to d' 6 with a rounded floor and a
#   rolled lip (R ~2.5). Kit on: a plug fills the pocket (0.25 gap), a 3 mm trim collar seats on the lip, an orange stadium
#   push button sits in the collar's centre, and the arm (a sweep of the r5 22 x 31 section) leaves from the collar's front,
#   round the front edge, to the depth-piece tenon, kept inside the arm-rest line (x <= 165).
#   The internal lock is TBD (not modelled). Run: ../.venv/bin/python gen_exh3d.py   (writes exh_*.stl, exh.json)
import os, sys, json, math, numpy as np
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'options'))
from lib5 import *
import shapely.geometry as sg
OUT = os.path.dirname(os.path.abspath(__file__)); os.chdir(OUT)
W = wing_full()
TC, ZC, HT, HZ, PROUD, FLOOR = 7.0, 231.0, 15.0, 25.0, 6.0, 6.0
# ---- base: the r5 wing's square mount slab (outer face d' 16 -> 24.5, z ~200-272) is cut back to the wing's own section
#      (taken at z 195, below the slab), so the new tip is one smooth swell instead of a slab with a bump on it
sec = [p for p in W.slice(195.0).to_polygons() if len(p) > 2]
pr = mf.CrossSection([np.asarray(p) for p in sec], mf.FillRule.EvenOdd).extrude(84.6).translate([0, 0, 196.7])
W0 = (W - box(12.0, 40, -20, 45, 197.0, 281.0)) + (pr ^ box(11.7, 40, -20, 45, 196.7, 281.3))
W0 = max(W0.decompose(), key=lambda m: m.volume())
# ---- one distance field over the whole wing tip; everything inside the box K is rebuilt from it: no seams near the mouth
KLO, KHI = (-12.0, -22.0, 176.0), (40.0, 33.5, 393.0)   # whole tip, top to bottom: the only seam is t' 33.5, on the CAD's own corner
G = Grid(W0, (-14.0, -24.0, 174.0), (42.0, 36.0, 395.0), h=0.25, blur=1.5)
from scipy.ndimage import gaussian_filter1d   # soften the ~0.3 mm step where the section changes (z 281) along z only
wz = np.exp(-((G.Z - 281.0) / 6.0) ** 2); G.F = (G.F * (1 - wz) + gaussian_filter1d(G.F, 3.0 / 0.25, axis=2) * wz).astype(np.float32)
rt = (G.T - TC) / 22.0; rz = (G.Z - ZC) / 38.0
B = PROUD * np.exp(-(rt ** 2 + rz ** 2) ** 2) * sm((G.D - 6.0) / 8.0)      # flat-topped, smooth (C-infinity) swell
FB = G.F + B
Kf = sdf_box(G, KLO, KHI, 0.0); Kin = sdf_box(G, KLO, (KHI[0], KHI[1] + 0.3, KHI[2]), 0.0)
WB = W0 - box(KLO[0], KHI[0], KLO[1], KHI[1], KLO[2], KHI[2])
# ---- mouth shapes (2D signed distance in (t, z), positive inside)
def shape(kind):
    t = G.T - TC; z = G.Z - ZC
    if kind == 'A':   # ellipse 30 x 50 (scaled-distance approximation)
        q = np.sqrt((t / HT) ** 2 + (z / HZ) ** 2); return (1 - q) * HT
    if kind == 'B':   # stadium (pill): radius 15, straight 20
        dz = np.maximum(np.abs(z) - (HZ - HT), 0); return HT - np.sqrt(t ** 2 + dz ** 2)
    n = 3.6           # superellipse
    q = (np.abs(t / HT) ** n + np.abs(z / HZ) ** n) ** (1 / n); return (1 - q) * HT
res = {}
for kind in ('A', 'B', 'C'):
    S2 = shape(kind)
    T = smin(S2, G.D - FLOOR, 6.0)                     # pocket with a rounded floor (R ~3)
    Res = smin(FB, -T, 5.0)                            # rolled lip ~R2.5
    tip = G.mesh(np.minimum(Res, Kin))
    wing = max((WB + tip).decompose(), key=lambda m: m.volume())
    # kit: plug (fills the pocket, 0.25 gap) + collar (3 mm proud over the mouth outline + 5 mm, feathered)
    plug = np.minimum(np.minimum(-Res - 0.25, T + 0.0), FB + 0.25)
    ring = sm((S2 + 5.0) / 3.0)
    collar = np.minimum(Res + 3.0 * ring * sm((G.D - 12.0) / 4.0), -Res - 0.15)
    kitF = smax(plug, collar, 2.0)
    kit = max(G.mesh(kitF).decompose(), key=lambda m: m.volume())
    # push button: stadium 12 x 24 on the collar face at the mouth centre, crowned 0.8
    dface = 16.1 + PROUD + 3.0                         # outer face (d' 16.1 at t' 7) + swell + collar
    st = mf.CrossSection.square([0.01, 12.0], center=True).offset(6.0, mf.JoinType.Round, circular_segments=48)
    btn = tf(st.extrude(4.0), np.array([[0, 0, 1, dface - 3.2], [1, 0, 0, TC], [0, 1, 0, ZC], [0, 0, 0, 1]], float))
    btn = btn + (mf.Manifold.sphere(1.0, 64).scale([0.8, 6.0, 12.0]).translate([dface + 0.8, TC, ZC]) ^ box(dface + 0.75, dface + 2, -40, 60, 190, 270))
    kit = kit - tf(st.offset(0.3, mf.JoinType.Round).extrude(8.0), np.array([[0, 0, 1, dface - 4.5], [1, 0, 0, TC], [0, 1, 0, ZC], [0, 0, 0, 1]], float))
    # arm: collar front -> round the front edge -> tenon; pick the first control set that stays inside x <= 165
    car = load_fit('GBG-SP-AR-L-L-LO-r5'); tenon = car ^ box(-80, 80, -120, -40, 150, 215)
    p0 = QSw(13.0, TC - HT - 3.0, ZC); dT = QSw(13.0, TC - HT - 13.0, ZC) - p0
    p3 = np.array([127.8, 38.0, 183.0]); arm = None; xm = None
    for k1, (ex, ey, ez) in ((1.8, (2, -8, 24)), (1.4, (0, -8, 24)), (1.0, (0, -10, 20))):
        path = bezier(p0, p0 + dT * k1, p3 + np.array([ex, ey, ez]), p3, 80)
        a = tf(sweep_pill(path, 22.0, 31.0, 96, flare=lambda u: 1.0 + 0.18 * math.exp(-(u / 0.08) ** 2)), W2S)
        xm = float(to_tm(tf(a, S2W)).bounds[1][0])
        arm = a
        if xm <= 165.0: break
    kitall = max(((kit + arm + tenon) - wing).decompose(), key=lambda m: m.volume())
    save_W(wing, f'exh_wing_{kind}.stl'); save_W(kitall, f'exh_kit_{kind}.stl'); save_W(btn, f'exh_btn_{kind}.stl')
    kx = float(to_tm(tf(kitall, S2W)).bounds[1][0])
    res[kind] = dict(arm_x_max=round(xm, 1), kit_x_max=round(kx, 1), clash=round((kitall ^ wing).volume(), 2), button_face_d=round(dface, 1))
    print(kind, res[kind], flush=True)
# clearances (plan, S frame): mouth/pocket footprint vs S1; bottom of the mouth vs the z 186 hole
s1 = sg.Polygon(json.load(open('../options/sections.json'))['s1'][0])
pk = sg.box(FLOOR, TC - HT, 60, TC + HT)
res['clearances'] = dict(pocket_to_S1=round(pk.distance(s1), 2), mouth_bottom_to_hole_z186=round((ZC - HZ) - 186.6 - 1.65, 2), proud=PROUD,
                         mouth=f'{2 * HT:.0f} (t) x {2 * HZ:.0f} (z), centre t\' {TC}, z {ZC}', floor_d=FLOOR)
json.dump(res, open('exh.json', 'w'), indent=1)
