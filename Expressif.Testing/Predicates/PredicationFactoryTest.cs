using Expressif.Parsers;
using System.Diagnostics;
using Expressif.Predicates;
using Expressif.Predicates.Numeric;
using Expressif.Predicates.Text;

namespace Expressif.Testing.Predicates
{
    public class PredicationFactoryTest
    {
        [SetUp]
        public void Setup()
        { }

        
        [Test]
        [TestCase(typeof(Even), 0)]
        [TestCase(typeof(EqualTo), 1)]
        [TestCase(typeof(Modulo), 1)]
        [TestCase(typeof(Modulo), 2)]
        [TestCase(typeof(EquivalentTo), 1)]
        [TestCase(typeof(EquivalentTo), 2)]
        public void GetMatchingConstructor_TypeAndParams_Valid(Type type, int paramCount)
        {
            var ctor = new PredicationFactory().GetMatchingConstructor(type, paramCount);
            Assert.That(ctor, Is.Not.Null);
            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(paramCount));
        }

        [Test]
        [TestCase(typeof(Even), 1)]
        [TestCase(typeof(EqualTo), 0)]
        [TestCase(typeof(Modulo), 3)]
        [TestCase(typeof(Modulo), 0)]
        [TestCase(typeof(EquivalentTo), 0)]
        [TestCase(typeof(EquivalentTo), 3)]
        public void GetMatchingConstructor_TypeAndParams_Invalid(Type type, int paramCount)
            => Assert.That(() => new PredicationFactory().GetMatchingConstructor(type, paramCount), Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());

        [Test]
        public void Instantiate_NumericEqualToLiteralParameter_Valid()
        {
            var predicate = new PredicationFactory().Instantiate(new SinglePredication([new Function("EqualTo", new[] { new LiteralParameter("1") })]), new Context());
            Assert.That(predicate, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(predicate, Is.TypeOf<EqualTo>());
                Assert.That((predicate as EqualTo)!.Reference.Invoke(), Is.EqualTo(1));
            });
        }

        [Test]
        public void Instantiate_NumericEqualToVariableParameter_Valid()
        {
            var context = new Context();
            context.Variables.Add<int>("myVar", 2);
            var predicate = new PredicationFactory().Instantiate(new SinglePredication([new Function("EqualTo", new[] { new VariableParameter("myVar") })]), context);
            Assert.That(predicate, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(predicate, Is.TypeOf<EqualTo>());
                Assert.That((predicate as EqualTo)!.Reference.Invoke(), Is.EqualTo(2));
            });
        }

        [Test]
        public void Instantiate_NumericEqualToObjectPropertyParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new { Digits = 3 });
            var predicate = new PredicationFactory().Instantiate(new SinglePredication([new Function("EqualTo", new[] { new ObjectPropertyParameter("Digits") })]), context);
            Assert.That(predicate, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(predicate, Is.TypeOf<EqualTo>());
                Assert.That((predicate as EqualTo)!.Reference.Invoke(), Is.EqualTo(3));
            });
        }

        [Test]
        public void Instantiate_NumericEqualToObjectIndexParameter_Valid()
        {
            var context = new Context();
            context.CurrentObject.Set(new List<int> { 0, 4 });
            var predicate = new PredicationFactory().Instantiate(new SinglePredication([new Function("EqualTo", new[] { new ObjectIndexParameter(1) })]), context);
            Assert.That(predicate, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(predicate, Is.TypeOf<EqualTo>());
                Assert.That((predicate as EqualTo)!.Reference.Invoke(), Is.EqualTo(4));
            });
        }
    }
}
