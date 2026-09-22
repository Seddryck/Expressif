using System.Collections;
using Expressif.Functions.Accumulation;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Values.Types;

namespace Expressif.Introspection;

internal sealed class ExpressifTypeMapper
{
    private readonly ITypeRegistry types;
    private readonly IReadOnlyDictionary<ParameterIntrospectionKey, string> parameterTypes;

    public ExpressifTypeMapper(IntrospectionOptions options)
        => (types, parameterTypes) = (options.Types, options.ParameterTypes);

    public string ToExpressifType(
        Type type,
        bool unwrapProvider = false,
        Type? declaringType = null,
        string? parameterName = null)
    {
        if (declaringType is not null && parameterName is not null
            && parameterTypes.TryGetValue(new(declaringType, parameterName), out var parameterType))
            return parameterType;

        type = Nullable.GetUnderlyingType(type) ?? type;
        if (unwrapProvider && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>))
            type = Nullable.GetUnderlyingType(type.GetGenericArguments()[0]) ?? type.GetGenericArguments()[0];

        if (type == typeof(IFunction))
            return "expression";
        if (type == typeof(IPredicate))
            return "predicate";
        if (type == typeof(IAccumulator))
            return "accumulator";
        if (type == typeof(TypeDescriptor))
            return "type";
        if (type == typeof(string) || type == typeof(char) || type.IsEnum)
            return "text";
        if (type == typeof(bool))
            return "boolean";
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            return "numeric";
        if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short)
            || type == typeof(ushort) || type == typeof(int) || type == typeof(uint)
            || type == typeof(long) || type == typeof(ulong))
            return "integer";
        if (type == typeof(IPositionalValue) || type == typeof(Expressif.Values.Tuple))
            return "tuple";
        if (type == typeof(Vector))
            return "vector";
        if (type == typeof(Pair))
            return "pair";
        if (type == typeof(Group))
            return "group";
        if (type == typeof(Expressif.Values.Grouping))
            return "grouping";
        if (type == typeof(Dictionary))
            return "dictionary";
        if (type == typeof(RecordValue))
            return "record";
        if (type == typeof(OrderingValue))
            return "ordering";
        if (type == typeof(SortTerm))
            return "sort-term";
        if (type == typeof(SortKey))
            return "sort-key";
        if (type == typeof(SortTableValue))
            return "sort-table";

        var descriptor = types.All.FirstOrDefault(candidate => candidate.RuntimeType == type);
        if (descriptor is not null)
        {
            return descriptor.Name.Equals("datetime", StringComparison.OrdinalIgnoreCase)
                ? "date-time"
                : descriptor.Name;
        }

        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            return "array";

        return "any";
    }
}
