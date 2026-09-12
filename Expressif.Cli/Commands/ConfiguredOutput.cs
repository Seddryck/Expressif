using Expressif.Cli.Configuration;
using Expressif.Serialization;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class ConfiguredOutput
{
    public static bool TryResolve(CliConfiguration configuration, string command, ValueFormat? requested,
        bool pretty, bool compact, string? indent, out ValueFormat style, out string indentation, out string? error)
        => TryResolve(configuration, command, null, false, requested, pretty, compact, indent,
            out _, out style, out indentation, out error);

    public static bool TryResolve(CliConfiguration configuration, string command,
        ValueSerializationFormat? output, bool raw, ValueFormat? requested,
        bool pretty, bool compact, string? indent, out IValueSerializer serializer,
        out ValueFormat style, out string indentation, out string? error)
    {
        indentation = "  ";
        if (!OutputSerializationSelection.TryResolve(output, raw, out serializer, out error))
        {
            style = default;
            return false;
        }

        return TryResolveStyle(configuration, command, requested, pretty, compact, indent,
            out style, out indentation, out error);
    }

    private static bool TryResolveStyle(CliConfiguration configuration, string command, ValueFormat? requested,
        bool pretty, bool compact, string? indent, out ValueFormat style, out string indentation, out string? error)
    {
        indentation = "  ";
        if (!OutputStyleSelection.TryResolve(requested, pretty, compact, out style, out error))
            return false;
        try
        {
            if (requested is null && !pretty && !compact)
                style = configuration.Get(command + ".output-style") == "pretty" ? ValueFormat.Pretty : ValueFormat.Compact;
            var storedIndent = configuration.Get(command + ".indent");
            if (indent is not null)
                return OutputIndentation.TryResolve(indent, style, out indentation, out error);
            return OutputIndentation.TryResolve(storedIndent, ValueFormat.Pretty, out indentation, out error);
        }
        catch (Exception exception) when (exception is FormatException or IOException or UnauthorizedAccessException)
        {
            error = $"Cannot read configuration '{configuration.Path}': {exception.Message}";
            return false;
        }
    }
}
