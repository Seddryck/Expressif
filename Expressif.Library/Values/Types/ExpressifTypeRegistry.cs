using Expressif.Functions;

namespace Expressif.Values.Types;

/// <summary>
/// Provides the type registry composed from the Expressif Core value model and official Library type descriptors.
/// </summary>
internal static class ExpressifTypeRegistry
{
    public static ITypeRegistry Instance { get; } = new TypeRegistry(
        typeof(ExpressifTypeRegistry).Assembly,
        typeof(IExpression).Assembly);
}
