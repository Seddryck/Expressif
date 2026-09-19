using System.Text.Json;

namespace Expressif.Testing.Documentation;

[TestFixture]
[Category("MetadataConsistency")]
public class ParameterOmissionAuditTest
{
    [TestCaseSource(nameof(AuditedContracts))]
    public void OptionalParameter_HasAuditedOmissionContract(OmissionContract expected)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Documentation", $"{expected.Kind}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var member = document.RootElement.EnumerateArray()
            .Single(x => x.GetProperty("Name").GetString() == expected.Member);
        var parameter = member.GetProperty("Parameters").EnumerateArray()
            .Single(x => x.GetProperty("Name").GetString() == expected.Parameter);
        var omission = parameter.GetProperty("Omission");
        var actualValue = omission.TryGetProperty("Value", out var value) ? value.GetRawText() : null;
        var actualSource = omission.TryGetProperty("Source", out var source) ? source.GetString() : null;
        var expectedValue = expected.Mode == "constant" ? JsonSerializer.Serialize(expected.Value) : null;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(omission.GetProperty("Mode").GetString(), Is.EqualTo(expected.Mode));
            Assert.That(actualValue, Is.EqualTo(expectedValue));
            Assert.That(actualSource, Is.EqualTo(expected.Source));
        }
    }

    private static IEnumerable<TestCaseData> AuditedContracts()
    {
        foreach (var contract in Contracts)
        {
            yield return new TestCaseData(contract)
                .SetName($"{contract.Kind}.{contract.Member}.{contract.Parameter}.{contract.Mode}");
        }
    }

    private static readonly OmissionContract[] Contracts =
    [
        new("function", "throw", "predicate", "absent"),
        new("function", "array", "values", "empty-variadic"),
        new("function", "distribute-random-split", "seed", "environment-derived", Source: "the runtime's shared random-number generator"),
        new("function", "add", "times", "constant", 1),
        new("function", "format-currency-prefix", "decimals", "constant", 2),
        new("function", "format-currency-prefix", "separator", "constant", "."),
        new("function", "format-currency-prefix", "grouping", "constant", ","),
        new("function", "format-currency-prefix", "negative", "constant", "-"),
        new("function", "format-currency-suffix", "decimals", "constant", 2),
        new("function", "format-currency-suffix", "separator", "constant", "."),
        new("function", "format-currency-suffix", "grouping", "constant", ","),
        new("function", "format-currency-suffix", "negative", "constant", "-"),
        new("function", "rename-fields", "filter", "absent"),
        new("function", "set-public", "names", "absent"),
        new("function", "set-private", "names", "absent"),
        new("function", "record", "entries", "empty-variadic"),
        new("function", "rotate", "offset", "constant", 1),
        new("function", "dictionary", "values", "empty-variadic"),
        new("function", "grouping", "values", "empty-variadic"),
        new("function", "tuple", "values", "empty-variadic"),
        new("function", "backward", "times", "constant", 1),
        new("function", "forward", "times", "constant", 1),
        new("function", "after-substring", "count", "constant", 0),
        new("function", "before-substring", "count", "constant", 0),
        new("function", "split-lengths", "lengths", "empty-variadic"),
        new("function", "text", "values", "empty-variadic"),
        new("function", "token", "separator", "absent"),
        new("function", "tokenize", "separator", "absent"),
        new("function", "text-to-datetime", "culture", "constant", ""),
        new("function", "generate", "result", "absent"),
        new("predicate", "contains", "comparer", "absent"),
        new("predicate", "ends-with", "comparer", "absent"),
        new("predicate", "is-any-of", "comparer", "absent"),
        new("predicate", "is-equivalent-to", "comparer", "absent"),
        new("predicate", "is-sorted-after", "comparer", "absent"),
        new("predicate", "is-sorted-after-or-equivalent-to", "comparer", "absent"),
        new("predicate", "is-sorted-before", "comparer", "absent"),
        new("predicate", "is-sorted-before-or-equivalent-to", "comparer", "absent"),
        new("predicate", "matches-regex", "comparer", "absent"),
        new("predicate", "starts-with", "comparer", "absent"),
        new("accumulator", "concat", "separator", "constant", ""),
        new("accumulator", "reduce", "initial", "absent"),
    ];

    public sealed record OmissionContract(
        string Kind,
        string Member,
        string Parameter,
        string Mode,
        object? Value = null,
        string? Source = null);
}
