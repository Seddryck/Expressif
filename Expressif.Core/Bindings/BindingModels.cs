using Expressif.Types;
using Expressif.Discovery;
using Expressif.Collections;

namespace Expressif.Bindings;

public interface IBoundExpression { }
public interface IRootExpression { }
public sealed record OpenRootExpression(OpenExpression Expression) : IRootExpression;
public sealed record ClosedRootExpression(ClosedExpression Expression) : IRootExpression;

internal enum BoundFunctionRole
{
    Operator,
    InputBinding,
    ConditionalForward,
    ConditionalBackward,
    ImplicitAccumulator,
}

internal enum SourceNotation
{
    StandardCall,
    MapShorthand,
    GroupMapShorthand,
    CurrentField,
    RootField,
    EnclosingRootField,
    TupleProjectionShorthand,
    ScopedTupleProjectionShorthand,
    InputFieldShorthand,
    InputTupleProjectionShorthand,
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
    internal Function(
        string name,
        IEnumerable<IParameter> parameters,
        BoundFunctionRole role = BoundFunctionRole.Operator,
        SourceNotation notation = SourceNotation.StandardCall)
        : this(
            OperatorIdentity.Parse(name),
            parameters.Select(x => new FunctionArgument(null, x)),
            role,
            notation,
            FunctionImplementationKind.Unspecified,
            !name.Contains("::", StringComparison.Ordinal)) { }

    internal Function(string name, IEnumerable<IParameter> parameters, SourceNotation notation)
        : this(name, parameters, BoundFunctionRole.Operator, notation) { }

    private Function(
        OperatorIdentity identity,
        IEnumerable<FunctionArgument> arguments,
        BoundFunctionRole role,
        SourceNotation notation,
        FunctionImplementationKind implementationKind,
        bool isUnqualified = false)
    {
        Identity = identity;
        Arguments = ValidateArguments(arguments);
        Role = role;
        Notation = notation;
        ImplementationKind = implementationKind;
        IsUnqualified = isUnqualified;
    }

    public static Function FromArguments(string name, IEnumerable<FunctionArgument> arguments)
        => new(OperatorIdentity.Parse(name), arguments, BoundFunctionRole.Operator, SourceNotation.StandardCall,
            FunctionImplementationKind.Unspecified, !name.Contains("::", StringComparison.Ordinal));

    public static Function FromArguments(OperatorIdentity identity, IEnumerable<FunctionArgument> arguments)
        => new(identity, arguments, BoundFunctionRole.Operator, SourceNotation.StandardCall,
            FunctionImplementationKind.Unspecified);

    public static Function FromParameters(string name, IEnumerable<IParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return FromArguments(name, parameters.Select(parameter => new FunctionArgument(null, parameter)));
    }

    internal static Function FromArguments(
        string name,
        IEnumerable<FunctionArgument> arguments,
        SourceNotation notation)
        => new(OperatorIdentity.Parse(name), arguments, BoundFunctionRole.Operator, notation,
            FunctionImplementationKind.Unspecified, !name.Contains("::", StringComparison.Ordinal));

    internal static Function FromArguments(
        string name,
        IEnumerable<FunctionArgument> arguments,
        BoundFunctionRole role)
        => new(OperatorIdentity.Parse(name), arguments, role, SourceNotation.StandardCall,
            FunctionImplementationKind.Unspecified, !name.Contains("::", StringComparison.Ordinal));

    internal static Function FromArguments(
        OperatorIdentity identity,
        IEnumerable<FunctionArgument> arguments,
        SourceNotation notation,
        FunctionImplementationKind implementationKind)
        => new(identity, arguments, BoundFunctionRole.Operator, notation, implementationKind);

    internal static Function FromArguments(
        string name,
        IEnumerable<FunctionArgument> arguments,
        BoundFunctionRole role,
        SourceNotation notation,
        FunctionImplementationKind implementationKind)
        => new(OperatorIdentity.Parse(name), arguments, role, notation, implementationKind,
            !name.Contains("::", StringComparison.Ordinal));

    internal static Function FromArguments(
        OperatorIdentity identity,
        IEnumerable<FunctionArgument> arguments,
        BoundFunctionRole role,
        SourceNotation notation,
        FunctionImplementationKind implementationKind)
        => new(identity, arguments, role, notation, implementationKind);

