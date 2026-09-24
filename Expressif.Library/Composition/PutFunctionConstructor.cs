using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class PutFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Record.Put>,
    IFunctionConstructor<Expressif.Library.Record.PutPresent>,
    IFunctionConstructor<Expressif.Library.Record.PutAbsent>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var name = function.Name.ToKebabCase();
        var target = name switch
        {
            "put" => typeof(Expressif.Library.Record.Put),
            "put-present" => typeof(Expressif.Library.Record.PutPresent),
            "put-absent" => typeof(Expressif.Library.Record.PutAbsent),
            _ => throw new NotImplementedFunctionException(name),
        };
        var layout = ParameterArgumentBinder.BindLayout(target, function.Arguments);
        var assignments = layout.Named
            .Select(argument => new Expressif.Library.Record.RecordAssignmentEvaluator(
                argument.Name!,
                constructionContext.CreateValueEvaluator(argument.Value, context)))
            .ToArray();
        return name switch
        {
            "put" => new Expressif.Library.Record.Put(() => assignments),
            "put-present" => new Expressif.Library.Record.PutPresent(() => assignments),
            "put-absent" => new Expressif.Library.Record.PutAbsent(() => assignments),
            _ => throw new NotImplementedFunctionException(name),
        };
    }
}
