using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Expressif;

/// <summary>
/// Provides immutable runtime values that can be shared by many evaluations.
/// </summary>
public sealed class EvaluationContext
{
    private readonly IReadOnlyDictionary<string, object?> variables;

    public static EvaluationContext Empty { get; } = new();

    public EvaluationContext()
        : this(new Dictionary<string, object?>()) { }

    public EvaluationContext(IDictionary<string, object?> variables)
        => this.variables = new ReadOnlyDictionary<string, object?>(
            new Dictionary<string, object?>(variables, System.StringComparer.OrdinalIgnoreCase));

    public IReadOnlyDictionary<string, object?> Variables => variables;

    internal bool TryGetVariable(string name, out object? value)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (!variables.TryGetValue(name, out value))
            return false;

        value = value is Delegate provider
            && provider.GetType().IsGenericType
            && provider.GetType().GetGenericTypeDefinition() == typeof(Func<>)
                ? provider.DynamicInvoke()
                : value;
        return true;
    }
}
