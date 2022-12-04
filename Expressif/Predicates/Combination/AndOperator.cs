using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    internal class AndOperator
    {
        public Predication RightMember { get; set; }

        public AndOperator(Predication rightMember)
            => RightMember = rightMember;

        public bool Evaluate(bool state, object? value) => 
            state && state && RightMember.Evaluate(value);
    }
}
