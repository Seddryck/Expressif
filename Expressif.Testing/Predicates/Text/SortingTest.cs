using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class SortingTest
{
    [Test]
    public void Reference_EvaluatedOnceIncludingNullChecks(
        [Values("equivalent", "after", "before", "after-or-equivalent", "before-or-equivalent")] string operation,
        [Values("abc", null)] string? value,
        [Values("abc", null)] string? reference)
    {
        var evaluations = 0;
        Func<string?> getReference = () => { evaluations++; return reference; };
        BaseTextPredicateReference predicate = operation switch
        {
            "equivalent" => new EquivalentTo(getReference),
            "after" => new SortedAfter(getReference),
            "before" => new SortedBefore(getReference),
            "after-or-equivalent" => new SortedAfterOrEquivalentTo(getReference),
            _ => new SortedBeforeOrEquivalentTo(getReference),
        };
        var strict = operation is "after" or "before";

        var result = predicate.Evaluate(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(!strict && (reference is null || value is not null)));
            Assert.That(evaluations, Is.EqualTo(strict && value is null ? 0 : 1));
        });
    }

    [Conformance]
    public void IsEquivalentTo_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new EquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void IsSortedAfter_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedAfter(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void IsSortedAfterOrEquivalentTo_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedAfterOrEquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void IsSortedBefore_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedBefore(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void IsSortedBeforeOrEquivalentTo_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedBeforeOrEquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }
}
