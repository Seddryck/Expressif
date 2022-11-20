using Expressif.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    class AnyOf : BaseTextPredicate
    {
        public IEnumerable<IScalarResolver<string>> References { get; }
        protected StringComparer Comparer { get; }

        public AnyOf(IEnumerable<IScalarResolver<string>> references)
            : this(references, StringComparer.InvariantCultureIgnoreCase) { }
        public AnyOf(IEnumerable<IScalarResolver<string>> references, StringComparer comparer)
                   => (References, Comparer) = (references, comparer);

        protected override bool EvaluateBaseText(string value)
        {
            foreach (var reference in References)
            {
                var predicate = new EquivalentTo(reference, Comparer);
                if (predicate.Evaluate(value))
                    return true;
            }
            return false;
        }
    }
}
