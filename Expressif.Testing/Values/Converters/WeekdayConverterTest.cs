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
    }
}
