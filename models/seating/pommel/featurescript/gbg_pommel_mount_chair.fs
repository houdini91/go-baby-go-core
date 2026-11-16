FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// GbG chair: pommel v0.7.3 mount. Cuts into the seat-pan body (found at chair point (30, -60, 10) mm):
// - a SHALLOW recess on the seat top (RECESS_D = 1.5 mm), shaped like the pommel saddle + RECESS_C clearance, with a 14 deg
//   ramped wall (1.5 mm over RECESS_RAMP = 6 mm), its rim blended with a RECESS_R fillet (20 mm) and its floor with RECESS_RF (3 mm),
//   so a child sitting WITHOUT the pommel feels no edge. It only lines the saddle up; the hook, screws and rods hold it.
// - FACE_CUT = 3 mm off the pan front face over the hook width (x +-40.5), with 45 deg flared ends, so the hook sits close
// - the 3 M3 holes re-drilled to BOLT_D 3.5 (clearance) with a 0.5 mm mouth chamfer at the recess floor
// - optional 2x CHAIR_BORE_D carbon-rod holes at x +-12, z 7, from the front face back to y -33 (60 deep), with a lead-in chamfer
// The chair's nut pockets are unchanged (Part Studio 1, 2.4 mm deep, open at the pan bottom; KNOWN ITEM: they are 5.7 AF,
// tighter than the 5.8 AF print-fit policy for M3 nuts, see README). Keep every constant below identical to the copy in gbg_pommel_v07.fs.
// Source: research/onshape/gobabygo/addons/pommel/gbg_pommel_v07.fs

const SAD_REAR = -36;      // saddle rear edge (chair y)
const SAD_HW_REAR = 15;    // saddle half width at the rear
const HOOK_HW = 40;        // saddle half width at the seat edge / hook
const ROD_X = 12;          // carbon rod holes at x = +-12
const ROD_Z = 7;           //   and z = 7
const BOLTS_Y = [-45, -60, -75];
const PAN_FRONT = -96;     // seat-pan front face (chair y)
// ---- seat recess (shared with gbg_pommel_v07.fs) ----
const RECESS_D = 1.5;      // recess depth below the seat top (z 20): the saddle floor sits at z 18.5
const RECESS_C = 0.5;      // clearance per side around the saddle footprint at the floor
const RECESS_RAMP = 6;     // horizontal run of the recess wall (1.5 over 6 = 14 deg)
const RECESS_R = [20, 14, 10, 6, 4, 2];   // rim blend radii (convex edge a child can feel), tried in order
const RECESS_RF = [3, 2, 1];     // floor blend radii (inside corner); small so the blend (0.37 mm wide at r3) stays inside the 0.5 clearance
const FACE_CUT = 3;        // hook seat cut into the pan front face
// ---- print fits (Bambu P1S, PETG; FDM holes print 0.1-0.3 mm small) ----
const CHAIR_BORE_D = 6.5;  // slide fit for a 6.0 mm rod (0.25/side); confirm with the rod fit test part
const BOLT_D = 3.5;        // M3 clearance
const BORE_END = -33;      // 3 mm past the rod end = rod-length slack

function mmv(v) returns ValueWithUnits
{
    return v * millimeter;
}

function pt(a, b) returns Vector
{
    return vector(a, b) * millimeter;
}

function p3(x, y, z) returns Vector
{
    return vector(x, y, z) * millimeter;
}

annotation { "Feature Type Name" : "GbG Pommel Mount (chair)", "Feature Type Description" : "Cuts the shallow, blended saddle recess and 2 carbon-rod holes into the GbG_chair_forth seat pan" }
export const gbgPommelMount = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Rod holes for long pommels" }
        definition.rodHoles is boolean;
    }
    {
        const base = qContainsPoint(qBodyType(qEverything(EntityType.BODY), BodyType.SOLID), p3(30, -60, 10));
        if (size(evaluateQuery(context, base)) != 1)
            throw regenError("Could not find the seat-pan body at (30, -60, 10) mm");
        const r = cutMount(context, id, base, definition.rodHoles);
        reportFeatureInfo(context, id, "recess " ~ RECESS_D ~ " mm deep, rim blend r" ~ r.rim ~ ", floor blend r" ~ r.floor);
    }, { rodHoles : true });


