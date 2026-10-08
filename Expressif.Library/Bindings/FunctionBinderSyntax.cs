using Expressif.Syntax;

namespace Expressif.Bindings;

internal static class FunctionBinderSyntax
{
    public static ExpressionSyntax RequireValue(ArgumentSyntax argument)
        => argument.Value ?? throw new BindingException("A non-spread argument must include an expression.");

    public static bool TryGetBareFunctionName(ExpressionSyntax syntax, out string name)
    {
        var function = syntax switch
        {
            FunctionCallSyntax { Arguments.Count: 0 } call => call,
            OpenExpressionSyntax { Pipeline: [FunctionCallSyntax { Arguments.Count: 0 } call] } => call,
            _ => null,
        };
        name = function?.Name ?? string.Empty;
        return function is not null;
    }

    public static BindingException Unsupported(SyntaxNode syntax)
        => new($"Syntax kind '{syntax.Kind}' is not bound in this iteration (source: '{syntax.Text}').");
}
