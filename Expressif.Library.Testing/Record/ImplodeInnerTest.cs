using Expressif.Introspection;
using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Discovery;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Record;

public class ImplodeInnerTest
{
    [Conformance]
    public void ImplodeInner_Valid_Rows(object? value, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(value)), Is.EqualTo(expected));

    [Conformance]
    public void ImplodeInner_Valid_Null(object? value, string expression, object? expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [TestCase("implode-inner")]
    public void InvalidSelectors_FailBinding(string name)
    {
        foreach (var arguments in new[] { "", "42", "\"tags\"", ".tags, .other", "unknown := .tags", ".tags | first", ".child.tags", "...{\"tags\"}" })
            Assert.That(() => TestExpression.Create($"{name}({arguments})"), Throws.InstanceOf<BindingException>(), arguments);
    }

    [TestCase("implode-inner")]
    public void InvalidInputs_FailEvaluation(string name)
    {
        foreach (var input in new[] { "42", "\"text\"", "{id := 1, tags := 2}", "{42}", "{#null}", "T(1, 2)" })
            Assert.That(() => TestExpression.Create($"{input} | {name}(.tags)").Evaluate(null), Throws.TypeOf<ArgumentException>(), input);
    }

    [TestCase("implode-inner")]
    public void Introspection_ExposesClosedArrayContract(string name)
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(info => info.Name == name);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Input, Is.EqualTo("array"));
            Assert.That(info.Output, Is.EqualTo("array"));
            Assert.That(info.Parameters.Single().Name, Is.EqualTo("selector"));
            Assert.That(info.Parameters.Single().Type, Is.EqualTo("expression"));
        }
    }

    [Test]
    public void TypedEvaluation_VisitsOnceAndRetainsNullOnlyParents()
    {
        var nested = new RecordValue();
        nested.Set("id", 1);
        var rows = new[] { new RecordValue(), new RecordValue() };
        rows[0].Set("parent", nested);
        rows[0].Set("tags", DBNull.Value);
        rows[1].Set("parent", nested);
        rows[1].Set("tags", null);
        var visits = new List<object?>();
        IFunction<System.Collections.IEnumerable, RecordValue[]> function = new ImplodeInner(new NamedFieldSelector("tags", value =>
        {
            visits.Add(value);
            return ((RecordValue)value!)["tags"];
        }));
        var result = function.Evaluate(rows);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(visits, Is.EqualTo(rows));
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0]["tags"], Is.Empty);
            Assert.That(result[0]["parent"], Is.SameAs(nested));
            Assert.That(rows[0]["tags"], Is.SameAs(DBNull.Value));
            Assert.That(rows[1]["tags"], Is.Null);
        }
    }

    [TestCase("implode-inner")]
    public void BoundExpression_CanBeReusedConcurrently(string name)
    {
        var expression = TestExpression.Create($"{name}(.tags)");
        Parallel.For(0, 20, i =>
        {
            var parent = new RecordValue();
            parent.Set("tags", i);
            var rows = (RecordValue[])expression.Evaluate(new[] { parent })!;
            Assert.That(rows.Single()["tags"], Is.EqualTo(new[] { i }));
        });
    }
}
