using System.Text.Json;
using Expressif.Discovery;
using Expressif.Planning;
using Expressif.Values;
using Expressif.Values.Types;
using RuntimeExpression = Expressif.IExpression;
using RuntimeExpressionFactory = Expressif.Functions.FunctionFactory;

namespace Expressif.Bindings;

/// <summary>
/// Experimental adapter that binds a logical plan without consulting its source syntax tree.
/// </summary>
internal sealed class LogicalPlanBinder
{
    private readonly IContext context;
    private readonly RuntimeExpressionFactory runtimeFactory;

    public LogicalPlanBinder(IContext? context = null)
    {
        this.context = context ?? new Context();
        runtimeFactory = new RuntimeExpressionFactory(new AssemblyTypeSource(typeof(ExpressionBinder).Assembly));
    }

    public RuntimeExpression Bind(LogicalPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new Expressif.Expression(runtimeFactory.Instantiate(Root(plan.Pipeline), context));
    }

    public RuntimeExpression BindClosed(LogicalPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new Expressif.Expression(runtimeFactory.InstantiateClosed(ClosedRoot(plan.Pipeline), context));
    }

    private IRootExpression Root(LogicalPipeline pipeline)
        => IsSourceValue(pipeline.Items.FirstOrDefault())
            ? ClosedRoot(pipeline)
            : new OpenRootExpression(Open(pipeline));

    private ClosedRootExpression ClosedRoot(LogicalPipeline pipeline)
    {
        if (pipeline.Items.Count == 0)
            throw new LogicalPlanBindingException("A closed logical pipeline must contain a source value.");

        return new ClosedRootExpression(new ClosedExpression(
            SourceValue(pipeline.Items[0]),
            pipeline.Items.Skip(1).Select(Call).ToArray()));
    }

    private IParameter SourceValue(LogicalValue value)
        => value is LogicalCall { Function.Name: "record" } record
            ? RecordLiteral(record)
            : Value(value);

    private OpenExpression Open(LogicalPipeline pipeline)
    {
        if (pipeline.Items is [LogicalCall { Function.Name: "input-binding" } binding])
            return new OpenExpression(InputBinding(binding));

        return new OpenExpression(pipeline.Items.Select(Call).ToArray());
    }

    private InputBoundExpression InputBinding(LogicalCall call)
    {
        var names = Argument(call, "names") as LogicalCall
            ?? throw Invalid(call, "names must be represented by an array call");
        var positional = LiteralValue(Argument(call, "positional")) as bool?
            ?? throw Invalid(call, "positional must be a boolean literal");
        var body = Argument(call, "body") as LogicalPipeline
            ?? throw Invalid(call, "body must be a pipeline");

        var boundNames = names.Arguments
            .Where(argument => argument.IsExplicit)
            .Select(argument =>
            {
                return LiteralValue(argument.Value!) as string
                    ?? throw Invalid(call, "every binding name must be text");
            })
            .ToArray();
        return new InputBoundExpression(boundNames, positional, Root(body));
    }

    private Function Call(LogicalValue value)
        => value is LogicalCall call
            ? Call(call)
            : throw new LogicalPlanBindingException(
                $"A pipeline operation must be a logical call, but found '{value.GetType().Name}'.");

    private Function Call(LogicalCall call)
    {
        if (IsStructural(call.Function.Name))
        {
            throw new LogicalPlanBindingException(
                $"Structural value '{call.Function.Name}' cannot be used as a pipeline operation.");
        }

        var arguments = call.Arguments
            .Where(argument => argument.IsExplicit)
            .Select(argument => new FunctionArgument(
                argument.Parameter.Variadic || IsSynthetic(argument.Parameter) ? null : argument.Parameter.Name,
                Value(argument.Value
                    ?? throw Invalid(call, $"explicit argument '{argument.Parameter.Name}' has no value"),
                    argument.Parameter.Type),
                argument.IsSpread))
            .ToArray();
        return Function.FromArguments(call.Function.Name, arguments, Syntax(call));
    }

