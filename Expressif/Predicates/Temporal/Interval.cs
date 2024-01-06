using Expressif.Predicates.Numeric;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal;

/// <summary>
/// Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.
/// </summary>
public class ContainedIn : BaseDateTimePredicate
{
    public Func<Interval<DateTime>> Interval { get; }

    /// <param name="interval">A temporal interval to compare to the argument.</param>
    public ContainedIn(Func<Interval<DateTime>> interval)
        => Interval = interval;

    protected override bool EvaluateDateTime(DateTime dt)
        => Interval.Invoke().Contains(dt);
}
