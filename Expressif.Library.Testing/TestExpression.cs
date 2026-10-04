using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Testing;

internal static class TestExpression
{
    internal static ITypeSource LibraryTypeSource
        => new AssemblyTypeSource([typeof(ExpressionBinder).Assembly]);

    public static IExpression Create(string text)
        => new ExpressionFactory(new ExpressionBinder()).Create(text);

    public static IExpression Create(string text, EvaluationContext context)
        => Create(text).WithContext(context);

    public static IExpression CreateClosed(string text)
        => new ExpressionFactory(new ExpressionBinder()).CreateClosed(text);

    public static IExpression CreateClosed(string text, EvaluationContext context)
        => CreateClosed(text).WithContext(context);
}
