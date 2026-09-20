using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class RenameFieldsFunctionConstructor : IFunctionConstructor<Expressif.Library.Record.RenameFields>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Record.RenameFields), function.Arguments).Parameters;
        var transform = bound[0] is OpenExpressionParameter open
            ? constructionContext.CreateOpenExpression(open.Expression, context)
            : new DelegatedFunction(constructionContext.CreateValueEvaluator(bound[0], context));
        var filter = bound.Length == 2
            ? constructionContext.CreatePredicateProvider(bound[1], context, function.Name).Invoke()
            : null;
        return new Expressif.Library.Record.RenameFields(
            () => transform,
            filter is null ? null : () => filter);
    }
}
