namespace Expressif.Testing;

public class ClosedExpressionTest
{
    [Test]
    public void Evaluate_ArrayLiteralPipeSum_Valid()
    {
        var expression = ClosedExpression.Create("{1,2,3} | sum");
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(6m));
    }

    [Test]
    public void Evaluate_VariableArrayPipeCount_Valid()
    {
        var context = new Context();
        context.Variables.Add<int[]>("arr", new[] { 1, 2, 3, 4 });

        var expression = ClosedExpression.Create("@arr | count", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_ClosedExpressionWithoutMembers_ReturnsRootValue()
    {
        var context = new Context();
        context.Variables.Add<int>("n", 7);

        var expression = ClosedExpression.Create("@n", context);
        var result = expression.Evaluate();
        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void Evaluate_OpenExpression_ThrowsExpressionRequiresInputException()
    {
        var exception = Assert.Throws<ExpressionRequiresInputException>(() => ClosedExpression.Create("upper"));

        Assert.That(exception!.Message, Is.EqualTo("The expression cannot be evaluated without an input because it references 'upper'."));
    }
}
