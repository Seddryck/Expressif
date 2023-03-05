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
    
    public class YearMonthConverterTest
    {
        [Test]
        public void CanConvertFrom_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(YearMonth)).CanConvertFrom(typeof(string))
                , Is.True);

        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeOnly))]
        public void CanConvertFrom_Invalid(Type type)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(YearMonth)).CanConvertFrom(type)
                , Is.False);


        [Test]
        public void ConvertFromInvariantString_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(YearMonth)).ConvertFromInvariantString("2022-12")
                , Is.EqualTo(new YearMonth(2022, 12)));

        [Test]
        [TestCase("2022-78")]
        [TestCase("foo")]
        public void ConvertFromInvariantString_Invalid(string value)
            => Assert.Throws<FormatException>(
                () => TypeDescriptor.GetConverter(typeof(YearMonth)).ConvertFromInvariantString(value));


        [Test]
        public void CanConvertTo_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(YearMonth)).CanConvertTo(typeof(string))
                , Is.True);

        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeOnly))]
        public void CanConvertTo_Invalid(Type type)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(YearMonth)).CanConvertTo(type)
                , Is.False);


        [Test]
        [TestCase(2023, 3, "2023-03")]
        [TestCase(2023, 12, "2023-12")]
        public void ConvertToInvariantString_Valid(int year, int month, string expected)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(YearMonth)).ConvertToInvariantString(new YearMonth(year, month))
                , Is.EqualTo(expected));
    }
}
