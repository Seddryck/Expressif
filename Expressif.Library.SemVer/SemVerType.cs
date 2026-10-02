using Expressif.Types;
using Expressif.Values.Types;

namespace Expressif.Library.SemVer;

/// <summary>A semantic version with major, minor, patch, pre-release, and build components.</summary>
[ExpressifType(
    Name = "semver",
    Parent = "scalar",
    LiteralSyntax = "#\"major.minor.patch[-pre-release][+build]\":semver",
    LiteralExamples = ["#\"1.25.0+abc\":semver"])]
public sealed class SemVerTypeDescriptor : ExpressifTypeDefinition<SemanticVersion>;

public sealed class SemVerLiteralParser : IQuotedLiteralParser
{
    public string TypeName => "semver";
    public Type RuntimeType => typeof(SemanticVersion);

    public bool TryParse(string representation, out object? value)
    {
        var accepted = SemanticVersion.TryParse(representation, out var version);
        value = version;
        return accepted;
    }

    public string Format(object value) => ((SemanticVersion)value).ToString();
}
