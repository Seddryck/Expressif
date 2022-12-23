using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Casters
{
    public class NumericCasterTest
    {
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
           => Assert.That(new NumericCaster().Execute(
                Activator.CreateInstance(type)!)
                , Is.EqualTo(0));
    }
}
