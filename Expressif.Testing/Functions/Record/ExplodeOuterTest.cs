using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Functions.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Record;

public class ExplodeOuterTest
{
    [Conformance]
    public void ExplodeOuter_Valid_Rows(object? value, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(value)), Is.EqualTo(expected));

    [Conformance]
    public void ExplodeOuter_Valid_Null(object? value, string expression, object? expected)
        => Assert.That(Expression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [TestCase("explode-outer()")]
    [TestCase("explode-outer(.tags, .other)")]
    [TestCase("explode-outer(unknown := .tags)")]
    [TestCase("explode-outer(42)")]
    [TestCase("explode-outer(\"tags\")")]
    [TestCase("explode-outer(.tags | first)")]
    [TestCase("explode-outer(.child.tags)")]
    [TestCase("explode-outer(...{\"tags\"})")]
    public void InvalidSelector_FailsBinding(string source)
        => Assert.That(() => Expression.Create(source), Throws.InstanceOf<BindingException>());

    [TestCase("42")]
    [TestCase("\"A\"")]
    [TestCase("\"{1, 2}\"")]
    [TestCase("{name := 1}")]
    [TestCase("T(1, 2)")]
    public void NonCollectionField_FailsEvaluation(string selected)
        => Assert.That(() => Expression.Create($"{{tags := {selected}}} | explode-outer(.tags)").Evaluate(null),
            Throws.TypeOf<ArgumentException>());

    [TestCase("42")]
    [TestCase("{42}")]
    [TestCase("{#null}")]
    [TestCase("T(1, 2)")]
    public void NonRecordParent_FailsEvaluation(string source)
        => Assert.That(() => Expression.Create($"{source} | explode-outer(.tags)").Evaluate(null),
            Throws.TypeOf<ArgumentException>());

    [Test]
    public void TypedEvaluation_PreservesReferencesOrderAndInput()
    {
        var other = new RecordValue();
        other.Set("nested", 42);
        var children = new object?[] { other, null, other };
        var parent = new RecordValue();
        parent.Set("before", other);
        parent.Set("tags", children);
        parent.Set("after", other);
        var seen = new List<object?>();
        IFunction<RecordValue, RecordValue[]> function = new ExplodeOuter(new NamedFieldSelector("tags", value =>
        {
            seen.Add(value);
            return ((RecordValue)value!)["tags"];
        }));

        var rows = function.Evaluate(parent);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(seen, Is.EqualTo(new[] { parent }));
            Assert.That(rows, Has.Length.EqualTo(3));
            Assert.That(rows.Select(row => row.Keys), Is.All.EqualTo(new[] { "before", "tags", "after" }));
            Assert.That(rows[0]["tags"], Is.SameAs(other));
            Assert.That(rows[1]["tags"], Is.Null);
            Assert.That(rows[2]["tags"], Is.SameAs(other));
            Assert.That(rows.Select(row => row["before"]), Is.All.SameAs(other));
            Assert.That(parent["tags"], Is.SameAs(children));
            Assert.That(rows[0], Is.Not.SameAs(rows[2]));
        }
    }

    [Test]
    public void SupportedEnumerables_AreVisitedOnceInParentOrder()
    {
        var visits = new List<string>();
        IEnumerable<object?> Children(string name)
        {
            visits.Add(name);
            yield return name;
        }
        var first = new RecordValue();
        first.Set("tags", Children("first"));
        var second = new RecordValue();
        second.Set("tags", Children("second"));
        IFunction<System.Collections.IEnumerable, RecordValue[]> function = new ExplodeOuter(
            new NamedFieldSelector("tags", value => ((RecordValue)value!)["tags"]));

        var rows = function.Evaluate(new[] { first, second });
        Assert.That(visits, Is.EqualTo(new[] { "first", "second" }));
        Assert.That(rows.Select(row => row["tags"]), Is.EqualTo(visits));
    }

    [Test]
    public void BoundExpression_CanBeReusedConcurrently()
    {
        var expression = Expression.Create("explode-outer(.tags)");
        Parallel.For(0, 20, i =>
        {
            var input = new RecordValue();
            input.Set("tags", new[] { i });
            var rows = (RecordValue[])expression.Evaluate(input)!;
            Assert.That(rows.Single()["tags"], Is.EqualTo(i));
        });
    }

    [Test]
    public void Introspection_ExposesRecordAndArrayContracts()
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == "explode-outer");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.ImplementationType, Is.EqualTo(typeof(ExplodeOuter)));
            Assert.That(info.Input, Is.EqualTo("array | record"));
            Assert.That(info.Output, Is.EqualTo("array"));
            Assert.That(info.Converted, Is.True);
            Assert.That(info.Parameters.Single().Type, Is.EqualTo("expression"));
            Assert.That(info.Parameters.Single().Name, Is.EqualTo("selector"));
        }
    }
}
