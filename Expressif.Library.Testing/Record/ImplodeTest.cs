using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Introspection;
using Expressif.Library.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Record;

public class ImplodeTest
{
    [Conformance]
    public void Implode_Valid_Rows(object? value, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(value)), Is.EqualTo(expected));

    [Conformance]
    public void Implode_Valid_Null(object? value, string expression, object? expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void InvalidSelectors_FailBinding()
    {
        foreach (var arguments in new[] { "", "42", "\"tags\"", ".tags, .other", "unknown := .tags", ".tags | first", ".child.tags", "...{\"tags\"}" })
            Assert.That(() => TestExpression.Create($"implode({arguments})"), Throws.InstanceOf<BindingException>(), arguments);
    }

    [Test]
    public void InvalidInputs_FailEvaluation()
    {
        foreach (var input in new[] { "42", "\"text\"", "{id := 1, tags := 2}", "{42}", "{#null}", "T(1, 2)" })
            Assert.That(() => TestExpression.Create($"{input} | implode(.tags)").Evaluate(null), Throws.TypeOf<ArgumentException>(), input);
    }

    [Test]
    public void Introspection_ExposesClosedArrayContract()
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(info => info.Name == "implode");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.ImplementationType, Is.EqualTo(typeof(Implode)));
            Assert.That(info.Input, Is.EqualTo("array"));
            Assert.That(info.Output, Is.EqualTo("array"));
            Assert.That(info.Parameters.Single().Name, Is.EqualTo("selector"));
            Assert.That(info.Parameters.Single().Type, Is.EqualTo("expression"));
        }
    }

    [Test]
    public void TypedEvaluation_VisitsOnceAndPreservesNulls()
    {
        var rows = new[] { new RecordValue(), new RecordValue() };
        rows[0].Set("id", 1);
        rows[0].Set("tags", DBNull.Value);
        rows[1].Set("id", 1);
        rows[1].Set("tags", null);
        var visits = new List<object?>();
        IFunction<System.Collections.IEnumerable, RecordValue[]> function = new Implode(new NamedFieldSelector("tags", value =>
        {
            visits.Add(value);
            return ((RecordValue)value!)["tags"];
        }));

        var result = function.Evaluate(rows);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(visits, Is.EqualTo(rows));
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0]["tags"], Is.EqualTo(new object?[] { DBNull.Value, null }));
            Assert.That(rows[0]["tags"], Is.SameAs(DBNull.Value));
            Assert.That(rows[1]["tags"], Is.Null);
        }
    }

    [Test]
    public void BoundExpression_CanBeReusedConcurrently()
    {
        var expression = TestExpression.Create("implode(.tags)");
        Parallel.For(0, 20, i =>
        {
            var parent = new RecordValue();
            parent.Set("tags", i);
            var rows = (RecordValue[])expression.Evaluate(new[] { parent })!;
            Assert.That(rows.Single()["tags"], Is.EqualTo(new[] { i }));
        });
    }
}
