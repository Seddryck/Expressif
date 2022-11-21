using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.DateTime
{
    class OnTheDay : BaseDateTimePredicate
    {
        protected override bool EvaluateDateTime(System.DateTime value) 
            => value.TimeOfDay.Ticks == 0;
    }

    class OnTheHour : BaseDateTimePredicate
    {
        protected override bool EvaluateDateTime(System.DateTime value)
            => (value.TimeOfDay.Ticks) % (new TimeSpan(1, 0, 0).Ticks) == 0;
    }

    class OnTheMinute : BaseDateTimePredicate
    {
        protected override bool EvaluateDateTime(System.DateTime value)
            => (value.TimeOfDay.Ticks) % (new TimeSpan(0, 1, 0).Ticks) == 0;
    }
}
