using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Expressif.Syntax;
using Expressif.Discovery;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Values.Types;
using RuntimeExpression = Expressif.IExpression;
using RuntimeExpressionFactory = Expressif.Functions.FunctionFactory;

namespace Expressif.Bindings;

/// <summary>
/// Provides the standard syntax-to-runtime binder.
/// </summary>
public sealed class ExpressionBinder : IExpressionBinder
{
    private static readonly ITypeSource BuiltInSource = new AssemblyTypeSource(typeof(ExpressionBinder).Assembly);
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private IContext Context { get; }
    private LogicalPlanner Planner { get; }
    private LogicalPlanBinder PlanBinder { get; }
    private RuntimeExpressionFactory RuntimeFactory { get; }

    public ExpressionBinder()
        : this(new Context()) { }

    public ExpressionBinder(IContext context)
        : this(context, new CompositeTypeSource(BuiltInSource)) { }

    /// <summary>
    /// Creates a binder with the built-in vocabulary and additional extension types.
    /// </summary>
    /// <param name="extensions">The source of extension operator types.</param>
    public ExpressionBinder(ITypeSource extensions)
        : this(new Context(), extensions) { }

    /// <summary>
    /// Creates a binder with the supplied context, built-in vocabulary, and additional extension types.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    /// <param name="extensions">The source of extension operator types.</param>
    public ExpressionBinder(IContext context, ITypeSource extensions)
        : this(context, Compose(extensions)) { }

    private ExpressionBinder(IContext context, CompositeTypeSource source)
        : this(
            context,
            LogicalPlannerFactory.Create(),
            new LogicalPlanBinder(source, ExpressifTypeRegistry.Instance),
            new RuntimeExpressionFactory(source)) { }

    internal ExpressionBinder(
        IContext context,
        LogicalPlanner planner,
        LogicalPlanBinder planBinder,
        RuntimeExpressionFactory runtimeFactory)
        => (Context, Planner, PlanBinder, RuntimeFactory) = (context, planner, planBinder, runtimeFactory);

    /// <summary>
    /// Binds syntax to an executable expression.
    /// </summary>
    public static RuntimeExpression Bind(RootExpressionSyntax syntax)
        => new ExpressionBinder().BindCore(syntax);

    /// <summary>
    /// Binds a logical plan to an executable expression.
    /// </summary>
    public static RuntimeExpression Bind(LogicalPlan plan)
        => new ExpressionBinder().BindCore(plan);

    /// <summary>
    /// Binds syntax to an executable expression that does not require input.
    /// </summary>
    public static RuntimeExpression BindClosed(RootExpressionSyntax syntax)
        => new ExpressionBinder().BindClosedCore(syntax);

    /// <summary>
    /// Binds a logical plan to an executable expression that does not require input.
    /// </summary>
    public static RuntimeExpression BindClosed(LogicalPlan plan)
        => new ExpressionBinder().BindClosedCore(plan);

    RuntimeExpression IExpressionBinder.Bind(RootExpressionSyntax syntax)
        => BindCore(syntax);

    RuntimeExpression IExpressionBinder.Bind(LogicalPlan plan)
        => BindCore(plan);

    RuntimeExpression IExpressionBinder.BindClosed(RootExpressionSyntax syntax)
        => BindClosedCore(syntax);

    RuntimeExpression IExpressionBinder.BindClosed(LogicalPlan plan)
        => BindClosedCore(plan);

    private RuntimeExpression BindCore(RootExpressionSyntax syntax)
        => BindSyntax(syntax, requireClosed: false);

    private RuntimeExpression BindClosedCore(RootExpressionSyntax syntax)
        => BindSyntax(syntax, requireClosed: true);

