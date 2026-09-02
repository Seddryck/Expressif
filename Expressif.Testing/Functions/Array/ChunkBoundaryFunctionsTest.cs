using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

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
        Assert.That(actual is null ? null : Expressif.Values.ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Expression_Position_InstantiatesAndEvaluates()
        => Assert.That(Expressif.Values.ValueFormatter.Format(Expression.Create("chunk-on(2)").Evaluate(new[] { 10, 20, 30 })),
            Is.EqualTo("T({10, 20}, {30})"));

    [Test]
    public void Expression_PositionBeyondEnd_UsesEndBoundary()
        => Assert.That(Expressif.Values.ValueFormatter.Format(Expression.Create("chunk-on(4)").Evaluate(new[] { 10, 20 })),
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
        Assert.That(actual is null ? null : Expressif.Values.ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Expression_Position_InstantiatesAndEvaluates()
        => Assert.That(Expressif.Values.ValueFormatter.Format(Expression.Create("chunk-around(1)").Evaluate(new[] { 10, 20, 30 })),
            Is.EqualTo("T({10}, 20, {30})"));
}
