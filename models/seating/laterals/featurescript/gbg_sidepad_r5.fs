FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// ===================================================================================================================
// GoBabyGo SIDE PADS (lateral trunk supports), ROUND 5 production design, for GbG_chair_forth.
// Spec: models/seating/laterals/ROUND5_PLAN.md (AGREED items binding; part numbers per §8, scheme awaiting owner OK).
// Wing-side geometry ported from viewer/r5mesh/gen_r5.py (the mesh study the owner saw); kit from gbg_sidepad_v1.fs v1.1.
//
//   WING (chair change, feature "GbG Side Pad Wing (chair)"): v1.1 tip block/rails/blanks removed. One 40 mm notch in the
//     wing's front edge (z 207-247) with a 16.2 x 32.3 slot, a catch groove (6 deg locking face) in its outer wall behind a
//     15 mm shoulder. Everything of the old mount is rebuilt from the wing's own sections: the old tongue pocket, the slit and
//     recess behind the cheek, the dead M5 pockets and the open pocket above the nose; the cheek continues straight back as
//     a swell (inside the old mount's envelope) that fairs into the blade at t' 36. The piece of wing cut out becomes the
//     BLANK CAP (nose + stub tongue with crush ribs, hidden nail notch).
//   KIT ("GbG Side Pad r5", r5b): carrier = the cap's nose (the wing's own front section) + solid tongue 15.8 x 32.1 + one
//     smooth 22 x 31 arm (short S drop 22, long L drop 44) + hidden 16 x 24 tenon; latch A "deep catch" (button 12 x 24 with
//     a 0.8 crown in the nose, a bar on the tongue top, tooth with 6 deg face at the tongue's rear; spring, split orange
//     tell sleeve, hard stop at 4.0, one-way barb); 4 x Ø2 x 29 carbon tenon pins (the only carbon); depth piece D0 with
//     tilt feet 0 / A8 / B8 / A15 / B15 (foot 27 x 48 R10) and two dovetail grooves (pad positions upper / lower = ±11)
//     closed at the front with a snap notch at the back, hidden step at the tenon tip; pad shell (one dovetail rail with a
//     snap bump, z-symmetric: one part for both sides); clip ring with snap barb; reference foam (5/10/20/30) + 3 mm cover.
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
// tongue (r5b): solid, 15.8 wide (d' 2.0-17.8), back edge 6.25 (normal) from strap slit S1's front face, straight back edge at
// t' 25.48 (the swell behind the cheek gives the outer side >= 5.8 of wall); 1.5 deg wedge on the inner face t' 8.5 -> 16.6
const TONGUE = [[2.0, 1.0], [17.8, 1.0], [17.8, 25.48], [9.0, 25.48], [2.2, 16.57], [2.0, 8.5]];
const SLOT = [[1.8, 1.0], [18.0, 1.0], [18.0, 25.68], [8.901, 25.68], [2.002, 16.64], [1.8, 8.502]];     // tongue + 0.2 everywhere
const PADS = [5.5, 13.5];  const PAD_REL = 0.3;                 // tongue top/bottom relieved 0.3 between t' 5.5 and 13.5: it bears on pads at the mouth and the tip
const RIBS = [12.0, 14.0];  const RIB_R = 0.5;                  // crush ribs r 0.5, 0.3 interference: top/bottom across the tip, and 2 per side face
const MOUTH = 3.3;  const MOUTH_OUT = 0.5;  const TG_ROOT = 3.0;  const TG_ROOT_OUT = 0.5;   // slot mouth chamfer 3.3 x 45 (inner, top, bottom), 0.5 on the outer (latch) side
// swell (wing, inside the old mount's envelope): the cheek's own (d', z) section at t' 9 is carried straight back and trimmed by
// this plan outline (d', t'), which leaves the cheek face d' 24.65 at t' 26.5 and fairs into the blade at t' 36
const SWELL_T = 9.0;
const SWELL = [[7.0, 9.0], [26.0, 9.0], [26.0, 25.5], [24.65, 26.5], [24.41, 27.88], [23.73, 29.05], [22.67, 30.07], [21.26, 31.0], [19.57, 31.87],
               [17.65, 32.76], [15.55, 33.71], [13.31, 34.77], [11.0, 36.0], [10.4, 36.8], [10.6, 34.0], [10.2, 31.0], [9.0, 28.0], [7.0, 24.0]];