    public Expressif.Syntax.SourceSpan? SourceSpan { get; internal set; }
    public OperatorIdentity Identity { get; }
    public string Name => Identity.Name;
    public string Namespace => Identity.Namespace;
    public IReadOnlyList<FunctionArgument> Arguments { get; }
    /// <value>Gets a positional projection that omits argument names and spread markers.</value>
    public IReadOnlyList<IParameter> Parameters => BindingCollections.Freeze(
        Arguments.Select(argument => argument.Value));
    internal BoundFunctionRole Role { get; }
    internal SourceNotation Notation { get; }
    internal FunctionImplementationKind ImplementationKind { get; }
    internal bool IsUnqualified { get; }

    private static IReadOnlyList<FunctionArgument> ValidateArguments(IEnumerable<FunctionArgument> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var snapshot = BindingCollections.Freeze(arguments);
        var named = false;
        foreach (var argument in snapshot)
        {
            ArgumentNullException.ThrowIfNull(argument);
            ArgumentNullException.ThrowIfNull(argument.Value);
            if (argument.Name is { } name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Argument names cannot be empty.", nameof(arguments));
                named = true;
            }
            else if (named)
            {
                throw new ArgumentException("Positional arguments cannot follow named arguments.", nameof(arguments));
            }
        }
        return snapshot;
    }
}

public sealed class OpenExpression(IEnumerable<Function> members) : IBoundExpression
{
    public IReadOnlyList<Function> Members { get; } = BindingCollections.Freeze(members);

    internal OpenExpression(InputBoundExpression inputBinding)
        : this([]) => InputBinding = inputBinding;

    internal InputBoundExpression? InputBinding { get; }
}

public sealed class ClosedExpression(IParameter parameter, IEnumerable<Function> members) : IBoundExpression
{
    public IParameter Parameter { get; } = parameter;
    public IReadOnlyList<Function> Members { get; } = BindingCollections.Freeze(members);
}

public interface IParameter { }
public sealed record LiteralParameter(
    object? Value,
    string? LiteralType = null,
    bool IsLiteralTypeExplicit = false) : IParameter;
public sealed record CallableReferenceParameter(string Name) : IParameter;
public sealed record SortCriterionParameter(IParameter Selector, TypeDescriptor Type, bool Ascending, bool NullsFirst) : IParameter;
public abstract record CoercionSpecificationParameter(Type TargetType) : IParameter
{
    internal abstract bool IsKnownVariant { get; }
}
public sealed record PositionalCoercionParameter(Type TargetType) : CoercionSpecificationParameter(TargetType)
{
    internal override bool IsKnownVariant => true;
}
public sealed record FieldCoercionParameter(string Field, Type TargetType) : CoercionSpecificationParameter(TargetType)
{
    internal override bool IsKnownVariant => true;
}
public sealed record TupleCoercionParameter(int Position, Type TargetType) : CoercionSpecificationParameter(TargetType)
{
    internal override bool IsKnownVariant => true;
}
public sealed record IntervalParameter(IntervalBinding Value) : IParameter;
public sealed record VariableParameter(string Name) : IParameter;
public sealed record ObjectPropertyParameter(string Name) : IParameter;
public sealed record EnclosingObjectPropertyParameter(string Name) : IParameter;
public sealed record ObjectIndexParameter(int Index) : IParameter;
public sealed record TupleProjectionParameter(int Index, bool FromEnd = false) : IParameter;
public sealed record ScopedTupleProjectionParameter(int Index, int ScopeDepth) : IParameter;
internal sealed record ArgumentProviderParameter(Func<ArgumentEvaluationContext, object?> Provider) : IParameter;
public sealed record ArrayElementParameter(IParameter Value, bool IsSpread = false);
public sealed record ArrayParameter : IParameter
{
    public ArrayParameter(IEnumerable<ArrayElementParameter> elements)
        => Elements = BindingCollections.Freeze(elements);

    public ArrayParameter(IEnumerable<IParameter> values)
        : this(values.Select(value => new ArrayElementParameter(value))) { }

