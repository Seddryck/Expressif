namespace Expressif.Semantics;

// These operations are shared by runtime frames and symbolic analysis. Pipeline
// advancement never derives a frame; entering a nested evaluation does.
internal sealed record ScopeFrame<T>(T Current, T Root, ScopeFrame<T>? Parent = null)
{
    public ScopeFrame<T> Derive(T input) => new(input, input, this);
    public T? EnclosingRoot => Parent is null ? default : Parent.Root;

    public T Resolve(FieldReferenceKind kind, T input, T missing)
        => kind switch
        {
            FieldReferenceKind.CurrentInput => input,
            FieldReferenceKind.ExpressionRoot => Root,
            FieldReferenceKind.EnclosingExpressionRoot => Parent is null ? missing : Parent.Root,
            _ => missing,
        };
}

internal sealed record DirectionalScope<T>(T Input, T Arguments)
{
    public static DirectionalScope<T> Create(bool mapOver, T outer, T item)
        => new(mapOver ? outer : item, item);
}

internal static class ArgumentScope
{
    public static T Root<T>(T? contextInput, T root)
        where T : class?
        => contextInput ?? root;
}
