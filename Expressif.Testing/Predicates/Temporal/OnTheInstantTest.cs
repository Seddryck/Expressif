using Expressif.Predicates.Temporal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Temporal
{
    public class OnTheInstantTest
    {
        [Test]
        [TestCase("2022-11-21", true)]
        [TestCase("2022-11-21 00:00:00", true)]
        [TestCase("2022-11-21 17:00:00", false)]
        [TestCase("2022-11-21 17:12:00", false)]
        [TestCase("2022-11-21 17:12:25", false)]
        [TestCase(null, false)]
        [TestCase("(null)", false)]
        public void OnTheDay_DateTime_Expected(object value, bool expected)
            => Assert.That(new OnTheDay().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", true)]
        public void OnTheDay_Date_Expected(string value, bool expected)
            => Assert.That(new OnTheDay().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", true)]
        [TestCase("2022-11-21 00:00:00", true)]
        [TestCase("2022-11-21 17:00:00", true)]
        [TestCase("2022-11-21 17:12:00", false)]
        [TestCase("2022-11-21 17:12:25", false)]
        [TestCase(null, false)]
        [TestCase("(null)", false)]
        public void OnTheHour_DateTime_Expected(object value, bool expected)
            => Assert.That(new OnTheHour().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", true)]
        public void OnTheHour_Date_Expected(string value, bool expected)
            => Assert.That(new OnTheHour().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", true)]
        [TestCase("2022-11-21 00:00:00", true)]
        [TestCase("2022-11-21 17:00:00", true)]
        [TestCase("2022-11-21 17:12:00", true)]
        [TestCase("2022-11-21 17:12:25", false)]
        [TestCase(null, false)]
        [TestCase("(null)", false)]
        public void OnTheMinute_DateTime_Expected(object value, bool expected)
            => Assert.That(new OnTheMinute().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-21", true)]
        public void OnTheMinute_Date_Expected(string value, bool expected)
            => Assert.That(new OnTheMinute().Evaluate(DateOnly.Parse(value)), Is.EqualTo(expected));
    }
}
