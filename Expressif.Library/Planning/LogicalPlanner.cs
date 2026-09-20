using Expressif.Bindings;
using Expressif.Library.Catalog;
using Expressif.Types;
using Expressif.Values;
using Expressif.Values.Types;

namespace Expressif.Planning;

/// <summary>
/// Creates canonical logical plans from parsed Expressif syntax.
/// </summary>
public sealed class LogicalPlanner
{
    private readonly FunctionCatalog catalog;

    public LogicalPlanner()
        : this(FunctionCatalog.Default) { }

    public LogicalPlanner(FunctionCatalog catalog)
        => this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

    public static LogicalPlan Plan(Expressif.Syntax.RootExpressionSyntax syntax)
        => new LogicalPlanner().Build(syntax);

    public LogicalPlan Build(Expressif.Syntax.RootExpressionSyntax syntax)
    {
        ArgumentNullException.ThrowIfNull(syntax);
        var bound = ExpressifBinderFactory.Create(applyCoercion: false).Bind(syntax);
        return new LogicalPlan(bound switch
        {
            OpenRootExpression open => Pipeline(open.Expression),
            ClosedRootExpression closed => new LogicalPipeline(
                [Value(closed.Expression.Parameter), .. closed.Expression.Members.Select(member => Call(member))]),
            _ => throw new LogicalPlanningException($"Unsupported bound root '{bound.GetType().Name}'."),
        });
    }

    private LogicalPipeline Pipeline(OpenExpression expression, string? expectedKind = null)
        => expression is InputBoundExpression binding
            ? new LogicalPipeline([InputBinding(binding, expectedKind)])
            : Pipeline(expression.Members, expectedKind);

    private LogicalPipeline Pipeline(ClosedExpression expression, string? expectedKind = null)
        => new([Value(expression.Parameter), .. PipelineItems(expression.Members, expectedKind)]);

    private LogicalPipeline Pipeline(IRootExpression expression, string? expectedKind = null) => expression switch
    {
        OpenRootExpression open => Pipeline(open.Expression, expectedKind),
        ClosedRootExpression closed => Pipeline(closed.Expression, expectedKind),
        _ => throw new LogicalPlanningException($"Unsupported input-binding body '{expression.GetType().Name}'."),
    };

    private LogicalPipeline Pipeline(IEnumerable<Function> members, string? expectedKind)
        => new(PipelineItems(members, expectedKind));

    private IReadOnlyList<LogicalValue> PipelineItems(IEnumerable<Function> members, string? expectedKind)
    {
        var calls = members.ToArray();
        return calls.Select((call, index) => (LogicalValue)Call(
            call,
            index == calls.Length - 1 ? expectedKind : null)).ToArray();
    }

    private LogicalCall InputBinding(InputBoundExpression binding, string? expectedKind = null)
        => SyntheticCall(
            "input-binding",
            ("names", Collection("array", binding.Names.Select(name => ((IParameter)new QuotedLiteralParameter(name), false)))),
            ("positional", new LogicalLiteral("boolean", binding.IsPositional)),
            ("body", Pipeline(binding.Body, expectedKind)));

    private LogicalCall Call(Function function, string? expectedKind = null)
    {
        var documentation = Resolve(function.Name, expectedKind);
        var descriptor = documentation is null
            ? SyntheticFunction(function.Name)
            : Descriptor(documentation);
        var parameters = documentation?.Parameters
            ?? SyntheticParameters(function.Arguments.Length);
        var arguments = function.Parameters is [WithDefinitionParameter with] && descriptor.Name == "with"
            ? WithArguments(parameters, with)
            : NormalizeArguments(descriptor.Name, parameters, function.Arguments);
        var contextDepth = ContextDepth(function.Syntax);
        if (descriptor.Name == "tuple-at" && function.Arguments is [{ Value: ScopedTupleProjectionParameter projection }])
        {
            arguments = [arguments[0] with { Value = Literal(projection.Index) }];
            contextDepth = projection.ScopeDepth;
        }
        else if (descriptor.Name == "tuple-at"
                 && function.Syntax is FunctionSyntax.TupleProjectionShorthand or FunctionSyntax.InputTupleProjectionShorthand
                 && arguments is [{ Value: LogicalLiteral { Value: string position } }])
        {
            arguments = [arguments[0] with { Value = Literal(int.Parse(position, System.Globalization.CultureInfo.InvariantCulture)) }];
        }
        return new LogicalCall(descriptor, arguments, contextDepth);
    }

