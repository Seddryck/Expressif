using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Introspection;

public class FunctionIntrospector : BaseIntrospector
{
    public FunctionIntrospector()
        : this(new AssemblyTypesProbe()) { }
    public FunctionIntrospector(Assembly[] assemblies)
        : this(new AssemblyTypesProbe(assemblies.Distinct().ToArray())) { }
    public FunctionIntrospector(ITypesProbe probe)
        : base(probe) { }

    public IEnumerable<FunctionInfo> Locate()
        => Locate<FunctionAttribute>(true);

    public IEnumerable<FunctionInfo> Describe()
        => Locate<FunctionAttribute>(false);

    protected IEnumerable<FunctionInfo> Locate<T>(bool fast) where T : FunctionAttribute
    {
        var functions = LocateAttribute<FunctionAttribute>();

        foreach (var function in functions)
        {
            yield return new FunctionInfo(
                    function.Type.Name.ToKebabCase()
                    , function.Type.IsPublic
                    , function.Attribute.Prefix != null && string.IsNullOrEmpty(function.Attribute.Prefix)
                        ? function.Attribute.Aliases
                        : CalculateImplicitAliases(function.Attribute.Prefix, function.Type.Namespace!, function.Type.Name)
                            .Union(function.Attribute.Aliases)
                            .Where(x => !string.IsNullOrEmpty(x)).ToArray()
                    , function.Type.Namespace!.ToToken('.').Last()
                    , function.Type
                    , fast ? "" : function.Type.GetSummary()
                    , fast ? [] : BuildParameters(function.Type.GetInfoConstructors()).ToArray()
                );
        }
    }

    protected virtual IEnumerable<string> CalculateImplicitAliases(string? forcedPrefix, string ns, string function)
    {
        var prefix = string.IsNullOrEmpty(forcedPrefix) ? ns.Split('.').Last() : forcedPrefix;
        yield return $"{prefix.ToKebabCase()}-to-{function.ToKebabCase()}";
        if (prefix.ToLowerInvariant() == "datetime")
            yield return $"dateTime-to-{function.ToKebabCase()}";
        if (function.ToLowerInvariant().Contains("datetime"))
            yield return $"{prefix.ToKebabCase()}-to-{function.ToKebabCase().Replace("date-time", "dateTime")}";
    }
}
