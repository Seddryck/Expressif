using Expressif.Values.Special;

namespace Expressif.Testing.Values.Special;

public class AnyTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("foo")]
    [TestCase(null)]
    [TestCase("(null)")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("(any)")]
    [TestCase("(value)")]
    public void Equals_Any_Valid(object? value)
        => Assert.That(new Any().Equals(value), Is.True);

    [Test]
    [TestCase("foo")]
    [TestCase(null)]
    [TestCase("(null)")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("(any)")]
    [TestCase("(value)")]
    public void EqualOperator_Null_Valid(object? value)
        => Assert.That(new Any() == value, Is.True);

    [Test]
    public void Equals_AnyLiteral_Valid()
        => Assert.That(new Any().Equals("(any)"), Is.True);

    [Test]
    public void EqualOperator_AnyLiteral_Valid()
        => Assert.That(new Any() == "(any)", Is.True);
}
