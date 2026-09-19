using Expressif.Functions.Array;
using Expressif.Functions.Special;
using System.Text;

namespace Expressif.Functions.Text;

/// <summary>
/// Constructs text by evaluating zero or more positional expressions from left to right against the same input,
/// converting each result to text, and concatenating the converted values in order. Spread arguments expand array
/// values in place. Returns empty text when no expressions are supplied.
/// </summary>
[Function(prefix: "")]
[Scope("text/concatenation")]
public sealed class Text : IFunction<object?, string>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Values { get; }

    /// <summary>Creates an empty text constructor.</summary>
    public Text()
        : this(() => []) { }

    /// <param name="values">Zero or more expressions whose results are converted to text and concatenated in declaration order. Spread arguments expand array values in place.</param>
    public Text(Func<ValueArgumentEvaluator[]> values)
        => Values = values;

    public string Evaluate(object? value)
    {
        var result = new StringBuilder();
        var coercion = new CoerceText();
        foreach (var item in ValueArguments.Evaluate(Values.Invoke(), value))
            result.Append(coercion.Evaluate(item));

        return result.ToString();
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
