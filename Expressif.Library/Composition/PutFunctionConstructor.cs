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
        if (function.Arguments.Length == 0
            || function.Arguments.Any(argument => argument.Name is null || argument.IsSpread))
        {
            throw new BindingException($"Function '{name}' expects one or more named assignments.");
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        var duplicate = function.Arguments.FirstOrDefault(argument => !names.Add(argument.Name!));
        if (duplicate is not null)
            throw new BindingException($"Duplicate assignment '{duplicate.Name}' in {name}(...).");

        var assignments = function.Arguments
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
