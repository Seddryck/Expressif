using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif.Values.Casters
{
    public abstract class BaseNumericCaster<T>
    {
        protected readonly NumberStyles Style = NumberStyles.Number;
        protected readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;
        protected abstract T One { get; }
        public virtual bool TryCast(object obj, [NotNullWhen(true)] out T value)
        {
            if (TryNumericCast(obj, out var num))
                return (Result: true, value = num).Result;

            return obj switch
            {
                bool b => (Result: true, value = (b ? One : default)!).Result,
                string str => (Result: TryParse(str, out var valueStr), value = valueStr).Result,
                _ => (Result: false, value = default!).Result
            };
        }

        protected abstract bool TryNumericCast(object obj, [NotNullWhen(true)] out T value);

        protected abstract T CastNumeric(object numeric);

        public abstract bool TryParse(string text, [NotNullWhen(true)] out T value);

        public virtual T Parse(string text)
            => TryParse(text, out var value) ? value : throw new FormatException();

    }

    public class NumericCaster : BaseNumericCaster<decimal>, ICaster<decimal>, IParser<decimal>
    {
        protected override decimal One { get => 1m; }

        protected override bool TryNumericCast(object obj, [NotNullWhen(true)] out decimal value)
        {
            if (TypeChecker.IsNumericType(obj))
                return (Result: true, value = CastNumeric(obj)).Result;
            return (Result: false, value = default!).Result;
        }

        protected override decimal CastNumeric(object numeric)
            => Convert.ToDecimal(numeric, CultureInfo.InvariantCulture.NumberFormat);

        public virtual decimal Cast(object obj)
            => TryCast(obj, out var d)
                ? d
                : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type Numeric. The type Numeric can only be casted from the underlying numeric types (int, float, ...), Boolean and String. The expect string format can include decimal point, thousand separators, sign symbol and white spaces.");

        public override bool TryParse(string text, [NotNullWhen(true)] out decimal value)
            => (Result: decimal.TryParse(text, Style, Format, out var decimalStr), value = decimalStr).Result;
    }
}