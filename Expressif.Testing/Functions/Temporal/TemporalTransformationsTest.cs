using Expressif.Functions.Temporal;
using Expressif.Values;
using Expressif.Values.Special;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Temporal
{
    [TestFixture]
    public class TemporalTransformationsTest
    {
        [Test]
        [TestCase("2019-03-11", "2019-03-11")]
        [TestCase("2019-02-11", "2019-03-01")]
        [TestCase("2019-04-11", "2019-03-31")]
        public void DateTimeToClip_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToClip(new LiteralScalarResolver<DateTime>("2019-03-01"), new LiteralScalarResolver<DateTime>("2019-03-31"))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-03-12")]
        [TestCase("2019-02-11", "2019-02-12")]
        [TestCase("2019-03-31", "2019-04-01")]
        public void DateTimeToNextDay_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToNextDay().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-04-11")]
        [TestCase("2019-03-31", "2019-04-30")]
        [TestCase("2020-01-31", "2020-02-29")]
        public void DateTimeToNextMonth_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToNextMonth().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2020-03-11")]
        [TestCase("2020-02-29", "2021-02-28")]
        public void DateTimeToNextYear_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToNextYear().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-03-10")]
        [TestCase("2019-02-01", "2019-01-31")]
        [TestCase("2020-03-01", "2020-02-29")]
        [TestCase("2020-03-01 17:30:12", "2020-02-29 17:30:12")]
        public void DateTimeToPreviousDay_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToPreviousDay().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-02-11")]
        [TestCase("2019-03-31", "2019-02-28")]
        [TestCase("2020-01-31", "2019-12-31")]
        [TestCase("2020-01-31 17:30:12", "2019-12-31 17:30:12")]
        public void DateTimeToPreviousMonth_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToPreviousMonth().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2018-03-11")]
        [TestCase("2020-02-29", "2019-02-28")]
        public void DateTimeToPreviousYear_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToPreviousYear().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 12:00:00", "07:00:00", "2019-03-11 07:00:00")]
        [TestCase("2019-02-11 08:45:12", "07:13:11", "2019-02-11 07:13:11")]
        public void DateTimeToSetTime_Valid(object value, string instant, DateTime expected)
            => Assert.That(new DateTimeToSetTime(new LiteralScalarResolver<string>(instant)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:24", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:40:00", "2019-03-11 17:00:00")]
        public void DateTimeToFloorHour_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToFloorHour().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 18:00:00")]
        [TestCase("2019-03-11 17:20:24", "2019-03-11 18:00:00")]
        [TestCase("2019-03-11 17:40:00", "2019-03-11 18:00:00")]
        public void DateTimeToCeilingHour_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToCeilingHour().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 17:20:00")]
        [TestCase("2019-03-11 17:20:24.120", "2019-03-11 17:20:00")]
        [TestCase("2019-03-11 17:40:59", "2019-03-11 17:40:00")]
        public void DateTimeToFloorMinute_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToFloorMinute().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 17:20:00")]
        [TestCase("2019-03-11 17:20:24.120", "2019-03-11 17:21:00")]
        [TestCase("2019-03-11 17:59:59", "2019-03-11 18:00:00")]
        public void DateTimeToCeilingMinute_Valid(object value, DateTime expected)
            => Assert.That(new DateTimeToCeilingMinute().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", 0, "04:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:00:00", 1, "04:00:00", "2019-03-11 21:00:00")]
        [TestCase("2019-03-11 17:00:00", 2, "04:00:00", "2019-03-12 01:00:00")]
        [TestCase("2019-03-11 17:00:00", -1, "04:00:00", "2019-03-11 13:00:00")]
        public void DateTimeToAdd_Valid(object value, int times, string timeSpan, DateTime expected)
            => Assert.That(new DateTimeToAdd(new LiteralScalarResolver<string>(timeSpan), new LiteralScalarResolver<int>(times))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", 0, "04:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:00:00", 1, "04:00:00", "2019-03-11 13:00:00")]
        [TestCase("2019-03-11 17:00:00", 5, "04:00:00", "2019-03-10 21:00:00")]
        [TestCase("2019-03-11 17:00:00", -1, "04:00:00", "2019-03-11 21:00:00")]
        public void DateTimeToSubtract_Valid(object value, int times, string timeSpan, DateTime expected)
        => Assert.That(new DateTimeToSubtract(new LiteralScalarResolver<string>(timeSpan), new LiteralScalarResolver<int>(times))
            .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(9, 8, 44)]
        [TestCase(12, 28, 43)]
        public void DateToAge_Born1978_Min43(int month, int day, int age)
            => Assert.That(new DateToAge().Evaluate(new DateTime(1978, month, day)), Is.AtLeast(age));

        [Test]
        [TestCase("2018-02-01 00:00:00", "2018-02-01 01:00:00")]
        [TestCase("2018-08-01 00:00:00", "2018-08-01 02:00:00")]
        public void UtcToLocal_RomanceStandardTime_Valid(object value, DateTime expected)
            => Assert.That(new UtcToLocal(new LiteralScalarResolver<string>("Romance Standard Time")).Evaluate(value)
            , Is.EqualTo(expected));

        [Test]
        [TestCase("2018-02-01 00:00:00", "2018-02-01 01:00:00")]
        [TestCase("2018-08-01 00:00:00", "2018-08-01 02:00:00")]
        public void UtcToLocal_CityName_Valid(object value, DateTime expected)
            => Assert.That(new UtcToLocal(new LiteralScalarResolver<string>("Brussels")).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2018-02-01 03:00:00", "2018-02-01 02:00:00")]
        [TestCase("2018-08-01 02:00:00", "2018-08-01 00:00:00")]
        public void LocalToUtc_RomanceStandardTime_Valid(object value, DateTime expected)
            => Assert.That(new LocalToUtc(new LiteralScalarResolver<string>("Romance Standard Time")).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2018-02-01 07:00:00", "2018-02-01 06:00:00")]
        [TestCase("2018-08-01 01:00:00", "2018-07-31 23:00:00")]
        public void LocalToUtc_CityName_Valid(object value, DateTime expected)
            => Assert.That(new LocalToUtc(new LiteralScalarResolver<string>("Brussels")).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2018-02-01")]
        [TestCase("2018-02-01 00:00:00")]
        [TestCase("2018-02-01 07:00:00")]
        public void DateTimeToDate_Valid(object value)
            => Assert.That(new DateTimeToDate().Evaluate(value), Is.EqualTo(new DateTime(2018, 2, 1)));

        [Test]
        [TestCase("2018-02-01 07:00:00", "2018-02-01 07:00:00")]
        [TestCase("(null)", "2001-01-01")]
        public void NullToDate_Valid(object value, DateTime expected)
            => Assert.That(new NullToDate(new LiteralScalarResolver<DateTime>(new DateTime(2001, 1, 1))).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2018-02-01 07:00:00", "2018-02-01 07:00:00")]
        [TestCase("(empty)", "2001-01-01")]
        [TestCase("2018-02-31", "2001-01-01")]
        [TestCase("(null)", null)]
        public void InvalidToDate_Valid(object value, DateTime? expected)
            => Assert.That(new InvalidToDate(new LiteralScalarResolver<DateTime>(new DateTime(2001, 1, 1))).Evaluate(value)
                , Is.EqualTo(expected==null ? new Null() : expected));

        [Test]
        [TestCase("2018-02-01 00:00:00", "2018-02-01")]
        [TestCase("2018-02-01 07:00:00", "2018-02-01")]
        [TestCase("2018-02-12 07:00:00", "2018-02-01")]
        [TestCase(null, null)]
        [TestCase("(null)", null)]
        public void DateTimeToFirstOfMonth_Valid(object value, DateTime? expected)
        {
            var function = new DateTimeToFirstOfMonth();
            var result = function.Evaluate(value);
            if (expected == new DateTime(1, 1, 1))
                Assert.That(result, Is.Null);
            else
                Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("2018-02-01 00:00:00", "2018-01-01")]
        [TestCase("2018-02-01 07:00:00", "2018-01-01")]
        [TestCase("2018-02-12 07:00:00", "2018-01-01")]
        [TestCase(null, null)]
        [TestCase("(null)", null)]
        public void DateTimeToFirstOfYear_Valid(object value, DateTime? expected)
        {
            var function = new DateTimeToFirstOfYear();
            var result = function.Evaluate(value);
            if (expected == new DateTime(1, 1, 1))
                Assert.That(result, Is.Null);
            else
                Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("2018-02-01 00:00:00", "2018-02-28")]
        [TestCase("2018-02-01 07:00:00", "2018-02-28")]
        [TestCase("2018-02-12 07:00:00", "2018-02-28")]
        [TestCase("2020-02-12 07:00:00", "2020-02-29")]
        [TestCase(null, null)]
        [TestCase("(null)", null)]
        public void DateTimeToLastOfMonth_Valid(object value, DateTime? expected)
        {
            var function = new DateTimeToLastOfMonth();
            var result = function.Evaluate(value);
            if (expected == new DateTime(1, 1, 1))
                Assert.That(result, Is.Null);
            else
                Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("2018-02-01 00:00:00", "2018-12-31")]
        [TestCase("2018-02-01 07:00:00", "2018-12-31")]
        [TestCase("2018-02-12 07:00:00", "2018-12-31")]
        [TestCase(null, null)]
        [TestCase("(null)", null)]
        public void DateTimeToLastOfYear_Valid(object value, DateTime? expected)
        {
            var function = new DateTimeToLastOfYear();
            var result = function.Evaluate(value);
            if (expected == new DateTime(1, 1, 1))
                Assert.That(result, Is.Null);
            else
                Assert.That(result, Is.EqualTo(expected));
        }
    }
}
