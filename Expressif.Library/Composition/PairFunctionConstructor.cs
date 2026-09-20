using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class PairFunctionConstructor : IFunctionConstructor<Expressif.Library.Pair.Pair>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Pair.Pair), function.Arguments).Parameters;
        if (bound is not [var key, var value])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        return new Expressif.Library.Pair.Pair(
            constructionContext.CreateValueEvaluator(key, context),
            constructionContext.CreateValueEvaluator(value, context));
    }
}
