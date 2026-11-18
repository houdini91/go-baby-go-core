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
