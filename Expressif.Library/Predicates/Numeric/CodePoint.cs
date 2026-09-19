namespace Expressif.Predicates.Numeric;

/// <summary>
/// Returns `true` when the input is an integer Unicode scalar value. Returns `false` otherwise.
/// </summary>
[Predicate(prefix: "")]
public sealed class CodePoint : BaseNumericPredicate
{
    protected override bool EvaluateNumeric(decimal value)
        => value == decimal.Truncate(value)
            && value is >= 0 and <= 0x10FFFF
            && value is not (>= 0xD800 and <= 0xDFFF);
}
