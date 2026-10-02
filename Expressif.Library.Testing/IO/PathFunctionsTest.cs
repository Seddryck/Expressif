using Expressif.Library.IO;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.IO;

[TestFixture]
public class PathFunctionsTest
{
    [Conformance]
    public void Filename_Valid(string value, string expected)
        => Assert.That(new Filename().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FilenameWithoutExtension_Valid(string value, string expected)
        => Assert.That(new FilenameWithoutExtension().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Extension_Valid(string value, string expected)
        => Assert.That(new Extension().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Root_Valid(string value, string expected)
        => Assert.That(new Root().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Directory_Valid(string value, string expected)
        => Assert.That(new Expressif.Library.IO.Directory().Evaluate(value), Is.EqualTo(expected));
}
