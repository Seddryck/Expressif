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
    internal class EqualTo : BaseNumericPredicateReference
    {
        /// <param name="reference">A numeric value to compare to the argument</param>
        public EqualTo(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value == Reference.Execute();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is greater than the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    internal class GreaterThan : BaseNumericPredicateReference
    {
        /// <param name="reference">A numeric value to compare to the argument</param>
        public GreaterThan(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            =>  value > Reference.Execute();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is greater than or equal to the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    internal class GreaterThanOrEqual : EqualTo
    {
        /// <param name="reference">A numeric value to compare to the argument</param>
        public GreaterThanOrEqual(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value >= Reference.Execute();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is less than the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    internal class LessThan : BaseNumericPredicateReference
    {
        /// <param name="reference">A numeric value to compare to the argument</param>
        public LessThan(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value < Reference.Execute();
    }

    /// <summary>
    /// Returns true if the numeric value passed as argument is less than or equal to the numeric value passed as parameter. Returns `false` otherwise.
    /// </summary>
    internal class LessThanOrEqual : EqualTo
    {
        /// <param name="reference">A numeric value to compare to the argument</param>
        public LessThanOrEqual(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value) 
            => value <= Reference.Execute();
    }
}
