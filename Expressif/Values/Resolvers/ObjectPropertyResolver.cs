using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Expressif.Values.Resolvers
{
    internal class ObjectPropertyResolver<T> : IScalarResolver<T>
    {
        private string PropertyName { get; }
        private ContextObject Object { get; }

        public ObjectPropertyResolver(string propertyName, ContextObject obj)
            => (PropertyName, Object) = (propertyName, obj);

        public T? Execute() => new Caster().Cast<T>(Object[PropertyName]);
        object? IScalarResolver.Execute() => Execute();
    }
}
