namespace Expressif.Bindings;

public interface IExpression { }
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

public sealed class Function(string name, IParameter[] parameters, FunctionSyntax syntax = FunctionSyntax.Standard) : IExpression
{
    public string Name { get; } = name;
    public IParameter[] Parameters { get; } = parameters;
    public FunctionSyntax Syntax { get; } = syntax;
}

public class OpenExpression(IEnumerable<Function> members) : IExpression
{
    public IEnumerable<Function> Members { get; } = members;
}

public class ClosedExpression(IParameter parameter, IEnumerable<Function> members) : IExpression
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
public sealed record IntervalParameter(IntervalBinding Value) : IParameter;
public sealed record VariableParameter(string Name) : IParameter;
public sealed record ObjectPropertyParameter(string Name) : IParameter;
public sealed record ObjectIndexParameter(int Index) : IParameter;
public sealed record TupleProjectionParameter(int Index, bool FromEnd = false) : IParameter;
public sealed record ContextParameter(Func<IContext, object?> Function) : IParameter;
public sealed record ArrayParameter(IParameter[] Values) : IParameter;
public sealed record TupleParameter(IParameter[] Values) : IParameter;
public sealed record QuotedLiteralParameter(string Value) : IParameter;
public sealed record IncomingValueParameter() : IParameter;
public sealed record RecordLiteralField(string Name, IParameter Value);
public sealed record RecordLiteralParameter(RecordLiteralField[] Fields) : IParameter;
public interface IRecordDefinitionEntry;
public sealed record RecordNamedEntry(string Name, IParameter Value) : IRecordDefinitionEntry;
public sealed record RecordSpreadEntry() : IRecordDefinitionEntry;
public sealed record RecordDefinitionParameter(IRecordDefinitionEntry[] Entries) : IParameter;
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
public sealed class SinglePredication(params Function[] members) : IPredication
{
    public Function[] Members { get; } = members;
}

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
