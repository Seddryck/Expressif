using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class MapWithFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.MapWith>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
        => DirectionalMapConstruction.Build(function, context, constructionContext, mapOver: false);
}
