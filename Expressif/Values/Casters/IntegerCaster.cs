using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class IntegerCaster : BaseNumericCaster<int>, ICaster<int>, IParser<int>
    {
        protected override int One { get => 1; }

        protected override int CastNumeric(object numeric)
            => Convert.ToInt32(numeric, CultureInfo.InvariantCulture.NumberFormat);

        public virtual int Cast(object obj)
            => TryCast(obj, out var d)
                ? d
                : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type Integer. The type Integer can only be casted from the underlying numeric types (int, float, ...), Boolean and String. The expect string format can include decimal point, thousand separators, sign symbol and white spaces.");

        public override bool TryParse(string text, [NotNullWhen(true)] out int value)
            => (Result: int.TryParse(text, Style, Format, out var integer), value = integer).Result;

        protected override bool TryNumericCast(object obj, [NotNullWhen(true)] out int value)
        {
            if (TypeChecker.IsNumericType(obj) && Convert.ToDouble(obj) % 1 == 0)
                return (Result: true, value = CastNumeric(obj)).Result;
            return (Result: false, value = default!).Result;
        }
    }
}
