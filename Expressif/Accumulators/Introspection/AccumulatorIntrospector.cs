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
                    , accumulator.Attribute.Prefix != null && string.IsNullOrEmpty(accumulator.Attribute.Prefix)
                        ? accumulator.Attribute.Aliases
                        : accumulator.Attribute.Aliases.AsQueryable()
                            .Prepend(string.IsNullOrEmpty(accumulator.Attribute.Prefix)
                                ? $"{accumulator.Type.Namespace!.Split('.').Last().ToKebabCase()}-to-{accumulator.Type.Name.ToKebabCase()}"
                                : $"{accumulator.Attribute.Prefix}-to-{accumulator.Type.Name.ToKebabCase()}"
                            ).Where(x => !string.IsNullOrEmpty(x)).ToArray()
                    , "Array"
                    , accumulator.Type
                    , fast ? "" : accumulator.Type.GetSummary()
                    , fast ? [] : BuildParameters(accumulator.Type.GetInfoConstructors()).ToArray()
                );
        }
    }
}
