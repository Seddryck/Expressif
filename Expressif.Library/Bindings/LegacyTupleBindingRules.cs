using Expressif.Library.Tuple;

namespace Expressif.Bindings;

internal static class LegacyTupleBindingRules
{
    public static bool IsCandidate(string consumer, OpenExpression expression)
    {
        var rule = Semantics.UsageLifecycle.Find(consumer);
        if (rule is null) return false;
        if (expression.InputBinding is not null
            || TupleBindingPatternRecognizer.TryMatchLeading(expression, out _))
        {
            return false;
        }
        var members = expression.Members.ToArray();
        return members is [{ Parameters.Count: 0 }, ..]
            && (rule.AllowFollowingStages || members.Length == 1);
    }

    public static bool HasBinarySignature(Type type)
        => type.GetConstructors().Any(constructor => constructor.GetParameters().Length == 1);
}
