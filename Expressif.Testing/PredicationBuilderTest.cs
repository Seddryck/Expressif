using Expressif.Functions.Text;
using Expressif.Predicates.Combination;
using Expressif.Predicates.Serializer;
using Expressif.Predicates.Text;
using NUnit.Framework;
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
        public void Chain_WithoutParameter_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder().Chain<LowerCase>();
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.False);
        }

        [Test]
        public void Chain_WithParameter_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder().Chain<StartsWith>("Nik");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Ever_WithParameter_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder().Ever<StartsWith>("Nik");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Not_WithParameters_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder().Not<StartsWith>("Nik");
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.False);
        }

        [Test]
        public void Chain_CombinationAnd_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("Nik")
                .Chain<AndOperator, EndsWith>("sla");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_CombinationOr_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, EndsWith>("sla");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void AndOrXor_Generic_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Or<EndsWith>("sla")
                .And<SortedAfter>("Alan Turing")
                .Xor<SortedBefore>("Marie Curie");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void AndOrXor_NonGeneric_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Or(typeof(EndsWith), ("sla"))
                .And(typeof(SortedAfter), "Alan Turing")
                .Xor(typeof(SortedBefore), "Marie Curie");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_NonGeneric_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain(typeof(StartsWith), "ola")
                .Chain(typeof(OrOperator), typeof(EndsWith), "sla");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_NegateGeneric_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, NotOperator, EndsWith>("Tes");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_NegateGenericFluent_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Or<NotOperator, EndsWith>("Tes");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_NegateGenericNotFluent_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, NotOperator, EndsWith>("Tes");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Chain_NegateType_CorrectlyEvaluate()
        {
            var builder = new PredicationBuilder()
                .Chain(typeof(StartsWith), "ola")
                .Chain(typeof(OrOperator), typeof(NotOperator), typeof(EndsWith), "Tes");
            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
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

            var predication = builder.Build();
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        }

        [Test]
        public void Serialize_SubPredication_CorrectlySerialized()
        {
            var subPredicationBuilder = new PredicationBuilder()
                .Chain<StartsWith>("Nik")
                .Chain<AndOperator, EndsWith>("sla"); ;

            var builder = new PredicationBuilder()
                .Chain<LowerCase>()
                .Chain<OrOperator>(subPredicationBuilder)
                .Chain<OrOperator, UpperCase>();

            var str = builder.Serialize();
            Assert.That(str, Is.EqualTo("lower-case |OR { starts-with(Nik) |AND ends-with(sla) } |OR upper-case"));
        }

        [Test]
        public void Serialize_SerializerCalledOnce()
        {
            var serializer = new Mock<PredicationSerializer>();
            serializer.Setup(x => x.Serialize(It.IsAny<PredicationBuilder>())).Returns("serialization");
            var builder = new PredicationBuilder(serializer: serializer.Object)
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, EndsWith>("sla");
            var str = builder.Serialize();
            serializer.Verify(x => x.Serialize(builder), Times.Once);
        }

        [Test]
        public void Serialize_Not_CorrectlySerialized()
        {
            var builder = new PredicationBuilder().Not<StartsWith>("Nik");
            var str = builder.Serialize();
            Assert.That(str, Is.EqualTo("!starts-with(Nik)"));
        }

        [Test]
        public void Serialize_Negate_CorrectlySerialized()
        {
            var builder = new PredicationBuilder()
                .Chain<StartsWith>("ola")
                .Chain<OrOperator, NotOperator, EndsWith>("sla");
            var str = builder.Serialize();
            Assert.That(str, Is.EqualTo("starts-with(ola) |OR !ends-with(sla)"));
        }
    }
}
