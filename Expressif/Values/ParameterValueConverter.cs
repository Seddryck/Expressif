using Expressif.Serializers;
using Expressif.Syntax;
using Expressif.Bindings;

namespace Expressif.Values;

/// <summary>
/// Converts bound parameter syntax into its runtime value representation.
/// </summary>
public sealed class ParameterValueConverter
{
    private readonly ParameterSerializer serializer = new();

    public object? Parse(string text)
        => Convert(new ExpressifBinder().BindParameter(ExpressionParser.Parse(text)));

    public object? Convert(IParameter parameter)
        => parameter switch
        {
            LiteralParameter literal => literal.Value,
            QuotedLiteralParameter quoted => quoted.Value,
            ArrayParameter array => ConvertArray(array),
            TupleParameter tuple => new Tuple(tuple.Values.Select(Convert).ToArray()),
            RecordLiteralParameter record => ConvertRecord(record),
            _ => serializer.Serialize(parameter),
        };

    private object?[] ConvertArray(ArrayParameter array)
    {
        var values = new List<object?>();
        foreach (var element in array.Elements)
        {
            var value = Convert(element.Value);
            if (element.IsSpread)
                Functions.Array.SpreadValues.Append(value, values);
            else
                values.Add(value);
        }
        return values.ToArray();
    }

    private RecordValue ConvertRecord(RecordLiteralParameter record)
    {
        var value = new RecordValue();
        foreach (var field in record.Fields)
        {
            if (value.ContainsKey(field.Name))
                throw new ArgumentException($"Duplicate field '{field.Name}' in record literal.");

            value.Set(field.Name, Convert(field.Value));
        }

        return value;
    }
}
