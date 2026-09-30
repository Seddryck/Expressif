using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class ExpressionShapeNormalizerTest
{
    [Test]
    public void DirectField_RecognizesExactlyOneShorthandMember()
    {
        var selector = Field("tags");

        Assert.That(ExpressionShapeNormalizer.TryGetDirectFieldName(selector, out var name), Is.True);
        Assert.That(name, Is.EqualTo("tags"));
    }

    [Test]
    public void DirectField_RejectsComputedAndNestedPaths()
    {
        var nested = new OpenExpressionParameter(new OpenExpression([
            new Function("field", [new LiteralParameter("customer")], FunctionSyntax.FieldShorthand),
            new Function("field", [new LiteralParameter("tags")], FunctionSyntax.FieldShorthand),
        ]));
        var computed = new OpenExpressionParameter(new OpenExpression([new Function("upper", [])]));

        Assert.That(ExpressionShapeNormalizer.TryGetDirectFieldName(nested, out _), Is.False);
        Assert.That(ExpressionShapeNormalizer.TryGetLeadingFieldName(nested, out var leading), Is.True);
        Assert.That(leading, Is.EqualTo("customer"));
        Assert.That(ExpressionShapeNormalizer.TryGetDirectFieldName(computed, out _), Is.False);
    }

    [Test]
    public void DirectField_InvalidShape_HasParameterSpecificDiagnostic()
        => Assert.That(() => ExpressionShapeNormalizer.RequireDirectFieldName(
                new LiteralParameter("tags"), "explode", "selector"),
            Throws.TypeOf<BindingException>()
                .With.Message.Contains("explode").And.Message.Contains("selector")
                .And.Message.Contains("direct field selector"));

    [Test]
    public void CallableReference_InvalidShape_HasParameterSpecificDiagnostic()
        => Assert.That(() => ExpressionShapeNormalizer.RequireCallableReference(
                new LiteralParameter("compare-numeric"), "sort-term", "comparer"),
            Throws.TypeOf<BindingException>()
                .With.Message.Contains("sort-term").And.Message.Contains("comparer")
                .And.Message.Contains("tuple-bound callable reference"));

    private static OpenExpressionParameter Field(string name)
        => new(new OpenExpression([
            new Function("field", [new LiteralParameter(name)], FunctionSyntax.FieldShorthand),
        ]));
}
