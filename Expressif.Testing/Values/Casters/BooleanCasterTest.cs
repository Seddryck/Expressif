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
    public class BooleanCasterTest
    {
        public BooleanCasterTest()
        {
            TypeDescriptor.AddAttributes(
                typeof(DateOnly)
                , new TypeConverterAttribute(typeof(Expressif.Values.Converters.DateOnlyConverter))
            );
        }

        [Test]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("True", true)]
        [TestCase("False", false)]
        [TestCase("TRUE", true)]
        [TestCase("FALSE", false)]
        [TestCase("1", true)]
        [TestCase("0", false)]
        [TestCase("yes", true)]
        [TestCase("no", false)]
        [TestCase("Yes", true)]
        [TestCase("No", false)]
        [TestCase("YES", true)]
        [TestCase("NO", false)]
        [TestCase("foo", false)]
        public void TryCast_String_Success(string text, bool expected)
        {
            Assert.Multiple(() =>
            {
                Assert.That(new BooleanCaster().TryCast(text, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase("2022-12-01 16:45:12Z", typeof(DateTime))]
        [TestCase("2022-12-01", typeof(DateOnly))]
        [TestCase("2022-12", typeof(YearMonth))]
        public void TryCast_Failure(string text, Type type)
        {
            var obj = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(text)!;
            Assert.That(new BooleanCaster().TryCast(obj, out _), Is.False);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(1, true)]
        [TestCase(0, false)]
        [TestCase(125, true)]
        [TestCase(125.12f, true)]
        [TestCase(125.12d, true)]
        [TestCase(-1, true)]
        [TestCase(-125, true)]
        [TestCase(-125.12f, true)]
        [TestCase(-125.12d, true)]
        public void TryCast_Success(object obj, bool expected)
            => Assert.Multiple(() =>
            {
                Assert.That(new BooleanCaster().TryCast(obj, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expected));
            });
    }
}
