using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class MapOverFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.MapOver>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
        => DirectionalMapConstruction.Build(function, context, constructionContext, mapOver: true);
}
