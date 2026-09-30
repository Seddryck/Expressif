using Expressif.Bindings;
using Expressif.Serialization;

namespace Expressif.Testing.Serialization;

public class ExpressionSerializerTest
{
    [TestCase(".a.b.c", ".a | .b | .c")]
    [TestCase(".a | ^.a.b.c", ".a | ^.a | .b | .c")]
    [TestCase("map(.a.b)", "map(.a | .b)")]
    [TestCase("suffix(^.a.b)", "suffix(^.a.b)")]
    [TestCase("greater-than(^^.a.b)", "greater-than(^^.a.b)")]
    public void Serialize_ChainedFields_ProducesEquivalentExpression(string source, string expected)
    {
        var binder = ExpressifBinderFactory.Create();
        var root = (OpenRootExpression)binder.Bind(Expressif.Syntax.ExpressionParser.Parse(source));
        var serialized = new ExpressionSerializer().Serialize(root.Expression);
        Assert.That(serialized, Is.EqualTo(expected));
        Assert.That(() => binder.Bind(Expressif.Syntax.ExpressionParser.Parse(serialized)), Throws.Nothing);
    }

    [Test]
    public void Serialize_GroupMapShorthand_PreservesShorthand()
    {
        var root = ExpressifBinderFactory.Create().Bind(
            Expressif.Syntax.ExpressionParser.Parse("@groups |#> reverse"));
        var expression = ((ClosedRootExpression)root).Expression;

        Assert.That(new ExpressionSerializer().Serialize(expression),
            Is.EqualTo("@groups |#> reverse"));
    }

    [Test]
    public void Serialize_SingleMember_NoPipe()
    {
        var expression = new Function("Lower", []);
        Assert.That(new ExpressionSerializer().Serialize([expression]), Is.EqualTo("lower"));
    }

    [Test]
    public void Serialize_MultipleMembers_WithPipe()
    {
        var lowerExpression = new Function("Lower", []);
        var firstCharsExpression = new Function("FirstChars", [new LiteralParameter("5")]);
        var padRightExpression = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
        Assert.That(new ExpressionSerializer().Serialize([lowerExpression, firstCharsExpression, padRightExpression])
            , Is.EqualTo("lower | first-chars(5) | pad-right(7, \"*\")"));
    }

    [Test]
    public void Serialize_ClosedExpression_WithRootAndMembers()
    {
        var expression = new Expressif.Bindings.ClosedExpression(new VariableParameter("arr"), [new Function("count", [])]);

        Assert.That(new ExpressionSerializer().Serialize(expression), Is.EqualTo("@arr | count"));
    }

    [Test]
    public void Serialize_MapShorthand_PreservesShorthand()
    {
        var root = ExpressifBinderFactory.Create().Bind(
            ExpressifSyntax.Parse("{1,2,3} |> (absolute | add(5)) | reverse"));
        var expression = ((ClosedRootExpression)root).Expression;

        Assert.That(new ExpressionSerializer().Serialize(expression),
            Is.EqualTo("{1, 2, 3} |> (absolute | add(5)) | reverse"));
    }

    [Test]
    public void Serialize_EnclosingRootField_PreservesShorthand()
    {
        var root = ExpressifBinderFactory.Create().Bind(
            Expressif.Syntax.ExpressionParser.Parse("greater-than(^^.threshold)"));
        var expression = ((OpenRootExpression)root).Expression;

        Assert.That(new ExpressionSerializer().Serialize(expression),
            Is.EqualTo("greater-than(^^.threshold)"));
    }

    [Test]
    public void Serialize_EnclosingRootFieldFunction_PreservesShorthand()
    {
        var function = new Function(
            "field",
            [new QuotedLiteralParameter("threshold")],
            FunctionSyntax.EnclosingRootFieldShorthand);

        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo("^^.threshold"));
    }

    [Test]
    [Ignore("Limited added-value to manage subexpression")]
    public void Serialize_WithSubExpression_WithPipe()
    {
        var lowerExpression = new Function("Lower", []);
        var firstCharsExpression = new Function("FirstChars", [new LiteralParameter("5")]);
        var padRightExpression = new Function("PadRight", [new LiteralParameter("7"), new LiteralParameter("*")]);
        var upperExpression = new Function("Upper", []);

        //var subExpression = new Expressif.Bindings.ClosedExpression([firstCharsExpression, PadRightExpression]);

        //var expression = new Expressif.Bindings.Expression([lowerExpression, subExpression, upperExpression]);

        //Assert.That(new ExpressionSerializer().Serialize(expression)
        //    , Is.EqualTo("lower | { first-chars(5) | pad-right(7, *) } | upper"));
    }
}