    private IParameter Value(LogicalValue value, string? expectedType = null)
        => value switch
        {
            LogicalLiteral literal => new LiteralParameter(LiteralValue(literal), literal.Type),
            LogicalPipeline pipeline => Expression(pipeline),
            LogicalCall call => StructuralValue(call, expectedType),
            _ => throw new LogicalPlanBindingException(
                $"Logical value '{value.GetType().Name}' cannot be bound as a runtime parameter."),
        };

    private IParameter Expression(LogicalPipeline pipeline)
    {
        if (IsDefiniteNestedSource(pipeline.Items.FirstOrDefault()))
        {
            var closed = ClosedRoot(pipeline).Expression;
            return new InputExpressionParameter(closed);
        }

        return new OpenExpressionParameter(Open(pipeline));
    }

    private IParameter StructuralValue(LogicalCall call, string? expectedType)
        => call.Function.Name switch
        {
            "callable-reference" => new CallableReferenceParameter(Text(call, "name")),
            "sort-criterion" => new SortCriterionParameter(
                Value(Argument(call, "selector")),
                ExpressifTypeRegistry.Instance.Resolve(TypeName(call, "type")),
                Boolean(call, "ascending"),
                Boolean(call, "nulls-first")),
            "variable" => new VariableParameter(Text(call, "name")),
            "incoming" => new IncomingValueParameter(),
            "field" => Field(call),
            "tuple-at" => TupleProjection(call),
            "array" => new ArrayParameter(Explicit(call)
                .Select(argument => new ArrayElementParameter(Value(argument.Value!), argument.IsSpread)).ToArray()),
            "tuple" => new TupleParameter(Explicit(call)
                .Select(argument => new TupleElementParameter(Value(argument.Value!), argument.IsSpread)).ToArray()),
            "vector" => new VectorParameter(Explicit(call)
                .Select(argument => new TupleElementParameter(Value(argument.Value!), argument.IsSpread)).ToArray()),
            "pair" => new PairParameter(Value(Argument(call, "key")), Value(Argument(call, "value"))),
            "grouping" => new GroupingParameter(Explicit(call).Select(Pair).ToArray()),
            "dictionary" => new DictionaryParameter(Explicit(call).Select(Pair).ToArray()),
            "record" when expectedType == "entry" => Record(call),
            "record" => RecordLiteral(call),
            "let-definition" => new LetDefinitionParameter(Explicit(call)
                .Select(argument => new LetBinding(argument.Parameter.Name, Value(argument.Value!))).ToArray()),
            "interval" => Interval(call),
            "coercion" => new PositionalCoercionParameter(RuntimeType(call, "type")),
            "field-coercion" => new FieldCoercionParameter(Text(call, "field"), RuntimeType(call, "type")),
            "tuple-coercion" => new TupleCoercionParameter(Integer(call, "position"), RuntimeType(call, "type")),
            "branch" => Branch(call),
            "with" => With(call),
            _ when IsPredicate(expectedType) => new PredicationParameter(new SinglePredication(Call(call))),
            _ => throw new LogicalPlanBindingException(
                $"Logical call '{call.Function.Name}' has no parameter representation for expected type '{expectedType ?? "unknown"}'."),
        };

    private static bool IsPredicate(string? type)
        => type?.Contains("predicate", StringComparison.OrdinalIgnoreCase) == true;

    private IParameter Field(LogicalCall call)
    {
        var value = Argument(call, call.Arguments.Single().Parameter.Name);
        return call.ContextDepth switch
        {
            1 when LiteralValue(value) is string name => new ObjectPropertyParameter(name),
            1 when LiteralValue(value) is int index => new ObjectIndexParameter(index),
            2 when LiteralValue(value) is string name => new EnclosingObjectPropertyParameter(name),
            _ => throw Invalid(call, $"unsupported field reference at context depth {call.ContextDepth}"),
        };
    }

