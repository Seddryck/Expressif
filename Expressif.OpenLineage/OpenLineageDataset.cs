namespace Expressif.OpenLineage;

/// <summary>
/// Identifies a dataset using the namespace and name conventions of its source.
/// </summary>
public sealed record OpenLineageDataset(string Namespace, string Name)
{
    /// <summary>
    /// Identifies a local file by its absolute, escaped file URI.
    /// </summary>
    public static OpenLineageDataset FromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var uri = new Uri(new Uri(Path.GetFullPath(path)).AbsoluteUri);
        return new OpenLineageDataset("file://" + uri.Host, "/" + uri.AbsolutePath.TrimStart('/'));
    }
}