// Recess outline at height z, grown by o (mm) around the saddle footprint: one smooth spline round the rear, straight sides
// out past the front face (y -112). k converts a normal offset into an x offset on the slanted saddle sides.
function recessSketch(context is Context, id is Id, z is number, o is number)
{
    const k = 1.09;
    const yF = PAN_FRONT + FACE_CUT;
    const right = [pt(HOOK_HW + o, yF), pt((SAD_HW_REAR + HOOK_HW) / 2 + k * o, (SAD_REAR + yF) / 2), pt(SAD_HW_REAR + 1 + k * o, SAD_REAR + o), pt(0, SAD_REAR + o + 0.5)];
    var pts = right;
    for (var i = size(right) - 2; i >= 0; i -= 1)
        pts = append(pts, vector(-right[i][0], right[i][1]));
    const sk = newSketchOnPlane(context, id, { "sketchPlane" : plane(p3(0, 0, z), vector(0, 0, 1), vector(1, 0, 0)) });
    skFitSpline(sk, "s", { "points" : pts, "startDerivative" : vector(0, 40) * millimeter, "endDerivative" : vector(0, -40) * millimeter });
    skLineSegment(sk, "r", { "start" : pts[0], "end" : pt(HOOK_HW + o, -112) });
    skLineSegment(sk, "f", { "start" : pt(HOOK_HW + o, -112), "end" : pt(-(HOOK_HW + o), -112) });
    skLineSegment(sk, "l", { "start" : pt(-(HOOK_HW + o), -112), "end" : pts[size(pts) - 1] });
    skSolve(sk);
}

