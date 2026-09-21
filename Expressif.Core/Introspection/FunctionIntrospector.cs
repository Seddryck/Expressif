using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Introspection;

public class FunctionIntrospector : BaseIntrospector
{
    private IntrospectionOptions Options { get; }
    private ExpressifTypeMapper TypeMapper { get; }
    private FunctionContractIntrospector ContractIntrospector { get; }

    public FunctionIntrospector(IntrospectionOptions options, params Assembly[] assemblies)
        : this(new AssemblyTypeSource(assemblies.Distinct().ToArray()), options) { }

    public FunctionIntrospector(ITypeSource source, IntrospectionOptions options)
        : base(source)
    {
        Options = options;
        TypeMapper = new ExpressifTypeMapper(options);
        ContractIntrospector = new FunctionContractIntrospector(options, TypeMapper);
    }

    public IEnumerable<FunctionInfo> Locate()
        => Locate<FunctionAttribute>(true);

    public IEnumerable<FunctionInfo> Describe()
        => Locate<FunctionAttribute>(false);

    protected IEnumerable<FunctionInfo> Locate<T>(bool fast)
        where T : FunctionAttribute
    {
        var functions = LocateAttribute<FunctionAttribute>();

        foreach (var function in functions)
        {
            var name = function.Attribute.Name ?? function.Type.Name.ToKebabCase();
            var scope = function.Type.GetCustomAttribute<ScopeAttribute>(true)?.Name
                ?? function.Type.Namespace!.ToToken('.').Last().ToKebabCase();
            var scopePrefix = scope.Split('/')[0];
            var contract = ContractIntrospector.Describe(function.Type, name);
            var lifecycle = function.Type.GetCustomAttribute<FunctionLifecycleAttribute>(false);
            yield return new FunctionInfo(
                    name
                    , function.Type.IsPublic
                    , function.Attribute.Prefix != null && string.IsNullOrEmpty(function.Attribute.Prefix)
                        ? function.Attribute.Aliases
                        : function.Attribute.Aliases.AsQueryable()
                            .Prepend(string.IsNullOrEmpty(function.Attribute.Prefix)
                                ? $"{scopePrefix}-to-{function.Type.Name.ToKebabCase()}"
                                : $"{function.Attribute.Prefix}-to-{function.Type.Name.ToKebabCase()}"
                            ).Where(x => !string.IsNullOrEmpty(x)).ToArray()
                    , scope
                    , contract.Input
                    , contract.Output
                    , contract.Converted
                    , contract.Reason
                    , function.Type
                    , fast ? "" : function.Type.GetSummary()
                    , fast ? [] : BuildParameters(function.Type.GetInfoConstructors(TypeMapper, Options)).ToArray()
                    , lifecycle?.Deprecated ?? false
                    , lifecycle?.Replacement
                    , lifecycle?.Sunset
                    , lifecycle?.ReplacementIsEquivalent ?? false
                    , lifecycle?.MigrationNotes
                    , Options.TupleBindingSignatures(function.Type)
                )
            {
                DeprecatedAliases = function.Type.GetCustomAttributes<FunctionAliasLifecycleAttribute>()
                    .Select(alias => new FunctionAliasLifecycleInfo(
                        alias.Name,
                        alias.Replacement,
                        alias.Message,
                        alias.Sunset))
                    .ToArray(),
            };
        }
    }
}
