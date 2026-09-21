using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Discovery;

namespace Expressif.Introspection;

public abstract class BaseIntrospector
{
    protected record class AttributeInfo<T>(Type Type, T Attribute) { }
    private ITypeSource Source { get; }

    private Type[]? types;
    protected Type[] Types
        => types ??= Source.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .ToArray();

    protected BaseIntrospector(ITypeSource source)
        => Source = source;

    protected IEnumerable<AttributeInfo<T>> LocateAttribute<T>()
        where T : Attribute
    {
        var types = Types.Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
        return types.Select(x => (Type: x, Attribute: x.GetCustomAttribute<T>() ?? throw new InvalidOperationException()))
                .Select(x => new AttributeInfo<T>
                (
                    x.Type,
                    x.Attribute
                ));
    }

    protected IEnumerable<ParameterInfo> BuildParameters(CtorInfo[] ctorInfos)
        => ctorInfos.SelectMany(x => x.Parameters)
                    .GroupBy(x => x.Name)
                    .Select(parameters =>
                    {
                        var optional = !ctorInfos.All(c => c.Parameters.Any(p => p.Name == parameters.Key));
                        var variadic = parameters.Any(x => x.Variadic);
                        return new ParameterInfo(
                            parameters.Key,
                            string.Join(" | ", parameters.Select(x => x.Type).Distinct().OrderBy(x => x)),
                            optional,
                            variadic,
                            variadic ? parameters.Max(x => x.MinimumCardinality) : optional ? 0 : 1,
                            parameters.First().Summary);
                    });
}
