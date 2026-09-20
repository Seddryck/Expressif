using Expressif.Bindings;
using Expressif.Library.Text;
using Expressif.Library.Tuple;
using Expressif.Discovery;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Values.Casters;
using System.Reflection;

namespace Expressif.Library.Composition;

internal sealed class TupleBindFunctionConstructor : IFunctionConstructor<Bind>, ITupleFunctionInvoker
{
    private readonly Caster caster = new();
    private readonly IImplementationRegistry? functions;
    private readonly IImplementationRegistry? predicates;

    public TupleBindFunctionConstructor()
    { }

    public TupleBindFunctionConstructor(ITypesProbe probe)
        => (functions, predicates) = (new FunctionRegistry(probe), new PredicateRegistry(probe));

    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Bind), function.Arguments);
        var provider = (Func<string>)constructionContext.CreateParameter(
            bound.Parameters[0],
            typeof(string),
            context);
        if (bound.Parameters[0] is QuotedLiteralParameter literal)
            constructionContext.ResolveTupleTarget(literal.Value, function.SourceSpan);
        return new Bind(provider, (name, tuple) => constructionContext.InvokeTuple(name, tuple, function.SourceSpan));
    }

    public object? Invoke(
        string name,
        IPositionalValue tuple,
        IImplementationRegistry functions,
        IImplementationRegistry predicates,
        Syntax.SourceSpan? sourceSpan = null)
    {
        try
        {
            var type = ResolveTarget(name, functions, predicates);
            if (tuple.Arity == 0)
            {
                throw new TupleBindingException(
                    TupleBindingFailure.InvalidInput,
                    "bind requires at least one tuple position for pipeline input.");
            }

            var arguments = Enumerable.Range(1, tuple.Arity - 1)
                .Select(index => new FunctionArgument(null, new LiteralParameter(tuple.GetPosition(index))))
                .ToArray();
            var binding = TupleBindingCapabilities.Resolve(type, arguments);
            var signature = TupleBindingCapabilities.Describe(type)
                .Single(candidate => candidate.Constructor == binding.Constructor);
            object?[] providers;
            if (signature.Variadic)
            {
                var values = arguments
                    .Select(argument => new ValueArgumentEvaluator(_ => ((LiteralParameter)argument.Value).Value))
                    .ToArray();
                providers = [(Func<ValueArgumentEvaluator[]>)(() => values)];
            }
            else
            {
                providers = binding.Constructor.GetParameters().Zip(binding.Parameters, (parameter, value) =>
                {
                    var target = parameter.ParameterType.GetGenericArguments()[0];
                    var raw = ((LiteralParameter)value).Value;
                    try
                    {
                        var converted = Cast(raw, target);
                        return CreateProvider(converted, target);
                    }
                    catch (Exception exception) when (exception is ArgumentException or InvalidCastException or System.Reflection.TargetInvocationException)
                    {
                        throw new TupleBindingException(
                            TupleBindingFailure.IncompatibleValue,
                            $"Invalid argument '{parameter.Name}' for '{name}': {exception.GetBaseException().Message}");
                    }
                }).ToArray();
            }

            var input = tuple.GetPosition(0);
            var inputs = type.GetInterfaces()
                .Where(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(IFunction<,>))
                .Select(contract => Nullable.GetUnderlyingType(contract.GetGenericArguments()[0])
                    ?? contract.GetGenericArguments()[0])
                .Distinct()
                .ToArray();
            if (input is not null
                && inputs.Length > 0
                && !inputs.Any(target => target.IsInstanceOfType(input))
                && !inputs.Any(target => caster.TryCast(input, target, out _)))
            {
                throw new TupleBindingException(
                    TupleBindingFailure.IncompatibleValue,
                    $"Incompatible pipeline input for '{name}'.");
            }

            var callable = (IFunction)binding.Constructor.Invoke(providers);
            return callable.Evaluate(input);
        }
        catch (TupleBindingException exception)
        {
            throw new TupleBindingException(exception.Failure, exception.Message, sourceSpan);
        }
    }

    public object? Invoke(string name, IPositionalValue tuple, Syntax.SourceSpan? sourceSpan = null)
        => Invoke(name, tuple, RequireFunctions(), RequirePredicates(), sourceSpan);

    public Type ResolveTarget(string name, Syntax.SourceSpan? sourceSpan = null)
        => ResolveTarget(name, RequireFunctions(), RequirePredicates(), sourceSpan);

    Type ITupleFunctionInvoker.ResolveTarget(
        string name,
        IImplementationRegistry functions,
        IImplementationRegistry predicates,
        Syntax.SourceSpan? sourceSpan)
        => ResolveTarget(name, functions, predicates, sourceSpan);

    private IImplementationRegistry RequireFunctions()
        => functions ?? throw new InvalidOperationException("Tuple invocation requires a type probe.");

    private IImplementationRegistry RequirePredicates()
        => predicates ?? throw new InvalidOperationException("Tuple invocation requires a type probe.");

    internal static Type ResolveTarget(
        string name,
        IImplementationRegistry functions,
        IImplementationRegistry predicates,
        Syntax.SourceSpan? span = null)
    {
        if (string.IsNullOrWhiteSpace(name)
            || (!functions.TryResolve(name, out var type) && !predicates.TryResolve(name, out type)))
        {
            throw new TupleBindingException(
                TupleBindingFailure.UnknownTarget,
                $"Unknown tuple-binding target '{name}'.",
                span);
        }
        if (!TupleBindingCapabilities.Describe(type).Any(signature => signature.SupportsTupleBinding))
        {
            throw new TupleBindingException(
                TupleBindingFailure.IneligibleTarget,
                $"Callable '{name}' does not support tuple binding.",
                span);
        }
        return type;
    }

    private object? Cast(object? value, Type targetType)
    {
        var method = typeof(Caster).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(candidate => candidate.Name == nameof(Caster.Cast)
                && candidate.IsGenericMethodDefinition);
        try
        {
            return method.MakeGenericMethod(targetType).Invoke(caster, [value]);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }

    private static object CreateProvider(object? value, Type targetType)
    {
        var method = typeof(TupleBindFunctionConstructor).GetMethod(
            nameof(CreateProviderCore),
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException();
        return method.MakeGenericMethod(targetType).Invoke(null, [value])!;
    }

    private static Func<T?> CreateProviderCore<T>(object? value)
    {
        var typed = (T?)value;
        return () => typed;
    }
}
