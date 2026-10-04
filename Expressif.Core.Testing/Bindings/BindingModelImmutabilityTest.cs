using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class BindingModelImmutabilityTest
{
    [Test]
    public void Function_FreezesParametersAndArguments()
    {
        var original = new LiteralParameter("original");
        var replacement = new LiteralParameter("replacement");
        IParameter[] parameters = [original];

        var function = new Function("identity", parameters);
        parameters[0] = replacement;

        Assert.Multiple(() =>
        {
            Assert.That(function.Parameters[0], Is.SameAs(original));
            Assert.That(function.Arguments[0].Value, Is.SameAs(original));
            AssertReadOnly(function.Parameters, replacement);
            AssertReadOnly(function.Arguments, new FunctionArgument(null, replacement));
        });
    }

    [Test]
    public void CompositeParameters_FreezeConstructorInputs()
    {
        var original = new LiteralParameter("original");
        var replacement = new LiteralParameter("replacement");

        AssertFrozen(
            new[] { new ArrayElementParameter(original) },
            new ArrayElementParameter(replacement),
            values => new ArrayParameter(values).Elements);
        AssertFrozen(
            new[] { new TupleElementParameter(original) },
            new TupleElementParameter(replacement),
            values => new TupleParameter(values).Elements);
        AssertFrozen(
            new[] { new TupleElementParameter(original) },
            new TupleElementParameter(replacement),
            values => new VectorParameter(values).Elements);
        AssertFrozen(
            new[] { new PairParameter(original, original) },
            new PairParameter(replacement, replacement),
            values => new GroupingParameter(values).Entries);
        AssertFrozen(
            new[] { new PairParameter(original, original) },
            new PairParameter(replacement, replacement),
            values => new DictionaryParameter(values).Entries);
        AssertFrozen(
            new[] { new RecordLiteralField("field", original) },
            new RecordLiteralField("field", replacement),
            values => new RecordLiteralParameter(values).Fields);
        AssertFrozen<IRecordDefinitionEntry>(
            [new RecordNamedEntry("field", original)],
            new RecordNamedEntry("field", replacement),
            values => new RecordDefinitionParameter(values).Entries);
        AssertFrozen(
            new[] { new LetBinding("value", original) },
            new LetBinding("value", replacement),
            values => new LetDefinitionParameter(values).Bindings);
        AssertFrozen(
            new[] { new WithProjection("value", original) },
            new WithProjection("value", replacement),
            values => new WithDefinitionParameter(values, original).Projections);
    }

    private static void AssertFrozen<T>(
        T[] source,
        T replacement,
        Func<IEnumerable<T>, IReadOnlyList<T>> construct)
    {
        var original = source[0];
        var frozen = construct(source);

        source[0] = replacement;

        Assert.Multiple(() =>
        {
            Assert.That(frozen[0], Is.SameAs(original));
            AssertReadOnly(frozen, replacement);
        });
    }

    private static void AssertReadOnly<T>(IReadOnlyList<T> values, T replacement)
        => Assert.That(() => ((IList<T>)values)[0] = replacement, Throws.TypeOf<NotSupportedException>());
}
