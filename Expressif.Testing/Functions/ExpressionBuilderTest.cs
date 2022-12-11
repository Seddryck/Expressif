using Expressif.Functions;
using Expressif.Functions.Text;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions
{
    public class ExpressionBuilderTest
    {
        [Test]
        public void As_WithoutParameter_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder().Chain<Lower>();
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla"));
        }

        [Test]
        public void As_WithParameter_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder().Chain<FirstChars>(5);
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikol"));
        }

        [Test]
        public void As_WithParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder().Chain<PadRight>(15, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikola Tesla***"));
        }

        [Test]
        public void Chain_WithParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder()
                .Chain<Lower>()
                .Chain<FirstChars>(5)
                .Chain<PadRight>(7, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
        }

        [Test]
        public void Chain_NotGenericWithParameters_CorrectlyEvaluate()
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
            var subExpressionBuilder = new ExpressionBuilder();
            subExpressionBuilder.Chain<FirstChars>(5).Chain<PadRight>(7, '*');

            var builder = new ExpressionBuilder();
            builder.Chain<Lower>().Chain(subExpressionBuilder).Chain<Upper>();

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
    }   
}
    