    public IReadOnlyList<ArrayElementParameter> Elements { get; }
    public IReadOnlyList<IParameter> Values => BindingCollections.Freeze(Elements.Select(element => element.Value));
    public void Deconstruct(out IReadOnlyList<ArrayElementParameter> elements) => elements = Elements;
}
public sealed record TupleElementParameter(IParameter Value, bool IsSpread = false);
public sealed record TupleParameter : IParameter
{
    public TupleParameter(IEnumerable<TupleElementParameter> elements)
        => Elements = BindingCollections.Freeze(elements);

    public TupleParameter(IEnumerable<IParameter> values)
        : this(values.Select(value => new TupleElementParameter(value))) { }

    public IReadOnlyList<TupleElementParameter> Elements { get; }
    public IReadOnlyList<IParameter> Values => BindingCollections.Freeze(Elements.Select(element => element.Value));
    public void Deconstruct(out IReadOnlyList<TupleElementParameter> elements) => elements = Elements;
}
public sealed record VectorParameter : IParameter
{
    public VectorParameter(IEnumerable<TupleElementParameter> elements)
        => Elements = BindingCollections.Freeze(elements);

    public IReadOnlyList<TupleElementParameter> Elements { get; }
    public void Deconstruct(out IReadOnlyList<TupleElementParameter> elements) => elements = Elements;
}
public sealed record PairParameter(IParameter Key, IParameter Value) : IParameter;
public sealed record GroupingParameter : IParameter
{
    public GroupingParameter(IEnumerable<PairParameter> entries)
        => Entries = BindingCollections.Freeze(entries);

    public IReadOnlyList<PairParameter> Entries { get; }
    public void Deconstruct(out IReadOnlyList<PairParameter> entries) => entries = Entries;
}
public sealed record DictionaryParameter : IParameter
{
    public DictionaryParameter(IEnumerable<PairParameter> entries)
        => Entries = BindingCollections.Freeze(entries);

    public IReadOnlyList<PairParameter> Entries { get; }
    public void Deconstruct(out IReadOnlyList<PairParameter> entries) => entries = Entries;
}
public sealed record QuotedLiteralParameter(string Value) : IParameter;
public sealed record IncomingValueParameter() : IParameter;
public sealed record RecordLiteralField(string Name, IParameter Value);
public sealed record RecordLiteralParameter : IParameter
{
    public RecordLiteralParameter(IEnumerable<RecordLiteralField> fields)
        => Fields = BindingCollections.Freeze(fields);

    public IReadOnlyList<RecordLiteralField> Fields { get; }
    public void Deconstruct(out IReadOnlyList<RecordLiteralField> fields) => fields = Fields;
}
public interface IRecordDefinitionEntry;
public sealed record RecordNamedEntry(string Name, IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordSpreadEntry(IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordDefinitionParameter : IParameter
{
    public RecordDefinitionParameter(IEnumerable<IRecordDefinitionEntry> entries)
        => Entries = BindingCollections.Freeze(entries);

    public IReadOnlyList<IRecordDefinitionEntry> Entries { get; }
    public void Deconstruct(out IReadOnlyList<IRecordDefinitionEntry> entries) => entries = Entries;
}
public sealed record LetBinding(string Name, IParameter Value);
public sealed record LetDefinitionParameter : IParameter
{
    public LetDefinitionParameter(IEnumerable<LetBinding> bindings)
        => Bindings = BindingCollections.Freeze(bindings);

    public IReadOnlyList<LetBinding> Bindings { get; }
    public void Deconstruct(out IReadOnlyList<LetBinding> bindings) => bindings = Bindings;
}
public sealed record WithProjection(string Name, IParameter Value);
public sealed record WithDefinitionParameter : IParameter
{
    public WithDefinitionParameter(IEnumerable<WithProjection> projections, IParameter body)
        => (Projections, Body) = (BindingCollections.Freeze(projections), body);

    public IReadOnlyList<WithProjection> Projections { get; }
    public IParameter Body { get; }
    public void Deconstruct(out IReadOnlyList<WithProjection> projections, out IParameter body)
        => (projections, body) = (Projections, Body);
}
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

internal static class BindingCollections
{
    internal static IReadOnlyList<T> Freeze<T>(IEnumerable<T> values)
        => StructuralReadOnlyList<T>.Create(values);
}
