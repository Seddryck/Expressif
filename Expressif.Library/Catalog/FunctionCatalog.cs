using System.Reflection;
using System.Text.Json;

namespace Expressif.Library.Catalog;

public sealed class FunctionCatalog
{
    internal const string ResourceName = "Expressif.FunctionCatalog.json";
    internal const string PredicateResourceName = "Expressif.PredicateCatalog.json";
    private static readonly Lazy<FunctionCatalog> LazyDefault = new(() => Load(typeof(FunctionCatalog).Assembly));
    private readonly FunctionDocumentation[] functions;

    private FunctionCatalog(FunctionDocumentation[] functions)
        => this.functions = functions;

    public static FunctionCatalog Default => LazyDefault.Value;

    public IReadOnlyList<FunctionDocumentation> Functions => functions;

    public FunctionDocumentation? Find(string name)
        => Find(name, functions);

    public FunctionDocumentation? Find(string name, string kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        return Find(name, functions.Where(
            function => function.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)));
    }

    private static FunctionDocumentation? Find(string name, IEnumerable<FunctionDocumentation> candidates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var exact = candidates.Where(x => IsExactMatch(x, name)).ToArray();
        if (exact.Length > 0)
            return SingleCanonicalMatch(exact);

        var insensitive = candidates.Where(x => IsInsensitiveMatch(x, name)).ToArray();
        return SingleCanonicalMatch(insensitive);
    }

    public IEnumerable<FunctionDocumentation> ForScope(string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        return functions.Where(x => string.Equals(x.Scope, scope, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<FunctionDocumentation> Suggest(string name, int count = 3)
        => Suggest(name, functions.Where(function => function.Kind != "accumulator"), count);

    public IEnumerable<FunctionDocumentation> Suggest(string name, string kind, int count = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        return Suggest(
            name,
            functions.Where(function => function.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)),
            count);
    }

    private static IEnumerable<FunctionDocumentation> Suggest(
        string name,
        IEnumerable<FunctionDocumentation> candidates,
        int count)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var maximumDistance = Math.Max(2, name.Length / 3);
        var suggestions = candidates
            .Select(x => new
            {
                Function = x,
                Distance = Names(x).Min(candidate => EditDistance(name, candidate)),
            })
            .Where(x => x.Distance <= maximumDistance)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Function.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Function.Kind, StringComparer.Ordinal)
            .ToArray();
        if (suggestions.Length == 0)
            return [];

        var closestDistance = suggestions[0].Distance;
        return suggestions
            .Where(x => x.Distance == closestDistance)
            .Take(count)
            .Select(x => x.Function);
    }

    internal static FunctionCatalog Load(Assembly assembly)
    {
        var entries = Merge(
            LoadEntries(assembly, ResourceName, "function"),
            LoadEntries(assembly, PredicateResourceName, "predicate"));

        // Functions and accumulators intentionally share some callable names; they are
        // resolved in distinct runtime contexts. Predicates share the function lookup,
        // so validate each of those domains against predicates independently.
        ValidateNames(entries.Where(entry => entry.Kind != "accumulator"));
        ValidateNames(entries.Where(entry => entry.Kind != "function"));
        ValidateOmissions(entries);
        ValidateSemantics(entries);
        ValidateSchemas(entries);

        return new FunctionCatalog(entries);
    }

    internal static FunctionDocumentation[] Merge(
        IEnumerable<FunctionDocumentation> functions,
        IEnumerable<FunctionDocumentation> predicates)
        => [
            .. functions.Where(entry => entry.IsPublic),
            .. predicates.Where(entry => entry.IsPublic).Select(entry => entry with
            {
                Kind = "predicate",
            }),
        ];

    internal static void ValidateNames(IEnumerable<FunctionDocumentation> entries)
    {
        var collisions = entries
            .SelectMany(entry => Names(entry)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => (Name: name, Entry: entry)))
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group
                .GroupBy(item => item.Entry.Kind, StringComparer.OrdinalIgnoreCase)
                .Select(kind => new
                {
                    group.Key,
                    Entries = kind.Select(item => item.Entry)
                        .Distinct()
                        .ToArray(),
                }))
            .Where(collision => collision.Entries.Length > 1)
            .OrderBy(collision => collision.Key, StringComparer.Ordinal)
            .ToArray();
        if (collisions.Length == 0)
            return;

        var collision = collisions[0];
        var members = string.Join(", ", collision.Entries
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .Select(entry => $"{entry.Kind} '{entry.Name}'"));
        throw new InvalidOperationException($"Catalog name '{collision.Key}' is ambiguous between {members}.");
    }

    internal static void ValidateOmissions(IEnumerable<FunctionDocumentation> entries)
    {
        foreach (var function in entries)
        {
            foreach (var parameter in function.Parameters)
            {
                var member = $"Function '{function.Name}' parameter '{parameter.Name}'";
                if (parameter.AllowsSpread && !parameter.Variadic)
                    throw new InvalidOperationException($"{member} allows spread but is not variadic.");
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

    internal static void ValidateSchemas(IEnumerable<FunctionDocumentation> entries)
    {
        foreach (var function in entries)
        {
            var schema = function.Schema
                ?? throw new InvalidOperationException(
                    $"Function '{function.Name}' must declare a schema classification.");
            var member = $"Function '{function.Name}' schema";
            if (schema.Classification is not ("fixed" or "contract" or "intrinsic" or "dynamic"))
            {
                throw new InvalidOperationException(
                    $"{member} has unsupported classification '{schema.Classification}'.");
            }
            if (schema.Classification == "fixed")
            {
                EnsureNoSchemaDetails(schema, member);
                if (schema.DynamicReason is not null)
                    throw new InvalidOperationException($"{member} can declare a dynamic reason only when classified as dynamic.");
                if (string.IsNullOrWhiteSpace(function.Input) || string.IsNullOrWhiteSpace(function.Output))
                {
                    throw new InvalidOperationException(
                        $"{member} must declare canonical input and output types on the function.");
                }
                continue;
            }
            if (schema.Classification == "dynamic")
            {
                EnsureNoSchemaDetails(schema, member);
                if (string.IsNullOrWhiteSpace(schema.DynamicReason))
                    throw new InvalidOperationException($"{member} must explain why its result is dynamic.");
                continue;
            }
            if (schema.DynamicReason is not null)
                throw new InvalidOperationException($"{member} can declare a dynamic reason only when classified as dynamic.");
            if (schema.Classification == "intrinsic")
            {
                if (string.IsNullOrWhiteSpace(schema.Intrinsic))
                    throw new InvalidOperationException($"{member} must name its intrinsic.");
                if (schema.Input is not null || schema.Output is not null
                    || schema.Parameters is not null || schema.Nullability is not null
                    || schema.NullableWhen is not null)
                {
                    throw new InvalidOperationException(
                        $"{member} cannot combine an intrinsic with a declarative contract.");
                }
                continue;
            }
            if (string.IsNullOrWhiteSpace(schema.Input) || string.IsNullOrWhiteSpace(schema.Output))
                throw new InvalidOperationException($"{member} must declare both input and output expressions.");
            if (schema.Intrinsic is not null)
                throw new InvalidOperationException($"{member} cannot combine an intrinsic with a declarative contract.");
            if (schema.Nullability is not null && schema.Nullability != "propagate-input")
                throw new InvalidOperationException($"{member} has unsupported nullability policy '{schema.Nullability}'.");
            if (schema.Nullability is not null && schema.NullableWhen is not null)
                throw new InvalidOperationException($"{member} cannot combine legacy and conditional nullability policies.");
            var parameterNames = function.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
            var invalidNullableSource = schema.NullableWhen?.FirstOrDefault(
                source => source != "input" && !parameterNames.Contains(source));
            if (invalidNullableSource is not null)
                throw new InvalidOperationException($"{member} references unknown nullable source '{invalidNullableSource}'.");
            var unknown = schema.Parameters?.Keys.FirstOrDefault(parameter => !parameterNames.Contains(parameter));
            if (unknown is not null)
            {
                throw new InvalidOperationException($"{member} references unknown parameter '{unknown}'.");
            }
            var invalidCombination = schema.Parameters?.FirstOrDefault(parameter =>
                parameter.Value.Combine is not null
                && (parameter.Value.Output is null || parameter.Value.Combine is not ("union" or "tuple")));
            if (invalidCombination is { Value.Combine: not null })
            {
                throw new InvalidOperationException(
                    $"{member} parameter '{invalidCombination.Value.Key}' has unsupported combination "
                    + $"'{invalidCombination.Value.Value.Combine}'.");
            }
        }
    }

    private static void EnsureNoSchemaDetails(FunctionSchemaDocumentation schema, string member)
    {
        if (schema.Input is not null || schema.Output is not null || schema.Parameters is not null
            || schema.Intrinsic is not null || schema.Nullability is not null || schema.NullableWhen is not null)
        {
            throw new InvalidOperationException(
                $"{member} classification '{schema.Classification}' cannot declare schema transfer details.");
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
        => Names(function).Contains(name, StringComparer.Ordinal);

    private static bool IsInsensitiveMatch(FunctionDocumentation function, string name)
        => Names(function).Contains(name, StringComparer.OrdinalIgnoreCase);

    private static IEnumerable<string> Names(FunctionDocumentation function)
        => function.Aliases
            .Prepend(function.Name)
            .Concat(function.DeprecatedAliases?.Select(alias => alias.Name) ?? []);

    private static FunctionDocumentation[] LoadEntries(Assembly assembly, string resourceName, string kind)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded {kind} catalog '{resourceName}' was not found in assembly '{assembly.GetName().Name}'.");

        return JsonSerializer.Deserialize<FunctionDocumentation[]>(stream)
            ?? throw new InvalidOperationException($"The embedded {kind} catalog could not be deserialized.");
    }

    private static FunctionDocumentation? SingleCanonicalMatch(FunctionDocumentation[] matches)
    {
        var canonical = matches
            .DistinctBy(x => $"{x.Kind}\0{x.Name}", StringComparer.Ordinal)
            .ToArray();
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
