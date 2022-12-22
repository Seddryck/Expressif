using Expressif.Functions;
using Expressif.Functions.Serializer;
using Expressif.Functions.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing
{
    public class ExpressionBuilderTest
    {
        [Test]
        public void Chain_WithoutParameter_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder().Chain<Lower>();
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla"));
        }

        [Test]
        public void Chain_WithParameter_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder().Chain<FirstChars>(5);
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikol"));
        }

        [Test]
        public void Chain_WithParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder().Chain<PadRight>(15, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikola Tesla***"));
        }

        [Test]
        public void Chain_MultipleWithoutParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder()
                .Chain<Lower>()
                .Chain<Length>();
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo(12));
        }

        [Test]
        public void Chain_Multiple_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder()
                .Chain<Lower>()
                .Chain<FirstChars>(5)
                .Chain<PadRight>(7, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
        }

        [Test]
        public void Chain_NotGeneric_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder()
                .Chain(typeof(Lower))
                .Chain(typeof(FirstChars), 5)
                .Chain(typeof(PadRight), 7, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
        }

        [Test]
        public void Chain_SubExpression_CorrectlyEvaluate()
        {
            var subExpressionBuilder = new ExpressionBuilder()
                .Chain<FirstChars>(5)
                .Chain<PadRight>(7, '*');

            var builder = new ExpressionBuilder()
                .Chain<Lower>()
                .Chain(subExpressionBuilder)
                .Chain<Upper>();

            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("NIKOL**"));
        }


        [Test]
        public void Chain_IFunction_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder();
            var expression = builder.Chain(new Lower()).Chain(new FirstChars(new LiteralScalarResolver<int>(5))).Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol"));
        }

        [Test]
        public void Serialize_WithParameters_CorrectlyEvaluate()
        {
            var serializer = new Mock<ExpressionSerializer>();
            serializer.Setup(x => x.Serialize(It.IsAny<ExpressionBuilder>())).Returns("serialization");
            var builder = new ExpressionBuilder(serializer: serializer.Object)
                .Chain<Lower>()
                .Chain<FirstChars>(5)
                .Chain<PadRight>(7, '*');
            var str = builder.Serialize();
            serializer.Verify(x => x.Serialize(builder), Times.Once);
        }

        [Test]
        public void Serialize_WithParameters_CorrectlySerialized()
        {
            var builder = new ExpressionBuilder()
                .Chain<Lower>()
                .Chain<FirstChars>(5)
                .Chain<PadRight>(7, '*');
            var str = builder.Serialize();
            Assert.That(str, Is.EqualTo("lower | first-chars(5) | pad-right(7, *)"));
        }
    }
}
