# Round 5b mockups, part 2 (mock fidelity, real dimensions). Writes *.stl + report.json here.
#   features: feat_slits.stl (S1, S2), feat_tear.stl (teardrop channel), feat_holes.stl (hole column + hole at z 186)
#   orig_block.stl: the chair's original mount, a faint size reference only (it is removed from the clean wing)
#   pad:      foot_r5.stl, pad_r5.stl, snap_r5.stl, nut_r5.stl, screw_r5.stl, foam_r5.stl
# The clean wing, the round-5 parts on it and the best versions of the mount options are in gen_clean.py.
# Run: ../.venv/bin/python gen_mock5b.py
import json, math, os, numpy as np, trimesh, manifold3d as mf, shapely.geometry as sg
from common import load_r5, HERE
g = load_r5()
W, env, CS, JT = g['W'], g['env'], g['CS'], g['JT']
rect, poly, prism_z, box, prism_d, tf = g['rect'], g['poly'], g['prism_z'], g['box'], g['prism_d'], g['tf']
W2S, S2W, shp = g['W2S'], g['S2W'], g['shp']
C, NZ0, NZ1, SEAM, T_FRONT, T_BACK = g['C'], g['NZ0'], g['NZ1'], g['SEAM'], g['T_FRONT'], g['T_BACK']
b = g['BTN']; fd = b['face']
os.chdir(HERE)
rep = {}
def tm_of(m): return g['to_trimesh'](m)
def export_S(m, fn): t = tm_of(tf(m, S2W)); t.export(fn); return t
def export_W(m, fn): t = tm_of(m); t.export(fn); return t
def ext_S(m):
    lo, hi = np.array(m.bounding_box()[:3]), np.array(m.bounding_box()[3:]); return dict(d=[round(lo[0], 1), round(hi[0], 1)], t=[round(lo[1], 1), round(hi[1], 1)], z=[round(lo[2], 1), round(hi[2], 1)])

# ------------------------------------------------------------------ protected features (S frame unless noted)
s240, s280 = shp(W.slice(240.0)), shp(W.slice(280.0))
ref = sg.Polygon(max(getattr(s280, 'geoms', [s280]), key=lambda p: p.area).exterior)
air = ref.difference(s240)
s1_box = sg.Polygon([(-4.5, 17.0), (10.5, 37.0), (13.0, 39.5), (8.0, 41.5), (-6.0, 23.5), (-8.0, 20.5)]).buffer(1.0)
s2_box = sg.Polygon([(-20.0, 40.5), (-12.0, 37.0), (2.0, 54.0), (6.0, 57.5), (1.0, 59.0), (-16.0, 45.0)]).buffer(1.0)
def cs_of(pg):
    pgs = getattr(pg, 'geoms', [pg]); out = None
    for p in pgs:
        if p.area < 0.5: continue
        p = sg.polygon.orient(p, 1.0); c = CS([np.asarray(p.exterior.coords)[:-1]]); out = c if out is None else out + c
    return out
S1 = cs_of(air.intersection(s1_box)); S2 = cs_of(air.intersection(s2_box))
SL_Z = (202.5, 272.0)
slits = prism_z(S1, *SL_Z) + prism_z(S2, *SL_Z)
tear = None
for p in getattr(s280, 'geoms', [s280]):
    for i in p.interiors:
        ip = sg.Polygon(i)
        if ip.area > 20: tear = CS([np.asarray(i.coords)[:-1]])
lo, hi = W.bounding_box()[2], W.bounding_box()[5]
tear_m = prism_z(tear, lo + 0.3, hi - 0.3) ^ W.hull() if tear is not None else None
export_S(slits, 'feat_slits.stl'); export_S(tear_m, 'feat_tear.stl')
# hole column (world: along y at x 75.5, Ø3.3) and the M3 hole at z 186.6 (along d through the blade)
from wingfix import repaired_wing
Ww, _ = repaired_wing()
Px = np.array([[0, 1, 0, 0], [0, 0, 1, 0], [1, 0, 0, 0]], float)          # (x,y,z) -> (y,z,x): slice at x = 75.5
sec = shp(Ww.transform(Px).slice(75.5)); parts = sorted(getattr(sec, 'geoms', [sec]), key=lambda p: p.bounds[1])
zc_holes = [round((parts[i].bounds[3] + parts[i + 1].bounds[1]) / 2, 1) for i in range(len(parts) - 1)]
ymin, ymax = sec.bounds[0], sec.bounds[2]
holes = None
for zh in zc_holes:
    cyl = mf.Manifold.cylinder(ymax - ymin + 4, 1.65, 1.65, 32)
    cyl = tf(cyl, np.array([[1, 0, 0, 75.5], [0, 0, 1, ymin - 2], [0, -1, 0, zh], [0, 0, 0, 1]], float))   # z -> +y... axis along y
    holes = cyl if holes is None else holes + cyl
