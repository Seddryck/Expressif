using Expressif.Serializers;
using Expressif.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Serializers;

public class ParameterSerializerTest
{
    [Test]
    public void Serialize_TypedLiteralParameter_PreservesSyntaxKind()
    {
        var serializer = new ParameterSerializer();

        Assert.Multiple(() =>
        {
            Assert.That(serializer.Serialize(new LiteralParameter(42m)), Is.EqualTo("42"));
            Assert.That(serializer.Serialize(new LiteralParameter(true)), Is.EqualTo("#true"));
            Assert.That(serializer.Serialize(new LiteralParameter(null)), Is.EqualTo("#null"));
            Assert.That(serializer.Serialize(new LiteralParameter(new DateOnly(2026, 8, 17))), Is.EqualTo("#\"2026-08-17\""));
            Assert.That(serializer.Serialize(new LiteralParameter(new DateTime(2026, 8, 17, 14, 30, 0))), Is.EqualTo("#\"2026-08-17T14:30:00\""));
            Assert.That(serializer.Serialize(new LiteralParameter(new TimeOnly(14, 30, 0))), Is.EqualTo("#\"14:30:00\""));
        });
    }

    [Test]
    [TestCase("I[1, 10]", "I[1, 10]")]
    [TestCase("I[1, 10[", "I[1, 10)")]
    [TestCase("I]1, 10]", "I(1, 10]")]
    [TestCase("I]1, 10[", "I(1, 10)")]
    [TestCase("I[-INF, +INF]", "I[-INF, +INF]")]
    [TestCase("I[#\"2022-12-10\", #\"2022-12-31\"[", "I[#\"2022-12-10\", #\"2022-12-31\")")]
    public void Serialize_IntervalParameter_NormalizesAliases(string source, string expected)
    {
        var parameter = new ExpressifBinder().BindParameter(ExpressifSyntax.Parse(source));

        Assert.That(new ParameterSerializer().Serialize(parameter), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("foo")]
    [TestCase("foo_bar")]
    [TestCase("123")]
    public void Serialize_LiteralParameter_Unquoted(string value)
        => Assert.That(new ParameterSerializer().Serialize(new LiteralParameter(value)), Is.EqualTo(value));

    [Test]
    [TestCase("foo@bar")]
    [TestCase("{foo_bar}")]
    [TestCase("(123)")]
    [TestCase("123,125")]
    [TestCase("123.125")]
    public void Serialize_LiteralParameter_Quoted(string value)
        => Assert.That(new ParameterSerializer().Serialize(new LiteralParameter(value)), Is.EqualTo($"\"{value}\""));

    [Test]
    public void Serialize_LiteralParameter_WithEmbeddedDoubleQuotes_RoundTrip()
    {
        var serializer = new ParameterSerializer();
        var parameter = new LiteralParameter("Alice said \"hello\"");

        var serialized = serializer.Serialize(parameter);
        var parsed = new ExpressifBinder().BindParameter(ExpressifSyntax.Parse(serialized));

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("\"Alice said \\\"hello\\\"\""));
            Assert.That(parsed, Is.TypeOf<QuotedLiteralParameter>());
            Assert.That(((QuotedLiteralParameter)parsed).Value, Is.EqualTo("Alice said \"hello\""));
        });
    }

    [Test]
    [TestCase("foo")]
    [TestCase("foo123")]
    public void Serialize_VariableParameter_Arobas(string value)
        => Assert.That(new ParameterSerializer().Serialize(new VariableParameter(value)), Is.EqualTo($"@{value}"));

    [Test]
    [TestCase("foo")]
    [TestCase("foo123")]
    public void Serialize_ObjectPropertyParameter_Brakets(string value)
        => Assert.That(new ParameterSerializer().Serialize(new ObjectPropertyParameter(value)), Is.EqualTo($"[{value}]"));

    [Test]
    [TestCase(0)]
    [TestCase(10)]
    public void Serialize_ObjectIndexParameter_Brakets(int value)
        => Assert.That(new ParameterSerializer().Serialize(new ObjectIndexParameter(value)), Is.EqualTo($"#{value}"));

    [Test]
    public void Serialize_RecordLiteralParameter_EmptyRecord_UsesRecordMarker()
    {
        var serializer = new ParameterSerializer();
        var parameter = new RecordLiteralParameter(Array.Empty<RecordLiteralField>());

        Assert.That(serializer.Serialize(parameter), Is.EqualTo("{:}"));
    }

    [Test]
    public void Serialize_RecordLiteralParameter_EmptyRecord_RoundTrip()
    {
        var serializer = new ParameterSerializer();
        var serialized = serializer.Serialize(new RecordLiteralParameter(Array.Empty<RecordLiteralField>()));
        var parsed = new ExpressifBinder().BindParameter(ExpressifSyntax.Parse(serialized));

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("{:}"));
            Assert.That(parsed, Is.TypeOf<RecordLiteralParameter>());
            Assert.That(((RecordLiteralParameter)parsed).Fields, Is.Empty);
        });
    }

    [Test]
    public void Serialize_TupleSpread_PreservesSpreadMarkers()
    {
        var parameter = new ExpressifBinder().BindParameter(ExpressifSyntax.Parse("T(1, ...T(2, 3), ...)"));

        Assert.That(new ParameterSerializer().Serialize(parameter), Is.EqualTo("T(1, ...T(2, 3), ...)"));
    }

    [Test]
    public void Serialize_RecordSpread_PreservesSpreadExpressions()
    {
        var function = new ExpressifBinder().BindFunction(ExpressifSyntax.Parse(
            "record(a := 1, ...{b := 2}, ..., c := 3)"));

        Assert.That(new FunctionSerializer().Serialize(function),
            Is.EqualTo("record(a := 1, ...{b := 2}, ..., c := 3)"));
    }
}
