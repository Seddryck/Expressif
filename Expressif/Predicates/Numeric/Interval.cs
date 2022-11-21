using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    class WithinInterval : BaseNumericPredicate
    {
        public Interval<decimal> Interval { get; }

        public WithinInterval(Interval<decimal> interval) 
            => Interval = interval;

        protected override bool EvaluateNumeric(decimal numeric) 
            => Interval.Contains(numeric);
    }
}
