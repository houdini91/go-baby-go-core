# Shared geometry for the r5 side-pad strength check. All numbers from featurescript/gbg_sidepad_r5.fs (the truth).
import numpy as np, math
P0 = np.array([131.3, 80.7]); AXH = np.array([0.79577626, 0.60559074]); TH = np.array([-0.6083053, 0.79370313])
D_SHIFT = 2.0; Z0 = 227.8; SHEAR = np.array([0.8 / 32.9, 2.7 / 32.9]); ZREF = 244.25
LD, LT, ZC = 0.0565, 0.069, 227.0
C = 227.0; NZ = (207.0, 247.0); SEAM = 0.3; T_FRONT = -14.27; T_BACK = 2.0
TG_Z = (214.3, 246.4); CLR = 0.2
TONGUE = [[3.75,1.0],[17.45,1.0],[17.45,3.0],[17.285,9.3],[11.4,15.0],[9.6,18.8],[8.65,22.2],[7.9,23.4],[4.012,18.5],[3.75,8.5]]
SLOT = [[3.55,1.0],[17.65,1.0],[17.65,1.6],[17.5,3.0],[17.335,9.3],[11.565,15.118],[9.788,18.87],[8.835,22.282],[7.919,23.746],[3.962,18.5],[3.7,8.5],[3.55,7.7]]
PADS = (5.5, 13.5); PAD_REL = 0.3; RIBS = (15.0, 17.0); MOUTH = 3.3; TG_ROOT = 3.0
GK = math.tan(math.radians(6)); GR_T = 4.5; GR_D = 22.2; GR_Z = (223.2, 246.3); GR_CH = 0.5
BT_TC = -6.3; BT_W = 12.0; BT_H = 24.0; BT_THK = 4.6; TRAVEL = 4.9
BLK_D = (13.0, 17.2); BLK_T = (-4.0, 9.0); BLK_Z = (223.5, 246.0); TO_D = (17.15, 22.0)
ROD = (8.6, 218.75, 3.25); ROD_T = (13.5, -22.0)
TAPER_T = -14.0; DC = 9.4; YEND = 35.0; ARM_W = 18.0; ARM_H = 28.0; ARM_R = 9.0; GROW = 1.0; ROOT_R = 3.5
YAX = YEND + GROW; DROPS = (22.0, 44.0)
TEN_W = 13.0; TEN_H = 20.0; TEN_L = 13.0; TEN_R = 2.5; JS_DX = 6.5; JGAP = 0.3; STEP_GAP = 0.1
TPIN = [[3.5, 7.4], [-3.5, 7.4], [3.5, -7.4], [-3.5, -7.4]]; TPIN_R = 1.15; TPIN_IN = 8.0; PIN_L = 21.0
XS = 104.0; PAD_OFF = 11.0; FLARE_L = 18.0; FOOT_Y = (19.0, 46.0); FOOT_HZ = 20.0; FOOT_R = 14.5
DV = (9.0, 12.0, 2.5); DV_CL = 0.15; DV_FRONT = 22.0; PS_Y = 32.5
PAD_Y0 = -27.0; PAD_Y1 = 55.0; PAD_H = 80.0; SHELL_T = 4.5
def Q(d, t, z):
    dd = d + D_SHIFT
    xy = P0 + dd * AXH + t * TH + (ZREF - Z0) * SHEAR
    return np.array([xy[0], xy[1], z])
def QS(d, t, z): return Q(d + LD * (z - ZC), t + LT * (z - ZC), z)
def toF(p):   # world -> (d, t, z) tip frame F (with lean)
    r = np.asarray(p[:2]) - P0 - D_SHIFT * AXH - (ZREF - Z0) * SHEAR
    det = AXH[0] * TH[1] - TH[0] * AXH[1]
    return np.array([(r[0] * TH[1] - TH[0] * r[1]) / det, (AXH[0] * r[1] - r[0] * AXH[1]) / det, p[2]])
def toS(p):
    f = toF(p); return np.array([f[0] - LD * (f[2] - ZC), f[1] - LT * (f[2] - ZC), f[2]])
dirD = np.array([*AXH, 0.0]); dirT = np.array([*TH, 0.0])
def w2f_vec(v): return np.array([v @ dirD, v @ dirT, v[2]])
def armPath(c, zp):
    st = Q(DC, TAPER_T, c); h0 = -dirT
    th0 = math.degrees(math.atan2(h0[1], h0[0]))
    R = (YEND - st[1]) / (math.sin(math.radians(th0 - 90)) - 1)
    Cc = [st[0] + R * math.cos(math.radians(th0 - 90)), st[1] + R * math.sin(math.radians(th0 - 90))]
    L = R * math.radians(th0 + 180)
    dz = zp - c; thv = 2 * math.degrees(math.atan(abs(dz) / L)); Rv = L / (2 * math.sin(math.radians(thv)))
    return dict(st=st, th0=th0, R=R, C=Cc, L=L, dz=dz, Rv=Rv, c=c, h0=h0)
def armAt(p, u):
    if u < 0: return np.array([p['st'][0] + p['h0'][0] * u, p['st'][1] + p['h0'][1] * u, p['c']]), p['h0'].copy()
    th = p['th0'] - math.degrees(u / p['R'])
    x = p['C'][0] + p['R'] * math.cos(math.radians(th + 90)); y = p['C'][1] + p['R'] * math.sin(math.radians(th + 90))
    if u <= p['L'] / 2:
        ph = math.asin(min(u / p['Rv'], 1)); zz = p['Rv'] * (1 - math.cos(ph)); sl = math.tan(ph)
    else:
        ph = math.asin(min((p['L'] - u) / p['Rv'], 1)); zz = abs(p['dz']) - p['Rv'] * (1 - math.cos(ph)); sl = math.tan(ph)
    s = -1 if p['dz'] < 0 else 1
    t = np.array([math.cos(math.radians(th)), math.sin(math.radians(th)), s * sl]); t /= np.linalg.norm(t)
    return np.array([x, y, p['c'] + s * zz]), t
def shellXi(y, xs=XS):
    A_REF, B_REF, YC_REF, WRAP = 85.0, 88.0, 34.0, 0.7
    ex = A_REF * max(1 - abs((y - YC_REF) / B_REF) ** 3, 0) ** (1 / 3)
    return xs - SHELL_T - WRAP * (A_REF - ex)
# materials (r4 basis, P1S PETG, solid-section; 6 walls/100 % infill modifiers where the guide says so)
MAT = dict(Xa=40.0, Xc=18.0, S=22.0, Sil=15.0, brg=25.0, E=1900.0, nu=0.38)
CONFIGS = [(arm, kit, pp) for arm in ('S', 'L') for kit in ('LOW', 'HIGH') for pp in (+1, -1)]
def zp_of(arm, kit): d = DROPS[0] if arm == 'S' else DROPS[1]; return C - d if kit == 'LOW' else C + d
