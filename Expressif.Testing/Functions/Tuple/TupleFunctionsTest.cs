using Expressif.Functions.Tuple;
using Expressif.Values;
using Expressif.Testing.Conformance;
using Expressif.Values.Special;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Expressif.Testing.Functions.Tuple;

public class TupleFunctionsTest
{
    [Conformance]
    public void Tuple_Valid_VariadicValues(object? input, string expression, string expected)
        => Assert.That(
            ValueFormatter.Format(Expression.Create(expression).Evaluate(input)),
            Is.EqualTo(expected));

    [Conformance]
    public void Tuple_Valid_VariableSpread(object? input, string expression, decimal[] values, string expected)
    {
        var context = new Context();
        context.Variables.Add<decimal[]>("values", values);

        Assert.That(
            ValueFormatter.Format(Expression.Create(expression, context).Evaluate(input)),
            Is.EqualTo(expected));
    }

    [TestCase("tuple(...#null)", "Spread argument cannot be null.")]
    [TestCase("tuple(...42)", "Spread argument must evaluate to an array.")]
    [TestCase("tuple(...\"abc\")", "Spread argument must evaluate to an array.")]
    public void Tuple_InvalidSpread_ThrowsSpecificError(string source, string message)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<SpreadArgumentException>().With.Message.EqualTo(message));

    [Conformance]
    public void Arity_Valid(string value, int expected)
        => Assert.That(
            new Arity().Evaluate(ParseTupleLike(value)),
            Is.EqualTo(expected));

    private static TupleValue ParseTuple(string value)
    {
        var source = Regex.Replace(
            NormalizeTupleSyntax(value),
            @"(?<=\(|,)\s*([A-Za-z][\w-]*)\s*(?=,|\))",
            "\"$1\"");
        return source == "T()" ? new Expressif.Values.Tuple() : (TupleValue)Expression.CreateClosed(source).Evaluate(null)!;
    }

    private static TupleValue ParseTupleLike(string value)
        => value.Contains("=>", StringComparison.Ordinal)
            ? (TupleValue)Expression.CreateClosed(value).Evaluate(null)!
            : ParseTuple(value);

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

    [Conformance]
    public void TupleAt_Valid_Position(string value, int position, object? expected)
        => Assert.That(new TupleAt(() => position).Evaluate(ParseInput(value)), Is.EqualTo(ParseExpected(expected)));

    [Conformance]
    public void TupleFirst_Valid(string value, object? expected)
        => Assert.That(new TupleFirst().Evaluate(ParseInput(value)), Is.EqualTo(ParseExpected(expected)));

    [Conformance]
    public void TupleSecond_Valid(string value, object? expected)
        => Assert.That(new TupleSecond().Evaluate(ParseInput(value)), Is.EqualTo(ParseExpected(expected)));

    private static object? ParseInput(string value)
        => value switch
        {
            "(null)" => null,
            "(empty)" => new Empty(),
            _ => ParseTupleLike(value),
        };

    private static object? ParseExpected(object? expected)
        => expected is string text && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? number
            : expected;

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

    [Conformance]
    public void Swap_Valid_Default(string value, string expected)
        => Assert.That(new Swap().Evaluate(ParseTupleLike(value)), Is.EqualTo(ParseTupleLike(expected)));

    [Conformance]
    public void Swap_Valid_Explicit(string value, int first, int second, string expected)
        => Assert.That(
            new Swap(() => first, () => second).Evaluate(ParseTupleLike(value)),
            Is.EqualTo(ParseTupleLike(expected)));

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
                new Extend(_ => new Expressif.Values.Tuple()).Evaluate(ParseTupleLike(value)),
                Is.EqualTo(ParseTupleLike(expected)));
            return;
        }

        Assert.That(
            Expression.CreateClosed($"{NormalizeTupleSyntax(value)} | extend({parameter})").Evaluate(null),
            Is.EqualTo(ParseTupleLike(expected)));
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

    [Conformance]
    public void Pick_Valid(string value, int[] positions, string expected)
        => Assert.That(
            Expression.Create($"pick({string.Join(", ", positions)})").Evaluate(ParseTupleLike(value)),
            Is.EqualTo(ParseTupleLike(expected)));

    [Test]
    public void Group_TupleOperations_ReturnOrdinaryTuples()
    {
        var group = new Expressif.Values.Group("USA", new[] { 1, 2 });

        Assert.Multiple(() =>
        {
            Assert.That(new Arity().Evaluate(group), Is.EqualTo(2));
            Assert.That(new TupleAt(() => 0).Evaluate(group), Is.EqualTo("USA"));
            Assert.That(new TupleAt(() => 1).Evaluate(group), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(new Pick(() => new[] { 0, 1 }).Evaluate(group), Is.TypeOf<Expressif.Values.Tuple>());
            Assert.That(new Swap().Evaluate(group), Is.TypeOf<Expressif.Values.Tuple>());
            Assert.That(new Extend(_ => new Expressif.Values.Tuple()).Evaluate(group), Is.TypeOf<Expressif.Values.Tuple>());
        });
    }

    [Test]
    public void Swap_Pair_ReturnsOrdinaryTuple()
        => Assert.That(
            new Swap().Evaluate(new PairValue("USA", 42)),
            Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(new TupleValue(42, "USA")));

    [Test]
    public void Pick_Pair_ReturnsOrdinaryTuple()
        => Assert.That(
            new Pick(() => new[] { 1, 0 }).Evaluate(new PairValue("USA", 42)),
            Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(new TupleValue(42, "USA")));

    [Test]
    public void Extend_Pair_ReturnsOrdinaryTuple()
        => Assert.That(
            new Extend(_ => new Expressif.Values.Tuple()).Evaluate(new PairValue("USA", 42)),
            Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(new TupleValue("USA", 42)));
}
