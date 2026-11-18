FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// ===================================================================================================================
// GoBabyGo SIDE PADS (lateral trunk supports) v1.1 for GbG_chair_forth.
//
// v1.1 (2026-11-19, owner's brief): the assembled arm reads as ONE flowing piece.
//   * ARM: tongue in the wing slot -> taper inside the slot -> one smooth 16 x 24 arm: a circular plan arc (R ~22) combined
//     with a two-arc vertical S, ending level and heading inboard. No knee, no neck taper, no polyline faces.
//   * RELEASE: a flush pill button whose cap IS the patch of arm surface where the latch rod's line leaves the arm; it
//     drives the v1.1 printed shuttle catch (coupons C1-C5) with the owner's own spring.
//   * HEIGHT: 3 fixed tongue positions (one rib each in the wing) x LOW / HIGH flip = 6 pad heights. (Comb later.)
//   * JOINT: a hidden tenon on the arm end slides into the depth piece's sleeve (same 16 x 24 skin: only a hairline shows),
//     one flush countersunk M5 from the back into a nut inside the tenon.
//   * PAD MOUNT: the depth piece flares into a foot that follows the pad back; one rib in one groove; one M5 from the child
//     side (under the foam, reached through the cover) into a nut at the bottom of the depth piece's hollow.
// Loads: rib + slot walls (height, lean), the arm, the tenon in its sleeve, the foot rib. Screws and the catch only hold.
// Frame: chair WORLD, mm, z up, -y = front; LEFT side at +x, the right side is its mirror.
// Wing-tip frame: Q(d, t, z) = P0 + d * AXH + t * TH + shear (d across the tip, +d = outer wall; t into the wing).
// ===================================================================================================================

export enum SidePadSide { annotation { "Name" : "Left (+x)" } LEFT, annotation { "Name" : "Right (-x)" } RIGHT, annotation { "Name" : "Both" } BOTH }
export enum SidePadKit { annotation { "Name" : "LOW (pad below the tongue)" } LOW, annotation { "Name" : "HIGH (carrier flipped: pad above)" } HIGH }
export enum SidePadPos { annotation { "Name" : "1 (lowest)" } P1, annotation { "Name" : "2" } P2, annotation { "Name" : "3 (highest)" } P3 }
export enum SidePadDepth { annotation { "Name" : "D0 (pad back x 104)" } D0, annotation { "Name" : "D1 (x 92)" } D1, annotation { "Name" : "D2 (x 80)" } D2, annotation { "Name" : "D3 (x 68)" } D3 }
export enum SidePadTilt { annotation { "Name" : "none" } T0, annotation { "Name" : "8 deg top in" } P8, annotation { "Name" : "8 deg top out" } M8, annotation { "Name" : "15 deg top in" } P15, annotation { "Name" : "15 deg top out" } M15 }

// ---------------- chair / wing-tip frame ----------------
const P0 = [131.3, 80.7];  const AXH = [0.79577626, 0.60559074];  const TH = [-0.6083053, 0.79370313];
const D_SHIFT = 2.0;
const Z0 = 227.8;  const SHEAR = [0.8 / 32.9, 2.7 / 32.9];  const ZREF = 244.25;
// ---------------- wing tip (chair change) ----------------
const TIP_D = [-7.0, 25.0];  const TIP_T = [-16.0, 38.0];  const TIP_Z = [198.0, 347.0];     // bottom 0.2 above the arm rest top
const SL_D = [1.2, 17.6];  const SL_T = [-30.0, 28.0];  const SL_Z = [201.0, 318.0];      // vertical slot, open at the front
const RAIL_P = 4.0;  const RAIL_H = 6.0;  const RAIL_T = [-12.0, 28.0];   // one rail per position on the slot's inner wall (the tongue's groove rides on it: height + pitch)
// ---------------- heights ----------------
const C_POS = [227.0, 259.5, 292.0];                      // tongue centre z at positions 1 / 2 / 3
const D_OFF = 47.5;                                        // pad centre below (LOW) / above (HIGH) the tongue
// ---------------- carrier ----------------
const TG_R = 1.5;  const TG_D = [1.4, 17.4];  const TG_T = [-2.0, 27.5];  const TG_H = 25.0;    // full-height tongue 16 x 29.5 x 50
const TAPER_T = -14.0;                                     // taper 50 -> 24 tall from t -2 to t -14: it ends INSIDE the slot (mouth t -16)
const ARM_W = 18.0;  const ARM_H = 28.0;  const ARM_R = 9.0;                                    // the arm section (pill: 18 wide, 28 tall)
const TAPER_W = 16.0;  const TAPER_H = 32.0;              // the taper end inside the slot: 16 x 32 (then 18 wide by u 3, 28 tall by u ~10)
const YEND = 35.0;                                         // arm end / joint centre y
const YAX = YEND + (ARM_W - TAPER_W) / 2;                 // the arm widens toward the inside of the bend only (outer face stays put): joint axis y
const RG_CL = 0.15;                                       // rail groove clearance per side (no rocking)
const CF = [[5.5, -7.0, 2.15, -34.0], [9.5, -11.0, 1.65, -31.0]];  const CF_T1 = 28.5;  // carbon rods (d, z LOW, bore r, t end): Ø4 x 62 and Ø3 x 59, slid in from the tongue's rear face, bonded; 18-20 mm past the mouth
// latch (printed shuttle, as coupons C1-C5): rod along t, shuttle across d, spring behind the rod
const LZ = 0.0;  const ROD_DC = 10.0;  const ROD_S = 6.4;  const ROD_TEND = 10.5;               // rod axis on the tongue centre line; channel 6.4 (a 6 mm spring passes)
const SPR_T = [15.4, 19.5];  const SPR_R = 3.25;                                            // solid seat: 2.1 mm before the rib notch
const SH_T = [0.0, 8.0];  const SH_Z = [3.6, 9.6];  const SH_D0 = 10.6;  const ENG = 4.0;       // shuttle (LOW: on top of the rod)
const PEG_T = 4.0;  const PEG_D = 12.6;  const PEG_R = 1.5;  const RT = 3.5;  const SR = 4.5;   // rod travel 3.5 (thumb), shuttle retraction 4.5 (52 deg slot)
const RT_MAX = RT + 0.4;                                    // the cap floor is the press stop (not the peg)
const PILL_W = 7.0;  const PILL_H = 9.0;  const CAP_T = 4.5;                                   // flush pill cap (d x z), 4 thick
// ---------------- joint (hidden tenon in a sleeve) ----------------
const TEN_W = 11.0;  const TEN_H = 20.0;  const TEN_L = 13.0;  const TEN_R = 2.5;  const JS_DX = 6.5;  const JGAP = 0.3;
// ---------------- depth piece + pad mount ----------------
const FLARE_L = 18.0;  const FOOT_Y = [19.0, 46.0];  const FOOT_HZ = 20.0;  const FOOT_R = 14.5;
const FLOOR_T = 6.0;                                       // foot wall between the pad back and the hollow's floor
const PSY = YEND + (ARM_W - TAPER_W) / 2;  const BOSS_R = 7.0;  const BOSS_H = 3.0;                    // pad-screw boss on the shell BACK (hidden under the foot): M5 nut from the foot side
const KEY_L = 20.0;  const KEY_H = 1.75;  const KEY_D = 1.6;                                    // foot rib / shell groove
// ---------------- pad ----------------
const PAD_Y0 = -27.0;  const PAD_Y1 = 55.0;  const PAD_H = 80.0;  const SHELL_T = 4.5;
const A_REF = 85.0;  const B_REF = 88.0;  const YC_REF = 34.0;  const WRAP = 0.7;  const SE_P = 4.0;
const SHIM0 = 3.0;
const GROOVE = [3.2, 8.8];  const GROOVE_D = 2.5;                                           // cover groove in the BACK face (ring 1.6 + 2 x 2 mm neoprene)
// ---------------- fits (P1S / PETG, as the headrest) ----------------
const CL = 0.2;  const M5_CLEAR = 5.5;  const M5_NUT_AF = 8.3;  const M5_NUT_P = 5.1;  const CSK_M5_R = 5.3;   // nut pocket 5.1 deep: DIN 934 (4.0) and ISO 4032 (4.7)

