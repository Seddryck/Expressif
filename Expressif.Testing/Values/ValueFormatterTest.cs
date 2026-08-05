using Expressif.Values;

namespace Expressif.Testing.Values;

public class ValueFormatterTest
{
    [Test]
    public void Format_Record_UsesMinimalQuotingWithoutTypeAmbiguity()
    {
        var record = new RecordValue();
        record.Set("name", "Alice");
        record.Set("country", "United Kingdom");
        record.Set("booleanText", "true");

        var result = ValueFormatter.Format(record);

        Assert.That(result, Is.EqualTo("{name := Alice, country := \"United Kingdom\", booleanText := \"true\"}"));
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

        Assert.That(result, Is.EqualTo("{name := Alice, address := {city := Brussels, country := Belgium}, roles := {admin, reviewer}}"));
    }

    [Test]
    public void Format_Record_UsesBareGeneratedFieldName()
    {
        var record = new RecordValue();
        record.Set("__NONAME_0", "Alice");
        record.Set("active", true);

        var result = ValueFormatter.Format(record);

        Assert.That(result, Is.EqualTo("{__NONAME_0 := Alice, active := true}"));
    }
}
