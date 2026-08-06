using Expressif.Parsers;
using Expressif.Serializers;
using Sprache;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Expressif.Values.Casters;

public class ArrayCaster : ICaster<object?[]>, IParser<object?[]>
{
    private static readonly ParameterSerializer ParameterSerializer = new();

    public virtual bool TryCast(object obj, [NotNullWhen(true)] out object?[]? value)
        => obj switch
        {
            string text => TryParse(text, out value),
            IEnumerable enumerable when obj is not string => (value = [.. enumerable.Cast<object?>()]) == value,
            _ => (value = null) != value
        };

    public virtual object?[] Cast(object obj)
        => TryCast(obj, out var value)
            ? value
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to virtual type Array. The type Array can only be casted from non-string IEnumerable values and Expressif array literals encoded as String.");

    public virtual bool TryParse(string text, [NotNullWhen(true)] out object?[]? value)
    {
        value = null;

        try
        {
            var parameter = Parameter.Parser.End().Parse(text);
            if (parameter is not ArrayParameter array)
                return false;

            value = array.Values.Select(ConvertToRuntimeValue).ToArray();
            return true;
        }
        catch (ParseException)
        {
            return false;
        }
    }

    public virtual object?[] Parse(string text)
        => TryParse(text, out var value) ? value : throw new FormatException();

    private static object? ConvertToRuntimeValue(IParameter parameter)
    {
        return parameter switch
        {
            LiteralParameter literal => RecordSyntax.TryParseTypedToken(literal.Value, out var typed)
                ? typed
                : literal.Value,
            QuotedLiteralParameter quoted => quoted.Value,
            ArrayParameter array => array.Values.Select(ConvertToRuntimeValue).ToArray(),
            RecordLiteralParameter record => ConvertRecord(record),
            _ => ParameterSerializer.Serialize(parameter),
        };
    }

    private static RecordValue ConvertRecord(RecordLiteralParameter record)
    {
        var value = new RecordValue();
        foreach (var field in record.Fields)
            value.Set(field.Name, ConvertToRuntimeValue(field.Value));

        return value;
    }
}