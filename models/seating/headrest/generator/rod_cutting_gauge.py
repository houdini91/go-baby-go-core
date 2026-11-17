"""Rod cutting gauge with clamp lid, for the headrest's Ø8 carbon rods (workshop tool, PETG).

Base: V-groove for the rod, an end stop, and a saw slot at each rod length 95 + H
(H0 95, H25 120, H50 145, H75 170, H100 195).  v1.3 (2026-11-17): +10 mm, the rods now run ROD_EXT = 10 mm past the
mast plug into seats in the chair's socket floor (gbg_headrest_v10.fs ROD_EXT). The slot's near edge (stop side) is at the
rod length, so the piece between the stop and the slot is exactly that long.
Lid: pressed onto the rod by 2x M3x30 countersunk screws (DIN 7991) into M3 nuts trapped
under the base. Flanges have Ø4.5 holes to screw the gauge to a board.

Both parts print as exported: flat side on the bed, no supports.
Run: python3 rod_cutting_gauge.py [out_dir]   (needs numpy, manifold3d, matplotlib)
"""
import math, os, sys
import numpy as np
from manifold3d import Manifold as M, CrossSection as CS, FillRule

ROD_D = 8.0
ROD_EXT = 10.0       # = ROD_EXT in featurescript/gbg_headrest_v10.fs (rod past the plug into the chair)
LENGTHS = [(n, 85 + h + ROD_EXT) for n, h in (("H0", 0), ("H25", 25), ("H50", 50), ("H75", 75), ("H100", 100))]
W = 32.0             # body width (y)
H = 22.0             # body height (z): M3x30 tip stays inside the base even with no rod in
PROUD = 2.0          # rod top above the base top; the lid presses here
STOP_T = 8.0         # end stop thickness (x < 0)
STOP_UP = 6.0        # stop rises above the rod top
X_END = LENGTHS[-1][1] + 15.0   # base ends 15 mm past the last slot (210)
SLOT_W = 1.8         # saw kerf guide (hacksaw ~1.0, diamond disc ~0.6-1.5)
SLOT_CLEAR = 2.0     # slot floor this far below the rod bottom
FL_W, FL_T = 12.0, 5.0         # side flanges
FL_HOLES_X = [20.0, 108.0, 183.0]  # clear of the slots (95/120/145/170/195 + 1.8); Ø4.5 wood-screw holes
LID_X0, LID_X1 = 12.0, 62.0    # lid sits between the stop and the first slot
LID_T = 8.0
SCREW_X, SCREW_Y = 37.0, 12.0
M3_CLR, M3_CSK_D = 3.5, 6.5    # clearance +0.5 (P1S/PETG policy); countersink 90 deg, DIN 7991 head Ø6
CH = 1.0             # 45 deg chamfer on every outside edge (no sharp corners; bed edges print without overhang)
NUT_AF, NUT_DEPTH = 5.8, 5.4   # trap AF as the headrest; pocket 5.4 deep so the nut sits 2.2 mm up (screw tip never pokes out)
TXT_H, TXT_DEPTH = 4.0, 0.6

R = ROD_D / 2
ZC = H + PROUD - R                 # rod centre
Z_APEX = ZC - R * math.sqrt(2)     # 90 deg V-groove apex
HALF_TOP = H - Z_APEX              # groove half-width at the top face


def box(x0, x1, y0, y1, z0, z1):
    return M.cube([x1 - x0, y1 - y0, z1 - z0]).translate([x0, y0, z0])


def cbox(x0, x1, y0, y1, z0, z1, c=CH):
    """Box with all 12 edges chamfered 45 deg by c (hull of two inset boxes)."""
    return M.batch_hull([box(x0 + c, x1 - c, y0 + c, y1 - c, z0, z1), box(x0, x1, y0, y1, z0 + c, z1 - c)])


