using System.Text.Json;
using Expressif.Values;

namespace Expressif.Serialization.Testing;

public class ValueSerializersTests
{
    private static readonly IValueSerializer Json = ValueSerializers.Resolve(ValueSerializationFormat.Json);

    [TestCase("Alice", "\"Alice\"")]
    [TestCase(42, "42")]
    [TestCase(true, "true")]
    [TestCase(null, "null")]
    public void Json_ScalarResult_SerializesAsRootValue(object? value, string expected)
        => Assert.That(Json.Serialize(value), Is.EqualTo(expected));

    [Test]
    public void Json_NestedArrayAndRecord_SerializesActualShape()
    {
        var record = new RecordValue();
        record.Set("name", "Alice");
        record.Set("active", true);

        Assert.That(Json.Serialize(new object?[] { 42, null, record }),
            Is.EqualTo("[42,null,{\"name\":\"Alice\",\"active\":true}]"));
    }

    [TestCase("  ", "{\n  \"items\": [\n    1,\n    2\n  ]\n}")]
    [TestCase("", "{\n\"items\": [\n1,\n2\n]\n}")]
    [TestCase("\t", "{\n\t\"items\": [\n\t\t1,\n\t\t2\n\t]\n}")]
    public void Json_PrettyStyle_UsesRequestedIndentation(string indentation, string expected)
    {
        var record = new RecordValue();
        record.Set("items", new[] { 1, 2 });

        Assert.That(Json.Serialize(record, ValueFormat.Pretty, indentation), Is.EqualTo(expected));
        Assert.DoesNotThrow(() => JsonDocument.Parse(expected));
    }

    [TestCase("x")]
    [TestCase(" \tx")]
    [TestCase("\n")]
    [TestCase("\u00a0")]
    public void Json_InvalidIndentation_ThrowsArgumentException(string indentation)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => Json.Serialize(Array.Empty<object>(), ValueFormat.Pretty, indentation));

        Assert.That(exception!.ParamName, Is.EqualTo("indentation"));
    }

    [Test]
    public void Json_DateTime_PreservesFractionalSecondsWithoutTimeZone()
    {
        var value = new DateTime(2026, 9, 13, 12, 34, 56, DateTimeKind.Utc).AddTicks(1234567);

        Assert.That(Json.Serialize(value), Is.EqualTo("\"2026-09-13T12:34:56.1234567\""));
    }

    [Test]
    public void Json_SupportedScalarTypes_ProduceJsonValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Json.Serialize('x'), Is.EqualTo("\"x\""));
            Assert.That(Json.Serialize(new DateOnly(2026, 9, 13)), Is.EqualTo("\"2026-09-13\""));
            Assert.That(Json.Serialize(new DateTimeOffset(2026, 9, 13, 12, 34, 56, TimeSpan.FromHours(2))),
                Is.EqualTo("\"2026-09-13T12:34:56.0000000\\u002B02:00\""));
            Assert.That(Json.Serialize(new TimeOnly(12, 34, 56)), Is.EqualTo("\"12:34:56.0000000\""));
            Assert.That(Json.Serialize(Guid.Parse("12345678-1234-1234-1234-123456789abc")),
                Is.EqualTo("\"12345678-1234-1234-1234-123456789abc\""));
            Assert.That(Json.Serialize(ValueSerializationFormat.Json), Is.EqualTo("\"Json\""));
        });
    }

    [Test]
    public void Json_EmptyAndPairValues_PreserveTheirShapes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Json.Serialize(new RecordValue()), Is.EqualTo("{}"));
            Assert.That(Json.Serialize(Array.Empty<object>()), Is.EqualTo("[]"));
            Assert.That(Json.Serialize(new PairValue("key", 42)), Is.EqualTo("[\"key\",42]"));
        });
    }

    [Test]
    public void UnknownFormatOrStyle_ThrowsArgumentOutOfRangeException()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ValueSerializers.Resolve((ValueSerializationFormat)42));
            Assert.Throws<ArgumentOutOfRangeException>(() => Json.Serialize(42, (ValueFormat)42));
        });
    }

    [Test]
    public void Raw_UsesExistingValueFormatting()
    {
        var serializer = ValueSerializers.Resolve(ValueSerializationFormat.Raw);

        Assert.That(serializer.Serialize(new object?[] { "Alice", true }), Is.EqualTo("{\"Alice\", #true}"));
    }
}
