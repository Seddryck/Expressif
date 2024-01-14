using Expressif.Parsers;
using Expressif.Values;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Parsers;

public class IntervalTest
{
    [Test]
    [TestCase("[25;40]", '[', "25", "40", ']')]
    [TestCase("]25;40]", ']', "25", "40", ']')]
    [TestCase("]25;40[", ']', "25", "40", '[')]
    [TestCase("[25;40[", '[', "25", "40", '[')]
    [TestCase("[-25.1221;40.125]", '[', "-25.1221", "40.125", ']')]
    public void Parse_IntervalDecimal_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = IntervalParser.Parser.End().Parse(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            Assert.That(interval.LowerBound, Is.EqualTo(lowerBound));
            Assert.That(interval.UpperBound, Is.EqualTo(upperBound));
        });
    }

    [Test]
    [TestCase("[25;+INF]", '[', "25", "+INF", ']')]
    [TestCase("[-INF;40[", '[', "-INF", "40", '[')]
    public void Parse_IntervalInfinite_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = IntervalParser.Parser.End().Parse(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            Assert.That(interval.LowerBound, Is.EqualTo(lowerBound));
            Assert.That(interval.UpperBound, Is.EqualTo(upperBound));
        });
    }

    [Test]
    [TestCase("(+)", ']', "0", "+INF", ']')]
    [TestCase("(-)", '[', "-INF", "0", '[')]
    [TestCase("(0+)", '[', "0", "+INF", ']')]
    [TestCase("(0-)", '[', "-INF", "0", ']')]
    public void Parse_IntervalZeroBasedShorthand_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = IntervalParser.Parser.End().Parse(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            Assert.That(interval.LowerBound, Is.EqualTo(lowerBound));
            Assert.That(interval.UpperBound, Is.EqualTo(upperBound));
        });
    }

    [Test]
    [TestCase("(absolutely-positive)", ']', "0", "+INF", ']')]
    [TestCase("(absolutely-negative)", '[', "-INF", "0", '[')]
    [TestCase("(positive)", '[', "0", "+INF", ']')]
    [TestCase("(negative)", '[', "-INF", "0", ']')]
    public void Parse_IntervalZeroBasedLonghand_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = IntervalParser.Parser.End().Parse(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            Assert.That(interval.LowerBound, Is.EqualTo(lowerBound));
            Assert.That(interval.UpperBound, Is.EqualTo(upperBound));
        });
    }

    [Test]
    [TestCase("(>40)", ']', "40", "+INF", ']')]
    [TestCase("(<40)", '[', "-INF", "40", '[')]
    [TestCase("(>=40)", '[', "40", "+INF", ']')]
    [TestCase("(<=40)", '[', "-INF", "40", ']')]
    public void Parse_IntervalNonZeroBasedShorthand_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = IntervalParser.Parser.End().Parse(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            Assert.That(interval.LowerBound, Is.EqualTo(lowerBound));
            Assert.That(interval.UpperBound, Is.EqualTo(upperBound));
        });
    }

    [Test]
    [TestCase("[2022-10-01;2022-12-01]", '[', "2022-10-01", "2022-12-01", ']')]
    [TestCase("]2022-10-01;2022-12-01]", ']', "2022-10-01", "2022-12-01", ']')]
    [TestCase("]2022-10-01;2022-12-01[", ']', "2022-10-01", "2022-12-01", '[')]
    [TestCase("[2022-10-01;2022-12-01[", '[', "2022-10-01", "2022-12-01", '[')]
    [TestCase("[2022-10-01 16:45:12;2022-12-17 12:24:20]", '[', "2022-10-01 16:45:12", "2022-12-17 12:24:20", ']')]
    public void Parse_IntervalDateTime_Valid(string value, char lowerBoundIntervalType, string lowerBound, string upperBound, char upperBoundIntervalType)
    {
        var interval = IntervalParser.Parser.End().Parse(value);
        Assert.That(interval, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(interval.LowerBoundType, Is.EqualTo(lowerBoundIntervalType));
            Assert.That(interval.UpperBoundType, Is.EqualTo(upperBoundIntervalType));
            Assert.That(interval.LowerBound, Is.EqualTo(lowerBound));
            Assert.That(interval.UpperBound, Is.EqualTo(upperBound));
        });
    }
}
