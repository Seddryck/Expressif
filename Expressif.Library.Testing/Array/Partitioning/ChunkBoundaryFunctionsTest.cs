using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Array.Partitioning;

[TestFixture]
public class ChunkOnTest
{
    [Conformance]
    public void ChunkOn_Valid_Position(object? input, int position, string? expected)
    {
        var value = input is "(null)"
            ? null
            : input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = new ChunkOn(() => position).Evaluate(value);
        Assert.That(actual is null ? null : Expressif.Values.Formatting.ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Expression_Position_InstantiatesAndEvaluates()
        => Assert.That(Expressif.Values.Formatting.ValueFormatter.Format(TestExpression.Create("chunk-on(2)").Evaluate(new[] { 10, 20, 30 })),
            Is.EqualTo("T({10, 20}, {30})"));

    [Test]
    public void Expression_PositionBeyondEnd_UsesEndBoundary()
        => Assert.That(Expressif.Values.Formatting.ValueFormatter.Format(TestExpression.Create("chunk-on(4)").Evaluate(new[] { 10, 20 })),
            Is.EqualTo("T({10, 20}, {})"));
}

[TestFixture]
public class ChunkAroundTest
{
    [Conformance]
    public void ChunkAround_Valid_Position(object? input, int position, string? expected)
    {
        var value = input is "(null)"
            ? null
            : input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = new ChunkAround(() => position).Evaluate(value);
        Assert.That(actual is null ? null : Expressif.Values.Formatting.ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Expression_Position_InstantiatesAndEvaluates()
        => Assert.That(Expressif.Values.Formatting.ValueFormatter.Format(TestExpression.Create("chunk-around(1)").Evaluate(new[] { 10, 20, 30 })),
            Is.EqualTo("T({10}, 20, {30})"));
}
