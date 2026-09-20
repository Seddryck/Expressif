using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ConditionalFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.ConditionalForward>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        var backward = function.Syntax == FunctionSyntax.ConditionalBackward;
        return new Expressif.Library.Flow.ConditionalForward(
            BuildEvaluator(function.Parameters[backward ? 0 : 1], context, constructionContext),
            BuildEvaluator(function.Parameters[backward ? 1 : 0], context, constructionContext),
            backward);
    }

    internal static Func<object?, object?> BuildEvaluator(
        IParameter parameter,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (parameter is OpenExpressionParameter open)
            return constructionContext.CreateOpenExpression(open.Expression, context).Evaluate;
        if (parameter is InputExpressionParameter closed)
        {
            var source = BuildEvaluator(closed.Expression.Parameter, context, constructionContext);
            var expression = constructionContext.CreateOpenExpression(
                new OpenExpression(closed.Expression.Members), context);
            return input => expression.Evaluate(source.Invoke(input));
        }
        return constructionContext.CreateValueEvaluator(parameter, context);
    }
}
