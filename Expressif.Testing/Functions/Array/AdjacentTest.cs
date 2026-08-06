using System.Collections;

namespace Expressif.Testing.Functions.Array;

public class AdjacentTest
{
    [Test]
    public void Expression_ShorthandSubtract_ReturnsDifferences()
        => Assert.That(new Expression("adjacent(subtract)").Evaluate(new[] { 100, 105, 120 }), Is.EqualTo(new decimal?[] { 5, 15 }));

    [Test]
    public void Expression_OpenComposition_KeepsTupleProjectionsLexicallyBound()
        => Assert.That(new Expression("adjacent($1 | subtract($0) | multiply($1))").Evaluate(new[] { 100, 105, 120 }),
            Is.EqualTo(new decimal?[] { 525, 1800 }));

    [Test]
    public void Expression_PredicateShorthand_IsSupported()
        => Assert.That(new Expression("adjacent(greater-than)").Evaluate(new[] { 100, 105, 90 }), Is.EqualTo(new[] { true, false }));

    [Test]
    public void Expression_Boundaries_ReturnEmptySequence()
    {
        Assert.Multiple(() =>
        {
            Assert.That(((IEnumerable)new Expression("adjacent(subtract)").Evaluate(System.Array.Empty<int>())!).Cast<object>(), Is.Empty);
            Assert.That(((IEnumerable)new Expression("adjacent(subtract)").Evaluate(new[] { 1 })!).Cast<object>(), Is.Empty);
        });
    }
}
