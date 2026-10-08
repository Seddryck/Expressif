using System.Text.Json;
using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Syntax;
using Expressif.Collections;

namespace Expressif.Planning;

#pragma warning disable SA1313 // Preserve record-compatible public constructor parameter names.

/// <summary>
/// A portable logical representation of an Expressif expression.
/// </summary>
public sealed record LogicalPlan
{
    private IReadOnlyList<LogicalNamedExpressionDefinition> definitions = PlanningCollections.Empty<LogicalNamedExpressionDefinition>();

    public LogicalPlan(LogicalPipeline Pipeline) => this.Pipeline = Pipeline;
    public LogicalPipeline Pipeline { get; init; }
    public IReadOnlyList<LogicalNamedExpressionDefinition> Definitions
    {
        get => definitions;
        init => definitions = PlanningCollections.Freeze(value);
    }
    public bool HasEntry => Pipeline.Items.Count > 0;
    public void Deconstruct(out LogicalPipeline pipeline) => pipeline = Pipeline;
}

/// <summary>
/// A logical plan together with the schemas discovered for its nodes.
/// </summary>
public sealed record AnalyzedLogicalPlan(LogicalPlan Plan, SchemaAnalysis Analysis);

/// <summary>
/// A value that can appear in a logical plan.
/// </summary>
public abstract record LogicalValue
{
    internal abstract bool IsKnownVariant { get; }
}

/// <summary>
/// A reusable expression declared in a logical document.
/// </summary>
public sealed record LogicalNamedExpressionDefinition
{
    private IReadOnlyList<LogicalNamedExpressionParameter>? parameters;
    private IReadOnlyList<LogicalNamedExpressionReceiver>? receivers;
    public LogicalNamedExpressionDefinition(
        string Name,
        LogicalPipeline Body,
        IReadOnlyList<LogicalNamedExpressionParameter>? Parameters = null,
        IReadOnlyList<LogicalNamedExpressionReceiver>? Receivers = null,
        LogicalTypeContract? InputContract = null,
        LogicalTypeContract? OutputContract = null)
        => (this.Name, this.Body, this.Parameters, this.Receivers, this.InputContract, this.OutputContract) =
            (Name, Body, PlanningCollections.FreezeNullable(Parameters), PlanningCollections.FreezeNullable(Receivers), InputContract, OutputContract);

    public string Name { get; init; }
    public LogicalPipeline Body { get; init; }
    public IReadOnlyList<LogicalNamedExpressionParameter>? Parameters
    {
        get => parameters;
        init => parameters = PlanningCollections.FreezeNullable(value);
    }
    public IReadOnlyList<LogicalNamedExpressionReceiver>? Receivers
    {
        get => receivers;
        init => receivers = PlanningCollections.FreezeNullable(value);
    }
    public LogicalTypeContract? InputContract { get; init; }
    public LogicalTypeContract? OutputContract { get; init; }
    public IReadOnlyList<LogicalNamedExpressionParameter> EffectiveParameters => Parameters ?? [];
    public IReadOnlyList<LogicalNamedExpressionReceiver> EffectiveReceivers => Receivers ?? [];
}

/// <summary>
/// An explicit parameter in a named-expression signature.
/// </summary>
public sealed record LogicalNamedExpressionParameter(
    string Name,
    LogicalLiteral? Default = null,
    LogicalTypeContract? Contract = null);

/// <summary>
/// A named component decomposed from a named expression's tuple input.
/// </summary>
public sealed record LogicalNamedExpressionReceiver(string Name, LogicalTypeContract? Contract = null);

/// <summary>
/// A type boundary on a named-expression signature member.
/// </summary>
public sealed record LogicalTypeContract(string Type, bool Strict = false);

/// <summary>
/// Invokes a definition in the containing logical document.
/// </summary>
public sealed record LogicalNamedExpressionInvocation : LogicalValue
{
    private IReadOnlyList<LogicalValue> arguments = PlanningCollections.Empty<LogicalValue>();
    public LogicalNamedExpressionInvocation(string Name, IReadOnlyList<LogicalValue> Arguments)
        => (this.Name, this.Arguments) = (Name, PlanningCollections.Freeze(Arguments));
    public string Name { get; init; }
    public IReadOnlyList<LogicalValue> Arguments
    {
        get => arguments;
        init => arguments = PlanningCollections.Freeze(value);
    }
    internal override bool IsKnownVariant => true;
}

/// <summary>
/// A sequence whose output flows from one item to the next.
/// </summary>
public sealed record LogicalPipeline : LogicalValue
{
    private IReadOnlyList<LogicalValue> items = PlanningCollections.Empty<LogicalValue>();
    public LogicalPipeline(IReadOnlyList<LogicalValue> Items)
        => this.Items = PlanningCollections.Freeze(Items);
    public IReadOnlyList<LogicalValue> Items
    {
        get => items;
        init => items = PlanningCollections.Freeze(value);
    }
    internal override bool IsKnownVariant => true;
    internal bool IsScalarReference { get; init; }
}

/// <summary>
/// A canonical operator invocation.
/// </summary>
public sealed record LogicalCall : LogicalValue
{
    private IReadOnlyList<LogicalArgument> arguments = PlanningCollections.Empty<LogicalArgument>();
    public LogicalCall(PlannerFunctionDescriptor Function, IReadOnlyList<LogicalArgument> Arguments, int ContextDepth = 0)
        => (this.Function, this.Arguments, this.ContextDepth) = (Function, PlanningCollections.Freeze(Arguments), ContextDepth);
    public PlannerFunctionDescriptor Function { get; init; }
    public IReadOnlyList<LogicalArgument> Arguments
    {
        get => arguments;
        init => arguments = PlanningCollections.Freeze(value);
    }
    public int ContextDepth { get; init; }
    internal override bool IsKnownVariant => true;
    internal SourceSpan? SourceSpan { get; init; }
    internal bool IsReferenceShorthand { get; init; }
    internal bool IsReferenceContinuation { get; init; }
    internal SourceNotation SourceNotation { get; init; }
}

