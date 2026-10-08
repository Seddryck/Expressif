namespace Expressif.Cli.Expressions;

using Expressif.Planning;
using Expressif.Functions;

internal interface IExpressionService
{
    IExpression CompileOpen(string code, Context context);
    IExpression CompileClosed(string code, Context context);
    IExpression CompileOpen(LogicalPlan plan, Context context);
    IExpression CompileClosed(LogicalPlan plan, Context context);
    object? Evaluate(IExpression expression, object? input);
}

internal sealed class ExpressionService : IExpressionService
{
    public IExpression CompileOpen(string code, Context context)
        => new ExpressionFactory(new Bindings.ExpressionBinder(context)).Create(code);

    public IExpression CompileClosed(string code, Context context)
        => new ExpressionFactory(new Bindings.ExpressionBinder(context)).CreateClosed(code);

    public IExpression CompileOpen(LogicalPlan plan, Context context)
        => ((Bindings.IExpressionBinder)new Bindings.ExpressionBinder(context)).Bind(plan);

    public IExpression CompileClosed(LogicalPlan plan, Context context)
        => ((Bindings.IExpressionBinder)new Bindings.ExpressionBinder(context)).BindClosed(plan);

    public object? Evaluate(IExpression expression, object? input) => expression.Evaluate(input);
}
