using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Record;

public class NestedFieldTest
{
    [Conformance]
    public void NestedField_Valid_Value(object? value, string expression, object? expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NestedField_Valid_Structured(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Conformance]
    public void NestedField_Valid_Array(object? value, string expression, decimal[] expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NestedField_Valid_Numeric(object? value, string expression, decimal expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [TestCase("nested-field()")]
    [TestCase("nested-field(...{})")]
    public void EmptyPath_ThrowsArgumentError(string source)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("at least one field name"));

    [TestCase("nested-field(42)")]
    [TestCase("nested-field(#null)")]
    [TestCase("nested-field({\"name\"})")]
    [TestCase("nested-field(...{\"missing\", 42})")]
    [TestCase("nested-field(...{name := \"value\"})")]
    public void NonTextSegment_ThrowsEvenForUnresolvedInput(string source)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("must be text"));

    [TestCase("nested-field(...#null)")]
    [TestCase("nested-field(...42)")]
    [TestCase("nested-field(...\"name\")")]
    public void UnsupportedSpread_UsesSharedError(string source)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<SpreadArgumentException>());

    [Test]
    public void NamedArgument_IsRejectedDuringBinding()
        => Assert.That(
            () => Expression.Create("nested-field(path := \"name\")"),
            Throws.InstanceOf<BindingException>());

    [TestCase("name")]
    [TestCase("Name")]
    [TestCase("missing")]
    public void SingleSegment_MatchesFieldOnObjects(string name)
    {
        var input = new { Name = "Ada" };
        var function = new NestedField(() => [new(_ => name)]);
        Assert.That(function.Evaluate(input), Is.EqualTo(new Field(() => name).Evaluate(input)));
    }

    [Test]
    public void PathArguments_UseOriginalInputInOrderAndPreserveSelectedValue()
    {
        var selected = new RecordValue();
        selected.Set("value", 42);
        var input = new RecordValue();
        input.Set("child", selected);
        var seen = new List<object?>();
        var function = new NestedField(() =>
        [
            new(value => { seen.Add(value); return "child"; }),
            new(value => { seen.Add(value); return System.Array.Empty<object?>(); }, true),
        ]);

        Assert.That(function.Evaluate(input), Is.SameAs(selected));
        Assert.That(seen, Is.EqualTo(new object?[] { input, input }));
    }

    [Test]
    public void BoundExpression_CanBeReusedConcurrently()
    {
        var expression = Expression.Create("nested-field(.path, \"value\")");
        Parallel.For(0, 20, i =>
        {
            var input = new RecordValue();
            var child = new RecordValue();
            child.Set("value", i);
            input.Set("path", $"child{i}");
            input.Set($"child{i}", child);
            Assert.That(expression.Evaluate(input), Is.EqualTo(i));
        });
    }

    [Test]
    public void Introspection_DescribesVariadicTextPathAndDynamicOutput()
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == "nested-field");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.ImplementationType, Is.EqualTo(typeof(NestedField)));
            Assert.That(info.Input, Is.EqualTo("any"));
            Assert.That(info.Output, Is.EqualTo("any"));
            Assert.That(info.Converted, Is.False);
            Assert.That(info.Reason, Is.EqualTo("Output depends on the value selected by the runtime field path."));
            Assert.That(info.Parameters.Single().Name, Is.EqualTo("path"));
            Assert.That(info.Parameters.Single().Type, Is.EqualTo("text"));
            Assert.That(info.Parameters.Single().Variadic, Is.True);
            Assert.That(info.Parameters.Single().MinimumCardinality, Is.EqualTo(1));
        }
    }
}
