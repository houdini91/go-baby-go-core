"""Rod cutting gauge v2 with clamp lid: every carbon rod length of the project, one lane per diameter
(workshop tool, PETG, P1S).

Lanes side by side, all against ONE shared end stop (x = 0):
  Ø6  side-pad arm rod SP-6 (SIDEPAD_ROD6_L), pommel rods PM-6 155 (K23) and PM-6 200 (K45)
  Ø2  side-pad tenon pins SP-2 (SIDEPAD_PIN2_L, 4 per side); the groove sits in a raised rail
  Ø8  headrest rods H0…H100 = 85…185 (unchanged from v1: same groove, same PROUD, same slots)
Each lane's 90° V-groove is sunk so that every rod's top is at the same height, H + PROUD. So one
flat lid clamps any lane. The lid is short (LID_LX) and bolts at either clamp station (STATIONS):
station A (next to the stop) works for every length; station B holds long rods further out.
Lid: 2x M3x30 countersunk screws (DIN 7991) into M3 nuts trapped under the base. Raised lands
under the screws sit LAND_GAP below the rod tops, so the lid stays level whichever lane is in use.
Saw slots run through the full width (a hacksaw stroke and a 30 mm disc both need open ends). The
slot's near edge (stop side) is at the rod length, so the piece between the stop and the slot is
exactly that long. Every slot is labelled in its own lane's label strip ("H0 85", "SP-6 55", ...),
and the lane diameters are engraved on top of the stop.
Flanges have Ø4.5 holes to screw the gauge to a board.

Both parts print as exported: flat side on the bed, no supports.
Run: python3 rod_cutting_gauge.py [out_dir] [png]   (needs numpy, manifold3d, matplotlib, trimesh)
"""
import math, os, sys
import numpy as np
from manifold3d import Manifold as M, CrossSection as CS, FillRule

# ---- rod lengths (mm): update after CAD ------------------------------------------------------
SIDEPAD_ROD6_L = 55.0     # side-pad carrier arm rod Ø6 (GBG-SP-CF6), placeholder until the r5 CAD
SIDEPAD_PIN2_L = 21.5     # side-pad tenon pin Ø2 (GBG-SP-CF2), 4 per carrier; may become 24.5
POMMEL_K23_L = 155.0      # pommel K23A/B, 2 per pommel (models/seating/pommel/README.md)
POMMEL_K45_L = 200.0      # pommel K45A/B, 2 per pommel
HEADREST_L = [("H0", 85), ("H25", 110), ("H50", 135), ("H75", 160), ("H100", 185)]   # 85 + H

# lanes in y order (−y → +y): (lane label, rod Ø, [(slot tag, length)])
LANES = [
    ("Ø6", 6.0, [("SP-6", SIDEPAD_ROD6_L), ("PM-6", POMMEL_K23_L), ("PM-6", POMMEL_K45_L)]),
    ("Ø2", 2.0, [("SP-2", SIDEPAD_PIN2_L)]),
    ("Ø8", 8.0, HEADREST_L),
]

H = 22.0             # body height (z): M3x30 tip stays inside the base even with no rod in
PROUD = 2.0          # every rod top is this far above the base top; the lid presses here
LAND_GAP = 0.5       # screw lands sit this far below the rod tops (lid flex = clamp preload)
STOP_T = 8.0         # end stop thickness (x < 0)
STOP_UP = 6.0        # stop rises above the rod tops
X_END = 212.0        # base ends past the last slot
SLOT_W = 1.8         # saw kerf guide (hacksaw ~1.0, diamond disc ~0.6-1.5)
SLOT_CLEAR = 2.0     # slot floor this far below the deepest rod bottom
RAIL_SHOULDER = 1.5  # Ø2 rail: flat each side of its groove
EDGE_W = 10.0        # outer strips (screw lands), each side
SCREW_IN = 5.6       # screw axis from the body edge
LBL_W = 6.0          # label strip on the −y side of every groove
FL_W, FL_T = 12.0, 5.0              # side flanges
FL_HOLES_X = [30.0, 122.0, 195.0]   # Ø4.5 wood-screw holes
LID_LX = 16.0        # lid length along the rod
LID_T = 8.0
STATIONS = [("A", 10.5), ("B", 70.0)]  # lid centre x; A clears the shortest slot, B is for rods >= 85
M3_CLR, M3_CSK_D = 3.5, 6.5    # clearance +0.5 (P1S/PETG policy); countersink 90 deg, DIN 7991 head Ø6
CH = 1.0             # 45 deg chamfer on every outside edge (no sharp corners; bed edges print without overhang)
NUT_AF, NUT_DEPTH = 5.8, 5.4   # trap AF as the headrest; pocket 5.4 deep so the nut sits 2.2 mm up (screw tip never pokes out)
TXT_H, TXT_DEPTH = 3.6, 0.6
MIN_WALL = 1.2
ROD_TOP = H + PROUD


