using NUnit.Framework.Interfaces;

namespace Expressif.Testing.Conformance;

public class ConformanceAttributeTest
{
    [Test]
    public void Attribute_ImpliesFixture()
        => Assert.That(new ConformanceAttribute(), Is.InstanceOf<IImplyFixture>());
}
