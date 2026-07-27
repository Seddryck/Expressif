using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;
using System.ComponentModel;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class WeekdayPredicatesTest
{
    public WeekdayPredicatesTest()
    {
        TypeDescriptor.AddAttributes(
            typeof(DateOnly)
            , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
        );
    }

    [Conformance]
    public void Weekday_Valid_Date(string text, string dayOfWeek, bool expected)
        => Assert.That(new Weekday(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void Weekend_Valid_Date(string text, bool expected)
        => Assert.That(new Weekend()
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void Weekend_Valid_DateTime(DateTime dateTime, bool expected)
        => Assert.That(new Weekend().Evaluate(dateTime)
            , Is.EqualTo(expected));

    [Conformance]
    public void BusinessDay_Valid_Date(string text, bool expected)
        => Assert.That(new BusinessDay()
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void BusinessDay_Valid_DateTime(DateTime dateTime, bool expected)
        => Assert.That(new BusinessDay().Evaluate(dateTime)
            , Is.EqualTo(expected));
}
