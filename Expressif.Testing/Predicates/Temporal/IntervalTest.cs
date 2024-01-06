using Expressif.Predicates.Temporal;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Temporal;

public class IntervalTest
{
    [Test]
    [TestCase("2022-11-21", true)]
    [TestCase("2022-11-25", false)]
    [TestCase("2022-11-21 17:12:25", true)]
    [TestCase("2022-11-25 17:12:25", false)]
    [TestCase(null, false)]
    [TestCase("(null)", false)]
    public void ContainedIn_DateTime_Expected(object? value, bool expected)
        => Assert.That(new ContainedIn(
            () => new Interval<DateTime>(
                new DateTime(2022, 11, 20)
                , new DateTime(2022, 11, 24)
                , IntervalType.Open
                , IntervalType.Closed)
            ).Evaluate(value), Is.EqualTo(expected));
}