def lane_geom(d):
    """Groove for rod Ø d with its top at ROD_TOP: centre z, apex z, surface z, half-width at surface, lip chamfer."""
    r = d / 2
    zc = ROD_TOP - r
    z_apex = zc - r * math.sqrt(2)
    if z_apex < H - 3.0:          # deep enough in the base top: groove cut from the top face
        z_surf, c = H, CH
    else:                          # too small (Ø2): groove sits in a rail up to the rod centre
        z_surf, c = zc, 0.4
    return dict(d=d, r=r, zc=zc, z_apex=z_apex, z_surf=z_surf, half=z_surf - z_apex, c=c)


def layout():
    """y positions of every lane; body width W."""
    y = EDGE_W
    out = []
    for name, d, cuts in LANES:
        g = lane_geom(d)
        g.update(name=name, cuts=cuts, y_lbl=y + LBL_W / 2)
        y += LBL_W
        rail = g["z_surf"] > H
        band = 2 * (g["half"] + g["c"] + (RAIL_SHOULDER if rail else 0))
        g.update(y0=y, y1=y + band, y=y + band / 2, rail=rail)
        y += band
        out.append(g)
    y += EDGE_W
    for g in out:                                   # centre the body on y = 0
        for k in ("y_lbl", "y0", "y1", "y"):
            g[k] -= y / 2
    return out, y


LAN, W = layout()

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




def text_width(s, size=TXT_H):
    b = text_cs(s, size).bounds()
    return b[2] - b[0]


def engrave_left(part, s, x_left, y_center, z_top, size=TXT_H):
    return engrave(part, s, x_left + text_width(s, size) / 2, y_center, z_top, size)


def fmt(L):
    return ("%.1f" % L).rstrip("0").rstrip(".")


def slot_labels():
    """(text, x_left, x_right, y) per slot: just past the slot, or before it if that runs off the end."""
    out = []
    for g in LAN:
        for tag, L in g["cuts"]:
            s = "%s %s" % (tag, fmt(L))
            w = text_width(s)
            x0 = L + SLOT_W + 1.5
            if x0 + w > X_END - 2.5:
                x0 = L - 1.5 - w
            out.append((s, x0, x0 + w, g["y_lbl"], g["name"]))
    return out


def all_lengths():
    return sorted({L for g in LAN for _, L in g["cuts"]})


def screw_ys():
    return (-W / 2 + SCREW_IN, W / 2 - SCREW_IN)


def _lip_chamfer(g):
    # two 45-deg chamfers along the groove lips (convex prisms), like v1
    out = None
    zs, c, yc = g["z_surf"], g["c"], g["y"]
    for sgn in (-1, 1):
        y = yc + sgn * g["half"]
        tri = [(y - sgn * c, zs - c), (y + sgn * c, zs + 0.001), (y + sgn * c, zs + 3), (y - sgn * 3, zs + 3)]
        if sgn < 0:
            tri = tri[::-1]
        p = _prism_x(tri, 0, X_END + 1)
        out = p if out is None else out + p
    return out


