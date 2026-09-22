using Expressif.Functions.Accumulation;

namespace Expressif.Introspection;

/// <summary>Describes a function or accumulator available to Expressif.</summary>
public sealed class FunctionInfo
{
    internal FunctionInfo(FunctionInfoDefinition definition)
    {
        Name = definition.Name;
        IsPublic = definition.IsPublic;
        Aliases = Array.AsReadOnly(definition.Aliases.ToArray());
        Scope = definition.Scope;
        Input = definition.Input;
        Output = definition.Output;
        Converted = definition.Converted;
        Reason = definition.Reason;
        ImplementationType = definition.ImplementationType;
        Summary = definition.Summary;
        Parameters = Array.AsReadOnly(definition.Parameters.ToArray());
        Deprecated = definition.Deprecated;
        Replacement = definition.Replacement;
        Sunset = definition.Sunset;
        ReplacementIsEquivalent = definition.ReplacementIsEquivalent;
        MigrationNotes = definition.MigrationNotes;
        Signatures = Array.AsReadOnly(definition.Signatures.ToArray());
        DeprecatedAliases = Array.AsReadOnly(definition.DeprecatedAliases.ToArray());
    }

    public string Name { get; }
    public bool IsPublic { get; }
    public IReadOnlyList<string> Aliases { get; }
    public string Scope { get; }
    public string Input { get; }
    public string Output { get; }
    public bool Converted { get; }
    public string Reason { get; }
    public Type ImplementationType { get; }
    public string Summary { get; }
    public IReadOnlyList<ParameterInfo> Parameters { get; }
    public bool Deprecated { get; }
    public string? Replacement { get; }
    public string? Sunset { get; }
    public bool ReplacementIsEquivalent { get; }
    public string? MigrationNotes { get; }
    public IReadOnlyList<TupleBindingInfo> Signatures { get; }
    public IReadOnlyList<FunctionAliasLifecycleInfo> DeprecatedAliases { get; }
    public string Kind => typeof(IAccumulator).IsAssignableFrom(ImplementationType) ? "accumulator" : "function";
}

internal sealed class FunctionInfoDefinition
{
    public required string Name { get; init; }
    public required bool IsPublic { get; init; }
    public required IEnumerable<string> Aliases { get; init; }
    public required string Scope { get; init; }
    public required string Input { get; init; }
    public required string Output { get; init; }
    public required bool Converted { get; init; }
    public required string Reason { get; init; }
    public required Type ImplementationType { get; init; }
    public required string Summary { get; init; }
    public required IEnumerable<ParameterInfo> Parameters { get; init; }
    public required bool Deprecated { get; init; }
    public string? Replacement { get; init; }
    public string? Sunset { get; init; }
    public required bool ReplacementIsEquivalent { get; init; }
    public string? MigrationNotes { get; init; }
    public required IEnumerable<TupleBindingInfo> Signatures { get; init; }
    public IEnumerable<FunctionAliasLifecycleInfo> DeprecatedAliases { get; init; } = [];
}

/// <summary>Describes the deprecation lifecycle of a function alias.</summary>
public sealed class FunctionAliasLifecycleInfo
{
    internal FunctionAliasLifecycleInfo(
        string name,
        string replacement,
        string message,
        string? sunset = null,
        bool replacementIsEquivalent = true)
        => (Name, Replacement, Message, Sunset, ReplacementIsEquivalent) = (
            name,
            replacement,
            message,
            sunset,
            replacementIsEquivalent);

    public string Name { get; }
    public string Replacement { get; }
    public string Message { get; }
    public string? Sunset { get; }
    public bool ReplacementIsEquivalent { get; }
}

/// <summary>Describes one parameter accepted by a function or predicate.</summary>
public sealed class ParameterInfo
{
    internal ParameterInfo(
        string name,
        string type,
        bool optional,
        bool variadic,
        int minimumCardinality,
        string summary)
        => (Name, Type, Optional, Variadic, MinimumCardinality, Summary) = (
            name,
            type,
            optional,
            variadic,
            minimumCardinality,
            summary);

    public string Name { get; }
    public string Type { get; }
    public bool Optional { get; }
    public bool Variadic { get; }
    public int MinimumCardinality { get; }
    public string Summary { get; }
}
