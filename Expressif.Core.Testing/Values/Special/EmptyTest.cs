using Expressif.Values.Special;

namespace Expressif.Testing.Values.Special;

public class EmptyTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Equals_Empty_Valid()
        => Assert.That(Empty.Instance.Equals(string.Empty), Is.True);

    [Test]
    public void EqualOperator_Null_Valid()
        => Assert.That(Empty.Instance.Equals(string.Empty), Is.True);

    [Test]
    public void Equals_EmptyLiteral_Valid()
        => Assert.That(Empty.Instance.Equals("(empty)"), Is.True);

    [Test]
    public void EqualOperator_EmptyLiteral_Valid()
        => Assert.That(Empty.Instance.Equals("(empty)"), Is.True);
}