def _prism_x(poly_yz, x0, x1):
    # build directly from vertices: two end caps + sides (convex polygons only)
    n = len(poly_yz)
    verts = [(x0, y, z) for (y, z) in poly_yz] + [(x1, y, z) for (y, z) in poly_yz]
    tris = []
    for i in range(1, n - 1):
        tris.append((0, i + 1, i))
        tris.append((n, n + i, n + i + 1))
    for i in range(n):
        j = (i + 1) % n
        tris += [(i, j, n + j), (i, n + j, n + i)]
    from manifold3d import Mesh
    m = M(Mesh(vert_properties=np.array(verts, np.float32), tri_verts=np.array(tris, np.uint32)))
    if m.volume() < 0 or m.is_empty():
        tris = [(a, c, b) for (a, b, c) in tris]
        m = M(Mesh(vert_properties=np.array(verts, np.float32), tri_verts=np.array(tris, np.uint32)))
    return m


def cyl_z(x, y, d, z0, z1, n=48):
    return M.cylinder(z1 - z0, d / 2, d / 2, n).translate([x, y, z0])


def cone_z(x, y, d0, d1, z0, z1, n=48):
    return M.cylinder(z1 - z0, d0 / 2, d1 / 2, n).translate([x, y, z0])


def hex_z(x, y, af, z0, z1, corner_axis="x"):
    r = af / math.sqrt(3)  # circumradius
    pts = [(r * math.cos(math.radians(a)), r * math.sin(math.radians(a))) for a in range(0, 360, 60)]
    return M.extrude(CS([pts]), z1 - z0).translate([x, y, z0])


def text_cs(s, size):
    from matplotlib.textpath import TextPath
    from matplotlib.font_manager import FontProperties
    tp = TextPath((0, 0), s, size=size, prop=FontProperties(family="DejaVu Sans", weight="bold"))
    polys = [p for p in tp.to_polygons() if len(p) > 2]
    cs = CS([np.array(p, float) for p in polys], FillRule.EvenOdd)
    return cs


def engrave(part, s, x_center, y_center, z_top, size=TXT_H, rot=0):
    cs = text_cs(s, size)
    (x0, y0), (x1, y1) = cs.bounds()[:2], cs.bounds()[2:]
    cs = cs.translate([-(x0 + x1) / 2, -(y0 + y1) / 2]).rotate(rot)
    cut = M.extrude(cs, TXT_DEPTH + 0.5).translate([x_center, y_center, z_top - TXT_DEPTH])
    return part - cut


def _lip_chamfer():
    # two 45-deg chamfers along the groove lips (convex prisms)
    out = None
    for sgn in (-1, 1):
        y = sgn * HALF_TOP
        tri = [(y - sgn * CH, H - CH), (y + sgn * CH, H + 0.001), (y + sgn * CH, H + 3), (y - sgn * 3, H + 3)] if sgn > 0 else [(y - sgn * 3, H + 3), (y + sgn * CH, H + 3), (y + sgn * CH, H + 0.001), (y - sgn * CH, H - CH)]
        p = _prism_x(tri, 0, X_END + 1)
        out = p if out is None else out + p
    return out


