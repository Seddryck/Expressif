namespace Expressif.Bindings;

public sealed record NamedFieldSelector(string Name, Func<object?, object?> Evaluate);
