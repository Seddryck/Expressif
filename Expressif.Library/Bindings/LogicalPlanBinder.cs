using System.Globalization;
using System.Text.Json;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Functions.Accumulation;
using Expressif.Planning;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Values.Types;

namespace Expressif.Bindings;

internal sealed class LogicalPlanBinder
{
    private static readonly HashSet<string> StructuralOperators = new(StringComparer.Ordinal)
    {
        "branch",
        "callable-reference",
        "coercion",
        "dictionary",
        "field-coercion",
        "grouping",
        "incoming",
        "input-binding",
        "interval",
        "let-definition",
        "named-entry",
        "negative-infinity",
        "pair",
        "positive-infinity",
        "sort-criterion",
        "spread-entry",
        "tuple-coercion",
        "variable",
        "with",
    };

    private static readonly HashSet<string> FunctionValueOperators = new(StringComparer.Ordinal)
    {
        "array",
        "record",
        "tuple",
        "vector",
    };

    private static readonly HashSet<string> StructuralRootValueOperators = new(StringComparer.Ordinal)
    {
        "dictionary",
        "grouping",
        "interval",
        "pair",
        "variable",
    };

    private readonly IImplementationRegistry functions;
    private readonly IImplementationRegistry predicates;
    private readonly IImplementationRegistry accumulators;
    private readonly ITypeRegistry types;

    public LogicalPlanBinder(ITypeSource source, ITypeRegistry types)
        : this(
            new FunctionRegistry(source),
            new PredicateRegistry(source),
            new AccumulatorRegistry(source),
            types) { }

    private LogicalPlanBinder(
        IImplementationRegistry functions,
        IImplementationRegistry predicates,
        IImplementationRegistry accumulators,
        ITypeRegistry types)
        => (this.functions, this.predicates, this.accumulators, this.types) =
            (functions, predicates, accumulators, types);

    public IRootExpression Bind(LogicalPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.Pipeline is null || plan.Pipeline.Items is null || plan.Pipeline.Items.Count == 0)
            throw Error("A logical plan must contain a non-empty root pipeline.");

        var items = plan.Pipeline.Items;
        if (items[0] is LogicalLiteral || (items[0] is LogicalCall call && IsRootValue(call)))
        {
            var source = BindValue(items[0]);
            var members = items.Skip(1).Select(item => BindPipelineCall(item, inputBound: false)).ToArray();
            return new ClosedRootExpression(new ClosedExpression(source, members));
        }

