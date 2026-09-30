using Expressif.Syntax;
using Expressif.Types;

namespace Expressif.Planning;

/// <summary>
/// Creates canonical logical plans directly from parsed Expressif syntax.
/// </summary>
public sealed class LogicalPlanner
{
    private readonly ILogicalPlanningContext context;

    public LogicalPlanner(ILogicalPlanningContext context)
        => this.context = context ?? throw new ArgumentNullException(nameof(context));

    public LogicalPlan Build(RootExpressionSyntax syntax)
    {
        ArgumentNullException.ThrowIfNull(syntax);
        return new LogicalPlan(PlanRoot(syntax));
    }

    private LogicalPipeline PlanRoot(RootExpressionSyntax syntax, string? expectedKind = null) => syntax switch
    {
        OpenExpressionSyntax open => PlanOpen(open, expectedKind),
        ClosedExpressionSyntax
        {
            Value: IncomingValueSyntax,
            Pipeline: [InputBindingExpressionSyntax binding],
        } => new LogicalPipeline([PlanInputBinding(binding)]),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed when IsRelativeRecordAccess(access)
            => new LogicalPipeline([
                .. PlanRecordAccess(access),
                .. PlanPipelineStages(closed.Pipeline, expectedKind),
            ]),
        ClosedExpressionSyntax closed => new LogicalPipeline([
            PlanValue(closed.Value),
            .. PlanPipelineStages(closed.Pipeline, expectedKind),
        ]),
        _ => throw Unsupported(syntax),
    };

    private LogicalPipeline PlanOpen(OpenExpressionSyntax syntax, string? expectedKind = null)
        => syntax.Source is null && syntax.Pipeline is [InputBindingExpressionSyntax binding]
            ? new LogicalPipeline([PlanInputBinding(binding)])
            : new LogicalPipeline(PlanPipelineStages(
                [.. syntax.Source is null ? [] : new[] { syntax.Source }, .. syntax.Pipeline],
                expectedKind));

    private IReadOnlyList<LogicalValue> PlanPipelineStages(
        IReadOnlyList<ExpressionSyntax> stages,
        string? expectedKind)
        => stages.SelectMany((stage, index) => PlanPipelineMembers(
            stage,
            index == stages.Count - 1 ? expectedKind : null)).ToArray();

    private LogicalPipeline PlanExpression(ExpressionSyntax syntax, string? expectedKind = null) => syntax switch
    {
        RootExpressionSyntax root => PlanRoot(root, expectedKind),
        ParenthesizedExpressionSyntax parenthesized => PlanRoot(parenthesized.Expression, expectedKind),
        ParameterizedExpressionSyntax parameterized => new LogicalPipeline([
            PlanArgument(parameterized.Source),
            .. PlanOpen(parameterized.Expression, expectedKind).Items,
        ]),
        _ => new LogicalPipeline(PlanPipelineMembers(syntax, expectedKind).ToArray()),
    };

    private IEnumerable<LogicalValue> PlanPipelineMembers(ExpressionSyntax syntax, string? expectedKind = null)
    {
        switch (syntax)
        {
            case TupleBindingShorthandSyntax shorthand:
                if (shorthand.Direction == TupleBindingDirection.Prefix)
                    yield return CreateCall("rotate", []) with { SourceSpan = shorthand.Span };
                yield return CreateCall(
                    "bind",
                    [Raw(new LogicalLiteral("text", shorthand.Name))],
                    expectedKind) with { SourceSpan = shorthand.Span };
                yield break;
            case RecordAccessSyntax access:
                foreach (var item in PlanRecordAccess(access))
                    yield return item;
                yield break;
            case GuardedExpressionSyntax guarded:
                yield return CreateCall("guard", [Raw(PlanExpression(guarded.Expression))], expectedKind);
                yield break;
            case UnaryExpressionSyntax unary:
                foreach (var item in PlanExpression(unary.Operand).Items)
                    yield return item;
                yield return CreateCall(PlanUnaryOperator(unary.Operator), [], expectedKind);
                yield break;
            case BinaryExpressionSyntax binary:
                yield return CreateBinaryCall(binary, expectedKind);
                yield break;
            case ConditionalExpressionSyntax conditional:
                yield return CreateCall(
                    conditional.Operator.Direction == ConditionalDirection.Forward
                        ? "conditional-forward"
                        : "conditional-backward",
                    [Raw(conditional.Left), Raw(conditional.Right)],
                    expectedKind);
                yield break;
            case ParenthesizedExpressionSyntax { Expression: OpenExpressionSyntax open }:
                foreach (var item in PlanOpen(open, expectedKind).Items)
                    yield return item;
                yield break;
            case ParenthesizedExpressionSyntax parenthesized:
                foreach (var item in PlanRoot(parenthesized.Expression, expectedKind).Items)
                    yield return item;
                yield break;
            case InputBindingExpressionSyntax binding:
                yield return CreateCall("apply", [Raw(new LogicalPipeline([PlanInputBinding(binding)]))], expectedKind);
                yield break;
            case ValueReferenceStageSyntax reference:
                yield return CreateCall("apply", [Raw(PlanVariable(reference.Reference.Name))], expectedKind);
                yield break;
            case TupleProjectionSyntax projection:
                yield return PlanTupleReference(projection);
                yield break;
            case GroupingMapShorthandSyntax map:
                yield return CreateCall("map-groups", [Raw(PlanOpen(map.Expression))], expectedKind);
                yield break;
            case ControlFlowCallSyntax controlFlow:
                yield return PlanControlFlow(controlFlow, expectedKind);
                yield break;
            case FunctionCallSyntax call:
                yield return PlanFunctionCall(call, expectedKind);
                yield break;
            case PairComponentAccessSyntax access:
                yield return CreateCall(access.Component == PairComponent.Key ? "pair-key" : "pair-value", [], expectedKind);
                yield break;
            case MapShorthandSyntax map:
                yield return CreateCall("map", [Raw(PlanOpen(map.Expression))], expectedKind);
                yield break;
            case ParameterizedExpressionSyntax parameterized:
                yield return CreateCall("map", [Raw(PlanOpen(parameterized.Expression))], expectedKind);
                yield break;
            default:
                throw Unsupported(syntax);
        }
    }

