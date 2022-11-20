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
    internal class EqualTo : BaseNumericPredicateReference
    {
        public EqualTo(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value == Reference.Execute();
    }

    internal class GreaterThan : BaseNumericPredicateReference
    {
        public GreaterThan(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            =>  value > Reference.Execute();
    }

    internal class GreaterThanOrEqual : EqualTo
    {
        public GreaterThanOrEqual(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value >= Reference.Execute();
    }

    internal class LessThan : BaseNumericPredicateReference
    {
        public LessThan(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value)
            => value < Reference.Execute();
    }

    internal class LessThanOrEqual : EqualTo
    {
        public LessThanOrEqual(IScalarResolver<decimal> reference)
            : base(reference) { }

        protected override bool EvaluateNumeric(decimal value) 
            => value <= Reference.Execute();
    }
}
