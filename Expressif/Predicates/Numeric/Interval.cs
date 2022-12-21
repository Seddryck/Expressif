using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    /// <summary>
    /// Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.
    /// </summary>
    class WithinInterval : BaseNumericPredicate
    {
        public Interval<decimal> Interval { get; }

        /// <param name="interval">A numeric interval to compare to the argument</param>
        public WithinInterval(Interval<decimal> interval) 
            => Interval = interval;

        protected override bool EvaluateNumeric(decimal numeric) 
            => Interval.Contains(numeric);
    }
}
