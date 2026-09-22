using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using System.Collections;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Sequencing;

public class AdjacentTest
{
    [Conformance]
    public void Adjacent_ExplicitBinding(object? value, string expression, string expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(TestExpression.Create(expected).Evaluate(null)));

    [Conformance]
    public void Adjacent_Valid_Operation(object? value, string operation, decimal?[]? expected)
        => Assert.That(TestExpression.Create($"adjacent({operation})").Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Expression_ShorthandSubtract_ReturnsDifferences()
        => Assert.That(TestExpression.Create("adjacent(subtract)").Evaluate(new[] { 100, 105, 120 }), Is.EqualTo(new decimal?[] { 5, 15 }));

    [Test]
    public void Expression_OpenComposition_KeepsTupleProjectionsLexicallyBound()
        => Assert.That(TestExpression.Create("adjacent($1 | subtract($0) | multiply($1))").Evaluate(new[] { 100, 105, 120 }),
            Is.EqualTo(new decimal?[] { 525, 1800 }));

    [Test]
    public void Expression_PredicateShorthand_IsSupported()
        => Assert.That(TestExpression.Create("adjacent(greater-than)").Evaluate(new[] { 100, 105, 90 }), Is.EqualTo(new[] { true, false }));

    [Test]
    public void Expression_Boundaries_ReturnEmptySequence()
    {
        Assert.Multiple(() =>
        {
            Assert.That(((IEnumerable)TestExpression.Create("adjacent(subtract)").Evaluate(System.Array.Empty<int>())!).Cast<object>(), Is.Empty);
            Assert.That(((IEnumerable)TestExpression.Create("adjacent(subtract)").Evaluate(new[] { 1 })!).Cast<object>(), Is.Empty);
        });
    }
}
