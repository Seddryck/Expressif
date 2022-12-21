using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    abstract class OnTheInstant : BaseDateTimePredicate
    {
        protected override bool EvaluateDate(DateOnly date) => true;
    }

    /// <summary>
    /// Returns `true` if the argument is of type `DateOnly` or of type `DateTime` but the Time part is set at exactly midnight. Returns `false` otherwise.
    /// </summary>
    class OnTheDay : OnTheInstant
    {
        protected override bool EvaluateDateTime(DateTime value) 
            => value.TimeOfDay.Ticks == 0;
    }

    /// <summary>
    /// Returns `true` if the argument is of type `DateTime` and the minutes, seconds and milliseconds are all set at `0`. Returns `false` otherwise.
    /// </summary>
    class OnTheHour : OnTheInstant
    {
        protected override bool EvaluateDateTime(DateTime value)
            => (value.TimeOfDay.Ticks) % (new TimeSpan(1, 0, 0).Ticks) == 0;
    }

    /// <summary>
    /// Returns `true` if the argument is of type `DateTime` and the seconds and milliseconds are all set at `0`. Returns `false` otherwise.
    /// </summary>
    class OnTheMinute : OnTheInstant
    {
        protected override bool EvaluateDateTime(DateTime value)
            => (value.TimeOfDay.Ticks) % (new TimeSpan(0, 1, 0).Ticks) == 0;
    }
}
