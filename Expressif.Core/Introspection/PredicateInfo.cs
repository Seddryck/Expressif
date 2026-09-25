namespace Expressif.Introspection;

/// <summary>Describes a predicate available to Expressif.</summary>
public sealed class PredicateInfo
{
    internal PredicateInfo(PredicateInfoDefinition definition)
    {
        Name = definition.Name;
        IsPublic = definition.IsPublic;
        Aliases = Array.AsReadOnly(definition.Aliases.ToArray());
        Scope = definition.Scope;
        ImplementationType = definition.ImplementationType;
        Summary = definition.Summary;
        Parameters = Array.AsReadOnly(definition.Parameters.ToArray());
        Signatures = Array.AsReadOnly(definition.Signatures.ToArray());
    }

    public string Name { get; }
    public bool IsPublic { get; }
    public IReadOnlyList<string> Aliases { get; }
    public string Scope { get; }
    public Type ImplementationType { get; }
    public string Summary { get; }
    public IReadOnlyList<ParameterInfo> Parameters { get; }
    public IReadOnlyList<TupleBindingInfo> Signatures { get; }
}

internal sealed class PredicateInfoDefinition
{
    public required string Name { get; init; }
    public required bool IsPublic { get; init; }
    public required IEnumerable<string> Aliases { get; init; }
    public required string Scope { get; init; }
    public required Type ImplementationType { get; init; }
    public required string Summary { get; init; }
    public required IEnumerable<ParameterInfo> Parameters { get; init; }
    public required IEnumerable<TupleBindingInfo> Signatures { get; init; }
}
