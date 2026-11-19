# Round 5 wing-side parts of the GoBabyGo side-pad kit (LEFT side, LOW = pad below), as real solids (manifold3d).
#   wing_r5.stl          original wing - 40 mm nose notch - tongue slot - catch groove (+ the dead lower-bolt pocket and slit filled
#                        inside the notch band only, so the slot gets a real U of walls)
#   nose_arm_r5_L.stl    carrier, long arm (pad drop 44): the nose (= the wing's own front edge z 207.3-246.7), tongue, arm, tenon
#   nose_arm_r5_S.stl    carrier, short arm (pad drop 22): same nose/tongue, the arm from the same FS path with dz = -22
#   button_r5.stl        latch A slider: 12 x 24 stadium cap (flush, cylindrical 0.8 thumb scoop, 0.4 chamfer) + bar + tooth
#   sleeve_r5.stl        orange tell sleeve (0.6 wall, 0.6 below the face): shows only when the button is sunk = not latched
#   cap_r5.stl           blank cap for the notch with the kit off (same nose solid) + a stub tongue with a snap finger into the groove
# Frames: chair world mm. F = wing-tip frame of gbg_sidepad_v1.fs (d across the tip, + outboard; t into the wing, - = front).
# S = F with the wing's lean taken out: d' = d - LD (z - 227), t' = t - LT (z - 227). In S the wing tip is a vertical extrusion
# (front face t' = -14.27 at every z, measured), so every part here is built as straight prisms in S and sheared back.
# Run: ../.venv/bin/python gen_r5.py   (writes *.stl + report.json here)
import json, math, numpy as np, trimesh, manifold3d as mf
from frame import P0, AXH, TH, SH, M_F2W, M_W2F
from wingfix import repaired_wing

LD, LT, ZC = 0.0565, 0.069, 227.0           # lean of the blade: +d 3.2 deg and +t 3.95 deg per mm up (measured on the original)
C = 227.0                                   # tongue centre
NZ0, NZ1, SEAM = 207.0, 247.0, 0.3          # notch band in the wing; the nose is 0.3 smaller each side (hairline seams)
T_FRONT = -14.27                            # the wing's front face (S frame)
T_BACK = 2.0                                # notch back face (S): nose t' -14.27 .. 1.7 (16 deep: the 12-wide button + 2 mm each side)
SL_D = (3.75, 17.45)                        # tongue 13.7 wide (slot 14.1): outer face on the old pocket's outer wall d' 17.6
TG_Z = (214.3, 239.7)                       # tongue height (slot 214.1-239.9): the cheek is >= 5.5 thick from z 214 up
CLR = 0.2
WALL_OUT = 5.5                              # min outer wall behind the cheek
GROOVE_T, GROOVE_Z, GROOVE_DEPTH = (5.0, 9.0), (217.0, 237.0), 3.5     # 3 mm shoulder to the notch face; the slot's outer face is straight to t' 9.3
BTN = dict(tc=-6.3, w=12.0, h=24.0, face=24.6, thick=4.6, travel=5.0)

F2S = np.array([[1, 0, -LD, LD * ZC], [0, 1, -LT, LT * ZC], [0, 0, 1, 0], [0, 0, 0, 1]], float)
W2S = F2S @ M_W2F
S2W = np.linalg.inv(W2S)
def tf(m, M): return m.transform(np.asarray(M[:3, :4], float))
def to_S(p): p = np.c_[np.atleast_2d(p), np.ones(len(np.atleast_2d(p)))]; return (p @ W2S.T)[:, :3]

# ------------------------------------------------------------------ 2D/3D helpers (S frame)
CS = mf.CrossSection
def rect(x0, x1, y0, y1): return CS([np.array([[x0, y0], [x1, y0], [x1, y1], [x0, y1]], float)])
def poly(pts): return CS([np.asarray(pts, float)])
def prism_z(cs, z0, z1): return cs.extrude(z1 - z0).translate([0, 0, z0])
def box(d0, d1, t0, t1, z0, z1): return prism_z(rect(d0, d1, t0, t1), z0, z1)
def stadium(w, h, n=48):                     # w along x, h along y, semicircular ends top/bottom
    r = w / 2; c = h / 2 - r; a = np.linspace(0, np.pi, n)
    top = np.c_[r * np.cos(a), c + r * np.sin(a)]; bot = np.c_[-r * np.cos(a), -c - r * np.sin(a)]
    return poly(np.r_[top, bot])
