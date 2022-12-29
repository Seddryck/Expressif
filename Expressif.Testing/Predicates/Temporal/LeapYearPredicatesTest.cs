using Expressif.Predicates.Temporal;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Temporal
{
    public class LeapYearPredicatesTest
    {
        [Test]
        [TestCase(1900, false)]
        [TestCase(1999, false)]
        [TestCase(2000, true)]
        [TestCase(2024, true)]
        public void LeapYear_Year_Valid(int year, bool expected)
            => Assert.That(new LeapYear().Evaluate(year), Is.EqualTo(expected));

        [Test]
        [TestCase("1900-03", false)]
        [TestCase("1999-03", false)]
        [TestCase("2000-03", true)]
        [TestCase("2024-03", true)]
        public void LeapYear_YearMonth_Valid(string yearMonth, bool expected)
            => Assert.That(new LeapYear().Evaluate((YearMonth)yearMonth), Is.EqualTo(expected));

        [Test]
        [TestCase("1900-03-15", false)]
        [TestCase("1999-03-15", false)]
        [TestCase("2000-03-15", true)]
        [TestCase("2024-03-15", true)]
        public void LeapYear_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new LeapYear().Evaluate(dt), Is.EqualTo(expected));

        [Test]
        [TestCase("2000-03-15", true)]
        [TestCase("2000-03", true)]
        [TestCase("2000", true)]
        public void LeapYear_Text_Valid(string text, bool expected)
            => Assert.That(new LeapYear().Evaluate(text), Is.EqualTo(expected));
    }
}