    private LogicalCall PlanFunctionCall(FunctionCallSyntax syntax, string? expectedKind)
    {
        var metadata = context.FindFunction(syntax.Name, expectedKind);
        var canonicalName = metadata?.Function.Name ?? syntax.Name.ToLowerInvariant();
        if (IsValueSpreadFunction(canonicalName)
            && syntax.Arguments.Any(argument => argument is NamedArgumentSyntax))
        {
            throw Error($"Function '{syntax.Name}' does not support named arguments.", syntax);
        }
        if (canonicalName == "drill-down"
            && syntax.Arguments.Any(argument => argument is NamedArgumentSyntax))
        {
            throw Error($"Function '{syntax.Name}' does not support named arguments.", syntax);
        }
        var call = canonicalName switch
        {
            "coerce" => CreateCall(syntax.Name, PlanCoercions(syntax), expectedKind),
            "let" => CreateCall(syntax.Name, [Raw(PlanLetDefinition(syntax))], expectedKind),
            "record" => CreateCall(syntax.Name, syntax.Arguments.Count == 0
                ? []
                : [Raw(PlanRecordDefinition(syntax.Arguments))], expectedKind),
            "sort-by" or "rank-by" or "dense-rank-by"
                => CreateCall(syntax.Name, syntax.Arguments.Select(argument => Raw(PlanSortCriterion(
                    RequireArgumentValue(argument)))).ToArray(), expectedKind),
            "sort-term" => PlanSortTerm(syntax, metadata, expectedKind),
            "with" => PlanWith(syntax, metadata, expectedKind),
            _ => CreateCall(
                syntax.Name,
                PlanOrdinaryArguments(syntax, canonicalName),
                expectedKind,
                allowMissingRequired: !syntax.HasParentheses && syntax.Arguments.Count == 0),
        };
        return call with { SourceSpan = syntax.Span };
    }

    private LogicalCall PlanSortTerm(
        FunctionCallSyntax syntax,
        PlannerFunctionMetadata? metadata,
        string? expectedKind)
    {
        var supplied = PlanSortTermArguments(syntax);
        if (supplied.Count != 4)
            return CreateCall(syntax.Name, supplied, expectedKind);

        metadata ??= context.FindFunction(syntax.Name, expectedKind)
            ?? throw Error("The planning catalog does not define 'sort-term'.", syntax);
        var parameters = metadata.Parameters.ToList();
        parameters.Add(SyntheticParameter("ascending"));
        parameters.Add(SyntheticParameter("nulls-first"));
        return new LogicalCall(
            metadata.Function,
            NormalizeArguments(metadata.Function.Name, parameters, supplied));
    }

    private static bool IsValueSpreadFunction(string name)
        => name is "array" or "text" or "tuple" or "grouping" or "grouping-sets"
            or "dictionary" or "nested-field" or "split-lengths" or "sort-key";

    private LogicalCall CreateBinaryCall(BinaryExpressionSyntax syntax, string? expectedKind)
    {
        var name = PlanBinaryOperator(syntax.Operator);
        var metadata = context.FindFunction(name, expectedKind);
        var descriptor = metadata?.Function ?? SyntheticFunction(name);
        return new LogicalCall(descriptor, [
            new LogicalArgument(
                SyntheticParameter("left").Descriptor,
                PlanExpression(syntax.Left),
                IsSpread: false,
                IsExplicit: true),
            new LogicalArgument(
                SyntheticParameter("right").Descriptor,
                PlanExpression(syntax.Right),
                IsSpread: false,
                IsExplicit: true),
        ]);
    }

    private RawArgument[] PlanOrdinaryArguments(FunctionCallSyntax syntax, string canonicalName)
    {
        var result = new List<RawArgument>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasNamedArgument = false;
        foreach (var argument in syntax.Arguments)
        {
            switch (argument)
            {
                case NamedArgumentSyntax named:
                    hasNamedArgument = true;
                    if (!names.Add(named.Name.Value))
                        throw Error($"Parameter '{named.Name.Value}' is supplied more than once.", named);
                    result.Add(PlanOrdinaryArgument(canonicalName, named.Name.Value, named.Value));
                    break;
                case SpreadArgumentSyntax spread:
                    if (hasNamedArgument)
                        throw Error($"Function '{syntax.Name}' has a positional argument after a named argument.", spread);
                    result.Add(spread.IsImplicitSpread
                        ? Raw(PlanIncoming(), isSpread: true)
                        : Raw(RequireArgumentValue(spread), isSpread: true));
                    break;
                default:
                    if (hasNamedArgument)
                        throw Error($"Function '{syntax.Name}' has a positional argument after a named argument.", argument);
                    result.Add(PlanOrdinaryArgument(canonicalName, null, RequireArgumentValue(argument)));
                    break;
            }
        }
        return result.ToArray();
    }

