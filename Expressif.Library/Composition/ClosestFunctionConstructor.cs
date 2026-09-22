using Expressif.Functions.Accumulation;
using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ClosestFunctionConstructor :
    IFunctionConstructor<ClosestAccumulator>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(ClosestAccumulator), function.Arguments).Parameters;
        var target = constructionContext.CreateValueEvaluator(bound[0], context);
        return new ClosestAccumulator(() => target.Invoke(EvaluationRuntime.Frame?.Current));
    }
}
