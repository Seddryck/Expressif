using Expressif.Syntax;
using Expressif.Functions;
using Expressif.Functions.Coercions;
using Expressif.Types;

namespace Expressif.Bindings;

public sealed class ExpressifBinder
{
    private readonly FunctionTypeMapper functionTypeMapper = new();
    private readonly CoercionRegistry coercionRegistry = new();

    public bool ApplyCoercion { get; }

    public ExpressifBinder(bool applyCoercion = true)
        => ApplyCoercion = applyCoercion;

    public IRootExpression Bind(RootExpressionSyntax syntax) => syntax switch
    {
        OpenExpressionSyntax open => new OpenRootExpression(BindOpen(open)),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed when IsRelativeRecordAccess(access)
            => new OpenRootExpression(BindRecordAccessExpression(closed, access)),
        ClosedExpressionSyntax closed => new ClosedRootExpression(BindClosed(closed)),
        _ => throw Unsupported(syntax),
    };

    public Function BindFunction(RootExpressionSyntax syntax)
    {
        var root = Bind(syntax);
        return root is OpenRootExpression open && open.Expression.Members.Count() == 1
            ? open.Expression.Members.Single()
            : throw new BindingException($"Source '{syntax.Text}' is not a single function.");
    }

    public IParameter BindParameter(RootExpressionSyntax syntax)
    {
        if (syntax is OpenExpressionSyntax
            {
                Source: null,
                Pipeline: [ParenthesizedExpressionSyntax parenthesized],
            })
            return BindArgument(parenthesized);

        var root = Bind(syntax);
        return root is ClosedRootExpression closed && !closed.Expression.Members.Any()
            ? closed.Expression.Parameter
            : throw new BindingException($"Source '{syntax.Text}' is not a standalone parameter.");
    }

    public IPredication BindPredication(RootExpressionSyntax syntax)
    {
        var root = Bind(syntax);
        return root switch
        {
            OpenRootExpression open => BindPredication(open.Expression),
            ClosedRootExpression closed when closed.Expression.Members.Any()
                => BindPredication(new OpenExpression(closed.Expression.Members)),
            _ => throw new BindingException($"Predication '{syntax.Text}' is not bound in this iteration."),
        };
    }

    private static IPredication BindPredication(OpenExpression expression)
    {
        var members = expression.Members.ToArray();
        if (members is [var combinator]
            && combinator.Name is "and" or "or" or "xor"
            && combinator.Parameters is [OpenExpressionParameter left, OpenExpressionParameter right])
        {
            return new BinaryPredication(
                new BinaryOperator(combinator.Name),
                BindPredication(left.Expression),
                BindPredication(right.Expression));
        }

        return members switch
        {
            [var member] => new SinglePredication(member),
            _ => new PipelinePredication(expression),
        };
    }

    private OpenExpression BindOpen(OpenExpressionSyntax syntax)
    {
        var members = ApplyCoercions([
            .. syntax.Source is null ? [] : BindPipelineMembers(syntax.Source),
            .. syntax.Pipeline.SelectMany(BindPipelineMembers),
        ]).ToArray();
        ValidateCoercePipeline(members, null);
        return new OpenExpression(members);
    }

    private ClosedExpression BindClosed(ClosedExpressionSyntax syntax)
    {
        var source = BindValue(syntax.Value);
        var members = ApplyCoercions(syntax.Pipeline.SelectMany(BindPipelineMembers)).ToArray();
        ValidateCoercePipeline(members, GetStaticType(source));
        return new ClosedExpression(source, members);
    }

    private void ValidateCoercePipeline(
        IReadOnlyList<Function> members,
        Type? inputType)
    {
        var currentType = inputType;
        foreach (var member in members)
        {
            if (member.Name.Equals("coerce", StringComparison.OrdinalIgnoreCase))
            {
                if (currentType is not null)
                    ValidateCoerceInput(member, currentType);
                continue;
            }

            if (TryGetContract(member, out _, out var outputType) && outputType != typeof(object))
            {
                currentType = Nullable.GetUnderlyingType(outputType) ?? outputType;
            }
            else
            {
                currentType = null;
            }
        }
    }

