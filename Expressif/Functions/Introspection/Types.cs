using Expressif.Types;

namespace Expressif.Functions.Introspection;

/// <summary>
/// Returns the canonical Expressif type descriptors accepted by type literals.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("introspection")]
public sealed class Types : IFunction<object?, TypeDescriptor[]>
{
    public TypeDescriptor[] Evaluate(object? value) => [.. TypeRegistry.All.OrderBy(type => type.Name)];

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
