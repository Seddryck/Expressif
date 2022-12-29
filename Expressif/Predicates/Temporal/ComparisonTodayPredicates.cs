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
}

