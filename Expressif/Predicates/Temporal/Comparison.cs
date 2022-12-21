using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    /// <summary>
    /// Returns true if the temporal value passed as argument is equal to the temporal value passed as parameter.
    /// </summary>
    internal class SameInstant : BaseDateTimePredicateReference
    {
        /// <param name="reference">A temporal value to compare to the argument</param>
        public SameInstant(IScalarResolver<DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(DateTime value)
            => value.Equals(Reference.Execute());
    }

    /// <summary>
    /// Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter. Returns `false` otherwise.
    /// </summary>
    internal class After : BaseDateTimePredicateReference
    {
        /// <param name="reference">A temporal value to compare to the argument</param>
        public After(IScalarResolver<DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(DateTime value)
            => value > Reference.Execute();
    }

    /// <summary>
    /// Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.
    /// </summary>
    internal class AfterOrSameInstant : BaseDateTimePredicateReference
    {
        /// <param name="reference">A temporal value to compare to the argument</param>
        public AfterOrSameInstant(IScalarResolver<DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(DateTime value)
            => value >= Reference.Execute();
    }

    /// <summary>
    /// Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise.
    /// </summary>
    internal class Before : BaseDateTimePredicateReference
    {
        /// <param name="reference">A temporal value to compare to the argument</param>
        public Before(IScalarResolver<DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(DateTime value)
            => value < Reference.Execute();
    }

    /// <summary>
    /// Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.
    /// </summary>
    internal class BeforeOrSameInstant : BaseDateTimePredicateReference
    {
        /// <param name="reference">A temporal value to compare to the argument</param>
        public BeforeOrSameInstant(IScalarResolver<DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(DateTime value)
            => value <= Reference.Execute();
    }
}
