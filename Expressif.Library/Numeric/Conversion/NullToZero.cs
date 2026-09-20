using Expressif.Library.Numeric;

namespace Expressif.Library.Numeric.Conversion;

/// <summary>
/// Returns the unmodified argument value except if the argument value is `null`, `empty` or `whitespace` then it returns `0`.
/// </summary>
[Function(prefix: "")]
[Scope("numeric/conversion")]
public class NullToZero : BaseNumericFunction
{
    protected override object EvaluateNull() => 0;
    protected override decimal? EvaluateNumeric(decimal numeric) => numeric;
}
