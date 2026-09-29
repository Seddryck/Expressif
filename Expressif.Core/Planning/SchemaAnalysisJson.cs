using System.Text;
using System.Text.Json;

namespace Expressif.Planning;

/// <summary>
/// Writes deterministic, machine-readable schema-analysis JSON.
/// </summary>
public static class SchemaAnalysisJson
{
    public const string FormatName = "expressif.schema-analysis";
    public const int FormatVersion = 1;

    public static string Serialize(SchemaAnalysis analysis, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = indented }))
        {
            writer.WriteStartObject();
            writer.WriteString("format", FormatName);
            writer.WriteNumber("version", FormatVersion);
            writer.WritePropertyName("input");
            WriteSchema(writer, analysis.Input);
            writer.WritePropertyName("output");
            WriteSchema(writer, analysis.Output);
            writer.WriteString("completeness", Completeness(analysis.Completeness));
            writer.WritePropertyName("nodes");
            writer.WriteStartArray();
            foreach (var node in analysis.Nodes.OrderBy(node => node.Path, StringComparer.Ordinal))
                WriteNode(writer, node);
            writer.WriteEndArray();
            writer.WritePropertyName("diagnostics");
            writer.WriteStartArray();
            foreach (var diagnostic in analysis.Diagnostics
                         .OrderBy(diagnostic => diagnostic.Path, StringComparer.Ordinal)
                         .ThenBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
                         .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("code", diagnostic.Code);
                writer.WriteString("path", diagnostic.Path);
                writer.WriteString("message", diagnostic.Message);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteNode(Utf8JsonWriter writer, SchemaAnalysisNode node)
    {
        writer.WriteStartObject();
        writer.WriteString("path", node.Path);
        writer.WriteString("kind", node.Kind);
        if (node.Operator is not null)
            writer.WriteString("operator", node.Operator);
        writer.WritePropertyName("input");
        WriteSchema(writer, node.Input);
        writer.WritePropertyName("output");
        WriteSchema(writer, node.Output);
        writer.WriteEndObject();
    }

    private static void WriteSchema(Utf8JsonWriter writer, LogicalSchema schema)
    {
        writer.WriteStartObject();
        switch (schema)
        {
            case NoInputLogicalSchema:
                writer.WriteString("type", "none");
                break;
            case AnyLogicalSchema any:
                writer.WriteString("type", "any");
                writer.WriteBoolean("nullable", any.IsNullable);
                break;
            case ScalarLogicalSchema scalar:
                writer.WriteString("type", scalar.Type);
                writer.WriteBoolean("nullable", scalar.IsNullable);
                break;
            case RecordLogicalSchema record:
                writer.WriteString("type", "record");
                writer.WriteBoolean("nullable", record.IsNullable);
                writer.WriteBoolean("additionalFields", record.AllowsAdditionalFields);
                writer.WritePropertyName("fields");
                writer.WriteStartObject();
                foreach (var field in record.Fields.OrderBy(field => field.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(field.Key);
                    writer.WriteStartObject();
                    writer.WriteBoolean("optional", field.Value.Optional);
                    writer.WritePropertyName("schema");
                    WriteSchema(writer, field.Value.Schema);
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
                break;
            case ArrayLogicalSchema array:
                writer.WriteString("type", "array");
                writer.WriteBoolean("nullable", array.IsNullable);
                writer.WritePropertyName("items");
                WriteSchema(writer, array.Items);
                break;
            case TupleLogicalSchema tuple:
                writer.WriteString("type", "tuple");
                writer.WriteBoolean("nullable", tuple.IsNullable);
                writer.WritePropertyName("items");
                writer.WriteStartArray();
                foreach (var item in tuple.Items)
                    WriteSchema(writer, item);
                writer.WriteEndArray();
                break;
            case ConflictingLogicalSchema conflict:
                writer.WriteString("type", "conflict");
                writer.WriteBoolean("nullable", conflict.IsNullable);
                writer.WritePropertyName("left");
                WriteSchema(writer, conflict.Left);
                writer.WritePropertyName("right");
                WriteSchema(writer, conflict.Right);
                break;
            default:
                throw new ArgumentException(
                    $"Unsupported logical schema '{schema.GetType().Name}'.",
                    nameof(schema));
        }
        writer.WriteEndObject();
    }

    private static string Completeness(SchemaAnalysisCompleteness completeness) => completeness switch
    {
        SchemaAnalysisCompleteness.Known => "known",
        SchemaAnalysisCompleteness.Partial => "partial",
        SchemaAnalysisCompleteness.Dynamic => "dynamic",
        SchemaAnalysisCompleteness.Conflicting => "conflicting",
        _ => throw new ArgumentOutOfRangeException(nameof(completeness), completeness, null),
    };
}