AX = np.array([0.79577626, 0.60559074, 0])
p186 = np.array([141.3, 88.5, 186.6]); zax = AX; xax = np.array([0, 0, 1.0]); yax = np.cross(zax, xax)
M186 = np.eye(4); M186[:3, 0], M186[:3, 1], M186[:3, 2], M186[:3, 3] = xax, yax, zax, p186 - 22 * zax
holes = holes + tf(mf.Manifold.cylinder(44, 1.65, 1.65, 32), M186)
export_W(holes, 'feat_holes.stl')
rep['hole_column_z'] = zc_holes
rep['S1_area_mm2'] = round(S1.area(), 1); rep['S2_area_mm2'] = round(S2.area(), 1)

# ------------------------------------------------------------------ the chair's original mount (size reference only; removed from the clean wing)
orig_block = W ^ box(16.8, 40, -20, 16.5, 195, 285)
export_S(orig_block, 'orig_block.stl')
rep['orig_block'] = dict(extent_S=ext_S(orig_block), volume_cm3=round(orig_block.volume() / 1000, 1))
arm_w, info = g['build_arm'](44.0)
# (the saddle / carrier / drop-in meshes moved to gen_clean.py, built on the clean wing)

# ------------------------------------------------------------------ 4. pad screw from the child side (world frame, long arm, LOW, upper pad)
XS, PS_Y, ZP = 104.0, 32.5, info['zp']; ZR = ZP + 11.0      # pad centre / rail height: upper position
FOOT_Y, FOOT_HZ = (19.0, 46.0), 21.5                          # outer groove lip thickened (strength fix 7: FOOT_HZ 20 -> 21.5)
DV, DV_CL, DV_FRONT = (9.0, 12.0, 2.5), 0.15, 22.0
YAX, xe = 36.0, info['xe']
def rr_x(cy, cz, w, h, r, x0, x1):                            # rounded rectangle (y w, z h) extruded along x
    cs = CS.square([w - 2 * r, h - 2 * r], center=True).offset(r, JT, circular_segments=40).translate([cy, cz])
    return tf(cs.extrude(x1 - x0), np.array([[0, 0, 1, x0], [1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 0, 1]], float))
fw = FOOT_Y[1] - FOOT_Y[0]; fyc = (FOOT_Y[0] + FOOT_Y[1]) / 2
footb = mf.Manifold.batch_hull([rr_x(fyc, ZP, fw, 2 * FOOT_HZ, 13.4, XS, XS + 1.5), rr_x(YAX, ZP, 18, 28, 8.9, XS + 18.0, XS + 18.1)])
footb = footb + rr_x(YAX, ZP, 18, 28, 8.9, XS + 18.0, xe - 0.3)
ten = rr_x(YAX, ZP, 13.4, 20.4, 2.7, xe - 13.1, xe)            # the arm's tenon sleeve
footb = footb - ten
def dovetail_x(xb, zr, y0, y1, wr, wt, h):                     # dovetail along y: root width wr at x = xb, top width wt at x = xb + h
    pts = [[xb - 0.6, zr - wr / 2], [xb, zr - wr / 2], [xb + h, zr - wt / 2], [xb + h, zr + wt / 2], [xb, zr + wr / 2], [xb - 0.6, zr + wr / 2]]
    cs = CS([np.asarray(pts, float)])                          # (x, z) section, extruded along y
    return tf(cs.extrude(y1 - y0), np.array([[1, 0, 0, 0], [0, 0, 1, y0], [0, 1, 0, 0], [0, 0, 0, 1]], float))
xb = XS
sl = (DV[1] - DV[0]) / 2 / DV[2]
for zr in (ZP + 11.0, ZP - 11.0):
    grv = dovetail_x(xb - 1.0, zr, DV_FRONT, FOOT_Y[1] + 5, 2 * (DV[0] / 2 + DV_CL - sl * 1.0), 2 * (DV[0] / 2 + DV_CL + sl * (DV[2] + 0.1)), DV[2] + 1.1)
    nutp = tf(mf.Manifold.cylinder(5.6, 8.3 / math.sqrt(3) + 0.15, 8.3 / math.sqrt(3) + 0.15, 6), np.array([[0, 0, 1, xb + DV[2]], [0, 1, 0, PS_Y], [-1, 0, 0, zr], [0, 0, 0, 1]], float))
    mouth = mf.Manifold.cube([DV[2] + 0.2, 8.7, 8.7]).translate([xb + 0.1, PS_Y - 4.35, zr - 4.35])   # nut goes in through the groove mouth
    hole = tf(mf.Manifold.cylinder(9.0, 2.75, 2.75, 32), np.array([[0, 0, 1, xb], [0, 1, 0, PS_Y], [-1, 0, 0, zr], [0, 0, 0, 1]], float))
    snapn = mf.Manifold.cube([2.0, 1.6, 6.0]).translate([xb + DV[2] + 0.1, 44.2, zr - 3.0])           # snap notch in the groove floor
    footb = footb - grv - nutp - mouth - hole - snapn
