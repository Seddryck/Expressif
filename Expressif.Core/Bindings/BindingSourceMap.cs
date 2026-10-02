using Expressif.Syntax;

namespace Expressif.Bindings;

internal sealed class BindingSourceMap(bool enabled)
{
    private readonly Dictionary<object, SyntaxNode> nodes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, (RecordAccessSyntax Access, int Index)> fields = new(ReferenceEqualityComparer.Instance);

    public T Add<T>(T bound, SyntaxNode syntax)
        where T : class
    {
        if (enabled)
            nodes.TryAdd(bound, syntax);
        return bound;
    }

    public T AddField<T>(T bound, RecordAccessSyntax syntax, int index)
        where T : class
    {
        if (enabled)
            fields.Add(bound, (syntax, index));
        return Add(bound, syntax);
    }

    public SyntaxNode? Get(object bound) => nodes.GetValueOrDefault(bound);
    public (RecordAccessSyntax Access, int Index)? GetField(object bound)
        => fields.TryGetValue(bound, out var field) ? field : null;
}
