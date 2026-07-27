using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class SubstringTest
{
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
