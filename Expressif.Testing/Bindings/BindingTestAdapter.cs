using Expressif.Bindings;
using Expressif.Functions;
namespace Expressif.Testing.Bindings;

internal static class BindingTestAdapter
{
    private static ExpressifBinder Binder { get; } = new();

    public static IRootExpression Root(string source) => Binder.Bind(ExpressionParser.Parse(source));
    public static Function Function(string source) => Binder.BindFunction(ExpressionParser.Parse(source));
    public static IParameter Parameter(string source) => Binder.BindParameter(ExpressionParser.Parse(source));
    public static IParameter[] Parameters(string source) => Binder.BindFunction(ExpressionParser.Parse($"test{source}")).Parameters;
    public static IPredication Predication(string source) => Binder.BindPredication(ExpressionParser.Parse(source));
    public static OpenExpression Open(string source) => ((OpenRootExpression)Root(source)).Expression;
    public static Expressif.Bindings.ClosedExpression Closed(string source)
        => ((ClosedRootExpression)Root(source)).Expression;

    public static IntervalBinding Interval(string source)
        => ((IntervalParameter)Parameter(source)).Value;

    public static IFunction Executable(string source, IContext? context = null)
        => new FunctionFactory().Instantiate(Root(source), context ?? new Context());

    public static IFunction ExecutableClosed(string source, IContext? context = null)
        => new FunctionFactory().InstantiateClosed(Root(source), context ?? new Context());
}
