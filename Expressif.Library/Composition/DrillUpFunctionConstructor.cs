using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class DrillUpFunctionConstructor : IFunctionConstructor<Expressif.Library.Grouping.DrillUp>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Grouping.DrillUp), function.Arguments).Parameters;
        if (bound is not [var expression])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        var evaluator = new DelegatedFunction(constructionContext.CreateValueEvaluator(expression, context));
        return new Expressif.Library.Grouping.DrillUp(key => FunctionConstructorSupport.EvaluateNested(evaluator, key));
    }
}
