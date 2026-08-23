using Expressif.Accumulators;
using Expressif.Predicates;
using Expressif.Values;
using System.Collections;

namespace Expressif.Functions.Introspection;

internal static class ExpressifTypeMapper
{
    public static string ToExpressifType(Type type, bool unwrapProvider = false)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (unwrapProvider && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>))
            type = Nullable.GetUnderlyingType(type.GetGenericArguments()[0]) ?? type.GetGenericArguments()[0];

        if (type == typeof(IFunction))
            return "expression";
        if (type == typeof(IPredicate))
            return "predicate";
        if (type == typeof(IAccumulator))
            return "accumulator";
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
