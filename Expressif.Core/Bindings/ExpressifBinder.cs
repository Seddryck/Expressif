using Expressif.Syntax;
using Expressif.Functions;
using Expressif.Types;
using Expressif.Values;
using Expressif.Functions.Accumulation;

namespace Expressif.Bindings;

internal sealed class ExpressifBinder : IFunctionBindingContext
{
    private readonly IImplementationRegistry[] implementationRegistries;
    private readonly FunctionBinderRegistry functionBinders;
    private readonly ITypeRegistry typeRegistry;
    private readonly QuotedLiteralRegistry quotedLiteralRegistry;
    private bool inputBoundBody;

    internal BindingSourceMap Sources { get; }

    internal ExpressifBinder(
        IEnumerable<IImplementationRegistry> implementationRegistries,
        FunctionBinderRegistry functionBinders,
        ITypeRegistry typeRegistry,
        QuotedLiteralRegistry? quotedLiteralRegistry = null)
        : this(
            implementationRegistries,
            functionBinders,
            typeRegistry,
            false,
            quotedLiteralRegistry) { }

    internal ExpressifBinder(
        IEnumerable<IImplementationRegistry> implementationRegistries,
        FunctionBinderRegistry functionBinders,
        ITypeRegistry typeRegistry,
        bool trackSources,
        QuotedLiteralRegistry? quotedLiteralRegistry = null)
        => (this.implementationRegistries, this.functionBinders, this.typeRegistry,
            this.quotedLiteralRegistry, Sources) =
            (implementationRegistries.ToArray(), functionBinders, typeRegistry,
                quotedLiteralRegistry ?? QuotedLiteralRegistry.Default, new(trackSources));

    TypeDescriptor IFunctionBindingContext.ResolveType(string name)
        => typeRegistry.Resolve(name);

    Type? IFunctionBindingContext.ResolveRuntimeType(string name)
        => typeRegistry.ResolveRuntimeType(name);

    internal IRootExpression Bind(RootExpressionSyntax syntax)
    {
        if (syntax is ClosedExpressionSyntax { Value: IncomingValueSyntax, Pipeline: [InputBindingExpressionSyntax binding] })
            return new OpenRootExpression(BindInputBound(binding));
        return BindRoot(syntax);
    }

    private IRootExpression BindRoot(RootExpressionSyntax syntax) => syntax switch
    {
        OpenExpressionSyntax open => new OpenRootExpression(BindOpen(open)),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed when IsRelativeRecordAccess(access)
            => new OpenRootExpression(BindRecordAccessExpression(closed, access)),
        ClosedExpressionSyntax closed => new ClosedRootExpression(BindClosed(closed)),
        _ => throw Unsupported(syntax),
    };

    internal IParameter BindParameter(RootExpressionSyntax syntax)
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

    private OpenExpression BindOpen(OpenExpressionSyntax syntax)
    {
        if (syntax.Source is null && syntax.Pipeline is [InputBindingExpressionSyntax binding])
            return BindInputBound(binding);
        Function[] members =
        [
            .. (syntax.Source is null ? [] : BindPipelineMembers(syntax.Source)),
            .. syntax.Pipeline.SelectMany(BindPipelineMembers),
        ];
        return new OpenExpression(members);
    }

    private ClosedExpression BindClosed(ClosedExpressionSyntax syntax)
    {
        var source = BindValue(syntax.Value);
        var members = syntax.Pipeline.SelectMany(BindPipelineMembers).ToArray();
        return new ClosedExpression(source, members);
    }

