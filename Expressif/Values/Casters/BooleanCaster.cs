using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class BooleanCaster
    {
        public bool Execute(object value)
        {
            if (value is bool boolean)
                return boolean;

            if (TryParseNumeric(value, out decimal? numeric))
                return Cast(numeric ?? throw new ArgumentNullException(nameof(value)));

            return Cast(value.ToString());
        }

        protected bool TryParseNumeric(object value, [NotNullWhen(true)] out decimal? numeric)
        {
            var caster = new NumericCaster();
            if (caster.IsNumericType(value))
                numeric = caster.Execute(value);
            else
                numeric = null;
            return numeric is not null;
        }


        protected virtual bool Cast(decimal numeric) => Convert.ToBoolean(numeric);

        protected virtual bool Cast(string? str)
            => str?.Trim().ToLowerInvariant() switch
                {
                    "true" => true,
                    "yes" => true,
                    "1" => true,
                    _ => false,
                };
    }
}
