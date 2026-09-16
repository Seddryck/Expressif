using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Tuple;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class LabelTest
{
    [Conformance]
    public void Label_Valid(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void LabelConflicts_Valid(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("T(1, 2) | label(\"only\")")]
    [TestCase("T(1) | label(\"one\", \"two\")")]
    [TestCase("T(1, 2) | label-conflicts(\"only\")")]
    public void LabelCount_MustMatchTupleArity(string source)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("must match the tuple arity"));

    [TestCase("T(1) | label(42)")]
    [TestCase("T(1) | label(#null)")]
    [TestCase("T(1) | label-conflicts(42)")]
    public void Labels_MustBeText(string source)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("must be text"));

    [TestCase("T(1, 2) | label(\"same\", \"same\")")]
    [TestCase("T(1, 2) | label-conflicts(\"same\", \"same\")")]
    [TestCase("T({key := 1}, {key := 2}) | label-conflicts(\"side\", \"side\")")]
    public void DuplicateFinalFieldNames_AreRejected(string source)
        => Assert.That(
            () => Expression.Create(source).Evaluate(null),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("duplicate field"));

    [TestCase("label(\"value\")")]
    [TestCase("label-conflicts(\"value\")")]
    public void NonPositionalInput_ReturnsNull(string operation)
        => Assert.That(Expression.Create(operation).Evaluate(42), Is.Null);

    [Test]
    public void Labels_AreEvaluatedAgainstOriginalTupleInOrder()
    {
        var input = new TupleValue("left", "right");
        var seen = new List<object?>();
        var function = new Label(() =>
        [
            new(value => { seen.Add(value); return "first"; }),
            new(value => { seen.Add(value); return "second"; }),
        ]);

        var result = function.Evaluate(input);

        Assert.Multiple(() =>
        {
            Assert.That(seen, Is.EqualTo(new object?[] { input, input }));
            Assert.That(result.Fields, Is.EqualTo(new[] { "first", "second" }));
            Assert.That(result["first"], Is.EqualTo("left"));
            Assert.That(result["second"], Is.EqualTo("right"));
        });
    }

    [Test]
    public void Introspection_ReportsVariadicTextLabelsAndRecordOutput()
    {
        var infos = new FunctionIntrospector().Describe()
            .Where(info => info.Name is "label" or "label-conflicts")
            .OrderBy(info => info.Name)
            .ToArray();

        Assert.That(infos, Has.Length.EqualTo(2));
        foreach (var info in infos)
        {
            Assert.Multiple(() =>
            {
                Assert.That(info.Input, Is.EqualTo("tuple"));
                Assert.That(info.Output, Is.EqualTo("record"));
                Assert.That(info.Parameters, Has.Length.EqualTo(1));
                Assert.That(info.Parameters[0].Name, Is.EqualTo("labels"));
                Assert.That(info.Parameters[0].Type, Is.EqualTo("text"));
                Assert.That(info.Parameters[0].Variadic, Is.True);
                Assert.That(info.Parameters[0].MinimumCardinality, Is.Zero);
            });
        }
    }

    [Test]
    public void BoundExpression_CanBeReusedConcurrently()
    {
        var expression = Expression.Create("label(\"left\", \"right\")");
        Parallel.For(0, 20, i =>
        {
            var result = (RecordValue)expression.Evaluate(new TupleValue(i, -i))!;
            Assert.Multiple(() =>
            {
                Assert.That(result["left"], Is.EqualTo(i));
                Assert.That(result["right"], Is.EqualTo(-i));
            });
        });
    }
}
