using Expressif.Parsers;
using Expressif.Values;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers
{
    public class GrammarTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("foo")]
        [TestCase("foo  ")]
        [TestCase("  foo")]
        [TestCase("FoO")]
        [TestCase("foo-bar")]
        [TestCase("foo-BAr-foo")]
        public void Parse_FunctionName_Valid(string value)
            => Assert.That(Grammar.FunctionName.End().Parse(value), Is.EqualTo(value.Trim()));

        [Test]
        [TestCase("1foo")]
        [TestCase("fo1o")]
        [TestCase("foo1")]
        [TestCase("@foo")]
        [TestCase("-foo")]
        [TestCase("foo--bar")]
        [TestCase("foo-")]
        public void Parse_FunctionName_Invalid(string value)
            => Assert.That(() => Grammar.FunctionName.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("|")]
        [TestCase("  |")]
        [TestCase("|  ")]
        public void Parse_Delimitator_Valid(string value)
            => Assert.That(Grammar.Delimitator.End().Parse(value), Is.EqualTo('|'));

        [Test]
        [TestCase("@")]
        [TestCase("foo")]
        public void Parse_Delimitator_Invalid(string value)
            => Assert.That(() => Grammar.Delimitator.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("@a")]
        [TestCase("@foo")]
        [TestCase("@foo1")]
        [TestCase("@fo1o")]
        [TestCase("@Foo  ")]
        [TestCase("  @Foo")]
        public void Parse_Variable_Valid(string value)
            => Assert.That(Grammar.Variable.End().Parse(value), Is.EqualTo(value.Trim().TrimStart('@')));

        [Test]
        [TestCase("@")]
        [TestCase("foo")]
        [TestCase("@foo-1")]
        public void Parse_Variable_Invalid(string value)
            => Assert.That(() => Grammar.Delimitator.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("foo")]
        [TestCase("  foo")]
        [TestCase("foo  ")]
        [TestCase("foo-bar")]
        [TestCase("%foo!bar")]
        [TestCase("\"foo\"")]
        [TestCase("\" foo bar \"")]
        [TestCase("\"foo , bar\"")]
        [TestCase("\"(foo)\"")]
        public void Parse_Literal_Valid(string value)
            => Assert.That(Grammar.Literal.End().Parse(value), Is.EqualTo(value.Trim().Trim('\"')));

        [Test]
        [TestCase("@foo")]
        [TestCase("foo bar")]
        [TestCase("foo , bar")]
        [TestCase("(foo)")]
        public void Parse_Literal_Invalid(string value)
            => Assert.That(() => Grammar.Literal.End().Parse(value), Throws.TypeOf<ParseException>());

        [Test]
        [TestCase("[25;40]", IntervalType.Closed, 25, 40, IntervalType.Closed)]
        [TestCase("]25;40]", IntervalType.Open, 25, 40, IntervalType.Closed)]
        [TestCase("]25;40[", IntervalType.Open, 25, 40, IntervalType.Open)]
        [TestCase("[25;40[", IntervalType.Closed, 25, 40, IntervalType.Open)]
        [TestCase("[-25.1221;40.125]", IntervalType.Closed, -25.1221, 40.125, IntervalType.Closed)]
        public void Parse_IntervalDecimal_Valid(string value, IntervalType lowerBoundIntervalType, decimal lowerBound, decimal upperBound, IntervalType upperBoundIntervalType)
        {
            var interval = Grammar.Interval.End().Parse(value);
            Assert.That(interval, Is.Not.Null);
            Assert.That(interval, Is.TypeOf<Interval<decimal>>());
            Assert.Multiple(() =>
            {
                Assert.That(((Interval<decimal>)interval).LowerBoundIntervalType, Is.EqualTo(lowerBoundIntervalType));
                Assert.That(((Interval<decimal>)interval).UpperBoundIntervalType, Is.EqualTo(upperBoundIntervalType));
                Assert.That(((Interval<decimal>)interval).LowerBound, Is.EqualTo(lowerBound));
                Assert.That(((Interval<decimal>)interval).UpperBound, Is.EqualTo(upperBound));
            });
        }

        [Test]
        [TestCase("[2022-10-01;2022-12-01]", IntervalType.Closed, "2022-10-01", "2022-12-01", IntervalType.Closed)]
        [TestCase("]2022-10-01;2022-12-01]", IntervalType.Open, "2022-10-01", "2022-12-01", IntervalType.Closed)]
        [TestCase("]2022-10-01;2022-12-01[", IntervalType.Open, "2022-10-01", "2022-12-01", IntervalType.Open)]
        [TestCase("[2022-10-01;2022-12-01[", IntervalType.Closed, "2022-10-01", "2022-12-01", IntervalType.Open)]
        [TestCase("[2022-10-01 16:45:12;2022-12-17 12:24:20]", IntervalType.Closed, "2022-10-01 16:45:12", "2022-12-17 12:24:20", IntervalType.Closed)]
        public void Parse_IntervalDateTime_Valid(string value, IntervalType lowerBoundIntervalType, DateTime lowerBound, DateTime upperBound, IntervalType upperBoundIntervalType)
        {
            var interval = Grammar.Interval.End().Parse(value);
            Assert.That(interval, Is.Not.Null);
            Assert.That(interval, Is.TypeOf<Interval<DateTime>>());
            Assert.Multiple(() =>
            {
                Assert.That(((Interval<DateTime>)interval).LowerBoundIntervalType, Is.EqualTo(lowerBoundIntervalType));
                Assert.That(((Interval<DateTime>)interval).UpperBoundIntervalType, Is.EqualTo(upperBoundIntervalType));
                Assert.That(((Interval<DateTime>)interval).LowerBound, Is.EqualTo(lowerBound));
                Assert.That(((Interval<DateTime>)interval).UpperBound, Is.EqualTo(upperBound));
            });
        }
    }
}