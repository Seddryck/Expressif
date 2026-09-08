using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class SubstringTest
{
    [Test]
    public void Reference_EvaluatedOnceWhenNeeded(
        [Values("contains", "starts-with", "ends-with", "matches-regex")] string operation,
        [Values("abc", null)] string? value,
        [Values("abc", " ", null)] string? reference)
    {
        var evaluations = 0;
        Func<string> getReference = () => { evaluations++; return reference!; };
        BaseTextPredicateReference predicate = operation switch
        {
            "contains" => new Expressif.Predicates.Text.Contains(getReference),
            "starts-with" => new StartsWith(getReference),
            "ends-with" => new EndsWith(getReference),
            _ => new MatchesRegex(getReference),
        };

        for (var call = 1; call <= 2; call++)
        {
            var result = predicate.Evaluate(value);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(value == "abc" && reference == "abc"));
                Assert.That(evaluations, Is.EqualTo(value is null ? 0 : call));
            });
        }
    }

    [Conformance]
    public void StartsWith_Valid_Text(object value, string reference, bool expected)
    {
        var predicate = new StartsWith(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void EndsWith_Valid_Text(object value, string reference, bool expected)
    {
        var predicate = new EndsWith(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void Contains_Valid_Text(object value, string reference, bool expected)
    {
        var predicate = new Expressif.Predicates.Text.Contains(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void MatchesRegex_Valid_IgnoreCase_Text(object? value, string reference, bool expected)
    {
        var predicate = new MatchesRegex(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void MatchesRegex_Valid_CaseSensitive_Text(object value, string reference, bool expected)
    {
        var predicate = new MatchesRegex(() => reference, StringComparer.InvariantCulture);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }
}
