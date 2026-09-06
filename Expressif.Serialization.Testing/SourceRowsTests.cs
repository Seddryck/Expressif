using System.Collections;
using System.Data;

namespace Expressif.Serialization.Testing;

public class SourceRowsTests
{
    [Test]
    public void Normalize_EnumerableInScalarMode_RejectsNonTabularSource()
    {
        Assert.That(
            () => SourceRows.Read(new[] { 1 }, "source.expr", scalar: true).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("tabular source"));
    }

    [Test]
    public void Normalize_Enumerable_DisposesEnumerator()
    {
        var source = new TrackingEnumerable();

        Assert.That(SourceRows.Read(source, "source.expr").ToArray(), Is.EqualTo(new[] { 1 }));
        Assert.That(source.IsDisposed, Is.True);
    }

    [Test]
    public void Normalize_MultiColumnReaderInScalarMode_ReportsColumnCount()
    {
        var table = new DataTable();
        table.Columns.Add("first");
        table.Columns.Add("second");
        using var reader = table.CreateDataReader();

        Assert.That(
            () => SourceRows.Read(reader, "source.sql", scalar: true).ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("exactly one column; found 2"));
    }

    [Test]
    public void Normalize_ReaderWithDuplicateColumnNames_RejectsDuplicate()
    {
        var table = new DataTable();
        table.Columns.Add("first");
        table.Columns.Add("second");
        table.Rows.Add("alpha", "beta");
        using var reader = new DuplicateNameDataReader(table.CreateDataReader());

        Assert.That(
            () => SourceRows.Read(reader, "source.sql").ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("duplicate column name 'name'"));
    }

    [Test]
    public void Normalize_CsvReaderWithEmptyHeader_ReportsFieldPosition()
    {
        var table = new DataTable();
        table.Columns.Add("column");
        table.Rows.Add(string.Empty);
        using var inner = table.CreateDataReader();
        using var reader = new OwnedDataReader(inner, new TrackingDisposable(), headersAreRows: true);

        Assert.That(
            () => SourceRows.Read(reader, "source.csv").ToArray(),
            Throws.TypeOf<FormatException>().With.Message.Contains("field 1 is empty"));
    }

    private sealed class TrackingEnumerable : IEnumerable
    {
        public bool IsDisposed { get; private set; }

        public IEnumerator GetEnumerator() => new Enumerator(this);

        private sealed class Enumerator(TrackingEnumerable owner) : IEnumerator, IDisposable
        {
            private bool beforeFirst = true;

            public object Current => 1;

            public bool MoveNext()
            {
                if (!beforeFirst)
                    return false;
                beforeFirst = false;
                return true;
            }

            public void Reset() => beforeFirst = true;
            public void Dispose() => owner.IsDisposed = true;
        }
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class DuplicateNameDataReader(IDataReader inner) : IDataReader
    {
        public object this[int i] => inner[i];
        public object this[string name] => inner[name];
        public int Depth => inner.Depth;
        public bool IsClosed => inner.IsClosed;
        public int RecordsAffected => inner.RecordsAffected;
        public int FieldCount => inner.FieldCount;
        public void Close() => inner.Close();
        public void Dispose() => inner.Dispose();
        public bool GetBoolean(int i) => inner.GetBoolean(i);
        public byte GetByte(int i) => inner.GetByte(i);
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => inner.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        public char GetChar(int i) => inner.GetChar(i);
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => inner.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        public IDataReader GetData(int i) => inner.GetData(i);
        public string GetDataTypeName(int i) => inner.GetDataTypeName(i);
        public DateTime GetDateTime(int i) => inner.GetDateTime(i);
        public decimal GetDecimal(int i) => inner.GetDecimal(i);
        public double GetDouble(int i) => inner.GetDouble(i);
        public Type GetFieldType(int i) => inner.GetFieldType(i);
        public float GetFloat(int i) => inner.GetFloat(i);
        public Guid GetGuid(int i) => inner.GetGuid(i);
        public short GetInt16(int i) => inner.GetInt16(i);
        public int GetInt32(int i) => inner.GetInt32(i);
        public long GetInt64(int i) => inner.GetInt64(i);
        public string GetName(int i) => "name";
        public int GetOrdinal(string name) => inner.GetOrdinal(name);
        public DataTable? GetSchemaTable() => inner.GetSchemaTable();
        public string GetString(int i) => inner.GetString(i);
        public object GetValue(int i) => inner.GetValue(i);
        public int GetValues(object[] values) => inner.GetValues(values);
        public bool IsDBNull(int i) => inner.IsDBNull(i);
        public bool NextResult() => inner.NextResult();
        public bool Read() => inner.Read();
    }
}
