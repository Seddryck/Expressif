using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Expressif.Cli.Commands;

internal static class TreeDocumentFormatter
{
    public static bool IsSupported(string output)
        => output.Equals("tree", StringComparison.OrdinalIgnoreCase)
            || output.Equals("json", StringComparison.OrdinalIgnoreCase)
            || output.Equals("yaml", StringComparison.OrdinalIgnoreCase);

    public static string Format(TreeDocument root, string output)
        => output.ToLowerInvariant() switch
        {
            "json" => JsonSerializer.Serialize(ToSerializable(root), new JsonSerializerOptions { WriteIndented = true }),
            "yaml" => FormatYaml(root),
            _ => FormatTree(root)
        };

    private static string FormatTree(TreeDocument root)
    {
        var builder = new StringBuilder();
        AppendTree(builder, root, string.Empty, isLast: true, isRoot: true);
        return builder.ToString().TrimEnd('\r', '\n');
    }

    private static void AppendTree(StringBuilder builder, TreeDocument node, string indent, bool isLast, bool isRoot)
    {
        if (!isRoot)
            builder.Append(indent).Append(isLast ? "└─ " : "├─ ");

        builder.AppendLine(node.Label);
        var childIndent = isRoot ? string.Empty : indent + (isLast ? "   " : "│  ");
        for (var index = 0; index < node.Children.Count; index++)
            AppendTree(builder, node.Children[index], childIndent, index == node.Children.Count - 1, isRoot: false);
    }

    private static string FormatYaml(TreeDocument root)
    {
        var builder = new StringBuilder();
        AppendYaml(builder, root, string.Empty, listItem: false);
        return builder.ToString().TrimEnd();
    }

    private static IReadOnlyDictionary<string, object?> ToSerializable(TreeDocument node)
    {
        var document = node.Properties.ToDictionary(property => property.Key, property => property.Value);
        document["Children"] = node.Children.Select(ToSerializable).ToArray();
        return document;
    }

    private static void AppendYaml(StringBuilder builder, TreeDocument node, string indent, bool listItem)
    {
        var first = node.Properties.First();
        builder.Append(indent).Append(listItem ? "- " : string.Empty)
            .Append(ToCamelCase(first.Key)).Append(": ").AppendLine(FormatYamlValue(first.Value));
        var propertyIndent = indent + (listItem ? "  " : string.Empty);
        foreach (var property in node.Properties.Skip(1))
            AppendYamlProperty(builder, property.Key, property.Value, propertyIndent);

        if (node.Children.Count == 0)
        {
            builder.Append(propertyIndent).AppendLine("children: []");
            return;
        }

        builder.Append(propertyIndent).AppendLine("children:");
        foreach (var child in node.Children)
            AppendYaml(builder, child, propertyIndent + "  ", listItem: true);
    }

    private static void AppendYamlProperty(StringBuilder builder, string key, object? value, string indent)
    {
        builder.Append(indent).Append(ToCamelCase(key)).Append(':');
        if (value is IReadOnlyDictionary<string, object?> properties)
        {
            builder.AppendLine();
            foreach (var property in properties)
                AppendYamlProperty(builder, property.Key, property.Value, indent + "  ");
            return;
        }

        builder.Append(' ').AppendLine(FormatYamlValue(value));
    }

    private static string FormatYamlValue(object? value)
        => value switch
        {
            null => "null",
            bool boolean => boolean ? "true" : "false",
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal
                => Convert.ToString(value, CultureInfo.InvariantCulture)!,
            _ => JsonSerializer.Serialize(value.ToString())
        };

    private static string ToCamelCase(string value)
        => string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}

internal sealed class TreeDocument
{
    public string Label { get; }
    public IReadOnlyDictionary<string, object?> Properties { get; }
    public IReadOnlyList<TreeDocument> Children { get; }

    public TreeDocument(
        string label,
        IReadOnlyDictionary<string, object?> properties,
        IReadOnlyList<TreeDocument> children)
        => (Label, Properties, Children) = (label, properties, children);
}
