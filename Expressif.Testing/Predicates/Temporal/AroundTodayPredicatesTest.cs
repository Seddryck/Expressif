using Expressif.Predicates.Temporal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif.Testing.Predicates.Temporal;

public class AroundTodayPredicatesTest
{
    public AroundTodayPredicatesTest()
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
    public void WithinCurrentWeek_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-11-30", false)]
    [TestCase("2022-12-01", true)]
    [TestCase("2022-12-28", true)]
    [TestCase("2022-12-29", true)]
    [TestCase("2022-12-30", true)]
    [TestCase("2023-01-01", false)]
    [TestCase("1970-05-06", false)]
    public void WithinCurrentMonth_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2021-12-31", false)]
    [TestCase("2022-11-30", true)]
    [TestCase("2022-12-01", true)]
    [TestCase("2022-12-28", true)]
    [TestCase("2022-12-29", true)]
    [TestCase("2022-12-30", true)]
    [TestCase("2023-01-01", false)]
    [TestCase("1970-05-06", false)]
    public void WithinCurrentYear_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-25", false)]
    [TestCase("2022-12-28", false)]
    [TestCase("2022-12-29", false)]
    [TestCase("2022-12-30", false)]
    [TestCase("2023-01-01", false)]
    [TestCase("2023-01-02", true)]
    [TestCase("2023-01-08", true)]
    [TestCase("2023-01-09", false)]
    [TestCase("1970-05-06", false)]
    public void WithinUpcomingWeek_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-11-30", false)]
    [TestCase("2022-12-01", false)]
    [TestCase("2022-12-28", false)]
    [TestCase("2022-12-29", false)]
    [TestCase("2022-12-30", false)]
    [TestCase("2023-01-01", true)]
    [TestCase("2023-01-31", true)]
    [TestCase("2023-02-01", false)]
    [TestCase("1970-05-06", false)]
    public void WithinUpcomingMonth_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2021-12-31", false)]
    [TestCase("2022-11-30", false)]
    [TestCase("2022-12-01", false)]
    [TestCase("2022-12-28", false)]
    [TestCase("2022-12-29", false)]
    [TestCase("2022-12-30", false)]
    [TestCase("2023-01-01", true)]
    [TestCase("2023-12-31", true)]
    [TestCase("2024-01-01", false)]
    [TestCase("1970-05-06", false)]
    public void WithinUpcomingYear_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-18", false)]
    [TestCase("2022-12-19", true)]
    [TestCase("2022-12-25", true)]
    [TestCase("2022-12-28", false)]
    [TestCase("2022-12-29", false)]
    [TestCase("2022-12-30", false)]
    [TestCase("2023-01-01", false)]
    public void WithinLastWeek_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinLastWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-10-31", false)]
    [TestCase("2022-11-01", true)]
    [TestCase("2022-11-30", true)]
    [TestCase("2022-12-01", false)]
    [TestCase("2022-12-28", false)]
    [TestCase("2022-12-29", false)]       
    [TestCase("2023-01-01", false)]
    public void WithinLastMonth_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinLastMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2020-12-31", false)]
    [TestCase("2021-01-01", true)]
    [TestCase("2021-12-31", true)]
    [TestCase("2022-01-01", false)]
    [TestCase("2022-11-30", false)]
    [TestCase("2022-12-01", false)]
    [TestCase("2022-12-30", false)]
    [TestCase("2023-01-01", false)]
    public void WithinLastYear_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinLastYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-25", false)]
    [TestCase("2022-12-26", false)]
    [TestCase("2022-12-28", false)]
    [TestCase("2022-12-29", false)]
    [TestCase("2022-12-30", true)]
    [TestCase("2023-01-01", true)]
    [TestCase("2023-01-02", false)]
    [TestCase("2023-01-08", false)]
    [TestCase("2023-01-09", false)]
    [TestCase("1970-05-06", false)]
    public void WithinNextDays_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinNextDays(new DateTime(2022, 12, 29), () => 3).Evaluate(dt), Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-25", false)]
    [TestCase("2022-12-26", true)]
    [TestCase("2022-12-28", true)]
    [TestCase("2022-12-29", false)]
    [TestCase("2022-12-30", false)]
    [TestCase("2023-01-01", false)]
    [TestCase("2023-01-02", false)]
    [TestCase("2023-01-08", false)]
    [TestCase("2023-01-09", false)]
    [TestCase("1970-05-06", false)]
    public void WithinPreviousDays_DateTime_Valid(DateTime dt, bool expected)
        => Assert.That(new WithinPreviousDays(new DateTime(2022, 12, 29), () => 3).Evaluate(dt), Is.EqualTo(expected));
}
