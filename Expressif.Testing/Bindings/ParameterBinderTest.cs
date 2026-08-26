using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class ParameterBinderTest
{
    private static ExpressifBinder Binder { get; } = new();

    [Test]
    public void Bind_LiteralSyntax_PreservesTypedValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(BindParameter(SyntaxFactory.Number(42)), Is.EqualTo(new LiteralParameter(42m)));
            Assert.That(BindParameter(SyntaxFactory.Boolean(true)), Is.EqualTo(new LiteralParameter(true)));
            Assert.That(BindParameter(SyntaxFactory.Null()), Is.EqualTo(new LiteralParameter(null)));
            Assert.That(BindParameter(SyntaxFactory.Text("Alice")),
                Is.EqualTo(new QuotedLiteralParameter("Alice")));
        });
    }

    [Test]
    public void Bind_References_PreserveParameterTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(BindParameter(SyntaxFactory.Variable("foo")), Is.TypeOf<VariableParameter>());
            Assert.That(BindParameter(SyntaxFactory.RecordAccess("foo")), Is.TypeOf<ObjectPropertyParameter>());
            Assert.That(BindParameter(SyntaxFactory.Incoming()), Is.TypeOf<IncomingValueParameter>());
        });
    }

    [Test]
    public void Bind_ArrayLiteral_PreservesSpreadElements()
    {
        var syntax = SyntaxFactory.Array(
            SyntaxFactory.ArrayElement(SyntaxFactory.Number(1)),
            SyntaxFactory.ArrayElement(SyntaxFactory.Variable("items"), true),
            SyntaxFactory.ArrayElement(null, true));

        var parameter = (ArrayParameter)BindParameter(syntax);

        Assert.Multiple(() =>
        {
            Assert.That(parameter.Elements.Select(element => element.IsSpread),
                Is.EqualTo(new[] { false, true, true }));
            Assert.That(parameter.Elements.Last().Value, Is.TypeOf<IncomingValueParameter>());
        });
    }

    [Test]
    public void Bind_RecordLiteral_PreservesNamedFields()
    {
        var syntax = SyntaxFactory.Record(
            SyntaxFactory.Field("name", SyntaxFactory.Text("Alice")),
            SyntaxFactory.Field("active", SyntaxFactory.Boolean(true)));

        var parameter = (RecordLiteralParameter)BindParameter(syntax);

        Assert.That(parameter.Fields.Select(field => field.Name), Is.EqualTo(new[] { "name", "active" }));
    }

    [Test]
    public void Bind_RecordLiteralUnnamedSpread_ThrowsBindingError()
    {
        var syntax = SyntaxFactory.Record(new RecordSpreadSyntax(new SourceSpan(0, 3), "..."));

        Assert.That(
            () => BindParameter(syntax),
            Throws.TypeOf<BindingException>()
                .With.Message.EqualTo("Record literal spread entries must specify a field name."));
    }

    [Test]
    public void Bind_RecordLiteralSpreadField_ThrowsBindingError()
    {
        var syntax = SyntaxFactory.Record(SyntaxFactory.Field("field", SyntaxFactory.Variable("value"), true));

        Assert.That(
            () => BindParameter(syntax),
            Throws.TypeOf<BindingException>()
                .With.Message.EqualTo("Record literal field 'field' does not support spread values."));
    }

    [Test]
    public void Bind_TupleLiteral_PreservesNestedTuple()
    {
        var syntax = SyntaxFactory.Tuple(
            SyntaxFactory.Number(1),
            SyntaxFactory.Tuple(SyntaxFactory.Number(2), SyntaxFactory.Number(3)));

        var parameter = (TupleParameter)BindParameter(syntax);

        Assert.That(parameter.Values.Last(), Is.TypeOf<TupleParameter>());
    }

    private static IParameter BindParameter(ValueSyntax value)
        => Binder.BindParameter(SyntaxFactory.Closed(value));
}
