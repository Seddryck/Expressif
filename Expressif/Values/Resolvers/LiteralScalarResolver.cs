using System;
using System.ComponentModel;

namespace Expressif.Values.Resolvers
{
    internal class LiteralScalarResolver<T> : IScalarResolver<T>
    {
        private readonly object value;

        public LiteralScalarResolver(object value)
            => this.value = value;

        public T? Execute()
            => ConvertValue(value);

        object? IScalarResolver.Execute()
            => Execute();

        protected virtual T? ConvertValue(object value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter.CanConvertFrom(value.GetType()))
                try
                { return (T?)converter.ConvertFrom(null, System.Globalization.CultureInfo.InvariantCulture, value); }
                catch (Exception)
                { throw new ArgumentException($"Cannot convert the value '{value}' to a '{typeof(T).Name}'"); }
            else
                try
                { return (T)Convert.ChangeType(value, typeof(T)); }
                catch (Exception)
                { throw new ArgumentException($"Cannot convert the value '{value}' to a '{typeof(T).Name}'"); }
        }
    }
}
