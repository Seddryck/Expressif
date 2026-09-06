using System.Collections;
using System.Numerics;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns the original source element whose expression result is smallest. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/selection")]
public sealed class MinBy : BaseArrayFunction<object>
{
    private Func<IFunction> Expression { get; }

    /// <param name="expression">Expression evaluated once for each source element to obtain its comparison criterion.</param>
    public MinBy(Func<IFunction> expression) => Expression = expression;

    protected override object? EvaluateArray(IEnumerable enumerable)
        => ExpressionSelection.Select(enumerable, Expression.Invoke(), (candidate, best) => ExpressionSelection.Compare(candidate, best) < 0);
}

/// <summary>
/// Returns the original source element whose expression result is greatest. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/selection")]
public sealed class MaxBy : BaseArrayFunction<object>
{
    private Func<IFunction> Expression { get; }

    /// <param name="expression">Expression evaluated once for each source element to obtain its comparison criterion.</param>
    public MaxBy(Func<IFunction> expression) => Expression = expression;

    protected override object? EvaluateArray(IEnumerable enumerable)
        => ExpressionSelection.Select(enumerable, Expression.Invoke(), (candidate, best) => ExpressionSelection.Compare(candidate, best) > 0);
}

/// <summary>
/// Returns the original source element whose expression result is nearest to the numeric target. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/selection")]
public sealed class ClosestBy : BaseArrayFunction<object>
{
    private Func<IFunction> Expression { get; }
    private Func<decimal> Target { get; }

    /// <param name="expression">Expression evaluated once for each source element to obtain its comparison criterion.</param>
    /// <param name="target">Numeric value against which criterion distances are compared.</param>
    public ClosestBy(Func<IFunction> expression, Func<decimal> target)
        => (Expression, Target) = (expression, target);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var target = ExpressionSelection.Units(Target.Invoke());
        return ExpressionSelection.Select(enumerable, Expression.Invoke(),
            (candidate, best) => (BigInteger)candidate < (BigInteger)best,
            criterion => BigInteger.Abs(ExpressionSelection.Units(ExpressionSelection.Numeric(criterion)) - target));
    }
}

internal static class ExpressionSelection
{
    public static object? Select(IEnumerable source, IFunction expression, Func<object, object, bool> better, Func<object, object>? project = null)
    {
        object? selected = null;
        object? best = null;
        foreach (var item in source)
        {
            using var scope = EvaluationRuntime.Derive(item);
            var criterion = expression.Evaluate(item);
            if (criterion is null)
                continue;

            criterion = project is null ? Validate(criterion) : project(criterion);
            if (best is null || better(criterion, best))
            {
                selected = item;
                best = criterion;
            }
        }

        return selected;
    }

    public static int Compare(object candidate, object best)
    {
        if (candidate is string text && best is string other)
            return StringComparer.Ordinal.Compare(text, other);
        if (candidate.GetType() != best.GetType())
            throw new ArgumentException("Selection criteria must have compatible comparable types.");
        return ((IComparable)candidate).CompareTo(best);
    }

    public static decimal Numeric(object value)
        => IsNumeric(value) ? Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture)
            : throw new ArgumentException("Closest-by requires numeric criteria.");

    // Decimal coefficients at a common scale keep distances exact without decimal overflow.
    public static BigInteger Units(decimal value)
    {
        var bits = decimal.GetBits(value);
        var coefficient = (BigInteger)(uint)bits[0] + ((BigInteger)(uint)bits[1] << 32) + ((BigInteger)(uint)bits[2] << 64);
        var scale = (bits[3] >> 16) & 0xff;
        return (bits[3] < 0 ? -coefficient : coefficient) * BigInteger.Pow(10, 28 - scale);
    }

    private static object Validate(object value)
    {
        if (IsNumeric(value))
            return Numeric(value);
        return value is IComparable ? value : throw new ArgumentException("Selection criteria must be comparable scalar values.");
    }

    private static bool IsNumeric(object value)
        => value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
}
