namespace Expressif.Planning;

#pragma warning disable SA1313 // Preserve record-compatible public constructor parameter names.

/// <summary>
/// Static schema information discovered from a logical plan.
/// </summary>
public abstract record LogicalSchema
{
    internal abstract bool IsKnownVariant { get; }
}

/// <summary>
/// Indicates that a plan does not require an externally supplied input value.
/// </summary>
public sealed record NoInputLogicalSchema : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A value for which no more precise static schema is known.
/// </summary>
public sealed record AnyLogicalSchema(bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A scalar Expressif semantic type.
/// </summary>
public sealed record ScalarLogicalSchema(string Type, bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A field in a record schema.
/// </summary>
public sealed record LogicalSchemaField(LogicalSchema Schema, bool Optional = false);

/// <summary>
/// A record with statically known fields.
/// </summary>
public sealed record RecordLogicalSchema : LogicalSchema
{
    private IReadOnlyDictionary<string, LogicalSchemaField> fields =
        PlanningCollections.Freeze(new Dictionary<string, LogicalSchemaField>());
    public RecordLogicalSchema(
        IReadOnlyDictionary<string, LogicalSchemaField> Fields,
        bool AllowsAdditionalFields = true,
        bool IsNullable = false)
        => (this.Fields, this.AllowsAdditionalFields, this.IsNullable) =
            (PlanningCollections.Freeze(Fields), AllowsAdditionalFields, IsNullable);
    public IReadOnlyDictionary<string, LogicalSchemaField> Fields
    {
        get => fields;
        init => fields = PlanningCollections.Freeze(value);
    }
    public bool AllowsAdditionalFields { get; init; }
    public bool IsNullable { get; init; }
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// An array whose elements share a schema.
/// </summary>
public sealed record ArrayLogicalSchema(LogicalSchema Items, bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A fixed-size tuple with a schema for each position.
/// </summary>
public sealed record TupleLogicalSchema : LogicalSchema
{
    private IReadOnlyList<LogicalSchema> items = PlanningCollections.Empty<LogicalSchema>();
    public TupleLogicalSchema(
        IReadOnlyList<LogicalSchema> Items,
        bool IsNullable = false,
        LogicalSchema? AdditionalItems = null)
        => (this.Items, this.IsNullable, this.AdditionalItems) =
            (PlanningCollections.Freeze(Items), IsNullable, AdditionalItems);
    public IReadOnlyList<LogicalSchema> Items
    {
        get => items;
        init => items = PlanningCollections.Freeze(value);
    }
    public bool IsNullable { get; init; }
    public LogicalSchema? AdditionalItems { get; init; }
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A key/value pair with independently inferred component schemas.
/// </summary>
public sealed record PairLogicalSchema(
    LogicalSchema Key,
    LogicalSchema Value,
    bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A dictionary with independently inferred key and value schemas.
/// </summary>
public sealed record DictionaryLogicalSchema(
    LogicalSchema Keys,
    LogicalSchema Values,
    bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A grouping whose keys identify arrays of values with a common item schema.
/// </summary>
public sealed record GroupingLogicalSchema(
    LogicalSchema Keys,
    LogicalSchema Items,
    bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A normalized sort table retaining the schema of each original row value.
/// </summary>
public sealed record SortTableLogicalSchema(
    LogicalSchema Items,
    bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A value matching one of several alternative schemas.
/// </summary>
public sealed record UnionLogicalSchema : LogicalSchema
{
    private IReadOnlyList<LogicalSchema> alternatives = PlanningCollections.Empty<LogicalSchema>();
    public UnionLogicalSchema(IReadOnlyList<LogicalSchema> Alternatives, bool IsNullable = false)
        => (this.Alternatives, this.IsNullable) = (PlanningCollections.Freeze(Alternatives), IsNullable);
    public IReadOnlyList<LogicalSchema> Alternatives
    {
        get => alternatives;
        init => alternatives = PlanningCollections.Freeze(value);
    }
    public bool IsNullable { get; init; }
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// Mutually incompatible schema constraints.
/// </summary>
public sealed record ConflictingLogicalSchema(
    LogicalSchema Left,
    LogicalSchema Right,
    bool IsNullable = false) : LogicalSchema
{
    internal override bool IsKnownVariant => true;
}

public enum SchemaAnalysisCompleteness
{
    Known,
    Partial,
    Dynamic,
    Conflicting,
}

/// <summary>
/// A structured diagnostic produced during schema analysis.
/// </summary>
public sealed record SchemaAnalysisDiagnostic(string Code, string Path, string Message);

/// <summary>
/// Input and output schema information for one logical-plan node.
/// </summary>
public sealed record SchemaAnalysisNode(
    string Path,
    string Kind,
    string? Operator,
    LogicalSchema Input,
    LogicalSchema Output);

/// <summary>
/// The input requirements and output schema discovered for a logical plan.
/// </summary>
public sealed record SchemaAnalysis
{
    private IReadOnlyList<SchemaAnalysisDiagnostic> diagnostics = PlanningCollections.Empty<SchemaAnalysisDiagnostic>();
    private IReadOnlyList<SchemaAnalysisNode> nodes = PlanningCollections.Empty<SchemaAnalysisNode>();
    public SchemaAnalysis(
        LogicalSchema input,
        LogicalSchema output,
        SchemaAnalysisCompleteness completeness,
        IReadOnlyList<SchemaAnalysisDiagnostic> diagnostics,
        IReadOnlyList<SchemaAnalysisNode> nodes)
        => (Input, Output, Completeness, Diagnostics, Nodes) =
            (input, output, completeness, PlanningCollections.Freeze(diagnostics), PlanningCollections.Freeze(nodes));
    public LogicalSchema Input { get; init; }
    public LogicalSchema Output { get; init; }
    public SchemaAnalysisCompleteness Completeness { get; init; }
    public IReadOnlyList<SchemaAnalysisDiagnostic> Diagnostics
    {
        get => diagnostics;
        init => diagnostics = PlanningCollections.Freeze(value);
    }
    public IReadOnlyList<SchemaAnalysisNode> Nodes
    {
        get => nodes;
        init => nodes = PlanningCollections.Freeze(value);
    }
}

#pragma warning restore SA1313
