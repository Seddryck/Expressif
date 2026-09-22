using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class CatchFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.Catch>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Flow.Catch), function.Arguments).Parameters;
        var recovery = new DelegatedFunction(
            constructionContext.CreateValueEvaluator(bound[0], context));
        return new Expressif.Library.Flow.Catch(() => recovery);
    }
}
