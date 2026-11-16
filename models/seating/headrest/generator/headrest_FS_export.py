# Exports the data the Onshape FeatureScript port needs that is NOT a closed-form formula:
#   the W1 button-pod outer hull (convex hull of the surface footprint band + the PVC collar), as planar sections
#   perpendicular to the collar axis, one set per yaw (right side; the FS mirrors for the left).
import math, numpy as np
from manifold3d import Manifold as M, CrossSection as CS
import headrest_SW_mount as W, headrest_F_flow as F
def rot_to_z(d):
    d = np.asarray(d, float); d /= np.linalg.norm(d); z = np.array([0, 0, 1.0])
    v = np.cross(d, z); s = np.linalg.norm(v); c = d @ z
    if s < 1e-9: return np.eye(3)
    vx = np.array([[0, -v[2], v[1]], [v[2], 0, -v[0]], [-v[1], v[0], 0]])
    return np.eye(3) + vx + vx @ vx * ((1 - c) / s ** 2)
def pod_hull(yaw):
    E, d = W.axis("W1", yaw, 1)
    collar = W.cyl_along(E - d * (W.SOCK + 4.0), d, W.SOCK + 4.0, 13.5, 96)
    return M.batch_hull([W.skin(W.footprint("W1"), 0.3, 3.0), collar]), E, d
def sections(yaw, n=16, npts=40):
    h, E, d = pod_hull(yaw); Rm = rot_to_z(d)
    v = np.asarray(h.to_mesh().vert_properties)[:, :3]
    hz = M.hull_points(((v - E) @ Rm.T).tolist())
    b = hz.bounding_box(); z0, z1 = b[2] + 1.5, b[5]
    out = []
    for z in np.linspace(z0, z1 - 1.5, n):
        polys = hz.slice(z).to_polygons(); p = np.asarray(polys[0])
        c = p.mean(0); ang = np.arctan2(p[:, 1] - c[1], p[:, 0] - c[0]); o = np.argsort(ang); p = p[o]; ang = ang[o]
        # resample: ray from the centroid at npts angles, intersect the convex polygon
        q = []
        for t in np.linspace(-math.pi, math.pi, npts, endpoint=False):
            u = np.array([math.cos(t), math.sin(t)]); best = None
            for a, bb in zip(p, np.roll(p, -1, 0)):
                e = bb - a; den = u[0] * (-e[1]) - u[1] * (-e[0])
                if abs(den) < 1e-12: continue
                rhs = a - c; s_ = (rhs[0] * (-e[1]) - rhs[1] * (-e[0])) / den; w = (u[0] * rhs[1] - u[1] * rhs[0]) / den
                if s_ > 0 and -1e-9 <= w <= 1 + 1e-9: best = s_ if best is None else min(best, s_)
            q.append(c + u * best)
        q = np.asarray(q); P3 = np.column_stack([q, np.full(len(q), z)]) @ Rm + E
        out.append(P3)
    return out, h
if __name__ == "__main__":
    # v1.1: the pod data now comes from headrest_POD11.write_fsdata(); this module keeps rot_to_z() and sections() helpers
    import headrest_POD11 as P11; P11.write_fsdata(); print("pod_sections.fsdata written")
