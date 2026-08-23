using System.Globalization;
using System.Text;
using System.Text.Json;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class SyntaxTreeFormatter
{
    public static bool IsSupported(string output)
        => output.Equals("tree", StringComparison.OrdinalIgnoreCase)
            || output.Equals("json", StringComparison.OrdinalIgnoreCase)
            || output.Equals("yaml", StringComparison.OrdinalIgnoreCase);

    public static string Format(SyntaxNode root, string output)
        => output.ToLowerInvariant() switch
        {
            "json" => FormatJson(root),
            "yaml" => FormatYaml(root),
            _ => FormatTree(root)
        };

    private static string FormatTree(SyntaxNode root)
    {
        var builder = new StringBuilder();
        AppendTree(builder, root, string.Empty, isLast: true, isRoot: true);
        return builder.ToString().TrimEnd('\r', '\n');
    }

    private static void AppendTree(StringBuilder builder, SyntaxNode node, string indent, bool isLast, bool isRoot)
    {
        if (!isRoot)
            builder.Append(indent).Append(isLast ? "└─ " : "├─ ");

        builder.AppendLine(GetLabel(node));
        var childIndent = isRoot ? string.Empty : indent + (isLast ? "   " : "│  ");
        for (var index = 0; index < node.Children.Count; index++)
            AppendTree(builder, node.Children[index], childIndent, index == node.Children.Count - 1, isRoot: false);
    }

    private static string GetLabel(SyntaxNode node)
    {
        var value = node switch
        {
            FunctionCallSyntax function => function.Name,
            NumericLiteralSyntax numeric => numeric.Value.ToString(CultureInfo.InvariantCulture),
            BooleanLiteralSyntax boolean => boolean.Value.ToString().ToLowerInvariant(),
            NullLiteralSyntax => "null",
            QuotedLiteralSyntax quoted => quoted.Value,
            VariableSyntax variable => variable.Name,
            ArgumentNameSyntax argument => argument.Value,
            RecordFieldNameSyntax field => field.Value,
            _ => null
        };

        return value is null ? node.Kind.ToString() : $"{node.Kind}: {value}";
    }

    private static string FormatJson(SyntaxNode root)
        => JsonSerializer.Serialize(ToDocument(root), new JsonSerializerOptions { WriteIndented = true });

    private static string FormatYaml(SyntaxNode root)
    {
        var builder = new StringBuilder();
        AppendYaml(builder, ToDocument(root), string.Empty, listItem: false);
        return builder.ToString().TrimEnd();
    }

    private static SyntaxDocument ToDocument(SyntaxNode node)
        => new(
            node.Kind.ToString(),
            node.Text,
            new SpanDocument(node.Span.Start, node.Span.Length),
            node.Children.Select(ToDocument).ToArray());

    private static void AppendYaml(StringBuilder builder, SyntaxDocument node, string indent, bool listItem)
    {
        builder.Append(indent).Append(listItem ? "- kind: " : "kind: ").AppendLine(Quote(node.Kind));
        var propertyIndent = indent + (listItem ? "  " : string.Empty);
        builder.Append(propertyIndent).Append("text: ").AppendLine(Quote(node.Text));
        builder.Append(propertyIndent).AppendLine("span:");
        builder.Append(propertyIndent).Append("  start: ").AppendLine(node.Span.Start.ToString(CultureInfo.InvariantCulture));
        builder.Append(propertyIndent).Append("  length: ").AppendLine(node.Span.Length.ToString(CultureInfo.InvariantCulture));
        if (node.Children.Count == 0)
        {
            builder.Append(propertyIndent).AppendLine("children: []");
            return;
        }

        builder.Append(propertyIndent).AppendLine("children:");
        foreach (var child in node.Children)
            AppendYaml(builder, child, propertyIndent + "  ", listItem: true);
    }

    private static string Quote(string value)
        => JsonSerializer.Serialize(value);

    private sealed record SyntaxDocument(string Kind, string Text, SpanDocument Span, IReadOnlyList<SyntaxDocument> Children);

    private sealed record SpanDocument(int Start, int Length);
}
