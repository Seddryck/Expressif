using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Numeric
{
    public abstract class BaseNumericArithmetic : BaseNumericFunction
    {
        public Func<decimal> Value { get; }

        public BaseNumericArithmetic(Func<decimal> value)
            => Value = value;
    }

    /// <summary>
    /// Returns the sum of an argument number and the parameter value.
    /// </summary>
    public class Add : BaseNumericArithmetic
    {
        public Func<int> Times { get; }

        /// <param name="value">The value to be added to the argument value.</param>
        /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the sum.</param>
        public Add(Func<decimal> value, Func<int> times)
            : base(value) => Times = times;

        public Add(Func<decimal> value)
            : this(value, () => 1) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value + (Value.Invoke() * Times.Invoke());
    }

    /// <summary>
    /// Returns the difference between the argument number and the parameter value.
    /// </summary>
    public class Subtract : Add
    {
        /// <param name="value">The value to be subtracted to the argument value.</param>
        /// <param name="times">An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction.</param>
        public Subtract(Func<decimal> value, Func<int> times)
            : base(value, times) { }

        public Subtract(Func<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value - (Value.Invoke() * Times.Invoke());
    }

    /// <summary>
    /// Returns the argument number incremented of one unit.
    /// </summary>
    public class Increment : Add
    {
        public Increment()
        : base(() => 1) { }
    }

    /// <summary>
    /// Returns the argument number decremented of one unit.
    /// </summary>
    public class Decrement : Subtract
    {
        public Decrement()
        : base(() => 1) { }
    }

    /// <summary>
    /// Returns the argument number multiplied by the parameter value.
    /// </summary>
    public class Multiply : BaseNumericArithmetic
    {
        /// <param name="value">The value to be multiplied by the argument value.</param>
        public Multiply(Func<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => value * Value.Invoke();
    }

    /// <summary>
    /// Returns the argument number divided by the parameter value. If the parameter value is `0`, it returns `null`.
    /// </summary>
    public class Divide : BaseNumericArithmetic
    {
        /// <param name="value">The value to divide the argument value.</param>
        public Divide(Func<decimal> value)
            : base(value) { }

        protected override decimal? EvaluateNumeric(decimal value)
            => Value.Invoke() == 0 ? null : value / Value.Invoke();
    }
}
