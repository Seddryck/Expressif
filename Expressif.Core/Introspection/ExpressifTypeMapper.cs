using System.Collections;
using Expressif.Functions.Accumulation;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Values.Types;

namespace Expressif.Introspection;

internal sealed class ExpressifTypeMapper
{
    private static readonly IReadOnlyDictionary<Type, string> BuiltInTypes =
        new System.Collections.Generic.Dictionary<Type, string>
        {
            [typeof(IFunction)] = "expression",
            [typeof(IPredicate)] = "predicate",
            [typeof(IAccumulator)] = "accumulator",
            [typeof(TypeDescriptor)] = "type",
            [typeof(string)] = "text",
            [typeof(char)] = "text",
            [typeof(bool)] = "boolean",
            [typeof(decimal)] = "numeric",
            [typeof(double)] = "numeric",
            [typeof(float)] = "numeric",
            [typeof(byte)] = "integer",
            [typeof(sbyte)] = "integer",
            [typeof(short)] = "integer",
            [typeof(ushort)] = "integer",
            [typeof(int)] = "integer",
            [typeof(uint)] = "integer",
            [typeof(long)] = "integer",
            [typeof(ulong)] = "integer",
            [typeof(IPositionalValue)] = "tuple",
            [typeof(Expressif.Values.Tuple)] = "tuple",
            [typeof(Vector)] = "vector",
            [typeof(Pair)] = "pair",
            [typeof(Group)] = "group",
            [typeof(Expressif.Values.Grouping)] = "grouping",
            [typeof(Dictionary)] = "dictionary",
            [typeof(RecordValue)] = "record",
            [typeof(OrderingValue)] = "ordering",
            [typeof(SortTerm)] = "sort-term",
            [typeof(SortKey)] = "sort-key",
            [typeof(SortTableValue)] = "sort-table",
        };

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
        if (TryGetParameterType(declaringType, parameterName, out var parameterType))
            return parameterType;

        type = Unwrap(type, unwrapProvider);
        if (type.IsEnum)
            return "text";
        if (BuiltInTypes.TryGetValue(type, out var builtInType))
            return builtInType;

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

    private bool TryGetParameterType(Type? declaringType, string? parameterName, out string parameterType)
    {
        if (declaringType is not null
            && parameterName is not null
            && parameterTypes.TryGetValue(new(declaringType, parameterName), out parameterType!))
            return true;

        parameterType = string.Empty;
        return false;
    }

    private static Type Unwrap(Type type, bool unwrapProvider)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (!unwrapProvider || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Func<>))
            return type;

        var providedType = type.GetGenericArguments()[0];
        return Nullable.GetUnderlyingType(providedType) ?? providedType;
    }
}
