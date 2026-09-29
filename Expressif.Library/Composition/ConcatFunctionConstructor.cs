using Expressif.Functions.Accumulation;
using Expressif.Bindings;
using Expressif.Library.Text;

namespace Expressif.Library.Composition;

internal sealed class ConcatFunctionConstructor :
    IFunctionConstructor<ConcatAccumulator>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(ConcatAccumulator), function.Arguments).Parameters;
        if (bound.Length == 0)
            return new ConcatAccumulator();

        var separator = (Func<string>)constructionContext.CreateParameter(bound[0], typeof(string), context);
        return new ConcatAccumulator(separator);
    }
}
