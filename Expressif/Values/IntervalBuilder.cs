using Expressif.Bindings;
using Expressif.Predicates.Text;
using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values;

public class IntervalBuilder
{
    public virtual IInterval Create(char lowerBoundChar, string lowerBound, string upperBound, char upperBoundChar)
    {
        var lowerBoundType = lowerBoundChar == ']' ? IntervalType.Open : IntervalType.Closed;
        var upperBoundType = upperBoundChar == '[' ? IntervalType.Open : IntervalType.Closed;

        if (new MatchesNumeric().Evaluate(lowerBound) && new MatchesNumeric().Evaluate(upperBound))
        {
            var caster = new NumericCaster();
            return new Interval<decimal>(caster.Cast(lowerBound), caster.Cast(upperBound), lowerBoundType, upperBoundType);
        }
        else if(
            (new MatchesDateTime().Evaluate(lowerBound) && new MatchesDateTime().Evaluate(upperBound))
            || (new MatchesDate().Evaluate(lowerBound) && new MatchesDate().Evaluate(upperBound))
        )
        {
            var caster = new DateTimeCaster();
            return new Interval<DateTime>(caster.Cast(lowerBound), caster.Cast(upperBound), lowerBoundType, upperBoundType);
        }
        throw new InvalidOperationException();
    }

    public virtual IInterval Create(string value)
        => new ExpressifBinder().BindParameter(value) is IntervalParameter interval
            ? Create(interval.Value)
            : throw new BindingException($"Source '{value}' is not an interval.");

    public virtual IInterval Create(IntervalBinding interval)
    {
        var lowerBoundType = interval.IsLowerInclusive ? IntervalType.Closed : IntervalType.Open;
        var upperBoundType = interval.IsUpperInclusive ? IntervalType.Closed : IntervalType.Open;
        var finiteValue = interval.LowerBound.Value ?? interval.UpperBound.Value;

        return finiteValue switch
        {
            DateOnly => new Interval<DateTime>(
                ResolveDateTime(interval.LowerBound),
                ResolveDateTime(interval.UpperBound),
                lowerBoundType,
                upperBoundType),
            DateTime => new Interval<DateTime>(
                ResolveDateTime(interval.LowerBound),
                ResolveDateTime(interval.UpperBound),
                lowerBoundType,
                upperBoundType),
            TimeOnly => new Interval<TimeOnly>(
                ResolveTime(interval.LowerBound),
                ResolveTime(interval.UpperBound),
                lowerBoundType,
                upperBoundType),
            decimal or null => new Interval<decimal>(
                ResolveNumeric(interval.LowerBound),
                ResolveNumeric(interval.UpperBound),
                lowerBoundType,
                upperBoundType),
            _ => throw new InvalidOperationException($"Unsupported interval bound type '{finiteValue.GetType().Name}'."),
        };
    }

    private static decimal ResolveNumeric(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => decimal.MinValue,
        IntervalBoundBindingKind.PositiveInfinity => decimal.MaxValue,
        IntervalBoundBindingKind.Finite when bound.Value is decimal value => value,
        _ => throw new InvalidOperationException("Interval bounds must have compatible numeric types."),
    };

    private static DateTime ResolveDateTime(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => DateTime.MinValue,
        IntervalBoundBindingKind.PositiveInfinity => DateTime.MaxValue,
        IntervalBoundBindingKind.Finite when bound.Value is DateTime value => value,
        IntervalBoundBindingKind.Finite when bound.Value is DateOnly value => value.ToDateTime(TimeOnly.MinValue),
        _ => throw new InvalidOperationException("Interval bounds must have compatible temporal types."),
    };

    private static TimeOnly ResolveTime(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => TimeOnly.MinValue,
        IntervalBoundBindingKind.PositiveInfinity => TimeOnly.MaxValue,
        IntervalBoundBindingKind.Finite when bound.Value is TimeOnly value => value,
        _ => throw new InvalidOperationException("Interval bounds must have compatible temporal types."),
    };
}
