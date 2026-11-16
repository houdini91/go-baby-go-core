# GbG headrest M1: how the mast meets the CHAIR (base options, review). Same M1 stem/chin/head above the backrest top edge.
#   B1 "Spine"  : reprinted top_back with a narrow lens receiver that runs down the back like a spine; 56x26 plug in a 45 mm socket,
#                 2x M5 from behind into nuts in the plug. Rods go 45 mm down into the socket.
#   B2 "Roll"   : reprinted top_back whose top becomes a wide soft ROLL (car-seat top edge) housing the same socket; the mast rises
#                 out of the middle of the roll. Same plug, rods, bolts.
#   B3 "Tongue" : NO chair reprint. The mast foot continues down the back as a tongue bolted through the chair's OWN rear M5 holes
#                 (x 0/±37 at z 374, x ±30 at z 297). Works on every existing chair.
# Relief window (optional, all three): one soft stadium window through the stem between the two rods, for H >= 50.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_M_mast as MM
from headrest_M_mast import section, loft, sm, lerp, TOP, FOOT, frame, L

HOLES_HI = [(-37.0, 373.8), (0.0, 373.8), (37.0, 373.8)]
HOLES_LO = [(-30.0, 296.5), (30.0, 296.5)]
BACK = 144.0                                         # backrest back face (local)

# ---------------- chair sides ----------------
def roll_shell():
    secs = []
    for z in np.concatenate([np.linspace(318.0, MM.SOCK_Z0 - 4, 12), [TOP]]):
        t = sm((z - 318.0) / (MM.SOCK_Z0 - 4 - 318.0))
        secs.append((z, section(lerp(40.0, 58.0, t ** 0.6), 126.0, lerp(144.5, 166.0, t), p=lerp(2.6, 4.2, t))))
    return loft(secs)
_CH = {}
def chair(b):
    if b in _CH: return _CH[b]
    tb = MM.top_back_local()
    if b == "B1": c = MM.receiver_chair()
    elif b == "B2":
        c = tb + (roll_shell() - M.cube([300, 300, 100]).translate([-150, 0, TOP]))
        c = c - MM.socket() - MM.bolt_cuts()
    else: c = tb                                       # B3: chair unchanged
    _CH[b] = c; return c

# ---------------- B3 tongue ----------------
def tongue():
    secs = []
    for z in np.concatenate([np.linspace(284.0, 330.0, 8), np.linspace(338.0, TOP + 0.3, 8)]):
        t = sm((z - 284.0) / (TOP - 284.0))
        secs.append((z, section(lerp(38.0, FOOT[0], t), 140.0, lerp(158.0, FOOT[2], t), p=lerp(2.6, FOOT[3], t))))
    return loft(secs)
def tongue_bolts(cut=False):
    out = []
    for x, z in HOLES_HI + HOLES_LO:
        if cut:
            out.append(M.cylinder(60, 2.75, 2.75, 24).rotate([-90, 0, 0]).translate([x, 110, z]))
            nut = CS.circle(8.1 / math.sqrt(3), 6).extrude(30).rotate([-90, 0, 0])
            out.append(nut.translate([x, 151.0, z]))                                     # captive nut pocket, open at the back (nut + cap below)
        else:
            out.append(M.cylinder(26, 2.5, 2.5, 24).rotate([-90, 0, 0]).translate([x, 128.0, z]) + M.cylinder(2.8, 4.5, 4.5, 32).rotate([-90, 0, 0]).translate([x, 125.5, z]))   # button head sits in the chair front recess
    return M.batch_boolean(out, 0)

# ---------------- relief window ----------------
def relief(n, H):
    zch = MM.chin_z(n, H) - MM.STEM_DROP; gap = zch - (TOP + 0.3)
    ff = min(30.0, 0.42 * gap); cf = min(26.0, 0.45 * gap)
    z0, z1 = TOP + 0.3 + ff + 6.0, zch - cf - 2.0
    if z1 - z0 < 26.0: return None
    w = 8.0                                            # half-width: rods at ±16 (r 4) keep 4 mm of wall
    cs = CS.batch_hull([CS.circle(w, 48).translate([0, z0 + w]), CS.circle(w, 48).translate([0, z1 - w])])
    win = cs.extrude(120).translate([0, 0, -60]).rotate([90, 0, 0]).translate([0, 145, 0])    # (x, z) profile through y
    # soft edges: chamfer-ish rounding by hulling a slightly bigger stadium at each face
    big = CS.batch_hull([CS.circle(w + 2.0, 48).translate([0, z0 + w]), CS.circle(w + 2.0, 48).translate([0, z1 - w])])
    for y0 in (131.0, 157.0):
        win = win + M.batch_hull([cs.extrude(0.01).rotate([90, 0, 0]).translate([0, 145 + (y0 - 145) * 0.7, 0]),
                                  big.extrude(0.01).rotate([90, 0, 0]).translate([0, y0 + (1.5 if y0 > 145 else -1.5), 0])])
    return win

# ---------------- mast per base ----------------
_C = {}
def mast(b, n, H, window=True):
    key = (b, n, H, window)
    if key in _C: return _C[key]
    body = MM.m1_body(n, H) + L(MM.head_locked(), n, H)
    if b in ("B1", "B2"):
        m = MM.plug() + body
        m = m - MM.pad(n, H) - MM.rod_holes(n, H) - MM.bolt_cuts(head=False) - MM.nut_slots()
    else:
        m = tongue() + body
        m = m - MM.pad(n, H) - MM.rod_holes(n, H, z0=TOP + 2.0) - tongue_bolts(cut=True)
    for g in (1, -1): m = m - L(MM.F.screw_cut_mast(g), n, H)
    if window:
        w = relief(n, H)
        if w is not None: m = m - w
    m = m - (chair(b) if b != "B3" else MM.top_back_local())      # the CHAIR AS BUILT (socket cut), not the original top_back
    _C[key] = m; return m
def rods(b, n, H): return MM.rods(n, H, z0=(MM.SOCK_Z0 + 1) if b != "B3" else TOP + 2.5)
def bolts(b): return MM.bolts() if b != "B3" else tongue_bolts()

if __name__ == "__main__":
    import sys, time
    for b in (sys.argv[1:] or ("B1", "B2", "B3")):
        c = chair(b)
        for H in (0, 50, 100):
            t = time.time(); m = mast(b, 0, H); p = MM.pad(0, H)
            sup = c.slice(TOP - 0.6) + MM.socket().slice(TOP - 0.6)
            r = rods(b, 0, H)
            print(b, "H", H, m.status(), "%.0f cm3" % (m.volume() / 1000), "| ∩chair %.1f ∩pad %.1f ∩foam %.1f | over air %.0f mm2 | rods outside %.1f | bolts∩mast %.1f ∩chair %.1f | lift-out %s | window %s" % (
                (m ^ c).volume(), (m ^ p).volume(), (m ^ MM.foam(0, H)).volume(), (m.slice(TOP + 0.8) - sup).area(), (r - (m + MM.rod_holes(0, H))).volume(),
                (bolts(b) ^ m).volume(), (bolts(b) ^ c).volume(),
                " ".join("%g:%.1f" % (d, ((m.translate([0, d, 0]) if b == "B3" else m.translate([0, 0, d])) ^ c).volume()) for d in (2, 20, 60)),
                relief(0, H) is not None), "%.0fs" % (time.time() - t), flush=True)
        print(b, "chair part %.0f cm3 (top_back alone %.0f)" % (c.volume() / 1000, MM.top_back_local().volume() / 1000), flush=True)
