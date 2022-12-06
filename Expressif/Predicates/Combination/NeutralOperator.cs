using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    internal class NeutralOperator
    {
        public Predication RightMember { get; set; }

        public NeutralOperator(Predication rightMember)
            => RightMember = rightMember;

        public bool Evaluate(bool state, object? value) => true && RightMember.Evaluate(value);
    }
}