# prism along d' of a (t', z) section: local x -> t', y -> z, extrusion -> d'
PERM = np.array([[0, 0, 1, 0], [1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 0, 1]], float)
def prism_d(cs_tz, d0, d1, scale_top=(1, 1)):
    return tf(cs_tz.extrude(d1 - d0, scale_top=scale_top).translate([0, 0, d0]), PERM)
def cyl_d(t, z, r, d0, d1, n=48): return prism_d(CS.circle(r, n).translate([t, z]), d0, d1)
def to_trimesh(m):
    g = m.to_mesh(); return trimesh.Trimesh(np.asarray(g.vert_properties)[:, :3], np.asarray(g.tri_verts), process=False)

# ------------------------------------------------------------------ the original wing in S
wing0_world, n_dropped = repaired_wing()
W = tf(wing0_world, W2S)
sec240 = W.slice(240.0)          # clean blade section (body + old pocket + cheek), no bolt features
sec215 = W.slice(215.2)          # cheek merged with the body (below the old pocket and slit)
W1 = rect(7.3, 17.9, T_FRONT + 0.02, 14.0)      # the old tongue pocket W1 (d' 7.6-17.6): bridged inside the band only
SLIT = rect(8.0, 26.0, 11.0, 22.0)              # where the old slit behind the cheek was: closed with the z 215 section (cheek merged)
env = (sec240 + W1 + (sec215 ^ SLIT)) ^ rect(-14, 34, -20, 40)   # filled blade section near the tip (S1 stays open)

# features we must not touch (holes of the section), for clearance checks
def holes_of(cs):
    out = []
    for p in cs.to_polygons():
        p = np.asarray(p); a = 0.5 * np.sum(p[:, 0] * np.roll(p[:, 1], -1) - np.roll(p[:, 0], -1) * p[:, 1])
        out.append((a, p))
    return out

# ------------------------------------------------------------------ wing_r5
fill_w = prism_z(env ^ rect(-14, 34, T_BACK - 0.6, 26.0), 215.0, NZ1)
notch = box(-40, 50, -40, T_BACK, NZ0, NZ1)
tb = lambda d: 9.2 + 1.31 * (d + 4.0)           # S1 front edge 19.1 + 1.31 (d + 4) less a 6 mm normal web (9.9 along t)
s1_half = poly([[-20, -60], [40, -60], [40, tb(40)], [-20, tb(-20)]])
JT = mf.JoinType.Round
tongue2d = rect(SL_D[0], SL_D[1], T_BACK - 1.0, 45) ^ s1_half.translate([0, -0.5]) ^ env.offset(-(WALL_OUT + CLR), JT)
tongue2d = tongue2d.offset(-1.0, JT).offset(1.0, JT)
slot2d = tongue2d.offset(CLR, JT) ^ rect(-20, 40, T_BACK - 1.0, 60)
slot = prism_z(slot2d, TG_Z[0] - CLR, TG_Z[1] + CLR)
gk = math.tan(math.radians(6.0))
groove2d = poly([[SL_D[1] - 0.6, GROOVE_T[0] - 0.85 * gk], [17.6 + GROOVE_DEPTH, GROOVE_T[0] + GROOVE_DEPTH * gk],
                 [17.6 + GROOVE_DEPTH, GROOVE_T[1]], [SL_D[1] - 0.6, GROOVE_T[1]]])
groove = prism_z(groove2d, *GROOVE_Z)
wing_r5 = ((W + fill_w) - notch) - slot - groove
# OPTION (not the default, owner to decide): also close the rest of the old receiver pocket W1 above the nose (z 247-268), its slit and
# the dead upper M5 clamp bolt (z 260.7) -> the wing's front edge reads unbroken above the nose
fill_opt = prism_z(env ^ rect(-14, 34, T_FRONT + 0.02, 26.0), NZ1 - 0.01, 270.0)
wing_r5_opt = wing_r5 + fill_opt

