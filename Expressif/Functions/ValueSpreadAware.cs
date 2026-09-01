namespace Expressif.Functions;

/// <summary>Identifies a function whose positional arguments support value spread.</summary>
public interface IValueSpreadAware
{
}

public sealed record ValueArgumentEvaluator(
    Func<object?, object?> Evaluator,
    bool IsSpread = false);

public static class ValueArguments
{
    public static IEnumerable<object?> Evaluate(
        IEnumerable<ValueArgumentEvaluator> arguments,
        object? input)
    {
        foreach (var argument in arguments)
        {
            var value = argument.Evaluator.Invoke(input);
            if (!argument.IsSpread)
            {
                yield return value;
                continue;
            }

            foreach (var item in Array.SpreadValues.Enumerate(value))
                yield return item;
        }
    }
}
