using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class KeyFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.Grouping.Key>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
        => new Expressif.Library.Array.Grouping.Key(FunctionConstructorSupport.BuildExpressionEvaluators(function, context, constructionContext));
}
