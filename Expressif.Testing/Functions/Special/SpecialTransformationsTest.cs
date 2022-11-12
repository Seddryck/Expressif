using Expressif.Functions.Special;
using Expressif.Values.Special;

namespace Expressif.Testing.Functions.Special
{
    [TestFixture]
    public class SpecialTransformationsTest
    {

        [Test]
        [TestCase("foo")]
        [TestCase("(any)")]
        [TestCase("(empty)")]
        [TestCase("(blank)")]
        [TestCase("(value)")]
        [TestCase("(null)")]
        [TestCase(null)]
        [TestCase(150)]
        public void AnyToAny_Any(object value)
            => Assert.That(new AnyToAny().Evaluate(value), Is.EqualTo(new Any()));


        [Test]
        [TestCase("foo")]
        [TestCase("(any)")]
        [TestCase("(empty)")]
        [TestCase("(blank)")]
        [TestCase("(value)")]
        [TestCase(150)]
        public void ValueToValue_NotNull_Value(object value)
            => Assert.That(new ValueToValue().Evaluate(value), Is.EqualTo(new Value()));

        [Test]
        [TestCase("(null)")]
        [TestCase(null)]
        public void ValueToValue_Null_Null(object value)
            => Assert.That(new ValueToValue().Evaluate(value), Is.EqualTo(new Null()));

        [Test]
        [TestCase("(null)")]
        [TestCase(null)]
        public void NullToValue_Null_Value(object value)
            => Assert.That(new NullToValue().Evaluate(value), Is.EqualTo(new Value()));

        [Test]
        [TestCase("foo")]
        [TestCase("(empty)")]
        [TestCase("(blank)")]
        [TestCase("(value)")]
        public void NullToValue_NotNull_Unchanged(object value)
            => Assert.That(new NullToValue().Evaluate(value), Is.EqualTo(new Value()));
    }
}
