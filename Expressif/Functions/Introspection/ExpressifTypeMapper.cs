using Expressif.Accumulators;
using Expressif.Predicates;
using Expressif.Types;
using Expressif.Values;
using System.Collections;

namespace Expressif.Functions.Introspection;

internal static class ExpressifTypeMapper
{
    private static readonly IReadOnlyDictionary<(string Function, string Parameter), string> ParameterOverrides =
        new Dictionary<(string, string), string>
        {
            [("Broadcast", "accumulator")] = "accumulator",
            [("Fold", "accumulator")] = "accumulator",
            [("Scan", "accumulator")] = "accumulator",
            [("DurationBetween", "previous")] = "date | date-time | year-month",
            [("Array", "values")] = "any",
            [("Tuple", "values")] = "any",
            [("Pair", "key")] = "any",
            [("Pair", "value")] = "any",
            [("Grouping", "values")] = "pair",
            [("Dictionary", "values")] = "pair",
            [("Key", "expressions")] = "expression",
            [("GroupBy", "expressions")] = "expression",
            [("Text", "values")] = "expression",
            [("Record", "entries")] = "entry",
            [("Put", "assignments")] = "entry",
            [("PutPresent", "assignments")] = "entry",
            [("PutAbsent", "assignments")] = "entry",
            [("PutPath", "path")] = "expression",
            [("PutPath", "value")] = "expression",
            [("PutPresentPath", "path")] = "expression",
            [("PutPresentPath", "value")] = "expression",
            [("PutAbsentPath", "path")] = "expression",
            [("PutAbsentPath", "value")] = "expression",
            [("With", "projections")] = "entry",
            [("With", "body")] = "expression",
            [("Coalesce", "expressions")] = "expression",
            [("Coerce", "specifications")] = "type | mapping",
            [("TransformWith", "operation")] = "expression",
            [("TransformWith", "expressions")] = "expression",
            [("TransformAs", "operation")] = "expression",
            [("TransformAs", "expressions")] = "entry",
            [("Majority", "predicates")] = "predicate",
        };

    public static string ToExpressifType(
        Type type,
        bool unwrapProvider = false,
        Type? declaringType = null,
        string? parameterName = null)
    {
        if (declaringType is not null && parameterName is not null
            && ParameterOverrides.TryGetValue((declaringType.Name, parameterName), out var parameterType))
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
        if (type == typeof(Weekday))
            return "weekday";
        if (type == typeof(TupleValue) || type == typeof(Expressif.Values.Tuple))
            return "tuple";
        if (type == typeof(PairValue) || type == typeof(Expressif.Values.Pair))
            return "pair";
        if (type == typeof(Group))
            return "group";
        if (type == typeof(Expressif.Values.Grouping))
            return "grouping";
        if (type == typeof(DictionaryValue) || type == typeof(Expressif.Values.Dictionary))
            return "dictionary";
        if (type == typeof(RecordValue))
            return "record";
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            return "array";

        return "any";
    }
}
