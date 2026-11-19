FeatureScript 3112;
import(path : "onshape/std/common.fs", version : "3112.0");
export enum SidePadKit { LOW, HIGH }
function writeReport(context is Context, id is Id, rep is string)
{
    const ch = splitIntoCharacters(rep);
    var k = 0;  var i = 0;
    while (i < size(ch))
    {
        var piece = "";
        for (var j = i; j < min(i + 200, size(ch)); j += 1) piece = piece ~ ch[j];
        fCuboid(context, id + ("rp" ~ k), { "corner1" : vector(2000 + 2 * k, 2000, 0) * millimeter, "corner2" : vector(2001 + 2 * k, 2001, 1) * millimeter });
        setProperty(context, { "entities" : qCreatedBy(id + ("rp" ~ k), EntityType.BODY), "propertyType" : PropertyType.NAME, "value" : "RPT" ~ (k < 10 ? "0" : "") ~ k ~ " " ~ piece });
        i += 200;  k += 1;
    }
}
annotation { "Feature Type Name" : "GbG Side Pad review test" }
export const gbgSidePadReview = defineFeature(function(context is Context, id is Id, definition is map)
    precondition { }
    {
        var rep = "";
        try
        {
            const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(vector(0, 0, 0) * millimeter, vector(0, 0, 1), vector(1, 0, 0)) });
            skRectangle(sk, "r", { "firstCorner" : vector(0, 0) * millimeter, "secondCorner" : vector(10, 10) * millimeter });
            skSolve(sk);
            const dir = normalize(vector(0.5, 0, 1));
            opExtrude(context, id + "ex", { "entities" : qSketchRegion(id + "sk"), "direction" : dir, "endBound" : BoundingType.BLIND, "endDepth" : 20 * millimeter });
            const bb = evBox3d(context, { "topology" : qCreatedBy(id + "ex", EntityType.BODY) });
            rep = "box " ~ toString(bb.minCorner / millimeter) ~ " .. " ~ toString(bb.maxCorner / millimeter) ~ " vol " ~ evVolume(context, { "entities" : qCreatedBy(id + "ex", EntityType.BODY) }) / millimeter ^ 3;
            // non-rigid transform test
            var ok2 = "rigid-only";
            try { opTransform(context, id + "sh", { "bodies" : qCreatedBy(id + "ex", EntityType.BODY), "transform" : transform(matrix([[1, 0, 0.3], [0, 1, 0], [0, 0, 1]]), vector(0, 0, 0) * millimeter) }); ok2 = "shear ok"; } catch (e) { ok2 = "shear fails " ~ toString(e); }
            const bb2 = evBox3d(context, { "topology" : qCreatedBy(id + "ex", EntityType.BODY) });
            rep = rep ~ " | " ~ ok2 ~ " box2 " ~ toString(bb2.minCorner / millimeter) ~ " .. " ~ toString(bb2.maxCorner / millimeter);
            var vars = "ctxvar: ";
            try silent { vars = vars ~ getVariable(context, "zzz"); } 
            rep = rep ~ " | " ~ vars;
        }
        catch (e) { rep = "ERR " ~ toString(e); }
        writeReport(context, id + "rep", rep);
    }, {});
