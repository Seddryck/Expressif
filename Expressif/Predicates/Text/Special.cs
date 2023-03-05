using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    /// <summary>
    /// Returns `true` if argument value has a length of `0`. Return `false` otherwise.
    /// </summary>
    public class Empty : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateText(string value) => value.Length == 0;
    }

    /// <summary>
    /// Returns `true` if argument value has a length of `0` or is `null`. Return `false` otherwise.
    /// </summary>
    public class EmptyOrNull : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value) => string.IsNullOrEmpty(value);
    }
}
