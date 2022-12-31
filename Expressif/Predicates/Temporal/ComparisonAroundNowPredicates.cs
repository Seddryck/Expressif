using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{

    public abstract class BaseTemporalAroundNowPredicate : BaseTemporalAroundTodayPredicate
    {
        protected DateTime Now { get; }
        protected BaseTemporalAroundNowPredicate()
            : this(DateTime.Now) { }

        protected BaseTemporalAroundNowPredicate(DateTime now)
            : base(DateOnly.FromDateTime(now)) { Now = now; }
    }

    /// <summary>
    /// Returns true if the date passed as argument is after today. Returns false otherwise.
    /// </summary>
    public class InTheFuture : BaseTemporalAroundTodayPredicate
    {
        protected internal InTheFuture(DateTime now) : base(now) { }

        public InTheFuture() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(1);
    }

    /// <summary>
    /// Returns true if the date passed as argument is today or a date after. If a DateTime is passed as argument, it must be today or after. Returns false otherwise.
    /// </summary>
    public class InTheFutureOrToday : BaseTemporalAroundTodayPredicate
    {
        protected internal InTheFutureOrToday(DateTime now) : base(now) { }

        public InTheFutureOrToday() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today;
    }

    /// <summary>
    /// Returns true if the dateTime passed as argument is after now. If a Date is passed as argument, it returns true if the date is today or after. Returns false otherwise.
    /// </summary>
    public class InTheFutureOrNow : BaseTemporalAroundNowPredicate
    {
        protected internal InTheFutureOrNow(DateTime now) : base(now) { }

        public InTheFutureOrNow() : base() { }

        protected override bool EvaluateDateTime(DateTime dt)
            => dt >= Now;

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today;
    }

    /// <summary>
    /// Returns true if the date passed as argument is before today. Returns false otherwise.
    /// </summary>
    public class InThePast : BaseTemporalAroundTodayPredicate
    {
        protected internal InThePast(DateTime now) : base(now) { }

        public InThePast() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date < Today;
    }

    /// <summary>
    /// Returns true if the date passed as argument is today or a date before. If a DateTime is passed as argument, it returns true if the date of this datetime is today or any other date before today. Returns false otherwise.
    /// </summary>
    public class InThePastOrToday : BaseTemporalAroundTodayPredicate
    {
        protected internal InThePastOrToday(DateTime now) : base(now) { }

        public InThePastOrToday() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date <= Today;
    }

    /// <summary>
    /// Returns true if the dateTime passed as argument is before now. If a Date is passed as argument, it returns true if the date is today or before. Returns false otherwise.
    /// </summary>
    public class InThePastOrNow : BaseTemporalAroundNowPredicate
    {
        protected internal InThePastOrNow(DateTime now) : base(now) { }

        public InThePastOrNow() : base() { }

        protected override bool EvaluateDateTime(DateTime dt)
            => dt <= Now;

        protected override bool EvaluateDate(DateOnly date)
            => date <= Today;
    }
}
