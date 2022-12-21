using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Boolean
{
    /// <summary>
    /// Returns `true` if the boolean passed as argument has the same value than the boolean passed as parameter.
    /// </summary>
    class IdenticalTo : BaseBooleanPredicate
    {
        public IScalarResolver<bool> Reference { get; }

        /// <param name="reference">A boolean value to compare to the argument</param>
        public IdenticalTo(IScalarResolver<bool> reference)
            => Reference = reference;

        protected override bool EvaluateBoolean(bool boolean) => boolean.Equals(Reference.Execute());
    }
}