def base():
    b = cbox(-STOP_T, X_END, -W / 2, W / 2, 0, H)
    b += cbox(-STOP_T, 0, -W / 2, W / 2, H - 2 * CH, H + PROUD + STOP_UP)          # end stop
    b += cbox(-STOP_T, X_END, -W / 2 - FL_W, W / 2 + FL_W, 0, FL_T)                # flanges
    groove = _prism_x([(-HALF_TOP, H + 5), (-HALF_TOP, H + 0.001), (0, Z_APEX), (HALF_TOP, H + 0.001), (HALF_TOP, H + 5)], 0, X_END + 1)
    b -= groove
    b -= _lip_chamfer()                                                             # 45 deg chamfer on the groove lips
    z_floor = ZC - R - SLOT_CLEAR
    for _, L in LENGTHS:                                                             # saw slots, near edge at L
        b -= box(L, L + SLOT_W, -W / 2 - 1, W / 2 + 1, z_floor, H + 1)
    for y in (-SCREW_Y, SCREW_Y):                                                    # M3 holes + nut traps from below
        b -= cyl_z(SCREW_X, y, M3_CLR, -1, H + 1)
        b -= hex_z(SCREW_X, y, NUT_AF, -1, NUT_DEPTH)
    for x in FL_HOLES_X:
        for y in (-(W / 2 + FL_W / 2), W / 2 + FL_W / 2):
            b -= cyl_z(x, y, 4.5, -1, FL_T + 1)
    for name, L in LENGTHS:                                                          # labels next to each slot
        xc = L - 9.0                     # reads along the gauge, just before its slot
        yc = (HALF_TOP + W / 2) / 2
        b = engrave(b, name, xc, yc, H, size=3.6)
        b = engrave(b, "%d" % L, xc, -yc, H, size=3.6)
    return b


def lid():
    z0 = H + PROUD
    l = cbox(LID_X0, LID_X1, -W / 2, W / 2, z0, z0 + LID_T)
    for y in (-SCREW_Y, SCREW_Y):
        l -= cyl_z(SCREW_X, y, M3_CLR, z0 - 1, z0 + LID_T + 1)
        l -= cone_z(SCREW_X, y, M3_CLR, M3_CSK_D + 0.4, z0 + LID_T - (M3_CSK_D + 0.4 - M3_CLR) / 2, z0 + LID_T + 0.01)
    l = engrave(l, "ROD CLAMP", (LID_X0 + LID_X1) / 2, 0, z0 + LID_T, size=3.6)
    return l


def rod(L=LENGTHS[-1][1]):
    return M.cylinder(L, R, R, 48).rotate([0, 90, 0]).translate([0, 0, ZC])


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "stl", "tools")
    os.makedirs(out, exist_ok=True)
    B, Lid, Rd = base(), lid(), rod()
    # checks
    for n, p in (("base", B), ("lid", Lid)):
        assert p.status().name == "NoError" and not p.is_empty(), n
        print(n, "volume %.1f cm3, genus %d" % (p.volume() / 1000, p.genus()))
    for x in FL_HOLES_X:
        assert all(not (L - 2.25 - 2 < x < L + SLOT_W + 2.25 + 2) for _, L in LENGTHS), ("flange hole hits a slot", x)
    assert LID_X1 < LENGTHS[0][1] - 12, "lid over the first slot label"
    print("slots at", [L for _, L in LENGTHS], "X_END", X_END)
    print("rod in groove, overlap with base %.4f mm3 (want ~0)" % (B ^ Rd).volume())
    print("rod vs lid overlap %.4f mm3 (lid touches the rod top)" % (Lid ^ Rd).volume())
    print("rod top z %.2f, base top z %.2f, lid bottom z %.2f" % (ZC + R, H, H + PROUD))
    # screw length check: M3x30 countersunk, head flush in the lid
    tip_z = H + PROUD + LID_T - 30.0
    print("M3x30 tip at z %.2f clamped, %.2f with no rod; nut (2.4 thick) sits at z %.1f-%.1f" % (tip_z, tip_z - PROUD, NUT_DEPTH - 2.4, NUT_DEPTH))
    import trimesh
    def save(m, name, flip=False):
        mesh = m.to_mesh(); t = trimesh.Trimesh(mesh.vert_properties[:, :3], mesh.tri_verts)
        if flip:   # lid prints with its top (countersinks) up: already so; move to z=0
            pass
        t.apply_translation([0, 0, -t.bounds[0][2]])
        t.export(os.path.join(out, name)); print("wrote", name, t.bounds[1] - t.bounds[0])
    save(B, "rod_cutting_gauge_base.stl")
    save(Lid, "rod_cutting_gauge_lid.stl")
