using Expressif.Bindings;
using Expressif.Serialization;

namespace Expressif.Testing.Serialization;

public class FunctionSerializerTest
{
    [Test]
    public void Serialize_FieldShorthand_PreservesShorthand()
    {
        var function = ExpressifBinderFactory.Create().BindSingleFunction(ExpressifSyntax.Parse(".name"));

        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo(".name"));
    }

    [Test]
    public void Serialize_DynamicFieldName_PreservesLongForm()
    {
        var function = ExpressifBinderFactory.Create().BindSingleFunction(ExpressifSyntax.Parse("field(\"requested-field\")"));

        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("field(\"requested-field\")"));
    }

    [TestCase(0, false, "$0")]
    [TestCase(1, true, "$^1")]
    [TestCase(0, true, "$^0")]
    public void Serialize_InputTupleProjection_UsesTypedParameter(int index, bool fromEnd, string expected)
    {
        var function = new Function(
            "tuple-at",
            [new TupleProjectionParameter(index, fromEnd)],
            FunctionSyntax.InputTupleProjectionShorthand);

        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo(expected));
    }

    [Test]
    public void Serialize_NoParameter_NoParenthesis()
    {
        var function = new Function("Lower", []);
        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("lower"));
    }

    [Test]
    public void Serialize_WithSingleParameter_Parenthesis()
    {
        var function = new Function("FirstChars", [new LiteralParameter("5")]);
        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("first-chars(5)"));
    }

    [Test]
    public void Serialize_MultipleParameter_ParenthesisAndComas()
    {
        var function = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("pad-right(7, \"*\")"));
    }

    [Test]
    public void Serialize_NamedArguments_PreservesNames()
    {
        var function = ExpressifBinderFactory.Create().BindSingleFunction(ExpressifSyntax.Parse("replace-slice(2, append := \"abc\", length := 4)"));

        Assert.That(new FunctionSerializer().Serialize(function),
            Is.EqualTo("replace-slice(2, append := \"abc\", length := 4)"));
    }
}
