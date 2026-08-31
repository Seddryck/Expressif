using Expressif.Functions.Coercions;

namespace Expressif.Functions.Introspection;

internal sealed record FunctionContract(string Input, string Output, bool Converted, string Reason);

internal static class FunctionContractIntrospector
{
    private static readonly IReadOnlyDictionary<string, ICoercionDescriptor> CoercionDescriptors =
        new CoercionRegistry().Descriptors.ToDictionary(x => x.Name, StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, string> UntypedReasons =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["coalesce"] = "Output depends on the first non-null expression result at runtime.",
            ["field"] = "Output depends on the selected field in the runtime record shape.",
            ["guard"] = "Output preserves the input type when entry is incompatible and otherwise depends on the guarded expression.",
            ["neutral"] = "Identity semantics require an open TIn -> TIn contract rather than one closed contract.",
            ["walk"] = "Output preserves each container kind while leaf output types depend on the supplied expression.",
        };

    private static readonly IReadOnlyDictionary<string, string> OutputOverrides =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["datetime-to-date"] = "date",
        };

    public static FunctionContract Describe(Type implementationType, string name)
    {
        if (CoercionDescriptors.TryGetValue(name, out var coercion))
        {
            return new FunctionContract(
                JoinTypes(coercion.SourceTypes),
                ExpressifTypeMapper.ToExpressifType(coercion.TargetType),
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
                UntypedReasons.GetValueOrDefault(
                    name,
                    "No unambiguous closed IFunction<TIn, TOut> contract is exposed."));
        }

        var output = JoinTypes(contracts.Select(x => x[1]));
        return new FunctionContract(
            JoinTypes(contracts.Select(x => x[0])),
            OutputOverrides.GetValueOrDefault(name, output),
            true,
            "Exposes at least one closed IFunction<TIn, TOut> contract.");
    }

    private static string JoinTypes(IEnumerable<Type> types)
    {
        var names = types.Select(x => ExpressifTypeMapper.ToExpressifType(x)).Distinct().OrderBy(x => x).ToArray();
        if (names.Length > 1)
            names = names.Where(x => x != "any").ToArray();

        return string.Join(" | ", names);
    }
}
