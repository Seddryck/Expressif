using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Resolvers
{

    public class LiteralScalarResolverTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            => value switch
            {
                string v => new LiteralScalarResolver<string>(v),
                int i => new LiteralScalarResolver<int>(i),
                decimal d => new LiteralScalarResolver<decimal>(d),
                DateTime dateTime => new LiteralScalarResolver<DateTime>(dateTime),
                bool b => new LiteralScalarResolver<bool>(b),
                _ => base.ConvertFrom(context, culture, value)
            };
    }
}
