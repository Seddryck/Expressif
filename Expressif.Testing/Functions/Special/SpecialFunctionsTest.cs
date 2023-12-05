using Expressif.Functions.Special;
using Expressif.Values.Special;
using System.Reflection;

namespace Expressif.Testing.Functions.Special
{
    [TestFixture]
    public class SpecialFunctionsTest
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
        public void AnyToAny_Any(object? value)
            => Assert.That(new AnyToAny().Evaluate(value), Is.EqualTo(new Any()));

        [Test]
        [TestCase(typeof(Null))]
        [TestCase(typeof(Empty))]
        [TestCase(typeof(Whitespace))]
        [TestCase(typeof(Any))]
        [TestCase(typeof(Value))]
        public void AnyToAny_SpecialType_Any(Type type)
            => Assert.That(new AnyToAny().Evaluate(
                type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<Type>()))
                , Is.EqualTo(new Any()));

        [Test]
        [TestCase(typeof(DBNull))]
        public void AnyToAny_DBNull_Any(Type type)
            => Assert.That(new AnyToAny().Evaluate(
                type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
                , Is.EqualTo(new Any()));

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
        [TestCase(typeof(Empty))]
        [TestCase(typeof(Whitespace))]
        [TestCase(typeof(Any))]
        [TestCase(typeof(Value))]
        public void ValueToValue_SpecialType_Value(Type type)
            => Assert.That(new ValueToValue().Evaluate(
                type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<Type>()))
                , Is.EqualTo(new Value()));

        [Test]
        [TestCase("(null)")]
        [TestCase(null)]
        public void ValueToValue_Null_Null(object? value)
            => Assert.That(new ValueToValue().Evaluate(value), Is.EqualTo(new Null()));

        [Test]
        [TestCase(typeof(Null))]
        public void ValueToValue_SpecialType_Null(Type type)
            => Assert.That(new ValueToValue().Evaluate(
                type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<Type>()))
                , Is.EqualTo(new Null()));

        [Test]
        [TestCase(typeof(DBNull))]
        public void ValueToValue_DBNull_Null(Type type)
            => Assert.That(new ValueToValue().Evaluate(
                type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
                , Is.EqualTo(new Null()));

        [Test]
        [TestCase("(null)")]
        [TestCase(null)]
        public void NullToValue_Null_Value(object? value)
            => Assert.That(new NullToValue().Evaluate(value), Is.EqualTo(new Value()));

        [Test]
        [TestCase(typeof(Null))]
        public void NullToValue_SpecialType_Null(Type type)
            => Assert.That(new NullToValue().Evaluate(
                type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<Type>()))
                , Is.EqualTo(new Value()));

        [Test]
        [TestCase(typeof(DBNull))]
        public void NullToValue_DBNull_Null(Type type)
            => Assert.That(new NullToValue().Evaluate(
                type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
                , Is.EqualTo(new Value()));

        [Test]
        [TestCase("foo")]
        [TestCase("(empty)")]
        [TestCase("(blank)")]
        [TestCase("(value)")]
        public void NullToValue_NotNull_Value(object value)
            => Assert.That(new NullToValue().Evaluate(value), Is.EqualTo(value));

        [Test]
        [TestCase(typeof(Empty))]
        [TestCase(typeof(Whitespace))]
        [TestCase(typeof(Any))]
        [TestCase(typeof(Value))]
        public void NullToValue_SpecialType_Value(Type type)
        {
            var obj = type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<Type>());
            Assert.That(new NullToValue().Evaluate(obj), Is.EqualTo(obj));
        }
    }
}
