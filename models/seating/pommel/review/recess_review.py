#!/usr/bin/env python3
# usage: python3 recess_review.py <out_dir>   (needs numpy, scipy, matplotlib)
# Re-model (Python, NOT Onshape) of the seat-pan top around the pommel mount: old 6 mm recess vs new 1.5 mm ramped recess,
# simplified S23A saddle (no fillets). Heightfield approach. Writes PNGs + prints check numbers.
import numpy as np, matplotlib; matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.path import Path
from scipy.interpolate import CubicSpline
from scipy.spatial import cKDTree
import sys
OUT = sys.argv[1]
SAD_REAR, SAD_HW_REAR, HOOK_HW, PAN_FRONT = -36, 15, 40, -96

def spl(pts, n=400, bc="not-a-knot"):
    p = np.array(pts, float); t = np.r_[0, np.cumsum(np.linalg.norm(np.diff(p, axis=0), axis=1))]
    cs = CubicSpline(t, p, bc_type=bc); return cs(np.linspace(0, t[-1], n))

def new_outline(o):
    k, yF = 1.09, PAN_FRONT + 3
    right = [(HOOK_HW + o, yF), ((SAD_HW_REAR + HOOK_HW) / 2 + k * o, (SAD_REAR + yF) / 2), (SAD_HW_REAR + 1 + k * o, SAD_REAR + o), (0, SAD_REAR + o + 0.5)]
    pts = right + [(-x, y) for x, y in reversed(right[:-1])]
    p = np.array(pts, float); t = np.r_[0, np.cumsum(np.linalg.norm(np.diff(p, axis=0), axis=1))]
    d0 = np.array([0, 1.0]) * 40 / 1; d1 = np.array([0, -1.0]) * 40
    cs = CubicSpline(t, p, bc_type=((1, d0 * (t[-1] / 40) / t[-1] * 1), (1, d1 / 40)))   # unit-ish tangents (shape approx only)
    c = cs(np.linspace(0, t[-1], 600))
    return np.vstack([[[HOOK_HW + o, -112]], c, [[-(HOOK_HW + o), -112]]])

def old_outline():
    c = 0.5
    half = [(SAD_HW_REAR + c, SAD_REAR + c), ((SAD_HW_REAR + HOOK_HW) / 2 + c, (SAD_REAR - 93) / 2), (HOOK_HW + c, -93)]
    r = spl(half, 300); l = r * [-1, 1]
    return np.vstack([[[HOOK_HW + c, -110]], r[::-1], l, [[-(HOOK_HW + c), -110]]])

def saddle_outline(code="S23A"):
    HF = -99; b = 171
    half = [(15, -36), (27.5, -64.5), (40, -93), (37, HF - 2), (30, HF - 14), (35, -(b - 12))]
    return spl(half, 500)

def sdist(poly, X, Y):
    path = Path(np.vstack([poly, poly[:1]])); pts = np.c_[X.ravel(), Y.ravel()]
    inside = path.contains_points(pts)
    dense = np.vstack([np.linspace(poly[i], poly[(i + 1) % len(poly)], 8, endpoint=False) for i in range(len(poly))])
    d, _ = cKDTree(dense).query(pts)
    return np.where(inside, -d, d).reshape(X.shape)

def lip(y):  # dummy pan front lip: R16 rounding from y -80 to the -96 face (chair notes: z 16 at y -90)
    yy = np.clip(y, -96, -80); return np.where(y > -80, 20.0, 4 + np.sqrt(np.maximum(0, 256 - (yy + 80) ** 2)))

def blend_profile(s, depth=1.5, run=6.0, r_rim=20.0, r_floor=3.0):
    # z of the new recess vs signed distance s from the floor outline (s<0 inside): flat floor, 14 deg ramp, filleted both ends
    th = np.arctan(depth / run); z = np.where(s <= 0, 20 - depth, np.where(s >= run, 20.0, 20 - depth + s * depth / run))
    tf, tr = r_floor * np.tan(th / 2), r_rim * np.tan(th / 2)
    m = np.abs(s) < tf;  u = s[m] + tf
    z[m] = 20 - depth + r_floor - np.sqrt(np.maximum(0, r_floor ** 2 - u ** 2))
    m = np.abs(s - run) < tr; u = (run + tr) - s[m]
    z[m] = 20 - r_rim + np.sqrt(np.maximum(0, r_rim ** 2 - u ** 2))
    return z

