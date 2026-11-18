FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// ===================================================================================================================
// GoBabyGo SIDE PADS (lateral trunk supports) v1.0 for GbG_chair_forth.  Native FeatureScript port of the reviewed
// Python sketch model (research: options_v05, tools/v7..v10_geom.py; design freeze 2026-11-18).
//
// Kit (per side): CARRIER (tongue in the wing slot + swept neck + dovetail half), DEPTH PIECE N0/N12/N24/N36 (dovetail
// half + straight neck + flared oval foot), PAD SHELL (curved strip, grooved foot seat, captive M5 nut pockets, cover
// bead + groove), CLIP RING (traps the neoprene cover in the groove: no sewing), TILT SHIMS 8 / 15 deg (flip = minus),
// SNAP-BUTTON BOLT + retainer washer (one spring from home stock, OD <= 6), cutting templates.
// Chair change (separate feature "GbG Side Pad Wing"): a vertical slot in each wing tip, comb ribs every 12.5 mm at the
// back of the slot, a column of Ø6.4 bolt holes (one per height step) in a 12 mm recess on the outer face, entry ramp.
//
// Loads: comb ribs carry up/down, slot walls carry the lean twist, the dovetail and the foot-in-seat carry the arm joints.
// Screws (from home stock, DIN 7991 + DIN 934 nuts, no printed threads) only hold parts together.
// Frame: chair WORLD, mm, z up, -y = front; the LEFT side is built at +x, the right side is its mirror.
// Wing-tip frame (d, t) at ZREF: Q(d, t) = P0 + d * AXh + t * TH + (ZREF - Z0) * SHEAR  (d across the tip, t into the wing).
// ===================================================================================================================

export enum SidePadSide { annotation { "Name" : "Left (+x)" } LEFT, annotation { "Name" : "Right (-x)" } RIGHT, annotation { "Name" : "Both" } BOTH }
export enum SidePadKit { annotation { "Name" : "LOW (pad below the tongue)" } LOW, annotation { "Name" : "HIGH (carrier flipped: pad above)" } HIGH }
export enum SidePadDepth { annotation { "Name" : "N0  (pad face x 118)" } N0, annotation { "Name" : "N12 (x 106)" } N12, annotation { "Name" : "N24 (x 94)" } N24, annotation { "Name" : "N36 (x 82)" } N36 }
export enum SidePadSeat { annotation { "Name" : "-6.25" } DOWN, annotation { "Name" : "0" } MID, annotation { "Name" : "+6.25" } UP }
export enum SidePadTilt { annotation { "Name" : "none" } T0, annotation { "Name" : "8 deg top in" } P8, annotation { "Name" : "8 deg top out" } M8, annotation { "Name" : "15 deg top in" } P15, annotation { "Name" : "15 deg top out" } M15 }

