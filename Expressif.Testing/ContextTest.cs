using Expressif.Values;
using Expressif.Values.Special;
using NUnit.Framework.Constraints;
using System.Data;

namespace Expressif.Testing;

public class ContextTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("foo")]
    [TestCase("@foo")]
    public void VariableAdd_OneVariable_CanBeRetrieved(string name)
    {
        var context = new Context();
        Assert.That(context.Variables, Has.Count.EqualTo(0));
        context.Variables.Add<string>(name, "123");
        Assert.That(context.Variables, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(context.Variables.Contains(name), Is.True);
            Assert.That(context.Variables.Contains(name.Replace("@", "")), Is.True);
        });
    }

    [Test]
    [TestCase("foo", "foo")]
    [TestCase("@foo", "@foo")]
    [TestCase("@foo", "foo")]
    [TestCase("foo", "@foo")]
    public void VariableAdd_TwiceTheSameVariable_ThrowException(string name1, string name2)
    {
        var context = new Context();
        context.Variables.Add<string>(name1, "123");
        Assert.That(() => context.Variables.Add<string>(name2, "456"), Throws.TypeOf<VariableAlreadyExistingException>());
    }

    [Test]
    [TestCase("foo", "foo")]
    [TestCase("@foo", "@foo")]
    [TestCase("@foo", "foo")]
    [TestCase("foo", "@foo")]
    public void VariableSet_TwiceTheSameVariable_FinalValue(string name1, string name2)
    {
        var context = new Context();
        Assert.That(context.Variables, Has.Count.EqualTo(0));
        context.Variables.Set(name1, "123");
        Assert.That(context.Variables, Has.Count.EqualTo(1));
        context.Variables.Set(name2, "456");
        Assert.That(context.Variables, Has.Count.EqualTo(1));
        Assert.That(context.Variables[name1], Is.EqualTo("456"));
    }

    [Test]
    [TestCase("foo", "foo")]
    [TestCase("@foo", "@foo")]
    [TestCase("@foo", "foo")]
    [TestCase("foo", "@foo")]
    public void VariableRemove_TwiceTheSameVariable_NoVariableRemaining(string name1, string name2)
    {
        var context = new Context();
        Assert.That(context.Variables, Has.Count.EqualTo(0));
        context.Variables.Set(name1, "123");
        Assert.That(context.Variables, Has.Count.EqualTo(1));
        context.Variables.Remove(name2);
        Assert.That(context.Variables, Has.Count.EqualTo(0));
    }

    [Test]
    [TestCase("foo", true)]
    [TestCase("@foo", true)]
    [TestCase("@bar", false)]
    [TestCase("bar", false)]
    public void VariableContains_FooExisting_CorrectResult(string name, bool expected)
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.That(context.Variables.Contains(name), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("foo", true)]
    [TestCase("@foo", true)]
    [TestCase("@bar", false)]
    [TestCase("bar", false)]
    public void VariableTryGetValue_FooExisting_CorrectResult(string name, bool expected)
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.Multiple(() =>
        {
            Assert.That(context.Variables.TryGetValue(name, out var result), Is.EqualTo(expected));
            if (expected)
                Assert.That(result, Is.EqualTo("123"));
            else
                Assert.That(result, Is.Null);
        });
    }

    [Test]
    public void VariableCount_AddRemove_CorrectResult()
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.That(context.Variables.Count, Is.EqualTo(1));

        context.Variables.Add<string>("bar", "456");
        Assert.That(context.Variables.Count, Is.EqualTo(2));

        context.Variables.Remove("foo");
        Assert.That(context.Variables.Count, Is.EqualTo(1));
        context.Variables.Remove("bar");
        Assert.That(context.Variables.Count, Is.EqualTo(0));
    }

    [Test]
    public void VariableKeys_AddRemove_CorrectResult()
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.Multiple(() =>
        {
            Assert.That(context.Variables.Keys, Does.Contain("foo"));
            Assert.That(context.Variables.Keys, Does.Not.Contain("bar"));
        });

        context.Variables.Add<string>("bar", "456");
        Assert.Multiple(() =>
        {
            Assert.That(context.Variables.Keys, Does.Contain("foo"));
            Assert.That(context.Variables.Keys, Does.Contain("bar"));
        });

        context.Variables.Remove("foo");
        Assert.Multiple(() =>
        {
            Assert.That(context.Variables.Keys, Does.Not.Contain("foo"));
            Assert.That(context.Variables.Keys, Does.Contain("bar"));
        });

        context.Variables.Remove("bar");
        Assert.Multiple(() =>
        {
            Assert.That(context.Variables.Keys, Does.Not.Contain("foo"));
            Assert.That(context.Variables.Keys, Does.Not.Contain("bar"));
        });
    }

    [Test]
    public void CurrentObjectName_DictionaryWithExistingKey_KeyReturned()
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
    public void CurrentObjectName_ObjectWithExistingProperty_ValueReturned()
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
    public void CurrentObjectName_DataRowWithExistingColumn_ValueReturned()
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
    public void CurrentObjectName_DictionaryWithUnavailableKey_ThrowsException()
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
    public void CurrentObjectName_AnonymousObjectWithUnavailableProperty_ThrowsException()
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
                .With.Message.EqualTo("Cannot find a property named 'bar' in the object of type '<>f__AnonymousType1`1'."));
        });
    }

    private record ObjectTest(string Name) { }
    [Test]
    public void CurrentObjectName_ObjectWithUnavailableProperty_ThrowsException()
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
    public void CurrentObjectName_ObjectNull_ThrowsException()
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
    public void CurrentObjectName_DataRowWithUnavailableColumn_ThrowsException()
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
    public void CurrentObjectName_List_ThrowsException()
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
    public void CurrentObjectNameTryGetValue_NameExisting_CorrectResult(string name, bool expected)
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
    public void CurrentObjectIndex_ListWithExistingIndex_ValueReturned()
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
    public void CurrentObjectIndex_DataRowWithExistingColumn_ValueReturned()
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
    public void CurrentObjectIndex_ListWithUnavailableIndex_ThrowsException()
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
    public void CurrentObjectIndex_DataRowWithUnavailableColumn_ThrowsException()
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
    public void CurrentObjectIndex_Object_ThrowsException()
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
    public void CurrentObjectIndex_Dictionary_ThrowsException()
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
    public void CurrentObjectIndexTryGetValue_IndexExisting_CorrectResult(int index, bool expected)
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
    public void CurrentObjectValue_Any_CorrectResult()
    {
        var context = new Context();
        Assert.That(context.CurrentObject.Value, Is.Null);

        context.CurrentObject.Set(new List<int>() { 123, 456 });
        Assert.That(context.CurrentObject.Value, Is.AssignableTo<IList<int>>());
    }

    private class DataRowWrapper(DataRow row) : IReadOnlyDataRow
    {
        private DataRow Row { get; } = row;

        public object? this[string columnName] => Row[columnName];

        public object? this[int index] => Row[index];

        public int ColumnsCount => Row.Table.Columns.Count;

        public bool ContainsColumn(string columnName) => Row.Table.Columns.Contains(columnName);
    }

    [Test]
    public void CurrentObjectIndex_IReadOnlyDataRowWithUnavailableColumn_ThrowsException()
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
    public void CurrentObjectName_IReadOnlyDataRowWithExistingColumn_ValueReturned()
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
