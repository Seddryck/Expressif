namespace Expressif.Bindings;

/// <summary>An expression with explicitly bound invocation input.</summary>
public sealed class InputBoundExpression(string[] names, bool positional, IRootExpression body)
    : OpenExpression([])
{
    public IReadOnlyList<string> Names { get; } = System.Array.AsReadOnly(names);
    public bool IsPositional { get; } = positional;
    public IRootExpression Body { get; } = body;
}