// ---------------- chair / wing-tip frame ----------------
const P0 = [131.3, 80.7];  const AXH = [0.79577626, 0.60559074];  const TH = [-0.6083053, 0.79370313];
const Z0 = 227.8;  const SHEAR = [0.8 / 32.9, 2.7 / 32.9];  const ZREF = 244.25;
// ---------------- wing tip (chair change) ----------------
const TIP_D = [-7.0, 27.0];  const TIP_T = [-16.0, 38.0];  const TIP_Z = [193.0, 347.0];
const SL_D = [1.2, 17.6];  const SL_T = [-30.0, 28.0];  const SL_Z = [197.0, 343.0];      // vertical slot, open at the front
const RIB_T = [22.0, 28.5];  const RIB_H = 5.6;  const RIB_Z0 = 203.25;  const PITCH = 12.5;
const BOLT_T = 8.0;                         // bolt axis position along the slot (t)
const HOLE_R = 3.2;  const RECESS_R = 6.0;  const WALL = 3.0;                               // Ø6.4 holes, 3 mm wall, Ø12 recess track
const RAMP = 4.0;                           // entry ramp on the outer wall at the slot mouth (t -30 .. -14)
// ---------------- carrier ----------------
const TG_D = [1.4, 17.4];  const TG_T = [-14.0, 27.5];  const TG_H = 25.0;                 // tongue 16 x 41.5 x 50
const NOTCH = [-18.75, -6.25, 6.25, 18.75];
const D_OFF = 50.0;                         // pad centre 50 below the tongue (LOW) / above (HIGH, flipped)
const T_POS0 = 222.0;                       // tongue centre positions T_POS0 + 12.5 k, k = 0..7
const NK_Y = [25.0, 43.0];  const NK_HZ = 18.0;  const NK_R = 6.0;                         // neck section 18 x 36, centre y 34
const J0 = 128.0;  const J1 = 146.0;  const JS = 137.0;                                     // dovetail joint zone, retainer screw
const KEY_LO = -6.0;  const KEY_HI = 2.0;  const FL = 3.5;  const ENDL = 2.5;
// ---------------- snap-button bolt ----------------
const B_BORE_R = 4.7;  const B_BORE_D0 = 3.0;                                               // Ø9.4 bore in the tongue from d 3 to the outer face
const B_SH_R = 4.5;  const B_HEAD_R = 3.0;  const B_SPR_R = 3.3;
const WASH_T = 1.6;                                                                         // retainer washer, 2x M3 csk + nuts
// ---------------- pad ----------------
const PAD_Y0 = -31.0;  const PAD_Y1 = 51.0;  const PAD_H = 80.0;  const SHELL_T = 4.5;
const A_REF = 85.0;  const B_REF = 88.0;  const YC_REF = 30.0;  const WRAP = 0.7;  const SE_P = 4.0;
const FOOT_Y = [4.0, 42.0];  const FOOT_HZ = 20.0;  const NOSE = 1.5;  const SEAT_D = 1.5;  const FINE = 6.25;
const PSY = 15.0;                                                                           // pad screws (y), z = +-10
const BEAD = [1.0, 7.5];  const GROOVE = [2.5, 6.5];  const BEAD_H = 3.5;  const GROOVE_D = 3.0;  // cover bead + groove (insets from the pad edge)
// ---------------- fits (P1S / PETG) ----------------
const CL = 0.2;  const M5_CLEAR = 5.5;  const M5_NUT_AF = 8.3;  const M5_NUT_T = 4.4;  const CSK_M5_R = 5.2;
const M3_CLEAR = 3.4;  const M3_NUT_AF = 5.8;  const M3_NUT_T = 2.8;  const CSK_M3_R = 3.2;

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
    return vector(P0[0] + d * AXH[0] + t * TH[0] + (ZREF - Z0) * SHEAR[0], P0[1] + d * AXH[1] + t * TH[1] + (ZREF - Z0) * SHEAR[1], z);
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
// closed planar section (array of [x, y, z]) -> face; sections -> lofted solid (fallback: pairwise lofts, united)
function sectionFace(context is Context, id is Id, pts is array) returns Query
{
    var p = [];
    for (var q in pts) p = append(p, vv(q));
    p = append(p, vv(pts[0]));
    opFitSpline(context, id + "w", { "points" : p });
    opFillSurface(context, id + "f", { "edgesG0" : qCreatedBy(id + "w", EntityType.EDGE) });
    return qCreatedBy(id + "f", EntityType.FACE);
}
function loftSections(context is Context, id is Id, secs is array)
{
    var faces = [];
    for (var i = 0; i < size(secs); i += 1) faces = append(faces, sectionFace(context, id + ("s" ~ i), secs[i]));
    try silent { opLoft(context, id + "loft", { "profileSubqueries" : faces, "bodyType" : ToolBodyType.SOLID }); }
    if (size(evaluateQuery(context, solids(context, id + "loft"))) == 0)
    {
        for (var i = 0; i + 1 < size(faces); i += 1)
            try silent { opLoft(context, id + ("pc" ~ i), { "profileSubqueries" : [faces[i], faces[i + 1]], "bodyType" : ToolBodyType.SOLID }); }
        if (size(evaluateQuery(context, solids(context, id))) > 1)
            opBoolean(context, id + "pu", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    }
    cleanup(context, id + "cl", id);
}
// rounded-rect section (w across, h tall) centred c, in the plane normal to the horizontal tangent tg
function rsec(c is Vector, tg is Vector, w is number, h is number, r is number) returns array
{
    var t = normalize(vector(tg[0], tg[1], 0));
    const n = cross(vector(0, 0, 1), t);
    var o = [];
    const corners = [[1, 1, 0], [-1, 1, 90], [-1, -1, 180], [1, -1, 270]];
    for (var cc in corners)
    {
        const cx = cc[0] * (w / 2 - r);  const cz = cc[1] * (h / 2 - r);
        for (var k = 0; k < 6; k += 1)
        {
            const a = (cc[2] + 90 * k / 5) * degree;
            const u = cx + r * cos(a);  const v = cz + r * sin(a);
            const p = c + n * u + vector(0, 0, v);
            o = append(o, [p[0], p[1], p[2]]);
        }
    }
    return o;
}
// superellipse oval (exponent 2.6) in the plane x = const, centre (yc, zc)
function ovalX(x is number, yc is number, zc is number, ry is number, rz is number) returns array
{
    var o = [];
    for (var k = 0; k < 40; k += 1)
    {
        const a = 2 * PI * k / 40;
        const c = cos(a * radian);  const s = sin(a * radian);
        o = append(o, [x, yc + ry * sgn(c) * abs(c) ^ (2 / 2.6), zc + rz * sgn(s) * abs(s) ^ (2 / 2.6)]);
    }
    return o;
}
function bez(p0 is Vector, p1 is Vector, p2 is Vector, p3 is Vector, s is number) returns Vector
{
    return p0 * (1 - s) ^ 3 + p1 * 3 * (1 - s) ^ 2 * s + p2 * 3 * (1 - s) * s ^ 2 + p3 * s ^ 3;
}
// straight neck bar along x (rounded 18 x 36 section)
function barX(context is Context, id is Id, x0 is number, x1 is number, zc is number)
{
    loftSections(context, id, [rsec(vector(x0, (NK_Y[0] + NK_Y[1]) / 2, zc), vector(1, 0, 0), NK_Y[1] - NK_Y[0], 2 * NK_HZ, NK_R),
                                rsec(vector(x1, (NK_Y[0] + NK_Y[1]) / 2, zc), vector(1, 0, 0), NK_Y[1] - NK_Y[0], 2 * NK_HZ, NK_R)]);
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
// keyed dovetail split region (x-z polygon extruded through y): the part BELOW the split line (carrier when LOW)
function splitRegion(context is Context, id is Id, zc is number, below is boolean)
{
    const x0 = J0 - 0.01;  const x1 = J1 + 0.01;  const s = below ? 1 : -1;
    const S = [[x0, zc + s * KEY_LO], [J0 + ENDL + FL, zc + s * KEY_LO], [J0 + ENDL, zc + s * KEY_HI], [J1 - ENDL, zc + s * KEY_HI], [J1 - ENDL - FL, zc + s * KEY_LO], [x1, zc + s * KEY_LO]];
    const far = zc - s * 60;
    var poly = concatenateArrays([S, [[x1, far], [x0, far]]]);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, 0), vector(0, -1, 0), vector(1, 0, 0)) });
    for (var i = 0; i < size(poly); i += 1)
    {
        const a = poly[i];  const b = poly[(i + 1) % size(poly)];
        skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
    }
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : 100 * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}

