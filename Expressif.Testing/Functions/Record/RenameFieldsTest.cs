using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Record;

public class RenameFieldsTest
{
    [Conformance]
    public void RenameFields_Valid_Record(object? value, string expression, string expected)
        => Assert.That(Expression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Conformance]
    public void RenameFields_Invalid_Collision(object? value, string expression, string expected)
        => Assert.That(() => Expression.Create(expression).Evaluate(value),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains($"duplicate field name '{expected}'"));

    [TestCase("rename-fields()")]
    [TestCase("rename-fields(lower, contains(\"-\"), lower)")]
    [TestCase("rename-fields(unknown := lower)")]
    public void InvalidArguments_FailBinding(string expression)
        => Assert.That(() => Expression.Create(expression), Throws.InstanceOf<BindingException>());

    [TestCase("42")]
    [TestCase("#null")]
    [TestCase("length")]
    public void NonTextResult_FailsEvaluation(string transform)
        => Assert.That(() => Expression.Create($"{{name := 1}} | rename-fields({transform})").Evaluate(null),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("must return text"));

    [Test]
    public void NonBooleanFilter_FailsEvaluation()
        => Assert.That(() => Expression.Create("{name := 1} | rename-fields(lower, length)").Evaluate(null),
            Throws.TypeOf<InvalidCastException>().With.Message.Contains("Boolean"));

    [Test]
    public void Transformation_DoesNotReadFieldsFromTheOuterRecord()
        => Assert.That(() => Expression.Create("{suffix := \"outer\"} | rename-fields(.suffix)").Evaluate(null),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("must return text"));

    [Test]
    public void RejectedFields_DoNotEvaluateTransformation()
        => Assert.That(Expression.Create("{name := 1} | rename-fields(42, contains(\"-\"))").Evaluate(null)?.ToString(),
            Is.EqualTo("{name := 1}"));

    [Test]
    public void TypedEvaluation_PreservesValuesOrderAndInput()
    {
        var nested = new RecordValue();
        nested.Set("child", 42);
        var input = new RecordValue();
        input.Set("FIRST", nested);
        input.Set("SECOND", null);
        IFunction<RecordValue, RecordValue> function = new RenameFields(() => Expression.Create("lower"));

        var result = function.Evaluate(input);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Keys, Is.EqualTo(new[] { "first", "second" }));
            Assert.That(result["first"], Is.SameAs(nested));
            Assert.That(result["second"], Is.Null);
            Assert.That(input.Keys, Is.EqualTo(new[] { "FIRST", "SECOND" }));
        }
    }

    [Test]
    public void FieldNames_AreCaseSensitive()
        => Assert.That(Expression.Create("{foo := 1, Foo := 2} | rename-fields(trim)").Evaluate(null)?.ToString(),
            Is.EqualTo("{foo := 1, Foo := 2}"));

    [Test]
    public void EmptyText_IsAValidFieldName()
    {
        var result = (RecordValue)Expression.Create("{name := 1} | rename-fields(\"\")").Evaluate(null)!;
        Assert.That(result.Keys, Is.EqualTo(new[] { string.Empty }));
    }

    [Test]
    public void BoundExpression_CanBeReusedConcurrently()
    {
        var expression = Expression.Create("rename-fields(trim | lower, contains(\"FIELD\"))");
        Parallel.For(0, 30, i =>
        {
            var input = new RecordValue();
            input.Set($" FIELD{i} ", i);
            input.Set("OTHER", null);
            var result = (RecordValue)expression.Evaluate(input)!;
            Assert.That(result.Keys, Is.EqualTo(new[] { $"field{i}", "OTHER" }));
            Assert.That(result[$"field{i}"], Is.EqualTo(i));
        });
    }

    [Test]
    public void Introspection_DescribesClosedContractAndOptionalFilter()
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == "rename-fields");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.ImplementationType, Is.EqualTo(typeof(RenameFields)));
            Assert.That(info.Input, Is.EqualTo("record"));
            Assert.That(info.Output, Is.EqualTo("record"));
            Assert.That(info.Parameters.Select(parameter => parameter.Name), Is.EqualTo(new[] { "transform", "filter" }));
            Assert.That(info.Parameters.Select(parameter => parameter.Type), Is.EqualTo(new[] { "expression", "predicate" }));
            Assert.That(info.Parameters.Select(parameter => parameter.Optional), Is.EqualTo(new[] { false, true }));
        }
    }
}
