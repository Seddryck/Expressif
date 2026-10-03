namespace Expressif.Planning;

internal sealed class RecordSchemaInferenceRule(
    SchemaAlgebra algebra,
    ICollection<SchemaAnalysisDiagnostic> diagnostics)
{
    public LogicalSchema Infer(
        LogicalCall call,
        string path,
        Func<int, LogicalSchema?> inferArgument,
        Func<LogicalCall, string?> literalName)
    {
        var contributions = call.Arguments
            .Select((argument, index) => (Argument: argument, Index: index))
            .Where(item => item.Argument.IsExplicit && item.Argument.Value is not null)
            .Select(item => (item.Argument, Schema: inferArgument(item.Index)))
            .Select(item => Classify(item.Argument, item.Schema, literalName))
            .ToArray();
        return Fold(contributions, path);
    }

    private static IRecordSchemaContribution Classify(
        LogicalArgument argument,
        LogicalSchema? schema,
        Func<LogicalCall, string?> literalName)
    {
        if (!argument.IsSpread
            && argument.Value is LogicalCall entry
            && entry.Function.Schema?.Intrinsic == "named-entry")
        {
            var name = literalName(entry);
            return name is not null && schema is not null
                ? new NamedRecordContribution(name, new LogicalSchemaField(schema))
                : DynamicRecordContribution.Instance;
        }
        if (!argument.IsSpread
            && argument.Value is LogicalCall nested
            && nested.Function.Schema?.Intrinsic == "record"
            && schema is RecordLogicalSchema nestedRecord)
        {
            return new RecordShapeContribution(nestedRecord);
        }
        if (argument.IsSpread && schema is RecordLogicalSchema spreadRecord)
            return new RecordShapeContribution(spreadRecord);
        return DynamicRecordContribution.Instance;
    }

    private LogicalSchema Fold(IReadOnlyList<IRecordSchemaContribution> contributions, string path)
    {
        var result = RecordSchemaContributionFold.Apply(contributions, algebra);
        var opensShape = result.AllowsAdditionalFields;
        if (opensShape)
        {
            diagnostics.Add(new SchemaAnalysisDiagnostic(
                "schema.dynamic",
                path,
                "Record contribution has a dynamic shape."));
        }
        return result;
    }
}

internal interface IRecordSchemaContribution;

internal sealed record NamedRecordContribution(
    string Name,
    LogicalSchemaField Field) : IRecordSchemaContribution;

internal sealed record RecordShapeContribution(
    RecordLogicalSchema Schema) : IRecordSchemaContribution;

internal sealed record DynamicRecordContribution : IRecordSchemaContribution
{
    public static DynamicRecordContribution Instance { get; } = new();
}

internal static class RecordSchemaContributionFold
{
    public static RecordLogicalSchema Apply(
        IReadOnlyList<IRecordSchemaContribution> contributions,
        SchemaAlgebra algebra)
    {
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        var opensShape = false;
        foreach (var contribution in contributions)
        {
            switch (contribution)
            {
                case NamedRecordContribution named:
                    fields[named.Name] = named.Field;
                    break;
                case RecordShapeContribution shape:
                    Merge(fields, shape.Schema, algebra);
                    opensShape |= shape.Schema.AllowsAdditionalFields;
                    break;
                case DynamicRecordContribution:
                    opensShape = true;
                    break;
            }
        }
        return new RecordLogicalSchema(fields, AllowsAdditionalFields: opensShape);
    }

    private static void Merge(
        IDictionary<string, LogicalSchemaField> fields,
        RecordLogicalSchema source,
        SchemaAlgebra algebra)
    {
        foreach (var field in source.Fields)
        {
            if (field.Value.Optional && fields.TryGetValue(field.Key, out var existing))
            {
                fields[field.Key] = new LogicalSchemaField(
                    SchemaAlgebra.Union(existing.Schema, field.Value.Schema),
                    existing.Optional);
            }
            else
            {
                fields[field.Key] = field.Value;
            }
        }
    }
}