    private IEnumerable<Function> BindPipelineMembers(ExpressionSyntax syntax)
        => syntax switch
        {
            TupleBindingShorthandSyntax shorthand => BindTupleBinding(shorthand),
            RecordAccessSyntax access => BindRecordAccessFunctions(access),
            GuardedExpressionSyntax guarded => [BindGuardedExpression(guarded)],
            UnaryExpressionSyntax unary => BindUnaryExpression(unary).Members,
            BinaryExpressionSyntax binary => BindBinaryExpression(binary).Members,
            ConditionalExpressionSyntax conditional => BindConditionalExpression(conditional).Members,
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

    private IEnumerable<Function> BindTupleBinding(TupleBindingShorthandSyntax syntax)
    {
        if (syntax.Direction == TupleBindingDirection.Prefix)
            yield return Sources.Add(new Function("rotate", []) { SourceSpan = syntax.Span }, syntax);
        yield return Sources.Add(new Function("bind", [new QuotedLiteralParameter(syntax.Name)]) { SourceSpan = syntax.Span }, syntax);
    }

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

    private OpenExpression BindConditionalExpression(ConditionalExpressionSyntax syntax)
        => new([
            new Function(
                syntax.Operator.Direction is ConditionalDirection.Forward
                    ? "conditional-forward"
                    : "conditional-backward",
                [BindArgument(syntax.Left), BindArgument(syntax.Right)],
                syntax.Operator.Direction is ConditionalDirection.Forward
                    ? BoundFunctionRole.ConditionalForward
                    : BoundFunctionRole.ConditionalBackward)
        ]);

    private OpenExpression BindExpression(ExpressionSyntax syntax) => syntax switch
    {
        GuardedExpressionSyntax guarded => new OpenExpression([BindGuardedExpression(guarded)]),
        OpenExpressionSyntax open => BindOpen(open),
        UnaryExpressionSyntax unary => BindUnaryExpression(unary),
        BinaryExpressionSyntax binary => BindBinaryExpression(binary),
        ConditionalExpressionSyntax conditional => BindConditionalExpression(conditional),
        ParenthesizedExpressionSyntax parenthesized => BindOpenRoot(parenthesized.Expression),
        _ => new OpenExpression(BindPipelineMembers(syntax)),
    };

    private OpenExpression BindOpenRoot(RootExpressionSyntax syntax) => syntax switch
    {
        OpenExpressionSyntax open => BindOpen(open),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed
            when access.RootDepth > 0 => BindRecordAccessExpression(closed, access),
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

    private Function BindPipelineMember(ExpressionSyntax syntax)
        => Sources.Add(BindPipelineMemberCore(syntax), syntax);

    private Function BindPipelineMemberCore(ExpressionSyntax syntax) => syntax switch
    {
        InputBindingExpressionSyntax binding => new Function("apply", [new OpenExpressionParameter(BindInputBound(binding))], BoundFunctionRole.InputBinding),
        ValueReferenceStageSyntax reference => new Function("apply", [new VariableParameter(reference.Reference.Name)]),
        TupleProjectionSyntax { RootDepth: > 0 } reference => new Function(
            "tuple-at",
            [new ScopedTupleProjectionParameter(reference.Index, reference.RootDepth)],
            SourceNotation.ScopedTupleProjectionShorthand),
        GroupingMapShorthandSyntax map => new Function(
            "map-groups",
            [new OpenExpressionParameter(BindOpen(map.Expression))],
            SourceNotation.GroupMapShorthand),
        ControlFlowCallSyntax controlFlow => BindControlFlowCall(controlFlow),
        FunctionCallSyntax call => BindFunction(call),
        TupleProjectionSyntax projection => BindTupleProjection(projection),
        PairComponentAccessSyntax access => new Function(
            access.Component is PairComponent.Key ? "pair-key" : "pair-value",
            []),
        MapShorthandSyntax map => new Function("map", [new OpenExpressionParameter(BindOpen(map.Expression))], SourceNotation.MapShorthand),
        ParameterizedExpressionSyntax parameterized => new Function("map", [new OpenExpressionParameter(BindOpen(parameterized.Expression))], SourceNotation.MapShorthand),
        _ => throw Unsupported(syntax),
    };

    private Function BindTupleProjection(TupleProjectionSyntax projection)
    {
        if (inputBoundBody)
        {
            return new Function(
                "tuple-at",
                [new TupleProjectionParameter(
                    projection.Index,
                    projection.Direction == TupleProjectionDirection.FromEnd)],
                SourceNotation.InputTupleProjectionShorthand);
        }

        var position = projection.Direction == TupleProjectionDirection.FromEnd
            ? projection.Index == 0 ? int.MinValue : -projection.Index
            : projection.Index;
        return new Function(
            "tuple-at",
            [new LiteralParameter(position.ToString())],
            SourceNotation.TupleProjectionShorthand);
    }

    private Function BindControlFlowCall(ControlFlowCallSyntax syntax)
    {
        var isTry = syntax.Name.Equals("try", StringComparison.OrdinalIgnoreCase);
        if (syntax.Branches.Count < (isTry ? 2 : 1))
            throw new BindingException($"Function '{syntax.Name}' has too few branches.");
        var branches = new List<IParameter>();
        for (var index = 0; index < syntax.Branches.Count; index++)
        {
            switch (syntax.Branches[index])
            {
                case ControlFlowFallbackSyntax fallback when index > 0 && index == syntax.Branches.Count - 1:
                    branches.Add(new ControlFlowBranchParameter(BindArgument(fallback.Action), null));
                    break;
                case ConditionalControlFlowBranchSyntax branch:
                    branches.Add(new ControlFlowBranchParameter(
                        BindArgument(isTry ? branch.Condition : branch.Action),
                        BindArgument(isTry ? branch.Action : branch.Condition)));
                    break;
                case ControlFlowFallbackSyntax:
                    throw new BindingException("A catch-all fallback must follow ordinary branches and be final.");
                default:
                    throw new BindingException("Invalid control-flow branch.");
            }
        }
        return new Function(syntax.Name.ToLowerInvariant(), branches.ToArray());
    }

    private Function BindFunction(FunctionCallSyntax syntax)
        {
        var function = Sources.Add(BindFunctionCore(syntax), syntax);
        function.SourceSpan = syntax.Span;
        return function;
    }

    private Function BindFunctionCore(FunctionCallSyntax syntax)
    {
        if (TryResolveFunctionType(syntax.Name, out var functionType)
            && functionBinders.TryGet(functionType, out var binder))
            return binder.Bind(syntax, this);

        var arguments = BindFunctionArguments(syntax);
        return arguments.Length == 0 && IsAccumulator(syntax.Name)
            ? Function.FromArguments(syntax.Name, arguments, BoundFunctionRole.ImplicitAccumulator)
            : Function.FromArguments(syntax.Name, arguments);
    }

    private bool IsAccumulator(string name)
        => implementationRegistries.Any(registry => registry.TryResolve(name, out var implementationType)
            && typeof(IIncrementalAggregation).IsAssignableFrom(implementationType));

    private bool TryResolveFunctionType(string name, out Type functionType)
    {
        foreach (var registry in implementationRegistries)
        {
            if (registry.TryResolve(name, out functionType))
                return true;
        }
        functionType = null!;
        return false;
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

    private static ExpressionSyntax RequireArgumentValue(ArgumentSyntax argument)
        => argument.Value ?? throw new BindingException("A non-spread argument must include an expression.");

    private IParameter BindArgument(ExpressionSyntax syntax)
        => Sources.Add(BindArgumentCore(syntax), syntax);

    IParameter IFunctionBindingContext.BindArgument(ExpressionSyntax syntax)
        => BindArgument(syntax);

    private IParameter BindArgumentCore(ExpressionSyntax syntax) => syntax switch
    {
        _ when FindTupleScope(syntax) is { } reference => new ScopedTupleProjectionParameter(reference.Index, reference.RootDepth),
        TupleBindingShorthandSyntax shorthand => new OpenExpressionParameter(new OpenExpression(BindTupleBinding(shorthand))),
        InputBindingExpressionSyntax bound => new OpenExpressionParameter(BindInputBound(bound)),
        GuardedExpressionSyntax guarded => new OpenExpressionParameter(
            new OpenExpression([BindGuardedExpression(guarded)])),
        RecordAccessSyntax access when IsRelativeRecordAccess(access)
            => new OpenExpressionParameter(new OpenExpression(BindRecordAccessFunctions(access))),
        ValueSyntax value => BindValue(value),
        FunctionCallSyntax call => new OpenExpressionParameter(new OpenExpression([BindFunction(call)])),
        UnaryExpressionSyntax unary => new OpenExpressionParameter(BindUnaryExpression(unary)),
        BinaryExpressionSyntax binary => new OpenExpressionParameter(BindBinaryExpression(binary)),
        ConditionalExpressionSyntax conditional => new OpenExpressionParameter(BindConditionalExpression(conditional)),
        ControlFlowCallSyntax controlFlow => new OpenExpressionParameter(new OpenExpression([BindControlFlowCall(controlFlow)])),
        ParenthesizedExpressionSyntax
        {
            Expression: ClosedExpressionSyntax
            {
                Value: RecordAccessSyntax access,
            } closed,
        } when IsRelativeRecordAccess(access)
            => new OpenExpressionParameter(BindRecordAccessExpression(closed, access)),
        ParenthesizedExpressionSyntax { Expression: ClosedExpressionSyntax { Value: IncomingValueSyntax, Pipeline: [InputBindingExpressionSyntax binding] } }
            => new OpenExpressionParameter(BindInputBound(binding)),
        ParenthesizedExpressionSyntax { Expression: ClosedExpressionSyntax closed }
            => new InputExpressionParameter(BindClosed(closed)),
        ParenthesizedExpressionSyntax parenthesized => new OpenExpressionParameter(BindOpenRoot(parenthesized.Expression)),
        ParameterizedExpressionSyntax parameterized => new InputExpressionParameter(new ClosedExpression(BindArgument(parameterized.Source), BindOpen(parameterized.Expression).Members)),
        OpenExpressionSyntax open => new OpenExpressionParameter(BindOpen(open)),
        ClosedExpressionSyntax { Value: IncomingValueSyntax, Pipeline: [InputBindingExpressionSyntax binding] }
            => new OpenExpressionParameter(BindInputBound(binding)),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed
            => new OpenExpressionParameter(BindRecordAccessExpression(closed, access)),
        ClosedExpressionSyntax closed => new InputExpressionParameter(BindClosed(closed)),
        TupleProjectionSyntax { RootDepth: > 0 } projection => new ScopedTupleProjectionParameter(projection.Index, projection.RootDepth),
        TupleProjectionSyntax projection => new TupleProjectionParameter(
            projection.Index,
            projection.Direction is TupleProjectionDirection.FromEnd),
        PairComponentAccessSyntax access => new OpenExpressionParameter(
            new OpenExpression([BindPipelineMember(access)])),
        _ => throw Unsupported(syntax),
    };

    private static TupleProjectionSyntax? FindTupleScope(ExpressionSyntax syntax) => syntax switch
    {
        TupleProjectionSyntax { RootDepth: > 0 } reference => reference,
        ParenthesizedExpressionSyntax parenthesized => FindTupleScope(parenthesized.Expression),
        OpenExpressionSyntax { Source: null, Pipeline: [var member] } => FindTupleScope(member),
        OpenExpressionSyntax { Source: { } source, Pipeline.Count: 0 } => FindTupleScope(source),
        _ => null,
    };

    private static string[] BindPositionalNames(PositionalBindingPatternSyntax pattern)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in pattern.Names)
        {
            if (!names.Add(name.Name))
                throw new BindingException($"Duplicate input binding name '{name.Name}' at offset {name.Span.Start}.");
        }
        return pattern.Names.Select(name => name.Name).ToArray();
    }

    private OpenExpression BindInputBound(InputBindingExpressionSyntax syntax)
    {
        var names = syntax.Binding switch
        {
            BindingNameSyntax name => new[] { name.Name },
            null => [],
            PositionalBindingPatternSyntax pattern => BindPositionalNames(pattern),
            _ => throw new BindingException("Unsupported input binding declaration."),
        };
        var previous = inputBoundBody;
        inputBoundBody = true;
        try
        {
            return new OpenExpression(new InputBoundExpression(
                names,
                syntax.Binding is PositionalBindingPatternSyntax,
                Bind(syntax.Body)));
        }
        finally
        {
            inputBoundBody = previous;
        }
    }

    private OpenExpression BindRecordAccessExpression(ClosedExpressionSyntax syntax, RecordAccessSyntax access)
        => new([.. BindRecordAccessFunctions(access), .. syntax.Pipeline.SelectMany(BindPipelineMembers)]);

    private IParameter BindValue(ValueSyntax syntax)
        => Sources.Add(BindValueCore(syntax), syntax);

    private IParameter BindValueCore(ValueSyntax syntax) => syntax switch
    {
        VariableSyntax variable => new VariableParameter(variable.Name),
        IncomingValueSyntax => new IncomingValueParameter(),
        RecordAccessSyntax access => BindRecordAccessParameter(access),
        NumericLiteralSyntax numeric => new LiteralParameter(numeric.Value),
        BooleanLiteralSyntax boolean => new LiteralParameter(boolean.Value),
        NullLiteralSyntax => new LiteralParameter(null),
        AllLiteralSyntax => new LiteralParameter(AllDimension.Instance),
        OrderingLiteralSyntax ordering => new LiteralParameter(ordering.Text switch
        {
            "#less" => OrderingValue.Less,
            "#equal" => OrderingValue.Equal,
            "#greater" => OrderingValue.Greater,
            _ => throw Unsupported(ordering),
        }),
        QuotedLiteralSyntax quoted => new QuotedLiteralParameter(quoted.Value),
        QuotedTypedLiteralSyntax quoted => BindQuotedTypedLiteral(quoted),
        DateLiteralSyntax date => new LiteralParameter(date.Value, "date"),
        DateTimeLiteralSyntax dateTime => new LiteralParameter(dateTime.Value, "datetime"),
        TimeLiteralSyntax time => new LiteralParameter(time.Value, "time"),
        TypeLiteralSyntax type => new LiteralParameter(typeRegistry.Resolve(type.Name)),
        IntervalLiteralSyntax interval => new IntervalParameter(BindInterval(interval)),
        ArrayLiteralSyntax array => new ArrayParameter(array.Elements.Select(BindArrayElement).ToArray()),
        VectorLiteralSyntax vector => new VectorParameter(vector.Elements.Select(BindVectorElement).ToArray()),
        TupleLiteralSyntax tuple => BindTupleLike(tuple),
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

    private IParameter BindQuotedTypedLiteral(QuotedTypedLiteralSyntax syntax)
    {
        var typeName = syntax.Type?.Name ?? string.Empty;
        var literal = quotedLiteralRegistry.Parse(syntax.Representation.Value, typeName, typeRegistry);
        return new LiteralParameter(literal.Value, literal.TypeName, !string.IsNullOrEmpty(typeName));
    }

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

    private TupleElementParameter BindVectorElement(VectorElementSyntax element)
        => new(
            element.IsImplicitSpread
                ? new IncomingValueParameter()
                : BindArgument(element.Expression
                    ?? throw new BindingException("An explicit vector spread must include an expression.")),
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

    private IParameter BindTupleLike(TupleLiteralSyntax tuple)
    {
        var elements = tuple.Elements.Select(BindTupleElement).ToArray();
        return new TupleParameter(elements);
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

    private IParameter BindRecordAccessParameter(RecordAccessSyntax syntax)
    {
        if (syntax.RootDepth == 0 || syntax.Fields.Count == 0)
            throw new BindingException($"Record access '{syntax.Text}' cannot be used as a scalar parameter in this iteration.");
        var field = syntax.Fields.First();
        IParameter source = field switch
        {
            { Name: string name } when syntax.RootDepth == 1 => new ObjectPropertyParameter(name),
            { Name: string name } when syntax.RootDepth == 2 => new EnclosingObjectPropertyParameter(name),
            { Index: int index } => new ObjectIndexParameter(index),
            _ => throw InvalidRecordFieldSelector(syntax),
        };
        Sources.AddField(source, syntax, 0);
        return syntax.Fields.Count == 1
            ? source
            : new InputExpressionParameter(new ClosedExpression(source, BindRecordAccessFunctions(syntax).Skip(1)));
    }

    private IEnumerable<Function> BindRecordAccessFunctions(RecordAccessSyntax syntax)
    {
        return syntax.Fields.Select((field, index) => Sources.AddField(new Function(
            "field",
            [new LiteralParameter(field switch
            {
                { Name: string name } => name,
                { Index: int position } => position.ToString(),
                _ => throw InvalidRecordFieldSelector(syntax),
            })],
            (index == 0 ? syntax.RootDepth : 0) switch
            {
                0 when index == 0 && inputBoundBody => SourceNotation.InputFieldShorthand,
                0 => SourceNotation.CurrentField,
                1 => SourceNotation.RootField,
                2 => SourceNotation.EnclosingRootField,
                _ => throw new BindingException($"Expression root depth '{syntax.RootDepth}' is not supported."),
            }), syntax, index)).ToArray();
    }

    private static BindingException InvalidRecordFieldSelector(RecordAccessSyntax syntax)
        => new($"Record access '{syntax.Text}' contains neither a named nor positional field selector.");
    private static bool IsRelativeRecordAccess(RecordAccessSyntax syntax)
        => syntax.RootDepth == 0 && syntax.Text.StartsWith('.');
    private static BindingException Unsupported(SyntaxNode syntax)
        => new($"Syntax kind '{syntax.Kind}' is not bound in this iteration (source: '{syntax.Text}').");
}
