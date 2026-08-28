using System;
using System.Collections;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

public abstract class BaseZipFunction : BaseArrayFunction
{
    protected Func<object?[]> Array { get; }

    protected BaseZipFunction(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var array = Array.Invoke();
        return array is null ? null : Enumerate(enumerable, array);
    }

    protected abstract IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable left, IEnumerable right);

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

    protected override IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable left, IEnumerable right)
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

    protected override IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable left, IEnumerable right)
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
/// Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Throws an invalid-operation error when the arrays have different lengths and returns `null` when either value cannot be evaluated as an array.
/// </summary>
[Function(prefix: "")]
public sealed class ZipStrict : BaseZipFunction
{
    /// <param name="array">Specifies the equally sized second array whose values form the second element of each tuple.</param>
    public ZipStrict(Func<object?[]> array)
        : base(array) { }

    protected override IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable left, IEnumerable right)
    {
        var leftEnumerator = GetEnumerator(left);
        var rightEnumerator = GetEnumerator(right);
        try
        {
            while (true)
            {
                var hasLeft = leftEnumerator.MoveNext();
                var hasRight = rightEnumerator.MoveNext();
                if (hasLeft != hasRight)
                    throw new InvalidOperationException("Cannot zip arrays of different lengths in strict mode.");
                if (!hasLeft)
                    yield break;

                yield return new Expressif.Values.Tuple(leftEnumerator.Current, rightEnumerator.Current);
            }
        }
        finally
        {
            Dispose(leftEnumerator);
            Dispose(rightEnumerator);
        }
    }
}
