using Expressif.Values.Casters;

namespace Expressif.Predicates.Boolean;

internal static class BooleanConversion
{
    private static readonly BooleanCaster Caster = new();
    private static readonly NumericCaster NumericCaster = new();

    public static bool ToBoolean(object? value)
    {
        if (value is null)
            return false;

        if (Caster.TryCast(value, out var boolean))
            return boolean;

        return NumericCaster.TryCast(value, out var numeric) && numeric != 0;
    }
}

/// <summary>
/// Returns the logical conjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `true`.
/// </summary>
[Predicate(false, prefix: "")]
public class And : BaseBooleanPredicate
{
    public Func<object?>? Expression { get; }
    public Func<bool>? Left { get; }
    public Func<bool>? Right { get; }

    /// <param name="expression">Specifies the secondary predicate expression evaluated when the converted input is `true`.</param>
    public And(Func<object?> expression)
        => Expression = expression;

    public And(Func<bool> left, Func<bool> right)
        => (Left, Right) = (left, right);

    public override bool Evaluate(object? value)
    {
        if (Left is null)
            return base.Evaluate(value);

        using var scope = EvaluationRuntime.Derive(value);
        return Left.Invoke() && Right!.Invoke();
    }

    protected override bool EvaluateBoolean(bool boolean)
        => boolean && BooleanConversion.ToBoolean(Expression!.Invoke());
}

/// <summary>
/// Returns the logical disjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `false`.
/// </summary>
[Predicate(false, prefix: "")]
public class Or : BaseBooleanPredicate
{
    public Func<object?>? Expression { get; }
    public Func<bool>? Left { get; }
    public Func<bool>? Right { get; }

    /// <param name="expression">Specifies the secondary predicate expression evaluated when the converted input is `false`.</param>
    public Or(Func<object?> expression)
        => Expression = expression;

    public Or(Func<bool> left, Func<bool> right)
        => (Left, Right) = (left, right);

    public override bool Evaluate(object? value)
    {
        if (Left is null)
            return base.Evaluate(value);

        using var scope = EvaluationRuntime.Derive(value);
        return Left.Invoke() || Right!.Invoke();
    }

    protected override bool EvaluateBoolean(bool boolean)
        => boolean || BooleanConversion.ToBoolean(Expression!.Invoke());

    protected override bool EvaluateNull()
        => BooleanConversion.ToBoolean(Expression!.Invoke());
}

/// <summary>
/// Returns `true` when exactly one of the Boolean-converted input and a secondary predicate expression evaluates to `true`. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Always evaluates the secondary expression.
/// </summary>
[Predicate(false, prefix: "")]
public class Xor : BaseBooleanPredicate
{
    public Func<object?>? Expression { get; }
    public Func<bool>? Left { get; }
    public Func<bool>? Right { get; }

    /// <param name="expression">Specifies the secondary predicate expression evaluated after the input.</param>
    public Xor(Func<object?> expression)
        => Expression = expression;

    public Xor(Func<bool> left, Func<bool> right)
        => (Left, Right) = (left, right);

    public override bool Evaluate(object? value)
    {
        if (Left is null)
            return base.Evaluate(value);

        using var scope = EvaluationRuntime.Derive(value);
        return Left.Invoke() ^ Right!.Invoke();
    }

    protected override bool EvaluateBoolean(bool boolean)
        => boolean ^ BooleanConversion.ToBoolean(Expression!.Invoke());

    protected override bool EvaluateNull()
        => BooleanConversion.ToBoolean(Expression!.Invoke());
}

/// <summary>
/// Returns the logical negation of the Boolean-converted input. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`.
/// </summary>
[Predicate(false, prefix: "")]
public class Not : BaseBooleanPredicate
{
    protected override bool EvaluateBoolean(bool boolean)
        => !boolean;

    protected override bool EvaluateNull()
        => true;
}

/// <summary>
/// Returns the negation of the logical conjunction of the Boolean input and a secondary Boolean expression. Evaluates the secondary expression only when the input is `true`.
/// </summary>
[Predicate(false, prefix: "")]
public class Nand : BasePredicate
{
    private And Conjunction { get; }
    private Not Negation { get; } = new();

    /// <param name="expression">Specifies the secondary Boolean expression evaluated when the input is `true`.</param>
    public Nand(Func<bool> expression)
        => Conjunction = new(() => expression.Invoke());

    public override bool Evaluate(object? value)
        => Negation.Evaluate(Conjunction.Evaluate(value));
}

/// <summary>
/// Returns the negation of the logical disjunction of the Boolean input and a secondary Boolean expression. Evaluates the secondary expression only when the input is `false`.
/// </summary>
[Predicate(false, prefix: "")]
public class Nor : BasePredicate
{
    private Or Disjunction { get; }
    private Not Negation { get; } = new();

    /// <param name="expression">Specifies the secondary Boolean expression evaluated when the input is `false`.</param>
    public Nor(Func<bool> expression)
        => Disjunction = new(() => expression.Invoke());

    public override bool Evaluate(object? value)
        => Negation.Evaluate(Disjunction.Evaluate(value));
}

/// <summary>
/// Returns the negation of the exclusive disjunction of the Boolean input and a secondary Boolean expression. Always evaluates the secondary expression after the input.
/// </summary>
[Predicate(false, prefix: "")]
public class Xnor : BasePredicate
{
    private Xor ExclusiveDisjunction { get; }
    private Not Negation { get; } = new();

