using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    internal class XorOperator
    {
        public Predication RightMember { get; set; }

        public XorOperator(Predication rightMember)
            => RightMember = rightMember;

        public bool Evaluate(bool state, object? value) => state ^ RightMember.Evaluate(value);
    }
}
