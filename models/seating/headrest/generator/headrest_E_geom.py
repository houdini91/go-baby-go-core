# GbG headrest option E (concept v0.1): solid geometry for all parts, in GbG_chair_forth coordinates (mm; x lateral, y rearward, z up).
#
# Parts (printed):  RAIL  - dovetail channel bolted to the backrest (4x M6 through the existing holes), 2 captive nuts for the knobs
#                   BAR   - dovetail height bar with a hole ladder (12.5 mm steps), 2x 6 mm carbon rods inside
#                   NECK  - clevis on the bar top + arm over the backrest; printed in 3 lengths (pad 0 / 30 / 60 mm forward)
#                   CRADLE- foam-lined head cradle; strap slots + 20 mm PVC sockets (push-button) designed in
# Bought: 2 star knobs M6, carbon rods, M6 bolts/nylocks, EVA foam + hook-and-loop, (strap, PVC stub, kit button)
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType

SEG = 64
TH = math.atan2(13.0, 160.0)                      # backrest recline: rear face y 131 @ z 230 -> 144 @ z 390
O = np.array([0.0, 131.0, 230.0])                 # rail origin on the backrest rear face
U = np.array([0.0, math.sin(TH), math.cos(TH)])   # along the rail (up)
V = np.array([0.0, math.cos(TH), -math.sin(TH)])  # away from the backrest (rearward)
SEAT_Z = 20.0                                     # seat surface (backrest top 390 = 370 above seat)

# ---- key numbers ----
RAIL_U = (-15.0, 130.0)          # rail length along u
FLOOR = 8.0                      # rail floor (against backrest)
CHAIR_HOLES_Z = (231.0, 296.0)   # existing backrest holes, x +-30
KNOB_U = (70.0, 120.0)           # captive nuts / knob axes
PITCH = 12.5
NPOS = 13                        # 13 heights, k = 0 (highest) .. 12 (lowest)
BAR_L = 276.0
BAR_UB0 = KNOB_U[0] - 12.0       # bar bottom at k = 0
FOOT_W0, FOOT_W1, BODY_W = 32.0, 26.0, 22.0
BAR_V = (FLOOR + 0.25, 16.0, 36.0)   # foot from, foot/body split, body rear
ROD_D, ROD_X, ROD_V = 6.0, 10.0, 11.5
EAR_R = 120.0                    # PVC socket axis radius (keeps the button 10 mm clear of a 147 mm head)
R_IN = 86.0                      # cradle shell inner radius (10 mm EVA + 2 mm hook-and-loop -> 74 mm pad radius)
PAD_R = 74.0
SHELL_T = 4.0
SPAN = 70.0                      # cradle half-angle (deg)
CR_Z = (-45.0, 55.0)             # cradle z range relative to zp (arm top)
ARM_H, ARM_W = 20.0, 40.0
NECKS = (0, 30, 60)

def lu2w(x, v, u):
    return O + u * U + v * V + np.array([x, 0, 0])

def to_world(m):
    """local rail frame (X = x, Y = v, Z = u) -> chair coords"""
    return m.rotate([-math.degrees(TH), 0, 0]).translate(O.tolist())

def rounded(poly, r=1.5):
    cs = CS([poly])
    return cs.offset(-r, JoinType.Round).offset(2 * r, JoinType.Round).offset(-r, JoinType.Round)

