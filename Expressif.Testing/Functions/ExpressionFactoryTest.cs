using Expressif.Parsers;
using Expressif.Functions;
using Expressif.Functions.IO;
using Expressif.Functions.Numeric;
using Expressif.Functions.Special;
using Expressif.Functions.Temporal;
using Expressif.Functions.Text;
using System.Diagnostics;


namespace Expressif.Testing.Functions
{
    public class ExpressionFactoryTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase("neutral", typeof(Neutral))]
        [TestCase("null-to-zero", typeof(NullToZero))]
        [TestCase("numeric-to-ceiling", typeof(Ceiling))]
        [TestCase("datetime-to-date", typeof(DateTimeToDate))]
        [TestCase("local-to-utc", typeof(LocalToUtc))]
        [TestCase("text-to-without-diacritics", typeof(WithoutDiacritics))]
        [TestCase("path-to-filename-without-extension", typeof(FilenameWithoutExtension))]
        public void GetFunctionType_FunctionName_Valid(string value, Type expected)
            => Assert.That(new ExpressionFactory().GetFunctionType(value), Is.EqualTo(expected));

        [Test]
        [TestCase("null-to-zero")]
        [TestCase("Null-To-Zero")]
        [TestCase("NULL-To-Zero")]
        [TestCase("null - to - zero")]
        public void GetFunctionType_FunctionNameVariations_Valid(string value)
            => Assert.That(new ExpressionFactory().GetFunctionType(value), Is.EqualTo(typeof(NullToZero)));

        [Test]
        [TestCase("foo")]
        [TestCase("foo-to-bar")]
        [TestCase("foo - to - bar")]
        public void GetFunctionType_FunctionName_Invalid(string value)
            => Assert.That(() => new ExpressionFactory().GetFunctionType(value), Throws.TypeOf<NotImplementedFunctionException>());

        [Test]
        [TestCase(typeof(NullToZero), 0)]
        [TestCase(typeof(Round), 1)]
        [TestCase(typeof(PadRight), 2)]
        [TestCase(typeof(Token), 1)]
        [TestCase(typeof(Token), 2)]
        public void GetMatchingConstructor_TypeAndParams_Valid(Type type, int paramCount)
        {
            var ctor = new ExpressionFactory().GetMatchingConstructor(type, paramCount);
            Assert.That(ctor, Is.Not.Null);
            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(paramCount));
        }

        [Test]
        [TestCase(typeof(NullToZero), 1)]
        [TestCase(typeof(Round), 2)]
        [TestCase(typeof(PadRight), 3)]
        [TestCase(typeof(Token), 0)]
        [TestCase(typeof(Token), 3)]
        public void GetMatchingConstructor_TypeAndParams_Invalid(Type type, int paramCount)
            => Assert.That(() => new ExpressionFactory().GetMatchingConstructor(type, paramCount), Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());


        [Test]
        public void Instantiate_RoundLiteralParameter_Valid()
        {
            var function = new ExpressionFactory().Instantiate(typeof(Round), new[] { new LiteralParameter("1") }, new Context());
            Assert.That(function, Is.Not.Null);
            Assert.That(function, Is.TypeOf<Round>());
            Assert.That((function as Round)!.Digits.Execute(), Is.EqualTo(1));
        }

        [Test]
        public void Instantiate_RoundVariableParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<int>("myVar", 2);
            var function = new ExpressionFactory().Instantiate(typeof(Round), new[] { new VariableParameter("myVar") }, context);
            Assert.That(function, Is.Not.Null);
            Assert.That(function, Is.TypeOf<Round>());
            Assert.That((function as Round)!.Digits.Execute(), Is.EqualTo(2));
        }

        [Test]
        public void Instantiate_RoundObjectPropertyParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new { Digits = 3 });
            var function = new ExpressionFactory().Instantiate(typeof(Round), new[] { new ObjectPropertyParameter("Digits") }, context);
            Assert.That(function, Is.Not.Null);
            Assert.That(function, Is.TypeOf<Round>());
            Assert.That((function as Round)!.Digits.Execute(), Is.EqualTo(3));
        }


        [Test]
        public void Instantiate_RoundObjectIndexParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new List<int> { 0, 4 });
            var function = new ExpressionFactory().Instantiate(typeof(Round), new[] { new ObjectIndexParameter(1) }, context);
            Assert.That(function, Is.Not.Null);
            Assert.That(function, Is.TypeOf<Round>());
            Assert.That((function as Round)!.Digits.Execute(), Is.EqualTo(4));
        }

        [Test]
        public void Instantiate_RoundExpressionParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<int>("myVar", 4);
            var subFunction = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar"), new[] { new Function("numeric-to-increment", Array.Empty<IParameter>()) }));
            var function = new ExpressionFactory().Instantiate(typeof(Round), new[] { subFunction }, context);
            Assert.That(function, Is.Not.Null);
            Assert.That(function, Is.TypeOf<Round>());
            Assert.That((function as Round)!.Digits.Execute(), Is.EqualTo(5));
        }

        [Test]
        public void Instantiate_RoundMultipleExpressionParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<int>("myVar1", 4);
            context.Variables.Add<int>("myVar2", 5);
            var subFunction1 = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar1"), new[] { new Function("numeric-to-decrement", Array.Empty<IParameter>()) }));
            var subFunction2 = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar2"), new[] { new Function("numeric-to-increment", Array.Empty<IParameter>()) }));
            var subFunction3 = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar1"), new[] { new Function("numeric-to-add", new IParameter[] { subFunction1 }), new Function("numeric-to-multiply", new IParameter[] { subFunction2 }) }));
            var function = new ExpressionFactory().Instantiate(typeof(Round), new[] { subFunction3 }, context);
            Assert.That(function, Is.Not.Null);
            Assert.That(function, Is.TypeOf<Round>());
            Assert.That((function as Round)!.Digits.Execute(), Is.EqualTo(42)); // (4+3)*6
        }

    }
}