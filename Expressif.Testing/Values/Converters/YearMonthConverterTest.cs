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
    }
}
