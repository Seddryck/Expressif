using System.Text;
using Expressif.Functions.Catalog;

namespace Expressif.Cli.Commands;

internal static class FunctionHelpFormatter
{
    public static string Format(FunctionDocumentation function)
    {
        var builder = new StringBuilder();
        builder.Append(function.Input).AppendLine(" →");
        AppendSignature(builder, function);
        builder.AppendLine();
        builder.AppendLine(function.Summary).AppendLine();

        if (function.Parameters.Length > 0)
        {
            builder.AppendLine("Parameters:");
            var nameWidth = function.Parameters.Max(x => x.Name.Length);
            var typeWidth = function.Parameters.Max(x => x.TypeOrKind.Length);
            foreach (var parameter in function.Parameters)
            {
                builder.Append("  ").Append(parameter.Name.PadRight(nameWidth)).Append("  ")
                    .Append(parameter.TypeOrKind.PadRight(typeWidth));
                if (parameter.Variadic)
                    builder.Append(" (variadic, ").Append(FormatMinimumCardinality(parameter)).Append(" or more)");
                else if (parameter.Optional)
                    builder.Append(" (optional)");

                builder.Append("  ").AppendLine(parameter.Summary);
            }

            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(function.Behavior))
        {
            builder.AppendLine("Behavior:");
            builder.AppendLine(function.Behavior).AppendLine();
        }

        if (function.Examples is { Length: > 0 })
        {
            builder.AppendLine("Examples:");
            foreach (var example in function.Examples)
                builder.Append("  ").AppendLine(example);

            builder.AppendLine();
        }

        builder.Append("Aliases: ").AppendLine(
            function.Aliases.Length == 0 ? "(none)" : string.Join(", ", function.Aliases));
        builder.Append("Scope:   ").AppendLine(function.Scope);

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

    private static void AppendSignature(StringBuilder builder, FunctionDocumentation function)
    {
        if (function.Parameters.Length == 0)
        {
            builder.Append(function.Name).Append("() → ").AppendLine(function.Output);
            return;
        }

        builder.Append(function.Name).AppendLine("(");
        for (var i = 0; i < function.Parameters.Length; i++)
        {
            var parameter = function.Parameters[i];
            builder.Append("    ");
            if (parameter.Variadic)
                builder.Append("...");

            builder.Append(parameter.Name);
            if (parameter.Optional && !parameter.Variadic)
                builder.Append('?');

            builder.Append(": ").Append(parameter.TypeOrKind);
            if (i < function.Parameters.Length - 1)
                builder.Append(',');

            builder.AppendLine();
        }

        builder.Append(") → ").AppendLine(function.Output);
    }

    private static string FormatMinimumCardinality(FunctionParameterDocumentation parameter)
        => parameter.MinimumCardinality switch
        {
            0 => "zero",
            1 => "one",
            2 => "two",
            _ => parameter.MinimumCardinality.ToString(),
        };
}
