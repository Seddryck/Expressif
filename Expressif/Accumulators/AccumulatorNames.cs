using Expressif.Accumulators.Introspection;

namespace Expressif.Accumulators;

internal static class AccumulatorNames
{
    private static readonly IReadOnlyDictionary<string, string> CanonicalNames = new AccumulatorIntrospector()
        .Locate().SelectMany(info => info.Aliases.Prepend(info.Name).Distinct()
            .Select(alias => new KeyValuePair<string, string>(alias, info.Name)))
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

    public static string Resolve(string name)
        => CanonicalNames.TryGetValue(name, out var canonical) ? canonical : name;
}
