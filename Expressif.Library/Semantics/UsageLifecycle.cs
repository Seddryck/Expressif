using System.Text.Json;

namespace Expressif.Semantics;

/// <summary>A usage lifecycle rule shared by binding, diagnostics and generated documentation.</summary>
public sealed record UsageLifecycleRule(
    string Id, string DiagnosticCode, string Operator, bool Active,
    string? DeprecatedSince, string IntroducedCommit, string? Sunset,
    bool AllowFollowingStages, string ReplacementTemplate, string Condition,
    string OperatorPath, string MigrationPath, string MigrationConditions,
    IReadOnlyList<UsageMigrationExample> Examples)
{
    public string ReplacementFor(string callable) => ReplacementTemplate.Replace("{callable}", callable, StringComparison.Ordinal);
}

/// <summary>A complete expression replacement and the conditions under which it preserves behavior.</summary>
public sealed record UsageMigrationExample(
    string Deprecated, string Replacement, string Availability, string AppliesWhen, string Expected);

public static class UsageLifecycle
{
    private static readonly Lazy<IReadOnlyList<UsageLifecycleRule>> Rules = new(() =>
    {
        using var stream = typeof(UsageLifecycle).Assembly.GetManifestResourceStream("Expressif.UsageLifecycle.json")
            ?? throw new InvalidOperationException("The shared usage lifecycle resource is missing.");
        return Array.AsReadOnly(JsonSerializer.Deserialize<UsageLifecycleRule[]>(stream)
            ?? throw new InvalidOperationException("The shared usage lifecycle resource is invalid."));
    });

    public static IReadOnlyList<UsageLifecycleRule> All => Rules.Value;

    public static UsageLifecycleRule? Find(string consumer)
        => All.SingleOrDefault(rule => rule.Operator == consumer);
}
