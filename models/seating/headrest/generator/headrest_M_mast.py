# GbG headrest: MAST options under the locked F3 pad (K2 tapered tenon + nuts, 2-rod spine, SW button mount W1).
# Built in the LOCAL chair frame (backrest un-reclined: front face y 123.5, top edge z 391.3), then recline() -> world.
# Everything above the pad's bottom line (pad frame z BOT) is the locked K2 head (tenon, column stub, spine bosses).
# Below it each option is a LOFT of convex sections (hull of neighbours) so it flows; the chair side gets a matching receiver.
#   M1 "Stem"      : one printed piece; the column flares into the pad's chin and into a saddle on the backrest top. 2x Ø8 rods.
#   M2 "Twin posts": printed HEAD + printed FOOT + 2 exposed carbon tubes 16x12 (height = tube length; same 2 prints for every child).
#   M3 "High back" : the backrest seems to grow up into the headrest: wide shoulders over the top edge, waist, chin flare. 2x Ø8 rods.
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS, JoinType
import headrest_F_flow as F, headrest_K_options as K, headrest_E4_geom as G, headrest_E3_geom as E
from headrest_E4_geom import rrbox, recline

KP = "F3"
TOP = 391.3                     # backrest top edge (local)
FRONT = 123.5                   # backrest front face (local)
SOCK_Z0, SCL = 345.0, 0.2       # receiver socket floor, fit
NUT_M5_AF = F.FIT.NUT_M5_AF      # printed M5 nut slot across flats (tolerance block: headrest_FITS.py)
import os
RVAR = os.environ.get("MAST_R", "R2")
_RV = dict(   # receiver / connection variants (Mikey 2026-11-15: less chunky, less depth down the back)
    R0=dict(plug=(-28.0, 28.0, 132.0, 158.0), foot=(42.0, 126.0, 166.0, 3.0), fade=270.0, nut_y=139.5, spine=None),
    R1=dict(plug=(-28.0, 28.0, 132.0, 158.0), foot=(36.0, 126.0, 164.0, 3.6), fade=322.0, nut_y=139.5, spine=None),
    R2=dict(plug=(-28.0, 28.0, 128.0, 154.0), foot=(38.0, 125.0, 160.0, 4.0), fade=322.0, nut_y=136.0, spine=None),
    R3=dict(plug=(-28.0, 28.0, 127.5, 148.5), foot=(38.0, 125.0, 154.5, 4.0), fade=322.0, nut_y=134.0, spine=((-16.0, 112.0), (16.0, 112.0))))
RV = _RV[RVAR]
if RV["spine"]: K.SPINE = RV["spine"]
PLUG = RV["plug"]
ELL, TO, BOT = F.ELL, F.TO, F.BOT
HEIGHTS = (0, 25, 50, 75, 100)
def frame(n, H):
    zc, cy = G.frame(n, H); return zc, cy           # pad-frame origin in local coords (zc = 451 + H, cy = 28.8 - n)
def L(m, n, H): zc, cy = frame(n, H); return m.translate([0, cy, zc])
def chin_z(n, H): return frame(n, H)[0] + BOT       # pad bottom line, local
# v1.2: the M1 stem top sits STEM_DROP under the chin line = 0.1 mm under the pad's flat bottom (outline z -52.5).  At 0.3 it
# poked 0.2 mm into the pad: cutting the pad out of the mast left 0.2 mm feather ridges along the column-socket / channel
# clearances and the chin screw spot-faces.  The head (column, bosses) now starts HEAD_DROP under the chin, 0.3 into the stem.
STEM_DROP, HEAD_DROP = 0.6, 0.9

# ---------------- convex section + loft ----------------
def oval_back(x, cy):
    """local y of the pad's back oval (r = TO) at x"""
    return cy + ELL * np.sqrt(np.maximum(TO ** 2 - x ** 2, 1.0))
def section(hw, y0, y1, p=3.2, back=None, n=72, bw=1.0):
    """convex rounded section: superellipse |x/hw|^p + |v|^p = 1; front flat-ish at y0, back at y1 or following `back(x)` (convex)"""
    t = np.linspace(0, 2 * np.pi, n, endpoint=False)
    c, s = np.cos(t), np.sin(t)
    x = hw * np.sign(c) * np.abs(c) ** (2 / p); v = np.sign(s) * np.abs(s) ** (2 / p)
    ym = (y0 + y1) / 2; hd = (y1 - y0) / 2
    y = ym + hd * v
    if back is not None:
        yb = y1 + (np.minimum(back(x), y1) - y1) * bw          # bw: 0 = flat back, 1 = follows the pad oval (blended, no step)
        y = np.where(v > 0, ym + (yb - ym) * v, y)
    return np.stack([x, y], 1)