    /// <param name="expression">Specifies the secondary Boolean expression evaluated after the input.</param>
    public Xnor(Func<bool> expression)
        => ExclusiveDisjunction = new(() => expression.Invoke());

    public override bool Evaluate(object? value)
        => Negation.Evaluate(ExclusiveDisjunction.Evaluate(value));
}

/// <summary>
/// Returns logical implication from the Boolean input to a secondary Boolean expression. Returns `true` without evaluating the expression when the input is `false`.
/// </summary>
[Predicate(false, prefix: "")]
public class Implies : BasePredicate
{
    private Func<bool> Expression { get; }

    /// <param name="expression">Specifies the secondary Boolean expression evaluated when the input is `true`.</param>
    public Implies(Func<bool> expression)
        => Expression = expression;

    public override bool Evaluate(object? value)
    {
        if (!BooleanConversion.ToBoolean(value))
            return true;

        return Expression.Invoke();
    }
}

/// <summary>
/// Returns `true` when strictly more than half of the supplied predicates are satisfied by the input. Returns `false` when no predicates are supplied and stops evaluating as soon as the result is known.
/// </summary>
[Predicate(false, prefix: "")]
public class Majority : BasePredicate
{
    private Func<bool>[] Predicates { get; }

    /// <param name="predicates">Specifies the predicate expressions evaluated against the same input value, in declaration order.</param>
    public Majority(IEnumerable<Func<bool>> predicates)
        => Predicates = predicates.ToArray();

    public override bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Derive(value);
        var required = (Predicates.Length / 2) + 1;
        var satisfied = 0;
        for (var index = 0; index < Predicates.Length; index++)
        {
            if (Predicates[index].Invoke())
                satisfied++;

            if (satisfied >= required)
                return true;

            var remaining = Predicates.Length - index - 1;
            if (satisfied + remaining < required)
                return false;
        }

        return false;
    }
}

public abstract class PredicateCardinalityBase : BasePredicate
{
    private Func<int> Count { get; }
    protected Func<bool>[] Predicates { get; }

    protected PredicateCardinalityBase(Func<int> count, IEnumerable<Func<bool>> predicates)
        => (Count, Predicates) = (count, predicates.ToArray());

    protected int GetValidatedCount()
    {
        var count = Count.Invoke();
        return count >= 0
            ? count
            : throw new ArgumentOutOfRangeException(nameof(count), count, "Predicate count must be non-negative.");
    }
}

/// <summary>
/// Returns `true` when exactly the requested number of supplied predicates are satisfied by the input. The count must be non-negative, and evaluation stops as soon as the result is known.
/// </summary>
[Predicate(false, prefix: "")]
public class SatisfiesExactly : PredicateCardinalityBase
{
    /// <param name="count">Specifies the exact non-negative number of predicates that must be satisfied.</param>
    /// <param name="predicates">Specifies the predicate expressions evaluated against the same input value, in declaration order.</param>
    public SatisfiesExactly(Func<int> count, IEnumerable<Func<bool>> predicates)
        : base(count, predicates) { }

    public override bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Derive(value);
        var count = GetValidatedCount();
        if (count > Predicates.Length)
            return false;

        var satisfied = 0;
        for (var index = 0; index < Predicates.Length; index++)
        {
            if (Predicates[index].Invoke())
                satisfied++;
            if (satisfied > count)
                return false;

            var remaining = Predicates.Length - index - 1;
            if (satisfied + remaining < count)
                return false;
        }
        return satisfied == count;
    }
}

/// <summary>
/// Returns `true` when at least the requested number of supplied predicates are satisfied by the input. The count must be non-negative, and evaluation stops as soon as the result is known.
/// </summary>
[Predicate(false, prefix: "")]
public class SatisfiesAtLeast : PredicateCardinalityBase
{
    /// <param name="count">Specifies the minimum non-negative number of predicates that must be satisfied.</param>
    /// <param name="predicates">Specifies the predicate expressions evaluated against the same input value, in declaration order.</param>
    public SatisfiesAtLeast(Func<int> count, IEnumerable<Func<bool>> predicates)
        : base(count, predicates) { }

    public override bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Derive(value);
        var count = GetValidatedCount();
        if (count == 0)
            return true;
        if (count > Predicates.Length)
            return false;

        var satisfied = 0;
        foreach (var predicate in Predicates)
        {
            if (predicate.Invoke() && ++satisfied >= count)
                return true;
        }
        return false;
    }
}

/// <summary>
/// Returns `true` when at most the requested number of supplied predicates are satisfied by the input. The count must be non-negative, and evaluation stops as soon as the result is known.
/// </summary>
[Predicate(false, prefix: "")]
public class SatisfiesAtMost : PredicateCardinalityBase
{
    /// <param name="count">Specifies the maximum non-negative number of predicates that may be satisfied.</param>
    /// <param name="predicates">Specifies the predicate expressions evaluated against the same input value, in declaration order.</param>
    public SatisfiesAtMost(Func<int> count, IEnumerable<Func<bool>> predicates)
        : base(count, predicates) { }

    public override bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Derive(value);
        var count = GetValidatedCount();
        if (count >= Predicates.Length)
            return true;

        var satisfied = 0;
        foreach (var predicate in Predicates)
        {
            if (predicate.Invoke() && ++satisfied > count)
                return false;
        }
        return true;
    }
}
