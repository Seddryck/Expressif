using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class DrillDownFunctionConstructor : IFunctionConstructor<Expressif.Library.Grouping.DrillDown>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var expressions = FunctionConstructorSupport.BuildExpressionEvaluators(function, context, constructionContext)
            .Select(evaluator => new DelegatedFunction(evaluator))
            .Select(expression => (Func<object?, object?>)(value =>
                FunctionConstructorSupport.EvaluateNested(expression, value)));
        return new Expressif.Library.Grouping.DrillDown(expressions);
    }
}
