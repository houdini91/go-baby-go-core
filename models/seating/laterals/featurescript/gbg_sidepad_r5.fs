FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// ===================================================================================================================
// GoBabyGo SIDE PADS (lateral trunk supports), ROUND 5 production design, for GbG_chair_forth.
// Spec: models/seating/laterals/ROUND5_PLAN.md (AGREED items binding; part numbers per §8, scheme awaiting owner OK).
// Wing-side geometry ported from viewer/r5mesh/gen_r5.py (the mesh study the owner saw); kit from gbg_sidepad_v1.fs v1.1.
//
//   WING (chair change, feature "GbG Side Pad Wing (chair)"): v1.1 tip block/rails/blanks removed. One 40 mm notch in the
//     wing's front edge (z 207-247) with a 14.1 x 26 slot, a catch groove (6 deg locking face) in its outer wall, and the old
//     tongue pocket / slit behind the cheek / dead lower M5 pocket filled inside the notch band. The piece of wing cut out
//     becomes the BLANK CAP (nose + stub tongue with a snap finger). Option: close the open pocket above the nose (z 247-270).
//   KIT ("GbG Side Pad r5"): carrier = the cap's nose (the wing's own front section) + tongue + one smooth arm (short S drop 22,
//     long L drop 44) + hidden tenon; latch A (button is the catch: 12 x 24 stadium, bar, tooth with 6 deg face, spring, orange
//     tell sleeve, hard stop); one Ø6 carbon rod (Ø6.5 teardrop bore); 4 x Ø2 tenon pins; depth piece D0 with tilt feet
//     0 / A8 / B8 / A15 / B15 and two dovetail grooves (pad positions upper / lower = ±11) closed at the front, hidden step
//     at the tenon tip; pad shell (one dovetail rail, z-symmetric: one part for both sides); clip ring with snap barb;
//     reference foam (5/10/20/30) + 3 mm neoprene cover.
// FLIP: the wing tip leans (+3.2 deg in d, +3.95 deg in t), so a z-mirrored nose would be 2.3 / 2.8 mm off the wing at the
//   notch ends: one carrier cannot serve LOW and HIGH. Each carrier is built for its side and LOW/HIGH (4 per arm length).
// Frames: chair WORLD, mm, z up, -y = front; LEFT side at +x (the right side is its mirror).
//   F (wing-tip frame of v1.1): Q(d, t, z); d across the tip (+ outboard), t into the wing (- = front).
//   S = F with the blade's lean removed (d' = d - LD (z - 227), t' = t - LT (z - 227)). Everything at the mount is built in S
//   with Q() and then sheared by shearS() (opTransform with an affine matrix), so it leans exactly like the wing.
// ===================================================================================================================

export enum SidePadSide { annotation { "Name" : "Left (+x)" } LEFT, annotation { "Name" : "Right (-x)" } RIGHT, annotation { "Name" : "Both" } BOTH }
export enum SidePadArm { annotation { "Name" : "Short (pad drop 22)" } SHORT, annotation { "Name" : "Long (pad drop 44)" } LONG }
export enum SidePadKit { annotation { "Name" : "LOW (pad below the mount)" } LOW, annotation { "Name" : "HIGH (pad above the mount)" } HIGH }
export enum SidePadPadPos { annotation { "Name" : "Upper (+11)" } UPPER, annotation { "Name" : "Lower (-11)" } LOWER }
export enum SidePadTilt { annotation { "Name" : "0 deg" } T0, annotation { "Name" : "8 deg top in" } IN8, annotation { "Name" : "8 deg top out" } OUT8, annotation { "Name" : "15 deg top in" } IN15, annotation { "Name" : "15 deg top out" } OUT15 }
export enum SidePadFoam { annotation { "Name" : "5 mm" } F5, annotation { "Name" : "10 mm" } F10, annotation { "Name" : "20 mm (standard)" } F20, annotation { "Name" : "30 mm" } F30 }
export enum SidePadMode { annotation { "Name" : "Kit on" } ON, annotation { "Name" : "Kit off, blank cap fitted" } OFF_CAP, annotation { "Name" : "Kit off, notch open" } OFF_OPEN }
export enum SidePadView { annotation { "Name" : "Fitted on the chair" } FITTED, annotation { "Name" : "Print layout (one side) + latch coupon" } PRINT, annotation { "Name" : "All parts and variants (grid)" } ALL }
export enum SidePadCheck { annotation { "Name" : "Off" } OFF, annotation { "Name" : "Left side (40 settings)" } LEFT, annotation { "Name" : "Right side (40 settings)" } RIGHT }

// ---------------- chair / wing-tip frame (v1.1) ----------------
const P0 = [131.3, 80.7];  const AXH = [0.79577626, 0.60559074];  const TH = [-0.6083053, 0.79370313];
const D_SHIFT = 2.0;
const Z0 = 227.8;  const SHEAR = [0.8 / 32.9, 2.7 / 32.9];  const ZREF = 244.25;
const LD = 0.0565;  const LT = 0.069;  const ZC = 227.0;        // blade lean (measured on the original wing, gen_r5.py)
// ---------------- mount (S frame) ----------------
const C = 227.0;                                                // tongue centre = the one mount
const NZ = [207.0, 247.0];  const SEAM = 0.3;                   // notch band; the nose is 0.3 smaller each side (hairlines)
const T_FRONT = -14.27;  const T_BACK = 2.0;                    // wing front face; notch back face (nose t' -14.27 .. 1.7)
const TG_Z = [214.3, 246.4];  const CLR = 0.2;                  // tongue height: up to the nose top (the old pocket above is filled); slot = tongue + 0.2, top flush with the notch ceiling
// tongue outline (d', t'): rectangle at the notch face, back edge 5.5 from the outer air and 6 (normal) from strap slit S1
const TONGUE = [[3.75, 1.0], [17.45, 1.0], [17.45, 3.0], [17.285, 9.3], [11.4, 15.0], [9.6, 18.8], [8.65, 22.2], [7.9, 23.4], [4.012, 18.5], [3.75, 8.5]];
// slot = tongue + 0.2, except the 1.5 deg/side wedge faces near the tip (inner t' 8.5-18.5, outer t' 3-9.3): zero clearance there
const SLOT = [[3.55, 1.0], [17.65, 1.0], [17.65, 1.6], [17.45, 3.0], [17.285, 9.3], [11.565, 15.118], [9.788, 18.87], [8.835, 22.282], [7.919, 23.746], [4.012, 18.5], [3.75, 8.5], [3.55, 7.7]];
const PADS = [5.5, 13.5];  const PAD_REL = 0.3;                 // tongue bottom relieved 0.3 between t' 5.5 and 13.5: it bears on pads at the mouth and the tip
const RIBS = [15.0, 17.0];  const RIB_R = 0.4;                  // 2 crush ribs (0.2 interference) across the top and bottom faces near the tip
const MOUTH = 3.3;  const TG_ROOT = 3.0;                        // slot mouth chamfer 3.3 x 45 (sides + bottom) clears the R3 tongue root fillet
const W1 = [7.3, 17.9, 14.0];                                   // old tongue pocket filled in the band (d0, d1, t1)
const SLIT = [8.0, 26.0, 11.0, 22.0];                           // old slit behind the cheek, closed with the z 215.2 section
const ENV = [-14.0, 34.0, -20.0, 40.0];                         // tip region (d', t') used for the live sections
// ---------------- latch A (S frame, LOW; HIGH = z-mirror about C) ----------------
const GK = 0.10510423526567646;                                 // tan 6 deg: the locking face
const GR_T = 4.5;  const GR_D = 22.2;  const GR_Z = [223.2, 246.3];  const GR_CH = 0.5;   // groove 4.0 deep; 0.5 chamfer at the shoulder edge   // catch groove: front face t' 4.5 at the slot wall (d' 17.65), to d' 21.1
const BT_TC = -6.3;  const BT_W = 12.0;  const BT_H = 24.0;  const BT_THK = 4.6;  const TRAVEL = 4.9;   // button 12 x 24, travel 3.8 to a hard stop
const BLK_D = [13.0, 17.35];  const BLK_T = [-4.0, 9.0];  const BLK_Z = [223.5, 246.0];          // block + tooth above the rod (LOW and HIGH alike)
const TO_D = [17.3, 22.0];                                      // tooth (6 deg face at t' 4.52, 45 deg lead-in behind), z = block
const LIP = 1.6;  const LIP_W = 13.2;  const FL_W = 13.0;  const FL_T = 0.6;  const SL_OUT = 13.8;  const SL_IN = 12.4;  const SL_H = 2.4;
const SPR = [-8.5, 231.0, 3.25, 10.0];                          // spring slot (t', z LOW, r, d' floor): OD 6, L0 14, installed 10
const ROD = [8.6, 218.75, 3.25];  const ROD_T = [13.5, -26.0, -22.0];  // Ø6 rod in a Ø6.5 teardrop: (d', z LOW, bore r); rod t' from .. to
const PIN_L = 21.0;                                             // Ø2 tenon pins
// ---------------- arm (v1.1 path) ----------------
const TAPER_T = -14.0;  const DC = 9.4;  const YEND = 35.0;
const ARM_W = 18.0;  const ARM_H = 28.0;  const ARM_R = 9.0;  const GROW = 1.0;  const ROOT_R = 3.5;
const YAX = YEND + GROW;
const DROPS = [22.0, 44.0];
// ---------------- joint ----------------
const TEN_W = 13.0;  const TEN_H = 20.0;  const TEN_L = 13.0;  const TEN_R = 2.5;  const JS_DX = 6.5;  const JGAP = 0.3;  const STEP_GAP = 0.1;
const TPIN = [[3.5, 7.4], [-3.5, 7.4], [3.5, -7.4], [-3.5, -7.4]];  const TPIN_R = 1.15;  const TPIN_IN = 8.0;
// ---------------- depth piece + pad mount ----------------
const XS = 104.0;                                               // D0: pad back x 104 (the one depth piece; width set by the foam)
const PAD_OFF = 11.0;                                           // pad screw positions: pad centre = arm axis +- 11
const FLARE_L = 18.0;  const FOOT_Y = [19.0, 46.0];  const FOOT_HZ = 20.0;  const FOOT_R = 14.5;
const DV = [9.0, 12.0, 2.5];                                    // dovetail rail: root width, top width (z), height
const DV_CL = 0.15;  const DV_FRONT = 22.0;                     // groove clearance per side; groove closed in front of y 22 (the stop)
const PS_Y = 32.5;                                              // pad screw y (foot centre)
// ---------------- pad ----------------
const PAD_Y0 = -27.0;  const PAD_Y1 = 55.0;  const PAD_H = 80.0;  const SHELL_T = 4.5;
const A_REF = 85.0;  const B_REF = 88.0;  const YC_REF = 34.0;  const WRAP = 0.7;  const SE_P = 4.0;
const GROOVE = [3.2, 7.6];  const GROOVE_D = 2.5;               // cover groove in the back face (3 mm neoprene, one layer, squeezed to 2.4)
const RING = [5.6, 7.6, 2.1];  const BARB = 0.4;  const UNDERCUT = 8.0;  // ring band, snap barb into an undercut to 8.0
const COVER_T = 3.0;
// ---------------- fits (P1S / PETG) ----------------
const CL = 0.2;  const M5_CLEAR = 5.5;  const M5_NUT_AF = 8.3;  const M5_NUT_P = 5.1;  const CSK_M5_R = 5.3;
const BH_R = 5.25;  const BH_K = 3.0;                           // ISO 7380 M5 button head (dk 9.5, k 2.75) counterbore

