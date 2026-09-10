using Expressif.Values;

namespace Expressif.Functions.Tuple;

/// <summary>
/// Returns a tuple with its positions rotated cyclically, preserving arity and item values, including nulls.
/// </summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class Rotate : IFunction<IPositionalValue, TupleValue?>
{
    private Func<int> Offset { get; }

    /// <summary>Creates a rotation that moves the last tuple item to the front.</summary>
    public Rotate()
        : this(() => -1) { }

    /// <param name="offset">Specifies the rotation offset: positive values rotate left and negative values rotate right, wrapping modulo tuple length. Defaults to -1; zero leaves the order unchanged.</param>
    public Rotate(Func<int> offset)
        => Offset = offset;

    public TupleValue? Evaluate(IPositionalValue value)
    {
        if (value is null)
            return null;
        var offset = Offset.Invoke();
        var values = new object?[value.Arity];
        if (value.Arity > 0)
        {
            var position = offset % value.Arity;
            if (position < 0)
                position += value.Arity;
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = value.GetPosition(position);
                if (++position == value.Arity)
                    position = 0;
            }
        }
        return new Expressif.Values.Tuple(values);
    }

    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}
