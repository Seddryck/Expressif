using Expressif.Syntax;

namespace Expressif.Bindings;

public sealed class ExpressifBinder
{
    public IRootExpression Bind(RootExpressionSyntax syntax) => syntax switch
    {
        OpenExpressionSyntax open => new OpenRootExpression(BindOpen(open)),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed when IsRelativeRecordAccess(access)
            => new OpenRootExpression(BindRelativeRecordAccess(closed, access)),
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

        return new SinglePredication(members);
    }

    private OpenExpression BindOpen(OpenExpressionSyntax syntax)
        => new([
            .. syntax.Source is null ? [] : BindPipelineMembers(syntax.Source),
            .. syntax.Pipeline.SelectMany(BindPipelineMembers),
        ]);

    private ClosedExpression BindClosed(ClosedExpressionSyntax syntax)
        => new(BindValue(syntax.Value), syntax.Pipeline.SelectMany(BindPipelineMembers));

    private IEnumerable<Function> BindPipelineMembers(ExpressionSyntax syntax)
        => syntax switch
        {
            UnaryExpressionSyntax unary => BindUnaryExpression(unary).Members,
            BinaryExpressionSyntax binary => BindBinaryExpression(binary).Members,
            ParenthesizedExpressionSyntax { Expression: OpenExpressionSyntax open } => BindOpen(open).Members,
            ParenthesizedExpressionSyntax
            {
                Expression: ClosedExpressionSyntax
                {
                    Value: RecordAccessSyntax access,
                } closed,
            } when IsRelativeRecordAccess(access) => BindRelativeRecordAccess(closed, access).Members,
            ParenthesizedExpressionSyntax parenthesized => BindOpenRoot(parenthesized.Expression).Members,
            _ => [BindPipelineMember(syntax)],
        };

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
        FunctionCallSyntax call => BindFunction(call),
        TupleProjectionSyntax projection => new Function("tuple-at", [new LiteralParameter(projection.Index.ToString())], FunctionSyntax.TupleProjectionShorthand),
        RecordAccessSyntax access when !access.IsOriginalInput => BindRecordAccessFunction(access),
        MapShorthandSyntax map => new Function("map", [new OpenExpressionParameter(BindOpen(map.Expression))], FunctionSyntax.MapShorthand),
        ParameterizedExpressionSyntax parameterized => new Function("map", [new OpenExpressionParameter(BindOpen(parameterized.Expression))], FunctionSyntax.MapShorthand),
        _ => throw Unsupported(syntax),
    };

    private Function BindFunction(FunctionCallSyntax syntax)
        => syntax.Name.ToLowerInvariant() switch
        {
            "field" => BindFieldFunction(syntax),
            "record" => BindRecordFunction(syntax),
            _ => Function.FromArguments(syntax.Name, BindFunctionArguments(syntax)),
        };

    private FunctionArgument[] BindFunctionArguments(FunctionCallSyntax syntax)
    {
        var arguments = new List<FunctionArgument>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasNamedArgument = false;
        foreach (var argument in syntax.Arguments)
        {
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
                arguments.Add(new FunctionArgument(null, BindArgument(argument.Value)));
            }
        }
        return arguments.ToArray();
    }

    private Function BindFieldFunction(FunctionCallSyntax syntax)
    {
        if (syntax.Arguments is [PositionalArgumentSyntax positional]
            && TryGetBareFunctionName(positional.Value, out var fieldName))
            return new Function(syntax.Name, [new LiteralParameter(fieldName)]);

        return new Function(syntax.Name, syntax.Arguments.Select(argument => BindArgument(argument.Value)).ToArray());
    }

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

    private IRecordDefinitionEntry BindRecordEntry(ArgumentSyntax syntax) => syntax switch
    {
        NamedArgumentSyntax named => new RecordNamedEntry(named.Name.Value, BindArgument(named.Value)),
        PositionalArgumentSyntax { Value: IncomingValueSyntax } => new RecordSpreadEntry(),
        _ => throw Unsupported(syntax),
    };

    private IParameter BindArgument(ExpressionSyntax syntax) => syntax switch
    {
        RecordAccessSyntax access when IsRelativeRecordAccess(access)
            => new OpenExpressionParameter(new OpenExpression([BindRecordAccessFunction(access)])),
        ValueSyntax value => BindValue(value),
        FunctionCallSyntax call => new OpenExpressionParameter(new OpenExpression([BindFunction(call)])),
        UnaryExpressionSyntax unary => new OpenExpressionParameter(BindUnaryExpression(unary)),
        BinaryExpressionSyntax binary => new OpenExpressionParameter(BindBinaryExpression(binary)),
        ParenthesizedExpressionSyntax { Expression: ClosedExpressionSyntax closed }
            => new InputExpressionParameter(BindClosed(closed)),
        ParenthesizedExpressionSyntax parenthesized => new OpenExpressionParameter(BindOpenRoot(parenthesized.Expression)),
        ParameterizedExpressionSyntax parameterized => new InputExpressionParameter(new ClosedExpression(BindArgument(parameterized.Source), BindOpen(parameterized.Expression).Members)),
        OpenExpressionSyntax open => new OpenExpressionParameter(BindOpen(open)),
        ClosedExpressionSyntax { Value: RecordAccessSyntax access } closed when IsRelativeRecordAccess(access)
            => new OpenExpressionParameter(BindRelativeRecordAccess(closed, access)),
        ClosedExpressionSyntax closed => new InputExpressionParameter(BindClosed(closed)),
        TupleProjectionSyntax projection => new TupleProjectionParameter(
            projection.Index,
            projection.Direction is TupleProjectionDirection.FromEnd),
        _ => throw Unsupported(syntax),
    };

    private OpenExpression BindRelativeRecordAccess(ClosedExpressionSyntax syntax, RecordAccessSyntax access)
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
        IntervalLiteralSyntax interval => new IntervalParameter(BindInterval(interval)),
        ArrayLiteralSyntax array => new ArrayParameter(array.Values.Select(BindArgument).ToArray()),
        TupleLiteralSyntax tuple => new TupleParameter(tuple.Values.Select(BindValue).ToArray()),
        RecordLiteralSyntax record => new RecordLiteralParameter(record.Fields.Select(field => new RecordLiteralField(field.Name.Value, BindValue(field.Value))).ToArray()),
        _ => throw Unsupported(syntax),
    };

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
        return new Function("field", [new LiteralParameter(value)], FunctionSyntax.FieldShorthand);
    }

    private static BindingException InvalidRecordFieldSelector(RecordAccessSyntax syntax)
        => new($"Record access '{syntax.Text}' contains neither a named nor positional field selector.");
    private static bool IsRelativeRecordAccess(RecordAccessSyntax syntax)
        => !syntax.IsOriginalInput && syntax.Text.StartsWith('.');
    private static BindingException Unsupported(SyntaxNode syntax)
        => new($"Syntax kind '{syntax.Kind}' is not bound in this iteration (source: '{syntax.Text}').");
}
