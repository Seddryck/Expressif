namespace Expressif.Testing.Bindings;

internal static class SyntaxFactory
{
    private static SourceSpan Span(string text) => new(0, text.Length);

    public static FunctionCallSyntax Function(string name, params ArgumentSyntax[] arguments)
        => new(Span(name), name, name, arguments.Length > 0, arguments);

    public static PositionalArgumentSyntax Argument(ExpressionSyntax value)
        => new(value.Span, value.Text, value);

    public static NamedArgumentSyntax Named(string name, ExpressionSyntax value)
        => new(Span(name), name, new ArgumentNameSyntax(Span(name), name, name, false, null), value);

    public static SpreadArgumentSyntax Spread(ExpressionSyntax? value = null)
        => new(Span("..."), "...", value);

    public static OpenExpressionSyntax Open(ExpressionSyntax? source = null, params ExpressionSyntax[] pipeline)
        => new(new SourceSpan(0, 0), string.Empty, source, pipeline);

    public static ClosedExpressionSyntax Closed(ValueSyntax value, params ExpressionSyntax[] pipeline)
        => new(new SourceSpan(0, 0), string.Empty, value, pipeline);

    public static ParenthesizedExpressionSyntax Parenthesized(RootExpressionSyntax expression)
        => new(new SourceSpan(0, 0), string.Empty, expression);

    public static MapShorthandSyntax Map(params ExpressionSyntax[] pipeline)
        => new(new SourceSpan(0, 0), string.Empty, Open(null, pipeline));

    public static NumericLiteralSyntax Number(decimal value)
    {
        var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return new NumericLiteralSyntax(Span(text), text);
    }

    public static QuotedLiteralSyntax Text(string value)
    {
        var text = $"\"{value}\"";
        return new QuotedLiteralSyntax(Span(text), text, QuotingStyle.DoubleQuote);
    }

    public static BooleanLiteralSyntax Boolean(bool value)
    {
        var text = value ? "#true" : "#false";
        return new BooleanLiteralSyntax(Span(text), text);
    }

    public static VariableSyntax Variable(string name)
    {
        var text = $"@{name}";
        return new VariableSyntax(Span(text), text);
    }

    public static IncomingValueSyntax Incoming() => new(Span("@_"), "@_");

    public static NullLiteralSyntax Null() => new(Span("#null"), "#null");

    public static ArrayLiteralSyntax Array(params ExpressionSyntax[] values)
        => new(
            new SourceSpan(0, 0),
            string.Empty,
            values.Select(value => new ArrayElementSyntax(value.Span, value.Text, value, false)));

    public static ArrayLiteralSyntax Array(params ArrayElementSyntax[] elements)
        => new(new SourceSpan(0, 0), string.Empty, elements);

    public static ArrayElementSyntax ArrayElement(ExpressionSyntax? value, bool spread = false)
        => new(new SourceSpan(0, 0), string.Empty, value, spread);

    public static TupleLiteralSyntax Tuple(params ValueSyntax[] values)
        => new(new SourceSpan(0, 0), string.Empty, values.Select(value => TupleElement(value)));

    public static TupleLiteralSyntax Tuple(params TupleElementSyntax[] elements)
        => new(new SourceSpan(0, 0), string.Empty, elements);

    public static TupleElementSyntax TupleElement(ExpressionSyntax? value, bool spread = false)
        => new(new SourceSpan(0, 0), string.Empty, value, spread);

    public static RecordLiteralSyntax Record(params RecordEntrySyntax[] entries)
        => new(new SourceSpan(0, 0), string.Empty, entries);

    public static RecordFieldSyntax Field(string name, ExpressionSyntax? value, bool spread = false)
        => new(
            new SourceSpan(0, 0),
            string.Empty,
            new RecordFieldNameSyntax(Span(name), name, name, false, null),
            value,
            spread);

    public static RecordAccessSyntax RecordAccess(string name, bool original = true)
    {
        var text = $"{(original ? "^" : string.Empty)}.{name}";
        return ExpressionParser.Parse(text) is ClosedExpressionSyntax { Value: RecordAccessSyntax access }
            ? access
            : throw new InvalidOperationException($"'{text}' did not parse as record access.");
    }

    public static TupleProjectionSyntax TupleProjection(int index)
        => new(new SourceSpan(0, 0), string.Empty, TupleProjectionDirection.FromStart, index);

    public static BinaryExpressionSyntax Binary(ExpressionSyntax left, string @operator, ExpressionSyntax right)
        => new(
            new SourceSpan(0, 0),
            string.Empty,
            left,
            new BinaryOperatorSyntax(Span(@operator), @operator),
            right);

    public static UnaryExpressionSyntax Unary(string @operator, ExpressionSyntax operand)
        => new(
            new SourceSpan(0, 0),
            string.Empty,
            new UnaryOperatorSyntax(Span(@operator), @operator),
            operand);

    public static IntervalLiteralSyntax Interval(
        IntervalBound lower,
        IntervalBound upper,
        bool lowerInclusive,
        bool upperInclusive)
        => new(new SourceSpan(0, 0), string.Empty, lower, upper, lowerInclusive, upperInclusive);
}
