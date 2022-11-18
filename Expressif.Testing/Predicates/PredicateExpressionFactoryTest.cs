using Expressif.Parsers;
using System.Diagnostics;
using Expressif.Predicates;
using Expressif.Predicates.Numeric;

namespace Expressif.Testing.Predicates
{
    public class PredicateExpressionFactoryTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        [TestCase("equal-to", typeof(EqualTo))]
        [TestCase("greater-than", typeof(GreaterThan))]
        public void GetFunctionType_FunctionName_Valid(string value, Type expected)
            => Assert.That(new PredicateExpressionFactory().GetFunctionType(value), Is.EqualTo(expected));

        [Test]
        [TestCase("even")]
        [TestCase("Even")]
        [TestCase("numeric-is-even")]
        public void GetFunctionType_FunctionNameVariations_Valid(string value)
            => Assert.That(new PredicateExpressionFactory().GetFunctionType(value), Is.EqualTo(typeof(Even)));

        [Test]
        [TestCase("foo")]
        [TestCase("foo-to-bar")]
        [TestCase("foo - to - bar")]
        public void GetFunctionType_FunctionName_Invalid(string value)
            => Assert.That(() => new PredicateExpressionFactory().GetFunctionType(value), Throws.TypeOf<NotImplementedFunctionException>());

        [Test]
        [TestCase(typeof(Even), 0)]
        [TestCase(typeof(EqualTo), 1)]
        [TestCase(typeof(Modulo), 1)]
        [TestCase(typeof(Modulo), 2)]
        public void GetMatchingConstructor_TypeAndParams_Valid(Type type, int paramCount)
        {
            var ctor = new PredicateExpressionFactory().GetMatchingConstructor(type, paramCount);
            Assert.That(ctor, Is.Not.Null);
            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(paramCount));
        }

        [Test]
        [TestCase(typeof(Even), 1)]
        [TestCase(typeof(EqualTo), 0)]
        [TestCase(typeof(Modulo), 3)]
        [TestCase(typeof(Modulo), 0)]
        public void GetMatchingConstructor_TypeAndParams_Invalid(Type type, int paramCount)
            => Assert.That(() => new PredicateExpressionFactory().GetMatchingConstructor(type, paramCount), Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());


        [Test]
        public void Instantiate_NumericEqualToLiteralParameter_Valid()
        {
            var predicate = new PredicateExpressionFactory().Instantiate(typeof(EqualTo), new[] { new LiteralParameter("1") }, new Context());
            Assert.That(predicate, Is.Not.Null);
            Assert.That(predicate, Is.TypeOf<EqualTo>());
            Assert.That((predicate as EqualTo)!.Reference.Execute(), Is.EqualTo(1));
        }

        [Test]
        public void Instantiate_NumericEqualToVariableParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<int>("myVar", 2);
            var predicate = new PredicateExpressionFactory().Instantiate(typeof(EqualTo), new[] { new VariableParameter("myVar") }, context);
            Assert.That(predicate, Is.Not.Null);
            Assert.That(predicate, Is.TypeOf<EqualTo>());
            Assert.That((predicate as EqualTo)!.Reference.Execute(), Is.EqualTo(2));
        }

        [Test]
        public void Instantiate_NumericEqualToObjectPropertyParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new { Digits = 3 });
            var predicate = new PredicateExpressionFactory().Instantiate(typeof(EqualTo), new[] { new ObjectPropertyParameter("Digits") }, context);
            Assert.That(predicate, Is.Not.Null);
            Assert.That(predicate, Is.TypeOf<EqualTo>());
            Assert.That((predicate as EqualTo)!.Reference.Execute(), Is.EqualTo(3));
        }


        [Test]
        public void Instantiate_NumericEqualToObjectIndexParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new List<int> { 0, 4 });
            var predicate = new PredicateExpressionFactory().Instantiate(typeof(EqualTo), new[] { new ObjectIndexParameter(1) }, context);
            Assert.That(predicate, Is.Not.Null);
            Assert.That(predicate, Is.TypeOf<EqualTo>());
            Assert.That((predicate as EqualTo)!.Reference.Execute(), Is.EqualTo(4));
        }

        //[Test]
        //public void Instantiate_NumericToRoundExpressionParameter_Valid()
        //{
        //    var context = new Context();
        //    context.Variables.Add<int>("myVar", 4);
        //    var subFunction = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar"), new[] { new Function("numeric-to-increment", Array.Empty<IParameter>()) }));
        //    var function = new ExpressionFactory().Instantiate(typeof(NumericToRound), new[] { subFunction }, context);
        //    Assert.That(function, Is.Not.Null);
        //    Assert.That(function, Is.TypeOf<NumericToRound>());
        //    Assert.That((function as NumericToRound)!.Digits.Execute(), Is.EqualTo(5));
        //}

        //[Test]
        //public void Instantiate_NumericToRoundMultipleExpressionParameter_Valid()
        //{
        //    var context = new Context();
        //    context.Variables.Add<int>("myVar1", 4);
        //    context.Variables.Add<int>("myVar2", 5);
        //    var subFunction1 = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar1"), new[] { new Function("numeric-to-decrement", Array.Empty<IParameter>()) }));
        //    var subFunction2 = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar2"), new[] { new Function("numeric-to-increment", Array.Empty<IParameter>()) }));
        //    var subFunction3 = new InputExpressionParameter(new InputExpression(new VariableParameter("myVar1"), new[] { new Function("numeric-to-add", new IParameter[] { subFunction1 }), new Function("numeric-to-multiply", new IParameter[] { subFunction2 }) }));
        //    var function = new ExpressionFactory().Instantiate(typeof(NumericToRound), new[] { subFunction3 }, context);
        //    Assert.That(function, Is.Not.Null);
        //    Assert.That(function, Is.TypeOf<NumericToRound>());
        //    Assert.That((function as NumericToRound)!.Digits.Execute(), Is.EqualTo(42)); // (4+3)*6
        //}

    }
}