# ------------------------------------------------------------------ the nose (shared by the carriers and the blank cap)
fill_n = prism_z((sec240 + W1) ^ rect(-14, 34, T_FRONT - 0.4, T_BACK + 0.6), 215.5, NZ1 + 0.5)
nose_box = box(-40, 50, -40, T_BACK - SEAM, NZ0 + SEAM, NZ1 - SEAM)
nose = (W + fill_n) ^ nose_box
tongue = prism_z(tongue2d ^ rect(-20, 40, T_BACK - 1.0, 60), *TG_Z)

# ------------------------------------------------------------------ button (latch A: the button is the catch), latched position
b = BTN
cap2d = stadium(b['w'], b['h']).translate([b['tc'], C])
k = (b['w'] - 0.8) / b['w'], (b['h'] - 0.8) / b['h']                  # 0.4 edge chamfer
cap = prism_d(stadium(b['w'], b['h']), b['face'] - b['thick'], b['face'] - 0.4) + prism_d(stadium(b['w'], b['h']), b['face'] - 0.4, b['face'], scale_top=k)
cap = cap.translate([0, b['tc'], C])
Rs = (12.0 ** 2 + 0.8 ** 2) / 1.6                # cylindrical thumb scoop, axis along t', 0.8 deep at the centre (spec fig2)
scoop = mf.Manifold.cylinder(40, Rs, Rs, 720).translate([0, 0, -20])
scoop = tf(scoop, np.array([[1, 0, 0, 0], [0, 0, 1, 0], [0, -1, 0, 0], [0, 0, 0, 1]], float)).translate([b['face'] - 0.8 + Rs, b['tc'], C])
cap = cap - scoop
bar = box(13.9, 17.35, -4.0, 9.0, 221.0, 233.0) + box(13.9, b['face'] - b['thick'] + 0.05, -4.0, -0.9, 221.0, 233.0)
tooth2d = poly([[17.2, GROOVE_T[0] + 0.02 - 0.4 * gk], [20.9, GROOVE_T[0] + 0.02 + 3.3 * gk], [20.9, 5.9], [17.35, 9.0]])   # 6 deg locking face, ~45 deg lead-in behind
tooth = prism_z(tooth2d, 217.2, 236.8)
button = cap + bar + tooth
# carrier cuts: lip opening, tell-sleeve counterbore, cap pocket to the hard stop, channel for bar + tooth, spring well
fd = b['face']
LIP = 1.2                                    # sleeve top below the face (spec said 0.6; at 0.6 the orange peeks through the 0.3 gap > 26 deg off-axis)
pocket = (prism_d(stadium(b['w'] + 0.6, b['h'] + 0.6), fd - LIP - 0.1, fd + 1.0)
          + prism_d(stadium(b['w'] + 1.8, b['h'] + 1.8), fd - LIP - 5.0, fd - LIP)
          + prism_d(stadium(b['w'] + 0.6, b['h'] + 0.6), fd - b['thick'] - b['travel'], fd - LIP - 4.9)).translate([0, b['tc'], C])
channel = box(13.9 - b['travel'] - 0.2, fd - b['thick'] + 0.1, -4.3, 12.6, 220.7, 233.3) + box(17.2 - b['travel'] - 0.2, 21.5, 4.8, 9.2, 216.8, 237.2)
spring_well = cyl_d(b['tc'] - 2.2, C, 3.25, 8.0, fd - b['thick'] + 0.1)
sleeve = (prism_d(stadium(b['w'] + 1.8, b['h'] + 1.8), fd - LIP - 5.0, fd - LIP) - prism_d(stadium(b['w'] + 0.6, b['h'] + 0.6), fd - LIP - 6, fd)).translate([0, b['tc'], C])

