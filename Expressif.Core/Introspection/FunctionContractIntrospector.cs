using Expressif.Functions;
using Expressif.Functions.Coercions;

namespace Expressif.Introspection;

internal sealed record FunctionContract(string Input, string Output, bool Converted, string Reason);

internal sealed class FunctionContractIntrospector
{
    private readonly IntrospectionOptions options;
    private readonly ExpressifTypeMapper typeMapper;
    private readonly IReadOnlyDictionary<string, ICoercionDescriptor> coercions;

    public FunctionContractIntrospector(IntrospectionOptions options, ExpressifTypeMapper typeMapper)
    {
        this.options = options;
        this.typeMapper = typeMapper;
        coercions = options.Coercions.Descriptors.ToDictionary(x => x.Name, StringComparer.Ordinal);
    }

    public FunctionContract Describe(Type implementationType, string name)
    {
        if (coercions.TryGetValue(name, out var coercion))
        {
            return new FunctionContract(
                JoinTypes(coercion.SourceTypes),
                typeMapper.ToExpressifType(coercion.TargetType),
                true,
                "Exposes direct typed coercion contracts through the coercion registry.");
        }

        var contracts = implementationType.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IFunction<,>))
            .Select(x => x.GetGenericArguments())
            .ToArray();

        if (contracts.Length == 0)
        {
            return new FunctionContract(
                "any",
                "any",
                false,
                implementationType.GetCustomAttributes(typeof(FunctionAttribute), true)
                    .OfType<FunctionAttribute>().FirstOrDefault()?.DynamicReason
                ?? options.UntypedReasons.GetValueOrDefault(
                    name,
                    "No unambiguous closed IFunction<TIn, TOut> contract is exposed."));
        }

        var output = JoinTypes(contracts.Select(x => x[1]));
        return new FunctionContract(
            JoinTypes(contracts.Select(x => x[0])),
            options.OutputOverrides.GetValueOrDefault(name, output),
            true,
            implementationType.GetCustomAttributes(typeof(FunctionAttribute), true)
                .OfType<FunctionAttribute>().FirstOrDefault()?.DynamicReason
            ?? "Exposes at least one closed IFunction<TIn, TOut> contract.");
    }

    private string JoinTypes(IEnumerable<Type> types)
    {
        var names = types.Select(type => typeMapper.ToExpressifType(type)).Distinct().OrderBy(x => x).ToArray();
        if (names.Length > 1)
            names = names.Where(x => x != "any").ToArray();

        return string.Join(" | ", names);
    }
}