    private static RawArgument PlanOrdinaryArgument(
        string functionName,
        string? argumentName,
        ExpressionSyntax syntax)
        => IsFieldNameFunction(functionName)
            && syntax is FunctionCallSyntax { HasParentheses: false } bare
                ? Raw(argumentName, new LogicalLiteral("text", bare.Name))
                : Raw(argumentName, syntax);

    private static bool IsFieldNameFunction(string name)
        => name.Equals("field", StringComparison.OrdinalIgnoreCase)
            || name.Equals("is-present", StringComparison.OrdinalIgnoreCase)
            || name.Equals("is-absent", StringComparison.OrdinalIgnoreCase);

    private LogicalCall PlanControlFlow(ControlFlowCallSyntax syntax, string? expectedKind)
    {
        var isTry = syntax.Name.Equals("try", StringComparison.OrdinalIgnoreCase);
        if (syntax.Branches.Count < (isTry ? 2 : 1))
            throw Error($"Function '{syntax.Name}' has too few branches.", syntax);
        var branches = new List<RawArgument>();
        for (var index = 0; index < syntax.Branches.Count; index++)
        {
            switch (syntax.Branches[index])
            {
                case ControlFlowFallbackSyntax fallback when index > 0 && index == syntax.Branches.Count - 1:
                    branches.Add(Raw(PlanBranch(fallback.Action, null)));
                    break;
                case ConditionalControlFlowBranchSyntax branch:
                    branches.Add(Raw(PlanBranch(
                        isTry ? branch.Condition : branch.Action,
                        isTry ? branch.Action : branch.Condition)));
                    break;
                case ControlFlowFallbackSyntax fallback:
                    throw Error("A catch-all fallback must follow ordinary branches and be final.", fallback);
                default:
                    throw Error("Invalid control-flow branch.", syntax.Branches[index]);
            }
        }
        return CreateCall(syntax.Name, branches, expectedKind);
    }

    private LogicalCall PlanWith(
        FunctionCallSyntax syntax,
        PlannerFunctionMetadata? metadata,
        string? expectedKind)
    {
        if (syntax.Arguments.Count < 2
            || syntax.Arguments[^1] is not PositionalArgumentSyntax { Value: { } body }
            || syntax.Arguments.Take(syntax.Arguments.Count - 1).Any(argument => argument is not NamedArgumentSyntax))
        {
            throw Error("Function 'with' expects one or more named projections followed by a body expression.", syntax);
        }
        metadata ??= context.FindFunction(syntax.Name, expectedKind);
        var descriptor = metadata?.Function ?? SyntheticFunction(syntax.Name);
        var parameters = metadata?.Parameters ?? SyntheticParameters(syntax.Arguments.Count);
        var projections = parameters.Single(parameter => parameter.Descriptor.Type == "entry");
        var bodyParameter = parameters.Single(parameter => parameter.Descriptor.Name == "body");
        var names = new HashSet<string>(StringComparer.Ordinal);
        var arguments = new List<LogicalArgument>();
        foreach (var named in syntax.Arguments.Take(syntax.Arguments.Count - 1).Cast<NamedArgumentSyntax>())
        {
            if (!names.Add(named.Name.Value))
                throw Error($"Duplicate projection '{named.Name.Value}' in with(...).", named);
            arguments.Add(new LogicalArgument(
                projections.Descriptor,
                PlanNamedEntry(named.Name.Value, PlanArgument(named.Value)),
                IsSpread: false,
                IsExplicit: true));
        }
        arguments.Add(new LogicalArgument(
            bodyParameter.Descriptor,
            PlanArgument(body, ExpectedKind(bodyParameter)),
            IsSpread: false,
            IsExplicit: true));
        return new LogicalCall(descriptor, arguments);
    }

    private IReadOnlyList<RawArgument> PlanCoercions(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments.Count == 0)
            throw Error("Function 'coerce' expects one or more coercion specifications.", syntax);
        if (syntax.Arguments.Any(argument => argument is not PositionalArgumentSyntax))
            throw Error("Function 'coerce' accepts positional coercion specifications only.", syntax);
        var specifications = syntax.Arguments
            .Select(argument => PlanCoercion(RequireArgumentValue(argument)))
            .ToArray();
        var modes = specifications.Select(specification => specification.Function.Name).ToArray();
        if (modes.Contains("coercion") && modes.Any(mode => mode != "coercion"))
            throw Error("Function 'coerce' cannot mix positional type descriptors and selector mappings.", syntax);
        if (modes.Contains("field-coercion") && modes.Contains("tuple-coercion"))
            throw Error("Function 'coerce' cannot mix field and tuple-position selector mappings.", syntax);
        var selectors = specifications
            .Where(specification => specification.Function.Name != "coercion")
            .Select(specification => specification.Arguments[0].Value)
            .OfType<LogicalLiteral>()
            .Select(literal => $"{literal.Type}:{literal.Value}")
            .ToArray();
        var duplicate = selectors.GroupBy(selector => selector, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw Error($"Duplicate coercion selector '{duplicate.Key[(duplicate.Key.IndexOf(':') + 1)..]}'.", syntax);
        return specifications.Select(specification => Raw(specification)).ToArray();
    }

