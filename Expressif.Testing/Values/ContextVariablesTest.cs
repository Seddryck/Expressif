using Expressif.Values;
using Expressif.Values.Special;
using NUnit.Framework.Constraints;
using System.Data;

namespace Expressif.Testing.Values;

public class ContextVariablesTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("foo")]
    [TestCase("@foo")]
    public void Add_OneVariable_CanBeRetrieved(string name)
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
    public void Add_TwiceTheSameVariable_ThrowException(string name1, string name2)
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
    public void Set_TwiceTheSameVariable_FinalValue(string name1, string name2)
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
    public void Remove_TwiceTheSameVariable_NoVariableRemaining(string name1, string name2)
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
    public void Contains_FooExisting_CorrectResult(string name, bool expected)
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.That(context.Variables.Contains(name), Is.EqualTo(expected));
    }

    [Test]
    public void Get_FooExisting_CorrectResult()
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.That(context.Variables["foo"], Is.EqualTo("123"));
    }

    [Test]
    public void Get_FooExistingAsFunction_CorrectResult()
    {
        var context = new Context(new() { { "foo", () => 123 } });
        Assert.That(context.Variables["foo"], Is.EqualTo(123));
    }

    [Test]
    public void Get_BarNotExisting_CorrectResult()
    {
        var context = new Context(new() { { "foo", "123" } });
        Assert.That(() => context.Variables["bar"], Throws.InstanceOf<UnexpectedVariableException>());
    }

    [Test]
    [TestCase("foo", true)]
    [TestCase("@foo", true)]
    [TestCase("@bar", false)]
    [TestCase("bar", false)]
    public void TryGetValue_FooExisting_CorrectResult(string name, bool expected)
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
    public void Count_AddRemove_CorrectResult()
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
    public void Keys_AddRemove_CorrectResult()
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
}
