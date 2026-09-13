using System.Text.Json;
using System.Text.Json.Serialization;

namespace Expressif.Functions.Catalog;

public sealed record FunctionDocumentation(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    string Input,
    string Output,
    string Summary,
    FunctionParameterDocumentation[] Parameters,
    string[]? Examples = null,
    string? Behavior = null,
    bool Deprecated = false,
    string? Replacement = null,
    string? Sunset = null,
    bool ReplacementIsEquivalent = false,
    string? MigrationNotes = null,
    FunctionTraversalDocumentation? Traversal = null);

public sealed record FunctionParameterDocumentation(
    string Name,
    string? Type,
    bool Optional,
    string Summary,
    bool Variadic = false,
    int MinimumCardinality = 1,
    string? Kind = null,
    ParameterOmissionDocumentation? Omission = null,
    ParameterEvaluationDocumentation? Evaluation = null)
{
    public string TypeOrKind => Type ?? Kind ?? "any";
}

public sealed record FunctionTraversalDocumentation(string Source, string Selection, string Summary);

public sealed record ParameterEvaluationDocumentation(
    string Frequency,
    string Summary,
    string? Source = null,
    string? Context = null);

[JsonConverter(typeof(ParameterOmissionModeJsonConverter))]
public enum ParameterOmissionMode
{
    Constant,
    EmptyVariadic,
    Absent,
    EnvironmentDerived,
}

public sealed record ParameterOmissionDocumentation(
    ParameterOmissionMode Mode,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] JsonElement Value = default,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Source = null);

public sealed class ParameterOmissionModeJsonConverter : JsonConverter<ParameterOmissionMode>
{
    public override ParameterOmissionMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString() switch
        {
            "constant" => ParameterOmissionMode.Constant,
            "empty-variadic" => ParameterOmissionMode.EmptyVariadic,
            "absent" => ParameterOmissionMode.Absent,
            "environment-derived" => ParameterOmissionMode.EnvironmentDerived,
            var value => throw new JsonException($"Unsupported parameter omission mode '{value}'."),
        };

    public override void Write(Utf8JsonWriter writer, ParameterOmissionMode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value switch
        {
            ParameterOmissionMode.Constant => "constant",
            ParameterOmissionMode.EmptyVariadic => "empty-variadic",
            ParameterOmissionMode.Absent => "absent",
            ParameterOmissionMode.EnvironmentDerived => "environment-derived",
            _ => throw new JsonException($"Unsupported parameter omission mode '{value}'."),
        });
}
