using Expressif.Serializers;
using Expressif.Parsers;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Serializers;

public class ParameterSerializerTest
{
    [Test]
    [TestCase("foo")]
    [TestCase("foo_bar")]
    [TestCase("123")]
    [TestCase("123.125")]
    public void Serialize_LiteralParameter_Unquoted(string value)
        => Assert.That(new ParameterSerializer().Serialize(new LiteralParameter(value)), Is.EqualTo(value));

    [Test]
    [TestCase("foo@bar")]
    [TestCase("{foo_bar}")]
    [TestCase("(123)")]
    [TestCase("123,125")]
    public void Serialize_LiteralParameter_Quoted(string value)
        => Assert.That(new ParameterSerializer().Serialize(new LiteralParameter(value)), Is.EqualTo($"\"{value}\""));

    [Test]
    public void Serialize_LiteralParameter_WithEmbeddedDoubleQuotes_RoundTrip()
    {
        var serializer = new ParameterSerializer();
        var parameter = new LiteralParameter("Alice said \"hello\"");

        var serialized = serializer.Serialize(parameter);
        var parsed = Parameter.Parser.Parse(serialized);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("\"Alice said \\\"hello\\\"\""));
            Assert.That(parsed, Is.TypeOf<LiteralParameter>());
            Assert.That(((LiteralParameter)parsed).Value, Is.EqualTo("Alice said \"hello\""));
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
        var parsed = Parameter.Parser.End().Parse(serialized);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("{:}"));
            Assert.That(parsed, Is.TypeOf<RecordLiteralParameter>());
            Assert.That(((RecordLiteralParameter)parsed).Fields, Is.Empty);
        });
    }
}
