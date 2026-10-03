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

    [Test]
    public void Bind_ExpressionUnionWithMultipleMatches_MergesChoiceBindings()
    {
        var bindings = Bind(
            "union<tuple<T>,tuple<U>>",
            new TupleLogicalSchema([new ScalarLogicalSchema("integer")]));

        Assert.Multiple(() =>
        {
            Assert.That(bindings["T"], Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(bindings["U"], Is.EqualTo(new ScalarLogicalSchema("integer")));
        });
    }

    [Test]
    public void Bind_ActualUnionWithSingleMatch_BindsMatchingAlternative()
    {
        var bindings = Bind(
            "array<T>",
            new UnionLogicalSchema(
                [
                    new ScalarLogicalSchema("text"),
                    new ArrayLogicalSchema(new ScalarLogicalSchema("integer")),
                ]));

        Assert.That(bindings["T"], Is.EqualTo(new ScalarLogicalSchema("integer")));
    }

    [Test]
    public void Bind_StructuralConstructors_BindTheirComponents()
    {
        var pair = Bind(
            "pair<K,V>",
            new PairLogicalSchema(new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")));
        var dictionary = Bind(
            "dictionary<K,V>",
            new DictionaryLogicalSchema(new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")));
        var grouping = Bind(
            "grouping<K,V>",
            new GroupingLogicalSchema(new ScalarLogicalSchema("text"), new ScalarLogicalSchema("integer")));
        var sorted = Bind(
            "sort-table<T>",
            new SortTableLogicalSchema(new ScalarLogicalSchema("text")));
        var variadic = Bind(
            "variadic-tuple<T>",
            new TupleLogicalSchema(
                [new ScalarLogicalSchema("integer")],
                AdditionalItems: new ScalarLogicalSchema("text")));

        Assert.Multiple(() =>
        {
            Assert.That(pair.Keys, Is.EqualTo(new[] { "K", "V" }));
            Assert.That(dictionary.Keys, Is.EqualTo(new[] { "K", "V" }));
            Assert.That(grouping.Keys, Is.EqualTo(new[] { "K", "V" }));
            Assert.That(sorted["T"], Is.EqualTo(new ScalarLogicalSchema("text")));
            Assert.That(variadic["T"], Is.TypeOf<UnionLogicalSchema>());
        });
    }

    [Test]
    public void Bind_NullableVariable_UnwrapsExpression()
    {
        var bindings = Bind("nullable<T>", new ScalarLogicalSchema("text"));

        Assert.That(bindings["T"], Is.EqualTo(new ScalarLogicalSchema("text")));
    }

    [Test]
    public void Bind_UnknownActual_DoesNotCreateBindings()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Bind("T", new AnyLogicalSchema()), Is.Empty);
            Assert.That(Bind("T", new NoInputLogicalSchema()), Is.Empty);
        });
    }

    [Test]
    public void Resolve_Constructors_ReconstructsBoundSchemaExpressions()
    {
        var binder = Binder();
        var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal)
        {
            ["T"] = new ScalarLogicalSchema("integer"),
        };

        Assert.Multiple(() =>
        {
            Assert.That(Resolve(binder, "T", bindings), Is.EqualTo(new ScalarLogicalSchema("integer")));
            Assert.That(Resolve(binder, "U", bindings), Is.TypeOf<AnyLogicalSchema>());
            Assert.That(Resolve(binder, "nullable<T>", bindings),
                Is.EqualTo(new ScalarLogicalSchema("integer", IsNullable: true)));
            Assert.That(Resolve(binder, "array<T>", bindings), Is.TypeOf<ArrayLogicalSchema>());
            Assert.That(Resolve(binder, "tuple<T,text>", bindings), Is.TypeOf<TupleLogicalSchema>());
            Assert.That(Resolve(binder, "variadic-tuple<T>", bindings), Is.TypeOf<TupleLogicalSchema>());
            Assert.That(Resolve(binder, "pair<T,text>", bindings), Is.TypeOf<PairLogicalSchema>());
            Assert.That(Resolve(binder, "dictionary<T,text>", bindings), Is.TypeOf<DictionaryLogicalSchema>());
            Assert.That(Resolve(binder, "grouping<T,text>", bindings), Is.TypeOf<GroupingLogicalSchema>());
            Assert.That(Resolve(binder, "sort-table<T>", bindings), Is.TypeOf<SortTableLogicalSchema>());
            Assert.That(Resolve(binder, "union<T,text>", bindings), Is.TypeOf<UnionLogicalSchema>());
            Assert.That(Resolve(binder, "text", bindings), Is.EqualTo(new ScalarLogicalSchema("text")));
        });
    }

    [Test]
    public void Resolve_UnsupportedConstructor_Throws()
    {
        var action = () => Resolve(Binder(), "unsupported<T>", new Dictionary<string, LogicalSchema>());

        Assert.That(action, Throws.InvalidOperationException
            .With.Message.EqualTo("Unsupported schema constructor 'unsupported'."));
    }

    [TestCase("text trailing")]
    [TestCase("")]
    [TestCase("array<text")]
    public void Parse_InvalidExpression_Throws(string expression)
    {
        var action = () => SchemaExpressionParser.Parse(expression);

        Assert.That(action, Throws.InvalidOperationException);
    }

    private static IReadOnlyDictionary<string, LogicalSchema> Bind(string expression, LogicalSchema actual)
    {
        var binder = Binder();
        var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);

        binder.Bind(SchemaExpressionParser.Parse(expression), actual, bindings, "root");

        return bindings;
    }

    private static SchemaExpressionBinder Binder()
        => new(new SchemaAlgebra([]), FromType);

    private static LogicalSchema Resolve(
        SchemaExpressionBinder binder,
        string expression,
        IReadOnlyDictionary<string, LogicalSchema> bindings)
        => binder.Resolve(SchemaExpressionParser.Parse(expression), bindings);

    private static LogicalSchema FromType(string type) => new ScalarLogicalSchema(type);
}
