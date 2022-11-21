using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    class Empty : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateText(string value) => string.IsNullOrEmpty(value);
    }

    class EmptyOrNull : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value) => string.IsNullOrEmpty(value);
    }
}
