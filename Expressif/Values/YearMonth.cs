using Expressif.Values.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values
{
    [TypeConverter(typeof(YearMonthConverter))]
    public record YearMonth(int Year, int Month)
    {
        public override string ToString() => $"{Year:####}-{Month:##}";
        public override int GetHashCode() => Year.GetHashCode()^97 * Month.GetHashCode();

        public virtual bool Equals(YearMonth? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Year == other.Year && Month == other.Month;
        }

        public static implicit operator YearMonth(string v)
            => (YearMonth)TypeDescriptor.GetConverter(typeof(YearMonth)).ConvertFromInvariantString(v)!;
    }
}
