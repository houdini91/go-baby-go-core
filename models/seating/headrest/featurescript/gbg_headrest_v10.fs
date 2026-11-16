FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// ===================================================================================================================
// GoBabyGo HEADREST v1.0 (PRE-FIRST-PRINT) for GbG_chair_forth.  Native FeatureScript port of the reviewed Python model
// (go-baby-go-core: models/seating/headrest/generator).  Everything is B-rep: the curved pad is two B-spline surfaces
// (head side + back, from the same formulas as the Python) closed by its outline wall (opEnclose); the mast and the
// chair receiver are lofts of the same superellipse sections; every hole, slot, pocket and nut trap is a native cut.
// v1.1 (2026-11-15): smooth single-surface mast stem, clean pod (collar + saddle, 2 screws from outside), teardrop holes
// oriented to each part's print direction.  Data: only POD_AXIS (the PVC axis per yaw, from the angle study).
//
// Frames (mm):  WORLD = chair coordinates (GbG_chair_forth, z up, -y = front).
//   LOCAL = backrest un-reclined (rotate WORLD about the x axis through PIV by +TH).  PAD = pad frame, origin at
//   LOCAL (0, CY, 451 + H): head-side oval radius R, depth ELL, z = 0 at the pad centre.
// Parts:  pad F3 (slit-free, 8 Velcro-dot pockets, strap slots, 8 mm triangle button pocket, K2 tapered channel),
//   mast M1 (B1 plug, R2, relief window when the stem is long enough), W1 button pod (yaw 12/20/28) or blank plug,
//   reference carbon rods.  The chair change (receiver in top_back) is the separate feature "GbG Headrest Receiver".
// ===================================================================================================================

export enum HeadrestHeight
{
    annotation { "Name" : "H0   (pad centre 431 mm above the seat)" } H0,
    annotation { "Name" : "H25  (456 mm)" } H25,
    annotation { "Name" : "H50  (481 mm)" } H50,
    annotation { "Name" : "H75  (506 mm)" } H75,
    annotation { "Name" : "H100 (531 mm)" } H100
}
export enum HeadrestButton
{
    annotation { "Name" : "None (blank plugs both sides)" } NONE,
    annotation { "Name" : "Right" } RIGHT,
    annotation { "Name" : "Left" } LEFT,
    annotation { "Name" : "Both" } BOTH
}
export enum HeadrestYaw
{
    annotation { "Name" : "12 deg out" } Y12,
    annotation { "Name" : "20 deg out (default)" } Y20,
    annotation { "Name" : "28 deg out" } Y28
}

// ---------------- pad (PAD frame) ----------------
const R = 86;  const ELL = 1.28;  const T = 5;  const RR = 2.2;  const LIP = 14;
const OUTLINE = [[0, -52.5], [47, -52.5], [57, -40], [60, -16], [62, 8], [68, 26], [75, 38], [72, 50], [50, 55.5], [0, 55.5]];
const STRAP_ANG = 57;  const STRAP_Z0 = 23;  const STRAP_Z1 = 45.5;            // forehead strap: only if the therapist prescribes it
const DOTS = [[-16, 36], [16, 36], [-42, 32], [42, 32], [-26, 4], [26, 4], [-22, -28], [22, -28]];
const DOT_R = 13;  const VEL_D = 1.5;                                          // Ø26 x 1.5 pockets for Ø25 hook-and-loop dots
// ---------------- K2 joint (PAD frame) ----------------
const R0 = 91;  const TCL = 0.25;  const TA = 32;  const LIPW = 2;  const LIPT = 3.5;  const TI = 91.25;  const TO = 100.25;
const BOT = -52;  const TZ1 = 0;  const TAPER = 3;
const COL_X = 28;  const COL_Y0 = 103.2;  const COL_TOP = -36;
const SPINE_X = 16;  const SPINE_Y = 118;  const ROD_D = 8;  const BOSS_R = 7;  const ROD_TOP = -20;  const BOSS_TOP = -16;
const SA = 24;  const SANG = 35;  const SR0 = 82;  const T_NUT = 24.408255;    // 2x M3x25 from under the chin into captive nuts
const CHIN_SINK = 1.0;   // chin screw head top 1 mm down its spot-face: M3x25 tip at T_NUT + 1.6 (whole nut), blind hole ends T_NUT + 4.2
const CHIN_HOLE_END = 4.2;  // mast clearance hole ends T_NUT + 4.2 (was + 8.7: broke out of the tenon back as 2 oblique teardrops)
// ---------------- button pocket (PAD frame, place() local at +-BTN_ANG) ----------------
const BTN_ANG = 46;  const B_D = 8;
const POCKET_Y0 = 93.6;                                                        // pocket floor (8 mm deep)
const TRI = [[-15, 16], [15, 16], [0, -14]];                                    // (x, z) point down
// ---------------- TOLERANCE BLOCK: P1S / PETG FDM fits (2026-11-16; mirrored in generator/headrest_FITS.py) ----------------
// FDM holes on a P1S print ~0.1-0.3 mm UNDERSIZE on the diameter (horizontal ones worse, teardrops included).  Policy:
//   clearance hole = nominal + 0.4..0.5 on the diameter | nut trap = nut AF + 0.3, depth = nut thickness + 0.4 |
//   sliding fit >= 0.2 per side | PVC / rod bores = slip fits (something else locks them).
// Confirm with the fit-test bar (it brackets every value below), then edit here.
const PVC_BORE = 20.8;      // 20 mm PVC, OD 20.0 +0.2/-0 (ISO 1452): slip fit even on a +0.2 pipe; the 3 wood screws lock it (was 20.3 = press fit)
const ROD_HOLE_D = 8.5;     // Ø8 pultruded carbon rod, often 8.0-8.1 as bought: slide fit (was 8.3)
const M3_CLEAR = 3.5;  const M5_CLEAR = 5.5;                                 // clearance +0.5 (were 3.4 / 5.4)
const NUT_M3_AF = 5.8;  const NUT_M5_AF = 8.3;                               // nut traps across flats: nut 5.5 / 8.0 + 0.3 (unchanged)
const NUT_M3_H = 2.4;  const NUT_M5_H = 4.0;                                 // DIN 934 nut thickness
const NUT_M3_TRAP = NUT_M3_H + 0.4;  const NUT_M5_TRAP = NUT_M5_H + 0.4;     // trap depth / slot thickness 2.8 / 4.4 (were 3.0 pad, 2.7 tenon / 4.3)
const WOOD_SCREW_HOLE = 4.0;  // pod -> PVC lock: ST3.5 x 6.5 pan head, clearance in the pod (the screw cuts only the PVC)
// sliding fits (checked against the policy, unchanged):
const SOCK_CL = 0.2;        // plug in the chair socket, per side (= the 0.2 minimum)
const CH_CL = 0.25;         // K2 tenon in the pad channel / column in the pad socket, per side (meets it, 0.05 spare)
const TRI_CL = 0.15;        // button pocket (+0.15) vs pod wedge (-0.05): 0.2 per side (= the minimum; the click dimples set the feel)
// ---------------- chair / mast (LOCAL frame) ----------------
const TH = 4.645078425891606;  const PIV = [0, 123.5, 390];
const TOP = 391.3;  const FRONT = 123.5;  const SOCK_Z0 = 345;
const PLUG = [-28, 28, 128, 154];  const FOOT = [38, 125, 160, 4];  const FADE = 322;
const NUT5_Y = 136;  const BOLTS_Z = [360, 379];
const CY = 28.78;

function v3(x, y, z) returns Vector { return vector(x, y, z) * millimeter; }
function vv(p) returns Vector { return vector(p[0], p[1], p[2]) * millimeter; }
function sgn(x) returns number { return x < 0 ? -1 : 1; }
function sm(t is number) returns number { const u = min(max(t, 0), 1); return u * u * (3 - 2 * u); }
function lerp(a is number, b is number, t is number) returns number { return a + (b - a) * t; }
function deg(a is number) returns number { return a * 180 / PI; }

export function rFace(z is number, ad is number) returns number
{
    const a = abs(ad);
    const roll = LIP * (1 - sm((z + 36) / 26)) * (1 - sm((a - 30) / 25));
    const lean = 7 * sm((z - 26) / 24) * (1 - sm((a - 40) / 30));
    return R - roll + lean;
}
// thickness field (F3 "full back" shield + strap-slot thickening): back face = rFace + T + dField
export function dField(z is number, ad is number) returns number
{
    const a = abs(ad);
    const A = 39 + 23 * sm((z + 40) / 14) * (1 - sm((z - 10) / 14));
    const s = (1 - sm((a - A) / 14)) * (1 - sm((z - 8) / 38));
    const d = max(0, TO - rFace(z, ad) - T) * s;
    const st = (1 - sm((abs(a - STRAP_ANG) - 5) / 6)) * (1 - sm((abs(z - (STRAP_Z0 + STRAP_Z1) / 2) - 14) / 8));
    return max(d, 2 * st);
}
function bend(u is number, z is number, r is number) returns Vector
{
    const a = -u / R;
    return vector(r * sin(a * radian), ELL * r * cos(a * radian), z);
}
function rIn(z, ad) { return rFace(z, ad); }
function rOut(z, ad) { return rFace(z, ad) + T + dField(z, ad); }

// B-spline surface through a (u, z) grid mapped by bend(); control points pre-filtered (P = S - d2S/6 per direction)
// so the cubic surface passes within ~0.01 mm of the formula surface.
function bentSurface(context is Context, id is Id, u0 is number, u1 is number, nu is number, z0 is number, z1 is number, nz is number, rfun is function)
{
    var S = [];
    for (var i = 0; i < nu; i += 1)
    {
        const u = u0 + (u1 - u0) * i / (nu - 1);
        const ad = deg(-u / R);
        var row = [];
        for (var j = 0; j < nz; j += 1)
        {
            const z = z0 + (z1 - z0) * j / (nz - 1);
            row = append(row, bend(u, z, rfun(z, ad)));
        }
        S = append(S, row);
    }
    var Q = [];
    for (var i = 0; i < nu; i += 1)
    {
        var row = [];
        for (var j = 0; j < nz; j += 1)
            row = append(row, (i == 0 || i == nu - 1) ? S[i][j] : S[i][j] - (S[i - 1][j] - 2 * S[i][j] + S[i + 1][j]) / 6);
        Q = append(Q, row);
    }
    var P = [];
    for (var i = 0; i < nu; i += 1)
    {
        var row = [];
        for (var j = 0; j < nz; j += 1)
        {
            const q = (j == 0 || j == nz - 1) ? Q[i][j] : Q[i][j] - (Q[i][j - 1] - 2 * Q[i][j] + Q[i][j + 1]) / 6;
            row = append(row, q * millimeter);
        }
        P = append(P, row);
    }
    opCreateBSplineSurface(context, id, { "bSplineSurface" : bSplineSurface({ "uDegree" : 3, "vDegree" : 3, "isUPeriodic" : false, "isVPeriodic" : false, "controlPoints" : controlPointMatrix(P) }) });
}
// closed ruled wall along a closed (u, z) curve, r from rlo(z, ad) to rhi(z, ad)
function bentWall(context is Context, id is Id, pts is array, rlo is function, rhi is function)
{
    var W = [];
    for (var p in pts)
    {
        const ad = deg(-p[0] / R);
        W = append(W, [bend(p[0], p[1], rlo(p[1], ad)) * millimeter, bend(p[0], p[1], rhi(p[1], ad)) * millimeter]);
    }
    opCreateBSplineSurface(context, id, { "bSplineSurface" : bSplineSurface({ "uDegree" : 3, "vDegree" : 1, "isUPeriodic" : true, "isVPeriodic" : false, "controlPoints" : controlPointMatrix(W) }) });
}
function chaikin(p is array, n is number) returns array
{
    var q = p;
    for (var k = 0; k < n; k += 1)
    {
        var o = [];
        const m = size(q);
        for (var i = 0; i < m; i += 1)
        {
            const a = q[i];  const b = q[(i + 1) % m];
            o = append(o, [0.75 * a[0] + 0.25 * b[0], 0.75 * a[1] + 0.25 * b[1]]);
            o = append(o, [0.25 * a[0] + 0.75 * b[0], 0.25 * a[1] + 0.75 * b[1]]);
        }
        q = o;
    }
    return q;
}
export function padOutline() returns array
{
    var full = [];
    for (var i = size(OUTLINE) - 1; i >= 0; i -= 1)
        full = append(full, [-OUTLINE[i][0] * PI / 180 * R, OUTLINE[i][1]]);
    for (var i = 1; i < size(OUTLINE) - 1; i += 1)
        full = append(full, [OUTLINE[i][0] * PI / 180 * R, OUTLINE[i][1]]);
    return chaikin(full, 4);
}
function circlePts(cu is number, cz is number, r is number, n is number) returns array
{
    var o = [];
    for (var k = 0; k < n; k += 1)
        o = append(o, [cu + r * cos((2 * PI * k / n) * radian), cz + r * sin((2 * PI * k / n) * radian)]);
    return o;
}
// solid between two bent surfaces (r = rlo .. rhi) inside a closed (u, z) curve
function bentSolid(context is Context, id is Id, pts is array, rlo is function, rhi is function, du is number, dz is number)
{
    var umin = 1e9; var umax = -1e9; var zmin = 1e9; var zmax = -1e9;
    for (var p in pts) { umin = min(umin, p[0]); umax = max(umax, p[0]); zmin = min(zmin, p[1]); zmax = max(zmax, p[1]); }
    umin -= 8; umax += 8; zmin -= 8; zmax += 8;
    const nu = max(5, ceil((umax - umin) / du) + 1);  const nz = max(5, ceil((zmax - zmin) / dz) + 1);
    bentSurface(context, id + "lo", umin, umax, nu, zmin, zmax, nz, rlo);
    bentSurface(context, id + "hi", umin, umax, nu, zmin, zmax, nz, rhi);
    bentWall(context, id + "wall", pts, function(z, ad) { return rlo(z, ad) - 1.5; }, function(z, ad) { return rhi(z, ad) + 1.5; });
    opEnclose(context, id + "enc", { "entities" : qUnion([qCreatedBy(id + "lo", EntityType.BODY), qCreatedBy(id + "hi", EntityType.BODY), qCreatedBy(id + "wall", EntityType.BODY)]), "mergeResults" : true });
    cleanup(context, id + "cl", id);
}
function cleanup(context is Context, id is Id, of is Id)
{
    const junk = qSubtraction(qCreatedBy(of, EntityType.BODY), qBodyType(qCreatedBy(of, EntityType.BODY), BodyType.SOLID));
    if (size(evaluateQuery(context, junk)) > 0)
        opDeleteBodies(context, id, { "entities" : junk });
}
function solids(context is Context, of is Id) returns Query { return qBodyType(qCreatedBy(of, EntityType.BODY), BodyType.SOLID); }

