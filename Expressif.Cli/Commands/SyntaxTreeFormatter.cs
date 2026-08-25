using System.Globalization;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class SyntaxTreeFormatter
{
    public static bool IsSupported(string output)
        => TreeDocumentFormatter.IsSupported(output);

    public static string Format(SyntaxNode root, string output)
        => TreeDocumentFormatter.Format(ToDocument(root), output);

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

    private static TreeDocument ToDocument(SyntaxNode node)
        => new(
            GetLabel(node),
            new Dictionary<string, object?>
            {
                ["Kind"] = node.Kind.ToString(),
                ["Text"] = node.Text,
                ["Span"] = new Dictionary<string, object?>
                {
                    ["Start"] = node.Span.Start,
                    ["Length"] = node.Span.Length
                }
            },
            node.Children.Select(ToDocument).ToArray());
}
