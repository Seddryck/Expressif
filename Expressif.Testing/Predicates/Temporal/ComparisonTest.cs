using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class ComparisonTest
{
    [Conformance]
    public void SameInstant_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new SameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void After_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new After(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void AfterOrSameInstant_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new AfterOrSameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Before_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new Before(() => reference).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void BeforeOrSameInstant_Valid_DateTime(object? value, DateTime reference, bool expected)
        => Assert.That(new BeforeOrSameInstant(() => reference).Evaluate(value), Is.EqualTo(expected));
}
