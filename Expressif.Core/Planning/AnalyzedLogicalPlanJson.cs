using System.Text.Json;
using System.Text.Json.Nodes;

namespace Expressif.Planning;

/// <summary>
/// Writes a logical plan with schema annotations attached to every analyzed value.
/// </summary>
public static class AnalyzedLogicalPlanJson
{
    public const string FormatName = "expressif.analyzed-logical-plan";
    public const int FormatVersion = 1;

    public static string Serialize(AnalyzedLogicalPlan analyzed, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(analyzed);
        var logical = JsonNode.Parse(LogicalPlanJson.Serialize(analyzed.Plan))!.AsObject();
        var schemas = JsonNode.Parse(SchemaAnalysisJson.Serialize(analyzed.Analysis))!.AsObject();
        var annotations = schemas["nodes"]!.AsArray()
            .Select(node => node!.AsObject())
            .ToDictionary(node => node["path"]!.GetValue<string>(), StringComparer.Ordinal);
        var plan = logical["plan"]!.AsObject();
        Annotate(plan, "plan", annotations);

        var document = new JsonObject
        {
            ["format"] = FormatName,
            ["version"] = FormatVersion,
            ["catalogCompatibility"] = LogicalPlanJson.CatalogCompatibility,
            ["completeness"] = schemas["completeness"]!.DeepClone(),
            ["diagnostics"] = schemas["diagnostics"]!.DeepClone(),
            ["plan"] = plan.DeepClone(),
        };
        return document.ToJsonString(new JsonSerializerOptions { WriteIndented = indented });
    }

    private static void Annotate(
        JsonObject value,
        string path,
        IReadOnlyDictionary<string, JsonObject> annotations)
    {
        if (annotations.TryGetValue(path, out var annotation))
        {
            value["input"] = annotation["input"]!.DeepClone();
            value["output"] = annotation["output"]!.DeepClone();
        }

        if (value["items"] is JsonArray items)
        {
            for (var index = 0; index < items.Count; index++)
                Annotate(items[index]!.AsObject(), $"{path}.items[{index}]", annotations);
        }

        if (value["arguments"] is not JsonArray arguments)
            return;
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index]!.AsObject();
            if (argument["value"] is JsonObject argumentValue)
                Annotate(argumentValue, $"{path}.arguments[{index}].value", annotations);
        }
    }
}
