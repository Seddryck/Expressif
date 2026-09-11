namespace Expressif.Bindings;

internal static class LegacyTupleBindingRules
{
    public static bool IsCandidate(string consumer, OpenExpression expression)
    {
        if (expression is InputBoundExpression || TupleBindingOperations.LeadingLength(expression) > 0) return false;
        var members = expression.Members.ToArray();
        return members is [{ Parameters.Length: 0 }, ..]
            && (consumer == "chunk-while" || members.Length == 1);
    }

    public static bool HasBinarySignature(Type type)
        => type.GetConstructors().Any(constructor => constructor.GetParameters().Length == 1);
}
