using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Testing;

internal static class TestExpression
{
    internal static ITypesProbe LibraryProbe
        => new AssemblyTypesProbe([typeof(ExpressionBinder).Assembly]);

    public static IExpression Create(string text, IContext? context = null)
        => Expression.Create(text, CreateBinder(context));

    public static IExpression CreateClosed(string text, IContext? context = null)
        => Expression.CreateClosed(text, CreateBinder(context));

    private static ExpressionBinder CreateBinder(IContext? context)
        => context is null ? new ExpressionBinder() : new ExpressionBinder(context);
}

internal sealed class TestExpressionBuilder : ExpressionBuilder
{
    public TestExpressionBuilder(IContext? context = null)
        : base(new FunctionFactory(TestExpression.LibraryProbe), context) { }
}
