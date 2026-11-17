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
// A finish (fillet, chamfer, lift step) that fails is skipped so the part still builds, but NOT silently: skipped() notes it
// and reportSkipped() shows the list as a feature INFO (blue note; the feature stays green) at the end of the feature.
function skipped(context is Context, what is string)
{
    var prev = "";
    try silent
    {
        prev = getVariable(context, "gbgSkipped");
        if (prev != "") prev = prev ~ "; ";
    }
    setVariable(context, "gbgSkipped", prev ~ what);
    println("SKIPPED: " ~ what);
}
// also written to the DESCRIPTION property of `target` (readable through the REST API: GET .../parts)
function reportSkipped(context is Context, id is Id, target is Query)
{
    // a real error throws and never gets here: an ERROR status still on the feature now can only come from a caught finish
    // failure, and it would roll the whole feature back (std defineFeature aborts a feature that ends with an error)
    if (getFeatureError(context, id) != undefined)
    {
        clearFeatureStatus(context, id, { "withDisplayData" : true });
        skipped(context, "a caught finish error status was cleared");
    }
    var msg = "";
    try silent
    {
        msg = getVariable(context, "gbgSkipped");
    }
    if (msg != "")
    {
        reportFeatureInfo(context, id, "Cosmetic finish skipped (part complete; check the edge): " ~ msg);
        if (size(evaluateQuery(context, target)) > 0)
            setProperty(context, { "entities" : target, "propertyType" : PropertyType.DESCRIPTION, "value" : "skipped: " ~ msg });
    }
}
// fillet (or 45 deg chamfer) a set of edges: all together, else edge by edge; every edge that fails is reported by skipped()
function finishEdges(context is Context, id is Id, edges is Query, r is number, chamfer is boolean, what is string)
{
    const list = evaluateQuery(context, edges);
    if (size(list) == 0)
    {
        skipped(context, what ~ " (no edges found)");
        return;
    }
    if (blendOnce(context, id + "all", edges, r, chamfer))
        return;
    var failed = 0;
    for (var e = 0; e < size(list); e += 1)
        if (!blendOnce(context, id + ("e" ~ e), list[e], r, chamfer))
            failed += 1;
    if (failed > 0)
        skipped(context, what ~ " (" ~ failed ~ " of " ~ size(list) ~ " edges)");
}
function blendOnce(context is Context, id is Id, edges is Query, r is number, chamfer is boolean) returns boolean
{
    return attempt(context, id, function(sid)
        {
            if (chamfer)
                opChamfer(context, sid + "op", { "entities" : edges, "chamferType" : ChamferType.EQUAL_OFFSETS, "width" : r * millimeter });
            else
                opFillet(context, sid + "op", { "entities" : edges, "radius" : r * millimeter });
        });
}
// v1.2c: every optional finish runs through attempt(): fn(id) runs as its own SUB-FEATURE.  If anything in it throws, the
// sub-feature is aborted = every change it made is rolled back (a failed or half-done finish can never delete, split or merge
// a body) and its status is cleared.  Why: in final2 the 4 pod lift steps and 3 tenon chamfer edges failed inside plain
// `try silent` blocks; REST then still said WARNING / 10 parts, but the browser regen showed both headrest features RED
// ("2 feature(s) failed to regenerate"), the pod gone and the print layout empty (2026-11-17).  The caught op failures
// left error statuses behind; with attempt() a failure leaves nothing behind but the note from skipped().
function attempt(context is Context, id is Id, fn is function) returns boolean
{
    var attemptOk = false;
    startFeature(context, id);
    try silent
    {
        fn(id);
        attemptOk = getFeatureError(context, id) == undefined;
    }
    catch (e)
    {
        println("attempt " ~ toString(id) ~ " failed: " ~ toString(e));
    }
    if (attemptOk)
    {
        endFeature(context, id);
        return true;
    }
    abortFeature(context, id);
    clearFeatureStatus(context, id, { "withDisplayData" : true });
    return false;
}

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
// pod / blank fixing (v1.2b): M3 countersunk from the pad's HEAD side (under the foam liner) out through the pad into hex nuts
// in pockets in the pod / blank, open to the outside (see POD_HEAD_D).  (x mm along the surface from the pocket centre, +fwd; z)
// PRINT-UP directions (chosen by surface-class support study, v1.1): pad prints UPSIDE DOWN -> up = -z (pad frame);
// pod prints lying on its pad-side underside -> up = +x for the right pod (s = +1), -x for the left.
const PAD_UP = vector(0, 0, -1);
function podUp(s is number) returns Vector { return vector(s, 0, 0); }
const POD_SCREWS = [[-14, -9], [14, -12]];
const BLANK_SCREW = [0, 4];
// v1.2b SCREWS FLIPPED (owner, 2026-11-16): pod -> pad and blank -> pad screws go in from the pad's HEAD side (DIN 7991 head
// in a countersink in the shell, under the foam liner), the shank runs OUT through the pad, the hex nut is trapped in a pocket
// in the pod / blank, open to the outside: a screw that is too long can only ever point AWAY from the child's head.
// Local y along each screw axis (place() frame at the screw; the pad's head-side face is at y = R = 86.0 there):
//   head top = R + HEAD_D (0.2 below flush; pod screw 1 sits 1.0 down a Ø6.6 spot-face so its M3x20 reaches through the nut)
//   tip = head top + L | nut pocket floor = *_NUT_Y (nut 2.4 on it, tip 0.7 past the nut, >= 1.2 plastic under the pocket)
const POD_HEAD_D = [1.0, 0.2];   const POD_NUT_Y = [103.9, 103.1];   const POD_SCREW_L = 20;
const BLANK_HEAD_D = 0.2;        const BLANK_NUT_Y = 95.1;           const BLANK_SCREW_L = 12;
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
    if (!blendOnce(context, id + "rnd", qOwnedByBody(pad, EntityType.EDGE), RR, false))
        skipped(context, "pad outline round r" ~ RR);
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
        // pod (2) + blank (1) screws (v1.2b, from the HEAD side): clearance through the shell (teardrop toward PAD_UP: the pad
        // prints upside down) + DIN 7991 countersink on the head side, head top HEAD_D under the face (a Ø6.6 spot-face above it)
        const scs = concatenateArrays([POD_SCREWS, [BLANK_SCREW]]);
        const hds = concatenateArrays([POD_HEAD_D, [BLANK_HEAD_D]]);
        for (var k = 0; k < size(scs); k += 1)
        {
            const xf = screwXf(s, scs[k]);  const z = scs[k][1];  const yh = R + hds[k];
            tdHole(context, sid + ("sh" ~ k), xp(xf, vector(0, R - 2, z)), xp(xf, vector(0, R + 30, z)), M3_CLEAR / 2, PAD_UP);
            fCone(context, sid + ("sk" ~ k), { "bottomCenter" : xp(xf, vector(0, yh - 0.05, z)) * millimeter, "topCenter" : xp(xf, vector(0, yh + CSK_M3_R - 1.5, z)) * millimeter, "bottomRadius" : CSK_M3_R * millimeter, "topRadius" : 1.5 * millimeter });
            fCylinder(context, sid + ("sp" ~ k), { "bottomCenter" : xp(xf, vector(0, R - 4, z)) * millimeter, "topCenter" : xp(xf, vector(0, yh, z)) * millimeter, "radius" : 3.3 * millimeter });
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
    // v1.2: strap-slot rims rounded r0.5 (both faces of the shell)
    if (strap)
        for (var s in [1, -1])
        {
            const sf = qCreatedBy(C + (s > 0 ? "R" : "L") + "st", EntityType.FACE);
            const rest = qSubtraction(qOwnedByBody(pad, EntityType.FACE), sf);
            finishEdges(context, id + ("srim" ~ (s > 0 ? "R" : "L")), qIntersection([qAdjacent(sf, AdjacencyType.EDGE, EntityType.EDGE), qAdjacent(rest, AdjacencyType.EDGE, EntityType.EDGE)]), 0.5, false, "strap-slot rim r0.5");
        }
    return pad;
}

// =============================================== MAST (LOCAL frame) ===============================================
function chinZ(H is number) returns number { return 451 + H + BOT; }
// v1.2: the stem top sits STEM_DROP under the chin line = 0.1 under the pad's flat bottom (outline z -52.5).  At 0.3 it poked
// 0.2 mm into the pad and the pad cut left 0.2 mm feather ridges on the stem top (column-socket / channel clearances, chin
// spot-faces).  The head (column, bosses) starts HEAD_DROP under the chin, 0.3 into the stem.  (= headrest_M_mast.py)
const STEM_DROP = 0.6;  const HEAD_DROP = 0.9;
export function windowSpan(H is number) returns array
{
    const zch = chinZ(H) - STEM_DROP;  const gap = zch - (TOP + 0.3);
    const ff = min(30, 0.42 * gap);  const cf = min(26, 0.45 * gap);
    return [TOP + 0.3 + ff + 6, zch - cf - 2];
}
// M1 stem section at height z (LOCAL frame): foot (= the receiver top) -> waist -> chin flare; v1.0.1: smooth waist and
// a blended back (the v1.0 hard switch + |x| waist left creases and forced a stepped loft)
function stemSection(H is number, z is number) returns array
{
    const zch = chinZ(H) - STEM_DROP;  const zf = TOP + 0.3;  const gap = zch - zf;
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
    const zf = TOP + 0.3;  const zch = chinZ(H) - STEM_DROP;
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
    fCuboid(context, hd + "clip", { "corner1" : v3(-200, -200, BOT - HEAD_DROP), "corner2" : v3(200, 300, 100) });
    opBoolean(context, hd + "colx", { "tools" : qUnion([solids(context, hd + "col"), solids(context, hd + "ov"), solids(context, hd + "clip")]), "operationType" : BooleanOperationType.INTERSECTION });
    cleanup(context, hd + "dov", hd);
    tSolid(context, hd + "ten", BOT, TZ1, TI, R0 + LIPT, TO, TA, TA + LIPW - 0.3);
    for (var x in [-SPINE_X, SPINE_X])
    {
        const bid = hd + (x < 0 ? "bossL" : "bossR");
        fCylinder(context, bid + "c", { "bottomCenter" : v3(x, SPINE_Y, BOT - HEAD_DROP), "topCenter" : v3(x, SPINE_Y, BOSS_TOP - BOSS_R), "radius" : BOSS_R * millimeter });
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
        finishEdges(context, id + "winr", qIntersection([qAdjacent(wf, AdjacencyType.EDGE, EntityType.EDGE), qAdjacent(other, AdjacencyType.EDGE, EntityType.EDGE)]), 2, false, "mast relief-window rim r2");
    }
    tenonBackChamfer(context, id + "tbc", mast, hd, zc);
    return solids(context, id);
}

// v1.2: 0.5 mm 45 deg chamfer round the outline of the head's flush back face (tenon + column on the TO oval: the seam seen
// on the pad's back).  Back faces = head faces whose mid point lies on the TO oval, facing back; outline = their edges shared
// with any other face, except the 0.4 mm ledge under the chin (column bottom, from the clip box).
function tenonBackChamfer(context is Context, id is Id, mast is Query, hd is Id, zc is number)
{
    var back = [];
    for (var f in evaluateQuery(context, qIntersection([qOwnedByBody(mast, EntityType.FACE), qCreatedBy(hd, EntityType.FACE)])))
    {
        const tp = evFaceTangentPlane(context, { "face" : f, "parameter" : vector(0.5, 0.5) });
        const p = tp.origin / millimeter - vector(0, CY, zc);
        const rc = sqrt(p[0] * p[0] + p[1] * p[1] / (ELL * ELL));
        if (abs(rc - TO) < 0.05 && tp.normal[1] > 0.5)
            back = append(back, f);
    }
    if (size(back) == 0)
    {
        skipped(context, "tenon back-face chamfer 0.5 (back face not found)");
        return;
    }
    const bq = qUnion(back);
    const ledge = qCreatedBy(hd + "clip", EntityType.FACE);
    var edges = [];
    for (var e in evaluateQuery(context, qAdjacent(bq, AdjacencyType.EDGE, EntityType.EDGE)))
    {
        const adj = qAdjacent(e, AdjacencyType.EDGE, EntityType.FACE);
        const other = qSubtraction(adj, bq);
        if (size(evaluateQuery(context, other)) == 1 && size(evaluateQuery(context, qIntersection([other, ledge]))) == 0)
            edges = append(edges, e);
    }
    // all 7 at 0.5, else edge by edge: chamfer 0.5, chamfer 0.3, round r0.3, else left sharp (3 edges meet short neighbour faces)
    if (blendOnce(context, id + "all", qUnion(edges), 0.5, true))
        return;
    var nSmall = 0;  var nNone = 0;
    for (var e = 0; e < size(edges); e += 1)
    {
        if (blendOnce(context, id + ("e" ~ e), edges[e], 0.5, true))
            continue;
        if (blendOnce(context, id + ("f" ~ e), edges[e], 0.3, true) || blendOnce(context, id + ("r" ~ e), edges[e], 0.3, false))
            nSmall += 1;
        else
            nNone += 1;
    }
    if (nNone > 0)
        skipped(context, "tenon back-face outline chamfer: " ~ nNone ~ " of " ~ size(edges) ~ " edges left sharp" ~ (nSmall > 0 ? ", " ~ nSmall ~ " at 0.3" : ""));
    else if (nSmall > 0)
        println("tenon back-face outline chamfer: " ~ nSmall ~ " of " ~ size(edges) ~ " edges at 0.3 (0.5 does not fit)");
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
// (carries the load), 2 M3x20 from the pad's head side into hex nuts in pockets in the pod (v1.2b, flipped).
// v1.2: the PVC is locked by 3 radial ST3.5 x 6.5 pan-head self-tappers (top / outward side / 50 deg below the side (v1.2b; was 60), half-way
// down the bore) through Ø4 holes with Ø8 flat seats; the old M3x30 through-bolt clamp (2 bolts, nut traps, lands) is gone.
// HARDWARE (v1.2): M3x25 DIN 7991 x2 (chin locks), M3x20 DIN 7991 x2 per pod + M3x12 x1 per blank (both from the pad's head side, nuts in the pod / blank), M5x30 DIN 7991 x2 (receiver -> plug),
// every M screw into a hex nut; ST3.5 x 6.5 pan head x3 per pod (PVC lock). Countersink: M3 head Ø6.3 (seat Ø6.5), M5 Ø10.4 (Ø10.6).
const CSK_M3_R = 3.25;  const CSK_M5_R = 5.3;
const SEAT_CB = 3.6;  const SEAT_FL = 1.5;   // pod -> pad seats: counterbore r3.6, 45 deg rim flare 1.5 starting at POD_SEAT_RIM
const BORE_DEPTH = 26;  const BORE_CAP = 0.6;  const BORE_CH = 0.8;   // blind PVC bore (4 mm back wall), teardrop flat 0.6 over r, mouth chamfer
const K_WS = 13;                 // PVC lock screws: mm back from the collar mouth (half the bore depth)
const WS_ANG = [90, 1, -50];     // deg round the bore: +90 top, ~0 outward side (away from the pad; 1 deg = the Python mesh export), -50 below the side (v1.2b; at -60 its seat flare left a thin ridge against pod screw 2)
const WS_SEAT = 13.3;  const WS_CB = 4.0;  const WS_CH = 0.5;   // flat Ø8 seat radius from the bore axis, seat radius, hole-mouth chamfer
const WS_FL = 2.75;  // 45 deg rim flare from WS_SEAT + 0.25 up to WS_SEAT + 3.0 (the hull stands 0.7-2.3 mm over the seats within r5.5, most on the -60 deg seat at yaw 20/28: a 1.85 flare left a lip)
const LIFT_STEPS = 4;  const LIFT_STEP = 0.26;  const LIFT_G0 = 0.15;
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
// NOTE: do not name locals/params `scope`, `box` or `left` here: with them Onshape silently failed to load the whole
// Feature Studio (no feature specs, no notices -> 0 parts in every Part Studio using it), 2026-11-16.
function podLift(context is Context, id is Id, scopeId is Id, s is number)
{
    // v1.2c: slab outline = a dense ellipse round the pod (was a 4-point rectangle = a 4-control-point periodic spline wall), kept
    // inside the pad's z range (46 x 42 mm half-axes round the pod, surfaces on a 4 mm grid: the review gallery lifts 6 pods
    // x 4 steps and timed out with the 3 mm grid); every step's keep wall sits LIFT_G0 inside / clear of the footprint band's own wall (step 1 was
    // -0.02 = near-coincident faces); each step is one attempt(): it either lifts the pod and leaves exactly one pod body of
    // nearly the same volume, or it is rolled back as a whole.
    const uc = -s * (BTN_ANG + 5) * PI / 180 * R;
    var slabUZ = [];
    for (var k = 0; k < 64; k += 1)
        slabUZ = append(slabUZ, [uc + 46 * cos((360 * k / 64) * degree), 2 + 42 * sin((360 * k / 64) * degree)]);
    for (var j = 1; j <= LIFT_STEPS; j += 1)
    {
        const w = 0.3 + LIFT_STEP * j;  const g = LIFT_STEP * (j - 1) - LIFT_G0;   // keep walls at -0.15 / 0.11 / 0.37 / 0.63
        const liftStage = new box("slab");
        const stepDone = attempt(context, id + ("j" ~ j), function(jid)
            {
                const podQ = qSubtraction(solids(context, scopeId), solids(context, id));
                if (size(evaluateQuery(context, podQ)) != 1) throw regenError("pod is not one body before the step");
                const v0 = evVolume(context, { "entities" : podQ });
                bentSolid(context, jid + "slab", slabUZ, function(z, ad) { return rOut(z, ad) - 3; }, function(z, ad) { return rOut(z, ad) + w; }, 4, 4);
                if (size(evaluateQuery(context, solids(context, jid + "slab"))) != 1) throw regenError("slab not one solid");
                liftStage[] = "keep";
                bentSolid(context, jid + "keep", podFootprint(s, g), function(z, ad) { return rOut(z, ad) - 4; }, function(z, ad) { return rOut(z, ad) + 2; }, 3, 3);
                if (size(evaluateQuery(context, solids(context, jid + "keep"))) != 1) throw regenError("keep not one solid");
                liftStage[] = "slab - keep";
                opBoolean(context, jid + "cut", { "tools" : solids(context, jid + "keep"), "targets" : solids(context, jid + "slab"), "operationType" : BooleanOperationType.SUBTRACTION });
                liftStage[] = "pod - slab";
                opBoolean(context, jid + "sub", { "tools" : solids(context, jid + "slab"), "targets" : podQ, "operationType" : BooleanOperationType.SUBTRACTION });
                liftStage[] = "check";
                const podAfter = qSubtraction(solids(context, scopeId), solids(context, id));
                if (size(evaluateQuery(context, podAfter)) != 1) throw regenError("pod split into " ~ size(evaluateQuery(context, podAfter)) ~ " bodies");
                const v1 = evVolume(context, { "entities" : podAfter });
                if (v1 < v0 * 0.97 || v1 > v0 * 1.0001) throw regenError("pod volume " ~ roundToPrecision(v0 / millimeter ^ 3, 0) ~ " -> " ~ roundToPrecision(v1 / millimeter ^ 3, 0));
                const liftLeft = qSubtraction(qCreatedBy(jid, EntityType.BODY), podAfter);   // never the pod itself
                if (size(evaluateQuery(context, liftLeft)) > 0)
                    opDeleteBodies(context, jid + "del", { "entities" : liftLeft });
            });
        if (!stepDone)
            skipped(context, "pod underside lift step " ~ j ~ " of " ~ LIFT_STEPS ~ " (failed at " ~ liftStage[] ~ "; feather edge may remain)");
    }
}
export function buildPod(context is Context, id is Id, yaw is HeadrestYaw, s is number) returns Query
{
    return buildPod(context, id, yaw, s, true);
}
// withLift = false: no underside lift (the review gallery: 6 pods x 4 lift steps made it time out in the browser, 2026-11-17)
export function buildPod(context is Context, id is Id, yaw is HeadrestYaw, s is number, withLift is boolean) returns Query
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
    if (withLift)
        podLift(context, id + "lift", id, s);                        // outside the footprint: lifted to 1.3 in 45 deg steps
    triPrism(context, id + "wg", -0.05, POCKET_Y0 + 0.2, R + T + 7.6 + 3.3);
    finishEdges(context, id + "wgc", qAdjacent(qCapEntity(id + "wg" + "ex", CapType.START, EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), 0.5, true, "pod wedge lead-in chamfer 0.5");   // lead-in
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
        fCone(context, C + ("wf" ~ i), { "bottomCenter" : (A + v * (WS_SEAT + 0.25)) * millimeter, "topCenter" : (A + v * (WS_SEAT + 0.25 + WS_FL)) * millimeter, "bottomRadius" : (WS_CB - 0.05) * millimeter, "topRadius" : (WS_CB - 0.05 + WS_FL) * millimeter });
    }
    for (var k = 0; k < size(POD_SCREWS); k += 1)
    {
        const xf = screwXf(s, POD_SCREWS[k]);  const z = POD_SCREWS[k][1];
        const yFloor = POD_SEAT_Y[k];          // v1.2b: start of the round Ø7.2 mouth of the nut pocket (flared rim above it)
        const hr = POD_SEAT_RIM[key][k];       // lowest flank height over the seat round the counterbore (from the Python hull)
        tdHole(context, C + ("sh" ~ k), xp(xf, vector(0, R - 2, z)), xp(xf, vector(0, yFloor + 1, z)), M3_CLEAR / 2, podUp(s));
        fCylinder(context, C + ("sf" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + hr + 0.05, z)) * millimeter, "radius" : SEAT_CB * millimeter });
        fCone(context, C + ("sl" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor + hr, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + hr + SEAT_FL, z)) * millimeter, "bottomRadius" : (SEAT_CB - 0.02) * millimeter, "topRadius" : (SEAT_CB + SEAT_FL) * millimeter });
        fCylinder(context, C + ("so" ~ k), { "bottomCenter" : xp(xf, vector(0, yFloor + hr + SEAT_FL - 0.05, z)) * millimeter, "topCenter" : xp(xf, vector(0, yFloor + 30, z)) * millimeter, "radius" : (SEAT_CB + SEAT_FL - 0.03) * millimeter });
        // v1.2b hex NUT POCKET (AF 5.8) from POD_NUT_Y out to the round mouth, one corner toward print-up (podUp): the M3x20
        // from the head side pulls the nut onto the pocket floor; the mouth's 45 deg flare is the opening chamfer
        const ax = xd(xf, vector(0, 1, 0));  var cu = podUp(s) - ax * dot(podUp(s), ax);  cu = normalize(cu);
        const yn = POD_NUT_Y[k];  const hl = yFloor + 0.1 - yn;
        hexPrism(context, C + ("nt" ~ k), xp(xf, vector(0, yn + hl / 2, z)) * millimeter, ax, cu, NUT_M3_AF, hl, 0);
    }
    opBoolean(context, id + "sub", { "tools" : solids(context, C), "targets" : pod, "operationType" : BooleanOperationType.SUBTRACTION });
    // the pod is ONE part, always: the core ops above throw if they fail (no silent loss); a cut that left a loose sliver
    // (< 1 % of the pod) drops the sliver, anything else is an error
    const podBodies = evaluateQuery(context, solids(context, id));
    if (size(podBodies) == 0) throw regenError("pod: no body built");
    if (size(podBodies) > 1)
    {
        var vMax = 0 * millimeter ^ 3;
        for (var bd in podBodies) vMax = max(vMax, evVolume(context, { "entities" : bd }));
        var nSliver = 0;
        for (var bd in podBodies)
            if (evVolume(context, { "entities" : bd }) < vMax * 0.01)
            {
                opDeleteBodies(context, id + ("sliver" ~ nSliver), { "entities" : bd });
                nSliver += 1;
            }
        if (size(evaluateQuery(context, solids(context, id))) != 1) throw regenError("pod: " ~ size(podBodies) ~ " bodies");
        skipped(context, "pod: " ~ nSliver ~ " loose sliver(s) removed");
    }
    return solids(context, id);
}
// pocket BLANK v1.2: the triangle plug + a 1.5 mm cover FLANGE lying 0.15 over the pad's back: it hides the pocket rim and the
// 2 unused pod-screw holes (outline = hull of the triangle grown 3 mm and Ø10 discs over the holes, top edge rounded r0.6).
// v1.2b: 1 countersunk M3x12 from the pad's head side into a hex nut in a pocket in the blank.  Prints wedge-down: the flange
// underside lies on the pad (hidden) and is the only face that takes support.
const BLANK_TOP_Y = 103.57;      // flange top along the blank screw axis (rOut + 1.65; headrest_SW_mount.py)
const BLANK_FL_GAP = 0.15;  const BLANK_FL_T = 1.5;  const BLANK_FL_R = 0.6;
function blankOutline(s is number) returns array
{
    var V = [];
    for (var p in TRI) V = append(V, vector(p[0], p[1]));
    const a = norm(V[1] - V[2]);  const b = norm(V[0] - V[2]);  const c = norm(V[0] - V[1]);
    const I = (a * V[0] + b * V[1] + c * V[2]) / (a + b + c);
    const sp = (a + b + c) / 2;
    const ri = sqrt(sp * (sp - a) * (sp - b) * (sp - c)) / sp;
    const k = (ri - 5) / ri;
    var pts = [];
    for (var i = 0; i < 3; i += 1)
    {
        const w = I + (V[i] - I) * k;                                              // rounded-corner centre (as triPrism)
        pts = concatenateArrays([pts, circlePts(-s * (BTN_ANG * PI / 180 * R + w[0]), w[1], 8, 32)]);
    }
    for (var sc in POD_SCREWS)
        pts = concatenateArrays([pts, circlePts(-s * (BTN_ANG * PI / 180 * R + sc[0]), sc[1], 5, 24)]);
    return hull2d(pts);
}
export function buildBlank(context is Context, id is Id, s is number) returns Query
{
    triPrism(context, id + "tri", -0.1, POCKET_Y0 + 0.2, R + T + 7.6 + 7);
    moveSolids(context, id + "mv", id + "tri", placeXf(s * BTN_ANG));
    trimBySurface(context, id + "trim", id, s, BLANK_FL_GAP + 0.3, false);           // plug top 0.3 into the flange
    bentSolid(context, id + "fl", blankOutline(s), function(z, ad) { return rOut(z, ad) + BLANK_FL_GAP; }, function(z, ad) { return rOut(z, ad) + BLANK_FL_GAP + BLANK_FL_T; }, 2, 2);
    opBoolean(context, id + "un", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    // r0.6 round on the flange's top edge (its top face is the face furthest out along the pocket normal)
    const nb = normalize(vector(sin((s * BTN_ANG) * degree), cos((s * BTN_ANG) * degree) / ELL, 0));
    finishEdges(context, id + "rt", qAdjacent(qFarthestAlong(qOwnedByBody(solids(context, id), EntityType.FACE), nb), AdjacencyType.EDGE, EntityType.EDGE), BLANK_FL_R, false, "blank flange top edge r" ~ BLANK_FL_R);
    // v1.2b: M3x12 from the pad's head side (head in the pad); the shank comes out through the pocket floor into the plug and
    // a hex nut sits on a pocket floor at BLANK_NUT_Y, the pocket open through the flange top (0.5 mm chamfer at its mouth)
    const xf = screwXf(s, BLANK_SCREW);  const z = BLANK_SCREW[1];
    fCylinder(context, id + "cut" + "h", { "bottomCenter" : xp(xf, vector(0, R - 2, z)) * millimeter, "topCenter" : xp(xf, vector(0, BLANK_NUT_Y + 1, z)) * millimeter, "radius" : M3_CLEAR / 2 * millimeter });
    const ax = xd(xf, vector(0, 1, 0));
    hexPrism(context, id + "cut" + "n", xp(xf, vector(0, (BLANK_NUT_Y + BLANK_TOP_Y + 3) / 2, z)) * millimeter, ax, vector(0, 0, 1), NUT_M3_AF, BLANK_TOP_Y + 3 - BLANK_NUT_Y, 0);
    fCone(context, id + "cut" + "f", { "bottomCenter" : xp(xf, vector(0, BLANK_TOP_Y - 0.5, z)) * millimeter, "topCenter" : xp(xf, vector(0, BLANK_TOP_Y + 0.4, z)) * millimeter, "bottomRadius" : (NUT_M3_AF / sqrt(3)) * millimeter, "topRadius" : (NUT_M3_AF / sqrt(3) + 0.9) * millimeter });   // 45 deg mouth chamfer
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
        setVariable(context, "gbgSkipped", "");      // context variables outlive a feature: start every feature with an empty list
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
                    buildPod(context, pid, y, s, false);
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
            skipped(context, "gallery pods are shown without the 1 mm underside lift (review speed; the real parts have it)");
            reportSkipped(context, id, padQ);
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
        reportSkipped(context, id, parts["pad"]);
    }, { height : HeadrestHeight.H50, button : HeadrestButton.RIGHT, yaw : HeadrestYaw.Y20, strapSlots : true, showRods : true, printSet : false, selfTest : false });

