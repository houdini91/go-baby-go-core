FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// GbG chair: pommel v0.7 mount. Cuts into the seat-pan body (found at chair point (30, -60, 10) mm):
// - a 6 mm deep recess on the seat top, shaped like the pommel saddle (triangle, 30 wide at y -36 to 80 wide at the edge), +0.5 mm clearance
// - 3 mm off the pan front face over the hook width (x +-40.5), so the hook sits flush
// - optional 2x 6.4 mm carbon-rod holes at x +-12, z 7, from the front face back to y -33 (60 deep), with a lead-in chamfer
// The 3 existing M3 pommel holes and their nut pockets are unchanged. Source: research/onshape/gobabygo/addons/pommel/gbg_pommel_v07.fs

const SAD_REAR = -36;      // saddle rear edge (chair y)
const SAD_HW_REAR = 15;    // saddle half width at the rear
const HOOK_HW = 40;        // saddle half width at the seat edge / hook
const ROD_X = 12;          // carbon rod holes at x = +-12
const ROD_Z = 7;           //   and z = 7
const CHAIR_BORE_D = 6.4;  // slide fit for a 6.0 mm rod on a P1S (confirm with the rod fit test part)
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

annotation { "Feature Type Name" : "GbG Pommel Mount (chair)", "Feature Type Description" : "Cuts the 6 mm saddle recess and 2 carbon-rod holes into the GbG_chair_forth seat pan" }
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
        cutMount(context, id, base, definition.rodHoles);
    }, { rodHoles : true });


// Cuts the saddle recess (+0.5 mm clearance, 6 mm deep on top and 3 mm off the front face) and the 2 rod holes into the seat-pan body.
export function cutMount(context is Context, id is Id, base is Query, rodHoles is boolean)
{
        // recess = the saddle footprint grown by 0.5 mm, 6 mm deep on top, plus 3 mm off the front face
        const c = 0.5;
        var half = [pt(SAD_HW_REAR + c, SAD_REAR + c), pt((SAD_HW_REAR + HOOK_HW) / 2 + c, (SAD_REAR - 93) / 2), pt(HOOK_HW + c, -93), pt(HOOK_HW + c, -110)];
        const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(p3(0, 0, 14), vector(0, 0, 1), vector(1, 0, 0)) });
        skFitSpline(sk, "r", { "points" : [half[0], half[1], half[2]] });
        skLineSegment(sk, "r2", { "start" : half[2], "end" : half[3] });
        skLineSegment(sk, "f", { "start" : half[3], "end" : vector(-half[3][0], half[3][1]) });
        skLineSegment(sk, "l2", { "start" : vector(-half[3][0], half[3][1]), "end" : vector(-half[2][0], half[2][1]) });
        skFitSpline(sk, "l", { "points" : [vector(-half[2][0], half[2][1]), vector(-half[1][0], half[1][1]), vector(-half[0][0], half[0][1])] });
        skLineSegment(sk, "b", { "start" : vector(-half[0][0], half[0][1]), "end" : half[0] });
        skSolve(sk);
        opExtrude(context, id + "rec", { "entities" : qSketchRegion(id + "sk"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : mmv(20) });
        var tools = [qCreatedBy(id + "rec", EntityType.BODY)];
        fCuboid(context, id + "face", { "corner1" : p3(-(HOOK_HW + c), -93, -1), "corner2" : p3(HOOK_HW + c, -110, 40) });
        tools = append(tools, qCreatedBy(id + "face", EntityType.BODY));
        if (rodHoles)
            for (var s in [-1, 1])
            {
                tools = append(tools, teardropY(context, id + ("bore" ~ (s + 1)), s * ROD_X, ROD_Z, -95, BORE_END, CHAIR_BORE_D / 2));
                fCone(context, id + ("bli" ~ (s + 1)), { "bottomCenter" : p3(s * ROD_X, -93.5, ROD_Z), "topCenter" : p3(s * ROD_X, -92, ROD_Z),
                            "bottomRadius" : mmv(CHAIR_BORE_D / 2 + 1.5), "topRadius" : mmv(CHAIR_BORE_D / 2) });
                tools = append(tools, qCreatedBy(id + ("bli" ~ (s + 1)), EntityType.BODY));
            }
        opBoolean(context, id + "cut", { "tools" : qUnion(tools), "targets" : base, "operationType" : BooleanOperationType.SUBTRACTION });
        opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
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

