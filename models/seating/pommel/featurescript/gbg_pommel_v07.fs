FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");

// GoBabyGo pommel v0.7: saddle + 90 deg hook + triangle bracing (design sketch: claude.ai artifact "GbG pommel v0.7b").
// Everything in CHAIR coordinates (GbG_chair_forth base: seat-pan bottom z = 0, top z = 20, front face y = -96, -Y = front).
// - Saddle: triangle in plan (30 wide at the rear bolt, 80 wide at the seat edge), 7.5 mm thick at the rear bolt (1.5 mm proud of the seat in the 6 mm recess), ~16 mm at the edge.
//   Held by the chair's 3 existing M3 holes (x 0, y -45/-60/-75) into the existing nut pockets under the pan.
// - Hook: 90 deg down the pan front face to z 0. Pressing down on the pommel pushes the hook into the front face.
// - Short (S): one part, solid down to z 0 (the brace triangle), pad reaches 75 past the seat edge.
// - Long (K): same saddle + slim neck + knee pad; 2x 6 mm carbon rods (x +-12, z 7) run from the chair's rod holes
//   through the hook into blind channels with crush ribs. One printed part; rods are trapped once the seat bolts are in.
// - "Chair has recess" = the chair branch with the 6 mm recess + rod holes (feature gbgPommelMount); off = today's chair.

export enum PommelSize
{
    annotation { "Name" : "S23A short, 2-3 y, alignment" } S23A,
    annotation { "Name" : "S23B short, 2-3 y, abduction (PT)" } S23B,
    annotation { "Name" : "K23A long, 2-3 y, alignment" } K23A,
    annotation { "Name" : "K23B long, 2-3 y, abduction (PT)" } K23B,
    annotation { "Name" : "K45A long, 4-5 y, alignment" } K45A,
    annotation { "Name" : "K45B long, 4-5 y, abduction (PT)" } K45B
}

const SAD_REAR = -36;      // saddle rear edge (chair y)
const SAD_HW_REAR = 15;    // half width at the rear
const HOOK_HW = 40;        // half width at the seat edge / hook
const HOOK_T = 6;          // hook thickness
const LIP_TOP = 30;        // saddle top at the seat edge (z)
const ROD_X = 12;
const ROD_Z = 7;
const BOLTS_Y = [-45, -60, -75];
const SEAT_EDGE = -96;
const SUP_GAP = 0.25;       // integral support: air gap under the saddle (about one 0.2 mm layer)
const LEG_R = [10, 8, 6.5, 5, 4, 3];   // tried in order on every edge the legs can touch
// ---- print fits (P1S / PETG first guesses; confirm with the "Rod fit test" part, then edit here) ----
const CHAIR_BORE_D = 6.4;  // rod hole in the chair: slide fit for a 6.0 rod (printed horizontal holes come out ~0.1-0.2 small)
const ROD_CH_D = 6.4;      // rod channel in the pommel
const RIB_I = 0.25;        // crush-rib interference per rib (radial), 3 ribs per channel, in 2 short zones
const RIB_ZONE = 20;       // length of each rib zone (rear, behind the hook / front, at the channel end)
const BORE_END = -33;      // chair rod hole ends 3 mm past the rod end: rod-length slack

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

// code, long?, s0 (start past the seat edge), L (contact length), H (height above seat), wR, wF
export function sizeSpec(s is PommelSize) returns map
{
    const t = {
        (PommelSize.S23A) : ["S23A", false, 0, 75, 60, 58, 70],
        (PommelSize.S23B) : ["S23B", false, 0, 75, 60, 80, 105],
        (PommelSize.K23A) : ["K23A", true, 40, 70, 65, 58, 70],
        (PommelSize.K23B) : ["K23B", true, 40, 70, 65, 80, 105],
        (PommelSize.K45A) : ["K45A", true, 85, 70, 78, 68, 80],
        (PommelSize.K45B) : ["K45B", true, 85, 70, 78, 100, 125]
    };
    const r = t[s];
    return { "code" : r[0], "long" : r[1], "s0" : r[2], "L" : r[3], "H" : r[4], "wR" : r[5], "wF" : r[6] };
}