    private RuntimeExpression BindSyntax(RootExpressionSyntax syntax, bool requireClosed)
    {
        LogicalPlan plan;
        try
        {
            plan = Planner.Build(syntax);
        }
        catch (LogicalPlanningException exception)
        {
            throw TranslatePlanningException(exception);
        }

        try
        {
            return BindPlan(plan, requireClosed);
        }
        catch (LogicalPlanBindingException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
        catch (LogicalPlanBindingException exception)
        {
            if (exception.Message.StartsWith(
                "The logical plan cannot be bound as closed",
                StringComparison.Ordinal))
            {
                throw new ExpressionRequiresInputException(
                    (plan.Pipeline.Items.FirstOrDefault() as LogicalCall)?.Function.Name);
            }

            var unknown = Regex.Match(
                exception.Message,
                "^Operator '([^']+)' of kind 'extension' is not registered\\.$",
                RegexOptions.CultureInvariant,
                RegexTimeout);
            if (unknown.Success)
            {
                throw new NotImplementedFunctionException(unknown.Groups[1].Value);
            }

            throw new BindingException(exception.Message);
        }
    }

    private static Exception TranslatePlanningException(LogicalPlanningException exception)
    {
        if (exception.Message.StartsWith("Duplicate input binding name '", StringComparison.Ordinal))
        {
            return new BindingException(exception.Message);
        }

        var message = Regex.Replace(
            exception.Message,
            " \\(at offset [0-9]+\\)$",
            string.Empty,
            RegexOptions.CultureInvariant,
            RegexTimeout);
        var required = Regex.Match(
            message,
            "^Required parameter '([^']+)' was not supplied(?: to '[^']+')?\\.$",
            RegexOptions.CultureInvariant,
            RegexTimeout);
        if (required.Success)
            return new MissingRequiredParameterException(required.Groups[1].Value);

        var unknown = Regex.Match(
            message,
            "^Function '([^']+)' has no parameter named '([^']+)'\\.$",
            RegexOptions.CultureInvariant,
            RegexTimeout);
        if (unknown.Success)
            return new UnknownParameterNameException(unknown.Groups[1].Value, unknown.Groups[2].Value);

        var duplicate = Regex.Match(
            message,
            "^(?:Parameter|Named argument) '([^']+)' (?:is supplied|was specified) more than once\\.$",
            RegexOptions.CultureInvariant,
            RegexTimeout);
        if (duplicate.Success)
            return new DuplicateNamedArgumentException(duplicate.Groups[1].Value);

        var tooMany = Regex.Match(
            message,
            "^Function '([^']+)' has too many positional arguments\\.$",
            RegexOptions.CultureInvariant,
            RegexTimeout);
        if (tooMany.Success)
            return new TooManyPositionalArgumentsException(tooMany.Groups[1].Value);

        var unsupportedNamed = Regex.Match(
            message,
            "^Function '([^']+)' does not support named arguments\\.$",
            RegexOptions.CultureInvariant,
            RegexTimeout);
        if (unsupportedNamed.Success && unsupportedNamed.Groups[1].Value == "drill-down")
        {
            return new MissingOrUnexpectedParametersFunctionException(
                unsupportedNamed.Groups[1].Value,
                1);
        }

        return new BindingException(message);
    }

    private RuntimeExpression BindCore(LogicalPlan plan)
        => BindPlan(plan, requireClosed: false);

    private RuntimeExpression BindClosedCore(LogicalPlan plan)
        => BindPlan(plan, requireClosed: true);

    private RuntimeExpression BindPlan(LogicalPlan plan, bool requireClosed)
    {
        try
        {
            var bound = requireClosed ? PlanBinder.BindClosed(plan) : PlanBinder.Bind(plan);
            var function = requireClosed
                ? RuntimeFactory.InstantiateClosed(bound, Context)
                : RuntimeFactory.Instantiate(bound, Context);
            return new Expressif.Expression(function);
        }
        catch (LogicalPlanBindingException)
        {
            throw;
        }
        catch (Exception exception) when (exception is BindingException
            or NotImplementedFunctionException
            or ExpressionRequiresInputException
            or ArgumentException
            or InvalidOperationException)
        {
            throw new LogicalPlanBindingException(
                $"The logical plan could not be bound: {exception.Message}",
                exception);
        }
    }

    private static CompositeTypeSource Compose(ITypeSource extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        return new CompositeTypeSource(BuiltInSource, extensions);
    }

    private sealed class CompositeTypeSource(params ITypeSource[] sources) : ITypeSource
    {
        public IEnumerable<Type> GetTypes()
            => sources.SelectMany(source => source.GetTypes()).Distinct();
    }
}
