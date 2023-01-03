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

        public bool TryParse(string text, [NotNullWhen(true)] out Weekday? value)
            => Weekday.TryParse(text, null, out value);

        public Weekday Parse(string text)
            => Weekday.Parse(text, null);
    }
}
