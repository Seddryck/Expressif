using Expressif.Values;

namespace Expressif.Library.Sorting;

/// <summary>Creates a sort term from a value and a tuple-bound ordering comparer.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class SortTerm : IFunction<object?, SortTermValue>
{
    private readonly Func<object?> value;
    private readonly Func<SortComparer> comparer;

    /// <param name="value">The value to compare.</param>
    /// <param name="comparer">The tuple-bound ordering comparer.</param>
    public SortTerm(Func<object?> value, Func<SortComparer> comparer)
        => (this.value, this.comparer) = (value, comparer);

    public SortTermValue Evaluate(object? input)
        => new Values.SortTerm(value.Invoke(), comparer.Invoke(), ascending: true, nullsFirst: false);

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

public abstract class BaseSortTermModifier : IFunction<SortTermValue, SortTermValue>
{
    public abstract SortTermValue Evaluate(SortTermValue value);

    object? IFunction.Evaluate(object? value)
        => value is SortTermValue term
            ? Evaluate(term)
            : throw new ArgumentException("A sort-term modifier requires a SortTerm value.", nameof(value));
}

/// <summary>Enables ascending sort direction.</summary>
[Function(prefix: "", aliases: ["asc"])]
[Scope("sorting")]
public sealed class Ascending : BaseSortTermModifier
{
    public override SortTermValue Evaluate(SortTermValue value)
        => new Values.SortTerm(value.Value, value.Comparer, ascending: true, value.NullsFirst);
}

/// <summary>Enables descending sort direction.</summary>
[Function(prefix: "", aliases: ["desc"])]
[Scope("sorting")]
public sealed class Descending : BaseSortTermModifier
{
    public override SortTermValue Evaluate(SortTermValue value)
        => new Values.SortTerm(value.Value, value.Comparer, ascending: false, value.NullsFirst);
}

/// <summary>Places null values before non-null values.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class NullsFirst : BaseSortTermModifier
{
    public override SortTermValue Evaluate(SortTermValue value)
        => new Values.SortTerm(value.Value, value.Comparer, value.Ascending, nullsFirst: true);
}

/// <summary>Places null values after non-null values.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class NullsLast : BaseSortTermModifier
{
    public override SortTermValue Evaluate(SortTermValue value)
        => new Values.SortTerm(value.Value, value.Comparer, value.Ascending, nullsFirst: false);
}
