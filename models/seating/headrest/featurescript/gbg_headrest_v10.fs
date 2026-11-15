FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// ===================================================================================================================
// GoBabyGo HEADREST v1.0 (PRE-FIRST-PRINT) for GbG_chair_forth.  Native FeatureScript port of the reviewed Python model
// (go-baby-go-core: models/seating/headrest/generator).  Everything is B-rep: the curved pad is two B-spline surfaces
// (head side + back, from the same formulas as the Python) closed by its outline wall (opEnclose); the mast and the
// chair receiver are lofts of the same superellipse sections; every hole, slot, pocket and nut trap is a native cut.
// Only the button-pod outer hull is data (POD_SECTIONS: planar sections of the reviewed convex hull).
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
const BTN_ANG = 46;  const BTN_Z = -2;  const B_D = 8;
const POCKET_Y0 = 93.6;                                                        // pocket floor (8 mm deep)
const TRI = [[-15, 16], [15, 16], [0, -14]];                                    // (x, z) point down
const NUT_Y = 103.1;                                                           // pod / blank M3 nut (drop-in channel to the outside)
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
            const yb = min(cyBack + ELL * sqrt(max(TO * TO - x * x, 1)) - 0.4, y1);
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
        ycyl(context, sid + "b" + "h", 0, R - 1, POCKET_Y0 + 1, BTN_Z, M3_CLEAR / 2);
        fCone(context, sid + "b" + "c", { "bottomCenter" : v3(0, R - 0.05, BTN_Z), "topCenter" : v3(0, R + 1.75, BTN_Z), "bottomRadius" : 3.1 * millimeter, "topRadius" : 1.5 * millimeter });
        moveSolids(context, sid + "bpm", sid + "b", placeXf(s * BTN_ANG));
        // K2 lock screw: clearance + countersink at the chin
        const a = scrPt(s, -8);  const b = scrPt(s, T_NUT + 6);
        fCylinder(context, sid + "sc", { "bottomCenter" : a * millimeter, "topCenter" : b * millimeter, "radius" : M3_CLEAR / 2 * millimeter });
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
function m1Sections(H is number) returns array
{
    const cy = CY;  const zch = chinZ(H) - 0.3;  const zf = TOP + 0.3;  const gap = zch - zf;
    const ff = min(30, 0.42 * gap);  const cf = min(26, 0.45 * gap);
    const ybk = cy + ELL * TO - 0.4;
    var zs = concatenateArrays([linspace(zf, zf + ff, 9), linspace(zf + ff, zch - cf, max(2, floor((gap - ff - cf) / 12) + 2)), linspace(zch - cf, zch, 9)]);
    var secs = [];  var last = -1e9;
    for (var i = 0; i < size(zs); i += 1)
    {
        const z = zs[i];
        if (z - last < 2.5 && i != size(zs) - 1) continue;
        last = z;
        const tf = sm((z - zf) / max(ff, 1e-3));  const tc = sm((z - (zch - cf)) / max(cf, 1e-3));
        const mid = 1 - abs(((z - zf) / max(gap, 1)) - 0.5) * 2;
        const hwStem = 28 * lerp(1, 0.9, sm(mid));
        const hw = lerp(lerp(FOOT[0], hwStem, tf), 44, tc);
        const y0 = lerp(lerp(FOOT[1], 132, tf), cy + 103.2, tc);
        const y1 = lerp(lerp(FOOT[2], ybk, tf), ybk, tc);
        secs = append(secs, section(hw, y0, y1, lerp(lerp(FOOT[3], 2.6, tf), 3.0, tc), tf > 0.5 ? cy : undefined, z));
    }
    return secs;
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
    loftSections(context, id + "stem", m1Sections(H));
    rrbox(context, id + "plug", PLUG[0], PLUG[1], PLUG[2], PLUG[3], SOCK_Z0 + 0.3, TOP + 0.5, 6);
    opBoolean(context, id + "uni", { "tools" : qUnion([solids(context, hd), solids(context, id + "stem"), solids(context, id + "plug")]), "operationType" : BooleanOperationType.UNION });
    const C = id + "cut";
    const mast = qSubtraction(solids(context, id), solids(context, C));
    // ---- cuts ----
    for (var x in [-SPINE_X, SPINE_X])
        fCylinder(context, C + (x < 0 ? "rodL" : "rodR"), { "bottomCenter" : v3(x, CY + SPINE_Y, SOCK_Z0 - 0.5), "topCenter" : v3(x, CY + SPINE_Y, zc + ROD_TOP + 0.5), "radius" : ROD_HOLE_D / 2 * millimeter });
    for (var i = 0; i < 2; i += 1)
    {
        ycyl(context, C + ("bolt" ~ i), 0, PLUG[2] + 2, PLUG[2] + 62, BOLTS_Z[i], M5_CLEAR / 2);
        hexPrism(context, C + ("nut" ~ i), v3(0, NUT5_Y, BOLTS_Z[i]), vector(0, 1, 0), vector(-1, 0, 0), NUT_M5_AF, 4.3, 40);   // slides in from -x
    }
    for (var s in [1, -1])
    {
        const sid = C + (s > 0 ? "scR" : "scL");
        fCylinder(context, sid + "c", { "bottomCenter" : scrPt(s, 0) * millimeter, "topCenter" : scrPt(s, T_NUT + 8) * millimeter, "radius" : M3_CLEAR / 2 * millimeter });
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
export function buildPod(context is Context, id is Id, yaw is HeadrestYaw, s is number) returns Query
{
    const key = yawKey(yaw);
    const data = POD_SECTIONS[key];
    var secs = [];
    for (var sec in data)
    {
        var pts = [];
        for (var k = 0; k < size(sec); k += 3) pts = append(pts, [s * sec[k], sec[k + 1], sec[k + 2]]);
        secs = append(secs, pts);
    }
    loftSections(context, id + "hull", secs);
    trimBySurface(context, id + "trim", id, s, 0.3, true);          // back hugs the pad surface (0.3 gap)
    triPrism(context, id + "wg", -0.05, POCKET_Y0 + 0.2, R + T + 7.6 + 3.3);
    moveSolids(context, id + "wgm", id + "wg", placeXf(s * BTN_ANG));
    opBoolean(context, id + "uw", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const C = id + "cut";
    const pod = qSubtraction(solids(context, id), solids(context, C));
    const E = vector(s * POD_AXIS[key][0][0], POD_AXIS[key][0][1], POD_AXIS[key][0][2]);
    const d = vector(s * POD_AXIS[key][1][0], POD_AXIS[key][1][1], POD_AXIS[key][1][2]);
    fCylinder(context, C + "bore", { "bottomCenter" : (E - d * 26) * millimeter, "topCenter" : (E + d * 30) * millimeter, "radius" : PVC_BORE / 2 * millimeter });
    for (var k in [8, 18])
        fCylinder(context, C + ("pil" ~ k), { "bottomCenter" : (E - d * k) * millimeter, "topCenter" : (E - d * k + vector(0, 0, 20)) * millimeter, "radius" : 1.4 * millimeter });   // kit wood-screw pilots into the PVC
    nutAndScrew(context, C + "sn", s);
    opBoolean(context, id + "sub", { "tools" : solids(context, C), "targets" : pod, "operationType" : BooleanOperationType.SUBTRACTION });
    return solids(context, id);
}
function nutAndScrew(context is Context, id is Id, s is number)
{
    ycyl(context, id + "h", 0, R - 2, R + 38, BTN_Z, M3_CLEAR / 2);
    hexPrism(context, id + "n", v3(0, NUT_Y + 12.5, BTN_Z), vector(0, 1, 0), vector(0, 0, 1), NUT_M3_AF, 25, 0);
    moveSolids(context, id + "mv", id, placeXf(s * BTN_ANG));
}
export function buildBlank(context is Context, id is Id, s is number) returns Query
{
    triPrism(context, id + "tri", -0.1, POCKET_Y0 + 0.2, R + T + 7.6 + 7);
    moveSolids(context, id + "mv", id + "tri", placeXf(s * BTN_ANG));
    trimBySurface(context, id + "trim", id, s, 0, false);           // flush with the pad's back
    nutAndScrew(context, id + "cut", s);
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

annotation { "Feature Type Name" : "GbG Headrest v1.0", "Feature Type Description" : "Pad F3 + mast M1 + button pod W1 for GbG_chair_forth (needs the receiver in top_back: feature GbG Headrest Receiver)" }
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
        annotation { "Name" : "Self-test: build every height and pod angle, report volumes (slow)" }
        definition.selfTest is boolean;
    }
    {
        if (definition.selfTest)
        {
            for (var h in [0, 25, 50, 75, 100])
            {
                const m = buildMast(context, id + ("tm" ~ h), h);
                report(context, "mast H" ~ h ~ " (before the pad cut)", m);
                opDeleteBodies(context, id + ("tmd" ~ h), { "entities" : qCreatedBy(id + ("tm" ~ h), EntityType.BODY) });
            }
            for (var y in [HeadrestYaw.Y12, HeadrestYaw.Y20, HeadrestYaw.Y28])
            {
                const p = buildPod(context, id + ("tp" ~ yawKey(y)), y, 1);
                report(context, "pod yaw " ~ yawKey(y), p);
                opDeleteBodies(context, id + ("tpd" ~ yawKey(y)), { "entities" : qCreatedBy(id + ("tp" ~ yawKey(y)), EntityType.BODY) });
            }
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
            // WORLD -> print orientation on z = 0: pad upright (as worn), mast on its side (+x down: loads run along the layers),
            // pod with the PVC bore vertical, opening up (round bore = PVC fit; slicer tree supports), blank on its wedge end.
            const back = inverse(reclineXf());
            var i = 0;
            for (var k in keys(parts))
            {
                var xf = back;
                if (k == "mast") xf = rotationAround(line(v3(0, 0, 0), vector(0, 1, 0)), 90 * degree) * back;
                if (k == "podR" || k == "podL")
                {
                    const key = yawKey(definition.yaw);  const s = k == "podR" ? 1 : -1;
                    const d = vector(s * POD_AXIS[key][1][0], POD_AXIS[key][1][1], POD_AXIS[key][1][2]);
                    xf = transform(rotationMatrix3d(d, vector(0, 0, 1)), vector(0, 0, 0) * millimeter) * transform(v3(0, -CY, -451 - H)) * back;
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
        ycyl(context, C + ("b" ~ i), 0, PLUG[2] + 2, PLUG[2] + 62, BOLTS_Z[i], M5_CLEAR / 2);
        ycyl(context, C + ("cb" ~ i), 0, FOOT[2] - 3, FOOT[2] + 17, BOLTS_Z[i], 5);         // counterbore: M5 head flush with the back
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

// ---------------- DATA: W1 pod hull sections (PAD frame, right side; generated by tools/headrest_FS_export.py) ----------------
const POD_SECTIONS = {
    "12" : [
        [56.0,108.6,3.4,56.0,108.6,2.8,55.9,108.6,2.2,55.9,108.7,1.5,55.9,108.7,0.8,56.0,108.8,-0.1,56.2,108.9,-1.2,56.6,109.0,-2.6,57.3,109.3,-4.4,58.7,109.6,-4.0,59.7,109.7,-2.0,60.2,109.7,-0.7,60.4,109.7,0.3,60.5,109.7,1.0,60.7,109.7,1.5,60.7,109.7,1.9,60.7,109.6,2.3,60.7,109.6,2.7,60.7,109.6,3.1,60.7,109.6,3.5,60.8,109.6,4.0,60.7,109.5,4.5,60.5,109.5,5.2,60.3,109.4,6.1,59.8,109.2,7.2,59.0,108.9,8.6,57.9,108.7,8.2,57.3,108.7,6.9,56.8,108.6,6.0,56.5,108.6,5.2,56.3,108.6,4.5,56.2,108.6,3.9],
        [57.3,104.0,6.4,57.1,104.1,4.5,57.0,104.1,2.5,56.9,104.2,0.5,57.1,104.4,-1.9,57.3,104.6,-4.7,57.9,104.9,-8.2,60.0,105.5,-10.7,63.2,106.2,-10.6,66.2,106.7,-7.9,68.2,107.0,-5.8,69.7,107.2,-4.0,70.7,107.3,-2.3,71.5,107.4,-0.9,72.0,107.5,0.5,72.4,107.5,1.8,72.7,107.5,3.0,72.8,107.4,4.3,72.8,107.3,5.5,72.6,107.2,6.8,72.3,107.1,8.2,71.8,106.9,9.5,71.0,106.7,10.9,69.9,106.4,12.1,68.4,106.0,13.1,66.7,105.6,13.8,64.8,105.2,14.2,62.6,104.7,14.6,60.6,104.3,13.8,58.9,104.0,12.5,58.1,104.0,10.3,57.6,104.0,8.3],
        [59.6,99.7,8.8,59.2,99.8,5.6,59.1,99.9,2.4,59.2,100.1,-1.0,59.7,100.4,-4.7,60.6,100.8,-8.9,62.0,101.4,-13.8,66.6,102.4,-14.4,71.5,103.3,-11.1,74.8,103.8,-8.8,77.2,104.3,-6.8,79.1,104.6,-5.1,80.7,104.8,-3.4,82.0,105.0,-1.8,83.0,105.1,-0.2,83.8,105.2,1.6,84.4,105.2,3.4,84.8,105.2,5.3,84.9,105.1,7.2,84.6,105.0,9.2,84.0,104.7,11.2,83.0,104.4,13.1,81.6,104.0,14.8,79.8,103.6,16.1,77.7,103.1,17.0,75.4,102.6,17.5,73.1,102.0,17.8,70.5,101.5,17.8,67.5,100.9,17.6,64.1,100.2,17.1,61.3,99.7,15.2,60.3,99.6,12.0],
        [62.1,95.3,12.2,61.5,95.4,7.8,61.2,95.6,3.2,61.6,96.0,-1.6,62.6,96.4,-6.5,64.1,97.1,-12.0,67.4,98.0,-16.7,74.8,99.4,-13.6,79.9,100.3,-10.5,83.5,101.0,-8.4,86.3,101.5,-6.6,88.6,101.9,-5.0,90.7,102.2,-3.4,92.6,102.5,-1.7,94.2,102.8,0.2,95.5,102.9,2.4,96.4,103.0,4.7,96.9,103.0,7.3,96.9,102.8,9.9,96.3,102.6,12.5,95.2,102.2,14.9,93.6,101.8,17.1,91.6,101.2,18.8,89.3,100.7,20.0,86.8,100.1,20.7,84.2,99.6,20.9,81.6,99.0,20.8,78.9,98.4,20.6,75.9,97.8,20.2,72.3,97.1,19.7,67.6,96.1,19.0,63.4,95.4,16.5],
        [64.6,91.0,15.3,63.7,91.1,9.7,63.4,91.3,3.9,64.1,91.8,-2.1,65.7,92.5,-8.1,67.8,93.3,-14.6,74.3,94.8,-16.6,83.1,96.4,-11.6,88.1,97.3,-8.7,91.7,97.9,-6.6,94.5,98.4,-4.9,96.9,98.9,-3.4,99.1,99.3,-2.0,101.2,99.6,-0.4,102.9,99.9,1.6,104.2,100.0,3.9,105.1,100.1,6.4,105.5,100.0,9.1,105.3,99.8,11.8,104.6,99.6,14.4,103.5,99.2,16.8,101.8,98.7,18.9,99.8,98.2,20.6,97.5,97.6,21.9,95.0,97.1,22.7,92.4,96.5,23.0,89.8,95.9,22.9,87.1,95.4,22.5,84.1,94.8,22.1,80.5,94.0,21.5,75.8,93.1,20.8,68.5,91.6,19.7],
        [66.3,86.7,15.0,65.7,86.8,9.6,65.6,87.1,4.2,66.5,87.6,-1.3,68.1,88.2,-6.9,70.1,89.0,-12.9,74.0,90.1,-18.0,82.9,91.7,-13.3,88.5,92.7,-9.8,92.3,93.4,-7.3,95.2,93.9,-5.4,97.6,94.3,-3.8,99.9,94.7,-2.3,102.1,95.1,-0.7,103.9,95.4,1.4,105.2,95.5,3.8,106.1,95.6,6.4,106.4,95.5,9.1,106.2,95.3,11.8,105.5,95.0,14.5,104.2,94.6,16.9,102.5,94.2,19.0,100.4,93.6,20.6,98.1,93.1,21.8,95.6,92.5,22.5,93.0,91.9,22.7,90.4,91.4,22.8,87.6,90.8,22.7,84.5,90.1,22.3,80.9,89.4,21.8,76.0,88.4,21.1,68.6,86.9,19.9],
        [68.4,82.4,15.9,67.9,82.5,10.7,67.8,82.8,5.4,68.7,83.3,0.1,70.3,83.9,-5.3,72.4,84.7,-11.0,75.0,85.6,-17.5,83.6,87.2,-13.8,89.6,88.3,-9.9,93.6,89.0,-7.3,96.7,89.5,-5.3,99.3,90.0,-3.6,101.7,90.4,-2.0,103.8,90.8,-0.1,105.5,91.0,2.2,106.7,91.1,4.7,107.3,91.1,7.4,107.4,91.0,10.1,106.9,90.7,12.7,106.0,90.4,15.2,104.7,90.0,17.3,103.0,89.6,19.1,101.1,89.1,20.6,98.9,88.6,21.6,96.7,88.0,22.3,94.3,87.5,22.5,91.9,87.0,22.5,89.4,86.5,22.6,86.3,85.8,22.6,82.8,85.1,22.3,78.1,84.1,21.6,70.9,82.7,20.5],
        [70.3,78.2,14.2,69.8,78.3,9.3,70.3,78.7,4.5,71.5,79.2,-0.2,73.1,79.8,-4.9,75.0,80.5,-9.9,77.4,81.3,-15.6,84.1,82.6,-14.3,89.9,83.7,-10.6,93.8,84.4,-8.0,96.8,84.9,-6.1,99.3,85.3,-4.4,101.6,85.8,-2.9,103.8,86.1,-1.3,105.7,86.4,0.8,107.1,86.6,3.3,108.0,86.6,6.0,108.3,86.6,8.8,108.1,86.3,11.6,107.2,86.0,14.3,105.9,85.6,16.7,104.1,85.1,18.8,102.0,84.6,20.4,99.6,84.0,21.5,97.1,83.4,22.1,94.5,82.9,22.3,91.9,82.3,22.3,89.1,81.7,22.4,85.8,81.0,22.4,81.6,80.2,22.3,76.4,79.1,21.6,70.8,78.0,19.3],
        [72.1,73.9,12.8,72.1,74.2,7.8,73.2,74.7,3.1,74.7,75.2,-1.3,76.3,75.8,-5.6,78.3,76.5,-10.2,81.3,77.4,-14.4,87.7,78.6,-12.5,92.5,79.5,-9.7,96.0,80.1,-7.5,98.6,80.6,-5.8,100.8,81.0,-4.3,102.9,81.3,-3.0,104.8,81.7,-1.5,106.5,81.9,0.3,107.8,82.1,2.4,108.7,82.2,4.8,109.2,82.1,7.3,109.2,82.0,9.9,108.7,81.7,12.5,107.7,81.4,15.0,106.2,81.0,17.3,104.3,80.4,19.2,102.0,79.9,20.7,99.4,79.3,21.6,96.7,78.7,22.0,93.8,78.1,22.1,90.8,77.4,22.1,87.1,76.6,22.2,82.4,75.6,22.3,76.2,74.3,21.7,72.7,73.8,17.9],
        [74.1,69.7,12.4,74.6,70.0,7.7,75.9,70.6,3.3,77.4,71.1,-0.7,79.1,71.7,-4.6,81.0,72.3,-8.7,84.5,73.2,-11.4,89.6,74.2,-10.4,93.7,75.0,-9.3,96.9,75.6,-7.7,99.6,76.1,-6.0,101.8,76.5,-4.5,103.9,76.9,-3.2,105.8,77.2,-1.7,107.5,77.4,0.2,108.8,77.6,2.3,109.7,77.7,4.7,110.2,77.6,7.2,110.2,77.5,9.8,109.7,77.2,12.4,108.7,76.9,14.8,107.2,76.5,17.1,105.2,76.0,19.0,102.9,75.4,20.4,100.4,74.8,21.4,97.6,74.2,21.8,94.8,73.6,21.8,91.8,72.9,21.9,88.2,72.2,21.9,83.4,71.2,22.0,76.7,69.7,21.9,74.6,69.5,17.3],
        [76.2,65.4,12.9,77.1,65.9,8.0,78.7,66.4,3.7,80.4,67.0,-0.3,82.3,67.6,-4.0,84.3,68.2,-7.8,89.6,69.4,-7.7,93.8,70.2,-7.0,97.0,70.9,-6.5,99.8,71.4,-5.9,102.3,71.9,-4.9,104.5,72.3,-3.7,106.4,72.6,-2.3,108.0,72.9,-0.6,109.3,73.1,1.2,110.3,73.2,3.3,110.9,73.2,5.4,111.2,73.1,7.6,111.1,73.0,9.8,110.7,72.8,12.0,109.8,72.5,14.1,108.7,72.1,16.1,107.1,71.7,17.9,105.2,71.2,19.4,102.9,70.7,20.6,100.4,70.1,21.3,97.8,69.5,21.5,94.9,68.9,21.6,91.4,68.2,21.7,86.9,67.2,21.8,80.1,65.8,21.9,76.7,65.3,18.0],
        [78.5,61.2,12.6,79.6,61.6,9.1,80.9,62.1,6.1,82.1,62.5,3.4,83.4,62.9,0.9,84.8,63.3,-1.7,86.3,63.8,-4.6,89.6,64.5,-4.9,92.6,65.1,-4.4,95.2,65.7,-3.9,97.6,66.1,-3.4,99.9,66.6,-2.5,101.9,66.9,-1.3,103.6,67.2,0.4,104.9,67.4,2.2,105.8,67.5,4.3,106.3,67.5,6.5,106.4,67.4,8.7,106.0,67.2,10.8,105.3,66.9,12.8,104.3,66.6,14.6,102.9,66.2,16.1,101.4,65.8,17.4,99.6,65.4,18.4,97.8,65.0,19.2,95.8,64.5,19.8,93.6,64.0,20.3,91.1,63.5,20.5,88.1,62.8,20.8,84.1,62.0,21.0,79.6,61.1,20.1,78.6,61.0,16.3],
        [81.7,57.3,11.5,82.7,57.6,9.5,83.5,57.8,7.9,84.3,58.1,6.4,85.0,58.3,5.0,85.7,58.5,3.6,86.6,58.8,2.0,87.6,59.1,0.0,89.1,59.6,-2.1,91.4,60.0,-1.6,93.4,60.4,-1.0,95.2,60.8,-0.2,96.9,61.1,1.0,98.2,61.2,2.5,99.1,61.4,4.1,99.7,61.4,5.8,99.9,61.3,7.6,99.8,61.2,9.3,99.4,61.1,10.9,98.7,60.8,12.2,97.8,60.6,13.4,96.7,60.3,14.4,95.6,60.0,15.1,94.4,59.7,15.8,93.2,59.4,16.4,91.8,59.1,17.0,90.2,58.7,17.6,88.3,58.3,18.2,85.8,57.7,18.8,82.3,57.0,19.2,80.7,56.8,17.1,80.7,56.9,14.2],
        [86.1,53.6,10.0,86.6,53.7,9.0,87.1,53.9,8.2,87.4,54.0,7.5,87.8,54.1,6.8,88.1,54.2,6.2,88.5,54.4,5.4,89.0,54.5,4.4,89.7,54.7,3.1,90.9,55.0,3.0,91.9,55.1,3.8,92.7,55.3,4.5,93.1,55.3,5.6,93.4,55.3,6.5,93.4,55.3,7.3,93.5,55.3,7.9,93.4,55.2,8.5,93.3,55.2,8.9,93.2,55.1,9.4,93.1,55.1,9.8,92.8,55.0,10.2,92.6,54.9,10.5,92.3,54.9,10.9,92.0,54.8,11.3,91.6,54.7,11.7,91.1,54.5,12.0,90.4,54.4,12.4,89.5,54.1,13.0,88.0,53.8,13.8,85.2,53.1,15.1,84.1,53.0,13.9,85.4,53.3,11.5]],
    "20" : [
        [56.0,108.4,3.8,55.9,108.4,3.3,55.9,108.4,2.8,55.8,108.4,2.3,55.8,108.4,1.7,55.7,108.4,1.0,55.9,108.5,0.2,56.0,108.7,-0.9,56.4,108.9,-2.3,57.2,109.2,-4.1,58.4,109.7,-3.6,59.2,109.8,-1.2,59.5,109.8,0.2,59.6,109.8,1.0,59.7,109.8,1.5,59.7,109.8,2.0,59.7,109.8,2.4,59.7,109.8,2.7,59.7,109.8,3.0,59.7,109.8,3.3,59.7,109.8,3.6,59.7,109.7,4.0,59.7,109.7,4.5,59.6,109.6,5.1,59.4,109.5,6.1,59.0,109.3,7.3,58.2,108.9,8.4,57.4,108.7,7.2,56.9,108.6,6.3,56.6,108.5,5.5,56.4,108.5,4.9,56.2,108.4,4.3],
        [57.5,103.7,6.3,57.3,103.7,4.8,57.2,103.8,3.2,57.1,103.8,1.7,57.1,103.9,-0.0,57.2,104.1,-2.0,57.5,104.3,-4.4,57.9,104.6,-7.6,59.5,105.4,-10.3,62.2,106.4,-10.8,64.8,107.1,-7.4,66.3,107.5,-4.9,67.3,107.8,-3.0,67.9,107.9,-1.5,68.3,108.0,-0.2,68.6,108.1,0.9,68.8,108.1,1.9,69.0,108.1,2.9,69.0,108.1,3.8,69.1,108.0,4.8,69.0,107.9,5.8,68.9,107.8,6.9,68.6,107.6,8.2,68.1,107.4,9.6,67.2,107.0,11.1,66.0,106.4,12.5,64.3,105.8,13.5,62.3,105.0,14.2,60.3,104.3,13.5,58.7,103.8,12.2,58.1,103.7,9.9,57.7,103.7,8.0],
        [59.8,99.3,9.3,59.5,99.4,6.8,59.3,99.4,4.4,59.3,99.5,1.9,59.4,99.7,-0.8,59.8,100.0,-3.8,60.4,100.4,-7.2,61.3,101.0,-11.5,64.0,102.2,-14.5,68.3,103.6,-12.0,71.3,104.5,-8.8,73.4,105.1,-6.4,74.9,105.6,-4.3,76.0,105.9,-2.5,76.8,106.1,-0.9,77.4,106.2,0.7,77.9,106.3,2.2,78.2,106.3,3.7,78.4,106.3,5.2,78.4,106.2,6.8,78.2,106.1,8.4,77.8,105.8,10.1,77.1,105.5,11.9,76.1,105.0,13.5,74.6,104.4,14.9,72.8,103.7,16.0,70.7,102.9,16.7,68.4,102.0,17.1,65.8,101.1,17.2,63.1,100.1,16.7,61.1,99.5,14.7,60.3,99.4,11.9],
        [62.3,95.0,12.7,61.8,95.0,9.3,61.5,95.0,6.0,61.5,95.2,2.5,61.8,95.5,-1.1,62.4,96.0,-5.0,63.5,96.6,-9.4,65.0,97.5,-14.8,69.5,99.2,-16.0,74.7,100.8,-11.8,78.1,101.9,-8.9,80.5,102.7,-6.6,82.5,103.3,-4.6,84.0,103.7,-2.7,85.3,104.1,-0.8,86.3,104.3,1.1,87.0,104.5,3.1,87.5,104.6,5.1,87.7,104.5,7.2,87.6,104.4,9.3,87.2,104.1,11.4,86.5,103.7,13.5,85.3,103.2,15.4,83.7,102.5,17.0,81.8,101.8,18.3,79.7,100.9,19.1,77.4,100.1,19.6,74.9,99.2,19.7,72.3,98.2,19.5,69.3,97.1,19.2,65.7,95.9,18.6,63.3,95.1,16.1],
        [65.0,90.7,16.3,64.2,90.6,11.8,63.7,90.7,7.3,63.7,91.0,2.7,64.3,91.4,-2.0,65.5,92.1,-7.0,67.1,93.0,-12.5,70.2,94.4,-17.6,77.7,96.9,-13.3,82.6,98.5,-9.6,85.8,99.6,-7.1,88.4,100.4,-5.2,90.5,101.0,-3.5,92.3,101.6,-1.8,93.9,102.1,0.0,95.2,102.4,2.0,96.2,102.7,4.2,96.8,102.8,6.5,97.1,102.7,8.9,96.9,102.6,11.3,96.4,102.2,13.7,95.4,101.7,15.9,93.9,101.1,17.9,92.1,100.4,19.6,90.0,99.5,20.9,87.7,98.6,21.8,85.2,97.7,22.1,82.6,96.7,22.0,79.9,95.8,21.6,76.9,94.7,21.1,73.2,93.4,20.5,68.2,91.6,19.6],
        [67.4,86.3,18.7,66.4,86.2,13.9,65.9,86.3,9.0,65.9,86.6,3.9,66.7,87.2,-1.2,68.1,88.0,-6.5,69.9,89.0,-12.3,73.1,90.4,-18.0,81.3,93.1,-13.1,86.5,94.8,-9.1,90.0,96.0,-6.4,92.7,96.8,-4.3,94.9,97.5,-2.5,97.0,98.2,-0.9,98.7,98.7,1.0,100.1,99.1,3.2,101.1,99.3,5.6,101.6,99.4,8.1,101.6,99.2,10.6,101.2,99.0,13.1,100.4,98.5,15.4,99.3,98.0,17.5,97.8,97.3,19.3,95.9,96.6,20.8,93.9,95.8,21.9,91.7,94.9,22.6,89.3,94.1,22.9,86.9,93.2,22.9,84.3,92.3,22.7,81.5,91.3,22.2,78.0,90.0,21.6,73.3,88.3,20.8],
        [69.2,81.9,19.2,68.5,81.9,14.4,68.0,82.0,9.6,68.1,82.3,4.7,69.0,82.9,-0.2,70.5,83.7,-5.2,72.3,84.7,-10.8,74.7,85.9,-17.3,82.3,88.5,-13.9,87.9,90.3,-9.6,91.6,91.5,-6.7,94.4,92.4,-4.5,96.7,93.1,-2.6,98.9,93.8,-0.9,100.7,94.3,1.2,102.0,94.7,3.6,102.9,94.9,6.1,103.3,94.9,8.7,103.2,94.7,11.2,102.6,94.4,13.6,101.7,93.9,15.9,100.5,93.3,17.8,98.9,92.7,19.5,97.1,92.0,20.8,95.2,91.2,21.8,93.1,90.4,22.4,90.9,89.6,22.6,88.6,88.7,22.7,86.1,87.8,22.7,83.1,86.8,22.6,79.6,85.5,22.1,74.9,83.8,21.3],
        [70.9,77.5,17.5,70.4,77.6,12.8,70.1,77.8,8.3,70.7,78.2,3.8,71.8,78.9,-0.6,73.3,79.6,-5.1,75.0,80.5,-10.0,77.2,81.7,-15.8,83.3,83.8,-14.6,88.7,85.6,-10.4,92.3,86.7,-7.6,95.1,87.6,-5.4,97.4,88.3,-3.6,99.5,89.0,-2.0,101.4,89.6,-0.1,103.0,90.1,2.1,104.1,90.3,4.5,104.8,90.4,7.2,104.9,90.3,9.9,104.5,90.0,12.5,103.6,89.6,15.0,102.3,89.0,17.3,100.6,88.3,19.2,98.6,87.4,20.7,96.4,86.6,21.7,94.1,85.7,22.3,91.6,84.8,22.4,89.1,83.9,22.4,86.4,82.9,22.5,83.0,81.7,22.5,79.0,80.2,22.1,73.6,78.3,21.2],
        [72.8,73.2,15.7,72.3,73.3,11.0,72.7,73.7,6.4,73.8,74.4,2.1,75.1,75.1,-2.0,76.6,75.8,-6.2,78.4,76.7,-10.7,81.3,78.0,-14.6,87.0,80.0,-12.9,91.5,81.5,-9.8,94.6,82.5,-7.3,97.0,83.2,-5.5,99.0,83.9,-3.9,100.9,84.5,-2.4,102.6,85.0,-0.9,104.1,85.5,0.9,105.3,85.8,3.1,106.1,85.9,5.4,106.5,85.9,7.9,106.4,85.8,10.5,105.9,85.4,13.1,104.9,84.9,15.5,103.4,84.3,17.7,101.5,83.5,19.6,99.2,82.5,21.0,96.6,81.6,21.9,93.9,80.6,22.2,91.1,79.5,22.2,88.0,78.4,22.2,84.3,77.0,22.3,79.3,75.2,22.2,73.4,73.1,21.0],
        [74.8,68.9,15.3,74.5,69.1,10.6,75.4,69.6,6.2,76.6,70.3,2.2,78.0,71.0,-1.6,79.4,71.8,-5.4,81.2,72.6,-9.4,84.6,74.0,-11.8,89.2,75.6,-11.0,93.0,76.9,-9.7,96.0,77.9,-7.7,98.4,78.7,-5.8,100.5,79.3,-4.3,102.3,79.9,-2.8,104.1,80.5,-1.3,105.7,81.0,0.6,106.9,81.3,2.8,107.7,81.5,5.1,108.1,81.5,7.7,108.1,81.3,10.3,107.5,81.0,12.9,106.5,80.4,15.4,104.9,79.8,17.6,103.0,78.9,19.4,100.6,78.0,20.8,98.1,77.0,21.7,95.4,76.0,21.9,92.5,75.0,21.9,89.4,73.9,22.0,85.7,72.5,22.0,80.6,70.6,22.1,75.4,68.9,20.3],
        [76.9,64.6,16.1,76.8,64.8,11.1,78.0,65.5,6.6,79.5,66.3,2.5,81.0,67.0,-1.2,82.7,67.8,-4.9,84.6,68.7,-8.7,89.5,70.5,-8.3,93.3,71.9,-8.0,96.4,73.0,-7.4,99.2,73.9,-6.4,101.5,74.7,-5.0,103.5,75.3,-3.4,105.2,75.9,-2.0,106.8,76.4,-0.3,108.0,76.7,1.5,108.9,76.9,3.6,109.5,77.0,5.8,109.8,77.0,8.0,109.7,76.8,10.3,109.2,76.5,12.6,108.3,76.1,14.8,107.0,75.5,16.8,105.4,74.8,18.6,103.4,74.0,20.0,101.1,73.1,21.1,98.5,72.1,21.6,95.8,71.1,21.7,92.9,70.1,21.7,89.3,68.8,21.8,84.4,67.0,21.8,78.1,64.7,21.1],
        [78.9,60.2,15.7,79.3,60.6,11.5,80.5,61.3,7.9,81.8,61.9,4.7,83.2,62.6,1.8,84.5,63.2,-1.1,85.9,63.9,-4.1,88.8,65.0,-5.4,92.1,66.2,-5.1,94.9,67.2,-4.8,97.4,68.1,-4.6,99.8,68.9,-4.1,102.0,69.7,-3.2,104.1,70.4,-1.8,105.8,70.9,-0.0,107.1,71.3,2.1,108.0,71.5,4.3,108.5,71.5,6.7,108.6,71.4,9.2,108.2,71.1,11.5,107.4,70.7,13.7,106.2,70.2,15.6,104.7,69.5,17.3,103.0,68.8,18.6,101.0,68.1,19.6,99.0,67.3,20.3,96.8,66.5,20.7,94.5,65.6,20.9,91.9,64.7,20.9,88.8,63.5,20.9,84.6,62.0,20.9,79.9,60.4,19.8],
        [81.5,56.2,13.4,82.6,56.8,10.7,83.6,57.3,8.6,84.4,57.6,6.9,85.1,58.0,5.3,85.8,58.4,3.7,86.6,58.7,2.1,87.5,59.2,0.1,88.7,59.7,-2.2,90.9,60.5,-2.1,93.0,61.3,-1.7,94.9,61.9,-1.2,96.7,62.5,-0.4,98.3,63.1,0.8,99.6,63.4,2.2,100.5,63.7,4.0,101.1,63.8,5.8,101.3,63.8,7.6,101.2,63.6,9.4,100.8,63.4,11.1,100.1,63.0,12.6,99.1,62.6,13.9,97.9,62.1,14.9,96.7,61.6,15.7,95.4,61.1,16.3,94.0,60.6,16.8,92.6,60.0,17.3,90.9,59.4,17.7,88.9,58.7,18.1,86.4,57.7,18.5,82.9,56.5,18.8,81.1,55.9,16.7],
        [86.4,53.1,10.7,87.0,53.4,9.3,87.4,53.6,8.2,87.8,53.8,7.3,88.2,54.0,6.5,88.5,54.2,5.7,88.9,54.3,4.8,89.4,54.6,3.8,90.0,54.9,2.6,91.2,55.3,2.8,92.2,55.6,3.3,93.0,55.9,4.0,93.5,56.0,5.0,93.9,56.1,5.8,94.0,56.1,6.5,94.1,56.1,7.1,94.1,56.1,7.6,94.1,56.1,8.1,94.0,56.0,8.5,93.9,55.9,8.9,93.8,55.9,9.3,93.7,55.8,9.7,93.6,55.7,10.1,93.3,55.6,10.5,93.0,55.5,10.9,92.6,55.3,11.3,92.0,55.1,11.8,91.3,54.8,12.2,90.3,54.4,12.6,88.9,53.9,13.1,86.6,53.0,13.9,85.8,52.8,12.6]],
    "28" : [
        [56.0,108.1,4.1,55.8,108.1,3.7,55.7,108.0,3.2,55.7,108.0,2.7,55.6,108.0,2.2,55.6,108.1,1.5,55.6,108.1,0.8,55.7,108.2,-0.0,55.9,108.4,-1.1,56.4,108.7,-2.4,57.2,109.2,-4.1,58.3,109.8,-2.9,58.8,109.9,-0.6,59.0,109.9,0.6,59.1,109.9,1.3,59.1,109.9,1.8,59.1,109.9,2.2,59.1,109.9,2.5,59.1,109.9,2.8,59.1,109.8,3.1,59.1,109.8,3.3,59.1,109.8,3.7,59.1,109.8,4.0,59.1,109.7,4.5,59.0,109.7,5.1,58.9,109.5,6.2,58.5,109.2,7.9,57.6,108.8,7.6,57.0,108.5,6.6,56.6,108.3,5.9,56.3,108.2,5.2,56.1,108.2,4.6],
        [57.7,103.4,7.0,57.5,103.4,5.6,57.4,103.4,4.2,57.3,103.4,2.9,57.3,103.5,1.6,57.3,103.6,0.0,57.4,103.7,-1.7,57.6,104.0,-4.0,58.0,104.4,-7.0,59.2,105.2,-10.2,61.7,106.5,-10.7,63.9,107.5,-6.6,65.0,107.9,-3.8,65.7,108.1,-1.9,66.0,108.2,-0.4,66.3,108.3,0.7,66.4,108.3,1.6,66.5,108.3,2.5,66.6,108.3,3.2,66.6,108.3,4.0,66.6,108.2,4.8,66.6,108.2,5.6,66.5,108.1,6.6,66.3,107.9,7.8,66.0,107.6,9.2,65.3,107.2,10.8,64.2,106.5,12.3,62.6,105.6,13.6,60.7,104.6,13.7,59.1,103.8,12.8,58.4,103.5,10.5,57.9,103.4,8.6],
        [60.1,99.0,10.3,59.8,99.0,8.1,59.7,99.0,6.1,59.5,99.0,4.1,59.5,99.1,2.0,59.6,99.3,-0.2,59.8,99.6,-2.8,60.3,100.0,-5.9,61.1,100.7,-10.0,62.9,101.9,-14.1,66.6,103.8,-12.2,69.4,105.0,-8.2,71.1,105.7,-5.3,72.2,106.2,-3.0,72.9,106.4,-1.1,73.4,106.6,0.5,73.7,106.7,2.0,73.9,106.7,3.3,74.1,106.7,4.5,74.1,106.7,5.8,74.1,106.6,7.0,74.0,106.4,8.4,73.7,106.2,9.8,73.2,105.9,11.4,72.4,105.3,12.9,71.2,104.6,14.3,69.7,103.7,15.6,67.8,102.7,16.5,65.6,101.5,17.0,63.3,100.3,16.7,61.4,99.4,15.2,60.6,99.1,12.6],
        [62.6,94.6,13.2,62.1,94.6,10.3,61.9,94.6,7.5,61.7,94.6,4.8,61.7,94.8,2.0,62.0,95.1,-1.0,62.5,95.6,-4.3,63.3,96.3,-8.2,64.5,97.2,-13.1,67.4,99.0,-16.5,72.0,101.1,-12.3,75.0,102.5,-8.7,77.0,103.4,-5.9,78.4,104.1,-3.6,79.5,104.5,-1.6,80.3,104.8,0.3,80.9,105.0,2.0,81.3,105.1,3.7,81.5,105.2,5.4,81.6,105.1,7.1,81.6,105.0,8.8,81.3,104.7,10.6,80.8,104.4,12.4,80.0,103.8,14.2,78.8,103.1,15.8,77.2,102.2,17.2,75.3,101.1,18.3,73.2,99.9,18.9,70.8,98.7,19.2,68.3,97.3,19.0,65.3,95.8,18.5,63.4,94.9,16.2],
        [65.2,90.3,16.7,64.5,90.2,13.0,64.1,90.2,9.4,63.9,90.3,5.9,64.0,90.5,2.2,64.5,91.0,-1.6,65.3,91.7,-5.7,66.5,92.6,-10.4,68.2,93.9,-16.2,73.2,96.5,-15.7,78.0,98.7,-10.8,81.0,100.2,-7.6,83.2,101.2,-5.2,84.9,101.9,-3.1,86.2,102.5,-1.1,87.3,103.0,0.8,88.1,103.3,2.8,88.7,103.5,4.7,89.0,103.6,6.7,89.1,103.5,8.8,89.0,103.3,10.8,88.6,103.0,12.8,87.8,102.5,14.8,86.7,101.8,16.7,85.3,100.9,18.3,83.5,99.9,19.7,81.4,98.7,20.6,79.1,97.5,21.1,76.7,96.2,21.1,74.1,94.8,20.8,71.2,93.3,20.3,67.5,91.4,19.5],
        [70.5,87.4,20.3,67.1,85.8,17.3,66.5,85.8,12.9,66.1,85.8,8.5,66.2,86.1,3.9,66.9,86.7,-0.8,68.0,87.7,-5.8,69.6,88.8,-11.5,72.0,90.5,-18.0,79.2,94.1,-13.1,84.1,96.4,-8.3,87.2,97.8,-5.3,89.6,98.9,-2.9,91.5,99.9,-1.0,93.2,100.7,0.8,94.5,101.2,2.9,95.5,101.6,5.1,96.0,101.8,7.4,96.2,101.8,9.7,96.1,101.6,11.9,95.7,101.2,14.0,94.9,100.7,16.0,93.9,100.0,17.8,92.7,99.3,19.4,91.2,98.4,20.8,89.4,97.4,21.9,87.5,96.3,22.6,85.4,95.2,23.0,83.2,94.0,23.0,80.9,92.8,22.5,78.3,91.5,22.0,75.0,89.8,21.3],
        [71.2,82.3,20.6,69.0,81.4,17.0,68.6,81.4,12.6,68.2,81.5,8.3,68.5,81.9,3.9,69.4,82.6,-0.5,70.6,83.5,-5.2,72.1,84.6,-10.4,74.2,86.1,-16.8,80.5,89.3,-14.5,85.6,91.7,-9.4,88.8,93.3,-6.2,91.3,94.4,-3.8,93.3,95.4,-1.8,95.1,96.2,0.1,96.5,96.9,2.2,97.6,97.3,4.5,98.3,97.5,6.9,98.5,97.5,9.3,98.4,97.3,11.7,97.9,96.9,14.0,97.1,96.3,16.1,95.9,95.6,18.0,94.5,94.8,19.6,92.9,93.8,20.9,91.1,92.8,21.9,89.1,91.7,22.6,87.0,90.6,22.8,84.7,89.4,22.8,82.2,88.0,22.8,79.4,86.6,22.3,76.0,84.8,21.7],
        [71.5,77.0,20.7,71.0,77.0,16.1,70.6,77.0,11.9,70.5,77.2,7.6,71.1,77.8,3.5,72.1,78.5,-0.6,73.3,79.4,-4.9,74.8,80.5,-9.7,76.8,82.0,-15.6,82.2,84.8,-15.1,87.2,87.2,-10.3,90.5,88.7,-7.0,92.9,89.9,-4.6,95.0,90.8,-2.6,96.8,91.7,-0.7,98.4,92.4,1.3,99.6,92.9,3.6,100.4,93.2,6.0,100.8,93.2,8.6,100.7,93.1,11.1,100.2,92.7,13.5,99.4,92.1,15.8,98.2,91.3,17.8,96.6,90.4,19.5,94.9,89.4,20.9,92.9,88.3,21.8,90.8,87.1,22.4,88.6,85.9,22.5,86.3,84.7,22.5,83.7,83.3,22.6,80.4,81.6,22.5,76.6,79.6,21.9],
        [73.3,72.6,18.3,72.8,72.6,13.5,72.7,72.8,9.1,73.4,73.4,4.9,74.4,74.2,1.1,75.5,75.0,-2.7,76.8,75.9,-6.7,78.3,77.0,-11.0,80.9,78.6,-15.0,85.9,81.1,-13.4,89.9,83.1,-10.1,92.7,84.4,-7.3,94.8,85.4,-5.2,96.6,86.2,-3.5,98.1,87.0,-1.9,99.6,87.7,-0.4,100.9,88.3,1.5,102.0,88.7,3.5,102.7,88.9,5.8,103.0,89.0,8.3,103.0,88.8,10.9,102.4,88.4,13.4,101.5,87.7,15.8,100.0,86.8,18.0,98.2,85.7,19.9,95.9,84.4,21.3,93.5,83.1,22.1,90.8,81.7,22.3,88.1,80.2,22.3,84.9,78.5,22.3,81.0,76.5,22.3,76.2,73.9,21.8],
        [75.3,68.2,18.0,74.9,68.2,13.2,75.1,68.6,8.8,76.1,69.4,4.7,77.2,70.2,1.0,78.4,71.1,-2.6,79.7,72.0,-6.2,81.3,73.0,-10.1,84.5,74.9,-12.3,88.6,77.0,-11.6,92.0,78.7,-10.0,94.8,80.1,-7.8,96.9,81.1,-5.7,98.7,81.9,-3.9,100.3,82.6,-2.3,101.8,83.4,-0.8,103.1,84.0,1.1,104.2,84.4,3.2,104.9,84.7,5.5,105.3,84.7,8.0,105.2,84.5,10.6,104.7,84.1,13.2,103.7,83.4,15.6,102.2,82.5,17.9,100.3,81.4,19.7,98.1,80.1,21.1,95.5,78.7,21.9,92.9,77.3,22.0,90.1,75.8,22.0,87.0,74.1,22.1,83.1,72.1,22.1,77.6,69.2,22.1],
        [77.6,63.9,19.2,77.1,63.8,14.2,77.6,64.4,9.4,78.8,65.3,5.2,80.1,66.2,1.4,81.5,67.2,-2.1,82.9,68.2,-5.7,85.0,69.4,-9.1,89.1,71.6,-9.1,92.5,73.4,-8.9,95.5,75.0,-8.1,98.1,76.3,-6.7,100.2,77.3,-4.9,101.9,78.1,-3.2,103.5,78.8,-1.7,104.9,79.5,0.0,106.0,80.0,1.9,106.8,80.3,3.9,107.4,80.4,6.1,107.6,80.4,8.4,107.4,80.2,10.8,106.9,79.8,13.1,106.0,79.2,15.3,104.7,78.4,17.3,103.0,77.4,19.1,101.0,76.2,20.5,98.7,74.9,21.4,96.3,73.6,21.8,93.6,72.2,21.8,90.6,70.6,21.8,86.9,68.6,21.8,81.8,65.9,21.8],
        [80.5,59.8,20.4,79.3,59.5,15.7,79.9,60.1,10.9,81.3,61.1,6.7,82.8,62.1,3.0,84.2,63.0,-0.5,85.7,64.0,-4.0,88.7,65.8,-5.8,92.4,67.7,-5.8,95.4,69.3,-5.9,98.0,70.7,-5.8,100.5,72.0,-5.3,102.8,73.2,-4.3,104.8,74.1,-2.8,106.5,74.9,-1.0,107.9,75.5,1.0,108.9,75.9,3.1,109.6,76.2,5.4,109.8,76.2,7.7,109.8,76.0,9.9,109.4,75.7,12.1,108.7,75.2,14.2,107.7,74.5,16.1,106.4,73.7,17.8,104.8,72.8,19.3,103.0,71.8,20.4,101.0,70.7,21.1,98.8,69.5,21.5,96.5,68.3,21.5,93.9,66.9,21.4,90.8,65.3,21.3,86.7,63.1,21.0],
        [81.5,55.1,16.7,82.1,55.6,13.2,83.2,56.4,10.3,84.2,57.0,8.1,85.0,57.6,6.1,85.7,58.1,4.3,86.5,58.6,2.5,87.3,59.1,0.4,88.3,59.8,-2.0,90.4,61.0,-2.6,92.5,62.1,-2.4,94.6,63.2,-2.3,96.5,64.2,-1.8,98.4,65.1,-0.9,100.0,65.9,0.4,101.3,66.5,2.1,102.3,66.9,4.0,102.9,67.1,6.1,103.0,67.0,8.2,102.8,66.8,10.2,102.3,66.4,12.1,101.4,65.8,13.7,100.3,65.2,15.0,99.0,64.4,16.1,97.6,63.6,16.8,96.2,62.9,17.2,94.7,62.1,17.6,93.3,61.3,17.8,91.6,60.4,18.0,89.7,59.4,18.2,87.4,58.1,18.3,84.1,56.3,18.4],
        [87.0,52.8,11.4,87.3,53.1,9.8,87.6,53.4,8.4,88.0,53.6,7.4,88.4,53.9,6.4,88.7,54.1,5.5,89.1,54.3,4.6,89.5,54.6,3.6,90.0,54.9,2.4,91.1,55.5,2.4,92.1,56.1,2.5,93.0,56.5,3.0,93.7,56.8,3.7,94.2,57.0,4.6,94.5,57.2,5.3,94.7,57.3,5.9,94.8,57.3,6.6,94.9,57.3,7.1,94.9,57.3,7.7,94.9,57.2,8.2,94.8,57.2,8.6,94.7,57.1,9.1,94.6,57.0,9.6,94.4,56.8,10.2,94.1,56.6,10.7,93.6,56.4,11.2,93.1,56.0,11.7,92.4,55.7,12.1,91.5,55.2,12.4,90.5,54.6,12.7,89.1,53.9,12.9,87.4,53.0,12.8]]
};
const POD_AXIS = {
    "12" : [[98.2136, 69.2996, 8.0000], [0.207627, -0.976807, -0.052336]],
    "20" : [[98.2136, 69.2996, 8.0000], [0.341551, -0.938405, -0.052336]],
    "28" : [[98.2136, 69.2996, 8.0000], [0.468828, -0.881738, -0.052336]]
};
