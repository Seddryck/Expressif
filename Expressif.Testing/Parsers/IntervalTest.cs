using Expressif.Parsers;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Parsers
{
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
            var interval = Interval.Parser.End().Parse(value);
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
            var interval = Interval.Parser.End().Parse(value);
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
}
