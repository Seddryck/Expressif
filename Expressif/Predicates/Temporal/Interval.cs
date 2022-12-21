using Expressif.Predicates.Numeric;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    /// <summary>
    /// Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.
    /// </summary>
    internal class ContainedIn : BaseDateTimePredicate
    {
        public Interval<DateTime> Interval { get; }

        /// <param name="interval">A temporal interval to compare to the argument</param>
        public ContainedIn(Interval<DateTime> interval)
            => Interval = interval;

        protected override bool EvaluateDateTime(DateTime dt)
            => Interval.Contains(dt);
    }
}