/// <summary>
/// A scalar value identified by its Expressif semantic type.
/// </summary>
public sealed record LogicalLiteral(string Type, object? Value) : LogicalValue
{
    internal override bool IsKnownVariant => true;
}

internal sealed record QuotedLiteralRepresentation(string Value);

/// <summary>
/// An argument associated with its canonical catalog parameter.
/// </summary>
public sealed record LogicalArgument(
    PlannerParameterDescriptor Parameter,
    LogicalValue? Value,
    bool IsSpread,
    bool IsExplicit,
    PlannerOmissionDescriptor? Omission = null);

/// <summary>
/// Describes how an omitted argument obtains its value.
/// </summary>
public sealed record PlannerOmissionDescriptor(
    PlannerOmissionMode Mode,
    JsonElement Value = default,
    string? Source = null);

/// <summary>
/// Identifies the source of an omitted argument value.
/// </summary>
public enum PlannerOmissionMode
{
    Constant,
    EmptyVariadic,
    Absent,
    EnvironmentDerived,
}

/// <summary>
/// The planner-relevant part of a catalog operator.
/// </summary>
public sealed record PlannerFunctionDescriptor(
    string Name,
    string Input,
    string Output,
    PlannerTraversalDescriptor? Traversal = null,
    PlannerSemanticsDescriptor? Semantics = null,
    string Kind = "function",
    PlannerSchemaDescriptor? Schema = null,
    string Namespace = "global")
{
    public OperatorIdentity Identity => new(Namespace, Name);
    public string CanonicalName => Identity.CanonicalName;
}

/// <summary>
/// The schema relationship declared by a planned operator.
/// </summary>
public sealed record PlannerSchemaDescriptor
{
    private IReadOnlyDictionary<string, PlannerParameterSchemaDescriptor>? parameters;
    private IReadOnlyList<string>? nullableWhen;
    public PlannerSchemaDescriptor(
        string? Input = null,
        string? Output = null,
        IReadOnlyDictionary<string, PlannerParameterSchemaDescriptor>? Parameters = null,
        string? Intrinsic = null,
        string? Nullability = null,
        string? Classification = null,
        string? DynamicReason = null,
        IReadOnlyList<string>? NullableWhen = null)
        => (this.Input, this.Output, this.Parameters, this.Intrinsic, this.Nullability, this.Classification,
            this.DynamicReason, this.NullableWhen) =
            (Input, Output, PlanningCollections.FreezeNullable(Parameters), Intrinsic, Nullability, Classification,
                DynamicReason, PlanningCollections.FreezeNullable(NullableWhen));

    public string? Input { get; init; }
    public string? Output { get; init; }
    public IReadOnlyDictionary<string, PlannerParameterSchemaDescriptor>? Parameters
    {
        get => parameters;
        init => parameters = PlanningCollections.FreezeNullable(value);
    }
    public string? Intrinsic { get; init; }
    public string? Nullability { get; init; }
    public string? Classification { get; init; }
    public string? DynamicReason { get; init; }
    public IReadOnlyList<string>? NullableWhen
    {
        get => nullableWhen;
        init => nullableWhen = PlanningCollections.FreezeNullable(value);
    }
}

/// <summary>
/// The input and output schema relationship of an operator argument.
/// </summary>
public sealed record PlannerParameterSchemaDescriptor(
    string? Input = null,
    string? Output = null,
    string? Combine = null);

/// <summary>
/// The machine-readable traversal contract of a planned operator.
/// </summary>
public sealed record PlannerTraversalDescriptor(string Source, string Selection);

/// <summary>
/// The machine-readable structural effect of a planned operator.
/// </summary>
public sealed record PlannerSemanticsDescriptor(string Cardinality, string Dependency, string Ordering);

/// <summary>
/// The planner-relevant part of a catalog parameter.
/// </summary>
public sealed record PlannerParameterDescriptor(
    string Name,
    string Type,
    bool Optional,
    bool Variadic,
    int MinimumCardinality,
    PlannerEvaluationDescriptor? Evaluation = null);

/// <summary>
/// The machine-readable evaluation contract of a planned argument.
/// </summary>
public sealed record PlannerEvaluationDescriptor(
    string Frequency,
    string? Source = null,
    string? Context = null);

internal static class PlanningCollections
{
    public static IReadOnlyList<T> Empty<T>() => StructuralReadOnlyList<T>.Create([]);
    public static IReadOnlyList<T> Freeze<T>(IEnumerable<T> values)
        => StructuralReadOnlyList<T>.Create(values);
    public static IReadOnlyList<T>? FreezeNullable<T>(IEnumerable<T>? values)
        => values is null ? null : Freeze(values);
    public static IReadOnlyDictionary<TKey, TValue>? FreezeNullable<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>>? values)
        where TKey : notnull
        => values is null ? null : StructuralReadOnlyDictionary<TKey, TValue>.Create(values);
    public static IReadOnlyDictionary<TKey, TValue> Freeze<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> values)
        where TKey : notnull
        => StructuralReadOnlyDictionary<TKey, TValue>.Create(values);
}

#pragma warning restore SA1313
