namespace Expressif.Functions;

/// <summary>
/// Reflects lifecycle metadata defined by the public function catalog.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class FunctionLifecycleAttribute : Attribute
{
    public FunctionLifecycleAttribute(
        string? replacement = null,
        string? sunset = null,
        bool replacementIsEquivalent = false,
        string? migrationNotes = null)
        => (Replacement, Sunset, ReplacementIsEquivalent, MigrationNotes)
            = (replacement, sunset, replacementIsEquivalent, migrationNotes);

    public bool Deprecated => true;
    public string? Replacement { get; }
    public string? Sunset { get; }
    public bool ReplacementIsEquivalent { get; }
    public string? MigrationNotes { get; }
}
