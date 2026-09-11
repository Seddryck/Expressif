using Expressif.Bindings;
using Expressif.Functions.Introspection;
using Expressif.Functions.Special;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Special;

public class RecodeTest
{
    [Conformance]
    public void Recode_Valid_Mapping(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("recode()", typeof(MissingRequiredParameterException))]
    [TestCase("recode(!{}, !{})", typeof(TooManyPositionalArgumentsException))]
    public void InvalidArity_IsRejected(string expression, Type exceptionType)
        => Assert.That(() => Expression.Create(expression), Throws.TypeOf(exceptionType));

    [TestCase("{(1 => 2)}")]
    [TestCase("T(1, 2)")]
    [TestCase("{key := 1, value := 2}")]
    [TestCase("grouping((1 => {2}))")]
    [TestCase("#{(1 => {2})}")]
    [TestCase("#null")]
    public void NonDictionaryMapping_IsRejected(string mapping)
        => Assert.That(() => Expression.Create($"recode({mapping})").Evaluate(1), Throws.ArgumentException);

    [TestCase("{code := \"A\", replacement := 42} | .code | recode(!{(\"A\" => .replacement)})", "42")]
    [TestCase("{replacement := 99, rows := {{code := \"A\", replacement := 42}}} | .rows | map(.code | recode(!{(\"A\" => .replacement)}))", "{42}")]
    [TestCase("\"A\" | recode(!{(\"A\" => 2)}) | add(3)", "5")]
    [TestCase("\"A\" | recode({(\"A\" => 2)} | apply(dictionary(...@_)))", "2")]
    public void MappingExpressions_ComposeWithSurroundingContext(string source, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(source).Evaluate(null)), Is.EqualTo(expected));

    [Test]
    public void Mapping_IsEvaluatedOnce_AndUnmatchedInputIsPreserved()
    {
        var calls = 0;
        var mapping = new DictionaryValue([]);
        var function = new Recode(() => { calls++; return mapping; });
        var input = new RecordValue();
        Assert.That(function.Evaluate(input), Is.SameAs(input));
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void BoundExpression_ReusesEachEvaluationsMappingConcurrently()
    {
        var expression = Expression.Create(".code | recode(^.status-codes)");
        Parallel.For(0, 20, i =>
        {
            var input = new RecordValue();
            input.Set("code", "A");
            input.Set("status-codes", new DictionaryValue([new Expressif.Values.Pair("A", i)]));
            Assert.That(expression.Evaluate(input), Is.EqualTo(i));
        });
    }

    [Test]
    public void Introspection_DescribesDictionaryParameterAndDynamicOutput()
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == "recode");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.ImplementationType, Is.EqualTo(typeof(Recode)));
            Assert.That(info.Input, Is.EqualTo("any"));
            Assert.That(info.Output, Is.EqualTo("any"));
            Assert.That(info.Converted, Is.False);
            Assert.That(info.Reason, Is.EqualTo("Output depends on the matched dictionary value, or preserves the input type when no key matches."));
            Assert.That(info.Parameters.Single().Name, Is.EqualTo("mapping"));
            Assert.That(info.Parameters.Single().Type, Is.EqualTo("dictionary"));
            Assert.That(info.Parameters.Single().Optional, Is.False);
        }
    }
}

