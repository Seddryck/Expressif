namespace Expressif.Cli.Expressions;

using Expressif.Observability;
using Expressif.Planning;

internal interface IExpressionService
{
    IExpression CompileOpen(string code, Context context);
    IExpression CompileClosed(string code, Context context);
    IExpression CompileOpen(LogicalPlan plan, Context context);
    IExpression CompileClosed(LogicalPlan plan, Context context);
    IExpression CompileOpen(string code, Context context, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? CompileOpen(code, context) : throw new NotSupportedException();
    IExpression CompileClosed(string code, Context context, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? CompileClosed(code, context) : throw new NotSupportedException();
    IExpression CompileOpen(LogicalPlan plan, Context context, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? CompileOpen(plan, context) : throw new NotSupportedException();
    IExpression CompileClosed(LogicalPlan plan, Context context, IReadOnlyList<IFunctionObserver> observers)
        => observers.Count == 0 ? CompileClosed(plan, context) : throw new NotSupportedException();
    object? Evaluate(IExpression expression, object? input);
}

internal sealed class ExpressionService : IExpressionService
{
    public IExpression CompileOpen(string code, Context context)
        => Expression.Create(code, new Bindings.ExpressionBinder(context));

    public IExpression CompileClosed(string code, Context context)
        => Expression.CreateClosed(code, new Bindings.ExpressionBinder(context));

    public IExpression CompileOpen(LogicalPlan plan, Context context)
        => ((Bindings.IExpressionBinder)new Bindings.ExpressionBinder(context)).Bind(plan);

    public IExpression CompileClosed(LogicalPlan plan, Context context)
        => ((Bindings.IExpressionBinder)new Bindings.ExpressionBinder(context)).BindClosed(plan);

    public IExpression CompileOpen(string code, Context context, IReadOnlyList<IFunctionObserver> observers)
        => new ExpressionFactory(new Bindings.ExpressionBinder(context))
            .WithFunctionObservers(observers).Create(code);

    public IExpression CompileClosed(string code, Context context, IReadOnlyList<IFunctionObserver> observers)
        => new ExpressionFactory(new Bindings.ExpressionBinder(context))
            .WithFunctionObservers(observers).CreateClosed(code);

    public IExpression CompileOpen(LogicalPlan plan, Context context, IReadOnlyList<IFunctionObserver> observers)
        => ((Bindings.IExpressionBinder)new Bindings.ExpressionBinder(context)).Bind(plan, observers);

    public IExpression CompileClosed(LogicalPlan plan, Context context, IReadOnlyList<IFunctionObserver> observers)
        => ((Bindings.IExpressionBinder)new Bindings.ExpressionBinder(context)).BindClosed(plan, observers);

    public object? Evaluate(IExpression expression, object? input) => expression.Evaluate(input);
}
