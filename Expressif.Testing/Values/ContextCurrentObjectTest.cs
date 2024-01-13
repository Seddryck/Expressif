using Expressif.Values;
using Expressif.Values.Special;
using NUnit.Framework.Constraints;
using System.Data;

namespace Expressif.Testing.Values;

public class ContextCurrentObjectTest
{
    [Test]
    public void Name_DictionaryWithExistingKey_KeyReturned()
    {
        var context = new Context();
        context.CurrentObject.Set(new Dictionary<string, object> { { "foo", 123 }, { "bar", 456 } });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject["foo"], Is.EqualTo(123));
            Assert.That(context.CurrentObject["bar"], Is.EqualTo(456));
        });
    }

    [Test]
    public void Name_ObjectWithExistingProperty_ValueReturned()
    {
        var context = new Context();
        context.CurrentObject.Set(new { foo = 123, bar = 456 });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject["foo"], Is.EqualTo(123));
            Assert.That(context.CurrentObject["bar"], Is.EqualTo(456));
        });
    }

    [Test]
    public void Name_DataRowWithExistingColumn_ValueReturned()
    {
        var dt = new DataTable();
        dt.Columns.Add("foo", typeof(int));
        dt.Columns.Add("bar", typeof(object));
        var row = dt.NewRow();
        row.ItemArray = new object[] { 123, 456 };

        var context = new Context();
        context.CurrentObject.Set(row);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject["foo"], Is.EqualTo(123));
            Assert.That(context.CurrentObject["bar"], Is.EqualTo(456));
        });
    }

    [Test]
    public void Name_DictionaryWithUnavailableKey_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new Dictionary<string, object> { { "foo", 123 } });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject["foo"], Throws.Nothing);
            Assert.That(() => context.CurrentObject["bar"], Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void Name_AnonymousObjectWithUnavailableProperty_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new { foo = 123 });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject["foo"], Throws.Nothing);
            Assert.That(() => context.CurrentObject["bar"], Throws.TypeOf<ArgumentException>()
                .With.Message.StartsWith("Cannot find a property named 'bar' in the object of type '<>f__AnonymousType"));
        });
    }

    private record ObjectTest(string Name) { }
    [Test]
    public void Name_ObjectWithUnavailableProperty_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new ObjectTest("foo"));
        Assert.That(context.CurrentObject.Contains("Bar"), Is.False);
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject["Bar"], Throws.TypeOf<ArgumentException>()
                .With.Message.EqualTo("Cannot find a property named 'Bar' in the object of type 'ObjectTest'."));
        });
    }

    [Test]
    public void Name_ObjectNull_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(null);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("bar"), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject["bar"], Throws.TypeOf<ArgumentException>()
                .With.Message.EqualTo("Cannot find a property named 'bar' in the object of type 'null'."));
        });
    }

    [Test]
    public void Name_DataRowWithUnavailableColumn_ThrowsException()
    {
        var dt = new DataTable();
        dt.Columns.Add("foo", typeof(int));
        var row = dt.NewRow();
        row.ItemArray = new object[] { 123 };

        var context = new Context();
        context.CurrentObject.Set(row);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject["foo"], Throws.Nothing);
            Assert.That(() => context.CurrentObject["bar"], Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void Name_List_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<int> { 123 });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject.Contains("myVar"), Throws.TypeOf<NotNameableContextObjectException>());
            Assert.That(() => context.CurrentObject["myVar"], Throws.TypeOf<NotNameableContextObjectException>());
        });
    }

    [Test]
    [TestCase("foo", true)]
    [TestCase("bar", false)]
    public void NameTryGetValue_NameExisting_CorrectResult(string name, bool expected)
    {
        var context = new Context();
        context.CurrentObject.Set(new Dictionary<string, object> { { "foo", 123 } });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.TryGetValue(name, out var result), Is.EqualTo(expected));
            if (expected)
                Assert.That(result, Is.EqualTo(123));
            else
                Assert.That(result, Is.Null);
        });
    }

    [Test]
    public void Index_ListWithExistingIndex_ValueReturned()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<int>() { 123, 456 });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains(0), Is.True);
            Assert.That(context.CurrentObject.Contains(1), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject[0], Is.EqualTo(123));
            Assert.That(context.CurrentObject[1], Is.EqualTo(456));
        });
    }

    [Test]
    public void Index_DataRowWithExistingColumn_ValueReturned()
    {
        var dt = new DataTable();
        dt.Columns.Add("foo", typeof(int));
        dt.Columns.Add("bar", typeof(object));
        var row = dt.NewRow();
        row.ItemArray = new object[] { 123, 456 };

        var context = new Context();
        context.CurrentObject.Set(row);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains(0), Is.True);
            Assert.That(context.CurrentObject.Contains(1), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject[0], Is.EqualTo(123));
            Assert.That(context.CurrentObject[1], Is.EqualTo(456));
        });
    }

    [Test]
    public void Index_ListWithUnavailableIndex_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new List<int>() { 123 });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains(0), Is.True);
            Assert.That(context.CurrentObject.Contains(1), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject[0], Throws.Nothing);
            Assert.That(() => context.CurrentObject[1], Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void Index_DataRowWithUnavailableColumn_ThrowsException()
    {
        var dt = new DataTable();
        dt.Columns.Add("foo", typeof(int));
        var row = dt.NewRow();
        row.ItemArray = new object[] { 123 };

        var context = new Context();
        context.CurrentObject.Set(row);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains(0), Is.True);
            Assert.That(context.CurrentObject.Contains(1), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject[0], Throws.Nothing);
            Assert.That(() => context.CurrentObject[1], Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void Index_Object_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new { foo = 123 });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject.Contains(0), Throws.TypeOf<NotIndexableContextObjectException>());
            Assert.That(() => context.CurrentObject[0], Throws.TypeOf<NotIndexableContextObjectException>());
        });
    }

    [Test]
    public void Index_Dictionary_ThrowsException()
    {
        var context = new Context();
        context.CurrentObject.Set(new Dictionary<string, object> { { "foo", 123 }, { "bar", 456 } });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject.Contains(0), Throws.TypeOf<NotIndexableContextObjectException>());
            Assert.That(() => context.CurrentObject[0], Throws.TypeOf<NotIndexableContextObjectException>());
        });
    }

    [Test]
    [TestCase(0, true)]
    [TestCase(100, false)]
    public void IndexTryGetValue_IndexExisting_CorrectResult(int index, bool expected)
    {
        var context = new Context();
        context.CurrentObject.Set(new List<int>() { 123, 456 });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.TryGetValue(index, out var result), Is.EqualTo(expected));
            if (expected)
                Assert.That(result, Is.EqualTo(123));
            else
                Assert.That(result, Is.Null);
        });
    }

    [Test]
    public void Value_Any_CorrectResult()
    {
        var context = new Context();
        Assert.That(context.CurrentObject.Value, Is.Null);

        context.CurrentObject.Set(new List<int>() { 123, 456 });
        Assert.That(context.CurrentObject.Value, Is.AssignableTo<IList<int>>());
    }

    private class DataRowWrapper(DataRow row) : ILiteDataRow
    {
        private DataRow Row { get; } = row;

        public object? this[string columnName] => Row[columnName];

        public object? this[int index] => Row[index];

        public int ColumnCount => Row.Table.Columns.Count;

        public bool ContainsColumn(string columnName) => Row.Table.Columns.Contains(columnName);
    }

    [Test]
    public void Index_IReadOnlyDataRowWithUnavailableColumn_ThrowsException()
    {
        var dt = new DataTable();
        dt.Columns.Add("foo", typeof(int));
        var row = dt.NewRow();
        row.ItemArray = new object[] { 123 };
        dt.Rows.Add(row);
        var wrapper = new DataRowWrapper(row);

        var context = new Context();
        context.CurrentObject.Set(wrapper);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains(0), Is.True);
            Assert.That(context.CurrentObject.Contains(1), Is.False);
        });
        Assert.Multiple(() =>
        {
            Assert.That(() => context.CurrentObject[0], Throws.Nothing);
            Assert.That(() => context.CurrentObject[1], Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void Name_IReadOnlyDataRowWithExistingColumn_ValueReturned()
    {
        var dt = new DataTable();
        dt.Columns.Add("foo", typeof(int));
        dt.Columns.Add("bar", typeof(object));
        var row = dt.NewRow();
        row.ItemArray = new object[] { 123, 456 };
        dt.Rows.Add(row);
        var wrapper = new DataRowWrapper(row);

        var context = new Context();
        context.CurrentObject.Set(wrapper);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject.Contains("foo"), Is.True);
            Assert.That(context.CurrentObject.Contains("bar"), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentObject["foo"], Is.EqualTo(123));
            Assert.That(context.CurrentObject["bar"], Is.EqualTo(456));
        });
    }
}
