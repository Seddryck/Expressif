using System.Reflection;
using Expressif.Bindings;

namespace Expressif.Functions;

/// <summary>A signature-level capability shared by runtime binding and semantic tooling.</summary>
public sealed record TupleBindingSignature([property: System.Text.Json.Serialization.JsonIgnore] ConstructorInfo Constructor, bool SupportsTupleBinding, bool Variadic)
{
    public int MinimumArguments => Variadic ? 0 : Constructor.GetParameters().Count(parameter => !parameter.IsOptional);
    public int? MaximumArguments => Variadic ? null : Constructor.GetParameters().Length;
}

public static class TupleBindingCapabilities
{
    public static IReadOnlyList<TupleBindingSignature> Describe(Type type)
        => type.GetConstructors().Select(constructor =>
        {
            var parameters = constructor.GetParameters();
            var variadic = typeof(IValueSpreadAware).IsAssignableFrom(type)
                && parameters is [var values] && values.ParameterType == typeof(Func<ValueArgumentEvaluator[]>);
            var standard = FunctionConstruction.Classify(type.Name) == FunctionConstructionKind.Standard
                && parameters.All(parameter => IsValueProvider(parameter.ParameterType));
            return new TupleBindingSignature(constructor, variadic || standard, variadic);
        }).ToArray();

    private static bool IsValueProvider(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>)
            && IsValueType(type.GetGenericArguments()[0]);

    private static bool IsValueType(Type type)
        => !typeof(Delegate).IsAssignableFrom(type)
            && !typeof(IFunction).IsAssignableFrom(type)
            && !typeof(Accumulators.IAccumulator).IsAssignableFrom(type)
            && type != typeof(ValueArgumentEvaluator)
            && (!type.IsArray || (type.GetElementType() is { } element && IsValueType(element)))
            && (!type.IsGenericType || type.GetGenericArguments().All(IsValueType));

    internal static ParameterArgumentBinding Resolve(Type type, FunctionArgument[] arguments)
    {
        var eligible = Describe(type).Where(signature => signature.SupportsTupleBinding).ToArray();
        if (eligible.Length == 0)
            throw new TupleBindingException(TupleBindingFailure.IneligibleTarget, $"Callable '{type.Name}' does not support tuple binding.");
        var variadic = eligible.SingleOrDefault(signature => signature.Variadic);
        if (variadic is not null)
            return new(variadic.Constructor, arguments.Select(argument => argument.Value).ToArray());
        try
        {
            return ParameterArgumentBinder.Bind(type, arguments, eligible.Select(signature => signature.Constructor).ToArray());
        }
        catch (BindingException exception)
        {
            throw new TupleBindingException(TupleBindingFailure.InvalidArity, $"Invalid tuple invocation of '{type.Name}': {exception.Message}");
        }
    }
}
