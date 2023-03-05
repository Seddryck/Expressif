using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif.Testing.Values.Casters
{
    public class YearMonthCasterTest
    {
        public YearMonthCasterTest()
        {
            TypeDescriptor.AddAttributes(
                typeof(DateOnly)
                , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
            );
        }

        [Test]
        [TestCase("2022-12", typeof(YearMonth), "2022-12")]
        [TestCase("2022-12", typeof(string), "2022-12")]
        [TestCase("2022-12-01", typeof(DateOnly), "2022-12")]
        [TestCase("2022-12-01 16:45:12", typeof(DateTime), "2022-12")]
        public void TryCast_Success(string text, Type type, string expected)
        {
            var obj = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text)!;
            Assert.Multiple(() =>
            {
                Assert.That(new YearMonthCaster().TryCast(obj, out var value), Is.True);
                Assert.That(value, Is.Not.Null);
                Assert.That(value, Is.EqualTo((YearMonth)expected));
            });
        }

        [Test]
        [TestCase("foo", typeof(string))]
        [TestCase("300BC", typeof(string))]
        [TestCase("2022-77", typeof(string))]
        [TestCase("2022-12-28", typeof(string))]
        public void TryCast_Failure(string text, Type type)
        {
            var obj = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text)!;
            Assert.Multiple(() =>
            {
                Assert.That(new YearMonthCaster().TryCast(obj, out var value), Is.False);
                Assert.That(value, Is.Null);
            });
        }
    }
}
