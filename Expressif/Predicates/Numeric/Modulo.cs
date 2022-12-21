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
    /// <summary>
    /// Returns `true` if the division of the numeric value passed as argument by the modulus provided as parameter value is equal to the remainder provided as a second parameter. If no remainder is provided then 0 is expected. Returns `false` otherwise.
    /// </summary>
    internal class Modulo : BaseNumericPredicateReference
    {
        public IScalarResolver<decimal> Remainder { get; }
        public IScalarResolver<decimal> Modulus { get => Reference; }

        /// <param name="modulus">An integer value used as the modulus.</param>
        public Modulo(IScalarResolver<decimal> modulus)
            : this(modulus, new LiteralScalarResolver<decimal>(0)) { }

        /// <param name="modulus">An integer value used as the modulus.</param>
        /// <param name="remainder">An integer value defined as the expected reminder.</param>
        public Modulo(IScalarResolver<decimal> modulus, IScalarResolver<decimal> remainder)
            : base(modulus) { Remainder = remainder; }

        protected override bool EvaluateNumeric(decimal value)
            => value % Modulus.Execute() == Remainder.Execute();
    }

    /// <summary>
    /// Returns `true` if the numeric value passed as argument is even. Returns `false` otherwise.
    /// </summary>
    internal class Even : Modulo
    {
        public Even()
            : base(new LiteralScalarResolver<decimal>(2)) { }

    }

    /// <summary>
    /// Returns `true` if the numeric value passed as argument is odd. Returns `false` otherwise.
    /// </summary>
    internal class Odd : Modulo
    {
        public Odd()
            : base(new LiteralScalarResolver<decimal>(2), new LiteralScalarResolver<decimal>(1)) { }
    }
}
