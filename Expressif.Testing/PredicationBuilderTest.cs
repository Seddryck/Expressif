using Expressif.Functions;
using Expressif.Functions.Serializer;
using Expressif.Functions.Text;
using Expressif.Predicates.Combination;
using Expressif.Predicates.Serializer;
using Expressif.Predicates.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing
{
    public class PredicationBuilderTest
    {
        [Test]
        public void As_WithoutParameter_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder().Chain<LowerCase>();
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.False);
        }

        [Test]
        public void As_WithParameter_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder().Chain<StartsWith>("Nik");
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.True);
        }

        //[Test]
        //public void As_WithParameters_CorrectlyEvaluate()
        //{
        //    var builder = new PredicationBuilder().Not<StartsWith>("Nik");
        //    var expression = builder.Build();
        //    Assert.That(expression.Evaluate("Nikola Tesla"), Is.False);
        //}

        [Test]
        public void Chain_CombinationAnd_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("Nik")
                .Chain<AndOperator, EndsWith>("sla");
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_CombinationOr_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, EndsWith>("sla");
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_NonGeneric_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain(typeof(StartsWith), "ola")
                .Chain(typeof(OrOperator), typeof(EndsWith), "sla");
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_SubPredication_CorrectlyEvaluate()
        {
            var subPredicationBuilder = new PredicationBuilder()
                .Chain<StartsWith>("Nik")
                .Chain<AndOperator, EndsWith>("sla"); ;

            var builder = new PredicationBuilder()
                .Chain<LowerCase>()
                .Chain<OrOperator>(subPredicationBuilder)
                .Chain<OrOperator, UpperCase>();

            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.True);
        }


        //[Test]
        //public void Chain_IFunction_CorrectlyEvaluate()
        //{
        //    var builder = new PredicationBuilder();
        //    var expression = builder.Chain(new Lower()).Chain(new FirstChars(new LiteralScalarResolver<int>(5))).Build();
        //    Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol"));
        //}

        [Test]
        public void Serialize_WithParameters_CorrectlyEvaluate()
        {
            var serializer = new Mock<PredicationSerializer>();
            serializer.Setup(x => x.Serialize(It.IsAny<PredicationBuilder>())).Returns("serialization");
            var builder = new PredicationBuilder(serializer: serializer.Object)
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, EndsWith>("sla");
            var str = builder.Serialize();
            serializer.Verify(x => x.Serialize(builder), Times.Once);
        }
    }
}
