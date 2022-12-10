using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal
{
    internal class SameInstant : BaseDateTimePredicateReference
    {
        public SameInstant(IScalarResolver<System.DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(System.DateTime value)
            => value.Equals(Reference.Execute());
    }

    internal class After : BaseDateTimePredicateReference
    {
        public After(IScalarResolver<System.DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(System.DateTime value)
            => value > Reference.Execute();
    }

    internal class AfterOrSameInstant : BaseDateTimePredicateReference
    {
        public AfterOrSameInstant(IScalarResolver<System.DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(System.DateTime value)
            => value >= Reference.Execute();
    }

    internal class Before : BaseDateTimePredicateReference
    {
        public Before(IScalarResolver<System.DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(System.DateTime value)
            => value < Reference.Execute();
    }

    internal class BeforeOrSameInstant : BaseDateTimePredicateReference
    {
        public BeforeOrSameInstant(IScalarResolver<System.DateTime> reference)
            : base(reference) { }

        protected override bool EvaluateDateTime(System.DateTime value)
            => value <= Reference.Execute();
    }
}
