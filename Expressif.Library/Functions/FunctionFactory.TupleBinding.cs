using Expressif.Bindings;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Functions;

public partial class FunctionFactory
{
    private IFunction BuildTupleBind(Bindings.Function function, IContext context)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Tuple.Bind), function.Arguments);
        var provider = (Func<string>)CreateParameter(bound.Parameters[0], typeof(string), context);
        if (bound.Parameters[0] is QuotedLiteralParameter literal)
            ResolveTupleTarget(literal.Value, function.SourceSpan);
        return new Tuple.Bind(provider, (name, tuple) =>
        {
            try { return InvokeTuple(name, tuple); }
            catch (TupleBindingException exception)
            {
                throw new TupleBindingException(exception.Failure, exception.Message, function.SourceSpan);
            }
        });
    }

    private Type ResolveTupleTarget(string name, Syntax.SourceSpan? span = null)
    {
        if (string.IsNullOrWhiteSpace(name)
            || (!TypeMapper.TryExecute(name, out var type) && !PredicateTypeMapper.TryExecute(name, out type)))
            throw new TupleBindingException(TupleBindingFailure.UnknownTarget, $"Unknown tuple-binding target '{name}'.", span);
        if (!TupleBindingCapabilities.Describe(type).Any(signature => signature.SupportsTupleBinding))
            throw new TupleBindingException(TupleBindingFailure.IneligibleTarget, $"Callable '{name}' does not support tuple binding.", span);
        return type;
    }

    /// <summary>Invokes an eligible callable using already-evaluated tuple values.</summary>
    public object? InvokeTuple(string name, IPositionalValue tuple)
    {
        var type = ResolveTupleTarget(name);
        if (tuple.Arity == 0)
            throw new TupleBindingException(TupleBindingFailure.InvalidInput, "bind requires at least one tuple position for pipeline input.");
        var arguments = Enumerable.Range(1, tuple.Arity - 1)
            .Select(index => new FunctionArgument(null, new LiteralParameter(tuple.GetPosition(index)))).ToArray();
        var binding = TupleBindingCapabilities.Resolve(type, arguments);
        var signature = TupleBindingCapabilities.Describe(type).Single(candidate => candidate.Constructor == binding.Constructor);
        object?[] providers;
        if (signature.Variadic)
        {
            var values = arguments.Select(argument => new ValueArgumentEvaluator(_ => ((LiteralParameter)argument.Value).Value)).ToArray();
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
                    // Freeze conversion before invocation; providers never reinterpret values or capture a frame.
                    var converted = CreateFunctionCast(() => raw, target).DynamicInvoke();
                    return (object)CreateFunctionCast(() => converted, target);
                }
                catch (Exception exception) when (exception is ArgumentException or System.Reflection.TargetInvocationException)
                {
                    throw new TupleBindingException(TupleBindingFailure.IncompatibleValue,
                        $"Invalid argument '{parameter.Name}' for '{name}': {exception.GetBaseException().Message}");
                }
            }).ToArray();
        }
        var input = tuple.GetPosition(0);
        var inputs = type.GetInterfaces().Where(contract => contract.IsGenericType
            && contract.GetGenericTypeDefinition() == typeof(IFunction<,>))
            .Select(contract => Nullable.GetUnderlyingType(contract.GetGenericArguments()[0]) ?? contract.GetGenericArguments()[0]).Distinct().ToArray();
        if (input is not null && inputs.Length > 0 && !inputs.Any(target => target.IsInstanceOfType(input)))
        {
            var caster = new Caster();
            if (!inputs.Any(target => caster.TryCast(input, target, out _)))
                throw new TupleBindingException(TupleBindingFailure.IncompatibleValue, $"Incompatible pipeline input for '{name}'.");
        }
        var callable = (IFunction)binding.Constructor.Invoke(providers);
        return callable.Evaluate(input);
    }
}
