namespace Expressif.Functions.Coercions;

public abstract class CoercionDescriptor : ICoercionDescriptor
{
    private Func<Type, IFunction> Factory { get; }
    private Func<Type, Type> ImplementationType { get; }

    public string Name { get; }
    public Type TargetType { get; }
    public IReadOnlySet<Type> SourceTypes { get; }

    protected CoercionDescriptor(
        string name,
        Type targetType,
        IEnumerable<Type> sourceTypes,
        Func<Type, Type> implementationType,
        Func<Type, IFunction> factory)
    {
        Name = name;
        TargetType = targetType;
        SourceTypes = sourceTypes.ToHashSet();
        ImplementationType = implementationType;
        Factory = factory;
    }

    public bool Supports(Type sourceType, Type targetType)
        => targetType == TargetType && SourceTypes.Contains(sourceType);

    public Type GetImplementationType(Type sourceType)
    {
        EnsureSourceTypeIsSupported(sourceType);
        return ImplementationType(sourceType);
    }

    public IFunction Create(Type sourceType)
    {
        EnsureSourceTypeIsSupported(sourceType);
        return Factory(sourceType);
    }

    private void EnsureSourceTypeIsSupported(Type sourceType)
    {
        if (!SourceTypes.Contains(sourceType))
        {
            throw new ArgumentException(
                $"The source type '{sourceType}' is not supported by coercion '{Name}'.",
                nameof(sourceType));
        }
    }
}