def base():
    b = cbox(-STOP_T, X_END, -W / 2, W / 2, 0, H)
    b += cbox(-STOP_T, 0, -W / 2, W / 2, H - 2 * CH, ROD_TOP + STOP_UP)            # shared end stop
    b += cbox(-STOP_T, X_END, -W / 2 - FL_W, W / 2 + FL_W, 0, FL_T)                # flanges
    z_land = ROD_TOP - LAND_GAP
    for y0, y1 in ((-W / 2, -W / 2 + EDGE_W - 0.6), (W / 2 - EDGE_W + 0.6, W / 2)):  # screw lands (lid stays level); 0.6 off the lips
        b += cbox(0.0 + 0.5, X_END, y0, y1, H - 2, z_land)
    for g in LAN:                                                                  # Ø2 rail
        if g["rail"]:
            b += cbox(0.0 - 0.5, X_END, g["y0"], g["y1"], H - 2, g["z_surf"])
    for g in LAN:                                                                  # V-grooves
        yc, hf, zs = g["y"], g["half"], g["z_surf"]
        b -= _prism_x([(yc - hf, zs + 5), (yc - hf, zs + 0.001), (yc, g["z_apex"]), (yc + hf, zs + 0.001), (yc + hf, zs + 5)], 0, X_END + 1)
        b -= _lip_chamfer(g)
    z_floor = min(g["zc"] - g["r"] for g in LAN) - SLOT_CLEAR
    for L in all_lengths():                                                        # saw slots, near edge at L, full width
        b -= box(L, L + SLOT_W, -W / 2 - 1, W / 2 + 1, z_floor, ROD_TOP + 1)
    for _, xs in STATIONS:                                                         # M3 holes + nut traps from below
        for y in screw_ys():
            b -= cyl_z(xs, y, M3_CLR, -1, ROD_TOP + 1)
            b -= hex_z(xs, y, NUT_AF, -1, NUT_DEPTH)
    for x in FL_HOLES_X:
        for y in (-(W / 2 + FL_W / 2), W / 2 + FL_W / 2):
            b -= cyl_z(x, y, 4.5, -1, FL_T + 1)
    for s, x0, _, y, _ in slot_labels():                                           # slot labels in each lane's strip
        b = engrave_left(b, s, x0, y, H)
    for g in LAN:                                                                  # lane diameters on top of the stop
        b = engrave(b, g["name"], -STOP_T / 2, g["y"], ROD_TOP + STOP_UP, size=4.0, rot=90)
    return b


def lid(x_centre=STATIONS[0][1]):
    z0 = ROD_TOP
    x0, x1 = x_centre - LID_LX / 2, x_centre + LID_LX / 2
    l = cbox(x0, x1, -W / 2, W / 2, z0, z0 + LID_T)
    for y in screw_ys():
        l -= cyl_z(x_centre, y, M3_CLR, z0 - 1, z0 + LID_T + 1)
        l -= cone_z(x_centre, y, M3_CLR, M3_CSK_D + 0.4, z0 + LID_T - (M3_CSK_D + 0.4 - M3_CLR) / 2, z0 + LID_T + 0.01)
    l = engrave(l, "ROD CLAMP", x_centre, 0, z0 + LID_T, size=3.6, rot=90)
    return l


def rod(g, L):
    return M.cylinder(L, g["r"], g["r"], 48).rotate([0, 90, 0]).translate([0, g["y"], g["zc"]])


