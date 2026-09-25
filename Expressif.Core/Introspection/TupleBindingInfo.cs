using System.Reflection;

namespace Expressif.Introspection;

/// <summary>Describes the positional tuple-binding capabilities of one callable signature.</summary>
public sealed class TupleBindingInfo
{
    internal TupleBindingInfo(
        bool supportsTupleBinding,
        bool variadic,
        int minimumArguments,
        int? maximumArguments)
        => (SupportsTupleBinding, Variadic, MinimumArguments, MaximumArguments) = (
            supportsTupleBinding,
            variadic,
            minimumArguments,
            maximumArguments);

    public bool SupportsTupleBinding { get; }
    public bool Variadic { get; }
    public int MinimumArguments { get; }
    public int? MaximumArguments { get; }
}

internal sealed class TupleBindingSignature
{
    internal TupleBindingSignature(ConstructorInfo constructor, bool supportsTupleBinding, bool variadic)
        => (Constructor, SupportsTupleBinding, Variadic) = (constructor, supportsTupleBinding, variadic);

    internal ConstructorInfo Constructor { get; }
    internal bool SupportsTupleBinding { get; }
    internal bool Variadic { get; }
    internal int MinimumArguments
        => Variadic ? 0 : Constructor.GetParameters().Count(parameter => !parameter.IsOptional);
    internal int? MaximumArguments => Variadic ? null : Constructor.GetParameters().Length;

    internal TupleBindingInfo ToInfo()
        => new(SupportsTupleBinding, Variadic, MinimumArguments, MaximumArguments);
}
