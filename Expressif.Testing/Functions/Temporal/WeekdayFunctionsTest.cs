﻿using Expressif.Functions.Temporal;
using Expressif.Predicates.Temporal;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Temporal;

public class WeekdayFunctionsTest
{
    public WeekdayFunctionsTest()
    {
        TypeDescriptor.AddAttributes(
            typeof(DateOnly)
            , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
        );
    }

    [Test]
    [TestCase("2022-12-28", "Tuesday", "2023-01-03")]
    [TestCase("2022-12-28", "Wednesday", "2023-01-04")]
    [TestCase("2022-12-28", "Thursday", "2022-12-29")]
    public void NextWeekday_Date_Valid(string argument, string dayOfWeek, string expected)
        => Assert.That(new NextWeekday(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", "Tuesday", "2023-01-03")]
    [TestCase("2022-12-28", "Wednesday", "2022-12-28")]
    [TestCase("2022-12-28", "Thursday", "2022-12-29")]
    public void NextWeekdayOrSame_Date_Valid(string argument, string dayOfWeek, string expected)
        => Assert.That(new NextWeekdayOrSame(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", "Tuesday", "2022-12-27")]
    [TestCase("2022-12-28", "Wednesday", "2022-12-21")]
    [TestCase("2022-12-28", "Thursday", "2022-12-22")]
    public void PreviousWeekday_Date_Valid(string argument, string dayOfWeek, string expected)
        => Assert.That(new PreviousWeekday(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", "Tuesday", "2022-12-27")]
    [TestCase("2022-12-28", "Wednesday", "2022-12-28")]
    [TestCase("2022-12-28", "Thursday", "2022-12-22")]
    public void PreviousWeekdayOrSame_Date_Valid(string argument, string dayOfWeek, string expected)
        => Assert.That(new PreviousWeekdayOrSame(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", "Tuesday", "2022-12-06")]
    [TestCase("2022-12-28", "Wednesday", "2022-12-07")]
    [TestCase("2022-12-28", "Thursday", "2022-12-01")]
    public void FirstInMonth_Date_Valid(string argument, string dayOfWeek, string expected)
        => Assert.That(new FirstInMonth(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", "Tuesday", "2022-12-27")]
    [TestCase("2022-12-28", "Wednesday", "2022-12-28")]
    [TestCase("2022-12-28", "Thursday", "2022-12-29")]
    public void LastInMonth_Date_Valid(string argument, string dayOfWeek, string expected)
        => Assert.That(new LastInMonth(() => (Expressif.Values.Weekday)
                    TypeDescriptor.GetConverter(typeof(Expressif.Values.Weekday))
                    .ConvertFromInvariantString(dayOfWeek)!
                )
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", 1, "2022-12-29")]
    [TestCase("2022-12-28", 4, "2023-01-03")]
    [TestCase("2022-12-28", 14, "2023-01-17")]
    [TestCase("2022-12-30", 1, "2023-01-02")]
    [TestCase("2022-12-31", 1, "2023-01-02")]
    [TestCase("2023-01-01", 1, "2023-01-02")]
    public void NextBusinessDays_Date_Valid(string argument, int count, string expected)
        => Assert.That(new NextBusinessDays(() => count)
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));

    [Test]
    [TestCase("2022-12-28", 1, "2022-12-27")]
    [TestCase("2022-12-28", 4, "2022-12-22")]
    [TestCase("2022-12-28", 14, "2022-12-08")]
    [TestCase("2022-12-31", 1, "2022-12-30")]
    [TestCase("2023-01-01", 1, "2022-12-30")]
    [TestCase("2023-01-02", 1, "2022-12-30")]
    public void PreviousBusinessDays_Date_Valid(string argument, int count, string expected)
        => Assert.That(new PreviousBusinessDays(() => count)
                .Evaluate(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(argument)!
                )
            , Is.EqualTo(
                    TypeDescriptor.GetConverter(typeof(DateOnly))
                    .ConvertFromInvariantString(expected)!
            ));
}
