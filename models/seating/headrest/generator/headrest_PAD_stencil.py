# Printed TEMPLATE PART for the foam lining: trace round it, punch through its guide holes, mark the dot centres.
import os, math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_PAD_foam as Q
TH = 2.4
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "padding")
def stencil():
    polys, pats = Q.template_polys()
    areas = [abs(CS([p.tolist()]).area()) for p in polys]; i0 = int(np.argmax(areas))
    outer = CS([polys[i0].tolist()]); outer = outer if outer.area() > 0 else CS([polys[i0][::-1].tolist()])
    holes = CS()
    for i, p in enumerate(polys):
        if i == i0: continue
        c = CS([p.tolist()]); c = c if c.area() > 0 else CS([p[::-1].tolist()])
        if c.area() < 120: continue                      # the Ø9 screwdriver holes: replaced by exact punch guides below
        holes = holes + c                                # strap slots: trace inside
    for x, z in Q.punch_centres(): holes = holes + CS.circle(4.85, 48).translate([x, z])      # Ø9.7: the Ø9 punch goes straight through
    for x, z in Q.dot_centres(): holes = holes + CS.circle(2.0, 32).translate([x, z])        # Ø4 pen mark = dot centre
    b = outer.bounds() if hasattr(outer, "bounds") else None
    allp = polys[i0]; zt, zb = allp[:, 1].max(), allp[:, 1].min()
    notch = CS([[[-3, zt + 1], [3, zt + 1], [0, zt - 3.5]]]) + CS([[[-3, zb - 1], [0, zb + 3.5], [3, zb - 1]]])          # centre-line notches
    arrow = CS([[[-5, zt - 16], [5, zt - 16], [0, zt - 8]]])                                                                 # "this way = top of the head"
    plate = (outer - holes - notch - arrow).extrude(TH)
    rings = CS()
    for x, z in Q.dot_centres(): rings = rings + (CS.circle(Q.DOT_D / 2, 64) - CS.circle(Q.DOT_D / 2 - 1.0, 64)).translate([x, z])
    plate = plate - rings.extrude(0.8).translate([0, 0, TH - 0.8])                                                           # engraved Ø25 dot outline on top
    return plate
if __name__ == "__main__":
    import trimesh
    s = stencil(); b = s.bounding_box(); me = s.to_mesh()
    trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts)).export(os.path.join(OUT, "foam_template_part.stl"))
    print(s.status(), "%.1f cm3" % (s.volume() / 1000), "size %.0f x %.0f x %.1f" % (b[3] - b[0], b[4] - b[1], b[5] - b[2]))