const SWELL_Z = [190.0, 292.0];
const SWELL_HOLE = [10.5, 18.2, 216.0, 267.5];                  // the old receiver pocket in that section (d', z), filled
const W1 = [7.3, 17.9, 14.0];                                   // old tongue pocket filled in the band (d0, d1, t1)
const SLIT = [8.0, 26.0, 11.0, 22.0];                           // old slit behind the cheek, closed with the z 215.2 section
const ENV = [-14.0, 34.0, -20.0, 40.0];                         // tip region (d', t') used for the live sections
// ---------------- latch A, r5b "deep catch" (S frame; same for LOW and HIGH) ----------------
// The button and its block live in the nose; a 3 mm bar on the tongue's top carries the tooth to the tongue's rear, where it
// hooks a groove in the wing's outer wall behind a 15 mm solid shoulder (t' 2 .. 17). Pull-out: tooth -> the tongue pocket's
// rear wall (0.1) -> carrier. The tongue section at the mouth stays solid below z 242.7.
const GK = 0.10510423526567646;                                 // tan 6 deg: the locking face
const GR_T = 17.0;  const GR_D = 20.6;  const GR_Z = [215.6, 246.2];   // catch groove: 6 deg front face at t' 17 (at the slot wall d' 18.0), 2.6 deep, skin 4.0 to the outer face
const BT_TC = -6.3;  const BT_W = 12.0;  const BT_H = 24.0;  const BT_THK = 4.6;  const TRAVEL = 4.0;   // button 12 x 24, hard stop at 4.0
const CROWN = 0.8;                                              // convex crown on the button face (rim flush)
const BLK_D = [13.0, 17.4];  const BLK_T = [-4.0, 1.2];  const BLK_Z = [223.5, 246.0];          // block in the nose
const BAR_T = [0.8, 21.0];  const BAR_Z = [241.5, 246.0];       // bar on the tongue top (d = block)
const TO_D = [13.0, 20.4];  const TO_T = [17.02, 21.0];  const TO_Z = [216.0, 245.5];          // tooth: reach 2.4 past the slot wall, 45 deg lead-in at its back
const LIP = 1.6;  const LIP_W = 12.6;  const FL_W = 12.8;  const FL_T = 0.6;  const SL_OUT = 13.4;  const SL_IN = 12.4;  const SL_H = 2.4;  const SL_SPLIT = 1.0;
const SPR = [-8.5, 231.0, 3.25, 10.0];                          // spring slot (t', z, r, d' floor) + 1.0 counterbore: OD 6, d 0.55, L0 16, 8 coils
const PIN_L = 29.0;                                             // Ø2 tenon pins
// ---------------- arm (v1.1 path) ----------------
const TAPER_T = -14.0;  const DC = 9.4;  const YEND = 35.0;
const ARM_W = 22.0;  const ARM_H = 31.0;  const ARM_H_END = 30.0;  const ARM_R = 10.5;  const GROW = 3.0;  const ROOT_R = 2.5;   // arm root round (the 22 x 31 arm leaves 2.5 of nose face on the long arm)
const YAX = YEND + GROW;
const DROPS = [22.0, 44.0];
// ---------------- joint ----------------
const TEN_W = 16.0;  const TEN_H = 24.0;  const TEN_L = 15.0;  const TEN_R = 3.0;  const JS_DX = 9.5;  const JGAP = 0.3;  const STEP_GAP = 0.1;
const TPIN = [[4.5, 9.0], [-4.5, 9.0], [4.5, -9.0], [-4.5, -9.0]];  const TPIN_R = 1.15;  const TPIN_IN = 16.5;  const TPIN_X0 = -13.0;   // pins x_e - 13 .. x_e + 16 (13 in the tenon, 16 in the arm)
// ---------------- depth piece + pad mount ----------------
const XS = 104.0;                                               // D0: pad back x 104 (the one depth piece; width set by the foam)
const PAD_OFF = 11.0;                                           // pad screw positions: pad centre = arm axis +- 11
const FLARE_L = 22.0;  const FOOT_Y = [19.0, 46.0];  const FOOT_HZ = 24.0;  const FOOT_R = 10.0;
const DV = [9.0, 12.0, 2.5];                                    // dovetail rail: root width, top width (z), height
const DV_CL = 0.15;  const DV_FRONT = 22.0;                     // groove clearance per side; groove closed in front of y 22 (the stop)
const PS_Y = 32.5;  const SNAP_Y = [44.3, 45.8];  const SNAP_H = 0.4;     // pad screw y; rail snap bump (y, height) into a groove-floor notch                                              // pad screw y (foot centre)
// ---------------- pad ----------------
const PAD_Y0 = -27.0;  const PAD_Y1 = 55.0;  const PAD_H = 80.0;  const SHELL_T = 4.5;
const A_REF = 85.0;  const B_REF = 88.0;  const YC_REF = 34.0;  const WRAP = 0.7;  const SE_P = 4.0;
const GROOVE = [3.2, 7.6];  const GROOVE_D = 2.5;               // cover groove in the back face (3 mm neoprene, one layer, squeezed to 2.4)
const RING = [5.6, 7.6, 2.1];  const BARB = 0.4;  const UNDERCUT = 8.0;  // ring band, snap barb into an undercut to 8.0
const COVER_T = 3.0;
// ---------------- fits (P1S / PETG) ----------------
const CL = 0.2;  const M5_CLEAR = 5.5;  const M5_NUT_AF = 8.0;  const NUT_RIB = 0.15;  const M5_NUT_P = 5.1;  const CSK_M5_R = 5.3;
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
// hex pocket tool with 2 crush ribs (r 0.3, NUT_RIB into the pocket) on the flats facing +-yd
function hexRibs(context is Context, id is Id, c is Vector, nIn is Vector, af is number, h is number)
{
    hexPrism(context, id, c, nIn, af, h);
    const n = normalize(nIn);
    var xd = cross(n, vector(0, 0, 1));
    if (norm(xd) < 0.3) xd = vector(1, 0, 0);
    const yd = cross(n, normalize(xd));
    for (var k in [-1, 1])
        cylAx(context, id + ("rb" ~ (k + 1)), c - n * (h / 2 + 0.5) + yd * k * (af / 2 + 0.3 - NUT_RIB), c + n * (h / 2 + 0.5) + yd * k * (af / 2 + 0.3 - NUT_RIB), 0.3);
    opBoolean(context, id + "rbc", { "tools" : qUnion([solids(context, id + "rb0"), solids(context, id + "rb2")]), "targets" : solids(context, id + "ex"), "operationType" : BooleanOperationType.SUBTRACTION });
}
// (x, y) polygon extruded over world z
function xyPrism(context is Context, id is Id, pts is array, z0 is number, z1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, z0), vector(0, 0, 1), vector(1, 0, 0)) });
    for (var i = 0; i < size(pts); i += 1)
        skLineSegment(sk, "l" ~ i, { "start" : vector(pts[i][0], pts[i][1]) * millimeter, "end" : vector(pts[(i + 1) % size(pts)][0], pts[(i + 1) % size(pts)][1]) * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
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
// child-safe rounds on the add-on location (kit off): every convex edge lying in the given S planes (z = zA / zB for
// t' < tP, or t' = tP for zA < z < zB), except where the slot mouth meets the back face (covered by the cap / the nose).
// sx = -1 for the right side (built mirrored). Tries ROUND_R, then smaller, never below 1.5 without a warning.
const ROUND_R = 2.5;
function sideEdges(context is Context, body is Query, zA is number, zB is number, tP is number, sx is number, skipMouth is boolean) returns array
{
    var out = [];
    for (var e in evaluateQuery(context, qOwnedByBody(body, EntityType.EDGE)))
    {
        var ps = [];
        for (var u in [0.0, 0.5, 1.0])
        {
            const o = evEdgeTangentLine(context, { "edge" : e, "parameter" : u }).origin / millimeter;
            ps = append(ps, toS(vector(sx * o[0], o[1], o[2])));
        }
        var onZ = true;  var onT = true;
        for (var q in ps)
        {
            if (!((abs(q[2] - zA) < 0.03 || abs(q[2] - zB) < 0.03) && q[1] < tP - 0.03)) onZ = false;
            if (!(abs(q[1] - tP) < 0.03 && q[2] > zA + 0.03 && q[2] < zB - 0.03)) onT = false;
        }
        if (onT && skipMouth)
        {
            const m = ps[1];
            if (m[0] > SLOT[0][0] - MOUTH - 0.6 && m[0] < SLOT[1][0] + MOUTH_OUT + 0.6 && m[2] > TG_Z[0] - CLR - MOUTH - 0.6 && m[2] < TG_Z[1] + CLR + MOUTH + 0.6) onT = false;
        }
        if ((onZ || onT) && evEdgeConvexity(context, { "edge" : e }) == EdgeConvexityType.CONVEX) out = append(out, e);
    }
    return out;
}
function zEdgesOnly(edges is array, context is Context, sx is number) returns array
{
    var out = [];
    for (var e in edges)
    {
        const o = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 }).origin / millimeter;
        if (toS(vector(sx * o[0], o[1], o[2]))[1] < T_BACK - SEAM - 0.05) out = append(out, e);
    }
    return out;
}
function roundEdges(context is Context, id is Id, edges is array, what is string, sx is number) returns number
{
    if (size(edges) == 0) { if (what != "") warn(context, what ~ ": no edges found to round");  return 0; }
    for (var rr in [ROUND_R, 2.0])
        if (attempt(context, id + ("r" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : qUnion(edges), "radius" : rr * millimeter }); }))
            return rr;
    // edge by edge (the wing's skin is split into many faces): largest radius that each edge takes
    var minR = 99;  var failed = 0;  var k = 0;  var where = "";
    var byLen = [];
    for (var e in edges) byLen = append(byLen, { "e" : e, "l" : evLength(context, { "entities" : e }) / millimeter });
    byLen = sort(byLen, function(a, b) { return b.l - a.l; });
    var sorted = [];
    for (var m in byLen) sorted = append(sorted, m.e);
    for (var e in sorted)
    {
        k += 1;
        if (size(evaluateQuery(context, e)) == 0) continue;            // consumed by a neighbour's round
        const o = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 }).origin / millimeter;
        const q = toS(vector(sx * o[0], o[1], o[2]));
        const len = evLength(context, { "entities" : e }) / millimeter;
        var got = 0;
        for (var rr in [ROUND_R, 2.0, 1.5])
            if (got == 0 && attempt(context, id + ("e" ~ k ~ "_" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : e, "radius" : rr * millimeter }); }))
                got = rr;
        if (got == 0) { failed += 1;  where = where ~ " (d " ~ r1(q[0]) ~ " t " ~ r1(q[1]) ~ " z " ~ r1(q[2]) ~ " L " ~ r1(len) ~ ")"; } else minR = min(minR, got);
    }
    if (failed > 0 && what != "") warn(context, what ~ ": " ~ failed ~ " of " ~ size(edges) ~ " edges could not take R1.5:" ~ where);
    return failed > 0 ? 0 : minR;
}
// the swell behind the cheek: the wing's own section at t' = SWELL_T (d' >= 7, z SWELL_Z), its old receiver pocket filled,
// extruded straight back along t' to t' 37 and trimmed by the SWELL plan outline
function swell(context is Context, id is Id, wing is Query) returns Query
{
    const cp = copyOf(context, id + "cp", wing);
    keepOnly(context, id + "k", cp, 7.0, 40.0, SWELL_T, SWELL_T + 1.0, SWELL_Z[0], SWELL_Z[1], true);
    const n = normalize(cross(dirD(), dirT() * LT + vector(0, 0, 1)));
    const faces = qCoincidesWithPlane(qOwnedByBody(cp, EntityType.FACE), plane(QS(0, SWELL_T, ZC) * millimeter, n));
    if (size(evaluateQuery(context, faces)) == 0) throw regenError("Side Pad Wing: no cheek section at t' " ~ SWELL_T);
    opExtrude(context, id + "ex", { "entities" : faces, "direction" : dirT(), "endBound" : BoundingType.BLIND, "endDepth" : (37.0 - SWELL_T) * millimeter });
    opDeleteBodies(context, id + "dcp", { "entities" : cp });
    const hole = sBox(context, id + "hole", SWELL_HOLE[0], SWELL_HOLE[1], SWELL_T, 37.0, SWELL_HOLE[2], SWELL_HOLE[3]);
    opBoolean(context, id + "u", { "tools" : qUnion([solids(context, id + "ex"), hole]), "operationType" : BooleanOperationType.UNION });
    const plan = sPoly(context, id + "pl", SWELL, SWELL_Z[0] - 5, SWELL_Z[1] + 5, 0);
    const body = qUnion([solids(context, id + "ex"), hole]);
    intersectInto(context, id + "ix", body, plan);
    return qBodyType(qUnion([qCreatedBy(id + "ex", EntityType.BODY), qCreatedBy(id + "hole", EntityType.BODY)]), BodyType.SOLID);
}
export function wingTools(context is Context, id is Id, wing is Query, closePocket is boolean) returns map
{
    const lm = measureLean(context, wing);
    var lean0 = "";  try silent { lean0 = getVariable(context, "gbgLean"); }
    setVariable(context, "gbgLean", lean0 ~ (lean0 == "" ? "" : " || ") ~ "measured lean LD " ~ roundToPrecision(lm.LD, 4) ~ " LT " ~ roundToPrecision(lm.LT, 4) ~ " (model " ~ LD ~ " / " ~ LT ~ "), front t' " ~ roundToPrecision(lm.front, 2));
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
    const capR = roundEdges(context, id + "nbr", zEdgesOnly(sideEdges(context, noseB, NZ[0] + SEAM, NZ[1] - SEAM, T_BACK - SEAM, 1, false), context, 1), "blank cap / nose edge round", 1);
    var pr0 = "";  try silent { pr0 = getVariable(context, "gbgRounds"); }
    setVariable(context, "gbgRounds", pr0 ~ "cap/nose edges R" ~ capR ~ "; ");
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
    fills = append(fills, swell(context, id + "sw", wing));
    opDeleteBodies(context, id + "dsrc", { "entities" : qUnion([p240, w1, slit]) });
    // cuts: notch, slot (tongue + 0.2), catch groove (6 deg locking face)
    const notch = sBox(context, id + "notch", -40, 50, -40, T_BACK, NZ[0], NZ[1]);
    const slot = sPoly(context, id + "slot", SLOT, TG_Z[0] - CLR, TG_Z[1] + CLR, 1.0);
    // mouth chamfer MOUTH x 45 on the inner side, floor and ceiling; MOUTH_OUT x 45 on the outer (latch) side
    const mz0 = TG_Z[0] - CLR;
    const mz1 = TG_Z[1] + CLR;
    const dS0 = SLOT[0][0];  const dS1 = SLOT[1][0];
    const M1 = MOUTH + 1.0;                                     // starts 1 mm in front of the notch face (no near-coincident faces)
    rrLoftPoly(context, id + "mouth", [[dS0 - M1, mz0 - M1], [dS1 - 0.05, mz0 - M1], [dS1 - 0.05, mz1 + M1], [dS0 - M1, mz1 + M1]],
                                     [[dS0, mz0], [dS1 - 0.05, mz0], [dS1 - 0.05, mz1], [dS0, mz1]], T_BACK - 1.0, T_BACK + MOUTH);
    const M2 = MOUTH_OUT + 1.0;
    rrLoftPoly(context, id + "mouo", [[dS1 - 3.0, mz0 - M2], [dS1 + M2, mz0 - M2], [dS1 + M2, mz1 + M2], [dS1 - 3.0, mz1 + M2]],
                                    [[dS1 - 3.0, mz0], [dS1, mz0], [dS1, mz1], [dS1 - 3.0, mz1]], T_BACK - 1.0, T_BACK + MOUTH_OUT);
    // catch groove in the slot's outer wall behind the 15 mm shoulder: 6 deg front face at t' GR_T, bottom d' GR_D (skin 4.0)
    const groove = sPoly(context, id + "gr", [[dS1 - 0.4, GR_T - 0.04], [dS1, GR_T], [GR_D, GR_T + (GR_D - dS1) * GK], [GR_D, TO_T[1] + 0.3], [dS1 - 0.4, TO_T[1] + 0.3]], GR_Z[0], GR_Z[1], 0);
    // blank cap: nose + stub tongue with 2 + 2 crush ribs (r 0.5, 0.3 interference) and a hidden nail notch under the nose's rear edge
    const stub = sPoly(context, id + "stub", [[TONGUE[0][0], 1.0], [TONGUE[1][0], 1.0], [TONGUE[1][0], 9.0], [TONGUE[0][0], 9.0]], TG_Z[0] + 1.5, TG_Z[1] - 1.5, 1.0);
    var capParts = [stub];
    for (var i = 0; i < 2; i += 1)
    {
        cylAx(context, id + ("cri" ~ i), QS(TONGUE[0][0], 5.0 + 2.0 * i, TG_Z[0] + 3.0), QS(TONGUE[0][0], 5.0 + 2.0 * i, TG_Z[1] - 3.0), RIB_R);
        cylAx(context, id + ("cro" ~ i), QS(TONGUE[1][0], 5.0 + 2.0 * i, TG_Z[0] + 3.0), QS(TONGUE[1][0], 5.0 + 2.0 * i, TG_Z[1] - 3.0), RIB_R);
        capParts = concatenateArrays([capParts, [solids(context, id + ("cri" ~ i)), solids(context, id + ("cro" ~ i))]]);
    }
    const capSrc = copyOf(context, id + "capc", noseB);
    opBoolean(context, id + "capu", { "tools" : qUnion(concatenateArrays([[capSrc], capParts])), "operationType" : BooleanOperationType.UNION });
    const cap = one(context, qUnion(concatenateArrays([[capSrc], capParts])), "blank cap");
    const nail = sBox(context, id + "nail", 6.0, 14.0, T_BACK - SEAM - 1.2, T_BACK, NZ[0] + SEAM - 1.0, NZ[0] + SEAM + 1.0);
    opBoolean(context, id + "nailc", { "tools" : nail, "targets" : cap, "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, id + "dnose", { "entities" : noseB });
    var mi = "mouth tool: ";
    for (var mb in evaluateQuery(context, solids(context, id + "mouth")))
    {
        const bb = evBox3d(context, { "topology" : mb });
        const lo = toS(bb.minCorner / millimeter);  const hi = toS(bb.maxCorner / millimeter);
        mi = mi ~ r1(evVolume(context, { "entities" : mb }) / millimeter ^ 3) ~ " mm3 d " ~ r1(lo[0]) ~ ".." ~ r1(hi[0]) ~ " t " ~ r1(lo[1]) ~ ".." ~ r1(hi[1]) ~ " z " ~ r1(lo[2]) ~ ".." ~ r1(hi[2]) ~ "; ";
    }
    var prevL = "";  try silent { prevL = getVariable(context, "gbgLean"); }
    setVariable(context, "gbgLean", prevL ~ " | " ~ mi);
    return { "fill" : qUnion(fills), "cut" : qUnion([notch, slot, groove, solids(context, id + "mouth"), solids(context, id + "mouo")]), "cuts" : [notch, slot, groove, solids(context, id + "mouth"), solids(context, id + "mouo")], "cap" : cap };
}

// =============================================== CARRIER + LATCH A ===============================================
// Built on the LEFT side from `noseSrc` (the blank cap). zs = +1 LOW (pad below), -1 HIGH (pad above).
// Latch A "deep catch", one sliding part: cap (the nose's own skin inside a 12 x 24 stadium, 0.8 convex crown) + flange +
// connector + block (in the nose) + bar (along the tongue's top, z 241.5-246) + tooth (in a pocket at the tongue's rear,
// 6 deg locking face into the groove behind the wing's 15 mm shoulder). It drops in from the nose's top face (hidden in the
// notch) past a one-way barb, and springs out into the lip; the split orange tell sleeve snaps in under the lip and holds
// the flange. Pull-out: tongue pocket rear wall (0.1) -> tooth -> groove face -> shoulder. The block and flange have >= 0.4.
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
    // ---- tongue: solid ----
    const tg = sPoly(context, id + "tg", TONGUE, TG_Z[0], TG_Z[1], 1.0);
    // ---- arm: ONE loft through sections square to the path (22 x 31 near the nose -> 22 x 30 at the joint) ----
    const p = armPath(C, zp);
    var secs = [];
    const us = [-5.0, 0.0, p.L / 6, 2 * p.L / 6, 3 * p.L / 6, 4 * p.L / 6, 5 * p.L / 6, p.L];
    for (var i = 0; i < size(us); i += 1)
        secs = append(secs, sec(armCentre(p, us[i]), armAt(p, us[i]).t, ARM_W, i <= 3 ? ARM_H : ARM_H_END, ARM_R));
    rrLoft(context, id + "arm", secs, true, true);
    const xe = armAt(p, p.L).p[0];
    rrLoft(context, id + "ten", [sec(vector(xe + 0.5, YAX, zp), vector(-1, 0, 0), TEN_W, TEN_H, TEN_R), sec(vector(xe - TEN_L, YAX, zp), vector(-1, 0, 0), TEN_W, TEN_H, TEN_R)], false, false);
    const parts = qUnion([nose, tg, solids(context, id + "arm"), solids(context, id + "ten")]);
    opBoolean(context, id + "u", { "tools" : parts, "operationType" : BooleanOperationType.UNION });
    const body = one(context, parts, "carrier");
    // R 3.5 root fillet where the arm leaves the nose's front face (smaller radius only with a visible warning)
    const rootE0 = qIntersection([qAdjacent(qCreatedBy(id + "arm", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE),
                                 qAdjacent(qCreatedBy(id + "nose", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE)]);
    const rootE = edgesAtT(context, rootE0, -30.0, T_FRONT + 1.5);
    var rootR = 0;
    for (var rr in [ROOT_R, 2.0])
        if (rootR == 0 && attempt(context, id + ("rootf" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : rootE, "radius" : rr * millimeter }); }))
            rootR = rr;
    if (rootR == 0) warn(context, "arm root fillet failed");
    else if (rootR < ROOT_R) warn(context, "arm root fillet only R" ~ rootR);
    // tongue root into the nose's back face: R3 inner side + bottom, R0.5 on the outer (latch) side; top edge stays sharp
    var tgOut = [];  var tgRest = [];
    for (var e in evaluateQuery(context, edgesAtT(context, qAdjacent(qCreatedBy(id + "tg", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), T_BACK - SEAM - 0.05, T_BACK - SEAM + 0.05)))
    {
        const ln = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
        const pS = toS(ln.origin / millimeter);
        if (pS[2] > TG_Z[1] - 0.5) continue;
        if (pS[0] > 16.5 && abs(ln.direction[2]) > 0.7) tgOut = append(tgOut, e); else tgRest = append(tgRest, e);
    }
    var tgRo = 0;  var tgR = 0;
    if (size(tgOut) > 0 && attempt(context, id + "tgro", function(sid) { opFillet(context, sid + "f", { "entities" : qUnion(tgOut), "radius" : TG_ROOT_OUT * millimeter }); })) tgRo = TG_ROOT_OUT;
    var rest2 = [];
    for (var e in evaluateQuery(context, edgesAtT(context, qAdjacent(qCreatedBy(id + "tg", EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), T_BACK - SEAM - 0.05, T_BACK - SEAM + 0.05)))
    {
        const pS = toS(evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 }).origin / millimeter);
        if (pS[2] < TG_Z[1] - 0.5 && pS[0] < 16.5) rest2 = append(rest2, e);
    }
    for (var rr in [TG_ROOT, 2.5, 2.0, 1.5])
        if (tgR == 0 && size(rest2) > 0 && attempt(context, id + ("tgrf" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : qUnion(rest2), "radius" : rr * millimeter }); }))
            tgR = rr;
    if (tgR == 0 && size(rest2) > 0)
    {
        // one edge at a time (inner side, then bottom)
        var mr = 99;  var ok = true;  var k = 0;
        for (var e in rest2)
        {
            k += 1;  var got = 0;
            for (var rr in [TG_ROOT, 2.5, 2.0, 1.5])
                if (got == 0 && attempt(context, id + ("tge" ~ k ~ "_" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : e, "radius" : rr * millimeter }); }))
                    got = rr;
            if (got == 0) ok = false; else mr = min(mr, got);
        }
        if (ok) tgR = mr;
    }
    setVariable(context, "gbgTgRoot", "tongue root R" ~ tgR ~ " (outer side R" ~ tgRo ~ "), arm root R" ~ rootR);
    if (tgR < TG_ROOT) warn(context, "tongue root fillet " ~ (tgR == 0 ? "failed" : "only R" ~ tgR));
    if (tgRo == 0) warn(context, "tongue root outer edge not rounded");
    // bearing pads: relieve the tongue's bottom and top 0.3 between the mouth pad and the tip pad
    sBox(context, id + "pr0", 0, 20, PADS[0], PADS[1], TG_Z[0] - 1, TG_Z[0] + PAD_REL);
    sBox(context, id + "pr1", 0, BLK_D[0] - TRAVEL - 0.6, PADS[0], PADS[1], TG_Z[1] - PAD_REL, TG_Z[1] + 1);
    opBoolean(context, id + "prc", { "tools" : qUnion([qCreatedBy(id + "pr0", EntityType.BODY), qCreatedBy(id + "pr1", EntityType.BODY)]), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    finishEdges(context, id + "tenf", qAdjacent(qCapEntity(id + "ten" + "loft", CapType.END, EntityType.FACE), AdjacencyType.EDGE, EntityType.EDGE), 1.0, "tenon end round");

    // ---- latch A, deep catch: tools built in S (unsheared boxes/prisms get sheared; stadiums are built leaned) ----
    const L = id + "lt";
    const capD = [FD - BT_THK, FD];
    stadiumD(context, L + "cb", BT_TC, C, BT_W, BT_H, capD[0], FD + CROWN + 0.2);   // cap, crowned below
    stadiumD(context, L + "fl", BT_TC, C, FL_W, BT_H + 0.8, capD[0], capD[0] + FL_T);
    circleD(context, L + "sp", SPR[0], SPR[1], 2.1, capD[0] - 2.0, capD[0] + 0.05);
    tipBox(context, L + "con", BLK_D[0], capD[0] + 0.05, BLK_T[0], -0.9, BLK_Z[0], C + BT_H / 2 - 0.5);     // block -> cap
    tipBox(context, L + "blk", BLK_D[0], BLK_D[1], BLK_T[0], BLK_T[1], BLK_Z[0], BLK_Z[1]);              // block in the nose
    tipBox(context, L + "bar", BLK_D[0], BLK_D[1], BAR_T[0], BAR_T[1], BAR_Z[0], BAR_Z[1]);              // bar on the tongue top
    const ws = SLOT[1][0];
    const rampT = TO_T[1] - (TO_D[1] - (ws - 0.1));            // 45 deg lead-in: the tooth's back edge is inside the slot wall
    dtPrism(context, L + "to", [[TO_D[0], TO_T[0]], [ws, TO_T[0]], [TO_D[1], TO_T[0] + (TO_D[1] - ws) * GK], [TO_D[1], rampT], [ws - 0.1, TO_T[1]], [TO_D[0], TO_T[1]]], TO_Z[0], TO_Z[1]);
    // split tell sleeve (orange): stadium ring with a 1.0 gap in its back straight side
    stadiumD(context, L + "so", BT_TC, C, SL_OUT, BT_H + 1.4, capD[0] + FL_T, capD[0] + FL_T + SL_H);
    stadiumD(context, L + "si", BT_TC, C, SL_IN, BT_H + 0.4, capD[0], FD + 1);
    tipBox(context, L + "sg", capD[0] - 1, FD + 1, BT_TC + SL_IN / 2 - 0.5, BT_TC + SL_OUT / 2 + 0.5, C - SL_SPLIT / 2, C + SL_SPLIT / 2);
    // carrier cuts
    const K = id + "ct";
    stadiumD(context, K + "lip", BT_TC, C, LIP_W, BT_H + 0.6, FD - LIP - 0.05, FD + 3);
    stadiumD(context, K + "cbr", BT_TC, C, SL_OUT + 0.2, BT_H + 1.6, capD[0] + FL_T, FD - LIP);
    stadiumD(context, K + "fp", BT_TC, C, FL_W + 1.0, BT_H + 1.2, capD[0] - TRAVEL, capD[0] + FL_T + 0.01);
    dropBox(context, K + "cch", capD[0] - TRAVEL, max(capD[1] - TRAVEL + 0.2, capD[0] + 0.3), BT_TC - (FL_W + 1.0) / 2, BT_TC + (FL_W + 1.0) / 2, C - BT_H / 2 - 0.5, 1);   // drop-in channel (pressed button)
    dropBox(context, K + "spr", SPR[3], capD[0] - TRAVEL + 0.2, SPR[0] - SPR[2], SPR[0] + SPR[2], SPR[1] - SPR[2], 1);
    circleD(context, K + "spc", SPR[0], SPR[1], SPR[2], SPR[3] - 1.0, SPR[3] + 0.1);                // spring seat counterbore
    dropBox(context, K + "bch", BLK_D[0] - TRAVEL - 0.2, BLK_D[1] + 0.3, BLK_T[0] - 0.3, BLK_T[1] + 0.4, BLK_Z[0] - 0.3, 1);   // block channel (nose only; 0.4 behind: the tooth takes the pull-out)
    dropBox(context, K + "brc", BLK_D[0] - TRAVEL - 0.2, TONGUE[1][0] + 0.7, BAR_T[0] - 0.1, BAR_T[1] + 0.1, BAR_Z[0] - 0.3, 1);   // bar channel on the tongue top
    dropBox(context, K + "tch", TO_D[0] - TRAVEL - 0.2, 22.0, TO_T[0] - 0.3, TO_T[1] + 0.1, TO_Z[0] - 0.4, 1);                // tooth pocket at the tongue's rear
    // shear the box/prism tools into the leaned frame
    const M = id + "lm";
    const shl = [L + "con", L + "blk", L + "bar", L + "to", L + "sg", K + "cch", K + "spr", K + "bch", K + "brc", K + "tch"];
    for (var i = 0; i < size(shl); i += 1)
        opTransform(context, M + ("sh" ~ i), { "bodies" : solids(context, shl[i]), "transform" : shearS() });
    opBoolean(context, M + "slg", { "tools" : qUnion([solids(context, L + "si"), solids(context, L + "sg")]), "targets" : solids(context, L + "so"), "operationType" : BooleanOperationType.SUBTRACTION });
    // convex crown: keep the cap inside two cylinders (curvature along z and along t), rim on the nose plane d' = FD
    const RZ = ((BT_H / 2) * (BT_H / 2) + CROWN * CROWN) / (2 * CROWN);  const RT = ((BT_W / 2) * (BT_W / 2) + CROWN * CROWN) / (2 * CROWN);
    cylAx(context, M + "cz", QS(FD + CROWN - RZ, BT_TC - 20, C), QS(FD + CROWN - RZ, BT_TC + 20, C), RZ);
    cylAx(context, M + "ct", QS(FD + CROWN - RT, BT_TC, C - 30), QS(FD + CROWN - RT, BT_TC, C + 30), RT);
    intersectInto(context, M + "ix", solids(context, L + "cb"), solids(context, M + "cz"));
    intersectInto(context, M + "iy", solids(context, L + "cb"), solids(context, M + "ct"));
    // round the ridge where the two crown cylinders meet (no crease)
    {
        var ridge = [];
        for (var e in evaluateQuery(context, qOwnedByBody(solids(context, L + "cb"), EntityType.EDGE)))
        {
            const fs = evaluateQuery(context, qAdjacent(e, AdjacencyType.EDGE, EntityType.FACE));
            var curved = 0;
            for (var f in fs) if (!(evSurfaceDefinition(context, { "face" : f }) is Plane) && size(evaluateQuery(context, qIntersection([f, qCreatedBy(L + "cb", EntityType.FACE)]))) == 0) curved += 1;
            if (curved == 2) ridge = append(ridge, e);
        }
        var cr = 0;
        for (var rr in [8.0, 5.0, 3.0])
            if (cr == 0 && size(ridge) > 0 && attempt(context, M + ("crr" ~ rr), function(sid) { opFillet(context, sid + "f", { "entities" : qUnion(ridge), "radius" : rr * millimeter }); }))
                cr = rr;
        if (cr == 0) warn(context, "button crown ridge round failed (" ~ size(ridge) ~ " edges)");
    }
    const bparts = qUnion([solids(context, L + "cb"), solids(context, L + "fl"), solids(context, L + "sp"), solids(context, L + "con"), solids(context, L + "blk"), solids(context, L + "bar"), solids(context, L + "to")]);
    opBoolean(context, M + "bu", { "tools" : bparts, "operationType" : BooleanOperationType.UNION });
    const button = one(context, bparts, "button");
    // 0.4 round on the cap's rim (crown faces to the stadium side)
    var rim = [];
    for (var e in evaluateQuery(context, qOwnedByBody(button, EntityType.EDGE)))
    {
        const bb = evBox3d(context, { "topology" : e });
        const mid = (bb.minCorner + bb.maxCorner) / 2 / millimeter;
        if (toS(mid)[0] > FD - 1.2 && size(evaluateQuery(context, qIntersection([qAdjacent(e, AdjacencyType.EDGE, EntityType.FACE), qCreatedBy(L + "cb", EntityType.FACE)]))) > 0) rim = append(rim, e);
    }
    var rimR = 0;
    for (var rr in [0.4, 0.3, 0.2])
        if (rimR == 0 && attempt(context, M + ("rim" ~ round(rr * 10)), function(sid) { opFillet(context, sid + "f", { "entities" : qUnion(rim), "radius" : rr * millimeter }); }))
            rimR = rr;
    if (rimR == 0) warn(context, "button face rim round failed (" ~ size(rim) ~ " edges)");
    const sleeve = one(context, solids(context, L + "so"), "tell sleeve");
    opBoolean(context, id + "cut", { "tools" : solids(context, K), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    // one-way barb in the cap channel (front wall, z 240-242.5): the flange clicks past it once on assembly; a pick pushes it aside
    tzPolyS(context, id + "barb", [[BT_TC - (FL_W + 1.0) / 2 - 0.05, C + (BT_H + 0.8) / 2 + 0.6], [BT_TC - FL_W / 2 + 0.3, C + (BT_H + 0.8) / 2 + 0.6], [BT_TC - FL_W / 2 + 0.3, C + (BT_H + 0.8) / 2 + 1.2], [BT_TC - (FL_W + 1.0) / 2 - 0.05, C + (BT_H + 0.8) / 2 + 3.6]], capD[0] - TRAVEL + 0.05, capD[0] + 0.25);
    // stop pads: 3 x Ø3 x 0.3 on the nose's back face (the nose seats on them; the visible seam stays 0.3)
    var pads = [];
    const PADP = [[-1.8, 230.0], [21.3, 222.0], [10.0, 209.2]];
    for (var i = 0; i < size(PADP); i += 1)
    {
        cylAx(context, id + ("sp" ~ i), Q(PADP[i][0], T_BACK - SEAM - 0.2, PADP[i][1]), Q(PADP[i][0], T_BACK - 0.02, PADP[i][1]), 1.5);
        opTransform(context, id + ("sps" ~ i), { "bodies" : solids(context, id + ("sp" ~ i)), "transform" : shearS() });   // end face on the leaned notch face
        pads = append(pads, solids(context, id + ("sp" ~ i)));
    }
    opBoolean(context, id + "padu", { "tools" : qUnion(concatenateArrays([[body, solids(context, id + "barb")], pads])), "operationType" : BooleanOperationType.UNION });
    var body2 = one(context, qContainsPoint(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), QS(0, -8, C) * millimeter), "carrier");
    tenonCuts(context, id + "tc", body2, xe, zp);
    // crush ribs (r 0.5, 0.3 interference): across the tongue's top and bottom near the tip, and 2 per side face
    if (ribs)
    {
        var rq = [];
        for (var i = 0; i < size(RIBS); i += 1)
        {
            cylAx(context, id + ("rb" ~ i), QS(3.0, RIBS[i] + 2.0, TG_Z[0]), QS(8.0, RIBS[i] + 2.0, TG_Z[0]), RIB_R);
            cylAx(context, id + ("rt" ~ i), QS(3.0, RIBS[i] + 2.0, TG_Z[1]), QS(8.0, RIBS[i] + 2.0, TG_Z[1]), RIB_R);
            cylAx(context, id + ("ri" ~ i), QS(2.0 + (RIBS[i] - 8.5) * 0.0262, RIBS[i], TG_Z[0] + 2.0), QS(2.0 + (RIBS[i] - 8.5) * 0.0262, RIBS[i], TG_Z[1] - 2.0), RIB_R);
            cylAx(context, id + ("ro" ~ i), QS(17.8, RIBS[i] - 4.0, TG_Z[0] + 2.0), QS(17.8, RIBS[i] - 4.0, BAR_Z[0] - 1.0), RIB_R);
            rq = concatenateArrays([rq, [solids(context, id + ("rb" ~ i)), solids(context, id + ("rt" ~ i)), solids(context, id + ("ri" ~ i)), solids(context, id + ("ro" ~ i))]]);
        }
        opBoolean(context, id + "rbu", { "tools" : qUnion(concatenateArrays([[body2], rq])), "operationType" : BooleanOperationType.UNION });
    }
    const carrierB = one(context, qContainsPoint(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), QS(0, -8, C) * millimeter), "carrier after ribs");
    var carbon = [];
    if (showCarbon)
    {
        for (var i = 0; i < size(TPIN); i += 1)
            cylAx(context, id + ("cf2" ~ i), vector(xe + TPIN_X0, YAX + TPIN[i][0], zp + TPIN[i][1]), vector(xe + TPIN_X0 + PIN_L, YAX + TPIN[i][0], zp + TPIN[i][1]), 1.0);
        carbon = evaluateQuery(context, qUnion([solids(context, id + "cf20"), solids(context, id + "cf21"), solids(context, id + "cf22"), solids(context, id + "cf23")]));
    }
    return { "carrier" : carrierB, "button" : button, "sleeve" : sleeve, "carbon" : carbon, "xe" : xe, "zp" : zp, "FD" : FD };
}
// (t', z) polygon of the S frame extruded along d from d0 to d1, built directly leaned
function tzPolyS(context is Context, id is Id, pts is array, d0 is number, d1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : tzPlaneS(d0) });
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = pts[i];  const b = pts[(i + 1) % size(pts)];
        skLineSegment(sk, "l" ~ i, { "start" : tz2(a[0], a[1]), "end" : tz2(b[0], b[1]) });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : dirD(), "endBound" : BoundingType.BLIND, "endDepth" : (d1 - d0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// joint nut + screw hole in the tenon, 4 x Ø2 pin bores across its root
export function tenonCuts(context is Context, id is Id, carrier is Query, xe is number, zp is number)
{
    const xj = xe - JS_DX;
    for (var i = 0; i < size(TPIN); i += 1)
        cylAx(context, id + ("tp" ~ i), vector(xe - TEN_L - 0.5, YAX + TPIN[i][0], zp + TPIN[i][1]), vector(xe + TPIN_IN, YAX + TPIN[i][0], zp + TPIN[i][1]), TPIN_R);
    // side-entry nut slot (flats on +-x), 2 crush ribs 0.15 on the flats: the nut is pressed in once and stays
    fCuboid(context, id + "ns", { "corner1" : v3(xj - M5_NUT_AF / 2, YAX - TEN_W / 2 - 1, zp - 4.8), "corner2" : v3(xj + M5_NUT_AF / 2, YAX - 1.0 + M5_NUT_P / 2, zp + 4.8) });
    for (var k in [-1, 1])
        cylAx(context, id + ("nr" ~ (k + 1)), vector(xj + k * (M5_NUT_AF / 2 + 0.3 - NUT_RIB), YAX - TEN_W / 2 - 2, zp), vector(xj + k * (M5_NUT_AF / 2 + 0.3 - NUT_RIB), YAX + 3, zp), 0.3);
    opBoolean(context, id + "nrc", { "tools" : qUnion([solids(context, id + "nr0"), solids(context, id + "nr2")]), "targets" : qCreatedBy(id + "ns", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
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
    // snap bump on the rail top (30 deg ramp toward the open back of the groove, 60 deg on the stop side)
    xyPrism(context, id + "bp", [[xb + DV[2] - 0.05, SNAP_Y[0] - SNAP_H * 1.7], [xb + DV[2] + SNAP_H, SNAP_Y[0]], [xb + DV[2] + SNAP_H, SNAP_Y[1] - 0.3], [xb + DV[2] - 0.05, SNAP_Y[1]]], zc - 3.0, zc + 3.0);
    opBoolean(context, id + "u", { "tools" : qUnion([solids(context, id + "ol"), solids(context, id + "dv"), solids(context, id + "bp")]), "operationType" : BooleanOperationType.UNION });
    one(context, qUnion([solids(context, id + "ol"), solids(context, id + "dv"), solids(context, id + "bp")]), "pad shell");
    const body = qBodyType(qUnion([qCreatedBy(id + "ol", EntityType.BODY), qCreatedBy(id + "dv", EntityType.BODY), qCreatedBy(id + "bp", EntityType.BODY)]), BodyType.SOLID);
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
    rrLoft(context, id + "fl", [sec(vector(x0, YAX, zp), vector(-1, 0, 0), ARM_W, ARM_H_END, ARM_R),
                                sec(vector(XS + FLARE_L, YAX, zp), vector(-1, 0, 0), ARM_W, ARM_H_END, ARM_R),
                                sec(vector(XS + 0.45 * FLARE_L, lerp(YAX, fyc, 0.45), zp), vector(-1, 0, 0), lerp(ARM_W, fw, 0.5), lerp(ARM_H_END, 2 * FOOT_HZ, 0.6), lerp(ARM_R, FOOT_R, 0.5)),
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
        hexRibs(context, P + (s > 0 ? "nu" : "nl"), vector(xb + DV[2] + 2.5, PS_Y, zr), vector(1, 0, 0), M5_NUT_AF, 5.0);
        // +y snap stop: notch in the groove floor for the rail's bump (the screw only locks)
        fCuboid(context, P + (s > 0 ? "su" : "sl"), { "corner1" : v3(xb + DV[2], SNAP_Y[0] - SNAP_H * 1.7 - 0.3, zr - 3.3), "corner2" : v3(xb + DV[2] + 0.1 + SNAP_H + 0.2, SNAP_Y[1] + 0.4, zr + 3.3) });
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
    var out = { "carrier" : car.carrier, "button" : car.button, "sleeve" : car.sleeve, "depth" : dp, "pad" : pad, "ring" : ring, "xe" : car.xe, "zp" : car.zp, "zc" : zc, "carbon" : car.carbon, "names" : nm };
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
        for (var i = 0; i < size(r.carbon); i += 1) name(context, r.carbon[i], "GBG-SP-CF2-r5 carbon pin Ø2 x " ~ PIN_L);
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
// volume and S-frame box of the overlap of two bodies (copies; nothing is left behind)
function overlapInfo(context is Context, id is Id, a is Query, b is Query) returns string
{
    var out = "";
    startFeature(context, id);
    try silent
    {
        opPattern(context, id + "a", { "entities" : a, "transforms" : [identityTransform()], "instanceNames" : ["a"] });
        opPattern(context, id + "b", { "entities" : b, "transforms" : [identityTransform()], "instanceNames" : ["b"] });
        opBoolean(context, id + "i", { "tools" : qUnion([qCreatedBy(id + "a", EntityType.BODY), qCreatedBy(id + "b", EntityType.BODY)]), "operationType" : BooleanOperationType.INTERSECTION });
        const r = qBodyType(qUnion([qCreatedBy(id + "a", EntityType.BODY), qCreatedBy(id + "b", EntityType.BODY)]), BodyType.SOLID);
        for (var c in evaluateQuery(context, r))
        {
            const bb = evBox3d(context, { "topology" : c });
            const lo = toS(bb.minCorner / millimeter);  const hi = toS(bb.maxCorner / millimeter);
            out = out ~ "[" ~ roundToPrecision(evVolume(context, { "entities" : c }) / millimeter ^ 3, 2) ~ " mm3 at d' " ~ r1(min(lo[0], hi[0])) ~ ".." ~ r1(max(lo[0], hi[0])) ~ " t' " ~ r1(min(lo[1], hi[1])) ~ ".." ~ r1(max(lo[1], hi[1])) ~ " z " ~ r1(lo[2]) ~ ".." ~ r1(hi[2]) ~ "]";
        }
    }
    abortFeature(context, id);
    return out;
}
function clashes(context is Context, id is Id, names is array, kit is array, others is array, detail is boolean) returns string
{
    var msg = "";
    for (var i = 0; i < size(kit); i += 1)
        for (var j = i + 1; j < size(kit); j += 1)
        {
            var hit = false;
            for (var cc in evCollision(context, { "tools" : kit[i], "targets" : kit[j] })) if (cc["type"] == ClashType.INTERFERE) hit = true;
            if (hit) msg = msg ~ names[i] ~ " x " ~ names[j] ~ (detail ? " " ~ overlapInfo(context, id + ("ok" ~ i ~ "_" ~ j), kit[i], kit[j]) : "") ~ ", ";
        }
    if (size(others) > 0)
        for (var i = 0; i < size(kit); i += 1)
        {
            for (var cc in evCollision(context, { "tools" : kit[i], "targets" : qUnion(others) }))
                if (cc["type"] == ClashType.INTERFERE)
                {
                    msg = msg ~ names[i] ~ " x chair " ~ (detail ? overlapInfo(context, id + ("ov" ~ i), kit[i], cc["targetBody"]) : "") ~ ", ";
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
            msg = clashes(context, sid + "cl", names, kit, others, n <= 2);
            // the pressed button (travel 4.0) must stay clear of the chair
            opTransform(context, sid + "press", { "bodies" : r.button, "transform" : transform(vector(s * dirD()[0], dirD()[1], 0) * (-TRAVEL) * millimeter) });
            for (var cc in evCollision(context, { "tools" : r.button, "targets" : qUnion(others) })) if (cc["type"] == ClashType.INTERFERE) { msg = msg ~ "PRESSED button x chair, ";  break; }
        }
        catch (e) { msg = "BUILD FAILED " ~ toString(e); }
        abortFeature(context, sid);
        if (msg != "") { nBad += 1; out = out ~ armCode(drop) ~ "-" ~ flipCode(zs) ~ "-" ~ (po > 0 ? "upper" : "lower") ~ "-" ~ deg ~ ": " ~ msg ~ "; "; }
    }
    return sideL(s) ~ ": " ~ n ~ " settings, " ~ nBad ~ " with clashes; kit |x| max " ~ r1(maxX) ~ " (arm rest 165)" ~ (out == "" ? "" : " -- " ~ out);
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
        var info = "";  var roundsInfo = "";
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
            const cnames = ["notch", "slot", "groove", "mouth chamfer", "outer mouth chamfer"];
            for (var ci = 0; ci < size(t.cuts); ci += 1)
            {
                try { opBoolean(context, sid + ("cut" ~ ci), { "tools" : t.cuts[ci], "targets" : w2[0], "operationType" : BooleanOperationType.SUBTRACTION }); }
                catch (e) { throw regenError("Side Pad Wing: " ~ cnames[ci] ~ " cut failed (" ~ size(evaluateQuery(context, t.cuts[ci])) ~ " tool bodies)"); }
            }
            const nb = size(evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * probe[0], probe[1], probe[2]))));
            if (nb != 1) throw regenError("Side Pad Wing: the wing split into pieces");
            // child-safe rounds on the notch's outside edges (floor/ceiling and back face meeting the wing's faces)
            // two passes: an edge that fails next to an unrounded neighbour usually takes its round once the neighbour has one
            var wr = 0;
            try silent { wr = roundEdges(context, sid + "wr", sideEdges(context, w2[0], NZ[0], NZ[1], T_BACK, s, true), "", s); }
            const left = sideEdges(context, w2[0], NZ[0], NZ[1], T_BACK, s, true);
            if (size(left) > 0) wr = roundEdges(context, sid + "wr2", left, "wing notch edge round (2nd pass)", s);
            roundsInfo = roundsInfo ~ sideL(s) ~ " notch edges R" ~ wr ~ "; ";
            // the swell's back meets the blade: round that crease
            var crease = [];
            for (var e in evaluateQuery(context, qOwnedByBody(w2[0], EntityType.EDGE)))
            {
                var inside = true;
                for (var u in [0.0, 0.5, 1.0])
                {
                    const o = evEdgeTangentLine(context, { "edge" : e, "parameter" : u }).origin / millimeter;
                    const q = toS(vector(s * o[0], o[1], o[2]));
                    if (!(q[1] > 32.0 && q[1] < 38.5 && q[0] > 9.5 && q[0] < 14.5 && q[2] > SWELL_Z[0] && q[2] < SWELL_Z[1])) inside = false;
                }
                if (inside) crease = append(crease, e);
            }
            var concave = [];
            for (var e in crease) if (evEdgeConvexity(context, { "edge" : e }) != EdgeConvexityType.SMOOTH) concave = append(concave, e);
            const crR = roundEdges(context, sid + "cr", concave, "swell/blade crease round", s);
            roundsInfo = roundsInfo ~ "swell crease R" ~ crR ~ "; ";
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
        else { var cr = ""; try silent { cr = getVariable(context, "gbgRounds"); } reportFeatureInfo(context, id, "Child-safe rounds: " ~ roundsInfo ~ cr); }
    }, { "dropSupports" : true, "engrave" : true });

function dropOf(a is SidePadArm) returns number { return a == SidePadArm.SHORT ? DROPS[0] : DROPS[1]; }
function zsOf(k is SidePadKit) returns number { return k == SidePadKit.LOW ? 1 : -1; }
function offOf(p is SidePadPadPos) returns number { return p == SidePadPadPos.UPPER ? PAD_OFF : -PAD_OFF; }

annotation { "Feature Type Name" : "GbG Side Pad r5", "Feature Type Description" : "Round 5 side-pad kit (carrier with nose + arm, latch A, depth piece with tilt foot, pad, clip ring, reference foam + cover). Needs 'GbG Side Pad Wing (chair)' before it." }
export const gbgSidePadV1 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Side", "Default" : SidePadSide.BOTH }
        definition.side is SidePadSide;
        annotation { "Name" : "Kit" }
        definition.mode is SidePadMode;
        annotation { "Name" : "Left: arm", "Default" : SidePadArm.LONG }
        definition.arm is SidePadArm;
        annotation { "Name" : "Left: LOW / HIGH" }
        definition.kit is SidePadKit;
        annotation { "Name" : "Left: pad position", "Default" : SidePadPadPos.LOWER }
        definition.padPos is SidePadPadPos;
        annotation { "Name" : "Left: tilt" }
        definition.tilt is SidePadTilt;
        annotation { "Name" : "Right: arm" }
        definition.rArm is SidePadArm;
        annotation { "Name" : "Right: LOW / HIGH", "Default" : SidePadKit.HIGH }
        definition.rKit is SidePadKit;
        annotation { "Name" : "Right: pad position" }
        definition.rPadPos is SidePadPadPos;
        annotation { "Name" : "Right: tilt" }
        definition.rTilt is SidePadTilt;
        annotation { "Name" : "Foam (reference cushion)", "Default" : SidePadFoam.F20 }
        definition.foam is SidePadFoam;
        annotation { "Name" : "Show foam + cover (reference, not printed)", "Default" : true }
        definition.cushion is boolean;
        annotation { "Name" : "Show carbon pins", "Default" : true }
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
            const rep = "CHECK " ~ checkAll(context, id, definition, definition.checkSet == SidePadCheck.LEFT ? 1 : -1);
            reportFeatureInfo(context, id, rep);
            // the report also goes into the names of tiny marker parts far from the chair (readable from the parts list / API)
            const ch = splitIntoCharacters(rep);
            var k = 0;  var i = 0;
            while (i < size(ch))
            {
                var piece = "";
                for (var j = i; j < min(i + 200, size(ch)); j += 1) piece = piece ~ ch[j];
                fCuboid(context, id + ("rp" ~ k), { "corner1" : v3(2000 + 2 * k, 2000, 0), "corner2" : v3(2001 + 2 * k, 2001, 1) });
                name(context, qCreatedBy(id + ("rp" ~ k), EntityType.BODY), "RPT" ~ (k < 10 ? "0" : "") ~ k ~ " " ~ piece);
                i += 200;  k += 1;
            }
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
                info = info ~ sideL(s) ~ ": pad centre z " ~ r1(r.zc) ~ ", top " ~ r1(r.zc + PAD_H / 2) ~ "; pins Ø2 x " ~ PIN_L ~ "; " ~ tgr ~ ". ";
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
    // print orientations: carrier + cap + stub + block on the nose's outer face (normal +d' -> down); button on its inner face
    // (block, bar and tooth share the plane d' 13); sleeve flat; depth piece on its foot face; pad child side down; ring flat side down
    const outer = normalize(cross(dirT(), leanV() + vector(0, 0, 1)));          // normal of the plane d' = const (S)
    const nOut = dot(outer, dirD()) > 0 ? outer : -outer;
    const down = vector(0, 0, -1);
    var x = 0;  const gap = 25;
    x += place(context, id + "p1", r.carrier, x, 0, turn(nOut, down)) + gap;
    x += place(context, id + "p2", r.button, x, 0, turn(-nOut, down)) + gap;
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
    x2 += place(context, id + "q3", cb, x2, -120, turn(-nOut, down)) + gap;
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
    var carRow = [];  var btRow = [];  var sleeveDone = false;
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
    return "All parts: " ~ size(keep) ~ " bodies; pins Ø2 x " ~ PIN_L;
}

annotation { "Feature Type Name" : "GbG Side Pad latch coupon (r5)", "Feature Type Description" : "C7 coupon for latch A: needs the chair + 'GbG Side Pad Wing (chair)'. Use 'GbG Side Pad r5' with View = Print layout instead." }
export const gbgSidePadCoupon = defineFeature(function(context is Context, id is Id, definition is map)
    precondition { }
    {
        throw regenError("The r5 latch coupon is part of 'GbG Side Pad r5' (View: Print layout + latch coupon) in a Part Studio with the chair and the r5 wing feature.");
    }, {});
