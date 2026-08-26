using System.Data;
using Expressif.Cli.Infrastructure;

namespace Expressif.Cli.Tests;

public class OwnedDataReaderTests
{
    [Test]
    public void Reader_DelegatesDataAccessAndMetadata()
    {
        var table = CreateTable();
        using var inner = table.CreateDataReader();
        var owner = new TrackingDisposable();
        using var reader = new OwnedDataReader(inner, owner, headersAreRows: true);

        Assert.That(reader.Read(), Is.True);
        var bytes = new byte[2];
        var chars = new char[2];
        var values = new object[table.Columns.Count];

        Assert.Multiple(() =>
        {
            Assert.That(reader.HeadersAreRows, Is.True);
            Assert.That(reader.Depth, Is.EqualTo(inner.Depth));
            Assert.That(reader.IsClosed, Is.False);
            Assert.That(reader.RecordsAffected, Is.EqualTo(inner.RecordsAffected));
            Assert.That(reader.FieldCount, Is.EqualTo(table.Columns.Count));
            Assert.That(reader[0], Is.True);
            Assert.That(reader["text"], Is.EqualTo("abc"));
            Assert.That(reader.GetSchemaTable(), Is.Not.Null);
            Assert.That(reader.GetBoolean(0), Is.True);
            Assert.That(reader.GetByte(1), Is.EqualTo(2));
            Assert.That(reader.GetBytes(2, 0, bytes, 0, bytes.Length), Is.EqualTo(2));
            Assert.That(reader.GetChar(3), Is.EqualTo('x'));
            Assert.That(reader.GetChars(4, 0, chars, 0, chars.Length), Is.EqualTo(2));
            Assert.That(reader.GetDataTypeName(5), Is.Not.Empty);
            Assert.That(reader.GetDateTime(5), Is.EqualTo(new DateTime(2026, 8, 25)));
            Assert.That(reader.GetDecimal(6), Is.EqualTo(1.5m));
            Assert.That(reader.GetDouble(7), Is.EqualTo(2.5d));
            Assert.That(reader.GetFieldType(8), Is.EqualTo(typeof(float)));
            Assert.That(reader.GetFloat(8), Is.EqualTo(3.5f));
            Assert.That(reader.GetGuid(9), Is.EqualTo(Guid.Empty));
            Assert.That(reader.GetInt16(10), Is.EqualTo(4));
            Assert.That(reader.GetInt32(11), Is.EqualTo(5));
            Assert.That(reader.GetInt64(12), Is.EqualTo(6));
            Assert.That(reader.GetName(13), Is.EqualTo("text"));
            Assert.That(reader.GetOrdinal("text"), Is.EqualTo(13));
            Assert.That(reader.GetString(13), Is.EqualTo("abc"));
            Assert.That(reader.GetValue(11), Is.EqualTo(5));
            Assert.That(reader.GetValues(values), Is.EqualTo(table.Columns.Count));
            Assert.That(reader.IsDBNull(14), Is.True);
            Assert.That(reader.NextResult(), Is.False);
        });
    }

    [Test]
    public void GetData_DelegatesUnsupportedOperation()
    {
        using var inner = CreateTable().CreateDataReader();
        using var reader = new OwnedDataReader(inner, new TrackingDisposable());
        Assert.That(reader.Read(), Is.True);

        Assert.That(() => reader.GetData(0), Throws.Exception);
    }

    [Test]
    public void Close_DisposesOwnerAndClosesReader()
    {
        using var inner = CreateTable().CreateDataReader();
        var owner = new TrackingDisposable();
        var reader = new OwnedDataReader(inner, owner);

        reader.Close();

        Assert.Multiple(() =>
        {
            Assert.That(reader.IsClosed, Is.True);
            Assert.That(owner.IsDisposed, Is.True);
        });
    }

    private static DataTable CreateTable()
    {
        var table = new DataTable();
        table.Columns.Add("boolean", typeof(bool));
        table.Columns.Add("byte", typeof(byte));
        table.Columns.Add("bytes", typeof(byte[]));
        table.Columns.Add("character", typeof(char));
        table.Columns.Add("characters", typeof(char[]));
        table.Columns.Add("date", typeof(DateTime));
        table.Columns.Add("decimal", typeof(decimal));
        table.Columns.Add("double", typeof(double));
        table.Columns.Add("float", typeof(float));
        table.Columns.Add("guid", typeof(Guid));
        table.Columns.Add("short", typeof(short));
        table.Columns.Add("integer", typeof(int));
        table.Columns.Add("long", typeof(long));
        table.Columns.Add("text", typeof(string));
        table.Columns.Add("missing", typeof(string));
        table.Rows.Add(
            true, (byte)2, new byte[] { 3, 4 }, 'x', new[] { 'y', 'z' }, new DateTime(2026, 8, 25),
            1.5m, 2.5d, 3.5f, Guid.Empty, (short)4, 5, 6L, "abc", DBNull.Value);
        return table;
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