def slab(pts, z, th=0.02):
    cs = CS([pts.tolist()]); cs = cs if cs.area() > 0 else CS([pts[::-1].tolist()])
    return cs.extrude(th).translate([0, 0, z])
def loft(secs):
    """secs: list of (z, pts) bottom->top; hull neighbours, union"""
    sl = [slab(p, z) for z, p in secs]
    return M.batch_boolean([M.batch_hull([a, b]) for a, b in zip(sl[:-1], sl[1:])], 0)
def sm(t): t = np.clip(t, 0, 1); return t * t * (3 - 2 * t)
def lerp(a, b, t): return a + (b - a) * t

# ---------------- common: locked head (K2 tenon + stub inside the pad) ----------------
_C = {}
def head_locked():
    """F3 mast above the pad's bottom line (pad frame): tenon, column stub, spine bosses, nut slots"""
    if "head" not in _C:
        _C["head"] = F.mast() ^ M.cube([400, 400, 200]).translate([-200, -200, BOT - HEAD_DROP])
    return _C["head"]
def pad(n, H): return L(F.pad(KP), n, H)
def foam(n, H): return L(F.foam(KP), n, H)
def lock_screws(n, H): return L(F.screws(), n, H)
def driver_paths(n, H): return L(M.batch_boolean([F._xf(F._scr_local(-60, 0, 7.0), g) for g in (1, -1)], 0), n, H)
SPINE = [(x, y) for x, y in K.SPINE]                 # pad frame (±16, 118) -> local y 146.8 at n = 0
def spine_local(n): return [(x, y + frame(n, 0)[1]) for x, y in SPINE]
def rods(n, H, z0=SOCK_Z0 + 1):
    zt = frame(n, H)[0] + K.ROD_TOP
    return M.batch_boolean([M.cylinder(zt - z0, 4.0, 4.0, 32).translate([x, y, z0]) for x, y in spine_local(n)], 0)
def rod_holes(n, H, z0=SOCK_Z0 - 0.5):
    zt = frame(n, H)[0] + K.ROD_TOP + 0.5
    return M.batch_boolean([M.cylinder(zt - z0, F.FIT.ROD_HOLE_D / 2, F.FIT.ROD_HOLE_D / 2, 32).translate([x, y, z0]) for x, y in spine_local(n)], 0)

# ---------------- chair side: RECEIVER (printed into top_back), restyled to flow down the back like a spine ----------------
FOOT = RV["foot"]   # common foot footprint (hw, front, back, p) = top of the receiver: nothing overhangs
BOLTS = (360.0, 379.0)          # 2x M5 from BEHIND (along y) at x 0, between the rods -> captive nuts in the plug
def receiver_shell():
    """lens-shaped housing behind the backrest: fades into the back face at z 290, full size at the top edge"""
    secs = []
    fz = RV["fade"]
    for z in np.concatenate([np.linspace(fz, SOCK_Z0 - 6, 16), [TOP]]):
        t = sm((z - fz) / (SOCK_Z0 - 6 - fz))                  # grows up the back; full size (= the foot's footprint) round the whole socket
        hw = lerp(18.0 if fz < 300 else 30.0, FOOT[0], t); yb = lerp(144.5, FOOT[2], t)
        secs.append((z, section(hw, FOOT[1], yb, p=lerp(2.2, FOOT[3], t))))
    return loft(secs)
def socket(cl=SCL):
    return rrbox(PLUG[0] - cl, PLUG[1] + cl, PLUG[2] - cl, PLUG[3] + cl, SOCK_Z0, TOP + 40, 6.0)
def bolt_cuts(head=True):
    out = []
    for z in BOLTS:
        out.append(M.cylinder(60, F.FIT.M5_CLEAR / 2, F.FIT.M5_CLEAR / 2, 24).rotate([-90, 0, 0]).translate([0, PLUG[2] + 2, z]))
        if head: out.append(M.cylinder(20, 5.0, 5.0, 32).rotate([-90, 0, 0]).translate([0, FOOT[2] - 3.0, z]))   # counterbore: head flush with the back
    return M.batch_boolean(out, 0)
