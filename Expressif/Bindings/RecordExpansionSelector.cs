namespace Expressif.Bindings;

public sealed record RecordExpansionSelector(string? Field, Func<object?, object?> Evaluate);
