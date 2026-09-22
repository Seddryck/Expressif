using Expressif.Functions.Accumulation;

namespace Expressif.Introspection;

/// <summary>Describes a function or accumulator available to Expressif.</summary>
public sealed class FunctionInfo
{
    internal FunctionInfo(
        string name,
        bool isPublic,
        IEnumerable<string> aliases,
        string scope,
        string input,
        string output,
        bool converted,
        string reason,
        Type implementationType,
        string summary,
        IEnumerable<ParameterInfo> parameters,
        bool deprecated,
        string? replacement,
        string? sunset,
        bool replacementIsEquivalent,
        string? migrationNotes,
        IEnumerable<TupleBindingInfo> signatures,
        IEnumerable<FunctionAliasLifecycleInfo>? deprecatedAliases = null)
    {
        Name = name;
        IsPublic = isPublic;
        Aliases = Array.AsReadOnly(aliases.ToArray());
        Scope = scope;
        Input = input;
        Output = output;
        Converted = converted;
        Reason = reason;
        ImplementationType = implementationType;
        Summary = summary;
        Parameters = Array.AsReadOnly(parameters.ToArray());
        Deprecated = deprecated;
        Replacement = replacement;
        Sunset = sunset;
        ReplacementIsEquivalent = replacementIsEquivalent;
        MigrationNotes = migrationNotes;
        Signatures = Array.AsReadOnly(signatures.ToArray());
        DeprecatedAliases = Array.AsReadOnly((deprecatedAliases ?? []).ToArray());
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
