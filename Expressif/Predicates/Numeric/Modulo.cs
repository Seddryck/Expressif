using Expressif.Predicates.Numeric;
using Expressif.Values;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    internal class Modulo : BaseNumericPredicateReference
    {
        public IScalarResolver<decimal> Remainder { get; }
        public IScalarResolver<decimal> Modulus { get => Reference; }

        public Modulo(IScalarResolver<decimal> modulus)
            : this(modulus, new LiteralScalarResolver<decimal>(0)) { }

        public Modulo(IScalarResolver<decimal> modulus, IScalarResolver<decimal> remainder)
            : base(modulus) { Remainder = remainder; }

        protected override bool EvaluateNumeric(decimal value)
            => value % Reference.Execute() == Remainder.Execute();
    }

    internal class Even : Modulo
    {
        public Even()
            : base(new LiteralScalarResolver<decimal>(2)) { }

    }

    internal class Odd : Modulo
    {
        public Odd()
            : base(new LiteralScalarResolver<decimal>(2), new LiteralScalarResolver<decimal>(1)) { }
    }
}
