using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ApplyFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.Apply>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters.Length != 1)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var operation = FunctionConstructorSupport.TryGetOpenExpression(function.Parameters[0], out var open)
            ? constructionContext.CreateOpenExpression(open.Expression, context)
            : new DelegatedFunction(constructionContext.CreateValueEvaluator(
                function.Parameters[0], context, establishScope: true));
        IFunction expression = function.Parameters[0] is InputExpressionParameter
            ? operation
            : new LexicallyBoundContextFunction(operation);
        return new Expressif.Library.Flow.Apply(() => expression);
    }
}