    private FunctionDocumentation? Resolve(string name, string? expectedKind)
        => expectedKind is "predicate" or "accumulator"
            ? catalog.Find(name, expectedKind)
            : catalog.Find(name, "function") ?? catalog.Find(name);

    private IReadOnlyList<LogicalArgument> NormalizeArguments(
        string functionName,
        IReadOnlyList<FunctionParameterDocumentation> parameters,
        IReadOnlyList<FunctionArgument> supplied)
    {
        if (parameters.Count == 0)
        {
            if (supplied.Count > 0)
                throw new LogicalPlanningException($"Function '{functionName}' does not accept arguments.");
            return [];
        }

        var associated = new List<(
            FunctionParameterDocumentation Parameter,
            FunctionArgument Argument,
            string? EntryName)>();
        var positionalIndex = 0;
        foreach (var argument in supplied)
        {
            FunctionParameterDocumentation? parameter;
            string? entryName = null;
            if (argument.Name is null)
            {
                parameter = positionalIndex < parameters.Count
                    ? parameters[positionalIndex]
                    : parameters.LastOrDefault(candidate => candidate.Variadic);
                if (parameter is null)
                    throw new LogicalPlanningException($"Function '{functionName}' has too many positional arguments.");
                if (!parameter.Variadic)
                    positionalIndex++;
            }
            else
            {
                parameter = parameters.SingleOrDefault(candidate => candidate.Kind == "entry");
                if (parameter is not null)
                {
                    entryName = argument.Name;
                }
                else
                {
                    parameter = parameters.SingleOrDefault(candidate => candidate.Name.Equals(argument.Name, StringComparison.OrdinalIgnoreCase))
                        ?? throw new LogicalPlanningException($"Function '{functionName}' has no parameter named '{argument.Name}'.");
                    if (associated.Any(item => ReferenceEquals(item.Parameter, parameter)))
                        throw new LogicalPlanningException($"Parameter '{parameter.Name}' is supplied more than once.");
                }
            }
            associated.Add((parameter, argument, entryName));
        }

        var result = new List<LogicalArgument>();
        foreach (var parameter in parameters)
        {
            var matches = associated.Where(item => ReferenceEquals(item.Parameter, parameter)).ToArray();
            foreach (var match in matches)
            {
                var value = match.EntryName is null
                    ? Value(match.Argument.Value, ExpectedKind(parameter))
                    : NamedEntry(match.EntryName, match.Argument.Value);
                result.Add(new LogicalArgument(Descriptor(parameter), value, match.Argument.IsSpread, IsExplicit: true));
            }
            if (matches.Length == 0)
            {
                if (!parameter.Optional)
                    throw new LogicalPlanningException($"Required parameter '{parameter.Name}' was not supplied to '{functionName}'.");
                result.Add(new LogicalArgument(Descriptor(parameter), null, IsSpread: false, IsExplicit: false, parameter.Omission));
            }
        }
        return result;
    }

    private IReadOnlyList<LogicalArgument> WithArguments(
        IReadOnlyList<FunctionParameterDocumentation> parameters,
        WithDefinitionParameter definition)
    {
        var projections = parameters.Single(parameter => parameter.Kind == "entry");
        var body = parameters.Single(parameter => parameter.Name == "body");
        return [
            .. definition.Projections.Select(projection => new LogicalArgument(
                Descriptor(projections),
                NamedEntry(projection.Name, projection.Value),
                IsSpread: false,
                IsExplicit: true)),
            new LogicalArgument(
                Descriptor(body),
                Value(definition.Body, ExpectedKind(body)),
                IsSpread: false,
                IsExplicit: true),
        ];
    }

