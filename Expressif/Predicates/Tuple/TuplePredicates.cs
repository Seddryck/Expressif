using Expressif.Functions;
using Expressif.Values;

namespace Expressif.Predicates.Tuple;

/// <summary>Returns whether the input value is a tuple.</summary>
[Predicate(appendIs: false, name: "is-tuple")]
[Scope("tuple")]
public sealed class IsTuple : BasePredicate
{
    public override bool Evaluate(object? value) => value is IPositionalValue;
}

/// <summary>Returns whether the input tuple has exactly the expected number of positions.</summary>
[Predicate(appendIs: false, name: "has-arity")]
[Scope("tuple")]
public sealed class HasArity : BasePredicate, IPredicate<IPositionalValue>
{
    private Func<int> Expected { get; }
    /// <param name="expected">Specifies the required non-negative tuple arity.</param>
    public HasArity(Func<int> expected) => Expected = expected;
    public bool Evaluate(IPositionalValue? value)
    {
        var expected = Expected.Invoke();
        if (expected < 0)
            throw new ArgumentOutOfRangeException(nameof(expected), "Expected arity must be non-negative.");
        return value is not null && value.Arity == expected;
    }
    public override bool Evaluate(object? value) => value is IPositionalValue tuple && Evaluate(tuple);
}
