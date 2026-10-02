using Expressif.Values.Types;
using Expressif.Discovery;

namespace Expressif.Bindings;

public interface IBoundExpression { }
public interface IRootExpression { }
public sealed record OpenRootExpression(OpenExpression Expression) : IRootExpression;
public sealed record ClosedRootExpression(ClosedExpression Expression) : IRootExpression;

public enum FunctionSyntax
{
    Standard,
    InputBindingStage,
    ConditionalForward,
    ConditionalBackward,
    MapShorthand,
    GroupMapShorthand,
    FieldShorthand,
    RootFieldShorthand,
    EnclosingRootFieldShorthand,
    TupleProjectionShorthand,
    ScopedTupleProjectionShorthand,
    InputFieldShorthand,
    InputTupleProjectionShorthand,
    ImplicitFoldAccumulator,
}

internal enum FunctionImplementationKind
{
    Unspecified,
    Function,
    Predicate,
    Accumulator,
}

public sealed record FunctionArgument(string? Name, IParameter Value, bool IsSpread = false);

public sealed class Function : IBoundExpression
{
    public Function(string name, IParameter[] parameters, FunctionSyntax syntax = FunctionSyntax.Standard)
        : this(
            OperatorIdentity.Parse(name),
            parameters.Select(x => new FunctionArgument(null, x)).ToArray(),
            syntax,
            FunctionImplementationKind.Unspecified,
            !name.Contains("::", StringComparison.Ordinal)) { }

    private Function(
        OperatorIdentity identity,
        FunctionArgument[] arguments,
        FunctionSyntax syntax,
        FunctionImplementationKind implementationKind,
        bool isUnqualified = false)
        => (Identity, Arguments, Syntax, ImplementationKind, IsUnqualified)
            = (identity, arguments, syntax, implementationKind, isUnqualified);

    internal static Function FromArguments(string name, FunctionArgument[] arguments)
        => new(OperatorIdentity.Parse(name), arguments, FunctionSyntax.Standard,
            FunctionImplementationKind.Unspecified, !name.Contains("::", StringComparison.Ordinal));

    internal static Function FromArguments(OperatorIdentity identity, FunctionArgument[] arguments)
        => new(identity, arguments, FunctionSyntax.Standard, FunctionImplementationKind.Unspecified);

    internal static Function FromArguments(string name, FunctionArgument[] arguments, FunctionSyntax syntax)
        => new(OperatorIdentity.Parse(name), arguments, syntax,
            FunctionImplementationKind.Unspecified, !name.Contains("::", StringComparison.Ordinal));

    internal static Function FromArguments(
        string name,
        FunctionArgument[] arguments,
        FunctionSyntax syntax,
        FunctionImplementationKind implementationKind)
        => new(OperatorIdentity.Parse(name), arguments, syntax, implementationKind,
            !name.Contains("::", StringComparison.Ordinal));

    internal static Function FromArguments(
        OperatorIdentity identity,
        FunctionArgument[] arguments,
        FunctionSyntax syntax,
        FunctionImplementationKind implementationKind)
        => new(identity, arguments, syntax, implementationKind);

    public Expressif.Syntax.SourceSpan? SourceSpan { get; internal set; }
    public OperatorIdentity Identity { get; }
    public string Name => Identity.Name;
    public string Namespace => Identity.Namespace;
    public FunctionArgument[] Arguments { get; }
    public IParameter[] Parameters => Arguments.Select(x => x.Value).ToArray();
    public FunctionSyntax Syntax { get; }
    internal FunctionImplementationKind ImplementationKind { get; }
    internal bool IsUnqualified { get; }
}

public sealed class OpenExpression(IEnumerable<Function> members) : IBoundExpression
{
    public IEnumerable<Function> Members { get; } = members;

    internal OpenExpression(InputBoundExpression inputBinding)
        : this([]) => InputBinding = inputBinding;

    internal InputBoundExpression? InputBinding { get; }
}

public sealed class ClosedExpression(IParameter parameter, IEnumerable<Function> members) : IBoundExpression
{
    public IParameter Parameter { get; } = parameter;
    public IEnumerable<Function> Members { get; } = members;
}

public interface IParameter { }
public sealed record LiteralParameter(
    object? Value,
    string? LiteralType = null,
    bool IsLiteralTypeExplicit = false) : IParameter;
