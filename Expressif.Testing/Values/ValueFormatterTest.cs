using System;
using Expressif.Values;

namespace Expressif.Testing.Values;

public class ValueFormatterTest
{
    [Test]
    public void Format_Record_QuotesStringsWithoutTypeAmbiguity()
    {
        var record = new RecordValue();
        record.Set("name", "Alice");
        record.Set("country", "United Kingdom");
        record.Set("booleanText", "true");

        var result = ValueFormatter.Format(record);

        Assert.That(result, Is.EqualTo("{name := \"Alice\", country := \"United Kingdom\", booleanText := \"true\"}"));
    }

    [Test]
    public void Format_Record_PreservesOrderAndNestedValues()
    {
        var nested = new RecordValue();
        nested.Set("city", "Brussels");
        nested.Set("country", "Belgium");

        var record = new RecordValue();
        record.Set("name", "Alice");
        record.Set("address", nested);
        record.Set("roles", new object?[] { "admin", "reviewer" });

        var result = ValueFormatter.Format(record);

        Assert.That(result, Is.EqualTo("{name := \"Alice\", address := {city := \"Brussels\", country := \"Belgium\"}, roles := {\"admin\", \"reviewer\"}}"));
    }

    [Test]
    public void Format_Record_UsesBareGeneratedFieldName()
    {
        var record = new RecordValue();
        record.Set("__NONAME_0", "Alice");
        record.Set("active", true);

        var result = ValueFormatter.Format(record);

        Assert.That(result, Is.EqualTo("{__NONAME_0 := \"Alice\", active := #true}"));
    }

    [Test]
    public void Format_Array_PreservesScalarSyntax()
    {
        object?[] array = [true, "Ada", 10.5m, new DateOnly(2026, 8, 28), new DateTime(2026, 8, 28, 14, 30, 45)];

        Assert.That(
            ValueFormatter.Format(array),
            Is.EqualTo("{#true, \"Ada\", 10.5, #\"2026-08-28\", #\"2026-08-28T14:30:45\"}"));
    }

    [Test]
    public void Format_Tuple_PreservesScalarSyntax()
    {
        var tuple = new TupleValue(true, "Ada", 10.5m, new DateOnly(2026, 8, 28), new DateTime(2026, 8, 28, 14, 30, 45));

        Assert.That(
            ValueFormatter.Format(tuple),
            Is.EqualTo("T(#true, \"Ada\", 10.5, #\"2026-08-28\", #\"2026-08-28T14:30:45\")"));
    }

    [Test]
    public void Format_Record_PreservesScalarSyntax()
    {
        var record = new RecordValue();
        record.Set("boolean", true);
        record.Set("string", "Ada");
        record.Set("numeric", 10.5m);
        record.Set("date", new DateOnly(2026, 8, 28));
        record.Set("datetime", new DateTime(2026, 8, 28, 14, 30, 45));

        Assert.That(
            ValueFormatter.Format(record),
            Is.EqualTo("{boolean := #true, string := \"Ada\", numeric := 10.5, date := #\"2026-08-28\", datetime := #\"2026-08-28T14:30:45\"}"));
    }

    [Test]
    public void Format_DbNull_ReturnsNullLiteral()
        => Assert.That(ValueFormatter.Format(DBNull.Value), Is.EqualTo("null"));
}
