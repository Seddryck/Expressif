using Expressif.Functions;
using Expressif.Values;

namespace Expressif.Predicates.Tuple;

/// <summary>Returns whether the input tuple has exactly the expected number of positions.</summary>
[Predicate(appendIs: false, name: "has-arity")]
[Scope("tuple")]
public sealed class HasArity : BasePredicate, IPredicate<TupleValue>
{
    private Func<int> Expected { get; }
    /// <param name="expected">Specifies the required non-negative tuple arity.</param>
    public HasArity(Func<int> expected) => Expected = expected;
    public bool Evaluate(TupleValue? value)
    {
        var expected = Expected.Invoke();
        if (expected < 0)
            throw new ArgumentOutOfRangeException(nameof(expected), "Expected arity must be non-negative.");
        return value is not null && value.Count == expected;
    }
    public override bool Evaluate(object? value) => value is TupleValue tuple && Evaluate(tuple);
}
