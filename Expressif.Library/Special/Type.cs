using Expressif.Functions;
using Expressif.Values.Types;

namespace Expressif.Library.Special;

/// <summary>Returns whether the input has the requested Expressif runtime type or belongs to the requested type family, without coercion.</summary>
[Predicate(appendIs: false, prefix: "", name: "is-type")]
[Scope("special")]
public sealed class IsType : BasePredicate
{
    private Func<TypeDescriptor> Expected { get; }

    /// <param name="type">Specifies the Expressif type descriptor to test.</param>
    public IsType(Func<TypeDescriptor> type) => Expected = type;

    public override bool Evaluate(object? value)
        => ExpressifTypeRegistry.Instance.IsInstance(value, Expected.Invoke());
}
