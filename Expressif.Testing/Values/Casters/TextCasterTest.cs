using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values.Casters
{
    public class TextCasterTest
    {
        public TextCasterTest()
        {
            //TypeDescriptor.AddAttributes(
            //    typeof(DateOnly)
            //    , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
            //);
        }

        [Test]
        
        [TestCase("Foo bar", typeof(string), "Foo bar")]
        //[TestCase("2022-12-01", typeof(DateOnly), "2022-12-01")]
        [TestCase("2022-12-01 16:45:12", typeof(DateTime), "2022-12-01 16:45:12")]
        //[TestCase("2022-12", typeof(YearMonth), "2022-12")]
        public void TryCast_Type_Success(string text, Type type, string expected)
        {
            var obj = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text)!;
            Assert.Multiple(() =>
            {
                Assert.That(new TextCaster().TryCast(obj, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase(true, "True")]
        [TestCase(125, "125")]
        [TestCase(79125.12f, "79125.12")]
        [TestCase(79125.12d, "79125.12")]
        public void TryCast_Success(object obj, string expected)
            => Assert.Multiple(() =>
            {
                Assert.That(new TextCaster().TryCast(obj, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expected));
            });
    }
}