# ------------------------------------------------------------------ the arm (port of armPath/armAt in gbg_sidepad_v1.fs, world)
YEND, TAPER_T, DC = 35.0, -14.0, 9.4
def Qw(d, t, z): return np.array([P0[0] + (d + 2) * AXH[0] + t * TH[0] + SH[0], P0[1] + (d + 2) * AXH[1] + t * TH[1] + SH[1], z])
def arm_path(c, zp):
    st = Qw(DC, TAPER_T, c); h0 = -np.array(TH); th0 = math.atan2(h0[1], h0[0])
    R = (YEND - st[1]) / (math.sin(th0 - math.pi / 2) - 1); Cc = st[:2] + R * np.array([math.cos(th0 - math.pi / 2), math.sin(th0 - math.pi / 2)])
    L = R * (th0 + math.pi); dz = zp - c; thv = 2 * math.atan(abs(dz) / L); Rv = L / (2 * math.sin(thv))
    return dict(st=st, th0=th0, R=R, C=Cc, L=L, dz=dz, Rv=Rv, c=c, h0=h0)
def arm_at(p, u):
    if u < 0:                                          # straight run back into the nose
        return np.r_[p['st'][:2] + p['h0'] * u, p['c']], np.r_[p['h0'], 0.0]
    th = p['th0'] - u / p['R']
    xy = p['C'] + p['R'] * np.array([math.cos(th + math.pi / 2), math.sin(th + math.pi / 2)])
    if u <= p['L'] / 2: ph = math.asin(min(u / p['Rv'], 1)); zz = p['Rv'] * (1 - math.cos(ph))
    else: ph = math.asin(min((p['L'] - u) / p['Rv'], 1)); zz = abs(p['dz']) - p['Rv'] * (1 - math.cos(ph))
    s = -1 if p['dz'] < 0 else 1; tv = np.array([math.cos(th), math.sin(th), s * math.tan(ph)])
    return np.r_[xy, p['c'] + s * zz], tv / np.linalg.norm(tv)
def pill_pts(w, h, n):                                 # stadium outline (x = width, y = height) + outward normals, arc-length sampled
    r = w / 2; c = h / 2 - r; per = 2 * math.pi * r + 4 * c; s = np.arange(n) / n * per; P = []; N = []
    for si in s:
        if si < math.pi * r: a = si / r; P.append([r * math.cos(a), c + r * math.sin(a)]); N.append([math.cos(a), math.sin(a)])
        elif si < math.pi * r + 2 * c: P.append([-r, c - (si - math.pi * r)]); N.append([-1, 0])
        elif si < 2 * math.pi * r + 2 * c: a = math.pi + (si - math.pi * r - 2 * c) / r; P.append([r * math.cos(a), -c + r * math.sin(a)]); N.append([math.cos(a), math.sin(a)])
        else: P.append([r, -c + (si - 2 * math.pi * r - 2 * c)]); N.append([1, 0])
    return np.array(P), np.array(N)
