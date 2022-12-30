using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class YearMonthCaster : ICaster<YearMonth>, IParser<YearMonth>
    {
        private readonly NumberStyles Style = NumberStyles.None;
        private readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public virtual bool TryCast(object obj, [NotNullWhen(true)] out YearMonth? value)
            => obj switch
            {
                YearMonth yearMonth => (value = yearMonth) is not null,
                DateTime dt => (value = new YearMonth(dt.Year, dt.Month)) is not null,
                DateOnly d => (value = new YearMonth(d.Year, d.Month)) is not null,
                string s => TryParse(s, out value),
                _ => (value = null) is not null
            };

        public virtual YearMonth Cast(object obj)
            => TryCast(obj, out var yearMonth)
                ? yearMonth
                : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(YearMonth)}. The type {nameof(YearMonth)} can only be casted from types DateTime, DateOnly and String. The expect string format is 'YYYY-MM' and MM should be less than 12");

        public virtual bool TryParse(string text, [NotNullWhen(true)] out YearMonth? value)
        {
            if (text.Length != 7
                    || text[4] != '-'
                    || !int.TryParse(text[..4], Style, Format, out var year)
                    || !int.TryParse(text[5..], Style, Format, out var month)
                    || month > 12
                )
                return (value = null) is not null;

            return (value = new YearMonth(year, month)) is not null;
        }

        public virtual YearMonth Parse(string text)
            => TryParse(text, out var value) ? value : throw new FormatException();
    }
}
