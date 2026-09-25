using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Values;
using System.Globalization;

namespace Expressif.Testing.Pair;

public class PairFunctionsTest
{
    [Conformance]
    public void Pair_Valid_Constructor(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Pair_Valid_Literal(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Pair_Valid_Accessors(object? input, string expression, object? expected)
        => Assert.That(
            ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)),
            Is.EqualTo(Convert.ToString(expected, CultureInfo.InvariantCulture)));

    [Conformance]
    public void Pair_Valid_Array(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Pair_ExposesTypedContract()
    {
        IFunction<object?, PairValue> function = new Expressif.Library.Pair.Pair(_ => "BE", _ => 42);

        Assert.That(function.Evaluate(null), Is.EqualTo(new PairValue("BE", 42)));
    }

    [Test]
    public void Pair_ExpressionsEvaluateAgainstSameInput()
    {
        var input = new RecordValue();
        input.Set("country", "BE");
        input.Set("amount", 42m);

        Assert.That(
            TestExpression.Create("pair(.country, .amount)").Evaluate(input),
            Is.EqualTo(new PairValue("BE", 42m)));
    }

    [Test]
    public void Pair_NamedArgumentsRetainIncomingValueAndRootScope()
    {
        var input = new RecordValue();
        input.Set("country", "BE");
        input.Set("amount", 42m);

        Assert.That(
            TestExpression.Create("pair(value := .amount, key := ^.country)").Evaluate(input),
            Is.EqualTo(new PairValue("BE", 42m)));
    }

    [Test]
    public void Pair_InputBoundExpressionKeepsItsSupplyingScope()
    {
        var input = new RecordValue();
        input.Set("amount", 42m);

        var result = (PairValue)TestExpression.Create(
            "pair(@_ | source :> @source, @_ | source :> @source | .amount)").Evaluate(input)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Key, Is.SameAs(input));
            Assert.That(result.Value, Is.EqualTo(42m));
        });
    }

    [Test]
    public void Pair_NestedExpressionRetainsEnclosingRoot()
    {
        var item = new RecordValue();
        item.Set("amount", 42m);
        var input = new RecordValue();
        input.Set("country", "BE");
        input.Set("items", new object?[] { item });

        var result = (object?[])TestExpression.Create(
            ".items | map(pair(^.amount, ^^.country))").Evaluate(input)!;

        Assert.That(result, Is.EqualTo(new object?[] { new PairValue(42m, "BE") }));
    }

    [TestCase("pair(1)", typeof(MissingRequiredParameterException))]
    [TestCase("pair(1, 2, 3)", typeof(TooManyPositionalArgumentsException))]
    [TestCase("pair(unknown := 1, value := 2)", typeof(UnknownParameterNameException))]
    public void Pair_InvalidArgumentsFailDuringConstruction(string source, Type exceptionType)
        => Assert.That(() => TestExpression.Create(source), Throws.TypeOf(exceptionType));
}
