using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Temporal
{
    public abstract class BaseTemporalWeekdayFunction : BaseTemporalFunction
    {
        protected override object EvaluateDateTime(DateTime dt)
            => EvaluateDate(DateOnly.FromDateTime(dt));

        protected abstract object EvaluateDate(DateOnly date);
    }

    /// <summary>
    /// Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument.
    /// </summary>
    public class NextWeekday : BaseTemporalWeekdayFunction
    {
        public Func<Weekday> DayOfWeek { get; }

        /// <param name="weekday">The day of week to compare to the argument</param>
        public NextWeekday(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var forward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(forward <= 0 ? forward + 7 : forward);
        }
    }

    /// <summary>
    /// Returns a new date value corresponding to the occurrence of the weekday passed as a parameter following the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.
    /// </summary>
    public class NextWeekdayOrSame : BaseTemporalWeekdayFunction
    {
        public Func<Weekday> DayOfWeek { get; }

        /// <param name="weekday">The day of week to compare to the argument</param>
        public NextWeekdayOrSame(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var forward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(forward < 0 ? forward + 7 : forward);
        }
    }

    /// <summary>
    /// Returns a new dateTime value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument.
    /// </summary>
    public class PreviousWeekday : BaseTemporalWeekdayFunction
    {
        public Func<Weekday> DayOfWeek { get; }

        /// <param name="weekday">The day of week to compare to the argument</param>
        public PreviousWeekday(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var backward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(backward >= 0 ? backward - 7 : backward);
        }
    }

    /// <summary>
    /// Returns a new dateTime value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.
    /// </summary>
    public class PreviousWeekdayOrSame : BaseTemporalWeekdayFunction
    {
        public Func<Weekday> DayOfWeek { get; }

        /// <param name="weekday">The day of week to compare to the argument</param>
        public PreviousWeekdayOrSame(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var backward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(backward > 0 ? backward - 7 : backward);
        }
    }
}
