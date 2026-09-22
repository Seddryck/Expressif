using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class JoinFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Array.Combination.Exists>,
    IFunctionConstructor<Expressif.Library.Array.Combination.Join>,
    IFunctionConstructor<Expressif.Library.Array.Combination.JoinLeft>,
    IFunctionConstructor<Expressif.Library.Array.Combination.JoinRight>,
    IFunctionConstructor<Expressif.Library.Array.Combination.JoinFull>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var name = function.Name.ToKebabCase();
        var type = name switch
        {
            "exists" => typeof(Expressif.Library.Array.Combination.Exists),
            "join-left" => typeof(Expressif.Library.Array.Combination.JoinLeft),
            "join-right" => typeof(Expressif.Library.Array.Combination.JoinRight),
            "join-full" => typeof(Expressif.Library.Array.Combination.JoinFull),
            _ => typeof(Expressif.Library.Array.Combination.Join),
        };
        var bound = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
        if (bound.Length is < 2 or > 3)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var rightEvaluator = constructionContext.CreateValueEvaluator(bound[0], context);
        Func<object?> right = () => rightEvaluator.Invoke(EvaluationRuntime.Frame?.Current);
        Func<object?, object?> BuildKey(IParameter parameter)
        {
            var evaluator = new DelegatedFunction(
                constructionContext.CreateValueEvaluator(parameter, context));
            return value => FunctionConstructorSupport.EvaluateNested(evaluator, value);
        }

        var leftKey = BuildKey(bound[1]);
        var rightKey = bound.Length == 3 ? BuildKey(bound[2]) : null;
        return name switch
        {
            "exists" => new Expressif.Library.Array.Combination.Exists(right, leftKey, rightKey),
            "join-left" => new Expressif.Library.Array.Combination.JoinLeft(right, leftKey, rightKey),
            "join-right" => new Expressif.Library.Array.Combination.JoinRight(right, leftKey, rightKey),
            "join-full" => new Expressif.Library.Array.Combination.JoinFull(right, leftKey, rightKey),
            _ => new Expressif.Library.Array.Combination.Join(right, leftKey, rightKey),
        };
    }
}
