using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class SortingTest
{
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