    private LogicalValue Value(IParameter parameter, string? expectedKind = null) => parameter switch
    {
        LiteralParameter literal => Literal(literal.Value),
        QuotedLiteralParameter quoted => new LogicalLiteral("text", quoted.Value),
        CallableReferenceParameter reference => SyntheticCall("callable-reference",
            ("name", new LogicalLiteral("text", reference.Name))),
        SortCriterionParameter criterion => SyntheticCall("sort-criterion",
            ("selector", Value(criterion.Selector)),
            ("type", Literal(criterion.Type)),
            ("ascending", Literal(criterion.Ascending)),
            ("nulls-first", Literal(criterion.NullsFirst))),
        VariableParameter variable => SyntheticCall("variable", ("name", new LogicalLiteral("text", variable.Name))),
        IncomingValueParameter => SyntheticCall("incoming"),
        ObjectPropertyParameter property => Reference("field", property.Name, 1),
        EnclosingObjectPropertyParameter property => Reference("field", property.Name, 2),
        ObjectIndexParameter index => Reference("field", index.Index, 1),
        TupleProjectionParameter projection => Reference("tuple-at", projection.FromEnd ? -projection.Index : projection.Index, 0),
        ScopedTupleProjectionParameter projection => Reference("tuple-at", projection.Index, projection.ScopeDepth),
        ArrayParameter array => Collection("array", array.Elements.Select(element => (element.Value, element.IsSpread))),
        TupleParameter tuple => Collection("tuple", tuple.Elements.Select(element => (element.Value, element.IsSpread))),
        VectorParameter vector => Collection("vector", vector.Elements.Select(element => (element.Value, element.IsSpread))),
        PairParameter pair => SyntheticCall("pair", ("key", Value(pair.Key)), ("value", Value(pair.Value))),
        GroupingParameter grouping => SyntheticCall("grouping", grouping.Entries.Select((entry, index) =>
            ($"entry-{index}", (LogicalValue)SyntheticCall("pair", ("key", Value(entry.Key)), ("value", Value(entry.Value))))).ToArray()),
        DictionaryParameter dictionary => SyntheticCall("dictionary", dictionary.Entries.Select((entry, index) =>
            ($"entry-{index}", (LogicalValue)SyntheticCall("pair", ("key", Value(entry.Key)), ("value", Value(entry.Value))))).ToArray()),
        RecordLiteralParameter record => Record(record.Fields.Select(field => (field.Name, field.Value, false))),
        RecordDefinitionParameter record => Record(record.Entries.Select(entry => entry switch
        {
            RecordNamedEntry named => (named.Name, named.Value, false),
            RecordSpreadEntry spread => ("spread", spread.Value, true),
            _ => throw new LogicalPlanningException($"Unsupported record entry '{entry.GetType().Name}'."),
        })),
        LetDefinitionParameter definition => SyntheticCall("let-definition",
            definition.Bindings.Select(binding => (binding.Name, Value(binding.Value))).ToArray()),
        OpenExpressionParameter open => Pipeline(open.Expression, expectedKind),
        InputExpressionParameter input => Pipeline(input.Expression, expectedKind),
        IntervalParameter interval => Interval(interval.Value),
        CoercionSpecificationParameter coercion => Coercion(coercion),
        PredicationParameter predication => Predication(predication.Predication),
        ControlFlowBranchParameter branch => SyntheticCall("branch",
            ("expression", Value(branch.Expression)),
            ("predicate", branch.Predicate is null
                ? new LogicalLiteral("null", null)
                : Value(branch.Predicate, "predicate"))),
        WithDefinitionParameter with => SyntheticCall("with",
            [.. with.Projections.Select(projection => (projection.Name, Value(projection.Value))), ("body", Value(with.Body))]),
        _ => throw new LogicalPlanningException($"Unsupported parameter '{parameter.GetType().Name}'."),
    };

