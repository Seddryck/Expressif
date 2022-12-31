using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    public abstract class BaseTemporalWeekdayPredicate : BaseDateTimePredicate
    {
        protected override bool EvaluateDateTime(DateTime dt)
            => EvaluateDate(DateOnly.FromDateTime(dt));
    }

    /// <summary>
    /// Returns `true` if the date passed as the argument corresponds to the weekday passed as the parameter. Returns `false` otherwise.
    /// </summary>
    public class Weekday : BaseTemporalWeekdayPredicate
    {
        public Func<Values.Weekday> DayOfWeek { get; }
        
        /// <param name="weekday">The day of week to compare to the argument</param>
        public Weekday(Func<Values.Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override bool EvaluateDate(DateOnly date)
            => date.ToWeekday() == DayOfWeek.Invoke();
    }

    /// <summary>
    /// Returns `true` if the date passed as the argument corresponds to a Saturday or a Sunday. Returns `false` otherwise.
    /// </summary>
    public class Weekend : BaseTemporalWeekdayPredicate
    {
        public Weekend() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date.ToWeekday() == Weekdays.Saturday || date.ToWeekday() == Weekdays.Sunday;
    }

    /// <summary>
    /// Returns `true` if the date passed as the argument doesn't correspond to a Saturday or a Sunday. Returns `false` otherwise.
    /// </summary>
    public class BusinessDay : BaseTemporalWeekdayPredicate
    {
        public BusinessDay() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date.ToWeekday() != Weekdays.Saturday && date.ToWeekday() != Weekdays.Sunday;
    }
}
