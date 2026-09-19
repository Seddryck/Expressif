using Expressif.Bindings;

namespace Expressif.Functions.Record;

/// <summary>Groups records by all non-selected fields and collects non-null selected values in source order.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class ImplodeInner : StructuralImplode
{
    /// <param name="selector">A direct field selector identifying the field whose values are collected.</param>
    public ImplodeInner(NamedFieldSelector selector)
        : base(selector, ignoreNull: true) { }
}