    private LogicalCall Reference(string name, object value, int depth)
    {
        var documentation = catalog.Find(name)!;
        return new LogicalCall(
            Descriptor(documentation),
            [new LogicalArgument(Descriptor(documentation.Parameters[0]), Literal(value), false, true)],
            depth);
    }

    private LogicalCall Collection(string name, IEnumerable<(IParameter Value, bool Spread)> elements)
    {
        var documentation = catalog.Find(name);
        var descriptor = documentation is null
            ? SyntheticFunction(name)
            : Descriptor(documentation);
        var parameter = documentation?.Parameters.SingleOrDefault() ?? SyntheticParameter("values", variadic: true);
        var arguments = elements.Select(element => new LogicalArgument(
            Descriptor(parameter), Value(element.Value), element.Spread, true)).ToList();
        if (arguments.Count == 0 && parameter.Optional)
            arguments.Add(new LogicalArgument(Descriptor(parameter), null, false, false, parameter.Omission));
        return new LogicalCall(descriptor, arguments);
    }

    private LogicalCall Record(IEnumerable<(string Name, IParameter Value, bool Spread)> entries)
    {
        var documentation = catalog.Find("record")!;
        var parameter = documentation.Parameters.Single();
        var arguments = entries.Select(entry => new LogicalArgument(
            Descriptor(parameter),
            entry.Spread
                ? SyntheticCall("spread-entry", ("value", Value(entry.Value)))
                : NamedEntry(entry.Name, entry.Value),
            entry.Spread,
            true)).ToList();
        if (arguments.Count == 0)
            arguments.Add(new LogicalArgument(Descriptor(parameter), null, false, false, parameter.Omission));
        return new LogicalCall(
            Descriptor(documentation),
            arguments);
    }

    private LogicalCall NamedEntry(string name, IParameter value)
        => SyntheticCall(
            "named-entry",
            ("name", new LogicalLiteral("text", name)),
            ("value", Value(value)));

    private LogicalCall Interval(IntervalBinding interval)
        => SyntheticCall("interval",
            ("lower", IntervalBound(interval.LowerBound)),
            ("upper", IntervalBound(interval.UpperBound)),
            ("lower-inclusive", new LogicalLiteral("boolean", interval.IsLowerInclusive)),
            ("upper-inclusive", new LogicalLiteral("boolean", interval.IsUpperInclusive)));

    private LogicalValue IntervalBound(IntervalBoundBinding bound)
        => bound.Kind switch
        {
            IntervalBoundBindingKind.Finite => Literal(bound.Value),
            IntervalBoundBindingKind.NegativeInfinity => SyntheticCall("negative-infinity"),
            IntervalBoundBindingKind.PositiveInfinity => SyntheticCall("positive-infinity"),
            _ => throw new LogicalPlanningException($"Unsupported interval bound '{bound.Kind}'."),
        };

    private LogicalCall Coercion(CoercionSpecificationParameter coercion)
    {
        var type = ExpressifTypeRegistry.Instance.All.SingleOrDefault(candidate => candidate.RuntimeType == coercion.TargetType)?.Name
            ?? throw new LogicalPlanningException("A coercion target has no Expressif semantic type.");
        return coercion switch
        {
            PositionalCoercionParameter => SyntheticCall("coercion", ("type", new LogicalLiteral("type", type))),
            FieldCoercionParameter field => SyntheticCall("field-coercion",
                ("field", new LogicalLiteral("text", field.Field)), ("type", new LogicalLiteral("type", type))),
            TupleCoercionParameter tuple => SyntheticCall("tuple-coercion",
                ("position", Literal(tuple.Position)), ("type", new LogicalLiteral("type", type))),
            _ => throw new LogicalPlanningException($"Unsupported coercion '{coercion.GetType().Name}'."),
        };
    }

