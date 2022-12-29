using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class TypeChecker
    {
        public static bool IsNumericType(object value)
        => value switch
        {
            Byte => true,
            SByte => true,
            Int16 => true,
            Int32 => true,
            Int64 => true,
#if NET7_0_OR_GREATER
            Int128 => true,
#endif
            UInt16 => true,
            UInt32 => true,
            UInt64 => true,
#if NET7_0_OR_GREATER
            UInt128 => true,
            Half => true,
#endif
            Single => true,
            Double => true,
            Decimal => true,
            _ => false
        };
    }
}
