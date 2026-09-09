using Expressif.Syntax;

namespace Expressif.Semantics;

public enum FieldReferenceKind
{
    CurrentInput,
    ExpressionRoot,
    EnclosingExpressionRoot,
}

public enum SemanticSourceKind
{
    ExternalInput,
    Expression,
    Element,
    Unresolved,
}

/// <summary>
/// Identifies a value supplier, not a declaration of a field. An element source
/// identifies its collection through <see cref="Input"/>.
/// </summary>
public sealed record SemanticSource(
    SemanticSourceKind Kind,
    SyntaxNode? Syntax = null,
    SemanticSource? Input = null,
    string? Reason = null,
    SourceSpan? Region = null)
{
    public SourceSpan? Span => Region ?? Syntax?.Span;
}

/// <summary>
/// Describes one selection in a field reference. Span includes the root prefix
/// for the first selection and the separator for subsequent selections.
/// </summary>
public sealed record FieldReference(
    RecordAccessSyntax Syntax,
    int SelectionIndex,
    SourceSpan Span,
    FieldReferenceKind Kind,
    SemanticSource Source,
    SemanticSource ExpressionRoot,
    SemanticSource EnclosingRoot);

/// <summary>
/// Contains source-ordered references and diagnostics. Invalid syntax produces
/// an empty reference list with diagnostics; unsupported bindings are unresolved.
/// </summary>
public sealed record SemanticAnalysis(
    RootExpressionSyntax? Syntax,
    IReadOnlyList<FieldReference> References,
    IReadOnlyList<string> Diagnostics);