// =============================================== CARRIER ===============================================
// built for the LEFT side; c = tongue centre z, zp = pad / neck centre z (c - 50 LOW, c + 50 HIGH)
export function buildCarrier(context is Context, id is Id, c is number, zp is number) returns Query
{
    const low = zp < c;
    // tongue in the slot
    tipRBox(context, id + "tg", TG_D[0], TG_D[1], TG_T[0], TG_T[1], c - TG_H, c + TG_H, 1.5);
    // swept neck: Bezier from the tongue nose (heading out of the slot) to the joint end (heading -x)
    const nose = Q((TG_D[0] + TG_D[1]) / 2, TG_T[0] + 4, c);
    const dout = -dirT();
    const endp = vector(J1, (NK_Y[0] + NK_Y[1]) / 2, zp);
    const p1 = nose + dout * 4;  const p2 = endp + vector(1, 0, 0) * 8;
    var secs = [];
    for (var s in linspace(0, 1, 13))
    {
        const c0 = bez(nose, p1, p2, endp, s);
        const tg = bez(nose, p1, p2, endp, min(s + 0.02, 1)) - bez(nose, p1, p2, endp, max(s - 0.02, 0));
        secs = append(secs, rsec(c0, tg, lerp(TG_D[1] - TG_D[0], NK_Y[1] - NK_Y[0], s), lerp(2 * TG_H, 2 * NK_HZ, s), 5.0));
    }
    loftSections(context, id + "sw", secs);
    // the sweep stops at the joint face (x J1) where it is behind the pad (y < 56)
    fCuboid(context, id + "swc", { "corner1" : v3(0, 0, zp - 60), "corner2" : v3(J1, 56, zp + 60) });
    opBoolean(context, id + "swcut", { "tools" : qCreatedBy(id + "swc", EntityType.BODY), "targets" : solids(context, id + "sw"), "operationType" : BooleanOperationType.SUBTRACTION });
    // joint half
    barX(context, id + "jb", J0, J1 + 2, zp);
    splitRegion(context, id + "jr", zp, low);
    opBoolean(context, id + "jint", { "tools" : qUnion([solids(context, id + "jb"), solids(context, id + "jr")]), "operationType" : BooleanOperationType.INTERSECTION });
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const body = oneBody(context, id, "carrier");
    // cuts: comb notches, bolt bore + washer pocket + 2 M3, retainer M5 + nut (6 mm up), all through tool bodies
    const C = id + "cut";
    for (var i = 0; i < size(NOTCH); i += 1)
        tipBox(context, C + ("n" ~ i), TG_D[0] - 1, TG_D[1] + 1, RIB_T[0] - 0.4, TG_T[1] + 1, c + NOTCH[i] - 3.1, c + NOTCH[i] + 3.1);
    cylAx(context, C + "bore", Q(B_BORE_D0, BOLT_T, c), Q(TG_D[1] + 1, BOLT_T, c), B_BORE_R);
    tipBox(context, C + "wp", TG_D[1] - WASH_T, TG_D[1] + 1, BOLT_T - 10, BOLT_T + 10, c - 6, c + 6);
    for (var s in [-1, 1])
    {
        // 2x M3x10 csk through the washer into nuts; each nut drops in from the inner face through a hex channel
        cylAx(context, C + ("m3" ~ s), Q(TG_D[1] - 9, BOLT_T + s * 7.5, c), Q(TG_D[1] + 1, BOLT_T + s * 7.5, c), M3_CLEAR / 2);
        hexPrism(context, C + ("m3n" ~ s), Q((TG_D[0] - 1 + TG_D[1] - 7) / 2, BOLT_T + s * 7.5, c), dirD(), M3_NUT_AF, (TG_D[1] - 7) - (TG_D[0] - 1));
    }
    tdHole(context, C + "js", vector(JS, 34, zp - NK_HZ - 2), vector(JS, 34, zp + NK_HZ + 2), M5_CLEAR / 2, vector(0, 1, 0));
    hexPrism(context, C + "jn", vector(JS, 34, low ? zp - NK_HZ + 6 + M5_NUT_T / 2 : zp + NK_HZ - 6 - M5_NUT_T / 2), vector(0, 0, 1), M5_NUT_AF, M5_NUT_T);
    // nut access channel from the bottom face up to the nut (insert the nut from below)
    cylAx(context, C + "jna", vector(JS, 34, low ? zp - NK_HZ - 1 : zp + NK_HZ + 1), vector(JS, 34, low ? zp - NK_HZ + 6 : zp + NK_HZ - 6), M5_NUT_AF / sqrt(3) + 0.05);
    opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return body;
}

