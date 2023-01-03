using Expressif.Values.Casters;
using Expressif.Values.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values
{
    [TypeConverter(typeof(WeekdayConverter))]
    public record Weekday(int Index, string Name)
#if NET7_0_OR_GREATER
        : IParsable<Weekday>
#endif
    {

        private static readonly NumberStyles Style = NumberStyles.None;
        private static readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public static bool TryParse(string? text, IFormatProvider? provider, [NotNullWhen(true)] out Weekday? value)
            => Weekdays.TryGetByName(string.IsNullOrEmpty(text) ? string.Empty : text.Trim().ToLowerInvariant(), out value)
               || (TryParseInteger(text, out var integer)
                    && Weekdays.TryGetByIndex(integer, out value)
                  );

        private static bool TryParseInteger(string? text, [NotNullWhen(true)] out int value)
            => int.TryParse(text, Style, Format, out value);

        public static Weekday Parse(string text, IFormatProvider? provider)
            => TryParse(text, provider, out var value) ? value : throw new FormatException();
    }

    public static class Weekdays
    {
        internal static Weekday GetByIndex(DayOfWeek dayOfWeek)
            => TryGetByIndex(((int)dayOfWeek + 6) % 7, out var value) ? value : throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
        internal static bool TryGetByIndex(int index, [NotNullWhen(true)] out Weekday? value)
            => (index >= 0 && index <= 6) 
                ? (value = weekdays[index]) is not null 
                : (value = null) is not null;

        internal static bool TryGetByName(string weekdayName, [NotNullWhen(true)] out Weekday? value)
            => (value = weekdays.FirstOrDefault(
                    x => StringComparer.InvariantCultureIgnoreCase.Equals(x.Name.Trim(), weekdayName)
                )) is not null;

        private static readonly List<Weekday> weekdays;

        static Weekdays()
        {
            var dayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
            weekdays= new List<Weekday>(dayNames.Length);
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                weekdays.Add(new Weekday(
                    ((int)dayOfWeek + 6) % 7
                    , dayNames.Single(x =>
                        StringComparer.InvariantCultureIgnoreCase.Equals(x
                            , Enum.GetName(typeof(DayOfWeek), dayOfWeek)!
                        )
                    )
                ));
            }
            weekdays = weekdays.OrderBy(x => x.Index).ToList();
        }

        public static Weekday Monday { get => weekdays[0]; }
        public static Weekday Tuesday { get => weekdays[1]; }
        public static Weekday Wednesday { get => weekdays[2]; }
        public static Weekday Thursday { get => weekdays[3]; }
        public static Weekday Friday { get => weekdays[4]; }
        public static Weekday Saturday { get => weekdays[5]; }
        public static Weekday Sunday { get => weekdays[6]; }

    }

}
