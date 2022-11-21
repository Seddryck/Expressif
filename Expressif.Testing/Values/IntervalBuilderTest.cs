using Expressif.Parsers;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values
{
    public class IntervalBuilderTest
    {
        [Test]
        [TestCase("7", "4", typeof(Interval<decimal>))]
        [TestCase("7.25", "4", typeof(Interval<decimal>))]
        [TestCase("-7.25", "4.1555", typeof(Interval<decimal>))]
        [TestCase("2020-12-16", "2022-12-17", typeof(Interval<DateTime>))]
        [TestCase("2020-12-16 15:12:00", "2020-12-17 03:12:10", typeof(Interval<DateTime>))]
        public void Create_TwoSameValidType_Valid(string lowerBound, string upperBound, Type expected)
            => Assert.That(new IntervalBuilder().Create(']', lowerBound, upperBound, '['), Is.TypeOf(expected));

        [Test]
        [TestCase("foo", "4")]
        [TestCase("7.25", "bar")]
        [TestCase("-7.25", "")]
        [TestCase("2020-12-16", "4.12355")]
        public void Create_MixedValidType_Invalid(string lowerBound, string upperBound)
            => Assert.That(() => new IntervalBuilder().Create(']', lowerBound, upperBound, '['), Throws.ArgumentException);
    }
}
