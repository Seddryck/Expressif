using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Syntax;

namespace Expressif.Cli.Expressions;

internal interface ISyntaxService
{
    RootExpressionSyntax Parse(string code);

    IRootExpression Bind(RootExpressionSyntax syntax);

    void Validate(IRootExpression expression, Context context);
}

internal sealed class SyntaxService : ISyntaxService
{
    public RootExpressionSyntax Parse(string code) => ExpressionParser.Parse(code);

    public IRootExpression Bind(RootExpressionSyntax syntax) => new ExpressifBinder().Bind(syntax);

    public void Validate(IRootExpression expression, Context context)
        => _ = new FunctionFactory().Instantiate(expression, context);
}