def checks(B):
    print("body W %.1f mm (with flanges %.1f), length %.1f" % (W, W + 2 * FL_W, X_END + STOP_T))
    for g in LAN:
        print("lane %s: y %.2f, groove %.2f..%.2f, rod top z %.2f, apex z %.2f, surface z %.2f, contact z %.2f%s"
              % (g["name"], g["y"], g["y0"], g["y1"], g["zc"] + g["r"], g["z_apex"], g["z_surf"],
                 g["zc"] - g["r"] / math.sqrt(2), " (rail)" if g["rail"] else ""))
        assert abs(g["zc"] + g["r"] - ROD_TOP) < 1e-9
    # lanes don't collide; walls between grooves / screw holes / edges >= MIN_WALL at the top face
    walls = []
    ys = screw_ys()
    walls.append(("edge -> screw hole", ys[0] - M3_CLR / 2 + W / 2))
    walls.append(("nut trap -> edge (bottom, after 1 mm chamfer)", ys[0] + W / 2 - NUT_AF / 2 - CH))
    prev = ("screw hole", ys[0] + M3_CLR / 2)
    for g in LAN:
        lo, hi = g["y"] - g["half"] - g["c"], g["y"] + g["half"] + g["c"]
        walls.append(("%s -> lane %s lip" % (prev[0], g["name"]), lo - prev[1]))
        prev = ("lane %s lip" % g["name"], hi)
    walls.append(("%s -> screw hole" % prev[0], ys[1] - M3_CLR / 2 - prev[1]))
    Ls = all_lengths()
    for a, b_ in zip(Ls, Ls[1:]):
        walls.append(("rib between slots %s/%s" % (fmt(a), fmt(b_)), b_ - (a + SLOT_W)))
    walls.append(("end block past slot %s" % fmt(Ls[-1]), X_END - Ls[-1] - SLOT_W))
    z_floor = min(g["zc"] - g["r"] for g in LAN) - SLOT_CLEAR
    walls.append(("slot floor above nut traps", z_floor - NUT_DEPTH))
    walls.append(("deepest groove apex above nut traps", min(g["z_apex"] for g in LAN) - NUT_DEPTH))
    for n, w in walls:
        print("  wall %-45s %6.2f %s" % (n, w, "ok" if w >= MIN_WALL else "TOO THIN"))
        assert w >= MIN_WALL, n
    # slots vs clamp stations: no slot under a lid footprint; station A clears the shortest slot
    for name, xs in STATIONS:
        a, b_ = xs - LID_LX / 2, xs + LID_LX / 2
        bad = [L for L in Ls if L + SLOT_W > a - 1 and L < b_ + 1]
        print("  station %s lid x %.1f..%.1f, slots under it: %s" % (name, a, b_, bad or "none"))
        assert not bad
    assert STATIONS[0][1] + LID_LX / 2 + 2.0 <= Ls[0], "station A must clear the shortest slot"
    assert STATIONS[0][1] - LID_LX / 2 >= 1.0
    # labels: same strip must not overlap; inside the body
    labs = slot_labels()
    for i, (s, a, b_, y, ln) in enumerate(labs):
        assert a > 0.5 and b_ < X_END - 1.5, s
        for s2, a2, b2, y2, ln2 in labs[i + 1:]:
            if ln == ln2:
                assert b_ + 1.0 < a2 or b2 + 1.0 < a, (s, s2)
        print("  label %-10s lane %s x %.1f..%.1f" % (s, ln, a, b_))
    # the lid clamps every lane: rod tops all at ROD_TOP, lid bottom there, lid spans every lane
    Lid = lid(STATIONS[1][1])
    for g in LAN:
        Lmax = max(L for _, L in g["cuts"])
        Rd = rod(g, Lmax + 10)
        ob, ol = (B ^ Rd).volume(), (Lid ^ Rd).volume()
        print("  lane %s rod overlap: base %.4f mm3 (want ~0), lid %.4f mm3 (lid touches the top)" % (g["name"], ob, ol))
        assert ob < 0.5 and ol < 0.5
    for g in LAN:
        assert -W / 2 < g["y0"] and g["y1"] < W / 2
    print("  lid bottom z %.2f = rod tops; lands z %.2f (lid flexes %.1f mm = preload); Ø2 rail top z %.2f"
          % (ROD_TOP, ROD_TOP - LAND_GAP, LAND_GAP, max(g["z_surf"] for g in LAN)))
    tip_z = ROD_TOP + LID_T - 30.0
    print("  M3x30 tip at z %.2f clamped, %.2f lowest (lid on the lands); nut (2.4 thick) at z %.1f-%.1f"
          % (tip_z, tip_z - LAND_GAP, NUT_DEPTH - 2.4, NUT_DEPTH))
    assert tip_z - LAND_GAP > 0.5 and tip_z < NUT_DEPTH - 2.4


def to_trimesh(m):
    import trimesh
    mesh = m.to_mesh()
    return trimesh.Trimesh(mesh.vert_properties[:, :3], mesh.tri_verts)


