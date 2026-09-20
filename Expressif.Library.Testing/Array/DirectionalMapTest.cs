using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array;

[TestFixture]
public class MapOverTest
{
    [Conformance]
    public void MapOver_ExplicitBinding(object? value, string expression, string expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(TestExpression.Create(expected).Evaluate(null)));

    [Test]
    public void Expression_ScalarValues_BroadcastsPipelineInput()
        => Assert.That(TestExpression.Create("map-over(subtract, {10, 11})").Evaluate(5), Is.EqualTo(new decimal?[] { -5, -6 }));

    [Test]
    public void Expression_TupleValues_SpreadsBareCallableArguments()
        => Assert.That(TestExpression.Create("map-over(subtract, {T(1, 2), T(3, 4)})").Evaluate(20), Is.EqualTo(new decimal?[] { 18, 8 }));

    [Test]
    public void Expression_TupleProjection_UsesOneBasedCurrentTuplePositions()
        => Assert.That(TestExpression.Create("map-over(subtract($2) | subtract($1), {T(1, 2), T(3, 4)})").Evaluate(20), Is.EqualTo(new decimal?[] { 17, 13 }));

    [Conformance]
    public void MapOver_Valid_ScalarValues(object? value, decimal?[] expected)
        => Assert.That(TestExpression.Create("map-over(subtract, {10, 11})").Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MapOver_Valid_EmptyValues(object? value, decimal?[] expected)
        => Assert.That(TestExpression.Create("map-over(subtract, {})").Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MapOver_Valid_TupleArguments(object? value, decimal?[] expected)
        => Assert.That(TestExpression.Create("map-over(subtract, {T(1, 2), T(3, 4)})").Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MapOver_Valid_TupleProjection(object? value, decimal?[] expected)
        => Assert.That(TestExpression.Create("map-over(subtract($2) | subtract($1), {T(1, 2), T(3, 4)})").Evaluate(value), Is.EqualTo(expected));
}

[TestFixture]
public class MapWithTest
{
    [Conformance]
    public void MapWith_ExplicitBinding(object? value, string expression, string expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(TestExpression.Create(expected).Evaluate(null)));

    [Test]
    public void Expression_ScalarValues_UsesOuterInputAsArgument()
        => Assert.That(TestExpression.Create("map-with(subtract, {10, 11})").Evaluate(5), Is.EqualTo(new decimal?[] { 5, 6 }));

    [Conformance]
    public void MapWith_Valid_ScalarValues(object? value, decimal?[] expected)
        => Assert.That(TestExpression.Create("map-with(subtract, {10, 11})").Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void MapWith_Valid_EmptyValues(object? value, decimal?[] expected)
        => Assert.That(TestExpression.Create("map-with(subtract, {})").Evaluate(value), Is.EqualTo(expected));
}