// ---------------- small helpers ----------------
// ---------------- small helpers (as the headrest studio) ----------------
function v3(x, y, z) returns Vector { return vector(x, y, z) * millimeter; }
function vv(p) returns Vector { return vector(p[0], p[1], p[2]) * millimeter; }
function sgn(x) returns number { return x < 0 ? -1 : 1; }
function lerp(a is number, b is number, t is number) returns number { return a + (b - a) * t; }
function linspace(a is number, b is number, n is number) returns array
{
    var o = [];
    for (var i = 0; i < n; i += 1) o = append(o, n == 1 ? a : a + (b - a) * i / (n - 1));
    return o;
}
function solids(context is Context, of is Id) returns Query { return qBodyType(qCreatedBy(of, EntityType.BODY), BodyType.SOLID); }
// the single solid made so far under `of` (frozen, so later tool bodies under the same id don't join the query)
function oneBody(context is Context, of is Id, what is string) returns Query
{
    const b = evaluateQuery(context, solids(context, of));
    if (size(b) != 1) throw regenError(what ~ ": expected 1 solid, found " ~ size(b));
    return b[0];
}
function cleanup(context is Context, id is Id, of is Id)
{
    const junk = qSubtraction(qCreatedBy(of, EntityType.BODY), qBodyType(qCreatedBy(of, EntityType.BODY), BodyType.SOLID));
    if (size(evaluateQuery(context, junk)) > 0) opDeleteBodies(context, id, { "entities" : junk });
}
function skipped(context is Context, what is string)
{
    var prev = "";
    try silent { prev = getVariable(context, "gbgSkipped"); if (prev != "") prev = prev ~ "; "; }
    setVariable(context, "gbgSkipped", prev ~ what);
    println("SKIPPED: " ~ what);
}
function attempt(context is Context, id is Id, fn is function) returns boolean
{
    var ok = false;
    startFeature(context, id);
    try silent { fn(id); ok = getFeatureError(context, id) == undefined; }
    catch (e) { println("attempt " ~ toString(id) ~ " failed: " ~ toString(e)); }
    if (ok) { endFeature(context, id); return true; }
    abortFeature(context, id);
    clearFeatureStatus(context, id, { "withDisplayData" : true });
    return false;
}
function finishEdges(context is Context, id is Id, edges is Query, r is number, what is string)
{
    if (size(evaluateQuery(context, edges)) == 0) return;
    if (!attempt(context, id, function(sid) { opFillet(context, sid + "op", { "entities" : edges, "radius" : r * millimeter }); }))
        skipped(context, what);
}
// wing-tip frame point (mm vector), z absolute
function Q(d is number, t is number, z is number) returns Vector
{
    const dd = d + D_SHIFT;
    return vector(P0[0] + dd * AXH[0] + t * TH[0] + (ZREF - Z0) * SHEAR[0], P0[1] + dd * AXH[1] + t * TH[1] + (ZREF - Z0) * SHEAR[1], z);
}
function dirD() returns Vector { return normalize(vector(AXH[0], AXH[1], 0)); }
function dirT() returns Vector { return normalize(vector(TH[0], TH[1], 0)); }
// box in the wing-tip frame (d0..d1, t0..t1, z0..z1)
function tipBox(context is Context, id is Id, d0, d1, t0, t1, z0, z1)
{
    fCuboid(context, id + "b", { "corner1" : v3(0, 0, 0), "corner2" : v3(d1 - d0, t1 - t0, z1 - z0) });
    const o = Q(d0, t0, z0);
    const xf = transform(v3(o[0], o[1], o[2])) * toWorld(coordSystem(vector(0, 0, 0) * meter, dirD(), vector(0, 0, 1))) ;
    opTransform(context, id + "x", { "bodies" : qCreatedBy(id + "b", EntityType.BODY), "transform" : xf });
}
// rounded box in the wing-tip frame (all edges r)
function tipRBox(context is Context, id is Id, d0, d1, t0, t1, z0, z1, r)
{
    tipBox(context, id, d0, d1, t0, t1, z0, z1);
    if (r > 0) finishEdges(context, id + "f", qOwnedByBody(qCreatedBy(id + "b", EntityType.BODY), EntityType.EDGE), r, "tip box round");
}
function cylAx(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number)
{
    fCylinder(context, id, { "bottomCenter" : p0 * millimeter, "topCenter" : p1 * millimeter, "radius" : r * millimeter });
}
// hex prism (nut pocket) centred on c, axis n, across-flats af, height h
function hexPrism(context is Context, id is Id, c is Vector, nIn is Vector, af is number, h is number)
{
    const n = normalize(nIn);
    const rv = af / sqrt(3);
    var xd = cross(n, vector(0, 0, 1));
    if (norm(xd) < 0.3) xd = vector(1, 0, 0);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane((c - n * (h / 2)) * millimeter, n, normalize(xd)) });
    var pts = [];
    for (var k = 0; k < 6; k += 1) pts = append(pts, vector(rv * cos((k * 60 + 30) * degree), rv * sin((k * 60 + 30) * degree)) * millimeter);
    for (var k = 0; k < 6; k += 1) skLineSegment(sk, "h" ~ k, { "start" : pts[k], "end" : pts[(k + 1) % 6] });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : n, "endBound" : BoundingType.BLIND, "endDepth" : h * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// TEARDROP hole p0 -> p1, tip toward `up` (print up)
function tdHole(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number, up is Vector)
{
    const ax = normalize(p1 - p0);
    var u = up - ax * dot(up, ax);
    if (norm(u) < 0.3) { cylAx(context, id + "c", p0, p1, r); return; }
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
// ---------------- smooth geometry (no polylines, no fallbacks) ----------------
// Every visible curve is a line / tangent-arc profile or a spline, and every loft is ONE loft through a few sections set
// square to the path. If a loft fails the feature fails with an error: there is no silent piecewise fallback (that is what
// left "layer lines" on v1.0's arm and pad base).
//
// rounded-rectangle profile: centre c, plane normal n, width w along the horizontal in-plane direction, height h along
// the in-plane "up", corner radius r. Lines + tangent arcs, always drawn in the same order so lofts do not twist.
function rrProfile(context is Context, id is Id, c is Vector, nIn is Vector, w is number, h is number, r is number, hint) returns Query
{
    const n = normalize(nIn);
    var xd = hint == undefined ? cross(vector(0, 0, 1), n) : hint - n * dot(hint, n);      // width direction, in the section plane
    if (norm(xd) < 1e-6) xd = vector(1, 0, 0);
    const sk = newSketchOnPlane(context, id, { "sketchPlane" : plane(c * millimeter, n, normalize(xd)) });
    const a = w / 2;  const b = h / 2;
    if (r >= min(a, b) - 0.06)
    {
        // stadium (pill): two half circles on the short sides joined by straight lines -- no corner lines on the part
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
// section record for rrLoft
function sec(c is Vector, n is Vector, w is number, h is number, r is number) returns map
{
    return { "c" : c, "n" : n, "w" : w, "h" : h, "r" : r, "x" : undefined };
}
// one smooth loft through rounded-rect sections; optional end tangency (square to the end sections)
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
// foot-family section in the plane x = const (foot, nose, seat recess, shim, boss): rounded rect, centre (yc, zc)
function footSec(x is number, yc is number, zc is number, w is number, h is number, r is number) returns map
{
    return sec(vector(x, yc, zc), vector(1, 0, 0), w, h, r);
}
function bez(p0 is Vector, p1 is Vector, p2 is Vector, p3 is Vector, s is number) returns Vector
{
    return p0 * (1 - s) ^ 3 + p1 * 3 * (1 - s) ^ 2 * s + p2 * 3 * (1 - s) * s ^ 2 + p3 * s ^ 3;
}
// vertical prism of a world x-y polygon (points as mm vectors, z ignored), z0..z1
function xyPrism(context is Context, id is Id, pts is array, z0 is number, z1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, z0), vector(0, 0, 1), vector(1, 0, 0)) });
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = pts[i];  const b = pts[(i + 1) % size(pts)];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}


