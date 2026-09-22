using System.Text.Json;
using Expressif.Discovery;
using Expressif.Introspection;

namespace Expressif.Testing.Documentation;

[TestFixture]
[Category("MetadataConsistency")]
public class MetadataCompletenessTest
{
    [Test]
    public void Functions_PublicRuntimeSurfaceMatchesGeneratedCatalog()
        => AssertComplete(
            "function",
            ExpressifIntrospection.Functions.Describe()
                .Where(x => x.IsPublic)
                .Select(x => new OperatorMetadata(x.Name, x.Aliases.ToArray())));

    [Test]
    public void Predicates_PublicRuntimeSurfaceMatchesGeneratedCatalog()
        => AssertComplete(
            "predicate",
            ExpressifIntrospection.Predicates.Describe()
                .Where(x => x.IsPublic)
                .Select(x => new OperatorMetadata(x.Name, x.Aliases.ToArray())));

    [TestCase("function")]
    [TestCase("predicate")]
    [TestCase("accumulator")]
    public void Catalog_ParameterOmissions_AreConsistent(string kind)
    {
        var failures = new List<string>();
        foreach (var (member, parameter) in LoadParameters(kind))
        {
            var description = $"{kind} '{member}' parameter '{parameter.GetProperty("Name").GetString()}'";
            var optional = parameter.GetProperty("Optional").GetBoolean();
            var hasOmission = parameter.TryGetProperty("Omission", out var omission);
            if (optional != hasOmission)
            {
                failures.Add(optional
                    ? $"{description} is optional but has no omission contract."
                    : $"{description} is required but declares an omission contract.");
                continue;
            }

            if (parameter.TryGetProperty("Default", out _))
                failures.Add($"{description} uses the legacy Default property.");
            if (!hasOmission)
                continue;

            var mode = omission.GetProperty("Mode").GetString();
            var hasValue = omission.TryGetProperty("Value", out _);
            var hasSource = omission.TryGetProperty("Source", out var source)
                && !string.IsNullOrWhiteSpace(source.GetString());
            switch (mode)
            {
                case "constant" when !hasValue || hasSource:
                    failures.Add($"{description} must declare exactly one constant omission value.");
                    break;
                case "empty-variadic" when hasValue || hasSource
                    || !parameter.TryGetProperty("Variadic", out var variadic) || !variadic.GetBoolean():
                    failures.Add($"{description} can use empty-variadic omission only without a value or source on a variadic parameter.");
                    break;
                case "absent" when hasValue || hasSource:
                    failures.Add($"{description} cannot attach a value or source to absent omission.");
                    break;
                case "environment-derived" when hasValue || !hasSource:
                    failures.Add($"{description} must name the source of environment-derived omission.");
                    break;
                case not ("constant" or "empty-variadic" or "absent" or "environment-derived"):
                    failures.Add($"{description} uses unsupported omission mode '{mode}'.");
                    break;
            }
        }

        Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
    }

    private static void AssertComplete(string kind, IEnumerable<OperatorMetadata> runtimeOperators)
    {
        var runtime = runtimeOperators.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var documented = LoadCatalog(kind).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var missing = runtime.Keys.Except(documented.Keys, StringComparer.OrdinalIgnoreCase).Order().ToArray();
        var unexpected = documented.Keys.Except(runtime.Keys, StringComparer.OrdinalIgnoreCase).Order().ToArray();
        var aliasMismatches = runtime.Keys.Intersect(documented.Keys, StringComparer.OrdinalIgnoreCase)
            .Where(name => !runtime[name].Aliases.ToHashSet(StringComparer.OrdinalIgnoreCase)
                .SetEquals(documented[name].Aliases))
            .Order()
            .Select(name => $"{name} (runtime: {Format(runtime[name].Aliases)}; JSON: {Format(documented[name].Aliases)})")
            .ToArray();

        var failures = new List<string>();
        if (missing.Length > 0)
            failures.Add($"Public runtime {kind}s missing from JSON: {string.Join(", ", missing)}.");
        if (unexpected.Length > 0)
            failures.Add($"JSON {kind}s without a public runtime operator: {string.Join(", ", unexpected)}.");
        if (aliasMismatches.Length > 0)
            failures.Add($"Alias mismatches: {string.Join("; ", aliasMismatches)}.");

        Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
    }

    private static OperatorMetadata[] LoadCatalog(string kind)
    {
        var path = GetCatalogPath(kind);
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        return document.RootElement.EnumerateArray()
            .Where(x => HasKind(x, kind))
            .Where(x => !x.TryGetProperty("IsPublic", out var isPublic) || isPublic.GetBoolean())
            .Select(x => new OperatorMetadata(
                x.GetProperty("Name").GetString()!,
                x.GetProperty("Aliases").EnumerateArray().Select(alias => alias.GetString()!).ToArray()))
            .ToArray();
    }

    private static IEnumerable<(string Member, JsonElement Parameter)> LoadParameters(string kind)
    {
        var path = GetCatalogPath(kind);
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        return document.RootElement.EnumerateArray()
            .Where(x => HasKind(x, kind))
            .SelectMany(member => member.GetProperty("Parameters").EnumerateArray()
                .Select(parameter => (member.GetProperty("Name").GetString()!, parameter.Clone())))
            .ToArray();
    }

    private static string GetCatalogPath(string kind)
        => Path.Combine(
            AppContext.BaseDirectory,
            "Documentation",
            kind == "accumulator" ? "function.json" : $"{kind}.json");

    private static bool HasKind(JsonElement member, string kind)
    {
        if (kind != "accumulator")
            return true;

        var isAccumulator = member.TryGetProperty("Kind", out var memberKind)
            && memberKind.GetString() == "accumulator";
        return isAccumulator;
    }

    private static string Format(IEnumerable<string> aliases)
    {
        var values = aliases.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        return values.Length == 0 ? "<none>" : string.Join(", ", values);
    }

    private sealed record OperatorMetadata(string Name, string[] Aliases);
}
