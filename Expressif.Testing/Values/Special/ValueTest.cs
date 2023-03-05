using Expressif.Values.Special;

namespace Expressif.Testing.Values.Special
{
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
            => Assert.That(new Value().Equals(value), Is.True);

        [Test]
        [TestCase(null)]
        [TestCase("(null)")]
        public void Equals_Null_Invalid(object value)
            => Assert.That(new Value().Equals(value), Is.False);

        [Test]
        [TestCase("foo")]
        [TestCase("(empty)")]
        [TestCase("(blank)")]
        [TestCase("(value)")]
        public void EqualOperator_Value_Valid(object value)
            => Assert.That(new Value() == value, Is.True);

        [Test]
        [TestCase(null)]
        [TestCase("(null)")]
        public void EqualOperator_Null_Invalid(object value)
            => Assert.That(new Value() == value, Is.False);

        [Test]
        public void Equals_ValueLiteral_Valid()
            => Assert.That(new Value().Equals("(any)"), Is.True);

        [Test]
        public void EqualOperator_ValueLiteral_Valid()
            => Assert.That(new Value() == "(any)", Is.True);
    }
}