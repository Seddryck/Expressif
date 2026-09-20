using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class NamedFieldFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Record.Explode>,
    IFunctionConstructor<Expressif.Library.Record.ExplodeOuter>,
    IFunctionConstructor<Expressif.Library.Record.ImplodeInner>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (!constructionContext.TryResolveImplementation(function.Name, out var type))
            throw new NotImplementedFunctionException(function.Name);
        var bound = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
        var field = bound[0] is OpenExpressionParameter open
            ? open.Expression.Members.ToArray()
            : [];
        if (field is not [{ Syntax: FunctionSyntax.FieldShorthand, Parameters: [LiteralParameter { Value: string name }] }])
        {
            throw new BindingException(
                "The selector must be a direct field selector such as .tags; computed expressions and nested paths are not supported.");
        }

        var evaluator = new DelegatedFunction(
            constructionContext.CreateValueEvaluator(bound[0], context));
        var selector = new NamedFieldSelector(
            name,
            value => FunctionConstructorSupport.EvaluateNested(evaluator, value));
        return type == typeof(Expressif.Library.Record.ExplodeOuter)
            ? new Expressif.Library.Record.ExplodeOuter(selector)
            : type == typeof(Expressif.Library.Record.ImplodeInner)
                ? new Expressif.Library.Record.ImplodeInner(selector)
                : new Expressif.Library.Record.Explode(selector);
    }
}
