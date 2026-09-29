using Expressif.Values;

namespace Expressif.Testing.Values;

public class VectorValueTest
{
    [Test]
    public void Constructor_EmptyVector_IsValid()
        => Assert.That(new Expressif.Values.Vector(), Has.Count.Zero);

    [Test]
    public void Formatter_Vector_UsesVectorSyntax()
        => Assert.That(ValueFormatter.Format(new VectorValue(1, 2.5m)), Is.EqualTo("V(1, 2.5)"));
}
