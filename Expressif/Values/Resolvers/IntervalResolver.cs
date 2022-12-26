using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Resolvers
{
    public class IntervalResolver<T> : IScalarResolver<T> where T : IInterval
    {
        private readonly object value;
        public IntervalResolver(object value)
            => this.value = value;

        public T? Execute() => (T) value;
        object? IScalarResolver.Execute() => Execute();
    }
}
