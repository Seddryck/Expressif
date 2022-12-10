using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;

namespace Expressif.Functions.Numeric
{
    [Function]
    abstract class AbstractNumericTransformation : IFunction
    {
        
        public AbstractNumericTransformation()
        { }

        public object? Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                DBNull _ => EvaluateNull(),
                decimal numeric => EvaluateNumeric(numeric),
                _ => EvaluateUncasted(value),
            };
        }

        private object? EvaluateUncasted(object value)
        {
            if (new Null().Equals(value) || new Empty().Equals(value) || new Whitespace().Equals(value))
                return EvaluateNull();

            var caster = new NumericCaster();
            var numeric = caster.Execute(value);
            return EvaluateNumeric(numeric);
        }

        protected virtual object? EvaluateNull() => null;
        protected abstract decimal? EvaluateNumeric(decimal numeric);
    }

    [Function(prefix: "")]
    class NullToZero : AbstractNumericTransformation
    {
        protected override object EvaluateNull() => 0;
        protected override decimal? EvaluateNumeric(decimal numeric) => numeric;
    }

    class Ceiling : AbstractNumericTransformation
    {
        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Ceiling(numeric);
    }

    class Floor : AbstractNumericTransformation
    {
        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Floor(numeric);
    }

    class Integer : AbstractNumericTransformation
    {
        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Round(numeric, 0);
    }

    class Round : AbstractNumericTransformation
    {
        public IScalarResolver<int> Digits { get; }

        public Round(IScalarResolver<int> digits)
            => Digits = digits;

        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Round(numeric, Digits.Execute());
    }

    class Clip : AbstractNumericTransformation
    {
        public IScalarResolver<decimal> Min { get; }
        public IScalarResolver<decimal> Max { get; }

        public Clip(IScalarResolver<decimal> min, IScalarResolver<decimal> max)
            => (Min, Max) = (min, max);

        protected override decimal? EvaluateNumeric(decimal numeric) 
            => (numeric < Min.Execute()) ? Min.Execute() : (numeric > Max.Execute()) ? Max.Execute() : numeric;
    }

    abstract class AbstractNumericArithmetic : AbstractNumericTransformation
    {
        public IScalarResolver<decimal> Value { get; }

        public AbstractNumericArithmetic(IScalarResolver<decimal> value)
            => Value = value;
    }

    class Add : AbstractNumericArithmetic
    {
        public IScalarResolver<int> Times { get; }

        public Add(IScalarResolver<decimal> value, IScalarResolver<int> times)
            : base(value) => Times = times;

        public Add(IScalarResolver<decimal> value)
            : this(value, new LiteralScalarResolver<int>(1)) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value + (Value.Execute() * Times.Execute());
    }

    class Subtract : Add
    {
        public Subtract(IScalarResolver<decimal> value, IScalarResolver<int> times)
            : base(value, times) { }

        public Subtract(IScalarResolver<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value - (Value.Execute() * Times.Execute());
    }

    class Increment : Add
    {
        public Increment()
        : base(new LiteralScalarResolver<decimal>(1)) { }
    }

    class Decrement : Subtract
    {
        public Decrement()
        : base(new LiteralScalarResolver<decimal>(1)) { }
    }

    class Multiply : AbstractNumericArithmetic
    {
        public Multiply(IScalarResolver<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value * Value.Execute();
    }

    class Divide : AbstractNumericArithmetic
    {
        public Divide(IScalarResolver<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value / Value.Execute();
    }

    class Invert : AbstractNumericTransformation
    {
        public Invert()
        { }

        protected override decimal? EvaluateNumeric(decimal value) => value==0 ? null : 1/value;
    }
}