// =============================================== DEPTH PIECE ===============================================
export function depthX(d is SidePadDepth) returns number
{
    return d == SidePadDepth.N0 ? 118 : (d == SidePadDepth.N12 ? 106 : (d == SidePadDepth.N24 ? 94 : 82));
}
export function buildDepth(context is Context, id is Id, xs is number, zp is number, low is boolean) returns Query
{
    barX(context, id + "jb", J0, J1, zp);
    splitRegion(context, id + "jr", zp, low);
    opBoolean(context, id + "jcut", { "tools" : solids(context, id + "jr"), "targets" : solids(context, id + "jb"), "operationType" : BooleanOperationType.SUBTRACTION });
    const L = 10.0;
    if (xs + L < J0) barX(context, id + "nk", xs + L, J0 + 0.02, zp);
    // trumpet flare: neck section -> oval foot with a concave lip
    const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
    var rings = [rsec(vector(min(xs + L, J0 + 0.02), (NK_Y[0] + NK_Y[1]) / 2, zp), vector(1, 0, 0), NK_Y[1] - NK_Y[0], 2 * NK_HZ, NK_R)];
    for (var u in linspace(L * 0.8, 0.6, 6))
    {
        const f = 1 - u / L;
        const ry = 9 + ((FOOT_Y[1] - FOOT_Y[0]) / 2 - 9) * (1 - (1 - f) ^ 2) + 3 * f ^ 3;
        const rz = NK_HZ + (FOOT_HZ - NK_HZ) * f + 3 * f ^ 3;
        const ycu = 34 + (yc - 34) * (1 - (1 - f) ^ 2);
        rings = append(rings, ovalX(xs + u, ycu, zp, ry, rz));
    }
    loftSections(context, id + "fl", rings);
    // nose into the seat + 2 ribs into the seat grooves
    loftSections(context, id + "ns", [ovalX(xs + 0.6, yc, zp, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 3.0, FOOT_HZ - 3.0), ovalX(xs - NOSE, yc, zp, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 3.6, FOOT_HZ - 3.6)]);
    for (var r in [-9.375, 9.375])
        fCuboid(context, id + ("rib" ~ (r > 0 ? "u" : "d")), { "corner1" : v3(xs - NOSE - 1.0, FOOT_Y[0] + 8, zp + r - 1.4), "corner2" : v3(xs - NOSE + 0.2, FOOT_Y[1] - 8, zp + r + 1.4) });
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const body = oneBody(context, id, "depth piece");
    const C = id + "cut";
    // retainer M5x30 (head flush, countersunk on the free face)
    tdHole(context, C + "js", vector(JS, 34, zp - NK_HZ - 2), vector(JS, 34, zp + NK_HZ + 2), M5_CLEAR / 2, vector(0, 1, 0));
    const zh = low ? zp + NK_HZ : zp - NK_HZ;  const sh = low ? 1 : -1;
    fCone(context, C + "jsk", { "bottomCenter" : v3(JS, 34, zh - sh * (CSK_M5_R - 2.75)), "topCenter" : v3(JS, 34, zh + sh * 0.05), "bottomRadius" : 2.75 * millimeter, "topRadius" : CSK_M5_R * millimeter });
    // pad screws from OUTSIDE: vertical slots (fine height) + spot-face slots for the heads
    for (var dz in [-10, 10])
    {
        for (var j in [-1, 1]) cylAx(context, C + ("ps" ~ dz ~ j), vector(xs - 3, PSY, zp + dz + j * FINE), vector(xs + 14, PSY, zp + dz + j * FINE), M5_CLEAR / 2);
        fCuboid(context, C + ("psb" ~ dz), { "corner1" : v3(xs - 3, PSY - M5_CLEAR / 2, zp + dz - FINE), "corner2" : v3(xs + 14, PSY + M5_CLEAR / 2, zp + dz + FINE) });
        for (var j in [-1, 1]) cylAx(context, C + ("pc" ~ dz ~ j), vector(xs + 4.5, PSY, zp + dz + j * FINE), vector(xs + 24, PSY, zp + dz + j * FINE), 5.2);
        fCuboid(context, C + ("pcb" ~ dz), { "corner1" : v3(xs + 4.5, PSY - 5.2, zp + dz - FINE), "corner2" : v3(xs + 24, PSY + 5.2, zp + dz + FINE) });
    }
    opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return body;
}

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
    var p = [];
    for (var y in linspace(PAD_Y0 - 2, PAD_Y1 + 2, 30)) p = append(p, vector(shellXi(y, xs) + a, y) * millimeter);
    for (var y in linspace(PAD_Y1 + 2, PAD_Y0 - 2, 30)) p = append(p, vector(shellXi(y, xs) + b, y) * millimeter);
    p = append(p, p[0]);
    for (var i = 0; i + 1 < size(p); i += 1) skLineSegment(sk, "l" ~ i, { "start" : p[i], "end" : p[i + 1] });
    skSolve(sk);
    opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
