using Expressif.Values.Special;

namespace Expressif.Testing.Values.Special;

public class ValueTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("foo")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("(value)")]
    public void Equals_Value_Valid(object value)
        => Assert.That(Value.Instance.Equals(value), Is.True);

    [Test]
    [TestCase(null)]
    [TestCase("(null)")]
    public void Equals_Null_Invalid(object? value)
        => Assert.That(Value.Instance.Equals(value), Is.False);

    [Test]
    [TestCase("foo")]
    [TestCase("(empty)")]
    [TestCase("(blank)")]
    [TestCase("(value)")]
    public void EqualOperator_Value_Valid(object? value)
        => Assert.That(Value.Instance == value, Is.True);

    [Test]
    [TestCase(null)]
    [TestCase("(null)")]
    public void EqualOperator_Null_Invalid(object? value)
        => Assert.That(Value.Instance == value, Is.False);

    [Test]
    public void Equals_ValueLiteral_Valid()
        => Assert.That(Value.Instance.Equals("(any)"), Is.True);

    [Test]
    public void EqualOperator_ValueLiteral_Valid()
        => Assert.That(Value.Instance == "(any)", Is.True);
}