x = np.linspace(-70, 70, 561); y = np.linspace(-100, -10, 361); X, Y = np.meshgrid(x, y)
base = np.minimum(20.0, lip(Y)); base[Y < -96] = np.nan
yF = -93; cut = (Y < yF) & (np.abs(X) < 40.5 + (yF - Y))         # hook-seat cut (45 deg flared ends in the new one)
cut_old = (Y < yF) & (np.abs(X) < 40.5)
o0 = new_outline(0.5); s_new = sdist(o0, X, Y)
z_new = np.minimum(base, blend_profile(s_new)); z_new[cut] = np.nan
po = old_outline(); s_old = sdist(po, X, Y)
z_old = np.where(s_old <= 0, np.minimum(base, 14.0), base); z_old[cut_old] = np.nan

# ---- checks ----
sad = saddle_outline(); sadP = np.vstack([sad, (sad * [-1, 1])[::-1]])
o_clip = new_outline(0.0)                       # pommel saddle is clipped to recessSketch(o = 0)
def tree(poly): return cKDTree(np.vstack([np.linspace(poly[i], poly[(i + 1) % len(poly)], 8, endpoint=False) for i in range(len(poly))]))
def contain_gap(inner, outer, ymin=-93):
    pin = inner[inner[:, 1] >= ymin]; outside = (~Path(outer).contains_points(pin)).sum(); d, _ = tree(outer).query(pin)
    return outside, d.min()
n_out, g = contain_gap(o_clip, o0); print("new: clipped saddle edge -> recess floor outline: %d points outside, min gap %.2f mm (floor blend r3 needs %.2f)" % (n_out, g, 3 * np.tan(np.arctan(1.5 / 6) / 2)))
n_out, g = contain_gap(o_clip, o_clip); print("control (same outline): %d outside of %d, min gap %.2f" % (n_out, (o_clip[:, 1] >= -93).sum(), g))
n_out, g = contain_gap(sadP, po); print("old design, unclipped saddle vs old recess wall (scipy splines): %d points outside" % n_out)
sadP = sadP  # renders: saddle footprint = unclipped plan AND clip outline
clipPath = Path(o_clip)
inside_sad = Path(sadP).contains_points(np.c_[X.ravel(), Y.ravel()]).reshape(X.shape) & clipPath.contains_points(np.c_[X.ravel(), Y.ravel()]).reshape(X.shape) & (Y >= -88.5)
print("new: max seat z under the saddle floor: %.3f (saddle floor 18.5)" % np.nanmax(z_new[inside_sad]))
# hook chamfer vs lip: line z = 11 + (y + 96) from y -93 to -88.5
yy = np.linspace(-93, -88.5, 50); print("hook chamfer clearance over lip, min (mm): %.2f" % (11 + yy + 96 - lip(yy)).min())
# feel: max slope and depth of the seat without the pommel
gy, gx = np.gradient(z_new, y, x); sl = np.degrees(np.arctan(np.hypot(gx, gy))); sl[(Y < -80) | np.isnan(sl)] = 0
print("new: max seat-top slope behind the lip (deg): %.1f, depth %.2f mm, recess footprint area %.0f cm2" % (np.nanmax(sl), 20 - np.nanmin(z_new[Y > -80]), np.sum(z_new[Y > -80] < 19.99) * (x[1] - x[0]) * (y[1] - y[0]) / 100))
gy, gx = np.gradient(z_old, y, x); so = np.degrees(np.arctan(np.hypot(gx, gy))); so[(Y < -80) | np.isnan(so)] = 0
print("old: max slope (deg) %.0f (vertical wall), depth %.1f mm" % (np.nanmax(so), 20 - np.nanmin(z_old[Y > -80])))

