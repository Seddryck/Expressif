using System.Reflection;

namespace Expressif.Cli.Tests;

public class VersionFormatterTests
{
    [Test]
    public void GetVersion_InformationalVersion_RemovesBuildMetadata()
    {
        var assembly = new StubAssembly(new Version(1, 2, 3, 4), "5.6.7-preview+abcdef");

        Assert.That(VersionFormatter.GetVersion(assembly), Is.EqualTo("5.6.7-preview"));
    }

    [TestCase(1, 2, -1, -1, "1.2")]
    [TestCase(1, 2, 3, -1, "1.2.3")]
    [TestCase(1, 2, 3, 4, "1.2.3.4")]
    public void GetVersion_AssemblyVersion_UsesAvailableComponents(
        int major,
        int minor,
        int build,
        int revision,
        string expected)
    {
        var version = build < 0
            ? new Version(major, minor)
            : revision < 0
                ? new Version(major, minor, build)
                : new Version(major, minor, build, revision);

        Assert.That(VersionFormatter.GetVersion(new StubAssembly(version)), Is.EqualTo(expected));
    }

    [Test]
    public void GetVersion_MissingVersion_ReturnsUnknown()
        => Assert.That(VersionFormatter.GetVersion(new StubAssembly(null)), Is.EqualTo("unknown"));

    private sealed class StubAssembly(Version? version, string? informationalVersion = null) : Assembly
    {
        public override AssemblyName GetName(bool copiedName) => new() { Version = version };

        public override object[] GetCustomAttributes(bool inherit)
            => informationalVersion is null
                ? Array.Empty<Attribute>()
                : new Attribute[] { new AssemblyInformationalVersionAttribute(informationalVersion) };

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => informationalVersion is not null && attributeType == typeof(AssemblyInformationalVersionAttribute)
                ? new Attribute[] { new AssemblyInformationalVersionAttribute(informationalVersion) }
                : Array.Empty<Attribute>();

        public override bool IsDefined(Type attributeType, bool inherit)
            => informationalVersion is not null && attributeType == typeof(AssemblyInformationalVersionAttribute);
    }
}
