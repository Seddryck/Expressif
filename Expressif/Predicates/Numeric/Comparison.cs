using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    /// <summary>
    /// Returns true if the numeric value passed as argument is equal to the numeric value passed as parameter.
    /// </summary>
    public class EqualTo : BaseNumericPredicateReference
    {
        /// <param name="reference">A numeric value to compare to the argument.</param>
        public EqualTo(Func<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value == Reference.Invoke();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is greater than the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    public class GreaterThan : BaseNumericPredicateReference
    {
        /// <param name="reference">A numeric value to compare to the argument.</param>
        public GreaterThan(Func<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            =>  value > Reference.Invoke();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is greater than or equal to the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    public class GreaterThanOrEqual : EqualTo
    {
        /// <param name="reference">A numeric value to compare to the argument.</param>
        public GreaterThanOrEqual(Func<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value >= Reference.Invoke();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is less than the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    public class LessThan : BaseNumericPredicateReference
    {
        /// <param name="reference">A numeric value to compare to the argument.</param>
        public LessThan(Func<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value < Reference.Invoke();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is less than or equal to the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    public class LessThanOrEqual : EqualTo
    {
        /// <param name="reference">A numeric value to compare to the argument.</param>
        public LessThanOrEqual(Func<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value) 
            => value <= Reference.Invoke();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument additive inverse of the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    public class Opposite : EqualTo
    {
        /// <param name="reference">A numeric value to compare to the argument.</param>
        public Opposite(Func<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value * -1 == Reference.Invoke();
    }
}