# ---- pictures ----
def saddle_top(yv):
    pts = [(-36, 25.2), (-55, 26.4), (-75, 27.8), (-91, 29), (-99, 30), (-109, 42), (-124, 70)]
    return spl(pts, 400)
def saddle_top_old():
    return spl([(-36, 21.5), (-55, 22.5), (-75, 25), (-91, 29), (-99, 30), (-109, 42), (-124, 70)], 400)
def pommel_field(top, F):
    T = np.interp(Y, top[::-1, 0], top[::-1, 1]); ins = Path(sadP).contains_points(np.c_[X.ravel(), Y.ravel()]).reshape(X.shape)
    if F > 15: ins &= clipPath.contains_points(np.c_[X.ravel(), Y.ravel()]).reshape(X.shape) | (Y < -93)
    Z = np.where(ins & (Y >= -96), T, np.nan); return Z
fig = plt.figure(figsize=(14, 10))
for i, (Z, P, t) in enumerate([(z_old, None, "BEFORE: 6 mm recess, square edges (no pommel)"), (z_new, None, "AFTER: 1.5 mm recess, 14 deg ramp, r20 rim / r3 floor (no pommel)"),
                               (z_old, pommel_field(saddle_top_old(), 14), "BEFORE: with pommel saddle (simplified)"), (z_new, pommel_field(saddle_top(0), 18.5), "AFTER: with pommel saddle (simplified)")]):
    ax = fig.add_subplot(2, 2, i + 1, projection="3d"); ax.computed_zorder = False
    ax.plot_surface(X, Y, Z, cmap="copper", vmin=12, vmax=21, rstride=3, cstride=3, linewidth=0, antialiased=True, shade=True)
    if P is not None:
        ax.plot_surface(X, Y, P, color="#2f7fc1", rstride=3, cstride=3, linewidth=0, alpha=0.95, shade=True)
    ax.set_box_aspect((140, 90, 30)); ax.view_init(38, -62); ax.set_zlim(4, 34); ax.set_title(t, fontsize=10)
    ax.set_xlabel("x"); ax.set_ylabel("y (- = front)"); ax.set_zlabel("z")
fig.suptitle("GbG chair seat top at the pommel mount — Python re-model (not Onshape), dummy pan with R16 front lip", fontsize=11)
fig.tight_layout(); fig.savefig(f"{OUT}/seat_recess_before_after_3d.png", dpi=110)

fig, axs = plt.subplots(2, 1, figsize=(10, 7))
for ax, yq in zip(axs, (-50, -75)):
    j = np.argmin(abs(y - yq))
    ax.plot(x, z_old[j], color="#b45309", lw=2, label="before: 6 mm recess (seat only)")
    ax.plot(x, z_new[j], color="#2f7fc1", lw=2, label="after: 1.5 mm ramped recess (seat only)")
    for top, F, c, lab in ((saddle_top_old(), 14, "#b45309", "before: saddle"), (saddle_top(0), 18.5, "#2f7fc1", "after: saddle")):
        T = np.interp(yq, top[::-1, 0], top[::-1, 1]); hw = np.interp(yq, sad[::-1, 1], sad[::-1, 0])
        if F > 15: hw = min(hw, np.interp(yq, o_clip[o_clip[:, 0] > 0][::-1, 1], o_clip[o_clip[:, 0] > 0][::-1, 0]))
        ax.fill([-hw, hw, hw, -hw], [F, F, T, T], color=c, alpha=0.25, label=lab)
    ax.axhline(30, color="#c2410c", ls="--", lw=1); ax.text(-68, 30.3, "keep-low limit: 10 mm above the seat", color="#c2410c", fontsize=8)
    ax.set_xlim(-70, 70); ax.set_ylim(12, 32); ax.set_aspect("equal"); ax.set_title(f"Cross-section at y = {yq} (saddle shown without its 10 mm rounding)", fontsize=10)
    ax.set_xlabel("x (mm)"); ax.set_ylabel("z (mm)"); ax.grid(alpha=0.3)
axs[0].legend(fontsize=8, loc="upper right", ncol=2)
fig.tight_layout(); fig.savefig(f"{OUT}/seat_recess_sections.png", dpi=110)
print("wrote", OUT)
