using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class ComparisonTest
{
    [Conformance]
    public void IsSameInstant_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new SameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsAfter_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new After(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsAfterOrSameInstant_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new AfterOrSameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsBefore_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new Before(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsBeforeOrSameInstant_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new BeforeOrSameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));
}
