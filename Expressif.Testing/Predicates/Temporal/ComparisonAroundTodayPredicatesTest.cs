using Expressif.Predicates.Temporal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif.Testing.Predicates.Temporal
{
    public class ComparisonAroundTodayPredicatesTest
    {
        public ComparisonAroundTodayPredicatesTest()
        {
            TypeDescriptor.AddAttributes(
                typeof(DateOnly)
                , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
            );
        }

        [Test]
        [TestCase("2022-12-28", false)]
        [TestCase("2022-12-29", false)]
        [TestCase("2022-12-30", true)]
        [TestCase("1970-05-06", false)]
        public void Tomorrow_Date_Valid(string text, bool expected)
            => Assert.That(new Tomorrow(new DateTime(2022, 12, 29))
                    .Evaluate(
                        TypeDescriptor.GetConverter(typeof(DateOnly))
                        .ConvertFromInvariantString(text)!
                    )
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28", false)]
        [TestCase("2022-12-29", false)]
        [TestCase("2022-12-30", true)]
        [TestCase("1970-05-06", false)]
        public void Tomorrow_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new Tomorrow(new DateTime(2022,12,29)).Evaluate(dt), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28", false)]
        [TestCase("2022-12-29", true)]
        [TestCase("2022-12-30", false)]
        [TestCase("1970-05-06", false)]
        public void Today_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new Today(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28", true)]
        [TestCase("2022-12-29", false)]
        [TestCase("2022-12-30", false)]
        [TestCase("1970-05-06", false)]
        public void Yesterday_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new Yesterday(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));


        [Test]
        [TestCase("2022-12-25", false)]
        [TestCase("2022-12-28", true)]
        [TestCase("2022-12-29", true)]
        [TestCase("2022-12-30", true)]
        [TestCase("2023-01-01", true)]
        [TestCase("2023-01-02", false)]
        [TestCase("1970-05-06", false)]
        public void CurrentWeek_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new CurrentWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-30", false)]
        [TestCase("2022-12-01", true)]
        [TestCase("2022-12-28", true)]
        [TestCase("2022-12-29", true)]
        [TestCase("2022-12-30", true)]
        [TestCase("2023-01-01", false)]
        [TestCase("1970-05-06", false)]
        public void CurrentMonth_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new CurrentMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

        [Test]
        [TestCase("2021-12-31", false)]
        [TestCase("2022-11-30", true)]
        [TestCase("2022-12-01", true)]
        [TestCase("2022-12-28", true)]
        [TestCase("2022-12-29", true)]
        [TestCase("2022-12-30", true)]
        [TestCase("2023-01-01", false)]
        [TestCase("1970-05-06", false)]
        public void CurrentYear_DateTime_Valid(DateTime dt, bool expected)
            => Assert.That(new CurrentYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));
    }
}
