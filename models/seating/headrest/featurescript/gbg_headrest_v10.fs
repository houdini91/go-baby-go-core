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
const SA = 24;  const SANG = 35;  const SR0 = 82;  const T_NUT = 24.408255;    // 2x M3x30 from under the chin into captive nuts
// ---------------- button pocket (PAD frame, place() local at +-BTN_ANG) ----------------
const BTN_ANG = 46;  const B_D = 8;
const POCKET_Y0 = 93.6;                                                        // pocket floor (8 mm deep)
const TRI = [[-15, 16], [15, 16], [0, -14]];                                    // (x, z) point down
// ---------------- print fits (P1S / PETG first guesses: confirm with the fit-test bar, then edit here) ----------------
const ROD_HOLE_D = 8.3;     // Ø8 pultruded carbon rod: slide fit (ream with an 8 mm drill if tight)
const SOCK_CL = 0.2;        // plug in the chair socket, per side
const CH_CL = 0.25;         // K2 tenon in the pad channel / column in the pad socket, per side
const TRI_CL = 0.15;        // button pocket (+0.15) vs pod wedge (-0.05): 0.2 per side
const M3_CLEAR = 3.4;  const M5_CLEAR = 5.4;
const NUT_M3_AF = 5.8;  const NUT_M5_AF = 8.3;                                 // printed nut traps across flats (nuts 5.5 / 8.0)
const PVC_BORE = 20.3;      // 20 mm PVC (kit head-switch rod)
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
const NUT_TRAP_D = 3.0;          // hex trap depth from the head-side face (M3 nut 2.4)
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
export function buildPad(context is Context, id is Id) returns Query
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
        // forehead-strap slot 22.5 x 4.5 (therapist-prescribed strap only)
        ycyl(context, sid + "st" + "a", 0, R - 15, R + 35, STRAP_Z0 + 2.25, 2.25);
        ycyl(context, sid + "st" + "b", 0, R - 15, R + 35, STRAP_Z1 - 2.25, 2.25);
        fCuboid(context, sid + "st" + "c", { "corner1" : v3(-2.25, R - 15, STRAP_Z0 + 2.25), "corner2" : v3(2.25, R + 35, STRAP_Z1 - 2.25) });
        moveSolids(context, sid + "stm", sid + "st", placeXf(s * STRAP_ANG));
        // triangle button pocket 8 mm deep + 2 click dimples + M3 countersunk from the head side
        triPrism(context, sid + "b" + "p", TRI_CL, POCKET_Y0, POCKET_Y0 + B_D + 1);
        opSphere(context, sid + "b" + "d1", { "center" : v3(9.3, POCKET_Y0 + B_D / 2, 4), "radius" : 1.3 * millimeter });
        opSphere(context, sid + "b" + "d2", { "center" : v3(-9.3, POCKET_Y0 + B_D / 2, 4), "radius" : 1.3 * millimeter });
        moveSolids(context, sid + "bpm", sid + "b", placeXf(s * BTN_ANG));
        // pod (2) + blank (1) screws: clearance through the shell (teardrop, pad prints upright) + hex nut traps on the head side
        const scs = concatenateArrays([POD_SCREWS, [BLANK_SCREW]]);
        for (var k = 0; k < size(scs); k += 1)
        {
            const xf = screwXf(s, scs[k]);  const z = scs[k][1];
            tdHole(context, sid + ("sh" ~ k), xp(xf, vector(0, R - 2, z)), xp(xf, vector(0, R + 30, z)), M3_CLEAR / 2, PAD_UP);
            hexPrism(context, sid + ("st" ~ k), xp(xf, vector(0, R - 1 + (NUT_TRAP_D + 1) / 2, z)) * millimeter, xd(xf, vector(0, 1, 0)), vector(0, 0, 1), NUT_M3_AF, NUT_TRAP_D + 1, 0);
        }
        // K2 lock screw: clearance + countersink at the chin
        const a = scrPt(s, -8);  const b = scrPt(s, T_NUT + 6);
        tdHole(context, sid + "sc", a, b, M3_CLEAR / 2, PAD_UP);
        fCone(context, sid + "sk", { "bottomCenter" : scrPt(s, -0.05) * millimeter, "topCenter" : scrPt(s, 1.75) * millimeter, "bottomRadius" : 3.15 * millimeter, "topRadius" : 1.5 * millimeter });
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
        hexPrism(context, C + ("nut" ~ i), v3(0, NUT5_Y, BOLTS_Z[i]), vector(0, 1, 0), vector(-1, 0, 0), NUT_M5_AF, 4.3, 40);   // slides in from -x
    }
    for (var s in [1, -1])
    {
        const sid = C + (s > 0 ? "scR" : "scL");
        tdHole(context, sid + "c", scrPt(s, 0), scrPt(s, T_NUT + 8), M3_CLEAR / 2, vector(-1, 0, 0));
        const n = normalize(scrPt(s, 1) - scrPt(s, 0));
        var xd = scrSlideDir(s);  xd = normalize(xd - n * dot(xd, n));
        hexPrism(context, sid + "n", scrPt(s, T_NUT) * millimeter, n, xd, NUT_M3_AF, 2.7, 30);                         // nut slides in from the tenon side edge
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
// W1 POD v1.1: the reviewed W1 form = convex hull of the PVC collar (Ø27 x 30) and a footprint band on the pad; the footprint
// now also carries the 2 screw seats.  Built as ONE loft through POD_SECTIONS (planar sections of the exact hull,
// arc-length resampled from a fixed start point: smooth, no ripples), underside trimmed to the pad + 0.3, wedge into the
// triangle pocket (carries the load), 2 M3 button heads from OUTSIDE on flat spot-faces into nuts trapped in the pad.
// HARDWARE (v1.1, 3 kinds, DIN 7991 countersunk like the pommel): M3x30 x4 (chin locks, PVC clamp), M3x16 x2-3 (pod / blank),
// M5x30 x2 (receiver -> plug); every screw ends in a hex nut. Countersink: M3 head Ø6.3 (seat Ø6.5), M5 head Ø10.4 (seat Ø10.6).
const CSK_M3_R = 3.25;  const CSK_M5_R = 5.3;
const SEAT_H = 4.5;              // flat screw seat height above the pad's back surface
const CLAMP_K = [17.5, 23.5];    // PVC clamp bolts, mm back from the collar mouth
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
    triPrism(context, id + "wg", -0.05, POCKET_Y0 + 0.2, R + T + 7.6 + 3.3);
    moveSolids(context, id + "wgm", id + "wg", placeXf(s * BTN_ANG));
    opBoolean(context, id + "uw", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const C = id + "cut";
    const pod = qSubtraction(solids(context, id), solids(context, C));
    // PVC bore (blind: 4 mm wall at the back), PVC clamp bolts, 2 pod screws + flat seats. The pod prints bore-up (up = d).
    tdHole(context, C + "bore", E - d * 26, E + d * 30, PVC_BORE / 2, podUp(s));        // horizontal in print: teardrop (the clamp bolts hold the PVC)
    // PVC clamp: 2 M3x30 through the collar AND the PVC (drill the PVC through the pod), heads on a flat land on top,
    // nuts in shallow hex traps (corner toward print-up = d) on a flat land underneath. Same M3x30 as the chin lock screws.
    var ne = normalize(vector(sin((s * 60) * degree), cos((s * 60) * degree) / ELL, 0));
    ne = normalize(ne - d * dot(ne, d));
    var bv = vector(0, 0, 1);
    bv = normalize(bv - d * dot(bv, d) - ne * dot(bv, ne));
    for (var i = 0; i < size(CLAMP_K); i += 1)
    {
        const A = E - d * CLAMP_K[i];
        tdHole(context, C + ("cl" ~ i), A - bv * 30, A + bv * 30, M3_CLEAR / 2, podUp(s));
        fCylinder(context, C + ("ct" ~ i), { "bottomCenter" : (A + bv * 12.5) * millimeter, "topCenter" : (A + bv * 32) * millimeter, "radius" : 3.6 * millimeter });
        fCone(context, C + ("ck" ~ i), { "bottomCenter" : (A + bv * (12.5 - (CSK_M3_R - 1.5))) * millimeter, "topCenter" : (A + bv * 12.55) * millimeter, "bottomRadius" : 1.5 * millimeter, "topRadius" : CSK_M3_R * millimeter });
        fCylinder(context, C + ("cb" ~ i), { "bottomCenter" : (A - bv * 40) * millimeter, "topCenter" : (A - bv * 17.7) * millimeter, "radius" : 4.2 * millimeter });
        hexPrism(context, C + ("cn" ~ i), (A - bv * 16.7) * millimeter, bv, normalize(podUp(s) - bv * dot(podUp(s), bv)), NUT_M3_AF, 4, 0);   // corner toward print-up
    }
    for (var k = 0; k < size(POD_SCREWS); k += 1)
    {
        const xf = screwXf(s, POD_SCREWS[k]);  const z = POD_SCREWS[k][1];
        const yFloor = rOut(z, abs(BTN_ANG + POD_SCREWS[k][0] / R * 180 / PI)) + SEAT_H;
        tdHole(context, C + ("sh" ~ k), xp(xf, vector(0, R - 2, z)), xp(xf, vector(0, yFloor + 1, z)), M3_CLEAR / 2, podUp(s));
        fCylinder(context, C + ("sf" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + 30, z)) * millimeter, "radius" : 3.6 * millimeter });
        fCone(context, C + ("sk" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor - (CSK_M3_R - 1.5), z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + 0.05, z)) * millimeter, "bottomRadius" : 1.5 * millimeter, "topRadius" : CSK_M3_R * millimeter });
    }
    opBoolean(context, id + "sub", { "tools" : solids(context, C), "targets" : pod, "operationType" : BooleanOperationType.SUBTRACTION });
    return solids(context, id);
}
// pocket BLANK: flush with the pad's back, 1 countersunk M3 from outside into the nut trapped on the head side
export function buildBlank(context is Context, id is Id, s is number) returns Query
{
    triPrism(context, id + "tri", -0.1, POCKET_Y0 + 0.2, R + T + 7.6 + 7);
    moveSolids(context, id + "mv", id + "tri", placeXf(s * BTN_ANG));
    trimBySurface(context, id + "trim", id, s, 0, false);           // flush with the pad's back
    const xf = screwXf(s, BLANK_SCREW);  const z = BLANK_SCREW[1];
    const yTop = rOut(z, BTN_ANG);
    fCylinder(context, id + "cut" + "h", { "bottomCenter" : xp(xf, vector(0, R - 2, z)) * millimeter, "topCenter" : xp(xf, vector(0, yTop + 5, z)) * millimeter, "radius" : M3_CLEAR / 2 * millimeter });
    fCone(context, id + "cut" + "k", { "bottomCenter" : xp(xf, vector(0, yTop - 2 - (CSK_M3_R - 1.5), z)) * millimeter, "topCenter" : xp(xf, vector(0, yTop - 1.95, z)) * millimeter, "bottomRadius" : 1.5 * millimeter, "topRadius" : CSK_M3_R * millimeter });
    fCylinder(context, id + "cut" + "r", { "bottomCenter" : xp(xf, vector(0, yTop - 2, z)) * millimeter, "topCenter" : xp(xf, vector(0, yTop + 5, z)) * millimeter, "radius" : CSK_M3_R * millimeter });   // 2 mm recess: M3x16 reaches the nut
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
export function buildHeadrest(context is Context, id is Id, H is number, button is HeadrestButton, yaw is HeadrestYaw, rods is boolean) returns map
{
    const zc = 451 + H;
    const pad = buildPad(context, id + "pad");
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
            const padQ = buildPad(context, id + "gp");
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
        const parts = buildHeadrest(context, id + "hr", H, definition.button, definition.yaw, definition.showRods && !definition.printSet);
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
    }, { height : HeadrestHeight.H50, button : HeadrestButton.RIGHT, yaw : HeadrestYaw.Y20, showRods : true, printSet : false, selfTest : false });

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
    rrbox(context, id + "fill", PLUG[0] - 7, PLUG[1] + 7, FRONT + 0.1, 144.5, SOCK_Z0 - 6, TOP - 0.05, 2);
    cleanup(context, id + "cl", id);
    const add = qUnion([solids(context, id + "lens"), solids(context, id + "up"), solids(context, id + "fill")]);
    const C = id + "cut";
    rrbox(context, C + "sock", PLUG[0] - SOCK_CL, PLUG[1] + SOCK_CL, PLUG[2] - SOCK_CL, PLUG[3] + SOCK_CL, SOCK_Z0, TOP + 40, 6);
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
        [56.21,112.30,-7.37,56.20,112.26,-6.75,56.20,112.23,-6.13,56.19,112.19,-5.52,56.19,112.16,-4.90,56.18,112.13,-4.28,56.18,112.09,-3.67,56.17,112.06,-3.05,56.17,112.02,-2.43,56.16,111.99,-1.82,56.16,111.96,-1.20,56.15,111.92,-0.59,56.15,111.89,0.03,56.14,111.85,0.65,56.13,111.82,1.26,56.13,111.78,1.88,56.12,111.75,2.50,56.11,111.72,3.11,56.10,111.68,3.73,56.05,111.64,4.34,55.98,111.59,4.95,55.86,111.53,5.56,55.73,111.47,6.15,55.55,111.40,6.74,55.32,111.38,6.19,55.12,111.37,5.60,54.93,111.36,5.02,54.79,111.37,4.41,54.66,111.37,3.81,54.57,111.38,3.20,54.49,111.40,2.59,54.42,111.42,1.98,54.35,111.43,1.36,54.27,111.45,0.75,54.20,111.47,0.14,54.12,111.48,-0.47,54.05,111.50,-1.09,53.98,111.52,-1.70,53.90,111.54,-2.31,53.83,111.55,-2.92,53.75,111.57,-3.54,53.68,111.59,-4.15,53.61,111.60,-4.76,53.53,111.62,-5.38,53.46,111.64,-5.99,53.39,111.66,-6.60,53.32,111.68,-7.21,53.25,111.69,-7.83,53.19,111.71,-8.44,53.17,111.74,-9.06,53.24,111.79,-9.67,53.37,111.85,-10.27,53.57,111.92,-10.85,53.82,112.00,-11.40,54.13,112.10,-11.93,54.48,112.20,-12.42,54.80,112.25,-12.05,55.08,112.28,-11.50,55.35,112.31,-10.94,55.58,112.32,-10.37,55.79,112.34,-9.79,55.96,112.34,-9.20,56.08,112.34,-8.59,56.17,112.32,-7.98],
        [62.35,110.93,-5.24,62.35,110.88,-4.28,62.34,110.82,-3.31,62.33,110.77,-2.35,62.33,110.72,-1.38,62.32,110.66,-0.42,62.31,110.61,0.55,62.30,110.56,1.51,62.29,110.50,2.48,62.28,110.45,3.44,62.27,110.39,4.41,62.19,110.33,5.37,62.05,110.25,6.32,61.83,110.15,7.26,61.52,110.03,8.16,61.13,109.91,9.04,60.65,109.76,9.86,60.02,109.59,10.57,59.31,109.40,11.20,58.55,109.21,11.76,57.73,109.02,12.10,57.07,108.92,11.41,56.45,108.82,10.67,55.87,108.74,9.90,55.33,108.67,9.10,54.85,108.61,8.27,54.41,108.56,7.41,54.04,108.53,6.52,53.89,108.55,5.56,53.76,108.58,4.61,53.66,108.61,3.65,53.59,108.64,2.68,53.52,108.68,1.72,53.44,108.71,0.76,53.37,108.75,-0.21,53.30,108.79,-1.17,53.23,108.82,-2.13,53.15,108.86,-3.09,53.08,108.90,-4.06,53.01,108.93,-5.02,52.93,108.97,-5.98,52.86,109.00,-6.95,52.79,109.04,-7.91,52.75,109.08,-8.87,52.77,109.14,-9.84,52.84,109.21,-10.80,52.95,109.28,-11.75,53.26,109.39,-12.64,53.90,109.57,-13.34,54.61,109.75,-13.96,55.38,109.95,-14.51,56.20,110.14,-14.99,57.01,110.33,-15.19,57.67,110.43,-14.50,58.32,110.53,-13.79,58.94,110.62,-13.05,59.53,110.71,-12.29,60.08,110.78,-11.50,60.60,110.85,-10.69,61.07,110.90,-9.85,61.49,110.95,-8.98,61.84,110.97,-8.08,62.11,110.98,-7.15,62.29,110.97,-6.20],
        [68.50,109.56,-3.11,68.49,109.50,-1.92,68.49,109.43,-0.73,68.48,109.37,0.46,68.47,109.30,1.65,68.45,109.23,2.84,68.44,109.17,4.03,68.43,109.10,5.22,68.32,109.02,6.40,68.11,108.91,7.57,67.79,108.78,8.71,67.36,108.63,9.80,66.78,108.45,10.83,66.03,108.24,11.73,65.16,108.02,12.51,64.21,107.78,13.18,63.18,107.53,13.73,62.10,107.28,14.18,61.00,107.02,14.55,59.88,106.77,14.86,58.87,106.58,14.45,57.94,106.42,13.73,57.05,106.27,12.94,56.27,106.15,12.08,55.99,106.15,10.92,55.73,106.16,9.75,55.47,106.17,8.59,55.25,106.18,7.42,55.08,106.21,6.24,54.95,106.24,5.05,54.85,106.29,3.87,54.75,106.33,2.68,54.66,106.38,1.49,54.57,106.42,0.30,54.48,106.46,-0.88,54.39,106.51,-2.07,54.30,106.55,-3.26,54.21,106.60,-4.45,54.12,106.64,-5.63,54.03,106.69,-6.82,53.99,106.74,-8.01,54.03,106.81,-9.20,54.16,106.90,-10.38,54.35,107.01,-11.56,54.60,107.12,-12.72,54.90,107.25,-13.86,55.25,107.38,-14.99,56.12,107.60,-15.66,57.21,107.86,-16.07,58.33,108.11,-16.39,59.42,108.34,-16.39,60.35,108.50,-15.67,61.27,108.66,-14.92,62.16,108.81,-14.14,63.02,108.95,-13.33,63.85,109.08,-12.49,64.65,109.20,-11.61,65.40,109.31,-10.69,66.10,109.41,-9.73,66.74,109.49,-8.73,67.31,109.56,-7.69,67.79,109.60,-6.60,68.16,109.62,-5.47,68.41,109.61,-4.30],
        [74.65,108.20,-0.99,74.64,108.12,0.39,74.63,108.05,1.77,74.62,107.97,3.15,74.61,107.89,4.53,74.59,107.81,5.91,74.48,107.72,7.28,74.22,107.59,8.63,73.82,107.44,9.95,73.26,107.25,11.19,72.52,107.03,12.34,71.60,106.78,13.34,70.54,106.51,14.17,69.38,106.23,14.87,68.15,105.94,15.43,66.87,105.64,15.87,65.57,105.35,16.23,64.25,105.05,16.50,62.92,104.75,16.71,61.58,104.46,16.89,60.34,104.22,16.46,59.16,104.01,15.77,58.03,103.81,15.01,57.64,103.80,13.68,57.31,103.80,12.34,57.00,103.80,10.99,56.70,103.81,9.64,56.46,103.83,8.28,56.28,103.87,6.92,56.15,103.91,5.54,56.03,103.96,4.16,55.93,104.02,2.79,55.82,104.07,1.41,55.72,104.12,0.03,55.62,104.17,-1.34,55.51,104.22,-2.72,55.41,104.27,-4.10,55.30,104.33,-5.48,55.24,104.39,-6.85,55.29,104.47,-8.23,55.44,104.58,-9.60,55.70,104.70,-10.95,56.03,104.84,-12.29,56.42,105.00,-13.60,56.83,105.16,-14.91,57.36,105.34,-16.18,58.49,105.61,-16.73,59.83,105.90,-16.93,61.16,106.20,-17.13,62.33,106.41,-16.53,63.46,106.61,-15.76,64.59,106.81,-14.98,65.71,107.01,-14.20,66.81,107.19,-13.38,67.87,107.37,-12.51,68.90,107.54,-11.60,69.88,107.70,-10.64,70.81,107.84,-9.63,71.68,107.97,-8.57,72.48,108.08,-7.44,73.18,108.17,-6.26,73.78,108.23,-5.01,74.24,108.25,-3.71,74.54,108.25,-2.36],
        [80.80,106.83,1.14,80.79,106.75,2.69,80.78,106.66,4.25,80.76,106.57,5.81,80.73,106.48,7.36,80.52,106.36,8.90,80.13,106.19,10.40,79.55,105.99,11.83,78.77,105.76,13.16,77.78,105.48,14.32,76.62,105.18,15.32,75.33,104.86,16.13,73.95,104.54,16.77,72.51,104.20,17.26,71.03,103.87,17.63,69.53,103.54,17.91,68.03,103.21,18.12,66.51,102.88,18.25,64.99,102.55,18.36,63.47,102.22,18.44,62.01,101.93,18.12,60.61,101.66,17.50,59.48,101.47,16.56,58.99,101.45,15.09,58.59,101.44,13.58,58.24,101.45,12.06,57.90,101.46,10.54,57.64,101.49,9.00,57.48,101.54,7.45,57.34,101.59,5.90,57.21,101.65,4.35,57.09,101.70,2.80,56.98,101.76,1.24,56.86,101.82,-0.31,56.74,101.88,-1.86,56.62,101.94,-3.41,56.51,102.00,-4.97,56.49,102.07,-6.52,56.63,102.19,-8.07,56.89,102.32,-9.60,57.26,102.48,-11.10,57.71,102.66,-12.59,58.17,102.84,-14.06,58.64,103.02,-15.54,59.41,103.25,-16.86,60.81,103.57,-17.32,62.31,103.90,-17.54,63.78,104.21,-17.53,65.06,104.44,-16.68,66.35,104.67,-15.82,67.63,104.89,-14.97,68.91,105.12,-14.10,70.18,105.34,-13.24,71.46,105.57,-12.37,72.72,105.79,-11.48,73.94,106.00,-10.53,75.11,106.19,-9.53,76.23,106.37,-8.45,77.27,106.53,-7.30,78.22,106.67,-6.08,79.06,106.78,-4.77,79.77,106.86,-3.39,80.31,106.89,-1.93,80.67,106.89,-0.41],
        [86.95,105.46,3.26,86.94,105.37,4.99,86.92,105.27,6.72,86.86,105.17,8.44,86.59,105.02,10.14,86.10,104.83,11.79,85.39,104.59,13.34,84.43,104.31,14.75,83.26,104.00,15.99,81.92,103.66,17.01,80.44,103.30,17.83,78.86,102.93,18.45,77.23,102.56,18.89,75.57,102.19,19.19,73.90,101.83,19.39,72.21,101.46,19.53,70.52,101.10,19.61,68.83,100.74,19.65,67.14,100.38,19.67,65.45,100.02,19.66,63.78,99.67,19.44,62.17,99.36,18.90,60.92,99.15,17.89,60.29,99.10,16.29,59.83,99.09,14.62,59.43,99.10,12.94,59.07,99.11,11.25,58.83,99.15,9.54,58.67,99.21,7.81,58.51,99.27,6.09,58.38,99.33,4.37,58.25,99.40,2.65,58.12,99.46,0.92,57.99,99.53,-0.80,57.86,99.59,-2.52,57.74,99.66,-4.25,57.76,99.76,-5.97,57.98,99.89,-7.68,58.35,100.06,-9.36,58.83,100.25,-11.01,59.34,100.45,-12.65,59.85,100.65,-14.29,60.39,100.85,-15.92,61.33,101.12,-17.32,62.87,101.48,-17.86,64.54,101.85,-18.09,66.13,102.17,-17.86,67.56,102.42,-16.93,68.98,102.68,-15.99,70.41,102.93,-15.05,71.84,103.18,-14.10,73.27,103.44,-13.16,74.69,103.69,-12.21,76.11,103.94,-11.25,77.53,104.19,-10.29,78.94,104.44,-9.33,80.31,104.67,-8.30,81.62,104.89,-7.19,82.84,105.09,-5.98,83.96,105.25,-4.68,84.94,105.39,-3.26,85.76,105.48,-1.75,86.40,105.53,-0.14,86.80,105.52,1.54],
        [93.10,104.10,5.39,93.08,103.99,7.28,93.03,103.88,9.17,92.74,103.72,11.04,92.19,103.51,12.84,91.38,103.24,14.53,90.31,102.93,16.06,89.01,102.59,17.39,87.51,102.21,18.49,85.87,101.81,19.34,84.12,101.41,19.95,82.31,101.00,20.36,80.48,100.60,20.60,78.63,100.20,20.74,76.78,99.80,20.79,74.92,99.41,20.81,73.07,99.02,20.79,71.21,98.62,20.74,69.36,98.23,20.69,67.51,97.84,20.63,65.66,97.46,20.48,63.85,97.10,20.02,62.38,96.84,19.04,61.56,96.75,17.34,61.03,96.74,15.52,60.60,96.75,13.68,60.23,96.77,11.82,60.02,96.82,9.93,59.85,96.89,8.05,59.68,96.95,6.16,59.54,97.02,4.27,59.39,97.10,2.38,59.25,97.17,0.49,59.11,97.24,-1.40,58.97,97.31,-3.29,59.03,97.42,-5.18,59.31,97.58,-7.04,59.78,97.78,-8.87,60.34,98.00,-10.67,60.89,98.21,-12.47,61.45,98.43,-14.27,62.03,98.64,-16.06,63.07,98.95,-17.59,64.66,99.33,-18.36,66.50,99.73,-18.60,68.22,100.08,-18.29,69.80,100.36,-17.27,71.37,100.64,-16.25,72.94,100.92,-15.23,74.51,101.20,-14.20,76.08,101.48,-13.18,77.65,101.75,-12.15,79.22,102.03,-11.12,80.78,102.31,-10.09,82.35,102.59,-9.05,83.90,102.86,-8.01,85.46,103.14,-6.95,86.97,103.40,-5.84,88.39,103.63,-4.61,89.68,103.83,-3.23,90.82,104.00,-1.73,91.76,104.11,-0.09,92.48,104.17,1.66,92.93,104.16,3.50],
        [99.24,102.73,7.51,99.22,102.62,9.57,98.96,102.45,11.61,98.40,102.23,13.57,97.55,101.95,15.42,96.41,101.62,17.10,95.01,101.24,18.56,93.40,100.83,19.77,91.61,100.40,20.70,89.71,99.96,21.35,87.74,99.52,21.74,85.73,99.09,21.91,83.72,98.66,21.95,81.70,98.23,21.92,79.69,97.81,21.85,77.68,97.38,21.76,75.66,96.96,21.67,73.65,96.54,21.56,71.64,96.12,21.45,69.63,95.70,21.34,67.61,95.27,21.23,65.62,94.87,20.89,63.86,94.54,20.04,62.81,94.41,18.28,62.20,94.39,16.32,61.73,94.39,14.32,61.39,94.43,12.29,61.20,94.50,10.24,61.02,94.57,8.19,60.84,94.64,6.14,60.69,94.72,4.08,60.53,94.80,2.03,60.38,94.87,-0.02,60.23,94.95,-2.07,60.28,95.07,-4.12,60.61,95.25,-6.15,61.15,95.47,-8.12,61.76,95.71,-10.07,62.37,95.94,-12.03,62.97,96.17,-13.98,63.59,96.41,-15.93,64.61,96.72,-17.67,66.19,97.12,-18.83,68.18,97.55,-19.08,70.07,97.94,-18.82,71.78,98.25,-17.71,73.49,98.55,-16.61,75.20,98.85,-15.50,76.91,99.16,-14.40,78.62,99.46,-13.29,80.33,99.77,-12.19,82.04,100.07,-11.08,83.75,100.38,-9.97,85.46,100.68,-8.86,87.17,100.98,-7.75,88.87,101.28,-6.63,90.57,101.59,-5.51,92.26,101.88,-4.37,93.90,102.17,-3.16,95.39,102.41,-1.76,96.69,102.60,-0.18,97.76,102.73,1.58,98.56,102.80,3.47,99.06,102.80,5.47],
        [105.21,101.33,9.57,105.05,101.18,11.77,104.55,100.96,13.91,103.71,100.67,15.94,102.55,100.33,17.79,101.11,99.93,19.43,99.43,99.50,20.80,97.55,99.04,21.86,95.51,98.57,22.60,93.39,98.10,22.99,91.22,97.64,23.02,89.06,97.18,22.89,86.90,96.73,22.76,84.73,96.28,22.62,82.57,95.83,22.49,80.40,95.37,22.36,78.24,94.92,22.22,76.08,94.47,22.09,73.91,94.01,21.96,71.75,93.56,21.82,69.58,93.11,21.69,67.42,92.66,21.49,65.38,92.26,20.91,64.06,92.07,19.16,63.36,92.03,17.06,62.87,92.04,14.90,62.58,92.10,12.71,62.38,92.18,10.50,62.18,92.25,8.30,62.00,92.33,6.09,61.84,92.42,3.88,61.67,92.50,1.68,61.50,92.58,-0.53,61.49,92.70,-2.74,61.83,92.89,-4.92,62.42,93.13,-7.04,63.08,93.38,-9.14,63.73,93.63,-11.24,64.38,93.88,-13.34,65.04,94.13,-15.44,65.89,94.42,-17.46,67.43,94.83,-18.99,69.47,95.29,-19.51,71.57,95.74,-19.47,73.41,96.06,-18.28,75.25,96.39,-17.10,77.09,96.72,-15.91,78.94,97.05,-14.73,80.78,97.38,-13.54,82.62,97.70,-12.36,84.46,98.03,-11.17,86.31,98.36,-9.99,88.15,98.69,-8.80,89.99,99.02,-7.62,91.83,99.34,-6.43,93.68,99.67,-5.25,95.52,100.00,-4.06,97.36,100.33,-2.87,99.19,100.65,-1.67,100.90,100.94,-0.29,102.37,101.17,1.34,103.57,101.32,3.19,104.45,101.40,5.22,105.00,101.41,7.36],
        [105.73,98.89,9.43,105.57,98.74,11.63,105.07,98.52,13.76,104.24,98.23,15.78,103.10,97.89,17.63,101.67,97.50,19.26,100.00,97.07,20.63,98.13,96.62,21.70,96.12,96.15,22.45,94.00,95.68,22.85,91.85,95.22,22.94,89.70,94.76,22.92,87.54,94.30,22.84,85.38,93.85,22.75,83.23,93.40,22.66,81.07,92.95,22.55,78.92,92.49,22.43,76.77,92.04,22.30,74.61,91.59,22.18,72.46,91.14,22.05,70.31,90.69,21.92,68.15,90.24,21.79,66.22,89.87,21.07,64.98,89.70,19.28,64.37,89.68,17.16,63.95,89.71,15.00,63.75,89.79,12.80,63.55,89.86,10.61,63.35,89.94,8.42,63.18,90.02,6.22,63.01,90.10,4.02,62.85,90.18,1.83,62.69,90.27,-0.37,62.84,90.41,-2.56,63.31,90.63,-4.70,63.96,90.88,-6.79,64.61,91.13,-8.89,65.26,91.38,-10.98,65.91,91.63,-13.07,66.56,91.88,-15.16,67.29,92.15,-17.22,68.72,92.54,-18.83,70.51,92.97,-19.81,72.66,93.43,-19.92,74.54,93.78,-18.92,76.37,94.10,-17.74,78.19,94.43,-16.54,80.02,94.75,-15.35,81.85,95.08,-14.16,83.67,95.40,-12.97,85.50,95.72,-11.78,87.32,96.05,-10.58,89.14,96.37,-9.38,90.96,96.69,-8.18,92.78,97.01,-6.97,94.59,97.34,-5.76,96.40,97.65,-4.54,98.18,97.97,-3.28,99.84,98.24,-1.86,101.46,98.51,-0.39,102.92,98.73,1.24,104.10,98.88,3.09,104.98,98.96,5.11,105.53,98.96,7.24],
        [106.25,96.45,9.30,106.09,96.30,11.48,105.60,96.08,13.60,104.78,95.80,15.61,103.65,95.46,17.45,102.24,95.07,19.08,100.59,94.65,20.45,98.74,94.20,21.53,96.75,93.73,22.28,94.65,93.26,22.70,92.52,92.80,22.80,90.37,92.35,22.85,88.23,91.89,22.82,86.09,91.44,22.75,83.95,90.99,22.67,81.81,90.54,22.58,79.67,90.09,22.49,77.53,89.64,22.40,75.39,89.19,22.30,73.25,88.74,22.20,71.11,88.29,22.10,68.97,87.84,21.97,67.08,87.48,21.25,65.93,87.33,19.43,65.40,87.33,17.31,65.12,87.39,15.14,64.92,87.47,12.96,64.72,87.54,10.78,64.53,87.62,8.60,64.36,87.70,6.42,64.19,87.78,4.24,64.03,87.86,2.06,63.95,87.96,-0.13,64.23,88.14,-2.29,64.81,88.37,-4.38,65.46,88.62,-6.46,66.11,88.87,-8.54,66.75,89.12,-10.61,67.40,89.37,-12.69,68.05,89.62,-14.77,68.71,89.87,-16.84,69.93,90.22,-18.59,71.53,90.64,-19.97,73.67,91.09,-20.00,75.63,91.47,-19.35,77.44,91.80,-18.16,79.25,92.12,-16.97,81.06,92.44,-15.78,82.86,92.76,-14.58,84.67,93.08,-13.39,86.47,93.40,-12.19,88.28,93.71,-10.99,90.08,94.03,-9.79,91.88,94.35,-8.58,93.67,94.67,-7.36,95.45,94.98,-6.12,97.19,95.28,-4.83,98.81,95.55,-3.39,100.42,95.81,-1.93,102.03,96.07,-0.47,103.47,96.29,1.16,104.64,96.44,3.00,105.51,96.52,5.01,106.05,96.52,7.13],
        [106.76,94.00,9.17,106.62,93.86,11.33,106.13,93.64,13.44,105.32,93.36,15.43,104.21,93.03,17.26,102.82,92.65,18.88,101.20,92.23,20.26,99.37,91.78,21.34,97.40,91.32,22.11,95.33,90.86,22.55,93.21,90.40,22.67,91.08,89.95,22.71,88.96,89.49,22.75,86.84,89.04,22.72,84.71,88.59,22.65,82.59,88.15,22.58,80.47,87.70,22.49,78.34,87.25,22.41,76.22,86.81,22.32,74.10,86.36,22.23,71.97,85.91,22.14,69.85,85.47,22.06,67.95,85.10,21.44,66.91,84.98,19.57,66.49,85.00,17.44,66.29,85.07,15.28,66.09,85.15,13.12,65.90,85.22,10.96,65.70,85.30,8.80,65.54,85.38,6.63,65.38,85.46,4.47,65.21,85.54,2.31,65.26,85.66,0.14,65.68,85.87,-1.97,66.31,86.11,-4.04,66.96,86.36,-6.10,67.60,86.61,-8.16,68.24,86.85,-10.22,68.88,87.10,-12.28,69.52,87.35,-14.34,70.16,87.59,-16.40,71.10,87.90,-18.31,72.63,88.30,-19.80,74.69,88.75,-20.00,76.73,89.17,-19.69,78.52,89.48,-18.51,80.31,89.80,-17.32,82.10,90.12,-16.13,83.89,90.43,-14.94,85.67,90.75,-13.74,87.46,91.06,-12.54,89.24,91.38,-11.34,91.01,91.69,-10.13,92.78,92.00,-8.90,94.54,92.31,-7.67,96.23,92.60,-6.33,97.83,92.86,-4.89,99.42,93.12,-3.44,101.02,93.38,-1.99,102.61,93.64,-0.54,104.03,93.86,1.09,105.19,94.00,2.92,106.04,94.08,4.91,106.57,94.08,7.01],
        [107.28,91.56,9.04,107.14,91.42,11.18,106.66,91.20,13.27,105.87,90.93,15.25,104.77,90.60,17.07,103.41,90.22,18.69,101.80,89.81,20.06,100.00,89.37,21.16,98.06,88.91,21.94,96.01,88.45,22.40,93.91,88.00,22.53,91.80,87.55,22.58,89.70,87.10,22.62,87.59,86.65,22.66,85.49,86.20,22.63,83.38,85.76,22.55,81.27,85.31,22.48,79.17,84.87,22.41,77.06,84.43,22.33,74.96,83.98,22.24,72.85,83.54,22.15,70.75,83.10,22.06,68.80,82.71,21.60,67.92,82.63,19.69,67.65,82.68,17.55,67.46,82.76,15.41,67.26,82.83,13.27,67.07,82.90,11.12,66.88,82.98,8.98,66.72,83.06,6.83,66.56,83.14,4.69,66.42,83.22,2.54,66.63,83.38,0.41,67.18,83.61,-1.66,67.82,83.86,-3.70,68.45,84.10,-5.74,69.09,84.35,-7.78,69.73,84.59,-9.83,70.36,84.83,-11.87,71.00,85.08,-13.91,71.63,85.32,-15.96,72.38,85.59,-17.95,73.79,85.97,-19.51,75.72,86.41,-20.00,77.83,86.86,-19.99,79.60,87.17,-18.81,81.37,87.48,-17.63,83.14,87.79,-16.44,84.90,88.11,-15.25,86.67,88.42,-14.05,88.43,88.73,-12.85,90.18,89.03,-11.64,91.93,89.34,-10.41,93.65,89.64,-9.17,95.29,89.91,-7.79,96.87,90.17,-6.36,98.45,90.43,-4.92,100.04,90.69,-3.49,101.62,90.95,-2.05,103.20,91.21,-0.60,104.60,91.42,1.01,105.73,91.56,2.84,106.57,91.64,4.81,107.09,91.64,6.90],
        [107.80,89.12,8.91,107.66,88.98,11.03,107.19,88.77,13.10,106.41,88.49,15.07,105.33,88.17,16.88,103.99,87.80,18.49,102.41,87.39,19.87,100.64,86.95,20.97,98.71,86.50,21.77,96.69,86.04,22.25,94.61,85.59,22.40,92.52,85.15,22.44,90.43,84.70,22.48,88.35,84.26,22.53,86.26,83.81,22.57,84.17,83.37,22.53,82.09,82.93,22.46,80.00,82.49,22.39,77.91,82.05,22.32,75.82,81.61,22.24,73.74,81.17,22.16,71.65,80.73,22.07,69.65,80.32,21.77,69.01,80.29,19.79,68.82,80.36,17.66,68.63,80.44,15.54,68.43,80.51,13.41,68.24,80.58,11.29,68.06,80.66,9.16,67.90,80.74,7.04,67.74,80.82,4.91,67.69,80.92,2.78,68.07,81.11,0.69,68.69,81.36,-1.33,69.32,81.60,-3.36,69.95,81.84,-5.38,70.58,82.08,-7.41,71.21,82.33,-9.43,71.84,82.57,-11.46,72.47,82.81,-13.48,73.10,83.05,-15.51,73.76,83.30,-17.52,74.95,83.64,-19.23,76.75,84.07,-20.00,78.84,84.51,-20.00,80.66,84.85,-19.09,82.41,85.16,-17.91,84.16,85.47,-16.72,85.90,85.77,-15.52,87.64,86.08,-14.32,89.37,86.38,-13.12,91.10,86.68,-11.90,92.79,86.97,-10.62,94.37,87.24,-9.22,95.94,87.49,-7.80,97.51,87.75,-6.38,99.08,88.01,-4.95,100.65,88.27,-3.53,102.22,88.52,-2.11,103.78,88.78,-0.67,105.16,88.99,0.94,106.28,89.13,2.75,107.10,89.20,4.72,107.61,89.19,6.79],
        [108.32,86.68,8.78,108.18,86.54,10.88,107.72,86.33,12.94,106.95,86.06,14.89,105.90,85.74,16.69,104.57,85.37,18.30,103.02,84.97,19.67,101.27,84.54,20.78,99.37,84.09,21.59,97.37,83.64,22.09,95.31,83.19,22.26,93.24,82.75,22.31,91.17,82.31,22.35,89.11,81.86,22.39,87.04,81.42,22.43,84.97,80.98,22.47,82.90,80.54,22.44,80.83,80.11,22.37,78.76,79.67,22.30,76.69,79.23,22.22,74.62,78.80,22.15,72.55,78.36,22.08,70.51,77.94,21.94,70.18,77.98,19.88,69.99,78.05,17.78,69.80,78.12,15.67,69.60,78.19,13.57,69.41,78.26,11.46,69.24,78.34,9.35,69.08,78.42,7.25,68.92,78.50,5.14,69.04,78.64,3.04,69.57,78.86,1.00,70.20,79.10,-1.01,70.82,79.34,-3.01,71.45,79.58,-5.02,72.07,79.82,-7.03,72.70,80.06,-9.03,73.32,80.30,-11.04,73.95,80.54,-13.05,74.57,80.78,-15.05,75.20,81.02,-17.06,76.11,81.32,-18.93,77.76,81.72,-20.00,79.83,82.16,-20.00,81.71,82.53,-19.34,83.44,82.83,-18.16,85.16,83.14,-16.97,86.88,83.44,-15.77,88.60,83.74,-14.57,90.30,84.03,-13.35,91.93,84.31,-12.03,93.49,84.57,-10.63,95.05,84.82,-9.21,96.60,85.08,-7.80,98.16,85.33,-6.39,99.71,85.59,-4.98,101.27,85.84,-3.57,102.82,86.10,-2.16,104.37,86.35,-0.73,105.72,86.55,0.87,106.82,86.69,2.67,107.63,86.75,4.62,108.14,86.75,6.68],
        [108.84,84.24,8.65,108.70,84.10,10.73,108.25,83.89,12.76,107.50,83.63,14.69,106.47,83.31,16.48,105.17,82.95,18.08,103.65,82.55,19.46,101.93,82.13,20.57,100.06,81.69,21.40,98.09,81.24,21.92,96.06,80.80,22.13,94.01,80.36,22.17,91.97,79.92,22.21,89.92,79.49,22.25,87.88,79.05,22.29,85.83,78.61,22.34,83.78,78.17,22.38,81.74,77.74,22.35,79.69,77.31,22.28,77.65,76.88,22.21,75.60,76.45,22.14,73.55,76.02,22.07,71.58,75.61,21.87,71.33,75.66,19.80,71.14,75.74,17.72,70.95,75.81,15.64,70.76,75.88,13.56,70.57,75.95,11.47,70.41,76.03,9.39,70.25,76.11,7.31,70.16,76.20,5.22,70.51,76.38,3.17,71.13,76.62,1.18,71.74,76.86,-0.80,72.36,77.09,-2.79,72.98,77.33,-4.77,73.60,77.57,-6.76,74.21,77.81,-8.74,74.83,78.05,-10.73,75.45,78.28,-12.71,76.07,78.52,-14.70,76.68,78.76,-16.68,77.42,79.02,-18.62,78.86,79.40,-20.00,80.91,79.83,-20.00,82.82,80.21,-19.52,84.52,80.51,-18.34,86.22,80.81,-17.15,87.91,81.10,-15.95,89.56,81.39,-14.71,91.14,81.65,-13.36,92.68,81.90,-11.96,94.22,82.16,-10.57,95.75,82.41,-9.17,97.29,82.66,-7.78,98.83,82.91,-6.38,100.37,83.17,-4.99,101.91,83.42,-3.59,103.45,83.67,-2.20,104.97,83.92,-0.78,106.30,84.11,0.82,107.37,84.25,2.61,108.17,84.31,4.54,108.66,84.31,6.57],
        [109.36,81.79,8.52,109.23,81.66,10.57,108.79,81.46,12.58,108.06,81.20,14.49,107.05,80.89,16.26,105.78,80.53,17.85,104.29,80.14,19.23,102.61,79.73,20.35,100.78,79.29,21.20,98.84,78.85,21.75,96.84,78.41,21.99,94.82,77.98,22.03,92.80,77.55,22.07,90.78,77.12,22.12,88.76,76.69,22.16,86.75,76.25,22.20,84.73,75.82,22.24,82.71,75.39,22.28,80.69,74.96,22.26,78.67,74.54,22.19,76.65,74.11,22.12,74.63,73.69,22.05,72.82,73.33,21.59,72.46,73.36,19.59,72.28,73.43,17.53,72.09,73.50,15.47,71.90,73.57,13.42,71.72,73.64,11.36,71.57,73.72,9.30,71.41,73.80,7.25,71.54,73.93,5.20,72.10,74.16,3.22,72.71,74.39,1.26,73.32,74.63,-0.70,73.93,74.86,-2.65,74.54,75.10,-4.61,75.15,75.33,-6.57,75.76,75.57,-8.53,76.37,75.80,-10.49,76.98,76.03,-12.45,77.59,76.27,-14.41,78.20,76.50,-16.37,78.85,76.75,-18.31,80.05,77.09,-19.93,82.05,77.52,-20.00,83.97,77.90,-19.64,85.64,78.20,-18.46,87.29,78.48,-17.26,88.89,78.76,-15.99,90.42,79.01,-14.62,91.94,79.26,-13.24,93.45,79.50,-11.86,94.97,79.75,-10.48,96.49,80.00,-9.11,98.01,80.25,-7.73,99.53,80.50,-6.35,101.05,80.75,-4.98,102.57,81.00,-3.60,104.09,81.25,-2.22,105.58,81.49,-0.82,106.88,81.68,0.77,107.93,81.81,2.55,108.70,81.87,4.46,109.18,81.87,6.46],
        [109.88,79.35,8.39,109.75,79.22,10.42,109.32,79.02,12.40,108.61,78.77,14.29,107.63,78.46,16.04,106.39,78.12,17.63,104.94,77.73,19.00,103.29,77.32,20.13,101.50,76.90,20.99,99.60,76.46,21.57,97.63,76.03,21.84,95.63,75.60,21.90,93.64,75.17,21.94,91.65,74.75,21.98,89.66,74.32,22.02,87.66,73.90,22.06,85.67,73.47,22.10,83.68,73.05,22.14,81.68,72.62,22.18,79.69,72.20,22.18,77.70,71.78,22.11,75.70,71.36,22.04,74.07,71.05,21.31,73.60,71.05,19.37,73.42,71.12,17.34,73.23,71.19,15.31,73.05,71.26,13.28,72.88,71.33,11.25,72.73,71.41,9.22,72.66,71.50,7.19,73.09,71.70,5.22,73.69,71.93,3.28,74.29,72.16,1.35,74.89,72.40,-0.59,75.49,72.63,-2.52,76.10,72.86,-4.45,76.70,73.09,-6.39,77.30,73.32,-8.32,77.90,73.55,-10.25,78.50,73.79,-12.19,79.10,74.02,-14.12,79.71,74.25,-16.05,80.32,74.48,-17.98,81.29,74.78,-19.73,83.18,75.20,-20.00,85.10,75.59,-19.74,86.70,75.87,-18.51,88.24,76.12,-17.19,89.74,76.37,-15.83,91.23,76.61,-14.47,92.73,76.86,-13.12,94.23,77.11,-11.76,95.73,77.35,-10.40,97.23,77.60,-9.04,98.73,77.84,-7.68,100.23,78.09,-6.32,101.73,78.33,-4.96,103.23,78.58,-3.60,104.72,78.83,-2.24,106.19,79.06,-0.85,107.46,79.25,0.73,108.48,79.37,2.49,109.24,79.43,4.38,109.71,79.42,6.36],
        [110.40,76.91,8.26,110.27,76.78,10.26,109.86,76.58,12.21,109.17,76.34,14.08,108.21,76.04,15.82,107.01,75.70,17.39,105.59,75.32,18.76,103.98,74.92,19.90,102.23,74.50,20.78,100.36,74.07,21.38,98.43,73.65,21.69,96.46,73.22,21.76,94.50,72.80,21.80,92.53,72.38,21.84,90.57,71.97,21.88,88.60,71.55,21.92,86.64,71.13,21.96,84.67,70.71,22.00,82.71,70.29,22.04,80.74,69.87,22.08,78.78,69.45,22.09,76.81,69.03,22.03,75.34,68.77,21.06,74.74,68.75,19.19,74.56,68.81,17.19,74.38,68.88,15.19,74.20,68.95,13.19,74.04,69.02,11.19,73.89,69.10,9.19,74.09,69.25,7.20,74.67,69.48,5.29,75.27,69.70,3.39,75.86,69.93,1.48,76.45,70.16,-0.42,77.05,70.39,-2.33,77.64,70.62,-4.24,78.23,70.84,-6.14,78.82,71.07,-8.05,79.42,71.30,-9.96,80.01,71.53,-11.86,80.60,71.76,-13.77,81.20,71.99,-15.67,81.79,72.21,-17.58,82.55,72.47,-19.42,84.25,72.86,-19.94,86.16,73.25,-19.67,87.64,73.50,-18.34,89.12,73.74,-17.00,90.59,73.98,-15.66,92.07,74.22,-14.32,93.55,74.47,-12.98,95.03,74.71,-11.64,96.50,74.95,-10.30,97.98,75.19,-8.96,99.46,75.44,-7.62,100.94,75.68,-6.28,102.41,75.92,-4.94,103.89,76.16,-3.60,105.37,76.40,-2.26,106.81,76.64,-0.87,108.04,76.81,0.70,109.04,76.93,2.44,109.77,76.99,4.30,110.23,76.98,6.26],
        [110.92,74.47,8.13,110.80,74.34,10.09,110.40,74.15,12.01,109.73,73.91,13.85,108.80,73.62,15.57,107.64,73.29,17.13,106.27,72.92,18.49,104.71,72.53,19.64,103.00,72.12,20.54,101.18,71.70,21.17,99.29,71.28,21.53,97.36,70.86,21.62,95.44,70.45,21.66,93.51,70.04,21.70,91.58,69.63,21.74,89.65,69.22,21.78,87.72,68.80,21.82,85.79,68.39,21.86,83.86,67.98,21.90,81.93,67.57,21.93,80.00,67.15,21.97,78.07,66.74,22.00,76.68,66.50,20.95,75.90,66.43,19.17,75.72,66.50,17.21,75.54,66.57,15.24,75.36,66.64,13.28,75.22,66.71,11.31,75.14,66.80,9.34,75.61,67.00,7.44,76.20,67.22,5.57,76.78,67.45,3.70,77.36,67.67,1.83,77.94,67.90,-0.05,78.53,68.12,-1.92,79.11,68.35,-3.79,79.69,68.57,-5.66,80.27,68.79,-7.54,80.86,69.02,-9.41,81.44,69.24,-11.28,82.02,69.47,-13.15,82.60,69.69,-15.03,83.19,69.91,-16.90,83.80,70.14,-18.76,85.19,70.48,-19.52,87.10,70.87,-19.26,88.58,71.12,-17.99,90.05,71.36,-16.69,91.51,71.60,-15.39,92.98,71.84,-14.08,94.44,72.08,-12.78,95.89,72.32,-11.46,97.34,72.56,-10.14,98.79,72.80,-8.83,100.24,73.04,-7.51,101.69,73.27,-6.20,103.14,73.51,-4.88,104.60,73.75,-3.56,106.05,73.99,-2.25,107.44,74.21,-0.87,108.64,74.38,0.68,109.60,74.50,2.40,110.31,74.55,4.24,110.75,74.54,6.16],
        [111.30,72.00,7.99,111.19,71.87,9.91,110.81,71.69,11.78,110.17,71.46,13.58,109.29,71.18,15.26,108.18,70.86,16.79,106.87,70.51,18.15,105.38,70.13,19.30,103.74,69.74,20.21,101.99,69.33,20.89,100.17,68.92,21.31,98.30,68.51,21.47,96.42,68.11,21.51,94.54,67.71,21.54,92.66,67.31,21.58,90.78,66.91,21.61,88.90,66.51,21.64,87.03,66.10,21.68,85.15,65.70,21.71,83.27,65.30,21.75,81.39,64.90,21.78,79.51,64.50,21.81,78.10,64.24,20.97,77.16,64.13,19.30,76.90,64.18,17.42,76.72,64.24,15.50,76.55,64.31,13.59,76.41,64.38,11.68,76.51,64.51,9.78,77.06,64.72,7.95,77.63,64.94,6.13,78.20,65.16,4.31,78.76,65.38,2.48,79.33,65.59,0.66,79.90,65.81,-1.16,80.47,66.03,-2.98,81.03,66.25,-4.81,81.60,66.47,-6.63,82.17,66.69,-8.45,82.73,66.90,-10.27,83.30,67.12,-12.10,83.87,67.34,-13.92,84.44,67.56,-15.74,85.00,67.78,-17.56,86.01,68.05,-18.66,87.84,68.42,-18.22,89.38,68.69,-17.15,90.82,68.93,-15.91,92.27,69.17,-14.66,93.71,69.41,-13.42,95.15,69.65,-12.17,96.60,69.89,-10.93,98.04,70.13,-9.68,99.48,70.37,-8.44,100.92,70.61,-7.19,102.37,70.85,-5.95,103.81,71.09,-4.70,105.25,71.32,-3.46,106.67,71.56,-2.19,107.99,71.76,-0.80,109.13,71.93,0.73,110.05,72.03,2.41,110.73,72.08,4.21,111.15,72.07,6.08],
        [107.78,68.70,7.82,107.69,68.59,9.54,107.36,68.43,11.22,106.79,68.22,12.84,105.99,67.97,14.35,104.99,67.68,15.72,103.80,67.37,16.92,102.45,67.02,17.95,100.99,66.67,18.79,99.45,66.30,19.48,97.87,65.94,20.04,96.23,65.57,20.43,94.56,65.20,20.68,92.88,64.83,20.85,91.20,64.47,20.97,89.52,64.11,21.06,87.83,63.75,21.14,86.15,63.38,21.20,84.46,63.02,21.25,82.78,62.66,21.31,81.09,62.30,21.35,79.57,61.99,21.09,78.72,61.89,19.59,78.11,61.85,18.01,77.95,61.90,16.30,77.80,61.96,14.58,77.66,62.03,12.86,77.69,62.13,11.15,78.19,62.32,9.51,78.70,62.51,7.87,79.21,62.71,6.23,79.72,62.91,4.60,80.22,63.10,2.96,80.73,63.30,1.33,81.24,63.49,-0.31,81.75,63.69,-1.95,82.26,63.89,-3.58,82.77,64.08,-5.22,83.28,64.28,-6.86,83.79,64.47,-8.49,84.30,64.67,-10.13,84.81,64.87,-11.77,85.32,65.06,-13.40,85.83,65.26,-15.04,86.37,65.46,-16.66,87.63,65.73,-16.71,89.16,66.01,-15.96,90.45,66.23,-14.85,91.68,66.43,-13.66,92.90,66.62,-12.46,94.09,66.81,-11.22,95.28,67.00,-9.99,96.47,67.18,-8.75,97.66,67.37,-7.52,98.85,67.56,-6.28,100.04,67.74,-5.04,101.23,67.93,-3.81,102.41,68.11,-2.57,103.59,68.30,-1.32,104.74,68.48,-0.05,105.78,68.62,1.32,106.62,68.72,2.82,107.24,68.77,4.43,107.63,68.76,6.11],
        [104.25,65.40,7.65,104.19,65.31,9.12,103.93,65.17,10.56,103.47,65.00,11.95,102.81,64.79,13.25,101.98,64.55,14.43,100.98,64.28,15.48,99.85,63.99,16.38,98.63,63.69,17.14,97.35,63.39,17.80,96.02,63.08,18.37,94.68,62.76,18.89,93.31,62.45,19.31,91.90,62.13,19.62,90.48,61.82,19.85,89.06,61.50,20.03,87.62,61.19,20.18,86.19,60.88,20.30,84.75,60.57,20.41,83.32,60.26,20.50,81.88,59.95,20.59,80.58,59.69,20.36,79.85,59.60,19.08,79.24,59.54,17.76,79.11,59.59,16.30,78.98,59.64,14.83,78.87,59.70,13.36,79.05,59.81,11.92,79.48,59.98,10.52,79.91,60.15,9.13,80.35,60.31,7.73,80.78,60.48,6.33,81.22,60.65,4.94,81.65,60.82,3.54,82.09,60.98,2.14,82.52,61.15,0.74,82.96,61.32,-0.65,83.39,61.48,-2.05,83.83,61.65,-3.45,84.26,61.82,-4.84,84.70,61.99,-6.24,85.14,62.16,-7.63,85.61,62.33,-9.02,86.12,62.51,-10.39,86.71,62.71,-11.72,87.38,62.92,-13.02,88.40,63.12,-12.70,89.51,63.30,-11.74,90.61,63.49,-10.78,91.63,63.65,-9.73,92.64,63.81,-8.67,93.66,63.96,-7.62,94.67,64.12,-6.56,95.68,64.28,-5.51,96.70,64.44,-4.45,97.71,64.60,-3.39,98.71,64.76,-2.32,99.71,64.91,-1.25,100.70,65.06,-0.18,101.68,65.21,0.91,102.55,65.34,2.09,103.26,65.42,3.38,103.78,65.45,4.75,104.11,65.45,6.19],
        [100.73,62.10,7.48,100.70,62.03,8.66,100.51,61.93,9.83,100.17,61.80,10.96,99.69,61.63,12.03,99.06,61.45,13.02,98.28,61.24,13.89,97.41,61.01,14.66,96.46,60.77,15.33,95.46,60.53,15.93,94.43,60.28,16.46,93.37,60.03,16.94,92.30,59.78,17.38,91.22,59.53,17.81,90.13,59.27,18.19,89.01,59.02,18.50,87.88,58.76,18.74,86.73,58.51,18.95,85.59,58.26,19.13,84.44,58.00,19.28,83.28,57.75,19.41,82.13,57.50,19.53,81.21,57.33,19.00,80.62,57.26,17.96,80.32,57.26,16.84,80.21,57.30,15.66,80.12,57.34,14.48,80.22,57.43,13.31,80.57,57.56,12.19,80.92,57.70,11.06,81.27,57.83,9.93,81.62,57.97,8.81,81.97,58.10,7.68,82.32,58.24,6.55,82.67,58.37,5.43,83.03,58.51,4.30,83.38,58.64,3.17,83.73,58.78,2.05,84.08,58.91,0.92,84.44,59.05,-0.20,84.83,59.19,-1.31,85.28,59.35,-2.40,85.82,59.52,-3.44,86.36,59.69,-4.49,86.91,59.86,-5.53,87.45,60.03,-6.57,88.00,60.20,-7.61,88.72,60.37,-7.97,89.67,60.54,-7.28,90.59,60.70,-6.54,91.49,60.85,-5.78,92.31,60.98,-4.94,93.12,61.10,-4.08,93.93,61.23,-3.22,94.74,61.35,-2.36,95.55,61.48,-1.50,96.35,61.60,-0.63,97.15,61.73,0.24,97.94,61.85,1.11,98.72,61.97,2.00,99.40,62.06,2.98,99.94,62.12,4.03,100.34,62.14,5.14,100.61,62.14,6.30],
        [97.20,58.80,7.31,97.20,58.75,8.21,97.08,58.68,9.09,96.87,58.59,9.96,96.56,58.48,10.80,96.12,58.34,11.57,95.59,58.19,12.28,94.97,58.03,12.91,94.28,57.85,13.46,93.56,57.67,13.96,92.81,57.48,14.42,92.04,57.30,14.84,91.25,57.11,15.24,90.45,56.92,15.61,89.65,56.73,15.96,88.84,56.54,16.29,88.02,56.35,16.62,87.20,56.16,16.94,86.37,55.97,17.23,85.53,55.77,17.47,84.68,55.58,17.69,83.82,55.39,17.87,82.96,55.20,18.05,82.14,55.03,17.97,81.63,54.96,17.25,81.43,54.96,16.39,81.36,54.99,15.49,81.43,55.06,14.61,81.69,55.16,13.76,81.96,55.26,12.90,82.22,55.36,12.05,82.49,55.46,11.20,82.75,55.57,10.34,83.02,55.67,9.49,83.28,55.77,8.64,83.56,55.87,7.79,83.86,55.98,6.95,84.25,56.11,6.15,84.67,56.24,5.36,85.08,56.37,4.57,85.49,56.50,3.78,85.90,56.63,2.99,86.32,56.76,2.21,86.73,56.89,1.42,87.14,57.02,0.63,87.55,57.15,-0.16,87.97,57.28,-0.95,88.38,57.41,-1.74,88.79,57.54,-2.53,89.30,57.67,-2.98,90.03,57.80,-2.47,90.76,57.93,-1.96,91.47,58.05,-1.43,92.17,58.17,-0.87,92.79,58.27,-0.23,93.39,58.36,0.43,93.99,58.45,1.09,94.60,58.54,1.76,95.19,58.63,2.42,95.75,58.71,3.12,96.23,58.78,3.88,96.62,58.82,4.69,96.91,58.83,5.54,97.10,58.83,6.42],
        [93.68,55.50,7.14,93.68,55.47,7.74,93.64,55.43,8.34,93.54,55.38,8.94,93.37,55.31,9.51,93.16,55.23,10.07,92.87,55.14,10.59,92.51,55.04,11.07,92.10,54.93,11.50,91.65,54.81,11.88,91.17,54.69,12.23,90.67,54.57,12.56,90.16,54.45,12.86,89.65,54.32,13.16,89.13,54.20,13.45,88.61,54.07,13.71,88.08,53.94,13.98,87.55,53.82,14.24,87.01,53.69,14.49,86.47,53.56,14.73,85.93,53.43,14.97,85.38,53.31,15.19,84.83,53.18,15.42,84.28,53.05,15.64,83.74,52.92,15.86,83.18,52.79,16.06,82.62,52.66,16.25,82.69,52.71,15.73,82.97,52.79,15.20,83.25,52.88,14.66,83.53,52.97,14.13,83.81,53.06,13.60,84.08,53.14,13.07,84.36,53.23,12.54,84.64,53.32,12.01,84.92,53.41,11.48,85.19,53.49,10.95,85.47,53.58,10.42,85.75,53.67,9.89,86.03,53.76,9.36,86.30,53.84,8.83,86.58,53.93,8.30,86.86,54.02,7.77,87.14,54.10,7.24,87.41,54.19,6.71,87.69,54.28,6.17,87.97,54.37,5.64,88.25,54.45,5.11,88.53,54.54,4.58,88.80,54.63,4.05,89.08,54.72,3.52,89.36,54.80,2.99,89.64,54.89,2.46,89.97,54.98,2.07,90.46,55.07,2.41,90.95,55.16,2.75,91.44,55.24,3.10,91.93,55.33,3.44,92.39,55.40,3.82,92.78,55.46,4.28,93.05,55.49,4.81,93.29,55.51,5.37,93.47,55.52,5.95,93.59,55.51,6.54],
        [91.80,54.03,7.08,91.80,54.01,7.39,91.81,54.00,7.69,91.81,53.98,7.99,91.72,53.95,8.28,91.64,53.92,8.57,91.56,53.88,8.86,91.42,53.84,9.13,91.27,53.79,9.39,91.12,53.75,9.65,90.96,53.70,9.90,90.77,53.65,10.13,90.57,53.59,10.35,90.38,53.54,10.58,90.17,53.48,10.80,89.95,53.43,10.99,89.73,53.37,11.19,89.49,53.31,11.36,89.21,53.24,11.48,88.94,53.18,11.59,88.67,53.12,11.71,88.39,53.05,11.81,88.11,52.99,11.90,87.82,52.92,11.96,87.52,52.86,11.98,87.24,52.80,11.94,87.06,52.77,11.72,87.04,52.78,11.42,87.04,52.80,11.11,87.10,52.83,10.82,87.19,52.86,10.53,87.28,52.90,10.25,87.38,52.94,9.96,87.50,52.98,9.69,87.62,53.02,9.41,87.75,53.06,9.14,87.87,53.10,8.86,87.99,53.14,8.59,88.11,53.18,8.31,88.24,53.22,8.04,88.37,53.26,7.77,88.51,53.31,7.50,88.64,53.35,7.23,88.78,53.39,6.97,88.91,53.44,6.70,89.05,53.48,6.43,89.18,53.52,6.16,89.31,53.56,5.89,89.45,53.61,5.62,89.58,53.65,5.35,89.72,53.69,5.09,89.85,53.74,4.82,89.99,53.78,4.55,90.13,53.82,4.29,90.34,53.86,4.48,90.54,53.89,4.71,90.74,53.92,4.94,90.93,53.95,5.17,91.11,53.97,5.42,91.26,53.99,5.68,91.41,54.01,5.94,91.55,54.02,6.21,91.64,54.03,6.50,91.72,54.03,6.79]],
    "20" : [
        [55.60,112.42,-7.51,55.60,112.38,-6.93,55.60,112.35,-6.34,55.60,112.32,-5.76,55.60,112.29,-5.17,55.60,112.25,-4.59,55.60,112.22,-4.00,55.60,112.19,-3.42,55.60,112.15,-2.83,55.60,112.12,-2.25,55.60,112.09,-1.67,55.60,112.06,-1.08,55.60,112.02,-0.50,55.60,111.99,0.09,55.60,111.96,0.67,55.59,111.92,1.26,55.59,111.89,1.84,55.59,111.86,2.43,55.59,111.82,3.01,55.58,111.79,3.60,55.56,111.75,4.18,55.51,111.70,4.76,55.45,111.64,5.34,55.35,111.58,5.91,55.16,111.53,5.48,54.98,111.49,4.92,54.83,111.47,4.36,54.69,111.45,3.79,54.61,111.46,3.21,54.53,111.46,2.63,54.45,111.46,2.05,54.38,111.47,1.47,54.30,111.47,0.89,54.23,111.48,0.31,54.15,111.48,-0.27,54.07,111.49,-0.85,54.00,111.49,-1.44,53.92,111.50,-2.02,53.85,111.50,-2.60,53.77,111.51,-3.18,53.70,111.51,-3.76,53.62,111.52,-4.34,53.55,111.52,-4.92,53.47,111.53,-5.50,53.40,111.53,-6.08,53.33,111.54,-6.66,53.25,111.55,-7.24,53.18,111.55,-7.82,53.12,111.56,-8.41,53.09,111.58,-8.99,53.16,111.64,-9.57,53.28,111.72,-10.13,53.46,111.81,-10.68,53.70,111.93,-11.20,53.99,112.06,-11.69,54.31,112.20,-12.16,54.60,112.30,-12.05,54.80,112.35,-11.51,54.99,112.39,-10.96,55.16,112.41,-10.39,55.30,112.44,-9.83,55.43,112.45,-9.26,55.52,112.45,-8.68,55.58,112.44,-8.10],
        [60.26,111.35,-5.75,60.26,111.30,-4.83,60.26,111.25,-3.92,60.26,111.19,-3.01,60.26,111.14,-2.09,60.26,111.09,-1.18,60.25,111.04,-0.26,60.25,110.99,0.65,60.25,110.94,1.56,60.25,110.89,2.48,60.25,110.83,3.39,60.24,110.78,4.30,60.18,110.71,5.21,60.08,110.62,6.12,59.94,110.52,7.02,59.72,110.39,7.90,59.46,110.25,8.76,59.11,110.07,9.59,58.64,109.86,10.35,58.10,109.63,11.04,57.49,109.37,11.62,56.88,109.18,10.97,56.28,109.00,10.31,55.73,108.84,9.59,55.21,108.69,8.86,54.73,108.57,8.08,54.31,108.45,7.28,54.07,108.42,6.41,53.94,108.42,5.50,53.83,108.43,4.59,53.75,108.45,3.68,53.68,108.48,2.77,53.61,108.50,1.86,53.55,108.53,0.95,53.48,108.56,0.03,53.42,108.59,-0.88,53.35,108.61,-1.79,53.29,108.64,-2.70,53.22,108.67,-3.62,53.15,108.69,-4.53,53.09,108.72,-5.44,53.02,108.75,-6.35,52.96,108.77,-7.27,52.90,108.81,-8.18,52.88,108.85,-9.09,52.91,108.91,-10.00,52.98,108.99,-10.91,53.09,109.08,-11.82,53.23,109.18,-12.72,53.84,109.43,-13.34,54.52,109.71,-13.89,55.23,110.00,-14.37,55.98,110.30,-14.80,56.70,110.57,-14.96,57.21,110.71,-14.21,57.69,110.84,-13.44,58.14,110.96,-12.66,58.56,111.07,-11.86,58.96,111.17,-11.03,59.31,111.25,-10.19,59.62,111.31,-9.33,59.87,111.36,-8.46,60.07,111.38,-7.56,60.21,111.38,-6.66],
        [64.91,110.28,-3.98,64.91,110.22,-2.88,64.91,110.16,-1.77,64.91,110.09,-0.67,64.91,110.03,0.43,64.91,109.97,1.54,64.90,109.91,2.64,64.90,109.84,3.75,64.90,109.78,4.85,64.83,109.70,5.95,64.70,109.59,7.04,64.49,109.45,8.12,64.20,109.29,9.17,63.80,109.08,10.18,63.26,108.83,11.11,62.61,108.55,11.96,61.86,108.24,12.71,61.04,107.90,13.37,60.16,107.55,13.94,59.24,107.19,14.43,58.36,106.90,13.87,57.51,106.63,13.21,56.69,106.37,12.51,56.13,106.21,11.63,55.90,106.19,10.55,55.66,106.16,9.47,55.44,106.14,8.38,55.25,106.13,7.30,55.10,106.14,6.20,54.99,106.16,5.10,54.90,106.19,4.00,54.82,106.22,2.90,54.74,106.26,1.79,54.66,106.29,0.69,54.58,106.32,-0.41,54.51,106.35,-1.51,54.43,106.39,-2.62,54.35,106.42,-3.72,54.27,106.45,-4.82,54.19,106.48,-5.92,54.12,106.52,-7.03,54.09,106.57,-8.13,54.14,106.65,-9.23,54.24,106.75,-10.33,54.40,106.87,-11.42,54.61,107.00,-12.50,54.86,107.15,-13.56,55.14,107.32,-14.62,55.68,107.56,-15.45,56.65,107.93,-15.82,57.64,108.31,-16.13,58.65,108.69,-16.39,59.40,108.92,-15.65,60.10,109.13,-14.83,60.78,109.33,-13.98,61.43,109.52,-13.10,62.04,109.69,-12.20,62.61,109.85,-11.26,63.14,109.99,-10.30,63.61,110.10,-9.31,64.03,110.20,-8.29,64.38,110.27,-7.24,64.65,110.31,-6.17,64.84,110.31,-5.08],
        [69.56,109.21,-2.22,69.57,109.14,-0.96,69.56,109.07,0.29,69.56,109.00,1.55,69.56,108.93,2.81,69.56,108.86,4.06,69.55,108.78,5.32,69.49,108.69,6.57,69.34,108.57,7.81,69.08,108.41,9.03,68.72,108.21,10.22,68.20,107.96,11.33,67.54,107.66,12.36,66.75,107.32,13.27,65.85,106.95,14.07,64.87,106.55,14.75,63.83,106.14,15.32,62.75,105.72,15.81,61.64,105.30,16.21,60.52,104.88,16.33,59.45,104.52,15.77,58.41,104.18,15.14,57.72,103.98,14.23,57.41,103.94,13.01,57.14,103.91,11.78,56.87,103.88,10.55,56.62,103.86,9.32,56.42,103.85,8.08,56.27,103.87,6.83,56.16,103.90,5.58,56.06,103.93,4.32,55.97,103.97,3.07,55.88,104.00,1.82,55.79,104.04,0.56,55.70,104.08,-0.69,55.61,104.11,-1.95,55.52,104.15,-3.20,55.43,104.19,-4.45,55.34,104.23,-5.71,55.30,104.28,-6.96,55.34,104.37,-8.22,55.47,104.49,-9.46,55.67,104.63,-10.69,55.93,104.79,-11.91,56.25,104.97,-13.12,56.59,105.16,-14.31,56.95,105.36,-15.50,57.55,105.64,-16.53,58.71,106.07,-16.73,59.88,106.50,-16.89,60.99,106.90,-16.81,61.90,107.19,-15.98,62.79,107.47,-15.14,63.66,107.73,-14.27,64.49,107.98,-13.36,65.28,108.22,-12.41,66.03,108.44,-11.43,66.73,108.64,-10.40,67.38,108.81,-9.34,67.96,108.96,-8.23,68.48,109.09,-7.09,68.92,109.18,-5.92,69.25,109.23,-4.71,69.47,109.25,-3.47],
        [74.22,108.14,-0.45,74.22,108.06,0.94,74.22,107.99,2.33,74.22,107.91,3.72,74.21,107.83,5.11,74.20,107.75,6.51,74.09,107.63,7.89,73.85,107.46,9.25,73.48,107.26,10.58,72.96,107.00,11.84,72.25,106.67,13.00,71.39,106.30,14.04,70.41,105.90,14.94,69.33,105.46,15.70,68.18,105.01,16.34,66.97,104.54,16.86,65.73,104.06,17.28,64.47,103.58,17.62,63.19,103.10,17.89,61.91,102.64,17.87,60.69,102.22,17.36,59.47,101.81,16.80,59.01,101.71,15.50,58.65,101.66,14.16,58.34,101.62,12.80,58.04,101.59,11.44,57.77,101.57,10.07,57.57,101.57,8.69,57.44,101.60,7.30,57.32,101.63,5.92,57.21,101.67,4.53,57.11,101.71,3.14,57.01,101.75,1.75,56.91,101.80,0.36,56.81,101.84,-1.03,56.71,101.88,-2.42,56.61,101.92,-3.81,56.52,101.96,-5.20,56.52,102.04,-6.60,56.63,102.16,-7.98,56.83,102.31,-9.35,57.12,102.49,-10.70,57.47,102.69,-12.04,57.85,102.90,-13.36,58.22,103.11,-14.69,58.65,103.34,-16.00,59.42,103.68,-17.07,60.72,104.16,-17.25,62.02,104.65,-17.42,63.20,105.06,-17.12,64.21,105.38,-16.22,65.22,105.69,-15.31,66.23,106.01,-14.40,67.23,106.32,-13.48,68.21,106.63,-12.54,69.15,106.91,-11.55,70.04,107.18,-10.51,70.87,107.42,-9.42,71.63,107.64,-8.27,72.33,107.82,-7.08,72.94,107.98,-5.84,73.45,108.09,-4.54,73.85,108.16,-3.21,74.11,108.18,-1.84],
        [78.87,107.07,1.31,78.87,106.99,2.83,78.87,106.90,4.35,78.87,106.82,5.88,78.85,106.72,7.40,78.69,106.58,8.91,78.39,106.39,10.38,77.92,106.14,11.81,77.26,105.83,13.15,76.42,105.45,14.36,75.42,105.03,15.43,74.29,104.57,16.34,73.06,104.08,17.09,71.76,103.57,17.70,70.41,103.05,18.17,69.02,102.53,18.54,67.62,102.00,18.82,66.21,101.47,19.04,64.79,100.95,19.18,63.37,100.44,19.11,62.00,99.96,18.67,60.81,99.57,17.96,60.24,99.44,16.55,59.83,99.37,15.08,59.48,99.33,13.60,59.16,99.30,12.11,58.89,99.28,10.61,58.72,99.30,9.10,58.59,99.34,7.58,58.46,99.38,6.06,58.34,99.42,4.54,58.23,99.47,3.02,58.13,99.51,1.50,58.02,99.56,-0.02,57.91,99.60,-1.53,57.80,99.65,-3.05,57.71,99.70,-4.57,57.75,99.80,-6.09,57.93,99.95,-7.60,58.21,100.13,-9.09,58.58,100.35,-10.55,58.99,100.58,-12.00,59.40,100.81,-13.45,59.81,101.04,-14.90,60.29,101.29,-16.32,61.17,101.68,-17.48,62.56,102.20,-17.74,63.98,102.73,-17.91,65.25,103.17,-17.51,66.37,103.52,-16.53,67.48,103.87,-15.55,68.59,104.22,-14.57,69.70,104.56,-13.58,70.80,104.91,-12.59,71.90,105.26,-11.59,72.99,105.59,-10.58,74.02,105.91,-9.50,74.99,106.20,-8.36,75.88,106.45,-7.15,76.69,106.68,-5.88,77.40,106.86,-4.54,77.99,107.00,-3.15,78.45,107.09,-1.70,78.75,107.11,-0.21],
        [83.52,106.00,3.08,83.53,105.91,4.72,83.52,105.82,6.37,83.51,105.72,8.01,83.33,105.57,9.65,82.99,105.35,11.24,82.45,105.07,12.78,81.70,104.72,14.20,80.75,104.30,15.48,79.64,103.83,16.60,78.38,103.32,17.54,77.02,102.79,18.30,75.59,102.23,18.90,74.11,101.67,19.34,72.59,101.10,19.67,71.06,100.53,19.89,69.52,99.96,20.05,67.98,99.39,20.14,66.43,98.83,20.20,64.89,98.27,20.10,63.37,97.74,19.74,62.13,97.33,18.91,61.44,97.16,17.43,60.97,97.08,15.85,60.60,97.04,14.24,60.26,97.00,12.63,60.02,97.00,11.00,59.87,97.04,9.36,59.73,97.08,7.71,59.59,97.12,6.07,59.47,97.17,4.43,59.35,97.22,2.78,59.23,97.27,1.14,59.12,97.32,-0.51,59.00,97.37,-2.15,58.91,97.43,-3.80,58.99,97.55,-5.44,59.23,97.72,-7.06,59.58,97.94,-8.65,60.02,98.19,-10.23,60.46,98.44,-11.79,60.91,98.69,-13.36,61.35,98.94,-14.93,61.86,99.21,-16.48,62.81,99.62,-17.74,64.22,100.17,-18.21,65.76,100.73,-18.39,67.15,101.22,-17.97,68.36,101.60,-16.93,69.57,101.98,-15.88,70.78,102.36,-14.82,71.99,102.75,-13.77,73.20,103.13,-12.71,74.40,103.50,-11.65,75.60,103.88,-10.58,76.79,104.25,-9.50,77.97,104.62,-8.41,79.08,104.96,-7.24,80.11,105.26,-5.99,81.04,105.53,-4.65,81.85,105.75,-3.23,82.53,105.91,-1.74,83.05,106.01,-0.18,83.39,106.05,1.43],
        [88.18,104.93,4.84,88.18,104.84,6.61,88.17,104.73,8.38,88.02,104.58,10.13,87.66,104.35,11.85,87.08,104.05,13.50,86.27,103.67,15.02,85.25,103.23,16.40,84.05,102.72,17.59,82.70,102.17,18.59,81.23,101.59,19.39,79.67,100.99,19.98,78.06,100.38,20.40,76.42,99.77,20.69,74.77,99.16,20.86,73.11,98.55,20.96,71.45,97.94,21.00,69.78,97.34,21.00,68.12,96.73,20.99,66.46,96.13,20.89,64.81,95.55,20.58,63.44,95.10,19.73,62.60,94.88,18.19,62.08,94.79,16.51,61.69,94.74,14.78,61.35,94.71,13.04,61.16,94.74,11.28,61.01,94.78,9.52,60.86,94.83,7.75,60.72,94.88,5.99,60.59,94.93,4.22,60.46,94.98,2.46,60.34,95.03,0.69,60.21,95.08,-1.08,60.12,95.15,-2.84,60.22,95.29,-4.60,60.51,95.49,-6.34,60.93,95.73,-8.04,61.41,96.00,-9.73,61.88,96.27,-11.41,62.36,96.54,-13.10,62.84,96.80,-14.78,63.35,97.08,-16.45,64.31,97.51,-17.85,65.69,98.06,-18.65,67.34,98.67,-18.84,68.86,99.21,-18.52,70.17,99.62,-17.40,71.48,100.04,-16.28,72.79,100.45,-15.16,74.10,100.86,-14.04,75.41,101.28,-12.92,76.71,101.69,-11.79,78.01,102.10,-10.66,79.31,102.51,-9.53,80.60,102.91,-8.38,81.88,103.31,-7.23,83.14,103.71,-6.05,84.31,104.06,-4.77,85.37,104.37,-3.39,86.30,104.63,-1.90,87.06,104.82,-0.32,87.65,104.94,1.35,88.03,104.98,3.08],
        [92.83,103.87,6.60,92.83,103.76,8.49,92.73,103.62,10.37,92.39,103.39,12.22,91.81,103.08,13.99,90.98,102.69,15.64,89.92,102.22,17.13,88.65,101.69,18.43,87.22,101.10,19.51,85.65,100.48,20.36,83.99,99.84,20.98,82.26,99.19,21.40,80.50,98.54,21.63,78.73,97.89,21.74,76.96,97.24,21.77,75.18,96.60,21.75,73.40,95.95,21.70,71.62,95.31,21.63,69.85,94.67,21.54,68.07,94.03,21.44,66.30,93.39,21.23,64.75,92.87,20.45,63.75,92.59,18.89,63.18,92.49,17.09,62.77,92.44,15.24,62.47,92.44,13.38,62.30,92.48,11.50,62.14,92.53,9.61,61.98,92.57,7.73,61.84,92.63,5.84,61.71,92.68,3.96,61.57,92.74,2.07,61.44,92.80,0.19,61.33,92.86,-1.70,61.44,93.01,-3.58,61.75,93.23,-5.43,62.23,93.50,-7.24,62.74,93.78,-9.03,63.25,94.07,-10.83,63.76,94.36,-12.63,64.27,94.64,-14.43,64.78,94.93,-16.22,65.67,95.34,-17.81,66.96,95.88,-19.07,68.72,96.53,-19.27,70.41,97.14,-19.15,71.81,97.58,-17.96,73.21,98.03,-16.78,74.62,98.47,-15.59,76.02,98.92,-14.40,77.42,99.36,-13.21,78.82,99.80,-12.02,80.22,100.25,-10.83,81.61,100.69,-9.63,83.01,101.13,-8.43,84.40,101.57,-7.23,85.78,102.00,-6.01,87.15,102.43,-4.79,88.48,102.85,-3.51,89.69,103.21,-2.10,90.73,103.50,-0.55,91.60,103.72,1.11,92.25,103.86,2.88,92.67,103.91,4.72],
        [97.48,102.80,8.37,97.45,102.67,10.37,97.17,102.46,12.35,96.62,102.15,14.25,95.80,101.76,16.04,94.73,101.27,17.67,93.43,100.72,19.10,91.94,100.11,20.29,90.30,99.46,21.24,88.53,98.78,21.92,86.70,98.09,22.33,84.82,97.40,22.51,82.93,96.71,22.53,81.05,96.03,22.46,79.16,95.35,22.36,77.27,94.66,22.26,75.39,93.98,22.15,73.50,93.30,22.03,71.61,92.62,21.92,69.73,91.94,21.81,67.84,91.27,21.67,66.08,90.65,21.15,64.90,90.31,19.58,64.28,90.19,17.68,63.86,90.15,15.71,63.62,90.17,13.72,63.45,90.22,11.72,63.28,90.27,9.72,63.11,90.32,7.72,62.97,90.38,5.72,62.82,90.44,3.71,62.68,90.50,1.71,62.54,90.56,-0.29,62.61,90.70,-2.29,62.95,90.93,-4.26,63.46,91.22,-6.18,64.00,91.53,-8.09,64.54,91.83,-10.00,65.08,92.13,-11.91,65.62,92.44,-13.82,66.16,92.74,-15.73,66.88,93.10,-17.56,68.18,93.66,-18.98,69.84,94.30,-19.65,71.72,94.99,-19.78,73.23,95.47,-18.57,74.73,95.95,-17.32,76.23,96.42,-16.07,77.72,96.90,-14.81,79.22,97.37,-13.56,80.72,97.85,-12.31,82.22,98.32,-11.06,83.71,98.80,-9.80,85.20,99.27,-8.54,86.69,99.74,-7.28,88.18,100.21,-6.02,89.67,100.68,-4.75,91.15,101.15,-3.48,92.61,101.61,-2.18,93.98,102.03,-0.77,95.16,102.37,0.81,96.12,102.63,2.55,96.85,102.78,4.42,97.31,102.84,6.37],
        [100.93,101.32,9.53,100.81,101.16,11.60,100.40,100.90,13.63,99.69,100.54,15.56,98.72,100.08,17.34,97.49,99.55,18.94,96.05,98.94,20.31,94.42,98.29,21.43,92.64,97.59,22.26,90.75,96.88,22.79,88.81,96.16,23.01,86.85,95.45,22.98,84.89,94.74,22.89,82.93,94.03,22.78,80.97,93.33,22.67,79.01,92.62,22.55,77.06,91.91,22.43,75.10,91.21,22.31,73.14,90.50,22.19,71.18,89.79,22.07,69.22,89.09,21.95,67.30,88.40,21.72,66.00,88.02,20.14,65.36,87.89,18.17,64.96,87.86,16.12,64.78,87.91,14.05,64.60,87.96,11.97,64.42,88.01,9.89,64.25,88.07,7.81,64.10,88.13,5.73,63.95,88.19,3.66,63.80,88.25,1.58,63.76,88.35,-0.50,64.03,88.56,-2.56,64.53,88.86,-4.56,65.09,89.17,-6.54,65.65,89.49,-8.53,66.22,89.80,-10.51,66.78,90.12,-12.50,67.34,90.43,-14.48,67.90,90.75,-16.46,68.86,91.20,-18.23,70.24,91.78,-19.68,72.13,92.48,-19.94,73.95,93.12,-19.51,75.50,93.61,-18.22,77.06,94.11,-16.92,78.62,94.60,-15.62,80.18,95.10,-14.33,81.73,95.59,-13.03,83.29,96.09,-11.73,84.85,96.58,-10.44,86.40,97.08,-9.14,87.96,97.57,-7.84,89.51,98.06,-6.55,91.07,98.56,-5.25,92.62,99.05,-3.94,94.17,99.54,-2.64,95.71,100.03,-1.31,97.16,100.47,0.12,98.44,100.85,1.72,99.48,101.13,3.50,100.25,101.30,5.43,100.74,101.37,7.45],
        [101.78,98.97,9.40,101.67,98.82,11.47,101.25,98.55,13.49,100.56,98.19,15.41,99.58,97.74,17.19,98.36,97.21,18.79,96.93,96.61,20.16,95.30,95.95,21.28,93.53,95.26,22.11,91.65,94.55,22.65,89.72,93.83,22.87,87.76,93.12,22.90,85.81,92.41,22.88,83.85,91.70,22.79,81.90,91.00,22.70,79.95,90.29,22.60,77.99,89.59,22.50,76.04,88.88,22.40,74.08,88.17,22.30,72.13,87.47,22.19,70.18,86.76,22.08,68.22,86.06,21.97,66.95,85.68,20.39,66.38,85.59,18.40,66.10,85.60,16.34,65.92,85.65,14.27,65.75,85.70,12.20,65.57,85.75,10.12,65.40,85.81,8.05,65.25,85.87,5.98,65.11,85.93,3.90,64.96,86.00,1.83,65.04,86.14,-0.24,65.42,86.39,-2.27,65.97,86.70,-4.25,66.53,87.02,-6.23,67.09,87.33,-8.20,67.65,87.64,-10.18,68.21,87.96,-12.16,68.77,88.27,-14.14,69.33,88.59,-16.12,70.08,88.97,-18.01,71.44,89.54,-19.48,73.21,90.21,-20.00,75.12,90.90,-19.88,76.67,91.40,-18.58,78.22,91.89,-17.29,79.77,92.38,-15.99,81.32,92.87,-14.69,82.87,93.36,-13.39,84.42,93.85,-12.09,85.97,94.34,-10.79,87.51,94.84,-9.49,89.06,95.32,-8.19,90.60,95.81,-6.88,92.14,96.30,-5.57,93.66,96.78,-4.24,95.15,97.24,-2.86,96.59,97.69,-1.43,98.03,98.13,0.01,99.31,98.51,1.61,100.34,98.79,3.39,101.11,98.96,5.31,101.59,99.02,7.33],
        [102.63,96.63,9.27,102.52,96.47,11.33,102.11,96.21,13.34,101.42,95.85,15.26,100.45,95.40,17.03,99.24,94.87,18.63,97.81,94.28,20.00,96.20,93.62,21.12,94.44,92.94,21.96,92.57,92.23,22.51,90.64,91.51,22.74,88.69,90.80,22.77,86.75,90.09,22.80,84.80,89.38,22.77,82.85,88.68,22.69,80.90,87.98,22.61,78.96,87.27,22.51,77.01,86.57,22.41,75.06,85.87,22.31,73.12,85.16,22.21,71.17,84.46,22.11,69.22,83.76,22.01,67.90,83.35,20.62,67.43,83.29,18.61,67.25,83.34,16.55,67.07,83.39,14.48,66.89,83.44,12.42,66.72,83.49,10.35,66.55,83.55,8.29,66.41,83.61,6.22,66.26,83.67,4.16,66.15,83.75,2.09,66.36,83.94,0.04,66.85,84.23,-1.95,67.41,84.54,-3.93,67.97,84.86,-5.90,68.53,85.17,-7.87,69.08,85.48,-9.84,69.64,85.80,-11.81,70.20,86.11,-13.78,70.76,86.42,-15.75,71.39,86.76,-17.70,72.62,87.30,-19.25,74.29,87.94,-20.00,76.24,88.65,-20.00,77.83,89.17,-18.88,79.38,89.66,-17.59,80.92,90.15,-16.29,82.46,90.64,-14.99,84.00,91.12,-13.69,85.53,91.61,-12.39,87.07,92.10,-11.08,88.60,92.58,-9.77,90.13,93.06,-8.46,91.64,93.54,-7.13,93.15,94.02,-5.79,94.60,94.47,-4.38,96.04,94.91,-2.95,97.48,95.35,-1.53,98.91,95.79,-0.09,100.18,96.17,1.50,101.20,96.44,3.27,101.97,96.61,5.19,102.45,96.68,7.21],
        [103.49,94.28,9.14,103.38,94.13,11.19,102.97,93.87,13.20,102.28,93.51,15.11,101.32,93.06,16.88,100.12,92.54,18.47,98.70,91.94,19.84,97.10,91.30,20.96,95.35,90.61,21.81,93.49,89.90,22.36,91.57,89.19,22.60,89.63,88.48,22.64,87.69,87.78,22.66,85.75,87.07,22.69,83.81,86.36,22.67,81.87,85.66,22.59,79.93,84.96,22.50,77.99,84.26,22.42,76.05,83.56,22.33,74.11,82.86,22.23,72.17,82.16,22.13,70.23,81.46,22.03,68.87,81.03,20.85,68.57,81.03,18.81,68.39,81.08,16.75,68.22,81.13,14.70,68.04,81.18,12.64,67.86,81.24,10.58,67.71,81.29,8.52,67.56,81.35,6.47,67.41,81.42,4.41,67.40,81.52,2.35,67.74,81.76,0.33,68.29,82.07,-1.64,68.85,82.38,-3.60,69.40,82.70,-5.56,69.96,83.01,-7.53,70.51,83.32,-9.49,71.07,83.63,-11.46,71.62,83.94,-13.42,72.18,84.25,-15.38,72.76,84.57,-17.34,73.80,85.05,-19.03,75.37,85.67,-20.00,77.31,86.38,-20.00,78.98,86.94,-19.15,80.51,87.43,-17.85,82.04,87.91,-16.55,83.57,88.39,-15.25,85.10,88.88,-13.95,86.62,89.36,-12.64,88.14,89.84,-11.33,89.65,90.31,-10.01,91.16,90.79,-8.67,92.63,91.25,-7.30,94.06,91.69,-5.88,95.50,92.13,-4.46,96.93,92.58,-3.05,98.36,93.02,-1.63,99.79,93.46,-0.20,101.05,93.83,1.39,102.07,94.10,3.17,102.82,94.27,5.08,103.30,94.33,7.08],
        [104.34,91.94,9.00,104.23,91.78,11.05,103.83,91.52,13.05,103.14,91.17,14.95,102.20,90.72,16.72,101.00,90.20,18.31,99.59,89.61,19.68,98.00,88.97,20.81,96.26,88.29,21.66,94.41,87.58,22.22,92.50,86.87,22.47,90.56,86.17,22.51,88.63,85.46,22.53,86.70,84.76,22.56,84.77,84.05,22.58,82.84,83.35,22.57,80.90,82.65,22.48,78.97,81.95,22.40,77.04,81.26,22.32,75.11,80.56,22.23,73.17,79.86,22.15,71.24,79.16,22.05,69.89,78.72,21.06,69.71,78.77,19.01,69.54,78.82,16.96,69.36,78.88,14.91,69.19,78.93,12.86,69.01,78.98,10.82,68.86,79.03,8.77,68.71,79.10,6.72,68.57,79.16,4.66,68.70,79.32,2.62,69.18,79.60,0.64,69.73,79.91,-1.31,70.28,80.22,-3.27,70.84,80.53,-5.23,71.39,80.85,-7.18,71.94,81.16,-9.14,72.50,81.47,-11.10,73.05,81.78,-13.05,73.60,82.09,-15.01,74.16,82.40,-16.96,74.98,82.80,-18.79,76.44,83.40,-20.00,78.37,84.10,-20.00,80.11,84.70,-19.39,81.63,85.18,-18.09,83.15,85.66,-16.79,84.67,86.14,-15.49,86.18,86.62,-14.17,87.69,87.09,-12.86,89.19,87.57,-11.53,90.67,88.03,-10.19,92.11,88.48,-8.79,93.54,88.92,-7.38,94.97,89.36,-5.96,96.40,89.80,-4.55,97.82,90.24,-3.14,99.25,90.68,-1.73,100.67,91.12,-0.30,101.92,91.48,1.29,102.93,91.75,3.06,103.68,91.92,4.96,104.16,91.98,6.96],
        [105.19,89.59,8.87,105.09,89.44,10.91,104.69,89.18,12.90,104.01,88.83,14.80,103.07,88.39,16.56,101.89,87.87,18.14,100.49,87.28,19.52,98.91,86.64,20.64,97.18,85.97,21.50,95.34,85.27,22.07,93.44,84.56,22.33,91.52,83.86,22.37,89.60,83.16,22.40,87.67,82.46,22.43,85.75,81.76,22.45,83.83,81.05,22.48,81.90,80.35,22.47,79.98,79.66,22.38,78.06,78.96,22.30,76.13,78.27,22.22,74.21,77.57,22.13,72.29,76.88,22.05,71.03,76.47,21.19,70.85,76.52,19.15,70.68,76.57,17.11,70.50,76.62,15.08,70.33,76.67,13.04,70.15,76.72,11.00,70.01,76.78,8.96,69.86,76.84,6.92,69.78,76.92,4.87,70.08,77.15,2.87,70.63,77.45,0.92,71.18,77.76,-1.03,71.73,78.07,-2.98,72.28,78.38,-4.92,72.83,78.69,-6.87,73.38,79.00,-8.82,73.93,79.31,-10.77,74.48,79.62,-12.71,75.03,79.92,-14.66,75.58,80.23,-16.61,76.25,80.58,-18.50,77.54,81.13,-19.98,79.46,81.83,-20.00,81.25,82.46,-19.59,82.76,82.94,-18.29,84.26,83.41,-16.98,85.76,83.89,-15.67,87.26,84.36,-14.35,88.74,84.82,-13.03,90.20,85.28,-11.66,91.62,85.72,-10.25,93.04,86.15,-8.85,94.46,86.59,-7.44,95.88,87.03,-6.04,97.30,87.47,-4.63,98.72,87.91,-3.22,100.15,88.35,-1.82,101.55,88.78,-0.40,102.79,89.14,1.19,103.80,89.41,2.95,104.54,89.58,4.85,105.01,89.64,6.84],
        [106.05,87.24,8.74,105.94,87.09,10.76,105.55,86.84,12.74,104.88,86.49,14.62,103.96,86.06,16.37,102.79,85.54,17.95,101.41,84.97,19.33,99.85,84.33,20.46,98.14,83.66,21.33,96.33,82.97,21.91,94.44,82.27,22.19,92.54,81.57,22.24,90.63,80.88,22.27,88.72,80.18,22.29,86.81,79.49,22.32,84.91,78.79,22.34,83.00,78.09,22.37,81.09,77.40,22.37,79.18,76.71,22.29,77.27,76.02,22.21,75.36,75.33,22.12,73.46,74.64,22.04,72.15,74.22,21.08,71.97,74.27,19.06,71.80,74.32,17.03,71.62,74.37,15.01,71.45,74.41,12.99,71.29,74.47,10.96,71.14,74.53,8.94,71.00,74.59,6.91,71.10,74.74,4.90,71.58,75.02,2.95,72.13,75.33,1.01,72.68,75.64,-0.92,73.22,75.94,-2.85,73.77,76.25,-4.78,74.32,76.56,-6.71,74.86,76.86,-8.65,75.41,77.17,-10.58,75.95,77.48,-12.51,76.50,77.78,-14.44,77.05,78.09,-16.37,77.65,78.41,-18.28,78.79,78.92,-19.87,80.65,79.60,-20.00,82.46,80.24,-19.69,83.95,80.71,-18.39,85.43,81.18,-17.08,86.91,81.64,-15.77,88.37,82.10,-14.43,89.79,82.54,-13.05,91.20,82.98,-11.65,92.61,83.41,-10.25,94.02,83.85,-8.86,95.43,84.28,-7.46,96.84,84.72,-6.07,98.25,85.15,-4.67,99.66,85.59,-3.28,101.07,86.02,-1.88,102.46,86.45,-0.47,103.68,86.81,1.11,104.67,87.07,2.86,105.41,87.23,4.75,105.87,87.29,6.72],
        [106.90,84.90,8.61,106.80,84.75,10.62,106.41,84.50,12.58,105.76,84.16,14.45,104.84,83.73,16.19,103.69,83.22,17.76,102.33,82.65,19.13,100.79,82.02,20.27,99.11,81.36,21.15,97.31,80.68,21.75,95.45,79.98,22.05,93.56,79.29,22.11,91.67,78.60,22.13,89.77,77.91,22.16,87.88,77.22,22.18,85.99,76.53,22.21,84.10,75.84,22.24,82.20,75.15,22.26,80.31,74.46,22.27,78.42,73.77,22.20,76.52,73.09,22.11,74.63,72.40,22.03,73.29,71.98,20.96,73.09,72.01,18.96,72.92,72.06,16.95,72.75,72.11,14.94,72.57,72.16,12.94,72.42,72.22,10.93,72.27,72.28,8.92,72.19,72.36,6.91,72.55,72.60,4.95,73.09,72.90,3.03,73.63,73.21,1.11,74.17,73.51,-0.80,74.71,73.81,-2.72,75.26,74.12,-4.64,75.80,74.42,-6.55,76.34,74.73,-8.47,76.88,75.03,-10.39,77.43,75.34,-12.30,77.97,75.64,-14.22,78.51,75.94,-16.14,79.07,76.25,-18.05,80.04,76.70,-19.72,81.84,77.37,-20.00,83.65,78.02,-19.76,85.12,78.48,-18.46,86.58,78.94,-17.15,88.00,79.38,-15.79,89.40,79.81,-14.40,90.80,80.24,-13.02,92.20,80.67,-11.64,93.59,81.11,-10.25,94.99,81.54,-8.87,96.39,81.97,-7.48,97.79,82.40,-6.10,99.19,82.83,-4.71,100.59,83.27,-3.33,101.99,83.70,-1.95,103.37,84.12,-0.54,104.57,84.47,1.03,105.55,84.73,2.78,106.27,84.89,4.65,106.73,84.95,6.61],
        [107.76,82.55,8.48,107.65,82.40,10.47,107.27,82.16,12.41,106.63,81.82,14.27,105.73,81.40,16.01,104.60,80.90,17.57,103.26,80.33,18.94,101.74,79.72,20.08,100.08,79.06,20.97,98.30,78.38,21.58,96.46,77.69,21.91,94.58,77.00,21.97,92.70,76.32,22.00,90.83,75.63,22.03,88.95,74.95,22.05,87.07,74.27,22.08,85.20,73.58,22.10,83.32,72.90,22.13,81.44,72.21,22.15,79.56,71.53,22.17,77.69,70.85,22.10,75.81,70.17,22.02,74.55,69.78,20.80,74.21,69.76,18.86,74.04,69.81,16.87,73.87,69.86,14.88,73.70,69.91,12.89,73.55,69.97,10.90,73.41,70.03,8.91,73.54,70.18,6.93,74.05,70.48,5.02,74.59,70.78,3.12,75.13,71.08,1.22,75.67,71.38,-0.68,76.21,71.69,-2.59,76.74,71.99,-4.49,77.28,72.29,-6.39,77.82,72.59,-8.29,78.36,72.89,-10.19,78.89,73.19,-12.09,79.43,73.50,-13.99,79.97,73.80,-15.89,80.51,74.10,-17.79,81.31,74.49,-19.57,83.02,75.14,-20.00,84.83,75.79,-19.81,86.26,76.23,-18.49,87.65,76.66,-17.11,89.04,77.09,-15.74,90.42,77.52,-14.37,91.81,77.95,-12.99,93.20,78.37,-11.62,94.59,78.80,-10.25,95.97,79.23,-8.87,97.36,79.66,-7.50,98.75,80.09,-6.13,100.13,80.52,-4.75,101.52,80.94,-3.38,102.91,81.37,-2.01,104.27,81.79,-0.61,105.46,82.14,0.96,106.42,82.39,2.69,107.13,82.54,4.55,107.58,82.60,6.49],
        [108.61,80.21,8.35,108.51,80.06,10.32,108.14,79.82,12.25,107.50,79.48,14.09,106.62,79.07,15.82,105.51,78.57,17.38,104.19,78.02,18.74,102.69,77.41,19.89,101.05,76.76,20.78,99.30,76.09,21.41,97.48,75.41,21.76,95.62,74.73,21.84,93.76,74.05,21.87,91.90,73.37,21.89,90.04,72.69,21.92,88.18,72.01,21.94,86.32,71.33,21.97,84.46,70.65,21.99,82.60,69.98,22.02,80.74,69.30,22.04,78.88,68.62,22.07,77.01,67.94,22.01,75.84,67.59,20.67,75.33,67.51,18.80,75.16,67.56,16.83,74.99,67.61,14.86,74.83,67.66,12.88,74.69,67.72,10.91,74.61,67.80,8.93,75.02,68.05,7.02,75.55,68.35,5.13,76.08,68.65,3.25,76.62,68.95,1.37,77.15,69.25,-0.52,77.68,69.55,-2.40,78.21,69.85,-4.28,78.75,70.15,-6.17,79.28,70.45,-8.05,79.81,70.74,-9.94,80.35,71.04,-11.82,80.88,71.34,-13.70,81.41,71.64,-15.59,81.95,71.94,-17.47,82.60,72.28,-19.30,84.14,72.88,-19.95,85.95,73.53,-19.75,87.34,73.95,-18.39,88.71,74.38,-17.03,90.09,74.80,-15.67,91.46,75.23,-14.31,92.84,75.65,-12.95,94.21,76.08,-11.59,95.59,76.50,-10.23,96.96,76.93,-8.87,98.34,77.35,-7.51,99.71,77.78,-6.15,101.09,78.20,-4.79,102.46,78.63,-3.43,103.84,79.05,-2.07,105.19,79.46,-0.68,106.35,79.80,0.89,107.30,80.05,2.61,108.00,80.20,4.45,108.44,80.25,6.38],
        [109.46,77.86,8.22,109.37,77.72,10.17,109.00,77.48,12.07,108.39,77.15,13.90,107.52,76.74,15.60,106.44,76.26,17.15,105.15,75.72,18.52,103.69,75.12,19.66,102.08,74.48,20.57,100.36,73.82,21.22,98.56,73.15,21.60,96.73,72.47,21.71,94.89,71.80,21.73,93.06,71.13,21.76,91.22,70.46,21.78,89.38,69.79,21.81,87.55,69.12,21.83,85.71,68.45,21.86,83.87,67.78,21.88,82.03,67.11,21.91,80.20,66.44,21.93,78.36,65.77,21.96,77.19,65.42,20.68,76.47,65.25,18.90,76.30,65.30,16.95,76.13,65.35,15.00,75.97,65.40,13.05,75.83,65.46,11.10,75.95,65.61,9.17,76.47,65.90,7.31,76.99,66.20,5.44,77.52,66.49,3.58,78.04,66.79,1.72,78.57,67.08,-0.14,79.10,67.38,-2.00,79.62,67.67,-3.86,80.15,67.97,-5.72,80.68,68.26,-7.58,81.20,68.56,-9.44,81.73,68.85,-11.30,82.25,69.15,-13.16,82.78,69.44,-15.02,83.31,69.74,-16.88,83.86,70.04,-18.73,85.15,70.56,-19.55,86.98,71.22,-19.37,88.38,71.65,-18.08,89.78,72.09,-16.78,91.17,72.52,-15.48,92.56,72.96,-14.17,93.93,73.38,-12.84,95.29,73.80,-11.50,96.65,74.22,-10.16,98.01,74.64,-8.81,99.36,75.06,-7.47,100.72,75.48,-6.12,102.08,75.90,-4.78,103.44,76.32,-3.44,104.80,76.74,-2.09,106.12,77.14,-0.71,107.26,77.47,0.84,108.18,77.71,2.54,108.87,77.86,4.37,109.30,77.91,6.27],
        [110.32,75.51,8.09,110.22,75.37,10.00,109.88,75.14,11.87,109.28,74.83,13.66,108.45,74.43,15.34,107.41,73.96,16.88,106.17,73.44,18.24,104.75,72.86,19.39,103.19,72.24,20.32,101.52,71.59,21.00,99.77,70.93,21.42,97.98,70.27,21.57,96.18,69.61,21.59,94.38,68.96,21.62,92.58,68.30,21.64,90.78,67.64,21.67,88.97,66.99,21.69,87.17,66.33,21.70,85.37,65.68,21.70,83.57,65.02,21.70,81.77,64.36,21.70,79.97,63.71,21.69,78.62,63.27,20.84,77.75,63.04,19.15,77.46,63.04,17.28,77.29,63.09,15.37,77.14,63.14,13.46,77.00,63.20,11.54,77.32,63.42,9.68,77.84,63.71,7.85,78.36,64.00,6.03,78.87,64.28,4.21,79.39,64.57,2.38,79.90,64.86,0.56,80.42,65.15,-1.26,80.93,65.44,-3.09,81.45,65.73,-4.91,81.97,66.02,-6.74,82.48,66.31,-8.56,83.00,66.60,-10.38,83.51,66.89,-12.21,84.03,67.18,-14.03,84.55,67.47,-15.86,85.06,67.76,-17.68,86.09,68.19,-18.69,87.87,68.82,-18.37,89.33,69.29,-17.26,90.73,69.73,-16.04,92.14,70.18,-14.81,93.54,70.62,-13.58,94.94,71.06,-12.34,96.33,71.49,-11.10,97.72,71.93,-9.85,99.10,72.36,-8.59,100.47,72.79,-7.32,101.81,73.21,-6.02,103.15,73.62,-4.70,104.48,74.03,-3.38,105.81,74.44,-2.07,107.09,74.83,-0.70,108.19,75.15,0.84,109.08,75.38,2.52,109.74,75.51,4.31,110.16,75.56,6.18],
        [109.69,72.63,7.94,109.60,72.50,9.74,109.28,72.28,11.51,108.74,71.99,13.20,107.97,71.62,14.80,106.99,71.18,16.26,105.84,70.69,17.55,104.51,70.15,18.66,103.06,69.57,19.56,101.50,68.96,20.24,99.87,68.34,20.72,98.20,67.72,21.02,96.51,67.09,21.15,94.81,66.47,21.19,93.11,65.86,21.20,91.41,65.24,21.20,89.71,64.62,21.20,88.01,64.00,21.19,86.31,63.38,21.18,84.61,62.76,21.17,82.91,62.15,21.16,81.21,61.53,21.15,79.82,61.06,20.55,79.00,60.84,18.96,78.58,60.79,17.24,78.42,60.83,15.44,78.28,60.88,13.63,78.26,60.97,11.83,78.73,61.24,10.11,79.22,61.51,8.39,79.70,61.79,6.67,80.19,62.06,4.95,80.68,62.33,3.23,81.16,62.61,1.51,81.65,62.88,-0.21,82.14,63.15,-1.93,82.62,63.43,-3.65,83.11,63.70,-5.38,83.60,63.97,-7.10,84.08,64.24,-8.82,84.57,64.52,-10.54,85.06,64.79,-12.26,85.54,65.06,-13.98,86.04,65.34,-15.69,86.68,65.66,-17.18,88.33,66.23,-16.69,89.82,66.73,-15.88,91.15,67.15,-14.72,92.48,67.57,-13.57,93.81,67.98,-12.42,95.14,68.40,-11.26,96.46,68.82,-10.10,97.77,69.23,-8.93,99.08,69.64,-7.75,100.36,70.04,-6.53,101.64,70.44,-5.32,102.93,70.84,-4.11,104.21,71.24,-2.90,105.48,71.63,-1.68,106.68,72.00,-0.37,107.70,72.29,1.09,108.53,72.50,2.68,109.14,72.63,4.37,109.53,72.67,6.14],
        [105.88,68.59,7.75,105.83,68.48,9.28,105.57,68.31,10.79,105.12,68.06,12.24,104.48,67.75,13.61,103.66,67.39,14.86,102.68,66.97,15.97,101.56,66.50,16.92,100.32,66.01,17.70,99.01,65.50,18.32,97.65,64.98,18.81,96.26,64.45,19.21,94.85,63.92,19.55,93.43,63.38,19.80,91.99,62.85,19.96,90.55,62.32,20.07,89.10,61.79,20.14,87.65,61.26,20.19,86.21,60.73,20.23,84.76,60.20,20.25,83.31,59.68,20.28,81.86,59.15,20.29,80.71,58.76,19.66,80.01,58.58,18.30,79.67,58.54,16.82,79.54,58.58,15.29,79.43,58.63,13.75,79.65,58.79,12.25,80.07,59.03,10.79,80.48,59.26,9.32,80.90,59.49,7.85,81.31,59.72,6.39,81.73,59.96,4.92,82.14,60.19,3.46,82.56,60.42,1.99,82.97,60.65,0.52,83.39,60.89,-0.94,83.80,61.12,-2.41,84.21,61.35,-3.87,84.63,61.58,-5.34,85.05,61.82,-6.80,85.49,62.06,-8.26,85.96,62.31,-9.71,86.47,62.58,-11.13,87.06,62.87,-12.53,87.83,63.20,-13.45,89.10,63.62,-12.68,90.36,64.04,-11.90,91.46,64.38,-10.87,92.55,64.72,-9.83,93.64,65.06,-8.80,94.73,65.40,-7.77,95.83,65.74,-6.74,96.92,66.08,-5.71,98.01,66.42,-4.67,99.10,66.75,-3.63,100.18,67.09,-2.59,101.25,67.42,-1.53,102.31,67.75,-0.46,103.32,68.05,0.66,104.19,68.30,1.91,104.89,68.48,3.27,105.41,68.59,4.71,105.74,68.62,6.22],
        [102.08,64.55,7.56,102.05,64.47,8.79,101.86,64.34,10.01,101.52,64.15,11.19,101.05,63.91,12.31,100.42,63.63,13.33,99.66,63.30,14.25,98.78,62.93,15.04,97.81,62.54,15.70,96.78,62.14,16.26,95.71,61.72,16.72,94.61,61.30,17.11,93.50,60.88,17.46,92.38,60.45,17.77,91.25,60.02,18.05,90.11,59.60,18.31,88.97,59.17,18.52,87.82,58.74,18.69,86.66,58.31,18.81,85.50,57.89,18.91,84.34,57.46,18.99,83.18,57.03,19.06,82.02,56.61,19.11,81.28,56.39,18.16,80.82,56.29,17.03,80.71,56.32,15.80,80.62,56.35,14.56,80.87,56.51,13.37,81.20,56.70,12.19,81.53,56.88,11.01,81.87,57.07,9.83,82.20,57.26,8.65,82.53,57.44,7.47,82.87,57.63,6.30,83.20,57.82,5.12,83.53,58.00,3.94,83.87,58.19,2.76,84.21,58.38,1.58,84.57,58.58,0.41,84.97,58.79,-0.74,85.44,59.02,-1.86,85.92,59.26,-2.98,86.40,59.50,-4.10,86.88,59.73,-5.21,87.37,59.97,-6.33,87.85,60.21,-7.45,88.39,60.46,-8.44,89.47,60.83,-7.96,90.50,61.17,-7.37,91.51,61.50,-6.73,92.38,61.77,-5.89,93.26,62.04,-5.06,94.13,62.31,-4.22,95.00,62.58,-3.38,95.86,62.85,-2.53,96.72,63.12,-1.68,97.58,63.38,-0.83,98.43,63.65,0.03,99.28,63.90,0.90,100.07,64.14,1.83,100.74,64.33,2.85,101.29,64.47,3.95,101.70,64.55,5.12,101.96,64.58,6.33],
        [98.28,60.52,7.37,98.27,60.46,8.30,98.15,60.37,9.22,97.94,60.24,10.12,97.62,60.07,10.98,97.18,59.87,11.78,96.65,59.64,12.51,96.01,59.37,13.14,95.31,59.08,13.68,94.55,58.78,14.14,93.76,58.47,14.54,92.96,58.16,14.89,92.14,57.84,15.20,91.31,57.53,15.50,90.47,57.21,15.76,89.63,56.89,16.02,88.78,56.56,16.25,87.94,56.24,16.47,87.08,55.92,16.68,86.23,55.60,16.88,85.37,55.28,17.07,84.51,54.95,17.24,83.65,54.63,17.38,82.78,54.31,17.50,82.08,54.08,17.10,81.87,54.05,16.21,81.85,54.10,15.28,82.11,54.24,14.40,82.36,54.38,13.51,82.61,54.52,12.62,82.86,54.66,11.73,83.11,54.80,10.84,83.37,54.95,9.96,83.69,55.11,9.09,84.05,55.29,8.25,84.42,55.47,7.41,84.78,55.65,6.57,85.14,55.83,5.72,85.51,56.01,4.88,85.87,56.19,4.04,86.23,56.36,3.20,86.60,56.54,2.36,86.96,56.72,1.51,87.33,56.90,0.67,87.69,57.08,-0.17,88.05,57.26,-1.01,88.42,57.44,-1.85,88.78,57.62,-2.69,89.26,57.83,-3.28,90.08,58.10,-2.93,90.89,58.38,-2.57,91.68,58.65,-2.15,92.41,58.88,-1.62,93.06,59.08,-0.98,93.70,59.28,-0.33,94.35,59.48,0.31,94.98,59.67,0.97,95.62,59.87,1.62,96.25,60.06,2.28,96.81,60.22,3.02,97.29,60.35,3.80,97.69,60.45,4.64,97.98,60.51,5.53,98.17,60.53,6.44],
        [94.47,56.48,7.18,94.47,56.45,7.74,94.45,56.41,8.29,94.36,56.34,8.84,94.23,56.27,9.38,94.04,56.17,9.90,93.81,56.06,10.40,93.52,55.92,10.86,93.17,55.77,11.27,92.77,55.61,11.62,92.35,55.44,11.95,91.89,55.26,12.22,91.43,55.07,12.48,90.96,54.89,12.72,90.47,54.70,12.94,89.99,54.51,13.14,89.49,54.32,13.31,88.99,54.13,13.48,88.49,53.94,13.64,87.98,53.75,13.79,87.48,53.55,13.95,86.98,53.36,14.11,86.47,53.17,14.26,85.97,52.98,14.40,85.46,52.79,14.55,84.98,52.61,14.52,84.89,52.61,13.98,84.95,52.66,13.43,85.11,52.75,12.90,85.29,52.85,12.38,85.49,52.95,11.86,85.70,53.05,11.35,85.91,53.16,10.85,86.13,53.27,10.34,86.35,53.37,9.84,86.57,53.48,9.33,86.79,53.59,8.83,87.01,53.70,8.32,87.22,53.80,7.81,87.44,53.91,7.31,87.66,54.02,6.80,87.88,54.13,6.30,88.10,54.23,5.79,88.32,54.34,5.29,88.53,54.45,4.78,88.75,54.56,4.28,88.97,54.66,3.77,89.19,54.77,3.27,89.41,54.88,2.76,89.63,54.99,2.26,89.87,55.10,1.78,90.36,55.27,1.98,90.85,55.44,2.19,91.35,55.61,2.41,91.84,55.77,2.62,92.33,55.94,2.83,92.75,56.07,3.16,93.11,56.18,3.58,93.43,56.27,4.03,93.70,56.35,4.51,93.96,56.41,5.01,94.14,56.45,5.53,94.31,56.48,6.07,94.40,56.48,6.62],
        [92.75,54.65,7.09,92.75,54.63,7.37,92.75,54.62,7.65,92.75,54.60,7.93,92.72,54.57,8.21,92.67,54.54,8.49,92.62,54.51,8.76,92.49,54.44,8.99,92.30,54.37,9.19,92.11,54.29,9.38,91.92,54.21,9.57,91.73,54.13,9.77,91.52,54.04,9.93,91.30,53.95,10.09,91.09,53.87,10.24,90.87,53.78,10.39,90.65,53.69,10.54,90.43,53.60,10.70,90.19,53.51,10.83,89.96,53.42,10.95,89.72,53.32,11.07,89.46,53.23,11.04,89.20,53.14,11.00,88.95,53.05,10.91,88.72,52.97,10.77,88.52,52.91,10.58,88.41,52.89,10.33,88.35,52.88,10.06,88.35,52.89,9.77,88.35,52.91,9.49,88.38,52.94,9.22,88.43,52.97,8.94,88.49,53.01,8.67,88.55,53.05,8.40,88.64,53.09,8.13,88.72,53.14,7.87,88.81,53.18,7.61,88.90,53.23,7.34,88.99,53.28,7.08,89.08,53.33,6.82,89.18,53.38,6.56,89.28,53.43,6.30,89.39,53.48,6.05,89.49,53.53,5.79,89.59,53.58,5.53,89.69,53.64,5.28,89.80,53.69,5.02,89.90,53.74,4.76,90.01,53.79,4.51,90.12,53.85,4.25,90.34,53.92,4.33,90.57,54.00,4.47,90.79,54.07,4.62,91.01,54.14,4.78,91.24,54.22,4.94,91.46,54.29,5.09,91.68,54.36,5.25,91.88,54.42,5.45,92.07,54.48,5.64,92.26,54.54,5.84,92.45,54.60,6.04,92.62,54.64,6.26,92.66,54.65,6.53,92.71,54.65,6.81]],
    "28" : [
        [55.23,112.47,-7.59,55.23,112.43,-7.03,55.24,112.40,-6.47,55.24,112.37,-5.90,55.25,112.34,-5.34,55.25,112.31,-4.78,55.25,112.28,-4.21,55.26,112.25,-3.65,55.26,112.22,-3.09,55.27,112.18,-2.52,55.27,112.15,-1.96,55.27,112.12,-1.40,55.28,112.09,-0.84,55.28,112.06,-0.27,55.28,112.03,0.29,55.29,112.00,0.85,55.29,111.96,1.42,55.29,111.93,1.98,55.30,111.90,2.54,55.30,111.87,3.11,55.30,111.83,3.67,55.28,111.79,4.23,55.25,111.74,4.79,55.21,111.69,5.35,55.03,111.62,4.88,54.88,111.57,4.34,54.73,111.52,3.79,54.64,111.51,3.24,54.56,111.50,2.68,54.48,111.49,2.12,54.40,111.48,1.56,54.32,111.47,1.01,54.24,111.46,0.45,54.16,111.45,-0.11,54.08,111.44,-0.67,54.00,111.43,-1.23,53.92,111.42,-1.79,53.84,111.41,-2.34,53.76,111.41,-2.90,53.68,111.40,-3.46,53.60,111.39,-4.02,53.52,111.38,-4.58,53.45,111.37,-5.14,53.37,111.36,-5.69,53.29,111.36,-6.25,53.22,111.35,-6.81,53.14,111.34,-7.37,53.07,111.34,-7.93,53.01,111.34,-8.49,52.99,111.36,-9.05,53.06,111.43,-9.61,53.20,111.54,-10.14,53.38,111.66,-10.66,53.62,111.82,-11.14,53.91,112.00,-11.59,54.22,112.19,-12.02,54.49,112.34,-12.02,54.64,112.38,-11.48,54.78,112.42,-10.94,54.90,112.46,-10.39,55.00,112.48,-9.83,55.10,112.50,-9.28,55.16,112.50,-8.72,55.21,112.49,-8.16],
        [58.96,111.52,-6.03,58.96,111.47,-5.14,58.97,111.42,-4.25,58.98,111.37,-3.37,58.98,111.32,-2.48,58.99,111.27,-1.60,58.99,111.22,-0.71,59.00,111.17,0.17,59.01,111.12,1.06,59.01,111.07,1.95,59.02,111.02,2.83,59.02,110.97,3.72,59.01,110.92,4.60,58.97,110.84,5.48,58.88,110.74,6.36,58.77,110.63,7.23,58.62,110.50,8.10,58.40,110.34,8.94,58.11,110.13,9.76,57.76,109.90,10.53,57.32,109.62,11.25,56.72,109.34,10.66,56.14,109.07,10.05,55.59,108.81,9.40,55.07,108.58,8.72,54.59,108.37,8.00,54.29,108.25,7.20,54.14,108.23,6.32,54.02,108.22,5.44,53.93,108.22,4.56,53.86,108.23,3.68,53.79,108.25,2.79,53.73,108.27,1.91,53.67,108.29,1.02,53.61,108.31,0.14,53.55,108.33,-0.75,53.49,108.35,-1.63,53.43,108.37,-2.52,53.37,108.39,-3.40,53.31,108.41,-4.29,53.25,108.43,-5.17,53.19,108.45,-6.05,53.13,108.47,-6.94,53.07,108.50,-7.82,53.04,108.54,-8.71,53.06,108.60,-9.60,53.11,108.67,-10.48,53.19,108.77,-11.36,53.31,108.88,-12.23,53.45,109.01,-13.09,54.06,109.37,-13.61,54.72,109.74,-14.07,55.41,110.14,-14.46,56.12,110.53,-14.82,56.64,110.79,-14.47,57.01,110.94,-13.68,57.36,111.08,-12.87,57.68,111.20,-12.06,57.97,111.31,-11.23,58.24,111.40,-10.38,58.47,111.47,-9.53,58.66,111.52,-8.67,58.81,111.55,-7.79,58.91,111.55,-6.91],
        [62.68,110.57,-4.46,62.69,110.51,-3.40,62.70,110.46,-2.34,62.71,110.40,-1.28,62.71,110.34,-0.23,62.72,110.28,0.83,62.73,110.22,1.89,62.73,110.16,2.95,62.74,110.10,4.01,62.74,110.04,5.07,62.68,109.94,6.12,62.58,109.83,7.17,62.41,109.68,8.21,62.19,109.50,9.23,61.87,109.27,10.21,61.45,108.99,11.14,60.92,108.66,12.00,60.32,108.29,12.79,59.64,107.89,13.50,58.91,107.47,14.07,58.09,107.06,13.53,57.29,106.67,12.96,56.51,106.29,12.35,56.12,106.14,11.43,55.90,106.09,10.40,55.69,106.04,9.36,55.49,105.99,8.32,55.33,105.96,7.27,55.20,105.96,6.22,55.09,105.97,5.16,55.01,105.99,4.11,54.94,106.01,3.05,54.87,106.03,1.99,54.79,106.06,0.93,54.72,106.08,-0.12,54.65,106.11,-1.18,54.58,106.13,-2.24,54.50,106.15,-3.30,54.43,106.18,-4.35,54.36,106.20,-5.41,54.29,106.23,-6.47,54.24,106.26,-7.53,54.25,106.34,-8.58,54.32,106.43,-9.64,54.43,106.55,-10.69,54.59,106.70,-11.72,54.78,106.86,-12.75,55.01,107.05,-13.77,55.27,107.24,-14.78,55.80,107.57,-15.51,56.69,108.06,-15.81,57.59,108.55,-16.07,58.46,109.02,-16.15,59.02,109.27,-15.28,59.55,109.50,-14.40,60.05,109.71,-13.48,60.52,109.90,-12.55,60.95,110.08,-11.60,61.35,110.23,-10.63,61.70,110.36,-9.64,62.01,110.46,-8.63,62.28,110.54,-7.61,62.48,110.59,-6.57,62.62,110.60,-5.52],
        [66.41,109.63,-2.89,66.42,109.56,-1.70,66.43,109.49,-0.51,66.44,109.43,0.68,66.44,109.36,1.87,66.45,109.29,3.06,66.46,109.23,4.25,66.46,109.16,5.44,66.41,109.06,6.62,66.29,108.93,7.80,66.10,108.76,8.97,65.81,108.54,10.10,65.40,108.26,11.18,64.88,107.92,12.20,64.24,107.52,13.12,63.52,107.09,13.96,62.72,106.62,14.71,61.86,106.12,15.36,60.94,105.60,15.92,59.97,105.09,15.91,59.00,104.60,15.41,58.04,104.12,14.89,57.62,103.96,13.85,57.36,103.89,12.69,57.11,103.83,11.53,56.88,103.77,10.36,56.66,103.72,9.19,56.48,103.70,8.01,56.35,103.70,6.83,56.25,103.72,5.64,56.16,103.74,4.45,56.08,103.77,3.26,56.00,103.80,2.08,55.92,103.82,0.89,55.84,103.85,-0.30,55.75,103.88,-1.49,55.67,103.90,-2.68,55.59,103.93,-3.87,55.51,103.96,-5.06,55.44,103.99,-6.25,55.44,104.06,-7.44,55.51,104.17,-8.62,55.65,104.32,-9.80,55.84,104.49,-10.96,56.08,104.68,-12.11,56.36,104.90,-13.25,56.65,105.12,-14.39,56.96,105.35,-15.51,57.47,105.68,-16.50,58.51,106.24,-16.68,59.55,106.81,-16.80,60.52,107.31,-16.68,61.23,107.64,-15.78,61.91,107.94,-14.86,62.56,108.23,-13.90,63.17,108.50,-12.91,63.75,108.75,-11.90,64.28,108.97,-10.85,64.77,109.16,-9.78,65.20,109.33,-8.69,65.59,109.47,-7.57,65.91,109.57,-6.42,66.16,109.64,-5.26,66.33,109.66,-4.08],
        [70.14,108.68,-1.33,70.15,108.61,-0.02,70.16,108.53,1.28,70.16,108.46,2.58,70.17,108.39,3.89,70.18,108.32,5.19,70.17,108.23,6.49,70.07,108.10,7.79,69.90,107.93,9.07,69.62,107.71,10.33,69.21,107.42,11.53,68.66,107.06,12.66,67.98,106.64,13.69,67.20,106.17,14.62,66.32,105.65,15.44,65.37,105.11,16.15,64.36,104.54,16.75,63.31,103.95,17.26,62.23,103.35,17.67,61.13,102.78,17.42,60.03,102.22,16.98,59.21,101.83,16.21,58.85,101.71,14.97,58.55,101.62,13.70,58.28,101.56,12.42,58.02,101.49,11.14,57.78,101.45,9.86,57.62,101.44,8.57,57.51,101.45,7.26,57.40,101.47,5.96,57.30,101.50,4.66,57.21,101.53,3.36,57.12,101.56,2.06,57.04,101.59,0.75,56.95,101.62,-0.55,56.86,101.65,-1.85,56.77,101.68,-3.15,56.68,101.71,-4.46,56.63,101.76,-5.76,56.67,101.86,-7.06,56.79,102.00,-8.35,56.99,102.18,-9.63,57.25,102.39,-10.89,57.56,102.63,-12.14,57.87,102.87,-13.38,58.19,103.12,-14.62,58.54,103.38,-15.85,59.15,103.76,-16.93,60.26,104.37,-17.15,61.40,104.98,-17.28,62.46,105.53,-17.12,63.25,105.90,-16.15,64.04,106.26,-15.17,64.83,106.62,-14.19,65.60,106.97,-13.20,66.32,107.29,-12.16,67.00,107.59,-11.08,67.63,107.86,-9.97,68.20,108.10,-8.82,68.72,108.30,-7.64,69.17,108.47,-6.43,69.55,108.60,-5.19,69.85,108.68,-3.92,70.05,108.71,-2.63],
        [73.86,107.73,0.24,73.87,107.65,1.65,73.88,107.58,3.06,73.89,107.50,4.47,73.90,107.42,5.88,73.88,107.32,7.29,73.76,107.18,8.68,73.54,106.98,10.06,73.18,106.71,11.40,72.66,106.36,12.67,72.00,105.93,13.84,71.20,105.45,14.90,70.30,104.91,15.84,69.30,104.33,16.65,68.23,103.72,17.33,67.10,103.09,17.90,65.93,102.44,18.36,64.74,101.78,18.72,63.52,101.12,19.00,62.30,100.49,18.66,61.09,99.87,18.28,60.42,99.58,17.16,60.01,99.44,15.81,59.68,99.35,14.45,59.39,99.27,13.07,59.11,99.21,11.68,58.89,99.18,10.29,58.76,99.19,8.88,58.64,99.21,7.48,58.53,99.23,6.07,58.43,99.26,4.66,58.33,99.30,3.25,58.24,99.33,1.84,58.14,99.36,0.44,58.04,99.39,-0.97,57.95,99.43,-2.38,57.85,99.46,-3.79,57.82,99.53,-5.20,57.91,99.66,-6.60,58.09,99.83,-7.99,58.35,100.05,-9.36,58.68,100.31,-10.71,59.02,100.57,-12.06,59.36,100.83,-13.40,59.71,101.10,-14.74,60.09,101.38,-16.07,60.77,101.81,-17.22,61.90,102.43,-17.61,63.14,103.10,-17.75,64.29,103.70,-17.60,65.17,104.10,-16.56,66.03,104.50,-15.52,66.90,104.90,-14.48,67.76,105.30,-13.43,68.61,105.69,-12.38,69.45,106.07,-11.31,70.23,106.42,-10.19,70.96,106.74,-9.02,71.63,107.02,-7.81,72.23,107.27,-6.56,72.75,107.47,-5.26,73.19,107.62,-3.93,73.53,107.72,-2.56,73.76,107.76,-1.17],
        [77.59,106.78,1.80,77.60,106.70,3.31,77.61,106.62,4.83,77.62,106.53,6.34,77.60,106.43,7.85,77.47,106.27,9.34,77.21,106.05,10.82,76.79,105.74,12.24,76.20,105.35,13.58,75.46,104.88,14.80,74.57,104.34,15.90,73.56,103.75,16.87,72.46,103.11,17.68,71.28,102.45,18.35,70.04,101.76,18.89,68.77,101.06,19.31,67.47,100.35,19.63,66.16,99.64,19.85,64.83,98.93,19.96,63.51,98.24,19.67,62.26,97.60,19.24,61.59,97.32,17.92,61.12,97.16,16.49,60.77,97.06,15.02,60.46,96.98,13.54,60.19,96.92,12.05,60.01,96.92,10.55,59.89,96.95,9.04,59.77,96.97,7.53,59.65,97.00,6.03,59.55,97.03,4.52,59.44,97.07,3.01,59.34,97.10,1.50,59.24,97.13,-0.01,59.13,97.17,-1.52,59.04,97.21,-3.03,59.03,97.29,-4.54,59.16,97.45,-6.04,59.39,97.66,-7.52,59.72,97.92,-8.98,60.08,98.20,-10.42,60.45,98.48,-11.86,60.82,98.77,-13.30,61.19,99.05,-14.74,61.59,99.34,-16.17,62.30,99.80,-17.40,63.41,100.43,-18.05,64.74,101.14,-18.19,66.01,101.82,-18.15,66.96,102.25,-17.05,67.90,102.69,-15.94,68.84,103.12,-14.84,69.77,103.55,-13.73,70.70,103.98,-12.61,71.62,104.40,-11.49,72.54,104.82,-10.36,73.44,105.23,-9.22,74.28,105.61,-8.01,75.04,105.94,-6.75,75.73,106.23,-5.44,76.33,106.46,-4.07,76.83,106.64,-2.65,77.22,106.76,-1.19,77.48,106.82,0.30],
        [81.32,105.84,3.37,81.33,105.75,4.98,81.34,105.66,6.59,81.33,105.56,8.19,81.20,105.40,9.79,80.93,105.16,11.36,80.48,104.83,12.87,79.84,104.41,14.29,79.03,103.90,15.58,78.07,103.32,16.74,76.98,102.68,17.74,75.79,102.00,18.58,74.52,101.28,19.26,73.19,100.54,19.78,71.82,99.79,20.17,70.42,99.03,20.44,69.01,98.27,20.62,67.60,97.51,20.73,66.18,96.76,20.72,64.76,96.02,20.49,63.50,95.38,19.91,62.71,95.04,18.55,62.20,94.86,17.04,61.84,94.76,15.47,61.52,94.69,13.89,61.29,94.66,12.30,61.15,94.68,10.70,61.02,94.70,9.09,60.88,94.73,7.49,60.77,94.76,5.88,60.66,94.80,4.27,60.55,94.84,2.67,60.44,94.87,1.06,60.33,94.91,-0.55,60.22,94.95,-2.16,60.24,95.05,-3.76,60.41,95.24,-5.35,60.69,95.48,-6.92,61.07,95.77,-8.46,61.46,96.07,-9.99,61.85,96.37,-11.52,62.25,96.67,-13.06,62.64,96.97,-14.59,63.04,97.28,-16.12,63.73,97.73,-17.49,64.79,98.35,-18.48,66.20,99.11,-18.62,67.61,99.87,-18.75,68.63,100.34,-17.60,69.64,100.80,-16.43,70.65,101.27,-15.27,71.65,101.74,-14.10,72.66,102.20,-12.93,73.65,102.66,-11.75,74.65,103.12,-10.57,75.63,103.57,-9.38,76.61,104.02,-8.18,77.56,104.45,-6.95,78.43,104.84,-5.65,79.22,105.18,-4.29,79.90,105.45,-2.85,80.46,105.67,-1.36,80.90,105.81,0.18,81.19,105.87,1.77],
        [85.04,104.89,4.94,85.06,104.80,6.64,85.06,104.70,8.34,84.96,104.54,10.04,84.69,104.30,11.70,84.23,103.96,13.31,83.57,103.52,14.82,82.72,102.99,16.20,81.71,102.38,17.43,80.56,101.70,18.49,79.29,100.98,19.37,77.94,100.22,20.07,76.51,99.43,20.58,75.05,98.63,20.95,73.57,97.83,21.18,72.07,97.02,21.31,70.56,96.22,21.36,69.06,95.42,21.36,67.55,94.62,21.30,66.05,93.83,21.12,64.70,93.15,20.49,63.80,92.75,19.11,63.26,92.56,17.50,62.89,92.47,15.84,62.58,92.40,14.16,62.41,92.41,12.47,62.27,92.44,10.77,62.13,92.47,9.07,62.00,92.49,7.37,61.88,92.53,5.66,61.76,92.57,3.96,61.65,92.61,2.26,61.53,92.65,0.56,61.42,92.69,-1.14,61.44,92.80,-2.84,61.64,93.01,-4.52,61.97,93.28,-6.17,62.38,93.60,-7.80,62.80,93.92,-9.42,63.21,94.23,-11.05,63.63,94.55,-12.67,64.05,94.87,-14.30,64.46,95.19,-15.92,65.08,95.60,-17.45,66.11,96.23,-18.65,67.51,96.99,-19.04,69.01,97.80,-19.18,70.17,98.36,-18.23,71.25,98.86,-17.00,72.32,99.36,-15.77,73.40,99.86,-14.55,74.47,100.35,-13.32,75.54,100.85,-12.08,76.61,101.34,-10.85,77.67,101.83,-9.61,78.73,102.32,-8.36,79.78,102.81,-7.10,80.81,103.28,-5.83,81.80,103.73,-4.52,82.69,104.12,-3.11,83.45,104.44,-1.62,84.09,104.68,-0.06,84.58,104.85,1.56,84.91,104.92,3.24],
        [88.77,103.94,6.50,88.78,103.85,8.30,88.73,103.71,10.09,88.49,103.48,11.86,88.04,103.14,13.57,87.38,102.69,15.18,86.52,102.15,16.66,85.48,101.51,17.98,84.28,100.81,19.12,82.95,100.05,20.06,81.52,99.24,20.80,80.02,98.41,21.33,78.47,97.57,21.67,76.89,96.72,21.86,75.30,95.87,21.93,73.71,95.02,21.92,72.12,94.18,21.86,70.54,93.34,21.79,68.95,92.50,21.69,67.36,91.66,21.59,65.89,90.92,21.05,64.88,90.46,19.64,64.32,90.26,17.95,63.94,90.17,16.19,63.69,90.14,14.41,63.54,90.17,12.62,63.40,90.20,10.82,63.25,90.23,9.03,63.11,90.26,7.23,62.99,90.30,5.44,62.87,90.34,3.64,62.75,90.38,1.85,62.62,90.43,0.05,62.63,90.54,-1.74,62.85,90.76,-3.51,63.21,91.05,-5.25,63.65,91.39,-6.96,64.09,91.72,-8.67,64.53,92.06,-10.39,64.97,92.39,-12.10,65.41,92.73,-13.81,65.85,93.06,-15.53,66.37,93.44,-17.20,67.39,94.06,-18.54,68.65,94.79,-19.44,70.23,95.64,-19.58,71.57,96.31,-18.93,72.72,96.84,-17.65,73.86,97.37,-16.36,75.00,97.90,-15.07,76.14,98.43,-13.78,77.28,98.96,-12.49,78.41,99.49,-11.20,79.55,100.02,-9.91,80.68,100.54,-8.61,81.81,101.06,-7.31,82.93,101.58,-6.00,84.04,102.09,-4.68,85.13,102.59,-3.33,86.13,103.04,-1.91,87.00,103.41,-0.38,87.72,103.70,1.25,88.26,103.89,2.95,88.62,103.97,4.71],
        [92.49,103.00,8.07,92.49,102.89,9.95,92.31,102.68,11.82,91.91,102.35,13.64,91.27,101.91,15.36,90.42,101.37,16.96,89.37,100.72,18.39,88.14,100.00,19.63,86.77,99.21,20.66,85.28,98.37,21.46,83.70,97.49,22.02,82.07,96.61,22.36,80.41,95.71,22.49,78.74,94.83,22.48,77.07,93.94,22.41,75.40,93.06,22.31,73.73,92.18,22.20,72.06,91.30,22.10,70.39,90.42,22.00,68.72,89.54,21.90,67.11,88.69,21.62,65.98,88.18,20.20,65.39,87.97,18.43,65.02,87.88,16.57,64.83,87.90,14.69,64.68,87.93,12.81,64.53,87.96,10.93,64.37,87.99,9.04,64.24,88.03,7.16,64.11,88.07,5.27,63.98,88.11,3.39,63.85,88.16,1.50,63.81,88.25,-0.38,64.01,88.46,-2.25,64.40,88.78,-4.07,64.86,89.13,-5.87,65.32,89.48,-7.67,65.78,89.83,-9.47,66.24,90.19,-11.27,66.70,90.54,-13.07,67.16,90.89,-14.87,67.64,91.25,-16.66,68.48,91.79,-18.24,69.64,92.48,-19.56,71.22,93.34,-19.84,72.80,94.17,-19.61,74.01,94.73,-18.27,75.22,95.30,-16.94,76.44,95.86,-15.61,77.65,96.43,-14.27,78.86,96.99,-12.93,80.07,97.56,-11.59,81.27,98.12,-10.25,82.48,98.68,-8.91,83.68,99.24,-7.56,84.88,99.79,-6.20,86.07,100.35,-4.85,87.26,100.90,-3.48,88.43,101.44,-2.10,89.55,101.95,-0.67,90.53,102.38,0.89,91.33,102.71,2.57,91.94,102.92,4.34,92.33,103.02,6.19],
        [96.12,102.00,9.56,96.04,101.84,11.53,95.71,101.55,13.45,95.13,101.13,15.29,94.32,100.60,17.01,93.29,99.96,18.56,92.07,99.23,19.93,90.69,98.43,21.07,89.16,97.56,21.97,87.53,96.66,22.61,85.82,95.73,22.97,84.09,94.80,23.04,82.34,93.88,22.93,80.60,92.96,22.81,78.86,92.04,22.69,77.12,91.13,22.58,75.38,90.21,22.46,73.63,89.29,22.34,71.89,88.37,22.22,70.15,87.45,22.10,68.41,86.53,21.98,67.10,85.91,20.77,66.46,85.68,18.93,66.14,85.62,16.98,65.98,85.65,15.02,65.82,85.68,13.05,65.66,85.71,11.09,65.50,85.75,9.12,65.36,85.79,7.15,65.23,85.84,5.18,65.09,85.88,3.21,64.99,85.94,1.25,65.13,86.14,-0.71,65.51,86.45,-2.62,65.99,86.82,-4.50,66.47,87.18,-6.38,66.95,87.55,-8.25,67.43,87.92,-10.13,67.91,88.28,-12.01,68.40,88.65,-13.89,68.88,89.02,-15.77,69.45,89.43,-17.61,70.57,90.12,-19.07,71.98,90.92,-19.97,73.72,91.84,-19.99,75.09,92.51,-18.90,76.36,93.10,-17.51,77.63,93.69,-16.12,78.90,94.29,-14.73,80.17,94.88,-13.35,81.44,95.47,-11.96,82.71,96.07,-10.57,83.98,96.66,-9.18,85.25,97.25,-7.79,86.52,97.84,-6.40,87.79,98.44,-5.01,89.06,99.03,-3.62,90.33,99.62,-2.23,91.60,100.22,-0.84,92.85,100.80,0.57,93.95,101.29,2.12,94.85,101.66,3.84,95.51,101.91,5.68,95.94,102.02,7.60],
        [97.29,99.79,9.43,97.21,99.64,11.40,96.88,99.35,13.32,96.30,98.93,15.17,95.49,98.39,16.88,94.46,97.75,18.44,93.23,97.02,19.81,91.84,96.22,20.95,90.31,95.35,21.85,88.68,94.44,22.48,86.97,93.51,22.84,85.23,92.58,22.92,83.49,91.66,22.93,81.74,90.73,22.83,80.00,89.81,22.73,78.25,88.89,22.62,76.51,87.97,22.50,74.76,87.05,22.39,73.02,86.13,22.27,71.27,85.21,22.16,69.53,84.29,22.04,68.10,83.58,21.12,67.51,83.38,19.26,67.29,83.38,17.30,67.13,83.41,15.33,66.96,83.44,13.36,66.80,83.47,11.39,66.65,83.50,9.42,66.51,83.55,7.45,66.37,83.59,5.48,66.24,83.64,3.51,66.20,83.73,1.54,66.46,83.99,-0.40,66.91,84.34,-2.29,67.39,84.71,-4.17,67.87,85.07,-6.05,68.35,85.44,-7.93,68.84,85.81,-9.81,69.32,86.18,-11.70,69.80,86.54,-13.58,70.28,86.91,-15.46,70.80,87.30,-17.32,71.79,87.92,-18.89,73.12,88.69,-20.00,74.86,89.62,-20.00,76.34,90.35,-19.20,77.61,90.95,-17.80,78.88,91.54,-16.41,80.15,92.13,-15.02,81.42,92.72,-13.62,82.69,93.32,-12.23,83.96,93.91,-10.84,85.22,94.50,-9.44,86.49,95.09,-8.05,87.76,95.68,-6.65,89.02,96.27,-5.24,90.27,96.85,-3.84,91.52,97.43,-2.42,92.77,98.01,-1.00,94.01,98.59,0.42,95.12,99.08,1.98,96.02,99.46,3.70,96.69,99.71,5.54,97.11,99.82,7.47],
        [98.46,97.59,9.30,98.38,97.43,11.27,98.05,97.14,13.20,97.47,96.72,15.04,96.65,96.19,16.76,95.62,95.55,18.32,94.40,94.81,19.69,93.00,94.00,20.83,91.47,93.14,21.73,89.83,92.23,22.36,88.12,91.30,22.71,86.38,90.37,22.79,84.63,89.44,22.80,82.89,88.51,22.81,81.14,87.58,22.72,79.39,86.66,22.62,77.65,85.74,22.53,75.90,84.82,22.42,74.15,83.89,22.30,72.41,82.97,22.19,70.66,82.05,22.07,69.10,81.26,21.48,68.59,81.10,19.59,68.43,81.13,17.62,68.27,81.16,15.65,68.11,81.19,13.68,67.95,81.23,11.71,67.79,81.26,9.73,67.66,81.30,7.76,67.52,81.35,5.79,67.39,81.40,3.81,67.47,81.55,1.85,67.83,81.86,-0.07,68.31,82.23,-1.95,68.79,82.59,-3.84,69.27,82.96,-5.72,69.75,83.33,-7.60,70.24,83.70,-9.49,70.72,84.07,-11.37,71.20,84.44,-13.25,71.68,84.80,-15.14,72.17,85.18,-17.02,72.99,85.71,-18.71,74.26,86.46,-20.00,76.00,87.39,-20.00,77.56,88.19,-19.45,78.83,88.78,-18.06,80.10,89.37,-16.66,81.37,89.96,-15.26,82.64,90.55,-13.86,83.90,91.14,-12.46,85.17,91.73,-11.05,86.43,92.32,-9.65,87.69,92.90,-8.24,88.94,93.48,-6.82,90.19,94.06,-5.40,91.44,94.64,-3.98,92.69,95.22,-2.56,93.94,95.80,-1.14,95.18,96.38,0.29,96.29,96.88,1.85,97.19,97.25,3.57,97.86,97.50,5.41,98.28,97.61,7.33],
        [99.63,95.39,9.17,99.55,95.23,11.14,99.22,94.93,13.07,98.64,94.52,14.92,97.82,93.98,16.64,96.79,93.34,18.20,95.56,92.60,19.56,94.16,91.79,20.71,92.63,90.92,21.60,90.99,90.01,22.23,89.28,89.08,22.58,87.53,88.15,22.66,85.79,87.22,22.67,84.04,86.29,22.68,82.29,85.36,22.69,80.54,84.44,22.61,78.79,83.51,22.51,77.04,82.59,22.42,75.29,81.66,22.32,73.54,80.74,22.22,71.80,79.82,22.10,70.11,78.94,21.83,69.74,78.85,19.92,69.58,78.89,17.95,69.42,78.92,15.97,69.26,78.95,14.00,69.10,78.98,12.02,68.94,79.02,10.05,68.81,79.06,8.08,68.67,79.11,6.10,68.57,79.17,4.12,68.77,79.39,2.17,69.22,79.74,0.27,69.70,80.11,-1.61,70.19,80.48,-3.50,70.67,80.85,-5.38,71.15,81.22,-7.27,71.63,81.59,-9.15,72.12,81.96,-11.04,72.60,82.32,-12.92,73.08,82.69,-14.81,73.57,83.06,-16.69,74.22,83.52,-18.50,75.41,84.23,-19.91,77.13,85.15,-20.00,78.77,86.01,-19.68,80.04,86.60,-18.28,81.30,87.19,-16.87,82.57,87.77,-15.47,83.83,88.36,-14.06,85.09,88.95,-12.65,86.34,89.53,-11.23,87.59,90.11,-9.81,88.85,90.69,-8.39,90.10,91.27,-6.97,91.35,91.85,-5.54,92.60,92.43,-4.12,93.85,93.02,-2.70,95.10,93.60,-1.28,96.35,94.17,0.15,97.45,94.67,1.71,98.36,95.05,3.43,99.03,95.30,5.27,99.46,95.41,7.20],
        [100.80,93.18,9.04,100.73,93.02,11.01,100.39,92.73,12.94,99.81,92.31,14.79,98.99,91.78,16.51,97.96,91.13,18.07,96.73,90.40,19.44,95.33,89.59,20.58,93.80,88.72,21.47,92.16,87.81,22.10,90.44,86.88,22.45,88.70,85.94,22.53,86.95,85.01,22.54,85.20,84.08,22.55,83.45,83.15,22.56,81.70,82.22,22.57,79.95,81.30,22.50,78.20,80.37,22.40,76.45,79.45,22.31,74.71,78.52,22.21,72.96,77.60,22.11,71.21,76.67,22.01,70.88,76.61,20.20,70.72,76.64,18.23,70.56,76.67,16.25,70.40,76.70,14.28,70.24,76.74,12.31,70.09,76.77,10.33,69.95,76.82,8.35,69.82,76.86,6.38,69.80,76.97,4.40,70.14,77.27,2.48,70.62,77.64,0.59,71.11,78.00,-1.29,71.59,78.37,-3.18,72.07,78.74,-5.07,72.56,79.11,-6.95,73.04,79.48,-8.84,73.52,79.85,-10.72,74.00,80.22,-12.61,74.49,80.59,-14.49,74.97,80.95,-16.38,75.53,81.36,-18.23,76.61,82.03,-19.73,78.27,82.93,-20.00,79.98,83.82,-19.87,81.24,84.41,-18.46,82.50,85.00,-17.05,83.75,85.58,-15.63,85.01,86.16,-14.21,86.26,86.74,-12.79,87.51,87.32,-11.37,88.76,87.91,-9.94,90.01,88.49,-8.52,91.26,89.07,-7.10,92.52,89.65,-5.68,93.77,90.23,-4.26,95.02,90.81,-2.84,96.27,91.39,-1.41,97.52,91.97,0.01,98.63,92.47,1.57,99.53,92.84,3.30,100.20,93.09,5.14,100.63,93.20,7.07],
        [101.98,90.98,8.91,101.90,90.82,10.88,101.57,90.53,12.80,100.99,90.11,14.64,100.18,89.58,16.36,99.15,88.94,17.92,97.93,88.21,19.28,96.54,87.40,20.43,95.01,86.53,21.33,93.37,85.63,21.96,91.67,84.70,22.31,89.93,83.77,22.40,88.19,82.84,22.41,86.44,81.92,22.42,84.70,80.99,22.43,82.95,80.06,22.44,81.21,79.13,22.45,79.47,78.21,22.40,77.72,77.29,22.30,75.98,76.36,22.20,74.23,75.44,22.10,72.49,74.52,22.01,72.01,74.37,20.22,71.84,74.40,18.25,71.68,74.43,16.29,71.52,74.46,14.32,71.36,74.50,12.35,71.22,74.54,10.38,71.08,74.58,8.41,70.96,74.63,6.44,71.14,74.84,4.49,71.59,75.20,2.60,72.08,75.56,0.72,72.56,75.93,-1.16,73.04,76.30,-3.04,73.52,76.67,-4.92,74.00,77.04,-6.80,74.48,77.40,-8.68,74.96,77.77,-10.56,75.45,78.14,-12.44,75.93,78.51,-14.32,76.41,78.87,-16.20,76.93,79.26,-18.07,77.90,79.87,-19.64,79.52,80.75,-20.00,81.24,81.67,-19.94,82.49,82.25,-18.53,83.74,82.83,-17.11,84.99,83.41,-15.69,86.24,83.98,-14.28,87.48,84.56,-12.86,88.73,85.14,-11.44,89.98,85.72,-10.02,91.23,86.30,-8.61,92.47,86.88,-7.19,93.72,87.46,-5.77,94.97,88.04,-4.35,96.22,88.62,-2.94,97.46,89.20,-1.52,98.71,89.77,-0.10,99.81,90.27,1.46,100.71,90.64,3.18,101.37,90.89,5.02,101.80,91.00,6.95],
        [103.15,88.77,8.78,103.07,88.62,10.74,102.74,88.33,12.66,102.17,87.91,14.49,101.36,87.38,16.21,100.34,86.75,17.76,99.12,86.02,19.13,97.74,85.22,20.27,96.22,84.35,21.17,94.59,83.45,21.81,92.90,82.53,22.18,91.16,81.60,22.26,89.42,80.67,22.28,87.69,79.75,22.29,85.95,78.82,22.30,84.21,77.90,22.31,82.47,76.98,22.32,80.73,76.05,22.33,79.00,75.13,22.30,77.26,74.21,22.20,75.52,73.29,22.10,73.78,72.37,22.00,73.13,72.13,20.24,72.97,72.16,18.28,72.81,72.19,16.32,72.65,72.22,14.36,72.49,72.26,12.39,72.35,72.30,10.43,72.21,72.34,8.47,72.19,72.45,6.51,72.57,72.76,4.60,73.05,73.13,2.73,73.53,73.49,0.85,74.01,73.86,-1.02,74.49,74.23,-2.90,74.97,74.59,-4.77,75.45,74.96,-6.64,75.93,75.32,-8.52,76.41,75.69,-10.39,76.89,76.06,-12.27,77.37,76.42,-14.14,77.85,76.79,-16.01,78.34,77.16,-17.88,79.18,77.71,-19.55,80.76,78.58,-20.00,82.49,79.50,-19.99,83.74,80.08,-18.58,84.98,80.65,-17.17,86.22,81.23,-15.75,87.47,81.81,-14.34,88.71,82.38,-12.93,89.96,82.96,-11.51,91.20,83.54,-10.10,92.44,84.12,-8.69,93.69,84.69,-7.27,94.93,85.27,-5.86,96.17,85.85,-4.45,97.42,86.43,-3.04,98.66,87.00,-1.62,99.90,87.58,-0.20,100.99,88.07,1.35,101.89,88.44,3.07,102.55,88.68,4.90,102.97,88.80,6.82],
        [104.32,86.57,8.65,104.24,86.41,10.60,103.92,86.12,12.51,103.35,85.71,14.34,102.54,85.18,16.05,101.53,84.55,17.61,100.32,83.83,18.97,98.95,83.03,20.12,97.43,82.17,21.02,95.82,81.27,21.67,94.12,80.35,22.04,92.40,79.43,22.13,90.67,78.51,22.14,88.93,77.59,22.16,87.20,76.66,22.17,85.47,75.74,22.18,83.74,74.82,22.19,82.00,73.90,22.20,80.27,72.98,22.21,78.54,72.06,22.19,76.81,71.14,22.10,75.07,70.23,21.99,74.25,69.89,20.27,74.09,69.92,18.31,73.93,69.95,16.36,73.77,69.98,14.40,73.61,70.02,12.44,73.48,70.06,10.49,73.35,70.11,8.53,73.55,70.33,6.60,74.02,70.69,4.73,74.50,71.05,2.86,74.97,71.42,0.99,75.45,71.78,-0.88,75.93,72.15,-2.74,76.41,72.52,-4.61,76.89,72.88,-6.48,77.37,73.25,-8.35,77.85,73.61,-10.22,78.32,73.98,-12.08,78.80,74.34,-13.95,79.28,74.71,-15.82,79.76,75.07,-17.69,80.47,75.55,-19.44,81.99,76.39,-20.00,83.72,77.32,-20.00,84.97,77.90,-18.61,86.22,78.48,-17.21,87.46,79.06,-15.81,88.70,79.63,-14.40,89.94,80.21,-12.99,91.18,80.78,-11.58,92.42,81.36,-10.18,93.66,81.93,-8.77,94.90,82.51,-7.36,96.14,83.08,-5.95,97.38,83.66,-4.54,98.62,84.24,-3.13,99.86,84.81,-1.73,101.09,85.38,-0.31,102.18,85.87,1.25,103.06,86.24,2.95,103.73,86.48,4.78,104.15,86.59,6.70],
        [105.49,84.36,8.52,105.42,84.21,10.46,105.09,83.92,12.37,104.52,83.51,14.19,103.73,82.99,15.90,102.72,82.36,17.45,101.52,81.64,18.81,100.15,80.85,19.96,98.65,79.99,20.87,97.04,79.10,21.52,95.36,78.18,21.90,93.64,77.26,22.00,91.91,76.34,22.01,90.18,75.42,22.02,88.46,74.50,22.04,86.73,73.59,22.05,85.00,72.67,22.06,83.28,71.75,22.07,81.55,70.83,22.08,79.83,69.91,22.09,78.10,68.99,22.08,76.38,68.09,21.96,75.50,67.72,20.26,75.21,67.68,18.35,75.05,67.71,16.40,74.90,67.74,14.45,74.74,67.78,12.50,74.61,67.82,10.55,74.58,67.93,8.60,74.99,68.25,6.72,75.47,68.62,4.86,75.95,68.98,3.00,76.42,69.35,1.14,76.90,69.71,-0.73,77.38,70.07,-2.59,77.85,70.44,-4.45,78.33,70.80,-6.31,78.81,71.16,-8.17,79.28,71.53,-10.03,79.76,71.89,-11.89,80.24,72.26,-13.75,80.71,72.62,-15.61,81.19,72.98,-17.48,81.80,73.41,-19.28,83.21,74.21,-20.00,84.94,75.13,-20.00,86.19,75.71,-18.62,87.44,76.30,-17.24,88.69,76.88,-15.85,89.93,77.46,-14.45,91.17,78.03,-13.05,92.41,78.60,-11.65,93.64,79.18,-10.25,94.88,79.75,-8.84,96.11,80.32,-7.44,97.35,80.90,-6.04,98.58,81.47,-4.63,99.82,82.04,-3.23,101.05,82.62,-1.83,102.28,83.19,-0.41,103.37,83.67,1.14,104.24,84.04,2.84,104.90,84.28,4.67,105.32,84.39,6.57],
        [106.67,82.16,8.39,106.59,82.00,10.32,106.27,81.72,12.22,105.71,81.32,14.04,104.92,80.80,15.73,103.92,80.17,17.28,102.73,79.46,18.64,101.38,78.67,19.79,99.89,77.82,20.71,98.29,76.93,21.37,96.62,76.02,21.75,94.90,75.11,21.87,93.19,74.19,21.88,91.47,73.28,21.89,89.75,72.36,21.90,88.03,71.45,21.92,86.32,70.54,21.93,84.60,69.62,21.94,82.88,68.71,21.95,81.16,67.80,21.96,79.45,66.88,21.97,77.73,65.97,21.96,76.84,65.60,20.29,76.34,65.44,18.45,76.18,65.47,16.52,76.03,65.50,14.58,75.88,65.54,12.64,75.75,65.58,10.70,75.95,65.80,8.78,76.42,66.17,6.93,76.90,66.53,5.08,77.37,66.89,3.23,77.84,67.25,1.38,78.32,67.61,-0.47,78.79,67.98,-2.33,79.27,68.34,-4.18,79.74,68.70,-6.03,80.22,69.06,-7.88,80.69,69.42,-9.73,81.16,69.79,-11.59,81.64,70.15,-13.44,82.11,70.51,-15.29,82.59,70.87,-17.14,83.12,71.26,-18.97,84.35,71.97,-19.86,86.07,72.88,-19.79,87.38,73.50,-18.50,88.66,74.10,-17.17,89.92,74.69,-15.81,91.17,75.28,-14.44,92.42,75.86,-13.07,93.66,76.43,-11.68,94.89,77.01,-10.29,96.12,77.58,-8.90,97.35,78.15,-7.50,98.58,78.72,-6.10,99.81,79.29,-4.71,101.04,79.86,-3.31,102.26,80.43,-1.92,103.48,81.00,-0.51,104.56,81.48,1.04,105.43,81.84,2.74,106.08,82.08,4.56,106.49,82.18,6.45],
        [107.84,79.96,8.26,107.76,79.80,10.17,107.45,79.52,12.05,106.90,79.13,13.85,106.13,78.61,15.54,105.15,78.00,17.08,103.98,77.30,18.44,102.65,76.53,19.59,101.19,75.69,20.51,99.61,74.81,21.18,97.96,73.91,21.60,96.27,73.01,21.74,94.57,72.10,21.75,92.87,71.20,21.76,91.17,70.29,21.77,89.47,69.39,21.78,87.77,68.48,21.80,86.07,67.58,21.81,84.37,66.68,21.82,82.67,65.77,21.81,80.97,64.87,21.79,79.27,63.97,21.76,78.24,63.50,20.44,77.48,63.20,18.71,77.33,63.23,16.79,77.17,63.26,14.87,77.03,63.30,12.95,76.94,63.36,11.03,77.33,63.68,9.18,77.80,64.04,7.34,78.27,64.40,5.51,78.74,64.76,3.68,79.21,65.11,1.85,79.68,65.47,0.01,80.15,65.83,-1.82,80.62,66.19,-3.65,81.09,66.55,-5.48,81.56,66.91,-7.32,82.03,67.26,-9.15,82.49,67.62,-10.98,82.96,67.98,-12.81,83.43,68.34,-14.65,83.90,68.70,-16.48,84.38,69.06,-18.31,85.43,69.68,-19.30,87.13,70.57,-19.17,88.48,71.22,-18.01,89.80,71.85,-16.76,91.11,72.47,-15.49,92.40,73.08,-14.20,93.69,73.69,-12.90,94.95,74.28,-11.57,96.19,74.86,-10.22,97.42,75.43,-8.86,98.65,76.00,-7.49,99.86,76.57,-6.10,101.08,77.13,-4.72,102.30,77.70,-3.34,103.51,78.26,-1.96,104.72,78.82,-0.56,105.77,79.29,0.98,106.62,79.64,2.66,107.26,79.87,4.46,107.67,79.98,6.34],
        [109.01,77.75,8.13,108.94,77.60,10.01,108.64,77.33,11.85,108.11,76.95,13.62,107.37,76.45,15.28,106.42,75.86,16.81,105.30,75.18,18.16,104.01,74.43,19.32,102.59,73.62,20.26,101.06,72.76,20.96,99.45,71.88,21.41,97.80,70.99,21.60,96.13,70.10,21.62,94.46,69.21,21.63,92.79,68.33,21.64,91.13,67.44,21.65,89.46,66.55,21.63,87.79,65.67,21.58,86.12,64.79,21.53,84.45,63.90,21.46,82.78,63.02,21.39,81.11,62.14,21.31,79.71,61.42,20.74,78.91,61.10,19.06,78.49,60.98,17.26,78.33,61.01,15.38,78.19,61.05,13.50,78.20,61.16,11.62,78.65,61.51,9.82,79.11,61.86,8.02,79.57,62.21,6.23,80.03,62.56,4.43,80.49,62.92,2.63,80.95,63.27,0.83,81.41,63.62,-0.97,81.87,63.97,-2.77,82.34,64.32,-4.56,82.80,64.67,-6.36,83.26,65.02,-8.16,83.72,65.38,-9.96,84.18,65.73,-11.76,84.64,66.08,-13.55,85.10,66.43,-15.35,85.56,66.78,-17.15,86.47,67.33,-18.21,88.13,68.20,-18.00,89.52,68.88,-16.97,90.86,69.52,-15.80,92.19,70.16,-14.62,93.51,70.79,-13.43,94.83,71.42,-12.23,96.14,72.05,-11.03,97.44,72.67,-9.81,98.73,73.28,-8.56,99.99,73.87,-7.29,101.23,74.45,-5.98,102.43,75.01,-4.64,103.63,75.57,-3.29,104.82,76.12,-1.93,105.99,76.66,-0.56,107.01,77.12,0.97,107.84,77.46,2.63,108.45,77.68,4.40,108.85,77.78,6.25],
        [109.96,75.43,7.99,109.90,75.29,9.80,109.62,75.04,11.58,109.13,74.67,13.29,108.44,74.21,14.90,107.56,73.65,16.38,106.50,73.01,17.72,105.29,72.30,18.87,103.95,71.53,19.83,102.51,70.72,20.56,100.98,69.88,21.07,99.40,69.02,21.35,97.80,68.17,21.40,96.20,67.32,21.32,94.59,66.47,21.23,92.99,65.63,21.14,91.39,64.78,21.05,89.78,63.93,20.96,88.18,63.08,20.87,86.57,62.24,20.78,84.97,61.39,20.69,83.36,60.54,20.60,81.76,59.69,20.51,80.52,59.08,19.68,79.75,58.77,18.06,79.52,58.76,16.27,79.39,58.79,14.46,79.41,58.91,12.66,79.85,59.25,10.93,80.29,59.58,9.20,80.74,59.92,7.48,81.18,60.26,5.75,81.62,60.60,4.02,82.06,60.94,2.29,82.51,61.27,0.56,82.95,61.61,-1.17,83.39,61.95,-2.90,83.84,62.29,-4.63,84.28,62.63,-6.36,84.72,62.96,-8.09,85.17,63.30,-9.82,85.62,63.65,-11.54,86.09,64.00,-13.26,86.58,64.36,-14.97,87.39,64.86,-16.11,88.98,65.68,-15.77,90.37,66.37,-14.90,91.68,67.00,-13.81,92.99,67.64,-12.72,94.30,68.27,-11.63,95.61,68.90,-10.55,96.92,69.53,-9.46,98.23,70.16,-8.37,99.54,70.80,-7.28,100.85,71.43,-6.19,102.16,72.06,-5.10,103.46,72.69,-4.00,104.76,73.31,-2.90,106.02,73.91,-1.73,107.14,74.43,-0.40,108.09,74.85,1.08,108.87,75.16,2.70,109.44,75.37,4.41,109.81,75.46,6.18],
        [105.78,70.38,7.77,105.74,70.27,9.26,105.52,70.07,10.73,105.13,69.78,12.14,104.59,69.41,13.48,103.87,68.96,14.71,103.02,68.44,15.82,102.03,67.86,16.78,100.93,67.23,17.57,99.75,66.56,18.19,98.50,65.87,18.65,97.22,65.17,18.97,95.92,64.47,19.19,94.61,63.76,19.36,93.30,63.05,19.48,91.98,62.35,19.57,90.67,61.65,19.59,89.34,60.94,19.59,88.02,60.24,19.57,86.70,59.54,19.53,85.38,58.84,19.48,84.06,58.14,19.43,82.74,57.45,19.37,81.55,56.84,18.95,80.88,56.56,17.65,80.64,56.52,16.20,80.53,56.55,14.71,80.74,56.75,13.26,81.10,57.02,11.83,81.47,57.30,10.41,81.83,57.58,8.99,82.20,57.86,7.56,82.56,58.14,6.14,82.93,58.42,4.71,83.29,58.69,3.29,83.66,58.97,1.87,84.02,59.25,0.44,84.39,59.53,-0.98,84.77,59.82,-2.39,85.17,60.11,-3.80,85.60,60.43,-5.20,86.08,60.76,-6.58,86.57,61.11,-7.95,87.07,61.45,-9.32,87.56,61.79,-10.69,88.41,62.29,-11.39,89.69,62.94,-10.99,90.91,63.56,-10.42,91.98,64.07,-9.52,93.05,64.59,-8.61,94.12,65.10,-7.70,95.19,65.62,-6.79,96.26,66.14,-5.89,97.33,66.65,-4.98,98.40,67.16,-4.06,99.46,67.67,-3.14,100.51,68.18,-2.21,101.55,68.67,-1.25,102.57,69.15,-0.26,103.47,69.57,0.85,104.25,69.91,2.08,104.88,70.16,3.41,105.35,70.33,4.82,105.65,70.40,6.28],
        [101.60,65.34,7.55,101.58,65.26,8.71,101.42,65.11,9.85,101.15,64.90,10.96,100.75,64.62,12.02,100.22,64.28,13.00,99.58,63.89,13.88,98.83,63.44,14.65,97.98,62.96,15.27,97.07,62.44,15.79,96.11,61.91,16.18,95.13,61.37,16.48,94.13,60.83,16.72,93.13,60.28,16.92,92.11,59.73,17.08,91.10,59.18,17.22,90.08,58.63,17.34,89.06,58.09,17.45,88.04,57.54,17.54,87.02,56.99,17.63,86.00,56.44,17.70,84.97,55.89,17.75,83.95,55.35,17.77,82.92,54.80,17.78,82.05,54.37,17.35,81.76,54.28,16.25,81.75,54.34,15.10,82.04,54.56,13.99,82.32,54.77,12.88,82.60,54.99,11.78,82.89,55.20,10.67,83.17,55.42,9.56,83.46,55.64,8.46,83.76,55.87,7.36,84.12,56.12,6.28,84.50,56.39,5.22,84.89,56.65,4.15,85.27,56.92,3.09,85.65,57.19,2.03,86.04,57.46,0.96,86.42,57.72,-0.10,86.80,57.99,-1.17,87.19,58.26,-2.23,87.57,58.52,-3.30,87.96,58.79,-4.36,88.34,59.06,-5.43,88.88,59.39,-6.21,89.90,59.93,-6.07,90.91,60.45,-5.80,91.86,60.93,-5.38,92.68,61.32,-4.66,93.51,61.72,-3.94,94.33,62.11,-3.22,95.15,62.51,-2.49,95.96,62.90,-1.76,96.78,63.28,-1.03,97.58,63.67,-0.28,98.38,64.05,0.47,99.15,64.41,1.26,99.84,64.72,2.15,100.42,64.97,3.12,100.90,65.17,4.16,101.26,65.29,5.26,101.48,65.35,6.40],
        [97.41,60.29,7.33,97.41,60.24,8.13,97.33,60.15,8.92,97.18,60.03,9.69,96.93,59.85,10.43,96.63,59.65,11.14,96.23,59.40,11.78,95.76,59.11,12.36,95.21,58.79,12.84,94.61,58.45,13.23,93.98,58.09,13.56,93.32,57.73,13.83,92.65,57.36,14.06,91.97,56.99,14.24,91.28,56.61,14.42,90.59,56.24,14.56,89.90,55.86,14.69,89.21,55.49,14.82,88.51,55.11,14.93,87.82,54.73,15.03,87.12,54.36,15.14,86.42,53.98,15.22,85.72,53.60,15.28,85.02,53.23,15.35,84.32,52.85,15.41,83.69,52.52,15.38,83.83,52.64,14.61,84.06,52.81,13.86,84.33,52.99,13.13,84.59,53.17,12.40,84.85,53.36,11.67,85.12,53.54,10.94,85.38,53.72,10.21,85.64,53.91,9.48,85.91,54.09,8.74,86.17,54.27,8.01,86.43,54.46,7.28,86.70,54.64,6.55,86.96,54.82,5.82,87.22,55.01,5.09,87.49,55.19,4.36,87.75,55.37,3.63,88.01,55.56,2.90,88.28,55.74,2.17,88.54,55.92,1.43,88.80,56.11,0.70,89.07,56.29,-0.03,89.33,56.47,-0.76,89.90,56.79,-0.96,90.60,57.15,-0.86,91.30,57.52,-0.77,92.00,57.88,-0.61,92.61,58.19,-0.24,93.16,58.45,0.27,93.71,58.71,0.79,94.26,58.97,1.31,94.81,59.23,1.83,95.35,59.49,2.35,95.83,59.71,2.95,96.28,59.91,3.58,96.64,60.06,4.27,96.94,60.18,5.00,97.18,60.26,5.76,97.32,60.29,6.54],
        [93.42,55.48,7.13,93.42,55.46,7.44,93.42,55.44,7.75,93.42,55.42,8.06,93.37,55.37,8.36,93.31,55.33,8.66,93.25,55.28,8.96,93.16,55.21,9.25,93.05,55.14,9.53,92.94,55.06,9.81,92.80,54.97,10.07,92.64,54.87,10.32,92.45,54.75,10.53,92.18,54.61,10.59,91.91,54.46,10.64,91.64,54.32,10.69,91.37,54.17,10.73,91.10,54.02,10.77,90.83,53.88,10.81,90.56,53.73,10.85,90.29,53.59,10.89,90.02,53.44,10.92,89.75,53.30,10.92,89.52,53.19,10.75,89.31,53.09,10.54,89.14,53.01,10.30,89.00,52.95,10.03,88.92,52.93,9.73,88.88,52.92,9.42,88.87,52.94,9.11,88.87,52.96,8.80,88.90,52.99,8.50,88.95,53.03,8.19,88.99,53.08,7.89,89.06,53.13,7.59,89.13,53.18,7.29,89.21,53.24,7.00,89.28,53.30,6.70,89.36,53.36,6.41,89.45,53.43,6.12,89.55,53.49,5.83,89.64,53.56,5.54,89.73,53.63,5.25,89.83,53.69,4.96,89.92,53.76,4.68,90.03,53.83,4.39,90.13,53.91,4.11,90.41,54.05,4.15,90.68,54.19,4.19,90.95,54.33,4.23,91.23,54.48,4.28,91.50,54.62,4.32,91.77,54.76,4.36,92.04,54.90,4.40,92.32,55.05,4.46,92.59,55.19,4.51,92.73,55.25,4.78,92.87,55.31,5.05,93.00,55.36,5.32,93.10,55.39,5.62,93.20,55.43,5.91,93.28,55.45,6.21,93.33,55.46,6.51,93.38,55.47,6.82]]
};
const POD_AXIS = {
    "12" : [[98.2136, 69.2996, 8.0000], [0.207627, -0.976807, -0.052336]],
    "20" : [[98.2136, 69.2996, 8.0000], [0.341551, -0.938405, -0.052336]],
    "28" : [[98.2136, 69.2996, 8.0000], [0.468828, -0.881738, -0.052336]]
};
