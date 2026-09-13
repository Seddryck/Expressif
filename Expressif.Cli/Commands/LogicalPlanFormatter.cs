using System.Globalization;
using Expressif.Planning;

namespace Expressif.Cli.Commands;

internal static class LogicalPlanFormatter
{
    public static string Format(LogicalPlan plan)
        => TreeDocumentFormatter.Format(ToDocument(plan.Pipeline), "tree");

    private static TreeDocument ToDocument(LogicalValue value) => value switch
    {
        LogicalPipeline pipeline => Node("Pipeline", pipeline.Items.Select(ToDocument)),
        LogicalCall call => Node(
            call.ContextDepth == 0
                ? $"Call: {call.Function.Name}"
                : $"Call: {call.Function.Name} (context depth {call.ContextDepth})",
            call.Arguments.Select(Argument)),
        LogicalLiteral literal => Node($"Literal: {literal.Type} = {Format(literal.Value)}"),
        _ => Node(value.GetType().Name),
    };

    private static TreeDocument Argument(LogicalArgument argument)
    {
        var suffix = argument.IsExplicit
            ? argument.IsSpread ? " (spread)" : string.Empty
            : $" (omitted: {argument.Omission?.Mode.ToString() ?? "unspecified"})";
        return Node(
            $"Argument: {argument.Parameter.Name}{suffix}",
            argument.Value is null ? [] : [ToDocument(argument.Value)]);
    }

    private static string Format(object? value) => value switch
    {
        null => "null",
        string text => $"\"{text}\"",
        bool boolean => boolean.ToString().ToLowerInvariant(),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static TreeDocument Node(string label, IEnumerable<TreeDocument>? children = null)
        => new(label, new Dictionary<string, object?> { ["Kind"] = label }, children?.ToArray() ?? []);
}
