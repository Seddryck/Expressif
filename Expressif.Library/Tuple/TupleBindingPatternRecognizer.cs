using Expressif.Bindings;
using Expressif.Discovery;

namespace Expressif.Library.Tuple;

internal enum TupleBindingInputPosition
{
    First,
    Last,
}

internal readonly record struct TupleBindingPattern(
    Function Binding,
    TupleBindingInputPosition InputPosition,
    int ConsumedStages);

internal static class TupleBindingPatternRecognizer
{
    private static OperatorIdentity UnqualifiedBindIdentity { get; } = new("global", "bind");
    private static OperatorIdentity ResolvedBindIdentity { get; } = new("tuple", "bind");
    private static OperatorIdentity UnqualifiedRotateIdentity { get; } = new("global", "rotate");
    private static OperatorIdentity ResolvedRotateIdentity { get; } = new("tuple", "rotate");

    internal static bool TryMatchLeading(OpenExpression expression, out TupleBindingPattern pattern)
    {
        pattern = default;
        if (expression.InputBinding is not null)
            return false;

        var members = expression.Members.ToArray();
        if (members is [var binding, ..] && IsBind(binding))
        {
            pattern = new TupleBindingPattern(binding, TupleBindingInputPosition.First, 1);
            return true;
        }

        if (members is [var rotation, var rotatedBinding, ..]
            && IsDefaultRotation(rotation)
            && IsBind(rotatedBinding))
        {
            pattern = new TupleBindingPattern(rotatedBinding, TupleBindingInputPosition.Last, 2);
            return true;
        }

        return false;
    }

    internal static bool IsBind(Function function)
        => function.Identity == UnqualifiedBindIdentity || function.Identity == ResolvedBindIdentity;

    internal static bool IsDefaultRotation(Function function)
        => (function.Identity == UnqualifiedRotateIdentity || function.Identity == ResolvedRotateIdentity)
            && (function.Arguments.Count == 0
                || (function.Arguments is [{ Name: null or "offset", Value: LiteralParameter { Value: { } offset } }]
                    && decimal.TryParse(Convert.ToString(offset, System.Globalization.CultureInfo.InvariantCulture),
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var value)
                    && value == 1));
}
