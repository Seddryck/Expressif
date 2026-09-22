using System.Reflection;

namespace Expressif.Introspection;

/// <summary>A constructor signature and its positional tuple-binding capabilities.</summary>
public sealed record TupleBindingSignature(
    [property: System.Text.Json.Serialization.JsonIgnore] ConstructorInfo Constructor,
    bool SupportsTupleBinding,
    bool Variadic)
{
    public int MinimumArguments => Variadic ? 0 : Constructor.GetParameters().Count(parameter => !parameter.IsOptional);
    public int? MaximumArguments => Variadic ? null : Constructor.GetParameters().Length;
}
