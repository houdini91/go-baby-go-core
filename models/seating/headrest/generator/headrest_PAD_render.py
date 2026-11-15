import os, sys, numpy as np
sys.path.insert(0, os.path.dirname(__file__))
import headrest_PAD_foam as Q, headrest_F_flow as F, headrest_E4_geom as G, headrest_P1_pad as P1, headrest_SW_mount as W
from manifold3d import Manifold as M, CrossSection as CS
from zrender import ZScene as Scene
import matplotlib; matplotlib.use("Agg"); import matplotlib.pyplot as plt
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "padding", "img"); os.makedirs(OUT, exist_ok=True)
T = G.tris
C = dict(pad="#4a90d9", foam="#3a3f47", hook="#f2b632", head="#e8c4a8", mast="#4a90d9")
def o(n): return os.path.join(OUT, n + ".png")
def renders():
    for var, t in (("S", "today: 5 slits for foam straps"), ("V", "new: no slits, clean back")):
        s = Scene(); s.add(T(Q.pad(var)), C["pad"]); s.add(T(F.mast()), C["mast"])
        s.render(o(f"back_{var}"), az=90, el=12, size=(4.6, 4.2), dpi=115, focus=((0, 95, 0), 125), title=t)
    s = Scene(); s.add(T(Q.pad("V")), C["pad"]); s.add(T(Q.hooks()), C["hook"])
    s.render(o("inside_hooks"), az=-90, el=20, size=(5.0, 4.4), dpi=115, focus=((0, 60, 5), 140), title="inside: 7 hook patches sit in 1.5 mm recesses")
    s = Scene(); s.add(T(Q.pad("V")), C["pad"]); s.add(T(Q.hooks()), C["hook"]); s.add(T(Q.foam().translate([0, -45, 0])), C["foam"])
    s.render(o("exploded"), az=-125, el=18, size=(5.0, 4.6), dpi=115, focus=((0, 40, 5), 170), title="foam off: loop patches on its back press onto the hooks")
    s = Scene(); s.add(T(Q.pad("V")), C["pad"]); s.add(T(Q.foam()), C["foam"]); s.add(T(F.mast()), C["mast"])
    s.render(o("fitted"), az=-125, el=14, size=(5.0, 4.6), dpi=115, focus=((0, 60, 0), 150), title="fitted: foam edge 3 mm inside the shell edge")
    s = Scene(); s.add(T(Q.pad("V")), C["pad"]); s.add(T(Q.foam()), C["foam"]); s.add(T(P1.head("big")), C["head"], 0.35)
    s.render(o("with_head"), az=-140, el=10, size=(5.0, 4.6), dpi=115, focus=((0, 40, 0), 190), title="with the 5-year-old head")
    s = Scene(); s.add(T(Q.pad("V")), C["pad"]); s.add(T(Q.foam()), C["foam"])
    s.render(o("edge_close"), az=-150, el=35, size=(4.6, 4.2), dpi=115, focus=((-60, 40, 30), 70), title="edge detail: wing, strap slot, rounded foam edge")
def template():
    polys, pats = Q.template_polys()
    W_, H_ = 297.0, 210.0
    fig = plt.figure(figsize=(W_ / 25.4, H_ / 25.4)); ax = fig.add_axes([0, 0, 1, 1]); ax.set_xlim(0, W_); ax.set_ylim(0, H_); ax.axis("off")
    allp = np.concatenate(polys); cx = (allp[:, 0].min() + allp[:, 0].max()) / 2; cz = (allp[:, 1].min() + allp[:, 1].max()) / 2
    ox, oz = W_ / 2 - cx, H_ / 2 - cz + 8
    for i, p in enumerate(polys):
        ax.plot(np.r_[p[:, 0], p[0, 0]] + ox, np.r_[p[:, 1], p[0, 1]] + oz, color="k", lw=0.6)
    for p in pats:
        ax.fill(p[:, 0] + ox, p[:, 1] + oz, fc="none", ec="#c27c0e", lw=0.5, ls=(0, (3, 2)))
    ax.plot([ox, ox], [allp[:, 1].min() + oz - 4, allp[:, 1].max() + oz + 4], color="#888", lw=0.3, ls=(0, (6, 3)))
    ax.text(ox, allp[:, 1].max() + oz + 6, "centre - TOP of the head", ha="center", fontsize=6)
    ax.add_patch(plt.Rectangle((12, 12), 50, 50, fill=False, lw=0.6)); ax.text(37, 37, "50 x 50 mm\ncheck before cutting", ha="center", va="center", fontsize=5.5)
    ax.text(W_ - 12, 14, "GbG headrest F3 - foam lining, 10 mm EVA (8-12 mm OK)\nPrint at 100% / actual size, A4 landscape. Cut on the solid line, knife vertical.\nDashed = loop patches (on the BACK of the foam, the side that faces the shell).\nSlots = forehead strap; round holes = screwdriver access to the button mount.",
            ha="right", va="bottom", fontsize=5.5)
    fig.savefig(os.path.join(OUT, "..", "foam_template_A4.pdf")); fig.savefig(o("template"), dpi=110); plt.close(fig)
    # printed stencil (trace with a pen, 2 mm PLA/PETG)
    cs = CS([p.tolist() for p in polys])
    if cs.area() < 0: cs = CS([p[::-1].tolist() for p in polys])
    st = cs.extrude(2.0)
    st.to_mesh()
    import trimesh
    me = st.to_mesh(); tm = trimesh.Trimesh(np.asarray(me.vert_properties)[:, :3], np.asarray(me.tri_verts))
    tm.export(os.path.join(OUT, "..", "foam_stencil_2mm.stl")); print("stencil", st.volume() / 1000, "cm3", st.bounding_box())
if __name__ == "__main__":
    template(); print("template ok", flush=True)
    if "t" not in sys.argv: renders(); print("renders ok")
