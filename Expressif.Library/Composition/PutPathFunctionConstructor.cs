using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class PutPathFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Record.PutPath>,
    IFunctionConstructor<Expressif.Library.Record.PutPresentPath>,
    IFunctionConstructor<Expressif.Library.Record.PutAbsentPath>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var name = function.Name.ToKebabCase();
        var type = name switch
        {
            "put-path" => typeof(Expressif.Library.Record.PutPath),
            "put-present-path" => typeof(Expressif.Library.Record.PutPresentPath),
            "put-absent-path" => typeof(Expressif.Library.Record.PutAbsentPath),
            _ => throw new NotImplementedFunctionException(name),
        };
        var bound = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
        if (bound is not [var path, var value])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var pathEvaluator = constructionContext.CreateValueEvaluator(path, context);
        var valueEvaluator = constructionContext.CreateValueEvaluator(value, context);
        return name switch
        {
            "put-path" => new Expressif.Library.Record.PutPath(pathEvaluator, valueEvaluator),
            "put-present-path" => new Expressif.Library.Record.PutPresentPath(pathEvaluator, valueEvaluator),
            "put-absent-path" => new Expressif.Library.Record.PutAbsentPath(pathEvaluator, valueEvaluator),
            _ => throw new NotImplementedFunctionException(name),
        };
    }
}
