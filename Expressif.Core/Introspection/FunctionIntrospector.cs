using System.Reflection;
using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Introspection;

/// <summary>Describes functions discovered from extension assemblies or a custom type source.</summary>
public sealed class FunctionIntrospector
{
    private readonly BaseIntrospector scanner;
    private readonly IntrospectionOptions options;
    private readonly ExpressifTypeMapper typeMapper;
    private readonly FunctionContractIntrospector contractIntrospector;

    public FunctionIntrospector(params Assembly[] assemblies)
        : this(new AssemblyTypeSource(RequireAssemblies(assemblies)), IntrospectionOptions.Default) { }

    public FunctionIntrospector(ITypeSource source)
        : this(source, IntrospectionOptions.Default) { }

    internal FunctionIntrospector(ITypeSource source, IntrospectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.options = options;
        scanner = new BaseIntrospector(source);
        typeMapper = new ExpressifTypeMapper(options);
        contractIntrospector = new FunctionContractIntrospector(options, typeMapper);
    }

    public IReadOnlyList<FunctionInfo> Describe()
        => DescribeFunctions().ToList().AsReadOnly();

    private IEnumerable<FunctionInfo> DescribeFunctions()
    {
        foreach (var function in scanner.LocateAttribute<FunctionAttribute>())
        {
            var name = function.Attribute.Name ?? function.Type.Name.ToKebabCase();
            var scope = function.Type.GetCustomAttribute<ScopeAttribute>(true)?.Name
                ?? function.Type.Namespace!.ToToken('.').Last().ToKebabCase();
            var scopePrefix = scope.Split('/')[0];
            var contract = contractIntrospector.Describe(function.Type, name);
            var lifecycle = function.Type.GetCustomAttribute<FunctionLifecycleAttribute>(false);
            IEnumerable<string> aliases = function.Attribute.Prefix != null && string.IsNullOrEmpty(function.Attribute.Prefix)
                ? function.Attribute.Aliases
                : function.Attribute.Aliases.AsEnumerable()
                    .Prepend(string.IsNullOrEmpty(function.Attribute.Prefix)
                        ? $"{scopePrefix}-to-{function.Type.Name.ToKebabCase()}"
                        : $"{function.Attribute.Prefix}-to-{function.Type.Name.ToKebabCase()}")
                    .Where(alias => !string.IsNullOrEmpty(alias));
            var deprecatedAliases = function.Type.GetCustomAttributes<FunctionAliasLifecycleAttribute>()
                .Select(alias => new FunctionAliasLifecycleInfo(
                    alias.Name,
                    alias.Replacement,
                    alias.Message,
                    alias.Sunset));

            yield return new FunctionInfo(new FunctionInfoDefinition
            {
                Name = name,
                IsPublic = function.Type.IsPublic,
                Aliases = aliases,
                Scope = scope,
                Input = contract.Input,
                Output = contract.Output,
                Converted = contract.Converted,
                Reason = contract.Reason,
                ImplementationType = function.Type,
                Summary = function.Type.GetSummary(),
                Parameters = BaseIntrospector.BuildParameters(function.Type.GetInfoConstructors(typeMapper, options)),
                Deprecated = lifecycle?.Deprecated ?? false,
                Replacement = lifecycle?.Replacement,
                Sunset = lifecycle?.Sunset,
                ReplacementIsEquivalent = lifecycle?.ReplacementIsEquivalent ?? false,
                MigrationNotes = lifecycle?.MigrationNotes,
                Signatures = options.TupleBindingSignatures(function.Type).Select(signature => signature.ToInfo()),
                DeprecatedAliases = deprecatedAliases,
            });
        }
    }

    private static Assembly[] RequireAssemblies(Assembly[] assemblies)
        => assemblies.Length > 0
            ? assemblies.Distinct().ToArray()
            : throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
}
