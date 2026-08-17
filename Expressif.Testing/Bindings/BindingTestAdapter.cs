using Expressif.Bindings;
namespace Expressif.Testing.Bindings;

internal static class BindingTestAdapter
{
    private static ExpressifBinder Binder { get; } = new();

    public static IRootExpression Root(string source) => Binder.Bind(source);
    public static Function Function(string source) => Binder.BindFunction(source);
    public static IParameter Parameter(string source) => Binder.BindParameter(source);
    public static IParameter[] Parameters(string source) => Binder.BindFunction($"test{source}").Parameters;
    public static IPredication Predication(string source) => Binder.BindPredication(source);
    public static OpenExpression Open(string source) => ((OpenRootExpression)Binder.Bind(source)).Expression;
    public static Expressif.Bindings.ClosedExpression Closed(string source)
        => ((ClosedRootExpression)Binder.Bind(source)).Expression;

    public static IntervalBinding Interval(string source)
        => ((IntervalParameter)Binder.BindParameter(source)).Value;
}