export function buildPadShell(context is Context, id is Id, xs is number, zc is number) returns Query
{
    // shell = plan strip (thickness SHELL_T) INTERSECT side outline prism
    planStrip(context, id + "st", xs, 0, SHELL_T, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "ol", padOutlineYZ(zc, 0), xs - 80, xs + 20);
    opBoolean(context, id + "int", { "tools" : qUnion([solids(context, id + "st"), solids(context, id + "ol")]), "operationType" : BooleanOperationType.INTERSECTION });
    // cover bead on the back face (rim band), with the groove for the clip ring
    planStrip(context, id + "bs", xs, SHELL_T - 0.2, SHELL_T + BEAD_H, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "bo", padOutlineYZ(zc, BEAD[0]), xs - 80, xs + 20);
    yzPrism(context, id + "bi", padOutlineYZ(zc, BEAD[1]), xs - 80, xs + 20);
    opBoolean(context, id + "bring", { "tools" : solids(context, id + "bi"), "targets" : solids(context, id + "bo"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "bint", { "tools" : qUnion([solids(context, id + "bs"), solids(context, id + "bo")]), "operationType" : BooleanOperationType.INTERSECTION });
    // inner thickening behind the foot seat (child side, under the EVA)
    const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
    loftSections(context, id + "boss", [ovalX(xs - SHELL_T + 0.5, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 + 2, FOOT_HZ + FINE + 2), ovalX(xs - SHELL_T - 2.5, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 1, FOOT_HZ + FINE - 1)]);
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const body = oneBody(context, id, "pad shell");
    const C = id + "cut";
    // groove for the clip ring
    planStrip(context, C + "gs", xs, SHELL_T + BEAD_H - GROOVE_D, SHELL_T + BEAD_H + 1, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, C + "go", padOutlineYZ(zc, GROOVE[0]), xs - 80, xs + 20);
    yzPrism(context, C + "gi", padOutlineYZ(zc, GROOVE[1]), xs - 80, xs + 20);
    opBoolean(context, C + "gring", { "tools" : solids(context, C + "gi"), "targets" : solids(context, C + "go"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, C + "gint", { "tools" : qUnion([solids(context, C + "gs"), solids(context, C + "go")]), "operationType" : BooleanOperationType.INTERSECTION });
    // foot seat: recess 12.5 taller than the foot nose, grooves every 6.25 in its floor
    loftSections(context, C + "seat", [ovalX(xs + 0.1, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 2.8, FOOT_HZ - 2.8 + FINE), ovalX(xs - SEAT_D - 0.1, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 3.4, FOOT_HZ - 3.4 + FINE)]);
    for (var k = -5; k <= 5; k += 1)
    {
        const z = zc + 3.125 + 6.25 * k;
        if (abs(z - zc) > FOOT_HZ + 3) continue;
        fCuboid(context, C + ("g" ~ (k + 5)), { "corner1" : v3(xs - SEAT_D - 1.2, FOOT_Y[0] + 7.5, z - 1.55), "corner2" : v3(xs - SEAT_D + 0.05, FOOT_Y[1] - 7.5, z + 1.55) });
    }
    // 2 holes + captive M5 nut pockets opening on the child side (under the EVA)
    const x0 = xs - SHELL_T - 2.5;
    for (var dz in [-10, 10])
    {
        cylAx(context, C + ("h" ~ dz), vector(xs - 15, PSY, zc + dz), vector(xs + 2, PSY, zc + dz), M5_CLEAR / 2);
        hexPrism(context, C + ("n" ~ dz), vector(x0 + 1.15, PSY, zc + dz), vector(1, 0, 0), M5_NUT_AF, M5_NUT_T + 0.3);
    }
    opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    return body;
}
// clip ring: a flange band over the bead with a tongue that presses the cover into the groove
export function buildClipRing(context is Context, id is Id, xs is number, zc is number) returns Query
{
    planStrip(context, id + "fs", xs, SHELL_T + BEAD_H + 0.4, SHELL_T + BEAD_H + 2.0, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "fo", padOutlineYZ(zc, BEAD[0] - 0.5), xs - 80, xs + 20);
    yzPrism(context, id + "fi", padOutlineYZ(zc, BEAD[1] + 0.5), xs - 80, xs + 20);
    opBoolean(context, id + "fr", { "tools" : solids(context, id + "fi"), "targets" : solids(context, id + "fo"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "fint", { "tools" : qUnion([solids(context, id + "fs"), solids(context, id + "fo")]), "operationType" : BooleanOperationType.INTERSECTION });
    planStrip(context, id + "ts", xs, SHELL_T + BEAD_H - GROOVE_D + 0.4, SHELL_T + BEAD_H + 0.5, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
    yzPrism(context, id + "to", padOutlineYZ(zc, (GROOVE[0] + GROOVE[1]) / 2 - 0.8), xs - 80, xs + 20);
    yzPrism(context, id + "ti", padOutlineYZ(zc, (GROOVE[0] + GROOVE[1]) / 2 + 0.8), xs - 80, xs + 20);
    opBoolean(context, id + "tr", { "tools" : solids(context, id + "ti"), "targets" : solids(context, id + "to"), "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "tint", { "tools" : qUnion([solids(context, id + "ts"), solids(context, id + "to")]), "operationType" : BooleanOperationType.INTERSECTION });
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    return oneBody(context, id, "clip ring");
}

// =============================================== BOLT ===============================================
export function buildBolt(context is Context, id is Id, c is number) returns Query
{
    // locked position: head tip 0.5 proud of the recess floor (d = SL_D[1] + WALL + 0.5)
    const tip = SL_D[1] + WALL + 0.5;  const headL = tip - (TG_D[1] - WASH_T);
    const dsh1 = TG_D[1] - WASH_T - 0.1;  const dsh0 = dsh1 - 7.0;
    cylAx(context, id + "sh", Q(dsh0, BOLT_T, c), Q(dsh1, BOLT_T, c), B_SH_R);
    cylAx(context, id + "hd", Q(dsh1 - 0.01, BOLT_T, c), Q(tip - 1.0, BOLT_T, c), B_HEAD_R);
    opSphere(context, id + "dome", { "center" : Q(tip - 1.0, BOLT_T, c) * millimeter, "radius" : B_HEAD_R * millimeter });
    opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
    const b = oneBody(context, id, "bolt");
    cylAx(context, id + "sp", Q(dsh0 - 1, BOLT_T, c), Q(dsh0 + 5.0, BOLT_T, c), B_SPR_R);
    opBoolean(context, id + "spc", { "tools" : solids(context, id + "sp"), "targets" : b, "operationType" : BooleanOperationType.SUBTRACTION });
    return b;
}
export function buildWasher(context is Context, id is Id, c is number) returns Query
{
    tipRBox(context, id + "w", TG_D[1] - WASH_T, TG_D[1] - 0.05, BOLT_T - 9.8, BOLT_T + 9.8, c - 5.8, c + 5.8, 0.6);
    const w = oneBody(context, id, "washer");
    const C = id + "cut";
    cylAx(context, C + "h", Q(TG_D[1] - 3, BOLT_T, c), Q(TG_D[1] + 1, BOLT_T, c), B_HEAD_R + 0.25);
    for (var s in [-1, 1])
    {
        cylAx(context, C + ("m" ~ s), Q(TG_D[1] - 3, BOLT_T + s * 7.5, c), Q(TG_D[1] + 1, BOLT_T + s * 7.5, c), M3_CLEAR / 2);
        fCone(context, C + ("k" ~ s), { "bottomCenter" : Q(TG_D[1] - 0.05 - (CSK_M3_R - 1.7), BOLT_T + s * 7.5, c) * millimeter, "topCenter" : Q(TG_D[1] + 0.01, BOLT_T + s * 7.5, c) * millimeter, "bottomRadius" : 1.7 * millimeter, "topRadius" : CSK_M3_R * millimeter });
    }
    opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : w, "operationType" : BooleanOperationType.SUBTRACTION });
    return w;
}

// =============================================== SHIM ===============================================
export function buildShim(context is Context, id is Id, xs is number, zc is number, ang is number) returns Query
{
    const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
    const t = 2 * FOOT_HZ * tan(abs(ang) * degree);
    var lo = ovalX(xs, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 0.2, FOOT_HZ - 0.2);
    var hi = [];
    for (var p in lo)
    {
        const f = (p[2] - (zc - FOOT_HZ)) / (2 * FOOT_HZ);
        const fr = ang > 0 ? f : 1 - f;
        hi = append(hi, [xs - 2.0 - t * fr, p[1], p[2]]);
    }
    loftSections(context, id + "w", [lo, hi]);
    return solids(context, id + "w");
}

// =============================================== WING (chair change) ===============================================
export function wingTools(context is Context, id is Id) returns map
{
    tipRBox(context, id + "add", TIP_D[0], TIP_D[1], TIP_T[0], TIP_T[1], TIP_Z[0], TIP_Z[1], 5.0);
    const C = id + "cut";
    tipBox(context, C + "slot", SL_D[0], SL_D[1], SL_T[0], SL_T[1], SL_Z[0], SL_Z[1]);
    // entry ramp on the outer wall at the mouth (30 deg lead-in that depresses the bolt)
    xyPrism(context, C + "ramp", [Q(SL_D[1] - 0.1, TIP_T[0] - 1, 0), Q(SL_D[1] + RAMP, TIP_T[0] - 1, 0), Q(SL_D[1] - 0.1, TIP_T[0] + RAMP / tan(30 * degree), 0)], SL_Z[0], SL_Z[1]);
    // bolt holes + recess track on the outer face
    for (var k = 0; k < 8; k += 1)
    {
        const z = T_POS0 + PITCH * k;
        cylAx(context, C + ("h" ~ k), Q(SL_D[1] - 1, BOLT_T, z), Q(TIP_D[1] + 1, BOLT_T, z), HOLE_R);
    }
    tipRBox(context, C + "track", SL_D[1] + WALL, TIP_D[1] + 2, BOLT_T - RECESS_R, BOLT_T + RECESS_R, T_POS0 - RECESS_R, T_POS0 + 7 * PITCH + RECESS_R, 0);
    // comb ribs are kept: subtract the slot, then add the ribs back
    var ribs = [];
    for (var k = 0; k < 12; k += 1)
    {
        const z = RIB_Z0 + PITCH * k;
        if (z > SL_Z[1] - 3) break;
        tipBox(context, id + ("rib" ~ k), SL_D[0] - 0.2, SL_D[1] + 0.2, RIB_T[0], RIB_T[1], z - RIB_H / 2, z + RIB_H / 2);
    }
    return { "add" : solids(context, id + "add"), "cut" : solids(context, C), "ribs" : qUnion(mapArray(range(0, 11), function(k) { return solids(context, id + ("rib" ~ k)); })) };
}

// =============================================== FEATURES ===============================================
function seatOff(s is SidePadSeat) returns number { return s == SidePadSeat.DOWN ? -FINE : (s == SidePadSeat.UP ? FINE : 0); }
function tiltDeg(t is SidePadTilt) returns number
{
    return t == SidePadTilt.P8 ? 8 : (t == SidePadTilt.M8 ? -8 : (t == SidePadTilt.P15 ? 15 : (t == SidePadTilt.M15 ? -15 : 0)));
}
export function buildSide(context is Context, id is Id, def is map) returns Query
{
    const c = T_POS0 + PITCH * def.step;
    const low = def.kit == SidePadKit.LOW;
    const za = low ? c - D_OFF : c + D_OFF;
    const zp = za + seatOff(def.seat);
    const xs = depthX(def.depth);
    var parts = [];
    const car = buildCarrier(context, id + "car", c, za);
    setProperty(context, { "entities" : car, "propertyType" : PropertyType.NAME, "value" : "sidepad carrier " ~ (low ? "A" : "A' (flipped)") });
    parts = append(parts, car);
    const dp = buildDepth(context, id + "dep", xs, za, low);
    setProperty(context, { "entities" : dp, "propertyType" : PropertyType.NAME, "value" : "sidepad depth piece " ~ toString(def.depth) });
    parts = append(parts, dp);
    const ang = tiltDeg(def.tilt);
    var padIds = [id + "pad", id + "ring"];
    const pad = buildPadShell(context, id + "pad", xs, zp);
    setProperty(context, { "entities" : pad, "propertyType" : PropertyType.NAME, "value" : "sidepad pad shell" });
    const ring = buildClipRing(context, id + "ring", xs, zp);
    setProperty(context, { "entities" : ring, "propertyType" : PropertyType.NAME, "value" : "sidepad clip ring" });
    if (ang != 0)
    {
        const sh = buildShim(context, id + "shim", xs, za, ang);
        setProperty(context, { "entities" : sh, "propertyType" : PropertyType.NAME, "value" : "sidepad tilt shim " ~ abs(ang) });
        parts = append(parts, sh);
        const t = 2 * FOOT_HZ * tan(abs(ang) * degree);
        const pivot = vector(xs - 2.0, 0, ang > 0 ? za - FOOT_HZ : za + FOOT_HZ);
        const xf = rotationAround(line(pivot * millimeter, vector(0, 1, 0)), -ang * degree) * transform(v3(-2.0, 0, 0));
        opTransform(context, id + "tilt", { "bodies" : qUnion([pad, ring]), "transform" : xf });
    }
    parts = concatenateArrays([parts, [pad, ring]]);
    if (def.bolt)
    {
        const b = buildBolt(context, id + "bolt", c);
        setProperty(context, { "entities" : b, "propertyType" : PropertyType.NAME, "value" : "sidepad snap-button bolt" });
        const w = buildWasher(context, id + "wash", c);
        setProperty(context, { "entities" : w, "propertyType" : PropertyType.NAME, "value" : "sidepad bolt retainer washer" });
        parts = concatenateArrays([parts, [b, w]]);
    }
    // flip for the HIGH kit: the carrier and depth piece were built for the side directly (zp above c), nothing to flip here
    return qUnion(parts);
}

annotation { "Feature Type Name" : "GbG Side Pad v1", "Feature Type Description" : "Side pad kit (carrier, depth piece, pad shell, clip ring, shim, snap-button bolt) for GbG_chair_forth; needs the wing change (GbG Side Pad Wing)" }
export const gbgSidePadV1 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Side" }
        definition.side is SidePadSide;
        annotation { "Name" : "Kit" }
        definition.kit is SidePadKit;
        annotation { "Name" : "Height step (0 = lowest, 12.5 mm each)" }
        isInteger(definition.step, { (unitless) : [0, 2, 7] } as IntegerBoundSpec);
        annotation { "Name" : "Depth piece" }
        definition.depth is SidePadDepth;
        annotation { "Name" : "Seat position" }
        definition.seat is SidePadSeat;
        annotation { "Name" : "Tilt shim" }
        definition.tilt is SidePadTilt;
        annotation { "Name" : "Snap-button bolt + washer" }
        definition.bolt is boolean;
    }
    {
        setVariable(context, "gbgSkipped", "");
        var q = [];
        if (definition.side != SidePadSide.RIGHT) q = append(q, buildSide(context, id + "L", definition));
        if (definition.side != SidePadSide.LEFT)
        {
            const r = buildSide(context, id + "R", definition);
            opTransform(context, id + "mir", { "bodies" : r, "transform" : mirrorAcross(plane(v3(0, 0, 0), vector(1, 0, 0))) });
        }
        var msg = "";
        try silent { msg = getVariable(context, "gbgSkipped"); }
        if (msg != "") reportFeatureInfo(context, id, "Finish skipped: " ~ msg);
    }, { "side" : SidePadSide.LEFT, "kit" : SidePadKit.LOW, "step" : 2, "depth" : SidePadDepth.N0, "seat" : SidePadSeat.MID, "tilt" : SidePadTilt.T0, "bolt" : true });

annotation { "Feature Type Name" : "GbG Side Pad Wing (chair)", "Feature Type Description" : "Adds the side-pad socket (vertical slot, comb ribs, bolt-hole track, entry ramp) to the chair's wings" }
export const gbgSidePadWing = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Wings (empty = found automatically)", "Filter" : EntityType.BODY && BodyType.SOLID, "MaxNumberOfPicks" : 2 }
        definition.wings is Query;
        annotation { "Name" : "Test only: use stand-in blocks for the wings" }
        definition.standIn is boolean;
    }
    {
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
            const t = wingTools(context, sid + "t");
            if (s < 0) opTransform(context, sid + "tm", { "bodies" : qUnion([t.add, t.cut, t.ribs]), "transform" : mirrorAcross(plane(v3(0, 0, 0), vector(1, 0, 0))) });
            opBoolean(context, sid + "add", { "tools" : qUnion([wing, t.add]), "operationType" : BooleanOperationType.UNION });
            const keep = evaluateQuery(context, qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(s * Q(-3, 30, 300)[0], Q(-3, 30, 300)[1], 300)));
            if (size(keep) != 1) throw regenError("Side Pad Wing: merged wing not found");
            opBoolean(context, sid + "cut", { "tools" : t.cut, "targets" : keep[0], "operationType" : BooleanOperationType.SUBTRACTION });
            opBoolean(context, sid + "rib", { "tools" : qUnion([keep[0], t.ribs]), "operationType" : BooleanOperationType.UNION });
        }
    }, { "standIn" : false });
