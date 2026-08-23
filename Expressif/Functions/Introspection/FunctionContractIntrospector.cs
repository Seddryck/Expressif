using Expressif.Functions.Coercions;
using Expressif.Values;
using System.Collections;

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
            ["neutral"] = "Identity semantics require an open TIn -> TIn contract rather than one closed contract.",
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
                ToExpressifType(coercion.TargetType),
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
        var names = types.Select(ToExpressifType).Distinct().OrderBy(x => x).ToArray();
        if (names.Length > 1)
            names = names.Where(x => x != "any").ToArray();

        return string.Join(" | ", names);
    }

    private static string ToExpressifType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string) || type == typeof(char))
            return "text";
        if (type == typeof(bool))
            return "boolean";
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            return "numeric";
        if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short)
            || type == typeof(ushort) || type == typeof(int) || type == typeof(uint)
            || type == typeof(long) || type == typeof(ulong))
            return "integer";
        if (type == typeof(DateOnly))
            return "date";
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return "date-time";
        if (type == typeof(TimeOnly))
            return "time";
        if (type == typeof(TimeSpan))
            return "duration";
        if (type == typeof(YearMonth))
            return "year-month";
        if (type == typeof(TupleValue) || type == typeof(Expressif.Values.Tuple))
            return "tuple";
        if (type == typeof(RecordValue))
            return "record";
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            return "array";

        return "any";
    }
}
