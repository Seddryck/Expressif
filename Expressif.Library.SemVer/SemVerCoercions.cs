using Expressif.Functions;
using Expressif.Functions.Coercions;

namespace Expressif.Library.SemVer;

/// <summary>Attempts to convert text or an existing semantic version to a semantic version.</summary>
[Function(prefix: "", Name = "coerce-semver")]
[Scope("semver")]
public sealed class CoerceSemVer : IFunction<object?, SemanticVersion?>,
    IFunction<string, SemanticVersion?>,
    IFunction<SemanticVersion, SemanticVersion?>
{
    public SemanticVersion? Evaluate(object? value)
        => value switch
        {
            SemanticVersion version => version,
            string text when SemanticVersion.TryParse(text, out var version) => version,
            _ => null,
        };

    SemanticVersion? IFunction<string, SemanticVersion?>.Evaluate(string value) => Evaluate(value);
    SemanticVersion? IFunction<SemanticVersion, SemanticVersion?>.Evaluate(SemanticVersion value) => value;
    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>Converts a semantic version to its canonical text.</summary>
[Function(prefix: "", Name = "semver-to-text")]
[Scope("semver")]
public sealed class SemVerToText : Function<SemanticVersion, string>
{
    public override string Evaluate(SemanticVersion value) => value.ToString();
}

public sealed class CoerceSemVerDescriptor : CoercionDescriptor
{
    public CoerceSemVerDescriptor()
        : base(
            "coerce-semver",
            typeof(SemanticVersion),
            [typeof(string), typeof(SemanticVersion)],
            _ => typeof(CoerceSemVer),
            _ => new CoerceSemVer()) { }
}

public sealed class SemVerToTextDescriptor : CoercionDescriptor
{
    public SemVerToTextDescriptor()
        : base(
            "semver-to-text",
            typeof(string),
            [typeof(SemanticVersion)],
            _ => typeof(SemVerToText),
            _ => new SemVerToText()) { }
}
