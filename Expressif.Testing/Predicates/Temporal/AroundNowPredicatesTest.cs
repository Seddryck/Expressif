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
    public class AroundNowPredicatesTest
    {
        public AroundNowPredicatesTest()
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
        public void InTheFuture_Date_Valid(string text, bool expected)
            => Assert.That(new InTheFuture(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(
                        TypeDescriptor.GetConverter(typeof(DateOnly))
                        .ConvertFromInvariantString(text)!
                    )
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28", false)]
        [TestCase("2022-12-29", true)]
        [TestCase("2022-12-30", true)]
        public void InTheFutureOrToday_Date_Valid(string text, bool expected)
            => Assert.That(new InTheFutureOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(
                        TypeDescriptor.GetConverter(typeof(DateOnly))
                        .ConvertFromInvariantString(text)!
                    )
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28", true)]
        [TestCase("2022-12-29", false)]
        [TestCase("2022-12-30", false)]
        public void InThePast_Date_Valid(string text, bool expected)
            => Assert.That(new InThePast(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(
                        TypeDescriptor.GetConverter(typeof(DateOnly))
                        .ConvertFromInvariantString(text)!
                    )
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28", true)]
        [TestCase("2022-12-29", true)]
        [TestCase("2022-12-30", false)]
        public void InThePastOrToday_Date_Valid(string text, bool expected)
            => Assert.That(new InThePastOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(
                        TypeDescriptor.GetConverter(typeof(DateOnly))
                        .ConvertFromInvariantString(text)!
                    )
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28 14:00:00", false)]
        [TestCase("2022-12-29 14:00:00", false)]
        [TestCase("2022-12-29 16:00:00", true)]
        [TestCase("2022-12-30 16:00:00", true)]
        public void InTheFuture_Date_Valid(DateTime dt, bool expected)
            => Assert.That(new InTheFuture(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(dt)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28 14:00:00", false)]
        [TestCase("2022-12-29 14:00:00", true)]
        [TestCase("2022-12-29 16:00:00", true)]
        [TestCase("2022-12-30 16:00:00", true)]
        public void InTheFutureOrToday_Date_Valid(DateTime dt, bool expected)
            => Assert.That(new InTheFutureOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(dt)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28 14:00:00", true)]
        [TestCase("2022-12-29 14:00:00", true)]
        [TestCase("2022-12-29 16:00:00", false)]
        [TestCase("2022-12-30 16:00:00", false)]
        public void InThePast_Date_Valid(DateTime dt, bool expected)
            => Assert.That(new InThePast(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(dt)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2022-12-28 14:00:00", true)]
        [TestCase("2022-12-29 14:00:00", true)]
        [TestCase("2022-12-29 16:00:00", true)]
        [TestCase("2022-12-30 16:00:00", false)]
        public void InThePastOrToday_Date_Valid(DateTime dt, bool expected)
            => Assert.That(new InThePastOrToday(new DateTime(2022, 12, 29, 15, 0, 0))
                    .Evaluate(dt)
                , Is.EqualTo(expected));
    }
}
