using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class CliValueFormatting
{
    public static ValueFormattingOptions Create(ValueFormat format, string indentation)
        => new()
        {
            Format = format,
            Indentation = indentation,
            InlineValueTypes = new HashSet<Type>
            {
                typeof(TupleValue),
                typeof(Expressif.Values.Tuple),
            },
        };
}