annotation { "Feature Type Name" : "GbG Pommel v0.7", "Feature Type Description" : "Saddle + hook pommel for GbG_chair_forth, short or long (carbon rods)" }
export const gbgPommelV07 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Size" }
        definition.pommelSize is PommelSize;
        annotation { "Name" : "Long: reach adjust (15 mm steps, + = further out)" }
        isInteger(definition.step, { (unitless) : [-2, 0, 2] } as IntegerBoundSpec);
        annotation { "Name" : "Chair has the recess + rod holes" }
        definition.recess is boolean;
        annotation { "Name" : "Show carbon rods" }
        definition.showRods is boolean;
        annotation { "Name" : "Self-test all sizes" }
        definition.gallery is boolean;
        annotation { "Name" : "Print set: all 6 sizes + rod fit test, side by side, print-ready" }
        definition.printSet is boolean;
    }
    {
        if (definition.printSet)
        {
            // every part sits on z = 0 in its print orientation, laid out 320 mm in front of the chair (clear of an on-chair pommel); export each part as STL/3MF
            const sizes = [PommelSize.S23A, PommelSize.S23B, PommelSize.K23A, PommelSize.K23B, PommelSize.K45A, PommelSize.K45B];
            for (var i = 0; i < size(sizes); i += 1)
            {
                const pid = id + ("p" ~ i);
                buildPommel(context, pid, sizeSpec(sizes[i]), 0, definition.recess, false, true);
                opTransform(context, pid + "mv", { "bodies" : qBodyType(qCreatedBy(pid, EntityType.BODY), BodyType.SOLID), "transform" : transform(vector(140 * (i - 2.5), -320, 0) * millimeter) });
            }
            buildFitTest(context, id + "fit", 0, -320);
            reportFeatureInfo(context, id, getVariable(context, "gbgLog"));
            return;
        }
        if (definition.gallery)
        {
            var i = 0;
            for (var s in [PommelSize.S23A, PommelSize.S23B, PommelSize.K23A, PommelSize.K23B, PommelSize.K45A, PommelSize.K45B])
            {
                for (var rc in [true, false])
                {
                    const gid = id + ("g" ~ i);
                    buildPommel(context, gid, sizeSpec(s), 0, rc, false, true);
                    opDeleteBodies(context, id + ("gd" ~ i), { "entities" : qCreatedBy(gid, EntityType.BODY) });
                    i += 1;
                }
            }
        }
        buildPommel(context, id + "m", sizeSpec(definition.pommelSize), definition.step, definition.recess, definition.showRods, false);
        reportFeatureInfo(context, id, getVariable(context, "gbgLog"));
    }, { pommelSize : PommelSize.S23A, step : 0, recess : true, showRods : true, gallery : false, printSet : false });

