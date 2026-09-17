namespace Expressif.Accumulators;

/// <summary>Marks a compatibility alias as deprecated without deprecating its canonical accumulator.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class AccumulatorAliasLifecycleAttribute(string name, string replacement) : Attribute
{
    public string Name { get; } = name;
    public string Replacement { get; } = replacement;
    public string Message => $"{Name} is deprecated; use {Replacement} instead.";
}
