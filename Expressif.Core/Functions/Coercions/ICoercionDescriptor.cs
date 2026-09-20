namespace Expressif.Functions.Coercions;

public interface ICoercionDescriptor
{
    string Name { get; }
    Type TargetType { get; }
    IReadOnlySet<Type> SourceTypes { get; }

    bool Supports(Type sourceType, Type targetType);
    IFunction Create(Type sourceType);
}

public sealed record CoercionInfo(
    string Name,
    Type SourceType,
    Type TargetType,
    Type ImplementationType);
