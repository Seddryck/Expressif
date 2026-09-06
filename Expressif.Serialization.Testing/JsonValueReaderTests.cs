using System.Text;
using System.Text.Json;
using Expressif.Values;

namespace Expressif.Serialization.Testing;

public class JsonValueReaderTests
{
    [Test]
    public void Read_NestedValues_PreservesStructureAndTypes()
    {
        var record = (RecordValue)JsonValueReader.Read("""{"items":[null,true,false,{"text":"hello"}],"integer":42,"decimal":1.25,"large":2147483648,"double":1e100}""")!;
        Assert.Multiple(() =>
        {
            Assert.That(record["integer"], Is.TypeOf<int>().And.EqualTo(42));
            Assert.That(record["decimal"], Is.TypeOf<decimal>().And.EqualTo(1.25m));
            Assert.That(record["large"], Is.TypeOf<decimal>().And.EqualTo(2147483648m));
            Assert.That(record["double"], Is.TypeOf<double>().And.EqualTo(1e100));
            var items = (object?[])record["items"]!;
            Assert.That(items.Take(3), Is.EqualTo(new object?[] { null, true, false }));
            Assert.That(((RecordValue)items[3]!)["text"], Is.EqualTo("hello"));
        });
    }

    [TestCase("null", 1)]
    [TestCase("42", 1)]
    [TestCase("\"text\"", 1)]
    [TestCase("{}", 1)]
    [TestCase("[]", 0)]
    [TestCase("[1,[2,3]]", 2)]
    public void ReadRows_RootValues_KeepSourceCardinality(string json, int count)
        => Assert.That(JsonValueReader.ReadRows(json), Has.Length.EqualTo(count));

    [TestCase("")]
    [TestCase("{")]
    [TestCase("[1,]")]
    [TestCase("true false")]
    public void Read_MalformedJson_ThrowsJsonException(string json)
        => Assert.Catch<JsonException>(() => JsonValueReader.Read(json));

    [Test]
    public void Read_DuplicateKeys_PreservesLastValueAndOrder()
    {
        var record = (RecordValue)JsonValueReader.Read("""{"a":1,"b":2,"a":3}""")!;
        Assert.That(record.Keys, Is.EqualTo(new[] { "a", "b" }));
        Assert.That(record["a"], Is.EqualTo(3));
    }

    [Test]
    public void Read_BorrowedInputs_LeaveInputsOpen()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("[null,1]"));
        using var reader = new StringReader("[null,1]");
        Assert.That(JsonValueReader.Read(stream), Is.EqualTo(JsonValueReader.Read(reader)));
        Assert.That(stream.CanRead, Is.True);
        Assert.That(reader.Read(), Is.EqualTo(-1));
    }
}
