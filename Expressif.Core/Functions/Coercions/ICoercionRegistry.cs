using System.Diagnostics.CodeAnalysis;

namespace Expressif.Functions.Coercions;

/// <summary>
/// Resolves an available coercion between two runtime types.
/// </summary>
public interface ICoercionRegistry
{
    IReadOnlyList<ICoercionDescriptor> Descriptors { get; }

    bool TryResolve(
        Type sourceType,
        Type targetType,
        [NotNullWhen(true)] out string? functionName);

    IEnumerable<CoercionInfo> Describe()
        => Descriptors.SelectMany(descriptor => descriptor.SourceTypes.Select(sourceType =>
        {
            var function = descriptor.Create(sourceType);
            return new CoercionInfo(
                descriptor.Name,
                sourceType,
                descriptor.TargetType,
                function.GetType());
        }));
}
