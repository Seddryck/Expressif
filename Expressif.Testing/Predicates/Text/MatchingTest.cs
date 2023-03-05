using Expressif.Predicates.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Text
{
    public class MatchingTest
    {
        [Test]
        [TestCase("121", true)]
        [TestCase("1.21", true)]
        [TestCase("1000.21", true)]
        [TestCase("-1000.21", true)]
        [TestCase("52,000.21", false)]
        [TestCase("1000,21", false)]
        [TestCase("52.000,21", false)]
        [TestCase("$20", false)]
        [TestCase("A.1", false)]
        [TestCase("A121", false)]
        [TestCase("1!", false)]
        [TestCase("121A", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        [TestCase(125.17, true)]
        [TestCase(125, true)]
        public void MatchesNumeric_InvariantCulture_Success(object value, bool expected)
            => Assert.That(new MatchesNumeric().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("121", true)]
        [TestCase("1.21", false)]
        [TestCase("1000.21", false)]
        [TestCase("-1000.21", false)]
        [TestCase("52,000.21", false)]
        [TestCase("1000,21", true)]
        [TestCase("52.000,21", false)]
        [TestCase("$20", false)]
        [TestCase("A.1", false)]
        [TestCase("A121", false)]
        [TestCase("1!", false)]
        [TestCase("121A", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        [TestCase(125.17, true)]
        [TestCase(125, true)]
        public void MatchesNumeric_FrenchCulture_Success(object value, bool expected)
            => Assert.That(new MatchesNumeric(() => "fr-fr").Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-02", true)]
        [TestCase("2022-11-2", false)]
        [TestCase("2022-09-02", true)]
        [TestCase("2022-9-02", false)]
        [TestCase("22-09-02", false)]
        [TestCase("1978-09-02", true)]
        [TestCase("02-11-2022", false)]
        [TestCase("02/11/2022", false)]
        [TestCase("02.11.2022", false)]
        [TestCase("1.21", false)]
        [TestCase("foobar", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        public void MatchesDate_InvariantCulture_Success(object value, bool expected)
            => Assert.That(new MatchesDate().Evaluate(value), Is.EqualTo(expected));

        [Test]
        public void MatchesDate_InvariantCultureDateTime_Valid()
            => Assert.That(new MatchesDate().Evaluate(new DateTime(2022, 10, 5)), Is.True);

        [Test]
        public void MatchesDate_InvariantCultureDateTime_Invalid()
            => Assert.That(new MatchesDate().Evaluate(new DateTime(2022, 10, 5, 10, 5, 10)), Is.False);

        [Test]
        public void MatchesDate_InvariantCultureDateOnly_Valid()
            => Assert.That(new MatchesDate().Evaluate(new DateOnly(2022, 10, 5)), Is.True);

        [Test]
        public void MatchesDate_InvariantCultureTimeOnly_Valid()
            => Assert.That(new MatchesDate().Evaluate(new TimeOnly(10, 10, 5)), Is.False);

        [Test]
        [TestCase("2022-11-02", false)]
        [TestCase("2022-11-2", false)]
        [TestCase("2022-09-02", false)]
        [TestCase("02-11-2022", false)]
        [TestCase("02/11/2022", true)]
        [TestCase("20/09/1978", true)]
        [TestCase("2/11/2022", false)]
        [TestCase("02/9/2022", false)]
        [TestCase("02.11.2022", false)]
        [TestCase("1.21", false)]
        [TestCase("foobar", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        public void MatchesDate_FrenchCulture_Success(object value, bool expected)
            => Assert.That(new MatchesDate(() => "fr-fr").Evaluate(value), Is.EqualTo(expected));

        [Test]
        public void MatchesDate_FrenchCultureDateTime_Valid()
            => Assert.That(new MatchesDate().Evaluate(new DateTime(2022, 10, 5)), Is.True);

        [Test]
        public void MatchesDate_FrenchCultureDateTime_Invalid()
            => Assert.That(new MatchesDate().Evaluate(new DateTime(2022, 10, 5, 10, 5, 10)), Is.False);

        [Test]
        [TestCase("2022-11-02", false)]
        [TestCase("2022-11-2", false)]
        [TestCase("2022-09-02", false)]
        [TestCase("02-11-2022", false)]
        [TestCase("02/11/2022", true)]
        [TestCase("20/09/1978", true)]
        [TestCase("2/11/2022", true)]
        [TestCase("02/9/2022", false)]
        [TestCase("2/9/2022", false)]
        [TestCase("02.11.2022", false)]
        [TestCase("1.21", false)]
        [TestCase("foobar", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        public void MatchesDate_DutchCulture_Success(object value, bool expected)
            => Assert.That(new MatchesDate(() => "nl-be").Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-02", false)]
        [TestCase("2022-11-02 13:57:00", true)]
        [TestCase("2022-11-02 13:57:00.123", false)]
        [TestCase("2022-11-02 1:52PM", false)]
        [TestCase("1978-09-02", false)]
        [TestCase("1978-09-02 13:57:00", true)]
        [TestCase("02-11-2022 13:57:00", false)]
        [TestCase("02/11/2022 13:57:00", false)]
        [TestCase("02.11.2022 13:57:00", false)]
        [TestCase("1.21", false)]
        [TestCase("foobar", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        public void MatchesDateTime_InvariantCulture_Success(object value, bool expected)
            => Assert.That(new MatchesDateTime().Evaluate(value), Is.EqualTo(expected));

        [Test]
        public void MatchesDateTime_InvariantCultureDateTimeOnDate_Valid()
            => Assert.That(new MatchesDate().Evaluate(new System.DateTime(2022, 10, 5)), Is.True);

        [Test]
        public void MatchesDateTime_InvariantCultureDateTime_Valid()
            => Assert.That(new MatchesDateTime().Evaluate(new System.DateTime(2022, 10, 5, 10, 5, 10)), Is.True);

        [Test]
        public void MatchesDateTime_InvariantCultureDateOnly_Valid()
            => Assert.That(new MatchesDateTime().Evaluate(new DateOnly(2022, 10, 5)), Is.True);

        [Test]
        public void MatchesDateTime_InvariantCultureTimeOnly_Valid()
            => Assert.That(new MatchesDateTime().Evaluate(new TimeOnly(10, 10, 5)), Is.False);

        [Test]
        [TestCase("2022-11-02", false)]
        [TestCase("2022-11-02 13:57:00", false)]
        [TestCase("2022-11-02 13:57:00.123", false)]
        [TestCase("2022-11-02 1:52PM", false)]
        [TestCase("1978-09-02", false)]
        [TestCase("13:57:00", true)]
        [TestCase("13:57:00.125", false)]
        [TestCase("1:57PM", false)]
        [TestCase("1:57:00PM", false)]
        [TestCase("1.21", false)]
        [TestCase("foobar", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        public void MatchesTime_InvariantCulture_Success(object value, bool expected)
            => Assert.That(new MatchesTime().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase("2022-11-02", false)]
        [TestCase("2022-11-02 13:57:00", false)]
        [TestCase("2022-11-02 13:57:00.123", false)]
        [TestCase("2022-11-02 1:52PM", false)]
        [TestCase("1978-09-02", false)]
        [TestCase("13:57:00", true)]
        [TestCase("13:57:00.125", false)]
        [TestCase("1:57PM", false)]
        [TestCase("1:57:00PM", false)]
        [TestCase("1.21", false)]
        [TestCase("foobar", false)]
        [TestCase("(empty)", false)]
        [TestCase("", false)]
        [TestCase("(null)", false)]
        [TestCase(null, false)]
        public void MatchesTime_EnglishCulture_Success(object value, bool expected)
            => Assert.That(new MatchesTime(() => "en-gb").Evaluate(value), Is.EqualTo(expected));

        [Test]
        public void MatchesTime_InvariantCultureDateTimeOnDate_Valid()
            => Assert.That(new MatchesTime().Evaluate(new System.DateTime(2022, 10, 5)), Is.False);

        [Test]
        public void MatchesTime_InvariantCultureDateTime_Valid()
            => Assert.That(new MatchesTime().Evaluate(new System.DateTime(2022, 10, 5, 10, 5, 10)), Is.False);

        [Test]
        public void MatchesTime_InvariantCultureDateOnly_Valid()
            => Assert.That(new MatchesTime().Evaluate(new DateOnly(2022, 10, 5)), Is.False);

        [Test]
        public void MatchesTime_InvariantCultureTimeOnly_Valid()
            => Assert.That(new MatchesTime().Evaluate(new TimeOnly(13, 10, 5)), Is.True);

        [Test]
        public void MatchesTime_InvariantCultureTimeSpan_Valid()
            => Assert.That(new MatchesTime().Evaluate(new TimeSpan(13, 10, 5)), Is.True);

        [Test]
        public void MatchesTime_InvariantCultureTimeSpanOver24_Valid()
            => Assert.That(new MatchesTime().Evaluate(new TimeSpan(24, 10, 5)), Is.False);
    }
}
