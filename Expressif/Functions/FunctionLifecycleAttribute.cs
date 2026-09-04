namespace Expressif.Functions;

/// <summary>
/// Reflects lifecycle metadata defined by the public function catalog.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class FunctionLifecycleAttribute : Attribute
{
    public FunctionLifecycleAttribute(string? replacement = null, string? sunset = null)
        => (Replacement, Sunset) = (replacement, sunset);

    public bool Deprecated => true;
    public string? Replacement { get; }
    public string? Sunset { get; }
}
