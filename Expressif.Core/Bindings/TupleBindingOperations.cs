namespace Expressif.Bindings;

/// <summary>Recognizes the canonical leading invocation shared by explicit and shorthand forms.</summary>
public static class TupleBindingOperations
{
    public static int LeadingLength(OpenExpression expression)
    {
        if (expression is InputBoundExpression) return 0;
        var members = expression.Members.ToArray();
        if (members is [var first, ..] && IsBind(first)) return 1;
        return members is [var rotation, var binding, ..] && IsDefaultRotation(rotation) && IsBind(binding) ? 2 : 0;
    }

    private static bool IsBind(Function function)
        => function.Name.Equals("bind", StringComparison.OrdinalIgnoreCase);

    internal static bool IsDefaultRotation(Function function)
        => function.Name.Equals("rotate", StringComparison.OrdinalIgnoreCase)
            && (function.Arguments.Length == 0
                || (function.Arguments is [{ Name: null or "offset", Value: LiteralParameter { Value: { } offset } }]
                    && decimal.TryParse(Convert.ToString(offset, System.Globalization.CultureInfo.InvariantCulture),
                        System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var value) && value == 1));
}
