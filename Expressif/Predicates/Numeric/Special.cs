﻿using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    class Null : BaseNumericPredicate
    {
        protected override bool EvaluateNull() => true;
        protected override bool EvaluateNumeric(decimal value) => false;
    }

    class Integer : BaseNumericPredicate
    {
        protected override bool EvaluateNumeric(decimal value) => value % 1 == 0;
    }
}
