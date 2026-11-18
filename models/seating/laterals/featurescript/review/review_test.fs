FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

export enum SidePadSide { LEFT, RIGHT, BOTH }
export enum SidePadKit { LOW, HIGH }
export enum SidePadDepth { N0, N12, N24, N36 }
export enum SidePadSeat { DOWN, MID, UP }
export enum SidePadTilt { T0, P8, M8, P15, M15 }
const P0 = [131.3, 80.7]; const AXH = [0.79577626, 0.60559074]; const TH = [-0.6083053, 0.79370313];
const D_SHIFT = 2.0;
const Z0 = 227.8; const SHEAR = [0.8 / 32.9, 2.7 / 32.9]; const ZREF = 244.25;
const TIP_D = [-7.0, 23.6]; const TIP_T = [-16.0, 38.0]; const TIP_Z = [198.0, 347.0];
const SL_D = [1.2, 17.6]; const SL_T = [-30.0, 28.0]; const SL_Z = [201.0, 343.0];
const RIB_T = [22.0, 28.5]; const RIB_H = 5.6; const RIB_Z0 = 208.25; const PITCH = 12.5;
const BOLT_T = 8.0;
const HOLE_R = 3.2; const WALL = 3.0; const HOLE_CH = 0.8;
const CH_W = 16.0;
const RAMP = 6.5;
const K0 = 14.0; const Z_HOLD = 0.35;
const TG_D = [1.4, 17.4]; const TG_T = [-14.0, 27.5]; const TG_H = 25.0;
const NOTCH = [-18.75, -6.25, 6.25, 18.75];
const D_OFF = 50.0;
const T_POS0 = 227.0;
const NK_Y = [25.0, 43.0]; const NK_HZ = 18.0; const NK_R = 6.0;
const J0 = 128.0; const J1 = 146.0; const JS = 137.0;
const KEY_LO = -6.0; const KEY_HI = 2.0; const FL = 3.5; const ENDL = 2.5;
const B_BORE_R = 4.7; const B_BORE_D0 = 3.0;
const B_SH_R = 4.5; const B_SH_L = 5.0; const B_HEAD_R = 3.0; const B_SPR_R = 3.3; const B_SPR_D = 4.0;
const WASH_T = 2.0; const M3_DT = 9.0;
const PAD_Y0 = -31.0; const PAD_Y1 = 46.0; const PAD_H = 80.0; const SHELL_T = 4.5;
const A_REF = 85.0; const B_REF = 88.0; const YC_REF = 30.0; const WRAP = 0.7; const SE_P = 4.0;
const FOOT_Y = [4.0, 40.0]; const FOOT_HZ = 20.0; const NOSE = 1.5; const SEAT_D = 1.5; const FINE = 6.25;
const PSYS = [9.0, 19.0];
const HS = 7.0;
const TILT_Z = 5.5;
const BOSS_T = 8.5;
const NUT_FLOOR = 1.2;
const NOSE_RIB = 1.3;
const SHIM0 = 3.5;
const WEDGE_R = 5.5; const WEDGE_T0 = 1.5;
const BEAD = [1.0, 7.5]; const GROOVE = [2.5, 6.5]; const BEAD_H = 3.5; const GROOVE_D = 3.0;
const CL = 0.2; const M5_CLEAR = 5.5; const M5_NUT_AF = 8.3; const M5_NUT_T = 4.4; const CSK_M5_R = 5.3;
const M3_CLEAR = 3.5; const M3_NUT_AF = 5.8; const M3_NUT_T = 2.8; const CSK_M3_R = 3.25;
const M5_HEAD_R = 4.7;
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
function Q(d is number, t is number, z is number) returns Vector
{
const dd = d + D_SHIFT;
return vector(P0[0] + dd * AXH[0] + t * TH[0] + (ZREF - Z0) * SHEAR[0], P0[1] + dd * AXH[1] + t * TH[1] + (ZREF - Z0) * SHEAR[1], z);
}
function dirD() returns Vector { return normalize(vector(AXH[0], AXH[1], 0)); }
function dirT() returns Vector { return normalize(vector(TH[0], TH[1], 0)); }
function tipBox(context is Context, id is Id, d0, d1, t0, t1, z0, z1)
{
fCuboid(context, id + "b", { "corner1" : v3(0, 0, 0), "corner2" : v3(d1 - d0, t1 - t0, z1 - z0) });
const o = Q(d0, t0, z0);
const xf = transform(v3(o[0], o[1], o[2])) * toWorld(coordSystem(vector(0, 0, 0) * meter, dirD(), vector(0, 0, 1))) ;
opTransform(context, id + "x", { "bodies" : qCreatedBy(id + "b", EntityType.BODY), "transform" : xf });
}
function tipRBox(context is Context, id is Id, d0, d1, t0, t1, z0, z1, r)
{
tipBox(context, id, d0, d1, t0, t1, z0, z1);
if (r > 0) finishEdges(context, id + "f", qOwnedByBody(qCreatedBy(id + "b", EntityType.BODY), EntityType.EDGE), r, "tip box round");
}
function cylAx(context is Context, id is Id, p0 is Vector, p1 is Vector, r is number)
{
fCylinder(context, id, { "bottomCenter" : p0 * millimeter, "topCenter" : p1 * millimeter, "radius" : r * millimeter });
}
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
function rsec(c is Vector, tg is Vector, w is number, h is number, r is number) returns array
{
var t = normalize(vector(tg[0], tg[1], 0));
const n = cross(vector(0, 0, 1), t);
var o = [];
const corners = [[1, 1, 0], [-1, 1, 90], [-1, -1, 180], [1, -1, 270]];
for (var cc in corners)
{
const cx = cc[0] * (w / 2 - r); const cz = cc[1] * (h / 2 - r);
for (var k = 0; k < 6; k += 1)
{
const a = (cc[2] + 90 * k / 5) * degree;
const u = cx + r * cos(a); const v = cz + r * sin(a);
const p = c + n * u + vector(0, 0, v);
o = append(o, [p[0], p[1], p[2]]);
}
}
return o;
}
function ovalX(x is number, yc is number, zc is number, ry is number, rz is number) returns array
{
var o = [];
for (var k = 0; k < 40; k += 1)
{
const a = 2 * PI * k / 40;
const c = cos(a * radian); const s = sin(a * radian);
o = append(o, [x, yc + ry * sgn(c) * abs(c) ^ (2 / 2.6), zc + rz * sgn(s) * abs(s) ^ (2 / 2.6)]);
}
return o;
}
function bez(p0 is Vector, p1 is Vector, p2 is Vector, p3 is Vector, s is number) returns Vector
{
return p0 * (1 - s) ^ 3 + p1 * 3 * (1 - s) ^ 2 * s + p2 * 3 * (1 - s) * s ^ 2 + p3 * s ^ 3;
}
function barX(context is Context, id is Id, x0 is number, x1 is number, zc is number)
{
loftSections(context, id, [rsec(vector(x0, (NK_Y[0] + NK_Y[1]) / 2, zc), vector(1, 0, 0), NK_Y[1] - NK_Y[0], 2 * NK_HZ, NK_R),
rsec(vector(x1, (NK_Y[0] + NK_Y[1]) / 2, zc), vector(1, 0, 0), NK_Y[1] - NK_Y[0], 2 * NK_HZ, NK_R)]);
}
function xyPrism(context is Context, id is Id, pts is array, z0 is number, z1 is number)
{
const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, z0), vector(0, 0, 1), vector(1, 0, 0)) });
for (var i = 0; i < size(pts); i += 1)
{
const a = pts[i]; const b = pts[(i + 1) % size(pts)];
skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
}
skSolve(sk);
opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : (z1 - z0) * millimeter });
opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
function splitRegion(context is Context, id is Id, zc is number, below is boolean, cl is number)
{
const x0 = cl > 0 ? J0 - 0.3 : J0 - 0.01; const x1 = J1 + 0.01; const s = below ? 1 : -1;
const S = [[x0, zc + s * (KEY_LO + cl)], [J0 + ENDL + FL - cl, zc + s * (KEY_LO + cl)], [J0 + ENDL - cl, zc + s * (KEY_HI + cl)],
[J1 - ENDL + cl, zc + s * (KEY_HI + cl)], [J1 - ENDL - FL + cl, zc + s * (KEY_LO + cl)], [x1, zc + s * (KEY_LO + cl)]];
const far = zc - s * 60;
var poly = concatenateArrays([S, [[x1, far], [x0, far]]]);
const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(v3(0, 0, 0), vector(0, -1, 0), vector(1, 0, 0)) });
for (var i = 0; i < size(poly); i += 1)
{
const a = poly[i]; const b = poly[(i + 1) % size(poly)];
skLineSegment(sk, "l" ~ i, { "start" : vector(a[0], a[1]) * millimeter, "end" : vector(b[0], b[1]) * millimeter });
}
skSolve(sk);
opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 1, 0), "endBound" : BoundingType.BLIND, "endDepth" : 100 * millimeter });
opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}
export function buildCarrier(context is Context, id is Id, c is number, zp is number) returns Query
{
const low = zp < c;
tipRBox(context, id + "tg", TG_D[0], TG_D[1], TG_T[0], TG_T[1], c - TG_H, c + TG_H, 1.5);
const nose = Q((TG_D[0] + TG_D[1]) / 2, TG_T[0] + 1, c);
const dout = -dirT();
const endp = vector(J1, (NK_Y[0] + NK_Y[1]) / 2, zp);
const p1 = nose + dout * K0; const p2 = endp + vector(1, 0, 0) * 8;
var secs = [];
for (var s in linspace(0, 1, 13))
{
var c0 = bez(nose, p1, p2, endp, s);
const sz = min(max((s - Z_HOLD) / (1 - Z_HOLD), 0), 1); const ez = sz * sz * (3 - 2 * sz);
c0 = vector(c0[0], c0[1], c + (zp - c) * ez);
const tg = bez(nose, p1, p2, endp, min(s + 0.02, 1)) - bez(nose, p1, p2, endp, max(s - 0.02, 0));
const sw = min(max((s - 0.3) / 0.7, 0), 1); const ew = sw * sw * (3 - 2 * sw);
secs = append(secs, rsec(c0, tg, lerp(TG_D[1] - TG_D[0] - 0.2, NK_Y[1] - NK_Y[0], ew), lerp(2 * TG_H - 0.4, 2 * NK_HZ, ew), 5.0));
}
loftSections(context, id + "sw", secs);
fCuboid(context, id + "swc", { "corner1" : v3(0, 0, zp - 60), "corner2" : v3(J1, 56, zp + 60) });
opBoolean(context, id + "swcut", { "tools" : qCreatedBy(id + "swc", EntityType.BODY), "targets" : solids(context, id + "sw"), "operationType" : BooleanOperationType.SUBTRACTION });
barX(context, id + "jb", J0, J1 + 2, zp);
splitRegion(context, id + "jr", zp, low, 0);
opBoolean(context, id + "jint", { "tools" : qUnion([solids(context, id + "jb"), solids(context, id + "jr")]), "operationType" : BooleanOperationType.INTERSECTION });
opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
const body = oneBody(context, id, "carrier");
const C = id + "cut";
for (var i = 0; i < size(NOTCH); i += 1)
tipBox(context, C + ("n" ~ i), TG_D[0] - 1, TG_D[1] + 1, RIB_T[0] - 0.4, TG_T[1] + 1, c + NOTCH[i] - 3.1, c + NOTCH[i] + 3.1);
cylAx(context, C + "bore", Q(B_BORE_D0, BOLT_T, c), Q(TG_D[1] + 1, BOLT_T, c), B_BORE_R);
tipBox(context, C + "wp", TG_D[1] - WASH_T, TG_D[1] + 1, BOLT_T - M3_DT - 4.2, BOLT_T + M3_DT + 4.2, c - 6, c + 6);
for (var s in [-1, 1])
{
cylAx(context, C + ("m3" ~ s), Q(TG_D[1] - 9, BOLT_T + s * M3_DT, c), Q(TG_D[1] + 1, BOLT_T + s * M3_DT, c), M3_CLEAR / 2);
hexPrism(context, C + ("m3n" ~ s), Q((TG_D[0] - 1 + TG_D[1] - 7) / 2, BOLT_T + s * M3_DT, c), dirD(), M3_NUT_AF, (TG_D[1] - 7) - (TG_D[0] - 1));
}
tdHole(context, C + "js", vector(JS, 34, zp - NK_HZ - 2), vector(JS, 34, zp + NK_HZ + 2), M5_CLEAR / 2, vector(0, 1, 0));
hexPrism(context, C + "jn", vector(JS, 34, low ? zp - NK_HZ + 6 + M5_NUT_T / 2 : zp + NK_HZ - 6 - M5_NUT_T / 2), vector(0, 0, 1), M5_NUT_AF, M5_NUT_T);
cylAx(context, C + "jna", vector(JS, 34, low ? zp - NK_HZ - 1 : zp + NK_HZ + 1), vector(JS, 34, low ? zp - NK_HZ + 6 : zp + NK_HZ - 6), M5_NUT_AF / sqrt(3) + 0.05);
opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
return body;
}
function noseBody(context is Context, id is Id, xs is number, zc is number)
{
const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
loftSections(context, id + "ns", [ovalX(xs + 0.6, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 3.0, FOOT_HZ - 3.0), ovalX(xs - NOSE, yc, zc, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 3.6, FOOT_HZ - 3.6)]);
for (var r in [-9.375, 9.375])
fCuboid(context, id + ("rib" ~ (r > 0 ? "u" : "d")), { "corner1" : v3(xs - NOSE - 1.0, FOOT_Y[0] + 8, zc + r - NOSE_RIB), "corner2" : v3(xs - NOSE + 0.2, FOOT_Y[1] - 8, zc + r + NOSE_RIB) });
}
function seatCut(context is Context, id is Id, xs is number, zc is number)
{
const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
for (var k = -1; k <= 1; k += 1)
loftSections(context, id + ("seat" ~ (k + 1)), [ovalX(xs + 0.1, yc, zc + k * FINE, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 2.8, FOOT_HZ - 2.8), ovalX(xs - SEAT_D - 0.1, yc, zc + k * FINE, (FOOT_Y[1] - FOOT_Y[0]) / 2 - 3.4, FOOT_HZ - 3.4)]);
for (var k = -5; k <= 5; k += 1)
{
const z = zc + 3.125 + 6.25 * k;
if (abs(z - zc) > FOOT_HZ + 3) continue;
fCuboid(context, id + ("g" ~ (k + 5)), { "corner1" : v3(xs - SEAT_D - 1.2, FOOT_Y[0] + 7.5, z - 1.55), "corner2" : v3(xs - SEAT_D + 0.05, FOOT_Y[1] - 7.5, z + 1.55) });
}
}
export function depthX(d is SidePadDepth) returns number
{
return d == SidePadDepth.N0 ? 118 : (d == SidePadDepth.N12 ? 106 : (d == SidePadDepth.N24 ? 94 : 82));
}
export function buildDepth(context is Context, id is Id, xs is number, zp is number, low is boolean) returns Query
{
barX(context, id + "jb", J0, J1 - 0.3, zp);
const L = 10.0;
if (xs + L < J0) barX(context, id + "nk", xs + L, J0 + 0.02, zp);
const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
var rings = [rsec(vector(min(xs + L, J0 + 0.02), (NK_Y[0] + NK_Y[1]) / 2, zp), vector(1, 0, 0), NK_Y[1] - NK_Y[0], 2 * NK_HZ, NK_R)];
for (var u in linspace(L * 0.8, 0.6, 6))
{
const f = 1 - u / L;
const ry = 9 + ((FOOT_Y[1] - FOOT_Y[0]) / 2 - 9) * (1 - (1 - f) ^ 2) + 2 * f ^ 3;
const rz = NK_HZ + (FOOT_HZ - NK_HZ) * f + 2 * f ^ 3;
const ycu = 34 + (yc - 34) * (1 - (1 - f) ^ 2);
rings = append(rings, ovalX(xs + u, ycu, zp, ry, rz));
}
loftSections(context, id + "fl", rings);
noseBody(context, id + "nb", xs, zp);
const yl0 = FOOT_Y[0] + 0.3; const yl1 = PSYS[1] + M5_HEAD_R + 2.0; const hl = 2 * (FINE + TILT_Z + M5_HEAD_R + 1.0);
loftSections(context, id + "land", [rsec(vector(xs + 0.4, (yl0 + yl1) / 2, zp), vector(1, 0, 0), yl1 - yl0, hl, 3.0), rsec(vector(xs + HS, (yl0 + yl1) / 2, zp), vector(1, 0, 0), yl1 - yl0, hl, 3.0)]);
opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
const body = oneBody(context, id, "depth piece");
const C = id + "cut";
splitRegion(context, C + "jr", zp, low, CL);
tdHole(context, C + "js", vector(JS, 34, zp - NK_HZ - 2), vector(JS, 34, zp + NK_HZ + 2), M5_CLEAR / 2, vector(0, 1, 0));
const zh = low ? zp + NK_HZ : zp - NK_HZ; const sh = low ? 1 : -1;
fCone(context, C + "jsk", { "bottomCenter" : v3(JS, 34, zh - sh * (CSK_M5_R - 2.75)), "topCenter" : v3(JS, 34, zh + sh * 0.05), "bottomRadius" : 2.75 * millimeter, "topRadius" : CSK_M5_R * millimeter });
const zs = FINE + TILT_Z;
for (var i = 0; i < size(PSYS); i += 1)
{
const y = PSYS[i];
for (var j in [-1, 1]) cylAx(context, C + ("ps" ~ i ~ j), vector(xs - 3, y, zp + j * zs), vector(xs + HS + 0.01, y, zp + j * zs), M5_CLEAR / 2);
fCuboid(context, C + ("psb" ~ i), { "corner1" : v3(xs - 3, y - M5_CLEAR / 2, zp - zs), "corner2" : v3(xs + HS + 0.01, y + M5_CLEAR / 2, zp + zs) });
for (var j in [-1, 1]) cylAx(context, C + ("pc" ~ i ~ j), vector(xs + HS, y, zp + j * zs), vector(xs + 30, y, zp + j * zs), WEDGE_R + 0.2);
fCuboid(context, C + ("pcb" ~ i), { "corner1" : v3(xs + HS, y - WEDGE_R - 0.2, zp - zs), "corner2" : v3(xs + 30, y + WEDGE_R + 0.2, zp + zs) });
}
opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
return body;
}
function shellXi(y is number, xs is number) returns number
{
const ex = A_REF * max(1 - abs((y - YC_REF) / B_REF) ^ 3, 0) ^ (1 / 3);
return xs - SHELL_T - WRAP * (A_REF - ex);
}
function padOutlineYZ(zc is number, ins is number) returns array
{
var o = [];
const ym = (PAD_Y0 + PAD_Y1) / 2; const hy = (PAD_Y1 - PAD_Y0) / 2 - ins; const hz = PAD_H / 2 - ins;
for (var k = 0; k < 72; k += 1)
{
const a = 2 * PI * k / 72;
const c = cos(a * radian); const s = sin(a * radian);
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
planStrip(context, id + "st", xs, 0, SHELL_T, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
yzPrism(context, id + "ol", padOutlineYZ(zc, 0), xs - 80, xs + 20);
opBoolean(context, id + "int", { "tools" : qUnion([solids(context, id + "st"), solids(context, id + "ol")]), "operationType" : BooleanOperationType.INTERSECTION });
planStrip(context, id + "bs", xs, SHELL_T - 0.2, SHELL_T + BEAD_H, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
yzPrism(context, id + "bo", padOutlineYZ(zc, BEAD[0]), xs - 80, xs + 20);
yzPrism(context, id + "bi", padOutlineYZ(zc, BEAD[1]), xs - 80, xs + 20);
opBoolean(context, id + "bring", { "tools" : solids(context, id + "bi"), "targets" : solids(context, id + "bo"), "operationType" : BooleanOperationType.SUBTRACTION });
opBoolean(context, id + "bint", { "tools" : qUnion([solids(context, id + "bs"), solids(context, id + "bo")]), "operationType" : BooleanOperationType.INTERSECTION });
const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
const yb = (PSYS[0] + PSYS[1]) / 2;
loftSections(context, id + "boss", [ovalX(xs - SHELL_T + 0.5, yb, zc, 14.5, 12.0), ovalX(xs - SHELL_T - BOSS_T, yb, zc, 11.5, 8.5)]);
opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
const body = oneBody(context, id, "pad shell");
const C = id + "cut";
planStrip(context, C + "gs", xs, SHELL_T + BEAD_H - GROOVE_D, SHELL_T + BEAD_H + 1, zc - PAD_H / 2 - 2, zc + PAD_H / 2 + 2);
yzPrism(context, C + "go", padOutlineYZ(zc, GROOVE[0]), xs - 80, xs + 20);
yzPrism(context, C + "gi", padOutlineYZ(zc, GROOVE[1]), xs - 80, xs + 20);
opBoolean(context, C + "gring", { "tools" : solids(context, C + "gi"), "targets" : solids(context, C + "go"), "operationType" : BooleanOperationType.SUBTRACTION });
opBoolean(context, C + "gint", { "tools" : qUnion([solids(context, C + "gs"), solids(context, C + "go")]), "operationType" : BooleanOperationType.INTERSECTION });
seatCut(context, C + "sc", xs, zc);
const xf0 = xs - SEAT_D - NUT_FLOOR; const xc = xs - SHELL_T - BOSS_T - 0.1;
for (var i = 0; i < size(PSYS); i += 1)
{
cylAx(context, C + ("h" ~ i), vector(xs - 20, PSYS[i], zc), vector(xs + 2, PSYS[i], zc), M5_CLEAR / 2);
hexPrism(context, C + ("n" ~ i), vector((xf0 + xc) / 2, PSYS[i], zc), vector(1, 0, 0), M5_NUT_AF, xf0 - xc);
}
opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
return body;
}
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
export function buildBolt(context is Context, id is Id, c is number) returns Query
{
const dwall = SL_D[1] + WALL;
const dsh1 = TG_D[1] - WASH_T - 0.1; const dsh0 = dsh1 - B_SH_L;
cylAx(context, id + "sh", Q(dsh0, BOLT_T, c), Q(dsh1, BOLT_T, c), B_SH_R);
cylAx(context, id + "hd", Q(dsh1 - 0.01, BOLT_T, c), Q(dwall, BOLT_T, c), B_HEAD_R);
opSphere(context, id + "dome", { "center" : Q(dwall, BOLT_T, c) * millimeter, "radius" : B_HEAD_R * millimeter });
opBoolean(context, id + "u", { "tools" : solids(context, id), "operationType" : BooleanOperationType.UNION });
const b = oneBody(context, id, "bolt");
cylAx(context, id + "sp", Q(dsh0 - 1, BOLT_T, c), Q(dsh0 + B_SPR_D, BOLT_T, c), B_SPR_R);
opBoolean(context, id + "spc", { "tools" : solids(context, id + "sp"), "targets" : b, "operationType" : BooleanOperationType.SUBTRACTION });
return b;
}
export function buildWasher(context is Context, id is Id, c is number) returns Query
{
tipRBox(context, id + "w", TG_D[1] - WASH_T + 0.05, TG_D[1] - 0.05, BOLT_T - M3_DT - 4.0, BOLT_T + M3_DT + 4.0, c - 5.8, c + 5.8, 0.6);
const w = oneBody(context, id, "washer");
const C = id + "cut";
cylAx(context, C + "h", Q(TG_D[1] - 3, BOLT_T, c), Q(TG_D[1] + 1, BOLT_T, c), B_HEAD_R + 0.25);
for (var s in [-1, 1])
{
cylAx(context, C + ("m" ~ s), Q(TG_D[1] - 3, BOLT_T + s * M3_DT, c), Q(TG_D[1] + 1, BOLT_T + s * M3_DT, c), M3_CLEAR / 2);
fCone(context, C + ("k" ~ s), { "bottomCenter" : Q(TG_D[1] - 0.05 - (CSK_M3_R - M3_CLEAR / 2), BOLT_T + s * M3_DT, c) * millimeter, "topCenter" : Q(TG_D[1] + 0.01, BOLT_T + s * M3_DT, c) * millimeter, "bottomRadius" : M3_CLEAR / 2 * millimeter, "topRadius" : CSK_M3_R * millimeter });
}
opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : w, "operationType" : BooleanOperationType.SUBTRACTION });
return w;
}
export function tiltXf(xs is number, za is number, ang is number) returns Transform
{
if (ang == 0) return identityTransform();
const pivot = vector(xs - SHIM0, 0, ang > 0 ? za - FOOT_HZ : za + FOOT_HZ);
return rotationAround(line(pivot * millimeter, vector(0, 1, 0)), -ang * degree) * transform(v3(-SHIM0, 0, 0));
}
export function buildShim(context is Context, id is Id, xs is number, za is number, zp is number, ang is number) returns Query
{
const xf = tiltXf(xs, za, ang);
const yc = (FOOT_Y[0] + FOOT_Y[1]) / 2;
const t = 2 * FOOT_HZ * tan(abs(ang) * degree);
loftSections(context, id + "w", [ovalX(xs, yc, za, (FOOT_Y[1] - FOOT_Y[0]) / 2 + 0.5, FOOT_HZ + 0.5), ovalX(xs - SHIM0 - t - 3, yc, za, (FOOT_Y[1] - FOOT_Y[0]) / 2 + 0.5, FOOT_HZ + 0.5)]);
fCuboid(context, id + "hs", { "corner1" : v3(xs - 80, -80, zp - 120), "corner2" : v3(xs, 120, zp + 120) });
opTransform(context, id + "hsx", { "bodies" : qCreatedBy(id + "hs", EntityType.BODY), "transform" : xf });
opBoolean(context, id + "cutp", { "tools" : qCreatedBy(id + "hs", EntityType.BODY), "targets" : solids(context, id + "w"), "operationType" : BooleanOperationType.SUBTRACTION });
noseBody(context, id + "nb", xs, za + (zp - za));
opTransform(context, id + "nbx", { "bodies" : solids(context, id + "nb"), "transform" : xf });
opBoolean(context, id + "u", { "tools" : qUnion([solids(context, id + "w"), solids(context, id + "nb")]), "operationType" : BooleanOperationType.UNION });
const body = oneBody(context, id, "shim");
const C = id + "cut";
seatCut(context, C + "sc", xs, za);
for (var i = 0; i < size(PSYS); i += 1)
cylAx(context, C + ("h" ~ i), vector(xs - 40, PSYS[i], zp), vector(xs + 40, PSYS[i], zp), M5_CLEAR / 2);
opTransform(context, C + "hx", { "bodies" : qUnion(mapArray(range(0, size(PSYS) - 1), function(i) { return solids(context, C + ("h" ~ i)); })), "transform" : xf });
opBoolean(context, id + "cutop", { "tools" : solids(context, C), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
return body;
}
export function screwAxis(xs is number, za is number, zp is number, ang is number, y is number) returns map
{
const xf = tiltXf(xs, za, ang);
const n = xf * (v3(xs - SEAT_D - NUT_FLOOR, y, zp));
const a = normalize(xf * (v3(xs + 1, y, zp)) - xf * (v3(xs, y, zp)));
const s = (xs + HS - n[0] / millimeter) / a[0];
const w = ang == 0 ? 0 : WEDGE_T0 + WEDGE_R * tan(abs(ang) * degree);
return { "nut" : n / millimeter, "dir" : a, "toLand" : s, "wedge" : w, "head" : s + w };
}
export function buildWedge(context is Context, id is Id, xs is number, za is number, zp is number, ang is number) returns Query
{
const a0 = screwAxis(xs, za, zp, ang, PSYS[0]);
const p0 = a0.nut + a0.dir * (a0.toLand - 3.0);
const sp = PSYS[1] - PSYS[0]; const r = WEDGE_R;
const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p0 * millimeter, a0.dir, vector(0, 1, 0)) });
skLineSegment(sk, "a", { "start" : vector(0, r) * millimeter, "end" : vector(sp, r) * millimeter });
skArc(sk, "b", { "start" : vector(sp, r) * millimeter, "mid" : vector(sp + r, 0) * millimeter, "end" : vector(sp, -r) * millimeter });
skLineSegment(sk, "c", { "start" : vector(sp, -r) * millimeter, "end" : vector(0, -r) * millimeter });
skArc(sk, "d", { "start" : vector(0, -r) * millimeter, "mid" : vector(-r, 0) * millimeter, "end" : vector(0, r) * millimeter });
skSolve(sk);
opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : a0.dir, "endBound" : BoundingType.BLIND, "endDepth" : (a0.head - a0.toLand + 3.0) * millimeter });
opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
fCuboid(context, id + "land", { "corner1" : v3(xs - 40, PSYS[0] - 30, zp - 40), "corner2" : v3(xs + HS, PSYS[1] + 30, zp + 40) });
opBoolean(context, id + "cl", { "tools" : qCreatedBy(id + "land", EntityType.BODY), "targets" : solids(context, id + "ex"), "operationType" : BooleanOperationType.SUBTRACTION });
const w = oneBody(context, id, "wedge plate");
for (var i = 0; i < size(PSYS); i += 1)
{
const ax = screwAxis(xs, za, zp, ang, PSYS[i]);
cylAx(context, id + ("h" ~ i), ax.nut + ax.dir * (ax.toLand - 6.0), ax.nut + ax.dir * (ax.head + 1.0), M5_CLEAR / 2);
}
opBoolean(context, id + "hc", { "tools" : qUnion([solids(context, id + "h0"), solids(context, id + "h1")]), "operationType" : BooleanOperationType.SUBTRACTION, "targets" : w });
return w;
}
export function padScrew(xs is number, za is number, zp is number, ang is number, y is number) returns map
{
const ax = screwAxis(xs, za, zp, ang, y);
const nutEnd = M5_NUT_T - 0.4; const face = SHELL_T + BOSS_T - SEAT_D - NUT_FLOOR - 0.3;
for (var L in [abs(ang) < 1 ? 16 : (abs(ang) < 10 ? 25 : 30)])
{
const tip = L - ax.head;
if (tip >= nutEnd && tip <= face) return { "L" : L, "tipPastNutFloor" : tip, "window" : [nutEnd, face], "head" : ax.head };
}
return { "L" : 0, "tipPastNutFloor" : 0, "window" : [nutEnd, face], "head" : ax.head };
}
export function wingTools(context is Context, id is Id) returns map
{
tipRBox(context, id + "add", TIP_D[0], TIP_D[1], TIP_T[0], TIP_T[1], TIP_Z[0], TIP_Z[1], 5.0);
const C = id + "cut";
tipBox(context, C + "slot", SL_D[0], SL_D[1], SL_T[0], SL_T[1], SL_Z[0], SL_Z[1]);
xyPrism(context, C + "lead", [Q(SL_D[0] + 0.1, TIP_T[0] - 1, 0), Q(SL_D[0] - 3.0, TIP_T[0] - 1, 0), Q(SL_D[0] + 0.1, TIP_T[0] + 3.1, 0)], SL_Z[0], SL_Z[1]);
xyPrism(context, C + "ramp", [Q(SL_D[1] - 0.1, TIP_T[0] - 1, 0), Q(SL_D[1] + RAMP, TIP_T[0] - 1, 0), Q(SL_D[1] - 0.1, TIP_T[0] + RAMP / tan(30 * degree), 0)], SL_Z[0], SL_Z[1]);
for (var k = 0; k < 8; k += 1)
{
const z = T_POS0 + PITCH * k;
cylAx(context, C + ("h" ~ k), Q(SL_D[1] - 1, BOLT_T, z), Q(TIP_D[1] + 1, BOLT_T, z), HOLE_R);
fCone(context, C + ("hc" ~ k), { "bottomCenter" : Q(SL_D[1] - 0.05, BOLT_T, z) * millimeter, "topCenter" : Q(SL_D[1] + HOLE_CH, BOLT_T, z) * millimeter, "bottomRadius" : (HOLE_R + HOLE_CH + 0.05) * millimeter, "topRadius" : HOLE_R * millimeter });
}
const dw = SL_D[1] + WALL; const dd = TIP_D[1] - dw + 0.5;
xyPrism(context, C + "chan", [Q(dw, BOLT_T - CH_W / 2, 0), Q(dw, BOLT_T + CH_W / 2, 0), Q(dw + dd, BOLT_T + CH_W / 2 + dd, 0), Q(dw + dd, BOLT_T - CH_W / 2 - dd, 0)], T_POS0 - CH_W / 2, T_POS0 + 7 * PITCH + CH_W / 2);
var ribs = [];
for (var k = 0; k < 12; k += 1)
{
const z = RIB_Z0 + PITCH * k;
if (z > SL_Z[1] - 3) break;
tipBox(context, id + ("rib" ~ k), SL_D[0] - 0.2, SL_D[1] + 0.2, RIB_T[0], RIB_T[1], z - RIB_H / 2, z + RIB_H / 2);
}
return { "add" : solids(context, id + "add"), "cut" : solids(context, C), "ribs" : qUnion(mapArray(range(0, 11), function(k) { return solids(context, id + ("rib" ~ k)); })) };
}
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
const pad = buildPadShell(context, id + "pad", xs, zp);
setProperty(context, { "entities" : pad, "propertyType" : PropertyType.NAME, "value" : "sidepad pad shell" });
const ring = buildClipRing(context, id + "ring", xs, zp);
setProperty(context, { "entities" : ring, "propertyType" : PropertyType.NAME, "value" : "sidepad clip ring" });
if (ang != 0)
{
const sh = buildShim(context, id + "shim", xs, za, zp, ang);
setProperty(context, { "entities" : sh, "propertyType" : PropertyType.NAME, "value" : "sidepad tilt shim " ~ abs(ang) });
parts = append(parts, sh);
opTransform(context, id + "tilt", { "bodies" : qUnion([pad, ring]), "transform" : tiltXf(xs, za, ang) });
const wd = buildWedge(context, id + "wedge", xs, za, zp, ang);
setProperty(context, { "entities" : wd, "propertyType" : PropertyType.NAME, "value" : "sidepad wedge plate " ~ abs(ang) });
parts = append(parts, wd);
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
return qUnion(parts);
}


