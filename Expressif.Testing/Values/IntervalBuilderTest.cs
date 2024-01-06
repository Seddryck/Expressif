using Expressif.Parsers;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values;

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
        => Assert.That(() => new IntervalBuilder().Create(']', lowerBound, upperBound, '['), Throws.InvalidOperationException);

    [Test]
    [TestCase("[2022-10-12 12:16:10;2023-10-11 00:00:00]")]
    [TestCase("[2022-10-12;2023-10-11]")]
    public void CreateDateTime_FromString_valid(string value)
        => Assert.Multiple(() =>
        {
            Assert.That(() => new IntervalBuilder().Create(value), Is.Not.Null);
            Assert.That(() => new IntervalBuilder().Create(value), Is.TypeOf<Interval<DateTime>>());
        });

    [Test]
    [TestCase("[5;15]")]
    [TestCase("[-5.05;10.256]")]
    public void CreateNumeric_FromString_valid(string value)
        => Assert.Multiple(() =>
        {
            Assert.That(() => new IntervalBuilder().Create(value), Is.Not.Null);
            Assert.That(() => new IntervalBuilder().Create(value), Is.TypeOf<Interval<decimal>>());
        });
}
