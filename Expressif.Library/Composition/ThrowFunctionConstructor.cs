using Expressif.Bindings;
using Expressif.Predicates;

namespace Expressif.Library.Composition;

internal sealed class ThrowFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.Throw>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Flow.Throw), function.Arguments).Parameters;
        if (bound.Length == 0)
            return new Expressif.Library.Flow.Throw();
        var evaluator = new DelegatedFunction(
            constructionContext.CreateValueEvaluator(bound[0], context));
        return new Expressif.Library.Flow.Throw(() => new BooleanFunctionPredicate(evaluator));
    }
}
