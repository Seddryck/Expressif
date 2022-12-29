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
    public class WithinCurrentWeek : BaseTemporalAroundNowPredicate
    {
        internal WithinCurrentWeek(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(-ThisWeekDay) && date <= Today.AddDays(6 - ThisWeekDay);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the same month than the current date. Returns false otherwise.
    /// </summary>
    public class WithinCurrentMonth : BaseTemporalAroundNowPredicate
    {
        internal WithinCurrentMonth(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, Today.Month, 1) && date <= new DateOnly(Today.Year, Today.Month, 1).AddMonths(1).AddDays(-1);
    }


    /// <summary>
    /// Returns true if the date passed as argument is part of the same year than the current date. Returns false otherwise.
    /// </summary>
    public class WithinCurrentYear : BaseTemporalAroundNowPredicate
    {
        internal WithinCurrentYear(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, 1, 1) && date <= new DateOnly(Today.Year, 1, 1).AddYears(1).AddDays(-1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the week following the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.
    /// </summary>
    public class WithinFollowingWeek : BaseTemporalAroundNowPredicate
    {
        internal WithinFollowingWeek(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(-ThisWeekDay).AddDays(7) && date <= Today.AddDays(6 - ThisWeekDay).AddDays(7);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the month following than the current month. Returns false otherwise.
    /// </summary>
    public class WithinFollowingMonth : BaseTemporalAroundNowPredicate
    {
        internal WithinFollowingMonth(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, Today.Month, 1).AddMonths(1) && date <= new DateOnly(Today.Year, Today.Month, 1).AddMonths(1).AddDays(-1).AddMonths(1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the year following the current year. Returns false otherwise.
    /// </summary>
    public class WithinFollowingYear : BaseTemporalAroundNowPredicate
    {
        internal WithinFollowingYear(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, 1, 1).AddYears(1) && date <= new DateOnly(Today.Year, 1, 1).AddYears(1).AddDays(-1).AddYears(1);
    }


    /// <summary>
    /// Returns true if the date passed as argument is part of the week preceding the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.
    /// </summary>
    public class WithinPrecedingWeek : BaseTemporalAroundNowPredicate
    {
        internal WithinPrecedingWeek(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(-ThisWeekDay).AddDays(-7) && date <= Today.AddDays(6 - ThisWeekDay).AddDays(-7);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the month preceding than the current month. Returns false otherwise.
    /// </summary>
    public class WithinPrecedingMonth : BaseTemporalAroundNowPredicate
    {
        internal WithinPrecedingMonth(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, Today.Month, 1).AddMonths(-1) && date <= new DateOnly(Today.Year, Today.Month, 1).AddDays(-1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is part of the year preceding the current year. Returns false otherwise.
    /// </summary>
    public class WithinPrecedingYear : BaseTemporalAroundNowPredicate
    {
        internal WithinPrecedingYear(DateTime now)
            : base(now) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= new DateOnly(Today.Year, 1, 1).AddYears(-1) && date <= new DateOnly(Today.Year, 1, 1).AddDays(-1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is between tomorrow and the count of days after the current date. Returns false otherwise.
    /// </summary>
    public class WithinNextDays : BaseTemporalAroundNowPredicate
    {
        public Func<int> Count { get; }

        protected internal WithinNextDays(DateTime now, Func<int> count)
            : base(now) { Count = count; }

        /// <param name="count">Count of days to move forward. A value of 1 is equivalent to the predicate `tomorrow` and a value of 0 will return false.</param>
        internal WithinNextDays(Func<int> count)
            : this(DateTime.Now, count) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(1) && date <= Today.AddDays(Count.Invoke());
    }

    /// <summary>
    /// Returns true if the date passed as argument is between the count of days before the current date and yesterday (both included). Returns false otherwise.
    /// </summary>
    public class WithinPreviousDays : BaseTemporalAroundNowPredicate
    {
        public Func<int> Count { get; }

        protected internal WithinPreviousDays(DateTime now, Func<int> count)
            : base(now) { Count = count; }

        /// <param name="count">Count of days to move backward. A value of 1 is equivalent to the predicate `yesterday` and a value of 0 will return false.</param>
        internal WithinPreviousDays(Func<int> count)
            : this(DateTime.Now, count) { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(-1 * Count.Invoke()) && date <= Today.AddDays(-1);
    }
}

