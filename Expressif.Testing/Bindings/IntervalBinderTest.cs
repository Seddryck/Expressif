using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class IntervalBinderTest
{
    private static ExpressifBinder Binder { get; } = new();

    [Test]
    public void Bind_FiniteInterval_PreservesBoundsAndInclusivity()
    {
        var syntax = SyntaxFactory.Interval(
            new IntervalBound(IntervalBoundKind.Finite, SyntaxFactory.Number(25)),
            new IntervalBound(IntervalBoundKind.Finite, SyntaxFactory.Number(40)),
            true,
            false);

        var interval = BindInterval(syntax);

        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo('['));
            Assert.That(interval.UpperBoundType, Is.EqualTo('['));
            Assert.That(interval.LowerBound.Value, Is.EqualTo(25m));
            Assert.That(interval.UpperBound.Value, Is.EqualTo(40m));
        });
    }

    [Test]
    public void Bind_InfiniteInterval_PreservesBoundKinds()
    {
        var syntax = SyntaxFactory.Interval(
            new IntervalBound(IntervalBoundKind.NegativeInfinity, null),
            new IntervalBound(IntervalBoundKind.PositiveInfinity, null),
            true,
            true);

        var interval = BindInterval(syntax);

        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBound.Kind, Is.EqualTo(IntervalBoundBindingKind.NegativeInfinity));
            Assert.That(interval.UpperBound.Kind, Is.EqualTo(IntervalBoundBindingKind.PositiveInfinity));
        });
    }

    private static IntervalBinding BindInterval(IntervalLiteralSyntax interval)
        => ((IntervalParameter)Binder.BindParameter(SyntaxFactory.Closed(interval))).Value;
}
