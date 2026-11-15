# Push-button (head switch) geometry on the F3 headrest: where should the PVC rod go?  (pad frame, right side +x, face = -y)
import math, numpy as np
import headrest_P1_pad as P1, headrest_F_flow as F
HEADS = P1.HEADS; yb = F.P.ELL * (F.R - F.FOAM) - 2.0
GAP = 15.0            # skin -> switch cap face
STACK = 35.0          # cap face -> back of the kit switch mount (65 mm button + base + mount)
PLATE_H = 4.0         # connector plate
V_AX = 13.5           # vertical connector: rod axis above its plate
def head_pt(k, dy_fwd=25.0, dz=-5.0):
    """skin target: on the side of the head, dy_fwd mm in front of the ear canal, dz from the skull centre"""
    b, l, zc = HEADS[k]; c = np.array([0, yb - l / 2, zc]); ax = np.array([b / 2, l / 2, b / 2 * 1.05])
    y = yb - 0.45 * l - dy_fwd; z = zc + dz
    q = 1 - ((y - c[1]) / ax[1]) ** 2 - ((z - c[2]) / ax[2]) ** 2
    x = ax[0] * math.sqrt(max(q, 0)); p = np.array([x, y, z])
    n = (p - c) / ax ** 2; n /= np.linalg.norm(n); return p, n
def solve(E, k, cfg, **kw):
    S, n = head_pt(k, **kw)
    if cfg == "V":   # rod runs past the back of the switch, switch faces the head
        P = S + n * (GAP + STACK + PLATE_H + V_AX)
    else:            # "H": rod points at the switch, switch face square to the rod
        P = S + n * (GAP + STACK + PLATE_H + 10.0)
    d = P - E; L = np.linalg.norm(d); d = d / L
    yaw = math.degrees(math.atan2(d[0], -d[1]))        # + = toward OUTSIDE (away from the head), 0 = straight forward
    pitch = math.degrees(math.asin(d[2]))
    face_err = math.degrees(math.acos(abs(np.dot(d, n)))) if cfg == "H" else math.degrees(math.asin(abs(np.dot(d, n))))
    return dict(S=S, n=n, P=P, yaw=yaw, pitch=pitch, L=L, face_err=face_err)
