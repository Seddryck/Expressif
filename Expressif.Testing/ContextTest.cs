using Expressif.Values;
using Expressif.Values.Special;
using System.Data;

namespace Expressif
{
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
                Assert.That(context.Variables[name], Is.EqualTo("123"));
                Assert.That(context.Variables[name.Replace("@", "")], Is.EqualTo("123"));
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
            context.Variables.Set<string>(name1, "123");
            Assert.That(context.Variables, Has.Count.EqualTo(1));
            context.Variables.Set<string>(name2, "456");
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
            context.Variables.Set<string>(name1, "123");
            Assert.That(context.Variables, Has.Count.EqualTo(1));
            context.Variables.Remove(name2);
            Assert.That(context.Variables, Has.Count.EqualTo(0));
        }

        [Test]
        public void CurrentObjectName_DictionaryWithExistingKey_KeyReturned()
        {
            var context = new Context();
            context.CurrentObject.Set(new Dictionary<string, object>{ { "foo", 123 }, { "bar", 456 } });
            Assert.Multiple(() =>
            {
                Assert.That(context.CurrentObject.Exists("foo"), Is.True);
                Assert.That(context.CurrentObject.Exists("bar"), Is.True);
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
                Assert.That(context.CurrentObject.Exists("foo"), Is.True);
                Assert.That(context.CurrentObject.Exists("bar"), Is.True);
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
                Assert.That(context.CurrentObject.Exists("foo"), Is.True);
                Assert.That(context.CurrentObject.Exists("bar"), Is.True);
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
                Assert.That(context.CurrentObject.Exists("foo"), Is.True);
                Assert.That(context.CurrentObject.Exists("bar"), Is.False);
            });
            Assert.Multiple(() =>
            {
                Assert.That(() => context.CurrentObject["foo"], Throws.Nothing);
                Assert.That(() => context.CurrentObject["bar"], Throws.TypeOf<ArgumentOutOfRangeException>());
            });
        }

        [Test]
        public void CurrentObjectName_ObjectWithUnavailableProperty_ThrowsException()
        {
            var context = new Context();
            context.CurrentObject.Set(new { foo = 123 });
            Assert.Multiple(() =>
            {
                Assert.That(context.CurrentObject.Exists("foo"), Is.True);
                Assert.That(context.CurrentObject.Exists("bar"), Is.False);
            });
            Assert.Multiple(() =>
            {
                Assert.That(() => context.CurrentObject["foo"], Throws.Nothing);
                Assert.That(() => context.CurrentObject["bar"], Throws.TypeOf<ArgumentOutOfRangeException>());
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
                Assert.That(context.CurrentObject.Exists("foo"), Is.True);
                Assert.That(context.CurrentObject.Exists("bar"), Is.False);
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
            context.CurrentObject.Set(new List<int>{ 123 });
            Assert.Multiple(() =>
            {
                Assert.That(() => context.CurrentObject.Exists("myVar"), Throws.TypeOf<NotNameableContextObjectException>());
                Assert.That(() => context.CurrentObject["myVar"], Throws.TypeOf<NotNameableContextObjectException>());
            });
        }

        [Test]
        public void CurrentObjectIndex_ListWithExistingIndex_ValueReturned()
        {
            var context = new Context();
            context.CurrentObject.Set(new List<int>() { 123 , 456 });
            Assert.Multiple(() =>
            {
                Assert.That(context.CurrentObject.Exists(0), Is.True);
                Assert.That(context.CurrentObject.Exists(1), Is.True);
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
                Assert.That(context.CurrentObject.Exists(0), Is.True);
                Assert.That(context.CurrentObject.Exists(1), Is.True);
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
                Assert.That(context.CurrentObject.Exists(0), Is.True);
                Assert.That(context.CurrentObject.Exists(1), Is.False);
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
                Assert.That(context.CurrentObject.Exists(0), Is.True);
                Assert.That(context.CurrentObject.Exists(1), Is.False);
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
            context.CurrentObject.Set(new { foo=123 });
            Assert.Multiple(() =>
            {
                Assert.That(() => context.CurrentObject.Exists(0), Throws.TypeOf<NotIndexableContextObjectException>());
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
                Assert.That(() => context.CurrentObject.Exists(0), Throws.TypeOf<NotIndexableContextObjectException>());
                Assert.That(() => context.CurrentObject[0], Throws.TypeOf<NotIndexableContextObjectException>());
            });
        }

    }
}