function buildPommel(context is Context, id is Id, sp is map, step is number, recess is boolean, showRods is boolean, withSupport is boolean) returns Query
{
    const F = recess ? 14 : 20;              // saddle floor (6 mm recess, or the plain seat)
    const Y0 = recess ? -93 : -96;           // chair front face the hook bears on
    const HF = Y0 - HOOK_T;                  // hook outer face
    const zT = 20 + sp.H;                    // pad top
    var a = -SEAT_EDGE + sp.s0;              // pad start (distance forward of the chair origin)
    if (sp.long)
        a += step * 15;
    const b = sp.long ? a + sp.L : -SEAT_EDGE + sp.L;   // pad front
    if (sp.long && a < -HF + 25)
        throw regenError("Reach too short for a long pommel: use a short (S) size", ["step"]);

    // ---- sideSk profile (y, z), extruded across x ----
    var topPts = [pt(SAD_REAR, F + 7.5), pt(-55, F + 8.5), pt(-75, F + 11), pt(Y0 + 2, max(LIP_TOP - 1, F + 13)), pt(HF, max(LIP_TOP, F + 14))];
    if (sp.long)
        topPts = concatenateArrays([topPts, [pt(HF - 10, 25), pt(-(a - 22), 21), pt(-(a - 4), 26), pt(-(a + 12), zT - 12), pt(-(a + 28), zT), pt(-(b - 14), zT - 1), pt(-b, zT - 22)]]);
    else
        topPts = concatenateArrays([topPts, [pt(HF - 10, 42), pt(-124, zT - 10), pt(-140, zT), pt(-(b - 13), zT - 2), pt(-b, zT - 22)]]);
    const sideSk = newSketchOnPlane(context, id + "side", { "sketchPlane" : plane(p3(0, 0, 0), vector(1, 0, 0), vector(0, 1, 0)) });
    skFitSpline(sideSk, "top", { "points" : topPts });
    skLineSegment(sideSk, "f1", { "start" : pt(-b, zT - 22), "end" : pt(-b, 8) });
    skLineSegment(sideSk, "f2", { "start" : pt(-b, 8), "end" : pt(-(b - 8), 0) });
    skLineSegment(sideSk, "bot", { "start" : pt(-(b - 8), 0), "end" : pt(Y0, 0) });
    // hook inner corner: square (1 mm chamfer) into the recess; on the plain chair, clear the rounded seat lip
    const hookZ = recess ? F - 1 : 11;
    const hookY = recess ? Y0 + 1 : -87;
    skLineSegment(sideSk, "hi", { "start" : pt(Y0, 0), "end" : pt(Y0, hookZ) });
    skLineSegment(sideSk, "ch", { "start" : pt(Y0, hookZ), "end" : pt(hookY, F) });
    skLineSegment(sideSk, "fl", { "start" : pt(hookY, F), "end" : pt(SAD_REAR, F) });
    skLineSegment(sideSk, "re", { "start" : pt(SAD_REAR, F), "end" : pt(SAD_REAR, F + 7.5) });
    skSolve(sideSk);
    opExtrude(context, id + "sideX", { "entities" : qSketchRegion(id + "side"), "direction" : vector(1, 0, 0),
                "endBound" : BoundingType.BLIND, "endDepth" : mmv(80), "startBound" : BoundingType.BLIND, "startDepth" : mmv(80) });

    // ---- plan profile (x half width vs y), extruded along z ----
    var half = [pt(SAD_HW_REAR, SAD_REAR), pt((SAD_HW_REAR + HOOK_HW) / 2, (SAD_REAR + Y0) / 2), pt(HOOK_HW, Y0), pt(HOOK_HW - 3, HF - 2)];
    if (sp.long)
        half = concatenateArrays([half, [pt(18, HF - 16), pt(18, -(a - 14)), pt(sp.wR / 2, -(a + 6)), pt(sp.wF / 2, -(b - 12))]]);
    else
        half = concatenateArrays([half, [pt(max(sp.wR / 2, 30), HF - 14), pt(sp.wF / 2, -(b - 12))]]);
    var mir = [];
    for (var i = size(half) - 1; i >= 0; i -= 1)
        mir = append(mir, vector(-half[i][0], half[i][1]));
    const plan = newSketchOnPlane(context, id + "plan", { "sketchPlane" : XY_PLANE });
    skFitSpline(plan, "r", { "points" : half });
    skArc(plan, "front", { "start" : half[size(half) - 1], "mid" : pt(0, -b), "end" : mir[0] });
    skFitSpline(plan, "l", { "points" : mir });
    skLineSegment(plan, "rear", { "start" : mir[size(mir) - 1], "end" : half[0] });
    skSolve(plan);
    opExtrude(context, id + "planZ", { "entities" : qSketchRegion(id + "plan"), "direction" : vector(0, 0, 1),
                "endBound" : BoundingType.BLIND, "endDepth" : mmv(160), "startBound" : BoundingType.BLIND, "startDepth" : mmv(5) });
    opDeleteBodies(context, id + "delSk", { "entities" : qCreatedBy(id + "side", EntityType.BODY) });
    opBoolean(context, id + "x", { "tools" : qUnion([qCreatedBy(id + "sideX", EntityType.BODY), qCreatedBy(id + "planZ", EntityType.BODY)]),
                "operationType" : BooleanOperationType.INTERSECTION });
    var body = qUnion(evaluateQuery(context, qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID)));

    // ---- rounding: big radii where legs touch, small on faces hidden in the chair recess, 45 deg chamfer on the bed edge ----
    const bedFace = qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), XY_PLANE);
    const nCh = normalize(vector(0, -(F - hookZ), hookY - Y0));
    const hidden = qUnion([qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, 0, F), vector(0, 0, 1))),
                qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, Y0, 0), vector(0, 1, 0))),
                qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, SAD_REAR, 0), vector(0, 1, 0))),
                qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, Y0, hookZ), nCh))]);
    const bedEdges0 = qAdjacent(bedFace, AdjacencyType.EDGE, EntityType.EDGE);
    const legEdges = qEdgeConvexityTypeFilter(qSubtraction(qOwnedByBody(body, EntityType.EDGE), qUnion([bedEdges0, qAdjacent(hidden, AdjacencyType.EDGE, EntityType.EDGE)])), EdgeConvexityType.CONVEX);
    const r1 = roundAdaptive(context, id + "rndLeg", legEdges, LEG_R, false);
    // bed edge: 45 deg chamfer (prints without support, softer than a sharp corner at the shins)
    const bedEdges = qEdgeConvexityTypeFilter(qAdjacent(qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), XY_PLANE), AdjacencyType.EDGE, EntityType.EDGE), EdgeConvexityType.CONVEX);
    const rb = roundAdaptive(context, id + "bedCh", bedEdges, [2, 1.5, 1, 0.6], true);

    // hidden edges (saddle underside, hook inner face, rear face): small
    const hidden2 = qUnion([qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, 0, F), vector(0, 0, 1))),
                qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, Y0, 0), vector(0, 1, 0))),
                qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, SAD_REAR, 0), vector(0, 1, 0))),
                qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), plane(p3(0, Y0, hookZ), nCh))]);
    const bedFace2 = qCoincidesWithPlane(qOwnedByBody(body, EntityType.FACE), XY_PLANE);
    const r2 = roundLoop(context, id + "rndHid", qSubtraction(qAdjacent(hidden2, AdjacencyType.EDGE, EntityType.EDGE),
                    qAdjacent(bedFace2, AdjacencyType.EDGE, EntityType.EDGE)), [2, 1.5, 1, 0.5]);
    // ---- seat-level filler: the big rim rounding must not leave a groove between the saddle and the recess wall.
    //      Refill the saddle footprint up to the seat surface (z 20) so the seat reads continuous; only what is above the seat stays rounded.
    if (recess)
    {
        opExtrude(context, id + "fillP", { "entities" : qSketchRegion(id + "plan"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : mmv(20) });
        fCuboid(context, id + "fillB", { "corner1" : p3(-60, Y0, F), "corner2" : p3(60, SAD_REAR, 20) });
        opBoolean(context, id + "fillX", { "tools" : qUnion([qCreatedBy(id + "fillP", EntityType.BODY), qCreatedBy(id + "fillB", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.INTERSECTION });
        opBoolean(context, id + "fillU", { "tools" : qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.UNION });
    }
    body = qUnion(evaluateQuery(context, qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID)));
    if (size(evaluateQuery(context, body)) != 1)
        throw regenError("Pommel body did not come out as one solid");

    // ---- bolt holes (countersunk M3) ----
    var tools = [];
    var boltTops = [];
    for (var j = 0; j < size(BOLTS_Y); j += 1)
    {
        const y = BOLTS_Y[j];
        const hits = evRaycast(context, { "entities" : body, "ray" : line(p3(0, y, 200), vector(0, 0, -1)) });
        var zt = mmv(F + 4);
        if (size(hits) > 0)
            zt = hits[0].intersection[2];
        boltTops = append(boltTops, round(zt / millimeter * 10) / 10);
        tools = append(tools, cyl(context, id + ("bh" ~ j), p3(0, y, F - 1), vector(0 * meter, mmv(y), zt + mmv(1)), mmv(1.7)));
        fCone(context, id + ("cs" ~ j), { "bottomCenter" : vector(0 * meter, mmv(y), zt - mmv(1.5)), "topCenter" : vector(0 * meter, mmv(y), zt + mmv(0.01)),
                    "bottomRadius" : mmv(1.7), "topRadius" : mmv(3.2) });
        tools = append(tools, qCreatedBy(id + ("cs" ~ j), EntityType.BODY));
    }
    // ---- rod channels with crush ribs (long) ----
    var ribs = [];
    if (sp.long)
    {
        const yEnd = -(b - 15);
        for (var s in [-1, 1])
        {
            tools = append(tools, teardropY(context, id + ("ch" ~ (s + 1)), s * ROD_X, ROD_Z, Y0 + 1, yEnd, ROD_CH_D / 2));
            fCone(context, id + ("li" ~ (s + 1)), { "bottomCenter" : p3(s * ROD_X, Y0 + 0.5, ROD_Z), "topCenter" : p3(s * ROD_X, Y0 - 1, ROD_Z),
                        "bottomRadius" : mmv(ROD_CH_D / 2 + 1.5), "topRadius" : mmv(ROD_CH_D / 2) });
            tools = append(tools, qCreatedBy(id + ("li" ~ (s + 1)), EntityType.BODY));
            const zones = [[Y0 - 3, Y0 - 3 - RIB_ZONE], [yEnd + RIB_ZONE, yEnd]];
            for (var zi = 0; zi < 2; zi += 1)
                for (var k = 0; k < 3; k += 1)
                {
                    const ang = (-90 + 120 * k) * degree;      // bottom rib + 2 upper ribs
                    const rc = ROD_CH_D / 2 + 0.5 - RIB_I;
                    const cx = s * ROD_X + rc * cos(ang);
                    const cz = ROD_Z + rc * sin(ang);
                    ribs = append(ribs, cyl(context, id + ("rib" ~ (s + 1) ~ "_" ~ zi ~ "_" ~ k), p3(cx, zones[zi][0], cz), p3(cx, zones[zi][1], cz), mmv(0.5)));
                }
        }
    }
    // ---- size code debossed into the flat bed face, just in front of the hook (reads correctly when the part is turned over) ----
    const ts = newSketchOnPlane(context, id + "ts", { "sketchPlane" : plane(p3(0, 0, 0), vector(0, 0, -1), vector(1, 0, 0)) });
    skText(ts, "code", { "text" : sp.code, "fontName" : "OpenSans-Bold.ttf", "firstCorner" : pt(-8, -HF + 6), "secondCorner" : pt(8, -HF + 11) });
    skSolve(ts);
    opExtrude(context, id + "txt", { "entities" : qSketchRegion(id + "ts", true), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : mmv(0.6) });
    tools = append(tools, qCreatedBy(id + "txt", EntityType.BODY));
    opBoolean(context, id + "cut", { "tools" : qUnion(tools), "targets" : body, "operationType" : BooleanOperationType.SUBTRACTION });
    opDeleteBodies(context, id + "delTs", { "entities" : qCreatedBy(id + "ts", EntityType.BODY) });
    if (size(ribs) > 0)
        opBoolean(context, id + "ribs", { "tools" : qUnion(concatenateArrays([[body], ribs])), "operationType" : BooleanOperationType.UNION });
    const out = qUnion(evaluateQuery(context, qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID)));
    setProperty(context, { "entities" : out, "propertyType" : PropertyType.NAME, "value" : "pommel_" ~ sp.code });

    // ---- integral snap-off support under the saddle tail (the only overhang when printed bed-face down) ----
    // Thin rib grid from the bed up to one layer under the saddle floor; 0.8 mm clear of the hook's inner face.
    // A separate body: export it together with its pommel (one STL/3MF). Peels off by hand; the face it touches is hidden in the recess.
    if (withSupport)
    {
        const zTop = F - SUP_GAP;
        const yFront = (recess ? Y0 : hookY) + 0.8;
        opExtrude(context, id + "supP", { "entities" : qSketchRegion(id + "plan"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : mmv(zTop) });
        fCuboid(context, id + "supB", { "corner1" : p3(-60, yFront, 0), "corner2" : p3(60, SAD_REAR - 0.6, zTop) });
        opBoolean(context, id + "supX", { "tools" : qUnion([qCreatedBy(id + "supP", EntityType.BODY), qCreatedBy(id + "supB", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.INTERSECTION });
        var comb = [];
        for (var k = -6; k <= 6; k += 1)       // ribs along the saddle, every 6.5 mm
        {
            fCuboid(context, id + ("sr" ~ (k + 6)), { "corner1" : p3(k * 6.5 - 0.4, yFront - 1, 0), "corner2" : p3(k * 6.5 + 0.4, SAD_REAR + 1, zTop) });
            comb = append(comb, qCreatedBy(id + ("sr" ~ (k + 6)), EntityType.BODY));
        }
        for (var yc in [yFront + 0.4, (yFront + SAD_REAR) / 2, SAD_REAR - 1])   // 3 cross ribs tie the grid together
        {
            fCuboid(context, id + ("sc" ~ size(comb)), { "corner1" : p3(-60, yc - 0.4, 0), "corner2" : p3(60, yc + 0.4, zTop) });
            comb = append(comb, qCreatedBy(id + ("sc" ~ size(comb)), EntityType.BODY));
        }
        opBoolean(context, id + "supC", { "tools" : qUnion(comb), "operationType" : BooleanOperationType.UNION });
        opBoolean(context, id + "supI", { "tools" : qSubtraction(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), out),
                    "operationType" : BooleanOperationType.INTERSECTION });
        const sup = qSubtraction(qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID), out);
        setProperty(context, { "entities" : sup, "propertyType" : PropertyType.NAME, "value" : "pommel_" ~ sp.code ~ "_support" });
    }
    opDeleteBodies(context, id + "delPl", { "entities" : qCreatedBy(id + "plan", EntityType.BODY) });

    // ---- reference carbon rods ----
    if (sp.long && showRods)
    {
        for (var s in [-1, 1])
        {
            const rod = cyl(context, id + ("rod" ~ (s + 1)), p3(s * ROD_X, SAD_REAR, ROD_Z), p3(s * ROD_X, -(b - 15), ROD_Z), mmv(3));
            setProperty(context, { "entities" : rod, "propertyType" : PropertyType.NAME, "value" : "carbon_rod_6mm_" ~ round(b - 15 + SAD_REAR) ~ "mm" });
        }
    }
    const bb = evBox3d(context, { "topology" : out, "tight" : true });
    const sz = (bb.maxCorner - bb.minCorner) / millimeter;
    logMsg(context, sp.code ~ (recess ? " recess" : " plain") ~ " round=" ~ r1 ~ "/" ~ r2 ~ " bedCh=" ~ rb ~ " size(mm)=" ~ round(sz[0]) ~ "x" ~ round(sz[1]) ~ "x" ~ round(sz[2]) ~
            " vol_cm3=" ~ round(evVolume(context, { "entities" : out }) / centimeter ^ 3 * 10) / 10 ~
            " area_cm2=" ~ round(evArea(context, { "entities" : qOwnedByBody(out, EntityType.FACE) }) / centimeter ^ 2) ~
            " bodies=" ~ size(evaluateQuery(context, out)) ~ " boltTop_z=" ~ boltTops ~
            (sp.long ? " rods=" ~ round(b - 15 + SAD_REAR) ~ "mm" : ""));
    return out;
}

// Rod fit test: a 96 x 15 x 14 mm bar printed the same way as the real parts (holes horizontal).
// Left 3 = chair rod hole 6.3 / 6.4 / 6.5 mm.  Right 3 = pommel channel 6.4 mm with ribs pressing 0.15 / 0.25 / 0.35 mm.
// Push a cut rod into each: the chair hole should slide in by hand, the channel should need a firm push and hold the rod.
function buildFitTest(context is Context, id is Id, x0 is number, y0off is number) returns Query
{
    fCuboid(context, id + "blk", { "corner1" : p3(x0 - 48, 0, 0), "corner2" : p3(x0 + 48, 15, 14) });
    const blk = qCreatedBy(id + "blk", EntityType.BODY);
    try silent
    {
        opFillet(context, id + "rnd", { "entities" : qSubtraction(qOwnedByBody(blk, EntityType.EDGE),
                            qAdjacent(qCoincidesWithPlane(qOwnedByBody(blk, EntityType.FACE), XY_PLANE), AdjacencyType.EDGE, EntityType.EDGE)), "radius" : mmv(1.5) });
    }
    var tools = [];
    var ribs = [];
    const labels = ["6.3", "6.4", "6.5", ".15", ".25", ".35"];
    for (var i = 0; i < 6; i += 1)
    {
        const x = x0 - 40 + i * 16;
        const d = i < 3 ? 6.3 + 0.1 * i : ROD_CH_D;
        tools = append(tools, teardropY(context, id + ("h" ~ i), x, 7, -1, 16, d / 2));
        for (var e = 0; e < 2; e += 1)
        {
            const y0 = e == 0 ? -0.01 : 15.01;
            const y1 = e == 0 ? 1 : 14;
            fCone(context, id + ("li" ~ i ~ "_" ~ e), { "bottomCenter" : p3(x, y0, 7), "topCenter" : p3(x, y1, 7), "bottomRadius" : mmv(d / 2 + 1), "topRadius" : mmv(d / 2) });
            tools = append(tools, qCreatedBy(id + ("li" ~ i ~ "_" ~ e), EntityType.BODY));
        }
        if (i >= 3)
            for (var k = 0; k < 3; k += 1)
            {
                const ang = (-90 + 120 * k) * degree;
                const rc = ROD_CH_D / 2 + 0.5 - (0.15 + 0.1 * (i - 3));
                ribs = append(ribs, cyl(context, id + ("rib" ~ i ~ "_" ~ k), p3(x + rc * cos(ang), 2, 7 + rc * sin(ang)), p3(x + rc * cos(ang), 13, 7 + rc * sin(ang)), mmv(0.5)));
            }
        const ts = newSketchOnPlane(context, id + ("ts" ~ i), { "sketchPlane" : plane(p3(0, 0, 14), vector(0, 0, 1), vector(1, 0, 0)) });
        skText(ts, "t", { "text" : labels[i], "fontName" : "OpenSans-Bold.ttf", "firstCorner" : pt(x - 6, 2.5), "secondCorner" : pt(x + 6, 6) });
        skSolve(ts);
        opExtrude(context, id + ("tx" ~ i), { "entities" : qSketchRegion(id + ("ts" ~ i), true), "direction" : vector(0, 0, -1), "endBound" : BoundingType.BLIND, "endDepth" : mmv(0.5) });
        tools = append(tools, qCreatedBy(id + ("tx" ~ i), EntityType.BODY));
        opDeleteBodies(context, id + ("dts" ~ i), { "entities" : qCreatedBy(id + ("ts" ~ i), EntityType.BODY) });
    }
    opBoolean(context, id + "cut", { "tools" : qUnion(tools), "targets" : blk, "operationType" : BooleanOperationType.SUBTRACTION });
    opBoolean(context, id + "ribs", { "tools" : qUnion(concatenateArrays([[blk], ribs])), "operationType" : BooleanOperationType.UNION });
    const out = qUnion(evaluateQuery(context, qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID)));
    setProperty(context, { "entities" : out, "propertyType" : PropertyType.NAME, "value" : "rod_fit_test" });
    opTransform(context, id + "mv", { "bodies" : out, "transform" : transform(vector(0, y0off - 280, 0) * millimeter) });
    logMsg(context, "fit_test vol_cm3=" ~ round(evVolume(context, { "entities" : out }) / centimeter ^ 3 * 10) / 10 ~ " bodies=" ~ size(evaluateQuery(context, out)));
    return out;
}

// ---------------- chair change (use in the chair doc, on a branch) ----------------
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

// collect a one-line summary per part; shown as the feature's info notice (hover the feature)
function logMsg(context is Context, msg is string)
{
    var prev = "";
    try silent
    {
        prev = getVariable(context, "gbgLog") ~ " | ";
    }
    setVariable(context, "gbgLog", prev ~ msg);
    println(msg);
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

// Round (or 45 deg chamfer) a set of edges as large as possible: all together at the first two sizes, otherwise edge by edge
// down the ladder, so one awkward edge can't pull every other edge down to its size. Returns the smallest size used.
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

function roundLoop(context is Context, id is Id, edges is Query, radii is array) returns number
{
    if (size(evaluateQuery(context, edges)) == 0)
        return 0;
    for (var r in radii)
    {
        var ok = false;
        try silent
        {
            opFillet(context, id + ("r" ~ r), { "entities" : edges, "radius" : mmv(r) });
            ok = true;
        }
        if (ok)
            return r;
    }
    return 0;
}