    private static void ValidateCoerceInput(Function function, Type inputType)
    {
        var specifications = function.Parameters.Cast<CoercionSpecificationParameter>().ToArray();
        if (typeof(Values.TupleValue).IsAssignableFrom(inputType))
        {
            if (specifications.Any(specification => specification is FieldCoercionParameter))
                throw new BindingException("Tuple input requires tuple-position selectors.");
            return;
        }

        if (typeof(Values.RecordValue).IsAssignableFrom(inputType))
        {
            if (specifications.Any(specification => specification is not FieldCoercionParameter))
                throw new BindingException("Record input requires field selector mappings.");
            return;
        }

        if (specifications is not [PositionalCoercionParameter])
            throw new BindingException("Scalar input requires exactly one positional type descriptor.");
    }

    private static Type? GetStaticType(IParameter parameter)
        => parameter switch
        {
            QuotedLiteralParameter => typeof(string),
            LiteralParameter { Value: { } value } => value.GetType(),
            TupleParameter => typeof(Values.TupleValue),
            PairParameter => typeof(Values.PairValue),
            GroupingParameter => typeof(Values.Grouping),
            DictionaryParameter => typeof(Values.Dictionary),
            RecordLiteralParameter => typeof(Values.RecordValue),
            ArrayParameter => typeof(object[]),
            _ => null,
        };

    private IEnumerable<Function> ApplyCoercions(IEnumerable<Function> functions)
    {
        var members = functions.ToArray();
        if (!ApplyCoercion || members.Length < 2)
            return members;

        var rewritten = new List<Function> { members[0] };
        for (var index = 1; index < members.Length; index++)
        {
            var previous = members[index - 1];
            var current = members[index];
            if (TryGetContract(previous, out _, out var sourceType)
                && TryGetContract(current, out var targetType, out _)
                && !targetType.IsAssignableFrom(sourceType))
            {
                var coercionSourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
                var descriptor = coercionRegistry.Descriptors.SingleOrDefault(
                    candidate => candidate.Supports(coercionSourceType, targetType));
                if (descriptor is not null)
                {
                    rewritten.Add(new Function(descriptor.Name, []));
                }
            }

            rewritten.Add(current);
        }

        return rewritten;
    }

    private bool TryGetContract(Function function, out Type inputType, out Type outputType)
    {
        inputType = null!;
        outputType = null!;
        if (!functionTypeMapper.TryExecute(function.Name, out var implementationType))
            return false;

        var contracts = implementationType.GetInterfaces()
            .Where(candidate => candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(IFunction<,>))
            .Select(candidate => candidate.GetGenericArguments())
            .DistinctBy(candidate => (candidate[0], candidate[1]))
            .ToArray();
        if (contracts.Length != 1)
            return false;

        inputType = contracts[0][0];
        outputType = contracts[0][1];
        return true;
    }

    private IEnumerable<Function> BindPipelineMembers(ExpressionSyntax syntax)
        => syntax switch
        {
            GuardedExpressionSyntax guarded => [BindGuardedExpression(guarded)],
            UnaryExpressionSyntax unary => BindUnaryExpression(unary).Members,
            BinaryExpressionSyntax binary => BindBinaryExpression(binary).Members,
            ParenthesizedExpressionSyntax { Expression: OpenExpressionSyntax open } => BindOpen(open).Members,
            ParenthesizedExpressionSyntax
            {
                Expression: ClosedExpressionSyntax
                {
                    Value: RecordAccessSyntax access,
                } closed,
            } when IsRelativeRecordAccess(access) => BindRecordAccessExpression(closed, access).Members,
            ParenthesizedExpressionSyntax parenthesized => BindOpenRoot(parenthesized.Expression).Members,
            _ => [BindPipelineMember(syntax)],
        };

    private Function BindGuardedExpression(GuardedExpressionSyntax syntax)
        => new("guard", [new OpenExpressionParameter(BindExpression(syntax.Expression))]);

    private OpenExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        => new([.. BindExpression(syntax.Operand).Members, new Function(BindUnaryOperator(syntax.Operator), [])]);

