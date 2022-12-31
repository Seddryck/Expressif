using Sprache;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class WeekdayCaster : ICaster<Weekday>, IParser<Weekday>
    {
        private readonly NumberStyles Style = NumberStyles.None;
        private readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public virtual bool TryCast(object obj, [NotNullWhen(true)] out Weekday? value)
            => obj switch
            {
                Weekday w => (value = w) == value,
                string s => TryParse(s, out value),
                int i => Weekdays.TryGetByIndex(i, out value),
                _ => (value = null) != value
            };

        public virtual Weekday Cast(object obj)
            => TryCast(obj, out var weekday)
                ? weekday
                : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(Weekday)}. The type {nameof(Weekday)} can only be casted from types DateTime, DateOnly, and String. The expect string format is English weekday name or an integer between 0 (Monday) and 6 (Sunday).");

        public virtual bool TryParse(string text, [NotNullWhen(true)] out Weekday? value)
            => Weekdays.TryGetByName(text.Trim().ToLowerInvariant(), out value)
               || (TryParseInteger(text, out var integer)
                    && Weekdays.TryGetByIndex(integer, out value)
                  );

        protected virtual bool TryParseInteger(string text, [NotNullWhen(true)] out int value)
            => int.TryParse(text, Style, Format, out value);

        public virtual Weekday Parse(string text)
            => TryParse(text, out var value) ? value : throw new FormatException();
    }
}
