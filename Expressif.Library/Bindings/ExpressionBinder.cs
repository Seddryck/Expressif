using Expressif.Syntax;
using Expressif.Discovery;
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

    private IContext Context { get; }
    private ExpressifBinder SyntaxBinder { get; }
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
            ExpressifBinderFactory.Create(),
            new LogicalPlanBinder(source, ExpressifTypeRegistry.Instance),
            new RuntimeExpressionFactory(source)) { }

    internal ExpressionBinder(
        IContext context,
        ExpressifBinder syntaxBinder,
        LogicalPlanBinder planBinder,
        RuntimeExpressionFactory runtimeFactory)
        => (Context, SyntaxBinder, PlanBinder, RuntimeFactory) = (context, syntaxBinder, planBinder, runtimeFactory);

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
        => new Expressif.Expression(RuntimeFactory.Instantiate(SyntaxBinder.Bind(syntax), Context));

    private RuntimeExpression BindClosedCore(RootExpressionSyntax syntax)
        => new Expressif.Expression(RuntimeFactory.InstantiateClosed(SyntaxBinder.Bind(syntax), Context));

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