// Cuts the shallow recess (ramped + blended), the hook seat on the front face, the M3 clearance holes and the rod holes.
// Returns the blend radii reached { rim, floor }.
export function cutMount(context is Context, id is Id, base is Query, rodHoles is boolean) returns map
{
    const zF = 20 - RECESS_D;
    // recess tool = loft from the floor outline (z 18.5, +0.5) to the rim outline (z 20, +6.5), plus a flat slab above the seat.
    // The slab makes the rim a concave tool edge, so its fillet comes out tangent to the seat top.
    recessSketch(context, id + "sk0", zF, RECESS_C);
    recessSketch(context, id + "sk1", 20, RECESS_C + RECESS_RAMP);
    opLoft(context, id + "loft", { "profileSubqueries" : [qSketchRegion(id + "sk0"), qSketchRegion(id + "sk1")], "bodyType" : ToolBodyType.SOLID });
    fCuboid(context, id + "slab", { "corner1" : p3(-80, -112, 20), "corner2" : p3(80, -20, 40) });
    const recQ = qUnion([qCreatedBy(id + "loft", EntityType.BODY), qCreatedBy(id + "slab", EntityType.BODY)]);
    opBoolean(context, id + "recU", { "tools" : recQ, "operationType" : BooleanOperationType.UNION });
    const rec = qUnion(evaluateQuery(context, recQ));
    const front = qAdjacent(qCoincidesWithPlane(qOwnedByBody(rec, EntityType.FACE), plane(p3(0, -112, 0), vector(0, -1, 0))), AdjacencyType.EDGE, EntityType.EDGE);
    const rim = qEdgeConvexityTypeFilter(qSubtraction(qCoincidesWithPlane(qOwnedByBody(rec, EntityType.EDGE), plane(p3(0, 0, 20), vector(0, 0, 1))), front), EdgeConvexityType.CONCAVE);
    const rR = roundAdaptive(context, id + "rimR", rim, RECESS_R, false);
    const floor = qEdgeConvexityTypeFilter(qSubtraction(qCoincidesWithPlane(qOwnedByBody(rec, EntityType.EDGE), plane(p3(0, 0, zF), vector(0, 0, 1))), front), EdgeConvexityType.CONVEX);
    const rF = roundAdaptive(context, id + "flrR", floor, RECESS_RF, false);
    var tools = [rec];
    // hook seat: FACE_CUT off the front face over the hook width, ends flared at 45 deg (no square notch corner)
    const yF = PAN_FRONT + FACE_CUT;
    const fs = newSketchOnPlane(context, id + "fsk", { "sketchPlane" : plane(p3(0, 0, -1), vector(0, 0, 1), vector(1, 0, 0)) });
    skPolyline(fs, "p", { "points" : [pt(HOOK_HW + RECESS_C, yF), pt(HOOK_HW + RECESS_C + yF + 112, -112), pt(-(HOOK_HW + RECESS_C + yF + 112), -112), pt(-(HOOK_HW + RECESS_C), yF), pt(HOOK_HW + RECESS_C, yF)] });
    skSolve(fs);
    opExtrude(context, id + "face", { "entities" : qSketchRegion(id + "fsk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : mmv(41) });
    tools = append(tools, qCreatedBy(id + "face", EntityType.BODY));
    // M3 clearance holes (re-drilled to BOLT_D) with a 0.5 mm mouth chamfer at the recess floor
    for (var j = 0; j < size(BOLTS_Y); j += 1)
    {
        tools = append(tools, cyl(context, id + ("bh" ~ j), p3(0, BOLTS_Y[j], -1), p3(0, BOLTS_Y[j], zF + 1), mmv(BOLT_D / 2)));
        fCone(context, id + ("bc" ~ j), { "bottomCenter" : p3(0, BOLTS_Y[j], zF - 0.5), "topCenter" : p3(0, BOLTS_Y[j], zF + 0.01),
                    "bottomRadius" : mmv(BOLT_D / 2), "topRadius" : mmv(BOLT_D / 2 + 0.51) });
        tools = append(tools, qCreatedBy(id + ("bc" ~ j), EntityType.BODY));
    }
    if (rodHoles)
        for (var s in [-1, 1])
        {
            tools = append(tools, teardropY(context, id + ("bore" ~ (s + 1)), s * ROD_X, ROD_Z, yF - 2, BORE_END, CHAIR_BORE_D / 2));
            fCone(context, id + ("bli" ~ (s + 1)), { "bottomCenter" : p3(s * ROD_X, yF - 0.5, ROD_Z), "topCenter" : p3(s * ROD_X, yF + 1, ROD_Z),
                        "bottomRadius" : mmv(CHAIR_BORE_D / 2 + 1.5), "topRadius" : mmv(CHAIR_BORE_D / 2) });
            tools = append(tools, qCreatedBy(id + ("bli" ~ (s + 1)), EntityType.BODY));
        }
    opBoolean(context, id + "cut", { "tools" : qUnion(tools), "targets" : base, "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, id + "del", { "entities" : qUnion([qCreatedBy(id + "sk0", EntityType.BODY), qCreatedBy(id + "sk1", EntityType.BODY), qCreatedBy(id + "fsk", EntityType.BODY)]) });
    return { "rim" : rR, "floor" : rF };
}

// Horizontal hole along Y, as a print-friendly teardrop: the round hole plus a 45 deg peak pointing +Z (up in the print),
// so the top of the hole is self-supporting instead of a sagging bridge. Returns the tool bodies (subtract them).
function teardropY(context is Context, id is Id, x is number, z is number, ya is number, yb is number, r is number) returns Query
{
    const c = r * sqrt(0.5);
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p3(x, ya, z), vector(0, -1, 0), vector(1, 0, 0)) });
    skLineSegment(sk, "a", { "start" : pt(-c, c - 0.05), "end" : pt(c, c - 0.05) });
    skLineSegment(sk, "b", { "start" : pt(c, c - 0.05), "end" : pt(0, r * sqrt(2)) });
    skLineSegment(sk, "d", { "start" : pt(0, r * sqrt(2)), "end" : pt(-c, c - 0.05) });
    skSolve(sk);
    opExtrude(context, id + "pk", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, yb > ya ? 1 : -1, 0),
                "endBound" : BoundingType.BLIND, "endDepth" : mmv(abs(yb - ya)) });
    opDeleteBodies(context, id + "dsk", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
    fCylinder(context, id + "cy", { "bottomCenter" : p3(x, ya, z), "topCenter" : p3(x, yb, z), "radius" : mmv(r) });
    return qUnion([qCreatedBy(id + "pk", EntityType.BODY), qCreatedBy(id + "cy", EntityType.BODY)]);
}

function cyl(context is Context, id is Id, a is Vector, b is Vector, r is ValueWithUnits) returns Query
{
    fCylinder(context, id, { "bottomCenter" : a, "topCenter" : b, "radius" : r });
    return qCreatedBy(id, EntityType.BODY);
}

// Round a set of edges as large as possible: all together at the first two sizes, otherwise edge by edge down the ladder.
// Returns the smallest size used (0 = none fitted).
function roundAdaptive(context is Context, id is Id, edges is Query, sizes is array, chamfer is boolean) returns number
{
    const list = evaluateQuery(context, edges);
    if (size(list) == 0)
        return 0;
    for (var i = 0; i < size(sizes); i += 1)
        if (blendOnce(context, id + ("all" ~ i), edges, sizes[i], chamfer))
            return sizes[i];
    var smallest = sizes[0];
    for (var e = 0; e < size(list); e += 1)
    {
        var done = 0;
        for (var i = 0; i < size(sizes); i += 1)
            if (blendOnce(context, id + ("e" ~ e ~ "_" ~ i), list[e], sizes[i], chamfer))
            {
                done = sizes[i];
                break;
            }
        smallest = min(smallest, done);
    }
    return smallest;
}

function blendOnce(context is Context, id is Id, edges is Query, s is number, chamfer is boolean) returns boolean
{
    var ok = false;
    try silent
    {
        if (chamfer)
            opChamfer(context, id, { "entities" : edges, "chamferType" : ChamferType.EQUAL_OFFSETS, "width" : mmv(s) });
        else
            opFillet(context, id, { "entities" : edges, "radius" : mmv(s) });
        ok = true;
    }
    return ok;
}
