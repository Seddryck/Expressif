using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif.Library.SemVer;

public abstract class SemVerPredicate(Func<SemanticVersion?> reference) : IPredicate<SemanticVersion>
{
    protected SemanticVersion? Reference => reference.Invoke();

    public bool Evaluate(SemanticVersion? value)
        => value is not null && Reference is { } other && Evaluate(value, other);

    public bool Evaluate(object? value) => Evaluate(value as SemanticVersion);
    object? IFunction.Evaluate(object? value) => Evaluate(value);

    protected abstract bool Evaluate(SemanticVersion value, SemanticVersion reference);
}

/// <summary>Tests full semantic-version equality, including build metadata.</summary>
[Predicate(appendIs: false, prefix: "", name: "same-version")]
[Scope("semver")]
public sealed class SameVersion(Func<SemanticVersion?> version) : SemVerPredicate(version)
{
    protected override bool Evaluate(SemanticVersion value, SemanticVersion reference)
        => value.Equals(reference);
}

/// <summary>Tests whether the input has lower SemVer precedence than the parameter.</summary>
[Predicate(appendIs: false, prefix: "", name: "lower-precedence")]
[Scope("semver")]
public sealed class LowerPrecedence(Func<SemanticVersion?> version) : SemVerPredicate(version)
{
    protected override bool Evaluate(SemanticVersion value, SemanticVersion reference)
        => value.CompareTo(reference) < 0;
}

/// <summary>Tests whether the input has higher SemVer precedence than the parameter.</summary>
[Predicate(appendIs: false, prefix: "", name: "higher-precedence")]
[Scope("semver")]
public sealed class HigherPrecedence(Func<SemanticVersion?> version) : SemVerPredicate(version)
{
    protected override bool Evaluate(SemanticVersion value, SemanticVersion reference)
        => value.CompareTo(reference) > 0;
}

/// <summary>Tests equal SemVer precedence while ignoring build metadata.</summary>
[Predicate(appendIs: false, prefix: "", name: "same-precedence")]
[Scope("semver")]
public sealed class SamePrecedence(Func<SemanticVersion?> version) : SemVerPredicate(version)
{
    protected override bool Evaluate(SemanticVersion value, SemanticVersion reference)
        => value.CompareTo(reference) == 0;
}

/// <summary>Tests whether the major components match.</summary>
[Predicate(appendIs: false, prefix: "", name: "same-major")]
[Scope("semver")]
public sealed class SameMajor(Func<SemanticVersion?> version) : SemVerPredicate(version)
{
    protected override bool Evaluate(SemanticVersion value, SemanticVersion reference)
        => value.Major == reference.Major;
}

/// <summary>Tests whether the major and minor components match.</summary>
[Predicate(appendIs: false, prefix: "", name: "same-minor")]
[Scope("semver")]
public sealed class SameMinor(Func<SemanticVersion?> version) : SemVerPredicate(version)
{
    protected override bool Evaluate(SemanticVersion value, SemanticVersion reference)
        => value.Major == reference.Major && value.Minor == reference.Minor;
}
