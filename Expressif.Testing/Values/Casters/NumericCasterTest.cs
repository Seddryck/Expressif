using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values.Casters
{
    public class NumericCasterTest
    {
        [Test]
        [TestCase(125, 125)]
        [TestCase(125.12, 125.12)]
        [TestCase(-125.12, -125.12)]
        [TestCase(125.12f, 125.12)]
        [TestCase(125.12d, 125.12)]
        [TestCase(true, 1)]
        [TestCase(Math.PI, 3.1415926535897931)]
        [TestCase("-125.12", -125.12)]
        [TestCase("+125.12", 125.12)]
        [TestCase("-1025.12", -1025.12)]
        [TestCase("-1,025.12", -1025.12)]
        [TestCase("  -1,025.12  ", -1025.12)]
        public void TryCast_Success(object obj, decimal expected)
        {
            Assert.Multiple(() =>
            {
                Assert.That(new NumericCaster().TryCast(obj, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase("foo")]
        [TestCase("300BC")]
        [TestCase("2022-12-28")]
        public void TryCast_Failure(object obj)
            => Assert.That(new NumericCaster().TryCast(obj, out _), Is.False);

        [Test]
        [TestCase(typeof(Byte))]
        [TestCase(typeof(SByte))]
        [TestCase(typeof(Int16))]
        [TestCase(typeof(Int32))]
        [TestCase(typeof(Int64))]
        [TestCase(typeof(UInt16))]
        [TestCase(typeof(UInt32))]
        [TestCase(typeof(UInt64))]
        [TestCase(typeof(Single))]
        [TestCase(typeof(Double))]
        [TestCase(typeof(Decimal))]
        public void Cast_Type_0(Type type)
           => Assert.That(new NumericCaster().Cast(
                Activator.CreateInstance(type)!)
                , Is.EqualTo(0));
    }
}
