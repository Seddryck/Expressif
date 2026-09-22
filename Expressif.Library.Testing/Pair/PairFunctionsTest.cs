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
}
