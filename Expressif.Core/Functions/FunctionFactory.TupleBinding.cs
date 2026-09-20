using Expressif.Values;

namespace Expressif.Functions;

public partial class FunctionFactory
{
    /// <summary>Invokes an eligible callable using already-evaluated tuple values.</summary>
    public object? InvokeTuple(string name, IPositionalValue tuple)
        => InvokeTuple(name, tuple, null);

    private Type ResolveTupleTarget(string name, Syntax.SourceSpan? span = null)
        => tupleBinding.ResolveTarget(name, Registry, predicateRegistry, span);

    private object? InvokeTuple(string name, IPositionalValue tuple, Syntax.SourceSpan? span)
        => tupleBinding.Invoke(name, tuple, Registry, predicateRegistry, span);
}
