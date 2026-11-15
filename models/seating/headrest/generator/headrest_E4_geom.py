# GbG headrest E v0.4 (simplified): chair RECEIVER + one printed MAST per child + cradle (+ optional button mount).
# Everything above the receiver follows the backrest recline (4.6 deg), so the pad stays on the backrest line at any height.
# Base model: pad surface ON the extended backrest front plane (reach 0). Printed reach options: -15 / 0 / +20 / +40 mm (+ = forward).
import math, numpy as np
from manifold3d import Manifold as M
import headrest_E3_geom as E
from headrest_E3_geom import rbox, box, sector, ycyl, zcyl, xcyl, hexprism, rounded, place, tris

TH = math.degrees(math.atan2(13.0, 160.0))   # backrest recline
PIV = (0.0, 123.5, 390.0)                    # backrest top, front edge
TOP_Z, SEAT_Z = 390.0, 20.0
MX, MY0, MY1 = 28.0, 132.0, 158.0            # mast section 56 x 26, 4.5 mm behind the backrest front face
SOCK_Z0, WALL, CL = 345.0, 6.0, 0.2
CROSS = [(150.5, 358.0), (150.5, 380.0)]     # 2x M5 through-bolts across receiver + mast
RODS = [(-16.0, 139.0), (16.0, 139.0), (0.0, 151.0)]   # 3 carbon rods in a triangle: 2 front (tension) + 1 rear
ROD_LOW = {(-16.0, 139.0): 292.0, (16.0, 139.0): 292.0, (0.0, 151.0): 335.0}   # front pair go 53 mm into the solid backrest below the socket
ROD_D = 8.0
HEIGHTS = (0, 25, 50, 75, 100, 125)          # mast length above the backrest top, beyond the 15 mm minimum
REACH = (-15, 0, 15, 30, 45)                # default for most children: 0..+15 (estimate; confirm on a few kids)
YK_Z = (-39.5, -2.0)                         # yoke holds the cradle's lower half (rel. cradle centre), above the bottom rim
RIB_Z = (-36.0, -6.0)
BOLT_Z = -23.0

def rrbox(x0, x1, y0, y1, z0, z1, r=6.0):
    """box with every edge and corner rounded (hull of 8 spheres)"""
    r = min(r, (x1 - x0) / 2 - 0.01, (y1 - y0) / 2 - 0.01, (z1 - z0) / 2 - 0.01)
    sp = M.sphere(r, 32)
    return M.batch_hull([sp.translate([x, y, z]) for x in (x0 + r, x1 - r) for y in (y0 + r, y1 - r) for z in (z0 + r, z1 - r)])
def round_sector(r0, r1, a0, a1, z0, z1, rr=3.0):
    """ring sector with rounded profile AND rounded ends (end caps = hull of spheres)"""
    body = sector(r0, r1, a0, a1, z0, z1, r=rr)
    caps = []
    for a in (a0, a1):
        t = math.radians(a); rm = (r0 + r1) / 2; w = (r1 - r0) / 2
        cx, cy = rm * math.sin(t), rm * math.cos(t)
        caps.append(M.batch_hull([M.sphere(w, 24).translate([cx, cy, z0 + w]), M.sphere(w, 24).translate([cx, cy, z1 - w])]))
    return body + M.batch_boolean(caps, 0)

def recline(m):
    return m.translate([-PIV[0], -PIV[1], -PIV[2]]).rotate([-TH, 0, 0]).translate(list(PIV))
def recline_pts(p):
    p = np.asarray(p, float) - PIV; a = math.radians(-TH)
    y = p[..., 1] * math.cos(a) - p[..., 2] * math.sin(a); z = p[..., 1] * math.sin(a) + p[..., 2] * math.cos(a)
    out = p.copy(); out[..., 1] = y; out[..., 2] = z; return out + PIV

def col_top(H): return TOP_Z + 15.0 + H
def frame(n, H):
    """local (un-reclined) cradle centre z and axis y; pad back sits n mm ahead of the backrest plane (local y 123.5)"""
    zc = col_top(H) + 46.0              # cradle bottom rim sits 1 mm above the column top; the web reaches up to the yoke
    cy = 123.5 - n - E.ELL * E.PAD_R
    return zc, cy
def pad_above_seat(H, n=0):
    zc, cy = frame(n, H)
    p = recline_pts([0, cy + E.ELL * E.PAD_R, zc + 5]); return p[2] - SEAT_Z

# ---------------- receiver (printed into the chair's top_back) ----------------
def receiver_body():
    house = rrbox(-MX - WALL, MX + WALL, 134.0, MY1 + WALL, SOCK_Z0 - 5, TOP_Z + 6, 6.0) - box(-100, 100, 0, 300, TOP_Z, 500)
    gusset = rbox(-MX, MX, 136.0, 140.0, SOCK_Z0 - 55, SOCK_Z0 - 53, 1.5)
    return recline(M.batch_hull([house, gusset]))
