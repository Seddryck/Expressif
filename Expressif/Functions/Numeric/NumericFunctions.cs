using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;

namespace Expressif.Functions.Numeric
{
    [Function]
    abstract class BaseNumericFunction : IFunction
    {
        
        public BaseNumericFunction()
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

    /// <summary>
    /// Returns the unmodified argument value except if the argument value is `null`, `empty` or `whitespace` then it returns `0`.
    /// </summary>
    [Function(prefix: "")]
    class NullToZero : BaseNumericFunction
    {
        protected override object EvaluateNull() => 0;
        protected override decimal? EvaluateNumeric(decimal numeric) => numeric;
    }

    abstract class BaseNumericRounding : BaseNumericFunction
    { }

    /// <summary>
    /// Returns the smallest integer greater than or equal to the argument number.
    /// </summary>
    class Ceiling : BaseNumericRounding
    {
        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Ceiling(numeric);
    }

    /// <summary>
    /// Returns the largest integer less than or equal to the argument number.
    /// </summary>
    class Floor : BaseNumericRounding
    {
        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Floor(numeric);
    }

    /// <summary>
    /// Returns the value of an argument number rounded to the nearest integer. 
    /// </summary>
    class Integer : BaseNumericRounding
    {
        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Round(numeric, 0);
    }

    /// <summary>
    /// Returns the value of an argument number to the specified number of fractional digits.
    /// </summary>
    class Round : BaseNumericRounding
    {
        public IScalarResolver<int> Digits { get; }

        /// <param name="digits">An integer between 0 and +Infinity, indicating the number of fractional digits in the return value.</param>
        public Round(IScalarResolver<int> digits)
            => Digits = digits;

        protected override decimal? EvaluateNumeric(decimal numeric) => Math.Round(numeric, Digits.Execute());
    }

    /// <summary>
    /// Returns the value of an argument number, unless it is smaller than min, in which case it returns min, or greater than max, in which case it returns max.
    /// </summary>
    class Clip : BaseNumericFunction
    {
        public IScalarResolver<decimal> Min { get; }
        public IScalarResolver<decimal> Max { get; }

        /// <param name="min">value returned in case the argument value is smaller than it</param>
        /// <param name="max">value returned in case the argument value is greater than it</param>
        public Clip(IScalarResolver<decimal> min, IScalarResolver<decimal> max)
            => (Min, Max) = (min, max);

        protected override decimal? EvaluateNumeric(decimal numeric) 
            => (numeric < Min.Execute()) ? Min.Execute() : (numeric > Max.Execute()) ? Max.Execute() : numeric;
    }

    abstract class BaseNumericArithmetic : BaseNumericFunction
    {
        public IScalarResolver<decimal> Value { get; }

        public BaseNumericArithmetic(IScalarResolver<decimal> value)
            => Value = value;
    }

    /// <summary>
    /// Returns the sum of an argument number and the parameter value.
    /// </summary>
    class Add : BaseNumericArithmetic
    {
        public IScalarResolver<int> Times { get; }

        /// <param name="value">The value to be added to the argument value</param>
        /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the sum</param>
        public Add(IScalarResolver<decimal> value, IScalarResolver<int> times)
            : base(value) => Times = times;

        public Add(IScalarResolver<decimal> value)
            : this(value, new LiteralScalarResolver<int>(1)) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value + (Value.Execute() * Times.Execute());
    }

    /// <summary>
    /// Returns the difference between the argument number and the parameter value.
    /// </summary>
    class Subtract : Add
    {
        /// <param name="value">The value to be subtracted to the argument value</param>
        /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction</param>
        public Subtract(IScalarResolver<decimal> value, IScalarResolver<int> times)
            : base(value, times) { }

        public Subtract(IScalarResolver<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value - (Value.Execute() * Times.Execute());
    }

    /// <summary>
    /// Returns the argument number incremented of one unit.
    /// </summary>
    class Increment : Add
    {
        public Increment()
        : base(new LiteralScalarResolver<decimal>(1)) { }
    }

    /// <summary>
    /// Returns the argument number decremented of one unit.
    /// </summary>
    class Decrement : Subtract
    {
        public Decrement()
        : base(new LiteralScalarResolver<decimal>(1)) { }
    }

    /// <summary>
    /// Returns the argument number multiplied by the parameter value.
    /// </summary>
    class Multiply : BaseNumericArithmetic
    {
        /// <param name="value">The value to be multiplied by the argument value</param>
        public Multiply(IScalarResolver<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value * Value.Execute();
    }

    /// <summary>
    /// Returns the argument number divided by the parameter value. If the parameter value is `0`, it returns `null`.
    /// </summary>
    class Divide : BaseNumericArithmetic
    {
        /// <param name="value">The value to divide the argument value</param>
        public Divide(IScalarResolver<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => Value.Execute()==0 ? null : value / Value.Execute();
    }

    /// <summary>
    /// Returns the reciprocal of the argument number, meaning the result of the division of 1 by the argument number. If the argument value is `0`, it returns `null`.
    /// </summary>
    class Invert : BaseNumericFunction
    {
        public Invert()
        { }

        protected override decimal? EvaluateNumeric(decimal value) => value==0 ? null : 1/value;
    }
}
