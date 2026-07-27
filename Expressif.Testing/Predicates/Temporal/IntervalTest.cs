using Expressif.Predicates.Temporal;
using Expressif.Values;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class IntervalTest
{
    [Conformance]
    public void ContainedIn_Valid_DateTime(string? value, bool expected)
        => Assert.That(new ContainedIn(
            () => new Interval<DateTime>(
                new DateTime(2022, 11, 20)
                , new DateTime(2022, 11, 24)
                , IntervalType.Open
                , IntervalType.Closed)
            ).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ContainedIn_Valid_Date(string value, bool expected)
        => Assert.That(new ContainedIn(
            () => new Interval<DateTime>(
                new DateTime(2022, 11, 20)
                , new DateTime(2022, 11, 24)
                , IntervalType.Open
                , IntervalType.Closed)
            ).Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));
}