R_FIL = 3.5
def fillet_e(s): return R_FIL if s <= 0 else (0.0 if s >= R_FIL else R_FIL - math.sqrt(max(0.0, R_FIL ** 2 - (R_FIL - s) ** 2)))
def build_arm(drop):
    # rows of points around the pill (n per row). The first rows are the root fillet: per column j the row sits at its own u so that
    # every point is at a fixed distance s from the nose's front face (t' = -14.27): s = R(1 - cos phi), offset e = R(1 - sin phi).
    p = arm_path(C, C - drop); n = 160; PP, NN = pill_pts(18.0, 28.0, n)
    def frame_at(u):
        c, tv = arm_at(p, u)
        inw = np.r_[p['C'] - c[:2], 0]; inw /= np.linalg.norm(inw); c = c + inw * 1.0       # 18 wide, grown 1 toward the bend only (FS)
        xd = np.cross([0, 0, 1], tv); xd /= np.linalg.norm(xd); yd = np.cross(tv, xd)
        return c, xd, yd
    def pts_at(uj, e):                                   # uj, e: arrays (n,)
        out = np.zeros((n, 3))
        for j in range(n):
            c, xd, yd = frame_at(uj[j]); out[j] = c + PP[j, 0] * xd + PP[j, 1] * yd + e[j] * (NN[j, 0] * xd + NN[j, 1] * yd)
        return out
    # u at which each column meets the face (base surface, e = 0): solve t'(u) = T_FRONT by secant from u = -2, 2
    ua, ub = -2.0, 2.0; ta = to_S(pts_at(np.full(n, ua), np.zeros(n)))[:, 1]; tb_ = to_S(pts_at(np.full(n, ub), np.zeros(n)))[:, 1]
    uf = ua + (T_FRONT - ta) * (ub - ua) / (tb_ - ta)
    rings = [pts_at(uf - 2.5, np.full(n, R_FIL)), pts_at(uf - 0.6, np.full(n, R_FIL))]     # buried in the nose
    for ph in np.linspace(0, np.pi / 2, 25):
        rings.append(pts_at(uf + R_FIL * (1 - math.cos(ph)), np.full(n, R_FIL * (1 - math.sin(ph)))))
    u_end_fil = uf + R_FIL; u0 = u_end_fil.max() + 0.5
    for k in np.linspace(0, 1, 9)[1:]:                    # blend the per-column u back to one plane (still on the arm surface, e = 0)
        w = k * k * (3 - 2 * k); rings.append(pts_at((1 - w) * (u_end_fil + 4 * k) + w * (u0 + 4 * k), np.zeros(n)))
    for u in np.linspace(u0 + 4.0, p['L'], int((p['L'] - u0) / 0.8) + 1)[1:]:
        rings.append(pts_at(np.full(n, u), np.zeros(n)))
    V = np.concatenate(rings); m = len(rings); F = []
    for i in range(m - 1):
        a = i * n; b2 = (i + 1) * n
        for j in range(n):
            j2 = (j + 1) % n; F += [[a + j, b2 + j, a + j2], [a + j2, b2 + j, b2 + j2]]
    c0 = len(V); V = np.r_[V, [rings[0].mean(0)], [rings[-1].mean(0)]]
    for j in range(n): F += [[c0, j, (j + 1) % n], [c0 + 1, (m - 1) * n + (j + 1) % n, (m - 1) * n + j]]
    tm = trimesh.Trimesh(V, np.array(F), process=False)
    if tm.volume < 0: tm.invert()
    arm = mf.Manifold(mf.Mesh(vert_properties=np.asarray(tm.vertices, np.float32), tri_verts=np.asarray(tm.faces, np.uint32)))
    assert arm.status().name == 'NoError', arm.status()
    xe = arm_at(p, p['L'])[0][0]; zp = C - drop; YAX = YEND + 1.0
    rr = CS.square([13.0 - 5, 20.0 - 5], center=True).offset(2.5, JT, circular_segments=32)      # tenon 13 (y) x 20 (z) R2.5
    ten = tf(rr.extrude(13.5).translate([0, 0, -13.0]), np.array([[0, 0, 1, xe], [1, 0, 0, YAX], [0, 1, 0, zp], [0, 0, 0, 1]], float))
    m5 = tf(mf.Manifold.cylinder(30, 2.75, 2.75, 32).translate([0, 0, -15]), np.array([[1, 0, 0, xe - 6.5], [0, 0, 1, YAX], [0, 1, 0, zp], [0, 0, 0, 1]], float))
    return arm + ten - m5, dict(xe=xe, zp=zp, L=p['L'], R=p['R'], Rv=p['Rv'])

# ------------------------------------------------------------------ assemble (carrier booleans in S; the arm comes in from world)
nose_tongue = nose + tongue
out = {}
carriers = {}
for tag, drop in (('L', 44.0), ('S', 22.0)):
    arm_w, info = build_arm(drop)
    car = (nose_tongue + tf(arm_w, W2S)) - pocket - channel - spring_well
    carriers[tag] = car; out['arm_' + tag] = info
# blank cap: the same nose + a stub tongue with a snap finger that clicks into the latch groove
stub = prism_z(tongue2d ^ rect(-20, 40, T_BACK - 1.0, 9.0), 216.0, 238.0)
relief = box(13.6, 15.4, 2.6, 10.0, 215.0, 239.0)           # frees a 2-thick finger on the outer side (root in the nose)
bump2d = poly([[17.2, 5.4], [19.0, 5.8], [19.0, 6.6], [17.2, 8.6]])
cap_part = nose + (stub - relief) + prism_z(bump2d, 221.0, 233.0)

