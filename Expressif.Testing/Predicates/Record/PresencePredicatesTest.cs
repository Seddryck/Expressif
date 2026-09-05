using Expressif.Predicates.Record;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Record;

public class PresencePredicatesTest
{
    [Conformance]
    public void IsPresent_Valid_Record(object? value, string name, bool expected)
        => Assert.That(new IsPresent(() => name).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void IsAbsent_Valid_Record(object? value, string name, bool expected)
        => Assert.That(new IsAbsent(() => name).Evaluate(value), Is.EqualTo(expected));

    [TestCase("{age := #null}", true, false)]
    [TestCase("{name := \"Alice\"}", false, true)]
    public void PresencePredicates_AreLogicalOpposites(string source, bool present, bool absent)
    {
        var value = Expression.CreateClosed(source).Evaluate(null);

        Assert.Multiple(() =>
        {
            Assert.That(Expression.Create("is-present(age)").Evaluate(value), Is.EqualTo(present));
            Assert.That(Expression.Create("is-absent(age)").Evaluate(value), Is.EqualTo(absent));
        });
    }
}
