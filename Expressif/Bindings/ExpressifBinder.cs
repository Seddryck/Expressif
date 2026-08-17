using Expressif.Syntax;

namespace Expressif.Bindings;

public sealed class ExpressifBinder
{
    public IRootExpression Bind(string source) => Bind(ExpressifSyntax.Parse(source));

    public IRootExpression Bind(RootExpressionSyntax syntax) => syntax switch
    {
        OpenExpressionSyntax open => new OpenRootExpression(BindOpen(open)),
        ClosedExpressionSyntax closed => new ClosedRootExpression(BindClosed(closed)),
        _ => throw Unsupported(syntax),
    };

    public Function BindFunction(string source)
    {
        var root = Bind(source);
        return root is OpenRootExpression open && open.Expression.Members.Count() == 1
            ? open.Expression.Members.Single()
            : throw new BindingException($"Source '{source}' is not a single function.");
    }

    public IParameter BindParameter(string source)
    {
        var root = Bind(source);
        return root is ClosedRootExpression closed && !closed.Expression.Members.Any()
            ? closed.Expression.Parameter
            : throw new BindingException($"Source '{source}' is not a standalone parameter.");
    }

    public IPredication BindPredication(string source)
    {
        var root = Bind(source);
        return root is OpenRootExpression open && open.Expression.Members.Count() == 1
            ? new SinglePredication(open.Expression.Members.Single())
            : throw new BindingException($"Predication '{source}' is not bound in this iteration.");
    }

    private OpenExpression BindOpen(OpenExpressionSyntax syntax)
        => new(syntax.Pipeline.SelectMany(BindPipelineMembers));

    private ClosedExpression BindClosed(ClosedExpressionSyntax syntax)
        => new(BindValue(syntax.Value), syntax.Pipeline.SelectMany(BindPipelineMembers));

    private IEnumerable<Function> BindPipelineMembers(ExpressionSyntax syntax)
        => syntax switch
        {
            ParenthesizedExpressionSyntax { Expression: OpenExpressionSyntax open } => BindOpen(open).Members,
            _ => [BindPipelineMember(syntax)],
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
        => new(syntax.Name, syntax.Arguments.Select(argument => BindArgument(argument.Value)).ToArray());

    private IParameter BindArgument(ExpressionSyntax syntax) => syntax switch
    {
        ValueSyntax value => BindValue(value),
        FunctionCallSyntax call => new OpenExpressionParameter(new OpenExpression([BindFunction(call)])),
        ParameterizedExpressionSyntax parameterized => new InputExpressionParameter(new ClosedExpression(BindArgument(parameterized.Source), BindOpen(parameterized.Expression).Members)),
        OpenExpressionSyntax open => new OpenExpressionParameter(BindOpen(open)),
        ClosedExpressionSyntax closed => new InputExpressionParameter(BindClosed(closed)),
        TupleProjectionSyntax projection => new TupleProjectionParameter(
            projection.Index,
            projection.Direction is TupleProjectionDirection.FromEnd),
        _ => throw Unsupported(syntax),
    };

    private IParameter BindValue(ValueSyntax syntax) => syntax switch
    {
        VariableSyntax variable => new VariableParameter(variable.Name),
        RecordAccessSyntax access => BindRecordAccessParameter(access),
        NumericLiteralSyntax numeric => new LiteralParameter(numeric.Value),
        BooleanLiteralSyntax boolean => new LiteralParameter(boolean.Value),
        QuotedLiteralSyntax quoted => new QuotedLiteralParameter(quoted.Value),
        DateLiteralSyntax date => new LiteralParameter(date.Value),
        DateTimeLiteralSyntax dateTime => new LiteralParameter(dateTime.Value),
        TimeLiteralSyntax time => new LiteralParameter(time.Value),
        ArrayLiteralSyntax array => new ArrayParameter(array.Values.Select(BindValue).ToArray()),
        TupleLiteralSyntax tuple => new TupleParameter(tuple.Values.Select(BindValue).ToArray()),
        RecordLiteralSyntax record => new RecordLiteralParameter(record.Fields.Select(field => new RecordLiteralField(field.Name, BindValue(field.Value))).ToArray()),
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
    private static BindingException Unsupported(SyntaxNode syntax)
        => new($"Syntax kind '{syntax.Kind}' is not bound in this iteration (source: '{syntax.Text}').");
}
