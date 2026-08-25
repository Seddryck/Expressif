using System.Text;
using Expressif.Functions.Catalog;

namespace Expressif.Cli.Commands;

internal static class FunctionHelpFormatter
{
    public static string Format(FunctionDocumentation function)
    {
        var builder = new StringBuilder();
        builder.Append(function.Name).Append('(');
        builder.AppendJoin(", ", function.Parameters.Select(FormatParameter));
        builder.Append(") → ").AppendLine(function.Output).AppendLine();
        builder.AppendLine(function.Summary).AppendLine();
        builder.Append("Input:   ").AppendLine(function.Input);
        builder.Append("Scope:   ").AppendLine(function.Scope);

        builder.Append("Aliases: ").AppendLine(
            function.Aliases.Length == 0 ? "(none)" : string.Join(", ", function.Aliases));

        if (function.Parameters.Length > 0)
        {
            builder.AppendLine().AppendLine("Parameters:");
            var nameWidth = function.Parameters.Max(x => x.Name.Length + (x.Optional ? 1 : 0));
            var typeWidth = function.Parameters.Max(x => x.Type.Length);
            foreach (var parameter in function.Parameters)
            {
                var name = parameter.Name + (parameter.Optional ? "?" : string.Empty);
                builder.Append("  ").Append(name.PadRight(nameWidth)).Append("  ")
                    .Append(parameter.Type.PadRight(typeWidth)).Append("  ")
                    .AppendLine(parameter.Summary);
            }
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatList(IEnumerable<FunctionDocumentation> functions)
    {
        var builder = new StringBuilder();
        foreach (var group in functions.OrderBy(x => x.Scope).ThenBy(x => x.Name).GroupBy(x => x.Scope))
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append(group.Key).AppendLine(":");
            foreach (var function in group)
                builder.Append("  ").AppendLine(function.Name);
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatParameter(FunctionParameterDocumentation parameter)
        => $"{parameter.Name}{(parameter.Optional ? "?" : string.Empty)}: {parameter.Type}";
}
