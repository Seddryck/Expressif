using Expressif.Functions.Tuple;
using Expressif.Values;
using Expressif.Testing.Conformance;
using System.Globalization;

namespace Expressif.Testing.Functions.Tuple;

public class TupleFunctionsTest
{
    [Conformance]
    public void Arity_Valid(string value, int expected)
        => Assert.That(
            new Arity().Evaluate(ParseTuple(value)),
            Is.EqualTo(expected));

    private static TupleValue ParseTuple(string value)
    {
        var source = NormalizeTupleSyntax(value);
        return source == "T()" ? new Expressif.Values.Tuple() : (TupleValue)Expression.CreateClosed(source).Evaluate(null)!;
    }

    private static string NormalizeTupleSyntax(string value)
        => value.Trim('"').Replace('{', '(').Replace('}', ')');

    [Test]
    public void Evaluate_Accessors_Valid()
    {
        var tuple = new TupleValue(10, 20, 30);
        Assert.Multiple(() =>
        {
            Assert.That(new TupleFirst().Evaluate(tuple), Is.EqualTo(10));
            Assert.That(new TupleSecond().Evaluate(tuple), Is.EqualTo(20));
            Assert.That(new TupleAt(() => 2).Evaluate(tuple), Is.EqualTo(30));
            Assert.That(new TupleAt(() => 3).Evaluate(tuple), Is.Null);
            Assert.That(new TupleFirst().Evaluate(new object[] { 10, 20 }), Is.Null);
        });
    }

    [TestCase("T(10, 20, 30) | $^1", 30)]
    [TestCase("T(10, 20, 30) | $^2", 20)]
    [TestCase("T(10, 20, 30) | $^3", 10)]
    [TestCase("T(10, 20, 30) | $^0", null)]
    [TestCase("T(10, 20, 30) | $^4", null)]
    public void Evaluate_FromEndProjection_ReturnsExpected(string expression, object? expected)
        => Assert.That(Expression.CreateClosed(expression).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void Swap_DefaultAndExplicit_ExchangePositions()
    {
        var tuple = new TupleValue("a", "b", "c", "d");
        Assert.Multiple(() =>
        {
            Assert.That(new Swap().Evaluate(tuple), Is.EqualTo(new TupleValue("d", "b", "c", "a")));
            Assert.That(new Swap(() => 1, () => 2).Evaluate(tuple), Is.EqualTo(new TupleValue("a", "c", "b", "d")));
            Assert.That(new Swap(() => 1, () => 1).Evaluate(tuple), Is.EqualTo(tuple));
        });
    }

    [Test]
    public void Extend_AppendsPositionsWithoutMutation()
    {
        var source = new TupleValue(1, 2);
        var extension = new TupleValue(3, "foo");
        Assert.That(new Extend(_ => extension).Evaluate(source), Is.EqualTo(new TupleValue(1, 2, 3, "foo")));
        Assert.That(source, Is.EqualTo(new TupleValue(1, 2)));
    }
    [Conformance]
    public void Extend_Valid(string value, object? extension, string expected)
    {
        var parameter = extension switch
        {
            null => "#null",
            bool boolean => boolean ? "#true" : "#false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => extension is string text && text.StartsWith("T{", StringComparison.Ordinal)
                ? NormalizeTupleSyntax(text)
                : extension.ToString(),
        };

        if (parameter == "T()")
        {
            Assert.That(
                new Extend(_ => new Expressif.Values.Tuple()).Evaluate(ParseTuple(value)),
                Is.EqualTo(ParseTuple(expected)));
            return;
        }

        Assert.That(
            Expression.CreateClosed($"{NormalizeTupleSyntax(value)} | extend({parameter})").Evaluate(null),
            Is.EqualTo(ParseTuple(expected)));
    }

    [Test]
    public void Extend_AppendsScalarAsSinglePosition()
        => Assert.That(
            new Extend(_ => "foo").Evaluate(new TupleValue(1, 2)),
            Is.EqualTo(new TupleValue(1, 2, "foo")));

    [TestCase("T(1, 2) | extend(3)", "T(1, 2, 3)")]
    [TestCase("T(10, 20) | extend($1 | subtract($0))", "T(10, 20, 10)")]
    [TestCase("T(1, 2) | extend((1 | add(2)))", "T(1, 2, 3)")]
    [TestCase("T(10, 20) | extend(apply($1 | subtract($0)))", "T(10, 20, 10)")]
    public void Extend_EvaluatesSupportedParameterShapes(string source, string expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(ParseTuple(expected)));

    [Test]
    public void Pick_SelectsReordersAndRepeats()
        => Assert.That(
            Expression.Create("pick(1, 0, 1)").Evaluate(new TupleValue("John", "Smith")),
            Is.EqualTo(new TupleValue("Smith", "John", "Smith")));
}
