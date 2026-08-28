using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Expressif.Functions.Array;

public abstract class BaseZipFunction : BaseArrayFunction
{
    protected Func<object?[]> Array { get; }

    protected BaseZipFunction(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var array = Array.Invoke();
        return array is null ? null : EvaluateZip(enumerable, array);
    }

    protected abstract object? EvaluateZip(IEnumerable left, IEnumerable right);

    protected static IEnumerator GetEnumerator(IEnumerable source)
        => source.GetEnumerator();

    protected static void Dispose(IEnumerator enumerator)
        => (enumerator as IDisposable)?.Dispose();
}

/// <summary>
/// Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array.
/// </summary>
[Function(prefix: "")]
public sealed class Zip : BaseZipFunction
{
    /// <param name="array">Specifies the second array whose values form the second element of each tuple.</param>
    public Zip(Func<object?[]> array)
        : base(array) { }

    protected override object EvaluateZip(IEnumerable left, IEnumerable right)
        => Enumerate(left, right);

    private static IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable left, IEnumerable right)
    {
        var leftEnumerator = GetEnumerator(left);
        var rightEnumerator = GetEnumerator(right);
        try
        {
            while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
                yield return new Expressif.Values.Tuple(leftEnumerator.Current, rightEnumerator.Current);
        }
        finally
        {
            Dispose(leftEnumerator);
            Dispose(rightEnumerator);
        }
    }
}

/// <summary>
/// Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array.
/// </summary>
[Function(prefix: "")]
public sealed class ZipPadded : BaseZipFunction
{
    /// <param name="array">Specifies the second array whose values form the second element of each tuple.</param>
    public ZipPadded(Func<object?[]> array)
        : base(array) { }

    protected override object EvaluateZip(IEnumerable left, IEnumerable right)
        => Enumerate(left, right);

    private static IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable left, IEnumerable right)
    {
        var leftEnumerator = GetEnumerator(left);
        var rightEnumerator = GetEnumerator(right);
        try
        {
            while (true)
            {
                var hasLeft = leftEnumerator.MoveNext();
                var hasRight = rightEnumerator.MoveNext();
                if (!hasLeft && !hasRight)
                    yield break;

                yield return new Expressif.Values.Tuple(
                    hasLeft ? leftEnumerator.Current : null,
                    hasRight ? rightEnumerator.Current : null);
            }
        }
        finally
        {
            Dispose(leftEnumerator);
            Dispose(rightEnumerator);
        }
    }
}

/// <summary>
/// Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array.
/// </summary>
[Function(prefix: "")]
public sealed class ZipStrict : BaseZipFunction
{
    /// <param name="array">Specifies the equally sized second array whose values form the second element of each tuple.</param>
    public ZipStrict(Func<object?[]> array)
        : base(array) { }

    protected override object? EvaluateZip(IEnumerable left, IEnumerable right)
    {
        var leftValues = left.Cast<object?>().ToArray();
        var rightValues = right.Cast<object?>().ToArray();
        return leftValues.Length != rightValues.Length
            ? null
            : leftValues.Zip(
                rightValues,
                (leftValue, rightValue) => new Expressif.Values.Tuple(leftValue, rightValue))
                .ToArray();
    }
}
