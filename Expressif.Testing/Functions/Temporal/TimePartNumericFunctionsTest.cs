using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Temporal;

[TestFixture]
public class TimePartNumericFunctionsTest
{
    [Conformance]
    public void HourOfDay_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new HourOfDay().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void MinuteOfHour_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new MinuteOfHour().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void MinuteOfDay_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new MinuteOfDay().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void SecondOfMinute_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new SecondOfMinute().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void SecondOfHour_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new SecondOfHour().Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void SecondOfDay_DateTime_Valid(DateTime dt, int expected)
    => Assert.That(new SecondOfDay().Evaluate(dt), Is.EqualTo(expected));
}
