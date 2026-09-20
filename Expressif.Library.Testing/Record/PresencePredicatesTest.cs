using Expressif.Library.Record;
using Expressif.Library.Operators;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Record;

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
        var value = TestExpression.CreateClosed(source).Evaluate(null);

        Assert.Multiple(() =>
        {
            Assert.That(TestExpression.Create("is-present(age)").Evaluate(value), Is.EqualTo(present));
            Assert.That(TestExpression.Create("is-absent(age)").Evaluate(value), Is.EqualTo(absent));
        });
    }
}