// ---------------- small helpers ----------------
function v3(x, y, z) returns Vector { return vector(x, y, z) * millimeter; }
function sgn(x) returns number { return x < 0 ? -1 : 1; }
function lerp(a is number, b is number, t is number) returns number { return a + (b - a) * t; }
function linspace(a is number, b is number, n is number) returns array
{
    var o = [];
    for (var i = 0; i < n; i += 1) o = append(o, n == 1 ? a : a + (b - a) * i / (n - 1));
    return o;
}
function r1(x) returns number { return roundToPrecision(x, 1); }
function solids(context is Context, of is Id) returns Query { return qBodyType(qCreatedBy(of, EntityType.BODY), BodyType.SOLID); }
function oneBody(context is Context, of is Id, what is string) returns Query
{
    const b = evaluateQuery(context, solids(context, of));
    if (size(b) != 1) throw regenError(what ~ ": expected 1 solid, found " ~ size(b));
    return b[0];
}
function one(context is Context, q is Query, what is string) returns Query
{
    const b = evaluateQuery(context, qBodyType(q, BodyType.SOLID));
    if (size(b) != 1) throw regenError(what ~ ": expected 1 solid, found " ~ size(b));
    return b[0];
}
function warn(context is Context, what is string)
{
    var prev = "";
    try silent { prev = getVariable(context, "gbgSkipped"); if (prev != "") prev = prev ~ "; "; }
    setVariable(context, "gbgSkipped", prev ~ what);
}
function attempt(context is Context, id is Id, fn is function) returns boolean
{
    var ok = false;
    startFeature(context, id);
    try silent { fn(id); ok = getFeatureError(context, id) == undefined; }
    if (ok) { endFeature(context, id); return true; }
    abortFeature(context, id);
    return false;
}
function finishEdges(context is Context, id is Id, edges is Query, r is number, what is string)
{
    if (size(evaluateQuery(context, edges)) == 0) return;
    if (!attempt(context, id, function(sid) { opFillet(context, sid + "op", { "entities" : edges, "radius" : r * millimeter }); }))
        warn(context, what);
}
// fillet that must work (visible shape): failure is an error, never skipped silently
function mustFillet(context is Context, id is Id, edges is Query, r is number, what is string)
{
    if (size(evaluateQuery(context, edges)) == 0) throw regenError(what ~ ": no edges found");
    try { opFillet(context, id, { "entities" : edges, "radius" : r * millimeter }); }
    catch (e) { throw regenError(what ~ ": fillet " ~ r ~ " failed"); }
}
// wing-tip frame point (mm vector)
function Q(d is number, t is number, z is number) returns Vector
{
    const dd = d + D_SHIFT;
    return vector(P0[0] + dd * AXH[0] + t * TH[0] + (ZREF - Z0) * SHEAR[0], P0[1] + dd * AXH[1] + t * TH[1] + (ZREF - Z0) * SHEAR[1], z);
}
function dirD() returns Vector { return normalize(vector(AXH[0], AXH[1], 0)); }
function dirT() returns Vector { return normalize(vector(TH[0], TH[1], 0)); }
function leanV() returns Vector { return vector(LD * AXH[0] + LT * TH[0], LD * AXH[1] + LT * TH[1], 0); }
// S -> F shear (about z = ZC) as a world transform
function shearS() returns Transform
{
    const l = leanV();
    return transform(matrix([[1, 0, l[0]], [0, 1, l[1]], [0, 0, 1]]), vector(-ZC * l[0], -ZC * l[1], 0) * millimeter);
}
function unshearS() returns Transform
{
    const l = leanV();
    return transform(matrix([[1, 0, -l[0]], [0, 1, -l[1]], [0, 0, 1]]), vector(ZC * l[0], ZC * l[1], 0) * millimeter);
}
// S-frame point -> world (mm vector)
function QS(d is number, t is number, z is number) returns Vector { return Q(d + LD * (z - ZC), t + LT * (z - ZC), z); }
// world point (mm vector) -> [d', t', z] in S
function toS(p is Vector) returns array
{
    const bx = P0[0] + D_SHIFT * AXH[0] + (ZREF - Z0) * SHEAR[0];  const by = P0[1] + D_SHIFT * AXH[1] + (ZREF - Z0) * SHEAR[1];
    const rx = p[0] - bx;  const ry = p[1] - by;
    const det = AXH[0] * TH[1] - TH[0] * AXH[1];
    const d = (rx * TH[1] - TH[0] * ry) / det;  const t = (AXH[0] * ry - rx * AXH[1]) / det;
    return [d - LD * (p[2] - ZC), t - LT * (p[2] - ZC), p[2]];
}
function mirX() returns Transform { return mirrorAcross(plane(v3(0, 0, 0), vector(1, 0, 0))); }
// box in the wing-tip frame (unsheared)
function tipBox(context is Context, id is Id, d0, d1, t0, t1, z0, z1)
{
    fCuboid(context, id + "b", { "corner1" : v3(0, 0, 0), "corner2" : v3(d1 - d0, t1 - t0, z1 - z0) });
    const o = Q(d0, t0, z0);
    const xf = transform(v3(o[0], o[1], o[2])) * toWorld(coordSystem(vector(0, 0, 0) * meter, dirD(), vector(0, 0, 1)));
    opTransform(context, id + "x", { "bodies" : qCreatedBy(id + "b", EntityType.BODY), "transform" : xf });
}
function cylAx(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number)
{
    fCylinder(context, id, { "bottomCenter" : p0 * millimeter, "topCenter" : p1 * millimeter, "radius" : r * millimeter });
}
// polygon (d, t) of the tip frame extruded over z (unsheared)
function dtPrism(context is Context, id is Id, pts is array, z0 is number, z1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, z0), vector(0, 0, 1), vector(1, 0, 0)) });
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = Q(pts[i][0], pts[i][1], z0);  const b = Q(pts[(i + 1) % size(pts)][0], pts[(i + 1) % size(pts)][1], z0);
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// LEANED sketch plane at d' = d0 (S frame), spanned by t' and the leaned z direction: S point (t', z) -> 2D coords
function tzS() returns Vector { return leanV() + vector(0, 0, 1); }
function tzPlaneS(d0 is number) returns Plane
{
    const n = normalize(cross(dirT(), tzS()));
    return plane(Q(d0, 0, ZC) * millimeter, n, dirT());
}
function tz2(t is number, z is number) returns Vector
{
    const n = normalize(cross(dirT(), tzS()));
    const yd = cross(n, dirT());
    const w = dirT() * t + tzS() * (z - ZC);
    return vector(dot(w, dirT()), dot(w, yd)) * millimeter;
}
// stadium (t-z, S frame) prism along d, built directly in the leaned frame: centre (tc, zc), width w along t, height h along z
function stadiumD(context is Context, id is Id, tc is number, zc is number, w is number, h is number, d0 is number, d1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : tzPlaneS(d0) });
    const a = w / 2;  const b = h / 2;
    skArc(sk, "at", { "start" : tz2(tc + a, zc + b - a), "mid" : tz2(tc, zc + b), "end" : tz2(tc - a, zc + b - a) });
    skLineSegment(sk, "l", { "start" : tz2(tc - a, zc + b - a), "end" : tz2(tc - a, zc - b + a) });
    skArc(sk, "ab", { "start" : tz2(tc - a, zc - b + a), "mid" : tz2(tc, zc - b), "end" : tz2(tc + a, zc - b + a) });
    skLineSegment(sk, "r", { "start" : tz2(tc + a, zc - b + a), "end" : tz2(tc + a, zc + b - a) });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : dirD(), "endBound" : BoundingType.BLIND, "endDepth" : (d1 - d0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
function circleD(context is Context, id is Id, tc is number, zc is number, r is number, d0 is number, d1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : tzPlaneS(d0) });
    skCircle(sk, "c", { "center" : tz2(tc, zc), "radius" : r * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : dirD(), "endBound" : BoundingType.BLIND, "endDepth" : (d1 - d0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// TEARDROP bore p0 -> p1, tip toward `up`
function tdHole(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number, up is Vector)
{
    const ax = normalize(p1 - p0);
    var u = up - ax * dot(up, ax);
    u = normalize(u);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0 * millimeter, ax, u) });
    const c45 = r * sqrt(0.5);
    skArc(sk, "c", { "start" : vector(c45, c45) * millimeter, "mid" : vector(-r, 0) * millimeter, "end" : vector(c45, -c45) * millimeter });
    skLineSegment(sk, "a", { "start" : vector(c45, c45) * millimeter, "end" : vector(r * sqrt(2), 0) * millimeter });
    skLineSegment(sk, "b", { "start" : vector(c45, -c45) * millimeter, "end" : vector(r * sqrt(2), 0) * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : ax, "endBound" : BoundingType.BLIND, "endDepth" : norm(p1 - p0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
function hexPrism(context is Context, id is Id, c is Vector, nIn is Vector, af is number, h is number)
{
    const n = normalize(nIn);
    const rv = af / sqrt(3);
    var xd = cross(n, vector(0, 0, 1));
    if (norm(xd) < 0.3) xd = vector(1, 0, 0);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane((c - n * (h / 2)) * millimeter, n, normalize(xd)) });
    var pts = [];
    for (var k = 0; k < 6; k += 1) pts = append(pts, vector(rv * cos((k * 60) * degree), rv * sin((k * 60) * degree)) * millimeter);
    for (var k = 0; k < 6; k += 1) skLineSegment(sk, "h" ~ k, { "start" : pts[k], "end" : pts[(k + 1) % 6] });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : n, "endBound" : BoundingType.BLIND, "endDepth" : h * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// keep only the part of `target` inside a box (subtraction keeps the target's identity); sheared = box in S
function keepOnly(context is Context, id is Id, target is Query, d0, d1, t0, t1, z0, z1, sheared is boolean)
{
    tipBox(context, id + "big", d0 - 400, d1 + 400, t0 - 400, t1 + 400, z0 - 400, z1 + 400);
    tipBox(context, id + "keep", d0, d1, t0, t1, z0, z1);
    opBoolean(context, id + "hole", { "tools" : qCreatedBy(id + "keep" + "b", EntityType.BODY), "targets" : qCreatedBy(id + "big" + "b", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    if (sheared) opTransform(context, id + "sh", { "bodies" : qCreatedBy(id + "big" + "b", EntityType.BODY), "transform" : shearS() });
    opBoolean(context, id + "trim", { "tools" : qCreatedBy(id + "big" + "b", EntityType.BODY), "targets" : target, "operationType" : BooleanOperationType.SUBTRACTION });
}
// target := target ∩ other (by subtraction, so the target keeps its identity); `other` is deleted
function intersectInto(context is Context, id is Id, target is Query, other is Query)
{
    opPattern(context, id + "c", { "entities" : target, "transforms" : [identityTransform()], "instanceNames" : ["c"] });
    opBoolean(context, id + "a", { "tools" : other, "targets" : qCreatedBy(id + "c", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "b", { "tools" : qCreatedBy(id + "c", EntityType.BODY), "targets" : target, "operationType" : BooleanOperationType.SUBTRACTION });
}
function copyOf(context is Context, id is Id, q is Query) returns Query
{
    opPattern(context, id, { "entities" : q, "transforms" : [identityTransform()], "instanceNames" : ["c"] });
    return qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID);
}
function bboxStr(context is Context, q is Query) returns string
{
    const bb = evBox3d(context, { "topology" : q });
    return "[" ~ r1(bb.minCorner[0] / millimeter) ~ "," ~ r1(bb.minCorner[1] / millimeter) ~ "," ~ r1(bb.minCorner[2] / millimeter) ~ " .. " ~ r1(bb.maxCorner[0] / millimeter) ~ "," ~ r1(bb.maxCorner[1] / millimeter) ~ "," ~ r1(bb.maxCorner[2] / millimeter) ~ "]";
}
// engrave text 0.4 deep: plane origin o (centre of the text box), x along xd, normal n pointing OUT of the face
function engrave(context is Context, id is Id, target is Query, o is Vector, xd is Vector, n is Vector, txt is string, h is number, depth is number)
{
    const ok = attempt(context, id, function(sid)
        {
            const nn = normalize(n);
            const w = h * 0.62 * length(txt);
            const sk = newSketchOnPlane(context, sid + "sk", { "sketchPlane" : plane((o + nn * 0.01) * millimeter, nn, normalize(xd)) });
            skText(sk, "t", { "text" : txt, "fontName" : "OpenSans-Regular.ttf", "firstCorner" : vector(-w / 2, -h / 2) * millimeter, "secondCorner" : vector(w / 2, h / 2) * millimeter });
            skSolve(sk);
            opExtrude(context, sid + "ex", { "entities" : qSketchRegion(sid + "sk", true), "direction" : -nn, "endBound" : BoundingType.BLIND, "endDepth" : (depth + 0.01) * millimeter });
            opDeleteBodies(context, sid + "dsk", { "entities" : qCreatedBy(sid + "sk", EntityType.BODY) });
            opBoolean(context, sid + "cut", { "tools" : solids(context, sid + "ex"), "targets" : target, "operationType" : BooleanOperationType.SUBTRACTION });
        });
    if (!ok) warn(context, "engraving '" ~ txt ~ "' failed");
}
// edges of q whose midpoint lies at S-frame t' in [t0, t1]
function edgesAtT(context is Context, q is Query, t0 is number, t1 is number) returns Query
{
    var out = [];
    for (var e in evaluateQuery(context, q))
    {
        const ln = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
        const pS = toS(ln.origin / millimeter);
        if (pS[1] >= t0 && pS[1] <= t1) out = append(out, e);
    }
    return qUnion(out);
}
function stage(context is Context, st is string)
{
    var prev = "";
    try silent { prev = getVariable(context, "gbgStage"); }
    const parts = splitIntoCharacters(prev);
    if (size(parts) > 2000) { var t = ""; for (var i = size(parts) - 1500; i < size(parts); i += 1) t = t ~ parts[i]; prev = t; }
    setVariable(context, "gbgStage", prev ~ " | " ~ st);
}
function stageOf(context is Context) returns string { var st = "?"; try silent { st = getVariable(context, "gbgStage"); } return st; }
function name(context is Context, q is Query, n is string) { setProperty(context, { "entities" : q, "propertyType" : PropertyType.NAME, "value" : n }); }
function paint(context is Context, q is Query, c is Color) { setProperty(context, { "entities" : q, "propertyType" : PropertyType.APPEARANCE, "value" : c }); }

// ---------------- smooth geometry (lines + tangent arcs, splines, ONE loft) ----------------
function rrProfile(context is Context, id is Id, c is Vector, nIn is Vector, w is number, h is number, r is number, hint) returns Query
{
    const n = normalize(nIn);
    var xd = hint == undefined ? cross(vector(0, 0, 1), n) : hint - n * dot(hint, n);
    if (norm(xd) < 1e-6) xd = vector(1, 0, 0);
    const sk = newSketchOnPlane(context, id, { "sketchPlane" : plane(c * millimeter, n, normalize(xd)) });
    const a = w / 2;  const b = h / 2;
    if (r >= min(a, b) - 0.06)
    {
        if (w <= h)
        {
            skArc(sk, "at", { "start" : vector(a, b - a) * millimeter, "mid" : vector(0, b) * millimeter, "end" : vector(-a, b - a) * millimeter });
            skLineSegment(sk, "l", { "start" : vector(-a, b - a) * millimeter, "end" : vector(-a, -b + a) * millimeter });
            skArc(sk, "ab", { "start" : vector(-a, -b + a) * millimeter, "mid" : vector(0, -b) * millimeter, "end" : vector(a, -b + a) * millimeter });
            skLineSegment(sk, "r", { "start" : vector(a, -b + a) * millimeter, "end" : vector(a, b - a) * millimeter });
        }
        else
        {
            skArc(sk, "ar", { "start" : vector(a - b, -b) * millimeter, "mid" : vector(a, 0) * millimeter, "end" : vector(a - b, b) * millimeter });
            skLineSegment(sk, "t", { "start" : vector(a - b, b) * millimeter, "end" : vector(-a + b, b) * millimeter });
            skArc(sk, "al", { "start" : vector(-a + b, b) * millimeter, "mid" : vector(-a, 0) * millimeter, "end" : vector(-a + b, -b) * millimeter });
            skLineSegment(sk, "b", { "start" : vector(-a + b, -b) * millimeter, "end" : vector(a - b, -b) * millimeter });
        }
        skSolve(sk);
        return qSketchRegion(id);
    }
    const rr = min(r, min(a, b) - 0.05);  const k = rr * sqrt(0.5);
    skLineSegment(sk, "t", { "start" : vector(-a + rr, b) * millimeter, "end" : vector(a - rr, b) * millimeter });
    skArc(sk, "tr", { "start" : vector(a - rr, b) * millimeter, "mid" : vector(a - rr + k, b - rr + k) * millimeter, "end" : vector(a, b - rr) * millimeter });
    skLineSegment(sk, "r", { "start" : vector(a, b - rr) * millimeter, "end" : vector(a, -b + rr) * millimeter });
    skArc(sk, "br", { "start" : vector(a, -b + rr) * millimeter, "mid" : vector(a - rr + k, -b + rr - k) * millimeter, "end" : vector(a - rr, -b) * millimeter });
    skLineSegment(sk, "b", { "start" : vector(a - rr, -b) * millimeter, "end" : vector(-a + rr, -b) * millimeter });
    skArc(sk, "bl", { "start" : vector(-a + rr, -b) * millimeter, "mid" : vector(-a + rr - k, -b + rr - k) * millimeter, "end" : vector(-a, -b + rr) * millimeter });
    skLineSegment(sk, "l", { "start" : vector(-a, -b + rr) * millimeter, "end" : vector(-a, b - rr) * millimeter });
    skArc(sk, "tl", { "start" : vector(-a, b - rr) * millimeter, "mid" : vector(-a + rr - k, b - rr + k) * millimeter, "end" : vector(-a + rr, b) * millimeter });
    skSolve(sk);
    return qSketchRegion(id);
}
function sec(c is Vector, n is Vector, w is number, h is number, r is number) returns map
{
    return { "c" : c, "n" : n, "w" : w, "h" : h, "r" : r, "x" : undefined };
}
function rrLoft(context is Context, id is Id, secs is array, tanStart is boolean, tanEnd is boolean)
{
    var profs = [];
    for (var i = 0; i < size(secs); i += 1)
        profs = append(profs, rrProfile(context, id + ("p" ~ i), secs[i].c, secs[i].n, secs[i].w, secs[i].h, secs[i].r, secs[i].x));
    var di = [];
    if (tanStart) di = append(di, { "profileIndex" : 0, "vector" : normalize(secs[0].n), "magnitude" : 1, "tangentToPlane" : false });
    if (tanEnd) di = append(di, { "profileIndex" : size(secs) - 1, "vector" : normalize(secs[size(secs) - 1].n), "magnitude" : 1, "tangentToPlane" : false });
    var def = { "profileSubqueries" : profs, "bodyType" : ToolBodyType.SOLID };
    if (size(di) > 0) def.derivativeInfo = di;
    try { opLoft(context, id + "loft", def); }
    catch (e) { throw regenError("Side pad: smooth loft failed at " ~ toString(id) ~ " (" ~ size(secs) ~ " sections)"); }
    var sks = [];
    for (var i = 0; i < size(secs); i += 1) sks = append(sks, qCreatedBy(id + ("p" ~ i), EntityType.BODY));
    opDeleteBodies(context, id + "dsk", { "entities" : qUnion(sks) });
}

// =============================================== ARM PATH (v1.1) ===============================================
export function armPath(c is number, zp is number) returns map
{
    const st = Q(DC, TAPER_T, c);
    const h0 = -dirT();
    const th0 = atan2(h0[1], h0[0]) / radian;
    const R = (YEND - st[1]) / (sin((th0 - PI / 2) * radian) - 1);
    const Cc = [st[0] + R * cos((th0 - PI / 2) * radian), st[1] + R * sin((th0 - PI / 2) * radian)];
    const L = R * (th0 + PI);
    const dz = zp - c;
    const thv = 2 * atan(abs(dz) / L) / radian;
    const Rv = L / (2 * sin(thv * radian));
    return { "st" : st, "th0" : th0, "R" : R, "C" : Cc, "L" : L, "dz" : dz, "Rv" : Rv, "c" : c, "h0" : h0 };
}
export function armAt(p is map, u is number) returns map
{
    if (u < 0) return { "p" : vector(p.st[0] + p.h0[0] * u, p.st[1] + p.h0[1] * u, p.c), "t" : p.h0 };
    const th = p.th0 - u / p.R;
    const x = p.C[0] + p.R * cos((th + PI / 2) * radian);
    const y = p.C[1] + p.R * sin((th + PI / 2) * radian);
    var zz = 0;  var sl = 0;
    if (u <= p.L / 2)
    {
        const ph = asin(min(u / p.Rv, 1)) / radian;
        zz = p.Rv * (1 - cos(ph * radian));  sl = tan(ph * radian);
    }
    else
    {
        const ph = asin(min((p.L - u) / p.Rv, 1)) / radian;
        zz = abs(p.dz) - p.Rv * (1 - cos(ph * radian));  sl = tan(ph * radian);
    }
    const s = p.dz < 0 ? -1 : 1;
    return { "p" : vector(x, y, p.c + s * zz), "t" : normalize(vector(cos(th * radian), sin(th * radian), s * sl)) };
}
// arm section centre (grown GROW toward the inside of the bend)
function armCentre(p is map, u is number) returns Vector
{
    const a = armAt(p, u);
    var inw = normalize(vector(p.C[0] - a.p[0], p.C[1] - a.p[1], 0));
    if (u < 0) inw = normalize(vector(p.C[0] - p.st[0], p.C[1] - p.st[1], 0));
    return a.p + inw * GROW;
}

// =============================================== WING (chair change) ===============================================
// live section of `wing` at z: a slab copy cut to the tip region (unsheared F box around the S region), its face at z
// extruded along the lean from z0 to z1 -> a leaned prism of the wing's own section
function sectionPrism(context is Context, id is Id, wing is Query, z is number, reg is array, z0 is number, z1 is number, lean is Vector) returns Query
{
    const cp = copyOf(context, id + "cp", wing);
    keepOnly(context, id + "k", cp, reg[0] + LD * (z - ZC), reg[1] + LD * (z - ZC), reg[2] + LT * (z - ZC), reg[3] + LT * (z - ZC), z, z + 1.0, false);
    const faces = qCoincidesWithPlane(qOwnedByBody(cp, EntityType.FACE), plane(v3(0, 0, z), vector(0, 0, 1)));
    if (size(evaluateQuery(context, faces)) == 0) throw regenError("Side Pad Wing: no wing section at z " ~ z);
    const w = lean + vector(0, 0, 1);
    opExtrude(context, id + "up", { "entities" : faces, "direction" : normalize(w), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z) * norm(w) * millimeter });
    opExtrude(context, id + "dn", { "entities" : faces, "direction" : -normalize(w), "endBound" : BoundingType.BLIND, "endDepth" : (z - z0) * norm(w) * millimeter });
    opDeleteBodies(context, id + "dcp", { "entities" : cp });
    opBoolean(context, id + "u", { "tools" : qUnion([solids(context, id + "up"), solids(context, id + "dn")]), "operationType" : BooleanOperationType.UNION });
    return qUnion([solids(context, id + "up"), solids(context, id + "dn")]);
}
// loft between two (d', z) polygons in the planes t' = t0 and t' = t1 (S frame), then sheared into the leaned frame
function rrLoftPoly(context is Context, id is Id, p0 is array, p1 is array, t0 is number, t1 is number)
{
    var profs = [];
    const pls = [[p0, t0], [p1, t1]];
    for (var k = 0; k < 2; k += 1)
    {
        const sk = newSketchOnPlane(context, id + ("s" ~ k), { "sketchPlane" : plane(Q(0, pls[k][1], ZC) * millimeter, -dirT(), dirD()) });
        const pts = pls[k][0];
        for (var i = 0; i < size(pts); i += 1)
        {
            const a = pts[i];  const b = pts[(i + 1) % size(pts)];
            skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1] - ZC) * millimeter, "end" : vector(b[0], b[1] - ZC) * millimeter });
        }
        skSolve(sk);
        profs = append(profs, qSketchRegion(id + ("s" ~ k)));
    }
    opLoft(context, id + "loft", { "profileSubqueries" : profs, "bodyType" : ToolBodyType.SOLID });
    opDeleteBodies(context, id + "dsk", { "entities" : qUnion([qCreatedBy(id + "s0", EntityType.BODY), qCreatedBy(id + "s1", EntityType.BODY)]) });
    opTransform(context, id + "sh", { "bodies" : solids(context, id + "loft"), "transform" : shearS() });
}
// S box (sheared)
function sBox(context is Context, id is Id, d0, d1, t0, t1, z0, z1) returns Query
{
    tipBox(context, id, d0, d1, t0, t1, z0, z1);
    opTransform(context, id + "sh", { "bodies" : qCreatedBy(id + "b", EntityType.BODY), "transform" : shearS() });
    return qCreatedBy(id + "b", EntityType.BODY);
}
// S polygon prism over z (sheared), with optional vertical-edge round r
function sPoly(context is Context, id is Id, pts is array, z0 is number, z1 is number, r is number) returns Query
{
    dtPrism(context, id + "p", pts, z0, z1);
    const b = solids(context, id + "p");
    if (r > 0)
    {
        // round only the real corners (turn > 20 deg); the near-straight wedge kinks stay as they are
        var sharp = [];
        for (var i = 0; i < size(pts); i += 1)
        {
            const a0 = pts[(i + size(pts) - 1) % size(pts)];  const a1 = pts[i];  const a2 = pts[(i + 1) % size(pts)];
            const u = normalize(vector(a1[0] - a0[0], a1[1] - a0[1]));  const w = normalize(vector(a2[0] - a1[0], a2[1] - a1[1]));
            if (dot(u, w) < cos(20 * degree)) sharp = append(sharp, a1);
        }
        var vert = [];
        for (var e in evaluateQuery(context, qOwnedByBody(b, EntityType.EDGE)))
        {
            const ln = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
            if (abs(ln.direction[2]) < 0.99) continue;
            const pS = toS(ln.origin / millimeter);
            const pd = pS[0] + LD * (pS[2] - ZC);  const pt = pS[1] + LT * (pS[2] - ZC);     // back to the unsheared tip frame (not yet sheared)
            for (var v in sharp) if (abs(v[0] - pd) < 0.05 && abs(v[1] - pt) < 0.05) { vert = append(vert, e); break; }
        }
        mustFillet(context, id + "r", qUnion(vert), r, "slot/tongue corner round");
    }
    opTransform(context, id + "sh", { "bodies" : b, "transform" : shearS() });
    return b;
}
// all wing tools for the LEFT wing body `wing` (for the right side pass the mirrored copy and mirror the results back)
// the blade's lean measured on this wing (front face and cheek outer face at z 220 and 262), as a world vector per mm of z
function measureLean(context is Context, wing is Query) returns map
{
    var tf = [];  var dout = [];
    for (var z in [220.0, 262.0])
    {
        const hf = evRaycast(context, { "entities" : wing, "ray" : line(Q(4.0, -60, z) * millimeter, dirT()) });
        const ho = evRaycast(context, { "entities" : wing, "ray" : line(Q(60, -4.0, z) * millimeter, -dirD()) });
        if (size(hf) == 0 || size(ho) == 0) throw regenError("Side Pad Wing: could not measure the blade lean");
        tf = append(tf, -60 + hf[0].distance / millimeter);  dout = append(dout, 60 - ho[0].distance / millimeter);
    }
    const lt = (tf[1] - tf[0]) / 42;  const ld = (dout[1] - dout[0]) / 42;
    return { "LT" : lt, "LD" : ld, "v" : vector(ld * AXH[0] + lt * TH[0], ld * AXH[1] + lt * TH[1], 0), "front" : tf[0] - LT * (220 - ZC) };
}
export function wingTools(context is Context, id is Id, wing is Query, closePocket is boolean) returns map
{
    const lm = measureLean(context, wing);
    setVariable(context, "gbgLean", "measured lean LD " ~ roundToPrecision(lm.LD, 4) ~ " LT " ~ roundToPrecision(lm.LT, 4) ~ " (model " ~ LD ~ " / " ~ LT ~ "), front t' " ~ roundToPrecision(lm.front, 2));
    // filled blade section near the tip: section at z 240 (blade + cheek) + the old tongue pocket W1 + the slit closed with z 215.2
    const p240 = sectionPrism(context, id + "s240", wing, 240.0, ENV, 207.0, 270.0, lm.v);
    const slit = sectionPrism(context, id + "s215", wing, 215.2, SLIT, 215.0, 248.0, lm.v);
    const w1 = sectionPrism(context, id + "w1", wing, 215.2, [W1[0], W1[1], -20.0, W1[2]], 207.0, 270.0, lm.v);
    // NOSE / CAP source = (wing + fill) inside the nose box: the wing's own front section, 0.3 smaller each side
    const ncp = copyOf(context, id + "ncp", qUnion([p240, w1]));
    keepOnly(context, id + "nk", ncp, ENV[0], ENV[1], T_FRONT - 0.4, T_BACK + 0.6, 215.5, NZ[1] + 0.5, true);
    const wcp = copyOf(context, id + "wcp", wing);
    opBoolean(context, id + "nu", { "tools" : qUnion([wcp, ncp]), "operationType" : BooleanOperationType.UNION });
    const noseB = one(context, qUnion([wcp, ncp]), "nose");
    keepOnly(context, id + "nb", noseB, -40, 50, -40, T_BACK - SEAM, NZ[0] + SEAM, NZ[1] - SEAM, true);
    // wing fill inside the band (behind the notch face), optionally also closing the open pocket above the nose
    const fill = copyOf(context, id + "fcp", qUnion([p240, w1, slit]));
    opBoolean(context, id + "fu", { "tools" : fill, "operationType" : BooleanOperationType.UNION });
    const fillB = qBodyType(fill, BodyType.SOLID);              // may be more than one piece; each merges with the wing
    keepOnly(context, id + "fk", fillB, ENV[0], ENV[1], T_BACK - 0.6, 26.0, 215.0, NZ[1], true);
    var fills = [fillB];
    if (closePocket)
    {
        const f2 = copyOf(context, id + "f2", qUnion([p240, w1]));
        opBoolean(context, id + "f2u", { "tools" : f2, "operationType" : BooleanOperationType.UNION });
        const f2B = qBodyType(f2, BodyType.SOLID);
        keepOnly(context, id + "f2k", f2B, ENV[0], ENV[1], T_FRONT + 0.02, 26.0, NZ[1] - 0.01, 270.0, true);
        fills = append(fills, f2B);
    }
    opDeleteBodies(context, id + "dsrc", { "entities" : qUnion([p240, w1, slit]) });
    // cuts: notch, slot (tongue + 0.2), catch groove (6 deg locking face)
    const notch = sBox(context, id + "notch", -40, 50, -40, T_BACK, NZ[0], NZ[1]);
    const slot = sPoly(context, id + "slot", SLOT, TG_Z[0] - CLR, TG_Z[1] + CLR, 1.0);
    // mouth chamfer MOUTH x 45 deg on the slot's sides and bottom (clears the tongue's R3 root fillet; lead-in)
    const mz0 = TG_Z[0] - CLR;
    const mz1 = TG_Z[1] + CLR;
    rrLoftPoly(context, id + "mouth", [[SLOT[0][0] - MOUTH, mz0 - MOUTH], [SLOT[1][0] + MOUTH, mz0 - MOUTH], [SLOT[1][0] + MOUTH, mz1 + MOUTH], [SLOT[0][0] - MOUTH, mz1 + MOUTH]],
                                     [[SLOT[0][0], mz0], [SLOT[1][0], mz0], [SLOT[1][0], mz1], [SLOT[0][0], mz1]], T_BACK - 0.01, T_BACK + MOUTH);
    const wallT = 17.45 - (GR_T - 3.0) * 0.0262;
    const groove = sPoly(context, id + "gr", [[16.85, GR_T - GR_CH - 1.0], [wallT, GR_T - GR_CH - 1.0], [wallT, GR_T - GR_CH], [wallT + GR_CH, GR_T + (wallT + GR_CH - 17.65) * GK], [GR_D, GR_T + (GR_D - 17.65) * GK], [GR_D, BLK_T[1]], [16.85, BLK_T[1]]], GR_Z[0], GR_Z[1], 0);
    // blank cap: nose + stub tongue (relief frees a 2-thick finger on the outer side) + snap bump into the groove
    const stub = sPoly(context, id + "stub", [[3.75, 1.0], [17.45, 1.0], [17.45, 3.0], [17.29, 9.0], [3.75, 9.0]], 224.0, 245.0, 1.0);
    const relief = sBox(context, id + "rel", 13.6, 15.4, 2.6, 10.0, 223.0, 246.0);
    opBoolean(context, id + "stc", { "tools" : relief, "targets" : stub, "operationType" : BooleanOperationType.SUBTRACTION });
    const bump = sPoly(context, id + "bump", [[17.2, 5.0], [19.0, 5.4], [19.0, 6.2], [17.2, 8.2]], 228.0, 240.0, 0);
    const capSrc = copyOf(context, id + "capc", noseB);
    opBoolean(context, id + "capu", { "tools" : qUnion([capSrc, stub, bump]), "operationType" : BooleanOperationType.UNION });
    const cap = one(context, qUnion([capSrc, stub, bump]), "blank cap");
    opDeleteBodies(context, id + "dnose", { "entities" : noseB });
    return { "fill" : qUnion(fills), "cut" : qUnion([notch, slot, groove, solids(context, id + "mouth")]), "cuts" : [notch, slot, groove, solids(context, id + "mouth")], "cap" : cap };
}

// =============================================== CARRIER + LATCH A ===============================================
// Built on the LEFT side from `noseSrc` (the blank cap). zs = +1 LOW (pad below), -1 HIGH (pad above).
// Latch A, one sliding part (button = catch): cap (the nose's own skin inside a 12 x 24 stadium, 0.8 thumb scoop) + flange +
// connector + block + tooth (6 deg locking face). It drops in from the nose's free end face (top for LOW, bottom for HIGH:
// hidden in the notch), pressed, and springs out into the lip; the orange tell sleeve then snaps in under the lip and holds
// the flange (captive). The tooth's pull-out load goes block -> the channel's rear wall in the tongue (plastic). The Ø6 rod
// runs below the latch (LOW) through the tongue and nose into the arm root.
function zl(z is number, zs is number) returns number { return zs > 0 ? z : 2 * C - z; }
function zlr(z0 is number, z1 is number, zs is number) returns array { return zs > 0 ? [z0, z1] : [2 * C - z1, 2 * C - z0]; }
// box in S whose z range runs from z0 (LOW) to the nose's free end face (top for LOW, bottom for HIGH)
function dropBox(context is Context, id is Id, d0, d1, t0, t1, z0 is number, zs is number)
{
    const zz = zlr(z0, 262.0, zs);
    tipBox(context, id, d0, d1, t0, t1, zz[0], zz[1]);
}
export function buildCarrier(context is Context, id is Id, noseSrc is Query, drop is number, zs is number, showCarbon is boolean, ribs is boolean) returns map
{
    const zp = C - zs * drop;
    // ---- nose: copy of the cap, cut to the nose box ----
    const nose = copyOf(context, id + "nose", noseSrc);
    keepOnly(context, id + "nk", nose, -40, 50, -40, T_BACK - SEAM, NZ[0] + SEAM, NZ[1] - SEAM, true);
    const hits = evRaycast(context, { "entities" : nose, "ray" : line(QS(60, BT_TC, C) * millimeter, -dirD()) });
    if (size(hits) == 0) throw regenError("carrier: button ray missed the nose");
    const FD = 60 - hits[0].distance / millimeter;               // d' of the nose's outer face (S frame)
    const ncp = copyOf(context, id + "ncp", nose);              // for the button cap (the nose skin)
    // ---- tongue ----
    const tg = sPoly(context, id + "tg", TONGUE, TG_Z[0], TG_Z[1], 1.0);
    // ---- arm: ONE loft through sections square to the path, from inside the nose to the joint ----
    const p = armPath(C, zp);
    var secs = [];
    for (var u in [-5.0, 0.0, p.L / 6, 2 * p.L / 6, 3 * p.L / 6, 4 * p.L / 6, 5 * p.L / 6, p.L])
        secs = append(secs, sec(armCentre(p, u), armAt(p, u).t, ARM_W, ARM_H, ARM_R));
    rrLoft(context, id + "arm", secs, true, true);
    const xe = armAt(p, p.L).p[0];
    rrLoft(context, id + "ten", [sec(vector(xe + 0.5, YAX, zp), vector(-1, 0, 0), TEN_W, TEN_H, TEN_R), sec(vector(xe - TEN_L, YAX, zp), vector(-1, 0, 0), TEN_W, TEN_H, TEN_R)], false, false);
    const parts = qUnion([nose, tg, solids(context, id + "arm"), solids(context, id + "ten")]);
    opBoolean(context, id + "u", { "tools" : parts, "operationType" : BooleanOperationType.UNION });
    const body = one(context, parts, "carrier");
    // R 3.5 root fillet where the arm leaves the nose's front face
    const rootE0 = qIntersection([qAdjacent(qCreatedBy(id + "arm", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE),
                                 qAdjacent(qCreatedBy(id + "nose", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE)]);
    // R 3.5 root fillet where the arm leaves the nose's front face (smaller radius only with a visible warning)
    const rootE = edgesAtT(context, rootE0, -30.0, T_FRONT + 1.5);
    var rootR = 0;
    for (var rr in [ROOT_R, 3.0, 2.5, 2.0])
        if (rootR == 0 && attempt(context, id + ("rootf" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : rootE, "radius" : rr * millimeter }); }))
            rootR = rr;
    if (rootR == 0) warn(context, "arm root fillet failed");
    else if (rootR < ROOT_R) warn(context, "arm root fillet only R" ~ rootR);
    // R3 tongue root (tongue sides and bottom into the nose's back face)
    const tgRoot0 = qAdjacent(qCreatedBy(id + "tg", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE);
    const tgRoot = edgesAtT(context, tgRoot0, T_BACK - SEAM - 0.05, T_BACK - SEAM + 0.05);
    // the outer side edge runs 3 mm from the nose's outer chamfer near the bottom: it gets the largest radius that fits
    var tgOut = [];  var tgRest = [];
    for (var e in evaluateQuery(context, tgRoot))
    {
        const ln = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
        const pS = toS(ln.origin / millimeter);
        if (pS[0] > 16.5 && abs(ln.direction[2]) > 0.7) tgOut = append(tgOut, e); else tgRest = append(tgRest, e);
    }
    var tgR = 0;  var tgRo = 0;
    for (var rr in [TG_ROOT, 2.5, 2.0, 1.5, 1.0])
        if (tgRo == 0 && size(tgOut) > 0 && attempt(context, id + ("tgro" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : qUnion(tgOut), "radius" : rr * millimeter }); }))
            tgRo = rr;
    const tgRest2 = edgesAtT(context, qAdjacent(qCreatedBy(id + "tg", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), T_BACK - SEAM - 0.05, T_BACK - SEAM + 0.05);
    for (var rr in [TG_ROOT, 2.5, 2.0, 1.5])
        if (tgR == 0 && attempt(context, id + ("tgrf" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : tgRest2, "radius" : rr * millimeter }); }))
            tgR = rr;
    setVariable(context, "gbgTgRoot", "tongue root R" ~ tgR ~ " (outer side R" ~ tgRo ~ ")");
    if (tgR < TG_ROOT) warn(context, "tongue root fillet " ~ (tgR == 0 ? "failed" : "only R" ~ tgR) ~ " (" ~ size(tgRest) ~ " edges)");
    if (tgRo == 0 && size(tgOut) > 0) warn(context, "tongue root outer edge not rounded");
    // bearing pads: relieve the tongue's bottom and top 0.3 between the mouth pad and the tip pad
    {
        sBox(context, id + "pr0", 0, 20, PADS[0], PADS[1], TG_Z[0] - 1, TG_Z[0] + PAD_REL);
        sBox(context, id + "pr1", 0, 20, PADS[0], PADS[1], TG_Z[1] - PAD_REL, TG_Z[1] + 1);
        opBoolean(context, id + "prc", { "tools" : qUnion([qCreatedBy(id + "pr0", EntityType.BODY), qCreatedBy(id + "pr1", EntityType.BODY)]), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    }
    finishEdges(context, id + "tenf", qAdjacent(qCapEntity(id + "ten" + "loft", CapType.END, EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), 1.0, "tenon end round");

    // ---- latch A tools, built in S (unsheared) then sheared ----
    const L = id + "lt";
    const capD = [FD - BT_THK, FD];
    stadiumD(context, L + "ca", BT_TC, C, BT_W, BT_H, capD[0], FD + 3);           // A (cut by the nose)
    stadiumD(context, L + "cb", BT_TC, C, BT_W, BT_H, capD[0], FD + 3);           // B -> B - (A - nose) = cap
    stadiumD(context, L + "fl", BT_TC, C, FL_W, BT_H + 1.0, capD[0], capD[0] + FL_T);
    const bz = BLK_Z;
    circleD(context, L + "sp", SPR[0], SPR[1], 2.1, capD[0] - 2.0, capD[0] + 0.05);
    tipBox(context, L + "con", BLK_D[0], capD[0] + 0.05, BLK_T[0], -0.9, bz[0], bz[1]);
    tipBox(context, L + "blk", BLK_D[0], BLK_D[1], BLK_T[0], BLK_T[1], bz[0], bz[1]);
    const tf0 = GR_T + 0.02 + (TO_D[0] - 17.65) * GK;  const tf1 = GR_T + 0.02 + (TO_D[1] - 17.65) * GK;
    const wallT = 17.45 - (GR_T - 3.0) * 0.0262;              // slot outer wall at the shoulder (on the 1.5 deg wedge)
    dtPrism(context, L + "to", [[TO_D[0], GR_T - GR_CH - 0.98], [wallT + GR_CH + 0.05, GR_T + 0.02 + (wallT + GR_CH + 0.05 - 17.65) * GK], [TO_D[1], tf1], [TO_D[1], 6.3], [TO_D[1] - 2.7, BLK_T[1]], [TO_D[0], BLK_T[1]]], bz[0], bz[1]);
    // sleeve (orange tell ring)
    stadiumD(context, L + "so", BT_TC, C, SL_OUT, BT_H + 1.8, capD[0] + FL_T, capD[0] + FL_T + SL_H);
    stadiumD(context, L + "si", BT_TC, C, SL_IN, BT_H + 0.4, capD[0], FD + 1);
    opBoolean(context, L + "sr", { "tools" : solids(context, L + "si"), "targets" : solids(context, L + "so"), "operationType" : BooleanOperationType.SUBTRACTION });
    // carrier cuts
    const K = id + "ct";
    stadiumD(context, K + "lip", BT_TC, C, LIP_W, BT_H + 1.2, FD - LIP - 0.05, FD + 3);
    stadiumD(context, K + "cb", BT_TC, C, SL_OUT + 0.2, BT_H + 2.0, capD[0] + FL_T, FD - LIP);
    stadiumD(context, K + "fp", BT_TC, C, FL_W + 0.4, BT_H + 1.4, capD[0] - TRAVEL, capD[0] + FL_T + 0.01);
    dropBox(context, K + "cch", capD[0] - TRAVEL, capD[1] - TRAVEL + 0.2, BT_TC - (FL_W + 0.4) / 2, BT_TC + (FL_W + 0.4) / 2, C - BT_H / 2 - 0.5, 1);
    dropBox(context, K + "spr", SPR[3], capD[0] - TRAVEL + 0.2, SPR[0] - SPR[2], SPR[0] + SPR[2], SPR[1] - SPR[2], 1);
    dropBox(context, K + "bch", BLK_D[0] - TRAVEL - 0.2, BLK_D[1] + 0.25, BLK_T[0] - 0.3, BLK_T[1] + 0.3, BLK_Z[0] - 0.3, 1);
    dropBox(context, K + "tch", TO_D[0] - TRAVEL - 0.2, 22.6, GR_T - GR_CH - 1.3, BLK_T[1] + 0.3, BLK_Z[0] - 0.3, 1);
    // Ø6 rod bore (teardrop, peak toward -d = up when printed on the nose's outer face), open at the tongue's rear face
    const rz = ROD[1];  const rodEnd = zs > 0 ? ROD_T[1] : ROD_T[2];
    tdHole(context, id + "rodb", QS(ROD[0], 26.0, rz), QS(ROD[0], rodEnd, rz), ROD[2], -dirD());
    // shear everything into the leaned frame
    const M = id + "lm";
    const shl = [L + "con", L + "blk", L + "to", K + "cch", K + "spr", K + "bch", K + "tch"];
    for (var i = 0; i < size(shl); i += 1)
    {
        opTransform(context, M + ("sh" ~ i), { "bodies" : solids(context, shl[i]), "transform" : shearS() });
    }
    // the cap: B - (A - nose) = the nose skin inside the stadium
    opBoolean(context, M + "o1", { "tools" : ncp, "targets" : solids(context, L + "ca"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, M + "o2", { "tools" : solids(context, L + "ca"), "targets" : solids(context, L + "cb"), "operationType" : BooleanOperationType.SUBTRACTION });
    // thumb scoop: cylinder along t', 0.8 deep at the centre, 0 at the round ends
    const RS = ((BT_H / 2 + 1.0) * (BT_H / 2 + 1.0) + 0.64) / 1.6;   // scoop reaches the face 1 mm beyond the round ends (no slivers at the tips)
    cylAx(context, M + "scoop", QS(FD - 0.8 + RS, BT_TC - 20, C), QS(FD - 0.8 + RS, BT_TC + 20, C), RS);
    opBoolean(context, M + "sco", { "tools" : solids(context, M + "scoop"), "targets" : solids(context, L + "cb"), "operationType" : BooleanOperationType.SUBTRACTION });
    // hidden slits (0.4 wide, >= 1.2 inside every surface): the slicer wraps perimeters round them -> internal ribs in the tooth root
    for (var i = 0; i < 2; i += 1)
    {
        tipBox(context, M + ("sl" ~ i), 14.4, 19.0, 5.9 + i * 1.3, 6.3 + i * 1.3, BLK_Z[0] + 1.2, BLK_Z[1] - 1.2);
        opTransform(context, M + ("ss" ~ i), { "bodies" : qCreatedBy(M + ("sl" ~ i) + "b", EntityType.BODY), "transform" : shearS() });
    }
    const bparts = qUnion([solids(context, L + "cb"), solids(context, L + "fl"), solids(context, L + "sp"), qCreatedBy(L + "con" + "b", EntityType.BODY), qCreatedBy(L + "blk" + "b", EntityType.BODY), solids(context, L + "to")]);
    opBoolean(context, M + "bu", { "tools" : bparts, "operationType" : BooleanOperationType.UNION });
    const button = one(context, bparts, "button");
    opBoolean(context, M + "slc", { "tools" : qUnion([qCreatedBy(M + "sl0" + "b", EntityType.BODY), qCreatedBy(M + "sl1" + "b", EntityType.BODY)]), "targets" : button, "operationType" : BooleanOperationType.SUBTRACTION });
    // 0.4 round on the cap's face rim (the edges lying on the nose skin)
    var rim = [];
    for (var e in evaluateQuery(context, qOwnedByBody(button, EntityType.EDGE)))
    {
        const bb = evBox3d(context, { "topology" : e });
        const mid = (bb.minCorner + bb.maxCorner) / 2 / millimeter;
        if (toS(mid)[0] > FD - 1.0) rim = append(rim, e);
    }
    finishEdges(context, M + "rim", qUnion(rim), 0.4, "button face rim round");
    const sleeve = one(context, solids(context, L + "so"), "tell sleeve");
    opBoolean(context, id + "cut", { "tools" : qUnion([solids(context, K), solids(context, id + "rodb")]), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    // rod: bonded from t' ROD_T[0] (inside the tongue, short of the slanted rear face) to 0.5 short of the bore's end
    const rodL = round((ROD_T[0] - rodEnd - 0.5) * 2) / 2;
    // rod wall: a probe the size of the bore, t' 12 .. end, against every carrier face except the bore's own
    var rodWall = -1;
    cylAx(context, id + "rp", QS(ROD[0], 12.0, rz), QS(ROD[0], rodEnd, rz), ROD[2]);
    try silent { rodWall = evDistance(context, { "side0" : solids(context, id + "rp"), "side1" : qSubtraction(qOwnedByBody(body, EntityType.FACE), qCreatedBy(id + "rodb", EntityType.FACE)) }).distance / millimeter; }
    opDeleteBodies(context, id + "drp", { "entities" : solids(context, id + "rp") });
    tenonCuts(context, id + "tc", body, xe, zp);
    // crush ribs (0.2 interference with the slot) across the tongue's bottom and top near the tip
    if (ribs)
    {
        for (var i = 0; i < size(RIBS); i += 1)
        {
            cylAx(context, id + ("rb" ~ i), QS(4.3, RIBS[i], TG_Z[0]), QS(9.0, RIBS[i], TG_Z[0]), RIB_R);
            cylAx(context, id + ("rt" ~ i), QS(4.3, RIBS[i], TG_Z[1]), QS(8.4, RIBS[i], TG_Z[1]), RIB_R);
        }
        var rq = [];
        for (var i = 0; i < size(RIBS); i += 1) rq = concatenateArrays([rq, [solids(context, id + ("rb" ~ i)), solids(context, id + ("rt" ~ i))]]);
        opBoolean(context, id + "rbu", { "tools" : qUnion(concatenateArrays([[body], rq])), "operationType" : BooleanOperationType.UNION });
    }
    const carrierB = one(context, qContainsPoint(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), QS(0, -8, C) * millimeter), "carrier after ribs");
    var carbon = [];
    if (showCarbon)
    {
        cylAx(context, id + "cf6", QS(ROD[0], ROD_T[0], rz), QS(ROD[0], ROD_T[0] - rodL, rz), 3.0);
        for (var i = 0; i < size(TPIN); i += 1)
            cylAx(context, id + ("cf2" ~ i), vector(xe - TEN_L, YAX + TPIN[i][0], zp + TPIN[i][1]), vector(xe - TEN_L + PIN_L, YAX + TPIN[i][0], zp + TPIN[i][1]), 1.0);
        carbon = evaluateQuery(context, qUnion([solids(context, id + "cf6"), solids(context, id + "cf20"), solids(context, id + "cf21"), solids(context, id + "cf22"), solids(context, id + "cf23")]));
    }
    return { "carrier" : carrierB, "button" : button, "sleeve" : sleeve, "carbon" : carbon, "xe" : xe, "zp" : zp, "rodL" : rodL, "rodWall" : rodWall, "FD" : FD };
}
// joint nut + screw hole in the tenon, 4 x Ø2 pin bores across its root
export function tenonCuts(context is Context, id is Id, carrier is Query, xe is number, zp is number)
{
    const xj = xe - JS_DX;
    for (var i = 0; i < size(TPIN); i += 1)
        cylAx(context, id + ("tp" ~ i), vector(xe - TEN_L - 0.5, YAX + TPIN[i][0], zp + TPIN[i][1]), vector(xe + TPIN_IN, YAX + TPIN[i][0], zp + TPIN[i][1]), TPIN_R);
    hexPrism(context, id + "n", vector(xj, YAX - 1.0, zp), vector(0, 1, 0), M5_NUT_AF, M5_NUT_P);
    fCuboid(context, id + "ns", { "corner1" : v3(xj - M5_NUT_AF / 2, YAX - TEN_W / 2 - 1, zp - 4.8), "corner2" : v3(xj + M5_NUT_AF / 2, YAX - 1.0, zp + 4.8) });
    cylAx(context, id + "h", vector(xj, YAX + TEN_W, zp), vector(xj, YAX - TEN_W / 2 - 1, zp), M5_CLEAR / 2);
    opBoolean(context, id + "op", { "tools" : solids(context, id), "targets" : carrier, "operationType" : BooleanOperationType.SUBTRACTION });
}

// =============================================== PAD (shell, ring, foam, cover) ===============================================
function shellXi(y is number, xs is number) returns number
{
    const ex = A_REF * max(1 - abs((y - YC_REF) / B_REF) ^ 3, 0) ^ (1 / 3);
    return xs - SHELL_T - WRAP * (A_REF - ex);
}
function padOutlineYZ(zc is number, ins is number) returns array
{
    var o = [];
    const ym = (PAD_Y0 + PAD_Y1) / 2;  const hy = (PAD_Y1 - PAD_Y0) / 2 - ins;  const hz = PAD_H / 2 - ins;
    for (var k = 0; k < 72; k += 1)
    {
        const a = 2 * PI * k / 72;
        const c = cos(a * radian);  const s = sin(a * radian);
        o = append(o, [ym + hy * sgn(c) * abs(c) ^ (2 / SE_P), zc + hz * sgn(s) * abs(s) ^ (2 / SE_P)]);
    }
    return o;
}
function yzPrism(context is Context, id is Id, pts is array, x0 is number, x1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(x0, 0, 0), vector(1, 0, 0), vector(0, 1, 0)) });
    var p = [];
    for (var q in pts) p = append(p, vector(q[0], q[1]) * millimeter);
    p = append(p, p[0]);
    skFitSpline(sk, "s", { "points" : p });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(1, 0, 0), "endBound" : BoundingType.BLIND, "endDepth" : (x1 - x0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
function planStrip(context is Context, id is Id, xs is number, a is number, b is number, z0 is number, z1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, z0), vector(0, 0, 1), vector(1, 0, 0)) });
    var pa = [];  var pb = [];
    for (var y in linspace(PAD_Y0 - 4, PAD_Y1 + 4, 15)) { pa = append(pa, vector(shellXi(y, xs) + a, y) * millimeter);  pb = append(pb, vector(shellXi(y, xs) + b, y) * millimeter); }
    skFitSpline(sk, "sa", { "points" : pa });
    skFitSpline(sk, "sb", { "points" : pb });
    skLineSegment(sk, "e0", { "start" : pa[0], "end" : pb[0] });
    skLineSegment(sk, "e1", { "start" : pa[size(pa) - 1], "end" : pb[size(pb) - 1] });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// band between outline insets i0 < i1, x between shell-relative a..b (a ring in the back face)
function band(context is Context, id is Id, xs is number, zc is number, i0 is number, i1 is number, a is number, b is number)
{
    planStrip(context, id + "s", xs, a, b, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "o", padOutlineYZ(zc, i0), xs - 80, xs + 20);
    yzPrism(context, id + "i", padOutlineYZ(zc, i1), xs - 80, xs + 20);
    opBoolean(context, id + "r", { "tools" : solids(context, id + "i"), "targets" : solids(context, id + "o"), "operationType" : BooleanOperationType.SUBTRACTION });
    intersectInto(context, id + "x", solids(context, id + "o"), solids(context, id + "s"));
}
// dovetail rail along y (male) on the pad back at height zr; length y0..y1; x from xb (sunk 0.5) outward
function dovetail(context is Context, id is Id, xb is number, zr is number, y0 is number, y1 is number, wr is number, wt is number, h is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, y0, 0), vector(0, 1, 0), vector(1, 0, 0)) });
    const pts = [[xb - 0.6, zr - wr / 2], [xb, zr - wr / 2], [xb + h, zr - wt / 2], [xb + h, zr + wt / 2], [xb, zr + wr / 2], [xb - 0.6, zr + wr / 2]];
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = pts[i];  const b = pts[(i + 1) % size(pts)];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], -a[1]) * millimeter, "end" : vector(b[0], -b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : (y1 - y0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// pad shell around zc (untilted): plain curved plate, cover groove with the snap undercut, one dovetail rail with the pad screw
export function buildPadShell(context is Context, id is Id, xs is number, zc is number) returns Query
{
    planStrip(context, id + "st", xs, 0, SHELL_T, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "ol", padOutlineYZ(zc, 0), xs - 80, xs + 20);
    intersectInto(context, id + "x", solids(context, id + "ol"), solids(context, id + "st"));
    const plate = one(context, solids(context, id + "ol"), "pad plate");
    const xb = shellXi(PS_Y, xs) + SHELL_T;
    dovetail(context, id + "dv", xb, zc, DV_FRONT + 0.3, FOOT_Y[1], DV[0], DV[1], DV[2]);
    opBoolean(context, id + "u", { "tools" : qUnion([solids(context, id + "ol"), solids(context, id + "dv")]), "operationType" : BooleanOperationType.UNION });
    one(context, qUnion([solids(context, id + "ol"), solids(context, id + "dv")]), "pad shell");
    const body = qBodyType(qUnion([qCreatedBy(id + "ol", EntityType.BODY), qCreatedBy(id + "dv", EntityType.BODY)]), BodyType.SOLID);
    const K = id + "cut";
    band(context, K + "g", xs, zc, GROOVE[0], GROOVE[1], SHELL_T - GROOVE_D, SHELL_T + 1);
    band(context, K + "u", xs, zc, RING[1] - 0.2, UNDERCUT, SHELL_T - GROOVE_D, SHELL_T - GROOVE_D + 1.0);    // snap undercut
    // pad screw: M5 x 10 button head from the child side (counterbore under the foam) through the rail into the foot's nut
    cylAx(context, K + "ph", vector(xb - SHELL_T - 1, PS_Y, zc), vector(xb + DV[2] + 1, PS_Y, zc), M5_CLEAR / 2);
    cylAx(context, K + "cb", vector(xb - SHELL_T - 1, PS_Y, zc), vector(xb - SHELL_T + BH_K, PS_Y, zc), BH_R);
    opBoolean(context, id + "cutop", { "tools" : solids(context, K), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    const pb = evaluateQuery(context, body);
    if (size(pb) != 1)
    {
        var info2 = "";
        for (var b in pb) info2 = info2 ~ " vol " ~ r1(evVolume(context, { "entities" : b }) / millimeter ^ 3) ~ " " ~ bboxStr(context, b);
        throw regenError("pad shell after cuts: " ~ size(pb) ~ " bodies:" ~ info2);
    }
    // rail ends: round the free back end (front end butts on the groove's closed front)
    return body;
}
export function buildClipRing(context is Context, id is Id, xs is number, zc is number) returns Query
{
    band(context, id + "b", xs, zc, RING[0], RING[1], SHELL_T - RING[2], SHELL_T);
    band(context, id + "s", xs, zc, RING[0] + 0.6, RING[1] + BARB, SHELL_T - GROOVE_D + 0.15, SHELL_T - GROOVE_D + 0.85);   // snap barb
    opBoolean(context, id + "u", { "tools" : qUnion([solids(context, id + "b" + "o"), solids(context, id + "s" + "o")]), "operationType" : BooleanOperationType.UNION });
    return one(context, qUnion([solids(context, id + "b" + "o"), solids(context, id + "s" + "o")]), "clip ring");
}
// reference foam + cover (not printed)
export function buildCushion(context is Context, id is Id, xs is number, zc is number, foam is number) returns map
{
    planStrip(context, id + "fs", xs, -foam, 0.0, zc - PAD_H / 2 - 4, zc + PAD_H / 2 + 4);
    yzPrism(context, id + "fo", padOutlineYZ(zc, 0.5), xs - 80, xs + 20);
    intersectInto(context, id + "f", solids(context, id + "fo"), solids(context, id + "fs"));
    const fm = one(context, solids(context, id + "fo"), "foam");
    // cover: 3 mm skin over the foam face, round the edges and onto the back face up to the groove
    planStrip(context, id + "cs", xs, -foam - COVER_T, SHELL_T - GROOVE_D + 0.4, zc - PAD_H / 2 - 6, zc + PAD_H / 2 + 6);
    yzPrism(context, id + "co", padOutlineYZ(zc, -COVER_T + 0.5), xs - 80, xs + 20);
    intersectInto(context, id + "c", solids(context, id + "co"), solids(context, id + "cs"));
    const cv = one(context, solids(context, id + "co"), "cover");
    var rim = [];
    for (var e in evaluateQuery(context, qOwnedByBody(cv, EntityType.EDGE)))
        if (evBox3d(context, { "topology" : e }).maxCorner[0] < (shellXi(10, xs) - foam / 2) * millimeter) rim = append(rim, e);
    finishEdges(context, id + "r", qUnion(rim), min(10.0, foam * 0.8 + 2), "cover rim round");
    // hollow it: everything inside the cover that the shell + foam occupy
    planStrip(context, id + "hs", xs, -foam, SHELL_T + 2, zc - PAD_H / 2 - 6, zc + PAD_H / 2 + 6);
    yzPrism(context, id + "ho", padOutlineYZ(zc, 0), xs - 80, xs + 20);
    intersectInto(context, id + "h", solids(context, id + "ho"), solids(context, id + "hs"));
    opBoolean(context, id + "hc", { "tools" : solids(context, id + "ho"), "targets" : cv, "operationType" : BooleanOperationType.SUBTRACTION });
    return { "foam" : fm, "cover" : cv };
}

// =============================================== DEPTH PIECE ===============================================
export function tiltDeg(t is SidePadTilt) returns number
{
    return t == SidePadTilt.IN8 ? 8 : (t == SidePadTilt.OUT8 ? -8 : (t == SidePadTilt.IN15 ? 15 : (t == SidePadTilt.OUT15 ? -15 : 0)));
}
// rotation of the pad about the y axis through (XS, zp): top-in (positive deg) moves the top toward the child (-x) on the LEFT side
function padRot(zp is number, deg is number) returns Transform
{
    return rotationAround(line(v3(XS, 0, zp), vector(0, 1, 0)), -deg * degree);
}
export function buildDepth(context is Context, id is Id, zp is number, xe is number, deg is number) returns Query
{
    const fw = FOOT_Y[1] - FOOT_Y[0];  const fyc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
    const x0 = xe - JGAP;
    // ONE loft from the hairline (arm skin) through the trumpet into the foot; the foot continues past the pad back (trimmed)
    rrLoft(context, id + "fl", [sec(vector(x0, YAX, zp), vector(-1, 0, 0), ARM_W, ARM_H, ARM_R),
                                sec(vector(XS + FLARE_L, YAX, zp), vector(-1, 0, 0), ARM_W, ARM_H, ARM_R),
                                sec(vector(XS + 0.45 * FLARE_L, lerp(YAX, fyc, 0.45), zp), vector(-1, 0, 0), lerp(ARM_W, fw, 0.5), lerp(ARM_H, 2 * FOOT_HZ, 0.6), lerp(ARM_W, fw, 0.5) / 2),
                                sec(vector(XS + 1.5, fyc, zp), vector(-1, 0, 0), fw, 2 * FOOT_HZ, FOOT_R),
                                sec(vector(XS - 9.0, fyc, zp), vector(-1, 0, 0), fw, 2 * FOOT_HZ, FOOT_R)], true, false);
    const body = oneBody(context, id, "depth piece");
    const K = id + "cut";
    // tools in the PAD frame (untilted), then rotated with the pad: pad back trim, 2 dovetail grooves (closed front), nut recesses
    const P = id + "pf";
    planStrip(context, P + "pb", XS, -60, SHELL_T, zp - 60, zp + 60);
    const xb = shellXi(PS_Y, XS) + SHELL_T;
    for (var s in [1, -1])
    {
        const zr = zp + s * PAD_OFF;
        // groove flank = rail flank (slope (wt - wr) / 2h) offset DV_CL; from 1 mm outside the face to 0.1 past the rail top
        const sl = (DV[1] - DV[0]) / (2 * DV[2]);
        dovetail(context, P + (s > 0 ? "gu" : "gl"), xb - 1.0, zr, DV_FRONT, FOOT_Y[1] + 5, 2 * (DV[0] / 2 + DV_CL - sl * 1.0), 2 * (DV[0] / 2 + DV_CL + sl * (DV[2] + 0.1)), DV[2] + 1.1);
        // M5 nut dropped through the groove mouth (flats across z) into a pocket under the groove bottom; the pad screw
        // (M5 x 10 button head from the child side) clamps the rail between its head and this nut: the screw only locks y
        hexPrism(context, P + (s > 0 ? "nu" : "nl"), vector(xb + DV[2] + 2.5, PS_Y, zr), vector(1, 0, 0), M5_NUT_AF, 5.0);
        cylAx(context, P + (s > 0 ? "hu" : "hl"), vector(xb, PS_Y, zr), vector(xb + 9.0, PS_Y, zr), M5_CLEAR / 2);
    }
    const rot = padRot(zp, deg);
    opTransform(context, P + "rot", { "bodies" : solids(context, P), "transform" : rot });
    // sleeve for the tenon, ending at the tenon tip: its end face is the hidden step (outward push -> plastic, not the screw)
    rrLoft(context, K + "sl", [sec(vector(x0 + 1, YAX, zp), vector(-1, 0, 0), TEN_W + 2 * CL, TEN_H + 2 * CL, TEN_R + CL), sec(vector(xe - TEN_L - STEP_GAP, YAX, zp), vector(-1, 0, 0), TEN_W + 2 * CL, TEN_H + 2 * CL, TEN_R + CL)], false, false);
    // joint screw: M5 x 12 countersunk from the back face (+y) into the tenon's nut
    const xj = xe - JS_DX;  const yb = YAX + ARM_W / 2;
    cylAx(context, K + "js", vector(xj, yb + 1, zp), vector(xj, YAX - 3.0, zp), M5_CLEAR / 2);
    fCone(context, K + "jsk", { "bottomCenter" : v3(xj, yb - (CSK_M5_R - 2.75), zp), "topCenter" : v3(xj, yb + 0.05, zp), "bottomRadius" : 2.75 * millimeter, "topRadius" : CSK_M5_R * millimeter });
    opBoolean(context, id + "cutop", { "tools" : qUnion([solids(context, K), solids(context, P)]), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return body;
}
// keep the part of q with x <= x1 (subtract the half-space x > x1 ... x1 + w)
function keepOnlyX(context is Context, id is Id, q is Query, x1 is number, w is number)
{
    fCuboid(context, id + "b", { "corner1" : v3(x1, -400, -400), "corner2" : v3(x1 + w, 400, 800) });
    opBoolean(context, id + "c", { "tools" : qCreatedBy(id + "b", EntityType.BODY), "targets" : q, "operationType" : BooleanOperationType.SUBTRACTION });
}

// =============================================== PART NUMBERS (scheme §8, awaiting owner OK) ===============================================
function sideL(s is number) returns string { return s > 0 ? "L" : "R"; }
function armCode(drop is number) returns string { return drop < 30 ? "S" : "L"; }
function flipCode(zs is number) returns string { return zs > 0 ? "LO" : "HI"; }
// depth piece tilt code: A = left top-in = right top-out; B = its mirror
function tiltCode(deg is number, s is number) returns string
{
    if (deg == 0) return "T0";
    const a = (deg > 0) == (s > 0);
    return "T" ~ abs(deg) ~ (a ? "A" : "B");
}
function foamMM(f is SidePadFoam) returns number { return f == SidePadFoam.F5 ? 5 : (f == SidePadFoam.F10 ? 10 : (f == SidePadFoam.F20 ? 20 : 30)); }

// =============================================== ONE SIDE OF THE KIT ===============================================
// cfg: s (+1 left / -1 right), drop, zs, padOff (+11 / -11), deg (top-in +), foam, cushion, carbon, engrave, cap (the blank cap
// of that side, used as the nose source and deleted). Built on the left and mirrored for the right. Returns named bodies.
function capPoint(s is number) returns Vector { const p = QS(0, -8, C); return vector(s * p[0], p[1], p[2]); }
function findCap(context is Context, s is number) returns Query
{
    const hit = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), capPoint(s) * millimeter));
    if (size(hit) == 1 && evVolume(context, { "entities" : hit[0] }) > 40 * centimeter ^ 3) throw regenError("Side pad r5: no blank cap at the " ~ (s > 0 ? "left" : "right") ~ " nose (the wing feature failed or is missing)");
    if (size(hit) != 1) throw regenError("Side pad r5: the " ~ (s > 0 ? "left" : "right") ~ " blank cap (made by 'GbG Side Pad Wing (chair)') was not found (" ~ size(hit) ~ " bodies at the nose). Put the wing feature before this one.");
    return hit[0];
}
export function buildSide(context is Context, id is Id, cfg is map) returns map
{
    const s = cfg.s;
    var src = cfg.cap;
    if (s < 0)
    {
        src = copyOf(context, id + "srcm", cfg.cap);
        opTransform(context, id + "srcmx", { "bodies" : src, "transform" : mirX() });
    }
    const car = buildCarrier(context, id + "car", src, cfg.drop, cfg.zs, cfg.carbon, cfg.ribs != false);
    if (s < 0) opDeleteBodies(context, id + "dsrc", { "entities" : src });
    const zc = car.zp + cfg.padOff;
    const dp = buildDepth(context, id + "dep", car.zp, car.xe, cfg.deg);
    const pad = buildPadShell(context, id + "pad", XS, zc);
    const ring = buildClipRing(context, id + "ring", XS, zc);
    var soft = {};
    if (cfg.cushion) soft = buildCushion(context, id + "cush", XS, zc, cfg.foam);
    var padSet = [pad, ring];
    if (cfg.cushion) padSet = concatenateArrays([padSet, [soft.foam, soft.cover]]);
    const rot = padRot(car.zp, cfg.deg);
    if (cfg.deg != 0) opTransform(context, id + "prot", { "bodies" : qUnion(padSet), "transform" : rot });
    const S = sideL(s);
    const nm = {
        "carrier" : "GBG-SP-AR-" ~ S ~ "-" ~ armCode(cfg.drop) ~ "-" ~ flipCode(cfg.zs) ~ "-r5 carrier",
        "button" : "GBG-SP-BT-" ~ S ~ "-r5 button-catch",
        "sleeve" : "GBG-SP-TS-r5 tell sleeve (orange)",
        "depth" : "GBG-SP-DP-D0-" ~ tiltCode(cfg.deg, s) ~ "-r5 depth piece",
        "pad" : "GBG-SP-PD-r5 pad shell",
        "ring" : "GBG-SP-RG-r5 clip ring" };
    var jobs = [];
    if (cfg.engrave)
    {
        // carrier: nose end face opposite the latch's drop-in openings (LOW: bottom, HIGH: top); hidden in the notch
        const ez = NZ[0] + SEAM;
        jobs = append(jobs, [car.carrier, QS(7.5, -6.5, ez), dirD(), vector(0, 0, -1), "AR-" ~ S ~ "-" ~ armCode(cfg.drop) ~ "-" ~ flipCode(cfg.zs) ~ " r5", 3.0]);
        // button: inner face of the block (d' 13), hidden inside the carrier
        const bz = BLK_Z;
        jobs = append(jobs, [car.button, QS(BLK_D[0], 2.8, (bz[0] + bz[1]) / 2), dirT(), -dirD(), "BT-" ~ S, 2.2]);
        // depth piece: foot face between the two dovetail grooves (against the pad back)
        const xb = shellXi(PS_Y, XS) + SHELL_T;
        var o = vector(xb, (DV_FRONT + FOOT_Y[1]) / 2 + 1, car.zp);  var n = vector(-1, 0, 0);
        if (cfg.deg != 0) { o = (rot * (o * millimeter)) / millimeter;  n = rot.linear * n; }
        jobs = append(jobs, [dp, o, vector(0, 1, 0), n, "DP-D0-" ~ tiltCode(cfg.deg, s) ~ " r5", 2.6]);
        // pad: child face (under the foam)
        var op = vector(shellXi(10, XS), 10, zc);  var np = vector(-1, 0, 0);
        if (cfg.deg != 0) { op = (rot * (op * millimeter)) / millimeter;  np = rot.linear * np; }
        jobs = append(jobs, [pad, op, vector(0, 1, 0), np, "PD r5", 5.0]);
    }
    var out = { "carrier" : car.carrier, "button" : car.button, "sleeve" : car.sleeve, "depth" : dp, "pad" : pad, "ring" : ring, "rodL" : car.rodL, "rodWall" : car.rodWall, "xe" : car.xe, "zp" : car.zp, "zc" : zc, "carbon" : car.carbon, "names" : nm };
    if (cfg.cushion) { out.foam = soft.foam;  out.cover = soft.cover; }
    var all = [car.carrier, car.button, car.sleeve, dp, pad, ring];
    all = concatenateArrays([all, car.carbon]);
    if (cfg.cushion) all = concatenateArrays([all, [soft.foam, soft.cover]]);
    if (s < 0) opTransform(context, id + "mir", { "bodies" : qUnion(all), "transform" : mirX() });
    for (var i = 0; i < size(jobs); i += 1)
    {
        const j = jobs[i];
        const m = s < 0 ? vector(-1, 1, 1) : vector(1, 1, 1);
        engrave(context, id + ("e" ~ i), j[0], vector(m[0] * j[1][0], j[1][1], j[1][2]), vector(m[0] * j[2][0], j[2][1], j[2][2]), vector(m[0] * j[3][0], j[3][1], j[3][2]), j[4], j[5], 0.4);
    }
    opDeleteBodies(context, id + "dcap", { "entities" : cfg.cap });
    out.all = all;
    return out;
}
function nameSide(context is Context, r is map, foam is number)
{
    for (var k in ["carrier", "button", "sleeve", "depth", "pad", "ring"]) { stage(context, "name " ~ k ~ " n=" ~ size(evaluateQuery(context, r[k]))); name(context, r[k], r.names[k]); }
    stage(context, "carbon");
    paint(context, r.sleeve, color(1.0, 0.42, 0.16));
    if (size(r.carbon) > 0)
    {
        name(context, r.carbon[0], "GBG-SP-CF6-r5 carbon rod Ø6 x " ~ r.rodL);
        for (var i = 1; i < size(r.carbon); i += 1) name(context, r.carbon[i], "GBG-SP-CF2-r5 carbon pin Ø2 x " ~ PIN_L);
        paint(context, qUnion(r.carbon), color(0.15, 0.16, 0.17));
    }
    if (r.foam != undefined)
    {
        name(context, r.foam, "GBG-SP-FM-" ~ foam ~ "-r5 foam " ~ foam ~ " mm (reference, not printed)");
        name(context, r.cover, "GBG-SP-CV-r5 neoprene cover 3 mm (reference, not printed)");
        paint(context, r.foam, color(0.85, 0.85, 0.55));
        paint(context, r.cover, color(0.13, 0.17, 0.30));
    }
}

// =============================================== CHECK ===============================================
function clashes(context is Context, names is array, kit is array, others is array) returns string
{
    var msg = "";
    for (var i = 0; i < size(kit); i += 1)
        for (var j = i + 1; j < size(kit); j += 1)
        {
            var hit = false;
            for (var cc in evCollision(context, { "tools" : kit[i], "targets" : kit[j] })) if (cc["type"] == ClashType.INTERFERE) hit = true;
            if (hit) msg = msg ~ names[i] ~ " x " ~ names[j] ~ ", ";
        }
    if (size(others) > 0)
        for (var i = 0; i < size(kit); i += 1)
        {
            for (var cc in evCollision(context, { "tools" : kit[i], "targets" : qUnion(others) }))
                if (cc["type"] == ClashType.INTERFERE)
                {
                    msg = msg ~ names[i] ~ " x chair" ~ bboxStr(context, cc["targetBody"]) ~ ", ";
                    break;
                }
        }
    return msg;
}
function checkAll(context is Context, id is Id, definition is map, s is number) returns string
{
    var out = "";  var nBad = 0;  var maxX = 0;  var n = 0;  var rods = "";
    const capQ = findCap(context, s);
    const others = evaluateQuery(context, qSubtraction(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), capQ));
    for (var drop in DROPS) for (var zs in [1, -1]) for (var po in [PAD_OFF, -PAD_OFF]) for (var deg in [0, 8, -8, 15, -15])
    {
        const sid = id + ("chk" ~ n);  n += 1;
        var msg = "";
        startFeature(context, sid, definition);
        try
        {
            const cap = findCap(context, s);
            const r = buildSide(context, sid + "k", { "s" : s, "drop" : drop, "zs" : zs, "padOff" : po, "deg" : deg, "foam" : 30, "cushion" : true, "carbon" : false, "engrave" : false, "ribs" : false, "cap" : cap });
            const kit = [r.carrier, r.button, r.sleeve, r.depth, r.pad, r.ring, r.foam, r.cover];
            const names = ["carrier", "button", "sleeve", "depth", "pad", "ring", "foam", "cover"];
            for (var b in kit) maxX = max(maxX, abs(s > 0 ? evBox3d(context, { "topology" : b }).maxCorner[0] / millimeter : evBox3d(context, { "topology" : b }).minCorner[0] / millimeter));
            msg = clashes(context, names, kit, others);
            // the pressed button (travel 3.8) must stay clear of the chair
            opTransform(context, sid + "press", { "bodies" : r.button, "transform" : transform(vector(s * dirD()[0], dirD()[1], 0) * (-TRAVEL) * millimeter) });
            for (var cc in evCollision(context, { "tools" : r.button, "targets" : qUnion(others) })) if (cc["type"] == ClashType.INTERFERE) { msg = msg ~ "PRESSED button x chair, ";  break; }
            rods = rods == "" ? toString(r1(r.rodWall)) : toString(r1(min(r.rodWall, stringToNumber(rods))));
        }
        catch (e) { msg = "BUILD FAILED " ~ toString(e); }
        abortFeature(context, sid);
        if (msg != "") { nBad += 1; out = out ~ armCode(drop) ~ "-" ~ flipCode(zs) ~ "-" ~ (po > 0 ? "upper" : "lower") ~ "-" ~ deg ~ ": " ~ msg ~ "; "; }
    }
    return sideL(s) ~ ": " ~ n ~ " settings, " ~ nBad ~ " with clashes; kit |x| max " ~ r1(maxX) ~ " (arm rest 165); min rod wall " ~ rods ~ (out == "" ? "" : " -- " ~ out);
}

// =============================================== LAYOUTS ===============================================
// move a body so that its bounding box sits with min corner at (x, y, 0); optional rotation first
function place(context is Context, id is Id, b is Query, x is number, y is number, pre is Transform) returns number
{
    opTransform(context, id + "r", { "bodies" : b, "transform" : pre });
    const bb = evBox3d(context, { "topology" : b });
    const mn = bb.minCorner / millimeter;  const mx = bb.maxCorner / millimeter;
    opTransform(context, id + "t", { "bodies" : b, "transform" : transform(vector(x - mn[0], y - mn[1], -mn[2]) * millimeter) });
    return mx[0] - mn[0];
}
// rotation that turns direction `from` to `to` (unit vectors)
function turn(from is Vector, to is Vector) returns Transform
{
    const a = normalize(from);  const b = normalize(to);
    const c = cross(a, b);
    if (norm(c) < 1e-9) return dot(a, b) > 0 ? identityTransform() : rotationAround(line(v3(0, 0, 0), perpendicularVector(a)), 180 * degree);
    return rotationAround(line(v3(0, 0, 0), normalize(c)), acos(max(-1, min(1, dot(a, b)))));
}
function deleteChairExcept(context is Context, id is Id, keep is array)
{
    const del = qSubtraction(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), qUnion(keep));
    if (size(evaluateQuery(context, del)) > 0) opDeleteBodies(context, id, { "entities" : del });
}

// =============================================== FEATURES ===============================================
annotation { "Feature Type Name" : "GbG Side Pad Wing (chair)", "Feature Type Description" : "Round 5 wing reprint: 40 mm nose notch (z 207-247) with the tongue slot and catch groove; the cut-out piece becomes the blank cap. Needed before 'GbG Side Pad r5'." }
export const gbgSidePadWing = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Remove the modelled wing print supports (support_left/right_wing)", "Default" : true }
        definition.dropSupports is boolean;
        annotation { "Name" : "Engrave the part number in the notch ceiling", "Default" : true }
        definition.engrave is boolean;
    }
    {
        setVariable(context, "gbgSkipped", "");
        if (definition.dropSupports)
        {
            var sup = [];
            for (var b in evaluateQuery(context, qBodyType(qEverything(EntityType.BODY), BodyType.SOLID)))
            {
                var isPlate = false;
                try silent
                {
                    const bb = evBox3d(context, { "topology" : b });
                    const lo = bb.minCorner / millimeter;  const hi = bb.maxCorner / millimeter;
                    const v = evVolume(context, { "entities" : b }) / (centimeter ^ 3);
                    isPlate = v < 6 && min(abs(lo[0]), abs(hi[0])) > 95 && max(abs(lo[0]), abs(hi[0])) < 145 && lo[1] > 40 && hi[1] < 110 && lo[2] > 255 && hi[2] < 315;
                }
                if (isPlate) sup = append(sup, b);
            }
            if (size(sup) > 0) opDeleteBodies(context, id + "dropSup", { "entities" : qUnion(sup) });
        }
        var info = "";
        for (var s in [1, -1])
        {
            const sid = id + (s > 0 ? "L" : "R");
            const probe = Q(9, 20, 300);
            const hit = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * probe[0], probe[1], probe[2])));
            if (size(hit) != 1) throw regenError("Side Pad Wing: could not find the " ~ (s > 0 ? "left" : "right") ~ " wing (found " ~ size(hit) ~ " bodies)");
            const wing = hit[0];
            var src = wing;
            if (s < 0)
            {
                src = copyOf(context, sid + "src", wing);
                opTransform(context, sid + "srcx", { "bodies" : src, "transform" : mirX() });
            }
            var t = {};
            try { t = wingTools(context, sid + "t", src, true); }
            catch (e) { throw regenError("Side Pad Wing failed at " ~ stageOf(context) ~ ": " ~ toString(e)); }
            if (s < 0)
            {
                opDeleteBodies(context, sid + "dsrc", { "entities" : src });
                opTransform(context, sid + "tm", { "bodies" : qUnion([t.fill, t.cut, t.cap]), "transform" : mirX() });
            }
            try { opBoolean(context, sid + "add", { "tools" : qUnion([wing, t.fill]), "operationType" : BooleanOperationType.UNION }); }
            catch (e) { throw regenError("Side Pad Wing: fill union failed (" ~ size(evaluateQuery(context, t.fill)) ~ " fill bodies)"); }
            const w2 = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * probe[0], probe[1], probe[2])));
            if (size(w2) != 1) throw regenError("Side Pad Wing: merged wing not found");
            const cnames = ["notch", "slot", "groove", "mouth chamfer"];
            for (var ci = 0; ci < 4; ci += 1)
            {
                try { opBoolean(context, sid + ("cut" ~ ci), { "tools" : t.cuts[ci], "targets" : w2[0], "operationType" : BooleanOperationType.SUBTRACTION }); }
                catch (e) { throw regenError("Side Pad Wing: " ~ cnames[ci] ~ " cut failed (" ~ size(evaluateQuery(context, t.cuts[ci])) ~ " tool bodies)"); }
            }
            const nb = size(evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * probe[0], probe[1], probe[2]))));
            if (nb != 1) throw regenError("Side Pad Wing: the wing split into pieces");
            const S = sideL(s);
            name(context, w2[0], (s > 0 ? "left_wing" : "right_wing") ~ " (reprint GBG-SP-WG-" ~ S ~ "-r5)");
            name(context, t.cap, "GBG-SP-CP-" ~ S ~ "-r5 blank cap");
            if (definition.engrave)
            {
                const o = QS(7.5, -6.5, NZ[1]);
                const n = vector(0, 0, -1);
                engrave(context, sid + "eW", w2[0], vector(s * o[0], o[1], o[2]), vector(s * dirD()[0], dirD()[1], 0), n, "WG-" ~ S ~ " r5", 3.0, 0.4);
                const oc = QS(7.5, -6.5, NZ[0] + SEAM);
                engrave(context, sid + "eC", t.cap, vector(s * oc[0], oc[1], oc[2]), vector(s * dirD()[0], dirD()[1], 0), vector(0, 0, -1), "CP-" ~ S ~ " r5", 3.0, 0.4);
            }
        }
        var msg = "";
        try silent { msg = getVariable(context, "gbgSkipped"); }
        if (msg != "") reportFeatureWarning(context, id, "Finish skipped: " ~ msg);
        else { var lmsg = ""; try silent { lmsg = getVariable(context, "gbgLean"); } reportFeatureInfo(context, id, lmsg); }
    }, { "dropSupports" : true, "engrave" : true });