export_W(footb, 'foot_r5.stl')
# pad shell: rounded plate y -27..55, z ZR +- 40, x shellXi..+4.5, wrapped like the FS (WRAP 0.7)
A_REF, B_REF, YC_REF, WRAP, SHELL_T = 85.0, 88.0, 34.0, 0.7, 4.5
def wrapx(V):
    V = np.array(V); y = V[:, 1]; ex = A_REF * np.maximum(1 - np.abs((y - YC_REF) / B_REF) ** 3, 0) ** (1 / 3)
    V[:, 0] -= WRAP * (A_REF - ex); return V
padcs = CS.square([82 - 2 * 22, 80 - 2 * 22], center=True).offset(22, JT, circular_segments=64).translate([(-27 + 55) / 2, ZR])
def plate(x0, x1, cs):
    m = tf(cs.extrude(x1 - x0), np.array([[0, 0, 1, x0], [1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 0, 1]], float))
    return m.refine_to_length(4.0).warp_batch(wrapx)
pad = plate(XS - SHELL_T, XS, padcs)
rail = dovetail_x(xb, ZR, DV_FRONT + 0.3, FOOT_Y[1], DV[0], DV[1], DV[2])
pad = pad + rail
cb = tf(mf.Manifold.cylinder(3.0, 5.25, 5.25, 40), np.array([[0, 0, 1, xb - SHELL_T - 0.01], [0, 1, 0, PS_Y], [-1, 0, 0, ZR], [0, 0, 0, 1]], float))
ph = tf(mf.Manifold.cylinder(SHELL_T + DV[2] + 2, 2.75, 2.75, 32), np.array([[0, 0, 1, xb - SHELL_T - 1], [0, 1, 0, PS_Y], [-1, 0, 0, ZR], [0, 0, 0, 1]], float))
pad = pad - cb - ph
snap = mf.Manifold.cylinder(4.0, 0.8, 0.8, 24).scale([1.9, 1, 1]).translate([xb + DV[2] - 0.2, 45.0, ZR - 2.0])     # 1.5 snap bump on the rail top
snap = snap ^ mf.Manifold.cube([1.7, 3, 4.2]).translate([xb + DV[2] - 0.2, 43.5, ZR - 2.1])
export_W(pad, 'pad_r5.stl'); export_W(snap, 'snap_r5.stl')
nut = tf(mf.Manifold.cylinder(5.0, 8.3 / math.sqrt(3), 8.3 / math.sqrt(3), 6), np.array([[0, 0, 1, xb + DV[2] + 0.2], [0, 1, 0, PS_Y], [-1, 0, 0, ZR], [0, 0, 0, 1]], float))
nut = nut - tf(mf.Manifold.cylinder(6, 2.5, 2.5, 24), np.array([[0, 0, 1, xb], [0, 1, 0, PS_Y], [-1, 0, 0, ZR], [0, 0, 0, 1]], float))
export_W(nut, 'nut_r5.stl')
# M5 x 10 button head (ISO 7380: dk 9.5, k 2.75), head 0.25 below the shell's child face, hex socket 3
hx = xb - SHELL_T + 0.25
head = mf.Manifold.sphere(1.0, 48).scale([2.75, 4.75, 4.75]) ^ mf.Manifold.cube([3, 12, 12]).translate([0, -6, -6])
head = head.scale([-1, 1, 1]).translate([hx + 2.75, 0, 0])
head = head - tf(mf.Manifold.cylinder(2.0, 1.73, 1.73, 6), np.array([[0, 0, 1, hx - 0.5], [0, 1, 0, 0], [-1, 0, 0, 0], [0, 0, 0, 1]], float))
shank = tf(mf.Manifold.cylinder(10.0, 2.5, 2.5, 32), np.array([[0, 0, 1, hx + 2.75], [0, 1, 0, 0], [-1, 0, 0, 0], [0, 0, 0, 1]], float))
screw = (head + shank).translate([0, PS_Y, ZR])
export_W(screw, 'screw_r5.stl')
FOAM = 10.0
foamcs = padcs.offset(1.0, JT, circular_segments=64)
foam = plate(XS - SHELL_T - FOAM, XS - SHELL_T - 0.01, foamcs)
export_W(foam, 'foam_r5.stl')
rep['pad'] = dict(zp=ZP, rail_z=ZR, screw_head_x=round(hx, 2), screw_tip_x=round(hx + 12.75, 2), nut_x=[xb + DV[2] + 0.2, xb + DV[2] + 5.2],
                  foam=FOAM, clash_screw_foot=round((screw ^ footb).volume(), 2), clash_nut_foot=round((nut ^ footb).volume(), 2),
                  clash_pad_foot=round((pad ^ footb).volume(), 2), clash_snap_foot=round((snap ^ footb).volume(), 2))
json.dump(rep, open('report.json', 'w'), indent=1)
print(json.dumps(rep, indent=1))
