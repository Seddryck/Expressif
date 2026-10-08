namespace Expressif.Functions.Accumulation;

/// <summary>Accumulates items for one evaluation of an incremental aggregation.</summary>
/// <remarks>
/// A session is ready for use when it is created. <see cref="Snapshot"/> is non-terminal:
/// callers may add more items and take further snapshots. A snapshot must remain stable when
/// the session subsequently changes.
/// </remarks>
public interface IAggregationSession
{
    /// <summary>Adds one item to the current aggregation state.</summary>
    void Add(object? item);

    /// <summary>Projects a stable result from all items added so far.</summary>
    object? Snapshot();
}
