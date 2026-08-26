using Expressif.Predicates.Numeric;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Predicates.Numeric;

[TestFixture]
public class IntervalTest
{
    [Conformance]
    public void IsWithinInterval_Valid_OpenClosed(object? value, bool expected)
        => Assert.That(new WithinInterval(() => new Interval<decimal>(1, 12, IntervalType.Open, IntervalType.Closed)).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinInterval_Valid_NegativeInfinite(object? value, bool expected)
        => Assert.That(new WithinInterval(() => new Interval<decimal>(decimal.MinValue, 12, IntervalType.Closed, IntervalType.Closed)).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinInterval_Valid_PositiveInfinite(object? value, bool expected)
        => Assert.That(new WithinInterval(() => new Interval<decimal>(12, decimal.MaxValue, IntervalType.Closed, IntervalType.Closed)).Evaluate(value), Is.EqualTo(expected));
}
