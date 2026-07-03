using Expressif.Accumulators;
using Expressif.Functions.Introspection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Expressif.Accumulators.Introspection;

public class AccumulatorIntrospector : BaseIntrospector
{
    public AccumulatorIntrospector()
        : this(new AssemblyTypesProbe()) { }
    public AccumulatorIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
    public AccumulatorIntrospector(ITypesProbe probe)
        : base(probe) { }

    public IEnumerable<AccumulatorInfo> Locate()
        => Locate<AccumulatorAttribute>(true);

    public IEnumerable<AccumulatorInfo> Describe()
        => Locate<AccumulatorAttribute>(false);

    protected IEnumerable<AccumulatorInfo> Locate<T>(bool fast) where T : AccumulatorAttribute
    {
        var accumulators = LocateAttribute<AccumulatorAttribute>();

        foreach (var accumulator in accumulators)
        {
            yield return new AccumulatorInfo(
                    accumulator.Type.Name.ToKebabCase().Replace("-accumulator", "")
                    , accumulator.Type.IsPublic
                    , BuildAliases(new(typeof(T), accumulator))
                    , "Array"
                    , accumulator.Type
                    , fast ? "" : accumulator.Type.GetSummary()
                    , fast ? [] : BuildParameters(accumulator.Type.GetInfoConstructors()).ToArray()
                );
        }
    }

    private static string[] BuildAliases((Type Type, AttributeInfo<AccumulatorAttribute> Attribute) accumulator)
    {
        var prefix = accumulator.Attribute.Attribute.Prefix;
        if (prefix == string.Empty)
            return accumulator.Attribute.Attribute.Aliases;

        var implicitAlias = prefix is null
            ? $"{accumulator.Type.Namespace!.Split('.').Last().ToKebabCase()}-to-{accumulator.Type.Name.ToKebabCase()}"
            : $"{prefix}-to-{accumulator.Type.Name.ToKebabCase()}";

        return accumulator.Attribute.Attribute.Aliases.Prepend(implicitAlias).ToArray();
    }
}
