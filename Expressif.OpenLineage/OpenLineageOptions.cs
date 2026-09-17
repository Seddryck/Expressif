namespace Expressif.OpenLineage;

/// <summary>
/// Describes one Expressif job and its reliably identified datasets.
/// </summary>
public sealed class OpenLineageOptions
{
    public string Namespace { get; init; } = "expressif";

    public string JobName { get; init; } = "expression";

    public required string Expression { get; init; }

    public IReadOnlyList<OpenLineageDataset> Inputs { get; init; } = [];

    public IReadOnlyList<OpenLineageDataset> Outputs { get; init; } = [];
}