// =============================================== CHAIR: RECEIVER IN top_back ===============================================
// top_back's own mounting holes near the receiver (measured on chair_mesh/chair_top_back.stl, LOCAL frame): the side M5
// pair at x +-37.5, z 373.4: Ø5.2 teardrop through (z 370.8-377.05), Ø8.9 counterbore from the front face (y 124) to y 130.8,
// back face y 143.9.  The centre one (x 0) lies inside the socket and is lost.  The others (z 278 / 296 / 315) are below the
// receiver and untouched.
const TB_SIDE_MOUNTS = [[-37.5, 373.4], [37.5, 373.4]];  const TB_MOUNT_SEAT_Y = 143.0;  const TB_MOUNT_SEAT_R = 5.5;
const TB_MOUNT_HOLE_D = 5.2;  const TB_MOUNT_CB_D = 8.9;  const TB_MOUNT_CB_Y = 130.8;
const TB_FILL_X = 3;
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
    finishEdges(context, id + "upf", qAdjacent(qCapEntity(id + "up", CapType.END, EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), 1.5, false, "receiver housing top edge r1.5");   // housing top edge
    rrbox(context, id + "fill", PLUG[0] - TB_FILL_X, PLUG[1] + TB_FILL_X, FRONT + 0.1, 144.5, SOCK_Z0 - 6, TOP - 0.05, 2);   // v1.2b: +-31 (was +-35: it ran 0.3 mm into the side mount holes)
    cleanup(context, id + "cl", id);
    const add = qUnion([solids(context, id + "lens"), solids(context, id + "up"), solids(context, id + "fill")]);
    const C = id + "cut";
    rrbox(context, C + "sock", PLUG[0] - SOCK_CL, PLUG[1] + SOCK_CL, PLUG[2] - SOCK_CL, PLUG[3] + SOCK_CL, SOCK_Z0, TOP + 40, 6);
    // v1.2b: keep top_back's two side M5 mounts (x +-37.5, z 373.6, bolted from the front through a counterbore) usable: the
    // housing flank (half-width 38) covered their back exits.  A Ø11 seat for the washer / nut, 0.9 mm into the original back
    // face (flat), cleared backwards through the housing; >= 3.8 mm wall to the socket.
    // + explicit re-cuts of the original holes (the housing overlaps them): Ø5.2 teardrop (tip up: top_back prints upright)
    // through, Ø8.9 counterbore from the front face to y 130.8, as measured on chair_mesh/chair_top_back.stl
    for (var m = 0; m < size(TB_SIDE_MOUNTS); m += 1)
    {
        const mx = TB_SIDE_MOUNTS[m][0];  const mz = TB_SIDE_MOUNTS[m][1];
        fCylinder(context, C + ("tbm" ~ m), { "bottomCenter" : v3(mx, TB_MOUNT_SEAT_Y, mz), "topCenter" : v3(mx, FOOT[2] + 20, mz), "radius" : TB_MOUNT_SEAT_R * millimeter });
        tdHole(context, C + ("tbh" ~ m), vector(mx, FRONT - 1, mz), vector(mx, TB_MOUNT_SEAT_Y + 0.5, mz), TB_MOUNT_HOLE_D / 2, vector(0, 0, 1));
        fCylinder(context, C + ("tbc" ~ m), { "bottomCenter" : v3(mx, FRONT - 1, mz), "topCenter" : v3(mx, TB_MOUNT_CB_Y, mz), "radius" : TB_MOUNT_CB_D / 2 * millimeter });
    }
    // socket mouth: 1 mm x 45 deg chamfer (no sharp rim; lead-in for the plug) = a box 1 mm bigger all round from TOP - 1 up,
    // vertical edges rounded to the socket's r6 + 1, bottom loop chamfered 1 mm back to the socket outline
    var mouthOk = false;
    try silent
    {
        fCuboid(context, C + "mc", { "corner1" : v3(PLUG[0] - SOCK_CL - 1, PLUG[2] - SOCK_CL - 1, TOP - 1), "corner2" : v3(PLUG[1] + SOCK_CL + 1, PLUG[3] + SOCK_CL + 1, TOP + 40) });
        opFillet(context, C + "mcf", { "entities" : qParallelEdges(qOwnedByBody(qCreatedBy(C + "mc", EntityType.BODY), EntityType.EDGE), vector(0, 0, 1)), "radius" : 7 * millimeter });
        opChamfer(context, C + "mcc", { "entities" : qAdjacent(qFarthestAlong(qOwnedByBody(qCreatedBy(C + "mc", EntityType.BODY), EntityType.FACE), vector(0, 0, -1)), AdjacencyType.EDGE, EntityType.EDGE), "chamferType" : ChamferType.EQUAL_OFFSETS, "width" : 1 * millimeter });
        mouthOk = true;
    }
    if (!mouthOk)
    {
        if (size(evaluateQuery(context, qCreatedBy(C + "mc", EntityType.BODY))) > 0)
            opDeleteBodies(context, C + "mcd", { "entities" : qCreatedBy(C + "mc", EntityType.BODY) });   // no chamfer rather than a raw step
        skipped(context, "receiver socket-mouth chamfer 1 mm");
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
        setVariable(context, "gbgSkipped", "");
        const before = size(evaluateQuery(context, qBodyType(qEverything(EntityType.BODY), BodyType.SOLID))) + (definition.standIn ? 1 : 0);   // every solid must survive
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
                const hit = evaluateQuery(context, q);
            if (size(hit) == 1) { tb = hit[0]; break; }     // freeze it: a lazy qContainsPoint re-evaluated after receiverTools would also match the new tool bodies
            }
        }
        if (size(evaluateQuery(context, tb)) != 1) throw regenError("No top_back part found: select it");
        var rxStage = "start";
        try
        {
            rxStage = "receiver tools";
            const t = receiverTools(context, id + "rx");
            rxStage = "union with top_back";
            opBoolean(context, id + "add", { "tools" : qUnion([tb, t.add]), "operationType" : BooleanOperationType.UNION });
            const inside = reclineXf() * v3(0, 140, 330);                  // in the lens, below the socket: always material
            const mq = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), inside));
            if (size(mq) != 1) throw regenError("Receiver: expected 1 merged top_back body, found " ~ size(mq));
            const merged = mq[0];
            rxStage = "socket / hole cuts";
            opBoolean(context, id + "cut", { "tools" : t.cut, "targets" : merged, "operationType" : BooleanOperationType.SUBTRACTION });
            setProperty(context, { "entities" : merged, "propertyType" : PropertyType.NAME, "value" : "top_back" });
            report(context, "top_back with receiver", merged);
            const after = size(evaluateQuery(context, qBodyType(qEverything(EntityType.BODY), BodyType.SOLID)));
            if (after != before) skipped(context, "solid body count changed " ~ before ~ " -> " ~ after);
            reportSkipped(context, id, merged);
        }
        catch (e)
        {
            // never fail silently and never leave top_back without a socket if it can be helped: report the error on the part
            // (DESCRIPTION, readable through GET .../parts) and fall back to a plain socket cut in top_back
            const msg = "receiver error at " ~ rxStage ~ ": " ~ toString(e);
            const tbPoint = reclineXf() * v3(-50, 134, 330);                 // inside top_back, outside every receiver body
            const leftovers = qSubtraction(qCreatedBy(id + "rx", EntityType.BODY), qContainsPoint(qEverything(EntityType.BODY), tbPoint));   // only bodies this feature created, never top_back
            if (size(evaluateQuery(context, leftovers)) > 0)
                opDeleteBodies(context, id + "clean", { "entities" : leftovers });
            const tbNow = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), tbPoint));
            var fbNote = "fallback failed";
            try silent
            {
                rrbox(context, id + "fsock", PLUG[0] - SOCK_CL, PLUG[1] + SOCK_CL, PLUG[2] - SOCK_CL, PLUG[3] + SOCK_CL, SOCK_Z0, TOP + 40, 6);
                opTransform(context, id + "fsockW", { "bodies" : solids(context, id + "fsock"), "transform" : reclineXf() });
                opBoolean(context, id + "fcut", { "tools" : solids(context, id + "fsock"), "targets" : qUnion(tbNow), "operationType" : BooleanOperationType.SUBTRACTION });
                fbNote = "fallback: plain socket cut, no housing";
            }
            if (size(tbNow) == 1)
                setProperty(context, { "entities" : tbNow[0], "propertyType" : PropertyType.DESCRIPTION, "value" : msg ~ " | " ~ fbNote });
            reportFeatureWarning(context, id, msg ~ " | " ~ fbNote);
        }
    }, { topBack : qNothing(), standIn : false });

