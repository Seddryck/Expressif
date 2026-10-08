using System.Diagnostics.CodeAnalysis;

namespace Expressif.Functions.Coercions;

/// <summary>
/// Resolves an available coercion between two runtime types.
/// </summary>
public interface ICoercionRegistry
{
    bool TryResolve(
        Type sourceType,
        Type targetType,
        [NotNullWhen(true)] out string? functionName);

    bool TryResolve(
        string functionName,
        [NotNullWhen(true)] out Type? targetType);

    bool TryCreate(Type sourceType, Type targetType, out IFunction coercion);
}
