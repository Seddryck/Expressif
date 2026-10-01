using Expressif.Bindings;

namespace Expressif.Library.Record;

/// <summary>Groups records by all non-selected fields and collects selected values in source order.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class Implode : StructuralImplode
{
    /// <param name="selector">A direct field selector identifying the field whose values are collected.</param>
    public Implode([AcceptedExpressionShape(AcceptedExpressionShape.DirectFieldSelector)] NamedFieldSelector selector)
        : base(selector, ignoreNull: false) { }
}
