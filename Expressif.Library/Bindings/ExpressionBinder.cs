using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Expressif.Syntax;
using Expressif.Discovery;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Types;
using Expressif.Values;
using Expressif.Functions.Coercions;
using Expressif.Hosting;
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
    private ITypeRegistry Types { get; }
    private IValueConverter Converter { get; }

    public ExpressionBinder()
        : this(new Context()) { }

    internal ExpressionBinder(IContext context)
        : this(context, new CompositeTypeSource(BuiltInSource)) { }

    /// <summary>Creates a binder with the supplied immutable library environment.</summary>
    public ExpressionBinder(ExpressifEnvironment environment)
        : this(new Context(), environment) { }

    /// <summary>Creates a binder with the supplied context and immutable library environment.</summary>
    internal ExpressionBinder(IContext context, ExpressifEnvironment environment)
        : this(
            context,
            LogicalPlannerFactory.Create(
                RequireEnvironment(environment).Catalog,
                environment.Source,
                environment.Types,
                environment.QuotedLiterals),
            new LogicalPlanBinder(environment.Source, environment.Types, environment.QuotedLiterals),
            new RuntimeExpressionFactory(environment.Source),
            environment.Types,
            TypeSourceService.Create<IValueConverter>(environment.Source)) { }

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
    internal ExpressionBinder(IContext context, ITypeSource extensions)
        : this(context, Compose(extensions)) { }

    private ExpressionBinder(IContext context, CompositeTypeSource source)
        : this(
            context,
            LogicalPlannerFactory.Create(),
            new LogicalPlanBinder(source, ExpressifTypeRegistry.Instance),
            new RuntimeExpressionFactory(source),
            ExpressifTypeRegistry.Instance,
            TypeSourceService.Create<IValueConverter>(source)) { }

    internal ExpressionBinder(
        IContext context,
        LogicalPlanner planner,
        LogicalPlanBinder planBinder,
        RuntimeExpressionFactory runtimeFactory,
        ITypeRegistry types,
        IValueConverter converter)
        => (Context, Planner, PlanBinder, RuntimeFactory, Types, Converter) =
            (context, planner, planBinder, runtimeFactory, types, converter);

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
            LogicalNamedExpressionValidator.Validate(plan);
            if (plan.Definitions.Count > 0)
                return BindNamedExpressionDocument(plan, requireClosed);
            return new Expressif.Expression(BindFunction(plan, requireClosed));
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

    private IFunction BindFunction(LogicalPlan plan, bool requireClosed)
    {
        var bound = requireClosed ? PlanBinder.BindClosed(plan) : PlanBinder.Bind(plan);
        return requireClosed
            ? RuntimeFactory.InstantiateClosed(bound, Context)
            : RuntimeFactory.Instantiate(bound, Context);
    }

    private RuntimeExpression BindNamedExpressionDocument(LogicalPlan plan, bool requireClosed)
    {
        foreach (var definition in plan.Definitions)
            ValidateContracts(definition);
        var functions = plan.Definitions.ToDictionary(
            definition => definition.Name,
            definition => BindFunction(new LogicalPlan(definition.Body), requireClosed: false),
            StringComparer.Ordinal);
        var definitions = plan.Definitions.ToDictionary(
            definition => definition.Name,
            definition => CreateInvoker(definition, functions[definition.Name]),
            StringComparer.Ordinal);
        IFunction? entry = null;
        if (plan.HasEntry)
            entry = BindFunction(new LogicalPlan(plan.Pipeline), requireClosed);
        return new NamedExpressionDocument(entry, definitions);
    }

    private void ValidateContracts(LogicalNamedExpressionDefinition definition)
    {
        var contracts = definition.EffectiveParameters.Select(parameter => (parameter.Name, parameter.Contract))
            .Concat(definition.EffectiveReceivers.Select(receiver => (receiver.Name, receiver.Contract)))
            .Append(("input", definition.InputContract))
            .Append(("output", definition.OutputContract));
        foreach (var (boundary, contract) in contracts.Where(item => item.Item2 is not null))
        {
            if (!Types.TryResolve(contract!.Type, out _))
            {
                throw new LogicalPlanBindingException(
                    $"Named expression '{definition.Name}' has unknown type contract ':{contract.Type}' "
                    + $"for '{boundary}'.");
            }
        }
    }

    private NamedExpressionInvoker CreateInvoker(LogicalNamedExpressionDefinition definition, IFunction body)
        => (input, arguments) =>
        {
            var parameters = definition.EffectiveParameters;
            var required = parameters.Count(parameter => parameter.Default is null);
            if (arguments.Count < required || arguments.Count > parameters.Count)
            {
                throw new ArgumentException(
                    $"Named expression '{definition.Name}' expects between {required} and {parameters.Count} arguments "
                    + $"but received {arguments.Count}.");
            }

            var boundInput = ApplyContract(definition.Name, "input", input, definition.InputContract);
            var bindings = new Dictionary<string, object?>(StringComparer.Ordinal);
            if (definition.EffectiveReceivers.Count > 0)
            {
                if (boundInput is not IPositionalValue positional)
                {
                    throw new ArgumentException(
                        "Positional input binding requires a tuple, pair, group, or vector; received "
                        + (boundInput?.GetType().Name ?? "null") + ".");
                }
                if (positional.Arity != definition.EffectiveReceivers.Count)
                {
                    throw new ArgumentException(
                        $"Positional input binding expects {definition.EffectiveReceivers.Count} components "
                        + $"but received {positional.Arity}.");
                }
                for (var index = 0; index < definition.EffectiveReceivers.Count; index++)
                {
                    var receiver = definition.EffectiveReceivers[index];
                    bindings.Add(receiver.Name, ApplyContract(
                        definition.Name,
                        receiver.Name,
                        positional.GetPosition(index),
                        receiver.Contract));
                }
            }

            for (var index = 0; index < parameters.Count; index++)
            {
                var parameter = parameters[index];
                var value = index < arguments.Count ? arguments[index] : parameter.Default!.Value;
                bindings.Add(parameter.Name, ApplyContract(definition.Name, parameter.Name, value, parameter.Contract));
            }

            using var scope = EvaluationRuntime.BindNamedExpression(boundInput, bindings);
            var result = EvaluationRuntime.CaptureDeferredResult(body.Evaluate(boundInput));
            return ApplyContract(definition.Name, "output", result, definition.OutputContract);
        };

    private object? ApplyContract(
        string definition,
        string boundary,
        object? value,
        LogicalTypeContract? contract)
    {
        if (contract is null)
            return value;
        var descriptor = Types.Resolve(contract.Type);
        if (contract.Strict)
        {
            if (!Types.IsInstance(value, descriptor))
            {
                throw new InvalidOperationException(
                    $"Named expression '{definition}' requires '{boundary}' to be ':{descriptor.Name}', "
                    + $"but received '{value?.GetType().Name ?? "null"}'.");
            }
            return value;
        }

        return descriptor.RuntimeType is null ? value : Converter.Convert(value, descriptor.RuntimeType);
    }

    private static CompositeTypeSource Compose(ITypeSource extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        return new CompositeTypeSource(BuiltInSource, extensions);
    }

    private static ExpressifEnvironment RequireEnvironment(ExpressifEnvironment? environment)
        => environment ?? throw new ArgumentNullException(nameof(environment));

    private sealed class CompositeTypeSource(params ITypeSource[] sources) : ITypeSource
    {
        public IEnumerable<Type> GetTypes()
            => sources.SelectMany(source => source.GetTypes()).Distinct();
    }
}
