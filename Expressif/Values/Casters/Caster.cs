using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class Caster
    {
        public T? Cast<T>(object? value)
        {
            if (value == null)
                return default;

            var @switch = new Dictionary<Type, Func<object>> {
                { typeof(decimal), () => new NumericCaster().Cast(value) },
                { typeof(DateTime), () => new DateTimeCaster().Cast(value) },
                { typeof(string), () => new TextCaster().Cast(value) },
            };

            if (@switch.TryGetValue(typeof(T), out var cast))
                return (T?)cast.Invoke();
            else
                try
                { return (T)Convert.ChangeType(value, typeof(T)); }
                catch (Exception)
                { throw new ArgumentException($"Cannot convert the value '{value}' from a type '{value.GetType().Name}' to a '{typeof(T).Name}'"); }
        }
    }
}
