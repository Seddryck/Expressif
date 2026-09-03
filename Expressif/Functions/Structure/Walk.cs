using System.Collections;
using Expressif.Values;

namespace Expressif.Functions.Structure;

/// <summary>
/// Recursively traverses arrays, tuples, and records and evaluates an expression against each leaf value.
/// Container shape and record field names are preserved.
/// </summary>
[Function(prefix: "", aliases: ["walk"])]
[Scope("structure")]
public sealed class Walk : IFunction
{
    private readonly Func<IFunction> transformation;

    /// <param name="transformation">Expression evaluated against every leaf value.</param>
    public Walk(Func<IFunction> transformation) => this.transformation = transformation;

    public object? Evaluate(object? value) => Transform(value, transformation.Invoke());

    private static object? Transform(object? value, IFunction expression)
        => value switch
        {
            RecordValue record => TransformRecord(record, expression),
            TupleValue tuple => new TupleValue(tuple.Select(item => Transform(item, expression)).ToArray()),
            IEnumerable enumerable when value is not string => TransformArray(enumerable, expression),
            _ => EvaluateNested(expression, value),
        };

    private static object? EvaluateNested(IFunction expression, object? value)
    {
        using var scope = EvaluationRuntime.Derive(value);
        return expression.Evaluate(value);
    }

    private static RecordValue TransformRecord(RecordValue record, IFunction expression)
    {
        var result = new RecordValue();
        foreach (var field in record)
            result.Set(field.Key, Transform(field.Value, expression));
        return result;
    }

    private static object?[] TransformArray(IEnumerable enumerable, IFunction expression)
        => enumerable.Cast<object?>().Select(item => Transform(item, expression)).ToArray();
}
