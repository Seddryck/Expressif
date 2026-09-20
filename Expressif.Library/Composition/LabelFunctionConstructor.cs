using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class LabelFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Tuple.Label>,
    IFunctionConstructor<Expressif.Library.Tuple.LabelConflicts>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.FirstOrDefault(argument => argument.Name is not null) is { } named)
            throw new UnknownParameterNameException(function.Name, named.Name!);
        if (function.Arguments.Any(argument => argument.IsSpread))
            throw new SpreadArgumentException($"Spread arguments are not supported by {function.Name}.");

        var names = function.Arguments
            .Select(argument => (Func<string>)constructionContext.CreateParameter(
                argument.Value,
                typeof(string),
                context))
            .ToArray();
        Func<string[]> labels = () => names.Select(provider => provider.Invoke()).ToArray();
        return function.Name.Equals("label", StringComparison.OrdinalIgnoreCase)
            ? new Expressif.Library.Tuple.Label(labels)
            : new Expressif.Library.Tuple.LabelConflicts(labels);
    }
}
