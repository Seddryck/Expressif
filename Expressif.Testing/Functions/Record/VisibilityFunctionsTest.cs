using Expressif.Functions;
using Expressif.Functions.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Record;

public class VisibilityFunctionsTest
{
    [Conformance]
    public void SetPublic_Valid_Record(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Conformance]
    public void SetPrivate_Valid_Record(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Conformance]
    public void SetPublic_Invalid_Collision(object? value, string expression, string expected)
        => Assert.That(() => Expression.Create(expression).Evaluate(value),
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo(expected));

    [Conformance]
    public void SetPrivate_Invalid_Collision(object? value, string expression, string expected)
        => Assert.That(() => Expression.Create(expression).Evaluate(value),
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo(expected));

    [TestCase("public", 0)]
    [TestCase("set-public", 1)]
    [TestCase("set-private", 1)]
    public void Visibility_DeclaresTypedContractAndOptionalNames(string name, int parameterCount)
    {
        var info = new Expressif.Functions.Introspection.FunctionIntrospector().Describe().Single(x => x.Name == name);
        Assert.Multiple(() =>
        {
            Assert.That(info.Input, Is.EqualTo("record"));
            Assert.That(info.Output, Is.EqualTo("record"));
            Assert.That(info.Converted, Is.True);
            Assert.That(info.Parameters, Has.Length.EqualTo(parameterCount));
        });
        if (parameterCount > 0)
        {
            Assert.Multiple(() =>
            {
                Assert.That(info.Parameters[0].Name, Is.EqualTo("names"));
                Assert.That(info.Parameters[0].Type, Is.EqualTo("array"));
                Assert.That(info.Parameters[0].Optional, Is.True);
            });
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Visibility_TypedRename_PreservesInputOrderAndReferences(bool makePublic)
    {
        var nested = new RecordValue();
        nested.Set("_secret", 1);
        var input = new RecordValue();
        input.Set(makePublic ? "_nested" : "nested", nested);
        input.Set("unaffected", null);
        IFunction<RecordValue, RecordValue> function = makePublic
            ? new SetPublic(() => ["nested"])
            : new SetPrivate(() => ["nested"]);
        var result = function.Evaluate(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(input));
            Assert.That(result.Keys, Is.EqualTo(new[] { makePublic ? "nested" : "_nested", "unaffected" }));
            Assert.That(result[0], Is.SameAs(nested));
            Assert.That(result[1], Is.Null);
            Assert.That(input.Keys, Is.EqualTo(new[] { makePublic ? "_nested" : "nested", "unaffected" }));
        });
    }

    [TestCase("set-public({1})")]
    [TestCase("set-private({#null})")]
    public void Visibility_InvalidNames_ThrowsClearError(string source)
        => Assert.That(() => Expression.Create(source).Evaluate(new RecordValue()),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Every field name must be text."));

    [TestCase("set-public")]
    [TestCase("set-private")]
    public void Visibility_Collision_DoesNotMutateInput(string name)
    {
        var input = new RecordValue();
        input.Set("first", 1);
        input.Set("_id", 42);
        input.Set("id", "ABC");
        Assert.That(() => Expression.Create(name).Evaluate(input), Throws.TypeOf<InvalidOperationException>());
        Assert.That(input.Keys, Is.EqualTo(new[] { "first", "_id", "id" }));
        Assert.That(input["_id"], Is.EqualTo(42));
        Assert.That(input["id"], Is.EqualTo("ABC"));
    }
    [Conformance]
    public void Public_Valid_Record(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Test]
    public void Public_TypedProjection_PreservesInputAndValueIdentity()
    {
        var nested = new RecordValue();
        nested.Set("_secret", 1);
        var input = new RecordValue();
        input.Set("nested", nested);
        input.Set("_private", 2);
        IFunction<RecordValue, RecordValue> function = new Public();
        var result = function.Evaluate(input);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(input));
            Assert.That(result["nested"], Is.SameAs(nested));
            Assert.That(input.ContainsKey("_private"), Is.True);
        });
    }
}
