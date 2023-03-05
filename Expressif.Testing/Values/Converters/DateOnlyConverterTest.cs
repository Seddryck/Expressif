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
    
    public class DateOnlyConverterTest
    {
        public DateOnlyConverterTest()
        {
            TypeDescriptor.AddAttributes(
                typeof(DateOnly)
                , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
            );
        }

        [Test]
        public void CanConvertFrom_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(DateOnly)).CanConvertFrom(typeof(string))
                , Is.True);

        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeOnly))]
        [TestCase(typeof(DateTime))]
        public void CanConvertFrom_Invalid(Type type)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(DateOnly)).CanConvertFrom(type)
                , Is.False);

        [Test]
        public void ConvertFromInvariantString_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(DateOnly)).ConvertFromInvariantString("2022-12-29")
                , Is.EqualTo(new DateOnly(2022, 12, 29)));

        [Test]
        [TestCase("2022-15-78")]
        [TestCase("foo")]
        public void ConvertFromInvariantString_Invalid(string value)
            => Assert.Throws<FormatException>(
                () => TypeDescriptor.GetConverter(typeof(YearMonth)).ConvertFromInvariantString(value));

        [Test]
        public void CanConvertTo_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(DateOnly)).CanConvertTo(typeof(string))
                , Is.True);

        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeOnly))]
        public void CanConvertTo_Invalid(Type type)
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(DateOnly)).CanConvertTo(type)
                , Is.False);

        [Test]
        public void ConvertToInvariantString_Valid()
            => Assert.That(
                TypeDescriptor.GetConverter(typeof(DateOnly)).ConvertToInvariantString(new DateOnly(2022,12,29))
                , Is.EqualTo("2022-12-29"));
    }
}