# ------------------------------------------------------------------ checks
import shapely.geometry as sg
from functools import reduce
def vol(m): return round(m.volume(), 1)
def shp(cs):                                   # even-odd: xor of every contour
    ps = [sg.Polygon(p).buffer(0) for p in cs.to_polygons() if len(p) > 2]
    return reduce(lambda a, b2: a.symmetric_difference(b2), ps) if ps else sg.Polygon()
rep = dict(units='mm', frame="chair world; S = tip frame with the lean removed: d' = d - 0.0565 (z - 227), t' = t - 0.069 (z - 227)",
           dropped_bad_tris=n_dropped, T_BACK=T_BACK, nose_t=[T_FRONT, T_BACK - SEAM], nose_z=[NZ0 + SEAM, NZ1 - SEAM],
           tongue_d=SL_D, tongue_z=TG_Z, slot_width=round(SL_D[1] - SL_D[0] + 2 * CLR, 2), slot_z=[TG_Z[0] - CLR, TG_Z[1] + CLR])
for name, m in (('wing_r5', wing_r5), ('nose_arm_r5_L', carriers['L']), ('nose_arm_r5_S', carriers['S']), ('button_r5', button),
                ('sleeve_r5', sleeve), ('cap_r5', cap_part)):
    rep[name] = dict(volume=vol(m), genus=m.genus(), status=m.status().name, parts=len(m.decompose()))
rep['wing0_volume'] = vol(W)
for a, bb, nm in ((button, carriers['L'], 'button^carrier'), (button, wing_r5, 'button^wing'), (carriers['L'], wing_r5, 'carrierL^wing'),
                  (carriers['S'], wing_r5, 'carrierS^wing'), (cap_part, wing_r5, 'cap^wing'), (sleeve, carriers['L'], 'sleeve^carrier'),
                  (sleeve, button, 'sleeve^button'), (button.translate([-b['travel'], 0, 0]), wing_r5, 'button_pressed^wing'),
                  (button.translate([-b['travel'], 0, 0]), carriers['L'], 'button_pressed^carrier')):
    rep['clash_' + nm] = vol(a ^ bb)
# tongue engagement along t' (from the notch face) at a few d'
tg = shp(tongue2d)
rep['tongue_engagement'] = {str(d): round(max(0.0, tg.intersection(sg.LineString([(d, T_BACK), (d, 60)])).length), 1) for d in (4.0, 6.0, 9.0, 12.0, 15.0, 17.0)}
# walls around the slot at several heights: distance from the slot outline to air (outside, S1 or any other hole)
slot_s = shp(slot2d)
walls = {}
for z in (TG_Z[0] - CLR + 0.05, 217.0, C, 237.0, TG_Z[1] + CLR - 0.05):
    ws = shp(wing_r5.slice(z)); win = sg.box(-14, T_BACK + 0.3, 34, 45)
    air = win.difference(ws).difference(slot_s.buffer(0.05)).difference(shp(groove2d).buffer(0.05))
    pts = [slot_s.exterior.interpolate(f, normalized=True) for f in np.linspace(0, 1, 1200)]
    pts = [q for q in pts if q.y > T_BACK + 0.3]
    side = lambda q: 'inner' if q.x < SL_D[0] + 1 else ('outer' if q.x > SL_D[1] - 0.5 and q.y < 9.4 else 'back')
    w = {}
    for q in pts:
        k = side(q); dd = q.distance(air); w[k] = min(w.get(k, 99), dd)
    walls[round(z, 2)] = {k: round(v, 2) for k, v in w.items()}
rep['walls_around_slot'] = walls
g = shp(groove2d); wsC = shp(wing_r5.slice(C))
rep['groove'] = dict(t=GROOVE_T, z=GROOVE_Z, depth=GROOVE_DEPTH, skin_behind=round(sg.LineString([(17.6 + GROOVE_DEPTH, GROOVE_T[0] + 0.5), (17.6 + GROOVE_DEPTH, GROOVE_T[1])]).distance(sg.box(-14, -20, 40, 45).difference(wsC.union(g).union(slot_s))), 2),
                     shoulder_t=round(GROOVE_T[0] - T_BACK, 2))