def ycyl(d, L, x, y0, z):    # cylinder along +Y from y0, in local coords
    return M.cylinder(L, d / 2, d / 2, SEG // 2).rotate([-90, 0, 0]).translate([x, y0, z])

def xcyl(d, L, x0, y, z):
    return M.cylinder(L, d / 2, d / 2, SEG // 2).rotate([0, 90, 0]).translate([x0, y, z])

def zcyl(d, L, x, y, z0):
    return M.cylinder(L, d / 2, d / 2, SEG // 2).translate([x, y, z0])

def hexprism(af, h, x, y0, z):   # hex nut pocket along +Y
    r = af / math.sqrt(3)
    pts = [(r * math.cos(math.radians(a)), r * math.sin(math.radians(a))) for a in range(0, 360, 60)]
    return CS([pts]).extrude(h).rotate([-90, 0, 0]).translate([x, y0, z])

# ---------------- RAIL ----------------
def rail_local():
    floor = rounded([(-20, 0), (20, 0), (20, FLOOR), (-20, FLOOR)], 1.5)
    wall = [(-20, FLOOR - 1), (-16.25, FLOOR - 1), (-16.25, FLOOR), (-13.25, BAR_V[1]), (-20, BAR_V[1])]
    prof = CS.batch_boolean([floor, rounded(wall, 1.0), rounded([(-p[0], p[1]) for p in wall][::-1], 1.0)], 0)
    L = RAIL_U[1] - RAIL_U[0]
    m = prof.extrude(L).translate([0, 0, RAIL_U[0]])
    ears = rounded([(-40, 0), (40, 0), (40, 6), (-40, 6)], 1.5).extrude(95).translate([0, 0, RAIL_U[0]])
    ears = ears - M.cube([100, 40, 40], True).rotate([45, 0, 0]).translate([0, 6 + 14, RAIL_U[0] + 95])
    m = m + ears
    for z0, s_ in ((RAIL_U[1], 1),):
        m = m - M.cube([100, 30, 30], True).rotate([45, 0, 0]).translate([0, BAR_V[1] + 6, z0 + s_ * 6])
    for zw in CHAIR_HOLES_Z:
        u = (zw - O[2]) / U[2]
        for x in (-30, 30):
            m = m - ycyl(6.6, 30, x, -5, u)
            m = m - M.cylinder(3.3, 3.3, 6.6, SEG // 2).rotate([-90, 0, 0]).translate([x, 6 - 3.3 + 0.01, u])   # countersink
    for u in KNOB_U:
        m = m - ycyl(6.6, 30, 0, -5, u)
        m = m - hexprism(10.2, 6.5, 0, -0.01, u)       # M6 nylock pocket, opens toward the backrest
    return m

# ---------------- BAR ----------------
def bar_profile(cl=0.0):
    v0, v1, v2 = BAR_V
    pts = [(-FOOT_W0 / 2 - cl, v0 - cl), (FOOT_W0 / 2 + cl, v0 - cl), (FOOT_W1 / 2 + cl, v1), (BODY_W / 2 + cl, v1), (BODY_W / 2 + cl, v2 + cl),
           (-BODY_W / 2 - cl, v2 + cl), (-BODY_W / 2 - cl, v1), (-FOOT_W1 / 2 - cl, v1)]
    return rounded(pts, 1.5)

def bar_local(holes=True):
    m = bar_profile().extrude(BAR_L)
    for d in (1, -1):
        m = m - zcyl(ROD_D + 0.4, BAR_L - 8 + 0.01, d * ROD_X, ROD_V, -0.01)       # rod channels, blind 8 mm at the top
    if holes:
        for i in range(17):
            m = m - ycyl(6.5, 50, 0, 0, 12 + PITCH * i)                            # height ladder
        for s in (BAR_L - 12, BAR_L - 35):
            m = m - xcyl(6.5, 60, -30, 26, s)                                     # neck cross-bolts
    # bevel the bottom end
    m = m - M.cube([60, 30, 30], True).rotate([45, 0, 0]).translate([0, BAR_V[2] + 10, -10])
    return m

def rods_local():
    return M.batch_boolean([zcyl(ROD_D, BAR_L - 10, d * ROD_X, ROD_V, 1) for d in (1, -1)], 0)

def bar_ub(k):
    return BAR_UB0 - PITCH * k

def zp_of(k):
    """arm top / pad reference height for setting k"""
    top = lu2w(0, BAR_V[1], bar_ub(k) + BAR_L)
    return top[2] + 3.0

# ---------------- NECK (built for k = 0, moves with the bar) ----------------
def neck(n, k=0):
    ub = bar_ub(k); st = ub + BAR_L                    # bar top u
    zp = zp_of(k)
    # clevis around the bar top (local frame)
    cl = CS([[(-20, BAR_V[1]), (20, BAR_V[1]), (20, BAR_V[2] + 6), (-20, BAR_V[2] + 6)]]).offset(0)
    clev = rounded([(-17.5, BAR_V[1] - 1), (17.5, BAR_V[1] - 1), (17.5, BAR_V[2] + 6), (-17.5, BAR_V[2] + 6)], 2.5).extrude(51).translate([0, 0, st - 45])
    clev = to_world(clev)
    # arm (world), from cradle receiver tongue to the clevis
    cy = 38.0 - n                                       # cradle axis y at the reference (k = 6) - set so pad sits n mm ahead of the backrest face
    dy = (6 - k) * PITCH * U[1]
    yr = cy + dy + R_IN + SHELL_T                       # cradle outer back
    yf = yr + 4                                         # tongue starts inside the receiver
    yb = lu2w(0, BAR_V[1] + 4, st)[1]
    side = rounded([(yf, zp - ARM_H), (yb, zp - ARM_H), (yb, zp), (yf, zp)], 3.0)
    arm = side.extrude(ARM_W).rotate([0, 0, 0])
    # extrude is along +Z in (X=y, Y=z) section; map: section x->world y, section y->world z, extrude->world x
    arm = M.compose([arm]).rotate([90, 0, 90]).translate([-ARM_W / 2, 0, 0])
    m = clev + arm
    # clearance for the bar and its cross bolts
    slot = to_world(bar_profile(0.3).extrude(BAR_L + 40).translate([0, 0, ub - 0.01]))
    m = m - slot
    for s in (st - 12, st - 35):
        m = m - to_world(xcyl(6.5, 60, -30, 26, s))
    for x in (-10, 10):
        m = m - zcyl(6.5, 60, x, yr + 16, zp - 40)     # vertical bolts into the cradle receiver
    return m

# ---------------- CRADLE ----------------
def cradle(n, k=0, windows=True, accessories=True):
    zp = zp_of(k)
    cy = 38.0 - n + (6 - k) * PITCH * U[1]
    z0, z1 = CR_Z
    R = R_IN
    prof = rounded([(R, z0), (R + 12, z0), (R + 12, z0 + 4), (R + SHELL_T, z0 + 4 + 0.01), (R + SHELL_T, z1 - 12), (R + 12, z1 - 4),
                    (R + 12, z1), (R, z1)], 1.6)
    shell = M.revolve(prof, 128, 2 * SPAN).rotate([0, 0, 90 - SPAN])
    # round ends of the C
    m = shell
    if windows:
        for a in (-37, -16, 16, 37):
            w = rounded([(-10, -24), (10, -24), (10, 22), (-10, 22)], 9).extrude(40).rotate([-90, 0, 0]).translate([0, R - 15, 4])
            m = m - w.rotate([0, 0, a])
    # receiver for the neck tongue (open to the rear)
    rec = rounded([(-27, R - 2), (27, R - 2), (27, R + SHELL_T + 30), (-27, R + SHELL_T + 30)], 4).extrude(28).translate([0, 0, -24])
    rec = rec - M.cube([ARM_W + 0.6, 40, ARM_H + 0.6]).translate([-(ARM_W + 0.6) / 2, R + SHELL_T + 4 - 0.01 - 4 + 4, -ARM_H - 0.3])
    rec = rec - M.cube([ARM_W + 0.6, 40, ARM_H + 0.6]).translate([-(ARM_W + 0.6) / 2, R + SHELL_T + 4, -ARM_H - 0.3])
    for x in (-10, 10):
        rec = rec - zcyl(6.6, 60, x, R + SHELL_T + 16, -40)
    m = m + rec
    if accessories:
        for s in (1, -1):
            a = math.radians(s * 66)
            for q in (0,):
                slot = M.cube([4.5, 30, 28], True).translate([0, R + 2, 42]).rotate([0, 0, -math.degrees(a)])
                m = m - slot
            # "button ear": 20 mm PVC socket held out beside the head, bore pointing forward (-y); teardrop underside prints upright
            ab = math.radians(s * 66)
            rb = EAR_R
            bx, by = -rb * math.sin(ab), rb * math.cos(ab)
            ex, ey = -(R + 2) * math.sin(ab), (R + 2) * math.cos(ab)
            pad = M.cube([6, 30, 34], True).rotate([0, 0, -math.degrees(ab)]).translate([ex, ey, 0])
            ear = M.batch_hull([ycyl(32, 34, bx, by - 17, 2), ycyl(4, 34, bx, by - 17, -22), pad])
            m = m + ear
            m = m - ycyl(20.4, 60, bx, by - 30, 2)
            m = m - M.cube([1.6, 40, 14]).translate([bx - 0.8, by - 20, -16])         # clamp split (bottom)
            m = m - xcyl(4.4, 40, bx - 20, by, -10)                                   # M4 clamp screw
    m = m - M.cylinder(20, R - 0.01, R - 0.01, 128).translate([0, 0, -10])          # keep inside clean
    return m.translate([0, cy, zp])

def foam(n, k=0):
    zp = zp_of(k); cy = 38.0 - n + (6 - k) * PITCH * U[1]
    prof = rounded([(PAD_R, CR_Z[0] + 4), (R_IN - 0.5, CR_Z[0] + 4), (R_IN - 0.5, CR_Z[1] - 4), (PAD_R, CR_Z[1] - 4)], 4.0)
    return M.revolve(prof, 128, 2 * (SPAN - 2)).rotate([0, 0, 90 - SPAN + 2]).translate([0, cy, zp])

def accessories(n, k=0, side=1, reach=77):
    """20 mm PVC stub in the button ear + kit push button facing the side of the head (reach = how far forward the holder sits)"""
    zp = zp_of(k); cy = 38.0 - n + (6 - k) * PITCH * U[1]
    ab = math.radians(side * 66); rb = EAR_R
    bx, by = -rb * math.sin(ab), cy + rb * math.cos(ab)
    pvc = ycyl(20, reach + 34, bx, by - reach - 17, zp + 2)
    inward = 1 if bx < 0 else -1
    disc = M.cylinder(14, 32, 32, 48).rotate([0, 90, 0]).translate([bx + inward * 12 - (14 if inward < 0 else 0), by - reach, zp + 2])
    holder = M.cube([22, 26, 26], True).translate([bx + inward * 4, by - reach, zp + 2])
    return pvc, disc + holder

def knobs(k=0):
    out = []
    for u in KNOB_U:
        stem = to_world(ycyl(6, 45, 0, 2, u))
        head = to_world(M.batch_hull([ycyl(32, 12, 0, BAR_V[2] + 1, u)]))
        out.append((stem, head))
    return out

def strap(n, k=0, kid_hl=192, kid_yb=112.0):
    """soft forehead band from slot to slot round the front of the head (optional, therapist decision)"""
    zp = zp_of(k); cy = 38.0 - n + (6 - k) * PITCH * U[1]
    a = math.radians(66); R = R_IN + 2
    sx, sy = R * math.sin(a), cy + R * math.cos(a)
    fy = kid_yb - kid_hl - 3
    b = sy - fy
    ts = np.linspace(math.pi, 2 * math.pi, 64)
    pts = [(sx * math.cos(t), sy + b * math.sin(t)) for t in ts]
    out = []
    zc = lambda t: zp + 42 + 30 * math.sin(t - math.pi) ** 1.5      # rises from the slots (42) to the forehead (72)
    for i in range(len(pts) - 1):
        (x0, y0), (x1, y1) = pts[i], pts[i + 1]
        a0, a1 = zc(ts[i]), zc(ts[i + 1])
        out += [[(x0, y0, a0 - 12), (x1, y1, a1 - 12), (x1, y1, a1 + 12)], [(x0, y0, a0 - 12), (x1, y1, a1 + 12), (x0, y0, a0 + 12)]]
    return np.array(out, float)

# ---------------- assembly helpers ----------------
def tris(m, L=None):
    me = (m.refine_to_length(L) if L else m).to_mesh(); v = np.asarray(me.vert_properties)[:, :3]; t = np.asarray(me.tri_verts)
    return v[t]

def headset(n, k):
    """everything that moves with the height setting"""
    shift = -PITCH * k * U
    bar = to_world(bar_local().translate([0, 0, BAR_UB0]))
    rods = to_world(rods_local().translate([0, 0, BAR_UB0]))
    nk = neck(n, 0)
    cr = cradle(n, 0)
    fo = foam(n, 0)
    return {k_: v.translate(shift.tolist()) for k_, v in dict(bar=bar, rods=rods, neck=nk, cradle=cr, foam=fo).items()}

def rail():
    return to_world(rail_local())

if __name__ == "__main__":
    for k in (0, 6, 12):
        print("k", k, "zp", round(zp_of(k), 1), "pad centre above seat", round(zp_of(k) + 5 - SEAT_Z, 1))
    r = rail(); print("rail", round(r.volume() / 1000, 1), "cm3", r.status())
    b = bar_local(); print("bar", round(b.volume() / 1000, 1), "cm3", b.status())
    for n in NECKS:
        nk = neck(n); print("neck", n, round(nk.volume() / 1000, 1), "cm3", nk.status(), "genus", nk.genus())
    c = cradle(0); print("cradle", round(c.volume() / 1000, 1), "cm3", c.status())
