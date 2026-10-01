using System.Runtime.ExceptionServices;
using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Syntax;
using Expressif.Values.Types;

namespace Expressif.Cli.Expressions;

internal interface ISyntaxService
{
    RootExpressionSyntax Parse(string code);

    IRootExpression Bind(RootExpressionSyntax syntax);

    void Validate(IRootExpression expression, Context context);
}

internal sealed class SyntaxService : ISyntaxService
{
    private static readonly ITypeSource Source = new AssemblyTypeSource(typeof(ExpressionBinder).Assembly);
    private static readonly LogicalPlanner Planner = LogicalPlannerFactory.Create();
    private static readonly LogicalPlanBinder PlanBinder = new(Source, ExpressifTypeRegistry.Instance);

    public RootExpressionSyntax Parse(string code) => ExpressionParser.Parse(code);

    public IRootExpression Bind(RootExpressionSyntax syntax)
    {
        LogicalPlan plan;
        try
        {
            plan = Planner.Build(syntax);
        }
        catch (LogicalPlanningException exception)
        {
            throw new BindingException(exception.Message);
        }

        try
        {
            return PlanBinder.Bind(plan);
        }
        catch (LogicalPlanBindingException exception) when (exception.InnerException is BindingException)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
        catch (LogicalPlanBindingException exception)
        {
            const string prefix = "Operator '";
            const string suffix = "' of kind 'extension' is not registered.";
            if (exception.Message.StartsWith(prefix, StringComparison.Ordinal)
                && exception.Message.EndsWith(suffix, StringComparison.Ordinal))
            {
                var name = exception.Message[prefix.Length..^suffix.Length];
                throw new NotImplementedFunctionException(name);
            }

            throw new BindingException(exception.Message);
        }
    }

    public void Validate(IRootExpression expression, Context context)
        => _ = new FunctionFactory(new AssemblyTypeSource(typeof(ExpressionBinder).Assembly)).Instantiate(expression, context);
}