// place(): a part built at angle 0 (radial = +y, reference radius R) -> normal to the oval at circle angle theta
function placeXf(theta is number) returns Transform
{
    const t = theta * PI / 180;
    const phi = atan2(sin(t * radian), cos(t * radian) / ELL);
    return transform(v3(R * sin(t * radian), ELL * R * cos(t * radian), 0)) * rotationAround(line(v3(0, 0, 0), vector(0, 0, 1)), -phi) * transform(v3(0, -R, 0));
}
function moveSolids(context is Context, id is Id, of is Id, xf is Transform)
{
    opTransform(context, id, { "bodies" : solids(context, of), "transform" : xf });
}
function ycyl(context is Context, id is Id, x is number, y0 is number, y1 is number, z is number, r is number)
{
    fCylinder(context, id, { "bottomCenter" : v3(x, y0, z), "topCenter" : v3(x, y1, z), "radius" : r * millimeter });
}
// rounded triangle prism along +y (place() local): triangle inset 5 mm, then grown 5 + off (= the Python offset(-5).offset(5 + off))
function triPrism(context is Context, id is Id, off is number, y0 is number, y1 is number)
{
    var V = [];
    for (var p in TRI) V = append(V, vector(p[0], -p[1]));          // sketch on a +y plane: sketch y = -z
    var area = 0;
    for (var i = 0; i < 3; i += 1) area += V[i][0] * V[(i + 1) % 3][1] - V[(i + 1) % 3][0] * V[i][1];
    if (area < 0) V = [V[0], V[2], V[1]];
    const a = norm(V[1] - V[2]);  const b = norm(V[0] - V[2]);  const c = norm(V[0] - V[1]);
    const I = (a * V[0] + b * V[1] + c * V[2]) / (a + b + c);
    const s = (a + b + c) / 2;
    const ri = sqrt(s * (s - a) * (s - b) * (s - c)) / s;
    const k = (ri - 5) / ri;  const r = 5 + off;
    var W = [];  var N = [];
    for (var i = 0; i < 3; i += 1) W = append(W, I + (V[i] - I) * k);
    for (var i = 0; i < 3; i += 1) { const e = W[(i + 1) % 3] - W[i]; N = append(N, normalize(vector(e[1], -e[0]))); }
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, y0, 0), vector(0, 1, 0), vector(1, 0, 0)) });
    for (var i = 0; i < 3; i += 1)
    {
        const j = (i + 1) % 3;
        skLineSegment(sk, "l" ~ i, { "start" : (W[i] + r * N[i]) * millimeter, "end" : (W[j] + r * N[i]) * millimeter });
        skArc(sk, "a" ~ i, { "start" : (W[j] + r * N[i]) * millimeter, "mid" : (W[j] + r * normalize(N[i] + N[j])) * millimeter, "end" : (W[j] + r * N[j]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : (y1 - y0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// hex prism: centre c, axis n (unit), one vertex along xd, across flats af, thickness h (centred); optional slide L along xd
function hexPrism(context is Context, id is Id, c is Vector, n is Vector, xd is Vector, af is number, h is number, slide is number)
{
    const rv = af / sqrt(3);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(c - n * (h / 2) * millimeter, n, xd) });
    var pts = [];
    for (var k = 0; k <= 6; k += 1) pts = append(pts, vector(rv * cos((k * 60) * degree), rv * sin((k * 60) * degree)) * millimeter);
    for (var k = 0; k < 6; k += 1) skLineSegment(sk, "h" ~ k, { "start" : pts[k], "end" : pts[k + 1] });
    if (slide != 0)
        skRectangle(sk, "sl", { "firstCorner" : vector(0, -af / 2) * millimeter, "secondCorner" : vector(slide, af / 2) * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : n, "endBound" : BoundingType.BLIND, "endDepth" : h * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// apply a Transform to a unitless (mm) point / direction
function xp(xf is Transform, p is Vector) returns Vector { return (xf * (p * millimeter)) / millimeter; }
function xd(xf is Transform, v is Vector) returns Vector { return xp(xf, v) - xp(xf, vector(0, 0, 0)); }
// TEARDROP hole p0 -> p1 (mm), radius r, the point toward `up` = the PRINT up direction of the part, so a horizontal
// hole prints without support (45 deg roof).  Holes that are near vertical in print stay round.
function tdHole(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number, up is Vector)
{
    const ax = normalize(p1 - p0);
    var u = up - ax * dot(up, ax);
    if (norm(u) < 0.3)
    {
        fCylinder(context, id + "c", { "bottomCenter" : p0 * millimeter, "topCenter" : p1 * millimeter, "radius" : r * millimeter });
        return;
    }
    u = normalize(u);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0 * millimeter, ax, u) });
    const c45 = r * sqrt(0.5);
    skCircle(sk, "c", { "center" : vector(0, 0) * millimeter, "radius" : r * millimeter });
    skLineSegment(sk, "a", { "start" : vector(c45, c45) * millimeter, "end" : vector(r * sqrt(2), 0) * millimeter });
    skLineSegment(sk, "b", { "start" : vector(c45, -c45) * millimeter, "end" : vector(r * sqrt(2), 0) * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : ax, "endBound" : BoundingType.BLIND, "endDepth" : norm(p1 - p0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// TRUNCATED teardrop (big horizontal bores): as tdHole, the tip cut flat at `cap` mm from the axis (a short flat bridge),
// so the tip of a large teardrop can't break out through a thin wall (the Ø20.3 pod bore's tip did: a 30 mm slit on the collar)
function tdHoleCap(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number, up is Vector, cap is number)
{
    const ax = normalize(p1 - p0);
    var u = up - ax * dot(up, ax);
    if (norm(u) < 0.3 || cap >= r * sqrt(2) - 0.01)
    {
        tdHole(context, id, p0, p1, r, up);
        return;
    }
    u = normalize(u);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0 * millimeter, ax, u) });
    const c45 = r * sqrt(0.5);  const h = r * sqrt(2) - cap;
    skCircle(sk, "c", { "center" : vector(0, 0) * millimeter, "radius" : r * millimeter });
    skLineSegment(sk, "a", { "start" : vector(c45, c45) * millimeter, "end" : vector(cap, h) * millimeter });
    skLineSegment(sk, "b", { "start" : vector(c45, -c45) * millimeter, "end" : vector(cap, -h) * millimeter });
    skLineSegment(sk, "t", { "start" : vector(cap, h) * millimeter, "end" : vector(cap, -h) * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : ax, "endBound" : BoundingType.BLIND, "endDepth" : norm(p1 - p0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// 2D convex hull (monotone chain) of [[u, z], ...], counter-clockwise
function hull2d(pts is array) returns array
{
    var p = sort(pts, function(a, b) { return a[0] == b[0] ? a[1] - b[1] : a[0] - b[0]; });
    const cr = function(o, a, b) { return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0]); };
    var lo = [];
    for (var q in p) { while (size(lo) >= 2 && cr(lo[size(lo) - 2], lo[size(lo) - 1], q) <= 0) lo = resize(lo, size(lo) - 1); lo = append(lo, q); }
    var hi = [];
    for (var i = size(p) - 1; i >= 0; i -= 1) { const q = p[i]; while (size(hi) >= 2 && cr(hi[size(hi) - 2], hi[size(hi) - 1], q) <= 0) hi = resize(hi, size(hi) - 1); hi = append(hi, q); }
    return concatenateArrays([resize(lo, size(lo) - 1), resize(hi, size(hi) - 1)]);
}
// v1.1 pod / blank fixing: 2 M3 button-head screws from OUTSIDE through the pod saddle into hex nuts trapped on the pad's
// head side (under the foam); the blank: 1 countersunk M3 from outside.  (x mm along the surface from the pocket centre, +fwd; z)
// PRINT-UP directions (chosen by surface-class support study, v1.1): pad prints UPSIDE DOWN -> up = -z (pad frame);
// pod prints lying on its pad-side underside -> up = +x for the right pod (s = +1), -x for the left.
const PAD_UP = vector(0, 0, -1);
function podUp(s is number) returns Vector { return vector(s, 0, 0); }
const POD_SCREWS = [[-14, -9], [14, -12]];
const BLANK_SCREW = [0, 4];
const NUT_TRAP_D = NUT_M3_TRAP;  // hex trap depth from the head-side face: M3 nut 2.4 + 0.4 (was 3.0)
function screwXf(s is number, sc is array) returns Transform { return placeXf(s * (BTN_ANG + sc[0] / R * 180 / PI)); }
// rounded box (every edge rounded r) = Python rrbox
function rrbox(context is Context, id is Id, x0, x1, y0, y1, z0, z1, r)
{
    fCuboid(context, id + "b", { "corner1" : v3(x0, y0, z0), "corner2" : v3(x1, y1, z1) });
    opFillet(context, id + "f", { "entities" : qOwnedByBody(qCreatedBy(id + "b", EntityType.BODY), EntityType.EDGE), "radius" : r * millimeter });
}
// planar closed section (array of [x, y, z]) -> face
function sectionFace(context is Context, id is Id, pts is array) returns Query
{
    var p = [];
    for (var q in pts) p = append(p, vv(q));
    p = append(p, vv(pts[0]));
    opFitSpline(context, id + "w", { "points" : p });
    opFillSurface(context, id + "f", { "edgesG0" : qCreatedBy(id + "w", EntityType.EDGE) });
    return qCreatedBy(id + "f", EntityType.FACE);
}
// ruled solid between two planar sections with the same point count (exactly the Python hull-of-neighbours piece):
// periodic B-spline wall (extended 3 % past both ends) closed by the two section faces
function ruledPair(context is Context, id is Id, A is array, B is array)
{
    var W = [];
    for (var k = 0; k < size(A); k += 1)
    {
        const a = vector(A[k][0], A[k][1], A[k][2]);  const b = vector(B[k][0], B[k][1], B[k][2]);
        W = append(W, [(a - (b - a) * 0.03) * millimeter, (b + (b - a) * 0.03) * millimeter]);
    }
    opCreateBSplineSurface(context, id + "wall", { "bSplineSurface" : bSplineSurface({ "uDegree" : 3, "vDegree" : 1, "isUPeriodic" : true, "isVPeriodic" : false, "controlPoints" : controlPointMatrix(W) }) });
    sectionFace(context, id + "ca", A);
    sectionFace(context, id + "cb", B);
    opEnclose(context, id + "enc", { "entities" : qUnion([qCreatedBy(id + "wall", EntityType.BODY), qCreatedBy(id + "ca", EntityType.BODY), qCreatedBy(id + "cb", EntityType.BODY)]), "mergeResults" : true });
}
function loftSections(context is Context, id is Id, secs is array)
{
    var faces = [];
    for (var i = 0; i < size(secs); i += 1) faces = append(faces, sectionFace(context, id + ("s" ~ i), secs[i]));
    try silent { opLoft(context, id + "loft", { "profileSubqueries" : faces, "bodyType" : ToolBodyType.SOLID }); }
    if (size(evaluateQuery(context, solids(context, id + "loft"))) == 0)
    {
        // fallback = the Python model: ruled pieces between neighbouring sections, united
        for (var i = 0; i + 1 < size(faces); i += 1)
        {
            try silent { opLoft(context, id + ("pc" ~ i), { "profileSubqueries" : [faces[i], faces[i + 1]], "bodyType" : ToolBodyType.SOLID }); }
            if (size(evaluateQuery(context, solids(context, id + ("pc" ~ i)))) == 0)
                try silent { ruledPair(context, id + ("rp" ~ i), secs[i], secs[i + 1]); }
        }
        if (size(evaluateQuery(context, solids(context, id))) > 1)
            opBoolean(context, id + "pu", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    }
    cleanup(context, id + "cl", id);
}
// superellipse section (Python headrest_M_mast.section), z level, optional back = oval of the pad (cy)
function section(hw is number, y0 is number, y1 is number, p is number, cyBack, z is number) returns array
{
    return sectionB(hw, y0, y1, p, cyBack, 1, z);
}
// superellipse section; the back follows the pad oval when cyBack is given, blended in by bw (0 = flat, 1 = oval)
function sectionB(hw is number, y0 is number, y1 is number, p is number, cyBack, bw is number, z is number) returns array
{
    var o = [];
    const ym = (y0 + y1) / 2;  const hd = (y1 - y0) / 2;
    for (var k = 0; k < 72; k += 1)
    {
        const t = 2 * PI * k / 72;
        const c = cos(t * radian);  const s = sin(t * radian);
        const x = hw * sgn(c) * abs(c) ^ (2 / p);
        const v = sgn(s) * abs(s) ^ (2 / p);
        var y = ym + hd * v;
        if (cyBack != undefined && v > 0)
        {
            const yb = y1 + (min(cyBack + ELL * sqrt(max(TO * TO - x * x, 1)) - 0.4, y1) - y1) * bw;
            y = ym + (yb - ym) * v;
        }
        o = append(o, [x, y, z]);
    }
    return o;
}
function linspace(a is number, b is number, n is number) returns array
{
    var o = [];
    for (var i = 0; i < n; i += 1) o = append(o, n == 1 ? a : a + (b - a) * i / (n - 1));
    return o;
}
// T-profile (tenon or channel) at height z in PAD frame, taper applied: circle-space half angles a1 (main), a2 (lips)
function tProfileSketch(context is Context, id is Id, z is number, ri is number, rl is number, ro is number, a1 is number, a2 is number)
{
    const k = 1 - (TAPER / TA) * min(max((z - BOT) / (TZ1 - BOT), 0), 1);
    const b1 = a1 * k;  const b2 = a2 * k;
    const sk = newSketchOnPlane(context, id, { "sketchPlane" : plane(v3(0, 0, z), vector(0, 0, 1), vector(1, 0, 0)) });
    const P = function(r, a) { return vector(r * sin(a * degree), ELL * r * cos(a * degree)) * millimeter; };
    const arc = function(name, r, aa, ab) {
        var pts = [];
        const n = max(4, ceil(abs(ab - aa) / 1.5));
        for (var i = 0; i <= n; i += 1) pts = append(pts, P(r, aa + (ab - aa) * i / n));
        skFitSpline(sk, name, { "points" : pts });
    };
    arc("ai", ri, -b2, b2);
    skLineSegment(sk, "r1", { "start" : P(ri, b2), "end" : P(rl, b2) });
    arc("al1", rl, b2, b1);
    skLineSegment(sk, "r2", { "start" : P(rl, b1), "end" : P(ro, b1) });
    arc("ao", ro, b1, -b1);
    skLineSegment(sk, "r3", { "start" : P(ro, -b1), "end" : P(rl, -b1) });
    arc("al2", rl, -b1, -b2);
    skLineSegment(sk, "r4", { "start" : P(rl, -b2), "end" : P(ri, -b2) });
    skSolve(sk);
}
// tapered T solid from z0 to z1 (prism below BOT, tapered loft above)
function tSolid(context is Context, id is Id, z0 is number, z1 is number, ri is number, rl is number, ro is number, a1 is number, a2 is number)
{
    var zb = max(z0, BOT);
    if (z0 < BOT)
    {
        tProfileSketch(context, id + "s0", z0, ri, rl, ro, a1, a2);
        opExtrude(context, id + "pr", { "entities" : qSketchRegion(id + "s0"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (BOT - z0 + 0.01) * millimeter });
    }
    tProfileSketch(context, id + "s1", zb, ri, rl, ro, a1, a2);
    tProfileSketch(context, id + "s2", z1, ri, rl, ro, a1, a2);
    opLoft(context, id + "lo", { "profileSubqueries" : [qSketchRegion(id + "s1"), qSketchRegion(id + "s2")], "bodyType" : ToolBodyType.SOLID });
    if (z0 < BOT)
        opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    cleanup(context, id + "cl", id);
}
// screw axis of the K2 lock screw (sgn = +-1), parameter t along the axis (PAD frame), after the oval stretch
function scrPt(sg is number, t is number) returns Vector
{
    const s = SANG * PI / 180;
    const x = 0;  const y = SR0 + t * sin(s * radian);  const z = BOT - 0.5 + t * cos(s * radian);
    const th = -sg * SA * PI / 180;
    const xr = x * cos(th * radian) - y * sin(th * radian);
    const yr = x * sin(th * radian) + y * cos(th * radian);
    return vector(xr, ELL * yr, z);
}
function scrSlideDir(sg is number) returns Vector
{
    const th = -sg * SA * PI / 180;
    return normalize(vector(cos(th * radian), ELL * sin(th * radian), 0)) * sg;
}

// =============================================== PAD ===============================================
export function buildPad(context is Context, id is Id, strap is boolean) returns Query
{
    const ol = padOutline();
    bentSolid(context, id + "body", ol, function(z, ad) { return rIn(z, ad); }, function(z, ad) { return rOut(z, ad); }, 2.5, 2.0);
    const pad = solids(context, id + "body");
    try silent { opFillet(context, id + "rnd", { "entities" : qOwnedByBody(pad, EntityType.EDGE), "radius" : RR * millimeter }); }
    // ---- cuts ----
    const C = id + "cut";
    for (var s in [1, -1])
    {
        const sid = C + (s > 0 ? "R" : "L");
        // forehead-strap slot 22.5 x 4.5 (therapist-prescribed strap only: feature option "Forehead strap slots")
        if (strap)
        {
            ycyl(context, sid + "st" + "a", 0, R - 15, R + 35, STRAP_Z0 + 2.25, 2.25);
            ycyl(context, sid + "st" + "b", 0, R - 15, R + 35, STRAP_Z1 - 2.25, 2.25);
            fCuboid(context, sid + "st" + "c", { "corner1" : v3(-2.25, R - 15, STRAP_Z0 + 2.25), "corner2" : v3(2.25, R + 35, STRAP_Z1 - 2.25) });
            moveSolids(context, sid + "stm", sid + "st", placeXf(s * STRAP_ANG));
        }
        // triangle button pocket 8 mm deep + 2 click dimples + M3 countersunk from the head side
        triPrism(context, sid + "b" + "p", TRI_CL, POCKET_Y0, POCKET_Y0 + B_D + 1);
        opSphere(context, sid + "b" + "d1", { "center" : v3(9.3, POCKET_Y0 + B_D / 2, 4), "radius" : 1.3 * millimeter });
        opSphere(context, sid + "b" + "d2", { "center" : v3(-9.3, POCKET_Y0 + B_D / 2, 4), "radius" : 1.3 * millimeter });
        moveSolids(context, sid + "bpm", sid + "b", placeXf(s * BTN_ANG));
        // pod (2) + blank (1) screws: clearance through the shell (teardrop toward PAD_UP: the pad prints upside down) + hex nut
        // traps on the head side (depth NUT_TRAP_D: the nut sits 0.4 mm under the head-side face)
        const scs = concatenateArrays([POD_SCREWS, [BLANK_SCREW]]);
        for (var k = 0; k < size(scs); k += 1)
        {
            const xf = screwXf(s, scs[k]);  const z = scs[k][1];
            tdHole(context, sid + ("sh" ~ k), xp(xf, vector(0, R - 2, z)), xp(xf, vector(0, R + 30, z)), M3_CLEAR / 2, PAD_UP);
            hexPrism(context, sid + ("st" ~ k), xp(xf, vector(0, R - 1 + (NUT_TRAP_D + 1) / 2, z)) * millimeter, xd(xf, vector(0, 1, 0)), vector(0, 0, 1), NUT_M3_AF, NUT_TRAP_D + 1, 0);
        }
        // K2 lock screw (M3x25 DIN 7991): clearance + countersink at the chin.  The axis is ~41 deg off the chin face, so a bare
        // countersink left the head up to 2.6 mm proud: a flat Ø6.6 spot-face down the axis seats it, head top CHIN_SINK into it
        const a = scrPt(s, -8);  const b = scrPt(s, T_NUT + 6);
        tdHole(context, sid + "sc", a, b, M3_CLEAR / 2, PAD_UP);
        fCone(context, sid + "sk", { "bottomCenter" : scrPt(s, CHIN_SINK - 0.05) * millimeter, "topCenter" : scrPt(s, CHIN_SINK + CSK_M3_R - 1.5) * millimeter, "bottomRadius" : CSK_M3_R * millimeter, "topRadius" : 1.5 * millimeter });
        fCylinder(context, sid + "sf", { "bottomCenter" : scrPt(s, -12) * millimeter, "topCenter" : scrPt(s, CHIN_SINK) * millimeter, "radius" : 3.3 * millimeter });
    }
    // Velcro dot pockets: Ø26, floor 1.5 mm into the shell, following the head-side surface
    for (var i = 0; i < size(DOTS); i += 1)
    {
        const cu = -DOTS[i][0] * PI / 180 * R;
        bentSolid(context, C + ("dot" ~ i), circlePts(cu, DOTS[i][1], DOT_R, 32),
            function(z, ad) { return rFace(z, ad) - 2; }, function(z, ad) { return rFace(z, ad) + VEL_D; }, 3, 3);
    }
    // K2 tapered channel + lips, column socket, spine-boss sockets
    tSolid(context, C + "ch", BOT - 20, TZ1 + CH_CL, R0 - 0.01, R0 + LIPT + CH_CL, TO + 20, TA + CH_CL * 0.7, TA + LIPW + CH_CL * 0.7);
    rrbox(context, C + "col", -COL_X - CH_CL, COL_X + CH_CL, COL_Y0 - CH_CL, 200, BOT - 30, COL_TOP + CH_CL, 6);
    for (var x in [-SPINE_X, SPINE_X])
    {
        const bid = C + (x < 0 ? "bossL" : "bossR");
        fCylinder(context, bid + "c", { "bottomCenter" : v3(x, SPINE_Y, BOT - 32), "topCenter" : v3(x, SPINE_Y, BOSS_TOP - BOSS_R), "radius" : (BOSS_R + CH_CL) * millimeter });
        opSphere(context, bid + "s", { "center" : v3(x, SPINE_Y, BOSS_TOP - BOSS_R), "radius" : (BOSS_R + CH_CL) * millimeter });
    }
    opBoolean(context, id + "sub", { "tools" : solids(context, C), "targets" : pad, "operationType" : BooleanOperationType.SUBTRACTION });
    return pad;
}

// =============================================== MAST (LOCAL frame) ===============================================
function chinZ(H is number) returns number { return 451 + H + BOT; }
export function windowSpan(H is number) returns array
{
    const zch = chinZ(H) - 0.3;  const gap = zch - (TOP + 0.3);
    const ff = min(30, 0.42 * gap);  const cf = min(26, 0.45 * gap);
    return [TOP + 0.3 + ff + 6, zch - cf - 2];
}
// M1 stem section at height z (LOCAL frame): foot (= the receiver top) -> waist -> chin flare; v1.0.1: smooth waist and
// a blended back (the v1.0 hard switch + |x| waist left creases and forced a stepped loft)
function stemSection(H is number, z is number) returns array
{
    const zch = chinZ(H) - 0.3;  const zf = TOP + 0.3;  const gap = zch - zf;
    const ff = min(30, 0.42 * gap);  const cf = min(26, 0.45 * gap);
    const ybk = CY + ELL * TO - 0.4;
    const tf = sm((z - zf) / max(ff, 1e-3));  const tc = sm((z - (zch - cf)) / max(cf, 1e-3));
    const u = min(max((z - zf) / max(gap, 1), 0), 1);
    const mid = 1 - (2 * u - 1) * (2 * u - 1);
    const hw = lerp(lerp(FOOT[0], 28 * lerp(1, 0.9, sm(mid)), tf), 44, tc);
    const y0 = lerp(lerp(FOOT[1], 132, tf), CY + 103.2, tc);
    const y1 = lerp(lerp(FOOT[2], ybk, tf), ybk, tc);
    return sectionB(hw, y0, y1, lerp(lerp(FOOT[3], 2.6, tf), 3.0, tc), CY, sm((tf - 0.25) / 0.5), z);
}
// the stem as ONE smooth periodic B-spline surface through the section formula every ~2 mm (pre-filtered so it passes
// through the samples), cut to length by a box: no loft solver, no section seams
function buildStem(context is Context, id is Id, H is number)
{
    const zf = TOP + 0.3;  const zch = chinZ(H) - 0.3;
    const n = max(4, ceil((zch - zf) / 2));  const dz = (zch - zf) / n;
    var cols = [];
    for (var j = -1; j <= n + 1; j += 1) cols = append(cols, stemSection(H, zf + j * dz));
    const nv = size(cols);
    var S = [];
    for (var k = 0; k < 72; k += 1)
    {
        var row = [];
        for (var j = 0; j < nv; j += 1) row = append(row, vector(cols[j][k][0], cols[j][k][1], cols[j][k][2]));
        S = append(S, row);
    }
    var P = [];
    for (var k = 0; k < 72; k += 1)
    {
        var row = [];
        for (var j = 0; j < nv; j += 1)
        {
            const a = S[(k + 71) % 72][j];  const b = S[(k + 1) % 72][j];
            var q = S[k][j] - (a - 2 * S[k][j] + b) / 6;
            if (j > 0 && j < nv - 1)
            {
                const qa = S[k][j - 1] - (S[(k + 71) % 72][j - 1] - 2 * S[k][j - 1] + S[(k + 1) % 72][j - 1]) / 6;
                const qb = S[k][j + 1] - (S[(k + 71) % 72][j + 1] - 2 * S[k][j + 1] + S[(k + 1) % 72][j + 1]) / 6;
                q = q - (qa - 2 * q + qb) / 6;
            }
            row = append(row, q * millimeter);
        }
        P = append(P, row);
    }
    opCreateBSplineSurface(context, id + "srf", { "bSplineSurface" : bSplineSurface({ "uDegree" : 3, "vDegree" : 3, "isUPeriodic" : true, "isVPeriodic" : false, "controlPoints" : controlPointMatrix(P) }) });
    fCuboid(context, id + "box", { "corner1" : v3(-90, 60, zf), "corner2" : v3(90, 220, zch) });
    opSplitPart(context, id + "sp", { "targets" : solids(context, id + "box"), "tool" : qCreatedBy(id + "srf", EntityType.BODY), "keepTools" : false });
    const keep = qContainsPoint(solids(context, id), v3(0, 143, (zf + zch) / 2));
    const others = qSubtraction(solids(context, id), keep);
    if (size(evaluateQuery(context, others)) > 0) opDeleteBodies(context, id + "del", { "entities" : others });
    cleanup(context, id + "cl", id);
}
export function buildMast(context is Context, id is Id, H is number) returns Query
{
    const zc = 451 + H;
    const toLocal = transform(v3(0, CY, zc));
    // ---- head (PAD frame, above the chin line): column stub on the pad oval, K2 tapered tenon, spine bosses ----
    const hd = id + "head";
    rrbox(context, hd + "col", -COL_X, COL_X, COL_Y0, 140, BOT - 12, COL_TOP, 6);
    var ovp = [];
    for (var k = 0; k < 120; k += 1) ovp = append(ovp, [TO * sin((3 * k) * degree), ELL * TO * cos((3 * k) * degree), BOT - 20]);
    opExtrude(context, hd + "ov", { "entities" : sectionFace(context, hd + "ovs", ovp), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : 40 * millimeter });
    fCuboid(context, hd + "clip", { "corner1" : v3(-200, -200, BOT - 0.3), "corner2" : v3(200, 300, 100) });
    opBoolean(context, hd + "colx", { "tools" : qUnion([solids(context, hd + "col"), solids(context, hd + "ov"), solids(context, hd + "clip")]), "operationType" : BooleanOperationType.INTERSECTION });
    cleanup(context, hd + "dov", hd);
    tSolid(context, hd + "ten", BOT, TZ1, TI, R0 + LIPT, TO, TA, TA + LIPW - 0.3);
    for (var x in [-SPINE_X, SPINE_X])
    {
        const bid = hd + (x < 0 ? "bossL" : "bossR");
        fCylinder(context, bid + "c", { "bottomCenter" : v3(x, SPINE_Y, BOT - 0.3), "topCenter" : v3(x, SPINE_Y, BOSS_TOP - BOSS_R), "radius" : BOSS_R * millimeter });
        opSphere(context, bid + "s", { "center" : v3(x, SPINE_Y, BOSS_TOP - BOSS_R), "radius" : BOSS_R * millimeter });
    }
    moveSolids(context, hd + "mv", hd, toLocal);
    // ---- stem loft (LOCAL) + plug in the chair socket ----
    buildStem(context, id + "stem", H);
    rrbox(context, id + "plug", PLUG[0], PLUG[1], PLUG[2], PLUG[3], SOCK_Z0 + 0.3, TOP + 0.5, 6);
    opBoolean(context, id + "uni", { "tools" : qUnion([solids(context, hd), solids(context, id + "stem"), solids(context, id + "plug")]), "operationType" : BooleanOperationType.UNION });
    const C = id + "cut";
    const mast = qSubtraction(solids(context, id), solids(context, C));
    // ---- cuts ----
    for (var x in [-SPINE_X, SPINE_X])
        tdHole(context, C + (x < 0 ? "rodL" : "rodR"), vector(x, CY + SPINE_Y, SOCK_Z0 - 0.5), vector(x, CY + SPINE_Y, zc + ROD_TOP + 0.5), ROD_HOLE_D / 2, vector(-1, 0, 0));   // mast prints on its side: up = -x
    for (var i = 0; i < 2; i += 1)
    {
        tdHole(context, C + ("bolt" ~ i), vector(0, PLUG[2] + 0.5, BOLTS_Z[i]), vector(0, PLUG[2] + 62, BOLTS_Z[i]), M5_CLEAR / 2, vector(-1, 0, 0));
        hexPrism(context, C + ("nut" ~ i), v3(0, NUT5_Y, BOLTS_Z[i]), vector(0, 1, 0), vector(-1, 0, 0), NUT_M5_AF, NUT_M5_TRAP, 40);   // slides in from -x
    }
    for (var s in [1, -1])
    {
        const sid = C + (s > 0 ? "scR" : "scL");
        tdHole(context, sid + "c", scrPt(s, 0), scrPt(s, T_NUT + CHIN_HOLE_END), M3_CLEAR / 2, vector(-1, 0, 0));
        const n = normalize(scrPt(s, 1) - scrPt(s, 0));
        var xd = scrSlideDir(s);  xd = normalize(xd - n * dot(xd, n));
        hexPrism(context, sid + "n", scrPt(s, T_NUT) * millimeter, n, xd, NUT_M3_AF, NUT_M3_TRAP, 30);                         // nut slides in from the tenon side edge
        moveSolids(context, sid + "mv", sid, toLocal);
    }
    const w = windowSpan(H);
    if (w[1] - w[0] >= 26)
    {
        const sk2 = newSketchOnPlane(context, C + "wins", { "sketchPlane" : plane(v3(0, 90, 0), vector(0, 1, 0), vector(1, 0, 0)) });
        skCircle(sk2, "c0", { "center" : vector(0, -(w[0] + 8)) * millimeter, "radius" : 8 * millimeter });
        skCircle(sk2, "c1", { "center" : vector(0, -(w[1] - 8)) * millimeter, "radius" : 8 * millimeter });
        skRectangle(sk2, "r", { "firstCorner" : vector(-8, -(w[0] + 8)) * millimeter, "secondCorner" : vector(8, -(w[1] - 8)) * millimeter });
        skSolve(sk2);
        opExtrude(context, C + "win", { "entities" : qSketchRegion(C + "wins"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : 120 * millimeter });
        opDeleteBodies(context, C + "dwin", { "entities" : qCreatedBy(C + "wins", EntityType.BODY) });
    }
    opBoolean(context, id + "sub", { "tools" : solids(context, C), "targets" : mast, "operationType" : BooleanOperationType.SUBTRACTION });
    if (w[1] - w[0] >= 26)
    {
        const wf = qCreatedBy(C + "win", EntityType.FACE);
        const other = qSubtraction(qOwnedByBody(mast, EntityType.FACE), wf);
        try silent { opFillet(context, id + "winr", { "entities" : qIntersection([qAdjacent(wf, AdjacencyType.EDGE, EntityType.EDGE), qAdjacent(other, AdjacencyType.EDGE, EntityType.EDGE)]), "radius" : 2 * millimeter }); }
    }
    return solids(context, id);
}

// =============================================== BUTTON POD / BLANK (PAD frame) ===============================================
function yawKey(y is HeadrestYaw) returns string { return y == HeadrestYaw.Y12 ? "12" : (y == HeadrestYaw.Y28 ? "28" : "20"); }
// keep only the solid pieces (under `scope`) outside (keepOut) or inside the back surface r = rOut + off
function trimBySurface(context is Context, id is Id, scope is Id, s is number, off is number, keepOut is boolean)
{
    const ua = -(s * BTN_ANG + 45) * PI / 180 * R;  const ub = -(s * BTN_ANG - 45) * PI / 180 * R;
    bentSurface(context, id + "srf", min(ua, ub), max(ua, ub), 55, -50, 60, 45, function(z, ad) { return rOut(z, ad) + off; });
    opSplitPart(context, id + "sp", { "targets" : solids(context, scope), "tool" : qCreatedBy(id + "srf", EntityType.BODY), "keepTools" : false });
    var n = 0;
    for (var b in evaluateQuery(context, solids(context, scope)))
    {
        const c = evApproximateCentroid(context, { "entities" : b }) / millimeter;
        const ad = atan2(c[0], c[1] / ELL) / degree;
        const rc = sqrt(c[0] * c[0] + c[1] * c[1] / (ELL * ELL));
        if ((rc > rOut(c[2], ad) + off) != keepOut)
        {
            opDeleteBodies(context, id + ("del" ~ n), { "entities" : b });
            n += 1;
        }
    }
    cleanup(context, id + "cl", id);
}
// W1 POD v1.2: the reviewed W1 form = convex hull of the PVC collar (Ø28 x 30, both ends rounded r2) and a footprint band on
// the pad (top edge rounded r2, run 1 mm under the pad surface so the trim cuts a smooth wall); the footprint also carries the
// 2 screw seats.  Built as ONE loft through POD_SECTIONS (planar sections of the exact hull, resampled from a fixed start point,
// points crowded into the round-overs: smooth, no ripples), underside trimmed to the pad + 0.3, wedge into the triangle pocket
// (carries the load), 2 M3 countersunk from OUTSIDE on flat spot-faces into nuts trapped in the pad.
// v1.2: the PVC is locked by 3 radial ST3.5 x 6.5 pan-head self-tappers (top / outward side / 60 deg below the side, half-way
// down the bore) through Ø4 holes with Ø8 flat seats; the old M3x30 through-bolt clamp (2 bolts, nut traps, lands) is gone.
// HARDWARE (v1.2): M3x25 DIN 7991 x2 (chin locks), M3x20 DIN 7991 x2 per pod, M3x12 DIN 7991 x1 per blank, M5x30 DIN 7991 x2 (receiver -> plug),
// every M screw into a hex nut; ST3.5 x 6.5 pan head x3 per pod (PVC lock). Countersink: M3 head Ø6.3 (seat Ø6.5), M5 Ø10.4 (Ø10.6).
const CSK_M3_R = 3.25;  const CSK_M5_R = 5.3;
const SEAT_CB = 3.6;  const SEAT_FL = 1.5;   // pod -> pad seats: counterbore r3.6, 45 deg rim flare 1.5 starting at POD_SEAT_RIM
const BORE_DEPTH = 26;  const BORE_CAP = 0.6;  const BORE_CH = 0.8;   // blind PVC bore (4 mm back wall), teardrop flat 0.6 over r, mouth chamfer
const K_WS = 13;                 // PVC lock screws: mm back from the collar mouth (half the bore depth)
const WS_ANG = [90, 1, -60];     // deg round the bore: +90 top, ~0 outward side (away from the pad; 1 deg = the Python mesh export), -60 below the side
const WS_SEAT = 13.3;  const WS_CB = 4.0;  const WS_CH = 0.5;   // flat Ø8 seat radius from the bore axis, seat radius, hole-mouth chamfer
const LIFT_STEPS = 4;  const LIFT_STEP = 0.26;
// pod footprint on the pad (u, z) = hull of the pocket / exit / 2 screw-seat circles (Python headrest_POD11.footprint), grown by g
function podFootprint(s is number, g is number) returns array
{
    const uf = function(a) { return -s * a * PI / 180 * R; };
    var pts = concatenateArrays([circlePts(uf(46), 1, 21 + g, 48), circlePts(uf(56), 8, 14 + g, 36)]);
    for (var sc in POD_SCREWS)
        pts = concatenateArrays([pts, circlePts(uf(BTN_ANG + sc[0] / R * 180 / PI), sc[1], 8 + g, 24)]);
    return hull2d(pts);
}
// v1.2 underside LIFT: outside the footprint the pod's underside rises to 1.3 mm over the pad in 4 x 0.26 steps (a 45 deg
// chamfer): the v1.1 trim left a feather lip / thin rear tail where the collar and hull flank came down to the pad at a
// grazing angle; now every free edge is >= 1 mm thick.  Inside the footprint the pod still sits on the pad + 0.3.
function podLift(context is Context, id is Id, scope is Id, s is number)
{
    const ua = -(s * BTN_ANG + 45) * PI / 180 * R;  const ub = -(s * BTN_ANG - 45) * PI / 180 * R;
    const box = [[min(ua, ub), -55], [max(ua, ub), -55], [max(ua, ub), 60], [min(ua, ub), 60]];
    for (var j = 1; j <= LIFT_STEPS; j += 1)
    {
        const w = 0.3 + LIFT_STEP * j;  const g = j == 1 ? -0.02 : LIFT_STEP * (j - 1);
        const jid = id + ("j" ~ j);
        try silent
        {
            bentSolid(context, jid + "slab", box, function(z, ad) { return rOut(z, ad) - 3; }, function(z, ad) { return rOut(z, ad) + w; }, 3, 3);
            bentSolid(context, jid + "keep", podFootprint(s, g), function(z, ad) { return rOut(z, ad) - 4; }, function(z, ad) { return rOut(z, ad) + 2; }, 3, 3);
            opBoolean(context, jid + "cut", { "tools" : solids(context, jid + "keep"), "targets" : solids(context, jid + "slab"), "operationType" : BooleanOperationType.SUBTRACTION });
            opBoolean(context, jid + "sub", { "tools" : solids(context, jid + "slab"), "targets" : qSubtraction(solids(context, scope), solids(context, id)), "operationType" : BooleanOperationType.SUBTRACTION });
        }
        const left = qUnion([qCreatedBy(jid + "slab", EntityType.BODY), qCreatedBy(jid + "keep", EntityType.BODY)]);   // leftovers only if a step failed
        if (size(evaluateQuery(context, left)) > 0)
            opDeleteBodies(context, jid + "del", { "entities" : left });
    }
}
export function buildPod(context is Context, id is Id, yaw is HeadrestYaw, s is number) returns Query
{
    const key = yawKey(yaw);
    const E = vector(s * POD_AXIS[key][0][0], POD_AXIS[key][0][1], POD_AXIS[key][0][2]);
    const d = normalize(vector(s * POD_AXIS[key][1][0], POD_AXIS[key][1][1], POD_AXIS[key][1][2]));   // exactly unit for opExtrude etc.
    var secs = [];
    for (var sec in POD_SECTIONS[key])
    {
        var pts = [];
        for (var k = 0; k < size(sec); k += 3) pts = append(pts, [s * sec[k], sec[k + 1], sec[k + 2]]);
        secs = append(secs, pts);
    }
    loftSections(context, id + "hull", secs);
    trimBySurface(context, id + "trim", id, s, 0.3, true);          // underside = the pad surface + 0.3
    podLift(context, id + "lift", id, s);                            // outside the footprint: lifted to 1.3 in 45 deg steps
    triPrism(context, id + "wg", -0.05, POCKET_Y0 + 0.2, R + T + 7.6 + 3.3);
    try silent { opChamfer(context, id + "wgc", { "entities" : qAdjacent(qCapEntity(id + "wg" + "ex", CapType.START), AdjacencyType.EDGE, EntityType.EDGE), "chamferType" : ChamferType.EQUAL_OFFSETS, "width" : 0.5 * millimeter }); }   // lead-in
    moveSolids(context, id + "wgm", id + "wg", placeXf(s * BTN_ANG));
    opBoolean(context, id + "uw", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const C = id + "cut";
    const pod = qSubtraction(solids(context, id), solids(context, C));
    // PVC bore: slip fit, horizontal in print -> teardrop toward print-up, tip truncated (thin collar wall), 45 deg mouth chamfer
    const rb = PVC_BORE / 2;
    tdHoleCap(context, C + "bore", E - d * BORE_DEPTH, E + d * 30, rb, podUp(s), rb + BORE_CAP);
    fCone(context, C + "bch", { "bottomCenter" : (E - d * (BORE_CH + 0.05)) * millimeter, "topCenter" : (E + d * 0.3) * millimeter, "bottomRadius" : (rb - 0.05) * millimeter, "topRadius" : (rb + BORE_CH + 0.3) * millimeter });
    // PVC lock: 3 radial Ø4 holes (teardrop toward print-up unless near vertical in print), Ø8 flat seat, 45 deg rim flare,
    // 0.5 mm chamfer at the hole mouth. Drill the PVC Ø2.5 through them, then ST3.5 x 6.5 pan heads.
    var ne = normalize(vector(sin((s * 60) * degree), cos((s * 60) * degree) / ELL, 0));
    ne = normalize(ne - d * dot(ne, d));
    var bv = vector(0, 0, 1);
    bv = normalize(bv - d * dot(bv, d) - ne * dot(bv, ne));
    const A = E - d * K_WS;
    for (var i = 0; i < size(WS_ANG); i += 1)
    {
        const v = ne * cos(WS_ANG[i] * degree) + bv * sin(WS_ANG[i] * degree);
        tdHole(context, C + ("wh" ~ i), A + v * 5, A + v * (WS_SEAT + 1), WOOD_SCREW_HOLE / 2, podUp(s));
        fCone(context, C + ("wc" ~ i), { "bottomCenter" : (A + v * (WS_SEAT - WS_CH - 0.05)) * millimeter, "topCenter" : (A + v * (WS_SEAT + 0.05)) * millimeter, "bottomRadius" : (WOOD_SCREW_HOLE / 2 - 0.05) * millimeter, "topRadius" : (WOOD_SCREW_HOLE / 2 + WS_CH + 0.05) * millimeter });
        fCylinder(context, C + ("ws" ~ i), { "bottomCenter" : (A + v * WS_SEAT) * millimeter, "topCenter" : (A + v * (WS_SEAT + 0.3)) * millimeter, "radius" : WS_CB * millimeter });
        fCone(context, C + ("wf" ~ i), { "bottomCenter" : (A + v * (WS_SEAT + 0.25)) * millimeter, "topCenter" : (A + v * (WS_SEAT + 1.85)) * millimeter, "bottomRadius" : (WS_CB - 0.05) * millimeter, "topRadius" : (WS_CB + 1.55) * millimeter });
    }
    for (var k = 0; k < size(POD_SCREWS); k += 1)
    {
        const xf = screwXf(s, POD_SCREWS[k]);  const z = POD_SCREWS[k][1];
        const yFloor = POD_SEAT_Y[k];          // seat floor along the screw axis: >= 4 mm plastic under it, M3x20 reaches through the nut
        const hr = POD_SEAT_RIM[key][k];       // lowest flank height over the seat round the counterbore (from the Python hull)
        tdHole(context, C + ("sh" ~ k), xp(xf, vector(0, R - 2, z)), xp(xf, vector(0, yFloor + 1, z)), M3_CLEAR / 2, podUp(s));
        fCylinder(context, C + ("sf" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + hr + 0.05, z)) * millimeter, "radius" : SEAT_CB * millimeter });
        fCone(context, C + ("sl" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor + hr, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + hr + SEAT_FL, z)) * millimeter, "bottomRadius" : (SEAT_CB - 0.02) * millimeter, "topRadius" : (SEAT_CB + SEAT_FL) * millimeter });
        fCylinder(context, C + ("so" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor + hr + SEAT_FL - 0.05, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + 30, z)) * millimeter, "radius" : (SEAT_CB + SEAT_FL - 0.03) * millimeter });
        fCone(context, C + ("sk" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor - (CSK_M3_R - 1.5), z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + 0.05, z)) * millimeter, "bottomRadius" : 1.5 * millimeter, "topRadius" : CSK_M3_R * millimeter });
    }
    opBoolean(context, id + "sub", { "tools" : solids(context, C), "targets" : pod, "operationType" : BooleanOperationType.SUBTRACTION });
    return solids(context, id);
}
// pocket BLANK: flush with the pad's back, 1 countersunk M3x12 from outside into the nut trapped on the head side
const BLANK_SURF_Y = 101.75;     // pad's back along the blank screw axis (local y; the oval radius rOut is 1.5 lower there)
export function buildBlank(context is Context, id is Id, s is number) returns Query
{
    triPrism(context, id + "tri", -0.1, POCKET_Y0 + 0.2, R + T + 7.6 + 7);
    moveSolids(context, id + "mv", id + "tri", placeXf(s * BTN_ANG));
    trimBySurface(context, id + "trim", id, s, 0, false);           // flush with the pad's back
    // 0.4 mm round on the flush top edge (the face furthest out along the pocket normal)
    const nb = normalize(vector(sin((s * BTN_ANG) * degree), cos((s * BTN_ANG) * degree) / ELL, 0));
    try silent { opFillet(context, id + "rt", { "entities" : qAdjacent(qFarthestAlong(qOwnedByBody(solids(context, id), EntityType.FACE), nb), AdjacencyType.EDGE, EntityType.EDGE), "radius" : 0.4 * millimeter }); }
    const xf = screwXf(s, BLANK_SCREW);  const z = BLANK_SCREW[1];
    const yTop = rOut(z, BTN_ANG);   // head top at yTop - 2 = 3.5 mm under the flush face: the M3x12 tip ends 0.25 mm inside the pad's head-side face, through the whole nut
    fCylinder(context, id + "cut" + "h", { "bottomCenter" : xp(xf, vector(0, R - 2, z)) * millimeter, "topCenter" : xp(xf, vector(0, yTop + 5, z)) * millimeter, "radius" : M3_CLEAR / 2 * millimeter });
    fCone(context, id + "cut" + "k", { "bottomCenter" : xp(xf, vector(0, yTop - 2 - (CSK_M3_R - 1.5), z)) * millimeter, "topCenter" : xp(xf, vector(0, yTop - 1.95, z)) * millimeter, "bottomRadius" : 1.5 * millimeter, "topRadius" : CSK_M3_R * millimeter });
    fCylinder(context, id + "cut" + "r", { "bottomCenter" : xp(xf, vector(0, yTop - 2, z)) * millimeter, "topCenter" : xp(xf, vector(0, yTop + 5, z)) * millimeter, "radius" : CSK_M3_R * millimeter });   // head recess (3.5 mm deep at the flush face)
    fCone(context, id + "cut" + "f", { "bottomCenter" : xp(xf, vector(0, BLANK_SURF_Y - 0.6, z)) * millimeter, "topCenter" : xp(xf, vector(0, BLANK_SURF_Y + 0.3, z)) * millimeter, "bottomRadius" : (CSK_M3_R - 0.02) * millimeter, "topRadius" : (CSK_M3_R + 0.9) * millimeter });   // 45 deg rim chamfer
    opBoolean(context, id + "sub", { "tools" : solids(context, id + "cut"), "targets" : qSubtraction(solids(context, id), solids(context, id + "cut")), "operationType" : BooleanOperationType.SUBTRACTION });
    return solids(context, id);
}

// =============================================== FEATURE ===============================================
function heightOf(h is HeadrestHeight) returns number
{
    return { (HeadrestHeight.H0) : 0, (HeadrestHeight.H25) : 25, (HeadrestHeight.H50) : 50, (HeadrestHeight.H75) : 75, (HeadrestHeight.H100) : 100 }[h];
}
function reclineXf() returns Transform
{
    return rotationAround(line(v3(PIV[0], PIV[1], PIV[2]), vector(1, 0, 0)), -TH * degree);
}
function report(context is Context, name is string, q is Query)
{
    const n = size(evaluateQuery(context, q));
    const v = n > 0 ? evVolume(context, { "entities" : q }) / millimeter ^ 3 / 1000 : 0;
    println(name ~ ": " ~ n ~ " body, " ~ roundToPrecision(v, 2) ~ " cm3");
}
function nameParts(context is Context, q is Query, name is string)
{
    for (var b in evaluateQuery(context, q))
        setProperty(context, { "entities" : b, "propertyType" : PropertyType.NAME, "value" : name });
}
// builds one complete headrest in WORLD (chair) coordinates; returns map of part queries
export function buildHeadrest(context is Context, id is Id, H is number, button is HeadrestButton, yaw is HeadrestYaw, rods is boolean, strap is boolean) returns map
{
    const zc = 451 + H;
    const pad = buildPad(context, id + "pad", strap);
    var parts = { "pad" : pad };
    var side = [];
    for (var s in [1, -1])
    {
        const want = button == HeadrestButton.BOTH || (s > 0 && button == HeadrestButton.RIGHT) || (s < 0 && button == HeadrestButton.LEFT);
        const sid = id + (s > 0 ? "R" : "L");
        if (want) parts[s > 0 ? "podR" : "podL"] = buildPod(context, sid + "pod", yaw, s);
        else parts[s > 0 ? "blankR" : "blankL"] = buildBlank(context, sid + "blank", s);
    }
    // pad-frame parts -> LOCAL
    var padFrame = [pad];
    for (var k in keys(parts)) if (k != "pad") padFrame = append(padFrame, parts[k]);
    opTransform(context, id + "toL", { "bodies" : qUnion(padFrame), "transform" : transform(v3(0, CY, zc)) });
    const mast = buildMast(context, id + "mast", H);
    opBoolean(context, id + "mp", { "tools" : pad, "targets" : mast, "operationType" : BooleanOperationType.SUBTRACTION, "keepTools" : true });
    parts["mast"] = mast;
    if (rods)
    {
        for (var x in [-SPINE_X, SPINE_X])
            fCylinder(context, id + (x < 0 ? "rodL" : "rodR"), { "bottomCenter" : v3(x, CY + SPINE_Y, SOCK_Z0 + 1), "topCenter" : v3(x, CY + SPINE_Y, zc + ROD_TOP), "radius" : ROD_D / 2 * millimeter });
        parts["rods"] = qUnion([solids(context, id + "rodL"), solids(context, id + "rodR")]);
    }
    var all = [];
    for (var k in keys(parts)) all = append(all, parts[k]);
    opTransform(context, id + "toW", { "bodies" : qUnion(all), "transform" : reclineXf() });
    return parts;
}

annotation { "Feature Type Name" : "GbG Headrest v1.1", "Feature Type Description" : "Pad F3 + mast M1 + button pod W1 for GbG_chair_forth (needs the receiver in top_back: feature GbG Headrest Receiver)" }
export const gbgHeadrestV10 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Height (mast)" }
        definition.height is HeadrestHeight;
        annotation { "Name" : "Push-button pod (head switch)" }
        definition.button is HeadrestButton;
        annotation { "Name" : "Pod angle (toward the outside)" }
        definition.yaw is HeadrestYaw;
        annotation { "Name" : "Forehead strap slots (only if the therapist prescribes a strap)" }
        definition.strapSlots is boolean;
        annotation { "Name" : "Show carbon rods (2x 8 mm)" }
        definition.showRods is boolean;
        annotation { "Name" : "Print layout: parts on the bed in print orientation, 600 mm in front of the chair" }
        definition.printSet is boolean;
        annotation { "Name" : "Review gallery: every height, pod angle and side, laid out in x (slow)" }
        definition.selfTest is boolean;
    }
    {
        if (definition.selfTest)
        {
            // REVIEW GALLERY: every printed variant in WORLD placement, shifted +400 mm in x per part (index in the name):
            // 0 pad (at H50) | 1-5 mast H0..H100 (each cut by the pad at its height) | 6-11 pod yaw 12/20/28 right, left | 12-13 blank right, left
            const padQ = buildPad(context, id + "gp", definition.strapSlots);
            const atH = function(h) { return reclineXf() * transform(v3(0, CY, 451 + h)); };
            const shift = function(i) { return transform(v3(400 * i, 0, 0)); };
            var i = 1;
            for (var h in [0, 25, 50, 75, 100])
            {
                const mid = id + ("gm" ~ h);
                buildMast(context, mid, h);
                opPattern(context, id + ("gc" ~ h), { "entities" : padQ, "transforms" : [transform(v3(0, CY, 451 + h))], "instanceNames" : ["c"] });
                opBoolean(context, id + ("gs" ~ h), { "tools" : solids(context, id + ("gc" ~ h)), "targets" : solids(context, mid), "operationType" : BooleanOperationType.SUBTRACTION });
                opTransform(context, id + ("gt" ~ h), { "bodies" : solids(context, mid), "transform" : shift(i) * reclineXf() });
                nameParts(context, solids(context, mid), "rev" ~ i ~ "_mast_H" ~ h);
                report(context, "rev" ~ i ~ " mast H" ~ h, solids(context, mid));
                i += 1;
            }
            for (var s in [1, -1])
                for (var y in [HeadrestYaw.Y12, HeadrestYaw.Y20, HeadrestYaw.Y28])
                {
                    const pid = id + ("gq" ~ yawKey(y) ~ (s > 0 ? "R" : "L"));
                    buildPod(context, pid, y, s);
                    opTransform(context, pid + "t", { "bodies" : solids(context, pid), "transform" : shift(i) * atH(50) });
                    nameParts(context, solids(context, pid), "rev" ~ i ~ "_pod_yaw" ~ yawKey(y) ~ (s > 0 ? "_right" : "_left"));
                    report(context, "rev" ~ i ~ " pod yaw " ~ yawKey(y) ~ (s > 0 ? " right" : " left"), solids(context, pid));
                    i += 1;
                }
            for (var s in [1, -1])
            {
                const bid = id + ("gb" ~ (s > 0 ? "R" : "L"));
                buildBlank(context, bid, s);
                opTransform(context, bid + "t", { "bodies" : solids(context, bid), "transform" : shift(i) * atH(50) });
                nameParts(context, solids(context, bid), "rev" ~ i ~ "_blank" ~ (s > 0 ? "_right" : "_left"));
                report(context, "rev" ~ i ~ " blank", solids(context, bid));
                i += 1;
            }
            opTransform(context, id + "gpt", { "bodies" : padQ, "transform" : atH(50) });
            nameParts(context, padQ, "rev0_pad_F3");
            report(context, "rev0 pad", padQ);
            return;
        }
        const H = heightOf(definition.height);
        const parts = buildHeadrest(context, id + "hr", H, definition.button, definition.yaw, definition.showRods && !definition.printSet, definition.strapSlots);
        const names = { "pad" : "headrest_pad_F3", "mast" : "headrest_mast_H" ~ H, "podR" : "headrest_pod_right_yaw" ~ yawKey(definition.yaw), "podL" : "headrest_pod_left_yaw" ~ yawKey(definition.yaw),
                        "blankR" : "headrest_blank_right", "blankL" : "headrest_blank_left", "rods" : "carbon_rod_8mm (buy, cut to length)" };
        for (var k in keys(parts))
        {
            nameParts(context, parts[k], names[k]);
            report(context, names[k], parts[k]);
        }
        if (definition.printSet)
        {
            // WORLD -> print orientation on z = 0 (chosen by real slices, v1.1): pad UPSIDE DOWN, mast on its side (+x down: loads run along the layers),
            // pod lying on its pad-side underside (support only on hidden faces), blank on its wedge end.
            const back = inverse(reclineXf());
            var i = 0;
            for (var k in keys(parts))
            {
                var xf = back;
                if (k == "pad") xf = rotationAround(line(v3(0, 0, 0), vector(1, 0, 0)), 180 * degree) * back;   // upside down: joint + pocket open upward (support 16 -> 10 g)
                if (k == "mast") xf = rotationAround(line(v3(0, 0, 0), vector(0, 1, 0)), 90 * degree) * back;
                if (k == "podR" || k == "podL")
                {
                    const key = yawKey(definition.yaw);  const s = k == "podR" ? 1 : -1;
                    const d = normalize(vector(s * POD_AXIS[key][1][0], POD_AXIS[key][1][1], POD_AXIS[key][1][2]));
                    xf = rotationAround(line(v3(0, 0, 0), vector(0, 1, 0)), (-90 * s) * degree) * transform(v3(0, -CY, -451 - H)) * back;   // lies on its pad-side underside (+-x down)
                }
                if (k == "blankR" || k == "blankL")
                {
                    const s = k == "blankR" ? 1 : -1;  const t = s * BTN_ANG * PI / 180;
                    const nrm = normalize(vector(sin(t * radian), cos(t * radian) / ELL, 0));
                    xf = transform(rotationMatrix3d(nrm, vector(0, 0, 1)), vector(0, 0, 0) * millimeter) * transform(v3(0, -CY, -451 - H)) * back;
                }
                opTransform(context, id + ("pr" ~ i), { "bodies" : parts[k], "transform" : xf });
                const bb = evBox3d(context, { "topology" : parts[k], "tight" : true });
                opTransform(context, id + ("pd" ~ i), { "bodies" : parts[k], "transform" : transform(vector(-(bb.minCorner[0] + bb.maxCorner[0]) / 2 + (i - 2) * 170 * millimeter, -600 * millimeter - (bb.minCorner[1] + bb.maxCorner[1]) / 2, -bb.minCorner[2])) });
                i += 1;
            }
        }
    }, { height : HeadrestHeight.H50, button : HeadrestButton.RIGHT, yaw : HeadrestYaw.Y20, strapSlots : true, showRods : true, printSet : false, selfTest : false });

