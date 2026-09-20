using Expressif.Values.Casters;

namespace Expressif.Library.Coercions;

internal static class CoercionDescriptorSupport
{
    public static Type[] NumericSourceTypes { get; } = NumericCoercion.SupportedSourceTypes
        .SelectMany(sourceType => new[] { sourceType, typeof(Nullable<>).MakeGenericType(sourceType) })
        .ToArray();

    public static IFunction CreateNumericOrFallback(
        Type genericFunctionType,
        Type sourceType,
        Func<IFunction> fallbackFactory)
        => NumericCoercion.IsSupported(Nullable.GetUnderlyingType(sourceType) ?? sourceType)
            ? (IFunction)Activator.CreateInstance(genericFunctionType.MakeGenericType(sourceType))!
            : fallbackFactory.Invoke();
}
