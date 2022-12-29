using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class TextCaster : ICaster<string>, IParser<string>
    {
        protected static readonly string DateFormat = "yyyy-MM-dd";
        protected static readonly string DateTimeFormat = $"{DateFormat} HH:mm:ss";
        protected static readonly NumberFormatInfo NumberFormat = CultureInfo.InvariantCulture.NumberFormat;

        public virtual bool TryCast(object obj, [NotNullWhen(true)] out string? value)
        {
            if (TypeChecker.IsNumericType(obj))
                return (Result: true, value = Convert.ToDecimal(obj).ToString(NumberFormat)).Result;

            return (obj switch
            {
                //YearMonth yearMonth => (Result: true, value = yearMonth.ToString()),
                DateTime dt => (Result: true, value = dt.ToString(DateTimeFormat)),
                DateOnly d => (Result: true, value = d.ToString(DateFormat)),
                string s => (Result: true, value = s),
                object o => (Result: true, value = o.ToString()),
                _ => (Result: false, value = null)
            }).Result;
        }

        public virtual string Cast(object obj)
            => TryCast(obj, out var yearMonth)
                ? yearMonth
                : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type 'Text'. The type 'Text' can only be casted from types DateTime, DateOnly, Underlying numeric types, boolean and YearMonth.");

        public virtual bool TryParse(string text, [NotNullWhen(true)] out string? value)
            => (Result: true, value = text).Result;

        public virtual string Parse(string text)
            => text;
    }
}
