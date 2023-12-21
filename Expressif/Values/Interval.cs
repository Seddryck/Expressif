using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values;

public interface IInterval
{
    
}

public readonly struct Interval<T> : IInterval where T : struct, IComparable
{
    public T LowerBound { get; }
    public T UpperBound { get; }

    public IntervalType LowerBoundIntervalType { get; }
    public IntervalType UpperBoundIntervalType { get; }

    public Interval(
        T lowerbound,
        T upperbound,
        IntervalType lowerboundIntervalType = IntervalType.Closed,
        IntervalType upperboundIntervalType = IntervalType.Closed)
        : this()
    {
        var a = lowerbound;
        var b = upperbound;
        var comparison = a.CompareTo(b);

        if (comparison > 0)
        {
            a = upperbound;
            b = lowerbound;
        }

        LowerBound = a;
        UpperBound = b;
        LowerBoundIntervalType = lowerboundIntervalType;
        UpperBoundIntervalType = upperboundIntervalType;
    }

    /// <summary>
    /// Check if given point lies within the interval.
    /// </summary>
    /// <param name="point">Point to check.</param>
    /// <returns>True if point lies within the interval, otherwise false.</returns>
    public bool Contains(T point)
    {
        var lower = LowerBoundIntervalType == IntervalType.Open
            ? LowerBound.CompareTo(point) < 0
            : LowerBound.CompareTo(point) <= 0;
        var upper = UpperBoundIntervalType == IntervalType.Open
            ? UpperBound.CompareTo(point) > 0
            : UpperBound.CompareTo(point) >= 0;

        return lower && upper;
    }

    /// <summary>
    /// Convert to mathematical notation using open and closed parenthesis:
    /// [, and ].
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Format(
            "{0}{1}; {2}{3}",
            LowerBoundIntervalType == IntervalType.Open ? "]" : "[",
            LowerBound,
            UpperBound,
            UpperBoundIntervalType == IntervalType.Open ? "[" : "]"
        );
    }
}

public enum IntervalType
{
    Open,
    Closed
}
