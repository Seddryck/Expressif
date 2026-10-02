using Expressif.Functions;

namespace Expressif.Library.SemVer.Testing;

[Function(prefix: "", Name = "bump-patch")]
[Scope("semver")]
public sealed class CollisionFunction : Function<SemanticVersion, SemanticVersion>
{
    public override SemanticVersion Evaluate(SemanticVersion value) => value;
}
