﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Introspection;

public abstract class BaseIntrospector
{
    protected record class AttributeInfo<T>(Type Type, T Attribute) { }
    private ITypesProbe Probe { get; }

    private Type[]? types;
    protected Type[] Types { get => types ??= Probe.Locate().ToArray(); }

    protected BaseIntrospector(ITypesProbe probe)
        => Probe = probe;

    protected IEnumerable<AttributeInfo<T>> LocateAttribute<T>() where T : Attribute
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
                    .Select(y => new ParameterInfo(
                                y.Name
                                , !ctorInfos.All(c => c.Parameters.Any(p => p.Name == y.Name))
                                , ctorInfos.First(c => c.Parameters.Any(p => p.Name == y.Name))
                                                .Parameters
                                                .Single(p => p.Name == y.Name)
                                                .Summary
                        )
                    ).Distinct();
}

public class AssemblyTypesProbe : ITypesProbe
{
    public Assembly[] Assemblies { get; } = [typeof(Expression).Assembly];

    public AssemblyTypesProbe()
    { }

    public AssemblyTypesProbe(Assembly[] assemblies)
        => Assemblies = assemblies;

    public virtual IEnumerable<Type> Locate()
        => Assemblies.Aggregate(
                Array.Empty<Type>(), (types, asm)
                => types.Concat(asm.GetTypes().Where(x => x.IsClass && !x.IsAbstract)).ToArray()
            );
}

public interface ITypesProbe
{
    IEnumerable<Type> Locate();
}
