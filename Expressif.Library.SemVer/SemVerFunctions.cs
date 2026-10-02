using Expressif.Functions;
using Expressif.Values;

namespace Expressif.Library.SemVer;

public abstract class SemVerFunction<TOutput> : Function<SemanticVersion, TOutput>;

/// <summary>Increments the major component and resets the remaining components.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class BumpMajor : SemVerFunction<SemanticVersion>
{
    public override SemanticVersion Evaluate(SemanticVersion value)
        => new(checked(value.Major + 1), 0, 0);
}

/// <summary>Increments the minor component and resets the patch component.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class BumpMinor : SemVerFunction<SemanticVersion>
{
    public override SemanticVersion Evaluate(SemanticVersion value)
        => new(value.Major, checked(value.Minor + 1), 0);
}

/// <summary>Increments the patch component.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class BumpPatch : SemVerFunction<SemanticVersion>
{
    public override SemanticVersion Evaluate(SemanticVersion value)
        => new(value.Major, value.Minor, checked(value.Patch + 1));
}

/// <summary>Returns the major component.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class Major : SemVerFunction<int>
{
    public override int Evaluate(SemanticVersion value) => value.Major;
}

/// <summary>Returns the minor component.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class Minor : SemVerFunction<int>
{
    public override int Evaluate(SemanticVersion value) => value.Minor;
}

/// <summary>Returns the patch component.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class Patch : SemVerFunction<int>
{
    public override int Evaluate(SemanticVersion value) => value.Patch;
}

/// <summary>Returns the pre-release component without its leading hyphen.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class PreRelease : SemVerFunction<string>
{
    public override string Evaluate(SemanticVersion value) => value.PreRelease ?? string.Empty;
}

/// <summary>Returns the build metadata component without its leading plus sign.</summary>
[Function(prefix: "")]
[Scope("semver")]
public sealed class BuildMetadata : SemVerFunction<string>
{
    public override string Evaluate(SemanticVersion value) => value.BuildMetadata ?? string.Empty;
}

/// <summary>Compares semantic-version precedence, ignoring build metadata.</summary>
[Function(prefix: "", Name = "compare-semver")]
[Scope("semver")]
public sealed class CompareSemVer(Func<SemanticVersion?> right) : SemVerFunction<OrderingValue?>
{
    public override OrderingValue? Evaluate(SemanticVersion value)
    {
        var other = right.Invoke();
        if (other is null)
            return null;
        var comparison = value.CompareTo(other);
        return comparison < 0 ? OrderingValue.Less
            : comparison > 0 ? OrderingValue.Greater
            : OrderingValue.Equal;
    }
}
