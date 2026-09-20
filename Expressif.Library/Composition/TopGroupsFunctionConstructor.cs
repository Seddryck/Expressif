using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class TopGroupsFunctionConstructor : IFunctionConstructor<Expressif.Library.Grouping.TopGroups>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Any(argument => argument.IsSpread))
            throw new SpreadArgumentException("Spread arguments are not supported by top-groups.");
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Grouping.TopGroups), function.Arguments).Parameters;
        var count = (Func<int>)constructionContext.CreateParameter(bound[0], typeof(int), context);
        var evaluator = new DelegatedFunction(constructionContext.CreateValueEvaluator(bound[1], context));
        return new Expressif.Library.Grouping.TopGroups(count, () => evaluator);
    }
}
