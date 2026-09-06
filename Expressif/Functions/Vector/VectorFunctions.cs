using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Functions.Vector;

/// <summary>Returns the dot product of the input vector and another vector of the same dimension.</summary>
[Function(prefix: "", aliases: [])]
[Scope("vector")]
public sealed class Dot : IFunction<VectorValue, decimal>
{
    private Func<VectorValue> Vector { get; }

    /// <param name="vector">Specifies the vector whose components are multiplied with the input components.</param>
    public Dot(Func<VectorValue> vector) => Vector = vector;

    public decimal Evaluate(VectorValue value)
    {
        var other = Vector.Invoke();
        if (value.Arity != other.Arity)
            throw new ArgumentException("Dot product requires vectors with equal dimensions.", nameof(value));

        var caster = new NumericCaster();
        var result = 0m;
        for (var index = 0; index < value.Arity; index++)
        {
            caster.TryCast(value.GetPosition(index)!, out var left);
            caster.TryCast(other.GetPosition(index)!, out var right);
            result += left * right;
        }
        return result;
    }

    object? IFunction.Evaluate(object? value) => value is VectorValue vector ? Evaluate(vector) : null;
}
