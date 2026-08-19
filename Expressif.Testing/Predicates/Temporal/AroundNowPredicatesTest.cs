using System.ComponentModel;
using Expressif.Predicates.Temporal;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Temporal;

[TestFixture]
public class AroundNowPredicatesTest
{
    public AroundNowPredicatesTest()
    {
        TypeDescriptor.AddAttributes(
            typeof(DateOnly)
            , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
        );
    }

    [Conformance]
    public void InTheFuture_Valid_DateOnly(string text, bool expected)
        => Assert.That(new InTheFuture(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void InTheFutureOrToday_Valid_DateOnly(string text, bool expected)
        => Assert.That(new InTheFutureOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void InTheFutureOrNow_Valid_DateOnly(string text, bool expected)
        => Assert.That(new InTheFutureOrNow(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void InThePast_Valid_DateOnly(string text, bool expected)
        => Assert.That(new InThePast(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void InThePastOrToday_Valid_DateOnly(string text, bool expected)
        => Assert.That(new InThePastOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void InThePastOrNow_Valid_DateOnly(string text, bool expected)
        => Assert.That(new InThePastOrNow(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(text)!
                )
            , Is.EqualTo(expected));

    [Conformance]
    public void InTheFuture_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new InTheFuture(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(dt)
            , Is.EqualTo(expected));

    [Conformance]
    public void InTheFutureOrToday_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new InTheFutureOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(dt)
            , Is.EqualTo(expected));

    [Conformance]
    public void InTheFutureOrNow_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new InTheFutureOrNow(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(dt)
            , Is.EqualTo(expected));

    [Conformance]
    public void InThePast_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new InThePast(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(dt)
            , Is.EqualTo(expected));

    [Conformance]
    public void InThePastOrToday_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new InThePastOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(dt)
            , Is.EqualTo(expected));

    [Conformance]
    public void InThePastOrNow_Valid_DateTime(DateTime dt, bool expected)
        => Assert.That(new InThePastOrNow(new DateTime(2022, 12, 29, 15, 0, 0))
                .Evaluate(dt)
            , Is.EqualTo(expected));
}
