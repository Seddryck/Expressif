using System.Text;
using Expressif.Values;

namespace Expressif.Serialization.Testing;

public class CsvValueReaderTests
{
    [Test]
    public void Read_HeadersAndNull_ProducesRecords()
    {
        using var stream = Input("name,value\nAda,NULL\n");
        var row = (RecordValue)CsvValueReader.Read(stream, [new("null-sequence", "NULL")]).Single()!;
        Assert.That(row.Keys, Is.EqualTo(new[] { "name", "value" }));
        Assert.That(row["name"], Is.EqualTo("Ada"));
        Assert.That(row["value"], Is.Null);
        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void Read_HeaderlessRows_UsesGeneratedNames()
    {
        using var stream = Input("alpha;beta\ngamma;delta\n");
        var rows = CsvValueReader.Read(stream, [new("header", false), new("delimiter", ";")]).Cast<RecordValue>().ToArray();
        Assert.That(rows, Has.Length.EqualTo(2));
        Assert.That(rows[0].Keys, Is.EqualTo(new[] { "column1", "column2" }));
        Assert.That(rows[1]["column2"], Is.EqualTo("delta"));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Read_NullSequence_ReturnsNullWithHeaderModes(bool header, bool repeated)
    {
        using var stream = Input(header ? "value\nNULL\n" : "NULL\n");
        var options = new List<CsvSourceOption> { new("header", header), new("null-sequence", "NULL") };
        if (repeated && header)
            options.Add(new("header-repeat", true));
        Assert.That(CsvValueReader.Read(stream, options, scalar: true), Is.EqualTo(new object?[] { null }));
    }

    [Test]
    public void Read_CustomLineTerminator_ConsumesLfRows()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("value\none\ntwo\n"));
        Assert.That(CsvValueReader.Read(stream, [new("line-terminator", "\n")], scalar: true), Is.EqualTo(new[] { "one", "two" }));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Read_Scalar_ReturnsCells(bool header)
    {
        using var stream = Input(header ? "value\nalpha\nbeta\n" : "alpha\nbeta\n");
        Assert.That(CsvValueReader.Read(stream, [new("header", header)], scalar: true), Is.EqualTo(new[] { "alpha", "beta" }));
    }

    [TestCase("", "header row")]
    [TestCase("name,NAME\na,b\n", "duplicate")]
    [TestCase(",name\na,b\n", "empty")]
    [TestCase("a,b\n1\n", "fields")]
    [TestCase("a\n1,2", "syntax")]
    public void Read_MalformedCsv_ReportsFailureAndLeavesStreamOpen(string csv, string message)
    {
        using var stream = Input(csv);
        Assert.That(() => CsvValueReader.Read(stream).ToArray(), Throws.TypeOf<FormatException>().With.Message.Contains(message));
        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void Read_HeaderlessInconsistentWidth_RejectsRow()
    {
        using var stream = Input("a,b\nc\n");
        Assert.That(() => CsvValueReader.Read(stream, [new("header", false)]).ToArray(), Throws.TypeOf<FormatException>().With.Message.Contains("record 2"));
    }

    [Test]
    public void Read_MultiColumnScalar_RejectsSource()
    {
        using var stream = Input("a,b\n1,2\n");
        Assert.That(() => CsvValueReader.Read(stream, scalar: true).ToArray(), Throws.TypeOf<FormatException>().With.Message.Contains("exactly one column"));
    }

    [Test]
    public void Read_EarlyTermination_LeavesStreamOpen()
    {
        using var stream = Input("a\none\ntwo\n");
        Assert.That(CsvValueReader.Read(stream).Take(1).Count(), Is.EqualTo(1));
        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void OpenDataReader_TransferredOwnership_ClosesStream()
    {
        using var stream = Input("a\none\n");
        using (var reader = CsvValueReader.OpenDataReader(stream, leaveOpen: false))
            Assert.That(SourceRows.Read(reader, "test").Count(), Is.EqualTo(1));
        Assert.That(stream.CanRead, Is.False);
    }

    [Test]
    public void Read_ConfiguredHeaders_ConsumesHeaderRows()
    {
        using var stream = Input("person,person\nfirst,last\nAda,Lovelace\n");
        var row = (RecordValue)CsvValueReader.Read(stream, [new("header-rows", new object?[] { 1, 2 }), new("header-join", ".")]).Single()!;
        Assert.That(row.Keys, Is.EqualTo(new[] { "column1", "column2" }));
        Assert.That(row[0], Is.EqualTo("Ada"));
    }

    [Test]
    public void Read_RepeatedHeaders_SkipsHeaderRows()
    {
        using var stream = Input("name\nAda\nname\nGrace\n");
        Assert.That(CsvValueReader.Read(stream, [new("header-repeat", true)], scalar: true), Is.EqualTo(new[] { "Ada", "Grace" }));
    }

    [TestCase("unknown", 1)]
    [TestCase("delimiter", "long")]
    [TestCase("header", "true")]
    [TestCase("header-rows", 0)]
    public void Build_InvalidTypedOptions_ReportsName(string name, object value)
        => Assert.That(() => new CsvSourceProfileBuilder().Build([new(name, value)]), Throws.TypeOf<FormatException>().With.Message.Contains(name));

    private static MemoryStream Input(string text) => new(Encoding.UTF8.GetBytes(text.Replace("\n", "\r\n", StringComparison.Ordinal)));
}
