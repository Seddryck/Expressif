using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;
using System.ComponentModel;

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
    public void Tomorrow_Valid_Date(string text, bool expected)
        => Assert.That(new Tomorrow(new DateTime(2022, 12, 29))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void Tomorrow_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new Tomorrow(new DateTime(2022,12,29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Today_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new Today(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Today_Valid_Date(string text, bool expected)
            => Assert.That(new Today(new DateTime(2022, 12, 29)).Evaluate(
                TypeDescriptor.GetConverter(typeof(DateOnly))
                .ConvertFromInvariantString(text)!
            ), Is.EqualTo(expected));


    [Conformance]
    public void Yesterday_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new Yesterday(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void Yesterday_Valid_Date(string text, bool expected)
            => Assert.That(new Yesterday(new DateTime(2022, 12, 29)).Evaluate(
                TypeDescriptor.GetConverter(typeof(DateOnly))
                .ConvertFromInvariantString(text)!
            ), Is.EqualTo(expected));

    [Conformance]
    public void WithinCurrentWeek_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinCurrentMonth_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinCurrentYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinCurrentYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinUpcomingWeek_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinUpcomingMonth_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinUpcomingYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinUpcomingYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinLastWeek_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinLastWeek(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinLastMonth_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinLastMonth(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinLastYear_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinLastYear(new DateTime(2022, 12, 29)).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinNextDays_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinNextDays(new DateTime(2022, 12, 29), () => 3).Evaluate(dt), Is.EqualTo(expected));

    [Conformance]
    public void WithinPreviousDays_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new WithinPreviousDays(new DateTime(2022, 12, 29), () => 3).Evaluate(dt), Is.EqualTo(expected));
}
