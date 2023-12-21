using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Expressif.Values.Resolvers;

internal class ObjectIndexResolver<T> : IScalarResolver<T>
{
    private int Index { get; }
    private ContextObject Object { get; }

    public ObjectIndexResolver(int index, ContextObject obj)
        => (Index, Object) = (index, obj);

    public T? Execute() => new Caster().Cast<T>(Object[Index]);
    object? IScalarResolver.Execute() => Execute();
}
