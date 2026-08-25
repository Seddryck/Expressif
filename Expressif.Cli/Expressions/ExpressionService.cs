namespace Expressif.Cli.Expressions;

internal interface IExpressionService
{
    IExpression CompileOpen(string code, Context context);
    IExpression CompileClosed(string code, Context context);
    object? Evaluate(IExpression expression, object? input);
}

internal sealed class ExpressionService : IExpressionService
{
    public IExpression CompileOpen(string code, Context context) => Expression.Create(code, context);
    public IExpression CompileClosed(string code, Context context) => Expression.CreateClosed(code, context);
    public object? Evaluate(IExpression expression, object? input) => expression.Evaluate(input);
}
