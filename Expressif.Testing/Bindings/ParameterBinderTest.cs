using Expressif.Bindings;
using Expressif.Values;

namespace Expressif.Testing.Bindings;

public class ParameterBinderTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Bind_LiteralSyntax_PreservesTypedValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(((LiteralParameter)BindingTestAdapter.Parameter("42")).Value, Is.EqualTo(42m).And.TypeOf<decimal>());
            Assert.That(((LiteralParameter)BindingTestAdapter.Parameter("#true")).Value, Is.EqualTo(true).And.TypeOf<bool>());
            Assert.That(((LiteralParameter)BindingTestAdapter.Parameter("#null")).Value, Is.Null);
            Assert.That(((QuotedLiteralParameter)BindingTestAdapter.Parameter("\"Alice\"")).Value, Is.EqualTo("Alice").And.TypeOf<string>());
            Assert.That(((LiteralParameter)BindingTestAdapter.Parameter("#\"2026-08-17\"")).Value, Is.EqualTo(new DateOnly(2026, 8, 17)).And.TypeOf<DateOnly>());
            Assert.That(((LiteralParameter)BindingTestAdapter.Parameter("#\"2026-08-17T14:30:00\"")).Value, Is.EqualTo(new DateTime(2026, 8, 17, 14, 30, 0)).And.TypeOf<DateTime>());
            Assert.That(((LiteralParameter)BindingTestAdapter.Parameter("#\"14:30:00\"")).Value, Is.EqualTo(new TimeOnly(14, 30, 0)).And.TypeOf<TimeOnly>());
        });
    }

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
    [TestCase("( @foo | text-to-func(\"bar\") )", typeof(InputExpressionParameter))]
    [TestCase("T(10, 20)", typeof(TupleParameter))]
    [TestCase("T(1, T(2, 3))", typeof(TupleParameter))]
    public void Parse_Parameter_Valid(string value, Type type)
        => Assert.That(BindingTestAdapter.Parameter(value), Is.TypeOf(type));

    [Test]
    [TestCase("(\"foo\", \"bar\")")]
    [TestCase("( \"foo\", \"bar\" ) ")]
    [TestCase("(@foo , \"bar\")")]
    [TestCase("(^.foo , ^.1)")]
    [TestCase("(I[10, 45] , ^.1)")]
    [TestCase("(I[10, 45[ , ^.foo)")]
    [TestCase("(@foo , { @foo | text-to-func(\"bar\", @foo) })")]
    [TestCase("(@foo , { @foo | text-to-func(\"bar\", { @fool | numeric-to-func(^.3, ^.bez) }) })")]
    public void Parse_Parameters_Valid(string value)
        => Assert.That(BindingTestAdapter.Parameters(value).Count, Is.EqualTo(2));

    [Test]
    [TestCase("{{1, 2, 3}, {4, 5}}")]
    [TestCase("{{\"a\", \"b\"}, {\"c\"}}")]
    [TestCase("{{#true, #false}, {#null, 3}}")]
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

    [Test]
    public void Parse_TupleLiteral_RoundTripsThroughClosedExpression()
    {
        var value = new ClosedExpression("T(1, T(2, 3))").Evaluate();

        Assert.That(ValueFormatter.Format(value), Is.EqualTo("T(1, T(2, 3))"));
    }
}
