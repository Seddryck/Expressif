using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text;

public class SortingTest
{
    [TestCase("A", "A", true)]
    [TestCase("A", "B", false)]
    [TestCase("B", "A", false)]
    [TestCase("", "(empty)", true)]
    [TestCase("A", "(empty)", false)]
    [TestCase("(empty)", "", true)]
    [TestCase("(empty)", "A", false)]
    [TestCase("(null)", null, true)]
    [TestCase(null, null, true)]
    [TestCase(null, "(null)", true)]
    public void EquivalentTo_Text_Success(object? value, string? reference, bool expected)
    {
        var predicate = new EquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [TestCase("A", "A", false)]
    [TestCase("A", "B", false)]
    [TestCase("B", "A", true)]
    [TestCase("", "(empty)", false)]
    [TestCase("A", "(empty)", true)]
    [TestCase("(empty)", "", false)]
    [TestCase("(empty)", "A", false)]
    [TestCase("(null)", null, false)]
    [TestCase(null, null, false)]
    [TestCase(null, "(null)", false)]
    public void SortedAfter_Text_Success(object? value, string? reference, bool expected)
    {
        var predicate = new SortedAfter(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [TestCase("A", "A", true)]
    [TestCase("A", "B", false)]
    [TestCase("B", "A", true)]
    [TestCase("", "(empty)", true)]
    [TestCase("A", "(empty)", true)]
    [TestCase("(empty)", "", true)]
    [TestCase("(empty)", "A", false)]
    [TestCase("(null)", null, true)]
    [TestCase(null, null, true)]
    [TestCase(null, "(null)", true)]
    public void SortedAfterOrEquivalentTo_Text_Success(object? value, string? reference, bool expected)
    {
        var predicate = new SortedAfterOrEquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [TestCase("A", "A", false)]
    [TestCase("A", "B", true)]
    [TestCase("B", "A", false)]
    [TestCase("", "(empty)", false)]
    [TestCase("A", "(empty)", false)]
    [TestCase("(empty)", "", false)]
    [TestCase("(empty)", "A", true)]
    [TestCase("(null)", null, false)]
    [TestCase(null, null, false)]
    [TestCase(null, "(null)", false)]
    public void SortedBefore_Text_Success(object? value, string? reference, bool expected)
    {
        var predicate = new SortedBefore(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [TestCase("A", "A", true)]
    [TestCase("A", "B", true)]
    [TestCase("B", "A", false)]
    [TestCase("", "(empty)", true)]
    [TestCase("A", "(empty)", false)]
    [TestCase("(empty)", "", true)]
    [TestCase("(empty)", "A", true)]
    [TestCase("(null)", null, true)]
    [TestCase(null, null, true)]
    [TestCase(null, "(null)", true)]
    public void SortedBeforeOrEquivalentTo_Text_Success(object? value, string? reference, bool expected)
    {
        var predicate = new SortedBeforeOrEquivalentTo(() => reference);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(reference));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }
}
