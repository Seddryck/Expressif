using Expressif.Values.Types;

namespace Expressif.Values;

/// <summary>Represents the relative ordering of a left-hand value compared with a right-hand value.</summary>
[ExpressifType(
    Name = "ordering",
    Parent = "scalar",
    LiteralSyntax = "#less, #equal, or #greater",
    LiteralExamples = ["#less", "#equal", "#greater"])]
public sealed class OrderingValue : IExpressifValueType
{
    /// <summary>Gets the ordering where the left-hand value sorts before the right-hand value.</summary>
    /// <value>The less ordering value.</value>
    public static OrderingValue Less { get; } = new("#less", -1);

    /// <summary>Gets the ordering where the two values have equal ordering.</summary>
    /// <value>The equal ordering value.</value>
    public static OrderingValue Equal { get; } = new("#equal", 0);

    /// <summary>Gets the ordering where the left-hand value sorts after the right-hand value.</summary>
    /// <value>The greater ordering value.</value>
    public static OrderingValue Greater { get; } = new("#greater", 1);

    internal int NumericValue { get; }
    private string Literal { get; }

    private OrderingValue(string literal, int numericValue)
        => (Literal, NumericValue) = (literal, numericValue);

    /// <summary>Returns the canonical ordering literal.</summary>
    public override string ToString() => Literal;
}