def nut_slots():
    """M5 nut slides in from the side of the plug (hex across flats 8)"""
    out = []
    for z in BOLTS:
        nut = CS.circle(NUT_M5_AF / math.sqrt(3), 6).extrude(F.FIT.NUT_M5_TRAP).translate([0, 0, -F.FIT.NUT_M5_TRAP / 2]).rotate([90, 0, 0]).rotate([0, 0, 0])
        out.append(M.batch_hull([nut.translate([0, RV['nut_y'], z]), nut.translate([-40, RV['nut_y'], z])]))
    return M.batch_boolean(out, 0)
def bolts():
    return M.batch_boolean([M.cylinder(FOOT[2] - 3.0 - RV['nut_y'] + 4, 2.5, 2.5, 24).rotate([-90, 0, 0]).translate([0, RV['nut_y'] - 4, z]) + M.cylinder(2.8, 4.25, 4.25, 32).rotate([-90, 0, 0]).translate([0, FOOT[2] - 2.9, z]) for z in BOLTS], 0)
def top_back_local():
    if "tb" not in _C:
        tb = E.top_back()
        _C["tb"] = tb.translate([-G.PIV[0], -G.PIV[1], -G.PIV[2]]).rotate([G.TH, 0, 0]).translate(list(G.PIV))
    return _C["tb"]
def receiver_chair(spine_bores=True):
    """top_back with the restyled receiver (local frame)"""
    fill = rrbox(PLUG[0] - 3, PLUG[1] + 3, FRONT + 0.1, 144.5, SOCK_Z0 - 6, TOP - 0.05, 2.0) ^ M.cube([300, 300, 100]).translate([-150, 0, SOCK_Z0 - 6])   # v1.2b: +-31 (was +-35: ran into the side mount holes)
    r = top_back_local() + fill + (receiver_shell() - M.cube([300, 300, 100]).translate([-150, 0, TOP]))   # fill = the old front recess / centre hole round the socket
    r = r - socket() - bolt_cuts() - tb_mount_seats()
    return r
TB_MOUNT_HOLE_D, TB_MOUNT_CB_D, TB_MOUNT_CB_Y = 5.2, 8.9, 130.8   # measured on chair_mesh/chair_top_back.stl (= the .fs)
TB_SIDE_MOUNTS, TB_MOUNT_SEAT_Y, TB_MOUNT_SEAT_R = [(-37.5, 373.4), (37.5, 373.4)], 143.0, 5.5   # (= the .fs)
def tb_mount_seats():
    """v1.2b: top_back's side M5 mounts (x +-37.5, z 373.4) stay full holes: explicit re-cuts after the union of the Ø5.2
       teardrop through-hole (tip up) and the Ø8.9 front counterbore to y 130.8, plus an Ø11 washer/nut seat 0.9 mm into the
       original back face, cleared backwards through the receiver housing (= the .fs; no copy of top_back)"""
    import headrest_POD11 as P11
    out = []
    for x, z in TB_SIDE_MOUNTS:
        out += [M.cylinder(FOOT[2] + 20 - TB_MOUNT_SEAT_Y, TB_MOUNT_SEAT_R, TB_MOUNT_SEAT_R, 64).rotate([-90, 0, 0]).translate([x, TB_MOUNT_SEAT_Y, z]),
                P11.td_hole([x, FRONT - 1, z], [x, TB_MOUNT_SEAT_Y + 0.5, z], TB_MOUNT_HOLE_D / 2, [0, 0, 1]),
                M.cylinder(TB_MOUNT_CB_Y - (FRONT - 1), TB_MOUNT_CB_D / 2, TB_MOUNT_CB_D / 2, 64).rotate([-90, 0, 0]).translate([x, FRONT - 1, z])]
    return M.batch_boolean(out, 0)

# ---------------- the plug (inside the receiver), shared ----------------
def plug(z1=TOP + 0.5):
    return rrbox(PLUG[0], PLUG[1], PLUG[2], PLUG[3], SOCK_Z0 + 0.3, z1, 6.0)

# ---------------- M1 STEM ----------------
def _chin_sec(cy, hw, y0, p=3.0):
    return section(hw, y0, oval_back(0, cy), p=p, back=lambda x: oval_back(x, cy) - 0.4)
