using Expressif.Functions.Special;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Functions.Coercions;

public sealed class CoercionRegistry
{
    private static readonly Type[] NumericAndBooleanTextSources =
        NumericCoercion.SupportedSourceTypes.Concat([typeof(bool), typeof(string)]).ToArray();

    private static readonly Type[] TextSources =
        NumericCoercion.SupportedSourceTypes
            .Concat([typeof(string), typeof(bool), typeof(DateOnly), typeof(DateTime), typeof(YearMonth)])
            .ToArray();

    public IReadOnlyList<ICoercionDescriptor> Descriptors { get; }

    public CoercionRegistry()
        : this(CreateDefaultDescriptors()) { }

    public CoercionRegistry(IEnumerable<ICoercionDescriptor> descriptors)
        => Descriptors = descriptors.ToArray();

    public bool TryCreate(Type sourceType, Type targetType, out IFunction coercion)
    {
        var descriptor = Descriptors.SingleOrDefault(x => x.Supports(sourceType, targetType));
        if (descriptor is null)
        {
            coercion = null!;
            return false;
        }

        coercion = descriptor.Create(sourceType);
        return true;
    }

    public IEnumerable<CoercionInfo> Describe()
        => Descriptors.SelectMany(descriptor => descriptor.SourceTypes.Select(sourceType =>
        {
            var function = descriptor.Create(sourceType);
            return new CoercionInfo(
                descriptor.Name,
                sourceType,
                descriptor.TargetType,
                function.GetType());
        }));

    private static ICoercionDescriptor[] CreateDefaultDescriptors()
        =>
        [
            new CoercionDescriptor(
                "coerce-numeric",
                typeof(decimal?),
                NumericAndBooleanTextSources,
                sourceType => CreateNumericOrFallback(typeof(CoerceNumeric<>), sourceType, () => new CoerceNumeric())),
            new CoercionDescriptor(
                "coerce-int",
                typeof(int?),
                NumericAndBooleanTextSources,
                sourceType => CreateNumericOrFallback(typeof(CoerceInt<>), sourceType, () => new CoerceInt())),
            new CoercionDescriptor(
                "coerce-boolean",
                typeof(bool?),
                NumericAndBooleanTextSources,
                sourceType => CreateNumericOrFallback(typeof(CoerceBoolean<>), sourceType, () => new CoerceBoolean())),
            new CoercionDescriptor(
                "coerce-text",
                typeof(string),
                TextSources,
                sourceType => CreateNumericOrFallback(typeof(CoerceText<>), sourceType, () => new CoerceText())),
            new CoercionDescriptor(
                "coerce-date",
                typeof(DateOnly?),
                [typeof(DateOnly), typeof(DateTime), typeof(YearMonth), typeof(string)],
                _ => new CoerceDate()),
            new CoercionDescriptor(
                "coerce-time",
                typeof(TimeOnly?),
                [typeof(TimeOnly), typeof(DateTime), typeof(string)],
                _ => new CoerceTime()),
            new CoercionDescriptor(
                "coerce-datetime",
                typeof(DateTime?),
                [typeof(DateTime), typeof(DateOnly), typeof(YearMonth), typeof(string)],
                _ => new CoerceDateTime()),
        ];

    private static IFunction CreateNumericOrFallback(
        Type genericFunctionType,
        Type sourceType,
        Func<IFunction> fallbackFactory)
        => NumericCoercion.IsSupported(sourceType)
            ? (IFunction)Activator.CreateInstance(genericFunctionType.MakeGenericType(sourceType))!
            : fallbackFactory.Invoke();
}