function dropOf(a is SidePadArm) returns number { return a == SidePadArm.SHORT ? DROPS[0] : DROPS[1]; }
function zsOf(k is SidePadKit) returns number { return k == SidePadKit.LOW ? 1 : -1; }
function offOf(p is SidePadPadPos) returns number { return p == SidePadPadPos.UPPER ? PAD_OFF : -PAD_OFF; }

annotation { "Feature Type Name" : "GbG Side Pad r5", "Feature Type Description" : "Round 5 side-pad kit (carrier with nose + arm, latch A, depth piece with tilt foot, pad, clip ring, reference foam + cover). Needs 'GbG Side Pad Wing (chair)' before it." }
export const gbgSidePadV1 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Side" }
        definition.side is SidePadSide;
        annotation { "Name" : "Kit" }
        definition.mode is SidePadMode;
        annotation { "Name" : "Left: arm" }
        definition.arm is SidePadArm;
        annotation { "Name" : "Left: LOW / HIGH" }
        definition.kit is SidePadKit;
        annotation { "Name" : "Left: pad position" }
        definition.padPos is SidePadPadPos;
        annotation { "Name" : "Left: tilt" }
        definition.tilt is SidePadTilt;
        annotation { "Name" : "Right: arm" }
        definition.rArm is SidePadArm;
        annotation { "Name" : "Right: LOW / HIGH" }
        definition.rKit is SidePadKit;
        annotation { "Name" : "Right: pad position" }
        definition.rPadPos is SidePadPadPos;
        annotation { "Name" : "Right: tilt" }
        definition.rTilt is SidePadTilt;
        annotation { "Name" : "Foam (reference cushion)" }
        definition.foam is SidePadFoam;
        annotation { "Name" : "Show foam + cover (reference, not printed)", "Default" : true }
        definition.cushion is boolean;
        annotation { "Name" : "Show carbon rod + pins", "Default" : true }
        definition.carbon is boolean;
        annotation { "Name" : "Engrave part numbers", "Default" : true }
        definition.engrave is boolean;
        annotation { "Name" : "View" }
        definition.view is SidePadView;
        annotation { "Name" : "Check every setting against the chair (slow; builds nothing)" }
        definition.checkSet is SidePadCheck;
    }
    {
        setVariable(context, "gbgSkipped", "");
        if (definition.checkSet != SidePadCheck.OFF)
        {
            reportFeatureInfo(context, id, "CHECK " ~ checkAll(context, id, definition, definition.checkSet == SidePadCheck.LEFT ? 1 : -1));
            return;
        }
        const foam = foamMM(definition.foam);
        var info = "";
        if (definition.view == SidePadView.FITTED)
        {
            if (definition.mode != SidePadMode.ON)
            {
                if (definition.mode == SidePadMode.OFF_OPEN)
                    for (var s in [1, -1]) if ((s > 0 && definition.side != SidePadSide.RIGHT) || (s < 0 && definition.side != SidePadSide.LEFT))
                        opDeleteBodies(context, id + ("dc" ~ (s > 0 ? "L" : "R")), { "entities" : findCap(context, s) });
                return;
            }
            for (var s in [1, -1])
            {
                if ((s > 0 && definition.side == SidePadSide.RIGHT) || (s < 0 && definition.side == SidePadSide.LEFT)) continue;
                const cfg = s > 0 ? { "drop" : dropOf(definition.arm), "zs" : zsOf(definition.kit), "padOff" : offOf(definition.padPos), "deg" : tiltDeg(definition.tilt) }
                                  : { "drop" : dropOf(definition.rArm), "zs" : zsOf(definition.rKit), "padOff" : offOf(definition.rPadPos), "deg" : tiltDeg(definition.rTilt) };
                var r = {};
                try { r = buildSide(context, id + (s > 0 ? "L" : "R"), mergeMaps(cfg, { "s" : s, "foam" : foam, "cushion" : definition.cushion, "carbon" : definition.carbon, "engrave" : definition.engrave, "cap" : findCap(context, s) })); }
                catch (e) { throw regenError("Side pad r5 failed at " ~ stageOf(context) ~ ": " ~ toString(e)); }
                try { nameSide(context, r, foam); }
                catch (e) { throw regenError("Side pad r5 naming failed at " ~ stageOf(context) ~ ": " ~ toString(e)); }
                var tgr = "";  try silent { tgr = getVariable(context, "gbgTgRoot"); }
                info = info ~ sideL(s) ~ ": pad centre z " ~ r1(r.zc) ~ ", top " ~ r1(r.zc + PAD_H / 2) ~ "; rod Ø6 x " ~ r.rodL ~ " (wall " ~ r1(r.rodWall) ~ "); " ~ tgr ~ ". ";
            }
        }
        else if (definition.view == SidePadView.PRINT) info = printLayout(context, id, definition, foam);
        else info = allParts(context, id, definition, foam);
        var msg = "";
        try silent { msg = getVariable(context, "gbgSkipped"); }
        if (msg != "") reportFeatureWarning(context, id, "Finish skipped: " ~ msg);
        else reportFeatureInfo(context, id, info);
    }, { "side" : SidePadSide.BOTH, "mode" : SidePadMode.ON, "arm" : SidePadArm.LONG, "kit" : SidePadKit.LOW, "padPos" : SidePadPadPos.LOWER, "tilt" : SidePadTilt.T0,
         "rArm" : SidePadArm.SHORT, "rKit" : SidePadKit.HIGH, "rPadPos" : SidePadPadPos.UPPER, "rTilt" : SidePadTilt.T0,
         "foam" : SidePadFoam.F20, "cushion" : true, "carbon" : true, "engrave" : true, "view" : SidePadView.FITTED, "checkSet" : SidePadCheck.OFF });

