using System.ComponentModel;
using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class AroundTodayPredicatesTest
{
    public AroundTodayPredicatesTest()
    {
        TypeDescriptor.AddAttributes(
            typeof(DateOnly)
            , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
        );
    }

    [Conformance]
    public void IsTomorrow_Valid_Date(string text, bool expected)
        => Assert.That(new Tomorrow(new DateTime(2022, 12, 29))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void IsTomorrow_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new Tomorrow(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsToday_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new Today(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsToday_Valid_Date(string text, bool expected)
            => Assert.That(new Today(new DateTime(2022, 12, 29)).Evaluate(
                TypeDescriptor.GetConverter(typeof(DateOnly))
                .ConvertFromInvariantString(text)!
            ), Is.EqualTo(expected));

    [Conformance]
    public void IsYesterday_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new Yesterday(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsYesterday_Valid_Date(string text, bool expected)
            => Assert.That(new Yesterday(new DateTime(2022, 12, 29)).Evaluate(
                TypeDescriptor.GetConverter(typeof(DateOnly))
                .ConvertFromInvariantString(text)!
            ), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinCurrentWeek_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinCurrentMonth_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinCurrentYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinUpcomingWeek_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinUpcomingMonth_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinUpcomingYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinLastWeek_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinLastWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinLastMonth_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinLastMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinLastYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinLastYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinNextDays_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinNextDays(new DateTime(2022, 12, 29), () => 3).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void IsWithinPreviousDays_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinPreviousDays(new DateTime(2022, 12, 29), () => 3).Evaluate(dt), Is.EqualTo(expected));
}