# floor / ceiling of the slot
rep['slot_floor_ceiling'] = dict(floor_to_notch_bottom=round(TG_Z[0] - CLR - NZ0, 2), ceiling_to_notch_top=round(NZ1 - TG_Z[1] - CLR, 2))
# clearances from every hole of the original section (S1, S2, teardrop, hole column...) to what we cut, at the tongue's height and above
cut2d = shp(slot2d).union(shp(groove2d))
notch2d = sg.box(-40, -40, 50, T_BACK)
feat = {}
for z in (204.0, 210.0, C, 240.0, 244.0):
    sec = shp(W.slice(z)); air = sg.box(-40, -25, 45, 95).difference(sec)
    for comp in (air.geoms if air.geom_type == 'MultiPolygon' else [air]):
        if comp.area < 1: continue
        cx, cy = comp.centroid.x, comp.centroid.y
        lab = 'outside air' if comp.area > 1500 else f'feature centred d{cx:.0f} t{cy:.0f} ({comp.area:.0f} mm2)'
        if lab == 'outside air': continue
        dn = comp.distance(notch2d) if NZ0 <= z <= NZ1 else None
        feat[f'z{z:g} {lab}'] = dict(to_slot_or_groove=round(comp.distance(cut2d), 2) if TG_Z[0] <= z <= TG_Z[1] else None, to_notch=None if dn is None else round(dn, 2))
rep['hole_clearance_2d'] = feat
# strap slits S1 (world x 115.55-118.55) and S2 (x 98.15-101.15), hole column W6 (x 75.5, Ø3.3): all vertical/along y, so the clearance
# is the world-x distance from the nearest modified point (slot, groove, notch, fill) at the tongue's heights
def world_minx(cs2d, zs):
    xs = []
    for z in zs:
        for p in cs2d.to_polygons():
            P = np.c_[np.asarray(p), np.full(len(p), z), np.ones(len(p))] @ S2W.T; xs.append(P[:, 0].min())
    return min(xs)
mods = dict(slot=(slot2d, np.linspace(TG_Z[0] - CLR, TG_Z[1] + CLR, 9)), groove=(groove2d, np.linspace(*GROOVE_Z, 5)),
            notch_face=(rect(-9, 26, T_BACK - 0.01, T_BACK), np.linspace(NZ0, NZ1, 9)), fill=(env ^ rect(-14, 34, T_BACK - 0.6, 26.0) - sec240.offset(0.01, JT), np.linspace(215, NZ1, 5)))
for k, (cs2d, zs) in mods.items():
    if cs2d.is_empty(): continue
    x = world_minx(cs2d, zs)
    rep[f'clearance_{k}'] = dict(S1_front_wall_x118_55=round(x - 118.55, 2), S2_front_wall_x101_15=round(x - 101.15, 2), hole_column_x75_5=round(x - 75.5 - 1.65, 2))

# W7 M3 hole to the arm rest (world (141.3, 88.5, 186.6), r 1.65) and the upper W2 M5 bolt (z 260.7, counterbore R7.5): to the notch
rep['hole_z186_to_notch_bottom'] = round(NZ0 - 186.6 - 1.65, 2)
rep['upper_M5_counterbore_to_notch_top'] = round(260.7 - 7.5 - NZ1, 2)
rep.update({k: {kk: round(vv, 2) for kk, vv in v.items()} for k, v in out.items()})
json.dump(rep, open('report.json', 'w'), indent=1)

# ------------------------------------------------------------------ export (world)
def export(m, fn):
    tm = to_trimesh(tf(m, S2W)); tm.export(fn); return tm
export(wing_r5, 'wing_r5.stl'); export(carriers['L'], 'nose_arm_r5_L.stl'); export(carriers['S'], 'nose_arm_r5_S.stl')
export(wing_r5_opt, 'option_wing_r5_W1closed.stl'); export(button, 'button_r5.stl'); export(sleeve, 'sleeve_r5.stl'); export(cap_part, 'cap_r5.stl')
# working solids for the checks/renders
export(tf(W, np.eye(4)), 'work/wing0_fixed.stl')
print(json.dumps(rep, indent=1))