// ---------------- DATA: W1 pod hull sections + PVC axis (PAD frame, right side; generator/headrest_POD11.py) ----------------
const POD_SECTIONS = {
    "12" : [
        [56.86,111.24,-7.40,56.85,111.20,-6.76,56.84,111.17,-6.12,56.83,111.13,-5.48,56.82,111.10,-4.84,56.81,111.06,-4.20,56.81,111.02,-3.56,56.80,110.99,-2.92,56.79,110.95,-2.28,56.78,110.91,-1.63,56.77,110.88,-0.99,56.76,110.84,-0.35,56.75,110.81,0.29,56.74,110.77,0.93,56.73,110.73,1.57,56.72,110.70,2.21,56.71,110.66,2.85,56.70,110.62,3.49,56.65,110.58,4.13,56.56,110.53,4.76,56.44,110.47,5.39,56.28,110.40,6.01,56.01,110.31,6.53,55.43,110.18,6.64,55.02,110.12,6.16,54.65,110.07,5.64,54.41,110.05,5.04,54.24,110.05,4.42,54.08,110.05,3.80,53.98,110.06,3.17,53.90,110.08,2.53,53.81,110.09,1.90,53.73,110.11,1.26,53.64,110.13,0.63,53.56,110.14,-0.01,53.48,110.16,-0.65,53.39,110.18,-1.28,53.31,110.19,-1.92,53.23,110.21,-2.55,53.16,110.23,-3.19,53.08,110.25,-3.83,53.01,110.26,-4.47,52.93,110.28,-5.10,52.86,110.30,-5.74,52.78,110.32,-6.38,52.71,110.34,-7.01,52.63,110.36,-7.65,52.57,110.38,-8.29,52.54,110.40,-8.93,52.59,110.45,-9.57,52.72,110.51,-10.19,52.92,110.59,-10.79,53.22,110.68,-11.35,53.59,110.78,-11.87,54.09,110.91,-12.22,54.67,111.04,-12.39,55.23,111.15,-12.18,55.59,111.20,-11.65,55.89,111.23,-11.09,56.17,111.26,-10.51,56.42,111.28,-9.92,56.60,111.29,-9.30,56.74,111.28,-8.68,56.82,111.27,-8.04],
        [63.15,109.90,-5.17,63.14,109.83,-3.92,63.12,109.75,-2.60,63.10,109.68,-1.27,63.09,109.61,0.05,63.07,109.53,1.38,63.05,109.45,2.70,63.02,109.38,4.01,62.97,109.32,4.97,62.83,109.23,6.02,62.55,109.11,7.13,62.23,109.00,8.00,61.85,108.87,8.81,61.39,108.74,9.57,60.84,108.58,10.20,60.25,108.43,10.75,59.35,108.20,11.40,58.63,108.03,11.78,57.97,107.88,12.02,57.70,107.82,12.08,57.19,107.72,11.93,56.88,107.66,11.79,56.34,107.57,11.33,55.75,107.48,10.65,55.12,107.39,9.80,54.52,107.31,8.90,53.99,107.25,7.93,53.63,107.22,7.16,53.45,107.22,6.42,53.28,107.24,5.23,53.14,107.28,4.02,53.04,107.32,2.77,52.94,107.37,1.45,52.84,107.42,0.13,52.74,107.47,-1.19,52.64,107.52,-2.52,52.54,107.57,-3.84,52.44,107.62,-5.16,52.34,107.67,-6.49,52.24,107.72,-7.81,52.20,107.77,-8.91,52.23,107.84,-10.02,52.33,107.92,-11.15,52.46,108.00,-12.11,52.57,108.05,-12.59,52.86,108.13,-12.93,53.51,108.30,-13.59,54.24,108.49,-14.19,55.00,108.68,-14.70,55.78,108.86,-15.04,56.22,108.96,-15.14,56.75,109.07,-15.15,57.12,109.15,-15.03,57.67,109.24,-14.71,58.28,109.34,-14.14,59.08,109.47,-13.31,59.82,109.58,-12.48,60.55,109.69,-11.58,61.23,109.78,-10.66,61.81,109.86,-9.74,62.31,109.91,-8.80,62.67,109.94,-7.93,62.93,109.95,-7.05,63.09,109.94,-6.19],
        [69.45,108.56,-2.94,69.44,108.47,-1.41,69.41,108.38,0.22,69.39,108.29,1.85,69.37,108.20,3.48,69.34,108.10,5.09,69.24,108.02,6.26,69.04,107.92,7.38,68.73,107.80,8.46,68.29,107.64,9.55,67.68,107.46,10.61,67.08,107.29,11.34,66.41,107.11,12.01,65.37,106.85,12.76,64.44,106.63,13.26,63.36,106.37,13.73,62.04,106.07,14.18,60.69,105.76,14.56,59.69,105.53,14.79,58.98,105.38,14.78,58.57,105.30,14.66,57.95,105.19,14.31,56.99,105.03,13.53,56.00,104.86,12.63,55.75,104.83,12.36,55.61,104.83,11.75,55.27,104.84,10.27,54.92,104.85,8.74,54.66,104.87,7.33,54.47,104.90,5.90,54.33,104.95,4.41,54.20,105.01,2.80,54.08,105.07,1.17,53.96,105.13,-0.46,53.84,105.19,-2.08,53.71,105.26,-3.71,53.59,105.32,-5.34,53.48,105.38,-6.90,53.44,105.44,-8.23,53.52,105.53,-9.55,53.69,105.63,-10.89,53.94,105.76,-12.24,54.28,105.91,-13.60,54.68,106.06,-14.92,54.83,106.12,-15.33,55.05,106.17,-15.49,55.61,106.30,-15.74,56.76,106.57,-16.14,57.69,106.78,-16.38,58.42,106.94,-16.42,59.09,107.07,-16.31,59.71,107.19,-15.97,60.79,107.38,-15.23,61.94,107.58,-14.38,63.06,107.76,-13.48,64.16,107.95,-12.51,65.16,108.11,-11.52,66.08,108.25,-10.51,66.97,108.38,-9.40,67.68,108.47,-8.40,68.30,108.55,-7.33,68.83,108.60,-6.16,69.18,108.62,-5.11,69.38,108.61,-4.08],
        [75.75,107.22,-0.71,75.74,107.14,0.71,75.72,107.06,2.13,75.70,106.98,3.55,75.68,106.90,4.97,75.64,106.82,6.39,75.47,106.70,7.80,75.13,106.56,9.17,74.62,106.38,10.49,73.93,106.17,11.71,73.04,105.92,12.79,71.99,105.65,13.71,70.82,105.36,14.46,69.56,105.06,15.05,68.25,104.76,15.52,66.91,104.45,15.91,65.56,104.15,16.22,64.19,103.84,16.46,62.81,103.54,16.63,61.43,103.24,16.77,60.06,102.95,16.67,58.81,102.72,16.03,57.63,102.51,15.26,57.13,102.47,13.97,56.78,102.47,12.59,56.46,102.48,11.21,56.15,102.49,9.82,55.90,102.51,8.42,55.71,102.55,7.01,55.58,102.59,5.59,55.47,102.65,4.18,55.36,102.70,2.76,55.25,102.75,1.34,55.15,102.81,-0.08,55.04,102.86,-1.50,54.93,102.91,-2.92,54.83,102.97,-4.33,54.72,103.02,-5.75,54.68,103.09,-7.17,54.77,103.18,-8.59,54.96,103.30,-9.99,55.25,103.43,-11.38,55.62,103.58,-12.75,56.03,103.75,-14.10,56.47,103.91,-15.44,57.17,104.12,-16.64,58.55,104.43,-16.86,59.92,104.73,-17.03,61.30,105.01,-16.87,62.50,105.23,-16.14,63.68,105.44,-15.38,64.87,105.65,-14.62,66.05,105.86,-13.85,67.22,106.07,-13.07,68.35,106.26,-12.24,69.46,106.45,-11.36,70.52,106.63,-10.42,71.52,106.79,-9.43,72.47,106.93,-8.38,73.35,107.06,-7.27,74.14,107.16,-6.09,74.81,107.24,-4.84,75.32,107.28,-3.51,75.64,107.27,-2.13],
        [82.05,105.88,1.52,82.03,105.79,3.12,82.01,105.70,4.72,81.99,105.61,6.33,81.90,105.50,7.92,81.61,105.36,9.49,81.11,105.17,11.01,80.41,104.95,12.43,79.48,104.68,13.71,78.36,104.38,14.81,77.07,104.06,15.71,75.66,103.72,16.41,74.19,103.38,16.95,72.68,103.04,17.37,71.15,102.70,17.69,69.59,102.35,17.92,68.03,102.01,18.08,66.47,101.67,18.19,64.90,101.34,18.25,63.33,101.00,18.31,61.77,100.67,18.29,60.30,100.39,17.74,59.00,100.16,16.91,58.47,100.12,15.40,58.06,100.12,13.85,57.69,100.13,12.28,57.35,100.14,10.72,57.08,100.16,9.13,56.91,100.21,7.54,56.77,100.27,5.94,56.64,100.33,4.34,56.52,100.39,2.74,56.40,100.45,1.14,56.28,100.51,-0.46,56.16,100.57,-2.06,56.04,100.63,-3.66,55.93,100.69,-5.26,55.95,100.78,-6.86,56.13,100.90,-8.45,56.44,101.05,-10.02,56.85,101.22,-11.56,57.33,101.41,-13.08,57.80,101.59,-14.60,58.33,101.78,-16.10,59.37,102.06,-17.22,60.92,102.40,-17.45,62.48,102.74,-17.59,63.95,103.03,-17.09,65.29,103.27,-16.25,66.64,103.51,-15.40,67.98,103.75,-14.55,69.32,103.99,-13.70,70.66,104.23,-12.85,71.99,104.46,-11.99,73.32,104.70,-11.12,74.62,104.93,-10.21,75.87,105.14,-9.22,77.06,105.34,-8.17,78.18,105.52,-7.03,79.21,105.67,-5.81,80.15,105.80,-4.52,80.94,105.89,-3.12,81.54,105.94,-1.64,81.92,105.94,-0.08],
        [88.35,104.54,3.75,88.33,104.44,5.53,88.30,104.34,7.31,88.18,104.22,9.08,87.82,104.05,10.81,87.21,103.83,12.47,86.36,103.57,14.01,85.25,103.26,15.36,83.93,102.92,16.51,82.45,102.55,17.43,80.85,102.18,18.12,79.20,101.80,18.65,77.50,101.41,19.03,75.78,101.04,19.27,74.04,100.66,19.42,72.30,100.28,19.52,70.56,99.91,19.58,68.82,99.54,19.61,67.08,99.17,19.62,65.34,98.80,19.59,63.59,98.43,19.55,61.90,98.10,19.12,60.47,97.84,18.24,59.78,97.78,16.61,59.28,97.76,14.90,58.88,97.77,13.16,58.50,97.78,11.42,58.25,97.83,9.66,58.09,97.89,7.88,57.94,97.95,6.11,57.80,98.02,4.34,57.67,98.08,2.56,57.53,98.15,0.79,57.40,98.22,-0.99,57.27,98.28,-2.77,57.16,98.35,-4.54,57.24,98.47,-6.32,57.50,98.62,-8.07,57.92,98.80,-9.79,58.44,99.00,-11.49,58.96,99.20,-13.18,59.49,99.40,-14.87,60.11,99.62,-16.52,61.31,99.94,-17.75,63.03,100.32,-18.00,64.77,100.70,-18.11,66.34,100.99,-17.38,67.84,101.26,-16.46,69.34,101.53,-15.53,70.83,101.80,-14.59,72.33,102.07,-13.66,73.82,102.33,-12.73,75.31,102.60,-11.79,76.80,102.87,-10.85,78.28,103.13,-9.90,79.76,103.39,-8.94,81.21,103.65,-7.93,82.59,103.88,-6.83,83.89,104.09,-5.64,85.08,104.28,-4.32,86.16,104.43,-2.91,87.07,104.54,-1.39,87.76,104.60,0.25,88.20,104.60,1.98],
        [94.65,103.20,5.98,94.63,103.09,7.93,94.52,102.96,9.87,94.13,102.78,11.78,93.45,102.54,13.59,92.49,102.24,15.27,91.26,101.90,16.74,89.81,101.53,17.99,88.17,101.13,18.97,86.40,100.71,19.68,84.57,100.29,20.22,82.69,99.88,20.54,80.79,99.46,20.72,78.88,99.05,20.82,76.97,98.64,20.86,75.06,98.24,20.86,73.15,97.83,20.82,71.24,97.43,20.76,69.33,97.03,20.68,67.41,96.63,20.60,65.51,96.23,20.50,63.62,95.84,20.20,61.96,95.53,19.40,61.06,95.43,17.67,60.48,95.41,15.81,60.03,95.42,13.91,59.65,95.44,11.99,59.43,95.50,10.05,59.27,95.56,8.10,59.10,95.63,6.16,58.95,95.71,4.21,58.81,95.78,2.26,58.66,95.85,0.32,58.51,95.93,-1.63,58.40,96.01,-3.58,58.51,96.13,-5.52,58.85,96.31,-7.44,59.37,96.52,-9.31,59.95,96.74,-11.16,60.52,96.96,-13.02,61.10,97.19,-14.87,61.76,97.42,-16.69,63.04,97.77,-18.12,64.87,98.18,-18.51,66.77,98.59,-18.62,68.48,98.91,-17.78,70.13,99.21,-16.77,71.77,99.50,-15.76,73.42,99.80,-14.75,75.06,100.09,-13.73,76.71,100.39,-12.72,78.35,100.68,-11.70,79.99,100.98,-10.68,81.63,101.27,-9.66,83.26,101.56,-8.63,84.90,101.85,-7.59,86.52,102.14,-6.55,88.09,102.42,-5.42,89.58,102.67,-4.19,90.95,102.88,-2.81,92.17,103.06,-1.29,93.20,103.19,0.36,93.98,103.26,2.15,94.47,103.27,4.04],
        [100.95,101.86,8.21,100.89,101.73,10.31,100.61,101.58,12.08,100.07,101.37,13.83,99.33,101.13,15.41,98.36,100.84,16.90,97.18,100.52,18.22,95.84,100.18,19.35,94.33,99.81,20.27,92.69,99.42,20.98,90.75,98.97,21.58,88.85,98.55,21.90,86.59,98.07,22.07,84.18,97.55,22.11,81.58,97.00,22.04,78.89,96.44,21.93,76.17,95.87,21.78,73.43,95.29,21.62,70.70,94.72,21.44,67.95,94.15,21.26,65.87,93.71,21.10,64.11,93.36,20.78,63.76,93.29,20.61,63.09,93.19,19.91,62.42,93.10,18.84,61.85,93.06,17.33,61.29,93.06,15.09,60.86,93.09,12.94,60.62,93.17,10.44,60.38,93.27,7.69,60.17,93.37,4.94,59.96,93.47,2.13,59.75,93.58,-0.68,59.64,93.67,-2.86,59.79,93.81,-4.74,60.20,94.00,-6.71,60.92,94.28,-9.13,61.74,94.60,-11.78,62.57,94.91,-14.42,63.24,95.17,-16.47,63.70,95.31,-17.32,64.78,95.60,-18.40,65.22,95.71,-18.81,65.93,95.87,-18.92,67.99,96.32,-19.16,68.65,96.46,-19.13,69.97,96.71,-18.52,71.84,97.04,-17.38,74.23,97.47,-15.92,76.61,97.90,-14.46,78.99,98.33,-13.00,81.38,98.76,-11.54,83.76,99.19,-10.08,86.13,99.61,-8.62,88.50,100.04,-7.16,90.85,100.46,-5.70,93.10,100.86,-4.27,94.83,101.16,-3.02,96.43,101.43,-1.65,97.70,101.62,-0.25,98.90,101.79,1.38,99.77,101.89,2.98,100.40,101.93,4.65,100.80,101.92,6.42],
        [105.78,100.26,9.50,105.67,100.13,11.43,105.29,99.95,13.32,104.67,99.72,15.13,103.80,99.44,16.83,102.70,99.13,18.40,101.40,98.78,19.78,99.92,98.40,20.97,98.30,98.00,21.93,96.55,97.59,22.65,94.72,97.18,23.12,92.84,96.77,23.32,90.55,96.28,23.26,87.73,95.69,23.07,84.82,95.09,22.86,81.91,94.48,22.65,79.00,93.87,22.44,76.09,93.27,22.23,73.18,92.66,22.02,70.27,92.05,21.81,67.36,91.44,21.60,65.53,91.07,21.38,65.07,90.98,21.21,64.35,90.86,20.55,63.61,90.76,19.55,63.04,90.71,18.22,62.48,90.71,16.00,62.08,90.74,13.86,61.83,90.83,11.07,61.58,90.94,8.14,61.35,91.04,5.21,61.13,91.16,2.23,60.91,91.27,-0.69,60.91,91.37,-2.58,61.20,91.54,-4.56,61.81,91.79,-6.81,62.68,92.12,-9.62,63.56,92.46,-12.43,64.43,92.79,-15.24,65.04,93.02,-16.97,65.46,93.14,-17.63,67.03,93.56,-19.18,67.24,93.61,-19.35,68.42,93.87,-19.51,69.99,94.21,-19.62,70.91,94.40,-19.42,72.76,94.73,-18.34,75.19,95.17,-16.87,77.61,95.61,-15.39,80.04,96.04,-13.92,82.47,96.48,-12.45,84.90,96.92,-10.97,87.34,97.36,-9.49,89.88,97.81,-7.95,92.41,98.27,-6.41,94.95,98.73,-4.87,97.46,99.18,-3.34,99.67,99.57,-1.96,101.22,99.84,-0.73,102.51,100.04,0.61,103.64,100.20,2.17,104.55,100.30,3.88,105.21,100.34,5.69,105.63,100.33,7.58],
        [106.46,97.85,9.37,106.30,97.70,11.61,105.81,97.47,13.79,104.97,97.19,15.85,103.82,96.84,17.75,102.38,96.44,19.43,100.70,96.01,20.86,98.81,95.55,21.98,96.77,95.07,22.79,94.63,94.59,23.24,92.43,94.12,23.35,90.23,93.65,23.30,88.04,93.19,23.19,85.84,92.73,23.07,83.64,92.27,22.94,81.44,91.81,22.80,79.25,91.36,22.65,77.05,90.90,22.49,74.86,90.44,22.34,72.66,89.98,22.19,70.47,89.52,22.03,68.27,89.06,21.88,66.10,88.62,21.61,64.63,88.39,19.94,63.88,88.35,17.83,63.41,88.36,15.63,63.19,88.44,13.40,62.99,88.52,11.16,62.80,88.59,8.92,62.62,88.68,6.68,62.45,88.76,4.44,62.28,88.84,2.20,62.12,88.93,-0.04,62.24,89.08,-2.28,62.70,89.29,-4.47,63.36,89.54,-6.60,64.02,89.80,-8.74,64.68,90.05,-10.87,65.34,90.31,-13.00,66.00,90.56,-15.14,66.75,90.83,-17.24,68.20,91.23,-18.89,70.03,91.67,-19.85,72.22,92.14,-19.81,74.16,92.49,-18.74,76.06,92.83,-17.58,77.95,93.17,-16.42,79.85,93.51,-15.26,81.74,93.85,-14.10,83.64,94.19,-12.94,85.53,94.53,-11.77,87.42,94.87,-10.60,89.31,95.21,-9.44,91.20,95.55,-8.27,93.09,95.89,-7.09,94.98,96.23,-5.92,96.86,96.56,-4.73,98.72,96.89,-3.51,100.47,97.19,-2.14,102.16,97.48,-0.68,103.64,97.70,1.00,104.83,97.85,2.90,105.71,97.93,4.96,106.26,97.93,7.14],
        [106.98,95.41,9.24,106.83,95.26,11.46,106.33,95.04,13.63,105.51,94.75,15.68,104.37,94.41,17.57,102.96,94.02,19.25,101.29,93.59,20.67,99.43,93.13,21.80,97.41,92.66,22.62,95.28,92.18,23.09,93.10,91.71,23.22,90.92,91.25,23.22,88.74,90.78,23.17,86.55,90.33,23.06,84.37,89.87,22.95,82.19,89.41,22.83,80.01,88.95,22.71,77.82,88.50,22.59,75.64,88.04,22.46,73.46,87.58,22.33,71.28,87.13,22.20,69.10,86.67,22.06,66.94,86.22,21.85,65.54,86.02,20.13,64.90,86.00,18.00,64.55,86.04,15.80,64.36,86.12,13.57,64.17,86.20,11.35,63.97,86.27,9.13,63.80,86.36,6.90,63.63,86.44,4.68,63.46,86.52,2.45,63.36,86.62,0.22,63.62,86.79,-1.98,64.20,87.03,-4.13,64.85,87.28,-6.25,65.51,87.54,-8.36,66.17,87.79,-10.48,66.83,88.04,-12.60,67.48,88.30,-14.72,68.16,88.55,-16.83,69.38,88.91,-18.63,71.04,89.33,-19.98,73.22,89.80,-19.98,75.22,90.18,-19.14,77.10,90.52,-17.98,78.98,90.85,-16.82,80.86,91.19,-15.66,82.73,91.53,-14.50,84.61,91.86,-13.33,86.49,92.20,-12.17,88.36,92.53,-11.00,90.23,92.87,-9.83,92.10,93.20,-8.65,93.96,93.54,-7.46,95.81,93.87,-6.26,97.63,94.19,-5.01,99.35,94.48,-3.62,101.06,94.76,-2.21,102.74,95.04,-0.76,104.19,95.26,0.92,105.37,95.41,2.81,106.24,95.49,4.86,106.78,95.48,7.02],
        [107.50,92.97,9.11,107.35,92.82,11.31,106.86,92.60,13.46,106.05,92.32,15.50,104.93,91.98,17.38,103.54,91.59,19.05,101.90,91.17,20.48,100.06,90.71,21.62,98.06,90.25,22.45,95.96,89.77,22.94,93.80,89.31,23.09,91.63,88.85,23.09,89.47,88.38,23.08,87.30,87.93,23.03,85.13,87.47,22.93,82.97,87.02,22.83,80.80,86.56,22.72,78.64,86.11,22.60,76.47,85.66,22.48,74.31,85.20,22.36,72.15,84.75,22.24,69.98,84.29,22.12,67.82,83.84,22.00,66.47,83.65,20.29,65.93,83.65,18.15,65.72,83.72,15.95,65.53,83.80,13.74,65.34,83.87,11.54,65.15,83.95,9.33,64.98,84.03,7.12,64.81,84.12,4.92,64.64,84.20,2.71,64.65,84.32,0.50,65.05,84.52,-1.66,65.69,84.77,-3.77,66.35,85.02,-5.87,67.00,85.27,-7.97,67.65,85.52,-10.08,68.30,85.78,-12.18,68.95,86.03,-14.28,69.61,86.28,-16.38,70.55,86.58,-18.34,72.10,87.00,-19.86,74.23,87.46,-20.00,76.30,87.87,-19.46,78.16,88.20,-18.31,80.02,88.53,-17.15,81.88,88.87,-15.99,83.73,89.20,-14.83,85.59,89.53,-13.67,87.44,89.86,-12.50,89.29,90.19,-11.33,91.14,90.52,-10.15,92.98,90.85,-8.96,94.81,91.17,-7.76,96.57,91.48,-6.45,98.27,91.76,-5.06,99.97,92.05,-3.67,101.67,92.34,-2.27,103.32,92.61,-0.83,104.75,92.83,0.84,105.91,92.97,2.72,106.77,93.04,4.76,107.30,93.04,6.91],
        [108.01,90.52,8.98,107.87,90.38,11.17,107.39,90.16,13.30,106.59,89.88,15.32,105.49,89.55,17.19,104.12,89.17,18.86,102.51,88.75,20.28,100.69,88.30,21.43,98.72,87.84,22.27,96.64,87.37,22.78,94.50,86.90,22.96,92.35,86.45,22.96,90.20,85.99,22.95,88.06,85.53,22.94,85.91,85.08,22.90,83.76,84.63,22.80,81.61,84.18,22.70,79.47,83.73,22.60,77.32,83.28,22.49,75.17,82.83,22.37,73.03,82.38,22.25,70.88,81.93,22.13,68.74,81.48,22.02,67.43,81.28,20.43,67.08,81.32,18.27,66.89,81.40,16.08,66.70,81.48,13.90,66.51,81.55,11.71,66.32,81.63,9.52,66.15,81.71,7.33,65.99,81.80,5.15,65.83,81.88,2.96,66.00,82.03,0.78,66.55,82.26,-1.34,67.19,82.51,-3.42,67.84,82.76,-5.50,68.49,83.01,-7.59,69.13,83.26,-9.67,69.78,83.51,-11.76,70.43,83.76,-13.84,71.07,84.01,-15.92,71.82,84.27,-17.97,73.25,84.66,-19.57,75.25,85.11,-20.00,77.36,85.54,-19.72,79.21,85.88,-18.60,81.05,86.21,-17.45,82.89,86.54,-16.29,84.73,86.87,-15.13,86.56,87.19,-13.97,88.39,87.52,-12.79,90.21,87.84,-11.62,92.03,88.17,-10.43,93.83,88.48,-9.21,95.54,88.77,-7.86,97.22,89.06,-6.48,98.90,89.34,-5.10,100.59,89.63,-3.72,102.27,89.91,-2.33,103.91,90.18,-0.89,105.32,90.39,0.77,106.46,90.53,2.64,107.30,90.60,4.67,107.82,90.60,6.80],
        [108.53,88.08,8.85,108.43,87.96,10.74,108.07,87.78,12.60,107.48,87.56,14.38,106.64,87.29,16.07,105.59,86.99,17.63,104.35,86.65,19.02,102.93,86.28,20.22,101.36,85.89,21.22,99.67,85.49,21.98,97.89,85.09,22.51,96.06,84.68,22.79,93.76,84.19,22.83,90.95,83.60,22.82,88.14,83.00,22.81,85.43,82.42,22.80,82.73,81.86,22.68,79.91,81.26,22.55,77.08,80.67,22.42,74.36,80.10,22.29,72.38,79.68,22.18,70.40,79.27,22.07,69.14,79.00,21.99,68.84,78.96,21.68,68.65,78.93,21.36,68.42,78.94,20.41,68.19,79.03,17.76,67.94,79.13,14.91,67.69,79.23,12.06,67.46,79.33,9.23,67.24,79.44,6.36,67.07,79.54,3.87,67.18,79.65,2.26,67.60,79.83,0.46,68.45,80.16,-2.28,69.30,80.48,-5.01,70.14,80.81,-7.75,70.99,81.14,-10.48,71.84,81.46,-13.21,72.69,81.79,-15.95,73.34,82.03,-17.86,73.82,82.18,-18.68,74.97,82.48,-19.83,75.16,82.53,-19.97,75.86,82.68,-20.00,77.72,83.08,-20.00,78.65,83.26,-19.80,79.84,83.48,-19.13,82.25,83.91,-17.60,84.65,84.34,-16.07,87.04,84.77,-14.54,89.41,85.19,-12.99,91.61,85.58,-11.53,93.47,85.90,-10.12,95.68,86.27,-8.30,97.89,86.64,-6.49,100.10,87.01,-4.67,102.31,87.39,-2.86,104.13,87.69,-1.32,105.40,87.89,0.07,106.49,88.03,1.63,107.36,88.13,3.32,107.99,88.17,5.10,108.39,88.15,6.95],
        [109.05,85.64,8.72,108.91,85.49,10.87,108.45,85.28,12.96,107.68,85.01,14.96,106.62,84.69,16.80,105.29,84.32,18.46,103.72,83.91,19.88,101.96,83.47,21.04,100.04,83.02,21.91,98.00,82.55,22.47,95.91,82.10,22.69,93.80,81.65,22.70,91.69,81.20,22.69,89.58,80.75,22.68,87.47,80.30,22.68,85.35,79.85,22.67,83.24,79.41,22.64,81.13,78.96,22.54,79.02,78.52,22.44,76.91,78.08,22.34,74.80,77.63,22.24,72.69,77.19,22.14,70.58,76.75,22.04,69.59,76.61,20.66,69.41,76.69,18.52,69.22,76.76,16.37,69.03,76.84,14.22,68.85,76.91,12.07,68.67,76.99,9.92,68.51,77.07,7.77,68.35,77.15,5.62,68.41,77.28,3.47,68.92,77.50,1.39,69.55,77.75,-0.66,70.19,77.99,-2.71,70.82,78.24,-4.76,71.46,78.48,-6.81,72.10,78.73,-8.85,72.73,78.97,-10.90,73.37,79.22,-12.95,74.00,79.46,-15.00,74.64,79.70,-17.05,75.55,80.00,-18.96,77.26,80.42,-20.00,79.37,80.87,-19.97,81.28,81.23,-19.11,83.08,81.55,-17.96,84.87,81.87,-16.80,86.66,82.18,-15.64,88.44,82.50,-14.46,90.21,82.81,-13.27,91.90,83.10,-11.96,93.56,83.38,-10.60,95.21,83.66,-9.24,96.87,83.94,-7.88,98.52,84.22,-6.53,100.17,84.50,-5.17,101.83,84.78,-3.81,103.48,85.05,-2.45,105.08,85.32,-1.02,106.44,85.52,0.63,107.55,85.65,2.48,108.36,85.72,4.48,108.87,85.72,6.57],
        [109.57,83.20,8.59,109.44,83.06,10.71,108.99,82.85,12.78,108.23,82.58,14.76,107.19,82.26,16.59,105.89,81.90,18.24,104.36,81.50,19.66,102.63,81.07,20.83,100.74,80.62,21.71,98.74,80.16,22.29,96.67,79.71,22.55,94.59,79.26,22.57,92.50,78.82,22.56,90.41,78.38,22.55,88.33,77.93,22.55,86.24,77.49,22.54,84.15,77.05,22.54,82.07,76.60,22.51,79.98,76.17,22.42,77.89,75.73,22.32,75.81,75.29,22.22,73.72,74.85,22.13,71.64,74.41,22.03,70.74,74.30,20.54,70.55,74.38,18.41,70.37,74.45,16.29,70.18,74.53,14.16,70.00,74.60,12.04,69.83,74.68,9.91,69.67,74.76,7.79,69.55,74.85,5.66,69.86,75.03,3.56,70.48,75.27,1.54,71.11,75.51,-0.49,71.74,75.75,-2.51,72.36,75.99,-4.54,72.99,76.23,-6.56,73.62,76.48,-8.59,74.25,76.72,-10.61,74.88,76.96,-12.63,75.50,77.20,-14.66,76.13,77.44,-16.68,76.88,77.71,-18.66,78.37,78.10,-20.00,80.46,78.54,-20.00,82.39,78.91,-19.27,84.16,79.23,-18.12,85.92,79.54,-16.96,87.68,79.85,-15.80,89.39,80.15,-14.56,91.04,80.43,-13.22,92.67,80.70,-11.88,94.31,80.98,-10.54,95.94,81.25,-9.20,97.58,81.53,-7.86,99.21,81.80,-6.52,100.85,82.08,-5.17,102.48,82.36,-3.83,104.12,82.63,-2.49,105.68,82.89,-1.06,107.02,83.08,0.59,108.10,83.21,2.42,108.89,83.28,4.39,109.39,83.27,6.47],
        [110.09,80.76,8.46,109.96,80.62,10.55,109.52,80.41,12.60,108.79,80.15,14.56,107.77,79.84,16.37,106.50,79.48,18.01,105.00,79.09,19.43,103.31,78.66,20.61,101.46,78.22,21.51,99.49,77.77,22.11,97.46,77.32,22.41,95.40,76.88,22.44,93.34,76.45,22.43,91.28,76.01,22.42,89.22,75.57,22.42,87.16,75.13,22.41,85.10,74.70,22.41,83.04,74.26,22.40,80.98,73.82,22.39,78.92,73.39,22.30,76.86,72.96,22.20,74.80,72.52,22.11,72.74,72.09,22.01,71.87,72.00,20.32,71.69,72.07,18.22,71.51,72.14,16.13,71.33,72.22,14.03,71.14,72.29,11.93,70.99,72.37,9.83,70.83,72.45,7.74,70.90,72.58,5.64,71.44,72.80,3.62,72.06,73.04,1.62,72.68,73.28,-0.38,73.30,73.52,-2.37,73.92,73.75,-4.37,74.54,73.99,-6.37,75.16,74.23,-8.37,75.78,74.47,-10.37,76.40,74.71,-12.37,77.02,74.95,-14.36,77.64,75.19,-16.36,78.30,75.43,-18.35,79.53,75.78,-19.98,81.59,76.22,-20.00,83.52,76.60,-19.38,85.26,76.91,-18.23,86.98,77.21,-17.05,88.62,77.49,-15.76,90.23,77.76,-14.44,91.85,78.03,-13.12,93.46,78.31,-11.79,95.08,78.58,-10.47,96.69,78.85,-9.14,98.30,79.12,-7.82,99.92,79.39,-6.49,101.53,79.67,-5.17,103.15,79.94,-3.84,104.76,80.21,-2.52,106.29,80.46,-1.09,107.60,80.65,0.54,108.65,80.78,2.36,109.43,80.84,4.31,109.91,80.83,6.36],
        [110.61,78.31,8.33,110.48,78.18,10.40,110.06,77.98,12.42,109.34,77.72,14.35,108.35,77.41,16.15,107.11,77.06,17.78,105.64,76.68,19.20,103.99,76.26,20.38,102.17,75.83,21.29,100.25,75.38,21.92,98.25,74.94,22.26,96.21,74.50,22.31,94.18,74.07,22.30,92.15,73.64,22.29,90.11,73.21,22.29,88.08,72.78,22.28,86.05,72.35,22.28,84.01,71.91,22.27,81.98,71.48,22.27,79.95,71.05,22.26,77.92,70.62,22.19,75.88,70.19,22.09,73.86,69.77,21.96,73.01,69.69,20.11,72.83,69.76,18.04,72.65,69.84,15.97,72.47,69.91,13.90,72.30,69.98,11.83,72.14,70.06,9.76,72.03,70.15,7.69,72.42,70.34,5.66,73.03,70.57,3.69,73.64,70.81,1.72,74.25,71.05,-0.26,74.86,71.28,-2.23,75.48,71.52,-4.20,76.09,71.75,-6.17,76.70,71.99,-8.15,77.31,72.22,-10.12,77.92,72.46,-12.09,78.54,72.70,-14.06,79.15,72.93,-16.04,79.77,73.17,-18.01,80.75,73.47,-19.79,82.71,73.90,-20.00,84.63,74.28,-19.46,86.29,74.57,-18.23,87.88,74.84,-16.93,89.47,75.10,-15.62,91.07,75.37,-14.31,92.66,75.64,-13.00,94.25,75.91,-11.70,95.85,76.18,-10.39,97.44,76.45,-9.08,99.03,76.72,-7.77,100.63,76.98,-6.47,102.22,77.25,-5.16,103.81,77.52,-3.85,105.41,77.79,-2.54,106.90,78.03,-1.12,108.18,78.22,0.50,109.21,78.34,2.30,109.97,78.40,4.24,110.44,78.39,6.26],
        [111.13,75.87,8.20,111.01,75.74,10.23,110.59,75.54,12.23,109.90,75.29,14.14,108.93,74.99,15.92,107.73,74.65,17.54,106.30,74.27,18.95,104.68,73.86,20.14,102.91,73.43,21.07,101.02,73.00,21.73,99.05,72.56,22.10,97.05,72.13,22.18,95.05,71.71,22.17,93.05,71.28,22.16,91.04,70.85,22.16,89.04,70.43,22.15,87.03,70.00,22.15,85.03,69.58,22.14,83.03,69.15,22.14,81.02,68.73,22.13,79.02,68.30,22.13,77.01,67.88,22.08,75.11,67.49,21.75,74.15,67.38,19.95,73.97,67.45,17.91,73.80,67.52,15.87,73.62,67.60,13.83,73.45,67.67,11.79,73.30,67.75,9.75,73.41,67.88,7.72,73.99,68.11,5.77,74.59,68.34,3.82,75.20,68.57,1.88,75.80,68.80,-0.06,76.40,69.04,-2.01,77.01,69.27,-3.95,77.61,69.50,-5.90,78.21,69.73,-7.84,78.82,69.97,-9.78,79.42,70.20,-11.73,80.02,70.43,-13.67,80.63,70.66,-15.62,81.23,70.90,-17.56,81.99,71.16,-19.44,83.74,71.55,-19.87,85.64,71.93,-19.31,87.22,72.19,-18.02,88.79,72.46,-16.74,90.36,72.72,-15.45,91.93,72.99,-14.16,93.50,73.25,-12.87,95.07,73.52,-11.58,96.64,73.78,-10.29,98.21,74.05,-9.01,99.78,74.31,-7.72,101.35,74.58,-6.43,102.92,74.84,-5.14,104.49,75.11,-3.85,106.06,75.37,-2.56,107.52,75.61,-1.14,108.76,75.78,0.47,109.77,75.90,2.25,110.50,75.95,4.16,110.96,75.94,6.16],
        [111.49,73.40,8.07,111.37,73.26,10.06,110.97,73.07,12.01,110.29,72.83,13.88,109.36,72.54,15.63,108.19,72.20,17.22,106.80,71.83,18.62,105.23,71.44,19.80,103.51,71.02,20.73,101.67,70.59,21.39,99.75,70.17,21.78,97.80,69.74,21.94,95.84,69.32,21.98,93.88,68.91,22.00,91.92,68.49,22.01,89.96,68.07,22.02,88.00,67.66,22.01,86.04,67.24,22.01,84.08,66.82,22.01,82.12,66.41,22.00,80.16,65.99,22.00,78.20,65.57,21.99,76.42,65.22,21.58,75.44,65.10,19.83,75.12,65.14,17.88,74.95,65.21,15.88,74.78,65.28,13.89,74.62,65.35,11.89,74.50,65.44,9.89,74.92,65.63,7.95,75.51,65.86,6.04,76.10,66.08,4.14,76.69,66.31,2.24,77.28,66.54,0.34,77.87,66.77,-1.56,78.46,66.99,-3.47,79.05,67.22,-5.37,79.64,67.45,-7.27,80.24,67.68,-9.17,80.83,67.90,-11.07,81.42,68.13,-12.98,82.01,68.36,-14.88,82.60,68.59,-16.78,83.21,68.82,-18.67,84.65,69.16,-19.33,86.50,69.52,-18.75,88.05,69.78,-17.51,89.60,70.05,-16.26,91.14,70.31,-15.01,92.69,70.57,-13.76,94.24,70.83,-12.52,95.78,71.09,-11.27,97.33,71.35,-10.02,98.87,71.62,-8.77,100.42,71.88,-7.52,101.96,72.14,-6.27,103.51,72.40,-5.02,105.04,72.66,-3.76,106.57,72.92,-2.49,107.99,73.14,-1.09,109.20,73.31,0.50,110.17,73.43,2.25,110.88,73.48,4.12,111.33,73.47,6.07],
        [108.75,70.26,7.91,108.64,70.14,9.75,108.26,69.96,11.55,107.62,69.73,13.26,106.72,69.46,14.86,105.60,69.14,16.29,104.29,68.80,17.55,102.83,68.43,18.61,101.24,68.05,19.46,99.57,67.66,20.14,97.84,67.26,20.66,96.07,66.86,21.01,94.28,66.47,21.23,92.48,66.08,21.39,90.67,65.69,21.50,88.87,65.30,21.57,87.06,64.92,21.62,85.26,64.53,21.66,83.45,64.15,21.69,81.64,63.76,21.71,79.84,63.38,21.73,78.03,62.99,21.75,77.07,62.87,20.23,76.34,62.80,18.56,76.18,62.87,16.72,76.02,62.93,14.88,75.87,63.00,13.04,75.74,63.07,11.20,76.08,63.24,9.40,76.62,63.45,7.65,77.17,63.66,5.90,77.71,63.87,4.14,78.26,64.08,2.39,78.80,64.29,0.64,79.34,64.50,-1.12,79.89,64.71,-2.87,80.43,64.91,-4.62,80.98,65.12,-6.38,81.52,65.33,-8.13,82.06,65.54,-9.88,82.61,65.75,-11.64,83.15,65.96,-13.39,83.70,66.17,-15.14,84.24,66.38,-16.89,84.92,66.61,-18.42,86.67,66.95,-17.94,88.17,67.22,-16.92,89.57,67.45,-15.74,90.97,67.69,-14.56,92.37,67.92,-13.38,93.76,68.15,-12.17,95.14,68.38,-10.97,96.49,68.60,-9.74,97.85,68.82,-8.49,99.16,69.04,-7.22,100.43,69.23,-5.88,101.67,69.42,-4.53,102.92,69.62,-3.18,104.16,69.81,-1.83,105.39,70.00,-0.46,106.53,70.16,0.98,107.46,70.28,2.57,108.15,70.33,4.28,108.59,70.33,6.08],
        [105.24,66.97,7.75,105.15,66.86,9.38,104.83,66.70,10.99,104.27,66.50,12.52,103.48,66.26,13.94,102.48,65.98,15.22,101.32,65.67,16.34,100.02,65.35,17.29,98.63,65.01,18.09,97.17,64.66,18.77,95.67,64.31,19.34,94.13,63.96,19.81,92.56,63.61,20.16,90.98,63.26,20.42,89.38,62.91,20.61,87.78,62.56,20.77,86.18,62.21,20.90,84.58,61.86,21.00,82.97,61.52,21.08,81.36,61.17,21.15,79.76,60.83,21.20,78.59,60.62,20.43,77.79,60.53,19.00,77.40,60.53,17.44,77.25,60.59,15.80,77.12,60.64,14.16,76.99,60.71,12.52,77.22,60.84,10.91,77.70,61.03,9.35,78.19,61.21,7.79,78.67,61.40,6.23,79.16,61.59,4.67,79.64,61.77,3.11,80.12,61.96,1.55,80.61,62.15,-0.01,81.09,62.33,-1.57,81.58,62.52,-3.13,82.06,62.71,-4.69,82.55,62.89,-6.25,83.03,63.08,-7.81,83.51,63.27,-9.38,84.00,63.45,-10.94,84.49,63.64,-12.50,84.99,63.83,-14.05,85.53,64.03,-15.59,86.45,64.25,-16.20,87.84,64.50,-15.36,88.99,64.69,-14.20,90.10,64.86,-13.00,91.21,65.03,-11.79,92.31,65.20,-10.59,93.42,65.37,-9.39,94.53,65.54,-8.18,95.64,65.71,-6.98,96.74,65.88,-5.77,97.85,66.06,-4.57,98.96,66.23,-3.36,100.06,66.39,-2.15,101.15,66.56,-0.93,102.22,66.72,0.30,103.24,66.87,1.58,104.07,66.97,3.00,104.69,67.02,4.52,105.08,67.02,6.11],
        [101.73,63.67,7.58,101.68,63.58,8.95,101.43,63.46,10.30,101.00,63.30,11.59,100.37,63.10,12.80,99.58,62.87,13.90,98.64,62.62,14.88,97.59,62.35,15.72,96.46,62.07,16.45,95.27,61.79,17.09,94.05,61.50,17.66,92.81,61.21,18.16,91.54,60.91,18.62,90.26,60.62,19.01,88.95,60.32,19.32,87.63,60.03,19.57,86.30,59.74,19.77,84.96,59.44,19.94,83.63,59.15,20.09,82.29,58.86,20.21,80.95,58.57,20.32,79.78,58.34,20.01,79.09,58.26,18.81,78.56,58.21,17.57,78.44,58.26,16.20,78.33,58.31,14.83,78.23,58.36,13.46,78.45,58.48,12.12,78.86,58.63,10.81,79.26,58.79,9.51,79.67,58.95,8.20,80.07,59.10,6.90,80.48,59.26,5.59,80.88,59.41,4.28,81.29,59.57,2.98,81.69,59.73,1.67,82.10,59.88,0.37,82.50,60.04,-0.94,82.91,60.20,-2.25,83.32,60.35,-3.55,83.72,60.51,-4.86,84.15,60.67,-6.15,84.60,60.83,-7.44,85.11,61.01,-8.71,85.72,61.20,-9.93,86.35,61.40,-11.13,87.15,61.60,-11.69,88.19,61.78,-10.80,89.19,61.94,-9.88,90.11,62.08,-8.87,91.04,62.22,-7.86,91.97,62.37,-6.85,92.89,62.51,-5.84,93.81,62.65,-4.83,94.74,62.79,-3.82,95.65,62.93,-2.80,96.57,63.07,-1.79,97.48,63.21,-0.76,98.38,63.35,0.27,99.27,63.48,1.31,100.09,63.60,2.41,100.76,63.68,3.60,101.27,63.72,4.88,101.59,63.71,6.21],
        [98.22,60.37,7.42,98.20,60.31,8.50,98.03,60.22,9.58,97.73,60.10,10.61,97.28,59.95,11.60,96.72,59.78,12.51,96.01,59.59,13.32,95.23,59.38,14.04,94.37,59.17,14.69,93.47,58.94,15.25,92.53,58.72,15.77,91.58,58.49,16.24,90.61,58.26,16.68,89.63,58.03,17.09,88.63,57.80,17.47,87.63,57.56,17.83,86.61,57.33,18.14,85.58,57.10,18.40,84.54,56.86,18.63,83.49,56.63,18.83,82.44,56.40,18.99,81.39,56.17,19.16,80.49,56.00,18.75,79.94,55.93,17.82,79.65,55.92,16.79,79.55,55.96,15.71,79.47,56.00,14.62,79.62,56.09,13.56,79.94,56.21,12.52,80.26,56.34,11.49,80.58,56.46,10.45,80.90,56.58,9.42,81.22,56.71,8.38,81.54,56.83,7.35,81.86,56.95,6.32,82.18,57.08,5.28,82.50,57.20,4.25,82.82,57.33,3.21,83.15,57.45,2.18,83.51,57.58,1.16,83.93,57.72,0.16,84.42,57.88,-0.80,84.92,58.04,-1.75,85.42,58.19,-2.71,85.92,58.35,-3.66,86.43,58.51,-4.62,86.93,58.67,-5.57,87.43,58.83,-6.53,88.23,58.98,-6.29,89.08,59.13,-5.63,89.91,59.27,-4.93,90.64,59.38,-4.13,91.37,59.49,-3.33,92.09,59.60,-2.52,92.81,59.71,-1.71,93.53,59.82,-0.90,94.25,59.93,-0.08,94.96,60.04,0.73,95.67,60.14,1.56,96.37,60.25,2.39,96.98,60.33,3.29,97.48,60.38,4.25,97.85,60.41,5.27,98.10,60.41,6.33],
        [94.71,57.08,7.25,94.71,57.03,8.05,94.61,56.97,8.84,94.44,56.89,9.62,94.18,56.80,10.37,93.81,56.68,11.07,93.37,56.55,11.72,92.84,56.41,12.30,92.26,56.26,12.83,91.64,56.10,13.31,90.98,55.94,13.75,90.31,55.77,14.16,89.63,55.61,14.53,88.93,55.44,14.89,88.22,55.27,15.23,87.51,55.10,15.56,86.79,54.93,15.87,86.07,54.76,16.17,85.34,54.59,16.45,84.61,54.42,16.72,83.87,54.25,16.99,83.12,54.08,17.21,82.37,53.91,17.44,81.61,53.74,17.62,81.02,53.63,17.22,80.78,53.62,16.48,80.71,53.65,15.68,80.81,53.71,14.90,81.04,53.80,14.14,81.28,53.89,13.38,81.52,53.98,12.62,81.75,54.07,11.86,81.99,54.17,11.10,82.22,54.26,10.34,82.49,54.35,9.59,82.84,54.47,8.87,83.20,54.58,8.17,83.57,54.70,7.47,83.94,54.81,6.77,84.31,54.93,6.07,84.68,55.04,5.37,85.05,55.16,4.66,85.41,55.28,3.96,85.78,55.39,3.26,86.15,55.51,2.56,86.52,55.62,1.86,86.89,55.74,1.16,87.26,55.86,0.45,87.62,55.97,-0.25,87.99,56.09,-0.95,88.41,56.21,-1.52,89.06,56.32,-1.07,89.70,56.43,-0.60,90.33,56.54,-0.12,90.89,56.63,0.44,91.41,56.71,1.05,91.93,56.79,1.65,92.45,56.86,2.25,92.97,56.94,2.86,93.43,57.00,3.51,93.85,57.06,4.19,94.19,57.09,4.91,94.44,57.10,5.67,94.61,57.10,6.46],
        [90.78,53.43,7.08,90.78,53.41,7.39,90.78,53.39,7.70,90.78,53.38,8.01,90.69,53.34,8.30,90.61,53.31,8.59,90.52,53.28,8.89,90.38,53.23,9.16,90.23,53.18,9.42,90.08,53.14,9.68,89.91,53.09,9.93,89.71,53.03,10.17,89.52,52.98,10.40,89.32,52.93,10.63,89.11,52.87,10.84,88.88,52.81,11.04,88.66,52.75,11.24,88.41,52.69,11.41,88.14,52.63,11.53,87.86,52.56,11.65,87.59,52.50,11.77,87.30,52.43,11.88,87.02,52.37,11.98,86.73,52.30,12.03,86.43,52.23,12.06,86.14,52.17,12.04,85.95,52.15,11.82,85.93,52.16,11.51,85.93,52.18,11.21,85.99,52.20,10.91,86.09,52.24,10.62,86.18,52.27,10.33,86.29,52.31,10.04,86.41,52.35,9.76,86.54,52.40,9.49,86.66,52.44,9.21,86.78,52.48,8.93,86.91,52.52,8.65,87.04,52.56,8.37,87.17,52.60,8.10,87.30,52.65,7.83,87.44,52.69,7.55,87.58,52.73,7.28,87.71,52.78,7.01,87.85,52.82,6.74,87.99,52.87,6.46,88.12,52.91,6.19,88.26,52.95,5.92,88.40,53.00,5.65,88.53,53.04,5.38,88.67,53.08,5.10,88.81,53.13,4.83,88.95,53.17,4.56,89.09,53.22,4.29,89.30,53.25,4.44,89.50,53.28,4.68,89.70,53.31,4.91,89.90,53.34,5.14,90.07,53.37,5.39,90.23,53.39,5.66,90.38,53.40,5.93,90.52,53.42,6.20,90.61,53.42,6.49,90.69,53.42,6.79]],
    "20" : [
        [56.06,111.40,-7.63,56.06,111.36,-7.02,56.05,111.33,-6.41,56.05,111.29,-5.81,56.05,111.26,-5.20,56.05,111.22,-4.60,56.04,111.19,-3.99,56.04,111.15,-3.39,56.04,111.12,-2.78,56.03,111.08,-2.17,56.03,111.05,-1.57,56.03,111.01,-0.96,56.02,110.98,-0.36,56.02,110.94,0.25,56.02,110.91,0.85,56.01,110.87,1.46,56.01,110.84,2.07,56.00,110.80,2.67,56.00,110.77,3.28,55.98,110.72,3.88,55.92,110.67,4.48,55.86,110.61,5.08,55.58,110.48,5.59,55.20,110.33,5.87,54.76,110.19,5.47,54.43,110.10,5.00,54.25,110.07,4.42,54.08,110.04,3.84,53.97,110.03,3.24,53.88,110.03,2.64,53.79,110.03,2.04,53.70,110.03,1.44,53.61,110.03,0.84,53.52,110.03,0.24,53.44,110.04,-0.36,53.36,110.04,-0.96,53.29,110.05,-1.57,53.21,110.06,-2.17,53.13,110.06,-2.77,53.06,110.07,-3.37,52.98,110.07,-3.97,52.90,110.08,-4.57,52.83,110.08,-5.18,52.75,110.09,-5.78,52.67,110.10,-6.38,52.60,110.10,-6.98,52.52,110.11,-7.58,52.45,110.11,-8.19,52.42,110.14,-8.79,52.45,110.18,-9.39,52.55,110.25,-9.99,52.71,110.34,-10.56,52.96,110.46,-11.11,53.30,110.61,-11.58,53.67,110.77,-12.03,54.21,110.98,-12.22,54.74,111.17,-12.12,55.14,111.29,-11.73,55.39,111.35,-11.18,55.59,111.39,-10.61,55.76,111.42,-10.03,55.90,111.44,-9.43,55.99,111.44,-8.83,56.04,111.42,-8.23],
        [60.79,110.35,-5.79,60.79,110.30,-4.85,60.78,110.25,-3.91,60.78,110.19,-2.97,60.78,110.14,-2.03,60.77,110.08,-1.09,60.77,110.03,-0.15,60.76,109.97,0.79,60.76,109.92,1.73,60.75,109.87,2.67,60.74,109.81,3.61,60.72,109.75,4.55,60.64,109.67,5.49,60.49,109.56,6.41,60.29,109.44,7.32,60.03,109.30,8.21,59.69,109.12,9.08,59.24,108.91,9.87,58.69,108.67,10.59,58.06,108.41,11.24,57.26,108.10,11.55,56.45,107.82,11.23,55.78,107.62,10.60,55.20,107.44,9.89,54.65,107.28,9.14,54.16,107.15,8.35,53.74,107.04,7.52,53.56,107.03,6.60,53.43,107.03,5.67,53.32,107.05,4.73,53.23,107.07,3.79,53.16,107.09,2.86,53.09,107.12,1.92,53.03,107.15,0.98,52.96,107.18,0.04,52.89,107.21,-0.90,52.83,107.23,-1.84,52.76,107.26,-2.78,52.69,107.29,-3.72,52.63,107.32,-4.65,52.56,107.35,-5.59,52.49,107.37,-6.53,52.42,107.40,-7.47,52.37,107.44,-8.41,52.38,107.49,-9.35,52.42,107.56,-10.29,52.51,107.64,-11.22,52.64,107.74,-12.15,52.83,107.86,-13.05,53.49,108.14,-13.65,54.21,108.43,-14.19,54.97,108.73,-14.66,55.80,109.05,-14.96,56.67,109.36,-14.88,57.36,109.58,-14.32,57.90,109.73,-13.57,58.42,109.88,-12.79,58.90,110.01,-12.00,59.35,110.13,-11.18,59.75,110.23,-10.33,60.10,110.31,-9.46,60.40,110.36,-8.57,60.62,110.39,-7.66,60.75,110.39,-6.73],
        [65.53,109.31,-3.95,65.52,109.23,-2.53,65.51,109.14,-1.02,65.51,109.06,0.49,65.50,108.97,1.99,65.49,108.88,3.50,65.48,108.79,4.98,65.39,108.70,6.15,65.20,108.56,7.32,64.93,108.40,8.42,64.60,108.23,9.40,64.21,108.04,10.23,63.64,107.78,11.12,62.96,107.49,11.90,62.25,107.20,12.54,61.43,106.86,13.15,60.48,106.49,13.71,59.43,106.08,14.22,59.02,105.92,14.35,58.56,105.75,14.34,58.10,105.59,14.17,57.47,105.39,13.81,56.51,105.08,13.03,55.88,104.88,12.49,55.70,104.84,12.17,55.55,104.82,11.51,55.24,104.78,10.06,54.95,104.76,8.66,54.72,104.75,7.34,54.55,104.76,6.01,54.42,104.79,4.62,54.31,104.83,3.15,54.21,104.88,1.64,54.10,104.92,0.14,53.99,104.97,-1.37,53.88,105.01,-2.87,53.78,105.06,-4.38,53.67,105.10,-5.88,53.58,105.15,-7.25,53.59,105.22,-8.48,53.67,105.32,-9.73,53.83,105.45,-11.00,54.06,105.60,-12.27,54.36,105.78,-13.56,54.70,105.98,-14.82,54.87,106.06,-15.33,55.01,106.12,-15.49,55.32,106.24,-15.62,56.41,106.66,-16.00,57.30,107.00,-16.23,57.95,107.24,-16.31,58.54,107.45,-16.17,58.95,107.59,-15.95,59.62,107.80,-15.40,60.44,108.05,-14.54,61.30,108.31,-13.57,62.08,108.54,-12.58,62.84,108.75,-11.51,63.48,108.93,-10.51,64.10,109.09,-9.39,64.60,109.22,-8.30,64.98,109.30,-7.26,65.29,109.35,-6.11,65.47,109.35,-5.04],
        [70.26,108.27,-2.11,70.26,108.17,-0.49,70.25,108.08,1.22,70.24,107.98,2.93,70.23,107.88,4.63,70.20,107.79,6.02,70.07,107.67,7.32,69.82,107.51,8.58,69.46,107.31,9.79,69.02,107.09,10.83,68.39,106.81,11.87,67.75,106.53,12.65,66.96,106.20,13.41,66.06,105.83,14.10,65.00,105.41,14.74,63.72,104.91,15.36,62.52,104.45,15.82,61.26,103.97,16.20,60.40,103.65,16.37,59.96,103.48,16.37,59.24,103.24,16.12,58.16,102.88,15.51,57.63,102.71,15.18,57.36,102.62,14.96,57.17,102.59,14.30,56.81,102.54,12.80,56.45,102.50,11.19,56.12,102.47,9.61,55.88,102.46,8.15,55.71,102.48,6.65,55.58,102.52,5.01,55.46,102.57,3.33,55.33,102.63,1.62,55.21,102.68,-0.09,55.09,102.73,-1.80,54.97,102.78,-3.51,54.85,102.83,-5.22,54.78,102.88,-6.66,54.82,102.97,-8.02,54.96,103.10,-9.41,55.20,103.27,-10.82,55.52,103.46,-12.25,55.94,103.70,-13.77,56.36,103.94,-15.25,56.72,104.12,-16.17,56.92,104.21,-16.47,57.08,104.28,-16.60,57.59,104.47,-16.69,59.07,105.02,-16.89,59.84,105.30,-16.91,60.56,105.55,-16.75,60.97,105.69,-16.52,61.91,105.99,-15.77,63.14,106.38,-14.70,64.24,106.72,-13.70,65.26,107.03,-12.67,66.17,107.31,-11.64,67.03,107.56,-10.52,67.87,107.80,-9.31,68.56,107.98,-8.15,69.15,108.13,-6.96,69.63,108.24,-5.74,69.97,108.30,-4.54,70.19,108.31,-3.30],
        [75.00,107.22,-0.27,74.99,107.14,1.16,74.98,107.06,2.58,74.98,106.98,4.01,74.97,106.89,5.43,74.93,106.80,6.86,74.77,106.66,8.27,74.47,106.48,9.65,74.02,106.24,10.98,73.39,105.94,12.22,72.58,105.58,13.34,71.62,105.18,14.32,70.55,104.74,15.15,69.39,104.28,15.84,68.17,103.81,16.42,66.92,103.32,16.89,65.63,102.84,17.26,64.32,102.34,17.55,63.00,101.85,17.78,61.67,101.36,17.96,60.38,100.90,17.62,59.13,100.48,17.05,58.53,100.33,15.84,58.15,100.27,14.47,57.83,100.23,13.08,57.53,100.20,11.68,57.25,100.17,10.28,57.04,100.18,8.87,56.90,100.21,7.45,56.79,100.24,6.03,56.68,100.28,4.60,56.58,100.33,3.18,56.48,100.37,1.76,56.37,100.41,0.34,56.27,100.45,-1.09,56.17,100.50,-2.51,56.07,100.54,-3.93,55.98,100.59,-5.36,56.01,100.67,-6.78,56.14,100.80,-8.20,56.37,100.97,-9.59,56.69,101.16,-10.97,57.06,101.37,-12.34,57.44,101.58,-13.69,57.82,101.80,-15.05,58.31,102.05,-16.37,59.31,102.46,-17.17,60.64,102.95,-17.35,61.97,103.44,-17.42,63.19,103.85,-16.85,64.26,104.19,-15.96,65.32,104.53,-15.07,66.38,104.86,-14.17,67.43,105.19,-13.27,68.48,105.52,-12.35,69.49,105.84,-11.40,70.45,106.13,-10.38,71.34,106.40,-9.31,72.18,106.64,-8.18,72.95,106.85,-6.99,73.63,107.03,-5.75,74.18,107.16,-4.44,74.62,107.24,-3.09,74.90,107.27,-1.69],
        [79.73,106.18,1.57,79.73,106.09,3.13,79.72,106.00,4.69,79.71,105.91,6.25,79.66,105.80,7.80,79.45,105.64,9.34,79.07,105.42,10.84,78.51,105.14,12.26,77.74,104.79,13.57,76.78,104.37,14.73,75.67,103.91,15.73,74.45,103.42,16.56,73.15,102.91,17.24,71.79,102.38,17.80,70.38,101.85,18.22,68.96,101.31,18.54,67.51,100.77,18.77,66.06,100.23,18.96,64.60,99.69,19.11,63.14,99.16,19.21,61.71,98.65,18.89,60.38,98.20,18.31,59.77,98.06,16.88,59.32,97.98,15.39,58.97,97.94,13.87,58.64,97.90,12.34,58.36,97.89,10.81,58.18,97.91,9.26,58.05,97.95,7.71,57.92,97.99,6.15,57.81,98.03,4.60,57.70,98.08,3.04,57.59,98.12,1.48,57.48,98.17,-0.07,57.36,98.22,-1.63,57.25,98.26,-3.18,57.18,98.32,-4.74,57.25,98.44,-6.29,57.46,98.60,-7.83,57.77,98.79,-9.35,58.17,99.02,-10.84,58.58,99.26,-12.33,59.00,99.49,-13.81,59.42,99.73,-15.29,59.99,100.01,-16.72,61.07,100.46,-17.66,62.52,101.00,-17.85,63.98,101.53,-17.90,65.28,101.96,-17.20,66.45,102.34,-16.23,67.62,102.71,-15.27,68.78,103.08,-14.30,69.94,103.45,-13.32,71.10,103.81,-12.35,72.26,104.18,-11.36,73.40,104.54,-10.37,74.51,104.89,-9.32,75.54,105.20,-8.19,76.50,105.48,-7.00,77.38,105.73,-5.73,78.15,105.94,-4.39,78.80,106.09,-2.98,79.29,106.19,-1.51,79.61,106.22,0.02],
        [84.46,105.14,3.41,84.46,105.02,5.52,84.44,104.89,7.70,84.32,104.76,9.24,84.04,104.57,10.73,83.58,104.32,12.20,83.00,104.04,13.48,82.21,103.69,14.71,81.26,103.28,15.81,80.17,102.83,16.77,78.96,102.34,17.60,77.59,101.80,18.31,76.09,101.22,18.90,74.58,100.65,19.32,72.93,100.03,19.64,70.96,99.30,19.90,69.11,98.62,20.06,67.10,97.88,20.15,65.21,97.19,20.18,63.97,96.75,20.13,62.38,96.19,19.74,62.01,96.06,19.64,61.79,95.99,19.36,61.25,95.85,18.42,60.73,95.73,17.11,60.28,95.66,15.43,59.83,95.61,13.39,59.52,95.60,11.62,59.33,95.64,9.62,59.15,95.70,7.43,58.99,95.76,5.24,58.83,95.83,3.00,58.67,95.89,0.76,58.51,95.96,-1.47,58.38,96.02,-3.44,58.44,96.14,-5.05,58.66,96.31,-6.73,59.03,96.54,-8.47,59.60,96.86,-10.50,60.19,97.19,-12.61,60.79,97.53,-14.72,61.28,97.80,-16.32,61.66,97.98,-17.06,62.45,98.31,-17.92,62.66,98.40,-18.12,63.34,98.65,-18.22,65.06,99.29,-18.42,65.83,99.57,-18.39,66.44,99.78,-18.19,67.32,100.06,-17.51,69.01,100.60,-16.14,70.69,101.14,-14.77,72.37,101.67,-13.40,74.03,102.20,-12.02,75.68,102.72,-10.64,77.31,103.24,-9.26,78.76,103.70,-7.99,79.98,104.07,-6.75,81.13,104.41,-5.36,82.11,104.69,-3.98,82.92,104.91,-2.56,83.56,105.06,-1.13,84.06,105.15,0.42,84.35,105.18,1.90],
        [89.20,104.09,5.25,89.19,103.99,7.06,89.17,103.88,8.88,88.95,103.70,10.67,88.51,103.44,12.40,87.82,103.10,14.05,86.90,102.68,15.55,85.76,102.19,16.87,84.44,101.65,17.99,82.99,101.07,18.91,81.44,100.47,19.63,79.82,99.85,20.15,78.15,99.22,20.52,76.47,98.59,20.76,74.77,97.97,20.92,73.07,97.34,21.00,71.37,96.72,21.02,69.66,96.10,21.01,67.96,95.48,20.98,66.25,94.86,20.94,64.56,94.26,20.74,63.04,93.75,20.05,62.12,93.50,18.51,61.56,93.39,16.80,61.15,93.34,15.03,60.80,93.31,13.25,60.61,93.34,11.45,60.46,93.39,9.64,60.31,93.43,7.83,60.17,93.48,6.02,60.04,93.54,4.21,59.91,93.59,2.40,59.78,93.65,0.59,59.66,93.70,-1.21,59.59,93.78,-3.02,59.73,93.93,-4.83,60.06,94.15,-6.60,60.51,94.41,-8.33,61.00,94.68,-10.06,61.49,94.96,-11.78,61.98,95.23,-13.51,62.46,95.50,-15.24,63.05,95.81,-16.92,64.16,96.29,-18.26,65.72,96.88,-18.78,67.41,97.51,-18.89,68.93,98.02,-18.12,70.30,98.46,-17.02,71.68,98.90,-15.92,73.05,99.33,-14.82,74.42,99.77,-13.71,75.79,100.21,-12.60,77.15,100.64,-11.49,78.52,101.08,-10.38,79.88,101.51,-9.25,81.23,101.94,-8.12,82.57,102.36,-6.98,83.89,102.78,-5.81,85.11,103.15,-4.52,86.22,103.48,-3.12,87.21,103.75,-1.63,88.02,103.96,-0.02,88.64,104.09,1.68,89.05,104.14,3.45],
        [93.93,103.05,7.09,93.92,102.92,9.32,93.76,102.77,11.01,93.38,102.53,12.69,92.81,102.24,14.25,92.05,101.89,15.68,91.13,101.48,16.98,90.01,101.00,18.15,88.68,100.46,19.22,87.28,99.90,20.06,85.74,99.31,20.74,84.09,98.68,21.23,82.24,97.98,21.58,80.24,97.25,21.79,78.13,96.47,21.86,75.88,95.66,21.85,73.57,94.82,21.79,71.19,93.96,21.68,68.85,93.11,21.56,66.66,92.32,21.41,65.06,91.75,21.22,64.74,91.64,21.07,64.12,91.45,20.49,63.45,91.26,19.57,62.94,91.14,18.40,62.47,91.06,16.63,62.03,91.02,14.51,61.81,91.06,12.42,61.61,91.13,9.90,61.40,91.19,7.38,61.22,91.27,4.84,61.04,91.34,2.28,60.85,91.42,-0.29,60.80,91.50,-2.15,60.96,91.66,-3.93,61.34,91.90,-5.80,61.98,92.26,-8.10,62.66,92.65,-10.53,63.35,93.03,-12.96,64.03,93.42,-15.39,64.53,93.69,-16.91,64.90,93.86,-17.55,66.15,94.38,-18.88,66.40,94.49,-19.13,67.04,94.73,-19.21,68.73,95.35,-19.38,69.57,95.65,-19.26,70.08,95.82,-19.03,71.67,96.33,-17.77,73.62,96.95,-16.22,75.57,97.58,-14.67,77.52,98.20,-13.11,79.47,98.83,-11.55,81.42,99.45,-9.99,83.34,100.06,-8.43,85.24,100.66,-6.88,87.11,101.26,-5.31,88.73,101.77,-3.91,89.98,102.15,-2.59,91.18,102.50,-1.05,92.13,102.76,0.45,92.86,102.94,2.02,93.45,103.06,3.76,93.80,103.09,5.42],
        [98.67,102.00,8.93,98.60,101.87,10.89,98.33,101.68,12.60,97.83,101.40,14.28,97.11,101.05,15.86,96.19,100.64,17.33,95.09,100.16,18.63,93.76,99.61,19.82,92.36,99.05,20.79,90.81,98.44,21.56,89.20,97.82,22.10,87.29,97.11,22.50,85.39,96.40,22.68,83.16,95.59,22.71,80.78,94.73,22.63,78.30,93.84,22.49,75.73,92.91,22.33,73.19,91.99,22.15,70.62,91.07,21.96,68.06,90.15,21.78,66.38,89.54,21.65,65.86,89.36,21.54,65.33,89.20,21.07,64.52,88.95,20.06,64.03,88.84,19.00,63.56,88.77,17.21,63.18,88.74,15.18,62.96,88.80,12.70,62.74,88.87,10.02,62.53,88.94,7.36,62.33,89.02,4.64,62.14,89.10,1.91,61.99,89.18,-0.45,62.08,89.31,-2.21,62.41,89.54,-4.11,63.03,89.89,-6.42,63.76,90.30,-9.00,64.49,90.71,-11.59,65.22,91.12,-14.17,65.89,91.49,-16.51,66.34,91.72,-17.54,67.12,92.05,-18.44,68.18,92.50,-19.55,68.57,92.64,-19.64,69.75,93.08,-19.73,70.83,93.47,-19.73,71.53,93.71,-19.49,72.96,94.17,-18.41,74.68,94.72,-17.08,76.77,95.40,-15.44,78.87,96.07,-13.80,80.96,96.73,-12.17,83.04,97.40,-10.54,85.11,98.06,-8.90,87.18,98.73,-7.26,89.24,99.38,-5.61,91.24,100.02,-3.98,93.13,100.62,-2.39,94.47,101.04,-1.09,95.75,101.41,0.48,96.73,101.69,1.98,97.52,101.88,3.61,98.15,102.01,5.42,98.52,102.05,7.18],
        [101.77,100.44,9.47,101.68,100.30,11.36,101.36,100.08,13.18,100.83,99.79,14.90,100.06,99.42,16.59,99.11,98.99,18.11,97.94,98.48,19.52,96.62,97.94,20.71,95.15,97.35,21.72,93.57,96.73,22.51,91.90,96.09,23.06,90.15,95.44,23.37,88.37,94.78,23.43,85.94,93.91,23.26,83.52,93.04,23.08,81.10,92.17,22.91,78.69,91.30,22.73,76.28,90.43,22.56,73.87,89.56,22.38,71.45,88.70,22.21,69.04,87.83,22.03,66.78,87.01,21.86,66.61,86.96,21.72,65.63,86.67,20.61,65.18,86.55,19.75,64.74,86.48,18.16,64.38,86.46,16.20,64.15,86.52,13.52,63.92,86.59,10.74,63.70,86.67,7.97,63.50,86.75,5.16,63.30,86.84,2.34,63.19,86.92,0.17,63.36,87.08,-1.65,63.77,87.34,-3.60,64.51,87.75,-6.24,65.27,88.18,-8.92,66.02,88.60,-11.60,66.78,89.03,-14.28,67.47,89.41,-16.69,67.93,89.64,-17.75,68.74,89.99,-18.68,69.85,90.46,-19.85,70.07,90.54,-19.90,71.22,90.96,-19.95,72.41,91.39,-19.90,73.18,91.66,-19.62,74.98,92.23,-18.24,77.06,92.90,-16.63,79.14,93.57,-15.02,81.23,94.24,-13.40,83.31,94.91,-11.79,85.39,95.58,-10.18,87.48,96.24,-8.56,89.65,96.94,-6.88,91.82,97.64,-5.19,93.99,98.33,-3.51,96.14,99.02,-1.84,97.54,99.46,-0.61,98.80,99.84,0.83,99.80,100.12,2.35,100.61,100.32,3.99,101.23,100.45,5.76,101.62,100.49,7.63],
        [102.63,98.10,9.34,102.52,97.94,11.45,102.10,97.67,13.52,101.40,97.30,15.49,100.42,96.85,17.32,99.19,96.31,18.97,97.73,95.70,20.39,96.09,95.03,21.56,94.29,94.33,22.44,92.38,93.60,23.03,90.41,92.87,23.30,88.41,92.14,23.31,86.41,91.42,23.24,84.41,90.70,23.13,82.41,89.98,23.00,80.42,89.26,22.86,78.42,88.54,22.73,76.42,87.82,22.59,74.43,87.10,22.46,72.43,86.38,22.32,70.43,85.66,22.18,68.44,84.94,22.04,66.77,84.38,21.14,65.94,84.19,19.22,65.57,84.17,17.13,65.40,84.23,15.01,65.23,84.28,12.89,65.05,84.34,10.77,64.88,84.40,8.65,64.73,84.46,6.53,64.58,84.52,4.41,64.43,84.59,2.29,64.46,84.71,0.17,64.83,84.96,-1.91,65.39,85.28,-3.93,65.96,85.60,-5.96,66.53,85.92,-7.98,67.10,86.24,-10.00,67.67,86.56,-12.03,68.24,86.88,-14.05,68.81,87.21,-16.08,69.56,87.59,-18.02,70.94,88.17,-19.53,72.77,88.86,-20.00,74.71,89.55,-19.65,76.35,90.08,-18.40,77.98,90.60,-17.14,79.61,91.12,-15.87,81.23,91.64,-14.61,82.86,92.17,-13.34,84.49,92.69,-12.08,86.12,93.21,-10.81,87.74,93.73,-9.54,89.37,94.25,-8.27,90.99,94.77,-7.00,92.61,95.29,-5.72,94.22,95.80,-4.44,95.80,96.31,-3.11,97.35,96.79,-1.72,98.84,97.25,-0.29,100.13,97.63,1.36,101.18,97.91,3.19,101.95,98.08,5.16,102.44,98.15,7.22],
        [103.48,95.75,9.21,103.37,95.59,11.32,102.96,95.33,13.38,102.26,94.96,15.34,101.29,94.51,17.16,100.07,93.97,18.81,98.62,93.37,20.23,96.98,92.70,21.40,95.20,92.00,22.29,93.30,91.28,22.88,91.33,90.55,23.17,89.34,89.82,23.18,87.35,89.10,23.15,85.36,88.38,23.10,83.37,87.66,22.99,81.38,86.94,22.87,79.39,86.22,22.75,77.40,85.51,22.61,75.41,84.79,22.48,73.42,84.07,22.35,71.43,83.36,22.21,69.44,82.64,22.08,67.68,82.04,21.39,66.95,81.88,19.44,66.72,81.91,17.34,66.54,81.97,15.23,66.37,82.02,13.12,66.20,82.08,11.01,66.03,82.14,8.90,65.88,82.20,6.78,65.73,82.26,4.67,65.60,82.33,2.56,65.77,82.51,0.46,66.25,82.80,-1.59,66.82,83.12,-3.60,67.39,83.44,-5.62,67.96,83.76,-7.63,68.53,84.08,-9.65,69.10,84.40,-11.67,69.67,84.72,-13.68,70.24,85.04,-15.70,70.87,85.38,-17.69,72.11,85.92,-19.30,73.84,86.59,-20.00,75.81,87.30,-19.85,77.49,87.85,-18.69,79.11,88.36,-17.43,80.73,88.88,-16.16,82.35,89.40,-14.90,83.97,89.92,-13.63,85.58,90.44,-12.36,87.20,90.95,-11.09,88.81,91.47,-9.82,90.42,91.98,-8.54,92.02,92.50,-7.25,93.61,93.00,-5.94,95.16,93.49,-4.58,96.70,93.97,-3.20,98.23,94.45,-1.82,99.72,94.92,-0.39,101.00,95.29,1.25,102.04,95.57,3.08,102.81,95.74,5.04,103.30,95.80,7.10],
        [104.34,93.40,9.08,104.25,93.27,10.95,103.94,93.06,12.75,103.41,92.76,14.49,102.65,92.40,16.16,101.69,91.96,17.71,100.56,91.47,19.07,99.24,90.92,20.29,97.78,90.33,21.30,96.23,89.73,22.09,94.58,89.09,22.65,92.83,88.44,22.98,90.94,87.75,23.05,88.33,86.80,23.03,85.72,85.85,23.00,83.32,84.98,22.90,80.70,84.04,22.75,78.08,83.09,22.60,75.65,82.22,22.45,73.61,81.48,22.31,71.57,80.75,22.18,69.53,80.01,22.04,68.90,79.79,21.99,68.48,79.66,21.50,68.28,79.61,21.05,68.04,79.60,19.78,67.82,79.67,17.01,67.59,79.74,14.25,67.36,79.81,11.48,67.15,79.89,8.74,66.95,79.97,5.95,66.80,80.05,3.57,66.91,80.18,1.94,67.31,80.43,0.11,68.06,80.85,-2.55,68.81,81.27,-5.21,69.56,81.69,-7.87,70.31,82.11,-10.53,71.06,82.53,-13.19,71.81,82.95,-15.85,72.39,83.27,-17.74,72.83,83.48,-18.55,73.91,83.94,-19.73,74.18,84.05,-19.97,74.88,84.30,-20.00,76.50,84.89,-20.00,77.28,85.17,-19.84,78.20,85.47,-19.28,79.93,86.03,-17.91,82.07,86.71,-16.23,84.20,87.39,-14.55,86.33,88.07,-12.87,88.44,88.75,-11.18,90.52,89.41,-9.50,92.45,90.02,-7.89,94.42,90.64,-6.13,96.45,91.28,-4.32,98.47,91.92,-2.51,100.20,92.46,-0.92,101.38,92.81,0.45,102.40,93.09,2.00,103.20,93.29,3.64,103.81,93.42,5.41,104.19,93.45,7.21],
        [105.19,91.06,8.95,105.11,90.92,10.80,104.80,90.71,12.61,104.26,90.42,14.36,103.51,90.05,16.01,102.56,89.62,17.55,101.43,89.13,18.92,100.11,88.59,20.14,98.67,88.00,21.15,97.12,87.40,21.94,95.47,86.76,22.51,93.74,86.12,22.84,91.87,85.43,22.92,89.26,84.48,22.90,86.65,83.53,22.87,84.04,82.59,22.84,81.65,81.72,22.73,79.04,80.78,22.57,76.42,79.83,22.42,73.81,78.89,22.27,71.67,78.12,22.14,70.22,77.60,22.04,69.60,77.37,22.00,69.44,77.33,21.82,69.35,77.31,21.60,69.19,77.34,20.01,68.96,77.41,17.25,68.73,77.48,14.49,68.51,77.55,11.73,68.30,77.63,8.99,68.10,77.71,6.21,68.01,77.79,4.20,68.20,77.95,2.56,68.74,78.26,0.44,69.49,78.68,-2.21,70.23,79.10,-4.86,70.98,79.52,-7.51,71.73,79.94,-10.16,72.48,80.36,-12.81,73.23,80.78,-15.46,73.88,81.14,-17.67,74.32,81.36,-18.62,75.16,81.72,-19.57,75.56,81.89,-19.97,76.06,82.07,-20.00,77.92,82.75,-20.00,78.61,82.99,-19.89,79.25,83.21,-19.56,81.01,83.77,-18.16,83.13,84.45,-16.48,85.24,85.12,-14.80,87.33,85.79,-13.11,89.40,86.45,-11.42,91.28,87.04,-9.84,93.30,87.68,-8.03,95.32,88.31,-6.22,97.34,88.95,-4.41,99.36,89.58,-2.61,101.07,90.11,-1.03,102.25,90.47,0.34,103.26,90.75,1.88,104.07,90.95,3.52,104.67,91.07,5.29,105.04,91.11,7.09],
        [106.05,88.71,8.82,105.94,88.56,10.90,105.54,88.30,12.93,104.85,87.94,14.87,103.91,87.49,16.68,102.71,86.97,18.31,101.30,86.38,19.73,99.70,85.73,20.91,97.95,85.04,21.82,96.09,84.33,22.44,94.15,83.61,22.75,92.19,82.89,22.79,90.22,82.17,22.77,88.26,81.46,22.74,86.29,80.75,22.72,84.32,80.03,22.70,82.36,79.32,22.68,80.39,78.61,22.57,78.43,77.90,22.46,76.47,77.19,22.34,74.50,76.48,22.23,72.54,75.77,22.12,70.57,75.07,22.00,70.31,75.08,19.96,70.14,75.14,17.87,69.96,75.19,15.79,69.79,75.24,13.70,69.62,75.30,11.62,69.47,75.36,9.54,69.32,75.42,7.45,69.20,75.50,5.36,69.48,75.71,3.31,70.02,76.02,1.31,70.58,76.33,-0.68,71.15,76.65,-2.67,71.71,76.97,-4.66,72.27,77.28,-6.65,72.83,77.60,-8.64,73.39,77.91,-10.63,73.95,78.23,-12.62,74.51,78.54,-14.61,75.07,78.86,-16.60,75.75,79.21,-18.54,77.08,79.78,-20.00,79.05,80.49,-20.00,80.87,81.12,-19.35,82.46,81.63,-18.08,84.04,82.13,-16.81,85.62,82.64,-15.53,87.19,83.14,-14.25,88.76,83.64,-12.96,90.29,84.12,-11.62,91.81,84.59,-10.26,93.33,85.07,-8.90,94.85,85.55,-7.54,96.36,86.02,-6.19,97.88,86.50,-4.83,99.40,86.98,-3.47,100.91,87.45,-2.11,102.37,87.90,-0.68,103.62,88.27,0.95,104.64,88.54,2.76,105.39,88.70,4.70,105.86,88.76,6.73],
        [106.90,86.37,8.68,106.79,86.21,10.75,106.40,85.96,12.77,105.73,85.60,14.70,104.79,85.16,16.50,103.62,84.65,18.12,102.23,84.06,19.54,100.65,83.42,20.72,98.92,82.74,21.64,97.07,82.03,22.27,95.16,81.32,22.61,93.21,80.60,22.66,91.26,79.90,22.64,89.31,79.19,22.62,87.36,78.48,22.60,85.41,77.77,22.57,83.46,77.06,22.55,81.51,76.35,22.53,79.56,75.65,22.44,77.61,74.94,22.33,75.66,74.24,22.22,73.71,73.54,22.10,71.83,72.87,21.85,71.42,72.83,19.86,71.25,72.88,17.79,71.08,72.94,15.72,70.91,72.99,13.65,70.75,73.05,11.58,70.60,73.11,9.52,70.45,73.17,7.45,70.50,73.30,5.38,70.97,73.58,3.38,71.52,73.90,1.41,72.08,74.21,-0.57,72.64,74.52,-2.54,73.19,74.83,-4.51,73.75,75.15,-6.49,74.31,75.46,-8.46,74.87,75.77,-10.44,75.42,76.09,-12.41,75.98,76.40,-14.39,76.54,76.71,-16.36,77.14,77.04,-18.32,78.30,77.55,-19.94,80.23,78.26,-20.00,82.07,78.90,-19.44,83.64,79.40,-18.17,85.20,79.89,-16.90,86.75,80.39,-15.62,88.29,80.88,-14.31,89.80,81.35,-12.96,91.30,81.82,-11.61,92.81,82.29,-10.26,94.31,82.77,-8.92,95.82,83.24,-7.57,97.32,83.71,-6.22,98.83,84.18,-4.87,100.33,84.66,-3.53,101.84,85.13,-2.18,103.28,85.57,-0.75,104.51,85.93,0.87,105.51,86.20,2.67,106.25,86.36,4.60,106.72,86.42,6.62],
        [107.75,84.02,8.55,107.65,83.87,10.60,107.26,83.62,12.61,106.60,83.27,14.52,105.68,82.83,16.31,104.52,82.32,17.93,103.15,81.74,19.35,101.59,81.11,20.54,99.88,80.44,21.46,98.06,79.74,22.11,96.16,79.03,22.46,94.23,78.32,22.53,92.30,77.62,22.51,90.36,76.91,22.49,88.43,76.21,22.47,86.49,75.51,22.45,84.56,74.80,22.43,82.62,74.10,22.40,80.69,73.40,22.38,78.75,72.70,22.31,76.82,72.00,22.20,74.89,71.30,22.09,73.10,70.67,21.69,72.54,70.58,19.76,72.37,70.63,17.71,72.20,70.68,15.66,72.04,70.74,13.60,71.87,70.79,11.55,71.73,70.85,9.50,71.61,70.93,7.45,71.92,71.15,5.43,72.47,71.46,3.47,73.02,71.77,1.51,73.58,72.08,-0.45,74.13,72.39,-2.41,74.68,72.70,-4.37,75.23,73.01,-6.32,75.79,73.32,-8.28,76.34,73.63,-10.24,76.89,73.94,-12.20,77.44,74.26,-14.16,78.00,74.57,-16.12,78.56,74.88,-18.07,79.55,75.34,-19.79,81.41,76.03,-20.00,83.25,76.67,-19.50,84.80,77.16,-18.23,86.33,77.64,-16.95,87.83,78.12,-15.62,89.32,78.59,-14.28,90.82,79.05,-12.94,92.31,79.52,-11.60,93.80,79.99,-10.27,95.30,80.46,-8.93,96.79,80.93,-7.59,98.28,81.40,-6.26,99.78,81.87,-4.92,101.27,82.34,-3.58,102.76,82.81,-2.24,104.19,83.24,-0.82,105.40,83.60,0.80,106.39,83.86,2.58,107.12,84.01,4.50,107.58,84.07,6.51],
        [108.61,81.67,8.42,108.50,81.52,10.46,108.12,81.27,12.44,107.47,80.93,14.35,106.57,80.50,16.13,105.43,80.00,17.74,104.07,79.43,19.16,102.54,78.80,20.34,100.85,78.13,21.28,99.05,77.44,21.94,97.17,76.74,22.32,95.26,76.04,22.40,93.34,75.34,22.38,91.42,74.64,22.36,89.50,73.94,22.34,87.58,73.25,22.32,85.66,72.55,22.30,83.74,71.85,22.28,81.82,71.15,22.26,79.90,70.46,22.24,77.98,69.76,22.19,76.07,69.07,22.08,74.37,68.48,21.54,73.66,68.33,19.67,73.49,68.38,17.63,73.32,68.43,15.60,73.16,68.48,13.56,73.00,68.54,11.53,72.86,68.60,9.49,72.92,68.74,7.46,73.43,69.03,5.51,73.97,69.34,3.56,74.52,69.65,1.62,75.07,69.96,-0.32,75.62,70.26,-2.27,76.17,70.57,-4.21,76.71,70.88,-6.15,77.26,71.19,-8.10,77.81,71.49,-10.04,78.36,71.80,-11.98,78.91,72.11,-13.92,79.45,72.42,-15.87,80.01,72.73,-17.81,80.81,73.12,-19.63,82.59,73.79,-20.00,84.42,74.43,-19.54,85.91,74.90,-18.23,87.39,75.37,-16.90,88.88,75.83,-15.57,90.36,76.30,-14.25,91.84,76.76,-12.92,93.32,77.23,-11.60,94.80,77.69,-10.27,96.28,78.16,-8.94,97.76,78.62,-7.62,99.25,79.09,-6.29,100.73,79.55,-4.96,102.21,80.02,-3.64,103.69,80.48,-2.31,105.09,80.91,-0.89,106.29,81.26,0.72,107.26,81.51,2.50,107.98,81.67,4.40,108.43,81.72,6.39],
        [109.46,79.33,8.29,109.36,79.18,10.31,108.99,78.93,12.28,108.35,78.60,14.16,107.46,78.17,15.93,106.34,77.68,17.54,105.01,77.11,18.95,103.50,76.50,20.14,101.84,75.84,21.09,100.06,75.15,21.77,98.20,74.46,22.16,96.31,73.76,22.27,94.41,73.07,22.25,92.51,72.38,22.23,90.61,71.69,22.21,88.70,71.00,22.19,86.80,70.31,22.17,84.90,69.62,22.15,83.00,68.93,22.13,81.10,68.23,22.11,79.20,67.54,22.09,77.30,66.85,22.06,75.66,66.29,21.43,74.78,66.07,19.63,74.61,66.12,17.62,74.45,66.18,15.60,74.28,66.23,13.59,74.14,66.29,11.57,74.01,66.36,9.55,74.37,66.59,7.58,74.91,66.90,5.66,75.45,67.20,3.73,76.00,67.51,1.81,76.54,67.81,-0.12,77.08,68.12,-2.04,77.63,68.42,-3.97,78.17,68.73,-5.89,78.71,69.03,-7.82,79.25,69.34,-9.74,79.80,69.64,-11.67,80.34,69.95,-13.59,80.88,70.25,-15.52,81.43,70.56,-17.44,82.08,70.90,-19.32,83.68,71.51,-19.87,85.49,72.15,-19.38,86.98,72.62,-18.09,88.47,73.09,-16.80,89.94,73.55,-15.50,91.42,74.02,-14.19,92.89,74.48,-12.88,94.35,74.94,-11.56,95.82,75.40,-10.25,97.29,75.86,-8.94,98.76,76.32,-7.62,100.23,76.78,-6.31,101.69,77.24,-4.99,103.16,77.70,-3.68,104.63,78.16,-2.37,106.01,78.59,-0.95,107.19,78.93,0.65,108.14,79.17,2.42,108.84,79.33,4.31,109.29,79.38,6.28],
        [110.32,76.98,8.16,110.22,76.84,10.15,109.85,76.60,12.09,109.23,76.27,13.96,108.37,75.85,15.71,107.28,75.37,17.30,105.98,74.82,18.71,104.50,74.21,19.91,102.88,73.57,20.86,101.14,72.89,21.57,99.32,72.21,21.99,97.45,71.52,22.14,95.58,70.84,22.12,93.70,70.16,22.10,91.83,69.48,22.08,89.95,68.79,22.06,88.08,68.11,22.04,86.20,67.43,22.02,84.33,66.75,22.00,82.45,66.07,21.98,80.58,65.39,21.95,78.70,64.71,21.93,77.04,64.13,21.48,76.13,63.89,19.71,75.75,63.87,17.79,75.59,63.92,15.80,75.43,63.97,13.81,75.29,64.03,11.82,75.29,64.14,9.84,75.80,64.43,7.93,76.33,64.73,6.03,76.87,65.03,4.14,77.40,65.33,2.24,77.94,65.63,0.34,78.48,65.94,-1.56,79.01,66.24,-3.46,79.55,66.54,-5.36,80.08,66.84,-7.25,80.62,67.14,-9.15,81.15,67.44,-11.05,81.69,67.74,-12.95,82.22,68.04,-14.85,82.76,68.34,-16.75,83.31,68.65,-18.64,84.67,69.18,-19.33,86.46,69.80,-18.80,87.96,70.28,-17.58,89.45,70.75,-16.33,90.93,71.22,-15.09,92.42,71.69,-13.84,93.90,72.17,-12.60,95.38,72.63,-11.34,96.86,73.10,-10.09,98.33,73.57,-8.82,99.80,74.03,-7.55,101.26,74.49,-6.27,102.71,74.95,-4.98,104.16,75.40,-3.68,105.60,75.86,-2.39,106.95,76.27,-0.98,108.10,76.60,0.62,109.03,76.84,2.36,109.71,76.98,4.23,110.15,77.03,6.18],
        [110.62,74.44,8.03,110.53,74.29,9.95,110.17,74.06,11.83,109.57,73.74,13.63,108.72,73.34,15.32,107.66,72.87,16.86,106.39,72.33,18.21,104.95,71.74,19.36,103.37,71.12,20.26,101.68,70.46,20.92,99.92,69.80,21.32,98.12,69.13,21.57,96.31,68.47,21.65,94.50,67.81,21.70,92.69,67.15,21.71,90.87,66.49,21.70,89.06,65.83,21.68,87.24,65.17,21.66,85.43,64.51,21.64,83.62,63.85,21.62,81.80,63.19,21.60,79.99,62.53,21.58,78.31,61.93,21.32,77.43,61.71,19.61,76.88,61.61,17.81,76.72,61.66,15.88,76.57,61.71,13.96,76.43,61.77,12.04,76.70,61.97,10.15,77.22,62.26,8.31,77.74,62.55,6.48,78.25,62.85,4.64,78.77,63.14,2.80,79.29,63.43,0.97,79.81,63.72,-0.87,80.33,64.01,-2.70,80.84,64.30,-4.54,81.36,64.59,-6.38,81.88,64.88,-8.21,82.40,65.17,-10.05,82.92,65.46,-11.89,83.43,65.75,-13.72,83.95,66.05,-15.56,84.47,66.34,-17.40,85.55,66.78,-18.28,87.29,67.38,-17.74,88.74,67.84,-16.55,90.18,68.30,-15.36,91.63,68.76,-14.17,93.08,69.22,-12.97,94.52,69.68,-11.78,95.97,70.14,-10.59,97.41,70.60,-9.39,98.85,71.06,-8.19,100.30,71.52,-7.00,101.73,71.97,-5.79,103.16,72.43,-4.58,104.60,72.88,-3.37,106.03,73.33,-2.15,107.34,73.73,-0.80,108.46,74.06,0.74,109.36,74.29,2.43,110.03,74.44,4.23,110.46,74.48,6.11],
        [107.00,70.47,7.85,106.93,70.34,9.56,106.61,70.13,11.23,106.07,69.85,12.82,105.30,69.49,14.31,104.34,69.06,15.66,103.19,68.58,16.82,101.89,68.05,17.80,100.48,67.49,18.61,99.00,66.92,19.24,97.46,66.33,19.72,95.90,65.74,20.10,94.32,65.15,20.38,92.72,64.56,20.57,91.12,63.97,20.69,89.51,63.38,20.77,87.91,62.79,20.82,86.30,62.20,20.86,84.69,61.62,20.89,83.08,61.03,20.90,81.47,60.44,20.91,79.86,59.86,20.92,78.86,59.56,19.78,78.08,59.36,18.27,77.90,59.39,16.57,77.76,59.43,14.87,77.64,59.48,13.16,77.85,59.65,11.48,78.31,59.91,9.85,78.77,60.17,8.23,79.23,60.43,6.60,79.69,60.69,4.97,80.15,60.94,3.34,80.61,61.20,1.71,81.07,61.46,0.08,81.53,61.72,-1.55,81.99,61.98,-3.18,82.45,62.23,-4.81,82.91,62.49,-6.44,83.37,62.75,-8.07,83.83,63.01,-9.70,84.29,63.27,-11.33,84.76,63.53,-12.95,85.25,63.80,-14.57,85.77,64.08,-16.18,87.10,64.55,-16.02,88.46,65.00,-15.12,89.69,65.38,-14.00,90.92,65.77,-12.87,92.12,66.14,-11.70,93.32,66.51,-10.54,94.51,66.88,-9.37,95.71,67.25,-8.20,96.90,67.62,-7.03,98.10,67.99,-5.86,99.30,68.36,-4.69,100.49,68.73,-3.52,101.68,69.10,-2.35,102.86,69.46,-1.16,104.01,69.81,0.06,105.02,70.10,1.41,105.84,70.32,2.89,106.46,70.45,4.48,106.85,70.50,6.15],
        [103.18,66.42,7.67,103.13,66.32,9.09,102.89,66.16,10.48,102.46,65.93,11.82,101.85,65.63,13.07,101.07,65.29,14.21,100.13,64.89,15.21,99.08,64.46,16.06,97.93,64.00,16.76,96.72,63.53,17.35,95.47,63.05,17.84,94.19,62.56,18.25,92.91,62.07,18.61,91.60,61.58,18.92,90.29,61.09,19.18,88.97,60.59,19.39,87.64,60.10,19.54,86.31,59.61,19.66,84.98,59.12,19.76,83.64,58.63,19.83,82.30,58.14,19.89,80.96,57.65,19.94,79.94,57.32,19.25,79.29,57.15,17.99,79.03,57.13,16.61,78.92,57.17,15.19,78.82,57.21,13.77,79.12,57.40,12.40,79.51,57.62,11.04,79.89,57.83,9.69,80.27,58.05,8.33,80.65,58.26,6.97,81.04,58.47,5.62,81.42,58.69,4.26,81.80,58.90,2.91,82.18,59.12,1.55,82.57,59.33,0.19,82.95,59.55,-1.16,83.33,59.76,-2.52,83.72,59.98,-3.87,84.13,60.20,-5.22,84.56,60.44,-6.56,85.03,60.68,-7.88,85.57,60.95,-9.17,86.13,61.22,-10.45,86.68,61.50,-11.74,87.78,61.88,-11.43,88.92,62.25,-10.68,89.92,62.56,-9.70,90.91,62.87,-8.73,91.91,63.18,-7.76,92.90,63.49,-6.79,93.90,63.79,-5.81,94.89,64.10,-4.83,95.88,64.40,-3.85,96.86,64.71,-2.87,97.84,65.01,-1.88,98.82,65.31,-0.88,99.78,65.60,0.12,100.73,65.89,1.15,101.55,66.13,2.29,102.22,66.30,3.53,102.71,66.41,4.86,103.04,66.45,6.25],
        [99.35,62.37,7.48,99.33,62.30,8.60,99.16,62.18,9.70,98.85,62.01,10.76,98.42,61.80,11.78,97.85,61.53,12.70,97.15,61.23,13.52,96.34,60.90,14.22,95.47,60.55,14.83,94.55,60.18,15.36,93.59,59.81,15.80,92.61,59.43,16.19,91.61,59.05,16.55,90.61,58.67,16.87,89.60,58.28,17.16,88.58,57.90,17.44,87.56,57.51,17.69,86.53,57.12,17.91,85.50,56.74,18.11,84.46,56.35,18.27,83.42,55.96,18.42,82.37,55.58,18.53,81.32,55.19,18.63,80.59,54.96,17.88,80.18,54.87,16.86,80.09,54.90,15.75,80.04,54.94,14.63,80.33,55.11,13.56,80.63,55.28,12.49,80.93,55.45,11.42,81.23,55.62,10.36,81.53,55.79,9.29,81.83,55.95,8.22,82.13,56.12,7.16,82.43,56.29,6.09,82.73,56.46,5.02,83.05,56.64,3.96,83.40,56.82,2.91,83.80,57.03,1.88,84.24,57.24,0.87,84.68,57.46,-0.14,85.11,57.67,-1.15,85.55,57.89,-2.16,85.99,58.10,-3.17,86.42,58.32,-4.18,86.86,58.53,-5.19,87.30,58.75,-6.20,87.98,59.02,-6.60,88.94,59.34,-6.13,89.86,59.65,-5.57,90.64,59.89,-4.80,91.42,60.13,-4.02,92.19,60.36,-3.25,92.96,60.60,-2.47,93.73,60.84,-1.69,94.50,61.07,-0.90,95.26,61.31,-0.12,96.02,61.54,0.68,96.77,61.77,1.48,97.49,61.98,2.31,98.11,62.16,3.22,98.61,62.29,4.22,98.98,62.36,5.27,99.23,62.39,6.37],
        [95.52,58.32,7.29,95.52,58.28,8.11,95.42,58.20,8.91,95.25,58.09,9.70,94.98,57.95,10.45,94.61,57.78,11.16,94.16,57.58,11.80,93.60,57.35,12.35,93.01,57.10,12.84,92.36,56.84,13.27,91.70,56.58,13.66,91.01,56.31,14.00,90.31,56.04,14.31,89.60,55.76,14.60,88.88,55.49,14.87,88.16,55.21,15.12,87.43,54.93,15.35,86.70,54.65,15.59,85.97,54.37,15.80,85.23,54.09,16.00,84.49,53.81,16.20,83.75,53.53,16.38,83.01,53.25,16.56,82.26,52.97,16.73,81.51,52.69,16.88,81.25,52.64,16.18,81.34,52.71,15.39,81.56,52.83,14.62,81.77,52.96,13.84,82.00,53.08,13.07,82.30,53.23,12.33,82.61,53.39,11.59,82.93,53.54,10.86,83.25,53.70,10.13,83.57,53.86,9.39,83.88,54.01,8.66,84.20,54.17,7.93,84.52,54.33,7.19,84.84,54.48,6.46,85.15,54.64,5.73,85.47,54.80,4.99,85.79,54.95,4.26,86.10,55.11,3.53,86.42,55.26,2.79,86.74,55.42,2.06,87.06,55.58,1.33,87.37,55.73,0.59,87.69,55.89,-0.14,88.01,56.05,-0.87,88.34,56.21,-1.58,89.06,56.45,-1.28,89.77,56.69,-0.96,90.46,56.92,-0.60,91.03,57.10,-0.04,91.58,57.27,0.53,92.13,57.44,1.11,92.68,57.60,1.68,93.22,57.77,2.26,93.77,57.93,2.85,94.24,58.07,3.50,94.65,58.18,4.19,95.00,58.27,4.92,95.25,58.32,5.69,95.42,58.33,6.49],
        [91.32,53.90,7.09,91.32,53.88,7.36,91.32,53.87,7.64,91.32,53.85,7.91,91.28,53.82,8.18,91.22,53.78,8.45,91.16,53.75,8.71,91.09,53.71,8.98,90.98,53.65,9.22,90.87,53.60,9.47,90.76,53.55,9.71,90.57,53.47,9.89,90.35,53.38,10.04,90.14,53.29,10.19,89.93,53.21,10.34,89.71,53.12,10.49,89.50,53.03,10.64,89.28,52.95,10.78,89.05,52.86,10.90,88.82,52.77,11.02,88.58,52.68,11.08,88.32,52.58,11.04,88.07,52.49,11.00,87.83,52.41,10.89,87.60,52.34,10.76,87.44,52.29,10.55,87.33,52.26,10.30,87.30,52.27,10.03,87.29,52.28,9.75,87.29,52.30,9.48,87.33,52.33,9.21,87.39,52.36,8.94,87.45,52.40,8.68,87.51,52.43,8.41,87.59,52.48,8.15,87.68,52.52,7.89,87.76,52.57,7.64,87.85,52.62,7.38,87.93,52.66,7.12,88.02,52.71,6.87,88.12,52.76,6.62,88.22,52.81,6.36,88.32,52.86,6.11,88.42,52.91,5.86,88.52,52.96,5.61,88.62,53.01,5.36,88.72,53.06,5.11,88.82,53.11,4.86,88.93,53.16,4.61,89.04,53.22,4.36,89.19,53.28,4.24,89.42,53.36,4.36,89.64,53.43,4.51,89.86,53.50,4.67,90.07,53.57,4.82,90.29,53.64,4.98,90.51,53.71,5.13,90.72,53.78,5.29,90.86,53.81,5.52,90.96,53.84,5.77,91.07,53.86,6.02,91.15,53.88,6.28,91.21,53.89,6.55,91.27,53.89,6.82]],
    "28" : [
        [55.58,111.50,-8.11,55.59,111.47,-7.52,55.59,111.43,-6.93,55.59,111.40,-6.34,55.59,111.37,-5.75,55.59,111.33,-5.17,55.60,111.30,-4.58,55.60,111.26,-3.99,55.60,111.23,-3.40,55.60,111.20,-2.82,55.60,111.16,-2.23,55.60,111.13,-1.64,55.60,111.09,-1.05,55.61,111.06,-0.47,55.61,111.02,0.12,55.61,110.99,0.71,55.61,110.96,1.30,55.61,110.92,1.88,55.61,110.89,2.47,55.61,110.85,3.06,55.61,110.82,3.65,55.53,110.74,4.22,55.38,110.63,4.78,55.18,110.49,5.31,54.70,110.24,5.23,54.38,110.09,4.83,54.18,110.02,4.28,53.99,109.95,3.73,53.88,109.93,3.15,53.78,109.91,2.57,53.68,109.89,1.99,53.59,109.88,1.41,53.51,109.87,0.82,53.43,109.86,0.24,53.35,109.85,-0.34,53.27,109.84,-0.93,53.19,109.84,-1.51,53.11,109.83,-2.09,53.03,109.82,-2.67,52.95,109.81,-3.26,52.86,109.80,-3.84,52.78,109.79,-4.42,52.70,109.78,-5.01,52.62,109.78,-5.59,52.55,109.77,-6.17,52.47,109.76,-6.75,52.39,109.76,-7.34,52.31,109.75,-7.92,52.26,109.75,-8.51,52.24,109.78,-9.09,52.32,109.86,-9.67,52.47,109.97,-10.23,52.67,110.11,-10.76,52.92,110.27,-11.26,53.27,110.48,-11.69,53.65,110.71,-12.07,54.15,110.98,-12.17,54.62,111.22,-11.98,54.96,111.37,-11.56,55.15,111.44,-11.00,55.31,111.49,-10.44,55.42,111.52,-9.86,55.50,111.53,-9.28,55.56,111.52,-8.69],
        [59.36,110.58,-6.47,59.36,110.52,-5.56,59.37,110.47,-4.64,59.37,110.42,-3.73,59.37,110.37,-2.82,59.37,110.31,-1.91,59.38,110.26,-1.00,59.38,110.21,-0.08,59.38,110.15,0.83,59.38,110.10,1.74,59.38,110.05,2.65,59.39,109.99,3.56,59.37,109.93,4.48,59.32,109.85,5.38,59.22,109.75,6.29,59.08,109.62,7.18,58.91,109.47,8.06,58.67,109.29,8.93,58.33,109.07,9.74,57.92,108.80,10.51,57.34,108.46,11.12,56.57,108.05,11.16,55.88,107.71,10.67,55.29,107.43,10.03,54.73,107.17,9.36,54.21,106.94,8.65,53.89,106.82,7.82,53.73,106.79,6.92,53.60,106.77,6.02,53.49,106.77,5.11,53.40,106.77,4.20,53.34,106.79,3.29,53.27,106.81,2.38,53.21,106.84,1.47,53.15,106.86,0.56,53.09,106.88,-0.35,53.03,106.90,-1.26,52.96,106.92,-2.17,52.90,106.94,-3.09,52.84,106.96,-4.00,52.78,106.98,-4.91,52.72,107.01,-5.82,52.65,107.03,-6.73,52.60,107.05,-7.64,52.56,107.09,-8.55,52.58,107.15,-9.46,52.63,107.23,-10.37,52.71,107.33,-11.28,52.83,107.44,-12.18,52.98,107.57,-13.07,53.43,107.85,-13.72,54.11,108.24,-14.18,54.83,108.65,-14.58,55.59,109.07,-14.85,56.38,109.49,-14.77,57.00,109.78,-14.20,57.43,109.96,-13.41,57.82,110.12,-12.60,58.17,110.26,-11.77,58.49,110.38,-10.92,58.77,110.47,-10.06,59.00,110.55,-9.18,59.18,110.59,-8.28,59.31,110.60,-7.38],
        [63.14,109.65,-4.83,63.14,109.57,-3.47,63.15,109.49,-2.03,63.15,109.41,-0.58,63.15,109.32,0.86,63.16,109.24,2.30,63.16,109.15,3.75,63.15,109.08,4.97,63.08,108.97,6.19,62.94,108.82,7.35,62.72,108.64,8.46,62.43,108.42,9.55,62.04,108.16,10.47,61.61,107.88,11.26,61.07,107.55,12.02,60.46,107.19,12.73,59.74,106.76,13.40,59.09,106.39,13.87,58.63,106.13,14.04,58.29,105.95,14.09,57.86,105.73,13.93,57.33,105.47,13.64,56.37,105.00,12.94,55.88,104.76,12.57,55.78,104.72,12.28,55.66,104.69,11.73,55.37,104.62,10.35,55.10,104.56,8.98,54.88,104.52,7.71,54.71,104.50,6.44,54.59,104.52,5.12,54.49,104.55,3.72,54.39,104.58,2.29,54.29,104.61,0.84,54.20,104.65,-0.60,54.10,104.68,-2.04,54.00,104.71,-3.48,53.90,104.75,-4.92,53.80,104.78,-6.37,53.76,104.83,-7.58,53.78,104.91,-8.77,53.88,105.04,-9.99,54.04,105.19,-11.22,54.26,105.38,-12.46,54.54,105.61,-13.70,54.85,105.84,-14.93,54.98,105.94,-15.38,55.08,106.00,-15.56,55.25,106.10,-15.62,56.29,106.67,-15.94,57.12,107.12,-16.15,57.72,107.45,-16.19,58.16,107.67,-16.08,58.57,107.88,-15.85,58.89,108.03,-15.58,59.52,108.31,-14.69,60.21,108.61,-13.59,60.80,108.87,-12.56,61.37,109.10,-11.46,61.89,109.31,-10.30,62.28,109.46,-9.25,62.64,109.58,-8.11,62.90,109.65,-7.02,63.07,109.68,-5.92],
        [66.91,108.73,-3.19,66.92,108.66,-1.98,66.92,108.59,-0.76,66.92,108.52,0.46,66.93,108.45,1.67,66.93,108.37,2.89,66.93,108.30,4.10,66.93,108.23,5.32,66.87,108.13,6.53,66.74,107.99,7.73,66.53,107.80,8.92,66.21,107.57,10.07,65.76,107.26,11.16,65.19,106.90,12.17,64.50,106.48,13.08,63.72,106.01,13.89,62.86,105.51,14.59,61.95,105.00,15.22,61.01,104.46,15.77,59.99,103.90,16.11,58.95,103.36,15.88,57.96,102.87,15.36,57.26,102.54,14.55,56.98,102.46,13.37,56.72,102.39,12.18,56.48,102.34,10.99,56.24,102.28,9.80,56.04,102.25,8.60,55.90,102.24,7.39,55.79,102.26,6.17,55.70,102.28,4.96,55.61,102.31,3.75,55.53,102.34,2.53,55.45,102.36,1.32,55.37,102.39,0.10,55.28,102.42,-1.11,55.20,102.45,-2.33,55.12,102.48,-3.54,55.04,102.50,-4.76,54.96,102.54,-5.97,54.95,102.60,-7.19,55.02,102.71,-8.40,55.16,102.86,-9.60,55.35,103.03,-10.79,55.59,103.23,-11.97,55.87,103.44,-13.14,56.17,103.67,-14.30,56.47,103.90,-15.45,56.96,104.22,-16.51,57.99,104.79,-16.73,59.06,105.36,-16.85,60.12,105.91,-16.66,60.99,106.34,-15.95,61.75,106.68,-15.07,62.48,107.02,-14.15,63.17,107.33,-13.20,63.82,107.62,-12.21,64.44,107.88,-11.19,65.00,108.12,-10.14,65.50,108.33,-9.05,65.95,108.50,-7.93,66.33,108.63,-6.78,66.63,108.72,-5.61,66.83,108.75,-4.41],
        [70.69,107.80,-1.55,70.69,107.73,-0.22,70.70,107.65,1.11,70.70,107.57,2.44,70.70,107.49,3.77,70.71,107.42,5.10,70.69,107.33,6.43,70.58,107.19,7.76,70.38,107.01,9.06,70.08,106.77,10.34,69.63,106.46,11.55,69.03,106.07,12.67,68.30,105.63,13.69,67.45,105.12,14.59,66.52,104.58,15.37,65.52,104.01,16.05,64.48,103.42,16.64,63.39,102.82,17.12,62.28,102.20,17.51,61.13,101.58,17.71,59.99,100.99,17.37,58.89,100.43,16.89,58.48,100.29,15.63,58.15,100.19,14.34,57.87,100.12,13.04,57.60,100.05,11.74,57.35,100.00,10.43,57.16,99.98,9.11,57.04,99.99,7.78,56.93,100.01,6.45,56.83,100.04,5.12,56.74,100.07,3.79,56.65,100.10,2.46,56.56,100.13,1.13,56.47,100.16,-0.20,56.38,100.19,-1.53,56.29,100.22,-2.86,56.20,100.25,-4.19,56.14,100.30,-5.52,56.17,100.40,-6.85,56.30,100.54,-8.17,56.50,100.73,-9.47,56.76,100.94,-10.76,57.07,101.18,-12.04,57.40,101.43,-13.31,57.72,101.68,-14.58,58.07,101.94,-15.83,58.68,102.33,-16.94,59.79,102.94,-17.21,60.96,103.57,-17.33,62.12,104.17,-17.10,63.03,104.60,-16.25,63.87,104.99,-15.29,64.71,105.38,-14.33,65.54,105.77,-13.36,66.35,106.14,-12.36,67.11,106.48,-11.32,67.81,106.79,-10.24,68.47,107.07,-9.11,69.06,107.31,-7.94,69.58,107.52,-6.73,70.02,107.68,-5.48,70.36,107.78,-4.19,70.59,107.83,-2.88],
        [74.46,106.88,0.08,74.47,106.79,1.52,74.47,106.71,2.96,74.48,106.63,4.40,74.48,106.54,5.84,74.45,106.44,7.28,74.32,106.29,8.71,74.06,106.07,10.11,73.67,105.78,11.47,73.11,105.41,12.74,72.40,104.96,13.91,71.54,104.44,14.95,70.57,103.87,15.85,69.52,103.26,16.62,68.40,102.63,17.29,67.24,101.98,17.83,66.03,101.32,18.26,64.80,100.64,18.58,63.56,99.96,18.86,62.30,99.29,18.95,61.05,98.65,18.61,60.11,98.20,17.80,59.63,98.02,16.46,59.27,97.91,15.07,58.96,97.83,13.66,58.68,97.76,12.25,58.44,97.72,10.83,58.28,97.72,9.39,58.17,97.74,7.96,58.06,97.77,6.52,57.95,97.80,5.08,57.85,97.83,3.64,57.76,97.87,2.20,57.66,97.90,0.77,57.56,97.93,-0.67,57.46,97.97,-2.11,57.36,98.00,-3.55,57.33,98.07,-4.99,57.41,98.20,-6.42,57.60,98.38,-7.84,57.86,98.60,-9.24,58.19,98.86,-10.62,58.54,99.13,-11.99,58.90,99.40,-13.37,59.25,99.67,-14.74,59.63,99.95,-16.10,60.32,100.39,-17.27,61.47,101.02,-17.68,62.74,101.70,-17.81,63.99,102.35,-17.56,64.97,102.82,-16.61,65.89,103.25,-15.59,66.80,103.67,-14.56,67.71,104.10,-13.53,68.62,104.52,-12.49,69.52,104.93,-11.44,70.38,105.33,-10.35,71.20,105.69,-9.22,71.94,106.01,-8.03,72.61,106.30,-6.78,73.20,106.53,-5.49,73.70,106.72,-4.15,74.09,106.85,-2.77,74.36,106.90,-1.35],
        [78.24,105.95,1.72,78.25,105.84,3.65,78.25,105.72,5.70,78.25,105.61,7.50,78.11,105.45,9.10,77.87,105.23,10.54,77.50,104.95,11.89,76.97,104.60,13.16,76.32,104.18,14.29,75.55,103.71,15.32,74.64,103.18,16.27,73.59,102.57,17.11,72.41,101.89,17.87,71.15,101.19,18.49,69.84,100.46,18.99,68.38,99.66,19.37,66.82,98.81,19.69,65.26,97.97,19.91,64.07,97.34,19.98,63.04,96.80,19.84,62.12,96.32,19.62,61.83,96.17,19.46,61.56,96.05,19.05,61.05,95.84,18.01,60.61,95.69,16.69,60.22,95.58,15.04,59.83,95.49,13.15,59.58,95.45,11.54,59.42,95.47,9.67,59.26,95.51,7.66,59.11,95.55,5.65,58.97,95.60,3.61,58.84,95.64,1.56,58.70,95.69,-0.49,58.56,95.74,-2.54,58.52,95.81,-4.06,58.63,95.96,-5.61,58.87,96.18,-7.22,59.23,96.47,-8.88,59.73,96.85,-10.81,60.22,97.23,-12.75,60.72,97.61,-14.69,61.14,97.92,-16.21,61.50,98.16,-17.01,62.09,98.52,-17.74,62.37,98.69,-18.04,62.70,98.87,-18.09,64.31,99.73,-18.26,65.09,100.15,-18.25,65.59,100.40,-18.09,66.08,100.64,-17.76,67.18,101.16,-16.60,68.44,101.75,-15.22,69.74,102.35,-13.79,71.03,102.95,-12.35,72.30,103.54,-10.91,73.51,104.10,-9.49,74.54,104.57,-8.19,75.43,104.97,-6.86,76.22,105.31,-5.49,76.89,105.58,-4.06,77.46,105.79,-2.59,77.87,105.92,-1.17,78.13,105.98,0.27],
        [82.02,105.03,3.36,82.02,104.93,5.00,82.03,104.84,6.65,82.01,104.73,8.29,81.86,104.56,9.92,81.55,104.29,11.51,81.05,103.94,13.04,80.36,103.49,14.46,79.50,102.95,15.75,78.47,102.34,16.88,77.32,101.67,17.84,76.08,100.96,18.65,74.76,100.22,19.30,73.39,99.46,19.80,71.98,98.69,20.16,70.55,97.92,20.43,69.11,97.14,20.60,67.66,96.37,20.71,66.21,95.59,20.75,64.76,94.82,20.69,63.33,94.08,20.42,62.40,93.66,19.14,61.80,93.43,17.63,61.40,93.31,16.04,61.07,93.23,14.43,60.80,93.19,12.81,60.66,93.21,11.17,60.53,93.24,9.53,60.40,93.26,7.89,60.28,93.30,6.25,60.17,93.34,4.60,60.05,93.37,2.96,59.94,93.41,1.32,59.83,93.45,-0.32,59.73,93.49,-1.96,59.74,93.60,-3.60,59.91,93.79,-5.23,60.21,94.04,-6.83,60.59,94.33,-8.40,60.99,94.64,-9.96,61.39,94.95,-11.53,61.79,95.25,-13.10,62.19,95.56,-14.66,62.61,95.87,-16.22,63.34,96.34,-17.60,64.44,96.98,-18.56,65.88,97.76,-18.71,67.31,98.51,-18.55,68.46,99.06,-17.54,69.53,99.56,-16.39,70.60,100.06,-15.25,71.66,100.56,-14.09,72.72,101.05,-12.94,73.78,101.55,-11.78,74.83,102.04,-10.61,75.88,102.52,-9.43,76.91,103.00,-8.25,77.92,103.47,-7.03,78.86,103.89,-5.75,79.71,104.26,-4.39,80.45,104.57,-2.95,81.07,104.81,-1.45,81.56,104.98,0.11,81.88,105.05,1.72],
        [85.79,104.10,5.00,85.80,103.98,7.17,85.77,103.85,9.10,85.60,103.66,10.71,85.27,103.39,12.29,84.79,103.05,13.71,84.15,102.63,15.04,83.35,102.13,16.28,82.40,101.57,17.39,81.34,100.94,18.35,80.11,100.24,19.21,78.74,99.47,19.93,77.32,98.68,20.49,75.78,97.84,20.89,74.16,96.96,21.17,72.38,96.00,21.33,70.57,95.03,21.40,68.70,94.04,21.40,66.82,93.04,21.35,65.19,92.19,21.18,64.75,91.96,21.10,64.34,91.76,20.79,63.65,91.44,19.95,63.17,91.24,19.02,62.74,91.09,17.71,62.32,90.98,15.81,62.01,90.92,14.05,61.84,90.95,11.96,61.66,90.99,9.69,61.48,91.03,7.42,61.33,91.09,5.13,61.17,91.14,2.82,61.01,91.19,0.51,60.90,91.25,-1.45,60.97,91.38,-3.07,61.21,91.61,-4.78,61.63,91.94,-6.63,62.19,92.37,-8.82,62.75,92.80,-11.01,63.31,93.23,-13.20,63.87,93.65,-15.40,64.29,93.97,-16.83,64.62,94.18,-17.49,65.61,94.77,-18.64,65.93,94.97,-19.00,66.20,95.11,-19.04,67.87,96.01,-19.20,68.61,96.40,-19.16,69.17,96.69,-18.93,69.82,97.00,-18.32,71.23,97.65,-16.84,72.74,98.36,-15.23,74.24,99.07,-13.63,75.74,99.77,-12.02,77.22,100.46,-10.42,78.68,101.14,-8.81,80.11,101.81,-7.21,81.45,102.43,-5.66,82.53,102.92,-4.25,83.46,103.33,-2.81,84.23,103.65,-1.33,84.90,103.91,0.29,85.36,104.06,1.84,85.67,104.13,3.41],
        [89.57,103.18,6.63,89.58,103.07,8.47,89.50,102.93,10.30,89.23,102.67,12.10,88.73,102.31,13.83,88.02,101.83,15.46,87.09,101.25,16.94,85.98,100.58,18.24,84.72,99.84,19.35,83.32,99.05,20.25,81.84,98.22,20.95,80.29,97.37,21.45,78.70,96.50,21.79,77.09,95.63,21.96,75.47,94.77,22.03,73.85,93.90,22.04,72.22,93.04,21.98,70.60,92.19,21.90,68.97,91.33,21.80,67.35,90.47,21.69,65.75,89.63,21.49,64.57,89.08,20.20,63.88,88.81,18.53,63.48,88.71,16.73,63.19,88.66,14.92,63.04,88.69,13.09,62.90,88.72,11.25,62.75,88.76,9.42,62.62,88.79,7.59,62.49,88.83,5.75,62.37,88.88,3.92,62.24,88.92,2.08,62.12,88.96,0.25,62.13,89.08,-1.59,62.35,89.30,-3.40,62.74,89.61,-5.17,63.18,89.95,-6.92,63.63,90.30,-8.67,64.08,90.64,-10.42,64.53,90.98,-12.17,64.97,91.32,-13.92,65.42,91.66,-15.68,65.98,92.06,-17.38,67.04,92.71,-18.73,68.37,93.46,-19.54,69.99,94.33,-19.60,71.41,95.03,-18.75,72.62,95.60,-17.50,73.84,96.17,-16.24,75.05,96.74,-14.98,76.25,97.31,-13.71,77.46,97.87,-12.44,78.66,98.43,-11.16,79.86,99.00,-9.89,81.05,99.56,-8.60,82.25,100.11,-7.32,83.43,100.66,-6.02,84.60,101.21,-4.71,85.73,101.73,-3.36,86.78,102.20,-1.92,87.68,102.59,-0.37,88.43,102.89,1.28,89.02,103.10,3.01,89.41,103.20,4.80],
        [93.34,102.25,8.27,93.33,102.12,10.28,93.14,101.92,11.98,92.76,101.62,13.64,92.19,101.23,15.20,91.44,100.74,16.68,90.53,100.18,18.00,89.47,99.54,19.19,88.26,98.84,20.21,86.94,98.09,21.05,85.45,97.26,21.75,83.94,96.42,22.23,82.30,95.54,22.53,80.51,94.57,22.67,78.54,93.53,22.67,76.43,92.41,22.56,74.20,91.23,22.40,71.93,90.04,22.24,69.67,88.84,22.08,68.06,88.00,21.96,66.78,87.32,21.86,66.53,87.20,21.70,65.69,86.81,20.73,65.26,86.62,19.97,64.87,86.49,18.74,64.48,86.40,16.81,64.29,86.42,14.73,64.09,86.47,12.20,63.89,86.51,9.68,63.71,86.56,7.17,63.54,86.62,4.61,63.36,86.68,2.05,63.29,86.75,0.14,63.43,86.93,-1.59,63.78,87.23,-3.43,64.38,87.69,-5.79,65.00,88.16,-8.23,65.62,88.64,-10.66,66.24,89.11,-13.09,66.87,89.59,-15.52,67.35,89.94,-17.18,67.71,90.17,-17.90,68.79,90.83,-19.18,69.36,91.17,-19.83,69.89,91.45,-19.85,71.11,92.10,-19.89,71.82,92.47,-19.82,72.37,92.75,-19.56,73.31,93.20,-18.65,74.98,93.99,-16.95,76.66,94.78,-15.24,78.37,95.58,-13.51,80.06,96.38,-11.78,81.76,97.18,-10.04,83.43,97.96,-8.31,85.09,98.74,-6.56,86.73,99.51,-4.80,88.30,100.25,-3.09,89.63,100.86,-1.52,90.71,101.35,-0.03,91.59,101.72,1.50,92.32,102.01,3.19,92.87,102.20,4.87,93.20,102.28,6.56],
        [96.87,101.22,9.51,96.81,101.08,11.31,96.54,100.84,13.04,96.08,100.49,14.76,95.42,100.04,16.37,94.59,99.51,17.84,93.58,98.89,19.20,92.41,98.20,20.40,91.12,97.45,21.41,89.74,96.67,22.20,88.25,95.85,22.79,86.72,95.01,23.15,84.98,94.08,23.31,83.18,93.13,23.26,80.86,91.90,23.08,78.50,90.66,22.89,76.15,89.42,22.71,74.20,88.39,22.54,72.25,87.36,22.38,70.29,86.33,22.21,68.34,85.31,22.05,67.45,84.84,21.97,66.79,84.53,21.21,66.33,84.33,20.52,65.99,84.21,19.47,65.65,84.13,17.69,65.45,84.17,15.29,65.24,84.22,12.65,65.04,84.26,10.02,64.85,84.32,7.39,64.67,84.38,4.73,64.50,84.44,2.15,64.51,84.55,0.48,64.76,84.79,-1.32,65.28,85.20,-3.51,65.93,85.69,-6.05,66.58,86.19,-8.59,67.23,86.68,-11.12,67.87,87.18,-13.66,68.52,87.67,-16.20,68.98,88.00,-17.61,69.36,88.24,-18.25,70.64,89.01,-19.74,70.87,89.14,-19.96,71.32,89.38,-19.98,72.77,90.16,-19.99,73.54,90.56,-19.81,74.04,90.80,-19.50,75.46,91.47,-18.09,77.25,92.32,-16.29,79.05,93.17,-14.49,80.84,94.01,-12.70,82.63,94.86,-10.90,84.43,95.71,-9.09,86.22,96.56,-7.29,88.01,97.40,-5.49,89.79,98.24,-3.68,91.55,99.07,-1.90,92.99,99.74,-0.35,94.16,100.27,1.15,95.09,100.68,2.67,95.82,100.97,4.26,96.37,101.16,5.98,96.72,101.24,7.74],
        [98.22,99.11,9.38,98.16,98.98,11.14,97.90,98.73,12.93,97.43,98.38,14.66,96.77,97.93,16.30,95.95,97.40,17.77,94.93,96.78,19.16,93.76,96.09,20.37,92.50,95.35,21.38,91.09,94.56,22.21,89.54,93.69,22.84,88.02,92.87,23.21,86.42,92.00,23.36,84.36,90.91,23.32,82.18,89.76,23.17,80.04,88.63,22.99,77.95,87.53,22.82,75.86,86.43,22.64,73.78,85.34,22.47,71.69,84.24,22.29,69.60,83.14,22.12,68.18,82.39,21.99,67.80,82.21,21.57,67.36,82.01,20.95,67.08,81.92,20.06,66.81,81.87,18.42,66.61,81.92,15.76,66.40,81.97,13.11,66.19,82.01,10.45,66.00,82.07,7.82,65.82,82.13,5.14,65.67,82.19,2.75,65.75,82.33,1.12,66.05,82.60,-0.68,66.66,83.07,-3.10,67.31,83.57,-5.66,67.97,84.07,-8.21,68.62,84.56,-10.76,69.27,85.06,-13.31,69.92,85.56,-15.86,70.42,85.93,-17.59,70.80,86.17,-18.32,71.89,86.83,-19.60,72.23,87.03,-19.96,72.54,87.20,-20.00,74.25,88.11,-20.00,74.91,88.45,-19.88,75.44,88.72,-19.60,76.67,89.30,-18.37,78.48,90.15,-16.55,80.29,91.01,-14.74,82.10,91.86,-12.93,83.91,92.71,-11.11,85.71,93.57,-9.30,87.51,94.42,-7.49,89.31,95.27,-5.68,91.08,96.10,-3.88,92.84,96.93,-2.08,94.45,97.69,-0.40,95.54,98.18,0.99,96.43,98.57,2.46,97.17,98.87,4.10,97.73,99.06,5.83,98.07,99.14,7.56],
        [99.39,96.90,9.25,99.34,96.77,11.01,99.07,96.52,12.80,98.60,96.17,14.53,97.94,95.72,16.17,97.12,95.20,17.65,96.10,94.57,19.03,94.93,93.88,20.25,93.65,93.14,21.26,92.25,92.34,22.08,90.70,91.49,22.71,89.18,90.66,23.08,87.58,89.79,23.23,85.43,88.66,23.19,83.20,87.47,23.14,80.93,86.27,22.97,78.55,85.02,22.80,76.36,83.86,22.63,74.42,82.84,22.46,72.49,81.82,22.30,70.55,80.80,22.14,68.88,79.92,22.00,68.72,79.85,21.81,68.37,79.69,21.35,68.15,79.62,20.56,67.96,79.63,18.76,67.75,79.67,16.10,67.54,79.72,13.44,67.33,79.77,10.77,67.15,79.83,8.13,66.96,79.89,5.45,66.86,79.95,3.37,67.00,80.13,1.72,67.40,80.46,-0.20,68.06,80.96,-2.76,68.71,81.46,-5.31,69.36,81.95,-7.87,70.02,82.45,-10.43,70.67,82.95,-12.98,71.32,83.45,-15.54,71.87,83.86,-17.54,72.26,84.12,-18.42,73.15,84.66,-19.48,73.60,84.92,-19.96,73.90,85.09,-20.00,75.58,85.98,-20.00,76.29,86.35,-19.91,76.87,86.64,-19.62,77.88,87.12,-18.60,79.68,87.97,-16.79,81.48,88.82,-14.98,83.29,89.67,-13.16,85.08,90.52,-11.34,86.87,91.36,-9.53,88.65,92.20,-7.71,90.43,93.04,-5.89,92.21,93.87,-4.06,93.98,94.71,-2.24,95.62,95.48,-0.54,96.70,95.97,0.85,97.60,96.36,2.32,98.34,96.66,3.96,98.90,96.85,5.69,99.24,96.93,7.43],
        [100.56,94.70,9.12,100.48,94.54,11.14,100.15,94.24,13.11,99.56,93.82,15.01,98.73,93.27,16.78,97.68,92.62,18.39,96.44,91.87,19.80,95.02,91.05,20.99,93.46,90.16,21.93,91.78,89.23,22.61,90.04,88.28,23.00,88.25,87.32,23.09,86.46,86.37,23.06,84.67,85.42,23.02,82.87,84.47,22.99,81.08,83.52,22.89,79.29,82.58,22.75,77.50,81.64,22.62,75.71,80.69,22.49,73.92,79.75,22.35,72.13,78.80,22.21,70.34,77.86,22.06,69.24,77.35,20.93,69.09,77.38,18.90,68.93,77.42,16.88,68.77,77.45,14.86,68.61,77.49,12.83,68.45,77.53,10.81,68.31,77.57,8.79,68.18,77.62,6.76,68.05,77.67,4.74,68.20,77.87,2.73,68.64,78.22,0.78,69.13,78.60,-1.15,69.63,78.97,-3.09,70.12,79.35,-5.02,70.61,79.73,-6.95,71.11,80.11,-8.88,71.60,80.48,-10.81,72.09,80.86,-12.75,72.59,81.24,-14.68,73.08,81.61,-16.61,73.72,82.07,-18.48,74.92,82.79,-19.94,76.70,83.74,-20.00,78.40,84.61,-19.49,79.76,85.25,-18.13,81.11,85.89,-16.75,82.46,86.53,-15.38,83.81,87.17,-14.01,85.16,87.80,-12.63,86.51,88.44,-11.25,87.85,89.07,-9.87,89.20,89.70,-8.49,90.54,90.33,-7.11,91.89,90.97,-5.72,93.23,91.60,-4.34,94.58,92.23,-2.96,95.92,92.87,-1.58,97.23,93.48,-0.15,98.35,93.98,1.46,99.27,94.36,3.23,99.95,94.61,5.12,100.38,94.72,7.10],
        [101.74,92.50,8.99,101.66,92.33,11.01,101.32,92.04,12.98,100.73,91.61,14.87,99.91,91.07,16.64,98.86,90.42,18.25,97.61,89.67,19.67,96.20,88.85,20.86,94.64,87.96,21.80,92.97,87.03,22.47,91.22,86.08,22.86,89.43,85.13,22.96,87.64,84.18,22.93,85.85,83.22,22.89,84.06,82.27,22.86,82.27,81.32,22.82,80.48,80.38,22.74,78.69,79.43,22.61,76.90,78.49,22.47,75.11,77.54,22.34,73.32,76.60,22.21,71.53,75.66,22.08,70.38,75.10,21.14,70.22,75.14,19.12,70.06,75.17,17.10,69.91,75.21,15.07,69.75,75.25,13.05,69.59,75.28,11.03,69.45,75.33,9.01,69.32,75.38,6.98,69.26,75.47,4.96,69.56,75.75,2.98,70.05,76.12,1.05,70.55,76.50,-0.89,71.04,76.88,-2.82,71.53,77.25,-4.75,72.03,77.63,-6.68,72.52,78.01,-8.61,73.01,78.38,-10.54,73.51,78.76,-12.47,74.00,79.14,-14.40,74.49,79.51,-16.33,75.05,79.92,-18.24,76.15,80.60,-19.79,77.87,81.53,-20.00,79.61,82.43,-19.63,80.96,83.07,-18.27,82.31,83.70,-16.89,83.66,84.34,-15.52,85.00,84.97,-14.13,86.35,85.60,-12.75,87.69,86.24,-11.37,89.03,86.87,-9.99,90.38,87.50,-8.61,91.72,88.13,-7.23,93.07,88.77,-5.85,94.41,89.40,-4.47,95.75,90.03,-3.09,97.10,90.66,-1.70,98.40,91.27,-0.28,99.53,91.78,1.33,100.44,92.16,3.10,101.12,92.41,4.99,101.56,92.52,6.97],
        [102.91,90.29,8.86,102.83,90.13,10.87,102.49,89.83,12.84,101.91,89.41,14.73,101.09,88.87,16.49,100.05,88.22,18.10,98.81,87.48,19.51,97.40,86.66,20.70,95.85,85.78,21.65,94.18,84.85,22.33,92.45,83.91,22.72,90.67,82.95,22.83,88.88,82.01,22.80,87.10,81.06,22.76,85.31,80.11,22.73,83.52,79.16,22.69,81.74,78.22,22.65,79.95,77.27,22.60,78.17,76.33,22.47,76.38,75.39,22.33,74.60,74.45,22.20,72.81,73.50,22.07,71.50,72.86,21.15,71.34,72.90,19.14,71.18,72.93,17.12,71.03,72.97,15.11,70.87,73.00,13.09,70.72,73.04,11.07,70.58,73.09,9.06,70.44,73.14,7.04,70.57,73.32,5.04,71.01,73.68,3.10,71.51,74.05,1.17,72.00,74.43,-0.75,72.49,74.80,-2.68,72.98,75.18,-4.60,73.47,75.55,-6.53,73.96,75.93,-8.45,74.46,76.31,-10.38,74.95,76.68,-12.30,75.44,77.06,-14.23,75.93,77.43,-16.15,76.45,77.82,-18.07,77.43,78.44,-19.70,79.11,79.35,-20.00,80.85,80.26,-19.67,82.22,80.90,-18.33,83.56,81.53,-16.96,84.90,82.16,-15.58,86.24,82.80,-14.20,87.58,83.43,-12.82,88.92,84.06,-11.45,90.26,84.69,-10.07,91.60,85.32,-8.69,92.94,85.95,-7.32,94.28,86.58,-5.94,95.62,87.21,-4.56,96.96,87.84,-3.19,98.30,88.47,-1.81,99.59,89.08,-0.39,100.71,89.58,1.22,101.62,89.96,2.99,102.30,90.20,4.87,102.73,90.32,6.84],
        [104.08,88.09,8.73,104.00,87.93,10.73,103.67,87.63,12.70,103.09,87.21,14.58,102.27,86.67,16.34,101.24,86.03,17.94,100.01,85.29,19.36,98.61,84.47,20.55,97.06,83.60,21.50,95.41,82.68,22.18,93.67,81.73,22.59,91.90,80.78,22.70,90.12,79.84,22.67,88.34,78.89,22.63,86.56,77.95,22.60,84.78,77.00,22.56,83.00,76.06,22.53,81.22,75.12,22.49,79.44,74.17,22.45,77.66,73.23,22.33,75.88,72.29,22.20,74.10,71.36,22.06,72.75,70.69,21.13,72.46,70.66,19.16,72.30,70.69,17.15,72.15,70.73,15.14,71.99,70.76,13.13,71.84,70.80,11.12,71.71,70.85,9.11,71.64,70.93,7.10,71.98,71.23,5.14,72.47,71.60,3.22,72.96,71.98,1.30,73.45,72.35,-0.62,73.94,72.73,-2.53,74.43,73.10,-4.45,74.92,73.48,-6.37,75.41,73.85,-8.29,75.90,74.23,-10.21,76.39,74.60,-12.13,76.88,74.98,-14.05,77.37,75.35,-15.97,77.87,75.73,-17.88,78.71,76.28,-19.60,80.35,77.17,-20.00,82.09,78.08,-19.70,83.46,78.73,-18.38,84.80,79.36,-17.01,86.14,79.99,-15.64,87.47,80.62,-14.27,88.81,81.25,-12.89,90.14,81.88,-11.52,91.48,82.51,-10.15,92.81,83.14,-8.78,94.15,83.76,-7.40,95.49,84.39,-6.03,96.82,85.02,-4.66,98.16,85.65,-3.29,99.49,86.28,-1.91,100.79,86.88,-0.49,101.90,87.38,1.11,102.80,87.75,2.87,103.47,88.00,4.75,103.90,88.11,6.72],
        [105.25,85.88,8.59,105.17,85.72,10.59,104.84,85.43,12.55,104.27,85.01,14.43,103.46,84.48,16.19,102.43,83.84,17.79,101.21,83.10,19.20,99.81,82.29,20.39,98.28,81.42,21.34,96.63,80.50,22.03,94.90,79.56,22.45,93.14,78.61,22.57,91.36,77.67,22.54,89.59,76.73,22.50,87.81,75.79,22.47,86.04,74.85,22.43,84.27,73.90,22.40,82.49,72.96,22.36,80.72,72.02,22.32,78.94,71.08,22.29,77.17,70.14,22.19,75.39,69.21,22.06,74.06,68.55,21.10,73.58,68.41,19.19,73.42,68.45,17.19,73.27,68.49,15.18,73.11,68.52,13.18,72.97,68.57,11.18,72.84,68.61,9.17,72.96,68.79,7.18,73.43,69.16,5.27,73.92,69.53,3.35,74.40,69.91,1.44,74.89,70.28,-0.47,75.38,70.65,-2.38,75.87,71.03,-4.30,76.36,71.40,-6.21,76.85,71.77,-8.12,77.34,72.15,-10.04,77.82,72.52,-11.95,78.31,72.89,-13.86,78.80,73.27,-15.77,79.29,73.64,-17.69,80.00,74.12,-19.49,81.58,74.99,-20.00,83.31,75.90,-19.70,84.69,76.55,-18.40,86.03,77.18,-17.04,87.37,77.82,-15.69,88.71,78.45,-14.33,90.04,79.07,-12.96,91.37,79.70,-11.59,92.70,80.33,-10.23,94.03,80.95,-8.86,95.37,81.58,-7.49,96.70,82.21,-6.12,98.03,82.83,-4.75,99.36,83.46,-3.39,100.69,84.09,-2.02,101.98,84.69,-0.60,103.08,85.18,1.00,103.98,85.55,2.76,104.65,85.80,4.64,105.08,85.91,6.59],
        [106.42,83.68,8.46,106.35,83.52,10.46,106.02,83.23,12.41,105.45,82.81,14.28,104.64,82.28,16.03,103.62,81.64,17.63,102.41,80.91,19.04,101.02,80.11,20.24,99.49,79.24,21.19,97.85,78.32,21.88,96.14,77.39,22.31,94.38,76.44,22.44,92.61,75.51,22.41,90.84,74.57,22.37,89.07,73.63,22.34,87.31,72.69,22.30,85.54,71.75,22.27,83.77,70.82,22.23,82.00,69.88,22.20,80.23,68.94,22.16,78.46,68.00,22.12,76.70,67.07,22.05,75.37,66.42,21.07,74.70,66.17,19.23,74.55,66.21,17.23,74.39,66.24,15.24,74.24,66.28,13.24,74.10,66.33,11.25,74.02,66.40,9.25,74.39,66.71,7.31,74.88,67.09,5.41,75.36,67.46,3.50,75.85,67.83,1.59,76.34,68.20,-0.31,76.82,68.57,-2.22,77.31,68.95,-4.12,77.80,69.32,-6.03,78.28,69.69,-7.94,78.77,70.06,-9.84,79.26,70.43,-11.75,79.74,70.81,-13.65,80.23,71.18,-15.56,80.72,71.55,-17.47,81.33,71.98,-19.32,82.79,72.80,-19.99,84.52,73.70,-19.67,85.90,74.36,-18.38,87.24,74.99,-17.05,88.59,75.63,-15.70,89.93,76.26,-14.36,91.27,76.89,-13.01,92.60,77.52,-11.66,93.93,78.15,-10.30,95.26,78.77,-8.94,96.58,79.40,-7.57,97.91,80.02,-6.21,99.24,80.65,-4.85,100.56,81.27,-3.48,101.89,81.90,-2.12,103.17,82.49,-0.70,104.27,82.98,0.90,105.16,83.35,2.65,105.82,83.59,4.52,106.25,83.70,6.47],
        [107.60,81.47,8.33,107.52,81.32,10.31,107.20,81.03,12.25,106.63,80.62,14.11,105.84,80.09,15.86,104.83,79.46,17.45,103.63,78.74,18.86,102.25,77.94,20.06,100.74,77.08,21.02,99.12,76.17,21.72,97.42,75.24,22.16,95.67,74.30,22.31,93.91,73.37,22.28,92.16,72.44,22.25,90.40,71.51,22.21,88.64,70.57,22.18,86.88,69.64,22.14,85.13,68.71,22.10,83.37,67.78,22.07,81.61,66.84,22.03,79.85,65.91,22.00,78.10,64.98,21.96,76.72,64.30,21.13,75.88,63.95,19.36,75.68,63.96,17.39,75.52,64.00,15.41,75.37,64.04,13.42,75.24,64.08,11.44,75.33,64.25,9.47,75.81,64.62,7.57,76.29,64.99,5.68,76.77,65.36,3.78,77.26,65.73,1.89,77.74,66.10,-0.00,78.23,66.47,-1.90,78.71,66.83,-3.79,79.19,67.20,-5.69,79.68,67.57,-7.58,80.16,67.94,-9.48,80.65,68.31,-11.37,81.13,68.68,-13.26,81.61,69.05,-15.16,82.10,69.42,-17.05,82.62,69.81,-18.93,83.92,70.55,-19.72,85.62,71.44,-19.35,87.04,72.11,-18.13,88.41,72.77,-16.85,89.79,73.43,-15.57,91.15,74.07,-14.27,92.51,74.71,-12.97,93.85,75.35,-11.64,95.18,75.98,-10.31,96.51,76.61,-8.97,97.84,77.23,-7.62,99.16,77.85,-6.27,100.47,78.47,-4.91,101.79,79.09,-3.56,103.11,79.72,-2.20,104.38,80.31,-0.79,105.47,80.79,0.81,106.35,81.15,2.55,107.00,81.39,4.41,107.42,81.50,6.35],
        [108.77,79.27,8.20,108.69,79.11,10.16,108.38,78.83,12.08,107.83,78.43,13.92,107.05,77.92,15.65,106.07,77.30,17.23,104.89,76.59,18.63,103.55,75.81,19.83,102.07,74.96,20.80,100.47,74.07,21.53,98.80,73.15,21.99,97.08,72.22,22.18,95.34,71.30,22.16,93.61,70.38,22.12,91.87,69.46,22.09,90.14,68.54,22.05,88.40,67.62,22.01,86.66,66.70,21.96,84.93,65.78,21.89,83.19,64.86,21.83,81.46,63.94,21.76,79.72,63.02,21.68,78.14,62.20,21.32,77.30,61.86,19.57,76.82,61.72,17.71,76.67,61.75,15.75,76.52,61.79,13.79,76.39,61.84,11.83,76.69,62.11,9.92,77.17,62.48,8.05,77.65,62.84,6.18,78.12,63.21,4.31,78.60,63.57,2.44,79.08,63.94,0.56,79.56,64.30,-1.31,80.04,64.67,-3.18,80.51,65.03,-5.05,80.99,65.40,-6.92,81.47,65.76,-8.79,81.95,66.13,-10.66,82.43,66.49,-12.53,82.90,66.86,-14.40,83.38,67.22,-16.28,83.86,67.59,-18.15,85.00,68.24,-19.00,86.67,69.11,-18.58,88.09,69.79,-17.40,89.49,70.47,-16.19,90.89,71.14,-14.99,92.28,71.81,-13.77,93.67,72.47,-12.55,95.05,73.13,-11.32,96.42,73.79,-10.07,97.78,74.44,-8.81,99.13,75.08,-7.53,100.46,75.71,-6.23,101.77,76.33,-4.91,103.08,76.94,-3.57,104.38,77.56,-2.23,105.62,78.13,-0.83,106.69,78.60,0.76,107.55,78.96,2.49,108.19,79.19,4.33,108.60,79.30,6.25],
        [109.83,77.01,8.07,109.78,76.88,9.80,109.53,76.64,11.52,109.10,76.31,13.18,108.50,75.90,14.72,107.71,75.40,16.19,106.77,74.82,17.53,105.66,74.15,18.75,104.46,73.46,19.76,103.13,72.70,20.60,101.73,71.92,21.24,100.26,71.11,21.68,98.75,70.29,21.90,97.03,69.38,21.91,94.84,68.22,21.81,92.59,67.03,21.70,90.65,66.01,21.60,88.80,65.03,21.50,86.95,64.05,21.41,85.10,63.07,21.32,83.24,62.09,21.22,81.39,61.11,21.13,79.54,60.13,21.03,79.27,60.00,20.89,78.84,59.82,20.02,78.00,59.48,18.24,77.89,59.49,17.09,77.74,59.52,15.18,77.58,59.58,12.87,77.64,59.65,12.06,78.15,60.04,10.03,78.77,60.52,7.60,79.39,60.99,5.17,80.01,61.47,2.75,80.63,61.94,0.32,81.25,62.41,-2.11,81.87,62.89,-4.54,82.49,63.36,-6.96,83.11,63.83,-9.39,83.73,64.31,-11.82,84.35,64.78,-14.24,84.80,65.13,-16.00,85.01,65.28,-16.79,85.21,65.44,-17.59,85.50,65.60,-17.70,85.80,65.76,-17.67,86.93,66.35,-17.50,87.64,66.71,-17.18,89.36,67.54,-15.76,91.08,68.37,-14.34,92.80,69.20,-12.92,94.52,70.03,-11.50,96.24,70.86,-10.09,98.05,71.73,-8.59,99.89,72.62,-7.06,101.70,73.49,-5.55,103.48,74.35,-4.03,105.15,75.15,-2.59,106.30,75.69,-1.39,107.31,76.15,-0.05,108.15,76.51,1.39,108.86,76.79,3.00,109.37,76.97,4.65,109.69,77.04,6.33],
        [107.17,72.77,7.90,107.10,72.63,9.58,106.83,72.39,11.23,106.35,72.04,12.81,105.68,71.60,14.29,104.82,71.06,15.64,103.80,70.44,16.84,102.62,69.76,17.84,101.33,69.03,18.65,99.96,68.26,19.25,98.53,67.47,19.69,97.07,66.68,19.99,95.59,65.88,20.19,94.10,65.08,20.31,92.62,64.29,20.39,91.12,63.49,20.42,89.63,62.70,20.40,88.14,61.91,20.37,86.65,61.12,20.34,85.15,60.33,20.29,83.66,59.54,20.23,82.17,58.75,20.18,80.68,57.95,20.13,79.69,57.50,19.02,79.04,57.24,17.49,78.91,57.27,15.81,78.79,57.31,14.12,79.01,57.52,12.48,79.42,57.83,10.87,79.83,58.15,9.26,80.24,58.46,7.65,80.65,58.78,6.04,81.06,59.09,4.43,81.47,59.40,2.82,81.88,59.72,1.21,82.29,60.03,-0.39,82.70,60.35,-2.00,83.12,60.66,-3.61,83.53,60.97,-5.22,83.94,61.29,-6.83,84.37,61.61,-8.43,84.81,61.94,-10.03,85.27,62.28,-11.62,85.76,62.64,-13.20,86.30,63.02,-14.75,87.67,63.73,-14.42,88.94,64.35,-13.54,90.15,64.93,-12.50,91.35,65.51,-11.47,92.56,66.09,-10.43,93.76,66.66,-9.40,94.96,67.24,-8.36,96.17,67.82,-7.33,97.37,68.40,-6.29,98.58,68.98,-5.26,99.78,69.56,-4.22,100.98,70.13,-3.18,102.17,70.70,-2.13,103.34,71.26,-1.04,104.42,71.76,0.16,105.34,72.17,1.51,106.09,72.48,2.99,106.65,72.69,4.57,107.01,72.78,6.22],
        [102.89,67.67,7.68,102.86,67.57,9.03,102.65,67.38,10.36,102.29,67.12,11.63,101.78,66.77,12.84,101.11,66.35,13.95,100.31,65.87,14.93,99.39,65.33,15.75,98.37,64.75,16.43,97.28,64.13,16.95,96.15,63.51,17.36,94.99,62.88,17.68,93.82,62.24,17.94,92.64,61.60,18.14,91.46,60.96,18.31,90.27,60.32,18.46,89.08,59.68,18.58,87.89,59.04,18.68,86.70,58.40,18.75,85.50,57.77,18.80,84.31,57.13,18.82,83.11,56.49,18.84,81.91,55.85,18.83,80.83,55.30,18.49,80.20,55.03,17.32,80.04,55.03,15.98,79.96,55.07,14.63,80.27,55.31,13.33,80.60,55.56,12.04,80.93,55.81,10.75,81.26,56.06,9.46,81.59,56.31,8.17,81.92,56.57,6.88,82.25,56.82,5.59,82.58,57.07,4.30,82.91,57.32,3.01,83.25,57.58,1.72,83.61,57.85,0.44,84.00,58.13,-0.83,84.43,58.43,-2.07,84.88,58.75,-3.32,85.33,59.06,-4.56,85.77,59.37,-5.80,86.22,59.68,-7.04,86.67,59.99,-8.28,87.11,60.30,-9.52,88.27,60.91,-9.35,89.39,61.48,-8.88,90.37,61.94,-8.06,91.33,62.41,-7.23,92.30,62.87,-6.40,93.26,63.33,-5.56,94.22,63.80,-4.72,95.18,64.26,-3.88,96.13,64.71,-3.03,97.08,65.17,-2.18,98.03,65.62,-1.32,98.96,66.06,-0.44,99.88,66.50,0.45,100.72,66.89,1.44,101.45,67.21,2.54,102.03,67.45,3.74,102.47,67.61,5.01,102.76,67.68,6.33],
        [98.61,62.57,7.46,98.60,62.51,8.48,98.46,62.37,9.48,98.23,62.19,10.45,97.87,61.95,11.37,97.41,61.65,12.23,96.83,61.30,12.99,96.17,60.91,13.65,95.41,60.47,14.18,94.61,60.02,14.61,93.78,59.56,14.97,92.93,59.09,15.27,92.06,58.61,15.52,91.19,58.13,15.73,90.31,57.65,15.92,89.42,57.18,16.09,88.54,56.70,16.24,87.65,56.22,16.38,86.76,55.73,16.51,85.87,55.25,16.63,84.98,54.77,16.73,84.09,54.29,16.84,83.19,53.81,16.91,82.30,53.33,16.99,81.42,52.87,17.00,81.17,52.79,16.06,81.31,52.92,15.07,81.56,53.11,14.10,81.81,53.30,13.13,82.06,53.49,12.16,82.34,53.70,11.21,82.68,53.93,10.27,83.01,54.17,9.34,83.35,54.40,8.41,83.69,54.64,7.47,84.02,54.87,6.54,84.36,55.10,5.61,84.69,55.34,4.68,85.03,55.57,3.74,85.37,55.81,2.81,85.70,56.04,1.88,86.04,56.27,0.95,86.37,56.51,0.01,86.71,56.74,-0.92,87.05,56.98,-1.85,87.38,57.21,-2.79,87.72,57.44,-3.72,88.29,57.78,-4.24,89.18,58.24,-4.10,90.06,58.70,-3.86,90.82,59.07,-3.29,91.53,59.41,-2.65,92.24,59.75,-2.01,92.95,60.09,-1.36,93.66,60.42,-0.71,94.36,60.76,-0.05,95.06,61.09,0.61,95.75,61.42,1.28,96.44,61.74,1.96,97.04,62.02,2.74,97.55,62.24,3.59,97.98,62.41,4.49,98.30,62.53,5.45,98.50,62.58,6.45],
        [91.95,54.64,7.12,91.95,54.62,7.42,91.95,54.60,7.71,91.95,54.58,8.00,91.91,54.54,8.29,91.85,54.50,8.58,91.80,54.45,8.86,91.72,54.39,9.14,91.61,54.32,9.41,91.51,54.25,9.67,91.38,54.17,9.93,91.22,54.07,10.14,91.02,53.95,10.33,90.81,53.83,10.50,90.61,53.71,10.68,90.36,53.58,10.74,90.10,53.44,10.78,89.85,53.30,10.82,89.59,53.16,10.85,89.33,53.02,10.89,89.08,52.88,10.92,88.82,52.74,10.96,88.59,52.63,10.84,88.37,52.52,10.67,88.20,52.44,10.45,88.03,52.37,10.21,87.94,52.33,9.94,87.86,52.31,9.66,87.84,52.32,9.36,87.83,52.33,9.07,87.83,52.35,8.78,87.87,52.38,8.49,87.91,52.42,8.20,87.95,52.46,7.91,88.01,52.51,7.62,88.08,52.57,7.34,88.15,52.62,7.06,88.23,52.67,6.78,88.30,52.73,6.50,88.39,52.79,6.23,88.47,52.86,5.95,88.56,52.92,5.68,88.65,52.98,5.41,88.74,53.05,5.13,88.83,53.11,4.86,88.92,53.18,4.59,89.02,53.24,4.32,89.15,53.32,4.10,89.41,53.46,4.14,89.66,53.59,4.17,89.92,53.73,4.21,90.18,53.86,4.25,90.44,54.00,4.29,90.70,54.14,4.33,90.91,54.24,4.48,91.10,54.33,4.69,91.29,54.42,4.90,91.44,54.48,5.14,91.55,54.53,5.41,91.65,54.56,5.69,91.74,54.59,5.97,91.82,54.62,6.25,91.87,54.62,6.54,91.91,54.63,6.83]]
};
const POD_AXIS = {
    "12" : [[98.2136, 69.2996, 8.0000], [0.207627, -0.976807, -0.052336]],
    "20" : [[98.2136, 69.2996, 8.0000], [0.341551, -0.938405, -0.052336]],
    "28" : [[98.2136, 69.2996, 8.0000], [0.468828, -0.881738, -0.052336]]
};
// pod -> pad seats (generated, headrest_POD11.py): rim flare start over each seat floor (= lowest flank height there - 0.3)
const POD_SEAT_RIM = { "12" : [3.73, 1.37], "20" : [3.32, 1.28], "28" : [2.76, 1.14] };
// seat floor, local y along the screw axis (M3x20 tip 0.2 past the nut, 0.2 short of the head-side face; >= 3.5 mm plastic under the seat)
const POD_SEAT_Y = [106.20, 106.20];
