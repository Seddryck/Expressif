using System.Text.Json;
using Expressif.Accumulators.Introspection;
using Expressif.Functions.Introspection;
using Expressif.Predicates.Introspection;

namespace Expressif.Testing.Documentation;

[TestFixture]
[Category("MetadataConsistency")]
public class MetadataCompletenessTest
{
    [Test]
    public void Functions_PublicRuntimeSurfaceMatchesGeneratedCatalog()
        => AssertComplete(
            "function",
            new FunctionIntrospector().Describe()
                .Where(x => x.IsPublic)
                .Select(x => new OperatorMetadata(x.Name, x.Aliases)));

    [Test]
    public void Predicates_PublicRuntimeSurfaceMatchesGeneratedCatalog()
        => AssertComplete(
            "predicate",
            new PredicateIntrospector().Describe()
                .Where(x => x.IsPublic)
                .Select(x => new OperatorMetadata(x.Name, x.Aliases)));

    [Test]
    public void Accumulators_PublicRuntimeSurfaceMatchesGeneratedCatalog()
        => AssertComplete(
            "accumulator",
            new AccumulatorIntrospector().Describe()
                .Where(x => x.IsPublic)
                .Select(x => new OperatorMetadata(x.Name, x.Aliases)));

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
        var path = Path.Combine(AppContext.BaseDirectory, "Documentation", $"{kind}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        return document.RootElement.EnumerateArray()
            .Where(x => !x.TryGetProperty("IsPublic", out var isPublic) || isPublic.GetBoolean())
            .Select(x => new OperatorMetadata(
                x.GetProperty("Name").GetString()!,
                x.GetProperty("Aliases").EnumerateArray().Select(alias => alias.GetString()!).ToArray()))
            .ToArray();
    }

    private static string Format(IEnumerable<string> aliases)
    {
        var values = aliases.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        return values.Length == 0 ? "<none>" : string.Join(", ", values);
    }

    private sealed record OperatorMetadata(string Name, string[] Aliases);
}
