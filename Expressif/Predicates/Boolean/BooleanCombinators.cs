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
