using System.Text.Json;
using Expressif.Values;

namespace Expressif.Serialization.Testing;

public class ValueSerializersTests
{
    private static readonly IValueSerializer Raw = ValueSerializers.Resolve(ValueSerializationFormat.Raw);
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
        Assert.That(Raw.Serialize(new object?[] { "Alice", true }), Is.EqualTo("{\"Alice\", #true}"));
    }

    [Test]
    public void OrderingValues_SerializeWithoutNumericRepresentation()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Raw.Serialize(OrderingValue.Less), Is.EqualTo("#less"));
            Assert.That(Raw.Serialize(OrderingValue.Equal), Is.EqualTo("#equal"));
            Assert.That(Raw.Serialize(OrderingValue.Greater), Is.EqualTo("#greater"));
            Assert.That(Json.Serialize(OrderingValue.Less), Is.EqualTo("\"#less\""));
        });
    }

    [Test]
    public void Raw_AllDimensionValues_RoundTripThroughExpressifSource()
    {
        var source = new DictionaryValue([
            new PairValue(new TupleValue("BE", AllDimension.Instance), 230m),
            new PairValue(new TupleValue(AllDimension.Instance, AllDimension.Instance), 320m),
        ]);

        var serialized = Raw.Serialize(source);
        var parsed = Expression.CreateClosed(serialized, new Expressif.Bindings.ExpressionBinder()).Evaluate(null);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("!{(T(\"BE\", #all) => 230), (T(#all, #all) => 320)}"));
            Assert.That(parsed, Is.EqualTo(source));
        });
    }

    [TestCase(ValueSerializationFormat.Raw, 9, "{\n  T(1, 2)\n}")]
    [TestCase(ValueSerializationFormat.Raw, 8, "{\n  T(\n    1,\n    2\n  )\n}")]
    [TestCase(ValueSerializationFormat.Json, 7, "[\n  [1,2]\n]")]
    [TestCase(ValueSerializationFormat.Json, 6, "[\n  [\n    1,\n    2\n  ]\n]")]
    public void PrettyInlineTuple_UsesOriginalRuntimeTypeAndRemainingWidth(
        ValueSerializationFormat format,
        int width,
        string expected)
    {
        var serializer = ValueSerializers.Resolve(format);
        var value = new object?[] { new TupleValue(1, 2) };

        var result = serializer.Serialize(value, PrettyWithInlineTuples(width));

        Assert.That(result, Is.EqualTo(expected));
        if (format == ValueSerializationFormat.Json)
            Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void Json_PrettyInlineTuple_LeavesOrdinaryArrayMultiline()
    {
        var value = new object?[] { new TupleValue(1, 2), new object?[] { 3, 4 } };

        var result = Json.Serialize(value, PrettyWithInlineTuples(80));

        Assert.That(result, Is.EqualTo("[\n  [1,2],\n  [\n    3,\n    4\n  ]\n]"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void Json_PrettyInlineTuple_RecursesAfterParentFallback()
    {
        var value = new TupleValue(new object?[] { 12345 }, new TupleValue(2, 3));

        var result = Json.Serialize(value, PrettyWithInlineTuples(10));

        Assert.That(result, Is.EqualTo("[\n  [\n    12345\n  ],\n  [2,3]\n]"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    private static ValueFormattingOptions PrettyWithInlineTuples(int width)
        => new()
        {
            Format = ValueFormat.Pretty,
            InlineValueTypes = new HashSet<Type> { typeof(TupleValue) },
            PreferredLineWidth = width,
        };
}
