using System.Collections;

namespace Expressif.Functions.Array;

/// <summary>
/// Evaluates an array-producing expression for each input element and concatenates the resulting arrays in order.
/// Flattens one level, preserving nested arrays and null elements. Empty arrays contribute no elements.
/// Throws an argument error when an expression result is not an array, including null or text.
/// </summary>
[Function(prefix: "", aliases: [])]
public sealed class FlatMap : BaseArrayFunction
{
    private Func<IFunction> Expression { get; }

    /// <param name="expression">Expression producing the array of output elements for each source element.</param>
    public FlatMap(Func<IFunction> expression)
        => Expression = expression;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var expression = Expression.Invoke();
        var output = new List<object?>();
        foreach (var item in enumerable)
        {
            var result = EvaluationRuntime.EvaluateNested(expression, item);
            if (result is string || result is not IEnumerable elements)
                throw new ArgumentException("The flat-map expression must evaluate to an array.");

            foreach (var element in elements)
                output.Add(element);
        }

        return output.ToArray();
    }
}
