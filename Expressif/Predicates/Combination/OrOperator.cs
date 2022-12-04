using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    internal class OrOperator
    {
        public Predication RightMember { get; set; }

        public OrOperator(Predication rightMember)
            => RightMember = rightMember;

        public bool Evaluate(bool state, object? value) 
            => state || state || RightMember.Evaluate(value);
    }
}
