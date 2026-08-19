using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

public abstract class BaseArraySetFunction : BaseArrayFunction
{
    protected static object?[] DistinctByOrder(IEnumerable source)
    {
        var values = new List<object?>();
        var visited = new HashSet<object?>();
        foreach (var item in source)
        {
            if (visited.Add(item))
                values.Add(item);
        }

        return values.ToArray();
    }

    protected static object?[] DifferenceByOrder(IEnumerable source, IEnumerable valuesToExclude)
    {
        var excluded = new HashSet<object?>();
        foreach (var item in valuesToExclude)
            excluded.Add(item);

        var values = new List<object?>();
        var visited = new HashSet<object?>();
        foreach (var item in source)
        {
            if (!excluded.Contains(item) && visited.Add(item))
                values.Add(item);
        }

        return values.ToArray();
    }

    protected static HashSet<object?> ToSet(IEnumerable source)
    {
        var set = new HashSet<object?>();
        foreach (var item in source)
            set.Add(item);

        return set;
    }
}

/// <summary>
/// Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["distinct"])]
public class Distinct : BaseArrayFunction
{
    public Distinct() { }

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var values = new List<object?>();
        var visited = new HashSet<object?>();

        foreach (var item in enumerable)
        {
            if (visited.Add(item))
                values.Add(item);
        }

        return values.ToArray();
    }
}

/// <summary>
/// Returns the distinct values from the pipeline input that do not appear in the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["difference"])]
public class Difference : BaseArraySetFunction
{
    public Func<object?[]> Array { get; }

    /// <param name="array">Specifies the array containing values to exclude from the pipeline input.</param>
    public Difference(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var array = Array.Invoke();
        if (array is null)
            return null;

        return DifferenceByOrder(enumerable, array);
    }
}

/// <summary>
/// Returns the distinct values that appear in exactly one of the two arrays, listing pipeline-input exclusives first and parameter-array exclusives second while preserving order within each source. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["symmetric-difference"])]
public class SymmetricDifference : BaseArraySetFunction
{
    public Func<object?[]> Array { get; }

    /// <param name="array">Specifies the second array to compare against the pipeline input.</param>
    public SymmetricDifference(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var array = Array.Invoke();
        if (array is null)
            return null;

        var leftExclusive = DifferenceByOrder(enumerable, array);
        var rightExclusive = DifferenceByOrder(array, enumerable);

        var values = new object?[leftExclusive.Length + rightExclusive.Length];
        leftExclusive.CopyTo(values, 0);
        rightExclusive.CopyTo(values, leftExclusive.Length);

        return values;
    }
}

/// <summary>
/// Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["complement"])]
public class Complement : BaseArraySetFunction
{
    public Func<object?[]> Array { get; }

    /// <param name="array">Specifies the reference array from which values present in the pipeline input are excluded.</param>
    public Complement(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var array = Array.Invoke();
        if (array is null)
            return null;

        return DifferenceByOrder(array, enumerable);
    }
}

/// <summary>
/// Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["intersection"])]
public class Intersection : BaseArraySetFunction
{
    public Func<object?[]> Array { get; }

    /// <param name="array">Specifies the array to compare with the pipeline input.</param>
    public Intersection(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        // TODO REVIEW Scaffold
        var array = Array.Invoke();
        if (array is null)
            return null;

        var included = ToSet(array);
        var values = new List<object?>();
        var visited = new HashSet<object?>();
        foreach (var item in enumerable)
        {
            if (included.Contains(item) && visited.Add(item))
                values.Add(item);
        }

        return values.ToArray();
    }
}

/// <summary>
/// Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["union"])]
public class Union : BaseArraySetFunction
{
    public Func<object?[]> Array { get; }

    /// <param name="array">Specifies the second array whose values are combined with the pipeline input.</param>
    public Union(Func<object?[]> array)
        => Array = array;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        // TODO REVIEW Scaffold
        var array = Array.Invoke();
        if (array is null)
            return null;

        var values = new List<object?>();
        var visited = new HashSet<object?>();
        foreach (var item in enumerable)
        {
            if (visited.Add(item))
                values.Add(item);
        }

        foreach (var item in array)
        {
            if (visited.Add(item))
                values.Add(item);
        }

        return values.ToArray();
    }
}
