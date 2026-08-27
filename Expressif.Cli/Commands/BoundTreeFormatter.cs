using System.Collections;
using System.Globalization;
using Expressif.Bindings;

namespace Expressif.Cli.Commands;

internal static class BoundTreeFormatter
{
    public static string Format(IRootExpression root, string output)
        => TreeDocumentFormatter.Format(ToDocument(root), output);

    private static TreeDocument ToDocument(IRootExpression root)
        => root switch
        {
            OpenRootExpression open => Node("OpenExpression", children: open.Expression.Members.Select(ToDocument)),
            ClosedRootExpression closed => Node(
                "ClosedExpression",
                children: [NamedParameter("Source", closed.Expression.Parameter), .. closed.Expression.Members.Select(ToDocument)]),
            _ => Node(root.GetType().Name)
        };

    private static TreeDocument ToDocument(Function function)
        => Node(
            function.Syntax == FunctionSyntax.Standard
                ? $"Function: {function.Name}"
                : $"Function: {function.Name} (from {function.Syntax})",
            new Dictionary<string, object?>
            {
                ["Kind"] = "Function",
                ["Name"] = function.Name,
                ["Syntax"] = function.Syntax.ToString()
            },
            function.Parameters.Select((parameter, index) => NamedParameter($"Arg[{index}]", parameter)));

    private static TreeDocument ToDocument(IRecordDefinitionEntry entry)
        => entry switch
        {
            RecordNamedEntry named => NamedParameter($"Field: {named.Name}", named.Value),
            RecordSpreadEntry => Node("Spread: IncomingValue"),
            _ => Node(entry.GetType().Name)
        };

    private static TreeDocument NamedParameter(string name, IParameter parameter)
    {
        var value = ScalarValue(parameter);
        var label = value is null
            ? $"{name}: {ParameterKind(parameter)}"
            : $"{name}: {ParameterKind(parameter)} = {FormatValue(value)}";
        return Node(
            label,
            new Dictionary<string, object?>
            {
                ["Kind"] = ParameterKind(parameter),
                ["Name"] = name,
                ["Value"] = value
            },
            ParameterChildren(parameter));
    }

    private static IEnumerable<TreeDocument> ParameterChildren(IParameter parameter)
        => parameter switch
        {
            ArrayParameter array => array.Values.Select((item, index) => NamedParameter($"Item[{index}]", item)),
            TupleParameter tuple => tuple.Values.Select((item, index) => NamedParameter($"Item[{index}]", item)),
            RecordLiteralParameter record => record.Fields.Select(field => NamedParameter($"Field: {field.Name}", field.Value)),
            RecordDefinitionParameter record => record.Entries.Select(ToDocument),
            OpenExpressionParameter open => open.Expression.Members.Select(ToDocument),
            InputExpressionParameter input => [
                NamedParameter("Source", input.Expression.Parameter),
                .. input.Expression.Members.Select(ToDocument)
            ],
            IntervalParameter interval => [
                IntervalBound("LowerBound", interval.Value.LowerBound, interval.Value.IsLowerInclusive),
                IntervalBound("UpperBound", interval.Value.UpperBound, interval.Value.IsUpperInclusive)
            ],
            PredicationParameter predication => PredicationChildren(predication.Predication),
            _ => []
        };

    private static IEnumerable<TreeDocument> PredicationChildren(IPredication predication)
        => predication switch
        {
            SinglePredication single => [ToDocument(single.Member)],
            PipelinePredication pipeline => pipeline.Expression.Members.Select(ToDocument),
            _ => [Node(predication.GetType().Name)]
        };

    private static TreeDocument IntervalBound(string name, IntervalBoundBinding bound, bool inclusive)
        => Node(
            $"{name}: {bound.Kind}{(bound.Value is null ? string.Empty : $" = {FormatValue(bound.Value)}")}",
            new Dictionary<string, object?>
            {
                ["Kind"] = bound.Kind.ToString(),
                ["Name"] = name,
                ["Value"] = bound.Value,
                ["Inclusive"] = inclusive
            });

    private static string ParameterKind(IParameter parameter)
        => parameter.GetType().Name.Replace("Parameter", string.Empty, StringComparison.Ordinal);

    private static object? ScalarValue(IParameter parameter)
        => parameter switch
        {
            LiteralParameter literal => literal.Value,
            QuotedLiteralParameter quoted => quoted.Value,
            VariableParameter variable => variable.Name,
            ObjectPropertyParameter property => property.Name,
            ObjectIndexParameter index => index.Index,
            TupleProjectionParameter projection => projection.FromEnd ? $"^{projection.Index}" : projection.Index,
            _ => null
        };

    private static string FormatValue(object value)
        => value switch
        {
            string text => $"\"{text}\"",
            bool boolean => boolean.ToString().ToLowerInvariant(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            IEnumerable enumerable => string.Join(", ", enumerable.Cast<object?>()),
            _ => value.ToString() ?? string.Empty
        };

    private static TreeDocument Node(
        string label,
        IReadOnlyDictionary<string, object?>? properties = null,
        IEnumerable<TreeDocument>? children = null)
        => new(
            label,
            properties ?? new Dictionary<string, object?> { ["Kind"] = label },
            children?.ToArray() ?? []);
}