// =============================================== CHAIR: RECEIVER IN top_back ===============================================
export function receiverTools(context is Context, id is Id) returns map
{
    // lens housing growing up the back (z FADE -> SOCK_Z0 - 6), full size round the socket up to the top edge, + fill of the old centre recess
    var secs = [];
    for (var z in linspace(FADE, SOCK_Z0 - 6, 16))
    {
        const t = sm((z - FADE) / (SOCK_Z0 - 6 - FADE));
        secs = append(secs, section(lerp(30, FOOT[0], t), FOOT[1], lerp(144.5, FOOT[2], t), lerp(2.2, FOOT[3], t), undefined, z));
    }
    loftSections(context, id + "lens", secs);
    const top = sectionFace(context, id + "tops", section(FOOT[0], FOOT[1], FOOT[2], FOOT[3], undefined, SOCK_Z0 - 6));
    opExtrude(context, id + "up", { "entities" : top, "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (TOP - (SOCK_Z0 - 6)) * millimeter });
    try silent { opFillet(context, id + "upf", { "entities" : qAdjacent(qCapEntity(id + "up", CapType.END), AdjacencyType.EDGE, EntityType.EDGE), "radius" : 1.5 * millimeter }); }   // housing top edge
    rrbox(context, id + "fill", PLUG[0] - 7, PLUG[1] + 7, FRONT + 0.1, 144.5, SOCK_Z0 - 6, TOP - 0.05, 2);
    cleanup(context, id + "cl", id);
    const add = qUnion([solids(context, id + "lens"), solids(context, id + "up"), solids(context, id + "fill")]);
    const C = id + "cut";
    rrbox(context, C + "sock", PLUG[0] - SOCK_CL, PLUG[1] + SOCK_CL, PLUG[2] - SOCK_CL, PLUG[3] + SOCK_CL, SOCK_Z0, TOP + 40, 6);
    // socket mouth: 1 mm x 45 deg chamfer (no sharp rim; lead-in for the plug) = a box 1 mm bigger all round from TOP - 1 up,
    // vertical edges rounded to the socket's r6 + 1, bottom loop chamfered 1 mm back to the socket outline
    try
    {
        fCuboid(context, C + "mc", { "corner1" : v3(PLUG[0] - SOCK_CL - 1, PLUG[2] - SOCK_CL - 1, TOP - 1), "corner2" : v3(PLUG[1] + SOCK_CL + 1, PLUG[3] + SOCK_CL + 1, TOP + 40) });
        opFillet(context, C + "mcf", { "entities" : qParallelEdges(qOwnedByBody(qCreatedBy(C + "mc", EntityType.BODY), EntityType.EDGE), vector(0, 0, 1)), "radius" : 7 * millimeter });
        opChamfer(context, C + "mcc", { "entities" : qAdjacent(qFarthestAlong(qOwnedByBody(qCreatedBy(C + "mc", EntityType.BODY), EntityType.FACE), vector(0, 0, -1)), AdjacencyType.EDGE, EntityType.EDGE), "chamferType" : ChamferType.EQUAL_OFFSETS, "width" : 1 * millimeter });
    }
    catch (error)
    {
        opDeleteBodies(context, C + "mcd", { "entities" : qCreatedBy(C + "mc", EntityType.BODY) });   // no chamfer rather than a raw step
    }
    for (var i = 0; i < 2; i += 1)
    {
        tdHole(context, C + ("b" ~ i), vector(0, PLUG[2] + 0.5, BOLTS_Z[i]), vector(0, PLUG[2] + 62, BOLTS_Z[i]), M5_CLEAR / 2, vector(0, 0, 1));
        fCone(context, C + ("cb" ~ i), { "bottomCenter" : vector(0, FOOT[2] - (CSK_M5_R - 2.7), BOLTS_Z[i]) * millimeter, "topCenter" : vector(0, FOOT[2] + 0.05, BOLTS_Z[i]) * millimeter, "bottomRadius" : 2.7 * millimeter, "topRadius" : CSK_M5_R * millimeter });   // M5 countersunk flush with the back
    }
    opTransform(context, id + "toW", { "bodies" : qUnion([add, solids(context, C)]), "transform" : reclineXf() });
    return { "add" : add, "cut" : solids(context, C) };
}
annotation { "Feature Type Name" : "GbG Headrest Receiver (chair)", "Feature Type Description" : "Adds the headrest mast socket (R2 receiver) to the chair's top_back part" }
export const gbgHeadrestReceiver = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "top_back part (empty = found automatically)", "Filter" : EntityType.BODY && BodyType.SOLID, "MaxNumberOfPicks" : 1 }
        definition.topBack is Query;
        annotation { "Name" : "Test only: use a stand-in block for top_back" }
        definition.standIn is boolean;
    }
    {
        var tb = definition.topBack;
        if (definition.standIn)
        {
            fCuboid(context, id + "stand", { "corner1" : v3(-92, 123.5, 270), "corner2" : v3(92, 144, TOP) });
            opTransform(context, id + "standW", { "bodies" : solids(context, id + "stand"), "transform" : reclineXf() });
            tb = solids(context, id + "stand");
        }
        else if (size(evaluateQuery(context, tb)) == 0)
        {
            // part names can't be read during regeneration: take the solid that contains points inside the backrest's top segment
            for (var p in [[-50, 134, 330], [50, 134, 330], [-70, 134, 360], [0, 134, 385]])
            {
                const q = qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), reclineXf() * v3(p[0], p[1], p[2]));
                if (size(evaluateQuery(context, q)) == 1) { tb = q; break; }
            }
        }
        if (size(evaluateQuery(context, tb)) != 1) throw regenError("No top_back part found: select it");
        const t = receiverTools(context, id + "rx");
        opBoolean(context, id + "add", { "tools" : qUnion([tb, t.add]), "operationType" : BooleanOperationType.UNION });
        const inside = reclineXf() * v3(0, 140, 330);                  // in the lens, below the socket: always material
        const merged = qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), inside);
        opBoolean(context, id + "cut", { "tools" : t.cut, "targets" : merged, "operationType" : BooleanOperationType.SUBTRACTION });
        setProperty(context, { "entities" : merged, "propertyType" : PropertyType.NAME, "value" : "top_back" });
        report(context, "top_back with receiver", merged);
    }, { topBack : qNothing(), standIn : false });