    private OpenExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        => new([
            new Function(
                BindBinaryOperator(syntax.Operator),
                [
                    new OpenExpressionParameter(BindExpression(syntax.Left)),
                    new OpenExpressionParameter(BindExpression(syntax.Right))
                ])
        ]);

    private OpenExpression BindExpression(ExpressionSyntax syntax) => syntax switch
    {
        GuardedExpressionSyntax guarded => new OpenExpression([BindGuardedExpression(guarded)]),
        OpenExpressionSyntax open => BindOpen(open),
        UnaryExpressionSyntax unary => BindUnaryExpression(unary),
        BinaryExpressionSyntax binary => BindBinaryExpression(binary),
        ParenthesizedExpressionSyntax parenthesized => BindOpenRoot(parenthesized.Expression),
        _ => new OpenExpression([BindPipelineMember(syntax)]),
    };

    private OpenExpression BindOpenRoot(RootExpressionSyntax syntax) => syntax switch
    {
        OpenExpressionSyntax open => BindOpen(open),
        _ => throw Unsupported(syntax),
    };

    private static string BindUnaryOperator(UnaryOperatorSyntax syntax)
        => syntax.Text switch
        {
            "!" => "not",
            _ => throw Unsupported(syntax),
        };

    private static string BindBinaryOperator(BinaryOperatorSyntax syntax)
        => syntax.Text.ToUpperInvariant() switch
        {
            "|AND" => "and",
            "|OR" => "or",
            "|XOR" => "xor",
            _ => throw Unsupported(syntax),
        };

    private Function BindPipelineMember(ExpressionSyntax syntax) => syntax switch
    {
        FunctionCallSyntax { Name: "group-map-shorthand" } shorthand => Function.FromArguments(
            "map-groups",
            BindFunctionArguments(shorthand),
            FunctionSyntax.GroupMapShorthand),
        FunctionCallSyntax call => BindFunction(call),
        TupleProjectionSyntax projection => new Function(
            "tuple-at",
            [new LiteralParameter((projection.Direction is TupleProjectionDirection.FromEnd
                ? projection.Index == 0 ? int.MinValue : -projection.Index
                : projection.Index).ToString())],
            FunctionSyntax.TupleProjectionShorthand),
        PairComponentAccessSyntax access => new Function(
            access.Component is PairComponent.Key ? "pair-key" : "pair-value",
            []),
        RecordAccessSyntax access => BindRecordAccessFunction(access),
        MapShorthandSyntax map => new Function("map", [new OpenExpressionParameter(BindOpen(map.Expression))], FunctionSyntax.MapShorthand),
        ParameterizedExpressionSyntax parameterized => new Function("map", [new OpenExpressionParameter(BindOpen(parameterized.Expression))], FunctionSyntax.MapShorthand),
        _ => throw Unsupported(syntax),
    };

    private Function BindFunction(FunctionCallSyntax syntax)
        => syntax.Name.ToLowerInvariant() switch
        {
            "coerce" => BindCoerceFunction(syntax),
            "field" => BindFieldFunction(syntax),
            "enclosing-root-field" => BindEnclosingRootField(syntax),
            "is-present" or "is-absent" => BindFieldFunction(syntax),
            "record" => BindRecordFunction(syntax),
            "with" => BindWithFunction(syntax),
            "array" or "text" or "tuple" or "grouping" or "dictionary" => Function.FromArguments(syntax.Name, BindSpreadFunctionArguments(syntax)),
            _ => Function.FromArguments(syntax.Name, BindFunctionArguments(syntax)),
        };

    private Function BindEnclosingRootField(FunctionCallSyntax syntax)
    {
        var arguments = BindFunctionArguments(syntax);
        if (arguments is not [{ Value: QuotedLiteralParameter }])
            throw new BindingException("Enclosing-root field access expects exactly one field name.");

        return Function.FromArguments(syntax.Name, arguments, FunctionSyntax.EnclosingRootFieldShorthand);
    }

    private static Function BindCoerceFunction(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments.Count == 0)
            throw new BindingException("Function 'coerce' expects one or more coercion specifications.");
        if (syntax.Arguments.Any(argument => argument is not PositionalArgumentSyntax))
            throw new BindingException("Function 'coerce' accepts positional coercion specifications only.");

