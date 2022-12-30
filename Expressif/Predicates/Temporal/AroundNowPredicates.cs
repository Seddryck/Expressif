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
    public class InTheFuture : BaseTemporalAroundNowPredicate
    {
        protected internal InTheFuture(DateTime now) : base(now) { }

        public InTheFuture() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today.AddDays(1);
        protected override bool EvaluateDateTime(DateTime dt)
            => dt > Now;
    }

    /// <summary>
    /// Returns true if the date passed as argument is today or a date after. If a DateTime is passed as argument, it must be after now. Returns false otherwise.
    /// </summary>
    public class InTheFutureOrToday : BaseTemporalAroundNowPredicate
    {
        protected internal InTheFutureOrToday(DateTime now) : base(now) { }

        public InTheFutureOrToday() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date >= Today;
        protected override bool EvaluateDateTime(DateTime dt)
            => dt >= Today.ToDateTime(new TimeOnly(0, 0, 0));
    }

    /// <summary>
    /// Returns true if the date passed as argument is before today. Returns false otherwise.
    /// </summary>
    public class InThePast : BaseTemporalAroundNowPredicate
    {
        protected internal InThePast(DateTime now) : base(now) { }

        public InThePast() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date < Today;
        protected override bool EvaluateDateTime(DateTime dt)
            => dt < Now;
    }

    /// <summary>
    /// Returns true if the date passed as argument is today or a date before. If a DateTime is passed as argument, it must be after now. Returns false otherwise.
    /// </summary>
    public class InThePastOrToday : BaseTemporalAroundNowPredicate
    {
        protected internal InThePastOrToday(DateTime now) : base(now) { }

        public InThePastOrToday() : base() { }

        protected override bool EvaluateDate(DateOnly date)
            => date <= Today;
        protected override bool EvaluateDateTime(DateTime dt)
            => dt < Today.AddDays(1).ToDateTime(new TimeOnly(0, 0, 0));
    }
}
