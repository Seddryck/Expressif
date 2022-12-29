using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Converters
{

    public class DateOnlyConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType switch
            {
                Type _ when sourceType == typeof(string) => true,
                _ => base.CanConvertFrom(context, sourceType),
            };

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str)
                return DateOnly.Parse(str);
            return base.ConvertFrom(context, culture, value) ?? throw new NotSupportedException();
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? sourceType)
            => sourceType switch
            {
                Type _ when sourceType == typeof(string) => true,
                _ => base.CanConvertTo(context, sourceType),
            };

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (CanConvertTo(destinationType) && value is DateOnly date)
                    return date.ToString("O");
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}