public sealed record CallableReferenceParameter(string Name) : IParameter;
public sealed record SortCriterionParameter(IParameter Selector, TypeDescriptor Type, bool Ascending, bool NullsFirst) : IParameter;
public abstract record CoercionSpecificationParameter(Type TargetType) : IParameter;
public sealed record PositionalCoercionParameter(Type TargetType) : CoercionSpecificationParameter(TargetType);
public sealed record FieldCoercionParameter(string Field, Type TargetType) : CoercionSpecificationParameter(TargetType);
public sealed record TupleCoercionParameter(int Position, Type TargetType) : CoercionSpecificationParameter(TargetType);
public sealed record IntervalParameter(IntervalBinding Value) : IParameter;
public sealed record VariableParameter(string Name) : IParameter;
public sealed record ObjectPropertyParameter(string Name) : IParameter;
public sealed record EnclosingObjectPropertyParameter(string Name) : IParameter;
public sealed record ObjectIndexParameter(int Index) : IParameter;
public sealed record TupleProjectionParameter(int Index, bool FromEnd = false) : IParameter;
public sealed record ScopedTupleProjectionParameter(int Index, int ScopeDepth) : IParameter;
public sealed record ContextParameter(Func<IContext, object?> Function) : IParameter;
public sealed record ArrayElementParameter(IParameter Value, bool IsSpread = false);
public sealed record ArrayParameter(ArrayElementParameter[] Elements) : IParameter
{
    public ArrayParameter(IParameter[] values)
        : this(values.Select(value => new ArrayElementParameter(value)).ToArray()) { }

    public IParameter[] Values => Elements.Select(element => element.Value).ToArray();
}
public sealed record TupleElementParameter(IParameter Value, bool IsSpread = false);
public sealed record TupleParameter(TupleElementParameter[] Elements) : IParameter
{
    public TupleParameter(IParameter[] values)
        : this(values.Select(value => new TupleElementParameter(value)).ToArray()) { }

    public IParameter[] Values => Elements.Select(element => element.Value).ToArray();
}
public sealed record VectorParameter(TupleElementParameter[] Elements) : IParameter;
public sealed record PairParameter(IParameter Key, IParameter Value) : IParameter;
public sealed record GroupingParameter(PairParameter[] Entries) : IParameter;
public sealed record DictionaryParameter(PairParameter[] Entries) : IParameter;
public sealed record QuotedLiteralParameter(string Value) : IParameter;
public sealed record IncomingValueParameter() : IParameter;
public sealed record RecordLiteralField(string Name, IParameter Value);
public sealed record RecordLiteralParameter(RecordLiteralField[] Fields) : IParameter;
public interface IRecordDefinitionEntry;
public sealed record RecordNamedEntry(string Name, IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordSpreadEntry(IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordDefinitionParameter(IRecordDefinitionEntry[] Entries) : IParameter;
public sealed record LetBinding(string Name, IParameter Value);
public sealed record LetDefinitionParameter(LetBinding[] Bindings) : IParameter;
public sealed record WithProjection(string Name, IParameter Value);
public sealed record WithDefinitionParameter(WithProjection[] Projections, IParameter Body) : IParameter;
public sealed record InputExpressionParameter(ClosedExpression Expression) : IParameter;
public sealed record OpenExpressionParameter(OpenExpression Expression) : IParameter;
public sealed record PredicationParameter(IPredication Predication) : IParameter;
public enum IntervalBoundBindingKind
{
    Finite,
    NegativeInfinity,
    PositiveInfinity,
}
public sealed record IntervalBoundBinding(IntervalBoundBindingKind Kind, object? Value = null);
public sealed record IntervalBinding(
    IntervalBoundBinding LowerBound,
    IntervalBoundBinding UpperBound,
    bool IsLowerInclusive,
    bool IsUpperInclusive)
{
    public char LowerBoundType => IsLowerInclusive ? '[' : ']';
    public char UpperBoundType => IsUpperInclusive ? ']' : '[';
}

public interface IPredication { }
public sealed record SinglePredication(Function Member) : IPredication;
public sealed record PipelinePredication(OpenExpression Expression) : IPredication;

internal sealed class UnaryOperator(string name) { public string Name { get; } = name; }
internal sealed class BinaryOperator(string name)
{
    public string Name { get; } = name;
    public static BinaryOperator And => new("And");
    public static BinaryOperator Or => new("Or");
    public static BinaryOperator Xor => new("Xor");
}
internal sealed class UnaryPredication(UnaryOperator @operator, IPredication member) : IPredication
{
    public UnaryOperator Operator { get; } = @operator;
    public IPredication Member { get; } = member;
}

internal sealed class BinaryPredication(BinaryOperator @operator, IPredication left, IPredication right) : IPredication
{
    public BinaryOperator Operator { get; } = @operator;
    public IPredication LeftMember { get; } = left;
    public IPredication RightMember { get; } = right;
}

public sealed record ControlFlowBranchParameter(IParameter Expression, IParameter? Predicate) : IParameter;