// ---------------- DATA: W1 pod hull sections + PVC axis (PAD frame, right side; generator/headrest_POD11.py) ----------------
const POD_SECTIONS = {
    "12" : [
        [56.86,111.24,-7.40,56.84,111.18,-6.27,56.83,111.10,-5.00,56.81,111.03,-3.72,56.79,110.96,-2.44,56.77,110.89,-1.16,56.75,110.82,0.11,56.73,110.74,1.39,56.71,110.67,2.65,56.69,110.60,3.79,56.60,110.55,4.46,56.52,110.50,5.06,56.25,110.38,6.15,56.21,110.37,6.29,56.17,110.35,6.38,56.13,110.34,6.48,56.07,110.33,6.50,56.01,110.31,6.53,55.77,110.26,6.61,55.66,110.23,6.65,55.56,110.21,6.68,55.48,110.19,6.69,55.43,110.18,6.64,55.38,110.18,6.59,55.22,110.15,6.43,54.92,110.11,6.03,54.62,110.07,5.59,54.46,110.06,5.18,54.30,110.05,4.65,54.07,110.05,3.78,53.96,110.07,2.97,53.79,110.10,1.74,53.62,110.13,0.47,53.46,110.16,-0.79,53.30,110.20,-2.03,53.15,110.23,-3.26,53.00,110.27,-4.54,52.85,110.30,-5.81,52.70,110.34,-7.06,52.58,110.37,-8.13,52.54,110.40,-8.94,52.56,110.43,-9.31,52.63,110.47,-9.84,52.81,110.55,-10.56,52.98,110.60,-10.90,53.24,110.68,-11.38,53.51,110.76,-11.76,53.69,110.81,-12.01,53.74,110.83,-12.04,54.21,110.94,-12.28,54.38,110.98,-12.36,54.52,111.01,-12.42,54.61,111.03,-12.40,54.75,111.06,-12.37,54.97,111.10,-12.33,55.17,111.14,-12.27,55.28,111.16,-12.11,55.39,111.17,-11.95,55.68,111.21,-11.50,56.09,111.25,-10.69,56.40,111.28,-9.96,56.60,111.29,-9.33,56.72,111.29,-8.80,56.80,111.28,-8.30],
        [63.15,109.90,-5.17,63.13,109.80,-3.42,63.11,109.69,-1.44,63.08,109.58,0.54,63.05,109.46,2.52,63.01,109.35,4.44,62.93,109.29,5.31,62.81,109.22,6.12,62.51,109.10,7.25,62.08,108.95,8.36,61.75,108.84,9.00,61.37,108.73,9.60,60.84,108.58,10.21,60.41,108.47,10.63,59.35,108.20,11.40,58.70,108.05,11.75,58.19,107.93,11.96,57.89,107.86,12.05,57.73,107.82,12.09,57.62,107.80,12.05,57.07,107.69,11.89,56.93,107.67,11.83,56.63,107.62,11.59,56.17,107.54,11.17,55.53,107.45,10.38,54.84,107.35,9.39,54.26,107.28,8.42,53.80,107.23,7.54,53.54,107.21,6.89,53.40,107.22,6.10,53.20,107.26,4.58,53.06,107.32,2.97,52.91,107.39,1.01,52.76,107.46,-0.97,52.61,107.54,-2.95,52.46,107.61,-4.93,52.31,107.69,-6.91,52.21,107.75,-8.47,52.22,107.82,-9.72,52.31,107.91,-11.02,52.45,107.99,-12.06,52.52,108.03,-12.43,52.61,108.06,-12.64,52.73,108.09,-12.80,53.35,108.26,-13.45,54.08,108.45,-14.07,54.93,108.66,-14.66,55.38,108.77,-14.88,55.99,108.91,-15.13,56.23,108.96,-15.14,56.63,109.05,-15.15,56.88,109.10,-15.13,57.12,109.14,-15.04,57.46,109.21,-14.90,58.00,109.30,-14.40,58.62,109.40,-13.79,59.53,109.54,-12.81,60.47,109.68,-11.68,61.30,109.79,-10.56,61.90,109.87,-9.59,62.34,109.92,-8.73,62.69,109.95,-7.87,62.94,109.95,-7.02,63.09,109.94,-6.20],
        [69.45,108.56,-2.94,69.43,108.44,-0.80,69.39,108.30,1.64,69.36,108.16,4.06,69.32,108.07,5.65,69.15,107.97,6.89,68.86,107.84,8.09,68.46,107.70,9.16,68.07,107.57,9.96,67.59,107.43,10.75,67.04,107.28,11.40,66.44,107.12,11.99,65.38,106.85,12.76,64.61,106.67,13.18,63.67,106.44,13.61,62.04,106.07,14.18,60.34,105.68,14.64,59.68,105.53,14.79,59.30,105.45,14.81,58.74,105.34,14.75,58.58,105.30,14.67,58.01,105.20,14.35,57.40,105.10,13.89,56.30,104.91,12.92,55.87,104.85,12.51,55.75,104.83,12.37,55.66,104.83,11.96,55.40,104.83,10.88,54.92,104.85,8.72,54.61,104.87,7.02,54.40,104.92,5.24,54.22,105.00,3.07,54.04,105.09,0.63,53.86,105.18,-1.80,53.67,105.27,-4.23,53.49,105.37,-6.67,53.44,105.43,-8.09,53.52,105.52,-9.52,53.71,105.65,-11.02,54.02,105.79,-12.58,54.46,105.98,-14.20,54.79,106.10,-15.22,54.83,106.11,-15.32,54.88,106.13,-15.42,55.06,106.17,-15.50,55.26,106.22,-15.59,56.37,106.48,-16.02,57.45,106.73,-16.34,58.09,106.87,-16.42,58.43,106.94,-16.42,59.07,107.07,-16.31,59.27,107.11,-16.21,59.91,107.22,-15.83,61.24,107.45,-14.91,62.68,107.70,-13.80,63.87,107.90,-12.78,65.13,108.10,-11.55,66.20,108.27,-10.37,67.11,108.40,-9.22,67.86,108.50,-8.10,68.53,108.57,-6.89,68.92,108.61,-5.94,69.21,108.62,-4.99,69.38,108.60,-4.05],
        [75.75,107.22,-0.71,75.72,107.08,1.77,75.68,106.92,4.58,75.64,106.81,6.53,75.47,106.70,7.82,75.16,106.57,9.07,74.71,106.41,10.27,74.26,106.27,11.16,73.73,106.11,12.00,73.03,105.92,12.80,72.36,105.74,13.43,71.29,105.47,14.20,70.18,105.20,14.79,68.97,104.92,15.27,67.46,104.57,15.76,65.78,104.20,16.17,63.82,103.76,16.51,61.83,103.33,16.74,60.68,103.08,16.79,60.36,103.01,16.78,59.79,102.90,16.57,59.09,102.77,16.20,58.02,102.58,15.52,57.71,102.53,15.31,57.46,102.48,15.13,57.42,102.48,15.01,57.22,102.48,14.31,56.70,102.48,12.22,56.15,102.49,9.78,55.83,102.52,7.96,55.60,102.59,5.84,55.40,102.68,3.22,55.18,102.79,0.40,54.97,102.89,-2.41,54.76,103.00,-5.23,54.68,103.08,-6.95,54.76,103.17,-8.47,54.98,103.31,-10.09,55.35,103.48,-11.77,55.95,103.71,-13.82,56.46,103.91,-15.43,56.80,104.02,-16.18,56.99,104.08,-16.46,57.07,104.10,-16.54,57.16,104.12,-16.64,57.20,104.13,-16.65,57.94,104.29,-16.77,59.69,104.68,-17.02,60.44,104.84,-17.04,60.89,104.93,-16.97,61.34,105.02,-16.85,61.88,105.12,-16.53,63.97,105.49,-15.20,66.23,105.89,-13.73,67.99,106.20,-12.51,69.35,106.43,-11.45,70.91,106.69,-10.06,71.90,106.85,-9.04,73.15,107.03,-7.53,73.94,107.14,-6.42,74.66,107.22,-5.15,75.15,107.27,-4.01,75.48,107.28,-2.96,75.68,107.27,-1.90],
        [82.05,105.88,1.52,82.02,105.72,4.31,81.97,105.56,7.14,81.82,105.46,8.49,81.53,105.32,9.82,81.11,105.17,11.02,80.62,105.01,12.06,79.99,104.82,13.08,79.27,104.62,13.95,78.46,104.41,14.72,77.36,104.13,15.53,76.28,103.87,16.13,74.98,103.57,16.67,73.31,103.18,17.21,71.45,102.76,17.64,69.30,102.29,17.95,66.93,101.77,18.16,64.35,101.22,18.27,62.61,100.85,18.33,61.76,100.67,18.29,61.21,100.56,18.14,59.78,100.29,17.49,59.40,100.22,17.30,59.12,100.17,17.15,59.05,100.16,17.02,58.99,100.15,16.89,58.49,100.12,15.45,57.94,100.12,13.38,57.35,100.14,10.75,57.04,100.17,8.84,56.80,100.26,6.27,56.56,100.37,3.27,56.32,100.49,0.10,56.08,100.61,-3.08,55.92,100.70,-5.48,55.97,100.79,-7.02,56.17,100.93,-8.70,56.55,101.10,-10.47,57.24,101.37,-12.80,58.03,101.68,-15.33,58.37,101.80,-16.18,58.66,101.88,-16.66,58.96,101.96,-16.98,59.17,102.02,-17.18,59.20,102.02,-17.19,59.26,102.04,-17.20,61.37,102.50,-17.51,62.35,102.72,-17.59,62.73,102.80,-17.58,63.37,102.92,-17.42,63.67,102.98,-17.26,65.76,103.35,-15.96,68.39,103.82,-14.30,70.98,104.29,-12.64,73.50,104.73,-11.01,75.00,104.99,-9.91,76.63,105.27,-8.57,77.99,105.49,-7.25,79.06,105.65,-5.99,80.11,105.79,-4.58,80.80,105.88,-3.40,81.38,105.93,-2.09,81.74,105.95,-0.94,81.97,105.93,0.24],
        [88.35,104.54,3.75,88.31,104.37,6.82,88.24,104.25,8.63,88.02,104.13,10.05,87.64,103.98,11.41,87.10,103.79,12.72,86.40,103.58,13.94,85.72,103.39,14.84,84.76,103.13,15.84,83.75,102.87,16.65,82.68,102.61,17.31,81.39,102.30,17.92,79.55,101.88,18.55,77.80,101.48,18.97,75.51,100.98,19.30,73.12,100.46,19.49,70.11,99.81,19.60,67.12,99.18,19.62,64.31,98.58,19.57,63.01,98.31,19.46,62.26,98.16,19.24,61.22,97.96,18.86,60.84,97.89,18.72,60.68,97.86,18.62,60.63,97.86,18.54,60.20,97.81,17.70,59.70,97.77,16.37,59.15,97.77,14.34,58.54,97.78,11.63,58.25,97.83,9.66,57.97,97.93,6.53,57.71,98.06,3.15,57.45,98.19,-0.36,57.19,98.32,-3.74,57.17,98.40,-5.36,57.33,98.52,-7.05,57.68,98.70,-8.86,58.36,98.97,-11.23,59.34,99.35,-14.40,59.92,99.56,-16.11,60.21,99.65,-16.68,60.38,99.70,-16.92,61.15,99.91,-17.69,61.18,99.92,-17.72,61.23,99.93,-17.74,62.18,100.14,-17.88,64.00,100.54,-18.12,64.67,100.68,-18.12,65.29,100.80,-17.98,65.71,100.88,-17.77,66.95,101.10,-17.01,69.92,101.64,-15.16,72.87,102.16,-13.32,75.78,102.68,-11.49,78.62,103.19,-9.68,80.80,103.58,-8.22,82.49,103.86,-6.91,83.88,104.09,-5.65,85.10,104.28,-4.30,86.25,104.44,-2.77,86.98,104.53,-1.55,87.61,104.59,-0.18,88.01,104.61,1.09,88.26,104.59,2.38],
        [94.65,103.20,5.98,94.61,103.04,8.86,94.45,102.92,10.36,94.12,102.77,11.81,93.62,102.59,13.21,93.00,102.39,14.46,92.23,102.17,15.63,91.31,101.91,16.70,90.30,101.65,17.61,89.09,101.35,18.46,87.76,101.03,19.16,86.05,100.63,19.80,84.37,100.25,20.26,82.36,99.81,20.58,79.91,99.27,20.78,76.87,98.62,20.86,73.59,97.93,20.83,70.08,97.19,20.71,66.55,96.45,20.56,64.50,96.02,20.43,63.26,95.77,20.11,62.63,95.65,19.94,62.33,95.59,19.86,62.32,95.59,19.85,61.85,95.52,19.23,61.35,95.46,18.37,60.87,95.42,17.15,60.33,95.41,15.19,59.73,95.43,12.48,59.45,95.49,10.25,59.14,95.62,6.67,58.86,95.76,2.93,58.57,95.90,-0.93,58.40,96.01,-3.58,58.48,96.11,-5.20,58.76,96.27,-7.00,59.30,96.49,-9.08,60.39,96.91,-12.60,61.38,97.29,-15.76,61.75,97.42,-16.66,62.02,97.50,-17.09,62.57,97.65,-17.66,63.16,97.80,-18.23,63.21,97.82,-18.27,63.25,97.83,-18.28,65.37,98.29,-18.58,66.33,98.50,-18.64,66.71,98.58,-18.64,67.35,98.71,-18.46,68.41,98.90,-17.83,70.86,99.34,-16.32,74.13,99.92,-14.31,77.38,100.51,-12.30,80.62,101.09,-10.28,83.79,101.66,-8.30,86.73,102.18,-6.41,88.43,102.47,-5.15,90.00,102.73,-3.80,91.20,102.92,-2.52,92.38,103.09,-0.98,93.18,103.19,0.32,93.83,103.25,1.73,94.27,103.27,3.11,94.55,103.26,4.51],
        [100.95,101.86,8.21,100.89,101.74,10.29,100.66,101.60,11.84,100.24,101.43,13.35,99.65,101.23,14.80,98.88,101.00,16.15,97.99,100.74,17.36,96.92,100.45,18.46,95.71,100.15,19.43,94.41,99.83,20.23,93.00,99.49,20.87,91.11,99.06,21.49,89.43,98.68,21.83,86.99,98.15,22.05,84.28,97.57,22.11,80.75,96.83,22.00,76.94,96.03,21.82,72.89,95.18,21.59,68.85,94.33,21.32,66.07,93.75,21.14,64.22,93.38,20.81,63.94,93.32,20.73,63.77,93.29,20.63,63.49,93.25,20.36,62.93,93.16,19.71,62.47,93.11,18.94,62.01,93.07,17.86,61.49,93.06,15.97,60.93,93.08,13.35,60.64,93.16,10.63,60.30,93.30,6.72,59.99,93.45,2.61,59.68,93.61,-1.57,59.65,93.70,-3.24,59.83,93.83,-4.97,60.24,94.01,-6.87,61.26,94.41,-10.22,62.45,94.87,-14.06,63.20,95.15,-16.38,63.53,95.26,-17.07,63.75,95.32,-17.38,64.98,95.65,-18.59,65.19,95.70,-18.78,65.23,95.72,-18.81,65.54,95.78,-18.87,67.77,96.27,-19.15,68.45,96.42,-19.16,68.90,96.51,-19.06,69.61,96.64,-18.74,71.22,96.93,-17.76,74.79,97.57,-15.58,78.35,98.21,-13.39,81.92,98.86,-11.21,85.47,99.49,-9.03,88.98,100.12,-6.86,92.33,100.72,-4.77,94.31,101.07,-3.43,96.07,101.37,-1.99,97.31,101.56,-0.72,98.55,101.74,0.86,99.38,101.85,2.20,100.05,101.91,3.64,100.54,101.94,5.13,100.84,101.92,6.65],
        [102.58,101.51,8.79,102.51,101.40,10.67,102.26,101.26,12.24,101.83,101.09,13.76,101.21,100.88,15.21,100.41,100.63,16.57,99.45,100.36,17.82,98.38,100.08,18.91,97.15,99.76,19.88,95.84,99.44,20.66,94.40,99.10,21.29,92.50,98.67,21.90,90.81,98.29,22.21,88.37,97.76,22.40,85.53,97.16,22.41,81.76,96.36,22.26,77.74,95.52,22.04,73.64,94.66,21.77,69.45,93.79,21.49,66.41,93.15,21.29,64.75,92.82,21.00,64.35,92.74,20.93,64.13,92.70,20.79,63.81,92.64,20.51,63.21,92.55,19.82,62.75,92.50,19.08,62.31,92.46,18.03,61.79,92.45,16.16,61.24,92.47,13.59,60.95,92.56,10.72,60.61,92.70,6.73,60.29,92.86,2.53,59.99,93.01,-1.49,59.98,93.10,-3.14,60.19,93.24,-4.90,60.64,93.43,-6.85,61.77,93.87,-10.52,62.99,94.34,-14.45,63.69,94.60,-16.56,64.01,94.70,-17.18,64.20,94.76,-17.45,65.62,95.14,-18.85,65.71,95.16,-18.93,65.75,95.17,-18.95,66.10,95.25,-19.01,68.33,95.74,-19.29,68.98,95.87,-19.28,69.70,96.02,-19.09,70.02,96.07,-18.91,72.16,96.46,-17.60,75.81,97.12,-15.37,79.45,97.77,-13.15,83.09,98.43,-10.92,86.73,99.08,-8.69,90.34,99.73,-6.47,93.81,100.35,-4.32,95.89,100.72,-2.93,97.68,101.02,-1.48,98.90,101.21,-0.24,100.16,101.40,1.35,100.99,101.50,2.69,101.67,101.57,4.13,102.16,101.59,5.65,102.47,101.57,7.20],
        [105.24,100.81,9.54,105.16,100.70,11.20,104.89,100.56,12.80,104.40,100.37,14.41,103.73,100.15,15.92,102.93,99.90,17.27,101.87,99.61,18.62,100.71,99.30,19.77,99.52,98.99,20.70,98.01,98.63,21.58,96.52,98.28,22.22,94.95,97.92,22.66,93.35,97.56,22.90,90.99,97.06,23.03,87.66,96.36,22.92,83.68,95.52,22.68,79.39,94.63,22.38,75.04,93.72,22.07,70.83,92.84,21.77,66.91,92.02,21.48,65.14,91.66,21.25,64.97,91.62,21.19,64.73,91.58,21.05,64.40,91.53,20.78,63.73,91.42,20.04,63.29,91.37,19.35,62.87,91.33,18.39,62.36,91.32,16.63,61.83,91.34,14.10,61.53,91.44,11.05,61.18,91.58,6.93,60.85,91.75,2.57,60.57,91.89,-1.24,60.59,91.98,-2.87,60.84,92.13,-4.65,61.38,92.36,-6.77,62.65,92.85,-10.85,63.91,93.33,-14.92,64.54,93.57,-16.77,64.86,93.67,-17.35,65.09,93.73,-17.65,66.59,94.13,-19.12,66.68,94.15,-19.19,66.72,94.16,-19.21,67.09,94.24,-19.27,69.24,94.71,-19.52,69.89,94.85,-19.51,70.34,94.94,-19.37,71.79,95.20,-18.56,74.87,95.76,-16.69,77.94,96.31,-14.83,81.20,96.90,-12.84,84.97,97.58,-10.55,88.73,98.25,-8.25,92.49,98.93,-5.95,96.24,99.60,-3.66,98.75,100.05,-2.06,100.49,100.35,-0.69,101.68,100.54,0.46,102.72,100.69,1.75,103.59,100.80,3.16,104.29,100.87,4.67,104.81,100.89,6.25,105.13,100.87,7.90],
        [105.75,100.30,9.51,105.67,100.20,11.19,105.38,100.05,12.86,104.90,99.86,14.47,104.23,99.63,16.00,103.38,99.37,17.44,102.36,99.09,18.76,101.19,98.78,19.95,99.88,98.44,20.97,98.46,98.10,21.82,96.95,97.74,22.49,95.36,97.38,22.96,93.73,97.01,23.24,92.08,96.66,23.30,88.66,95.94,23.13,84.38,95.05,22.82,80.10,94.15,22.51,75.81,93.26,22.20,71.52,92.36,21.89,67.23,91.47,21.59,65.64,91.14,21.39,65.25,91.06,21.31,65.01,91.02,21.17,64.68,90.96,20.91,63.99,90.86,20.14,63.55,90.80,19.48,63.14,90.76,18.56,62.65,90.75,16.86,62.13,90.77,14.38,61.83,90.87,11.27,61.47,91.02,7.12,61.14,91.18,2.74,60.86,91.33,-1.04,60.89,91.42,-2.65,61.14,91.57,-4.43,61.73,91.81,-6.64,63.00,92.30,-10.74,64.27,92.79,-14.85,64.93,93.04,-16.80,65.25,93.14,-17.39,65.48,93.20,-17.68,67.07,93.62,-19.24,67.16,93.65,-19.31,67.20,93.66,-19.34,67.75,93.78,-19.42,69.54,94.17,-19.63,70.22,94.31,-19.60,70.74,94.42,-19.46,72.18,94.68,-18.66,75.44,95.27,-16.68,78.71,95.85,-14.70,81.98,96.44,-12.72,85.41,97.06,-10.63,89.21,97.74,-8.33,93.01,98.43,-6.02,96.76,99.10,-3.73,99.63,99.62,-1.95,100.92,99.84,-0.96,102.15,100.04,0.25,103.20,100.19,1.57,104.09,100.30,3.01,104.79,100.37,4.55,105.31,100.39,6.16,105.63,100.37,7.82],
        [106.03,99.75,9.48,105.94,99.64,11.18,105.66,99.49,12.85,105.17,99.30,14.48,104.50,99.08,16.02,103.65,98.82,17.47,102.62,98.53,18.82,101.45,98.21,20.00,100.14,97.88,21.04,98.71,97.53,21.90,97.19,97.17,22.58,95.60,96.81,23.07,93.93,96.44,23.35,92.30,96.08,23.43,89.05,95.41,23.22,84.69,94.50,22.91,80.32,93.58,22.59,75.95,92.67,22.27,71.59,91.76,21.96,67.27,90.86,21.64,65.77,90.55,21.47,65.52,90.50,21.42,65.28,90.46,21.29,64.95,90.40,21.02,64.22,90.29,20.22,63.80,90.23,19.58,63.40,90.20,18.69,62.92,90.19,17.01,62.42,90.21,14.60,62.11,90.31,11.38,61.76,90.46,7.22,61.43,90.63,2.84,61.16,90.77,-0.86,61.20,90.86,-2.45,61.46,91.01,-4.22,62.08,91.27,-6.55,63.36,91.76,-10.66,64.64,92.25,-14.78,65.32,92.51,-16.82,65.64,92.61,-17.42,65.86,92.67,-17.71,67.51,93.11,-19.33,67.64,93.14,-19.44,67.68,93.15,-19.47,67.97,93.21,-19.51,69.98,93.65,-19.71,70.65,93.79,-19.68,70.98,93.86,-19.60,72.29,94.10,-18.93,75.77,94.73,-16.82,79.25,95.35,-14.70,82.73,95.98,-12.59,86.20,96.61,-10.48,89.68,97.23,-8.36,93.32,97.89,-6.15,97.04,98.56,-3.88,99.89,99.07,-2.08,101.20,99.29,-1.06,102.41,99.48,0.14,103.47,99.64,1.47,104.36,99.75,2.93,105.06,99.81,4.47,105.59,99.84,6.10,105.91,99.82,7.78],
        [106.46,97.85,9.37,106.37,97.74,11.08,106.09,97.59,12.77,105.60,97.40,14.40,104.92,97.17,15.96,104.05,96.91,17.41,103.08,96.63,18.69,101.84,96.30,19.95,100.61,95.99,20.92,99.19,95.64,21.79,97.67,95.28,22.48,96.09,94.92,22.97,94.45,94.55,23.26,92.42,94.12,23.35,89.17,93.43,23.25,84.93,92.54,23.02,80.71,91.66,22.75,76.37,90.75,22.45,72.07,89.86,22.15,67.80,88.96,21.84,66.47,88.69,21.73,66.22,88.64,21.69,66.13,88.62,21.64,65.82,88.57,21.38,64.96,88.44,20.44,64.59,88.39,19.86,64.23,88.36,19.05,63.76,88.35,17.31,63.36,88.37,15.25,63.03,88.50,11.54,62.68,88.65,7.39,62.34,88.82,2.99,62.11,88.94,-0.30,62.18,89.04,-1.83,62.48,89.20,-3.58,63.26,89.51,-6.30,64.54,90.00,-10.42,65.82,90.49,-14.54,66.58,90.78,-16.87,66.91,90.89,-17.54,67.14,90.95,-17.85,68.72,91.37,-19.40,69.10,91.47,-19.78,69.12,91.48,-19.79,69.24,91.50,-19.80,71.04,91.89,-19.90,71.78,92.05,-19.88,72.12,92.12,-19.84,72.86,92.26,-19.53,75.73,92.77,-17.78,78.84,93.33,-15.88,82.61,94.01,-13.57,86.36,94.68,-11.26,90.11,95.36,-8.95,93.81,96.02,-6.65,97.24,96.63,-4.48,99.37,97.01,-3.04,101.61,97.39,-1.20,102.82,97.58,-0.00,103.92,97.74,1.39,104.77,97.84,2.79,105.49,97.91,4.35,106.02,97.94,6.01,106.34,97.92,7.66],
        [106.98,95.41,9.24,106.89,95.30,10.95,106.61,95.15,12.63,106.12,94.96,14.25,105.44,94.73,15.81,104.59,94.47,17.26,103.62,94.20,18.53,102.38,93.87,19.80,101.16,93.55,20.77,99.75,93.21,21.64,98.24,92.85,22.33,96.66,92.49,22.83,95.03,92.13,23.13,93.03,91.69,23.22,89.52,90.95,23.20,85.49,90.10,23.01,81.19,89.20,22.78,76.90,88.30,22.54,72.58,87.40,22.28,69.38,86.73,22.08,68.10,86.46,21.99,67.02,86.24,21.92,66.99,86.23,21.89,66.95,86.22,21.85,65.91,86.06,20.73,65.60,86.02,20.24,65.30,86.00,19.52,64.83,86.00,17.67,64.57,86.03,15.98,64.20,86.18,11.77,63.85,86.33,7.63,63.52,86.49,3.24,63.35,86.60,0.51,63.46,86.71,-1.00,63.78,86.87,-2.71,64.75,87.25,-5.92,66.03,87.74,-10.03,67.30,88.23,-14.14,68.19,88.56,-16.92,68.54,88.68,-17.68,68.80,88.75,-18.04,70.00,89.07,-19.24,70.72,89.26,-19.91,70.78,89.28,-19.96,70.82,89.29,-19.97,71.54,89.44,-19.98,73.22,89.80,-19.98,73.78,89.91,-19.90,74.41,90.03,-19.64,76.66,90.44,-18.25,79.77,91.00,-16.33,83.50,91.66,-14.02,87.21,92.33,-11.72,90.87,92.98,-9.43,94.38,93.61,-7.19,97.21,94.11,-5.33,99.43,94.49,-3.55,102.15,94.95,-1.31,103.36,95.14,-0.11,104.45,95.30,1.28,105.30,95.40,2.68,106.01,95.47,4.23,106.54,95.49,5.89,106.86,95.47,7.54],
        [107.50,92.97,9.11,107.41,92.86,10.81,107.13,92.71,12.48,106.65,92.52,14.10,105.98,92.29,15.65,105.12,92.03,17.11,104.16,91.76,18.38,102.94,91.43,19.63,101.71,91.12,20.61,100.31,90.77,21.49,98.80,90.42,22.18,97.21,90.05,22.69,95.58,89.69,22.99,93.71,89.29,23.09,89.58,88.41,23.08,86.08,87.67,22.97,81.92,86.80,22.78,77.63,85.90,22.54,73.34,85.00,22.31,69.05,84.10,22.07,67.86,83.85,22.00,67.78,83.83,22.00,67.75,83.83,21.98,67.59,83.80,21.81,66.86,83.69,21.02,66.62,83.66,20.63,66.38,83.64,20.02,65.98,83.64,18.38,65.74,83.71,16.19,65.38,83.86,11.98,65.03,84.01,7.85,64.70,84.17,3.51,64.59,84.26,1.34,64.73,84.37,-0.14,65.10,84.54,-1.83,66.24,84.98,-5.54,67.51,85.47,-9.62,68.78,85.96,-13.71,69.80,86.35,-16.97,70.17,86.47,-17.81,70.44,86.55,-18.22,71.35,86.80,-19.13,72.16,87.01,-19.91,72.24,87.03,-19.96,72.32,87.05,-19.99,72.55,87.10,-20.00,74.67,87.55,-20.00,75.36,87.69,-19.91,75.79,87.77,-19.73,77.05,88.00,-19.00,80.74,88.66,-16.70,84.41,89.32,-14.41,88.03,89.97,-12.13,91.57,90.60,-9.87,94.84,91.18,-7.74,96.72,91.50,-6.33,100.00,92.06,-3.64,102.71,92.51,-1.41,103.90,92.70,-0.21,104.99,92.86,1.17,105.83,92.96,2.57,106.54,93.03,4.12,107.06,93.05,5.77,107.38,93.03,7.41],
        [108.01,90.52,8.98,107.93,90.42,10.67,107.65,90.27,12.34,107.17,90.08,13.95,106.51,89.85,15.49,105.66,89.60,16.94,104.69,89.32,18.23,103.50,89.00,19.46,102.25,88.68,20.47,100.84,88.34,21.35,99.34,87.98,22.04,97.76,87.62,22.55,96.14,87.26,22.86,94.40,86.88,22.96,90.26,86.00,22.95,86.59,85.22,22.93,82.58,84.38,22.74,78.41,83.50,22.55,75.86,82.97,22.41,73.37,82.45,22.27,70.88,81.93,22.13,68.46,81.42,22.00,68.44,81.42,21.99,68.36,81.40,21.90,67.82,81.32,21.31,67.65,81.30,21.03,67.47,81.28,20.56,67.18,81.29,19.30,66.91,81.39,16.37,66.55,81.54,12.18,66.21,81.69,8.06,65.88,81.85,3.76,65.84,81.92,2.17,66.01,82.04,0.72,66.47,82.23,-1.09,67.73,82.72,-5.16,68.99,83.20,-9.22,70.25,83.69,-13.29,71.41,84.13,-16.98,71.79,84.27,-17.91,72.08,84.35,-18.38,72.73,84.53,-19.06,73.61,84.76,-19.91,73.69,84.78,-19.96,73.76,84.80,-19.98,74.17,84.88,-20.00,76.21,85.32,-20.00,76.79,85.44,-19.92,77.13,85.50,-19.82,78.66,85.78,-18.94,81.68,86.32,-17.05,85.28,86.96,-14.78,88.82,87.60,-12.52,92.27,88.21,-10.27,94.47,88.59,-8.73,97.31,89.07,-6.41,100.59,89.63,-3.71,103.27,90.08,-1.49,104.45,90.26,-0.31,105.52,90.42,1.06,106.37,90.52,2.47,107.07,90.59,4.01,107.58,90.61,5.65,107.90,90.59,7.29],
        [108.53,88.08,8.85,108.45,87.97,10.53,108.17,87.83,12.19,107.70,87.64,13.80,107.04,87.42,15.33,106.21,87.16,16.77,105.22,86.88,18.08,104.06,86.57,19.29,102.79,86.25,20.32,101.39,85.90,21.20,99.90,85.55,21.90,98.33,85.19,22.41,96.71,84.83,22.72,95.04,84.47,22.83,90.96,83.60,22.82,86.82,82.72,22.81,83.34,81.98,22.71,79.14,81.10,22.51,74.98,80.23,22.32,72.90,79.79,22.21,71.05,79.40,22.11,69.20,79.02,22.00,69.13,79.00,21.99,69.11,79.00,21.97,68.78,78.95,21.61,68.69,78.94,21.45,68.58,78.93,21.15,68.41,78.94,20.37,68.08,79.07,16.54,67.72,79.22,12.38,67.39,79.37,8.28,67.09,79.52,4.29,67.10,79.59,3.02,67.30,79.71,1.61,67.97,79.97,-0.74,69.22,80.46,-4.78,70.48,80.94,-8.82,71.73,81.42,-12.85,72.97,81.90,-16.86,73.41,82.06,-18.02,73.71,82.15,-18.53,74.14,82.26,-19.02,75.06,82.51,-19.91,75.14,82.53,-19.96,75.21,82.54,-19.99,75.67,82.64,-20.00,77.71,83.08,-20.00,78.11,83.16,-19.94,78.71,83.28,-19.78,79.46,83.41,-19.37,82.60,83.97,-17.38,86.12,84.60,-15.12,89.58,85.22,-12.88,92.16,85.67,-11.14,94.65,86.10,-9.14,97.93,86.65,-6.46,101.20,87.20,-3.77,103.83,87.64,-1.59,105.01,87.83,-0.40,106.05,87.98,0.94,106.90,88.08,2.37,107.59,88.15,3.91,108.10,88.17,5.52,108.42,88.15,7.17],
        [109.05,85.64,8.72,108.97,85.53,10.39,108.70,85.39,12.04,108.23,85.20,13.64,107.58,84.98,15.17,106.75,84.73,16.60,105.75,84.44,17.94,104.62,84.14,19.12,103.34,83.81,20.17,101.95,83.47,21.05,100.47,83.12,21.75,98.92,82.76,22.26,97.31,82.40,22.58,95.67,82.05,22.70,91.67,81.20,22.69,87.55,80.32,22.68,83.81,79.53,22.66,79.95,78.72,22.48,75.77,77.84,22.29,71.63,76.97,22.09,70.42,76.71,22.03,69.87,76.60,22.00,69.82,76.59,22.00,69.82,76.59,22.00,69.75,76.58,21.92,69.73,76.58,21.89,69.71,76.57,21.82,69.61,76.61,20.86,69.25,76.75,16.72,68.89,76.90,12.58,68.56,77.04,8.50,68.31,77.18,5.06,68.35,77.25,3.89,68.59,77.37,2.53,69.47,77.71,-0.38,70.71,78.19,-4.39,71.96,78.67,-8.40,73.20,79.15,-12.42,74.45,79.63,-16.43,75.02,79.84,-18.07,75.34,79.94,-18.67,75.65,80.03,-19.07,76.52,80.26,-19.91,76.59,80.28,-19.96,76.65,80.29,-19.99,77.10,80.39,-20.00,79.14,80.82,-20.00,79.72,80.94,-19.92,80.03,81.00,-19.82,80.91,81.16,-19.34,83.50,81.62,-17.69,86.95,82.24,-15.45,89.89,82.76,-13.49,92.04,83.13,-11.84,95.30,83.67,-9.17,98.55,84.22,-6.50,101.81,84.77,-3.83,104.39,85.21,-1.69,105.56,85.39,-0.49,106.57,85.54,0.82,107.44,85.64,2.27,108.12,85.71,3.80,108.62,85.73,5.39,108.94,85.70,7.05],
        [109.57,83.20,8.59,109.49,83.09,10.25,109.22,82.95,11.88,108.76,82.76,13.47,108.12,82.55,14.99,107.30,82.29,16.43,106.30,82.01,17.78,105.18,81.71,18.96,103.91,81.38,20.00,102.54,81.04,20.88,101.07,80.69,21.58,99.53,80.34,22.10,97.94,79.98,22.43,96.30,79.63,22.57,92.47,78.81,22.56,88.38,77.94,22.55,84.28,77.08,22.54,81.04,76.39,22.47,77.59,75.67,22.31,74.15,74.94,22.14,71.05,74.29,22.00,71.04,74.29,22.00,71.04,74.29,21.99,71.03,74.29,21.99,71.02,74.29,21.98,70.90,74.27,21.76,70.82,74.27,21.53,70.55,74.38,18.44,70.29,74.48,15.36,70.02,74.59,12.27,69.73,74.73,8.53,69.55,74.84,5.84,69.63,74.91,4.72,69.91,75.04,3.40,71.02,75.47,-0.19,72.25,75.95,-4.16,73.48,76.42,-8.14,74.71,76.90,-12.11,75.95,77.37,-16.08,76.63,77.63,-18.14,76.99,77.74,-18.85,77.29,77.83,-19.24,77.98,78.01,-19.92,78.04,78.03,-19.96,78.11,78.04,-19.99,78.50,78.12,-20.00,80.46,78.54,-20.00,80.85,78.62,-19.95,81.35,78.72,-19.84,82.01,78.84,-19.52,84.45,79.28,-17.93,87.62,79.84,-15.84,89.67,80.20,-14.34,92.78,80.72,-11.79,96.00,81.26,-9.15,99.23,81.81,-6.50,102.46,82.35,-3.85,104.95,82.77,-1.78,106.11,82.95,-0.58,107.11,83.10,0.71,107.98,83.20,2.19,108.66,83.26,3.71,109.14,83.28,5.26,109.46,83.26,6.93],
        [110.09,80.76,8.46,110.01,80.65,10.10,109.75,80.51,11.72,109.30,80.33,13.30,108.66,80.11,14.81,107.84,79.86,16.26,106.85,79.58,17.60,105.72,79.27,18.80,104.50,78.96,19.82,103.14,78.62,20.70,101.70,78.28,21.41,100.14,77.92,21.95,98.54,77.56,22.29,96.90,77.20,22.43,93.29,76.44,22.43,89.24,75.58,22.42,85.19,74.72,22.41,81.38,73.91,22.40,78.32,73.26,22.27,75.12,72.59,22.12,72.50,72.04,22.00,72.47,72.03,21.99,72.45,72.03,21.98,72.43,72.03,21.96,72.39,72.02,21.92,72.01,71.98,21.23,71.94,71.97,21.08,71.69,72.07,18.25,71.44,72.17,15.29,71.18,72.28,12.32,70.88,72.42,8.49,70.79,72.50,6.66,70.91,72.58,5.57,71.37,72.77,3.86,72.58,73.24,-0.07,73.80,73.71,-3.99,75.02,74.18,-7.92,76.24,74.65,-11.85,77.46,75.12,-15.78,78.28,75.43,-18.28,78.63,75.54,-19.00,78.92,75.62,-19.41,79.45,75.76,-19.93,79.49,75.77,-19.96,79.56,75.79,-19.99,79.87,75.86,-20.00,81.59,76.22,-20.00,82.26,76.36,-19.94,82.77,76.46,-19.79,83.16,76.54,-19.62,85.32,76.92,-18.19,87.70,77.33,-16.50,90.35,77.78,-14.34,93.54,78.32,-11.72,96.74,78.86,-9.10,99.93,79.40,-6.49,103.12,79.93,-3.87,105.52,80.34,-1.87,106.66,80.52,-0.68,107.65,80.66,0.62,108.52,80.76,2.10,109.19,80.82,3.62,109.66,80.84,5.13,109.98,80.82,6.81],
        [110.61,78.31,8.33,110.53,78.21,9.96,110.27,78.07,11.56,109.83,77.89,13.12,109.20,77.67,14.65,108.38,77.42,16.10,107.40,77.14,17.45,106.26,76.83,18.66,105.10,76.54,19.63,103.75,76.20,20.52,102.28,75.85,21.25,100.73,75.49,21.80,99.11,75.13,22.15,97.47,74.77,22.30,94.12,74.06,22.30,90.12,73.21,22.29,86.11,72.36,22.28,82.10,71.51,22.27,78.82,70.81,22.23,76.01,70.22,22.10,73.94,69.79,22.00,73.90,69.78,21.99,73.87,69.77,21.96,73.83,69.77,21.93,73.76,69.76,21.85,73.11,69.68,20.69,73.05,69.68,20.56,72.83,69.76,18.06,72.59,69.86,15.23,72.34,69.96,12.40,72.05,70.11,8.55,72.04,70.16,7.49,72.20,70.25,6.45,72.95,70.54,3.94,74.15,71.01,0.06,75.36,71.47,-3.82,76.56,71.94,-7.70,77.77,72.40,-11.58,78.97,72.86,-15.47,79.87,73.21,-18.29,80.24,73.33,-19.09,80.55,73.42,-19.57,80.91,73.51,-19.94,80.94,73.52,-19.96,81.02,73.54,-20.00,81.16,73.57,-20.00,82.49,73.85,-20.00,83.69,74.10,-19.92,83.82,74.13,-19.89,84.32,74.22,-19.65,85.46,74.43,-18.86,88.01,74.86,-16.82,91.16,75.39,-14.23,94.32,75.92,-11.64,97.47,76.45,-9.06,100.63,76.98,-6.47,103.78,77.52,-3.88,106.09,77.90,-1.94,107.20,78.08,-0.79,108.20,78.22,0.53,109.06,78.32,2.01,109.73,78.38,3.53,110.18,78.40,4.99,110.50,78.38,6.70],
        [111.13,75.87,8.20,111.05,75.77,9.81,110.80,75.63,11.40,110.37,75.46,12.94,109.73,75.24,14.49,108.91,74.98,15.96,107.92,74.70,17.31,106.81,74.40,18.49,105.70,74.11,19.44,104.31,73.77,20.36,102.87,73.42,21.09,101.38,73.08,21.63,99.72,72.71,22.01,98.15,72.36,22.16,94.99,71.69,22.17,91.03,70.85,22.16,87.07,70.01,22.15,83.12,69.17,22.14,79.16,68.33,22.13,76.68,67.81,22.06,75.38,67.53,21.99,75.33,67.52,21.98,75.28,67.51,21.95,75.22,67.50,21.90,75.13,67.49,21.78,74.30,67.39,20.30,74.17,67.38,20.04,73.97,67.45,17.91,73.74,67.55,15.23,73.51,67.65,12.46,73.27,67.77,9.20,73.30,67.82,8.38,73.49,67.91,7.40,74.51,68.31,4.09,75.70,68.77,0.26,76.89,69.22,-3.57,78.08,69.68,-7.40,79.27,70.14,-11.23,80.45,70.60,-15.06,81.46,70.98,-18.28,81.83,71.11,-19.16,82.16,71.21,-19.69,82.26,71.24,-19.83,82.32,71.25,-19.89,82.54,71.30,-19.93,83.19,71.44,-19.90,83.84,71.57,-19.86,84.36,71.68,-19.82,84.88,71.79,-19.70,85.45,71.89,-19.42,86.84,72.13,-18.33,89.44,72.57,-16.20,92.04,73.01,-14.07,95.14,73.53,-11.53,98.24,74.05,-8.98,101.35,74.58,-6.43,104.46,75.10,-3.88,106.69,75.47,-2.01,107.72,75.63,-0.92,108.74,75.78,0.43,109.59,75.88,1.90,110.26,75.94,3.45,110.70,75.96,4.87,111.02,75.93,6.58],
        [111.51,73.96,8.09,111.43,73.86,9.71,111.17,73.72,11.35,110.72,73.54,12.90,110.11,73.33,14.39,109.33,73.09,15.79,108.36,72.81,17.12,107.28,72.52,18.29,106.08,72.21,19.32,104.74,71.88,20.22,103.30,71.53,20.95,101.85,71.19,21.48,100.27,70.84,21.85,98.69,70.49,22.03,95.75,69.86,22.07,91.87,69.04,22.06,87.94,68.21,22.05,84.02,67.37,22.04,80.10,66.54,22.03,77.17,65.92,22.01,76.49,65.78,21.99,76.44,65.76,21.97,76.38,65.75,21.93,76.31,65.74,21.87,76.19,65.73,21.72,75.28,65.62,20.09,75.03,65.59,19.63,74.87,65.65,17.86,74.65,65.74,15.32,74.42,65.84,12.57,74.23,65.94,9.87,74.28,66.00,9.12,74.51,66.10,8.13,75.68,66.55,4.35,76.86,67.00,0.57,78.03,67.45,-3.21,79.20,67.90,-6.98,80.37,68.36,-10.76,81.55,68.81,-14.54,82.69,69.25,-18.20,83.13,69.40,-19.30,83.18,69.41,-19.40,83.23,69.43,-19.50,83.29,69.45,-19.60,83.66,69.53,-19.59,84.19,69.64,-19.52,84.73,69.75,-19.45,85.23,69.85,-19.39,85.46,69.90,-19.32,86.13,70.02,-19.04,87.15,70.20,-18.27,90.13,70.70,-15.86,93.11,71.21,-13.45,96.08,71.71,-11.04,99.06,72.21,-8.63,102.06,72.72,-6.21,105.06,73.23,-3.77,107.08,73.57,-2.08,108.17,73.74,-0.93,109.16,73.88,0.40,109.97,73.97,1.81,110.63,74.03,3.31,111.09,74.05,4.84,111.40,74.03,6.48],
        [111.48,73.34,8.06,111.40,73.24,9.67,111.14,73.10,11.28,110.71,72.93,12.80,110.09,72.72,14.29,109.31,72.48,15.68,108.38,72.21,16.97,107.29,71.91,18.15,106.10,71.61,19.17,104.78,71.28,20.06,103.36,70.94,20.77,101.87,70.59,21.31,100.35,70.25,21.67,98.37,69.82,21.90,95.55,69.21,21.97,91.83,68.42,22.01,88.00,67.61,22.01,84.11,66.78,22.00,80.26,65.96,22.00,77.08,65.29,21.99,76.79,65.23,21.98,76.77,65.22,21.97,76.71,65.21,21.93,76.64,65.20,21.86,76.52,65.18,21.70,75.57,65.07,20.00,75.30,65.04,19.51,75.14,65.10,17.80,74.92,65.19,15.32,74.70,65.28,12.56,74.52,65.38,10.09,74.58,65.43,9.35,74.88,65.56,8.17,76.05,66.01,4.42,77.21,66.46,0.67,78.38,66.90,-3.08,79.54,67.35,-6.83,80.70,67.80,-10.58,81.87,68.25,-14.33,83.02,68.69,-18.04,83.48,68.86,-19.26,83.55,68.88,-19.39,83.57,68.88,-19.42,83.58,68.89,-19.46,83.71,68.92,-19.45,83.90,68.96,-19.43,84.09,68.99,-19.41,84.87,69.15,-19.28,86.08,69.40,-19.00,86.23,69.43,-18.95,86.98,69.55,-18.38,90.00,70.06,-15.94,93.01,70.57,-13.50,96.03,71.09,-11.06,99.06,71.60,-8.61,102.12,72.12,-6.13,105.05,72.61,-3.74,107.09,72.95,-2.01,108.18,73.12,-0.86,109.14,73.26,0.44,109.96,73.35,1.83,110.61,73.41,3.33,111.08,73.43,4.87,111.37,73.41,6.46],
        [111.22,72.68,8.03,111.14,72.57,9.66,110.91,72.45,11.11,110.44,72.26,12.72,109.88,72.07,14.07,109.09,71.83,15.48,108.19,71.57,16.72,107.06,71.26,17.93,105.89,70.96,18.91,104.68,70.66,19.70,103.25,70.32,20.42,101.55,69.93,21.01,99.91,69.55,21.43,97.65,69.06,21.71,95.12,68.51,21.86,91.58,67.76,21.92,87.86,66.97,21.95,84.61,66.27,21.96,82.42,65.81,21.96,80.23,65.34,21.95,78.03,64.88,21.95,77.06,64.67,21.93,77.03,64.67,21.91,76.97,64.66,21.85,76.84,64.64,21.67,75.81,64.52,19.84,75.56,64.49,19.38,75.41,64.55,17.67,75.20,64.63,15.25,74.97,64.73,12.46,74.82,64.82,10.29,74.89,64.87,9.56,75.28,65.03,8.14,76.43,65.47,4.44,77.58,65.92,0.73,78.73,66.36,-2.97,79.88,66.80,-6.67,81.03,67.24,-10.38,82.17,67.69,-14.08,83.32,68.13,-17.78,83.75,68.29,-19.02,83.79,68.30,-19.10,83.83,68.31,-19.18,83.86,68.32,-19.26,84.13,68.38,-19.24,84.50,68.46,-19.18,84.87,68.53,-19.12,85.24,68.61,-19.06,86.07,68.77,-18.85,86.51,68.86,-18.68,87.10,68.96,-18.22,89.96,69.44,-15.90,92.91,69.94,-13.51,95.93,70.45,-11.06,98.92,70.95,-8.62,101.86,71.45,-6.19,104.46,71.89,-4.00,106.84,72.28,-1.87,108.00,72.46,-0.66,108.93,72.59,0.58,109.75,72.69,1.99,110.34,72.74,3.34,110.81,72.76,4.82,111.12,72.74,6.48],
        [109.66,71.12,7.96,109.60,71.03,9.44,109.35,70.90,10.92,108.94,70.73,12.35,108.37,70.54,13.70,107.64,70.31,14.99,106.78,70.07,16.15,105.66,69.77,17.33,104.54,69.48,18.28,103.23,69.15,19.15,101.79,68.81,19.88,100.21,68.44,20.47,98.30,68.01,20.99,96.36,67.58,21.30,93.59,66.98,21.56,90.56,66.33,21.69,87.14,65.59,21.76,83.46,64.81,21.81,81.62,64.42,21.82,80.21,64.12,21.83,78.79,63.82,21.83,77.64,63.57,21.83,77.61,63.57,21.80,77.58,63.56,21.76,77.41,63.54,21.49,76.21,63.40,19.33,76.09,63.39,19.11,75.92,63.45,17.26,75.73,63.53,14.98,75.50,63.64,12.09,75.42,63.70,10.67,75.51,63.75,9.97,76.10,63.99,7.97,77.21,64.41,4.42,78.31,64.84,0.86,79.41,65.26,-2.69,80.51,65.69,-6.24,81.62,66.11,-9.79,82.72,66.54,-13.35,83.82,66.96,-16.90,84.37,67.17,-18.64,84.39,67.18,-18.69,84.41,67.18,-18.73,84.43,67.19,-18.78,84.55,67.22,-18.77,84.73,67.25,-18.73,84.92,67.29,-18.70,85.10,67.33,-18.66,86.32,67.57,-18.36,86.94,67.69,-18.10,88.09,67.88,-17.19,90.26,68.25,-15.40,92.53,68.63,-13.53,95.29,69.09,-11.21,97.86,69.52,-9.00,100.34,69.93,-6.78,102.28,70.24,-4.93,104.46,70.58,-2.57,106.53,70.90,-0.28,107.46,71.03,0.93,108.22,71.12,2.21,108.83,71.18,3.58,109.28,71.20,5.01,109.56,71.18,6.48],
        [108.75,70.26,7.91,108.68,70.17,9.37,108.45,70.04,10.81,108.05,69.88,12.21,107.50,69.70,13.50,106.78,69.48,14.77,105.95,69.24,15.89,104.86,68.95,17.04,103.75,68.66,17.98,102.45,68.34,18.83,100.97,67.98,19.58,99.35,67.61,20.21,97.42,67.17,20.76,95.56,66.75,21.08,92.88,66.17,21.36,90.04,65.56,21.53,86.65,64.83,21.63,83.24,64.10,21.69,79.63,63.33,21.74,78.60,63.11,21.75,78.22,63.03,21.75,77.94,62.97,21.75,77.93,62.97,21.74,77.93,62.97,21.74,77.62,62.93,21.21,76.42,62.79,19.05,76.38,62.79,18.97,76.21,62.86,17.05,76.02,62.93,14.85,75.78,63.04,11.90,75.75,63.09,10.88,75.85,63.15,10.19,76.55,63.42,7.90,77.62,63.83,4.43,78.70,64.25,0.97,79.77,64.66,-2.50,80.85,65.07,-5.96,81.92,65.49,-9.42,83.00,65.90,-12.89,84.07,66.32,-16.35,84.64,66.54,-18.19,84.67,66.55,-18.27,84.69,66.56,-18.35,84.72,66.57,-18.43,84.89,66.60,-18.42,85.16,66.65,-18.36,85.42,66.71,-18.29,85.69,66.76,-18.22,86.26,66.87,-18.06,87.28,67.07,-17.64,87.45,67.10,-17.52,89.48,67.44,-15.82,92.22,67.90,-13.51,94.71,68.31,-11.34,97.12,68.71,-9.17,99.30,69.06,-7.08,101.19,69.35,-5.06,103.67,69.73,-2.36,105.67,70.04,-0.13,106.59,70.17,1.07,107.34,70.26,2.32,107.94,70.32,3.66,108.38,70.34,5.06,108.65,70.32,6.50],
        [105.24,66.97,7.75,105.19,66.88,9.07,104.98,66.77,10.39,104.61,66.62,11.66,104.16,66.47,12.74,103.49,66.26,13.92,102.81,66.07,14.85,101.80,65.80,15.91,100.76,65.53,16.78,99.52,65.22,17.60,98.14,64.89,18.34,96.37,64.47,19.08,94.39,64.02,19.74,92.68,63.63,20.14,90.58,63.17,20.47,87.91,62.59,20.76,85.28,62.02,20.96,82.49,61.41,21.10,81.53,61.21,21.14,80.58,61.01,21.17,79.63,60.80,21.21,79.04,60.68,21.22,79.04,60.67,21.22,79.03,60.67,21.21,78.49,60.61,20.26,77.51,60.50,18.49,77.47,60.50,18.30,77.31,60.56,16.46,77.15,60.63,14.61,76.98,60.72,12.26,77.01,60.75,11.77,77.16,60.82,11.12,78.11,61.19,8.03,79.07,61.55,4.94,80.03,61.92,1.85,80.99,62.29,-1.24,81.95,62.66,-4.32,82.90,63.03,-7.41,83.86,63.40,-10.50,84.78,63.75,-13.42,85.42,63.99,-15.31,85.53,64.03,-15.59,85.63,64.06,-15.87,85.73,64.10,-16.14,85.84,64.14,-16.42,85.89,64.15,-16.49,85.92,64.16,-16.48,85.95,64.16,-16.47,85.98,64.17,-16.45,86.36,64.24,-16.25,87.52,64.45,-15.58,88.38,64.59,-14.83,89.68,64.79,-13.46,91.88,65.13,-11.06,94.10,65.48,-8.65,96.31,65.82,-6.25,98.49,66.15,-3.87,100.56,66.47,-1.59,102.45,66.76,0.56,103.26,66.87,1.61,103.95,66.96,2.76,104.51,67.01,3.98,104.91,67.03,5.26,105.15,67.01,6.56],
        [101.73,63.67,7.58,101.70,63.60,8.77,101.52,63.50,9.94,101.21,63.37,11.02,100.85,63.25,11.91,100.26,63.07,12.97,99.73,62.92,13.72,98.85,62.68,14.68,98.01,62.46,15.40,96.97,62.20,16.13,95.56,61.86,16.94,94.05,61.50,17.66,92.21,61.07,18.38,90.44,60.66,18.95,88.97,60.33,19.31,86.96,59.88,19.67,84.72,59.39,19.97,82.40,58.88,20.20,81.35,58.66,20.29,80.74,58.52,20.34,80.14,58.39,20.38,80.06,58.38,20.36,80.03,58.37,20.33,79.89,58.35,20.16,79.58,58.31,19.69,78.61,58.20,17.95,78.59,58.20,17.90,78.48,58.24,16.62,78.35,58.29,15.17,78.22,58.37,13.29,78.24,58.39,12.92,78.40,58.46,12.29,79.20,58.77,9.71,80.00,59.07,7.13,80.80,59.38,4.55,81.60,59.69,1.97,82.40,60.00,-0.61,83.20,60.31,-3.19,83.97,60.60,-5.61,84.66,60.85,-7.58,85.28,61.06,-9.06,86.08,61.32,-10.61,86.77,61.53,-11.92,86.77,61.54,-11.93,86.78,61.54,-11.93,86.78,61.54,-11.94,86.78,61.54,-11.94,86.79,61.54,-11.94,86.79,61.54,-11.94,86.80,61.54,-11.93,86.88,61.55,-11.88,87.91,61.73,-11.05,89.12,61.93,-9.95,90.67,62.17,-8.26,92.51,62.45,-6.26,94.32,62.73,-4.28,96.09,63.00,-2.32,97.80,63.26,-0.39,99.41,63.50,1.47,100.05,63.59,2.35,100.60,63.66,3.27,101.09,63.71,4.36,101.45,63.72,5.50,101.66,63.70,6.65],
        [98.22,60.37,7.42,98.20,60.31,8.46,98.05,60.22,9.50,97.85,60.14,10.25,97.54,60.03,11.09,97.12,59.90,11.89,96.65,59.76,12.60,96.01,59.59,13.32,95.39,59.42,13.91,94.42,59.18,14.66,93.27,58.89,15.37,92.00,58.59,16.04,90.62,58.26,16.68,89.10,57.90,17.30,87.63,57.56,17.83,86.23,57.24,18.24,84.58,56.87,18.62,82.93,56.51,18.92,81.36,56.16,19.16,81.17,56.12,19.18,80.98,56.08,19.20,80.93,56.07,19.17,80.89,56.06,19.14,80.61,56.01,18.89,80.43,55.99,18.66,79.97,55.93,17.86,79.70,55.90,17.38,79.65,55.92,16.83,79.56,55.96,15.82,79.46,56.01,14.35,79.47,56.03,14.11,79.57,56.07,13.69,80.21,56.32,11.65,80.84,56.56,9.62,81.47,56.80,7.58,82.10,57.05,5.54,82.73,57.29,3.51,83.30,57.50,1.76,83.77,57.67,0.50,84.38,57.87,-0.72,85.26,58.15,-2.41,86.15,58.42,-4.09,87.03,58.70,-5.77,87.53,58.86,-6.71,87.53,58.86,-6.72,87.54,58.86,-6.73,87.55,58.86,-6.75,87.56,58.87,-6.75,87.57,58.87,-6.75,87.58,58.87,-6.74,87.59,58.87,-6.73,87.97,58.94,-6.47,89.12,59.14,-5.59,89.98,59.28,-4.87,91.00,59.44,-3.73,92.41,59.65,-2.16,93.80,59.86,-0.59,95.16,60.07,0.97,96.31,60.24,2.32,96.92,60.32,3.20,97.27,60.36,3.82,97.68,60.40,4.73,97.99,60.41,5.74,98.17,60.40,6.75],
        [94.71,57.08,7.25,94.71,57.03,8.18,94.59,56.96,9.00,94.50,56.91,9.43,94.20,56.80,10.32,93.92,56.71,10.88,93.53,56.60,11.52,93.15,56.49,11.97,92.57,56.34,12.57,91.94,56.17,13.08,91.27,56.01,13.57,90.21,55.75,14.22,89.11,55.48,14.80,88.03,55.22,15.32,86.83,54.94,15.85,85.67,54.67,16.32,84.27,54.34,16.84,83.21,54.10,17.19,82.12,53.85,17.50,81.44,53.70,17.66,81.44,53.70,17.66,81.43,53.70,17.65,81.40,53.69,17.63,81.36,53.68,17.60,81.14,53.65,17.40,81.07,53.64,17.31,80.81,53.61,16.85,80.81,53.61,16.84,80.76,53.62,16.35,80.69,53.66,15.39,80.70,53.67,15.28,80.78,53.70,15.00,81.24,53.88,13.52,81.69,54.05,12.04,82.15,54.23,10.56,82.48,54.35,9.61,82.83,54.46,8.89,83.52,54.68,7.57,84.22,54.90,6.24,84.91,55.12,4.92,85.60,55.34,3.60,86.30,55.55,2.28,86.99,55.77,0.95,87.69,55.99,-0.37,88.27,56.18,-1.49,88.29,56.18,-1.51,88.30,56.18,-1.53,88.32,56.19,-1.55,88.33,56.19,-1.56,88.35,56.20,-1.56,88.37,56.20,-1.54,88.39,56.20,-1.53,88.48,56.22,-1.47,89.46,56.39,-0.77,90.42,56.56,-0.05,90.76,56.61,0.29,91.56,56.73,1.22,92.57,56.88,2.39,93.15,56.97,3.08,93.72,57.04,3.96,94.02,57.07,4.55,94.26,57.10,5.08,94.53,57.11,5.98,94.66,57.09,6.79],
        [91.19,53.78,7.09,91.19,53.75,7.61,91.19,53.73,8.00,91.11,53.69,8.46,91.03,53.65,8.91,90.88,53.59,9.35,90.76,53.54,9.72,90.70,53.53,9.82,90.50,53.47,10.05,90.09,53.36,10.54,89.67,53.25,10.96,89.24,53.14,11.34,88.77,53.01,11.73,88.22,52.88,12.12,87.74,52.75,12.47,87.22,52.63,12.79,86.69,52.50,13.11,86.35,52.42,13.30,85.89,52.31,13.47,85.43,52.20,13.63,85.29,52.17,13.65,85.06,52.12,13.66,84.84,52.07,13.67,84.69,52.04,13.67,84.67,52.04,13.65,84.64,52.03,13.63,84.61,52.03,13.61,84.59,52.03,13.54,84.57,52.03,13.45,84.55,52.03,13.36,84.55,52.04,13.14,84.56,52.06,12.88,84.64,52.09,12.57,84.75,52.13,12.23,85.01,52.22,11.63,85.28,52.31,11.03,85.59,52.41,10.38,85.93,52.52,9.69,86.28,52.63,9.01,86.62,52.74,8.32,86.97,52.85,7.66,87.31,52.96,7.00,87.66,53.07,6.34,88.00,53.18,5.68,88.35,53.28,5.03,88.69,53.39,4.37,89.03,53.50,3.74,89.04,53.50,3.71,89.06,53.51,3.69,89.08,53.51,3.66,89.10,53.52,3.64,89.10,53.52,3.64,89.10,53.52,3.64,89.10,53.52,3.64,89.10,53.52,3.64,89.38,53.56,3.89,89.73,53.62,4.19,90.06,53.67,4.53,90.49,53.74,5.03,90.71,53.77,5.28,90.78,53.78,5.42,90.90,53.79,5.76,91.03,53.79,6.17,91.11,53.78,6.63],
        [90.78,53.43,7.08,90.78,53.40,7.50,90.78,53.38,7.91,90.78,53.38,8.00,90.72,53.35,8.22,90.59,53.30,8.66,90.49,53.26,8.96,90.34,53.22,9.23,90.10,53.14,9.65,89.89,53.08,9.96,89.60,53.00,10.30,89.33,52.93,10.62,89.08,52.86,10.87,88.81,52.79,11.11,88.55,52.72,11.34,88.37,52.68,11.43,87.94,52.58,11.62,87.49,52.47,11.81,87.13,52.39,11.94,86.83,52.32,12.01,86.58,52.27,12.06,86.42,52.23,12.06,86.27,52.20,12.07,86.16,52.18,12.05,86.11,52.17,12.01,86.05,52.16,11.98,86.00,52.15,11.94,85.96,52.15,11.89,85.95,52.15,11.82,85.93,52.15,11.76,85.93,52.16,11.51,85.93,52.17,11.24,85.95,52.19,11.04,86.07,52.23,10.67,86.19,52.28,10.31,86.37,52.34,9.85,86.60,52.42,9.35,86.82,52.49,8.85,87.04,52.56,8.36,87.29,52.64,7.85,87.56,52.73,7.32,87.83,52.81,6.78,88.09,52.90,6.25,88.36,52.99,5.72,88.63,53.07,5.18,88.86,53.14,4.73,89.07,53.21,4.33,89.10,53.22,4.27,89.11,53.22,4.27,89.11,53.22,4.26,89.11,53.22,4.26,89.11,53.22,4.26,89.11,53.22,4.26,89.11,53.22,4.26,89.11,53.22,4.26,89.19,53.24,4.33,89.29,53.25,4.43,89.68,53.31,4.89,89.94,53.35,5.19,90.12,53.37,5.47,90.37,53.40,5.92,90.51,53.42,6.16,90.60,53.42,6.45,90.73,53.42,6.92]],
    "20" : [
        [56.06,111.40,-7.63,56.05,111.34,-6.56,56.05,111.27,-5.35,56.04,111.20,-4.14,56.04,111.13,-2.93,56.03,111.06,-1.72,56.02,110.99,-0.51,56.02,110.92,0.70,56.01,110.85,1.90,56.00,110.78,3.09,55.98,110.73,3.80,55.88,110.63,4.93,55.86,110.62,5.08,55.82,110.59,5.28,55.67,110.52,5.47,55.55,110.47,5.62,55.49,110.45,5.70,55.42,110.42,5.79,55.36,110.39,5.87,55.29,110.36,5.95,55.26,110.35,5.92,55.22,110.34,5.89,55.19,110.33,5.86,55.15,110.32,5.83,54.72,110.18,5.44,54.54,110.13,5.28,54.50,110.11,5.18,54.37,110.09,4.81,54.12,110.04,3.96,54.01,110.03,3.48,53.83,110.03,2.37,53.66,110.03,1.19,53.50,110.03,0.08,53.35,110.04,-1.07,53.19,110.06,-2.28,53.04,110.07,-3.48,52.89,110.08,-4.68,52.74,110.09,-5.88,52.60,110.10,-6.97,52.47,110.11,-8.05,52.42,110.14,-8.85,52.43,110.16,-9.19,52.50,110.22,-9.80,52.63,110.30,-10.27,52.77,110.37,-10.70,53.03,110.50,-11.23,53.25,110.59,-11.52,53.64,110.76,-12.00,53.70,110.79,-12.06,53.85,110.84,-12.11,54.15,110.96,-12.20,54.28,111.01,-12.24,54.41,111.05,-12.27,54.56,111.11,-12.21,54.71,111.16,-12.14,54.85,111.20,-12.06,54.99,111.25,-11.99,55.08,111.28,-11.85,55.16,111.30,-11.70,55.40,111.35,-11.15,55.59,111.39,-10.59,55.80,111.43,-9.87,55.93,111.44,-9.23,56.02,111.44,-8.62],
        [60.79,110.35,-5.79,60.79,110.26,-4.13,60.78,110.15,-2.26,60.77,110.04,-0.38,60.76,109.93,1.49,60.74,109.83,3.34,60.73,109.76,4.49,60.62,109.65,5.69,60.41,109.51,6.87,60.11,109.34,8.01,59.80,109.18,8.82,59.59,109.08,9.28,59.01,108.81,10.22,58.60,108.64,10.69,58.18,108.46,11.14,57.86,108.33,11.35,57.60,108.23,11.50,57.46,108.17,11.57,57.32,108.12,11.55,57.12,108.05,11.53,56.79,107.93,11.48,56.76,107.92,11.47,56.34,107.79,11.14,55.95,107.67,10.79,55.25,107.46,9.96,54.56,107.26,9.02,53.99,107.11,8.07,53.78,107.05,7.70,53.67,107.04,7.15,53.51,107.03,6.19,53.32,107.05,4.74,53.19,107.08,3.22,53.06,107.14,1.37,52.92,107.19,-0.50,52.79,107.25,-2.37,52.66,107.30,-4.24,52.52,107.36,-6.11,52.40,107.41,-7.81,52.37,107.47,-9.07,52.43,107.56,-10.31,52.56,107.68,-11.60,52.73,107.80,-12.72,52.76,107.83,-12.90,52.85,107.87,-13.07,53.03,107.94,-13.25,53.34,108.07,-13.53,54.09,108.38,-14.11,54.96,108.73,-14.66,55.33,108.87,-14.80,55.74,109.03,-14.95,55.94,109.10,-14.98,56.49,109.30,-14.95,56.61,109.34,-14.91,56.96,109.46,-14.72,57.09,109.50,-14.66,57.22,109.54,-14.51,57.63,109.66,-13.95,58.42,109.88,-12.79,59.11,110.07,-11.65,59.56,110.18,-10.76,60.02,110.29,-9.69,60.39,110.36,-8.59,60.63,110.40,-7.60,60.75,110.39,-6.81],
        [65.53,109.31,-3.95,65.52,109.20,-1.95,65.51,109.07,0.30,65.50,108.94,2.55,65.48,108.81,4.74,65.40,108.71,6.01,65.21,108.57,7.26,64.94,108.41,8.40,64.63,108.24,9.33,64.32,108.09,10.05,63.70,107.81,11.04,63.26,107.62,11.57,62.71,107.39,12.16,61.76,107.00,12.93,60.78,106.61,13.54,59.77,106.21,14.06,59.26,106.01,14.29,59.02,105.92,14.35,58.75,105.82,14.35,58.44,105.71,14.34,58.25,105.65,14.25,57.71,105.46,13.98,56.85,105.19,13.32,55.94,104.90,12.55,55.82,104.86,12.43,55.72,104.84,12.24,55.64,104.83,11.89,55.38,104.80,10.71,54.96,104.76,8.71,54.69,104.75,7.11,54.49,104.77,5.45,54.33,104.82,3.44,54.17,104.89,1.21,54.01,104.96,-1.04,53.85,105.02,-3.29,53.69,105.09,-5.54,53.58,105.15,-7.33,53.59,105.23,-8.68,53.71,105.35,-10.10,53.92,105.51,-11.58,54.24,105.71,-13.10,54.67,105.96,-14.72,54.84,106.05,-15.26,54.88,106.07,-15.36,54.94,106.10,-15.46,55.09,106.16,-15.52,55.25,106.22,-15.59,56.37,106.65,-15.99,57.21,106.97,-16.22,57.94,107.24,-16.31,58.15,107.31,-16.26,58.54,107.45,-16.17,58.79,107.53,-16.06,59.15,107.65,-15.83,59.77,107.85,-15.25,60.63,108.11,-14.35,61.66,108.41,-13.12,62.63,108.69,-11.81,63.39,108.91,-10.67,64.08,109.09,-9.42,64.61,109.22,-8.28,64.99,109.30,-7.23,65.32,109.35,-5.98,65.47,109.35,-5.03],
        [70.26,108.27,-2.11,70.25,108.14,0.15,70.24,107.99,2.71,70.22,107.84,5.22,70.15,107.74,6.65,69.94,107.58,8.04,69.63,107.40,9.27,69.29,107.22,10.23,68.96,107.06,10.95,68.32,106.78,11.97,67.74,106.52,12.66,67.09,106.25,13.31,66.19,105.88,14.02,65.12,105.46,14.68,63.68,104.90,15.37,62.35,104.38,15.88,61.12,103.92,16.24,60.46,103.67,16.36,60.10,103.53,16.39,59.92,103.47,16.36,59.32,103.26,16.16,58.31,102.93,15.60,57.83,102.77,15.30,57.50,102.66,15.10,57.36,102.62,14.98,57.34,102.61,14.92,57.07,102.57,13.92,56.61,102.51,11.90,56.14,102.47,9.69,55.86,102.46,7.99,55.66,102.50,6.03,55.48,102.56,3.68,55.30,102.64,1.13,55.12,102.72,-1.43,54.94,102.79,-3.98,54.79,102.86,-6.13,54.79,102.94,-7.57,54.92,103.07,-9.08,55.17,103.25,-10.67,55.54,103.48,-12.33,56.12,103.81,-14.43,56.51,104.02,-15.69,56.75,104.14,-16.22,56.90,104.20,-16.44,56.96,104.23,-16.52,57.04,104.26,-16.59,57.17,104.31,-16.62,57.87,104.57,-16.73,59.25,105.09,-16.91,59.86,105.31,-16.91,60.09,105.39,-16.89,60.69,105.60,-16.71,60.99,105.70,-16.51,61.92,105.99,-15.76,63.69,106.55,-14.22,64.73,106.87,-13.23,65.94,107.24,-11.90,66.91,107.52,-10.69,67.87,107.80,-9.31,68.58,107.99,-8.11,69.23,108.15,-6.78,69.67,108.25,-5.61,70.01,108.30,-4.38,70.20,108.31,-3.25],
        [75.00,107.22,-0.27,74.99,107.08,2.24,74.97,106.92,5.06,74.94,106.81,6.80,74.77,106.67,8.24,74.47,106.48,9.63,74.10,106.28,10.78,73.69,106.08,11.71,73.03,105.78,12.77,72.42,105.52,13.53,71.73,105.23,14.22,70.71,104.81,15.05,69.61,104.37,15.72,68.14,103.80,16.43,66.72,103.25,16.96,65.21,102.68,17.37,63.49,102.03,17.71,61.60,101.33,17.97,61.50,101.29,17.95,60.89,101.08,17.82,60.30,100.88,17.58,59.48,100.60,17.21,59.13,100.48,17.05,58.92,100.41,16.94,58.90,100.41,16.88,58.73,100.37,16.44,58.28,100.29,14.95,57.78,100.23,12.87,57.30,100.18,10.56,57.03,100.18,8.78,56.82,100.23,6.44,56.62,100.31,3.77,56.42,100.39,0.95,56.22,100.48,-1.88,56.02,100.56,-4.68,55.98,100.63,-6.16,56.08,100.75,-7.71,56.33,100.94,-9.37,56.72,101.18,-11.11,57.39,101.55,-13.51,57.98,101.89,-15.58,58.30,102.04,-16.34,58.48,102.13,-16.65,58.81,102.27,-17.02,58.90,102.30,-17.11,58.93,102.31,-17.12,58.98,102.33,-17.13,60.79,103.01,-17.37,61.64,103.32,-17.42,62.04,103.46,-17.41,62.61,103.66,-17.23,62.86,103.74,-17.09,63.55,103.96,-16.56,65.39,104.55,-15.01,67.40,105.18,-13.29,68.98,105.68,-11.89,70.26,106.07,-10.60,71.34,106.40,-9.32,72.37,106.69,-7.91,73.22,106.92,-6.53,73.81,107.08,-5.34,74.34,107.19,-3.99,74.70,107.25,-2.73,74.93,107.26,-1.48],
        [79.73,106.18,1.57,79.72,106.02,4.30,79.70,105.86,7.12,79.59,105.74,8.48,79.35,105.58,9.80,78.99,105.38,11.08,78.50,105.13,12.29,77.94,104.87,13.27,77.32,104.60,14.13,76.45,104.23,15.07,75.38,103.79,15.96,74.28,103.35,16.67,73.00,102.85,17.31,71.51,102.28,17.89,69.97,101.69,18.33,68.26,101.05,18.67,65.77,100.13,19.00,63.56,99.31,19.19,63.05,99.12,19.21,62.43,98.90,19.12,61.30,98.51,18.75,60.83,98.35,18.59,60.47,98.23,18.47,60.47,98.23,18.47,60.35,98.20,18.24,59.89,98.08,17.21,59.43,98.00,15.79,58.92,97.93,13.66,58.44,97.89,11.33,58.20,97.90,9.48,57.96,97.97,6.65,57.74,98.06,3.69,57.52,98.15,0.61,57.30,98.24,-2.48,57.18,98.32,-4.62,57.24,98.42,-6.15,57.46,98.60,-7.83,57.83,98.83,-9.62,58.54,99.23,-12.17,59.33,99.68,-14.99,59.73,99.89,-16.21,60.01,100.03,-16.76,60.19,100.10,-17.00,60.75,100.34,-17.59,60.77,100.35,-17.61,60.81,100.36,-17.62,61.60,100.66,-17.73,63.20,101.25,-17.93,63.82,101.48,-17.92,64.08,101.57,-17.87,64.59,101.75,-17.71,65.01,101.88,-17.40,66.68,102.41,-16.04,68.95,103.13,-14.16,71.16,103.83,-12.29,73.29,104.51,-10.46,74.54,104.90,-9.29,75.75,105.26,-7.94,76.91,105.60,-6.43,77.77,105.84,-5.08,78.45,106.01,-3.80,79.02,106.14,-2.37,79.41,106.21,-1.07,79.65,106.22,0.29],
        [84.46,105.14,3.41,84.45,104.97,6.35,84.41,104.84,8.37,84.20,104.67,9.98,83.87,104.47,11.37,83.41,104.24,12.62,82.92,104.00,13.62,82.19,103.68,14.74,81.33,103.31,15.74,80.42,102.93,16.57,79.35,102.50,17.36,78.07,101.99,18.09,76.68,101.45,18.69,74.98,100.80,19.22,73.13,100.11,19.61,70.93,99.29,19.91,68.66,98.45,20.08,66.10,97.52,20.17,64.23,96.84,20.15,63.41,96.55,20.00,62.25,96.14,19.71,62.06,96.08,19.65,61.92,96.03,19.53,61.80,96.00,19.37,61.44,95.90,18.80,61.00,95.79,17.85,60.55,95.70,16.51,60.03,95.63,14.32,59.59,95.60,12.07,59.35,95.64,9.85,59.09,95.72,6.72,58.86,95.81,3.48,58.62,95.91,0.14,58.41,96.00,-2.89,58.40,96.09,-4.46,58.56,96.24,-6.12,58.90,96.46,-7.92,59.57,96.85,-10.42,60.44,97.33,-13.49,61.15,97.73,-15.96,61.47,97.89,-16.74,61.67,97.98,-17.07,62.27,98.24,-17.73,62.62,98.38,-18.09,62.66,98.40,-18.12,62.69,98.41,-18.13,64.33,99.02,-18.34,65.52,99.46,-18.41,65.84,99.57,-18.39,66.20,99.70,-18.27,66.47,99.79,-18.17,67.57,100.14,-17.31,70.07,100.94,-15.28,72.54,101.73,-13.25,74.97,102.50,-11.24,77.32,103.24,-9.25,79.07,103.79,-7.70,80.25,104.15,-6.45,81.40,104.49,-5.01,82.36,104.76,-3.57,83.10,104.95,-2.19,83.69,105.09,-0.76,84.12,105.16,0.66,84.38,105.18,2.06],
        [89.20,104.09,5.25,89.18,103.92,8.28,89.08,103.80,9.81,88.83,103.62,11.24,88.43,103.40,12.63,87.89,103.13,13.92,87.21,102.82,15.09,86.44,102.48,16.12,85.51,102.09,17.11,84.32,101.60,18.08,83.13,101.13,18.84,81.72,100.57,19.52,80.33,100.04,20.01,78.40,99.31,20.47,76.23,98.50,20.79,74.02,97.69,20.96,71.26,96.68,21.02,68.32,95.61,20.98,65.65,94.65,20.92,64.30,94.17,20.69,63.67,93.95,20.57,63.50,93.89,20.51,63.44,93.87,20.47,62.97,93.72,19.95,62.50,93.59,19.27,62.08,93.49,18.42,61.66,93.40,17.16,61.12,93.34,14.86,60.75,93.31,12.84,60.50,93.38,10.08,60.22,93.46,6.70,59.97,93.57,3.18,59.71,93.68,-0.40,59.59,93.76,-2.70,59.67,93.88,-4.28,59.94,94.07,-6.05,60.49,94.40,-8.25,61.43,94.92,-11.57,62.36,95.45,-14.89,62.88,95.73,-16.56,63.16,95.86,-17.12,63.34,95.94,-17.38,64.44,96.41,-18.55,64.50,96.43,-18.60,64.53,96.45,-18.63,64.80,96.55,-18.67,66.75,97.27,-18.89,67.51,97.54,-18.89,67.83,97.66,-18.80,68.18,97.78,-18.68,68.39,97.85,-18.54,70.79,98.61,-16.63,73.49,99.47,-14.46,76.17,100.33,-12.29,78.80,101.17,-10.14,81.35,101.98,-8.02,83.57,102.68,-6.11,84.79,103.05,-4.89,86.04,103.42,-3.37,87.01,103.70,-1.97,87.77,103.90,-0.57,88.37,104.03,0.86,88.83,104.12,2.39,89.10,104.14,3.83],
        [93.93,103.05,7.09,93.91,102.90,9.53,93.75,102.76,11.05,93.41,102.55,12.57,92.94,102.31,13.93,92.32,102.01,15.22,91.53,101.65,16.45,90.63,101.27,17.53,89.60,100.84,18.50,88.35,100.33,19.44,87.01,99.80,20.20,85.55,99.23,20.81,83.97,98.63,21.26,81.96,97.88,21.62,79.80,97.08,21.81,76.98,96.06,21.87,73.89,94.93,21.80,70.60,93.74,21.66,67.37,92.58,21.46,65.19,91.80,21.24,64.96,91.72,21.19,64.83,91.67,21.12,64.53,91.58,20.91,63.97,91.41,20.32,63.53,91.28,19.70,63.15,91.18,18.95,62.76,91.10,17.79,62.21,91.04,15.43,61.92,91.03,13.63,61.63,91.12,10.24,61.35,91.21,6.65,61.08,91.33,2.84,60.82,91.43,-0.79,60.81,91.52,-2.37,60.99,91.68,-4.06,61.37,91.92,-5.93,62.29,92.44,-9.20,63.29,93.00,-12.76,64.23,93.53,-16.05,64.57,93.71,-17.00,64.83,93.82,-17.45,65.41,94.07,-18.10,66.32,94.46,-19.06,66.39,94.49,-19.12,66.40,94.49,-19.13,67.12,94.75,-19.22,68.71,95.34,-19.38,69.30,95.56,-19.36,69.76,95.72,-19.20,70.00,95.80,-19.08,71.11,96.15,-18.22,74.00,97.08,-15.92,76.91,98.01,-13.61,79.81,98.93,-11.28,82.67,99.85,-8.98,85.42,100.72,-6.74,87.97,101.53,-4.58,89.35,101.96,-3.29,90.65,102.35,-1.77,91.64,102.63,-0.37,92.43,102.84,1.04,93.04,102.98,2.47,93.55,103.07,4.11,93.83,103.09,5.59],
        [98.64,102.01,8.92,98.58,101.89,10.78,98.36,101.72,12.29,97.97,101.50,13.79,97.40,101.21,15.22,96.68,100.87,16.56,95.81,100.49,17.78,94.80,100.06,18.89,93.53,99.54,19.97,92.29,99.04,20.81,90.89,98.49,21.51,89.44,97.93,22.02,87.58,97.23,22.44,85.76,96.56,22.65,83.22,95.63,22.70,80.15,94.52,22.60,76.50,93.20,22.38,72.75,91.85,22.12,68.94,90.48,21.84,66.61,89.64,21.67,65.99,89.42,21.62,65.86,89.38,21.54,65.62,89.30,21.36,65.04,89.12,20.75,64.54,88.98,20.11,64.20,88.89,19.45,63.85,88.82,18.42,63.32,88.76,16.12,63.08,88.77,14.29,62.77,88.87,10.41,62.47,88.98,6.57,62.18,89.10,2.52,61.98,89.19,-0.45,62.05,89.30,-1.98,62.32,89.50,-3.75,63.00,89.89,-6.34,64.07,90.49,-10.13,65.14,91.09,-13.93,65.92,91.53,-16.61,66.24,91.69,-17.38,66.46,91.79,-17.73,67.57,92.25,-18.92,68.17,92.51,-19.55,68.23,92.54,-19.61,68.74,92.72,-19.65,69.37,92.96,-19.70,70.52,93.38,-19.75,70.98,93.54,-19.72,71.49,93.72,-19.51,72.45,94.03,-18.80,74.21,94.59,-17.44,77.25,95.57,-15.05,80.38,96.57,-12.61,83.45,97.55,-10.20,86.52,98.53,-7.77,89.54,99.50,-5.35,92.27,100.36,-3.11,93.95,100.89,-1.61,95.13,101.25,-0.28,96.27,101.58,1.25,97.05,101.78,2.61,97.68,101.93,4.08,98.23,102.03,5.83,98.52,102.06,7.35],
        [100.79,101.48,9.54,100.73,101.37,11.15,100.47,101.18,12.79,100.04,100.94,14.36,99.47,100.66,15.74,98.72,100.30,17.12,97.81,99.90,18.39,96.67,99.42,19.62,95.59,98.98,20.53,94.32,98.46,21.37,92.83,97.88,22.09,91.39,97.33,22.57,89.54,96.64,22.95,87.91,96.04,23.09,85.24,95.06,23.08,81.77,93.81,22.86,77.84,92.40,22.58,73.90,90.98,22.30,69.95,89.56,22.02,66.65,88.37,21.78,66.40,88.28,21.76,66.34,88.26,21.73,66.18,88.21,21.61,65.64,88.05,21.05,65.04,87.87,20.32,64.73,87.79,19.73,64.40,87.72,18.80,63.89,87.66,16.61,63.66,87.68,14.71,63.33,87.78,10.72,63.02,87.89,6.78,62.73,88.01,2.62,62.56,88.11,-0.12,62.65,88.22,-1.67,62.95,88.43,-3.45,63.76,88.89,-6.43,64.86,89.51,-10.33,65.96,90.13,-14.23,66.70,90.54,-16.77,67.02,90.70,-17.51,67.24,90.80,-17.85,68.51,91.34,-19.21,69.03,91.55,-19.76,69.04,91.56,-19.77,69.18,91.61,-19.78,70.08,91.94,-19.84,71.17,92.34,-19.87,71.81,92.57,-19.79,72.21,92.70,-19.62,73.93,93.26,-18.35,76.56,94.10,-16.31,79.19,94.95,-14.27,81.83,95.79,-12.22,85.04,96.82,-9.73,88.24,97.85,-7.24,91.34,98.84,-4.80,94.21,99.76,-2.51,96.05,100.34,-0.92,97.22,100.70,0.35,98.37,101.03,1.85,99.19,101.25,3.23,99.89,101.42,4.81,100.36,101.50,6.29,100.67,101.53,7.93],
        [101.36,101.05,9.51,101.30,100.94,11.16,101.05,100.76,12.78,100.62,100.51,14.35,100.01,100.21,15.86,99.24,99.85,17.28,98.31,99.44,18.59,97.24,98.98,19.77,96.03,98.48,20.80,94.73,97.96,21.67,93.32,97.41,22.36,91.85,96.84,22.87,90.32,96.27,23.19,88.77,95.70,23.30,86.30,94.80,23.24,82.42,93.41,22.96,78.43,91.97,22.67,74.43,90.53,22.39,70.44,89.10,22.10,66.68,87.74,21.83,66.61,87.72,21.82,66.50,87.68,21.76,66.37,87.64,21.65,65.96,87.51,21.22,65.30,87.32,20.43,64.99,87.24,19.88,64.67,87.17,19.00,64.19,87.11,16.88,63.95,87.13,15.00,63.62,87.23,10.98,63.31,87.34,7.01,63.01,87.47,2.83,62.85,87.56,0.11,62.94,87.68,-1.43,63.25,87.89,-3.21,64.08,88.36,-6.28,65.19,88.99,-10.20,66.30,89.61,-14.13,67.07,90.04,-16.79,67.39,90.20,-17.55,67.62,90.30,-17.89,68.80,90.80,-19.17,69.43,91.06,-19.83,69.44,91.07,-19.84,69.87,91.23,-19.86,70.44,91.44,-19.88,71.63,91.87,-19.91,72.14,92.06,-19.86,72.64,92.22,-19.64,74.19,92.72,-18.48,77.01,93.63,-16.30,79.82,94.53,-14.12,82.64,95.43,-11.94,85.47,96.34,-9.74,88.73,97.39,-7.22,91.97,98.43,-4.70,95.01,99.40,-2.32,96.87,99.99,-0.76,97.99,100.33,0.42,98.96,100.61,1.73,99.79,100.83,3.15,100.44,100.99,4.64,100.93,101.08,6.24,101.24,101.10,7.85],
        [101.71,100.54,9.48,101.65,100.43,11.15,101.40,100.24,12.77,100.97,100.00,14.35,100.37,99.70,15.87,99.59,99.33,17.31,98.65,98.92,18.64,97.57,98.46,19.83,96.36,97.96,20.87,95.05,97.43,21.75,93.65,96.88,22.45,92.13,96.30,22.98,90.61,95.73,23.30,89.05,95.16,23.43,86.75,94.33,23.31,82.81,92.90,23.03,79.88,91.85,22.81,76.98,90.81,22.60,74.08,89.77,22.39,71.18,88.72,22.18,68.28,87.68,21.97,66.71,87.11,21.82,66.61,87.08,21.74,66.23,86.97,21.34,65.54,86.76,20.52,65.24,86.68,19.99,64.94,86.62,19.16,64.47,86.56,17.10,64.24,86.58,15.16,63.90,86.69,11.13,63.59,86.80,7.15,63.29,86.92,2.95,63.14,87.01,0.31,63.24,87.13,-1.23,63.55,87.35,-2.99,64.42,87.84,-6.16,65.53,88.46,-10.10,66.64,89.09,-14.04,67.44,89.53,-16.81,67.77,89.70,-17.58,67.99,89.80,-17.94,69.12,90.28,-19.15,69.78,90.55,-19.84,69.82,90.57,-19.88,70.09,90.67,-19.90,70.47,90.81,-19.91,71.99,91.36,-19.94,72.51,91.55,-19.87,73.13,91.76,-19.61,74.53,92.21,-18.53,77.46,93.15,-16.26,80.38,94.09,-14.00,83.30,95.02,-11.74,86.22,95.96,-9.47,89.15,96.90,-7.21,92.32,97.92,-4.75,95.50,98.94,-2.28,97.20,99.48,-0.88,98.32,99.82,0.32,99.29,100.10,1.61,100.14,100.33,3.08,100.78,100.48,4.56,101.28,100.57,6.19,101.59,100.59,7.81],
        [102.63,98.10,9.34,102.57,97.98,10.98,102.30,97.79,12.71,101.90,97.56,14.18,101.25,97.23,15.81,100.54,96.90,17.13,99.56,96.46,18.53,98.49,96.01,19.70,97.30,95.52,20.74,95.92,94.97,21.66,94.61,94.45,22.31,93.15,93.89,22.84,91.52,93.28,23.19,89.97,92.71,23.32,87.20,91.70,23.29,83.68,90.43,23.08,79.70,89.00,22.81,75.73,87.57,22.55,72.46,86.39,22.33,70.62,85.73,22.20,68.77,85.06,22.07,67.53,84.62,21.98,67.52,84.61,21.97,67.09,84.48,21.50,66.54,84.32,20.85,66.31,84.26,20.42,66.06,84.21,19.71,65.67,84.16,17.97,65.44,84.22,15.46,65.11,84.32,11.42,64.79,84.43,7.43,64.50,84.56,3.25,64.40,84.64,1.14,64.53,84.77,-0.38,64.89,85.00,-2.14,65.91,85.58,-5.79,67.02,86.20,-9.74,68.14,86.83,-13.68,69.05,87.34,-16.87,69.39,87.51,-17.72,69.63,87.62,-18.12,70.51,87.99,-19.08,71.30,88.33,-19.90,71.38,88.35,-19.96,71.46,88.38,-19.98,71.52,88.41,-20.00,73.50,89.13,-20.00,74.11,89.35,-19.91,74.43,89.46,-19.78,75.29,89.74,-19.23,77.17,90.34,-17.77,80.42,91.38,-15.24,83.66,92.42,-12.72,86.90,93.46,-10.20,90.11,94.49,-7.69,93.22,95.48,-5.24,95.57,96.23,-3.31,98.12,97.03,-1.03,99.23,97.37,0.15,100.20,97.65,1.46,101.02,97.87,2.87,101.68,98.03,4.37,102.21,98.12,6.07,102.51,98.14,7.69],
        [103.48,95.75,9.21,103.42,95.64,10.85,103.16,95.45,12.56,102.75,95.21,14.05,102.12,94.89,15.66,101.38,94.55,17.01,100.42,94.12,18.38,99.36,93.67,19.56,98.17,93.18,20.59,96.80,92.63,21.51,95.48,92.11,22.17,94.02,91.55,22.70,92.41,90.95,23.05,90.87,90.38,23.19,87.51,89.16,23.16,84.35,88.02,23.04,80.55,86.65,22.82,77.89,85.68,22.65,75.31,84.76,22.47,72.73,83.83,22.30,70.15,82.90,22.13,68.23,82.21,22.00,68.22,82.20,21.98,67.94,82.12,21.67,67.50,81.99,21.17,67.33,81.94,20.84,67.14,81.90,20.27,66.86,81.88,18.91,66.58,81.96,15.72,66.25,82.06,11.67,65.94,82.17,7.68,65.65,82.30,3.52,65.61,82.37,1.94,65.78,82.51,0.44,66.23,82.79,-1.50,67.34,83.41,-5.44,68.45,84.04,-9.38,69.56,84.66,-13.32,70.58,85.23,-16.89,70.93,85.41,-17.82,71.19,85.53,-18.28,71.85,85.81,-19.02,72.69,86.17,-19.90,72.77,86.20,-19.96,72.84,86.22,-19.98,73.33,86.40,-20.00,75.03,87.02,-20.00,75.50,87.19,-19.93,75.75,87.28,-19.87,76.66,87.58,-19.34,78.79,88.26,-17.68,81.49,89.13,-15.57,84.70,90.16,-13.05,87.88,91.17,-10.56,90.98,92.16,-8.09,93.68,93.02,-5.89,96.34,93.86,-3.52,98.98,94.69,-1.15,100.09,95.03,0.04,101.06,95.31,1.34,101.88,95.53,2.76,102.55,95.68,4.26,103.06,95.78,5.94,103.36,95.80,7.56],
        [104.34,93.40,9.08,104.28,93.29,10.72,104.02,93.10,12.40,103.61,92.87,13.93,102.98,92.55,15.51,102.23,92.20,16.89,101.29,91.78,18.23,100.23,91.33,19.41,99.04,90.84,20.45,97.68,90.30,21.36,96.35,89.77,22.04,94.87,89.21,22.57,93.30,88.62,22.91,91.76,88.05,23.05,88.39,86.82,23.03,84.88,85.55,22.98,81.37,84.28,22.79,77.51,82.89,22.57,74.86,81.93,22.40,72.75,81.17,22.26,70.65,80.42,22.12,68.92,79.79,22.00,68.91,79.79,21.99,68.76,79.74,21.83,68.47,79.66,21.49,68.36,79.63,21.28,68.23,79.60,20.86,68.06,79.59,19.96,67.73,79.70,15.97,67.40,79.80,11.93,67.09,79.91,7.94,66.82,80.03,4.07,66.83,80.10,2.76,67.03,80.26,1.29,67.66,80.63,-1.15,68.77,81.25,-5.09,69.88,81.87,-9.02,70.99,82.49,-12.95,72.08,83.11,-16.82,72.47,83.31,-17.93,72.74,83.44,-18.43,73.21,83.64,-18.98,74.09,84.01,-19.90,74.16,84.04,-19.96,74.22,84.06,-19.98,74.85,84.29,-20.00,76.50,84.89,-20.00,77.08,85.10,-19.91,77.37,85.20,-19.80,77.84,85.35,-19.56,79.34,85.84,-18.38,82.53,86.86,-15.87,85.68,87.87,-13.38,88.78,88.86,-10.90,91.73,89.79,-8.51,94.23,90.58,-6.31,97.21,91.52,-3.64,99.85,92.35,-1.27,100.96,92.69,-0.08,101.93,92.97,1.23,102.75,93.18,2.65,103.41,93.34,4.16,103.91,93.43,5.81,104.22,93.45,7.43],
        [105.19,91.06,8.95,105.13,90.94,10.60,104.88,90.76,12.25,104.46,90.52,13.80,103.84,90.21,15.35,103.09,89.86,16.76,102.16,89.44,18.09,101.09,88.99,19.27,99.90,88.50,20.31,98.57,87.96,21.21,97.21,87.43,21.90,95.73,86.86,22.44,94.19,86.28,22.78,92.65,85.71,22.92,89.30,84.50,22.90,85.47,83.10,22.85,82.27,81.95,22.76,78.40,80.55,22.54,74.53,79.15,22.31,71.79,78.16,22.15,70.64,77.75,22.07,69.60,77.38,22.00,69.60,77.37,22.00,69.55,77.36,21.95,69.44,77.33,21.81,69.40,77.32,21.74,69.34,77.31,21.54,69.21,77.33,20.27,68.88,77.43,16.23,68.54,77.54,12.18,68.24,77.65,8.20,68.01,77.75,4.81,68.05,77.84,3.59,68.28,78.00,2.16,69.09,78.46,-0.80,70.20,79.08,-4.73,71.30,79.71,-8.65,72.41,80.33,-12.58,73.52,80.95,-16.50,74.01,81.21,-18.00,74.29,81.35,-18.57,74.60,81.48,-18.98,75.48,81.85,-19.91,75.56,81.88,-19.96,75.61,81.90,-19.98,76.13,82.10,-20.00,77.92,82.75,-20.00,78.32,82.89,-19.94,78.81,83.06,-19.79,79.16,83.18,-19.62,80.39,83.57,-18.66,83.52,84.57,-16.17,86.62,85.56,-13.68,89.63,86.52,-11.24,92.11,87.30,-9.10,95.10,88.24,-6.42,98.09,89.18,-3.75,100.71,90.00,-1.39,101.83,90.34,-0.19,102.80,90.62,1.12,103.61,90.84,2.54,104.27,91.00,4.05,104.77,91.09,5.67,105.07,91.11,7.30],
        [106.05,88.71,8.82,105.98,88.60,10.47,105.74,88.42,12.10,105.31,88.17,13.68,104.71,87.87,15.20,103.94,87.51,16.62,103.02,87.10,17.94,101.96,86.65,19.13,100.77,86.16,20.17,99.46,85.63,21.06,98.08,85.09,21.77,96.61,84.53,22.30,95.09,83.95,22.64,93.54,83.38,22.79,90.27,82.19,22.77,86.43,80.80,22.73,82.68,79.44,22.69,79.76,78.38,22.53,76.49,77.20,22.34,73.21,76.02,22.15,70.55,75.06,22.00,70.55,75.06,22.00,70.55,75.06,22.00,70.55,75.06,22.00,70.55,75.06,22.00,70.52,75.05,21.93,70.42,75.05,21.36,70.17,75.13,18.26,69.91,75.21,15.17,69.66,75.29,12.07,69.38,75.39,8.37,69.20,75.48,5.58,69.28,75.57,4.42,69.55,75.75,3.01,70.55,76.31,-0.54,71.65,76.93,-4.45,72.75,77.55,-8.36,73.85,78.17,-12.27,74.96,78.79,-16.18,75.53,79.11,-18.06,75.85,79.26,-18.72,76.13,79.38,-19.12,76.88,79.70,-19.91,76.95,79.73,-19.96,77.00,79.75,-19.99,77.35,79.88,-20.00,79.05,80.49,-20.00,79.94,80.81,-19.90,79.94,80.81,-19.90,80.41,80.97,-19.68,81.69,81.38,-18.69,84.53,82.29,-16.41,87.55,83.25,-13.95,90.03,84.04,-11.85,93.02,84.97,-9.18,96.00,85.91,-6.51,98.99,86.85,-3.83,101.59,87.66,-1.50,102.70,88.00,-0.31,103.66,88.28,1.01,104.48,88.50,2.43,105.13,88.65,3.95,105.62,88.74,5.54,105.92,88.76,7.16],
        [106.90,86.37,8.68,106.84,86.25,10.34,106.59,86.07,11.95,106.17,85.83,13.55,105.58,85.53,15.04,104.81,85.17,16.47,103.89,84.76,17.79,102.83,84.31,18.98,101.64,83.82,20.02,100.37,83.31,20.90,98.97,82.76,21.62,97.52,82.20,22.15,95.99,81.63,22.50,94.45,81.05,22.66,91.29,79.91,22.64,87.46,78.52,22.60,83.64,77.13,22.56,80.48,75.98,22.50,77.39,74.87,22.32,74.30,73.75,22.14,71.94,72.90,22.00,71.93,72.89,21.99,71.91,72.89,21.99,71.90,72.89,21.97,71.87,72.88,21.94,71.61,72.81,21.42,71.53,72.80,21.15,71.28,72.88,18.15,71.04,72.95,15.16,70.79,73.03,12.16,70.52,73.14,8.37,70.41,73.21,6.37,70.52,73.32,5.22,70.94,73.57,3.47,72.04,74.19,-0.41,73.13,74.80,-4.30,74.23,75.42,-8.18,75.32,76.03,-12.07,76.42,76.65,-15.95,77.09,77.01,-18.18,77.42,77.17,-18.90,77.69,77.30,-19.29,78.28,77.55,-19.92,78.34,77.57,-19.96,78.40,77.59,-19.99,78.62,77.67,-20.00,80.23,78.26,-20.00,80.79,78.46,-19.97,81.35,78.66,-19.86,81.75,78.79,-19.67,82.55,79.05,-19.05,85.56,80.01,-16.60,88.15,80.83,-14.43,91.01,81.73,-11.87,93.98,82.66,-9.21,96.95,83.59,-6.55,99.92,84.53,-3.89,102.47,85.33,-1.60,103.57,85.66,-0.41,104.53,85.94,0.90,105.35,86.16,2.33,106.00,86.31,3.85,106.47,86.39,5.40,106.78,86.41,7.03],
        [107.75,84.02,8.55,107.69,83.90,10.21,107.45,83.73,11.79,107.02,83.48,13.41,106.44,83.19,14.88,105.68,82.83,16.32,104.76,82.42,17.64,103.69,81.97,18.84,102.52,81.48,19.87,101.26,80.98,20.74,99.87,80.43,21.47,98.43,79.88,22.00,96.88,79.29,22.37,95.36,78.73,22.52,92.32,77.62,22.51,88.51,76.24,22.47,84.70,74.86,22.43,80.90,73.47,22.39,78.14,72.48,22.28,75.33,71.46,22.12,73.33,70.74,22.00,73.30,70.73,21.99,73.27,70.72,21.97,73.24,70.71,21.95,73.19,70.70,21.88,72.69,70.57,20.90,72.63,70.55,20.75,72.40,70.62,18.04,72.16,70.70,15.16,71.93,70.77,12.28,71.65,70.89,8.39,71.62,70.95,7.17,71.77,71.06,6.06,72.44,71.45,3.57,73.53,72.06,-0.28,74.62,72.67,-4.14,75.71,73.28,-8.00,76.79,73.89,-11.86,77.88,74.50,-15.71,78.64,74.92,-18.29,78.97,75.08,-19.04,79.25,75.21,-19.46,79.69,75.39,-19.94,79.73,75.41,-19.96,79.79,75.43,-19.99,79.90,75.48,-20.00,81.41,76.03,-20.00,82.26,76.33,-19.94,82.66,76.47,-19.84,83.08,76.61,-19.63,83.62,76.78,-19.20,86.29,77.63,-16.98,89.04,78.50,-14.53,92.00,79.43,-11.88,94.95,80.35,-9.24,97.90,81.28,-6.60,100.86,82.21,-3.95,103.37,82.99,-1.69,104.45,83.32,-0.51,105.40,83.60,0.79,106.21,83.81,2.22,106.87,83.96,3.75,107.32,84.05,5.26,107.63,84.07,6.90],
        [108.61,81.67,8.42,108.54,81.56,10.09,108.31,81.39,11.64,107.88,81.14,13.27,107.31,80.85,14.73,106.54,80.49,16.17,105.62,80.08,17.51,104.57,79.63,18.69,103.41,79.15,19.72,102.15,78.64,20.59,100.75,78.09,21.33,99.36,77.56,21.85,97.80,76.97,22.22,96.28,76.41,22.39,93.35,75.34,22.38,89.57,73.97,22.34,85.78,72.59,22.30,82.00,71.22,22.26,78.55,69.97,22.21,76.23,69.13,22.09,74.71,68.58,22.00,74.67,68.57,21.98,74.63,68.55,21.96,74.59,68.54,21.92,74.51,68.52,21.81,73.82,68.34,20.48,73.71,68.31,20.25,73.52,68.37,17.93,73.29,68.44,15.18,73.06,68.52,12.37,72.82,68.62,8.89,72.84,68.68,8.01,73.03,68.81,6.94,73.94,69.32,3.68,75.02,69.93,-0.15,76.10,70.53,-3.98,77.18,71.14,-7.81,78.26,71.75,-11.64,79.34,72.36,-15.47,80.17,72.81,-18.32,80.51,72.98,-19.14,80.79,73.11,-19.61,81.09,73.24,-19.95,81.12,73.25,-19.96,81.19,73.28,-20.00,81.21,73.29,-20.00,82.58,73.79,-20.00,83.69,74.18,-19.91,83.78,74.22,-19.89,84.09,74.32,-19.73,84.94,74.59,-19.09,87.13,75.28,-17.14,90.06,76.20,-14.51,92.99,77.12,-11.89,95.93,78.05,-9.26,98.86,78.97,-6.63,101.79,79.89,-4.01,104.26,80.66,-1.78,105.33,80.99,-0.61,106.26,81.25,0.67,107.08,81.47,2.11,107.73,81.62,3.65,108.18,81.70,5.12,108.48,81.72,6.76],
        [109.46,79.33,8.29,109.40,79.21,9.97,109.17,79.05,11.48,108.74,78.79,13.13,108.17,78.51,14.58,107.40,78.14,16.04,106.48,77.74,17.37,105.45,77.30,18.53,104.31,76.82,19.55,103.02,76.31,20.45,101.62,75.75,21.19,100.30,75.25,21.69,98.73,74.65,22.08,97.23,74.10,22.25,94.41,73.07,22.25,90.66,71.71,22.21,86.90,70.34,22.17,83.15,68.98,22.13,79.39,67.61,22.09,76.60,66.60,22.03,76.10,66.42,21.99,76.04,66.40,21.98,75.99,66.38,21.94,75.93,66.36,21.89,75.83,66.34,21.75,75.03,66.13,20.21,74.80,66.07,19.74,74.63,66.12,17.85,74.42,66.19,15.25,74.20,66.26,12.43,74.01,66.35,9.69,74.07,66.41,8.89,74.35,66.58,7.66,75.42,67.18,3.86,76.49,67.79,0.06,77.56,68.39,-3.73,78.63,68.99,-7.53,79.70,69.59,-11.33,80.77,70.19,-15.13,81.68,70.70,-18.31,82.03,70.88,-19.21,82.33,71.02,-19.74,82.40,71.05,-19.84,82.44,71.06,-19.88,82.67,71.15,-19.91,83.31,71.38,-19.88,83.96,71.61,-19.86,84.33,71.75,-19.84,84.83,71.93,-19.72,85.20,72.05,-19.55,86.37,72.42,-18.63,88.27,73.02,-16.98,91.16,73.93,-14.42,94.06,74.84,-11.83,96.96,75.75,-9.23,99.86,76.66,-6.64,102.75,77.57,-4.04,105.16,78.33,-1.86,106.22,78.65,-0.69,107.12,78.91,0.55,107.94,79.13,2.00,108.59,79.28,3.55,109.03,79.36,4.99,109.34,79.38,6.61],
        [110.32,76.98,8.16,110.25,76.86,9.86,110.03,76.70,11.31,109.60,76.45,12.97,109.03,76.17,14.42,108.25,75.80,15.92,107.37,75.41,17.19,106.36,74.98,18.34,105.24,74.51,19.36,103.90,73.97,20.31,102.57,73.45,21.01,101.21,72.92,21.54,99.66,72.34,21.94,98.24,71.81,22.11,95.59,70.84,22.12,91.92,69.51,22.08,88.24,68.17,22.04,85.03,67.01,22.01,82.87,66.22,21.98,80.72,65.44,21.95,78.56,64.66,21.92,77.32,64.21,21.90,77.30,64.20,21.89,77.27,64.19,21.86,77.16,64.16,21.70,76.32,63.94,20.09,75.89,63.83,19.24,75.76,63.86,17.86,75.56,63.93,15.45,75.35,64.00,12.69,75.22,64.07,10.60,75.29,64.14,9.86,75.77,64.42,8.02,76.83,65.01,4.27,77.89,65.61,0.52,78.95,66.20,-3.23,80.00,66.79,-6.98,81.06,67.39,-10.73,82.12,67.98,-14.47,83.16,68.57,-18.15,83.58,68.79,-19.34,83.61,68.80,-19.39,83.62,68.80,-19.42,83.63,68.81,-19.44,83.77,68.86,-19.43,83.95,68.92,-19.41,84.13,68.99,-19.40,85.16,69.36,-19.27,85.96,69.64,-19.09,86.26,69.74,-18.94,87.03,69.99,-18.35,89.62,70.81,-16.19,92.56,71.74,-13.73,95.46,72.66,-11.27,98.32,73.56,-8.83,101.03,74.42,-6.48,103.80,75.29,-4.00,106.04,75.99,-1.97,107.13,76.32,-0.76,107.97,76.56,0.41,108.80,76.78,1.88,109.46,76.93,3.44,109.89,77.01,4.88,110.19,77.03,6.46],
        [110.73,75.77,8.09,110.67,75.66,9.72,110.44,75.49,11.27,110.03,75.25,12.85,109.46,74.96,14.33,108.73,74.62,15.72,107.83,74.21,17.04,106.83,73.78,18.21,105.67,73.30,19.26,104.41,72.80,20.16,103.10,72.28,20.88,101.67,71.73,21.44,100.18,71.16,21.83,98.68,70.61,22.02,96.45,69.79,22.03,92.75,68.45,21.98,89.04,67.10,21.93,86.87,66.31,21.90,84.74,65.54,21.86,82.62,64.77,21.83,80.50,64.00,21.80,78.37,63.23,21.77,77.89,63.05,21.75,77.88,63.05,21.74,77.84,63.04,21.67,77.01,62.83,20.08,76.45,62.68,18.98,76.34,62.70,17.90,76.15,62.76,15.60,75.94,62.84,12.89,75.84,62.90,11.09,75.91,62.97,10.40,76.48,63.29,8.29,77.53,63.88,4.58,78.58,64.47,0.87,79.62,65.06,-2.84,80.67,65.64,-6.56,81.72,66.23,-10.27,82.76,66.82,-13.98,83.81,67.41,-17.69,84.10,67.57,-18.66,84.14,67.59,-18.77,84.18,67.61,-18.88,84.22,67.63,-19.00,84.41,67.70,-18.97,84.62,67.77,-18.94,84.82,67.85,-18.91,85.47,68.08,-18.81,86.42,68.41,-18.61,86.68,68.50,-18.50,87.25,68.68,-18.04,90.20,69.62,-15.61,93.15,70.56,-13.18,96.10,71.49,-10.75,99.04,72.43,-8.32,101.94,73.35,-5.92,104.64,74.20,-3.64,106.46,74.78,-2.02,107.51,75.10,-0.85,108.44,75.36,0.44,109.24,75.57,1.86,109.86,75.72,3.35,110.33,75.80,4.91,110.61,75.82,6.49],
        [110.79,75.15,8.06,110.73,75.04,9.65,110.50,74.87,11.22,110.09,74.64,12.77,109.53,74.35,14.23,108.80,74.01,15.61,107.92,73.61,16.92,106.92,73.19,18.07,105.78,72.71,19.11,104.53,72.21,20.00,103.21,71.69,20.71,101.82,71.15,21.26,100.35,70.60,21.64,98.88,70.05,21.83,96.38,69.14,21.90,92.91,67.87,21.87,89.25,66.55,21.82,86.79,65.65,21.79,84.90,64.97,21.76,83.02,64.28,21.73,81.14,63.60,21.71,79.25,62.91,21.68,78.15,62.52,21.65,78.13,62.51,21.61,78.10,62.50,21.56,77.31,62.30,20.02,76.71,62.14,18.85,76.61,62.16,17.86,76.42,62.22,15.63,76.22,62.30,12.94,76.13,62.35,11.31,76.21,62.42,10.64,76.82,62.77,8.38,77.86,63.35,4.70,78.90,63.94,1.02,79.94,64.52,-2.66,80.98,65.10,-6.34,82.01,65.69,-10.02,83.05,66.27,-13.70,84.09,66.85,-17.39,84.46,67.06,-18.65,84.47,67.06,-18.68,84.48,67.07,-18.71,84.49,67.07,-18.74,84.64,67.13,-18.72,84.80,67.18,-18.69,84.95,67.24,-18.67,85.49,67.43,-18.57,86.66,67.84,-18.29,86.95,67.94,-18.19,87.46,68.10,-17.76,90.38,69.03,-15.36,93.30,69.96,-12.95,96.22,70.89,-10.55,99.14,71.81,-8.14,102.06,72.74,-5.73,104.98,73.67,-3.33,106.56,74.17,-1.95,107.60,74.49,-0.79,108.53,74.75,0.51,109.31,74.96,1.89,109.93,75.10,3.36,110.39,75.18,4.90,110.67,75.20,6.47],
        [110.63,74.46,8.03,110.56,74.34,9.66,110.35,74.19,11.07,109.93,73.94,12.66,109.39,73.67,14.06,108.69,73.34,15.39,107.77,72.93,16.72,106.87,72.55,17.75,105.72,72.07,18.80,104.52,71.58,19.65,103.13,71.04,20.39,101.78,70.52,20.90,100.33,69.97,21.26,98.34,69.23,21.56,95.75,68.28,21.68,92.71,67.17,21.71,89.20,65.90,21.69,85.61,64.59,21.65,82.03,63.29,21.61,80.42,62.70,21.59,79.64,62.42,21.58,78.86,62.14,21.57,78.42,61.98,21.56,78.42,61.98,21.54,78.41,61.98,21.53,77.56,61.76,19.90,76.96,61.61,18.73,76.87,61.63,17.76,76.69,61.68,15.60,76.49,61.76,12.90,76.42,61.81,11.53,76.50,61.88,10.87,77.18,62.26,8.41,78.21,62.84,4.78,79.23,63.41,1.14,80.26,63.99,-2.49,81.28,64.56,-6.12,82.31,65.14,-9.75,83.33,65.71,-13.38,84.35,66.29,-17.02,84.69,66.48,-18.20,84.71,66.49,-18.28,84.73,66.50,-18.35,84.76,66.51,-18.43,85.08,66.63,-18.37,85.41,66.75,-18.31,85.74,66.86,-18.25,86.08,66.98,-18.19,86.67,67.19,-18.05,87.13,67.35,-17.85,89.15,67.99,-16.22,91.48,68.73,-14.29,93.82,69.47,-12.37,96.24,70.24,-10.37,99.09,71.15,-8.00,101.87,72.03,-5.68,104.62,72.91,-3.35,106.45,73.48,-1.77,107.54,73.81,-0.56,108.42,74.06,0.67,109.17,74.26,2.00,109.80,74.41,3.50,110.21,74.48,4.87,110.52,74.50,6.50],
        [109.02,72.60,7.95,108.96,72.49,9.42,108.75,72.33,10.88,108.38,72.12,12.26,107.85,71.85,13.61,107.18,71.54,14.87,106.37,71.18,16.03,105.45,70.79,17.06,104.39,70.35,18.00,103.23,69.88,18.80,101.82,69.33,19.57,100.32,68.75,20.16,98.62,68.10,20.63,96.57,67.34,20.99,94.47,66.56,21.17,91.49,65.47,21.29,88.39,64.34,21.33,85.06,63.13,21.33,81.68,61.90,21.31,80.04,61.30,21.30,79.60,61.14,21.30,79.16,60.98,21.29,78.91,60.90,21.25,78.87,60.88,21.16,78.82,60.87,21.07,77.95,60.65,19.39,77.48,60.53,18.46,77.38,60.55,17.34,77.21,60.61,15.35,77.01,60.69,12.61,77.00,60.72,11.91,77.11,60.79,11.29,77.96,61.27,8.27,78.93,61.82,4.81,79.91,62.37,1.35,80.88,62.92,-2.11,81.86,63.46,-5.56,82.83,64.01,-9.02,83.81,64.56,-12.48,84.73,65.07,-15.73,84.86,65.15,-16.19,84.99,65.22,-16.65,85.13,65.30,-17.11,85.26,65.37,-17.57,85.31,65.40,-17.67,85.34,65.40,-17.67,85.36,65.41,-17.66,85.39,65.42,-17.66,86.82,65.92,-17.28,87.45,66.14,-17.04,88.43,66.45,-16.29,90.57,67.13,-14.48,92.82,67.85,-12.58,95.40,68.66,-10.35,97.75,69.40,-8.26,100.09,70.13,-6.09,102.46,70.86,-3.78,104.77,71.58,-1.51,106.06,71.97,-0.12,106.92,72.22,1.05,107.67,72.41,2.37,108.21,72.54,3.65,108.64,72.62,5.06,108.91,72.64,6.51],
        [107.00,70.47,7.85,106.96,70.37,9.25,106.75,70.22,10.63,106.42,70.03,11.88,105.92,69.78,13.16,105.30,69.49,14.32,104.55,69.15,15.40,103.72,68.80,16.33,102.69,68.37,17.24,101.60,67.93,17.98,100.25,67.40,18.71,98.71,66.81,19.34,96.94,66.13,19.86,94.85,65.35,20.30,92.84,64.60,20.56,90.37,63.69,20.73,87.49,62.64,20.84,84.44,61.53,20.90,83.12,61.04,20.90,81.88,60.59,20.91,80.64,60.14,20.92,79.49,59.72,20.92,79.49,59.72,20.92,79.48,59.72,20.92,79.30,59.67,20.63,78.35,59.43,18.78,78.04,59.35,18.18,77.94,59.38,16.98,77.79,59.42,15.19,77.62,59.49,12.83,77.64,59.53,12.38,77.83,59.64,11.55,78.74,60.15,8.32,79.65,60.66,5.10,80.56,61.18,1.87,81.47,61.69,-1.35,82.38,62.20,-4.58,83.29,62.71,-7.80,84.20,63.22,-11.03,85.06,63.70,-13.95,85.43,63.90,-15.15,85.55,63.96,-15.50,85.67,64.02,-15.86,85.79,64.09,-16.22,85.88,64.14,-16.48,85.91,64.15,-16.47,85.95,64.16,-16.46,85.98,64.17,-16.45,86.30,64.28,-16.33,87.58,64.72,-15.84,87.74,64.77,-15.76,89.30,65.26,-14.36,91.35,65.90,-12.46,93.72,66.64,-10.14,96.11,67.38,-7.80,98.50,68.11,-5.47,100.84,68.84,-3.18,102.95,69.49,-1.06,104.23,69.88,0.33,105.05,70.11,1.44,105.73,70.29,2.66,106.24,70.41,3.84,106.65,70.49,5.18,106.91,70.50,6.55],
        [103.18,66.42,7.67,103.14,66.34,8.90,102.96,66.20,10.12,102.71,66.06,11.10,102.28,65.84,12.24,101.78,65.60,13.19,101.15,65.32,14.12,100.51,65.05,14.85,99.55,64.65,15.70,98.70,64.31,16.30,97.49,63.83,16.99,96.11,63.29,17.60,94.31,62.60,18.22,92.53,61.93,18.71,90.55,61.18,19.14,88.74,60.51,19.42,86.61,59.72,19.64,84.28,58.86,19.80,82.35,58.16,19.89,81.59,57.88,19.92,80.83,57.60,19.95,80.47,57.47,19.95,80.44,57.46,19.92,80.31,57.42,19.79,80.05,57.35,19.44,79.46,57.19,18.31,79.12,57.11,17.66,79.06,57.13,16.89,78.94,57.16,15.54,78.82,57.21,13.79,78.85,57.24,13.46,79.11,57.39,12.46,79.86,57.82,9.78,80.62,58.24,7.10,81.37,58.66,4.42,82.13,59.09,1.74,82.89,59.51,-0.94,83.63,59.93,-3.56,84.32,60.31,-5.84,84.98,60.65,-7.74,85.76,61.04,-9.60,86.62,61.47,-11.60,86.77,61.54,-11.94,86.77,61.54,-11.95,86.78,61.54,-11.95,86.78,61.55,-11.96,86.79,61.55,-11.96,86.79,61.55,-11.95,86.79,61.55,-11.95,87.14,61.67,-11.80,88.39,62.08,-11.05,88.95,62.26,-10.66,90.25,62.67,-9.38,92.24,63.28,-7.44,94.20,63.89,-5.52,96.10,64.47,-3.63,97.98,65.05,-1.74,99.73,65.59,0.06,100.81,65.92,1.25,101.52,66.12,2.25,102.10,66.27,3.28,102.51,66.37,4.24,102.87,66.43,5.43,103.10,66.45,6.64],
        [99.35,62.37,7.48,99.33,62.30,8.56,99.18,62.19,9.59,99.00,62.09,10.32,98.64,61.90,11.32,98.29,61.73,12.01,97.75,61.49,12.83,97.31,61.30,13.36,96.58,61.00,14.02,95.85,60.70,14.59,94.70,60.25,15.28,93.58,59.81,15.80,92.20,59.28,16.34,90.67,58.69,16.85,89.03,58.07,17.32,87.46,57.48,17.71,85.64,56.79,18.09,84.03,56.19,18.34,82.37,55.58,18.53,81.56,55.28,18.62,81.30,55.18,18.64,81.22,55.15,18.63,81.20,55.14,18.62,81.01,55.08,18.46,80.77,55.01,18.19,80.59,54.96,17.88,80.21,54.87,17.15,80.18,54.87,16.86,80.11,54.89,16.00,80.03,54.93,14.79,80.04,54.95,14.59,80.31,55.10,13.61,80.90,55.43,11.51,81.49,55.77,9.42,82.09,56.10,7.32,82.68,56.43,5.22,83.19,56.71,3.52,83.66,56.96,2.22,84.42,57.33,0.46,85.22,57.73,-1.40,86.03,58.13,-3.27,86.84,58.52,-5.14,87.52,58.86,-6.71,87.53,58.86,-6.73,87.54,58.87,-6.75,87.55,58.87,-6.76,87.56,58.88,-6.77,87.57,58.88,-6.77,87.58,58.88,-6.76,87.60,58.89,-6.76,88.48,59.19,-6.37,89.61,59.56,-5.74,89.91,59.66,-5.52,90.81,59.94,-4.63,92.32,60.40,-3.12,93.82,60.87,-1.59,95.28,61.31,-0.10,96.68,61.74,1.38,97.39,61.96,2.18,98.01,62.13,3.06,98.49,62.26,3.97,98.78,62.32,4.66,99.10,62.38,5.69,99.29,62.39,6.75],
        [95.52,58.32,7.29,95.51,58.27,8.24,95.42,58.20,8.90,95.29,58.11,9.57,94.98,57.95,10.44,94.82,57.87,10.80,94.47,57.72,11.37,94.09,57.55,11.89,93.59,57.34,12.36,92.89,57.05,12.94,92.23,56.79,13.35,91.49,56.50,13.78,90.32,56.04,14.31,89.29,55.64,14.71,88.14,55.20,15.13,86.76,54.67,15.57,85.47,54.19,15.94,84.27,53.73,16.25,82.95,53.23,16.58,81.58,52.72,16.87,81.49,52.68,16.88,81.47,52.68,16.89,81.47,52.68,16.89,81.47,52.68,16.89,81.41,52.66,16.82,81.36,52.64,16.77,81.28,52.62,16.62,81.28,52.62,16.61,81.27,52.63,16.43,81.23,52.65,15.81,81.24,52.65,15.75,81.53,52.82,14.70,81.93,53.04,13.30,82.15,53.16,12.67,82.76,53.46,11.25,83.37,53.76,9.84,83.99,54.06,8.42,84.60,54.37,7.01,85.21,54.67,5.59,85.82,54.97,4.18,86.43,55.27,2.77,87.05,55.57,1.35,87.66,55.87,-0.06,88.27,56.18,-1.48,88.29,56.18,-1.51,88.30,56.19,-1.54,88.32,56.20,-1.57,88.34,56.21,-1.59,88.35,56.21,-1.58,88.37,56.22,-1.57,88.39,56.22,-1.57,88.99,56.43,-1.31,90.03,56.78,-0.83,90.46,56.92,-0.60,90.68,56.99,-0.39,91.58,57.27,0.53,92.64,57.59,1.64,93.64,57.90,2.71,94.01,58.00,3.17,94.47,58.14,3.84,94.90,58.24,4.68,95.07,58.28,5.12,95.33,58.33,5.95,95.48,58.33,6.84],
        [91.69,54.28,7.11,91.69,54.24,7.70,91.69,54.22,8.03,91.62,54.18,8.45,91.53,54.12,8.93,91.45,54.08,9.17,91.30,54.00,9.57,91.19,53.94,9.82,91.00,53.86,10.09,90.77,53.76,10.41,90.61,53.69,10.62,90.53,53.66,10.67,90.19,53.52,10.87,89.71,53.33,11.12,89.19,53.13,11.40,88.72,52.94,11.65,88.45,52.84,11.76,88.18,52.74,11.88,88.02,52.67,11.93,87.90,52.63,11.91,87.49,52.48,11.86,87.29,52.41,11.81,87.11,52.35,11.73,86.95,52.30,11.64,86.85,52.27,11.55,86.75,52.24,11.47,86.69,52.22,11.34,86.63,52.21,11.21,86.59,52.20,11.09,86.58,52.20,10.95,86.57,52.23,10.50,86.58,52.24,10.26,86.67,52.30,9.86,86.76,52.35,9.45,86.93,52.44,8.92,87.11,52.54,8.38,87.29,52.63,7.89,87.52,52.75,7.30,87.75,52.87,6.72,87.99,52.98,6.14,88.23,53.10,5.59,88.47,53.22,5.03,88.71,53.34,4.48,88.95,53.46,3.92,89.04,53.50,3.72,89.06,53.51,3.68,89.08,53.52,3.64,89.10,53.53,3.62,89.10,53.53,3.62,89.10,53.53,3.62,89.10,53.53,3.62,89.31,53.60,3.73,89.71,53.73,3.95,90.10,53.86,4.16,90.46,53.98,4.35,90.64,54.04,4.47,90.78,54.09,4.57,90.95,54.13,4.84,91.12,54.18,5.11,91.26,54.22,5.36,91.37,54.24,5.62,91.54,54.28,6.16,91.56,54.28,6.29,91.66,54.28,6.93],
        [91.32,53.90,7.09,91.32,53.87,7.49,91.32,53.85,7.89,91.32,53.84,8.01,91.25,53.80,8.33,91.16,53.75,8.73,91.08,53.70,9.01,90.93,53.63,9.33,90.83,53.58,9.57,90.74,53.54,9.74,90.68,53.51,9.81,90.50,53.44,9.93,90.17,53.31,10.17,89.79,53.15,10.43,89.41,53.00,10.70,89.18,52.91,10.83,88.97,52.82,10.94,88.76,52.74,11.06,88.66,52.71,11.09,88.60,52.68,11.08,88.23,52.55,11.03,88.05,52.49,11.00,87.89,52.43,10.92,87.70,52.37,10.82,87.59,52.33,10.74,87.52,52.31,10.69,87.45,52.29,10.57,87.39,52.28,10.42,87.32,52.26,10.28,87.30,52.26,10.19,87.30,52.27,9.94,87.29,52.29,9.53,87.31,52.31,9.34,87.39,52.36,8.96,87.46,52.41,8.59,87.59,52.48,8.17,87.74,52.56,7.71,87.89,52.64,7.26,88.02,52.71,6.86,88.22,52.81,6.37,88.41,52.90,5.89,88.60,53.00,5.41,88.80,53.10,4.93,88.96,53.18,4.53,89.10,53.25,4.22,89.10,53.25,4.22,89.11,53.25,4.21,89.11,53.25,4.20,89.11,53.25,4.20,89.11,53.25,4.20,89.11,53.25,4.20,89.15,53.26,4.22,89.32,53.32,4.31,89.52,53.39,4.43,89.86,53.50,4.67,90.29,53.64,4.98,90.66,53.76,5.24,90.73,53.78,5.30,90.78,53.80,5.36,90.89,53.82,5.59,91.00,53.85,5.86,91.11,53.87,6.12,91.18,53.88,6.41,91.27,53.89,6.86]],
    "28" : [
        [55.58,111.50,-8.11,55.59,111.44,-7.07,55.59,111.37,-5.90,55.60,111.31,-4.73,55.60,111.24,-3.55,55.60,111.17,-2.38,55.60,111.10,-1.21,55.61,111.03,-0.03,55.61,110.97,1.14,55.61,110.90,2.31,55.61,110.83,3.46,55.60,110.80,3.85,55.50,110.72,4.32,55.35,110.61,4.86,55.29,110.56,5.06,55.25,110.54,5.15,55.21,110.51,5.24,55.17,110.48,5.33,55.13,110.46,5.36,55.09,110.44,5.35,55.06,110.42,5.34,55.02,110.40,5.32,54.77,110.28,5.25,54.60,110.19,5.20,54.51,110.14,5.16,54.50,110.14,5.14,54.47,110.13,5.08,54.25,110.04,4.46,54.01,109.96,3.79,53.90,109.93,3.26,53.70,109.89,2.16,53.57,109.88,1.20,53.41,109.86,0.06,53.24,109.84,-1.11,53.08,109.82,-2.27,52.92,109.81,-3.44,52.76,109.79,-4.59,52.61,109.78,-5.69,52.47,109.76,-6.76,52.32,109.75,-7.83,52.25,109.76,-8.66,52.24,109.78,-9.09,52.30,109.84,-9.55,52.39,109.92,-9.99,52.56,110.03,-10.52,52.75,110.16,-10.92,52.97,110.30,-11.35,53.18,110.43,-11.61,53.55,110.65,-11.98,53.70,110.74,-12.11,53.75,110.77,-12.14,54.00,110.90,-12.16,54.21,111.01,-12.17,54.30,111.06,-12.17,54.43,111.13,-12.14,54.62,111.22,-11.97,54.82,111.31,-11.80,54.89,111.35,-11.74,54.92,111.36,-11.67,55.05,111.41,-11.30,55.18,111.45,-10.93,55.34,111.50,-10.28,55.48,111.53,-9.49,55.56,111.53,-8.79],
        [59.36,110.58,-6.47,59.37,110.48,-4.87,59.37,110.38,-3.05,59.38,110.27,-1.23,59.38,110.17,0.59,59.38,110.06,2.41,59.38,109.96,4.18,59.32,109.85,5.37,59.18,109.70,6.62,58.98,109.53,7.74,58.78,109.37,8.55,58.57,109.23,9.19,58.19,108.97,10.06,57.87,108.77,10.60,57.68,108.66,10.82,57.47,108.54,11.00,57.31,108.44,11.15,57.11,108.33,11.20,56.89,108.21,11.20,56.74,108.13,11.21,56.61,108.06,11.19,56.47,108.00,11.10,56.01,107.77,10.79,55.65,107.60,10.44,55.00,107.30,9.69,54.38,107.02,8.91,54.13,106.91,8.54,53.97,106.84,8.20,53.90,106.82,7.86,53.67,106.78,6.53,53.49,106.77,5.14,53.37,106.78,3.70,53.24,106.83,1.92,53.12,106.87,0.11,53.00,106.91,-1.71,52.87,106.95,-3.52,52.75,106.99,-5.34,52.63,107.04,-7.15,52.56,107.08,-8.46,52.58,107.16,-9.66,52.67,107.28,-10.91,52.84,107.45,-12.21,53.04,107.63,-13.44,53.04,107.63,-13.45,53.05,107.64,-13.46,53.30,107.78,-13.64,53.56,107.93,-13.81,54.08,108.23,-14.16,54.97,108.73,-14.66,55.32,108.92,-14.77,55.65,109.10,-14.86,55.87,109.22,-14.86,56.14,109.36,-14.83,56.38,109.49,-14.78,56.49,109.54,-14.70,56.77,109.67,-14.50,56.93,109.75,-14.32,57.22,109.87,-13.83,57.72,110.08,-12.81,58.17,110.26,-11.77,58.61,110.42,-10.55,58.94,110.53,-9.42,59.15,110.58,-8.50,59.31,110.60,-7.41],
        [63.14,109.65,-4.83,63.14,109.54,-2.93,63.15,109.42,-0.77,63.15,109.29,1.39,63.16,109.17,3.55,63.15,109.07,5.03,63.05,108.93,6.46,62.86,108.76,7.77,62.61,108.55,8.92,62.39,108.39,9.66,61.93,108.09,10.70,61.45,107.79,11.50,60.98,107.50,12.14,60.44,107.18,12.75,59.74,106.76,13.40,59.15,106.42,13.84,58.85,106.25,14.01,58.51,106.07,14.06,58.33,105.98,14.08,58.19,105.90,14.05,57.72,105.66,13.88,57.55,105.57,13.79,56.41,105.02,12.97,55.91,104.77,12.59,55.85,104.75,12.54,55.79,104.73,12.33,55.71,104.71,11.97,55.51,104.66,11.01,55.11,104.56,9.05,54.85,104.51,7.48,54.66,104.51,5.90,54.51,104.54,4.05,54.37,104.59,1.93,54.22,104.64,-0.23,54.07,104.69,-2.39,53.93,104.74,-4.54,53.79,104.78,-6.56,53.75,104.85,-7.93,53.82,104.96,-9.28,53.97,105.13,-10.71,54.21,105.34,-12.20,54.54,105.61,-13.74,54.92,105.90,-15.20,54.97,105.93,-15.34,55.01,105.96,-15.47,55.07,106.00,-15.55,55.15,106.04,-15.58,55.28,106.12,-15.63,56.52,106.80,-16.01,57.17,107.15,-16.15,57.62,107.39,-16.19,57.85,107.51,-16.16,58.23,107.71,-16.06,58.35,107.77,-16.02,58.82,108.00,-15.66,58.90,108.03,-15.57,59.60,108.35,-14.55,60.45,108.72,-13.19,61.17,109.02,-11.88,61.71,109.24,-10.72,62.24,109.44,-9.39,62.60,109.57,-8.23,62.90,109.65,-7.03,63.07,109.68,-5.92],
        [66.91,108.73,-3.19,66.92,108.60,-1.06,66.93,108.46,1.36,66.93,108.32,3.78,66.93,108.21,5.58,66.82,108.07,7.04,66.61,107.87,8.49,66.35,107.67,9.60,66.08,107.47,10.44,65.64,107.18,11.43,65.04,106.80,12.41,64.47,106.46,13.12,63.80,106.06,13.82,63.03,105.61,14.46,62.00,105.03,15.19,60.98,104.45,15.78,60.38,104.11,16.04,59.90,103.85,16.13,59.76,103.78,16.13,59.32,103.55,16.02,59.01,103.39,15.91,58.03,102.90,15.40,57.74,102.76,15.24,57.45,102.61,15.08,57.38,102.58,15.00,57.36,102.57,14.93,57.12,102.50,13.99,56.70,102.39,12.09,56.27,102.29,9.95,56.00,102.24,8.32,55.82,102.25,6.53,55.65,102.29,4.32,55.49,102.35,1.93,55.33,102.41,-0.49,55.16,102.46,-2.91,55.00,102.52,-5.32,54.95,102.58,-6.79,55.01,102.69,-8.21,55.17,102.87,-9.73,55.45,103.11,-11.32,55.85,103.43,-13.07,56.36,103.82,-15.05,56.62,104.00,-15.87,56.90,104.19,-16.44,56.92,104.20,-16.47,56.98,104.24,-16.54,57.07,104.29,-16.59,57.20,104.36,-16.62,58.10,104.84,-16.74,59.15,105.41,-16.84,59.63,105.66,-16.83,59.93,105.82,-16.73,60.28,106.00,-16.60,60.48,106.09,-16.44,61.11,106.39,-15.81,62.11,106.85,-14.63,62.98,107.25,-13.48,63.90,107.65,-12.09,64.62,107.96,-10.86,65.34,108.26,-9.44,65.92,108.48,-8.03,66.30,108.62,-6.88,66.66,108.73,-5.49,66.83,108.75,-4.39],
        [70.69,107.80,-1.55,70.70,107.67,0.77,70.70,107.51,3.42,70.71,107.36,5.97,70.64,107.25,7.29,70.46,107.07,8.67,70.17,106.84,10.02,69.88,106.63,10.94,69.40,106.31,12.04,68.79,105.93,13.05,68.23,105.58,13.78,67.49,105.14,14.56,66.69,104.68,15.24,65.62,104.06,16.00,64.51,103.44,16.62,63.29,102.76,17.16,62.02,102.05,17.58,61.40,101.72,17.72,61.23,101.63,17.73,60.69,101.35,17.65,59.67,100.83,17.25,59.34,100.66,17.11,59.02,100.50,16.98,58.90,100.44,16.91,58.88,100.43,16.86,58.70,100.36,16.35,58.29,100.23,14.91,57.85,100.11,12.94,57.40,100.01,10.73,57.16,99.98,9.05,56.97,100.00,6.91,56.79,100.05,4.45,56.61,100.11,1.81,56.43,100.18,-0.84,56.25,100.24,-3.48,56.14,100.29,-5.41,56.17,100.40,-6.84,56.33,100.57,-8.41,56.61,100.82,-10.07,57.06,101.17,-11.97,57.67,101.64,-14.37,58.06,101.93,-15.80,58.29,102.09,-16.38,58.45,102.19,-16.66,58.79,102.40,-17.07,58.80,102.40,-17.08,58.84,102.42,-17.09,59.04,102.53,-17.12,60.76,103.46,-17.32,61.49,103.84,-17.31,61.71,103.96,-17.25,62.05,104.13,-17.12,62.29,104.25,-16.98,62.70,104.45,-16.63,63.70,104.92,-15.49,65.27,105.64,-13.68,66.40,106.16,-12.30,67.31,106.57,-11.03,68.28,106.99,-9.45,68.94,107.27,-8.18,69.57,107.51,-6.75,70.05,107.69,-5.36,70.41,107.80,-3.96,70.60,107.83,-2.84],
        [74.46,106.88,0.08,74.47,106.73,2.60,74.48,106.57,5.45,74.45,106.44,7.36,74.30,106.27,8.86,74.06,106.06,10.13,73.71,105.81,11.36,73.23,105.48,12.50,72.73,105.17,13.39,72.13,104.80,14.25,71.40,104.35,15.10,70.54,103.85,15.88,69.34,103.16,16.74,68.27,102.56,17.35,67.09,101.90,17.89,65.61,101.08,18.37,63.84,100.12,18.80,62.88,99.59,18.99,62.31,99.30,18.95,61.62,98.94,18.78,60.46,98.34,18.43,60.43,98.33,18.42,60.38,98.31,18.37,60.28,98.26,18.15,60.17,98.22,17.94,59.82,98.08,17.03,59.41,97.95,15.64,58.95,97.83,13.58,58.53,97.73,11.41,58.31,97.72,9.67,58.10,97.76,7.07,57.90,97.82,4.36,57.71,97.88,1.50,57.51,97.95,-1.36,57.35,98.01,-3.87,57.34,98.10,-5.35,57.47,98.26,-6.93,57.73,98.50,-8.62,58.18,98.85,-10.57,58.85,99.36,-13.19,59.46,99.82,-15.54,59.74,100.03,-16.38,59.97,100.17,-16.83,60.27,100.36,-17.21,60.57,100.53,-17.55,60.59,100.55,-17.57,60.63,100.57,-17.57,61.57,101.08,-17.69,62.81,101.74,-17.81,63.31,102.01,-17.78,63.57,102.14,-17.71,64.00,102.36,-17.55,64.35,102.53,-17.26,64.93,102.80,-16.65,66.69,103.62,-14.69,68.40,104.41,-12.74,69.92,105.12,-10.96,70.93,105.57,-9.60,71.82,105.96,-8.23,72.59,106.29,-6.82,73.29,106.57,-5.27,73.80,106.76,-3.83,74.16,106.87,-2.45,74.37,106.91,-1.27],
        [78.24,105.95,1.72,78.25,105.80,4.41,78.25,105.63,7.22,78.17,105.50,8.63,77.92,105.28,10.28,77.60,105.03,11.58,77.21,104.75,12.64,76.72,104.43,13.64,76.04,104.01,14.70,75.25,103.53,15.67,74.36,103.01,16.52,73.26,102.38,17.34,72.11,101.72,18.04,70.87,101.03,18.62,69.65,100.36,19.04,67.85,99.37,19.49,65.99,98.37,19.81,64.45,97.54,19.98,63.69,97.13,19.97,62.37,96.45,19.68,62.12,96.32,19.62,61.89,96.20,19.55,61.80,96.16,19.43,61.71,96.12,19.30,61.30,95.94,18.57,60.90,95.79,17.61,60.50,95.66,16.26,60.02,95.53,14.05,59.65,95.45,12.06,59.44,95.47,9.96,59.22,95.52,7.07,59.01,95.58,4.12,58.80,95.66,1.06,58.59,95.73,-2.00,58.52,95.79,-3.78,58.60,95.93,-5.31,58.84,96.15,-7.01,59.23,96.47,-8.85,59.95,97.02,-11.69,60.67,97.58,-14.52,61.13,97.92,-16.19,61.38,98.09,-16.81,61.54,98.19,-17.08,62.29,98.64,-17.96,62.35,98.68,-18.03,62.38,98.69,-18.05,62.44,98.72,-18.06,64.17,99.66,-18.25,64.93,100.06,-18.26,65.24,100.22,-18.23,65.55,100.38,-18.10,65.81,100.51,-17.99,66.39,100.79,-17.46,67.80,101.45,-15.92,69.71,102.34,-13.82,71.56,103.20,-11.75,73.31,104.01,-9.73,74.52,104.56,-8.22,75.46,104.98,-6.81,76.31,105.34,-5.33,77.00,105.62,-3.81,77.54,105.82,-2.34,77.91,105.94,-0.96,78.14,105.98,0.34],
        [82.02,105.03,3.36,82.03,104.86,6.21,82.00,104.72,8.43,81.85,104.54,10.00,81.58,104.32,11.39,81.20,104.04,12.67,80.67,103.68,13.90,80.03,103.28,15.01,79.31,102.84,15.98,78.47,102.34,16.88,77.51,101.78,17.70,76.31,101.09,18.51,74.96,100.33,19.21,73.58,99.57,19.74,71.94,98.67,20.17,70.27,97.76,20.47,68.17,96.63,20.67,65.98,95.47,20.75,65.04,94.97,20.74,63.91,94.38,20.56,63.39,94.11,20.47,63.36,94.10,20.45,63.16,94.00,20.24,62.74,93.81,19.71,62.34,93.63,19.03,61.96,93.49,18.14,61.58,93.36,16.83,61.08,93.23,14.48,60.79,93.19,12.73,60.57,93.23,10.06,60.33,93.28,6.97,60.11,93.35,3.80,59.89,93.43,0.54,59.72,93.50,-2.12,59.74,93.60,-3.61,59.92,93.79,-5.27,60.26,94.08,-7.07,60.96,94.62,-9.85,61.74,95.21,-12.88,62.48,95.77,-15.76,62.78,95.99,-16.70,63.03,96.15,-17.21,63.39,96.37,-17.66,64.10,96.80,-18.47,64.14,96.82,-18.52,64.17,96.84,-18.53,64.60,97.07,-18.58,66.24,97.95,-18.74,66.86,98.28,-18.71,67.17,98.44,-18.61,67.47,98.59,-18.49,67.70,98.70,-18.30,68.74,99.19,-17.25,70.70,100.11,-15.13,72.76,101.07,-12.90,74.75,102.00,-10.70,76.65,102.88,-8.55,78.05,103.53,-6.86,79.07,103.98,-5.44,79.95,104.36,-3.96,80.68,104.66,-2.43,81.27,104.88,-0.86,81.67,105.01,0.56,81.91,105.06,1.94],
        [85.79,104.10,5.00,85.80,103.93,7.99,85.74,103.80,9.64,85.51,103.59,11.22,85.17,103.32,12.63,84.72,103.01,13.87,84.15,102.64,15.03,83.42,102.18,16.18,82.57,101.66,17.22,81.61,101.10,18.13,80.46,100.44,18.99,79.31,99.79,19.66,77.96,99.03,20.26,76.39,98.17,20.74,74.70,97.25,21.10,72.72,96.18,21.31,70.40,94.95,21.40,67.93,93.63,21.38,66.02,92.62,21.28,64.91,92.04,21.15,64.76,91.96,21.10,64.53,91.85,20.96,64.28,91.73,20.72,63.73,91.48,20.06,63.36,91.32,19.45,63.02,91.18,18.64,62.65,91.06,17.36,62.17,90.94,15.03,61.95,90.93,13.39,61.69,90.98,10.10,61.44,91.05,6.84,61.21,91.13,3.41,60.97,91.21,-0.03,60.90,91.27,-1.85,61.01,91.43,-3.42,61.29,91.68,-5.18,61.89,92.15,-7.67,62.72,92.78,-10.89,63.54,93.40,-14.12,64.14,93.86,-16.39,64.43,94.06,-17.15,64.61,94.18,-17.48,65.44,94.67,-18.45,65.88,94.94,-18.96,65.94,94.97,-19.00,65.95,94.98,-19.00,66.97,95.53,-19.11,68.13,96.15,-19.20,68.60,96.40,-19.17,69.13,96.66,-18.95,69.26,96.73,-18.85,69.64,96.91,-18.52,71.37,97.72,-16.68,73.61,98.77,-14.30,75.82,99.80,-11.94,77.97,100.81,-9.60,80.00,101.76,-7.34,81.64,102.52,-5.43,82.77,103.03,-3.91,83.65,103.41,-2.47,84.36,103.70,-1.03,85.01,103.94,0.61,85.43,104.08,2.10,85.68,104.13,3.54],
        [89.57,103.18,6.63,89.57,103.03,9.22,89.45,102.86,10.82,89.18,102.64,12.30,88.78,102.34,13.71,88.22,101.96,15.05,87.55,101.54,16.26,86.75,101.04,17.38,85.84,100.50,18.39,84.80,99.89,19.28,83.59,99.20,20.10,82.16,98.40,20.82,80.86,97.68,21.30,79.19,96.77,21.70,77.50,95.85,21.93,75.20,94.62,22.04,72.74,93.32,22.01,69.86,91.79,21.86,66.84,90.20,21.65,65.90,89.71,21.59,65.89,89.70,21.58,65.66,89.58,21.43,65.39,89.46,21.20,64.71,89.15,20.40,64.38,89.00,19.87,64.07,88.88,19.14,63.73,88.77,17.92,63.29,88.67,15.76,63.08,88.68,13.60,62.81,88.74,10.12,62.55,88.81,6.67,62.31,88.90,3.04,62.10,88.97,-0.04,62.12,89.07,-1.52,62.32,89.27,-3.18,62.73,89.60,-5.12,63.60,90.27,-8.53,64.47,90.93,-11.94,65.34,91.60,-15.34,65.79,91.93,-16.93,66.06,92.11,-17.52,66.26,92.23,-17.81,67.53,93.00,-19.29,67.68,93.08,-19.44,67.72,93.11,-19.48,67.73,93.12,-19.48,69.30,93.96,-19.62,69.95,94.30,-19.60,70.28,94.48,-19.55,70.59,94.63,-19.42,70.82,94.75,-19.30,71.81,95.22,-18.33,74.18,96.33,-15.89,76.52,97.43,-13.42,78.88,98.54,-10.93,81.19,99.62,-8.46,83.39,100.65,-6.06,85.28,101.52,-3.91,86.45,102.05,-2.41,87.36,102.45,-0.97,88.08,102.75,0.46,88.75,103.01,2.13,89.19,103.15,3.66,89.45,103.21,5.13],
        [93.34,102.25,8.27,93.32,102.12,10.36,93.15,101.94,11.87,92.82,101.67,13.40,92.36,101.34,14.80,91.74,100.93,16.15,90.97,100.45,17.41,90.09,99.91,18.54,89.08,99.32,19.55,87.94,98.66,20.44,86.74,97.97,21.16,85.29,97.17,21.81,83.85,96.38,22.25,82.25,95.51,22.54,80.31,94.47,22.67,78.01,93.25,22.64,74.88,91.59,22.45,71.54,89.83,22.21,68.70,88.33,22.01,67.67,87.79,21.93,66.70,87.28,21.84,66.60,87.23,21.76,66.49,87.18,21.67,65.68,86.81,20.72,65.40,86.68,20.27,65.13,86.58,19.65,64.82,86.48,18.52,64.45,86.40,16.61,64.22,86.44,13.85,63.93,86.50,10.18,63.67,86.58,6.53,63.41,86.67,2.72,63.28,86.74,0.40,63.37,86.87,-1.11,63.65,87.12,-2.84,64.36,87.68,-5.73,65.28,88.38,-9.32,66.20,89.08,-12.90,67.08,89.75,-16.33,67.40,89.98,-17.32,67.64,90.14,-17.81,68.17,90.45,-18.45,69.35,91.16,-19.82,69.36,91.17,-19.83,69.50,91.24,-19.84,70.09,91.56,-19.86,71.12,92.11,-19.89,71.68,92.40,-19.84,71.97,92.55,-19.75,72.31,92.72,-19.59,72.49,92.81,-19.45,74.59,93.80,-17.34,77.02,94.95,-14.89,79.56,96.15,-12.30,82.03,97.31,-9.76,84.44,98.44,-7.24,86.82,99.56,-4.70,88.86,100.51,-2.45,90.18,101.11,-0.81,91.08,101.51,0.56,91.81,101.81,1.98,92.51,102.08,3.68,92.95,102.23,5.22,93.23,102.28,6.73],
        [94.48,101.97,8.77,94.45,101.85,10.62,94.27,101.66,12.19,93.91,101.37,13.76,93.41,101.02,15.18,92.79,100.62,16.49,92.00,100.12,17.74,91.09,99.57,18.89,90.06,98.96,19.89,88.92,98.31,20.76,87.67,97.60,21.48,86.16,96.76,22.13,84.76,95.99,22.53,83.23,95.16,22.77,81.15,94.05,22.88,78.75,92.78,22.79,75.46,91.04,22.56,72.49,89.48,22.34,70.63,88.49,22.19,68.76,87.51,22.05,66.93,86.55,21.90,66.88,86.52,21.85,66.82,86.49,21.80,65.98,86.10,20.82,65.70,85.98,20.39,65.45,85.88,19.81,65.15,85.79,18.71,64.81,85.71,16.89,64.56,85.76,13.94,64.27,85.83,10.21,64.00,85.90,6.50,63.74,85.99,2.65,63.64,86.06,0.56,63.75,86.21,-0.97,64.05,86.48,-2.72,64.85,87.09,-5.89,65.78,87.80,-9.53,66.71,88.51,-13.17,67.57,89.16,-16.50,67.89,89.39,-17.44,68.12,89.54,-17.88,68.74,89.91,-18.63,69.78,90.54,-19.83,69.81,90.56,-19.87,69.95,90.63,-19.89,70.30,90.82,-19.90,71.43,91.42,-19.94,72.20,91.83,-19.87,72.48,91.97,-19.77,72.80,92.13,-19.61,73.02,92.24,-19.42,75.32,93.32,-17.12,77.82,94.50,-14.60,80.43,95.73,-11.95,83.01,96.95,-9.33,85.47,98.11,-6.79,87.87,99.23,-4.27,89.97,100.21,-1.97,91.31,100.83,-0.32,92.21,101.23,1.05,92.94,101.53,2.44,93.64,101.80,4.15,94.09,101.95,5.69,94.36,102.00,7.21],
        [96.29,101.53,9.54,96.24,101.41,11.10,96.02,101.19,12.77,95.64,100.90,14.26,95.12,100.54,15.69,94.47,100.12,16.99,93.63,99.59,18.30,92.71,99.04,19.40,91.66,98.42,20.39,90.42,97.71,21.30,89.09,96.96,22.02,87.72,96.20,22.57,86.35,95.45,22.93,84.58,94.50,23.14,82.80,93.55,23.17,79.86,92.00,22.98,76.38,90.16,22.70,72.88,88.31,22.42,70.29,86.95,22.21,68.91,86.23,22.10,67.54,85.50,21.98,67.30,85.37,21.96,67.29,85.37,21.96,66.44,84.98,20.97,66.19,84.87,20.59,65.96,84.78,20.06,65.68,84.69,19.02,65.37,84.63,17.35,65.11,84.68,14.09,64.81,84.75,10.28,64.54,84.83,6.48,64.27,84.92,2.54,64.22,84.99,0.82,64.36,85.16,-0.73,64.71,85.45,-2.53,65.63,86.16,-6.13,66.58,86.88,-9.85,67.53,87.61,-13.57,68.34,88.23,-16.73,68.66,88.45,-17.60,68.87,88.59,-18.01,69.63,89.04,-18.90,70.49,89.55,-19.88,70.57,89.60,-19.96,70.58,89.61,-19.96,71.05,89.86,-19.97,72.41,90.58,-19.98,72.90,90.84,-19.91,73.18,90.98,-19.82,73.57,91.18,-19.63,73.99,91.38,-19.23,76.47,92.55,-16.74,79.14,93.81,-14.05,81.81,95.07,-11.37,84.46,96.32,-8.71,87.09,97.56,-6.04,89.63,98.76,-3.45,91.72,99.74,-1.23,93.15,100.39,0.51,93.98,100.76,1.76,94.73,101.08,3.16,95.45,101.36,4.94,95.89,101.50,6.44,96.17,101.56,7.98],
        [96.91,101.18,9.51,96.87,101.06,11.13,96.65,100.85,12.73,96.28,100.56,14.26,95.75,100.19,15.75,95.06,99.74,17.15,94.23,99.23,18.43,93.26,98.64,19.62,92.20,98.01,20.64,91.01,97.33,21.52,89.75,96.62,22.23,88.40,95.87,22.77,87.03,95.12,23.12,85.42,94.25,23.31,83.82,93.40,23.31,80.57,91.69,23.06,77.05,89.83,22.78,74.72,88.61,22.59,72.51,87.44,22.40,70.30,86.28,22.21,68.09,85.11,22.03,67.47,84.79,21.97,67.46,84.78,21.96,66.68,84.42,21.06,66.44,84.32,20.70,66.23,84.23,20.22,65.97,84.15,19.28,65.67,84.08,17.66,65.41,84.14,14.41,65.10,84.21,10.57,64.83,84.29,6.75,64.56,84.38,2.79,64.51,84.45,1.07,64.65,84.62,-0.48,65.00,84.91,-2.28,65.94,85.63,-5.95,66.89,86.36,-9.70,67.85,87.09,-13.44,68.69,87.73,-16.72,69.02,87.96,-17.63,69.24,88.10,-18.05,69.94,88.52,-18.89,70.81,89.05,-19.88,70.89,89.10,-19.96,70.92,89.11,-19.97,71.25,89.29,-19.98,72.78,90.10,-20.00,73.34,90.39,-19.91,73.63,90.54,-19.78,73.93,90.69,-19.63,74.24,90.84,-19.34,76.86,92.08,-16.72,79.53,93.34,-14.04,82.20,94.60,-11.36,84.89,95.87,-8.66,87.56,97.13,-5.98,90.21,98.38,-3.29,92.49,99.45,-0.96,93.76,100.04,0.53,94.68,100.45,1.87,95.44,100.77,3.26,96.05,101.00,4.76,96.49,101.15,6.29,96.79,101.21,7.88],
        [97.33,100.73,9.48,97.29,100.61,11.09,97.08,100.40,12.69,96.70,100.11,14.26,96.16,99.73,15.77,95.46,99.27,17.19,94.62,98.75,18.50,93.66,98.17,19.68,92.59,97.54,20.71,91.39,96.85,21.60,90.14,96.14,22.32,88.76,95.37,22.88,87.39,94.62,23.23,85.95,93.85,23.41,84.47,93.06,23.40,81.47,91.48,23.15,78.88,90.11,22.93,76.29,88.75,22.72,73.70,87.39,22.50,71.11,86.02,22.28,68.52,84.66,22.06,67.65,84.20,21.98,67.63,84.19,21.97,66.92,83.86,21.14,66.69,83.76,20.80,66.49,83.68,20.36,66.24,83.60,19.47,65.95,83.54,17.92,65.69,83.60,14.60,65.39,83.66,10.74,65.11,83.75,6.91,64.84,83.84,2.93,64.79,83.91,1.28,64.94,84.08,-0.26,65.30,84.38,-2.06,66.26,85.11,-5.82,67.22,85.85,-9.59,68.18,86.58,-13.35,69.05,87.24,-16.72,69.37,87.47,-17.65,69.60,87.62,-18.09,70.26,88.01,-18.87,71.14,88.54,-19.88,71.22,88.59,-19.96,71.27,88.61,-19.97,71.43,88.70,-19.99,73.14,89.61,-20.00,73.54,89.82,-19.94,73.83,89.97,-19.86,74.25,90.18,-19.65,74.55,90.33,-19.39,77.17,91.56,-16.77,79.86,92.83,-14.08,82.55,94.10,-11.38,85.24,95.38,-8.68,87.95,96.65,-5.97,90.66,97.93,-3.26,93.18,99.12,-0.70,94.20,99.60,0.48,95.09,99.99,1.78,95.85,100.31,3.18,96.46,100.55,4.67,96.92,100.70,6.26,97.20,100.76,7.83],
        [98.22,99.11,9.38,98.18,98.99,10.98,97.97,98.79,12.56,97.57,98.48,14.22,97.04,98.11,15.69,96.33,97.65,17.14,95.48,97.11,18.46,94.52,96.54,19.62,93.46,95.91,20.64,92.29,95.24,21.52,91.04,94.53,22.23,89.61,93.73,22.81,88.30,93.02,23.16,86.81,92.21,23.34,84.63,91.06,23.33,81.82,89.57,23.14,79.27,88.23,22.93,76.76,86.91,22.72,74.25,85.59,22.51,71.75,84.27,22.30,69.24,82.95,22.09,68.18,82.39,21.99,68.16,82.38,21.98,67.62,82.13,21.36,67.42,82.04,21.07,67.26,81.97,20.71,67.05,81.91,19.93,66.83,81.87,18.66,66.54,81.94,14.87,66.23,82.00,11.00,65.96,82.09,7.16,65.69,82.18,3.29,65.68,82.25,1.88,65.85,82.43,0.35,66.33,82.81,-1.79,67.29,83.55,-5.56,68.25,84.29,-9.33,69.22,85.02,-13.11,70.14,85.73,-16.72,70.47,85.96,-17.72,70.72,86.12,-18.20,71.24,86.44,-18.85,72.15,86.98,-19.89,72.23,87.03,-19.96,72.28,87.06,-19.98,72.50,87.18,-20.00,74.25,88.11,-20.00,74.69,88.34,-19.93,74.98,88.49,-19.84,75.33,88.66,-19.66,75.51,88.75,-19.52,78.02,89.93,-17.02,80.73,91.21,-14.30,83.44,92.49,-11.58,86.14,93.77,-8.87,88.82,95.03,-6.17,91.41,96.25,-3.55,93.93,97.44,-0.95,95.11,97.99,0.39,95.95,98.36,1.62,96.73,98.69,3.07,97.33,98.93,4.54,97.82,99.08,6.20,98.09,99.14,7.68],
        [99.39,96.90,9.25,99.35,96.79,10.86,99.14,96.58,12.44,98.75,96.27,14.09,98.21,95.90,15.56,97.51,95.44,17.00,96.65,94.91,18.33,95.70,94.33,19.49,94.62,93.70,20.52,93.45,93.03,21.39,92.20,92.31,22.11,90.78,91.53,22.68,89.45,90.80,23.03,87.98,90.01,23.21,85.67,88.78,23.20,82.86,87.29,23.12,79.34,85.44,22.86,76.36,83.86,22.63,74.25,82.75,22.45,72.14,81.64,22.27,70.02,80.53,22.10,68.88,79.92,22.00,68.87,79.92,21.99,68.55,79.77,21.63,68.41,79.71,21.43,68.31,79.67,21.20,68.14,79.62,20.53,67.98,79.62,19.11,67.68,79.69,15.22,67.38,79.76,11.33,67.10,79.84,7.48,66.87,79.92,3.98,66.89,80.01,2.69,67.09,80.21,1.18,67.72,80.70,-1.43,68.68,81.44,-5.21,69.65,82.17,-9.00,70.62,82.91,-12.78,71.58,83.65,-16.56,71.97,83.93,-17.82,72.22,84.10,-18.35,72.60,84.32,-18.84,73.51,84.87,-19.89,73.59,84.92,-19.96,73.64,84.95,-19.98,73.97,85.12,-20.00,75.58,85.98,-20.00,75.99,86.19,-19.96,76.34,86.38,-19.89,76.85,86.63,-19.63,77.12,86.76,-19.37,79.21,87.75,-17.27,81.90,89.02,-14.56,84.58,90.28,-11.85,87.23,91.53,-9.16,89.85,92.77,-6.48,92.47,94.00,-3.79,95.09,95.23,-1.10,96.27,95.78,0.25,97.13,96.16,1.50,97.90,96.49,2.93,98.51,96.72,4.41,98.99,96.88,6.06,99.26,96.93,7.57],
        [100.56,94.70,9.12,100.52,94.58,10.73,100.31,94.38,12.32,99.92,94.07,13.95,99.39,93.70,15.43,98.69,93.24,16.86,97.83,92.71,18.19,96.87,92.13,19.36,95.79,91.50,20.39,94.62,90.82,21.27,93.35,90.10,21.98,91.95,89.32,22.55,90.61,88.59,22.90,89.15,87.80,23.08,86.79,86.55,23.06,83.34,84.72,23.00,80.46,83.20,22.84,76.90,81.32,22.57,73.38,79.46,22.31,71.64,78.55,22.17,70.40,77.89,22.07,69.58,77.46,22.00,69.57,77.46,22.00,69.46,77.41,21.87,69.40,77.38,21.79,69.36,77.36,21.70,69.29,77.34,21.38,69.13,77.37,19.47,68.83,77.44,15.57,68.52,77.51,11.66,68.25,77.60,7.80,68.05,77.67,4.74,68.10,77.77,3.52,68.33,77.98,2.04,69.11,78.58,-1.06,70.08,79.32,-4.86,71.05,80.06,-8.65,72.02,80.80,-12.44,72.99,81.54,-16.24,73.46,81.89,-17.89,73.72,82.07,-18.49,73.98,82.23,-18.86,74.88,82.76,-19.90,74.95,82.81,-19.96,75.00,82.83,-19.98,75.32,83.01,-20.00,76.78,83.78,-20.00,77.63,84.23,-19.92,77.77,84.30,-19.88,78.07,84.45,-19.72,78.42,84.62,-19.47,80.34,85.53,-17.53,83.02,86.79,-14.82,85.66,88.04,-12.12,88.31,89.28,-9.40,90.96,90.53,-6.68,93.60,91.78,-3.96,96.25,93.02,-1.24,97.43,93.57,0.11,98.31,93.96,1.38,99.07,94.28,2.80,99.68,94.52,4.28,100.15,94.67,5.91,100.43,94.73,7.46],
        [101.74,92.50,8.99,101.69,92.38,10.61,101.48,92.17,12.20,101.09,91.87,13.81,100.56,91.50,15.30,99.86,91.04,16.72,99.02,90.51,18.04,98.05,89.93,19.22,96.97,89.29,20.25,95.79,88.61,21.13,94.52,87.90,21.85,93.14,87.13,22.42,91.77,86.38,22.77,90.33,85.60,22.95,87.96,84.34,22.93,84.48,82.50,22.86,81.30,80.81,22.79,78.48,79.32,22.59,75.46,77.73,22.37,72.43,76.13,22.14,70.51,75.12,22.00,70.51,75.12,22.00,70.51,75.12,22.00,70.51,75.12,22.00,70.51,75.12,22.00,70.48,75.11,21.92,70.36,75.11,20.85,70.12,75.16,17.81,69.88,75.22,14.78,69.64,75.27,11.74,69.39,75.35,8.04,69.24,75.42,5.54,69.32,75.53,4.34,69.59,75.77,2.88,70.52,76.48,-0.78,71.49,77.22,-4.58,72.46,77.96,-8.37,73.43,78.70,-12.17,74.40,79.44,-15.97,74.95,79.85,-17.96,75.24,80.05,-18.64,75.49,80.20,-19.02,76.25,80.66,-19.90,76.32,80.70,-19.96,76.36,80.72,-19.98,76.60,80.85,-20.00,78.08,81.64,-20.00,78.74,81.99,-19.96,79.17,82.21,-19.86,79.52,82.39,-19.68,79.84,82.54,-19.41,81.49,83.32,-17.73,84.14,84.57,-15.02,86.80,85.82,-12.29,89.46,87.07,-9.56,92.11,88.32,-6.83,94.77,89.57,-4.10,97.43,90.82,-1.37,98.60,91.36,-0.03,99.48,91.76,1.26,100.25,92.08,2.67,100.86,92.31,4.16,101.32,92.47,5.77,101.61,92.53,7.34],
        [102.91,90.29,8.86,102.86,90.17,10.48,102.65,89.96,12.09,102.27,89.67,13.66,101.74,89.29,15.16,101.04,88.84,16.57,100.21,88.32,17.88,99.24,87.74,19.07,98.16,87.10,20.11,96.98,86.42,21.00,95.70,85.70,21.72,94.34,84.94,22.27,92.96,84.18,22.64,91.52,83.41,22.82,89.19,82.17,22.80,85.69,80.31,22.73,82.20,78.46,22.66,79.41,76.98,22.56,76.53,75.46,22.35,73.64,73.94,22.13,71.87,73.01,22.00,71.86,73.00,21.99,71.84,73.00,21.99,71.83,72.99,21.97,71.81,72.98,21.94,71.56,72.88,21.41,71.47,72.87,20.75,71.24,72.92,17.80,71.01,72.97,14.85,70.78,73.03,11.89,70.52,73.11,8.10,70.43,73.18,6.32,70.55,73.31,5.14,71.00,73.66,3.16,71.97,74.40,-0.63,72.93,75.14,-4.42,73.90,75.88,-8.21,74.87,76.62,-12.00,75.84,77.36,-15.79,76.46,77.83,-18.08,76.76,78.03,-18.81,77.00,78.18,-19.19,77.62,78.55,-19.92,77.68,78.59,-19.96,77.73,78.61,-19.99,77.89,78.70,-20.00,79.12,79.36,-20.00,80.23,79.94,-19.93,80.50,80.08,-19.86,80.92,80.29,-19.64,80.96,80.31,-19.61,82.70,81.13,-17.84,85.35,82.38,-15.11,88.00,83.63,-12.38,90.66,84.88,-9.66,93.31,86.13,-6.93,95.97,87.38,-4.20,98.62,88.63,-1.48,99.77,89.16,-0.16,100.66,89.56,1.14,101.43,89.88,2.55,102.03,90.11,4.05,102.49,90.26,5.63,102.78,90.32,7.22],
        [104.08,88.09,8.73,104.03,87.97,10.36,103.82,87.76,11.97,103.45,87.47,13.52,102.91,87.09,15.02,102.22,86.64,16.43,101.39,86.12,17.73,100.43,85.54,18.92,99.35,84.90,19.96,98.16,84.22,20.86,96.88,83.50,21.59,95.55,82.75,22.13,94.15,81.99,22.50,92.72,81.22,22.69,90.42,80.00,22.67,86.92,78.14,22.60,83.43,76.29,22.53,79.93,74.43,22.46,77.47,73.13,22.31,74.81,71.73,22.12,73.23,70.89,22.00,73.20,70.88,21.99,73.18,70.87,21.97,73.15,70.86,21.94,73.10,70.84,21.87,72.64,70.65,20.90,72.58,70.63,20.63,72.35,70.68,17.78,72.13,70.73,14.93,71.91,70.78,12.05,71.65,70.87,8.25,71.64,70.93,7.12,71.79,71.08,5.97,72.45,71.59,3.30,73.41,72.33,-0.48,74.38,73.07,-4.27,75.34,73.80,-8.05,76.31,74.54,-11.83,77.28,75.28,-15.61,77.96,75.80,-18.19,78.27,76.00,-18.95,78.51,76.16,-19.36,79.00,76.45,-19.93,79.04,76.47,-19.96,79.09,76.51,-19.99,79.18,76.55,-20.00,80.35,77.17,-20.00,81.26,77.66,-19.97,81.71,77.89,-19.90,81.95,78.01,-19.77,82.45,78.26,-19.40,84.00,78.99,-17.83,86.62,80.22,-15.15,89.26,81.46,-12.43,91.90,82.70,-9.72,94.54,83.95,-7.01,97.18,85.19,-4.29,99.82,86.43,-1.58,100.94,86.95,-0.29,101.84,87.36,1.02,102.60,87.67,2.43,103.21,87.91,3.93,103.66,88.06,5.48,103.96,88.12,7.10],
        [105.25,85.88,8.59,105.21,85.76,10.24,104.99,85.55,11.85,104.63,85.27,13.37,104.09,84.89,14.88,103.41,84.44,16.28,102.58,83.93,17.58,101.62,83.35,18.77,100.53,82.71,19.82,99.33,82.02,20.72,98.06,81.29,21.45,96.75,80.57,21.99,95.34,79.80,22.37,93.93,79.03,22.55,91.66,77.83,22.55,88.16,75.97,22.47,84.67,74.12,22.40,81.17,72.26,22.33,78.07,70.62,22.25,75.88,69.46,22.10,74.58,68.78,21.99,74.54,68.76,21.98,74.51,68.74,21.95,74.47,68.72,21.91,74.40,68.69,21.81,73.72,68.42,20.39,73.67,68.40,20.26,73.47,68.44,17.75,73.26,68.49,15.02,73.04,68.54,12.17,72.82,68.62,8.85,72.84,68.69,7.96,73.03,68.85,6.84,73.89,69.51,3.44,74.86,70.25,-0.33,75.82,70.99,-4.10,76.78,71.72,-7.88,77.75,72.46,-11.65,78.71,73.20,-15.42,79.45,73.76,-18.24,79.76,73.97,-19.06,80.02,74.14,-19.52,80.37,74.35,-19.94,80.40,74.36,-19.96,80.46,74.40,-20.00,80.49,74.42,-20.00,81.58,74.99,-20.00,82.52,75.49,-19.97,82.97,75.73,-19.88,83.29,75.89,-19.71,83.89,76.17,-19.20,85.30,76.84,-17.78,87.92,78.08,-15.13,90.52,79.30,-12.47,93.14,80.53,-9.77,95.77,81.77,-7.08,98.39,83.01,-4.38,101.01,84.24,-1.69,102.11,84.75,-0.43,103.03,85.16,0.91,103.78,85.47,2.31,104.39,85.71,3.82,104.83,85.85,5.34,105.13,85.91,6.98],
        [106.42,83.68,8.46,106.38,83.56,10.12,106.16,83.35,11.73,105.80,83.07,13.22,105.26,82.69,14.74,104.59,82.25,16.13,103.77,81.74,17.42,102.81,81.15,18.62,101.72,80.51,19.68,100.51,79.81,20.59,99.24,79.09,21.32,97.95,78.38,21.85,96.53,77.60,22.24,95.14,76.85,22.42,92.90,75.66,22.42,89.41,73.81,22.35,85.92,71.96,22.27,82.43,70.10,22.20,78.94,68.25,22.13,76.55,66.99,22.04,75.94,66.66,21.99,75.88,66.64,21.98,75.84,66.62,21.94,75.78,66.59,21.88,75.70,66.55,21.75,74.93,66.24,20.15,74.75,66.17,19.76,74.58,66.20,17.72,74.38,66.25,15.12,74.17,66.30,12.25,74.00,66.37,9.66,74.06,66.45,8.83,74.38,66.70,7.36,75.34,67.44,3.60,76.30,68.17,-0.16,77.26,68.91,-3.93,78.22,69.64,-7.69,79.18,70.38,-11.45,80.14,71.11,-15.21,80.93,71.71,-18.26,81.25,71.93,-19.16,81.52,72.11,-19.67,81.74,72.24,-19.95,81.75,72.25,-19.96,81.79,72.27,-19.98,81.91,72.33,-19.99,82.41,72.60,-19.99,83.83,73.35,-19.93,84.17,73.53,-19.86,84.60,73.74,-19.62,85.29,74.07,-18.97,86.69,74.73,-17.60,89.23,75.93,-15.06,91.84,77.16,-12.43,94.41,78.38,-9.81,97.01,79.60,-7.13,99.61,80.82,-4.46,102.21,82.05,-1.79,103.29,82.54,-0.56,104.21,82.96,0.80,104.95,83.27,2.18,105.57,83.51,3.70,106.00,83.65,5.20,106.30,83.71,6.86],
        [107.60,81.47,8.33,107.55,81.35,10.00,107.33,81.14,11.60,106.98,80.87,13.07,106.44,80.48,14.61,105.77,80.05,15.97,104.97,79.55,17.25,104.00,78.96,18.47,102.92,78.32,19.52,101.69,77.61,20.46,100.44,76.91,21.17,99.15,76.19,21.71,97.72,75.40,22.10,96.38,74.68,22.28,94.19,73.52,22.29,90.69,71.66,22.22,87.20,69.81,22.15,83.71,67.96,22.08,80.21,66.10,22.01,78.46,65.17,21.97,77.62,64.73,21.94,77.17,64.49,21.92,77.14,64.48,21.90,77.10,64.46,21.86,77.00,64.41,21.70,76.20,64.08,20.02,75.83,63.94,19.26,75.70,63.96,17.74,75.51,64.00,15.31,75.31,64.06,12.46,75.20,64.12,10.54,75.27,64.20,9.77,75.79,64.60,7.64,76.75,65.33,3.89,77.70,66.06,0.15,78.66,66.80,-3.59,79.61,67.53,-7.33,80.57,68.26,-11.08,81.53,68.99,-14.82,82.44,69.68,-18.37,82.75,69.90,-19.27,82.92,70.01,-19.63,82.95,70.03,-19.68,82.98,70.05,-19.74,83.27,70.21,-19.75,83.71,70.44,-19.73,84.14,70.67,-19.71,84.89,71.06,-19.65,85.28,71.27,-19.56,85.31,71.28,-19.55,86.27,71.74,-18.83,88.31,72.72,-16.95,90.78,73.89,-14.63,93.25,75.07,-12.23,95.80,76.27,-9.69,98.32,77.46,-7.12,100.88,78.66,-4.50,103.43,79.87,-1.87,104.47,80.35,-0.67,105.40,80.76,0.69,106.13,81.07,2.06,106.75,81.30,3.59,107.17,81.44,5.07,107.48,81.50,6.74],
        [108.77,79.27,8.20,108.72,79.14,9.89,108.51,78.94,11.46,108.16,78.67,12.92,107.61,78.28,14.48,106.98,77.87,15.79,106.17,77.36,17.09,105.19,76.77,18.31,104.16,76.16,19.33,102.93,75.45,20.28,101.70,74.75,21.00,100.34,73.99,21.58,98.95,73.24,21.96,97.65,72.53,22.15,95.62,71.45,22.16,92.20,69.63,22.09,88.80,67.83,22.02,85.47,66.07,21.91,82.60,64.55,21.81,81.42,63.92,21.75,80.23,63.30,21.70,79.05,62.67,21.65,78.28,62.26,21.61,78.26,62.26,21.58,78.25,62.25,21.55,77.52,61.96,20.03,76.92,61.71,18.75,76.83,61.72,17.82,76.66,61.76,15.61,76.46,61.81,12.86,76.39,61.86,11.49,76.48,61.94,10.80,77.15,62.46,8.12,78.10,63.19,4.42,79.04,63.91,0.72,79.99,64.63,-2.98,80.93,65.35,-6.68,81.88,66.07,-10.38,82.82,66.80,-14.08,83.77,67.52,-17.78,84.13,67.79,-19.05,84.13,67.79,-19.06,84.13,67.79,-19.07,84.14,67.79,-19.08,84.22,67.84,-19.07,84.31,67.88,-19.06,84.40,67.93,-19.06,85.69,68.61,-18.94,86.24,68.90,-18.81,86.65,69.10,-18.59,87.54,69.53,-17.87,89.98,70.70,-15.77,92.53,71.93,-13.55,95.09,73.15,-11.28,97.56,74.33,-9.02,99.91,75.45,-6.78,102.26,76.56,-4.41,104.69,77.70,-1.92,105.68,78.16,-0.76,106.60,78.57,0.62,107.30,78.86,1.93,107.93,79.10,3.48,108.35,79.24,4.95,108.65,79.30,6.63],
        [109.71,77.45,8.09,109.66,77.33,9.71,109.47,77.13,11.25,109.10,76.84,12.80,108.58,76.48,14.29,107.91,76.04,15.69,107.11,75.54,16.97,106.17,74.97,18.15,105.15,74.37,19.17,103.98,73.69,20.08,102.74,72.98,20.82,101.42,72.25,21.40,100.07,71.51,21.79,98.69,70.76,22.00,97.28,70.01,22.04,93.94,68.24,21.89,90.56,66.46,21.73,87.94,65.07,21.60,86.12,64.11,21.50,84.30,63.15,21.41,82.48,62.19,21.32,80.66,61.23,21.23,79.16,60.43,21.14,79.14,60.42,21.11,79.12,60.41,21.07,78.70,60.24,20.22,77.80,59.88,18.33,77.75,59.87,17.91,77.60,59.91,15.93,77.41,59.96,13.37,77.37,60.00,12.32,77.45,60.08,11.72,78.21,60.66,8.73,79.15,61.38,5.08,80.08,62.09,1.43,81.01,62.80,-2.21,81.94,63.51,-5.86,82.87,64.22,-9.50,83.80,64.93,-13.15,84.73,65.64,-16.79,84.92,65.79,-17.51,84.97,65.82,-17.70,85.01,65.86,-17.88,85.09,65.91,-18.03,85.31,66.02,-18.00,85.53,66.14,-17.98,85.75,66.25,-17.95,86.36,66.57,-17.87,87.30,67.06,-17.62,87.61,67.21,-17.39,89.78,68.26,-15.59,91.95,69.30,-13.79,94.11,70.35,-11.99,96.44,71.47,-10.05,99.11,72.75,-7.81,101.78,74.04,-5.52,104.00,75.10,-3.58,105.68,75.90,-1.96,106.68,76.36,-0.77,107.55,76.75,0.54,108.28,77.06,1.92,108.87,77.28,3.38,109.31,77.42,4.94,109.59,77.48,6.51],
        [109.85,76.84,8.06,109.81,76.73,9.64,109.60,76.53,11.21,109.25,76.25,12.72,108.74,75.89,14.17,108.07,75.45,15.57,107.28,74.96,16.84,106.35,74.40,18.00,105.33,73.79,19.02,104.18,73.13,19.92,102.95,72.43,20.65,101.65,71.71,21.22,100.31,70.97,21.61,98.94,70.23,21.82,97.31,69.36,21.84,94.23,67.73,21.72,90.88,65.95,21.56,87.52,64.18,21.39,85.08,62.89,21.27,83.62,62.12,21.19,82.16,61.34,21.12,80.70,60.57,21.04,79.40,59.89,20.97,79.35,59.87,20.88,79.30,59.85,20.79,78.99,59.72,20.17,78.06,59.34,18.21,78.02,59.34,17.90,77.87,59.37,16.00,77.69,59.42,13.49,77.66,59.46,12.56,77.74,59.54,11.99,78.53,60.14,8.90,79.45,60.84,5.29,80.37,61.55,1.68,81.29,62.25,-1.93,82.21,62.96,-5.53,83.13,63.66,-9.14,84.06,64.36,-12.75,84.97,65.06,-16.28,85.30,65.31,-17.54,85.30,65.31,-17.57,85.31,65.32,-17.60,85.32,65.33,-17.63,85.37,65.35,-17.62,85.41,65.37,-17.61,85.45,65.39,-17.61,86.25,65.81,-17.49,87.33,66.37,-17.25,87.65,66.53,-17.10,90.02,67.68,-15.14,92.40,68.83,-13.19,94.78,69.97,-11.23,97.15,71.12,-9.27,99.53,72.27,-7.31,102.15,73.53,-5.14,104.60,74.71,-3.06,105.87,75.32,-1.87,106.85,75.77,-0.71,107.71,76.15,0.58,108.44,76.46,1.95,109.02,76.68,3.40,109.45,76.82,4.93,109.73,76.87,6.49],
        [109.79,76.13,8.03,109.74,76.01,9.66,109.56,75.83,11.03,109.19,75.54,12.60,108.67,75.18,14.05,108.08,74.79,15.30,107.30,74.30,16.55,106.37,73.74,17.72,105.29,73.10,18.78,104.20,72.47,19.61,103.04,71.82,20.29,101.79,71.12,20.83,100.39,70.35,21.22,99.13,69.67,21.41,96.86,68.46,21.52,94.13,67.01,21.44,90.96,65.33,21.32,87.68,63.60,21.18,85.84,62.62,21.09,84.17,61.74,21.00,82.49,60.85,20.92,80.82,59.97,20.84,79.65,59.35,20.77,79.62,59.34,20.73,79.59,59.33,20.69,79.25,59.18,20.03,78.32,58.80,18.08,78.28,58.80,17.83,78.14,58.83,16.01,77.97,58.88,13.56,77.94,58.92,12.79,78.03,59.00,12.25,78.85,59.63,9.01,79.76,60.32,5.47,80.67,61.01,1.92,81.57,61.70,-1.63,82.48,62.40,-5.18,83.39,63.09,-8.73,84.29,63.78,-12.28,85.05,64.35,-15.17,85.18,64.46,-15.67,85.32,64.56,-16.17,85.46,64.66,-16.66,85.59,64.76,-17.13,85.70,64.82,-17.12,85.80,64.87,-17.10,85.91,64.93,-17.09,86.42,65.19,-17.00,87.43,65.72,-16.78,87.83,65.92,-16.58,90.02,66.98,-14.81,92.49,68.17,-12.78,94.96,69.37,-10.75,97.44,70.56,-8.72,99.91,71.76,-6.69,102.39,72.95,-4.67,104.82,74.13,-2.67,105.87,74.63,-1.68,106.83,75.07,-0.55,107.65,75.44,0.66,108.41,75.75,2.08,108.97,75.97,3.47,109.39,76.10,4.91,109.66,76.16,6.40],
        [108.10,73.88,7.95,108.06,73.78,9.37,107.88,73.60,10.74,107.54,73.33,12.16,107.07,73.00,13.48,106.48,72.62,14.70,105.75,72.16,15.85,104.91,71.65,16.88,103.99,71.11,17.78,102.95,70.51,18.57,101.82,69.87,19.21,100.57,69.18,19.73,99.02,68.32,20.19,97.29,67.38,20.50,95.22,66.27,20.68,92.88,65.03,20.76,90.16,63.58,20.72,87.21,62.02,20.62,84.14,60.39,20.51,82.79,59.68,20.46,81.65,59.08,20.40,80.51,58.47,20.35,80.11,58.26,20.31,80.07,58.25,20.26,79.95,58.19,20.09,79.59,58.04,19.41,78.83,57.73,17.83,78.79,57.73,17.47,78.67,57.76,15.87,78.51,57.81,13.60,78.52,57.84,13.23,78.70,57.98,12.42,79.54,58.63,9.11,80.39,59.27,5.79,81.23,59.92,2.48,82.08,60.57,-0.83,82.93,61.21,-4.14,83.77,61.86,-7.46,84.59,62.48,-10.59,85.41,63.09,-13.49,85.68,63.29,-14.42,85.81,63.38,-14.83,85.94,63.47,-15.25,86.07,63.57,-15.66,86.14,63.61,-15.80,86.18,63.63,-15.79,86.21,63.65,-15.78,86.25,63.67,-15.77,87.27,64.20,-15.50,88.01,64.57,-15.22,88.76,64.94,-14.62,90.73,65.88,-12.93,93.22,67.08,-10.79,95.72,68.28,-8.64,98.23,69.49,-6.49,100.73,70.69,-4.34,103.03,71.79,-2.35,104.46,72.47,-1.00,105.37,72.90,0.07,106.13,73.23,1.18,106.80,73.52,2.44,107.33,73.72,3.75,107.73,73.85,5.12,107.99,73.91,6.56],
        [107.17,72.77,7.90,107.13,72.67,9.28,106.96,72.49,10.60,106.63,72.24,11.96,106.18,71.92,13.24,105.61,71.55,14.41,104.92,71.12,15.51,104.11,70.63,16.51,103.23,70.11,17.37,102.23,69.53,18.12,101.15,68.92,18.74,99.90,68.23,19.26,98.37,67.38,19.73,96.63,66.44,20.05,94.55,65.32,20.28,92.32,64.13,20.40,89.74,62.76,20.40,87.01,61.31,20.35,84.82,60.15,20.28,83.40,59.40,20.23,81.97,58.64,20.17,80.55,57.89,20.12,80.35,57.78,20.12,80.35,57.78,20.11,80.10,57.67,19.78,79.78,57.53,19.20,79.07,57.24,17.71,79.03,57.24,17.37,78.91,57.27,15.88,78.77,57.32,13.80,78.78,57.34,13.45,78.99,57.51,12.52,79.81,58.13,9.34,80.62,58.75,6.16,81.43,59.37,2.98,82.25,60.00,-0.21,83.06,60.62,-3.39,83.87,61.23,-6.55,84.64,61.82,-9.44,85.42,62.39,-12.11,86.24,62.97,-14.57,86.35,63.05,-14.87,86.35,63.05,-14.87,86.35,63.05,-14.87,86.35,63.05,-14.87,86.35,63.05,-14.87,86.35,63.05,-14.87,86.37,63.06,-14.87,87.05,63.40,-14.64,88.06,63.92,-14.29,88.16,63.97,-14.21,90.48,65.09,-12.22,92.89,66.24,-10.15,95.29,67.40,-8.08,97.70,68.56,-6.01,100.09,69.71,-3.96,102.19,70.72,-2.11,103.66,71.41,-0.71,104.53,71.81,0.31,105.26,72.14,1.38,105.92,72.42,2.60,106.42,72.61,3.85,106.81,72.74,5.18,107.06,72.79,6.58],
        [102.89,67.67,7.68,102.87,67.59,8.88,102.73,67.45,9.91,102.48,67.25,11.06,102.10,66.98,12.13,101.67,66.70,13.06,101.11,66.35,13.96,100.46,65.96,14.77,99.80,65.57,15.42,98.98,65.09,16.05,98.09,64.59,16.58,96.89,63.92,17.10,95.58,63.20,17.52,93.98,62.32,17.90,92.28,61.40,18.20,90.26,60.32,18.46,88.22,59.22,18.65,86.22,58.15,18.77,84.21,57.08,18.82,82.58,56.21,18.84,81.82,55.80,18.83,81.15,55.45,18.82,81.14,55.44,18.81,81.08,55.41,18.75,80.76,55.27,18.41,80.61,55.20,18.17,80.14,55.01,17.18,80.13,55.01,17.08,80.05,55.02,16.12,79.96,55.06,14.75,79.97,55.08,14.54,80.26,55.30,13.38,80.91,55.79,10.84,81.56,56.29,8.30,82.21,56.79,5.75,82.86,57.28,3.21,83.46,57.74,0.97,84.04,58.16,-0.96,84.78,58.68,-3.05,85.60,59.25,-5.33,86.43,59.82,-7.61,87.12,60.30,-9.53,87.12,60.31,-9.54,87.13,60.31,-9.56,87.14,60.32,-9.57,87.15,60.32,-9.57,87.16,60.33,-9.57,87.22,60.36,-9.57,87.94,60.74,-9.44,89.03,61.29,-9.10,89.19,61.38,-9.04,89.75,61.65,-8.59,91.58,62.53,-7.01,93.46,63.43,-5.38,95.30,64.31,-3.78,97.09,65.17,-2.17,98.77,65.97,-0.62,100.04,66.57,0.61,100.75,66.90,1.47,101.32,67.15,2.32,101.88,67.39,3.38,102.26,67.53,4.36,102.59,67.64,5.47,102.81,67.69,6.68],
        [98.61,62.57,7.46,98.60,62.51,8.49,98.51,62.42,9.20,98.31,62.25,10.17,98.05,62.07,10.92,97.71,61.84,11.72,97.29,61.58,12.40,96.84,61.30,12.99,96.36,61.02,13.49,95.72,60.65,13.98,95.01,60.24,14.42,94.07,59.72,14.85,93.12,59.19,15.21,91.76,58.45,15.60,90.39,57.70,15.90,88.97,56.93,16.17,87.40,56.08,16.42,85.81,55.22,16.64,84.15,54.33,16.83,82.50,53.44,16.97,81.80,53.07,17.03,81.56,52.94,17.05,81.47,52.89,17.06,81.47,52.89,17.06,81.40,52.86,16.97,81.32,52.82,16.87,81.21,52.78,16.66,81.21,52.77,16.65,81.19,52.78,16.41,81.15,52.80,15.74,81.16,52.81,15.67,81.54,53.09,14.18,82.01,53.45,12.35,82.34,53.70,11.22,82.95,54.12,9.51,83.60,54.58,7.70,84.25,55.03,5.90,84.90,55.48,4.10,85.55,55.94,2.29,86.20,56.39,0.49,86.85,56.84,-1.32,87.50,57.29,-3.12,87.89,57.56,-4.19,87.91,57.57,-4.22,87.92,57.58,-4.25,87.93,57.59,-4.28,87.95,57.60,-4.27,87.97,57.61,-4.27,88.18,57.72,-4.26,89.20,58.26,-4.09,90.00,58.67,-3.88,90.12,58.73,-3.84,90.60,58.96,-3.48,91.55,59.42,-2.63,92.91,60.07,-1.40,94.24,60.70,-0.17,95.53,61.31,1.06,96.40,61.72,1.92,97.01,62.00,2.70,97.36,62.16,3.24,97.84,62.36,4.15,98.13,62.47,4.93,98.38,62.55,5.77,98.56,62.58,6.79],
        [94.34,57.48,7.24,94.33,57.42,8.11,94.31,57.39,8.41,94.15,57.26,9.25,94.07,57.20,9.55,93.77,56.99,10.31,93.64,56.91,10.59,93.34,56.72,11.06,92.98,56.50,11.48,92.81,56.40,11.65,92.33,56.13,12.00,91.79,55.82,12.34,91.27,55.53,12.58,90.45,55.08,12.92,89.89,54.76,13.13,88.94,54.24,13.40,88.17,53.82,13.61,87.24,53.31,13.81,86.56,52.95,13.89,85.66,52.46,13.98,85.46,52.36,14.00,85.26,52.25,14.02,85.16,52.19,14.02,85.14,52.18,14.00,85.11,52.17,13.98,85.08,52.16,13.92,84.91,52.08,13.62,84.83,52.05,13.44,84.80,52.05,13.27,84.78,52.06,12.90,84.79,52.09,12.48,84.86,52.15,12.04,85.04,52.29,11.36,85.30,52.48,10.52,85.64,52.72,9.53,86.02,52.98,8.49,86.39,53.24,7.45,86.77,53.50,6.41,87.14,53.76,5.36,87.52,54.02,4.32,87.89,54.29,3.28,88.27,54.55,2.24,88.64,54.81,1.20,88.67,54.83,1.12,88.70,54.84,1.08,88.72,54.86,1.03,88.74,54.87,1.02,88.77,54.88,1.02,88.79,54.90,1.02,89.30,55.17,1.08,90.21,55.64,1.21,90.56,55.82,1.28,90.64,55.86,1.30,91.02,56.05,1.57,91.37,56.22,1.86,92.02,56.53,2.50,92.63,56.81,3.09,92.95,56.96,3.50,93.29,57.11,3.96,93.62,57.25,4.57,93.81,57.33,4.96,94.08,57.43,5.74,94.17,57.46,6.08,94.31,57.48,6.93],
        [91.95,54.64,7.12,91.95,54.61,7.62,91.95,54.58,8.04,91.92,54.55,8.21,91.82,54.47,8.73,91.77,54.43,8.99,91.66,54.35,9.28,91.46,54.22,9.78,91.43,54.20,9.86,91.31,54.12,10.03,91.16,54.03,10.20,90.91,53.89,10.42,90.74,53.79,10.57,90.58,53.70,10.70,90.57,53.69,10.71,90.32,53.55,10.75,89.99,53.37,10.80,89.66,53.20,10.84,89.33,53.02,10.89,89.01,52.84,10.93,88.76,52.71,10.97,88.75,52.70,10.95,88.63,52.65,10.86,88.38,52.53,10.68,88.27,52.48,10.55,88.11,52.40,10.33,88.02,52.36,10.19,87.96,52.34,10.03,87.90,52.32,9.78,87.84,52.31,9.58,87.84,52.32,9.33,87.83,52.34,8.85,87.85,52.37,8.60,87.91,52.42,8.19,87.97,52.48,7.78,88.10,52.58,7.30,88.22,52.67,6.81,88.33,52.75,6.40,88.50,52.87,5.88,88.66,52.99,5.37,88.83,53.11,4.86,88.98,53.22,4.42,89.09,53.29,4.14,89.10,53.30,4.12,89.11,53.30,4.09,89.11,53.30,4.09,89.11,53.30,4.09,89.11,53.30,4.09,89.42,53.47,4.14,89.80,53.67,4.19,90.19,53.87,4.25,90.60,54.08,4.32,90.73,54.15,4.34,90.78,54.18,4.35,90.81,54.19,4.37,90.93,54.25,4.50,91.15,54.35,4.75,91.36,54.45,5.00,91.48,54.50,5.23,91.51,54.51,5.28,91.69,54.57,5.81,91.80,54.61,6.15,91.85,54.62,6.42,91.94,54.64,6.97]]
};
const POD_AXIS = {
    "12" : [[98.2136, 69.2996, 8.0000], [0.207627, -0.976807, -0.052336]],
    "20" : [[98.2136, 69.2996, 8.0000], [0.341551, -0.938405, -0.052336]],
    "28" : [[98.2136, 69.2996, 8.0000], [0.468828, -0.881738, -0.052336]]
};
// pod -> pad seats (generated, headrest_POD11.py): rim flare start over each seat floor (= lowest flank height there - 0.3)
const POD_SEAT_RIM = { "12" : [3.24, 1.37], "20" : [2.83, 1.28], "28" : [2.27, 1.14] };
// seat floor, local y along the screw axis (>= 4 mm plastic under the seat; M3x20 tip at the nut)
const POD_SEAT_Y = [106.69, 106.20];