// =============================================== ARM PATH ===============================================
// plan: circular arc (radius R, clockwise) from the arm start heading -t to heading -x at y = YEND;
// height: two equal circular arcs (radius Rv) taking the arm from the tongue height c to the pad height zp, level at both ends.
export function armPath(c is number, zp is number) returns map
{
    const st = Q((TG_D[0] + TG_D[1]) / 2, TAPER_T, c);
    const h0 = -dirT();
    const th0 = atan2(h0[1], h0[0]) / radian;
    const R = (YEND - st[1]) / (sin((th0 - PI / 2) * radian) - 1);
    const C = [st[0] + R * cos((th0 - PI / 2) * radian), st[1] + R * sin((th0 - PI / 2) * radian)];
    const L = R * (th0 + PI);
    const dz = zp - c;
    const thv = 2 * atan(abs(dz) / L) / radian;
    const Rv = L / (2 * sin(thv * radian));
    return { "st" : st, "th0" : th0, "R" : R, "C" : C, "L" : L, "dz" : dz, "Rv" : Rv, "c" : c };
}
export function armAt(p is map, u is number) returns map
{
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

// =============================================== CARRIER + LATCH ===============================================
// returns { carrier, button, shuttle, xe } ; zs = +1 LOW, -1 HIGH (the HIGH carrier is the flip of the other side's LOW)
export function buildCarrier(context is Context, id is Id, c is number, zp is number) returns map
{
    const zs = zp < c ? 1 : -1;
    const dc = (TG_D[0] + TG_D[1]) / 2;
    // tongue (full height): edges rounded, except round its front face where the taper continues flush
    tipBox(context, id + "tg", TG_D[0], TG_D[1], TG_T[0], TG_T[1], c - TG_H, c + TG_H);
    const tgFront = qContainsPoint(qCreatedBy(id + "tg" + "b", EntityType.FACE), Q(dc, TG_T[0], c) * millimeter);
    finishEdges(context, id + "tgf", qSubtraction(qCreatedBy(id + "tg" + "b", EntityType.EDGE), qAdjacent(tgFront, AdjacencyType.EDGE, EntityType.EDGE)), TG_R, "tongue edge round");
    // taper inside the slot: 16 x 50 -> 16 x 24, smooth into the tongue and into the arm
    rrLoft(context, id + "tp", [sec(Q(dc, TG_T[0], c), -dirT(), TG_D[1] - TG_D[0], 2 * TG_H, TG_R), sec(Q(dc, TAPER_T, c), -dirT(), TAPER_W, TAPER_H, TAPER_W / 2)], true, true);
    // the arm: one loft through 7 sections square to the path
    const p = armPath(c, zp);
    var secs = [];
    const uOut = (-TIP_T[0]) + TAPER_T + 0.6;                  // u where the arm is 0.6 mm clear of the slot mouth
    var us = [0, uOut, uOut + 3.0];
    for (var i = 1; i <= 6; i += 1) us = append(us, p.L * i / 6);
    for (var u in us)
    {
        const a = armAt(p, u);
        const w = u <= uOut ? TAPER_W : ARM_W;                 // 16 wide until clear of the slot, then grown toward the inside of the bend only
        const h = u == 0 ? TAPER_H : (u <= uOut ? lerp(TAPER_H, ARM_H, 0.1) : (u < 8 ? lerp(TAPER_H, ARM_H, 0.4) : ARM_H));
        const inw = normalize(vector(p.C[0] - a.p[0], p.C[1] - a.p[1], 0));
        secs = append(secs, sec(a.p + inw * (w - TAPER_W) / 2, a.t, w, h, w / 2));
    }
    rrLoft(context, id + "arm", secs, true, true);
    const e = armAt(p, p.L);
    const xe = e.p[0];
    // hidden tenon on the arm end (into the depth piece's sleeve)
    rrLoft(context, id + "ten", [sec(vector(xe + 0.5, YAX, zp), vector(-1, 0, 0), TEN_W, TEN_H, TEN_R), sec(vector(xe - TEN_L, YAX, zp), vector(-1, 0, 0), TEN_W, TEN_H, TEN_R)], false, false);
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const body = oneBody(context, id, "carrier");

    // ---- latch geometry from the arm as built (copies, before any cut); set algebra with subtractions only ----
    const zr = c + zs * LZ;                                     // rod axis z
    const ax = dirT();                                          // +t = into the wing (the pressing direction)
    opPattern(context, id + "cpy", { "entities" : body, "transforms" : [identityTransform(), identityTransform(), identityTransform()], "instanceNames" : ["a", "b", "r"] });
    const copies = evaluateQuery(context, qCreatedBy(id + "cpy", EntityType.BODY));
    const ps = Q(ROD_DC, TAPER_T + 6, zr);
    // the rod line's first exit from the arm: the pill prisms stop just past it (no second pass through the arm)
    const hits = evRaycast(context, { "entities" : copies[0], "ray" : line(ps * millimeter, -ax) });
    if (size(hits) == 0) throw regenError("pill button: the rod line never leaves the arm");
    const PL = hits[0].distance / millimeter + 7;
    // inside(X) = X2 - (X1 - arm): the part of prism X inside the arm
    pillPrism(context, id + "pp1", ps, -ax, PL, PILL_W, PILL_H);
    pillPrism(context, id + "pp2", ps, -ax, PL, PILL_W, PILL_H);
    opBoolean(context, id + "o1", { "tools" : copies[0], "targets" : solids(context, id + "pp1"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "o2", { "tools" : solids(context, id + "pp1"), "targets" : solids(context, id + "pp2"), "operationType" : BooleanOperationType.SUBTRACTION });
    // cap = inside(pill) minus itself shifted CAP_T deeper: the 4 mm skin of arm surface inside the pill outline
    opPattern(context, id + "cs", { "entities" : solids(context, id + "pp2"), "transforms" : [transform(ax * CAP_T * millimeter)], "instanceNames" : ["s"] });
    opBoolean(context, id + "cap", { "tools" : qCreatedBy(id + "cs", EntityType.BODY), "targets" : solids(context, id + "pp2"), "operationType" : BooleanOperationType.SUBTRACTION });
    // pocket = inside(pill + clearance) minus itself shifted CAP_T + RT_MAX deeper (the cap floor is the press stop)
    pillPrism(context, id + "pq1", ps, -ax, PL, PILL_W + 2 * CL + 0.1, PILL_H + 2 * CL + 0.1);
    pillPrism(context, id + "pq2", ps, -ax, PL, PILL_W + 2 * CL + 0.1, PILL_H + 2 * CL + 0.1);
    opBoolean(context, id + "q1", { "tools" : copies[1], "targets" : solids(context, id + "pq1"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "q2", { "tools" : solids(context, id + "pq1"), "targets" : solids(context, id + "pq2"), "operationType" : BooleanOperationType.SUBTRACTION });
    opPattern(context, id + "qs", { "entities" : solids(context, id + "pq2"), "transforms" : [transform(ax * (CAP_T + RT_MAX) * millimeter)], "instanceNames" : ["s"] });
    opBoolean(context, id + "pk", { "tools" : qCreatedBy(id + "qs", EntityType.BODY), "targets" : solids(context, id + "pq2"), "operationType" : BooleanOperationType.SUBTRACTION });
    // push-rod (5.6 square) along the axis, from inside the cap to the spring seat, clipped to the arm
    tipBox(context, id + "rdA", ROD_DC - ROD_S / 2 + CL, ROD_DC + ROD_S / 2 - CL, TAPER_T + 6 - PL, ROD_TEND - 0.4, zr - ROD_S / 2 + CL, zr + ROD_S / 2 - CL);
    tipBox(context, id + "rdB", ROD_DC - ROD_S / 2 + CL, ROD_DC + ROD_S / 2 - CL, TAPER_T + 6 - PL, ROD_TEND - 0.4, zr - ROD_S / 2 + CL, zr + ROD_S / 2 - CL);
    opBoolean(context, id + "r1", { "tools" : copies[2], "targets" : solids(context, id + "rdB"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "r2", { "tools" : solids(context, id + "rdB"), "targets" : solids(context, id + "rdA"), "operationType" : BooleanOperationType.SUBTRACTION });
    // 45 deg slot for the shuttle peg (blind: the rod's far 2.6 mm stays whole)
    const sz0 = zs > 0 ? zr - ROD_S / 2 + CL + 2.6 : zr - ROD_S / 2 - 1;
    const sz1 = zs > 0 ? zr + ROD_S / 2 + 1 : zr + ROD_S / 2 - CL - 2.6;
    // slot for the peg in the rod: pressing moves the rod +RT along t and must pull the shuttle SR inward (-d)
    const su = normalize(vector(-SR, -RT, 0));                  // (d, t) direction of the slot in the rod's frame
    const kM = RT_MAX / RT;
    slot45(context, id + "ps", Q(PEG_D - 0.6 * su[0], PEG_T - 0.6 * su[1], zr), Q(PEG_D - SR * kM + 0.6 * su[0], PEG_T - RT_MAX + 0.6 * su[1], zr), PEG_R + 0.15, sz0, sz1);
    opBoolean(context, id + "psc", { "tools" : solids(context, id + "ps"), "targets" : solids(context, id + "rdA"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "btn", { "tools" : qUnion([solids(context, id + "pp2"), solids(context, id + "rdA")]), "operationType" : BooleanOperationType.UNION });
    const bq = evaluateQuery(context, qUnion([solids(context, id + "pp2"), solids(context, id + "rdA")]));
    if (size(bq) != 1)
    {
        var info = "";
        for (var b in bq)
        {
            const bb = evBox3d(context, { "topology" : b });
            info = info ~ " [vol " ~ roundToPrecision(evVolume(context, { "entities" : b }) / millimeter ^ 3, 0) ~ " box " ~ toString(roundToPrecision(bb.minCorner[0] / millimeter, 1)) ~ "," ~ roundToPrecision(bb.minCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(bb.minCorner[2] / millimeter, 1) ~ ".." ~ roundToPrecision(bb.maxCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(bb.maxCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(bb.maxCorner[2] / millimeter, 1) ~ "]";
        }
        throw regenError("pill button: expected 1 solid, found " ~ size(bq) ~ info ~ " PL " ~ roundToPrecision(PL, 1));
    }
    const button = bq[0];
    // shuttle
    const shz = zs > 0 ? [c + SH_Z[0] + CL, c + SH_Z[1] - 0.1] : [c - SH_Z[1] + 0.1, c - SH_Z[0] - CL];
    tipBox(context, id + "sh", SH_D0, TG_D[1] + CL + ENG, SH_T[0], SH_T[1], shz[0], shz[1]);
    const tip = TG_D[1] + CL + ENG;
    xyPrism(context, id + "shc", [Q(tip - 2.5 - 1, SH_T[1] + 1, 0), Q(tip + 1, SH_T[1] + 1, 0), Q(tip + 1, SH_T[1] - 2.5 - 1, 0)], shz[0] - 1, shz[1] + 1);
    opBoolean(context, id + "shcc", { "tools" : solids(context, id + "shc"), "targets" : qCreatedBy(id + "sh" + "b", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
    const pz0 = zs > 0 ? shz[0] + 0.01 : shz[1] - 0.01;  const pz1 = zs > 0 ? zr + ROD_S / 2 - CL - 2.8 : zr - ROD_S / 2 + CL + 2.8;
    cylAx(context, id + "peg", Q(PEG_D, PEG_T, pz0), Q(PEG_D, PEG_T, pz1), PEG_R);
    opBoolean(context, id + "shu", { "tools" : qUnion([qCreatedBy(id + "sh" + "b", EntityType.BODY), solids(context, id + "peg")]), "operationType" : BooleanOperationType.UNION });
    const sq = evaluateQuery(context, qUnion([qBodyType(qCreatedBy(id + "sh" + "b", EntityType.BODY), BodyType.SOLID), solids(context, id + "peg")]));
    if (size(sq) != 1) throw regenError("shuttle: expected 1 solid, found " ~ size(sq));
    const shuttle = sq[0];

    // ---- cuts in the carrier ----
    const K = id + "cut";
    // groove along the tongue's inner face for the wing rail (flared at the tongue's rear end, where the rail enters)
    const gh = RAIL_H / 2 + RG_CL;
    tzPrism(context, K + "rg", [[RAIL_T[0] - 1, c - gh], [TG_T[1] - 2.0, c - gh], [TG_T[1] + 0.01, c - gh - 0.8], [TG_T[1] + 1, c - gh - 0.8],
                                [TG_T[1] + 1, c + gh + 0.8], [TG_T[1] + 0.01, c + gh + 0.8], [TG_T[1] - 2.0, c + gh], [RAIL_T[0] - 1, c + gh]], TG_D[0] - 1, SL_D[0] + RAIL_P + RG_CL);
    for (var i = 0; i < size(CF); i += 1) tdHole(context, K + ("cf" ~ i), Q(CF[i][0], CF_T1, c + zs * CF[i][1]), Q(CF[i][0], CF[i][3], c + zs * CF[i][1]), CF[i][2], -dirD());
    tipBox(context, K + "rc", ROD_DC - ROD_S / 2, ROD_DC + ROD_S / 2, TAPER_T + 6 - PL, ROD_TEND + RT_MAX + 1.0, zr - ROD_S / 2, zr + ROD_S / 2);
    cylAx(context, K + "spr", Q(ROD_DC, SPR_T[0] - 0.5, zr), Q(ROD_DC, SPR_T[1], zr), SPR_R);
    // window for the shuttle's peg (it travels across d, from PEG_D locked to PEG_D - SR pressed) between the channels
    const wz = zs > 0 ? [zr - 0.5, c + SH_Z[0] + 0.01] : [c - SH_Z[0] - 0.01, zr + 0.5];
    tipBox(context, K + "pw", PEG_D - SR * RT_MAX / RT - PEG_R - 0.3, TG_D[1] + 1, PEG_T - PEG_R - 0.4, PEG_T + PEG_R + 0.4, wz[0], wz[1]);
    const chz = zs > 0 ? [c + SH_Z[0] - 0.01, c + SH_Z[1] + CL] : [c - SH_Z[1] - CL, c - SH_Z[0] + 0.01];
    tipBox(context, K + "shc", SH_D0 - SR * RT_MAX / RT - 0.4, TG_D[1] + 1, SH_T[0] - CL, SH_T[1] + CL, chz[0], chz[1]);
    opBoolean(context, id + "cutop", { "tools" : qUnion([solids(context, K), solids(context, id + "pq2")]), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return { "carrier" : body, "button" : button, "shuttle" : shuttle, "xe" : xe };
}
// prism in the wing-tip frame: polygon in (t, z), extruded along d from d0 to d1
function tzPrism(context is Context, id is Id, pts is array, d0 is number, d1 is number)
{
    const o = Q(d0, 0, 0);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(vector(o[0], o[1], 0) * millimeter, dirD(), dirT()) });
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = pts[i];  const b = pts[(i + 1) % size(pts)];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : dirD(), "endBound" : BoundingType.BLIND, "endDepth" : (d1 - d0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// pill (stadium) prism: start point s, direction n, length L, w across (horizontal), h tall
function pillPrism(context is Context, id is Id, s is Vector, n is Vector, L is number, w is number, h is number)
{
    const q = rrProfile(context, id + "sk", s, n, w, h, w / 2 - 0.05, undefined);
    opExtrude(context, id + "ex", { "entities" : q, "direction" : normalize(n), "endBound" : BoundingType.BLIND, "endDepth" : L * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
// vertical stadium slot from a to b (world points), radius r, z0..z1
function slot45(context is Context, id is Id, a is Vector, b is Vector, r is number, z0 is number, z1 is number)
{
    cylAx(context, id + "a", vector(a[0], a[1], z0), vector(a[0], a[1], z1), r);
    cylAx(context, id + "b", vector(b[0], b[1], z0), vector(b[0], b[1], z1), r);
    const dd = normalize(vector(b[0] - a[0], b[1] - a[1], 0));  const nn = vector(-dd[1], dd[0], 0) * r;
    xyPrism(context, id + "m", [a + nn, b + nn, b - nn, a - nn], z0, z1);
}

// =============================================== DEPTH PIECE ===============================================
export function depthX(d is SidePadDepth) returns number
{
    return d == SidePadDepth.D0 ? 104 : (d == SidePadDepth.D1 ? 92 : (d == SidePadDepth.D2 ? 80 : 68));
}
const FOOT_YC = (FOOT_Y[0] + FOOT_Y[1]) / 2;
export function buildDepth(context is Context, id is Id, xs is number, zp is number, xe is number) returns Query
{
    const fw = FOOT_Y[1] - FOOT_Y[0];
    const x0 = xe - JGAP;                                       // sleeve face (hairline gap to the arm end)
    // ONE loft from the hairline to the foot (same skin as the arm, then the trumpet): no second seam
    var ds = [sec(vector(x0, YAX, zp), vector(-1, 0, 0), ARM_W, ARM_H, ARM_R)];
    if (x0 - (xs + FLARE_L) > 2) ds = append(ds, sec(vector(xs + FLARE_L, YAX, zp), vector(-1, 0, 0), ARM_W, ARM_H, ARM_R));
    ds = append(ds, sec(vector(xs + 0.4 * FLARE_L, lerp(YAX, FOOT_YC, 0.4), zp), vector(-1, 0, 0), lerp(ARM_W, fw, 0.45), lerp(ARM_H, 2 * FOOT_HZ, 0.55), lerp(ARM_W, fw, 0.45) / 2));
    ds = append(ds, sec(vector(xs - 1.0, FOOT_YC, zp), vector(-1, 0, 0), fw, 2 * FOOT_HZ, FOOT_R));
    rrLoft(context, id + "fl", ds, true, false);
    if (size(evaluateQuery(context, solids(context, id))) > 1)
        opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const body = oneBody(context, id, "depth piece");
    const K = id + "cut";
    // the foot follows the pad back exactly (no gap): trim by the pad shell's outer surface
    planStrip(context, K + "pb", xs, -60, SHELL_T, zp - 40, zp + 40);
    // sleeve + hollow down to the foot wall; the nut for the pad screw slides sideways from the hollow into its pocket
    rrLoft(context, K + "sl", [sec(vector(x0 + 1, YAX, zp), vector(-1, 0, 0), TEN_W + 2 * CL, TEN_H + 2 * CL, TEN_R + CL), sec(vector(xs + FLOOR_T, YAX, zp), vector(-1, 0, 0), TEN_W + 2 * CL, TEN_H + 2 * CL, TEN_R + CL)], false, false);
    // pad screw: M5x10 socket head sits on the hollow's floor (hex key down the hollow), clearance down into the boss recess
    cylAx(context, K + "ps", vector(xs - 2, PSY, zp), vector(xs + FLOOR_T + 1, PSY, zp), M5_CLEAR / 2);
    cylAx(context, K + "br", vector(xs - 2, PSY, zp), vector(xs + BOSS_H + 0.2, PSY, zp), BOSS_R + CL);
    // groove for the shell's rib
    fCuboid(context, K + "kg", { "corner1" : v3(xs - 2, FOOT_YC - KEY_L / 2 - CL, zp - KEY_H - CL), "corner2" : v3(xs + KEY_D + 0.2, FOOT_YC + KEY_L / 2 + CL, zp + KEY_H + CL) });
    // joint screw: M5x12 countersunk from the back face (+y, toward the chair back: out of normal view) into the tenon's nut
    const xj = xe - JS_DX;
    const yb = YAX + ARM_W / 2;
    cylAx(context, K + "js", vector(xj, yb + 1, zp), vector(xj, YAX - 3.0, zp), M5_CLEAR / 2);
    fCone(context, K + "jsk", { "bottomCenter" : v3(xj, yb - (CSK_M5_R - 2.75), zp), "topCenter" : v3(xj, yb + 0.05, zp), "bottomRadius" : 2.75 * millimeter, "topRadius" : CSK_M5_R * millimeter });
    opBoolean(context, id + "cutop", { "tools" : solids(context, K), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return body;
}
// joint nut + screw hole in the tenon (cut from the carrier)
export function tenonCuts(context is Context, id is Id, carrier is Query, xe is number, zp is number, zs is number)
{
    const xj = xe - JS_DX;
    // nut on the y axis (flats across x), its pocket opening toward the tenon's hidden -y face: the screw from +y pulls it
    // against 3.95 mm of tenon. Symmetric top-to-bottom, so the flipped HIGH carrier is the same part.
    hexPrism(context, id + "n", vector(xj, YAX - 1.0, zp), vector(0, 1, 0), M5_NUT_AF, M5_NUT_P);
    fCuboid(context, id + "ns", { "corner1" : v3(xj - M5_NUT_AF / 2, YAX - TEN_W / 2 - 1, zp - 4.8), "corner2" : v3(xj + M5_NUT_AF / 2, YAX - 1.0, zp + 4.8) });
    cylAx(context, id + "h", vector(xj, YAX + TEN_W, zp), vector(xj, YAX - TEN_W / 2 - 1, zp), M5_CLEAR / 2);
    opBoolean(context, id + "op", { "tools" : solids(context, id), "targets" : carrier, "operationType" : BooleanOperationType.SUBTRACTION });
}

// =============================================== PAD ===============================================
// =============================================== PAD SHELL ===============================================
function shellXi(y is number, xs is number) returns number
{
    const ex = A_REF * max(1 - abs((y - YC_REF) / B_REF) ^ 3, 0) ^ (1 / 3);
    return xs - SHELL_T - WRAP * (A_REF - ex);
}
// pad side outline (superellipse in y-z), inset by `ins`
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
// prism of a y-z outline along x (x0..x1)
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
// plan strip between x = shellXi(y) + a and shellXi(y) + b, extruded over z
function planStrip(context is Context, id is Id, xs is number, a is number, b is number, z0 is number, z1 is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, z0), vector(0, 0, 1), vector(1, 0, 0)) });
    // the two curved faces are smooth splines through the shell curve (offset a and b); short straight ends close it
    var pa = [];  var pb = [];
    for (var y in linspace(PAD_Y0 - 2, PAD_Y1 + 2, 15)) { pa = append(pa, vector(shellXi(y, xs) + a, y) * millimeter);  pb = append(pb, vector(shellXi(y, xs) + b, y) * millimeter); }
    skFitSpline(sk, "sa", { "points" : pa });
    skFitSpline(sk, "sb", { "points" : pb });
    skLineSegment(sk, "e0", { "start" : pa[0], "end" : pb[0] });
    skLineSegment(sk, "e1", { "start" : pa[size(pa) - 1], "end" : pb[size(pb) - 1] });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
export function buildClipRing(context is Context, id is Id, xs is number, zc is number) returns Query
{
    // a flat band that presses the cover into the bead's groove; its top sits flush with the bead top
    planStrip(context, id + "ts", xs, SHELL_T - GROOVE_D + 0.4, SHELL_T, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "to", padOutlineYZ(zc, (GROOVE[0] + GROOVE[1]) / 2 - 0.8), xs - 80, xs + 20);
    yzPrism(context, id + "ti", padOutlineYZ(zc, (GROOVE[0] + GROOVE[1]) / 2 + 0.8), xs - 80, xs + 20);
    opBoolean(context, id + "tr", { "tools" : solids(context, id + "ti"), "targets" : solids(context, id + "to"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "tint", { "tools" : qUnion([solids(context, id + "ts"), solids(context, id + "to")]), "operationType" : BooleanOperationType.INTERSECTION });
    return oneBody(context, id, "clip ring");
}


// pad shell v1.1: shell + cover bead/groove (unchanged), groove for the foot rib, one countersunk M5 hole from the child side.
export function buildPadShell(context is Context, id is Id, xs is number, zc is number) returns Query
{
    // one plain curved plate (no raised rim): the cover wraps its edge and is trapped in a groove in the back face
    planStrip(context, id + "st", xs, 0, SHELL_T, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "ol", padOutlineYZ(zc, 0), xs - 80, xs + 20);
    opBoolean(context, id + "int", { "tools" : qUnion([solids(context, id + "st"), solids(context, id + "ol")]), "operationType" : BooleanOperationType.INTERSECTION });
    // on the back, hidden under the foot: the key rib and the pad-screw boss (M5 nut drops in from the foot side)
    const xb = shellXi(PSY, xs) + SHELL_T;
    fCuboid(context, id + "key", { "corner1" : v3(xs - 2, FOOT_YC - KEY_L / 2, zc - KEY_H), "corner2" : v3(xs + KEY_D, FOOT_YC + KEY_L / 2, zc + KEY_H) });
    cylAx(context, id + "boss", vector(xb - 1, PSY, zc), vector(xs + BOSS_H, PSY, zc), BOSS_R);
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const body = oneBody(context, id, "pad shell");
    const K = id + "cut";
    planStrip(context, K + "gs", xs, SHELL_T - GROOVE_D, SHELL_T + 1, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, K + "go", padOutlineYZ(zc, GROOVE[0]), xs - 80, xs + 20);
    yzPrism(context, K + "gi", padOutlineYZ(zc, GROOVE[1]), xs - 80, xs + 20);
    opBoolean(context, K + "gring", { "tools" : solids(context, K + "gi"), "targets" : solids(context, K + "go"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, K + "gint", { "tools" : qUnion([solids(context, K + "gs"), solids(context, K + "go")]), "operationType" : BooleanOperationType.INTERSECTION });
    // pad screw: clearance through the boss; the M5 nut sits in a pocket opening on the CHILD face (put in before the foam),
    // so the screw (M5x12 socket head from inside the depth piece) pulls it against 2.4 mm of boss; the tip ends in the pocket
    cylAx(context, K + "ph", vector(xb - SHELL_T + 1, PSY, zc), vector(xs + BOSS_H + 1, PSY, zc), M5_CLEAR / 2);
    hexPrism(context, K + "nut", vector(xb - SHELL_T + M5_NUT_P / 2 - 0.01, PSY, zc), vector(1, 0, 0), M5_NUT_AF, M5_NUT_P);
    opBoolean(context, id + "cutop", { "tools" : solids(context, K), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return body;
}

// reference cushion (NOT printed): 30 mm foam + neoprene cover on the child side, so the assembled look is shown honestly
const FOAM = 30.0;
export function buildCushion(context is Context, id is Id, xs is number, zc is number) returns Query
{
    planStrip(context, id + "st", xs, -FOAM, 0.05, zc - PAD_H / 2 - 4, zc + PAD_H / 2 + 4);
    yzPrism(context, id + "ol", padOutlineYZ(zc, -0.8), xs - 80, xs + 20);
    opBoolean(context, id + "int", { "tools" : qUnion([solids(context, id + "st"), solids(context, id + "ol")]), "operationType" : BooleanOperationType.INTERSECTION });
    const body = oneBody(context, id, "cushion");
    const ch = qSubtraction(qOwnedByBody(body, EntityType.EDGE), qWithinRadius(qOwnedByBody(body, EntityType.EDGE), vector(xs, 10, zc) * millimeter, 1 * millimeter));
    // round the child-side rim generously (soft look); skip quietly is NOT allowed: report through the finish list
    var rim = [];
    for (var e in evaluateQuery(context, qOwnedByBody(body, EntityType.EDGE)))
        if (evBox3d(context, { "topology" : e }).maxCorner[0] < (shellXi(10, xs) - FOAM / 2) * millimeter) rim = append(rim, e);
    finishEdges(context, id + "r", qUnion(rim), 10.0, "cushion rim round");
    return body;
}

// =============================================== WING (chair change) ===============================================
// one vertical slot, a rib per position at its back, a catch groove per position in the outer wall, lead-ins at the mouth
export function wingTools(context is Context, id is Id) returns map
{
    tipRBox(context, id + "add", TIP_D[0], TIP_D[1], TIP_T[0], TIP_T[1], TIP_Z[0], TIP_Z[1], 5.0);
    const K = id + "cut";
    tipBox(context, K + "slot", SL_D[0], SL_D[1], SL_T[0], SL_T[1], SL_Z[0], SL_Z[1]);
    // inner wall lead-in 3 x 45 deg
    xyPrism(context, K + "lead", [Q(SL_D[0] + 0.1, TIP_T[0] - 1, 0), Q(SL_D[0] - 3.0, TIP_T[0] - 1, 0), Q(SL_D[0] + 0.1, TIP_T[0] + 3.1, 0)], SL_Z[0], SL_Z[1]);
    // outer wall lead-in (40 deg): pushes the shuttle in as the arm slides home
    const lr = ENG + 0.8;
    xyPrism(context, K + "olead", [Q(SL_D[1] - 0.1, TIP_T[0] - 1, 0), Q(SL_D[1] + lr, TIP_T[0] - 1, 0), Q(SL_D[1] - 0.1, TIP_T[0] + lr / tan(40 * degree), 0)], SL_Z[0], SL_Z[1]);
    // catch grooves (each takes the shuttle of the LOW and of the flipped HIGH carrier); all cuts first (contiguous ids)
    const gl = TG_D[1] + CL + ENG + SH_T[1] - 2.5 + 0.3;           // d + t of the groove's 45 deg wall (shuttle chamfer + 0.3)
    const g0 = SL_D[1] - 0.5;  const g1 = SL_D[1] + ENG + 0.3;
    for (var k = 0; k < size(C_POS); k += 1)
        xyPrism(context, K + ("g" ~ k), [Q(g0, SH_T[0] - CL - 0.2, 0), Q(g1, SH_T[0] - CL - 0.2, 0), Q(g1, gl - g1, 0), Q(g0, gl - g0, 0)], C_POS[k] - SH_Z[1] - 0.4, C_POS[k] + SH_Z[1] + 0.4);
    // rails on the slot's inner wall, one per height position; chamfered front so the tongue's groove finds it
    var ribs = [];
    for (var k = 0; k < size(C_POS); k += 1)
    {
        const c = C_POS[k];  const h = RAIL_H / 2;
        tzPrism(context, id + ("rib" ~ k), [[RAIL_T[0], c - h + 1.5], [RAIL_T[0] + 2.0, c - h], [RAIL_T[1], c - h], [RAIL_T[1], c + h], [RAIL_T[0] + 2.0, c + h], [RAIL_T[0], c + h - 1.5]], SL_D[0] - 0.5, SL_D[0] + RAIL_P);
        ribs = append(ribs, solids(context, id + ("rib" ~ k)));
    }
    return { "add" : solids(context, id + "add"), "cut" : solids(context, K), "ribs" : qUnion(ribs) };
}

// =============================================== KIT ===============================================
export function buildSide(context is Context, id is Id, def is map) returns Query
{
    const k = def.pos == SidePadPos.P1 ? 0 : (def.pos == SidePadPos.P2 ? 1 : 2);
    const c = C_POS[k];
    const low = def.kit == SidePadKit.LOW;
    const zp = low ? c - D_OFF : c + D_OFF;
    const xs = depthX(def.depth);
    const car = buildCarrier(context, id + "car", c, zp);
    tenonCuts(context, id + "tc", car.carrier, car.xe, zp, low ? 1 : -1);
    setProperty(context, { "entities" : car.carrier, "propertyType" : PropertyType.NAME, "value" : "sidepad carrier " ~ (low ? "A" : "A' (flipped)") });
    var parts = [car.carrier];
    if (def.latch)
    {
        setProperty(context, { "entities" : car.button, "propertyType" : PropertyType.NAME, "value" : "sidepad pill button + rod" });
        setProperty(context, { "entities" : car.shuttle, "propertyType" : PropertyType.NAME, "value" : "sidepad shuttle" });
        parts = concatenateArrays([parts, [car.button, car.shuttle]]);
    }
    else opDeleteBodies(context, id + "nolatch", { "entities" : qUnion([car.button, car.shuttle]) });
    const dp = buildDepth(context, id + "dep", xs, zp, car.xe);
    setProperty(context, { "entities" : dp, "propertyType" : PropertyType.NAME, "value" : "sidepad depth piece " ~ toString(def.depth) });
    const pad = buildPadShell(context, id + "pad", xs, zp);
    setProperty(context, { "entities" : pad, "propertyType" : PropertyType.NAME, "value" : "sidepad pad shell" });
    const ring = buildClipRing(context, id + "ring", xs, zp);
    setProperty(context, { "entities" : ring, "propertyType" : PropertyType.NAME, "value" : "sidepad clip ring" });
    if (def.cushion == true)
    {
        const cu = buildCushion(context, id + "cush", xs, zp);
        setProperty(context, { "entities" : cu, "propertyType" : PropertyType.NAME, "value" : "REFERENCE cushion (foam + cover, not printed)" });
        setProperty(context, { "entities" : cu, "propertyType" : PropertyType.APPEARANCE, "value" : color(0.13, 0.17, 0.30) });
    }
    setVariable(context, "gbgRoles", { "carrier" : car.carrier, "button" : def.latch ? car.button : qNothing(), "shuttle" : def.latch ? car.shuttle : qNothing(), "depth" : dp, "pad" : pad, "ring" : ring });
    return qUnion(concatenateArrays([parts, [dp, pad, ring]]));
}


// =============================================== CHECK (all settings, against the real chair) ===============================================
// For every height position x kit x depth piece: build both... the LEFT side, then report
//   - interference (volume) between any two kit parts and between any kit part and any chair part;
//   - the kit's max x (chair arm rests at x 165).
// Each setting is built inside startFeature / abortFeature, so nothing stays in the Part Studio.
function bodyName(context is Context, b) returns string
{
    var nm = "?";
    try silent { nm = replace(getProperty(context, { "entity" : b, "propertyType" : PropertyType.NAME }), "sidepad ", ""); }
    return nm;
}
function checkAll(context is Context, id is Id, definition is map) returns string
{
    var out = "";  var nBad = 0;  var maxX = 0;
    const kits = [SidePadKit.LOW, SidePadKit.HIGH];
    const poss = [SidePadPos.P1, SidePadPos.P2, SidePadPos.P3];
    const deps = [SidePadDepth.D0, SidePadDepth.D1, SidePadDepth.D2, SidePadDepth.D3];
    var n = 0;
    for (var kk in kits) for (var pp in poss) for (var dd in deps)
    {
        const sid = id + ("chk" ~ n);  n += 1;
        const others = evaluateQuery(context, qBodyType(qEverything(EntityType.BODY), BodyType.SOLID));
        var msg = "";
        startFeature(context, sid, definition);
        try
        {
            buildSide(context, sid + "L", { "kit" : kk, "pos" : pp, "depth" : dd, "latch" : true });
            const roles = getVariable(context, "gbgRoles");
            var kit = [];  var names = [];
            for (var r in ["carrier", "button", "shuttle", "depth", "pad", "ring"]) { kit = append(kit, roles[r]); names = append(names, r); }
            for (var b in kit) maxX = max(maxX, evBox3d(context, { "topology" : b }).maxCorner[0] / millimeter);
            for (var i = 0; i < size(kit); i += 1)
                for (var j = i + 1; j < size(kit); j += 1)
                {
                    var hit = false;
                    for (var cc in evCollision(context, { "tools" : kit[i], "targets" : kit[j] })) if (cc["type"] == ClashType.INTERFERE) hit = true;
                    if (hit) msg = msg ~ names[i] ~ " x " ~ names[j] ~ ", ";
                }
            for (var i = 0; i < size(kit); i += 1)
                for (var o in others)
                {
                    var hit = false;
                    for (var cc in evCollision(context, { "tools" : kit[i], "targets" : o })) if (cc["type"] == ClashType.INTERFERE) hit = true;
                    if (hit)
                    {
                        const ob = evBox3d(context, { "topology" : o });
                        msg = msg ~ names[i] ~ " x chair[" ~ roundToPrecision(ob.minCorner[0] / millimeter, 0) ~ ".." ~ roundToPrecision(ob.maxCorner[0] / millimeter, 0) ~ ", z " ~ roundToPrecision(ob.minCorner[2] / millimeter, 0) ~ ".." ~ roundToPrecision(ob.maxCorner[2] / millimeter, 0) ~ "], ";
                    }
                }
        }
        catch (e) { msg = "BUILD FAILED " ~ toString(e); }
        abortFeature(context, sid);
        if (msg != "") { nBad += 1; out = out ~ toString(kk) ~ "/" ~ toString(pp) ~ "/" ~ toString(dd) ~ ": " ~ msg ~ "; "; }
    }
    return n ~ " settings, " ~ nBad ~ " with clashes; kit max x " ~ roundToPrecision(maxX, 1) ~ (out == "" ? "" : " -- " ~ out);
}

annotation { "Feature Type Name" : "GbG Side Pad v1", "Feature Type Description" : "Side pad kit v1.1 (carrier with flush pill release, depth piece, pad shell, clip ring) for GbG_chair_forth; needs GbG Side Pad Wing" }
export const gbgSidePadV1 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Side" }
        definition.side is SidePadSide;
        annotation { "Name" : "Kit" }
        definition.kit is SidePadKit;
        annotation { "Name" : "Height position" }
        definition.pos is SidePadPos;
        annotation { "Name" : "Depth piece" }
        definition.depth is SidePadDepth;
        annotation { "Name" : "Release (pill button + shuttle)" }
        definition.latch is boolean;
        annotation { "Name" : "Show the cushion (reference, not printed)" }
        definition.cushion is boolean;
        annotation { "Name" : "Check all 24 settings against the chair (slow; builds nothing)" }
        definition.check is boolean;
    }
    {
        setVariable(context, "gbgSkipped", "");
        if (definition.check) { reportFeatureInfo(context, id, "CHECK: " ~ checkAll(context, id, definition)); return; }
        if (definition.side != SidePadSide.RIGHT) buildSide(context, id + "L", definition);
        if (definition.side != SidePadSide.LEFT)
        {
            const r = buildSide(context, id + "R", definition);
            opTransform(context, id + "mir", { "bodies" : r, "transform" : mirrorAcross(plane(v3(0, 0, 0), vector(1, 0, 0))) });
        }
        var msg = "";
        try silent { msg = getVariable(context, "gbgSkipped"); }
        if (msg != "") reportFeatureWarning(context, id, "Finish skipped: " ~ msg);
    }, { "side" : SidePadSide.LEFT, "kit" : SidePadKit.LOW, "pos" : SidePadPos.P2, "depth" : SidePadDepth.D0, "latch" : true, "cushion" : true, "check" : false });

annotation { "Feature Type Name" : "GbG Side Pad Wing (chair)", "Feature Type Description" : "Adds the side-pad socket v1.1 (vertical slot, a rib and a catch groove per height position, lead-ins) to the chair's wings" }
export const gbgSidePadWing = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Wings (empty = found automatically)", "Filter" : EntityType.BODY && BodyType.SOLID, "MaxNumberOfPicks" : 2 }
        definition.wings is Query;
        annotation { "Name" : "Remove the modelled wing print supports (support_left/right_wing)", "Default" : true }
        definition.dropSupports is boolean;
        annotation { "Name" : "Test only: use stand-in blocks for the wings" }
        definition.standIn is boolean;
    }
    {
        // support_left/right_wing are 1.5 mm plates touching the wing along one 10 mm line: print supports for printing
        // the wing face-down, not load paths. They sit where the HIGH kit's pad goes at height steps 0-2 (the review
        // matrix's only clashes), so the reprinted wing drops them and the slicer generates supports instead.
        if (definition.dropSupports)
        {
            var sup = [];
            for (var b in evaluateQuery(context, qBodyType(qEverything(EntityType.BODY), BodyType.SOLID)))
            {
                var n = "";
                try silent { n = getProperty(context, { "entity" : b, "propertyType" : PropertyType.NAME }); }
                var isPlate = false;
                try silent
                {
                    const bb = evBox3d(context, { "topology" : b });
                    const lo = bb.minCorner / millimeter;  const hi = bb.maxCorner / millimeter;
                    const v = evVolume(context, { "entities" : b }) / (centimeter ^ 3);
                    isPlate = v < 6 && min(abs(lo[0]), abs(hi[0])) > 95 && max(abs(lo[0]), abs(hi[0])) < 145 &&
                              lo[1] > 40 && hi[1] < 110 && lo[2] > 255 && hi[2] < 315;
                }
                if (n == "support_left_wing" || n == "support_right_wing" || isPlate) sup = append(sup, b);
            }
            if (size(sup) > 0) opDeleteBodies(context, id + "dropSup", { "entities" : qUnion(sup) });
        }
        for (var s in [1, -1])
        {
            const sid = id + (s > 0 ? "L" : "R");
            var wing = qNothing();
            if (definition.standIn)
            {
                tipBox(context, sid + "stand", -4, 20, -14, 40, 179, 390);
                wing = qCreatedBy(sid + "stand", EntityType.BODY);
                if (s < 0) opTransform(context, sid + "sm", { "bodies" : wing, "transform" : mirrorAcross(plane(v3(0, 0, 0), vector(1, 0, 0))) });
            }
            else
            {
                const probe = Q(9, 20, 300);
                const hit = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * probe[0], probe[1], probe[2])));
                if (size(hit) != 1) throw regenError("Side Pad Wing: could not find the " ~ (s > 0 ? "left" : "right") ~ " wing (found " ~ size(hit) ~ " bodies)");
                wing = hit[0];
            }
            var t = {};
            try { t = wingTools(context, sid + "t"); }
            catch (e) { throw regenError("Side Pad Wing: tools failed: " ~ toString(e)); }
            if (s < 0) opTransform(context, sid + "tm", { "bodies" : qUnion([t.add, t.cut, t.ribs]), "transform" : mirrorAcross(plane(v3(0, 0, 0), vector(1, 0, 0))) });
            // the tip block never grows into a neighbouring chair part (the wing bracket sits against the wing's inner face)
            const others = qSubtraction(qSubtraction(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), wing), qUnion([qCreatedBy(sid + "t", EntityType.BODY), qCreatedBy(sid + "stand", EntityType.BODY)]));
            if (size(evaluateQuery(context, others)) > 0)
            {
                opPattern(context, sid + "oc", { "entities" : others, "transforms" : [identityTransform()], "instanceNames" : ["o"] });
                opBoolean(context, sid + "oadd", { "tools" : qCreatedBy(sid + "oc", EntityType.BODY), "targets" : t.add, "operationType" : BooleanOperationType.SUBTRACTION });
            }
            opBoolean(context, sid + "add", { "tools" : qUnion([wing, t.add]), "operationType" : BooleanOperationType.UNION });
            const keep = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * Q(-3, 30, 300)[0], Q(-3, 30, 300)[1], 300)));
            if (size(keep) != 1) throw regenError("Side Pad Wing: merged wing not found");
            try { opBoolean(context, sid + "cut", { "tools" : t.cut, "targets" : keep[0], "operationType" : BooleanOperationType.SUBTRACTION }); }
            catch (e) { throw regenError("Side Pad Wing: cut failed: " ~ toString(e)); }
            try { opBoolean(context, sid + "rib", { "tools" : qUnion([keep[0], t.ribs]), "operationType" : BooleanOperationType.UNION }); }
            catch (e) { throw regenError("Side Pad Wing: ribs failed: " ~ toString(e)); }
        }
    }, { "standIn" : false, "dropSupports" : true });
