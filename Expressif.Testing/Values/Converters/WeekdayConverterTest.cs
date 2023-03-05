using Expressif.Values;
using Expressif.Values.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values.Converters
{
    
    public class WeekdayConverterTest
    {
        [Test]
        public void CanConvertFrom_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(Weekday)).CanConvertFrom(typeof(string))
                , Is.True);

        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeOnly))]
        public void CanConvertFrom_Invalid(Type type)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(Weekday)).CanConvertFrom(type)
                , Is.False);

        [Test]
        public void ConvertFromInvariantString_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(Weekday)).ConvertFromInvariantString("Saturday")
                , Is.EqualTo(Weekdays.Saturday));

        [Test]
        [TestCase("2022-12-30")]
        [TestCase("foo")]
        public void ConvertFromInvariantString_Invalid(string value)
            => Assert.Throws<FormatException>(
                () => TypeDescriptor.GetConverter(typeof(Weekday)).ConvertFromInvariantString(value));


        [Test]
        public void CanConvertTo_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(Weekday)).CanConvertTo(typeof(string))
                , Is.True);

        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeOnly))]
        public void CanConvertTo_Invalid(Type type)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(Weekday)).CanConvertTo(type)
                , Is.False);


        [Test]
        public void ConvertToInvariantString_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(Weekday)).ConvertToInvariantString(Weekdays.Monday)
                , Is.EqualTo("Monday"));
    }
}
