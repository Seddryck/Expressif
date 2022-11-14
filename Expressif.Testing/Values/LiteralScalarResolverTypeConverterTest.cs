using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values.Resolvers;

namespace Expressif.Values
{
    public class LiteralScalarResolverTypeConverterTest
    {
        [Test]
        [TestCase("foo", typeof(LiteralScalarResolver<string>))]
        [TestCase(123, typeof(LiteralScalarResolver<int>))]
        [TestCase(false, typeof(LiteralScalarResolver<bool>))]
        public void ConvertFrom_Value_ExpectedType(object value, Type expected)
            =>  Assert.That(new LiteralScalarResolverTypeConverter().ConvertFrom(value), Is.TypeOf(expected));

        [Test]
        public void ConvertFrom_DateTime_ExpectedType()
            => Assert.That(new LiteralScalarResolverTypeConverter().ConvertFrom(new DateTime(2022,11,14)), Is.TypeOf<LiteralScalarResolver<DateTime>>());
        
        [Test]
        public void ConvertFrom_Decimal_ExpectedType()
            => Assert.That(new LiteralScalarResolverTypeConverter().ConvertFrom(123.45m), Is.TypeOf<LiteralScalarResolver<decimal>>());
    }
}
