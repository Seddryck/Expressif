namespace Expressif.Library.Special;

/// <summary>
/// Returns true when the input does not satisfy is-null.
/// </summary>
[Predicate(prefix: "")]
public class NotNull : BasePredicate
{
    public override bool Evaluate(object? value)
        => !new Null().Evaluate(value);
}
