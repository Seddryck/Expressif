using Expressif.Parsers;
using Expressif.Functions;
using Expressif.Functions.IO;
using Expressif.Functions.Numeric;
using Expressif.Functions.Special;
using Expressif.Functions.Temporal;
using Expressif.Functions.Text;
using System.Diagnostics;


namespace Expressif.Testing.Parsers
{
    public class FunctionFactoryTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase("neutral", typeof(Neutral))]
        [TestCase("null-to-zero", typeof(NullToZero))]
        [TestCase("numeric-to-ceiling", typeof(NumericToCeiling))]
        [TestCase("datetime-to-date", typeof(DateTimeToDate))]
        [TestCase("local-to-utc", typeof(LocalToUtc))]
        [TestCase("text-to-without-diacritics", typeof(TextToWithoutDiacritics))]
        [TestCase("path-to-filename-without-extension", typeof(PathToFilenameWithoutExtension))]
        public void GetFunctionType_FunctionName_Valid(string value, Type expected)
            => Assert.That(new FunctionFactory().GetFunctionType(value), Is.EqualTo(expected));

        [Test]
        [TestCase("null-to-zero")]
        [TestCase("Null-To-Zero")]
        [TestCase("null - to - zero")]
        public void GetFunctionType_FunctionNameVariations_Valid(string value)
            => Assert.That(new FunctionFactory().GetFunctionType(value), Is.EqualTo(typeof(NullToZero)));

        [Test]
        [TestCase("foo")]
        [TestCase("foo-to-bar")]
        [TestCase("foo - to - bar")]
        public void GetFunctionType_FunctionName_Invalid(string value)
            => Assert.That(() => new FunctionFactory().GetFunctionType(value), Throws.TypeOf<NotImplementedFunctionException>());

        [Test]
        [TestCase(typeof(NullToZero), 0)]
        [TestCase(typeof(NumericToRound), 1)]
        [TestCase(typeof(TextToPadRight), 2)]
        [TestCase(typeof(TextToToken), 1)]
        [TestCase(typeof(TextToToken), 2)]
        public void GetMatchingConstructor_TypeAndParams_Valid(Type type, int paramCount)
        {
            var ctor = new FunctionFactory().GetMatchingConstructor(type, paramCount);
            Assert.That(ctor, Is.Not.Null);
            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(paramCount));
        }

        [Test]
        [TestCase(typeof(NullToZero), 1)]
        [TestCase(typeof(NumericToRound), 2)]
        [TestCase(typeof(TextToPadRight), 3)]
        [TestCase(typeof(TextToToken), 0)]
        [TestCase(typeof(TextToToken), 3)]
        public void GetMatchingConstructor_TypeAndParams_Invalid(Type type, int paramCount)
            => Assert.That(() => new FunctionFactory().GetMatchingConstructor(type, paramCount), Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());

    }
}