    private IParameter TupleProjection(LogicalCall call)
    {
        var position = Convert.ToInt32(LiteralValue(call.Arguments.Single().Value!), System.Globalization.CultureInfo.InvariantCulture);
        return call.ContextDepth == 0
            ? new TupleProjectionParameter(Math.Abs(position), position < 0)
            : new ScopedTupleProjectionParameter(position, call.ContextDepth);
    }

    private PairParameter Pair(LogicalArgument argument)
        => argument.Value is LogicalCall { Function.Name: "pair" } pair
            ? new PairParameter(Value(Argument(pair, "key")), Value(Argument(pair, "value")))
            : throw new LogicalPlanBindingException("A grouping or dictionary entry must be represented by a pair call.");

    private RecordDefinitionParameter Record(LogicalCall call)
        => new(Explicit(call).Select<LogicalArgument, IRecordDefinitionEntry>(argument => argument.Value switch
        {
            LogicalCall { Function.Name: "named-entry" } entry => new RecordNamedEntry(
                Text(entry, "name"), Value(Argument(entry, "value"))),
            LogicalCall { Function.Name: "spread-entry" } entry => new RecordSpreadEntry(Value(Argument(entry, "value"))),
            _ => throw Invalid(call, "record entries must be named-entry or spread-entry calls"),
        }).ToArray());

    private RecordLiteralParameter RecordLiteral(LogicalCall call)
        => new(Explicit(call).Select(argument => argument.Value switch
        {
            LogicalCall { Function.Name: "named-entry" } entry => new RecordLiteralField(
                Text(entry, "name"), Value(Argument(entry, "value"))),
            LogicalCall { Function.Name: "spread-entry" } => throw Invalid(
                call, "a closed record source cannot contain a spread entry"),
            _ => throw Invalid(call, "record entries must be named-entry calls"),
        }).ToArray());

    private IntervalParameter Interval(LogicalCall call)
        => new(new IntervalBinding(
            IntervalBound(Argument(call, "lower")),
            IntervalBound(Argument(call, "upper")),
            Boolean(call, "lower-inclusive"),
            Boolean(call, "upper-inclusive")));

    private IntervalBoundBinding IntervalBound(LogicalValue value)
        => value switch
        {
            LogicalCall { Function.Name: "negative-infinity" } => new(IntervalBoundBindingKind.NegativeInfinity),
            LogicalCall { Function.Name: "positive-infinity" } => new(IntervalBoundBindingKind.PositiveInfinity),
            LogicalLiteral literal => new(IntervalBoundBindingKind.Finite, LiteralValue(literal)),
            _ => throw new LogicalPlanBindingException("An interval bound must be a literal or infinity marker."),
        };

    private ControlFlowBranchParameter Branch(LogicalCall call)
    {
        var predicate = Argument(call, "predicate");
        return new ControlFlowBranchParameter(
            Value(Argument(call, "expression")),
            predicate is LogicalLiteral { Type: "null" } ? null : Value(predicate));
    }

    private WithDefinitionParameter With(LogicalCall call)
    {
        var arguments = Explicit(call).ToArray();
        var body = arguments.SingleOrDefault(argument => argument.Parameter.Name == "body")
            ?? throw Invalid(call, "with must contain a body");
        return new WithDefinitionParameter(
            arguments.Where(argument => !ReferenceEquals(argument, body))
                .Select(argument => new WithProjection(argument.Parameter.Name, Value(argument.Value!))).ToArray(),
            Value(body.Value!));
    }

    private static FunctionSyntax Syntax(LogicalCall call)
        => call.Function.Name switch
        {
            "conditional-forward" => FunctionSyntax.ConditionalForward,
            "conditional-backward" => FunctionSyntax.ConditionalBackward,
            "field" when call.ContextDepth == 0 => FunctionSyntax.FieldShorthand,
            "field" when call.ContextDepth == 1 => FunctionSyntax.RootFieldShorthand,
            "field" when call.ContextDepth == 2 => FunctionSyntax.EnclosingRootFieldShorthand,
            "tuple-at" when call.ContextDepth > 0 => FunctionSyntax.ScopedTupleProjectionShorthand,
            _ => FunctionSyntax.Standard,
        };

