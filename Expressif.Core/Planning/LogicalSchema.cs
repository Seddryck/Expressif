namespace Expressif.Planning;

/// <summary>
/// Static schema information discovered from a logical plan.
/// </summary>
public abstract record LogicalSchema;

/// <summary>
/// Indicates that a plan does not require an externally supplied input value.
/// </summary>
public sealed record NoInputLogicalSchema : LogicalSchema;

/// <summary>
/// A value for which no more precise static schema is known.
/// </summary>
public sealed record AnyLogicalSchema(bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A scalar Expressif semantic type.
/// </summary>
public sealed record ScalarLogicalSchema(string Type, bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A field in a record schema.
/// </summary>
public sealed record LogicalSchemaField(LogicalSchema Schema, bool Optional = false);

/// <summary>
/// A record with statically known fields.
/// </summary>
public sealed record RecordLogicalSchema(
    IReadOnlyDictionary<string, LogicalSchemaField> Fields,
    bool AllowsAdditionalFields = true,
    bool IsNullable = false) : LogicalSchema;

/// <summary>
/// An array whose elements share a schema.
/// </summary>
public sealed record ArrayLogicalSchema(LogicalSchema Items, bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A fixed-size tuple with a schema for each position.
/// </summary>
public sealed record TupleLogicalSchema(
    IReadOnlyList<LogicalSchema> Items,
    bool IsNullable = false,
    LogicalSchema? AdditionalItems = null)
    : LogicalSchema;

/// <summary>
/// A key/value pair with independently inferred component schemas.
/// </summary>
public sealed record PairLogicalSchema(
    LogicalSchema Key,
    LogicalSchema Value,
    bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A dictionary with independently inferred key and value schemas.
/// </summary>
public sealed record DictionaryLogicalSchema(
    LogicalSchema Keys,
    LogicalSchema Values,
    bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A grouping whose keys identify arrays of values with a common item schema.
/// </summary>
public sealed record GroupingLogicalSchema(
    LogicalSchema Keys,
    LogicalSchema Items,
    bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A normalized sort table retaining the schema of each original row value.
/// </summary>
public sealed record SortTableLogicalSchema(
    LogicalSchema Items,
    bool IsNullable = false) : LogicalSchema;

/// <summary>
/// A value matching one of several alternative schemas.
/// </summary>
public sealed record UnionLogicalSchema(
    IReadOnlyList<LogicalSchema> Alternatives,
    bool IsNullable = false) : LogicalSchema;

/// <summary>
/// Mutually incompatible schema constraints.
/// </summary>
public sealed record ConflictingLogicalSchema(
    LogicalSchema Left,
    LogicalSchema Right,
    bool IsNullable = false) : LogicalSchema;

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
public sealed record SchemaAnalysis(
    LogicalSchema Input,
    LogicalSchema Output,
    SchemaAnalysisCompleteness Completeness,
    IReadOnlyList<SchemaAnalysisDiagnostic> Diagnostics,
    IReadOnlyList<SchemaAnalysisNode> Nodes);
