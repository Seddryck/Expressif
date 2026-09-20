using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class AccumulatorFoldFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.Aggregation.Fold>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters is not [var accumulator])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        return new Expressif.Library.Array.Aggregation.Fold(constructionContext.CreateAccumulatorProvider(accumulator, context));
    }
}
