using Expressif.Bindings;
using Expressif.Values;

namespace Expressif.Testing.Parsers;

public class ParameterTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("`foo`", typeof(QuotedLiteralParameter))]
    [TestCase("\"foo\"", typeof(QuotedLiteralParameter))]
    [TestCase("@foo", typeof(VariableParameter))]
    [TestCase("^.foo", typeof(ObjectPropertyParameter))]
    [TestCase("^.52", typeof(ObjectIndexParameter))]
    [TestCase("{}", typeof(ArrayParameter))]
    [TestCase("{:}", typeof(RecordLiteralParameter))]
    [TestCase("{1,2,3}", typeof(ArrayParameter))]
    [TestCase("{name := \"Alice\"}", typeof(RecordLiteralParameter))]
    [TestCase("{`first name` := \"Alice Smith\", active := #true}", typeof(RecordLiteralParameter))]
    [TestCase("{@foo}", typeof(ArrayParameter))]
    [TestCase("{ @foo, ^.1, ^.bar }", typeof(ArrayParameter))]
    [TestCase("{ @foo | text-to-func(\"bar\") }", typeof(InputExpressionParameter))]
    [TestCase("T(10, 20)", typeof(TupleParameter))]
    [TestCase("T(1, T(2, 3))", typeof(TupleParameter))]
    public void Parse_Parameter_Valid(string value, Type type)
        => Assert.That(BindingTestAdapter.Parameter(value), Is.TypeOf(type));

    [Test]
    [TestCase("(\"foo\", \"bar\")")]
    [TestCase("( \"foo\", \"bar\" ) ")]
    [TestCase("(@foo , \"bar\")")]
    [TestCase("(^.foo , ^.1)")]
    [TestCase("([10;45] , ^.1)")]
    [TestCase("([10;45[ , ^.foo)")]
    [TestCase("(@foo , { @foo | text-to-func(\"bar\", @foo) })")]
    [TestCase("(@foo , { @foo | text-to-func(\"bar\", { @fool | numeric-to-func(^.3, ^.bez) }) })")]
    public void Parse_Parameters_Valid(string value)
        => Assert.That(BindingTestAdapter.Parameters(value).Count, Is.EqualTo(2));

    [Test]
    [TestCase("{{1, 2, 3}, {4, 5}}")]
    [TestCase("{{\"a\", \"b\"}, {\"c\"}}")]
    [TestCase("{{#true, #false}, {null, 3}}")]
    public void Parse_Parameter_NestedArrays_Valid(string value)
    {
        var parsed = BindingTestAdapter.Parameter(value);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.TypeOf<ArrayParameter>());
            var outer = (ArrayParameter)parsed;
            Assert.That(outer.Values, Has.Length.EqualTo(2));
            Assert.That(outer.Values[0], Is.TypeOf<ArrayParameter>());
            Assert.That(outer.Values[1], Is.TypeOf<ArrayParameter>());
        });
    }

    [Test]
    public void Parse_Parameter_EmptyRecordLiteral_ParsesNoFields()
    {
        var parsed = BindingTestAdapter.Parameter("{:}");

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.TypeOf<RecordLiteralParameter>());
            Assert.That(((RecordLiteralParameter)parsed).Fields, Is.Empty);
        });
    }

    [TestCase("T()")]
    [TestCase("T(10)")]
    public void Parse_TupleWithFewerThanTwoFields_Invalid(string value)
        => Assert.Throws<ExpressifSyntaxException>(() => BindingTestAdapter.Parameter(value));

    [Test]
    public void Parse_TupleLiteral_RoundTripsThroughClosedExpression()
    {
        var value = new ClosedExpression("T(1, T(2, 3))").Evaluate();

        Assert.That(ValueFormatter.Format(value), Is.EqualTo("T(1, T(2, 3))"));
    }
}