function interBox(context is Context, id is Id, a is Query, b is Query) returns string
{
    opPattern(context, id + "ca", { "entities" : a, "transforms" : [identityTransform()], "instanceNames" : ["a"] });
    opPattern(context, id + "cb", { "entities" : b, "transforms" : [identityTransform()], "instanceNames" : ["b"] });
    try silent { opBoolean(context, id + "i", { "tools" : qUnion([qCreatedBy(id + "ca", EntityType.BODY), qCreatedBy(id + "cb", EntityType.BODY)]), "operationType" : BooleanOperationType.INTERSECTION }); }
    const r = qBodyType(qUnion([qCreatedBy(id + "ca", EntityType.BODY), qCreatedBy(id + "cb", EntityType.BODY)]), BodyType.SOLID);
    var o = "";
    if (size(evaluateQuery(context, r)) > 0)
    {
        const bx = evBox3d(context, { "topology" : r });
        o = roundToPrecision(evVolume(context, { "entities" : r }) / (millimeter ^ 3), 2) ~ "mm3 @[" ~ roundToPrecision(bx.minCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(bx.minCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(bx.minCorner[2] / millimeter, 1) ~ "]-[" ~ roundToPrecision(bx.maxCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(bx.maxCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(bx.maxCorner[2] / millimeter, 1) ~ "]";
        opDeleteBodies(context, id + "d", { "entities" : r });
    }
    return o;
}
function clashes(context is Context, id is Id, names is array, bodies is array, nKit is number) returns string
{
    var out = "";  var k = 0;
    for (var i = 0; i < size(bodies); i += 1)
        for (var j = i + 1; j < size(bodies); j += 1)
        {
            const col = evCollision(context, { "tools" : bodies[i], "targets" : bodies[j] });
            for (var c in col)
                if (c["type"] == ClashType.INTERFERE || c["type"] == ClashType.TARGET_IN_TOOL || c["type"] == ClashType.TOOL_IN_TARGET)
                {
                    if (i >= nKit && j >= nKit) break;      // chair-to-chair contacts are original chair CAD (they exist in Main)
                    k += 1;
                    var where = "";
                    for (var cc in col)
                    {
                        if (cc["type"] != ClashType.INTERFERE) continue;
                        try silent { const b1 = evBox3d(context, { "topology" : cc["tool"] }); const b2 = evBox3d(context, { "topology" : cc["target"] });
                        where = where ~ " T[" ~ roundToPrecision(b1.minCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(b1.minCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(b1.minCorner[2] / millimeter, 1) ~ ".." ~ roundToPrecision(b1.maxCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(b1.maxCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(b1.maxCorner[2] / millimeter, 1) ~ "]G[" ~ roundToPrecision(b2.minCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(b2.minCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(b2.minCorner[2] / millimeter, 1) ~ ".." ~ roundToPrecision(b2.maxCorner[0] / millimeter, 1) ~ "," ~ roundToPrecision(b2.maxCorner[1] / millimeter, 1) ~ "," ~ roundToPrecision(b2.maxCorner[2] / millimeter, 1) ~ "]"; }
                    }
                    out = out ~ names[i] ~ "x" ~ names[j] ~ " " ~ interBox(context, id + ("ib" ~ k), bodies[i], bodies[j]) ~ where ~ " n" ~ size(col) ~ "; ";
                    break;
                }
        }
    return out == "" ? "none" : out;
}
annotation { "Feature Type Name" : "GbG Side Pad review test" }
export const gbgSidePadReview = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "step" } isInteger(definition.step, { (unitless) : [0, 2, 7] } as IntegerBoundSpec);
        annotation { "Name" : "Kit" } definition.kit is SidePadKit;
        annotation { "Name" : "Depth" } definition.depth is SidePadDepth;
        annotation { "Name" : "Seat" } definition.seat is SidePadSeat;
        annotation { "Name" : "Tilt" } definition.tilt is SidePadTilt;
        annotation { "Name" : "Real chair" } definition.real is boolean;
    }
    {
        try { reviewBody(context, id, definition); }
        catch (e) { var st = ""; try silent { st = getVariable(context, "gbgStage"); } var rp = ""; try silent { rp = getVariable(context, "gbgRep"); } setVariable(context, "gbgReport", "ERR after [" ~ st ~ "] " ~ toString(e) ~ " || " ~ rp); }
    }, { "step" : 2, "kit" : SidePadKit.LOW, "depth" : SidePadDepth.N12, "seat" : SidePadSeat.MID, "tilt" : SidePadTilt.P15, "real" : false });
function reviewBody(context is Context, id is Id, definition is map)
    {
        setVariable(context, "gbgSkipped", "");
        setVariable(context, "gbgStage", "1");
        const def = { "step" : definition.step, "kit" : definition.kit, "seat" : definition.seat, "depth" : definition.depth, "tilt" : definition.tilt, "bolt" : true };
        setVariable(context, "gbgStage", "2");
        const q = buildSide(context, id + "L", def);
        setVariable(context, "gbgStage", "3");
        var names = [];  var bodies = [];  var rep = "CFG " ~ definition.step ~ " " ~ definition.kit ~ " " ~ definition.depth ~ " " ~ definition.seat ~ " " ~ definition.tilt ~ " ## ";
        setVariable(context, "gbgStage", "4");
        const qq = evaluateQuery(context, q);
        setVariable(context, "gbgStage", "evaluated " ~ size(qq));
        for (var b in qq)
        {
            const order = size(qq) == 8 ? ["carrier", "depth", "shim", "wedge", "shell", "ring", "bolt", "washer"] : ["carrier", "depth", "shell", "ring", "bolt", "washer"];
            var n = order[size(names)];
            setVariable(context, "gbgStage", "loop " ~ n);
            names = append(names, n);  bodies = append(bodies, b);
            var v = -1;
            try silent { v = roundToPrecision(evVolume(context, { "entities" : b }) / (centimeter ^ 3), 1); }
            rep = rep ~ n ~ " " ~ v ~ ", ";
        }
        const cb = evBox3d(context, { "topology" : bodies[0] });
        rep = rep ~ " ## carrier max x " ~ roundToPrecision(cb.maxCorner[0] / millimeter, 1) ~ " ";
        const kitSet = qUnion(bodies);  const nKitParts = size(bodies);
        const t = wingTools(context, id + "wt");
        var wing = qNothing();
        if (definition.real)
        {
            const probe = Q(9, 20, 300);
            const hit = evaluateQuery(context, qSubtraction(qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(probe[0], probe[1], probe[2])), qUnion([kitSet, qCreatedBy(id + "wt", EntityType.BODY)])));
            if (size(hit) != 1) throw regenError("real wing not found: " ~ size(hit));
            wing = hit[0];
        }
        else
        {
            tipBox(context, id + "stand", -4, 20, -14, 40, 179, 390);
            wing = qCreatedBy(id + "stand", EntityType.BODY);
        }
        const chairOthers = evaluateQuery(context, qSubtraction(qSubtraction(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), kitSet), qUnion([wing, qCreatedBy(id + "wt", EntityType.BODY)])));
        if (size(chairOthers) > 0)
        {
            opPattern(context, id + "oc", { "entities" : qUnion(chairOthers), "transforms" : [identityTransform()], "instanceNames" : ["o"] });
            opBoolean(context, id + "oadd", { "tools" : qCreatedBy(id + "oc", EntityType.BODY), "targets" : t.add, "operationType" : BooleanOperationType.SUBTRACTION });
            var wl = "";
            for (var ob in chairOthers)
            {
                const dd = evDistance(context, { "side0" : ob, "side1" : t.cut }).distance / millimeter;
                if (dd < 6) wl = wl ~ roundToPrecision(dd, 2) ~ " ";
            }
            rep = rep ~ " ## slot-to-neighbour walls(mm): " ~ wl; setVariable(context, "gbgRep", rep);
        }
        opBoolean(context, id + "add", { "tools" : qUnion([wing, t.add]), "operationType" : BooleanOperationType.UNION });
        const keep = evaluateQuery(context, qSubtraction(qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), v3(Q(-3, 30, 300)[0], Q(-3, 30, 300)[1], 300)), qUnion([kitSet, qCreatedBy(id + "wt", EntityType.BODY)])));
        opBoolean(context, id + "cut", { "tools" : t.cut, "targets" : keep[0], "operationType" : BooleanOperationType.SUBTRACTION });
        opBoolean(context, id + "rib", { "tools" : qUnion([keep[0], t.ribs]), "operationType" : BooleanOperationType.UNION });
        for (var ob in chairOthers)
        {
            var on = "chair";
            try silent { on = getProperty(context, { "entity" : ob, "propertyType" : PropertyType.NAME }); }
            names = append(names, on);  bodies = append(bodies, ob);
        }
        names = append(names, "WING");  bodies = append(bodies, keep[0]);
        setVariable(context, "gbgStage", "11");
        rep = rep ~ " ## CLASH: " ~ clashes(context, id + "c1", names, bodies, nKitParts); setVariable(context, "gbgRep", rep);
        setVariable(context, "gbgStage", "12");
        const c = T_POS0 + PITCH * definition.step;  const low = definition.kit == SidePadKit.LOW;
        setVariable(context, "gbgStage", "13");
        const za = low ? c - D_OFF : c + D_OFF;  const zp = za + seatOff(definition.seat);
        setVariable(context, "gbgStage", "14");
        const xs = depthX(definition.depth);  const ang = tiltDeg(definition.tilt);
        setVariable(context, "gbgStage", "15");
        var pn = [];  var pb = [];
        setVariable(context, "gbgStage", "16");
        for (var i = 0; i < size(PSYS); i += 1)
        {
            const ps = padScrew(xs, za, zp, ang, PSYS[i]);
            const ax = screwAxis(xs, za, zp, ang, PSYS[i]);
            if (i == 0) rep = rep ~ " ## SCREW M5x" ~ ps.L ~ " head " ~ roundToPrecision(ps.head, 2) ~ " tip " ~ roundToPrecision(ps.tipPastNutFloor, 2) ~ " win " ~ ps.window[0] ~ ".." ~ ps.window[1];
            if (ps.L > 0)
            {
                cylAx(context, id + ("probe" ~ i), ax.nut + ax.dir * (ax.head - 0.05), ax.nut - ax.dir * ps.tipPastNutFloor, 2.3);
                var ad = "";
                for (var k = 0; k < size(bodies); k += 1)
                {
                    const dl = evDistance(context, { "side0" : bodies[k], "side1" : line(ax.nut * millimeter, ax.dir) }).distance / millimeter;
                    if (dl < 2.7) ad = ad ~ names[k] ~ ":" ~ roundToPrecision(dl, 2) ~ " ";
                }
                rep = rep ~ " ## axis" ~ i ~ " near(<2.7): " ~ ad; setVariable(context, "gbgRep", rep);
                pn = append(pn, "screw" ~ i);  pb = append(pb, qCreatedBy(id + ("probe" ~ i), EntityType.BODY));
            }
        }
        rep = rep ~ " ## SCREW CLASH: " ~ clashes(context, id + "c2", concatenateArrays([pn, names]), concatenateArrays([pb, bodies]), size(pn) + nKitParts); setVariable(context, "gbgRep", rep);
        setVariable(context, "gbgStage", "17");
        var bolt = qNothing();
        setVariable(context, "gbgStage", "18");
        for (var i = 0; i < size(names); i += 1) if (names[i] == "bolt") bolt = bodies[i];
        setVariable(context, "gbgStage", "19");
        opTransform(context, id + "press", { "bodies" : bolt, "transform" : transform(dirD() * (-(WALL + B_HEAD_R + 0.5)) * millimeter) });
        setVariable(context, "gbgStage", "20");
        const col = evCollision(context, { "tools" : bolt, "targets" : qSubtraction(qUnion(bodies), bolt) });
        setVariable(context, "gbgStage", "21");
        var pc = "";
        setVariable(context, "gbgStage", "22");
        for (var cc in col) if (cc["type"] == ClashType.INTERFERE) pc = pc ~ "clash ";
        setVariable(context, "gbgStage", "23");
        rep = rep ~ " ## PRESSED BOLT: " ~ (pc == "" ? "free" : pc); setVariable(context, "gbgRep", rep);
        setVariable(context, "gbgStage", "24");
        var msg = "";
        setVariable(context, "gbgStage", "25");
        try silent { msg = getVariable(context, "gbgSkipped"); }
        rep = rep ~ " ## skipped: " ~ msg; setVariable(context, "gbgRep", rep);
        setVariable(context, "gbgStage", "26");
        setVariable(context, "gbgReport", rep);
        setVariable(context, "gbgStage", "27");
    }
