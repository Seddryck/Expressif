namespace Expressif.Functions;

/// <summary>Marks a compatibility alias as deprecated without deprecating its canonical function.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class FunctionAliasLifecycleAttribute(string name, string replacement, string? sunset = null) : Attribute
{
    public string Name { get; } = name;
    public string Replacement { get; } = replacement;
    public string? Sunset { get; } = sunset;
    public string Message => $"{Name} is deprecated; use {Replacement} instead.";
}
