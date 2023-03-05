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

        /// <param name="weekday">The day of week to compare to the argument.</param>
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

        /// <param name="weekday">The day of week to compare to the argument.</param>
        public NextWeekdayOrSame(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var forward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(forward < 0 ? forward + 7 : forward);
        }
    }

    /// <summary>
    /// Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument.
    /// </summary>
    public class PreviousWeekday : BaseTemporalWeekdayFunction
    {
        public Func<Weekday> DayOfWeek { get; }

        /// <param name="weekday">The day of week to compare to the argument.</param>
        public PreviousWeekday(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var backward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(backward >= 0 ? backward - 7 : backward);
        }
    }

    /// <summary>
    /// Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.
    /// </summary>
    public class PreviousWeekdayOrSame : BaseTemporalWeekdayFunction
    {
        public Func<Weekday> DayOfWeek { get; }

        /// <param name="weekday">The day of week to compare to the argument.</param>
        public PreviousWeekdayOrSame(Func<Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override object EvaluateDate(DateOnly date)
        {
            var backward = DayOfWeek.Invoke().Index - date.ToWeekday().Index;
            return date.AddDays(backward > 0 ? backward - 7 : backward);
        }
    }

    /// <summary>
    /// Returns a new date value corresponding to the first occurrence of the weekday passed as a parameter of the month of the date passed as the argument.
    /// </summary>
    public class FirstInMonth : NextWeekdayOrSame
    {
        /// <param name="weekday">The day of week to compare to the argument.</param>
        public FirstInMonth(Func<Weekday> weekday)
            : base(weekday) { }

        protected override object EvaluateDate(DateOnly date)
            => base.EvaluateDate(new DateOnly(date.Year, date.Month, 1));
    }

    /// <summary>
    /// Returns a new dateTime value corresponding to the last occurrence of the weekday passed as a parameter of the month of the date passed as the argument.
    /// </summary>
    public class LastInMonth : PreviousWeekdayOrSame
    {
        /// <param name="weekday">The day of week to compare to the argument.</param>
        public LastInMonth(Func<Weekday> weekday)
            : base(weekday) { }

        protected override object EvaluateDate(DateOnly date)
            => base.EvaluateDate(new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1));
    }

    /// <summary>
    /// Returns a new date value corresponding to the date passed as the argument, counting forward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.
    /// </summary>
    [Function(aliases: new[] { "next-business-day", "add-business-days" })]
    public class NextBusinessDays : BaseTemporalWeekdayFunction
    {
        internal Func<int> Count { get; }
        internal Func<object?, bool> IsBusinessDay { get; }

        /// <param name="count">The count of business days to move forward.</param>
        public NextBusinessDays(Func<int> count)
            : base() => (Count, IsBusinessDay) = (count, new Predicates.Temporal.BusinessDay().Evaluate);

        protected override object EvaluateDate(DateOnly date)
            => BasicStrategy(date, 1);

        protected virtual DateOnly BasicStrategy(DateOnly date, int direction)
        {
            // Move backward to previous business day
            while (!IsBusinessDay(date))
                date = date.AddDays(-1 * direction);

            // Move forward to next Business day
            var i = 0;
            var count = Count.Invoke();
            while (i != count)
            {
                date = date.AddDays(direction);
                if (IsBusinessDay(date))
                    i += 1;
            }
            return date;
        }
    }

    /// <summary>
    /// Returns a new date value corresponding to the date passed as the argument, counting backward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.
    /// </summary>
    [Function(aliases: new[] { "previous-business-day", "subtract-business-days" })]
    public class PreviousBusinessDays : NextBusinessDays
    {
        /// <param name="count">The count of business days to move forward.</param>
        public PreviousBusinessDays(Func<int> count)
            : base(count) { }

        protected override object EvaluateDate(DateOnly date)
            => BasicStrategy(date, -1);
    }
}
