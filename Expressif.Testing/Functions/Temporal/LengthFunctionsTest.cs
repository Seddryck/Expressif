using Expressif.Functions.Temporal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Temporal
{
    public class LengthFunctionsTest
    {
        [Test]
        [TestCase(1900, 365)]
        [TestCase(1999, 365)]
        [TestCase(2000, 366)]
        [TestCase(2023, 365)]
        [TestCase(2024, 366)]
        public void LengthOfYear_Integer_Valid(int year, int expected)
            => Assert.That(new LengthOfYear().Evaluate(year), Is.EqualTo(expected));

        [Test]
        [TestCase("1900-05-25", 365)]
        [TestCase("1999-05-25", 365)]
        [TestCase("2000-05-25", 366)]
        [TestCase("2023-05-25", 365)]
        [TestCase("2024-05-25", 366)]
        public void LengthOfYear_DateTime_Valid(DateTime dt, int expected)
            => Assert.That(new LengthOfYear().Evaluate(dt), Is.EqualTo(expected));
    }
}
