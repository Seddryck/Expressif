using System.Collections;

namespace Expressif.Predicates.Array;

public abstract class BaseQuantifierPredicate : BaseArrayPredicate
{
    public Func<IPredicate> Predicate { get; }

    protected BaseQuantifierPredicate(Func<IPredicate> predicate)
        => Predicate = predicate;
}

/// <summary>
/// Returns whether no element of the input array satisfies the supplied predicate. Returns `false` when the input cannot be evaluated.
/// </summary>
public class None : BaseQuantifierPredicate
{
    /// <param name="predicate">Specifies the predicate evaluated against each array element.</param>
    public None(Func<IPredicate> predicate)
        : base(predicate) { }

    protected override bool EvaluateArray(IEnumerable array)
    {
        var predicate = Predicate.Invoke();
        foreach (var item in array)
            if (predicate.Evaluate(item))
                return false;

        return true;
    }
}

/// <summary>
/// Returns whether every element of the input array satisfies the supplied predicate. Returns `false` when the input cannot be evaluated.
/// </summary>
[Predicate(["every"])]
public class All : BaseQuantifierPredicate
{
    /// <param name="predicate">Specifies the predicate evaluated against each array element.</param>
    public All(Func<IPredicate> predicate)
        : base(predicate) { }

    protected override bool EvaluateArray(IEnumerable array)
    {
        var predicate = Predicate.Invoke();
        foreach (var item in array)
            if (!predicate.Evaluate(item))
                return false;

        return true;
    }
}

/// <summary>
/// Returns whether at least one element of the input array satisfies the supplied predicate. Returns `false` when the input cannot be evaluated.
/// </summary>
[Predicate(["any"])]
public class Some : BaseQuantifierPredicate
{
    /// <param name="predicate">Specifies the predicate evaluated against each array element.</param>
    public Some(Func<IPredicate> predicate)
        : base(predicate) { }

    protected override bool EvaluateArray(IEnumerable array)
    {
        var predicate = Predicate.Invoke();
        foreach (var item in array)
            if (predicate.Evaluate(item))
                return true;

        return false;
    }
}

/// <summary>
/// Returns whether exactly one element of the input array satisfies the supplied predicate. Returns `false` when the input cannot be evaluated.
/// </summary>
public class Single : BaseQuantifierPredicate
{
    /// <param name="predicate">Specifies the predicate evaluated against each array element.</param>
    public Single(Func<IPredicate> predicate)
        : base(predicate) { }

    protected override bool EvaluateArray(IEnumerable array)
    {
        var predicate = Predicate.Invoke();
        var found = false;

        foreach (var item in array)
        {
            if (!predicate.Evaluate(item))
                continue;

            if (found)
                return false;

            found = true;
        }

        return found;
    }
}
