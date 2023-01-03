using Expressif.Values.Casters;
using Expressif.Values.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif.Values
{
    [TypeConverter(typeof(YearMonthConverter))]
    public record YearMonth(int Year, int Month)
#if NET7_0_OR_GREATER
        : IParsable<YearMonth>
#endif
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

        private static readonly NumberStyles Style = NumberStyles.None;
        private static readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public static YearMonth Parse(string value, IFormatProvider? provider)
         => TryParse(value, provider, out var result) ? result : throw new FormatException();

        public static bool TryParse(string? value, IFormatProvider? provider, [NotNullWhen(true)] out YearMonth? result)
        {
            if (string.IsNullOrEmpty(value) 
                    || value.Length != 7
                    || value[4] != '-'
                    || !int.TryParse(value[..4], Style, Format, out var year)
                    || !int.TryParse(value[5..], Style, Format, out var month)
                    || month > 12
                )
                return (result = null) is not null;

            return (result = new YearMonth(year, month)) is not null;
        }
}
}
