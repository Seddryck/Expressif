﻿using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Combination
{
    internal class AndOperator : ICombinationOperator
    {
        public AndOperator() { }

        public bool Evaluate(IPredicate leftMember, IPredicate rightMember, object? value) 
            => leftMember.Evaluate(value) && rightMember.Evaluate(value);
    }
}