def m1_body(n, H, foot_hw=FOOT[0], chin_hw=44.0, waist=0.9):
    zc, cy = frame(n, H); zch = chin_z(n, H) - STEM_DROP
    z_foot = TOP + 0.3
    gap = zch - z_foot
    ff = min(30.0, 0.42 * gap); cf = min(26.0, 0.45 * gap)       # foot flare / chin flare heights
    ybk = oval_back(0, cy) - 0.4
    secs = []
    zs = np.append(np.arange(z_foot, zch - 0.5, 1.5), zch)          # FINAL v1.0.1: dense uniform sections (= the smooth Onshape surface)
    for z in zs:
        tf = sm((z - z_foot) / max(ff, 1e-3))            # 0 at the foot -> 1 at stem
        tc = sm((z - (zch - cf)) / max(cf, 1e-3))        # 0 in the stem -> 1 at the chin
        mid = 1 - (2 * ((z - z_foot) / max(gap, 1)) - 1) ** 2          # FINAL v1.0.1: smooth waist (was 1-|2u-1|: a crease at mid-height)
        hw_stem = 28.0 * lerp(1.0, waist, sm(mid))
        hw = lerp(lerp(foot_hw, hw_stem, tf), chin_hw, tc)
        y0 = lerp(lerp(FOOT[1], 132.0, tf), cy + 103.2, tc)
        y1 = lerp(lerp(FOOT[2], ybk, tf), ybk, tc)
        back = (lambda x, cy=cy: oval_back(x, cy) - 0.4)            # FINAL v1.0.1: blended in (was switched on at tf > 0.5: a step)
        secs.append((z, section(hw, y0, y1, p=lerp(lerp(FOOT[3], 2.6, tf), 3.0, tc), back=back, bw=sm((tf - 0.25) / 0.5))))
    return loft(secs)
def m1(n, H):
    m = plug() + m1_body(n, H) + L(head_locked(), n, H)
    return finish(m, n, H)
def finish(m, n, H, rods_=True):
    m = m - pad(n, H)
    if rods_: m = m - rod_holes(n, H)
    m = m - bolt_cuts(head=False) - nut_slots()
    for g in (1, -1): m = m - L(F.screw_cut_mast(g), n, H)
    return m - receiver_chair()

# ---------------- M2 TWIN POSTS (carbon tubes) ----------------
TUBE_OD, TUBE_ID = 16.0, 12.0
TX, TY = 17.0, 143.0              # tube axes x = ±17, local y 143 (n = 0)
HEAD_COL, FOOT_COL = 24.0, 11.0   # printed collar heights below the chin / above the top edge
def tube_axes(n): return [(s * TX, TY - n * 0.0) for s in (1, -1)]
def tube_span(n, H):
    return SOCK_Z0 + 4.0, chin_z(n, H) + 12.0             # bottom (in the plug) .. top (inside the pad stub)
def tubes(n, H):
    z0, z1 = tube_span(n, H)
    return M.batch_boolean([M.cylinder(z1 - z0, TUBE_OD / 2, TUBE_OD / 2, 48).translate([x, y, z0]) - M.cylinder(z1 - z0 + 2, TUBE_ID / 2, TUBE_ID / 2, 40).translate([x, y, z0 - 1]) for x, y in tube_axes(n)], 0)
def tube_bores(n, H):
    z0, z1 = tube_span(n, H)
    return M.batch_boolean([M.cylinder(z1 - z0 + 0.6, TUBE_OD / 2 + 0.15, TUBE_OD / 2 + 0.15, 48).translate([x, y, z0 - 0.3]) for x, y in tube_axes(n)], 0)
