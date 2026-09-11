namespace Expressif.Values;

/// <summary>Represents an aggregated grouping dimension, distinct from every ordinary key including null.</summary>
public sealed class AllDimension
{
    public static AllDimension Instance { get; } = new();

    private AllDimension() { }

    /// <summary>Returns the display token; this token is not a source literal.</summary>
    public override string ToString() => "#all";
}