        var specifications = syntax.Arguments
            .Select(argument => BindCoercionSpecification(RequireArgumentValue(argument)))
            .ToArray();
        ValidateCoercionModes(specifications);
        ValidateDuplicateCoercionSelectors(specifications);

        return new Function(syntax.Name, specifications);
    }

    private static void ValidateCoercionModes(CoercionSpecificationParameter[] specifications)
    {
        if (specifications.OfType<PositionalCoercionParameter>().Any()
            && specifications.Any(specification => specification is not PositionalCoercionParameter))
            throw new BindingException("Function 'coerce' cannot mix positional type descriptors and selector mappings.");
        if (specifications.Any(specification => specification is FieldCoercionParameter)
            && specifications.Any(specification => specification is TupleCoercionParameter))
            throw new BindingException("Function 'coerce' cannot mix field and tuple-position selector mappings.");
    }

    private static void ValidateDuplicateCoercionSelectors(CoercionSpecificationParameter[] specifications)
    {
        var duplicateField = specifications.OfType<FieldCoercionParameter>()
            .GroupBy(specification => specification.Field, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateField is not null)
            throw new BindingException($"Duplicate coercion selector '{duplicateField.Key}'.");

        var duplicatePosition = specifications.OfType<TupleCoercionParameter>()
            .GroupBy(specification => specification.Position)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicatePosition is not null)
            throw new BindingException($"Duplicate coercion selector '${duplicatePosition.Key}'.");
    }

    private static CoercionSpecificationParameter BindCoercionSpecification(ExpressionSyntax syntax)
        => syntax switch
        {
            TypeLiteralSyntax type => new PositionalCoercionParameter(ResolveCoercionType(type)),
            BinaryExpressionSyntax { Operator.Text: "->", Right: TypeLiteralSyntax type, Left: TupleProjectionSyntax selector }
                when selector.Direction is TupleProjectionDirection.FromStart
                => new TupleCoercionParameter(selector.Index, ResolveCoercionType(type)),
            BinaryExpressionSyntax { Operator.Text: "->", Right: TypeLiteralSyntax type, Left: FunctionCallSyntax selector }
                when selector.Arguments.Count == 0
                => new FieldCoercionParameter(selector.Name, ResolveCoercionType(type)),
            _ => throw new BindingException("A coerce specification must be ':type' or 'selector -> :type'."),
        };

    private static Type ResolveCoercionType(TypeLiteralSyntax syntax)
    {
        var targetType = RuntimeTypeRegistry.Resolve(syntax.Name);
        return targetType
            ?? throw new BindingException(
                $"Expressif type ':{syntax.Name}' cannot be used as a coercion target.");
    }

    private FunctionArgument[] BindSpreadFunctionArguments(FunctionCallSyntax syntax)
    {
        var arguments = new List<FunctionArgument>();
        foreach (var argument in syntax.Arguments)
        {
            if (argument is NamedArgumentSyntax)
                throw new BindingException($"Function '{syntax.Name}' does not support named arguments.");

            arguments.Add(new FunctionArgument(
                null,
                argument is SpreadArgumentSyntax { IsImplicitSpread: true }
                    ? new IncomingValueParameter()
                    : BindArgument(argument.Value
                        ?? throw new BindingException("An explicit spread argument must include an expression.")),
                argument is SpreadArgumentSyntax));
        }
        return arguments.ToArray();
    }

    private FunctionArgument[] BindFunctionArguments(FunctionCallSyntax syntax)
    {
        var arguments = new List<FunctionArgument>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasNamedArgument = false;
        foreach (var argument in syntax.Arguments)
        {
            if (argument is SpreadArgumentSyntax)
                throw new BindingException($"Function '{syntax.Name}' does not support spread arguments.");

            if (argument is NamedArgumentSyntax named)
            {
                hasNamedArgument = true;
                if (!names.Add(named.Name.Value))
                    throw new DuplicateNamedArgumentException(named.Name.Value);
                arguments.Add(new FunctionArgument(named.Name.Value, BindArgument(named.Value)));
            }
            else
            {
                if (hasNamedArgument)
                    throw new PositionalArgumentAfterNamedArgumentException(syntax.Name);
                arguments.Add(new FunctionArgument(null, BindArgument(RequireArgumentValue(argument))));
            }
        }
        return arguments.ToArray();
    }

    private Function BindFieldFunction(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments is [PositionalArgumentSyntax positional]
            && TryGetBareFunctionName(positional.Value, out var fieldName))
            return new Function(syntax.Name, [new LiteralParameter(fieldName)]);

        return new Function(syntax.Name, syntax.Arguments.Select(argument => BindArgument(RequireArgumentValue(argument))).ToArray());
    }

    private static ExpressionSyntax RequireArgumentValue(ArgumentSyntax argument)
        => argument.Value ?? throw new BindingException("A non-spread argument must include an expression.");

    private static bool TryGetBareFunctionName(ExpressionSyntax syntax, out string name)
    {
        var function = syntax switch
        {
            FunctionCallSyntax { Arguments.Count: 0 } call => call,
            OpenExpressionSyntax { Pipeline: [FunctionCallSyntax { Arguments.Count: 0 } call] } => call,
            _ => null,
        };
        name = function?.Name ?? string.Empty;
        return function is not null;
    }

    private Function BindRecordFunction(FunctionCallSyntax syntax)
        => syntax.Arguments.Count == 0
            ? new Function(syntax.Name, [])
            : new Function(syntax.Name, [new RecordDefinitionParameter(syntax.Arguments.Select(BindRecordEntry).ToArray())]);

    private Function BindWithFunction(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments.Count < 2
            || syntax.Arguments[^1] is not PositionalArgumentSyntax { Value: { } body }
            || syntax.Arguments.Take(syntax.Arguments.Count - 1).Any(argument => argument is not NamedArgumentSyntax))
            throw new BindingException("Function 'with' expects one or more named projections followed by a body expression.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        var projections = syntax.Arguments
            .Take(syntax.Arguments.Count - 1)
            .Cast<NamedArgumentSyntax>()
            .Select(named =>
            {
                if (!names.Add(named.Name.Value))
                    throw new BindingException($"Duplicate projection '{named.Name.Value}' in with(...).");
                return new WithProjection(named.Name.Value, BindArgument(named.Value));
            })
            .ToArray();

        return new Function(syntax.Name, [new WithDefinitionParameter(projections, BindArgument(body))]);
    }

    private IRecordDefinitionEntry BindRecordEntry(ArgumentSyntax syntax) => syntax switch
    {
        NamedArgumentSyntax named => new RecordNamedEntry(named.Name.Value, BindArgument(named.Value)),
        SpreadArgumentSyntax spread => new RecordSpreadEntry(
            spread.IsImplicitSpread
                ? new IncomingValueParameter()
                : BindArgument(spread.Value
                    ?? throw new BindingException("An explicit record spread must include an expression."))),
        PositionalArgumentSyntax { Value: IncomingValueSyntax } => new RecordSpreadEntry(new IncomingValueParameter()),
        _ => throw Unsupported(syntax),
    };

    private IParameter BindArgument(ExpressionSyntax syntax) => syntax switch
    {
        GuardedExpressionSyntax guarded => new OpenExpressionParameter(
            new OpenExpression([BindGuardedExpression(guarded)])),
        RecordAccessSyntax access when IsRelativeRecordAccess(access)
            => new OpenExpressionParameter(new OpenExpression([BindRecordAccessFunction(access)])),
        ValueSyntax value => BindValue(value),
        FunctionCallSyntax { Name: "enclosing-root-field" } call => BindEnclosingRootFieldParameter(call),
        FunctionCallSyntax call => new OpenExpressionParameter(new OpenExpression([BindFunction(call)])),
        UnaryExpressionSyntax unary => new OpenExpressionParameter(BindUnaryExpression(unary)),
        BinaryExpressionSyntax binary => new OpenExpressionParameter(BindBinaryExpression(binary)),
        ParenthesizedExpressionSyntax
        {
            Expression: ClosedExpressionSyntax
            {
                Value: RecordAccessSyntax access,
            } closed,
        } when IsRelativeRecordAccess(access)
            => new OpenExpressionParameter(BindRecordAccessExpression(closed, access)),
        ParenthesizedExpressionSyntax { Expression: ClosedExpressionSyntax closed }
            => new InputExpressionParameter(BindClosed(closed)),
        ParenthesizedExpressionSyntax parenthesized => new OpenExpressionParameter(BindOpenRoot(parenthesized.Expression)),
        ParameterizedExpressionSyntax parameterized => new InputExpressionParameter(new ClosedExpression(BindArgument(parameterized.Source), BindOpen(parameterized.Expression).Members)),
        OpenExpressionSyntax
        {
            Source: FunctionCallSyntax { Name: "enclosing-root-field" } call,
            Pipeline.Count: 0,
        } => BindEnclosingRootFieldParameter(call),
        OpenExpressionSyntax
        {
            Source: null,
            Pipeline: [FunctionCallSyntax { Name: "enclosing-root-field" } call],
        } => BindEnclosingRootFieldParameter(call),
        OpenExpressionSyntax open => new OpenExpressionParameter(BindOpen(open)),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed
            => new OpenExpressionParameter(BindRecordAccessExpression(closed, access)),
        ClosedExpressionSyntax closed => new InputExpressionParameter(BindClosed(closed)),
        TupleProjectionSyntax projection => new TupleProjectionParameter(
            projection.Index,
            projection.Direction is TupleProjectionDirection.FromEnd),
        PairComponentAccessSyntax access => new OpenExpressionParameter(
            new OpenExpression([BindPipelineMember(access)])),
        _ => throw Unsupported(syntax),
    };

    private static IParameter BindEnclosingRootFieldParameter(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments is not [PositionalArgumentSyntax { Value: QuotedLiteralSyntax field }])
            throw new BindingException("Enclosing-root field access expects exactly one field name.");

        return new EnclosingObjectPropertyParameter(field.Value);
    }

    private OpenExpression BindRecordAccessExpression(ClosedExpressionSyntax syntax, RecordAccessSyntax access)
        => new([BindRecordAccessFunction(access), .. syntax.Pipeline.SelectMany(BindPipelineMembers)]);

    private IParameter BindValue(ValueSyntax syntax) => syntax switch
    {
        VariableSyntax variable => new VariableParameter(variable.Name),
        IncomingValueSyntax => new IncomingValueParameter(),
        RecordAccessSyntax access => BindRecordAccessParameter(access),
        NumericLiteralSyntax numeric => new LiteralParameter(numeric.Value),
        BooleanLiteralSyntax boolean => new LiteralParameter(boolean.Value),
        NullLiteralSyntax => new LiteralParameter(null),
        QuotedLiteralSyntax quoted => new QuotedLiteralParameter(quoted.Value),
        DateLiteralSyntax date => new LiteralParameter(date.Value),
        DateTimeLiteralSyntax dateTime => new LiteralParameter(dateTime.Value),
        TimeLiteralSyntax time => new LiteralParameter(time.Value),
        TypeLiteralSyntax type => new LiteralParameter(TypeRegistry.Resolve(type.Name)),
        IntervalLiteralSyntax interval => new IntervalParameter(BindInterval(interval)),
        ArrayLiteralSyntax array => new ArrayParameter(array.Elements.Select(BindArrayElement).ToArray()),
        TupleLiteralSyntax tuple => new TupleParameter(tuple.Elements.Select(BindTupleElement).ToArray()),
        PairLiteralSyntax pair => new PairParameter(BindArgument(pair.Key), BindArgument(pair.Value)),
        GroupingLiteralSyntax grouping => new GroupingParameter(grouping.Entries
            .Select(pair => new PairParameter(BindArgument(pair.Key), BindArgument(pair.Value)))
            .ToArray()),
        DictionaryLiteralSyntax dictionary => new DictionaryParameter(dictionary.Entries
            .Select(pair => new PairParameter(BindArgument(pair.Key), BindArgument(pair.Value)))
            .ToArray()),
        RecordLiteralSyntax record => BindRecordLiteral(record),
        _ => throw Unsupported(syntax),
    };

    private ArrayElementParameter BindArrayElement(ArrayElementSyntax element)
        => new(
            element.IsImplicitSpread
                ? new IncomingValueParameter()
                : BindArgument(element.Expression
                    ?? throw new BindingException("An explicit array spread must include an expression.")),
            element.IsSpread);

    private TupleElementParameter BindTupleElement(TupleElementSyntax element)
        => new(
            element.IsImplicitSpread
                ? new IncomingValueParameter()
                : BindArgument(element.Expression
                    ?? throw new BindingException("An explicit tuple spread must include an expression.")),
            element.IsSpread);

    private RecordLiteralParameter BindRecordLiteral(RecordLiteralSyntax record)
    {
        if (record.Entries.Count != record.Fields.Count)
            throw new BindingException("Record literal spread entries must specify a field name.");

        var fields = new List<RecordLiteralField>();
        foreach (var field in record.Fields)
        {
            if (field.IsSpread)
                throw new BindingException($"Record literal field '{field.Name.Value}' does not support spread values.");

            fields.Add(new RecordLiteralField(
                field.Name.Value,
                field.Value is ValueSyntax value
                    ? BindValue(value)
                    : throw new BindingException($"Record literal field '{field.Name.Value}' must contain a value.")));
        }
        return new RecordLiteralParameter(fields.ToArray());
    }

    private static IntervalBinding BindInterval(IntervalLiteralSyntax syntax)
        => new(
            BindIntervalBound(syntax.LowerBound),
            BindIntervalBound(syntax.UpperBound),
            syntax.IsLowerInclusive,
            syntax.IsUpperInclusive);

    private static IntervalBoundBinding BindIntervalBound(IntervalBound bound)
        => bound.Kind switch
        {
            IntervalBoundKind.NegativeInfinity => new(IntervalBoundBindingKind.NegativeInfinity),
            IntervalBoundKind.PositiveInfinity => new(IntervalBoundBindingKind.PositiveInfinity),
            IntervalBoundKind.Finite when bound.Value is { } value
                => new(IntervalBoundBindingKind.Finite, BindIntervalBoundValue(value)),
            IntervalBoundKind.Finite => throw new BindingException("A finite interval bound must have a value."),
            _ => throw new BindingException($"Unsupported interval bound kind '{bound.Kind}'."),
        };

    private static object BindIntervalBoundValue(ValueSyntax syntax) => syntax switch
    {
        NumericLiteralSyntax numeric => numeric.Value,
        DateLiteralSyntax date => date.Value,
        DateTimeLiteralSyntax dateTime => dateTime.Value,
        TimeLiteralSyntax time => time.Value,
        _ => throw Unsupported(syntax),
    };

    private static IParameter BindRecordAccessParameter(RecordAccessSyntax syntax)
    {
        if (!syntax.IsOriginalInput || syntax.Fields.Count != 1)
            throw new BindingException($"Record access '{syntax.Text}' cannot be used as a scalar parameter in this iteration.");
        var field = syntax.Fields.Single();
        return field switch
        {
            { Name: string name } => new ObjectPropertyParameter(name),
            { Index: int index } => new ObjectIndexParameter(index),
            _ => throw InvalidRecordFieldSelector(syntax),
        };
    }

    private static Function BindRecordAccessFunction(RecordAccessSyntax syntax)
    {
        if (syntax.Fields.Count != 1)
            throw new BindingException($"Nested record access '{syntax.Text}' is not bound in this iteration.");
        var field = syntax.Fields.Single();
        var value = field switch
        {
            { Name: string name } => name,
            { Index: int index } => index.ToString(),
            _ => throw InvalidRecordFieldSelector(syntax),
        };
        return new Function(
            "field",
            [new LiteralParameter(value)],
            syntax.IsOriginalInput ? FunctionSyntax.RootFieldShorthand : FunctionSyntax.FieldShorthand);
    }

    private static BindingException InvalidRecordFieldSelector(RecordAccessSyntax syntax)
        => new($"Record access '{syntax.Text}' contains neither a named nor positional field selector.");
    private static bool IsRelativeRecordAccess(RecordAccessSyntax syntax)
        => !syntax.IsOriginalInput && syntax.Text.StartsWith('.');
    private static BindingException Unsupported(SyntaxNode syntax)
        => new($"Syntax kind '{syntax.Kind}' is not bound in this iteration (source: '{syntax.Text}').");
}
