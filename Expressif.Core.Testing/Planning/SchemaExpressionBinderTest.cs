using Expressif.Planning;

namespace Expressif.Testing.Planning;

public class SchemaExpressionBinderTest
{
    [Test]
    public void Bind_RepeatedVariable_IntersectsConstraints()
    {
        var bindings = Bind(
            "tuple<T,T>",
            new TupleLogicalSchema(
                [new ScalarLogicalSchema("integer"), new ScalarLogicalSchema("numeric")]));

        Assert.That(bindings["T"], Is.EqualTo(new ScalarLogicalSchema("integer")));
    }

    [Test]
    public void Bind_ExpressionUnion_PrefersExactRoot()
    {
        var bindings = Bind(
            "union<array<T>,dictionary<K,V>>",
            new DictionaryLogicalSchema(
                new ScalarLogicalSchema("text"),
                new ScalarLogicalSchema("integer")));

        Assert.Multiple(() =>
        {
            Assert.That(bindings, Does.Not.ContainKey("T"));
            Assert.That(bindings["K"], Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(bindings["V"], Is.EqualTo(new ScalarLogicalSchema("integer")));
        });
    }

    [Test]
    public void Bind_ActualUnion_UnionsAlternativeBindings()
    {
        var bindings = Bind(
            "array<T>",
            new UnionLogicalSchema(
                [
                    new ArrayLogicalSchema(new ScalarLogicalSchema("integer")),
                    new ArrayLogicalSchema(new ScalarLogicalSchema("text")),
                ]));

        Assert.That(bindings["T"], Is.TypeOf<UnionLogicalSchema>());
        Assert.That(
            ((UnionLogicalSchema)bindings["T"]).Alternatives,
            Is.EqualTo(new LogicalSchema[]
            {
                new ScalarLogicalSchema("integer"),
                new ScalarLogicalSchema("text"),
            }));
    }

    [Test]
    public void Bind_GroupingAsArray_AdaptsItemsToPairWithArrayValue()
    {
        var bindings = Bind(
            "array<pair<K,array<V>>>",
            new GroupingLogicalSchema(
                new ScalarLogicalSchema("text"),
                new ScalarLogicalSchema("integer")));

        Assert.Multiple(() =>
        {
            Assert.That(bindings["K"], Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(bindings["V"], Is.EqualTo(new ScalarLogicalSchema("integer")));
        });
    }

    private static IReadOnlyDictionary<string, LogicalSchema> Bind(string expression, LogicalSchema actual)
    {
        var binder = new SchemaExpressionBinder(new SchemaAlgebra([]), FromType);
        var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);

        binder.Bind(SchemaExpressionParser.Parse(expression), actual, bindings, "root");

        return bindings;
    }

    private static LogicalSchema FromType(string type) => new ScalarLogicalSchema(type);
}
