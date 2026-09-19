using Expressif.Bindings;

namespace Expressif.Functions.Record;

/// <summary>Emits one record per selected collection element, preserving parents with empty or null fields.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class ExplodeOuter : Explode
{
    /// <param name="selector">A direct field selector identifying the collection-valued field to replace.</param>
    public ExplodeOuter(NamedFieldSelector selector)
        : base(selector, preserveParent: true) { }
}
