namespace Expressif.Bindings;

public interface IBoundExpression { }
public interface IRootExpression { }
public sealed record OpenRootExpression(OpenExpression Expression) : IRootExpression;
public sealed record ClosedRootExpression(ClosedExpression Expression) : IRootExpression;

public enum FunctionSyntax
{
    Standard,
    MapShorthand,
    FieldShorthand,
    TupleProjectionShorthand,
}

public sealed record FunctionArgument(string? Name, IParameter Value, bool IsSpread = false);

public sealed class Function : IBoundExpression
{
    public Function(string name, IParameter[] parameters, FunctionSyntax syntax = FunctionSyntax.Standard)
        : this(name, parameters.Select(x => new FunctionArgument(null, x)).ToArray(), syntax) { }

    private Function(string name, FunctionArgument[] arguments, FunctionSyntax syntax)
        => (Name, Arguments, Syntax) = (name, arguments, syntax);

    internal static Function FromArguments(string name, FunctionArgument[] arguments)
        => new(name, arguments, FunctionSyntax.Standard);

    public string Name { get; }
    public FunctionArgument[] Arguments { get; }
    public IParameter[] Parameters => Arguments.Select(x => x.Value).ToArray();
    public FunctionSyntax Syntax { get; }
}

public class OpenExpression(IEnumerable<Function> members) : IBoundExpression
{
    public IEnumerable<Function> Members { get; } = members;
}

public class ClosedExpression(IParameter parameter, IEnumerable<Function> members) : IBoundExpression
{
    private static readonly HashSet<string> ImplicitFoldAccumulators =
        ["count", "sum", "min", "max", "first", "last", "every", "any"];

    public IParameter Parameter { get; } = parameter;
    public IEnumerable<Function> Members { get; } = members;
    public bool IsImplicitFoldAggregation => Members.Count() == 1
        && ImplicitFoldAccumulators.Contains(Members.First().Name, StringComparer.OrdinalIgnoreCase);
    public Function? GetImplicitFoldAccumulator() => IsImplicitFoldAggregation ? Members.First() : null;
}

public interface IParameter { }
public sealed record LiteralParameter(object? Value) : IParameter;
public abstract record CoercionSpecificationParameter(Type TargetType) : IParameter;
public sealed record PositionalCoercionParameter(Type TargetType) : CoercionSpecificationParameter(TargetType);
public sealed record FieldCoercionParameter(string Field, Type TargetType) : CoercionSpecificationParameter(TargetType);
public sealed record TupleCoercionParameter(int Position, Type TargetType) : CoercionSpecificationParameter(TargetType);
public sealed record IntervalParameter(IntervalBinding Value) : IParameter;
public sealed record VariableParameter(string Name) : IParameter;
public sealed record ObjectPropertyParameter(string Name) : IParameter;
public sealed record ObjectIndexParameter(int Index) : IParameter;
public sealed record TupleProjectionParameter(int Index, bool FromEnd = false) : IParameter;
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
public sealed record PairParameter(IParameter Key, IParameter Value) : IParameter;
public sealed record GroupingParameter(PairParameter[] Entries) : IParameter;
public sealed record QuotedLiteralParameter(string Value) : IParameter;
public sealed record IncomingValueParameter() : IParameter;
public sealed record RecordLiteralField(string Name, IParameter Value);
public sealed record RecordLiteralParameter(RecordLiteralField[] Fields) : IParameter;
public interface IRecordDefinitionEntry;
public sealed record RecordNamedEntry(string Name, IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordSpreadEntry(IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordDefinitionParameter(IRecordDefinitionEntry[] Entries) : IParameter;
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