def socket(cl=CL):
    return recline(rbox(-MX - cl, MX + cl, MY0 - cl, MY1 + cl, SOCK_Z0, TOP_Z + 30, 6.0))
def cross_cuts(head=True):
    out = []
    for y, z in CROSS:
        out.append(xcyl(5.4, 120, -60, y, z))
        if head:
            out.append(M.cylinder(5, 5.2, 5.2, 6).rotate([0, 90, 0]).translate([MX + WALL - 4.5, y, z]))
            out.append(M.cylinder(5, 5.0, 5.0, 32).rotate([0, 90, 0]).translate([-MX - WALL - 0.5, y, z]))
    return recline(M.batch_boolean(out, 0))
def top_back_receiver():
    bores = recline(M.batch_boolean([zcyl(ROD_D + 0.3, SOCK_Z0 + 1 - ROD_LOW[(x, y)], x, y, ROD_LOW[(x, y)]) for x, y in RODS], 0))
    return E.top_back() + receiver_body() - socket() - cross_cuts() - bores

# ---------------- mast ----------------
def mast(n, H):
    zt = col_top(H); zc, cy = frame(n, H)
    R0 = E.R_IN + E.SHELL_T
    S = lambda m_: m_.scale([1, E.ELL, 1]).translate([0, cy, zc])
    col = rrbox(-MX, MX, MY0, MY1, SOCK_Z0 + 0.3, zt, 6.0)
    f0, f1 = TOP_Z + 14, zt - 22
    if f1 - f0 > 30:
        nw = int((f1 - f0) // 32); h = (f1 - f0 - (nw - 1) * 7) / nw
        for i in range(nw):
            z0 = f0 + i * (h + 7); col = col - rrbox(-MX + 12, -4, MY0 - 6, MY1 + 6, z0, z0 + h, 4.0) - rrbox(4, MX - 12, MY0 - 6, MY1 + 6, z0, z0 + h, 4.0)
    yoke = S(round_sector(R0 + 0.3, R0 + E.YOKE_T + 0.3, -E.YOKE_A, E.YOKE_A, YK_Z[0], YK_Z[1], rr=3.0))
    back = S(sector(R0 + 0.3, R0 + E.YOKE_T + 0.3, -26, 26, YK_Z[0], YK_Z[1] - 4, r=4.0))
    top = rrbox(-MX, MX, MY0, MY1, zt - 14, zt, 6.0)
    m = col + yoke + M.batch_hull([top, back])
    # side braces: rounded struts from the column's top corners out to the underside of the yoke ends (left-right support)
    for s_ in (1, -1):
        a = math.radians(s_ * (E.YOKE_A - 7))
        rr = (R0 + 0.3 + R0 + E.YOKE_T + 0.3) / 2
        px, py = rr * math.sin(a), cy + E.ELL * rr * math.cos(a)
        tip = M.sphere(7.5, 32).translate([px, py, zc + YK_Z[0] + 8])
        rz0 = max(zt - 34, TOP_Z + 8); rz1 = max(rz0 + 11, zt - 4)
        root = rrbox(min(s_ * 8, s_ * MX), max(s_ * 8, s_ * MX), MY0 + 1, MY1 - 1, rz0, rz1, 6.0)
        m = m + M.batch_hull([root, tip])
    m = m - S(M.cylinder(300, R0 + 0.3, R0 + 0.3, 160).translate([0, 0, -150]))
    m = m - S(sector(E.R_IN - E.LIP - 2, E.R_IN + E.RIM + 0.5, -E.SPAN - 1, E.SPAN + 1, E.CR_Z[0] - 0.5, E.CR_Z[0] + 4.5, r=0.3))   # pocket for the bottom rim
    m = m - S(sector(R0 - 0.7, R0 + E.RIB_T + 0.25, -E.RIB_A - 0.25, E.RIB_A + 0.25, RIB_Z[0] - 0.25, YK_Z[1] + 30, r=0.5))
    for a in E.YOKE_BOLTS:
        m = m - place(ycyl(3.4, 60, 0, E.R_IN - 5, BOLT_Z), a).translate([0, cy, zc])
    m = m - M.batch_boolean([zcyl(ROD_D + 0.3, zt - 3 - SOCK_Z0 + 1, x, y, SOCK_Z0 - 1) for x, y in RODS], 0)
    m = recline(m)
    foot = rrbox(-56, 56, 124.8, 162.0, TOP_Z + 0.25, TOP_Z + 10, 5.0)
    gus = []
    zg = min(zt - 8, TOP_Z + 80)
    if zg > TOP_Z + 18:
        for s_ in (1, -1):
            low = rrbox(min(s_ * 42, s_ * 56), max(s_ * 42, s_ * 56), 127.0, 159.0, TOP_Z + 0.25, TOP_Z + 10, 4.5)
            high = recline(rrbox(min(s_ * 18, s_ * MX), max(s_ * 18, s_ * MX), MY0 + 1, MY1 - 1, zg - 12, zg, 5.0))
            gus.append(M.batch_hull([low, high]))
    m = m + foot + (M.batch_boolean(gus, 0) if gus else M())
    m = m - recline(S(M.cylinder(300, R0 + 0.3, R0 + 0.3, 160).translate([0, 0, -150])))
    m = m - recline(S(sector(E.R_IN - E.LIP - 2, E.R_IN + E.RIM + 0.5, -E.SPAN - 1, E.SPAN + 1, E.CR_Z[0] - 0.5, E.CR_Z[0] + 4.5, r=0.3)))
    m = m - recline(M.batch_boolean([zcyl(ROD_D + 0.3, zt - 3 - SOCK_Z0 + 1, x, y, SOCK_Z0 - 1) for x, y in RODS], 0))
    m = m - cross_cuts(head=False)
    return m
def rods(H):
    zt = col_top(H)
    return recline(M.batch_boolean([zcyl(ROD_D, zt - 4 - ROD_LOW[(x, y)] - 1, x, y, ROD_LOW[(x, y)] + 1) for x, y in RODS], 0))
def rod_lengths(H):
    zt = col_top(H); return {f'{x:+.0f},{y:.0f}': round(zt - 4 - ROD_LOW[(x, y)] - 1) for x, y in RODS}
def cross_bolts():
    out = []
    for y, z in CROSS:
        out += [xcyl(5, 2 * (MX + WALL), -MX - WALL, y, z), xcyl(9, 4.5, -MX - WALL - 4, y, z)]
    return recline(M.batch_boolean(out, 0))

# ---------------- cradle & friends: v0.3 parts moved so their tongue / bolts sit on the yoke ----------------
def _to_here(m, n, H):
    zp3, cy3 = E.cradle_frame(0, 50)
    zc, cy = frame(n, H)
    return recline(m.translate([0, cy - cy3, zc - zp3]))
def cradle(n, H):
    zp3, cy3 = E.cradle_frame(0, 50)
    old_rib, E.RIB_Z = E.RIB_Z, RIB_Z
    try:
        c = E.cradle(0, 50)
    finally:
        E.RIB_Z = old_rib
    R = E.R_IN
    # move the 3 bolt holes from z 0 to BOLT_Z: fill the old ones, cut new ones (local cradle frame)
    for a in E.YOKE_BOLTS:
        plug = place(ycyl(3.6, 6, 0, R - 0.1, 0) + hexprism(5.8, 2.8, 0, R - 0.1, 0), a).translate([0, cy3, zp3])
        c = c + (plug ^ M.batch_hull([c]))
        c = c - place(ycyl(3.4, 40, 0, R - 5, BOLT_Z) + hexprism(5.6, 2.6, 0, R - 0.01, BOLT_Z), a).translate([0, cy3, zp3])
    return _to_here(c, n, H)
def foam(n, H): return _to_here(E.foam(0, 50), n, H)
def button_mount(n, H, side=1, yaw=0): return _to_here(E.button_mount(0, 50, side, yaw=yaw), n, H)
def pvc_button(n, H, side=1, yaw=0): return _to_here(E.pvc_button(0, 50, side, yaw=yaw), n, H)
def yoke_bolts(n, H):
    zp3, cy3 = E.cradle_frame(0, 50)
    b = M.batch_boolean([place(ycyl(5.5, 2.5, 0, E.R_IN + (E.SHELL_T + E.YOKE_T) * E.ELL + 0.3, BOLT_Z), a).translate([0, cy3, zp3]) for a in E.YOKE_BOLTS], 0)
    return _to_here(b, n, H)
def strap(n, H):
    zp3, cy3 = E.cradle_frame(0, 50); zc, cy = frame(n, H)
    st = E.strap(0, 50) + np.array([0, cy - cy3, zc - zp3])
    return recline_pts(st)
def assembly(n, H):
    return dict(mast=mast(n, H), cradle=cradle(n, H), foam=foam(n, H), rods=rods(H))

if __name__ == "__main__":
    r = top_back_receiver(); print("top_back+receiver", r.status(), round(r.volume() / 1000, 1))
    for H, n in ((0, 0), (0, 20), (75, 0), (150, 0), (150, -15), (150, 40)):
        A = assembly(n, H)
        print("H", H, "reach", n, "pad above seat %.0f" % pad_above_seat(H, n), "mast", A["mast"].status(), round(A["mast"].volume() / 1000, 1),
              "mast∩chair", round((A["mast"] ^ r).volume(), 1), "cradle∩mast", round((A["cradle"] ^ A["mast"]).volume(), 1), "cradle∩chair", round((A["cradle"] ^ r).volume(), 1))
