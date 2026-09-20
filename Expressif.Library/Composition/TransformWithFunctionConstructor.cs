using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class TransformWithFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.TransformWith>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters.Length < 2
            || !FunctionConstructorSupport.TryGetOpenExpression(function.Parameters[0], out var open))
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }

        var operation = constructionContext.CreateOpenExpression(open.Expression, context);
        var expressions = function.Parameters.Skip(1)
            .Select(parameter => constructionContext.CreateValueEvaluator(parameter, context));
        return new Expressif.Library.Flow.TransformWith(
            () => new LexicallyBoundContextFunction(operation),
            expressions);
    }
}
