using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    class LowerCase : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value)
            => value.Equals(value.ToLowerInvariant(), StringComparison.InvariantCulture);
    }

    class UpperCase : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value)
            => value.Equals(value.ToUpperInvariant(), StringComparison.InvariantCulture);
    }
}
