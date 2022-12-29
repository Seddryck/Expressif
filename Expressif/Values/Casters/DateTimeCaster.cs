using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class DateTimeCaster : ICaster<DateTime>, IParser<DateTime>
    {
        private readonly DateTimeStyles Style = DateTimeStyles.AllowWhiteSpaces
                                                | DateTimeStyles.NoCurrentDateDefault
                                                | DateTimeStyles.AdjustToUniversal ;

        private readonly DateTimeFormatInfo Format = CultureInfo.InvariantCulture.DateTimeFormat;

        public virtual bool TryCast(object obj, [NotNullWhen(true)] out DateTime value)
            => (obj switch
            {
                YearMonth yearMonth => (Result: true, value = new DateTime(yearMonth.Year, yearMonth.Month, 1)),
                DateTime dt => (Result: true, value = dt),
                DateOnly d => (Result: true, value = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                string s => (Result: TryParse(s, out var dt), value = dt),
                _ => (Result: false, value = default)
            }).Result;

        public virtual DateTime Cast(object obj)
            => TryCast(obj, out var dt)
                ? dt
                : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(DateTime)}. The type {nameof(DateTime)} can only be casted from types YearMonth, DateOnly and String. The expect string format is '{Format.FullDateTimePattern}'");

        public virtual bool TryParse(string text, [NotNullWhen(true)] out DateTime value)
            => (Result: DateTime.TryParse(text, Format, Style, out var result), value=result).Result;

        public virtual DateTime Parse(string text)
            => TryParse(text, out var value) ? value : throw new FormatException();
    }
}
