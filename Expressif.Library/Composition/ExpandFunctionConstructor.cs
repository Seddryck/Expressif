using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ExpandFunctionConstructor : IFunctionConstructor<Expressif.Library.Record.Expand>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Any(argument => argument.IsSpread))
            throw new SpreadArgumentException("Spread arguments are not supported by expand.");
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Record.Expand), function.Arguments).Parameters;
        var members = bound[0] is OpenExpressionParameter open ? open.Expression.Members.ToArray() : [];
        var field = members.FirstOrDefault() is
            { Syntax: FunctionSyntax.FieldShorthand, Parameters: [LiteralParameter { Value: string fieldName }] }
            ? fieldName
            : null;
        if (bound.Length == 1 && (field is null || members.Length != 1))
        {
            throw new BindingException(
                "The expand selector must be a direct field selector such as .customer when no explicit label is supplied.");
        }
        var selector = new DelegatedFunction(constructionContext.CreateValueEvaluator(bound[0], context));
        var label = bound.Length == 2
            ? constructionContext.CreateValueEvaluator(bound[1], context)
            : null;
        return new Expressif.Library.Record.Expand(
            new RecordExpansionSelector(
                field,
                value => FunctionConstructorSupport.EvaluateNested(selector, value)),
            label is null
                ? null
                : value => label.Invoke(value) as string
                    ?? throw new InvalidOperationException("The expand label must return text."));
    }
}
