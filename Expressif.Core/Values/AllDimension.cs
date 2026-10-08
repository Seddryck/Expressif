namespace Expressif.Values;

/// <summary>Represents an aggregated grouping dimension, distinct from every ordinary key including null.</summary>
public sealed class AllDimension
{
    public static AllDimension Instance { get; } = new();

    private AllDimension() { }

    /// <summary>Returns the source literal for the aggregated dimension value.</summary>
    public override string ToString() => "#all";
}
