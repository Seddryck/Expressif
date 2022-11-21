using Expressif.Predicates.Numeric;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.DateTime
{
    internal class ContainedIn : BaseDateTimePredicate
    {
        public Interval<System.DateTime> Interval { get; }

        public ContainedIn(Interval<System.DateTime> interval)
            => Interval = interval;

        protected override bool EvaluateDateTime(System.DateTime dt)
            => Interval.Contains(dt);
    }
}
