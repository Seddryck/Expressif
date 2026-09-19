using System.Reflection;
using System.Text.Json;

namespace Expressif.Functions.Catalog;

public sealed class FunctionCatalog
{
    internal const string ResourceName = "Expressif.FunctionCatalog.json";
    private static readonly Lazy<FunctionCatalog> LazyDefault = new(() => Load(typeof(FunctionCatalog).Assembly));
    private readonly FunctionDocumentation[] functions;

    private FunctionCatalog(FunctionDocumentation[] functions)
        => this.functions = functions;

    public static FunctionCatalog Default => LazyDefault.Value;

    public IReadOnlyList<FunctionDocumentation> Functions => functions;

    public FunctionDocumentation? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var exact = functions.Where(x => IsExactMatch(x, name)).ToArray();
        if (exact.Length > 0)
            return SingleCanonicalMatch(exact);

        var insensitive = functions.Where(x => IsInsensitiveMatch(x, name)).ToArray();
        return SingleCanonicalMatch(insensitive);
    }

    public IEnumerable<FunctionDocumentation> ForScope(string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        return functions.Where(x => string.Equals(x.Scope, scope, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<FunctionDocumentation> Suggest(string name, int count = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var maximumDistance = Math.Max(2, name.Length / 3);
        return functions
            .Select(x => new
            {
                Function = x,
                Distance = x.Aliases.Prepend(x.Name).Min(candidate => EditDistance(name, candidate)),
            })
            .Where(x => x.Distance <= maximumDistance)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Function.Name, StringComparer.Ordinal)
            .Take(count)
            .Select(x => x.Function);
    }

    internal static FunctionCatalog Load(Assembly assembly)
    {
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded function catalog '{ResourceName}' was not found in assembly '{assembly.GetName().Name}'.");

        var entries = JsonSerializer.Deserialize<FunctionDocumentation[]>(stream)
            ?? throw new InvalidOperationException("The embedded function catalog could not be deserialized.");

        return new FunctionCatalog(entries.Where(x => x.IsPublic).ToArray());
    }

    private static bool IsExactMatch(FunctionDocumentation function, string name)
        => string.Equals(function.Name, name, StringComparison.Ordinal)
            || function.Aliases.Contains(name, StringComparer.Ordinal);

    private static bool IsInsensitiveMatch(FunctionDocumentation function, string name)
        => string.Equals(function.Name, name, StringComparison.OrdinalIgnoreCase)
            || function.Aliases.Contains(name, StringComparer.OrdinalIgnoreCase);

    private static FunctionDocumentation? SingleCanonicalMatch(FunctionDocumentation[] matches)
    {
        var canonical = matches.DistinctBy(x => x.Name, StringComparer.Ordinal).ToArray();
        return canonical.Length == 1 ? canonical[0] : null;
    }

    private static int EditDistance(string left, string right)
    {
        left = left.ToLowerInvariant();
        right = right.ToLowerInvariant();

        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        for (var i = 1; i <= left.Length; i++)
        {
            var current = new int[right.Length + 1];
            current[0] = i;
            for (var j = 1; j <= right.Length; j++)
            {
                var substitution = previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1);
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), substitution);
            }

            previous = current;
        }

        return previous[right.Length];
    }
}
