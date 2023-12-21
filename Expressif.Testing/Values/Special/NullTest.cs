using Expressif.Values.Special;
using Sprache;

namespace Expressif.Testing.Values.Special;

public class NullTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Equals_Null_Valid()
        => Assert.That(new Null().Equals(null), Is.True);

    [Test]
    public void EqualOperator_Null_Valid()
        => Assert.That(new Null() == null, Is.True);

    [Test]
    public void Equals_DBNull_Valid()
        => Assert.That(new Null().Equals(DBNull.Value), Is.True);

    [Test]
    public void EqualOperator_DBNull_Valid()
        => Assert.That(new Null() == DBNull.Value, Is.True);

    [Test]
    public void Equals_NullLiteral_Valid()
        => Assert.That(new Null().Equals("(null)"), Is.True);

    [Test]
    public void EqualOperator_NullLiteral_Valid()
        => Assert.That(new Null() == "(null)", Is.True);
}