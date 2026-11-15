# Strength / stiffness rating of the mast options from their real sections (sliced every 2 mm), carbon included.
# Beam model: mast = cantilever from the receiver exit (backrest top) up to the pad's chin (pad + head treated as rigid above).
# Carbon rods / tubes sit in close-fit bores, so they bend with the mast (same curvature): EI adds, no glued composite assumed.
import math, numpy as np
import headrest_M_mast as MM
from manifold3d import Manifold as M

E_P, SIG_P = 1900.0, 40.0             # PETG printed: modulus MPa, strength along the layers MPa (lying-down print)
G_P = 700.0
E_ROD, SIG_ROD = 120000.0, 1500.0     # pultruded carbon rod
E_TUBE, G_TUBE, SIG_TUBE = 70000.0, 5000.0, 500.0   # roll-wrapped carbon tube (conservative)
P_KNOCK, P_PUSH, P_SIDE = 300.0, 150.0, 250.0
def poly_props(cs):
    """area, centroid, second moments about the centroid (Ix: about x axis -> fore-aft bending; Iy: side bending)"""
    A = Sx = Sy = Ixx = Iyy = 0.0
    for p in cs.to_polygons():
        p = np.asarray(p); q = np.roll(p, -1, 0); cr = p[:, 0] * q[:, 1] - q[:, 0] * p[:, 1]
        A += cr.sum() / 2; Sx += ((p[:, 0] + q[:, 0]) * cr).sum() / 6; Sy += ((p[:, 1] + q[:, 1]) * cr).sum() / 6
        Ixx += ((p[:, 1] ** 2 + p[:, 1] * q[:, 1] + q[:, 1] ** 2) * cr).sum() / 12
        Iyy += ((p[:, 0] ** 2 + p[:, 0] * q[:, 0] + q[:, 0] ** 2) * cr).sum() / 12
    cx, cy = Sx / A, Sy / A
    pts = np.concatenate([np.asarray(p) for p in cs.to_polygons()])
    return A, cx, cy, Ixx - A * cy * cy, Iyy - A * cx * cx, np.abs(pts[:, 1] - cy).max(), np.abs(pts[:, 0] - cx).max()
