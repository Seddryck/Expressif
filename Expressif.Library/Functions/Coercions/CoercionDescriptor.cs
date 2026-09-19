namespace Expressif.Functions.Coercions;

public sealed class CoercionDescriptor : ICoercionDescriptor
{
    private Func<Type, IFunction> Factory { get; }

    public string Name { get; }
    public Type TargetType { get; }
    public IReadOnlySet<Type> SourceTypes { get; }

    public CoercionDescriptor(
        string name,
        Type targetType,
        IEnumerable<Type> sourceTypes,
        Func<Type, IFunction> factory)
    {
        Name = name;
        TargetType = targetType;
        SourceTypes = sourceTypes.ToHashSet();
        Factory = factory;
    }

    public bool Supports(Type sourceType, Type targetType)
        => targetType == TargetType && SourceTypes.Contains(sourceType);

    public IFunction Create(Type sourceType)
    {
        if (!SourceTypes.Contains(sourceType))
        {
            throw new ArgumentException(
                $"The source type '{sourceType}' is not supported by coercion '{Name}'.",
                nameof(sourceType));
        }

        return Factory(sourceType);
    }
}
