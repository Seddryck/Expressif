using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class CliValueFormatting
{
    private static readonly IReadOnlyDictionary<string, Type[]> RuntimeTypes =
        new Dictionary<string, Type[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["array"] = [typeof(object[])],
            ["tuple"] = [typeof(TupleValue), typeof(Expressif.Values.Tuple)],
            ["vector"] = [typeof(VectorValue), typeof(Expressif.Values.Vector)],
            ["pair"] = [typeof(PairValue), typeof(Expressif.Values.Pair)],
            ["group"] = [typeof(Group)],
            ["record"] = [typeof(RecordValue)],
            ["dictionary"] = [typeof(DictionaryValue), typeof(Expressif.Values.Dictionary)],
            ["grouping"] = [typeof(Grouping)],
        };

    public static ValueFormattingOptions Create(
        ValueFormat format,
        string indentation,
        int preferredLineWidth = 80,
        string inlineTypes = "tuple")
        => new()
        {
            Format = format,
            Indentation = indentation,
            PreferredLineWidth = preferredLineWidth,
            InlineValueTypes = inlineTypes == "none"
                ? new HashSet<Type>()
                : inlineTypes.Split(',').SelectMany(type => RuntimeTypes[type]).ToHashSet(),
        };
}
