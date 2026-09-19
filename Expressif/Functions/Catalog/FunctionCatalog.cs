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

        ValidateOmissions(entries);
        ValidateSemantics(entries);

        return new FunctionCatalog(entries.Where(x => x.IsPublic).ToArray());
    }

    internal static void ValidateOmissions(IEnumerable<FunctionDocumentation> entries)
    {
        foreach (var function in entries)
        {
            foreach (var parameter in function.Parameters)
            {
                var member = $"Function '{function.Name}' parameter '{parameter.Name}'";
                if (parameter.Optional && parameter.Omission is null)
                    throw new InvalidOperationException($"{member} is optional and must declare omission behavior.");
                if (!parameter.Optional && parameter.Omission is not null)
                    throw new InvalidOperationException($"{member} is required and cannot declare omission behavior.");
                if (parameter.Omission is null)
                    continue;

                var hasValue = parameter.Omission.Value.ValueKind != JsonValueKind.Undefined;
                var hasSource = !string.IsNullOrWhiteSpace(parameter.Omission.Source);
                switch (parameter.Omission.Mode)
                {
                    case ParameterOmissionMode.Constant when !hasValue || hasSource:
                        throw new InvalidOperationException($"{member} must declare exactly one constant omission value.");
                    case ParameterOmissionMode.EmptyVariadic when !parameter.Variadic || hasValue || hasSource:
                        throw new InvalidOperationException($"{member} can use empty-variadic omission only for a variadic parameter.");
                    case ParameterOmissionMode.Absent when hasValue || hasSource:
                        throw new InvalidOperationException($"{member} cannot attach a value or source to absent omission.");
                    case ParameterOmissionMode.EnvironmentDerived when hasValue || !hasSource:
                        throw new InvalidOperationException($"{member} must name the source of environment-derived omission.");
                }
            }
        }
    }

    internal static void ValidateSemantics(IEnumerable<FunctionDocumentation> entries)
    {
        string[] cardinalities = ["preserved", "non-increasing", "collapsed", "expanded", "partitioned", "unknown"];
        string[] dependencies = ["per-element", "prefix", "whole-input", "partition", "unknown"];
        string[] orderings = ["preserved", "reordered", "unordered", "not-applicable", "unknown"];

        foreach (var function in entries.Where(entry => entry.Semantics is not null))
        {
            var semantics = function.Semantics!;
            ValidateSemanticsChoice(function.Name, "cardinality", semantics.Cardinality, cardinalities);
            ValidateSemanticsChoice(function.Name, "dependency", semantics.Dependency, dependencies);
            ValidateSemanticsChoice(function.Name, "ordering", semantics.Ordering, orderings);
        }
    }

    private static void ValidateSemanticsChoice(
        string function,
        string dimension,
        string value,
        IEnumerable<string> choices)
    {
        if (choices.Contains(value, StringComparer.Ordinal))
            return;

        throw new InvalidOperationException(
            $"Function '{function}' has unsupported semantics {dimension} '{value}'.");
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
