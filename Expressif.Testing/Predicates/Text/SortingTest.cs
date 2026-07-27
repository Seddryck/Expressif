using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class SortingTest
{
    [Conformance]
    public void EquivalentTo_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new EquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void SortedAfter_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedAfter(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void SortedAfterOrEquivalentTo_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedAfterOrEquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void SortedBefore_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedBefore(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void SortedBeforeOrEquivalentTo_Valid(object? value, string? reference, bool expected)
    {
        var predicate = new SortedBeforeOrEquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }
}
