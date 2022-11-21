using Expressif.Predicates.Boolean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Boolean
{
    class True : BaseBooleanPredicate
    {
        protected override bool EvaluateBoolean(bool boolean) => boolean;
    }

    class TrueOrNull : True
    {
        protected override bool EvaluateNull() => true;
    }

    class False : BaseBooleanPredicate
    {
        protected override bool EvaluateBoolean(bool boolean) => !boolean;
    }

    class FalseOrNull : False
    {
        protected override bool EvaluateNull() => true;
    }
}