def I_circ(d, di=0.0): return math.pi * (d ** 4 - di ** 4) / 64
def analyse(k, H, n=0, dz=2.0, m_in=None, z_fix=None, carbon=None):
    zc, cy = MM.frame(n, H)
    z0 = MM.TOP - 8.0                                   # in the socket, just below the exit (plug section, nut slots)
    z1 = MM.chin_z(n, H) - 0.5
    if m_in is not None:
        m = m_in; tz0, tz1 = carbon
        if z_fix is not None: z0 = z_fix
    elif k == "M2":
        fp, hp = MM.m2_parts(n, H); m = fp + hp
        tz0, tz1 = MM.tube_span(n, H)
    else:
        m = MM.build(k, n, H); tz0, tz1 = None, None
    zs = np.arange(z0, z1, dz); rows = []
    for z in zs:
        cs = m.slice(z)
        if cs.area() < 1: A = Ix = Iy = 0.0; c_y = c_x = 0.0
        else: A, _, _, Ix, Iy, c_y, c_x = poly_props(cs)
        if k == "M2":
            EIc = 2 * E_TUBE * I_circ(MM.TUBE_OD, MM.TUBE_ID) if tz0 <= z <= tz1 else 0.0
            # tubes sit at x ±17: for side bending they also act as a couple only if both ends are fixed -> conservative: own EI only
            EIcx = EIc
        else:
            EIc = EIcx = 2 * E_ROD * I_circ(8.0) if (tz0 is None or tz0 <= z <= tz1) else 0.0
        rows.append((z, A, Ix, Iy, c_y, c_x, E_P * Ix + EIc, E_P * Iy + EIcx, EIc))
    R = np.array(rows)
    z, A, Ix, Iy, cy_, cx_, EIx, EIy, EIc = R.T
    zl = zc                                             # pad centre height (head contact)
    Mk = P_KNOCK * (zl - z); Ms = P_SIDE * (zl + 20 - z)
    kap = Mk / EIx; kas = Ms / EIy
    sig_p = E_P * kap * cy_ * (A > 1); sig_ps = E_P * kas * cx_ * (A > 1)
    rc = MM.TUBE_OD / 2 if k == "M2" else 4.0; Ec = E_TUBE if k == "M2" else E_ROD
    sig_c = Ec * np.maximum(kap, kas) * rc * (EIc > 0)
    # deflection at the pad centre under the 150 N push (unit load method), from the exit up to the chin
    sel = z >= (MM.TOP if z_fix is None else z_fix)
    defl = np.trapezoid((P_PUSH * (zl - z) * (zl - z) / EIx)[sel], z[sel])
    defl_s = np.trapezoid((P_SIDE * (zl + 20 - z) ** 2 / EIy)[sel], z[sel])
    # backup: plastic cracked at the worst section -> carbon alone carries the knock there
    plug_sig = float(sig_p[~sel].max()) if (~sel).any() else 0.0
    sig_p = sig_p * sel; sig_ps = sig_ps * sel
    iw = int(np.argmax(sig_p)); EIc_w = EIc[iw]
    backup = (Ec * Mk[iw] / EIc_w * rc) if EIc_w > 0 else float("inf")
    # torsion: side bash on the pad side, ~110 mm in front of the mast
    T = P_SIDE * 110.0
    exposed = z1 - MM.TOP
    if k == "M2":
        l = max(MM.tube_span(n, H)[1] - MM.tube_span(n, H)[0] - 60.0, 10.0)        # free tube length between the sockets
        kb = 2 * 12 * E_TUBE * I_circ(MM.TUBE_OD, MM.TUBE_ID) / l ** 3 * MM.TX ** 2
        kt = 2 * G_TUBE * (2 * I_circ(MM.TUBE_OD, MM.TUBE_ID)) / l
        th = T / (kb + kt)
    else:
        # solid section: J ~ 0.23 b t^3 of the slimmest exposed section; twist over the exposed length
        e = sel & (A > 1); i = np.argmin(A + (~e) * 1e9)
        b = max(np.sqrt(A[i] * 56 / 26), 1); t_ = A[i] / b
        th = T * max(exposed, 5) / (G_P * 0.23 * max(b, t_) * min(b, t_) ** 3)
    return dict(k=k, H=H, plug=plug_sig, sig_p=float(sig_p.max()), z_crit=float(z[np.argmax(sig_p)]), sig_side=float(sig_ps.max()), sig_c=float(sig_c.max()),
                SF=SIG_P / float(sig_p.max()), SF_side=SIG_P / float(sig_ps.max()), defl=float(defl), defl_side=float(defl_s), backup_sig=float(backup),
                backup_SF=(SIG_TUBE if k == "M2" else SIG_ROD) / float(backup), twist_deg=math.degrees(th), twist_mm=math.degrees(th) * math.pi / 180 * 75,
                vol=m.volume() / 1000, exposed=exposed, carbon_share=float((EIc / EIx)[sel].mean()), profile=R)
if __name__ == "__main__":
    import sys
    for k in (sys.argv[1:] or ("M1", "M2", "M3")):
        for H in (0, 50, 100):
            if k == "M2" and MM.m2_parts(0, H) is None: continue
            r = analyse(k, H)
            print("plug %.1f |" % r["plug"], "%s H%-3d plastic %.1f MPa (SF %.1f) at z %.0f | side %.1f (SF %.1f) | carbon %.0f MPa | push 150N defl %.2f mm, side %.2f | carbon share of EI %.0f%% | if plastic cracks: carbon %.0f MPa (SF %.1f) | twist %.1f deg | %.0f cm3" % (
                k, H, r["sig_p"], r["SF"], r["z_crit"], r["sig_side"], r["SF_side"], r["sig_c"], r["defl"], r["defl_side"], 100 * r["carbon_share"], r["backup_sig"], r["backup_SF"], r["twist_deg"], r["vol"]), flush=True)
