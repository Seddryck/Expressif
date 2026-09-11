using Expressif.Syntax;

namespace Expressif.Bindings;

public enum TupleBindingFailure
{
    UnknownTarget,
    IneligibleTarget,
    InvalidInput,
    InvalidArity,
    IncompatibleValue,
}

public sealed class TupleBindingException(TupleBindingFailure failure, string message, SourceSpan? span = null)
    : BindingException(message)
{
    public TupleBindingFailure Failure { get; } = failure;
    public SourceSpan? Span { get; } = span;
}
