using System.Reflection;
using Expressif.Bindings;
using Expressif.Planning;

namespace Expressif.Testing.Planning;

public sealed class ClosedRecordHierarchyTest
{
    [TestCase(typeof(LogicalValue))]
    [TestCase(typeof(LogicalSchema))]
    [TestCase(typeof(CoercionSpecificationParameter))]
    public void PublicHierarchyRequiresAssemblyInternalVariant(Type baseType)
    {
        var barrier = baseType.GetProperty(
            "IsKnownVariant",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var concreteVariants = baseType.Assembly.GetTypes()
            .Where(type => type != baseType && baseType.IsAssignableFrom(type) && !type.IsAbstract);

        Assert.Multiple(() =>
        {
            Assert.That(barrier, Is.Not.Null);
            Assert.That(barrier!.GetMethod!.IsAssembly, Is.True);
            Assert.That(barrier.GetMethod.IsAbstract, Is.True);
            Assert.That(concreteVariants, Has.All.Property(nameof(Type.IsSealed)).True);
        });
    }
}
