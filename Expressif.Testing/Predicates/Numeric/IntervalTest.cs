using Expressif.Predicates.Numeric;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Numeric;

public class IntervalTest
{
    [Test]
    [TestCase(10, true)]
    [TestCase(1, false)]
    [TestCase(12, true)]
    [TestCase(null, false)]
    public void WithinInterval_Numeric_Success(object? value, bool expected)
        => Assert.That(new WithinInterval(() => new Interval<decimal>(1,12,IntervalType.Open,IntervalType.Closed)).Evaluate(value), Is.EqualTo(expected));
}
