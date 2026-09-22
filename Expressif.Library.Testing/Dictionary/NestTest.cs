using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Dictionary;

public class NestTest
{
    [Conformance]
    public void Nest_Valid_Dictionary(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("!{(1 => 2)} | nest")]
    [TestCase("!{(T(1, 2) => 3), (T(1, 2, 3) => 4)} | nest")]
    public void InvalidKeys_ThrowExplicitly(string expression)
        => Assert.That(() => TestExpression.Create(expression).Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("Every nest key must be a tuple"));

    [Test]
    public void ShortTupleKeys_ThrowExplicitly()
    {
        var nest = new Expressif.Library.Dictionary.Nest();
        foreach (var key in new[] { new Expressif.Values.Tuple(), new Expressif.Values.Tuple(1) })
        {
            var input = new Expressif.Values.Dictionary([new Expressif.Values.Pair(key, 2)]);
            Assert.That(() => nest.Evaluate(input),
                Throws.ArgumentException.With.Message.StartsWith("Every nest key must be a tuple"));
        }
    }

    [Test]
    public void StructuralPrefixKeys_CollapseIntoOneBranch()
    {
        var input = new Expressif.Values.Dictionary([
            new Expressif.Values.Pair(new Expressif.Values.Tuple(new[] { 1, 2 }, "first"), 10),
            new Expressif.Values.Pair(new Expressif.Values.Tuple(new[] { 1, 2 }, "second"), 20),
        ]);

        var result = new Expressif.Library.Dictionary.Nest().Evaluate(input);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Value, Is.TypeOf<Expressif.Values.Dictionary>());
        Assert.That(((DictionaryValue)result[0].Value!).Count, Is.EqualTo(2));
    }

    [Test]
    public void PairKey_UsesItsTwoTuplePositions()
    {
        var input = new Expressif.Values.Dictionary([
            new Expressif.Values.Pair(new Expressif.Values.Pair("BE", 2025), 100),
        ]);

        var result = new Expressif.Library.Dictionary.Nest().Evaluate(input);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo("!{(\"BE\" => !{(2025 => 100)})}"));
    }
}
