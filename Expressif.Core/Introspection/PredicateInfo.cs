namespace Expressif.Introspection;

/// <summary>Describes a predicate available to Expressif.</summary>
public sealed class PredicateInfo
{
    internal PredicateInfo(
        string name,
        bool isPublic,
        IEnumerable<string> aliases,
        string scope,
        Type implementationType,
        string summary,
        IEnumerable<ParameterInfo> parameters,
        IEnumerable<TupleBindingInfo> signatures)
    {
        Name = name;
        IsPublic = isPublic;
        Aliases = Array.AsReadOnly(aliases.ToArray());
        Scope = scope;
        ImplementationType = implementationType;
        Summary = summary;
        Parameters = Array.AsReadOnly(parameters.ToArray());
        Signatures = Array.AsReadOnly(signatures.ToArray());
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
