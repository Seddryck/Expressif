using System.Collections;
using System.Text.Json;

namespace Expressif.Testing.Documentation;

public class DocumentationExamplesTest
{
    private const char Separator = '→';

    public static IEnumerable FunctionExamples => LoadExamples("function");

    public static IEnumerable PredicateExamples => LoadExamples("predicate");

    [TestCaseSource(nameof(FunctionExamples))]
    [Category("documentation")]
    public void FunctionExample_ReturnsDocumentedResult(string example)
        => AssertExample(example);

    [TestCaseSource(nameof(PredicateExamples))]
    [Category("documentation")]
    public void PredicateExample_ReturnsDocumentedResult(string example)
        => AssertExample(example);

    private static IEnumerable<TestCaseData> LoadExamples(string kind)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Documentation", $"{kind}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        foreach (var definition in document.RootElement.EnumerateArray())
        {
            if (!definition.TryGetProperty("Examples", out var examples))
                continue;

            var name = definition.GetProperty("Name").GetString()!;
            foreach (var example in examples.EnumerateArray())
            {
                var text = example.GetString()!;
                yield return new TestCaseData(text)
                    .SetName($"{kind} {name}: {text}");
            }
        }
    }

    private static void AssertExample(string example)
    {
        var separatorIndex = example.IndexOf(Separator);
        Assert.That(separatorIndex, Is.GreaterThan(0), $"The example must contain a '{Separator}' separator.");
        Assert.That(example.IndexOf(Separator, separatorIndex + 1), Is.EqualTo(-1), $"The example must contain exactly one '{Separator}' separator.");

        var expressionText = example[..separatorIndex].Trim();
        var expectedText = example[(separatorIndex + 1)..].Trim();
        var actual = Expression.CreateClosed(expressionText).Evaluate(null);
        var expected = Expression.CreateClosed(expectedText).Evaluate(null);

        Assert.That(actual, Is.EqualTo(expected));
    }
}
