using Expressif.Predicates.Temporal;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Temporal;

public class WeekdayPredicatesTest
{
    public WeekdayPredicatesTest()
    {
        TypeDescriptor.AddAttributes(
            typeof(DateOnly)
            , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
        );
    }

    [Test]
    [TestCase("2022-12-28", "Wednesday", true)]
    [TestCase("2022-12-28", "Thursday", false)]
    [TestCase("2022-12-29", "Thursday", true)]
    [TestCase("2022-12-29", "Monday", false)]
    public void Weekday_Date_Valid(string text, string dayOfWeek, bool expected)
        => Assert.That(new Weekday(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-30", false)]
    [TestCase("2022-12-31", true)]
    [TestCase("2023-01-01", true)]
    [TestCase("2023-01-02", false)]
    public void Weekend_Date_Valid(string text, bool expected)
        => Assert.That(new Weekend()
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-30 14:00:00", false)]
    [TestCase("2022-12-31 14:00:00", true)]
    [TestCase("2023-01-01 14:00:00", true)]
    [TestCase("2023-01-02 14:00:00", false)]
    public void Weekend_DateTime_Valid(DateTime dateTime, bool expected)
        => Assert.That(new Weekend().Evaluate(dateTime)
            , Is.EqualTo(expected));

    [Test]
    [TestCase("2022-12-30", true)]
    [TestCase("2022-12-31", false)]
    [TestCase("2023-01-01", false)]
    [TestCase("2023-01-02", true)]
    public void BusinessDay_Date_Valid(string text, bool expected)
        => Assert.That(new BusinessDay()
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));
}
