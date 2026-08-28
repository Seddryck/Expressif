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
            TupleParameter tuple => ConvertTuple(tuple),
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

    private Tuple ConvertTuple(TupleParameter tuple)
    {
        var values = new List<object?>();
        foreach (var element in tuple.Elements)
        {
            var value = Convert(element.Value);
            if (element.IsSpread)
            {
                if (value is null)
                    throw new SpreadArgumentException("Spread argument cannot be null.");
                if (value is not TupleValue spread)
                    throw new SpreadArgumentException("Spread argument must evaluate to a tuple.");
                values.AddRange(spread);
            }
            else
            {
                values.Add(value);
            }
        }

        return new Tuple(values.ToArray());
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
