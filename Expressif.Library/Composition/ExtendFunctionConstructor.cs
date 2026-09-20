using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ExtendFunctionConstructor : IFunctionConstructor<Expressif.Library.Tuple.Extend>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments is not [var extension]
            || (extension.Name is not null
                && !extension.Name.Equals("value", StringComparison.OrdinalIgnoreCase)))
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }
        var evaluator = constructionContext.CreateValueEvaluator(extension.Value, context);
        return new Expressif.Library.Tuple.Extend(value => evaluator.Invoke(value));
    }
}
