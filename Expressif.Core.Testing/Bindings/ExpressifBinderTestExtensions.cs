using Expressif.Syntax;

namespace Expressif.Bindings;

internal static class ExpressifBinderTestExtensions
{
    public static Function BindSingleFunction(this ExpressifBinder binder, RootExpressionSyntax syntax)
    {
        var root = binder.Bind(syntax);
        return root is OpenRootExpression open && open.Expression.Members.Count() == 1
            ? open.Expression.Members.Single()
            : throw new BindingException($"Source '{syntax.Text}' is not a single function.");
    }
}
