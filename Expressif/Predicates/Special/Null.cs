using Expressif.Predicates.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Special
{
    class Null : BasePredicate
    {
        public override bool Evaluate(object? value)
        {
            return value switch
            {
                null => true,
                DBNull => true,
                string text => EvaluateText(text),
                _ => false,
            };
        }

        protected bool EvaluateText(string value)
        {
            if (new Values.Special.Null().Equals(value))
                return true;
            return false;
        }
    }

}
