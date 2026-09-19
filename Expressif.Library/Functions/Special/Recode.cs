using Expressif.Values;

namespace Expressif.Functions.Special;

/// <summary>Returns the dictionary value associated with the input key, or preserves the input when no key matches.</summary>
[Function(prefix: "", DynamicReason = "Output depends on the matched dictionary value, or preserves the input type when no key matches.")]
[Scope("special")]
public sealed class Recode : IFunction
{
    private Func<DictionaryValue> Mapping { get; }

    /// <param name="mapping">The dictionary supplying replacement values for unique keys.</param>
    public Recode(Func<DictionaryValue> mapping) => Mapping = mapping;

    public object? Evaluate(object? value)
    {
        var mapping = Mapping.Invoke();
        if (mapping is null)
            throw new ArgumentException("The recode mapping must be a dictionary.", "mapping");
        return mapping.TryGetValue(value, out var replacement) ? replacement : value;
    }
}