    private static bool IsStructural(string name)
        => name is "input-binding" or "callable-reference" or "sort-criterion" or "variable" or "incoming"
            or "let-definition" or "interval"
            or "negative-infinity" or "positive-infinity" or "coercion" or "field-coercion"
            or "tuple-coercion" or "branch" or "with" or "named-entry" or "spread-entry";

    private static bool IsSourceValue(LogicalValue? value)
        => value is LogicalLiteral
            or LogicalCall
            {
                Function.Name: "array" or "tuple" or "vector" or "pair" or "grouping" or "dictionary"
                    or "record" or "variable" or "incoming" or "interval",
            };

    private static bool IsDefiniteNestedSource(LogicalValue? value)
        => value is LogicalLiteral
            or LogicalCall { Function.Name: "variable" or "incoming" or "interval" };

    private static IEnumerable<LogicalArgument> Explicit(LogicalCall call)
        => call.Arguments.Where(argument => argument.IsExplicit);

    private static bool IsSynthetic(PlannerParameterDescriptor parameter)
        => parameter.Name.StartsWith("argument-", StringComparison.Ordinal)
            && parameter.Type == "any";

    private static LogicalValue Argument(LogicalCall call, string name)
        => call.Arguments.SingleOrDefault(argument => argument.Parameter.Name == name)?.Value
            ?? throw Invalid(call, $"missing argument '{name}'");

    private static string Text(LogicalCall call, string name)
        => LiteralValue(Argument(call, name)) as string
            ?? throw Invalid(call, $"argument '{name}' must be text");

    private static string TypeName(LogicalCall call, string name)
        => Argument(call, name) is LogicalLiteral { Type: "type" } literal
            ? literal.Value?.ToString() ?? throw Invalid(call, $"argument '{name}' has no type name")
            : throw Invalid(call, $"argument '{name}' must be a type literal");

    private static bool Boolean(LogicalCall call, string name)
        => LiteralValue(Argument(call, name)) as bool?
            ?? throw Invalid(call, $"argument '{name}' must be boolean");

    private static int Integer(LogicalCall call, string name)
        => Convert.ToInt32(LiteralValue(Argument(call, name)), System.Globalization.CultureInfo.InvariantCulture);

    private static Type RuntimeType(LogicalCall call, string name)
        => ExpressifTypeRegistry.Instance.Resolve(TypeName(call, name)).RuntimeType
            ?? throw Invalid(call, $"type '{TypeName(call, name)}' has no runtime representation");

    private static object? LiteralValue(LogicalValue value)
        => value is LogicalLiteral literal
            ? LiteralValue(literal)
            : throw new LogicalPlanBindingException($"Expected a logical literal but found '{value.GetType().Name}'.");

    private static object? LiteralValue(LogicalLiteral literal)
        => literal.Type switch
        {
            "all" => AllDimension.Instance,
            "ordering" => literal.Value?.ToString() switch
            {
                "#less" => OrderingValue.Less,
                "#equal" => OrderingValue.Equal,
                "#greater" => OrderingValue.Greater,
                _ => throw new LogicalPlanBindingException($"Unknown ordering literal '{literal.Value}'."),
            },
            "type" => ExpressifTypeRegistry.Instance.Resolve(literal.Value?.ToString()
                ?? throw new LogicalPlanBindingException("A type literal must contain a type name.")),
            _ => JsonValue(literal.Value),
        };

    private static object? JsonValue(object? value)
        => value is JsonElement element ? element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var integer) => integer,
            JsonValueKind.Number => element.GetDecimal(),
            _ => throw new LogicalPlanBindingException($"Unsupported JSON literal kind '{element.ValueKind}'."),
        } : value;

    private static LogicalPlanBindingException Invalid(LogicalCall call, string reason)
        => new($"Logical call '{call.Function.Name}' is invalid: {reason}.");
}

internal sealed class LogicalPlanBindingException(string message) : Exception(message);
