using Expressif.Values.Special;
using Sprache;

namespace Expressif.Testing.Values.Special;

public class EmptyTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Equals_Empty_Valid()
        => Assert.That(new Empty().Equals(string.Empty), Is.True);

    [Test]
    public void EqualOperator_Null_Valid()
        => Assert.That(new Empty() == string.Empty, Is.True);

    [Test]
    public void Equals_EmptyLiteral_Valid()
        => Assert.That(new Empty().Equals("(empty)"), Is.True);

    [Test]
    public void EqualOperator_EmptyLiteral_Valid()
        => Assert.That(new Empty() == "(empty)", Is.True);
}