using Expressif.Functions.Accumulation;
using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class OnlyFunctionConstructor :
    IFunctionConstructor<OnlyAccumulator>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Any(argument => argument.IsSpread))
            throw new SpreadArgumentException("Spread arguments are not supported by only.");
        var bound = ParameterArgumentBinder.Bind(typeof(OnlyAccumulator), function.Arguments).Parameters;
        return new OnlyAccumulator(
            constructionContext.CreatePredicateProvider(bound[0], context, function.Name),
            constructionContext.CreateAccumulatorProvider(bound[1], context));
    }
}
