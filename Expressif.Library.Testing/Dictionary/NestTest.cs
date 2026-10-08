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
        foreach (var key in new[] { new Expressif.Values.TupleValue(), new Expressif.Values.TupleValue(1) })
        {
            var input = new Expressif.Values.DictionaryValue([new Expressif.Values.PairValue(key, 2)]);
            Assert.That(() => nest.Evaluate(input),
                Throws.ArgumentException.With.Message.StartsWith("Every nest key must be a tuple"));
        }
    }

    [Test]
    public void StructuralPrefixKeys_CollapseIntoOneBranch()
    {
        var input = new Expressif.Values.DictionaryValue([
            new Expressif.Values.PairValue(new Expressif.Values.TupleValue(new[] { 1, 2 }, "first"), 10),
            new Expressif.Values.PairValue(new Expressif.Values.TupleValue(new[] { 1, 2 }, "second"), 20),
        ]);

        var result = new Expressif.Library.Dictionary.Nest().Evaluate(input);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Value, Is.TypeOf<Expressif.Values.DictionaryValue>());
        Assert.That(((DictionaryValue)result[0].Value!).Count, Is.EqualTo(2));
    }

    [Test]
    public void PairKey_UsesItsTwoTuplePositions()
    {
        var input = new Expressif.Values.DictionaryValue([
            new Expressif.Values.PairValue(new Expressif.Values.PairValue("BE", 2025), 100),
        ]);

        var result = new Expressif.Library.Dictionary.Nest().Evaluate(input);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo("!{(\"BE\" => !{(2025 => 100)})}"));
    }
}