    private LogicalCall PlanCoercion(ExpressionSyntax syntax)
    {
        if (syntax is TypeLiteralSyntax type)
            return SyntheticCall("coercion", ("type", PlanType(type)));
        if (syntax is BinaryExpressionSyntax
            {
                Operator.Text: "->",
                Right: TypeLiteralSyntax target,
                Left: TupleProjectionSyntax selector,
            }
            && selector.Direction == TupleProjectionDirection.FromStart
            && selector.RootDepth == 0)
        {
            return SyntheticCall(
                "tuple-coercion",
                ("position", Literal(selector.Index)),
                ("type", PlanType(target)));
        }
        if (syntax is BinaryExpressionSyntax
            {
                Operator.Text: "->",
                Right: TypeLiteralSyntax fieldTarget,
                Left: FunctionCallSyntax { Arguments.Count: 0 } fieldSelector,
            })
        {
            return SyntheticCall(
                "field-coercion",
                ("field", new LogicalLiteral("text", fieldSelector.Name)),
                ("type", PlanType(fieldTarget)));
        }
        throw Error("A coerce specification must be ':type' or 'selector -> :type'.", syntax);
    }

    private IReadOnlyList<RawArgument> PlanSortTermArguments(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments.Count is not (2 or 4))
            throw Error("Function 'sort-term' expects a value and a tuple-bound comparer reference.", syntax);
        var arguments = new List<RawArgument>();
        for (var index = 0; index < syntax.Arguments.Count; index++)
        {
            var argument = syntax.Arguments[index];
            var name = argument is NamedArgumentSyntax named ? named.Name.Value : null;
            var value = RequireArgumentValue(argument);
            var isComparer = name?.Equals("comparer", StringComparison.OrdinalIgnoreCase) == true
                || (name is null && index == 1);
            if (!isComparer)
            {
                arguments.Add(Raw(name, value));
                continue;
            }
            var reference = value switch
            {
                TupleBindingShorthandSyntax direct => direct,
                OpenExpressionSyntax { Source: null, Pipeline: [TupleBindingShorthandSyntax nested] } => nested,
                _ => null,
            };
            if (reference is null || reference.Direction == TupleBindingDirection.Prefix)
                throw Error("The comparer for 'sort-term' must be a tuple-bound callable reference such as compare-numeric~.", value);
            arguments.Add(Raw(name, SyntheticCall(
                "callable-reference",
                ("name", new LogicalLiteral("text", reference.Name)))));
        }
        return arguments;
    }

    private LogicalCall PlanSortCriterion(ExpressionSyntax syntax)
    {
        BinaryExpressionSyntax? mapping = null;
        IReadOnlyList<ExpressionSyntax> modifiers = [];
        if (syntax is BinaryExpressionSyntax direct)
        {
            mapping = direct;
        }
        else if (syntax is OpenExpressionSyntax { Source: BinaryExpressionSyntax source } open)
        {
            mapping = source;
            modifiers = open.Pipeline;
        }
        else if (syntax is OpenExpressionSyntax { Source: null } openWithoutSource
            && openWithoutSource.Pipeline.FirstOrDefault() is BinaryExpressionSyntax first)
        {
            mapping = first;
            modifiers = openWithoutSource.Pipeline.Skip(1).ToArray();
        }
        if (mapping is not { Operator.Text: "->", Right: TypeLiteralSyntax type })
            throw Error("A sort-by criterion must use the form expression -> :type.", syntax);
        var ascending = true;
        var nullsFirst = false;
        foreach (var modifier in modifiers)
        {
            if (modifier is not FunctionCallSyntax { Arguments.Count: 0 } call)
                throw Error("Sort-by criteria accept only direction and null-placement modifiers.", modifier);
            switch (call.Name.ToLowerInvariant())
            {
                case "ascending" or "asc": ascending = true; break;
                case "descending" or "desc": ascending = false; break;
                case "nulls-first": nullsFirst = true; break;
                case "nulls-last": nullsFirst = false; break;
                default: throw Error($"Unsupported sort-by modifier '{call.Name}'.", call);
            }
        }
        return SyntheticCall(
            "sort-criterion",
            ("selector", PlanArgument(mapping.Left)),
            ("type", PlanType(type)),
            ("ascending", Literal(ascending)),
            ("nulls-first", Literal(nullsFirst)));
    }

    private LogicalValue PlanArgument(ExpressionSyntax syntax, string? expectedKind = null)
    {
        if (syntax is TupleProjectionSyntax projection)
            return PlanTupleReference(projection);
        if (FindTupleScope(syntax) is { } scoped)
            return PlanTupleReference(scoped);
        if (syntax is RecordAccessSyntax access && IsRelativeRecordAccess(access))
            return new LogicalPipeline(PlanRecordAccess(access).Cast<LogicalValue>().ToArray());
        if (syntax is ValueSyntax value)
            return PlanValue(value);
        return PlanExpression(syntax, expectedKind);
    }

    private LogicalValue PlanValue(ValueSyntax syntax) => syntax switch
    {
        VariableSyntax variable => PlanVariable(variable.Name),
        IncomingValueSyntax => PlanIncoming(),
        RecordAccessSyntax access => PlanRecordAccessValue(access),
        NumericLiteralSyntax numeric => Literal(numeric.Value),
        BooleanLiteralSyntax boolean => Literal(boolean.Value),
        NullLiteralSyntax => new LogicalLiteral("null", null),
        AllLiteralSyntax => new LogicalLiteral("all", "#all"),
        OrderingLiteralSyntax ordering => ordering.Text switch
        {
            "#less" or "#equal" or "#greater" => new LogicalLiteral("ordering", ordering.Text),
            _ => throw Unsupported(ordering),
        },
        QuotedLiteralSyntax quoted => new LogicalLiteral("text", quoted.Value),
        QuotedTypedLiteralSyntax quoted => PlanQuotedTypedLiteral(quoted),
        DateLiteralSyntax date => new LogicalLiteral("date", date.Value),
        DateTimeLiteralSyntax dateTime => new LogicalLiteral("datetime", dateTime.Value),
        TimeLiteralSyntax time => new LogicalLiteral("time", time.Value),
        TypeLiteralSyntax type => PlanType(type),
        IntervalLiteralSyntax interval => PlanInterval(interval),
        ArrayLiteralSyntax array => PlanCollection("array", array.Elements.Select(element =>
            (element.Expression, element.IsSpread, element.IsImplicitSpread))),
        VectorLiteralSyntax vector => PlanCollection("vector", vector.Elements.Select(element =>
            (element.Expression, element.IsSpread, element.IsImplicitSpread))),
        TupleLiteralSyntax tuple => PlanCollection("tuple", tuple.Elements.Select(element =>
            (element.Expression, element.IsSpread, element.IsImplicitSpread))),
        PairLiteralSyntax pair => SyntheticCall(
            "pair",
            ("key", PlanArgument(pair.Key)),
            ("value", PlanArgument(pair.Value))),
        GroupingLiteralSyntax grouping => SyntheticCall(
            "grouping",
            grouping.Entries.Select((pair, index) => ($"entry-{index}", (LogicalValue)SyntheticCall(
                "pair",
                ("key", PlanArgument(pair.Key)),
                ("value", PlanArgument(pair.Value))))).ToArray()),
        DictionaryLiteralSyntax dictionary => SyntheticCall(
            "dictionary",
            dictionary.Entries.Select((pair, index) => ($"entry-{index}", (LogicalValue)SyntheticCall(
                "pair",
                ("key", PlanArgument(pair.Key)),
                ("value", PlanArgument(pair.Value))))).ToArray()),
        RecordLiteralSyntax record => PlanRecordLiteral(record),
        _ => throw Unsupported(syntax),
    };

    private LogicalValue PlanRecordAccessValue(RecordAccessSyntax syntax)
    {
        if (syntax.RootDepth == 0 || syntax.Fields.Count == 0)
            throw Error($"Record access '{syntax.Text}' cannot be used as a scalar parameter.", syntax);
        var calls = PlanRecordAccess(syntax).Cast<LogicalValue>().ToArray();
        return calls.Length == 1
            ? calls[0]
            : new LogicalPipeline(calls) { IsScalarReference = true };
    }

    private IEnumerable<LogicalCall> PlanRecordAccess(RecordAccessSyntax syntax)
    {
        for (var index = 0; index < syntax.Fields.Count; index++)
        {
            var field = syntax.Fields[index];
            var value = field.Name is string name
                ? (object)name
                : field.Index is int position
                    ? position
                    : throw Error($"Record access '{syntax.Text}' contains neither a named nor positional field selector.", syntax);
            yield return CreateCall(
                "field",
                [Raw(Literal(value))],
                contextDepth: index == 0 ? syntax.RootDepth : 0) with
                {
                    IsReferenceShorthand = true,
                    IsReferenceContinuation = index > 0,
                };
        }
    }

    private LogicalCall PlanTupleReference(TupleProjectionSyntax syntax)
    {
        var position = syntax.Direction == TupleProjectionDirection.FromEnd
            ? syntax.Index == 0 ? int.MinValue : -syntax.Index
            : syntax.Index;
        return CreateCall("tuple-at", [Raw(Literal(position))], contextDepth: syntax.RootDepth) with
        {
            IsReferenceShorthand = true,
        };
    }

    private LogicalCall PlanCollection(
        string name,
        IEnumerable<(ExpressionSyntax? Expression, bool IsSpread, bool IsImplicitSpread)> elements)
    {
        var metadata = context.FindFunction(name);
        var descriptor = metadata is null
            ? SyntheticFunction(name)
            : metadata.Function with { Kind = "extension" };
        var parameter = SyntheticParameter("values", variadic: true).Descriptor;
        var arguments = elements.Select(element => new LogicalArgument(
            parameter,
            element.IsImplicitSpread
                ? PlanIncoming()
                : PlanArgument(element.Expression ?? throw new LogicalPlanningException(
                    $"An explicit {name} spread must include an expression.")),
            element.IsSpread,
            IsExplicit: true)).ToArray();
        return new LogicalCall(descriptor, arguments);
    }

    private LogicalCall PlanTextCollection(string name, IEnumerable<string> values)
    {
        var metadata = context.FindFunction(name);
        var descriptor = metadata?.Function ?? SyntheticFunction(name);
        var parameter = metadata?.Parameters.SingleOrDefault() ?? SyntheticParameter("values", variadic: true);
        var arguments = values.Select(value => new LogicalArgument(
            parameter.Descriptor,
            new LogicalLiteral("text", value),
            IsSpread: false,
            IsExplicit: true)).ToList();
        if (arguments.Count == 0 && parameter.Descriptor.Optional)
            arguments.Add(new LogicalArgument(parameter.Descriptor, null, false, false, parameter.Omission));
        return new LogicalCall(descriptor, arguments);
    }

    private LogicalCall PlanRecordLiteral(RecordLiteralSyntax syntax)
    {
        if (syntax.Entries.Count != syntax.Fields.Count)
            throw Error("Record literal spread entries must specify a field name.", syntax);
        var entries = new List<(string Name, LogicalValue Value, bool Spread)>();
        foreach (var field in syntax.Fields)
        {
            if (field.IsSpread)
                throw Error($"Record literal field '{field.Name.Value}' does not support spread values.", field);
            if (field.Value is not ValueSyntax value)
                throw Error($"Record literal field '{field.Name.Value}' must contain a value.", field);
            entries.Add((field.Name.Value, PlanValue(value), false));
        }
        return PlanRecord(entries);
    }

    private LogicalCall PlanRecordDefinition(IReadOnlyList<ArgumentSyntax> arguments)
    {
        var entries = new List<(string Name, LogicalValue Value, bool Spread)>();
        foreach (var argument in arguments)
        {
            switch (argument)
            {
                case NamedArgumentSyntax named:
                    entries.Add((named.Name.Value, PlanArgument(named.Value), false));
                    break;
                case SpreadArgumentSyntax spread:
                    entries.Add(("spread", spread.IsImplicitSpread
                        ? PlanIncoming()
                        : PlanArgument(RequireArgumentValue(spread)), true));
                    break;
                case PositionalArgumentSyntax { Value: IncomingValueSyntax }:
                    entries.Add(("spread", PlanIncoming(), true));
                    break;
                default:
                    throw Unsupported(argument);
            }
        }
        return PlanRecord(entries);
    }

    private LogicalCall PlanRecord(IEnumerable<(string Name, LogicalValue Value, bool Spread)> entries)
    {
        var metadata = context.FindFunction("record")
            ?? throw new LogicalPlanningException("The planning catalog does not define 'record'.");
        var parameter = metadata.Parameters.Single();
        var arguments = entries.Select(entry => new LogicalArgument(
            parameter.Descriptor,
            entry.Spread
                ? SyntheticCall("spread-entry", ("value", entry.Value))
                : PlanNamedEntry(entry.Name, entry.Value),
            entry.Spread,
            IsExplicit: true)).ToList();
        if (arguments.Count == 0)
            arguments.Add(new LogicalArgument(parameter.Descriptor, null, false, false, parameter.Omission));
        return new LogicalCall(metadata.Function, arguments);
    }

    private LogicalCall PlanLetDefinition(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments.Count == 0 || syntax.Arguments.Any(argument => argument is not NamedArgumentSyntax))
            throw Error("Function 'let' expects one or more named assignments.", syntax);
        var names = new HashSet<string>(StringComparer.Ordinal);
        var arguments = new List<(string Name, LogicalValue Value)>();
        foreach (var named in syntax.Arguments.Cast<NamedArgumentSyntax>())
        {
            if (named.Name.QuotingStyle is not null || named.Name.IsPrivate)
                throw Error("Let binding names must be unquoted value identifiers.", named);
            if (!names.Add(named.Name.Value))
                throw Error($"Duplicate let binding name '{named.Name.Value}'.", named);
            arguments.Add((named.Name.Value, PlanArgument(named.Value)));
        }
        return SyntheticCall("let-definition", arguments.ToArray());
    }

    private LogicalCall PlanInputBinding(InputBindingExpressionSyntax syntax)
    {
        string[] names;
        var positional = false;
        switch (syntax.Binding)
        {
            case BindingNameSyntax name:
                names = [name.Name];
                break;
            case null:
                names = [];
                break;
            case PositionalBindingPatternSyntax pattern:
                positional = true;
                names = pattern.Names.Select(name => name.Name).ToArray();
                var duplicate = names.GroupBy(name => name, StringComparer.Ordinal)
                    .FirstOrDefault(group => group.Count() > 1);
                if (duplicate is not null)
                    throw Error($"Duplicate input binding name '{duplicate.Key}'.", pattern);
                break;
            default:
                throw Error("Unsupported input binding declaration.", syntax.Binding);
        }
        return SyntheticCall(
            "input-binding",
            ("names", PlanTextCollection("array", names)),
            ("positional", Literal(positional)),
            ("body", PlanRoot(syntax.Body)));
    }

    private LogicalCall PlanBranch(ExpressionSyntax expression, ExpressionSyntax? predicate)
        => SyntheticCall(
            "branch",
            ("expression", PlanArgument(expression)),
            ("predicate", predicate is null
                ? new LogicalLiteral("null", null)
                : PlanArgument(predicate, "predicate")));

    private LogicalCall PlanNamedEntry(string name, LogicalValue value)
        => SyntheticCall(
            "named-entry",
            ("name", new LogicalLiteral("text", name)),
            ("value", value));

    private LogicalCall PlanInterval(IntervalLiteralSyntax syntax)
        => SyntheticCall(
            "interval",
            ("lower", PlanIntervalBound(syntax.LowerBound)),
            ("upper", PlanIntervalBound(syntax.UpperBound)),
            ("lower-inclusive", Literal(syntax.IsLowerInclusive)),
            ("upper-inclusive", Literal(syntax.IsUpperInclusive)));

    private LogicalValue PlanIntervalBound(IntervalBound bound) => bound.Kind switch
    {
        IntervalBoundKind.NegativeInfinity => SyntheticCall("negative-infinity"),
        IntervalBoundKind.PositiveInfinity => SyntheticCall("positive-infinity"),
        IntervalBoundKind.Finite when bound.Value is not null => PlanValue(bound.Value),
        IntervalBoundKind.Finite => throw new LogicalPlanningException("A finite interval bound must have a value."),
        _ => throw new LogicalPlanningException($"Unsupported interval bound kind '{bound.Kind}'."),
    };

    private LogicalLiteral PlanQuotedTypedLiteral(QuotedTypedLiteralSyntax syntax)
    {
        var authoredType = syntax.Type?.Name;
        var typeName = authoredType is null ? null : ResolveType(authoredType, syntax.Type!);
        var literal = QuotedLiteralRegistry.Default.Parse(syntax.Representation.Value, typeName);
        return Literal(literal.Value);
    }

    private LogicalLiteral PlanType(TypeLiteralSyntax syntax)
        => new("type", ResolveType(syntax.Name, syntax));

    private string ResolveType(string name, SyntaxNode syntax)
        => context.FindType(name)
            ?? throw Error($"Unknown Expressif type literal ':{name}'.", syntax);

    private LogicalCall CreateCall(
        string authoredName,
        IReadOnlyList<RawArgument> supplied,
        string? expectedKind = null,
        int contextDepth = 0,
        bool allowMissingRequired = false)
    {
        var metadata = context.FindFunction(authoredName, expectedKind);
        var descriptor = metadata?.Function ?? SyntheticFunction(authoredName);
        var parameters = metadata?.Parameters ?? SyntheticParameters(supplied.Count);
        var arguments = NormalizeArguments(
            descriptor.Name,
            parameters,
            supplied,
            allowMissingRequired);
        return new LogicalCall(descriptor, arguments, contextDepth);
    }

    private IReadOnlyList<LogicalArgument> NormalizeArguments(
        string functionName,
        IReadOnlyList<PlannerParameterMetadata> parameters,
        IReadOnlyList<RawArgument> supplied,
        bool allowMissingRequired = false)
    {
        if (parameters.Count == 0)
        {
            if (supplied.Count > 0)
                throw new LogicalPlanningException($"Function '{functionName}' does not accept arguments.");
            return [];
        }
        var associated = new List<(PlannerParameterMetadata Parameter, RawArgument Argument, string? EntryName)>();
        var positionalIndex = 0;
        foreach (var argument in supplied)
        {
            PlannerParameterMetadata? parameter;
            string? entryName = null;
            if (argument.Name is null)
            {
                parameter = positionalIndex < parameters.Count
                    ? parameters[positionalIndex]
                    : parameters.LastOrDefault(candidate => candidate.Descriptor.Variadic);
                if (parameter is null)
                    throw new LogicalPlanningException($"Function '{functionName}' has too many positional arguments.");
                if (!parameter.Descriptor.Variadic)
                    positionalIndex++;
            }
            else
            {
                parameter = parameters.SingleOrDefault(candidate => candidate.Descriptor.Type == "entry");
                if (parameter is not null)
                {
                    entryName = argument.Name;
                }
                else
                {
                    parameter = parameters.SingleOrDefault(candidate => candidate.Descriptor.Name.Equals(
                        argument.Name,
                        StringComparison.OrdinalIgnoreCase))
                        ?? throw new LogicalPlanningException(
                            $"Function '{functionName}' has no parameter named '{argument.Name}'.");
                    if (associated.Any(item => ReferenceEquals(item.Parameter, parameter)))
                        throw new LogicalPlanningException($"Parameter '{parameter.Descriptor.Name}' is supplied more than once.");
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
                var value = match.Argument.Value
                    ?? (parameter.Descriptor.Type == "text"
                        && TryGetBareFunctionName(match.Argument.Syntax!, out var bareName)
                            ? new LogicalLiteral("text", bareName)
                            : PlanArgument(match.Argument.Syntax!, ExpectedKind(parameter)));
                if (match.EntryName is not null)
                    value = PlanNamedEntry(match.EntryName, value);
                result.Add(new LogicalArgument(
                    parameter.Descriptor,
                    value,
                    match.Argument.IsSpread,
                    IsExplicit: true));
            }
            if (matches.Length == 0)
            {
                if (!parameter.Descriptor.Optional)
                {
                    if (allowMissingRequired || parameter.Descriptor.Variadic)
                        continue;
                    throw new LogicalPlanningException(
                        $"Required parameter '{parameter.Descriptor.Name}' was not supplied to '{functionName}'.");
                }
                result.Add(new LogicalArgument(
                    parameter.Descriptor,
                    null,
                    IsSpread: false,
                    IsExplicit: false,
                    parameter.Omission));
            }
        }
        return result;
    }

    private LogicalCall SyntheticCall(string name, params (string Name, LogicalValue Value)[] arguments)
        => new(
            SyntheticFunction(name),
            arguments.Select(argument => new LogicalArgument(
                SyntheticParameter(argument.Name).Descriptor,
                argument.Value,
                IsSpread: false,
                IsExplicit: true)).ToArray());

    private static PlannerFunctionDescriptor SyntheticFunction(string name)
        => new(
            name.ToLowerInvariant(),
            "any",
            "any",
            Kind: "extension",
            Schema: name switch
            {
                "named-entry" => new(Intrinsic: "named-entry", Classification: "intrinsic"),
                "spread-entry" => new(Intrinsic: "spread-entry", Classification: "intrinsic"),
                "sort-criterion" => new(Intrinsic: "sort-criterion", Classification: "intrinsic"),
                _ => null,
            });

    private static PlannerParameterMetadata[] SyntheticParameters(int count)
        => Enumerable.Range(0, count).Select(index => SyntheticParameter($"argument-{index}")).ToArray();

    private static PlannerParameterMetadata SyntheticParameter(string name, bool variadic = false)
        => new(new PlannerParameterDescriptor(name, "any", Optional: false, variadic, variadic ? 0 : 1));

    private static string? ExpectedKind(PlannerParameterMetadata parameter)
        => parameter.Descriptor.Type is "predicate" or "accumulator"
            ? parameter.Descriptor.Type
            : null;

    private static bool TryGetBareFunctionName(ExpressionSyntax syntax, out string name)
    {
        var call = syntax switch
        {
            FunctionCallSyntax { Arguments.Count: 0 } direct => direct,
            OpenExpressionSyntax { Pipeline: [FunctionCallSyntax { Arguments.Count: 0 } nested] } => nested,
            _ => null,
        };
        name = call?.Name ?? string.Empty;
        return call is not null;
    }

    private LogicalCall PlanVariable(string name)
        => SyntheticCall("variable", ("name", new LogicalLiteral("text", name)));

    private LogicalCall PlanIncoming()
        => SyntheticCall("incoming");

    private static LogicalLiteral Literal(object? value) => value switch
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
        _ => throw new LogicalPlanningException(
            $"Value of runtime type '{value.GetType().Name}' has no Expressif literal representation."),
    };

    private static TupleProjectionSyntax? FindTupleScope(ExpressionSyntax syntax) => syntax switch
    {
        TupleProjectionSyntax { RootDepth: > 0 } reference => reference,
        ParenthesizedExpressionSyntax parenthesized => FindTupleScope(parenthesized.Expression),
        OpenExpressionSyntax { Source: null, Pipeline: [var member] } => FindTupleScope(member),
        OpenExpressionSyntax { Source: { } source, Pipeline.Count: 0 } => FindTupleScope(source),
        _ => null,
    };

    private static string PlanUnaryOperator(UnaryOperatorSyntax syntax)
        => syntax.Text == "!" ? "not" : throw Unsupported(syntax);

    private static string PlanBinaryOperator(BinaryOperatorSyntax syntax)
        => syntax.Text.ToUpperInvariant() switch
        {
            "|AND" => "and",
            "|OR" => "or",
            "|XOR" => "xor",
            _ => throw Unsupported(syntax),
        };

    private static bool IsRelativeRecordAccess(RecordAccessSyntax syntax)
        => syntax.RootDepth == 0 && syntax.Text.StartsWith('.');

    private static ExpressionSyntax RequireArgumentValue(ArgumentSyntax argument)
        => argument.Value ?? throw Error("A non-spread argument must include an expression.", argument);

    private static RawArgument Raw(ExpressionSyntax syntax, bool isSpread = false)
        => new(null, syntax, null, isSpread);

    private static RawArgument Raw(string? name, ExpressionSyntax syntax, bool isSpread = false)
        => new(name, syntax, null, isSpread);

    private static RawArgument Raw(LogicalValue value, bool isSpread = false)
        => new(null, null, value, isSpread);

    private static RawArgument Raw(string? name, LogicalValue value, bool isSpread = false)
        => new(name, null, value, isSpread);

    private static LogicalPlanningException Unsupported(SyntaxNode syntax)
        => Error($"Syntax kind '{syntax.Kind}' is not planned (source: '{syntax.Text}').", syntax);

    private static LogicalPlanningException Error(string message, SyntaxNode? syntax = null)
        => new(syntax is null ? message : $"{message} (at offset {syntax.Span.Start})");

    private sealed record RawArgument(
        string? Name,
        ExpressionSyntax? Syntax,
        LogicalValue? Value,
        bool IsSpread);
}

public sealed class LogicalPlanningException : Exception
{
    public LogicalPlanningException(string message)
        : base(message) { }
}
