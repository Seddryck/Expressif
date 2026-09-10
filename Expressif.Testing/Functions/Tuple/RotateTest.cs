using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Tuple;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class RotateTest
{
    [Conformance]
    public void Rotate_Default(string value, string expected)
        => AssertRotation(value, "rotate", expected);

    [Conformance]
    public void Rotate_Offset(string value, int offset, string expected)
        => AssertRotation(value, $"rotate({offset})", expected);

    private static void AssertRotation(string value, string operation, string expected)
    {
        var input = ParseTuple(value);
        var result = Expression.Create(operation).Evaluate(input);
        Assert.That(result, Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(ParseTuple(expected)));
    }

    private static object? ParseTuple(string value)
        => Expression.Create(value.Replace("T(", "tuple(", StringComparison.Ordinal)).Evaluate(null);

    [TestCase("{items := T(10, 20, 30), offset := 1} | .items | rotate(.offset)")]
    [TestCase("T(1, T(10, 20, 30)) | $1 | rotate($0)")]
    [TestCase("T(10, 20, 30) | rotate(offset := 1)")]
    public void Offset_BindingUsesEnclosingContext(string source)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(new TupleValue(30m, 10m, 20m)));

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    public void Offset_EvaluatedOncePerTuple(int arity)
    {
        var count = 0;
        IFunction<IPositionalValue, TupleValue?> function = new Rotate(() => { count++; return 1; });
        var input = new TupleValue(Enumerable.Range(0, arity).Cast<object?>().ToArray());
        function.Evaluate(input);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void Evaluate_PreservesReferencesAndInput()
    {
        var item = new object();
        var input = new TupleValue(item, null, 10);
        var result = new Rotate().Evaluate(input)!;
        Assert.Multiple(() =>
        {
            Assert.That(result[1], Is.SameAs(item));
            Assert.That(result[2], Is.Null);
            Assert.That(input[0], Is.SameAs(item));
            Assert.That(input[1], Is.Null);
            Assert.That(input[2], Is.EqualTo(10));
        });
    }

    [TestCase(null)]
    [TestCase(42)]
    [TestCase("text")]
    public void Evaluate_NonPositionalInput_ReturnsNull(object? input)
        => Assert.That(Expression.Create("rotate").Evaluate(input), Is.Null);

    [Test]
    public void Evaluate_Pair_ReturnsTuple()
        => Assert.That(new Rotate().Evaluate(new PairValue("a", 10)),
            Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(new TupleValue(10, "a")));

    [TestCase("rotate(1, 2)")]
    [TestCase("rotate(unknown := 1)")]
    [TestCase("rotate(\"bad\")")]
    [TestCase("rotate(2147483648)")]
    public void InvalidOffset_UsesStandardIntegerValidation(string operation)
        => Assert.That(() => Expression.Create(operation).Evaluate(new TupleValue(10, 20, 30)), Throws.Exception);

    [Test]
    public void NullOffset_UsesStandardZeroConversion()
        => Assert.That(Expression.Create("rotate(#null)").Evaluate(new TupleValue(10, 20, 30)),
            Is.EqualTo(new TupleValue(10, 20, 30)));

    [Test]
    public void Introspection_ReportsTupleContractAndOptionalInteger()
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == "rotate");
        Assert.Multiple(() =>
        {
            Assert.That(info.Input, Is.EqualTo("tuple"));
            Assert.That(info.Output, Is.EqualTo("tuple"));
            Assert.That(info.Aliases, Is.Empty);
            Assert.That(info.Parameters, Has.Length.EqualTo(1));
            Assert.That(info.Parameters[0].Name, Is.EqualTo("offset"));
            Assert.That(info.Parameters[0].Type, Is.EqualTo("integer"));
            Assert.That(info.Parameters[0].Optional, Is.True);
        });
    }

    [Test]
    public void ConcurrentEvaluation_IsolatesOffsetContext()
    {
        var expression = Expression.Create("$1 | rotate($0)");
        Parallel.For(0, 30, offset =>
        {
            var tuple = new TupleValue(10, 20, 30);
            Assert.That(expression.Evaluate(new TupleValue(offset, tuple)),
                Is.EqualTo(new Rotate(() => offset).Evaluate(tuple)));
        });
    }
}
