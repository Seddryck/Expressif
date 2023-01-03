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
            => YearMonth.TryParse(text, null, out value);

        public virtual YearMonth Parse(string text)
            => YearMonth.Parse(text, null);
    }
}