        return new OpenRootExpression(BindOpenPipeline(plan.Pipeline, inputBound: false));
    }

    public IRootExpression BindClosed(LogicalPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.Pipeline is null || plan.Pipeline.Items is null || plan.Pipeline.Items.Count == 0)
            throw Error("A logical plan must contain a non-empty root pipeline.");
        if (RequiresCallerInput(plan.Pipeline))
            throw Error("The logical plan cannot be bound as closed because it requires caller-supplied pipeline input.");
        return Bind(plan);
    }

    private OpenExpression BindOpenPipeline(LogicalPipeline pipeline, bool inputBound)
    {
        if (pipeline.Items.Count == 0)
            throw Error("A logical pipeline must contain at least one value.");

        if (pipeline.Items[0] is LogicalCall { Function.Name: "incoming" } incoming)
        {
            RequireStructural(incoming, "incoming");
            RequireNoArguments(incoming);
            return new OpenExpression(pipeline.Items.Skip(1)
                .Select(item => BindPipelineCall(item, inputBound)).ToArray());
        }

        if (pipeline.Items.Count == 1
            && pipeline.Items[0] is LogicalCall { Function.Name: "input-binding" } binding)
        {
            return new OpenExpression(BindInputBinding(binding));
        }

        if (pipeline.Items[0] is LogicalLiteral
            || (pipeline.Items[0] is LogicalCall valueCall && IsRootValue(valueCall)))
        {
            var source = BindValue(pipeline.Items[0]);
            return new OpenExpression([
                Function.FromArguments(
                    "apply",
                    [new FunctionArgument(null, new InputExpressionParameter(new ClosedExpression(
                        source,
                        pipeline.Items.Skip(1).Select(item => BindPipelineCall(item, inputBound)).ToArray())))])
            ]);
        }

        return new OpenExpression(pipeline.Items.Select(item => BindPipelineCall(item, inputBound)).ToArray());
    }

    private Function BindPipelineCall(LogicalValue value, bool inputBound)
        => value is LogicalCall call
            ? BindCall(call, inputBound)
            : throw Error($"A pipeline stage must be an operator call, but found '{value.GetType().Name}'.");

    private Function BindCall(LogicalCall call, bool inputBound)
    {
        ValidateCall(call);
        if (call.Function.Kind == "extension" && StructuralOperators.Contains(call.Function.Name))
            throw Error($"Structural value '{call.Function.Name}' cannot be used as a pipeline stage.");

        ResolveOperator(call);
        if (call.Function.Name == "record")
            return BindRecordFunction(call);
        if (call.Function.Name == "with")
            return BindWithFunction(call);

        var arguments = BindFunctionArguments(call);
        var syntax = ResolveSyntax(call, inputBound, arguments);
        var implementationKind = ImplementationKind(call);
        if (syntax == FunctionSyntax.ScopedTupleProjectionShorthand)
        {
            var position = RequireInteger(call.Arguments.Single().Value, "tuple-at position");
            return Function.FromArguments(
                call.Function.Name,
                [new FunctionArgument(null, new ScopedTupleProjectionParameter(position, call.ContextDepth))],
                syntax,
                implementationKind);
        }
        if (syntax == FunctionSyntax.InputTupleProjectionShorthand)
        {
            var position = RequireInteger(call.Arguments.Single().Value, "tuple-at position");
            return Function.FromArguments(
                call.Function.Name,
                [new FunctionArgument(null, new LiteralParameter(
                    position.ToString(CultureInfo.InvariantCulture)))],
                syntax,
                implementationKind);
        }
        return Function.FromArguments(call.Function.Name, arguments, syntax, implementationKind);
    }

    private FunctionArgument[] BindFunctionArguments(LogicalCall call)
    {
        var result = new List<FunctionArgument>();
        var omitted = false;
        foreach (var argument in call.Arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (!argument.IsExplicit)
            {
                omitted = true;
                continue;
            }

            if (TryBindNamedEntry(argument.Value!, out var name, out var value))
            {
                result.Add(new FunctionArgument(name, value, argument.IsSpread));
                continue;
            }

            var parameter = BindArgumentValue(argument);
            var argumentName = omitted && !argument.Parameter.Variadic
                ? argument.Parameter.Name
                : null;
            result.Add(new FunctionArgument(argumentName, parameter, argument.IsSpread));
        }
        return result.ToArray();
    }

    private IParameter BindArgumentValue(LogicalArgument argument)
        => argument.Parameter.Type switch
        {
            "predicate" => new PredicationParameter(BindPredication(argument.Value!)),
            _ => BindValue(argument.Value!),
        };

    private Function BindRecordFunction(LogicalCall call)
    {
        var entries = new List<IRecordDefinitionEntry>();
        var arguments = call.Arguments is
        [
            {
                IsExplicit: true,
                IsSpread: false,
                Value: LogicalCall { Function.Name: "record" } nested,
            },
        ]
            ? nested.Arguments
            : call.Arguments;
        foreach (var argument in arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (!argument.IsExplicit)
                continue;
            if (argument.IsSpread)
            {
                var spread = RequireCall(argument.Value, "spread-entry");
                RequireStructural(spread, "spread-entry");
                entries.Add(new RecordSpreadEntry(BindValue(RequireArgument(spread, "value").Value!)));
            }
            else if (TryBindNamedEntry(argument.Value!, out var name, out var value))
            {
                entries.Add(new RecordNamedEntry(name, value));
            }
            else
            {
                throw Error("A record argument must be a named-entry or spread-entry value.");
            }
        }
        var parameters = entries.Count == 0
            ? []
            : new IParameter[] { new RecordDefinitionParameter(entries.ToArray()) };
        return Function.FromArguments(
            call.Function.Name,
            parameters.Select(parameter => new FunctionArgument(null, parameter)).ToArray(),
            FunctionSyntax.Standard,
            ImplementationKind(call));
    }

    private Function BindWithFunction(LogicalCall call)
    {
        var projections = new List<WithProjection>();
        LogicalArgument? body = null;
        foreach (var argument in call.Arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (!argument.IsExplicit)
                continue;
            if (argument.Parameter.Name == "body")
            {
                if (body is not null)
                    throw Error("Function 'with' contains more than one body argument.");
                body = argument;
                continue;
            }
            if (!TryBindNamedEntry(argument.Value!, out var name, out var value))
                throw Error("A with projection must be a named-entry value.");
            projections.Add(new WithProjection(name, value));
        }
        if (projections.Count == 0 || body?.Value is null)
            throw Error("Function 'with' requires one or more projections and one body argument.");
        return Function.FromArguments(
            call.Function.Name,
            [new FunctionArgument(null, new WithDefinitionParameter(
                projections.ToArray(),
                BindValue(body.Value)))],
            FunctionSyntax.Standard,
            ImplementationKind(call));
    }

    private IParameter BindValue(LogicalValue value) => value switch
    {
        LogicalLiteral literal => BindLiteral(literal),
        LogicalPipeline pipeline => BindExpressionParameter(pipeline),
        LogicalCall call => BindStructuralValue(call),
        _ => throw Error($"Logical value '{value.GetType().Name}' is not supported."),
    };

    private IParameter BindExpressionParameter(LogicalPipeline pipeline)
    {
        if (pipeline.Items.Count == 0)
            throw Error("An expression-valued pipeline must contain at least one value.");

        if (pipeline.Items[0] is LogicalLiteral
            || (pipeline.Items[0] is LogicalCall call && IsRootValue(call)))
        {
            return new InputExpressionParameter(new ClosedExpression(
                BindValue(pipeline.Items[0]),
                pipeline.Items.Skip(1).Select(item => BindPipelineCall(item, inputBound: false)).ToArray()));
        }

        return new OpenExpressionParameter(BindOpenPipeline(pipeline, inputBound: false));
    }

    private IParameter BindStructuralValue(LogicalCall call)
    {
        ValidateCall(call);
        return call.Function.Name switch
        {
            "array" => BindArray(call),
            "tuple" => BindTuple(call),
            "vector" => BindVector(call),
            "pair" => BindPair(call),
            "grouping" => BindGrouping(call),
            "dictionary" => BindDictionary(call),
            "record" => BindRecordLiteral(call),
            "callable-reference" => new CallableReferenceParameter(RequireText(
                RequireArgument(call, "name").Value, "callable-reference name")),
            "sort-criterion" => BindSortCriterion(call),
            "variable" => new VariableParameter(RequireText(
                RequireArgument(call, "name").Value, "variable name")),
            "incoming" => BindIncoming(call),
            "interval" => BindInterval(call),
            "coercion" => BindPositionalCoercion(call),
            "field-coercion" => BindFieldCoercion(call),
            "tuple-coercion" => BindTupleCoercion(call),
            "branch" => BindBranch(call),
            "let-definition" => BindLetDefinition(call),
            "input-binding" => new OpenExpressionParameter(new OpenExpression(BindInputBinding(call))),
            "field" => BindFieldReference(call),
            "tuple-at" => BindTupleReference(call),
            "with" when call.Function.Kind == "extension" => BindWithDefinition(call),
            _ when call.Function.Kind == "predicate" => new PredicationParameter(BindPredication(call)),
            _ => throw Error($"Operator '{call.Function.Name}' is not a supported structural plan value."),
        };
    }

    private IParameter BindLiteral(LogicalLiteral literal)
    {
        return literal.Type switch
        {
            "null" when literal.Value is null => new LiteralParameter(null),
            "boolean" when literal.Value is bool => new LiteralParameter(literal.Value),
            "text" when literal.Value is string => new LiteralParameter(literal.Value),
            "decimal" when literal.Value is decimal => new LiteralParameter(literal.Value),
            "integer" when IsInteger(literal.Value) => new LiteralParameter(literal.Value),
            "date" when literal.Value is DateOnly => new LiteralParameter(literal.Value, "date"),
            "datetime" when literal.Value is DateTime => new LiteralParameter(literal.Value, "datetime"),
            "time" when literal.Value is TimeOnly => new LiteralParameter(literal.Value, "time"),
            "duration" when literal.Value is TimeSpan => new LiteralParameter(literal.Value, "duration"),
            "type" when literal.Value is string type => new LiteralParameter(types.Resolve(type)),
            "all" when Equals(literal.Value, "#all") => new LiteralParameter(AllDimension.Instance),
            "ordering" when Equals(literal.Value, "#less") => new LiteralParameter(OrderingValue.Less),
            "ordering" when Equals(literal.Value, "#equal") => new LiteralParameter(OrderingValue.Equal),
            "ordering" when Equals(literal.Value, "#greater") => new LiteralParameter(OrderingValue.Greater),
            _ => throw Error($"Literal type '{literal.Type}' contains an invalid value."),
        };
    }

    private ArrayParameter BindArray(LogicalCall call)
        => new(BindCollectionElements(call).Select(element =>
            new ArrayElementParameter(element.Value, element.IsSpread)).ToArray());

    private TupleParameter BindTuple(LogicalCall call)
        => new(BindCollectionElements(call).Select(element =>
            new TupleElementParameter(element.Value, element.IsSpread)).ToArray());

    private VectorParameter BindVector(LogicalCall call)
        => new(BindCollectionElements(call).Select(element =>
            new TupleElementParameter(element.Value, element.IsSpread)).ToArray());

    private IReadOnlyList<(IParameter Value, bool IsSpread)> BindCollectionElements(LogicalCall call)
    {
        ResolveOperator(call);
        var elements = new List<(IParameter, bool)>();
        foreach (var argument in call.Arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (argument.IsExplicit)
                elements.Add((BindValue(argument.Value!), argument.IsSpread));
        }
        return elements;
    }

    private PairParameter BindPair(LogicalCall call)
    {
        RequireStructural(call, "pair");
        return new PairParameter(
            BindValue(RequireArgument(call, "key").Value!),
            BindValue(RequireArgument(call, "value").Value!));
    }

    private GroupingParameter BindGrouping(LogicalCall call)
        => new(BindPairEntries(call).ToArray());

    private DictionaryParameter BindDictionary(LogicalCall call)
        => new(BindPairEntries(call).ToArray());

    private IEnumerable<PairParameter> BindPairEntries(LogicalCall call)
    {
        RequireStructural(call, call.Function.Name);
        foreach (var argument in call.Arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (!argument.IsExplicit || argument.IsSpread)
                throw Error($"Structural value '{call.Function.Name}' accepts explicit pair entries only.");
            yield return BindPair(RequireCall(argument.Value, "pair"));
        }
    }

    private RecordLiteralParameter BindRecordLiteral(LogicalCall call)
    {
        ResolveOperator(call);
        var fields = new List<RecordLiteralField>();
        foreach (var argument in call.Arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (!argument.IsExplicit)
                continue;
            if (argument.IsSpread || !TryBindNamedEntry(argument.Value!, out var name, out var value))
                throw Error("A record literal cannot contain spread entries.");
            fields.Add(new RecordLiteralField(name, value));
        }
        return new RecordLiteralParameter(fields.ToArray());
    }

    private SortCriterionParameter BindSortCriterion(LogicalCall call)
    {
        RequireStructural(call, "sort-criterion");
        var typeName = RequireTypeName(RequireArgument(call, "type").Value);
        return new SortCriterionParameter(
            BindValue(RequireArgument(call, "selector").Value!),
            types.Resolve(typeName),
            RequireBoolean(RequireArgument(call, "ascending").Value, "sort direction"),
            RequireBoolean(RequireArgument(call, "nulls-first").Value, "sort null placement"));
    }

    private IncomingValueParameter BindIncoming(LogicalCall call)
    {
        RequireStructural(call, "incoming");
        RequireNoArguments(call);
        return new IncomingValueParameter();
    }

    private IntervalParameter BindInterval(LogicalCall call)
    {
        RequireStructural(call, "interval");
        return new IntervalParameter(new IntervalBinding(
            BindIntervalBound(RequireArgument(call, "lower").Value),
            BindIntervalBound(RequireArgument(call, "upper").Value),
            RequireBoolean(RequireArgument(call, "lower-inclusive").Value, "lower inclusivity"),
            RequireBoolean(RequireArgument(call, "upper-inclusive").Value, "upper inclusivity")));
    }

    private IntervalBoundBinding BindIntervalBound(LogicalValue? value)
    {
        if (value is LogicalCall { Function.Name: "negative-infinity" } negative)
        {
            RequireStructural(negative, "negative-infinity");
            RequireNoArguments(negative);
            return new IntervalBoundBinding(IntervalBoundBindingKind.NegativeInfinity);
        }
        if (value is LogicalCall { Function.Name: "positive-infinity" } positive)
        {
            RequireStructural(positive, "positive-infinity");
            RequireNoArguments(positive);
            return new IntervalBoundBinding(IntervalBoundBindingKind.PositiveInfinity);
        }
        if (value is not LogicalLiteral literal)
            throw Error("A finite interval bound must be a literal value.");
        return new IntervalBoundBinding(IntervalBoundBindingKind.Finite, ((LiteralParameter)BindLiteral(literal)).Value);
    }

    private PositionalCoercionParameter BindPositionalCoercion(LogicalCall call)
    {
        RequireStructural(call, "coercion");
        return new PositionalCoercionParameter(ResolveRuntimeType(RequireArgument(call, "type").Value));
    }

    private FieldCoercionParameter BindFieldCoercion(LogicalCall call)
    {
        RequireStructural(call, "field-coercion");
        return new FieldCoercionParameter(
            RequireText(RequireArgument(call, "field").Value, "coercion field"),
            ResolveRuntimeType(RequireArgument(call, "type").Value));
    }

    private TupleCoercionParameter BindTupleCoercion(LogicalCall call)
    {
        RequireStructural(call, "tuple-coercion");
        return new TupleCoercionParameter(
            RequireInteger(RequireArgument(call, "position").Value, "coercion position"),
            ResolveRuntimeType(RequireArgument(call, "type").Value));
    }

    private Type ResolveRuntimeType(LogicalValue? value)
    {
        var name = RequireTypeName(value);
        return types.ResolveRuntimeType(name)
            ?? throw Error($"Expressif type ':{name}' cannot be used as a coercion target.");
    }

    private ControlFlowBranchParameter BindBranch(LogicalCall call)
    {
        RequireStructural(call, "branch");
        var predicateValue = RequireArgument(call, "predicate").Value;
        return new ControlFlowBranchParameter(
            BindValue(RequireArgument(call, "expression").Value!),
            predicateValue is LogicalLiteral { Type: "null", Value: null }
                ? null
                : BindValue(predicateValue!));
    }

    private WithDefinitionParameter BindWithDefinition(LogicalCall call)
    {
        RequireStructural(call, "with");
        var projections = new List<WithProjection>();
        IParameter? body = null;
        foreach (var argument in call.Arguments)
        {
            ValidateArgument(argument, call.Function.Name);
            if (argument.Parameter.Name == "body")
            {
                body = BindValue(argument.Value!);
            }
            else
            {
                projections.Add(new WithProjection(argument.Parameter.Name, BindValue(argument.Value!)));
            }
        }
        return projections.Count > 0 && body is not null
            ? new WithDefinitionParameter(projections.ToArray(), body)
            : throw Error("A structural with definition requires projections and a body.");
    }

    private LetDefinitionParameter BindLetDefinition(LogicalCall call)
    {
        RequireStructural(call, "let-definition");
        var names = new HashSet<string>(StringComparer.Ordinal);
        var bindings = call.Arguments.Select(argument =>
        {
            ValidateArgument(argument, call.Function.Name);
            if (!argument.IsExplicit || argument.IsSpread || !names.Add(argument.Parameter.Name))
                throw Error("A let-definition must contain unique explicit named bindings.");
            return new LetBinding(argument.Parameter.Name, BindValue(argument.Value!));
        }).ToArray();
        if (bindings.Length == 0)
            throw Error("A let-definition must contain at least one binding.");
        return new LetDefinitionParameter(bindings);
    }

    private InputBoundExpression BindInputBinding(LogicalCall call)
    {
        RequireStructural(call, "input-binding");
        var namesCall = RequireCall(RequireArgument(call, "names").Value, "array");
        var names = BindCollectionElements(namesCall)
            .Select(element => element.IsSpread || element.Value is not LiteralParameter { Value: string name } || name.Length == 0
                ? throw Error("Input-binding names must be non-empty text values.")
                : name)
            .ToArray();
        var positional = RequireBoolean(RequireArgument(call, "positional").Value, "input-binding mode");
        var body = RequireArgument(call, "body").Value as LogicalPipeline
            ?? throw Error("An input-binding body must be a logical pipeline.");
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
            throw Error("An input binding cannot declare duplicate names.");
        if ((positional && names.Length < 2) || (!positional && names.Length > 1))
            throw Error("The input-binding declaration does not match its positional mode.");
        return new InputBoundExpression(names, positional, BindBody(body));
    }

    private IRootExpression BindBody(LogicalPipeline pipeline)
    {
        if (pipeline.Items.Count == 0)
            throw Error("An input-binding body must contain at least one value.");
        if (pipeline.Items[0] is LogicalLiteral
            || (pipeline.Items[0] is LogicalCall call && IsRootValue(call)))
        {
            return new ClosedRootExpression(new ClosedExpression(
                BindValue(pipeline.Items[0]),
                pipeline.Items.Skip(1).Select(item => BindPipelineCall(item, inputBound: true)).ToArray()));
        }
        return new OpenRootExpression(BindOpenPipeline(pipeline, inputBound: true));
    }

    private IParameter BindFieldReference(LogicalCall call)
    {
        ResolveOperator(call);
        var field = RequireArgument(call, "name").Value ?? RequireSingleValue(call);
        return (call.ContextDepth, BindLiteralValue(field)) switch
        {
            (1, string name) => new ObjectPropertyParameter(name),
            (2, string name) => new EnclosingObjectPropertyParameter(name),
            (1, int index) => new ObjectIndexParameter(index),
            (1, long index) when index is >= int.MinValue and <= int.MaxValue => new ObjectIndexParameter((int)index),
            _ => throw Error("A structural field reference must have context depth one or two and a text or integer selector."),
        };
    }

    private IParameter BindTupleReference(LogicalCall call)
    {
        ResolveOperator(call);
        var position = RequireInteger(RequireSingleValue(call), "tuple position");
        if (call.ContextDepth > 0)
            return new ScopedTupleProjectionParameter(position, call.ContextDepth);
        return new TupleProjectionParameter(
            position < 0 ? position == int.MinValue ? 0 : -position : position,
            position < 0);
    }

    private IPredication BindPredication(LogicalValue value)
    {
        if (value is LogicalCall call)
        {
            if (call.Function.Kind != "predicate")
                throw Error($"Operator '{call.Function.Name}' is not declared as a predicate.");
            return new SinglePredication(BindCall(call, inputBound: false));
        }
        if (value is not LogicalPipeline pipeline || pipeline.Items.Count == 0)
            throw Error("A predicate argument must be a predicate call or non-empty pipeline.");
        var members = pipeline.Items.Select(item => BindPipelineCall(item, inputBound: false)).ToArray();
        if (pipeline.Items[^1] is not LogicalCall { Function.Kind: "predicate" })
            throw Error("A predicate pipeline must end with a predicate operator.");
        return members.Length == 1
            ? new SinglePredication(members[0])
            : new PipelinePredication(new OpenExpression(members));
    }

    private bool TryBindNamedEntry(
        LogicalValue value,
        out string name,
        out IParameter parameter)
    {
        name = string.Empty;
        parameter = null!;
        if (value is not LogicalCall { Function.Name: "named-entry" } entry)
            return false;

        RequireStructural(entry, "named-entry");
        name = RequireText(RequireArgument(entry, "name").Value, "entry name");
        parameter = BindValue(RequireArgument(entry, "value").Value!);
        return true;
    }

    private FunctionSyntax ResolveSyntax(
        LogicalCall call,
        bool inputBound,
        IReadOnlyList<FunctionArgument> arguments)
    {
        if (call.Function.Kind == "accumulator")
            return FunctionSyntax.ImplicitFoldAccumulator;
        if (call.Function.Name == "conditional-forward")
            return FunctionSyntax.ConditionalForward;
        if (call.Function.Name == "conditional-backward")
            return FunctionSyntax.ConditionalBackward;
        if (call.Function.Name == "field")
        {
            return call.ContextDepth switch
            {
                0 when inputBound => FunctionSyntax.InputFieldShorthand,
                0 => FunctionSyntax.FieldShorthand,
                1 => FunctionSyntax.RootFieldShorthand,
                2 => FunctionSyntax.EnclosingRootFieldShorthand,
                _ => throw Error($"Field context depth '{call.ContextDepth}' is not supported."),
            };
        }
        if (call.Function.Name == "tuple-at")
        {
            if (arguments.Count != 1)
                throw Error("Operator 'tuple-at' requires exactly one position argument.");
            if (call.ContextDepth > 0)
                return FunctionSyntax.ScopedTupleProjectionShorthand;
            return inputBound
                ? FunctionSyntax.InputTupleProjectionShorthand
                : FunctionSyntax.TupleProjectionShorthand;
        }
        if (call.ContextDepth != 0)
            throw Error($"Operator '{call.Function.Name}' does not support context depth '{call.ContextDepth}'.");
        return FunctionSyntax.Standard;
    }

    private void ResolveOperator(LogicalCall call)
    {
        var resolved = call.Function.Kind switch
        {
            "function" => functions.TryResolve(call.Function.Name, out _),
            "predicate" => predicates.TryResolve(call.Function.Name, out _),
            "accumulator" => accumulators.TryResolve(call.Function.Name, out _),
            "extension" => functions.TryResolve(call.Function.Name, out _)
                || predicates.TryResolve(call.Function.Name, out _)
                || accumulators.TryResolve(call.Function.Name, out _),
            _ => false,
        };
        if (!resolved)
            throw Error($"Operator '{call.Function.Name}' of kind '{call.Function.Kind}' is not registered.");
    }

    private static FunctionImplementationKind ImplementationKind(LogicalCall call)
        => call.Function.Kind switch
        {
            "function" => FunctionImplementationKind.Function,
            "predicate" => FunctionImplementationKind.Predicate,
            "accumulator" => FunctionImplementationKind.Accumulator,
            _ => FunctionImplementationKind.Unspecified,
        };

    private static void ValidateCall(LogicalCall call)
    {
        if (call.Function is null || string.IsNullOrWhiteSpace(call.Function.Name))
            throw Error("A logical operator descriptor must have a name.");
        if (call.Arguments is null)
            throw Error($"Operator '{call.Function.Name}' has no argument collection.");
        if (call.ContextDepth < 0)
            throw Error($"Operator '{call.Function.Name}' has a negative context depth.");
    }

    private static void ValidateArgument(LogicalArgument argument, string functionName)
    {
        if (argument.Parameter is null || string.IsNullOrWhiteSpace(argument.Parameter.Name))
            throw Error($"Operator '{functionName}' contains an argument without parameter metadata.");
        if (argument.IsExplicit)
        {
            if (argument.Value is null || argument.Omission is not null)
                throw Error($"Explicit argument '{argument.Parameter.Name}' on '{functionName}' is malformed.");
            return;
        }
        if (argument.Value is not null || argument.IsSpread || !argument.Parameter.Optional || argument.Omission is null)
            throw Error($"Omitted argument '{argument.Parameter.Name}' on '{functionName}' is malformed.");
        ValidateOmission(argument.Omission, argument.Parameter);
    }

    private static void ValidateOmission(
        PlannerOmissionDescriptor omission,
        PlannerParameterDescriptor parameter)
    {
        var hasValue = omission.Value.ValueKind != JsonValueKind.Undefined;
        var hasSource = !string.IsNullOrWhiteSpace(omission.Source);
        var valid = omission.Mode switch
        {
            PlannerOmissionMode.Constant => hasValue && !hasSource,
            PlannerOmissionMode.EmptyVariadic => parameter.Variadic && !hasValue && !hasSource,
            PlannerOmissionMode.Absent => !hasValue && !hasSource,
            PlannerOmissionMode.EnvironmentDerived => !hasValue && hasSource,
            _ => false,
        };
        if (!valid)
            throw Error($"Omission metadata for parameter '{parameter.Name}' is invalid.");
    }

    private static void RequireStructural(LogicalCall call, string name)
    {
        ValidateCall(call);
        if (call.Function.Name != name || call.Function.Kind != "extension" || call.ContextDepth != 0)
            throw Error($"Structural value '{name}' has invalid operator metadata.");
    }

    private static void RequireNoArguments(LogicalCall call)
    {
        if (call.Arguments.Count != 0)
            throw Error($"Structural value '{call.Function.Name}' does not accept arguments.");
    }

    private static LogicalArgument RequireArgument(LogicalCall call, string name)
    {
        var matches = call.Arguments.Where(argument => argument.Parameter.Name == name).ToArray();
        if (matches is not [var argument])
            throw Error($"Structural value '{call.Function.Name}' requires exactly one '{name}' argument.");
        ValidateArgument(argument, call.Function.Name);
        if (!argument.IsExplicit || argument.IsSpread)
            throw Error($"Structural argument '{name}' on '{call.Function.Name}' must be explicit and non-spread.");
        return argument;
    }

    private static LogicalValue RequireSingleValue(LogicalCall call)
    {
        if (call.Arguments is not [var argument])
            throw Error($"Operator '{call.Function.Name}' requires exactly one argument.");
        ValidateArgument(argument, call.Function.Name);
        return argument.Value!;
    }

    private static LogicalCall RequireCall(LogicalValue? value, string name)
        => value is LogicalCall call && call.Function.Name == name
            ? call
            : throw Error($"Expected structural value '{name}'.");

    private static string RequireText(LogicalValue? value, string description)
        => value is LogicalLiteral { Type: "text", Value: string text } && text.Length > 0
            ? text
            : throw Error($"The {description} must be a non-empty text literal.");

    private static string RequireTypeName(LogicalValue? value)
        => value is LogicalLiteral { Type: "type", Value: string name } && name.Length > 0
            ? name
            : throw Error("A coercion or sort type must be a type literal.");

    private static bool RequireBoolean(LogicalValue? value, string description)
        => value is LogicalLiteral { Type: "boolean", Value: bool result }
            ? result
            : throw Error($"The {description} must be a boolean literal.");

    private static int RequireInteger(LogicalValue? value, string description)
    {
        var raw = value is LogicalLiteral { Type: "integer" } literal ? literal.Value : null;
        try
        {
            return raw is null
                ? throw Error($"The {description} must be an integer literal.")
                : Convert.ToInt32(raw, CultureInfo.InvariantCulture);
        }
        catch (OverflowException exception)
        {
            throw Error($"The {description} is outside the supported 32-bit range.", exception);
        }
    }

    private object? BindLiteralValue(LogicalValue value)
        => value is LogicalLiteral literal
            ? ((LiteralParameter)BindLiteral(literal)).Value
            : throw Error("A structural selector must be a literal value.");

    private static bool IsInteger(object? value)
        => value is sbyte or byte or short or ushort or int or uint or long or ulong;

    private static bool IsRootValue(LogicalCall call)
        => (call.Function.Kind == "function"
                && FunctionValueOperators.Contains(call.Function.Name)
                && (call.Function.Name != "record" || IsRecordValue(call)))
            || (call.Function.Kind == "extension" && StructuralRootValueOperators.Contains(call.Function.Name));

    private static bool IsRecordValue(LogicalCall call)
        => call.Arguments.All(argument => !argument.IsExplicit
            || argument.Value is LogicalCall { Function.Name: "named-entry" or "spread-entry" });

    private static bool RequiresCallerInput(LogicalPipeline pipeline)
    {
        var first = pipeline.Items[0];
        return first switch
        {
            LogicalLiteral => false,
            LogicalCall call when IsRootValue(call) => ValueRequiresCallerInput(call),
            _ => true,
        };
    }

    private static bool ValueRequiresCallerInput(LogicalValue value) => value switch
    {
        LogicalLiteral => false,
        LogicalPipeline pipeline => RequiresCallerInput(pipeline),
        LogicalCall { Function.Name: "incoming" or "input-binding" or "field" or "tuple-at" } => true,
        LogicalCall { Function.Name: "variable" or "callable-reference" } => false,
        LogicalCall call when call.Function.Name is "sort-criterion" or "coercion" or "field-coercion" or "tuple-coercion"
            => false,
        LogicalCall call => call.Arguments
            .Where(argument => argument.IsExplicit && argument.Value is not null)
            .Any(argument => ValueRequiresCallerInput(argument.Value!)),
        _ => true,
    };

    private static LogicalPlanBindingException Error(string message)
        => new(message);

    private static LogicalPlanBindingException Error(string message, Exception innerException)
        => new(message, innerException);
}
