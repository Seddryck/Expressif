using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Numeric
{
    public abstract class BaseNumericPowerRootFunction : BaseNumericFunction
    {
        public Func<decimal> Exponent { get; }

        public BaseNumericPowerRootFunction(Func<decimal> exponent)
            => Exponent = exponent;
    }

    /// <summary>
    /// Returns the the numeric argument value raised to the power specified by the parameter value.
    /// </summary>
    public class Power : BaseNumericPowerRootFunction
    {
        public Power(Func<decimal> exponent)
            : base(exponent) {}

        protected override decimal? EvaluateNumeric(decimal numeric) 
            => Convert.ToDecimal(Math.Pow(Convert.ToDouble(numeric), Convert.ToDouble(Exponent.Invoke())));
    }

    /// <summary>
    /// Returns the the numeric argument value raised to the square power.
    /// </summary>
    public class SquarePower : Power
    {
        public SquarePower()
            : base(() => 2) { }
    }


    /// <summary>
    /// Returns the the numeric argument value raised to the cube power.
    /// </summary>
    public class CubePower : Power
    {
        public CubePower()
            : base(() => 3) { }
    }

    /// <summary>
    /// Returns the root specified by the parameter value of the numeric argument value.
    /// </summary>
    public class Root : BaseNumericPowerRootFunction
    {
        public Root(Func<decimal> exponent)
            : base(exponent) { }

        protected override decimal? EvaluateNumeric(decimal numeric)
        {
            var exp = Exponent.Invoke();
            if (exp == 0)
                return null;

            var exponent = Convert.ToDouble(exp);
            var value = Convert.ToDouble(numeric);

            if (value >= 0)
                return Convert.ToDecimal(Math.Pow(value, 1.0 / exponent));
            else if (Math.Abs(exponent % 1) <= (double.Epsilon * 100) && exponent % 2 == 1)
                return Convert.ToDecimal(-1 * Math.Pow(Math.Abs(value), 1.0 / Math.Abs(exponent)));
            else
                return null;
        }
    }

    /// <summary>
    /// Returns square root of the numeric argument value
    /// </summary>
    public class SquareRoot : BaseNumericPowerRootFunction
    {
        public SquareRoot()
            : base(() => 2) { }

        protected override decimal? EvaluateNumeric(decimal numeric) 
            => Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(numeric)));
    }


    /// <summary>
    /// Returns cube root of the numeric argument value
    /// </summary>
    public class CubeRoot : BaseNumericPowerRootFunction
    {
        public CubeRoot()
            : base(() => 3) { }

        protected override decimal? EvaluateNumeric(decimal numeric) 
            => Convert.ToDecimal(Math.Cbrt(Convert.ToDouble(numeric)));
    }
}
