using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    public abstract class BaseTemporalAroundNowPredicate : BaseDateTimePredicate
    {
        protected DateOnly Today { get; }
        protected int ThisWeekDay { get => ((int)Today.DayOfWeek + 6) % 7; }
        public BaseTemporalAroundNowPredicate()
            : this(DateTime.Now) { }

        protected BaseTemporalAroundNowPredicate(DateOnly today)
            => Today = today;

        protected BaseTemporalAroundNowPredicate(DateTime now)
            : this(DateOnly.FromDateTime(now)) { }

        protected override bool EvaluateDateTime(DateTime dt)
            => EvaluateDate(DateOnly.FromDateTime(dt));
    }

    /// <summary>
    /// Returns true if the date passed as argument is representing the next date compared to the current date. Returns false otherwise.
    /// </summary>
    public class Tomorrow : BaseTemporalAroundNowPredicate
    {
        internal Tomorrow(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date == Today.AddDays(1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is representing the current date. Returns false otherwise.
    /// </summary>
    public class Today : BaseTemporalAroundNowPredicate
    {
        internal Today(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date == Today;
    }

    /// <summary>
    /// Returns true if the date passed as argument is representing the previous date compared to the current date. Returns false otherwise.
    /// </summary>
    public class Yesterday : BaseTemporalAroundNowPredicate
    {
        internal Yesterday(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date == Today.AddDays(-1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the same week than the current date. A week is starting on Monday and ending on Sunday. Returns false otherwise.
    /// </summary>
    public class CurrentWeek : BaseTemporalAroundNowPredicate
    {
        internal CurrentWeek(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(-ThisWeekDay) && date <= Today.AddDays(6-ThisWeekDay);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the same month than the current date. Returns false otherwise.
    /// </summary>
    public class CurrentMonth : BaseTemporalAroundNowPredicate
    {
        internal CurrentMonth(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, Today.Month, 1) && date <= new DateOnly(Today.Year, Today.Month, 1).AddMonths(1).AddDays(-1);
    }


    /// <summary>
    /// Returns true if the date passed as argument is part of the same year than the current date. Returns false otherwise.
    /// </summary>
    public class CurrentYear : BaseTemporalAroundNowPredicate
    {
        internal CurrentYear(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, 1, 1) && date <= new DateOnly(Today.Year, 1, 1).AddYears(1).AddDays(-1);
    }
}

