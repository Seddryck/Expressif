using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Functions.Vector;

/// <summary>Returns the Euclidean distance between the input vector and another vector of the same dimension.</summary>
[Function(prefix: "", aliases: [])]
[Scope("vector")]
public sealed class Distance : IFunction<VectorValue, decimal>
{
    private Func<VectorValue> Vector { get; }

    /// <param name="vector">Specifies the vector whose distance from the input vector is calculated.</param>
    public Distance(Func<VectorValue> vector) => Vector = vector;

    public decimal Evaluate(VectorValue value)
    {
        var other = Vector.Invoke();
        if (value.Arity != other.Arity)
            throw new ArgumentException("Distance requires vectors with equal dimensions.", nameof(value));

        var left = VectorMath.Components(value);
        var right = VectorMath.Components(other);
        return VectorMath.Magnitude(left.Zip(right, (first, second) => first - second));
    }

    object? IFunction.Evaluate(object? value) => value is VectorValue vector ? Evaluate(vector) : null;
}

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

/// <summary>Returns the Euclidean magnitude of the input vector.</summary>
[Function(prefix: "", aliases: [])]
[Scope("vector")]
public sealed class Magnitude : IFunction<VectorValue, decimal>
{
    public decimal Evaluate(VectorValue value) => VectorMath.Magnitude(value);

    object? IFunction.Evaluate(object? value) => value is VectorValue vector ? Evaluate(vector) : null;
}

/// <summary>Returns a unit vector pointing in the same direction as the input vector.</summary>
[Function(prefix: "", aliases: [])]
[Scope("vector")]
public sealed class Normalize : IFunction<VectorValue, VectorValue>
{
    public VectorValue Evaluate(VectorValue value)
    {
        var magnitude = VectorMath.Magnitude(value);
        if (magnitude == 0)
            throw new InvalidOperationException("A zero vector cannot be normalized.");

        return new Expressif.Values.Vector(VectorMath.Components(value)
            .Select(component => (object?)(component / magnitude))
            .ToArray());
    }

    object? IFunction.Evaluate(object? value) => value is VectorValue vector ? Evaluate(vector) : null;
}

internal static class VectorMath
{
    public static decimal[] Components(VectorValue value)
    {
        var caster = new NumericCaster();
        return Enumerable.Range(0, value.Arity).Select(index =>
        {
            caster.TryCast(value.GetPosition(index)!, out var component);
            return component;
        }).ToArray();
    }

    public static decimal Magnitude(VectorValue value) => Magnitude(Components(value));

    public static decimal Magnitude(IEnumerable<decimal> components)
        => (decimal)Math.Sqrt((double)components.Sum(component => component * component));
}
