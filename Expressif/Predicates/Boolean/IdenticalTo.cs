using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Boolean
{
    class IdenticalTo : BaseBooleanPredicate
    {
        public IScalarResolver<bool> Reference { get; }

        public IdenticalTo(IScalarResolver<bool> reference)
            => Reference = reference;

        protected override bool EvaluateBoolean(bool boolean) => boolean.Equals(Reference.Execute());
    }
}
