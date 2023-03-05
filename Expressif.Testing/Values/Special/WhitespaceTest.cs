using Expressif.Values.Special;
using Sprache;

namespace Expressif.Testing.Values.Special
{
    public class WhitespaceTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase(" ")]
        [TestCase("     ")]
        [TestCase("")]
        [TestCase("\t")]
        [TestCase("\t \t ")]
        [TestCase("\t \r \n")]
        public void Equals_Whitespace_Valid(string value)
            => Assert.That(new Whitespace().Equals(value), Is.True);

        [Test]
        [TestCase(" ")]
        [TestCase("     ")]
        [TestCase("")]
        [TestCase("\t")]
        [TestCase("\t \t ")]
        [TestCase("\t \r \n")]
        public void EqualOperator_Whitespace_Valid(string value)
            => Assert.That(new Whitespace() == value, Is.True);

        [Test]
        public void Equals_WhitespaceLiteral_Valid()
            => Assert.That(new Whitespace().Equals("(blank)"), Is.True);

        [Test]
        public void EqualOperator_WhitespaceLiteral_Valid()
            => Assert.That(new Whitespace() == "(blank)", Is.True);
    }
}