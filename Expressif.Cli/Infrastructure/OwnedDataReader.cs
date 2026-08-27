using System.Data;

namespace Expressif.Cli.Infrastructure;

internal interface IHeaderDataReader : IDataReader
{
    bool HeadersAreRows { get; }
}

internal sealed class OwnedDataReader(
    IDataReader inner,
    IDisposable owner,
    bool headersAreRows = false,
    bool skipRepeatedHeaders = false) : IHeaderDataReader
{
    public bool HeadersAreRows { get; } = headersAreRows;

    public int Depth => inner.Depth;
    public bool IsClosed => inner.IsClosed;
    public int RecordsAffected => inner.RecordsAffected;
    public int FieldCount => inner.FieldCount;
    public object this[int i] => inner[i];
    public object this[string name] => inner[name]!;

    public void Close()
    {
        try
        {
            inner.Close();
        }
        finally
        {
            owner.Dispose();
        }
    }

    public DataTable GetSchemaTable() => inner.GetSchemaTable()!;
    public bool NextResult() => inner.NextResult();
    public bool Read()
    {
        while (inner.Read())
        {
            if (!skipRepeatedHeaders || !IsHeaderRow())
                return true;
        }

        return false;
    }

    private bool IsHeaderRow()
    {
        for (var i = 0; i < FieldCount; i++)
        {
            if (!string.Equals(Convert.ToString(inner.GetValue(i)), inner.GetName(i), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    public void Dispose()
    {
        try
        {
            inner.Dispose();
        }
        finally
        {
            owner.Dispose();
        }
    }

    public bool GetBoolean(int i) => inner.GetBoolean(i);
    public byte GetByte(int i) => inner.GetByte(i);
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
        => inner.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
    public char GetChar(int i) => inner.GetChar(i);
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
        => inner.GetChars(i, fieldoffset, buffer, bufferoffset, length);
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
    public string GetName(int i) => inner.GetName(i);
    public int GetOrdinal(string name) => inner.GetOrdinal(name);
    public string GetString(int i) => inner.GetString(i);
    public object GetValue(int i) => inner.GetValue(i);
    public int GetValues(object[] values) => inner.GetValues(values);
    public bool IsDBNull(int i) => inner.IsDBNull(i);
}
