using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    class Null : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value) => false;
    }

    class Empty : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateText(string value) => string.IsNullOrEmpty(value);
    }

    class NullOrEmpty : BaseTextPredicateWithoutReference
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateText(string value) => string.IsNullOrEmpty(value);
    }
}
