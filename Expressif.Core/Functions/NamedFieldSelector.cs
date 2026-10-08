namespace Expressif.Functions;

public sealed record NamedFieldSelector(string Name, Func<object?, object?> Evaluate);
