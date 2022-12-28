using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Numeric
{
    /// <summary>
    /// Returns true if the numeric argument is equal to 0.
    /// </summary>
    public class Zero : EqualTo
    {
        public Zero()
            : base(() => 0) { }
    }

    /// <summary>
    /// Returns true if the numeric argument is equal to 1.
    /// </summary>
    public class One : EqualTo
    {
        public One()
            : base(() => 1) { }
    }

    /// <summary>
    /// Returns true if the numeric argument is greater than 0.
    /// </summary>
    public class Positive : GreaterThan
    {
        public Positive()
            : base(() => 0) { }
    }

    /// <summary>
    /// Returns true if the numeric argument is greater or equal to 0.
    /// </summary>
    public class PositiveOrZero : GreaterThanOrEqual
    {
        public PositiveOrZero()
            : base(() => 0) { }
    }

    /// <summary>
    /// Returns true if the numeric argument is less than 0.
    /// </summary>
    public class Negative : LessThan
    {
        public Negative()
            : base(() => 0) { }
    }


    /// <summary>
    /// Returns true if the numeric argument is less or equal to 0.
    /// </summary>
    public class NegativeOrZero : LessThanOrEqual
    {
        public NegativeOrZero()
            : base(() => 0) { }
    }
}
