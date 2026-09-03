using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Dictionary;

public class DictionaryTest
{
    [Conformance]
    public void Dictionary_Valid_Constructor(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Dictionary_Valid_Literal(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void DuplicateStructuralKeys_ThrowsExplicitly()
        => Assert.That(
            () => Expression.Create("dictionary(({1, 2} => \"first\"), ({1, 2} => \"second\"))").Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("A dictionary cannot contain duplicate key"));

    [Test]
    public void OrdinaryTupleAndRecord_AreRejected()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => Expression.Create("dictionary(T(1, 2))").Evaluate(null), Throws.ArgumentException);
            Assert.That(() => Expression.Create("dictionary({key := 1, value := 2})").Evaluate(null), Throws.ArgumentException);
        });
    }
}
