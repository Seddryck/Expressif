namespace Expressif.Testing.Discovery;

public sealed class ImplementationRegistryTest
{
    [Test]
    public void ResolveNormalizesNamesAndReturnsRegisteredImplementation()
    {
        var registry = new ImplementationRegistry([
            new ImplementationRegistration("SampleDateTime", typeof(SampleFunction)),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(registry.Resolve("sample-dateTime"), Is.EqualTo(typeof(SampleFunction)));
            Assert.That(registry.TryResolve("SAMPLE-DATE-TIME", out var implementation), Is.True);
            Assert.That(implementation, Is.EqualTo(typeof(SampleFunction)));
        });
    }

    [Test]
    public void ResolveUnknownNameThrowsNotImplementedFunctionException()
        => Assert.That(
            () => new ImplementationRegistry([]).Resolve("missing"),
            Throws.TypeOf<NotImplementedFunctionException>());

    [Test]
    public void DuplicateNormalizedNameIsRejected()
        => Assert.That(
            () => new ImplementationRegistry([
                new ImplementationRegistration("sample-name", typeof(SampleFunction)),
                new ImplementationRegistration("SampleName", typeof(OtherFunction)),
            ]),
            Throws.InvalidOperationException.With.Message.Contains("already registered"));

    [Test]
    public void CompositeRegistryUsesFirstRegistryThatContainsName()
    {
        var registry = new CompositeImplementationRegistry(
            new ImplementationRegistry([]),
            new ImplementationRegistry([new ImplementationRegistration("sample", typeof(SampleFunction))]));

        Assert.Multiple(() =>
        {
            Assert.That(registry.Resolve("sample"), Is.EqualTo(typeof(SampleFunction)));
            Assert.That(registry.TryResolve("missing", out _), Is.False);
            Assert.That(() => registry.Resolve("missing"), Throws.TypeOf<NotImplementedFunctionException>());
        });
    }

    private sealed class SampleFunction;
    private sealed class OtherFunction;
}
