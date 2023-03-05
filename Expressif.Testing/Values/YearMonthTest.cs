using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values
{
    public class YearMonthTest
    {
        [Test]
        public void Parse_Name_January2023()
            => Assert.That(YearMonth.Parse("2023-01", null), Is.EqualTo(new YearMonth(2023,1)));

        [Test]
        public void TryParse_Name_January2023()
        => Assert.Multiple(() =>
        {
            Assert.That(YearMonth.TryParse("2023-01", null, out var value), Is.True);
            Assert.That(value, Is.EqualTo(new YearMonth(2023, 1)));
        });

        [Test]
        public void TryParse_Name_AnyYearMonth()
            => Assert.That(YearMonth.TryParse("2023-89", null, out var _), Is.False);
    }
}
