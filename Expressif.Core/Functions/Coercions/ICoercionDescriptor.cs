namespace Expressif.Functions.Coercions;

public interface ICoercionDescriptor
{
    string Name { get; }
    Type TargetType { get; }
    IReadOnlySet<Type> SourceTypes { get; }

    bool Supports(Type sourceType, Type targetType);
    Type GetImplementationType(Type sourceType);
    IFunction Create(Type sourceType);
}