    private LogicalValue Predication(IPredication predication) => predication switch
    {
        SinglePredication single => Call(single.Member, "predicate"),
        PipelinePredication pipeline => Pipeline(pipeline.Expression, "predicate"),
        _ => throw new LogicalPlanningException($"Unsupported predication '{predication.GetType().Name}'."),
    };

    private LogicalLiteral Literal(object? value) => value switch
    {
        null => new LogicalLiteral("null", null),
        bool boolean => new LogicalLiteral("boolean", boolean),
        string text => new LogicalLiteral("text", text),
        decimal number => new LogicalLiteral("decimal", number),
        int number => new LogicalLiteral("integer", number),
        long number => new LogicalLiteral("integer", number),
        DateOnly date => new LogicalLiteral("date", date),
        DateTime dateTime => new LogicalLiteral("datetime", dateTime),
        TimeOnly time => new LogicalLiteral("time", time),
        TimeSpan duration => new LogicalLiteral("duration", duration),
        TypeDescriptor type => new LogicalLiteral("type", type.Name),
        AllDimension => new LogicalLiteral("all", "#all"),
        OrderingValue ordering => new LogicalLiteral("ordering", ordering.ToString()),
        _ => throw new LogicalPlanningException($"Value of runtime type '{value.GetType().Name}' has no Expressif literal representation."),
    };

    private LogicalCall SyntheticCall(string name, params (string Name, LogicalValue Value)[] arguments)
        => new(
            SyntheticFunction(name),
            arguments.Select(argument => new LogicalArgument(
                Descriptor(SyntheticParameter(argument.Name)), argument.Value, false, true)).ToArray());

    private static PlannerFunctionDescriptor SyntheticFunction(string name)
        => new(name.ToLowerInvariant(), "any", "any", Kind: "extension");

    private static FunctionParameterDocumentation[] SyntheticParameters(int count)
        => Enumerable.Range(0, count).Select(index => SyntheticParameter($"argument-{index}")).ToArray();

    private static FunctionParameterDocumentation SyntheticParameter(string name, bool variadic = false)
        => new(name, "any", Optional: false, string.Empty, variadic, variadic ? 0 : 1);

    private static PlannerFunctionDescriptor Descriptor(FunctionDocumentation function)
        => new(
            function.Name,
            string.IsNullOrWhiteSpace(function.Input) ? "any" : function.Input,
            string.IsNullOrWhiteSpace(function.Output) ? "any" : function.Output,
            function.Traversal is null
                ? null
                : new PlannerTraversalDescriptor(function.Traversal.Source, function.Traversal.Selection),
            function.Semantics is null
                ? null
                : new PlannerSemanticsDescriptor(
                    function.Semantics.Cardinality,
                    function.Semantics.Dependency,
                    function.Semantics.Ordering),
            function.Kind);

    private static string? ExpectedKind(FunctionParameterDocumentation parameter)
        => parameter.TypeOrKind is "predicate" or "accumulator" ? parameter.TypeOrKind : null;

    private static PlannerParameterDescriptor Descriptor(FunctionParameterDocumentation parameter)
        => new(
            parameter.Name,
            parameter.TypeOrKind,
            parameter.Optional,
            parameter.Variadic,
            parameter.MinimumCardinality,
            parameter.Evaluation is null
                ? null
                : new PlannerEvaluationDescriptor(
                    parameter.Evaluation.Frequency,
                    parameter.Evaluation.Source,
                    parameter.Evaluation.Context));

    private static int ContextDepth(FunctionSyntax syntax) => syntax switch
    {
        FunctionSyntax.RootFieldShorthand => 1,
        FunctionSyntax.EnclosingRootFieldShorthand => 2,
        _ => 0,
    };
}

public sealed class LogicalPlanningException : Exception
{
    public LogicalPlanningException(string message)
        : base(message) { }
}
