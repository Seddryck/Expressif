using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    /// <summary>
    /// Returns `true` if all characters of the text value passed as argument are lower-case. The value `null`, `empty` and `whitespace` also returns `true`. Returns `false` otherwise. 
    /// </summary>
    class LowerCase : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value)
            => value.Equals(value.ToLowerInvariant(), StringComparison.InvariantCulture);
    }

    /// <summary>
    /// Returns `true` if all characters of the text value passed as argument are upper-case. The value `null`, `empty` and `whitespace` also returns `true`. Returns `false` otherwise. 
    /// </summary>
    class UpperCase : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value)
            => value.Equals(value.ToUpperInvariant(), StringComparison.InvariantCulture);
    }
}