def render_png(B, path):
    """Review picture: iso view with a rod in every lane + the lid at station B, and the lane section."""
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    from mpl_toolkits.mplot3d.art3d import Poly3DCollection
    from matplotlib.patches import Polygon as PPoly, Circle
    fig = plt.figure(figsize=(13, 10), dpi=110)
    ax = fig.add_axes([-0.08, 0.28, 1.16, 0.74], projection="3d")
    light = np.array([0.35, -0.45, 0.82]); light /= np.linalg.norm(light)
    tris, cols = [], []

    def add(m, rgb):                     # one collection for everything, so faces sort across parts
        import trimesh
        t = to_trimesh(m)
        v, f = trimesh.remesh.subdivide_to_size(t.vertices, t.faces, max_edge=6.0)
        t = trimesh.Trimesh(v, f)
        sh = 0.40 + 0.60 * np.clip(t.face_normals @ light, 0, 1)
        tris.append(t.vertices[t.faces]); cols.append(np.c_[np.outer(sh, rgb), np.ones(len(sh))])
    add(B, np.array([0.86, 0.86, 0.83]))
    show = {"Ø6": POMMEL_K23_L + 40, "Ø2": SIDEPAD_PIN2_L, "Ø8": 150}
    for g in LAN:
        add(rod(g, show[g["name"]]), np.array([0.15, 0.15, 0.17]))
    add(lid(STATIONS[1][1]), np.array([0.45, 0.62, 0.82]))
    ax.add_collection3d(Poly3DCollection(np.concatenate(tris), facecolors=np.concatenate(cols), edgecolors="none"))
    ax.set_xlim(-10, 215); ax.set_ylim(-60, 60); ax.set_zlim(0, 40)
    ax.set_box_aspect((225, 120, 40), zoom=1.05); ax.view_init(elev=32, azim=-58); ax.set_axis_off()
    ax.set_title("Rod cutting gauge v2: lanes Ø6 / Ø2 / Ø8 against one stop; one lid (shown at station B) clamps any lane", fontsize=12)
    # section at x = 40 through all lanes, lid on
    ax2 = fig.add_axes([0.06, 0.03, 0.58, 0.29])
    bx = to_trimesh(B).section(plane_origin=[40, 0, 0], plane_normal=[1, 0, 0])
    lx = to_trimesh(lid(40.0)).section(plane_origin=[40, 0, 0], plane_normal=[1, 0, 0])
    for s, col in ((bx, "#d8d8d2"), (lx, "#8fb0d0")):
        p2, _ = s.to_2D(to_2D=np.array([[0, 1, 0, 0], [0, 0, 1, 0], [-1, 0, 0, 40], [0, 0, 0, 1]], float), check=False)
        for poly in p2.polygons_full:
            ax2.add_patch(PPoly(np.array(poly.exterior.coords), fc=col, ec="k", lw=0.6))
            for h in poly.interiors:
                ax2.add_patch(PPoly(np.array(h.coords), fc="white", ec="k", lw=0.6))
    for g in LAN:
        ax2.add_patch(Circle((g["y"], g["zc"]), g["r"], fc="#222", ec="none"))
        ax2.text(g["y"], ROD_TOP + LID_T + 2.5, g["name"], ha="center", fontsize=10)
    ax2.axhline(ROD_TOP, color="r", lw=0.6, ls="--")
    ax2.text(W / 2 + 2, ROD_TOP, "rod tops z %.0f" % ROD_TOP, color="r", fontsize=8, va="center")
    ax2.set_aspect("equal"); ax2.set_xlim(-W / 2 - FL_W - 2, W / 2 + FL_W + 22); ax2.set_ylim(-1, ROD_TOP + LID_T + 7)
    ax2.set_title("Section at x 40 with the lid in place (all rod tops at one height), mm", fontsize=9)
    ax2.tick_params(labelsize=7)
    ax3 = fig.add_axes([0.68, 0.03, 0.30, 0.29]); ax3.set_axis_off()
    lines = ["Slots (near edge = length, mm)"]
    for g in LAN:
        lines.append("%s: " % g["name"] + ", ".join("%s %s" % (t, fmt(L)) for t, L in g["cuts"]))
    lines += ["", "Lid: 2x M3x30 + 2 nuts", "Station A (x %.1f): every length" % STATIONS[0][1],
              "Station B (x %.0f): rods 85 mm and longer" % STATIONS[1][1]]
    ax3.text(0, 1, "\n".join(lines), va="top", fontsize=9, family="DejaVu Sans")
    fig.savefig(path, bbox_inches="tight", pad_inches=0.15); plt.close(fig)
    print("wrote", path)


if __name__ == "__main__":
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, "..", "stl", "tools")
    png = sys.argv[2] if len(sys.argv) > 2 else os.path.join(here, "..", "review", "rod_cutting_gauge.png")
    os.makedirs(out, exist_ok=True)
    B, Lid = base(), lid()
    for n, p in (("base", B), ("lid", Lid)):
        assert p.status().name == "NoError" and not p.is_empty(), n
        print(n, "volume %.1f cm3, genus %d" % (p.volume() / 1000, p.genus()))
    checks(B)

    def save(m, name):
        t = to_trimesh(m)
        t.apply_translation([0, 0, -t.bounds[0][2]])   # lid prints with its top (countersinks) up: already so
        t.export(os.path.join(out, name)); print("wrote", name, t.bounds[1] - t.bounds[0])
    save(B, "rod_cutting_gauge_base.stl")
    save(Lid, "rod_cutting_gauge_lid.stl")
    render_png(B, png)