// print layout: the left side's printable parts (current left settings) in print orientation + the latch coupon, chair removed
function printLayout(context is Context, id is Id, definition is map, foam is number) returns string
{
    const capL = findCap(context, 1);
    // coupon wing block: the reprinted left wing around the notch
    const wingQ = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), Q(9, 20, 300) * millimeter));
    if (size(wingQ) != 1) throw regenError("print layout: left wing not found");
    const blk = copyOf(context, id + "cwb", wingQ[0]);
    keepOnly(context, id + "cwk", blk, -10, 30, -16, 30, NZ[0] - 8, NZ[1] + 8, true);
    const capCopy = copyOf(context, id + "cpc", capL);
    const r = buildSide(context, id + "L", { "s" : 1, "drop" : dropOf(definition.arm), "zs" : zsOf(definition.kit), "padOff" : offOf(definition.padPos), "deg" : tiltDeg(definition.tilt), "foam" : foam, "cushion" : false, "carbon" : false, "engrave" : definition.engrave, "cap" : capL });
    nameSide(context, r, foam);
    // coupon: the carrier cut to the nose + tongue (+ button and sleeve copies)
    const cs = copyOf(context, id + "cs", r.carrier);
    keepOnly(context, id + "csk", cs, -12, 30, -20, 30, NZ[0] - 2, NZ[1] + 2, true);
    const cb = copyOf(context, id + "cbt", r.button);
    const csl = copyOf(context, id + "csl", r.sleeve);
    name(context, cs, "C7 latch coupon: carrier stub");  name(context, cb, "C7 latch coupon: button");  name(context, csl, "C7 latch coupon: tell sleeve");
    name(context, blk, "C7 latch coupon: wing block (notch, slot, groove)");  name(context, capCopy, "GBG-SP-CP-L-r5 blank cap");
    deleteChairExcept(context, id + "dch", [r.carrier, r.button, r.sleeve, r.depth, r.pad, r.ring, cs, cb, csl, blk, capCopy]);
    // print orientations: carrier + cap + stub + block on the nose's outer face (normal +d' -> down); button standing (as fitted,
    // turned so the block's end face is on the bed); sleeve flat; depth piece on its foot face; pad child side down; ring flat side down
    const outer = normalize(cross(dirT(), leanV() + vector(0, 0, 1)));          // normal of the plane d' = const (S)
    const nOut = dot(outer, dirD()) > 0 ? outer : -outer;
    const down = vector(0, 0, -1);
    var x = 0;  const gap = 25;
    x += place(context, id + "p1", r.carrier, x, 0, turn(nOut, down)) + gap;
    x += place(context, id + "p2", r.button, x, 0, definition.kit == SidePadKit.LOW ? rotationAround(line(v3(0, 0, 0), dirD()), 180 * degree) : identityTransform()) + gap;
    x += place(context, id + "p3", r.sleeve, x, 0, turn(dirD(), vector(0, 0, 1))) + gap;
    const rot = padRot(r.zp, tiltDeg(definition.tilt));
    const footN = rot.linear * vector(-1, 0, 0);
    x += place(context, id + "p4", r.depth, x, 0, turn(footN, down)) + gap;
    x += place(context, id + "p5", r.pad, x, 0, turn(rot.linear * vector(-1, 0, 0), down)) + gap;
    x += place(context, id + "p6", r.ring, x, 0, turn(rot.linear * vector(1, 0, 0), down)) + gap;
    x += place(context, id + "p7", capCopy, x, 0, turn(nOut, down)) + gap;
    var x2 = 0;
    x2 += place(context, id + "q1", cs, x2, -120, turn(nOut, down)) + gap;
    x2 += place(context, id + "q2", blk, x2, -120, turn(nOut, down)) + gap;
    x2 += place(context, id + "q3", cb, x2, -120, definition.kit == SidePadKit.LOW ? rotationAround(line(v3(0, 0, 0), dirD()), 180 * degree) : identityTransform()) + gap;
    x2 += place(context, id + "q4", csl, x2, -120, turn(dirD(), vector(0, 0, 1))) + gap;
    return "Print layout: left " ~ armCode(dropOf(definition.arm)) ~ "-" ~ flipCode(zsOf(definition.kit)) ~ ", " ~ tiltCode(tiltDeg(definition.tilt), 1) ~ "; front row = coupon C7.";
}
// every part and variant in a grid (as-fitted orientation), named by part number
function allParts(context is Context, id is Id, definition is map, foam is number) returns string
{
    const wingL = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), Q(9, 20, 300) * millimeter));
    const pR = Q(9, 20, 300);
    const wingR = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(-pR[0], pR[1], pR[2])));
    if (size(wingL) != 1 || size(wingR) != 1) throw regenError("all parts: wings not found");
    const capL = findCap(context, 1);  const capR = findCap(context, -1);
    var rows = [[wingL[0], wingR[0], capL, capR]];
    var keep = [wingL[0], wingR[0], capL, capR];
    var carRow = [];  var btRow = [];  var sleeveDone = false;  var rodL = 0;
    var k = 0;
    for (var s in [1, -1]) for (var drop in DROPS) for (var zs in [1, -1])
    {
        const src = copyOf(context, id + ("cap" ~ k), s > 0 ? capL : capR);
        const r = buildSide(context, id + ("s" ~ k), { "s" : s, "drop" : drop, "zs" : zs, "padOff" : PAD_OFF, "deg" : 0, "foam" : foam, "cushion" : false, "carbon" : k == 0, "engrave" : definition.engrave, "cap" : src });
        nameSide(context, r, foam);
        carRow = append(carRow, r.carrier);  keep = append(keep, r.carrier);
        if (drop == DROPS[0]) { btRow = append(btRow, r.button);  keep = append(keep, r.button); } else opDeleteBodies(context, id + ("db" ~ k), { "entities" : r.button });
        if (!sleeveDone) { btRow = append(btRow, r.sleeve);  keep = append(keep, r.sleeve);  sleeveDone = true; } else opDeleteBodies(context, id + ("ds" ~ k), { "entities" : r.sleeve });
        if (k == 0)
        {
            rodL = r.rodL;
            for (var c in r.carbon) keep = append(keep, c);
            rows = append(rows, []);
        }
        // the depth piece, pad and ring of the first build stay (T0); others go
        if (k == 0) { keep = concatenateArrays([keep, [r.depth, r.pad, r.ring]]); setVariable(context, "gbgFirst", r); }
        else opDeleteBodies(context, id + ("dd" ~ k), { "entities" : qUnion([r.depth, r.pad, r.ring]) });
        k += 1;
    }
    const first = getVariable(context, "gbgFirst");
    // tilted depth pieces (left side builds: IN = A, OUT = B)
    var dpRow = [first.depth];
    var j = 0;
    for (var deg in [8, -8, 15, -15])
    {
        const src = copyOf(context, id + ("tc" ~ j), capL);
        const r = buildSide(context, id + ("t" ~ j), { "s" : 1, "drop" : DROPS[0], "zs" : 1, "padOff" : PAD_OFF, "deg" : deg, "foam" : foam, "cushion" : false, "carbon" : false, "engrave" : definition.engrave, "cap" : src });
        name(context, r.depth, r.names.depth);
        opDeleteBodies(context, id + ("td" ~ j), { "entities" : qUnion([r.carrier, r.button, r.sleeve, r.pad, r.ring]) });
        dpRow = append(dpRow, r.depth);  keep = append(keep, r.depth);
        j += 1;
    }
    // reference foams 5 / 10 / 20 / 30 + cover
    var softRow = [first.pad, first.ring];
    for (var f in [5, 10, 20, 30])
    {
        const c = buildCushion(context, id + ("f" ~ f), XS, 300, f);
        name(context, c.foam, "GBG-SP-FM-" ~ f ~ "-r5 foam " ~ f ~ " mm (reference)");
        paint(context, c.foam, color(0.85, 0.85, 0.55));
        if (f == 20) { name(context, c.cover, "GBG-SP-CV-r5 neoprene cover 3 mm (reference)"); paint(context, c.cover, color(0.13, 0.17, 0.30)); softRow = append(softRow, c.cover); keep = append(keep, c.cover); }
        else opDeleteBodies(context, id + ("dcv" ~ f), { "entities" : c.cover });
        softRow = append(softRow, c.foam);  keep = append(keep, c.foam);
    }
    softRow = concatenateArrays([softRow, first.carbon]);
    deleteChairExcept(context, id + "dch", keep);
    rows = [rows[0], carRow, btRow, dpRow, softRow];
    var y = 0;
    for (var ri = 0; ri < size(rows); ri += 1)
    {
        var x = 0;  var h = 0;
        for (var ci = 0; ci < size(rows[ri]); ci += 1)
        {
            const b = rows[ri][ci];
            x += place(context, id + ("g" ~ ri ~ "_" ~ ci), b, x, y, identityTransform()) + 20;
            const bb = evBox3d(context, { "topology" : b });
            h = max(h, (bb.maxCorner[1] - bb.minCorner[1]) / millimeter);
        }
        y -= h + 40;
    }
    return "All parts: " ~ size(keep) ~ " bodies; rod Ø6 x " ~ rodL ~ ", pins Ø2 x " ~ PIN_L;
}

annotation { "Feature Type Name" : "GbG Side Pad latch coupon (r5)", "Feature Type Description" : "C7 coupon for latch A: needs the chair + 'GbG Side Pad Wing (chair)'. Use 'GbG Side Pad r5' with View = Print layout instead." }
export const gbgSidePadCoupon = defineFeature(function(context is Context, id is Id, definition is map)
    precondition { }
    {
        throw regenError("The r5 latch coupon is part of 'GbG Side Pad r5' (View: Print layout + latch coupon) in a Part Studio with the chair and the r5 wing feature.");
    }, {});
