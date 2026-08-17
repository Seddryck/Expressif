using Expressif.Bindings;
using Expressif.Syntax;

namespace Expressif.Testing.Parsers;

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
        => throw new BindingException($"Interval syntax '{source}' is not bound in this iteration.");

    public static string FunctionName(string source) => Function(source).Name;

    public static char Delimiter(string source)
    {
        _ = ExpressifSyntax.Parse($"left {source} right");
        return source.Trim().Single();
    }

    public static string Variable(string source) => ((VariableParameter)Parameter(source)).Name;

    public static object Literal(string source) => Parameter(source) switch
    {
        LiteralParameter literal => literal.Value,
        QuotedLiteralParameter quoted => quoted.Value,
        _ => throw new BindingException($"Source '{source}' is not a literal."),
    };
}
