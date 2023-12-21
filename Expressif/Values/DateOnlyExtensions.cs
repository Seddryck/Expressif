using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values;

internal static class DateOnlyExtensions
{
    public static Weekday ToWeekday(this DateOnly date)
        => Weekdays.GetByIndex(date.DayOfWeek);
}