def m2_parts(n, H):
    zc, cy = frame(n, H); zch = chin_z(n, H) - 0.3; ybk = oval_back(0, cy) - 0.4
    zf = TOP + 0.3; zh = zch - HEAD_COL
    if zh - (zf + FOOT_COL) < 6.0: return None                   # too short for visible tubes: use M1
    foot = []
    for z in np.linspace(zf, zf + FOOT_COL, 7):
        t = sm((z - zf) / FOOT_COL)
        foot.append((z, section(lerp(FOOT[0], 33.0, t ** 0.7), lerp(126.0, 133.0, t), lerp(FOOT[2], 156.0, t), p=lerp(FOOT[3], 2.4, t))))
    headc = []
    for z in np.linspace(zh, zch, 9):
        t = sm((z - zh) / HEAD_COL)
        headc.append((z, section(lerp(32.0, 44.0, t), lerp(133.0, cy + 103.2, t), ybk, p=lerp(2.4, 3.0, t), back=lambda x, cy=cy: oval_back(x, cy) - 0.4)))
    fp = plug() + loft(foot); hp = loft(headc) + L(head_locked(), n, H)
    fp = fp - tube_bores(n, H) - bolt_cuts(head=False) - nut_slots() - top_back_local()
    hp = hp - tube_bores(n, H) - pad(n, H)
    for g in (1, -1): hp = hp - L(F.screw_cut_mast(g), n, H)
    # pinch slit + M3 across each socket (locks only; the tube bottoms on the socket floor = shape carries the load)
    for (x, y) in tube_axes(n):
        for part, z in (("f", zf + FOOT_COL - 5.0), ("h", zh + 6.0)):
            cut = M.cylinder(60, 1.7, 1.7, 20).rotate([-90, 0, 0]).translate([x, 100, z])
            if part == "f": fp = fp - cut
            else: hp = hp - cut
    # spine rods are replaced by the tubes; head bosses stay solid (fill the pad sockets)
    return fp, hp
def m2(n, H):
    r = m2_parts(n, H)
    return None if r is None else r[0] + r[1]

# ---------------- M3 HIGH BACK ----------------
def m3(n, H):
    zc, cy = frame(n, H); zch = chin_z(n, H) - 0.3; ybk = oval_back(0, cy) - 0.4
    zf = TOP + 0.3; gap = zch - zf
    secs = []
    for z in np.linspace(zf, zch, max(14, int(gap / 5) + 2)):
        u = (z - zf) / max(gap, 1)
        # width: shoulders (58) -> waist (34 at 55 %) -> chin (56); concave curve
        wa = FOOT[0] + 16.0 * (1 - sm(u / 0.12)) * 0 - (FOOT[0] - 34.0) * sm(u / 0.55) ** 0.85
        hw = wa + (44.0 - 34.0) * sm((u - 0.55) / 0.45) ** 1.6
        y0 = lerp(127.0, cy + 103.2, sm((u - 0.5) / 0.5)) if u > 0.5 else lerp(126.0, 127.0, sm(u / 0.5))
        y1 = lerp(FOOT[2], ybk, sm(u / 0.6))
        back = (lambda x, cy=cy: oval_back(x, cy) - 0.4) if u > 0.35 else None
        secs.append((z, section(hw, y0, y1, p=lerp(FOOT[3], 3.0, u), back=back)))
    m = plug() + loft(secs) + L(head_locked(), n, H)
    return finish(m, n, H)

OPTS = dict(M1=m1, M2=m2, M3=m3)
def build(k, n, H):
    key = (k, n, H)
    if key not in _C: _C[key] = OPTS[k](n, H)
    return _C[key]

if __name__ == "__main__":
    import sys, time
    rc = receiver_chair()
    print("receiver", rc.status(), "%.0f cm3 (top_back alone %.0f)" % (rc.volume() / 1000, top_back_local().volume() / 1000))
    for k in (sys.argv[1:] or OPTS):
        for H in (0, 50, 100):
            t = time.time(); m = build(k, 0, H)
            if m is None: print(k, H, "-> too short, M1 used"); continue
            p = pad(0, H); f = foam(0, H)
            extra = ""
            if k == "M2": extra = " tubes∩mast %.1f tubes∩chair %.1f" % ((tubes(0, H) ^ m).volume(), (tubes(0, H) ^ rc).volume())
            else: extra = " rods outside %.1f" % (rods(0, H) - (m + rod_holes(0, H))).volume()
            print(k, "H", H, m.status(), "%.0f cm3" % (m.volume() / 1000), "| ∩pad %.1f ∩foam %.1f ∩chair %.1f | screws∩ %.1f driver∩ %.1f | slide-in %s" % (
                (m ^ p).volume(), (m ^ f).volume(), (m ^ rc).volume(), (lock_screws(0, H) ^ m).volume(), (driver_paths(0, H) ^ m).volume(),
                " ".join("%g:%.1f" % (dz, (p.translate([0, 0, dz]) ^ m).volume()) for dz in (2, 20, 60))) + extra, "%.0fs" % (time.time() - t), flush=True)
