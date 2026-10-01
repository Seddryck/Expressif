using Expressif.Discovery;

namespace Expressif.Testing.Discovery;

public sealed class OperatorIdentityTest
{
    [Test]
    public void Constructor_NormalizesNamespaceAndName()
    {
        var identity = new OperatorIdentity("Numeric Values", "SquareRoot");

        Assert.Multiple(() =>
        {
            Assert.That(identity.Namespace, Is.EqualTo("numeric-values"));
            Assert.That(identity.Name, Is.EqualTo("square-root"));
            Assert.That(identity.CanonicalName, Is.EqualTo("numeric-values::square-root"));
        });
    }

    [Test]
    public void Registry_SameShortNameInDifferentNamespaces_RequiresQualifiedLookup()
    {
        var registry = new ImplementationRegistry(
        [
            new ImplementationRegistration("first", "shared", typeof(First)),
            new ImplementationRegistration("second", "shared", typeof(Second)),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(registry.TryResolve("shared", out _), Is.False);
            Assert.That(registry.Resolve(new OperatorIdentity("first", "shared")), Is.EqualTo(typeof(First)));
            Assert.That(registry.Resolve(new OperatorIdentity("second", "shared")), Is.EqualTo(typeof(Second)));
        });
    }

    [Test]
    public void Registry_SameTypeInDifferentNamespaces_StillRequiresQualifiedLookup()
    {
        var registry = new ImplementationRegistry(
        [
            new ImplementationRegistration("first", "shared", typeof(First)),
            new ImplementationRegistration("second", "shared", typeof(First)),
        ]);

        Assert.That(registry.TryResolve("shared", out _), Is.False);
    }

    [Test]
    public void CompositeRegistry_UnqualifiedCollision_DoesNotDependOnRegistryOrder()
    {
        var first = new ImplementationRegistry(
            [new ImplementationRegistration("first", "shared", typeof(First))]);
        var second = new ImplementationRegistry(
            [new ImplementationRegistration("second", "shared", typeof(Second))]);

        Assert.Multiple(() =>
        {
            Assert.That(new CompositeImplementationRegistry(first, second).TryResolve("shared", out _), Is.False);
            Assert.That(new CompositeImplementationRegistry(second, first).TryResolve("shared", out _), Is.False);
        });
    }

    private sealed class First;
    private sealed class Second;
}
