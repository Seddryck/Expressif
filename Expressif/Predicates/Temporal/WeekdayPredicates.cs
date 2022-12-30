﻿using Expressif.Values;
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

    public class Weekday : BaseTemporalWeekdayPredicate
    {
        public Func<Values.Weekday> DayOfWeek { get; }
        public Weekday(Func<Values.Weekday> weekday)
            : base() { DayOfWeek = weekday; }

        protected override bool EvaluateDate(DateOnly date)
            => date.ToWeekday() == DayOfWeek.Invoke();
    }

    public class Weekend : BaseTemporalWeekdayPredicate
    {
        public Weekend() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date.ToWeekday() == Weekdays.Saturday || date.ToWeekday() == Weekdays.Sunday;
    }

    public class BusinessDay : BaseTemporalWeekdayPredicate
    {
        public BusinessDay() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date.ToWeekday() != Weekdays.Saturday && date.ToWeekday() != Weekdays.Sunday;
    }
}
