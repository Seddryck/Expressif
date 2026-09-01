using System.Collections;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Array;

public class AdjacentTest
{
    [Conformance]
    public void Adjacent_Valid_Operation(object? value, string operation, decimal?[]? expected)
        => Assert.That(Expression.Create($"adjacent({operation})").Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Expression_ShorthandSubtract_ReturnsDifferences()
        => Assert.That(Expression.Create("adjacent(subtract)").Evaluate(new[] { 100, 105, 120 }), Is.EqualTo(new decimal?[] { 5, 15 }));

    [Test]
    public void Expression_OpenComposition_KeepsTupleProjectionsLexicallyBound()
        => Assert.That(Expression.Create("adjacent($1 | subtract($0) | multiply($1))").Evaluate(new[] { 100, 105, 120 }),
            Is.EqualTo(new decimal?[] { 525, 1800 }));

    [Test]
    public void Expression_PredicateShorthand_IsSupported()
        => Assert.That(Expression.Create("adjacent(greater-than)").Evaluate(new[] { 100, 105, 90 }), Is.EqualTo(new[] { true, false }));

    [Test]
    public void Expression_Boundaries_ReturnEmptySequence()
    {
        Assert.Multiple(() =>
        {
            Assert.That(((IEnumerable)Expression.Create("adjacent(subtract)").Evaluate(System.Array.Empty<int>())!).Cast<object>(), Is.Empty);
            Assert.That(((IEnumerable)Expression.Create("adjacent(subtract)").Evaluate(new[] { 1 })!).Cast<object>(), Is.Empty);
        });
    }
}
