using Expressif.Functions.Temporal;
using Expressif.Functions.Text;
using Expressif.Values;
using Expressif.Values.Resolvers;
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
    public class TemporalFunctionsTest
    {
        [Test]
        [TestCase("2019-03-11", "2019-03-11")]
        [TestCase("2019-02-11", "2019-03-01")]
        [TestCase("2019-04-11", "2019-03-31")]
        public void Clamp_Valid(object value, DateTime expected)
            => Assert.That(new Clamp(new LiteralScalarResolver<DateTime>("2019-03-01"), new LiteralScalarResolver<DateTime>("2019-03-31"))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-03-12")]
        [TestCase("2019-02-11", "2019-02-12")]
        [TestCase("2019-03-31", "2019-04-01")]
        public void NextDay_Valid(object value, DateTime expected)
            => Assert.That(new NextDay().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-04-11")]
        [TestCase("2019-03-31", "2019-04-30")]
        [TestCase("2020-01-31", "2020-02-29")]
        public void NextMonth_Valid(object value, DateTime expected)
            => Assert.That(new NextMonth().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2020-03-11")]
        [TestCase("2020-02-29", "2021-02-28")]
        public void NextYear_Valid(object value, DateTime expected)
            => Assert.That(new NextYear().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-03-10")]
        [TestCase("2019-02-01", "2019-01-31")]
        [TestCase("2020-03-01", "2020-02-29")]
        [TestCase("2020-03-01 17:30:12", "2020-02-29 17:30:12")]
        public void PreviousDay_Valid(object value, DateTime expected)
            => Assert.That(new PreviousDay().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2019-02-11")]
        [TestCase("2019-03-31", "2019-02-28")]
        [TestCase("2020-01-31", "2019-12-31")]
        [TestCase("2020-01-31 17:30:12", "2019-12-31 17:30:12")]
        public void PreviousMonth_Valid(object value, DateTime expected)
            => Assert.That(new PreviousMonth().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11", "2018-03-11")]
        [TestCase("2020-02-29", "2019-02-28")]
        public void PreviousYear_Valid(object value, DateTime expected)
            => Assert.That(new PreviousYear().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 12:00:00", "07:00:00", "2019-03-11 07:00:00")]
        [TestCase("2019-02-11 08:45:12", "07:13:11", "2019-02-11 07:13:11")]
        public void SetTime_Valid(object value, string instant, DateTime expected)
            => Assert.That(new SetTime(new LiteralScalarResolver<string>(instant)).Evaluate(value)
                , Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:24", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:40:00", "2019-03-11 17:00:00")]
        public void FloorHour_Valid(object value, DateTime expected)
            => Assert.That(new FloorHour().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 18:00:00")]
        [TestCase("2019-03-11 17:20:24", "2019-03-11 18:00:00")]
        [TestCase("2019-03-11 17:40:00", "2019-03-11 18:00:00")]
        public void CeilingHour_Valid(object value, DateTime expected)
            => Assert.That(new CeilingHour().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 17:20:00")]
        [TestCase("2019-03-11 17:20:24.120", "2019-03-11 17:20:00")]
        [TestCase("2019-03-11 17:40:59", "2019-03-11 17:40:00")]
        public void FloorMinute_Valid(object value, DateTime expected)
            => Assert.That(new FloorMinute().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:20:00", "2019-03-11 17:20:00")]
        [TestCase("2019-03-11 17:20:24.120", "2019-03-11 17:21:00")]
        [TestCase("2019-03-11 17:59:59", "2019-03-11 18:00:00")]
        public void CeilingMinute_Valid(object value, DateTime expected)
            => Assert.That(new CeilingMinute().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", 0, "04:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:00:00", 1, "04:00:00", "2019-03-11 21:00:00")]
        [TestCase("2019-03-11 17:00:00", 2, "04:00:00", "2019-03-12 01:00:00")]
        [TestCase("2019-03-11 17:00:00", -1, "04:00:00", "2019-03-11 13:00:00")]
        public void Forward_Valid(object value, int times, string timeSpan, DateTime expected)
            => Assert.That(new Forward(new LiteralScalarResolver<string>(timeSpan), new LiteralScalarResolver<int>(times))
                .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2019-03-11 17:00:00", 0, "04:00:00", "2019-03-11 17:00:00")]
        [TestCase("2019-03-11 17:00:00", 1, "04:00:00", "2019-03-11 13:00:00")]
        [TestCase("2019-03-11 17:00:00", 5, "04:00:00", "2019-03-10 21:00:00")]
        [TestCase("2019-03-11 17:00:00", -1, "04:00:00", "2019-03-11 21:00:00")]
        public void Back_Valid(object value, int times, string timeSpan, DateTime expected)
        => Assert.That(new Back(new LiteralScalarResolver<string>(timeSpan), new LiteralScalarResolver<int>(times))
            .Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(9, 8, 44)]
        [TestCase(12, 28, 43)]
        public void DateToAge_Born1978_Min43(int month, int day, int age)
            => Assert.That(new Age().Evaluate(new DateTime(1978, month, day)), Is.AtLeast(age));

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
        public void FirstOfMonth_Valid(object value, DateTime? expected)
        {
            var function = new FirstOfMonth();
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
        public void FirstOfYear_Valid(object value, DateTime? expected)
        {
            var function = new FirstOfYear();
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
        public void LastOfMonth_Valid(object value, DateTime? expected)
        {
            var function = new LastOfMonth();
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
        public void LastOfYear_Valid(object value, DateTime? expected)
        {
            var function = new LastOfYear();
            var result = function.Evaluate(value);
            if (expected == new DateTime(1, 1, 1))
                Assert.That(result, Is.Null);
            else
                Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("2019-11-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Brussels", "2019-11-01 20:58:00")]
        [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Brussels", "2019-10-01 21:58:00")]
        [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Moscow", "2019-10-01 22:58:00")]
        [TestCase("2019-10-01T19:58Z", "yyyy-MM-ddTHH:mmZ", "Pacific Standard Time", "2019-10-01 12:58:00")]
        public void TextToDateThenTimeAndUtcToLocal_Valid(string value, string format, string timeZone, DateTime expected)
        {
            var textToDateTime = new TextToDateTime(new LiteralScalarResolver<string>(format));
            var utcToLocal = new UtcToLocal(new LiteralScalarResolver<string>(timeZone));
            var result = utcToLocal.Evaluate(textToDateTime.Evaluate(value));
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(((DateTime)result).Kind, Is.EqualTo(DateTimeKind.Unspecified));
        }
    }
}
