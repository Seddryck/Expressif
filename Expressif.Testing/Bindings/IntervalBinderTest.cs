using Expressif.Bindings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Bindings;

public class IntervalBinderTest
{
    [Test]
    [TestCase("I[25, 40]", '[', "25", "40", ']')]
    [TestCase("I(25, 40]", ']', "25", "40", ']')]
    [TestCase("I(25, 40)", ']', "25", "40", '[')]
    [TestCase("I[25, 40)", '[', "25", "40", '[')]
    [TestCase("I]25, 40]", ']', "25", "40", ']')]
    [TestCase("I]25, 40[", ']', "25", "40", '[')]
    [TestCase("I[25, 40[", '[', "25", "40", '[')]
    [TestCase("I[-25.1221, 40.125]", '[', "-25.1221", "40.125", ']')]
    public void Parse_IntervalDecimal_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = BindingTestAdapter.Interval(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            AssertBound(interval.LowerBound, lowerBound);
            AssertBound(interval.UpperBound, upperBound);
        });
    }

    [Test]
    [TestCase("I[25, +INF]", '[', "25", "+INF", ']')]
    [TestCase("I]25, +INF]", ']', "25", "+INF", ']')]
    [TestCase("I[-INF, 40[", '[', "-INF", "40", '[')]
    public void Parse_IntervalInfinite_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = BindingTestAdapter.Interval(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            AssertBound(interval.LowerBound, lowerBound);
            AssertBound(interval.UpperBound, upperBound);
        });
    }

    [Test]
    [TestCase("I(+)", ']', "0", "+INF", ']')]
    [TestCase("I(-)", '[', "-INF", "0", '[')]
    [TestCase("I(0+)", '[', "0", "+INF", ']')]
    [TestCase("I(0-)", '[', "-INF", "0", ']')]
    public void Parse_IntervalZeroBasedShorthand_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = BindingTestAdapter.Interval(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            AssertBound(interval.LowerBound, lowerBound);
            AssertBound(interval.UpperBound, upperBound);
        });
    }

    [Test]
    [TestCase("I(absolutely-positive)", ']', "0", "+INF", ']')]
    [TestCase("I(absolutely-negative)", '[', "-INF", "0", '[')]
    [TestCase("I(positive)", '[', "0", "+INF", ']')]
    [TestCase("I(negative)", '[', "-INF", "0", ']')]
    public void Parse_IntervalZeroBasedLonghand_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = BindingTestAdapter.Interval(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            AssertBound(interval.LowerBound, lowerBound);
            AssertBound(interval.UpperBound, upperBound);
        });
    }

    [Test]
    [TestCase("I(>40)", ']', "40", "+INF", ']')]
    [TestCase("I(<40)", '[', "-INF", "40", '[')]
    [TestCase("I(>=40)", '[', "40", "+INF", ']')]
    [TestCase("I(<=40)", '[', "-INF", "40", ']')]
    public void Parse_IntervalNonZeroBasedShorthand_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = BindingTestAdapter.Interval(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            AssertBound(interval.LowerBound, lowerBound);
            AssertBound(interval.UpperBound, upperBound);
        });
    }

    [Test]
    [TestCase("I[#\"2022-10-01\", #\"2022-12-01\"]", '[', "2022-10-01", "2022-12-01", ']')]
    [TestCase("I]#\"2022-10-01\", #\"2022-12-01\"]", ']', "2022-10-01", "2022-12-01", ']')]
    [TestCase("I]#\"2022-10-01\", #\"2022-12-01\"[", ']', "2022-10-01", "2022-12-01", '[')]
    [TestCase("I[#\"2022-10-01\", #\"2022-12-01\"[", '[', "2022-10-01", "2022-12-01", '[')]
    [TestCase("I[#\"2022-10-01T16:45:12\", #\"2022-12-17T12:24:20\"]", '[', "2022-10-01 16:45:12", "2022-12-17 12:24:20", ']')]
    public void Parse_IntervalDateTime_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = BindingTestAdapter.Interval(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            AssertBound(interval.LowerBound, lowerBound);
            AssertBound(interval.UpperBound, upperBound);
        });
    }

    private static void AssertBound(IntervalBoundBinding actual, string expected)
    {
        var expectedKind = expected switch
        {
            "-INF" => IntervalBoundBindingKind.NegativeInfinity,
            "+INF" => IntervalBoundBindingKind.PositiveInfinity,
            _ => IntervalBoundBindingKind.Finite,
        };
        Assert.That(actual.Kind, Is.EqualTo(expectedKind));

        if (expectedKind is not IntervalBoundBindingKind.Finite)
        {
            Assert.That(actual.Value, Is.Null);
            return;
        }

        object expectedValue = expected.Contains(':', StringComparison.Ordinal)
            ? (object)DateTime.Parse(expected, CultureInfo.InvariantCulture)
            : DateOnly.TryParse(expected, CultureInfo.InvariantCulture, out var date)
                ? (object)date
                : (object)decimal.Parse(expected, CultureInfo.InvariantCulture);
        Assert.That(actual.Value, Is.EqualTo(expectedValue));
    }
}
