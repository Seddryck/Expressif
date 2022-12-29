using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values.Casters
{
    public class IntegerCasterTest
    {
        [Test]
        [TestCase(125, 125)]
        [TestCase(-125, -125)]
        [TestCase(125.0f, 125)]
        [TestCase(125.0d, 125)]
        [TestCase(true, 1)]
        [TestCase("-125", -125)]
        [TestCase("+125", 125)]
        public void TryCast_Success(object obj, decimal expected)
        {
            Assert.Multiple(() =>
            {
                Assert.That(new IntegerCaster().TryCast(obj, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expected));
            });
        }

        [Test]
        [TestCase("foo")]
        [TestCase("300BC")]
        [TestCase("2022-12-28")]
        [TestCase("-125.12")]
        [TestCase("+125.12")]
        [TestCase("-1025.12")]
        [TestCase("-1,025.12")]
        [TestCase("  1,025.12  ")]
        [TestCase("  -1,025.12  ")]
        [TestCase(125.12)]
        [TestCase(-125.12)]
        [TestCase(125.12f)]
        [TestCase(125.12d)]
        [TestCase(Math.PI)]
        public void TryCast_Failure(object obj)
            => Assert.That(new IntegerCaster().TryCast(obj, out _), Is.False);

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
           => Assert.That(new IntegerCaster().Cast(
                Activator.CreateInstance(type)!)
                , Is.EqualTo(0));

        [Test]
        [TestCase(typeof(Byte))]
        [TestCase(typeof(SByte))]
        [TestCase(typeof(Int16))]
        [TestCase(typeof(Int32))]
        [TestCase(typeof(Int64))]
#if NET7_0_OR_GREATER
        [TestCase(typeof(Int128))]
#endif
        [TestCase(typeof(UInt16))]
        [TestCase(typeof(UInt32))]
        [TestCase(typeof(UInt64))]
#if NET7_0_OR_GREATER
        [TestCase(typeof(UInt128))]
        [TestCase(typeof(Half))]
#endif
        [TestCase(typeof(Single))]
        [TestCase(typeof(Double))]
        [TestCase(typeof(Decimal))]
        public void IsNumericType_Type_Valid(Type type)
           => Assert.That(TypeChecker.IsNumericType(Activator.CreateInstance(type)!), Is.True);

        [Test]
        [TestCase(typeof(string))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(DateOnly))]
        [TestCase(typeof(TimeOnly))]
        [TestCase(typeof(DateTimeOffset))]
        [TestCase(typeof(TimeSpan))]
        public void IsNumericType_Type_Invalid(Type type)
           => Assert.That(TypeChecker.IsNumericType(type), Is.False);
    }
}
