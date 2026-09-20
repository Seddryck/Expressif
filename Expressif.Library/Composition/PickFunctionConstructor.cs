using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class PickFunctionConstructor : IFunctionConstructor<Expressif.Library.Tuple.Pick>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var positions = function.Arguments
            .Select(argument => (Func<int>)constructionContext.CreateParameter(
                argument.Value,
                typeof(int),
                context))
            .ToArray();
        return new Expressif.Library.Tuple.Pick(() => positions.Select(position => position.Invoke()).ToArray());
    }
}
