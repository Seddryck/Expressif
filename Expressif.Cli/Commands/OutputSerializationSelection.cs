using Expressif.Serialization;

namespace Expressif.Cli.Commands;

internal static class OutputSerializationSelection
{
    public static bool TryResolve(
        ValueSerializationFormat? output,
        bool raw,
        out IValueSerializer serializer,
        out string? error)
    {
        if (output.HasValue && raw)
        {
            serializer = null!;
            error = "The --output and --raw options are mutually exclusive.";
            return false;
        }

        serializer = ValueSerializers.Resolve(raw ? ValueSerializationFormat.Raw : output ?? ValueSerializationFormat.Raw);
        error = null;
        return true;
    }
}
