namespace Expressif.Library.Record;

public sealed record ExpansionSelector(string? Field, Func<object?, object?> Evaluate);
