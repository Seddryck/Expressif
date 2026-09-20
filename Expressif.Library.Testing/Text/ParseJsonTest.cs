using Expressif.Introspection;
using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Character;
using Expressif.Library.Text.Concatenation;
using Expressif.Library.Text.Conversion;
using Expressif.Library.Text.Counting;
using Expressif.Library.Text.Encoding;
using Expressif.Library.Text.Filtering;
using Expressif.Library.Text.Masking;
using Expressif.Library.Text.Normalization;
using Expressif.Library.Text.Padding;
using Expressif.Library.Text.Partitioning;
using Expressif.Library.Text.Selection;
using Expressif.Library.Text.Tokenization;
using System.Text.Json;
using Expressif.Functions;
using Expressif.Discovery;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Text;

public class ParseJsonTest
{
    [Conformance]
    public void ParseJson_Valid(string value, string expected)
        => Assert.That(new ParseJson().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ParseJson_Fields(string value, string expression, object? expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ParseJson_Number(string value, decimal expected)
        => Assert.That(new ParseJson().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ParseJson_Boolean(string value, bool expected)
        => Assert.That(new ParseJson().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ParseJson_Array(string value, decimal[] expected)
        => Assert.That(new ParseJson().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ParseJson_Null(string? value, object? expected)
        => Assert.That(new ParseJson().Evaluate(value), Is.EqualTo(expected));
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("{")]
    [TestCase("{\"a\":1,}")]
    [TestCase("[1,]")]
    [TestCase("/* comment */ {}")]
    [TestCase("{} {}")]
    [TestCase("NaN")]
    public void InvalidJson_Throws(string value)
        => Assert.That(() => TestExpression.Create("parse-json").Evaluate(value), Throws.InstanceOf<JsonException>());

    [Test]
    public void NestedValues_AreNativeAndIndependent()
    {
        var expression = TestExpression.Create("parse-json");
        var first = (RecordValue)expression.Evaluate("{\"rows\":[{\"name\":\"Ada\"},null,true]}")!;
        var rows = (object?[])new Expressif.Library.Record.Field(() => "rows").Evaluate(first)!;
        Assert.That(rows[0], Is.TypeOf<RecordValue>());
        Assert.That(rows[1], Is.Null);
        Assert.That(rows[2], Is.True);
        Assert.That(TestExpression.Create("field(\"name\")").Evaluate(rows[0]), Is.EqualTo("Ada"));
        Assert.That(expression.Evaluate("{}"), Is.Not.SameAs(first));
    }

    [Test]
    public void TextField_CanBeParsedExplicitly()
    {
        var input = new RecordValue();
        input.Set("payload", "{\"name\":\"Ada\"}");
        Assert.That(TestExpression.Create(".payload | parse-json | field(\"name\")").Evaluate(input), Is.EqualTo("Ada"));
        Assert.That(TestExpression.Create(".payload").Evaluate(input), Is.TypeOf<string>());
    }

    [Test]
    public void Contract_IsTextToRuntimeValue()
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(x => x.Name == "parse-json");
        Assert.That(info.Input, Is.EqualTo("text"));
        Assert.That(info.Output, Is.EqualTo("any"));
        Assert.That(info.Reason, Is.EqualTo("Output depends on the JSON value parsed at runtime."));
        Assert.That(typeof(ParseJson).GetInterfaces(), Does.Contain(typeof(IFunction<string?, object?>)));
    }
}
