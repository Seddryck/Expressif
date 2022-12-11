using Expressif.Functions;
using Expressif.Functions.Text;
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
            var builder = new ExpressionBuilder();
            builder.As<Lower>();
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla"));
        }

        [Test]
        public void As_WithParameter_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder();
            builder.As<FirstChars>(5);
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikol"));
        }

        [Test]
        public void As_WithParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder();
            builder.As<PadRight>(15, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("Nikola Tesla***"));
        }

        [Test]
        public void Chain_WithParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder();
            builder.As<Lower>().Chain<FirstChars>(5).Chain<PadRight>(7, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
        }

        [Test]
        public void Chain_NotGenericWithParameters_CorrectlyEvaluate()
        {
            var builder = new ExpressionBuilder();
            builder.As(typeof(Lower)).Chain(typeof(FirstChars), 5).Chain(typeof(PadRight), 7, '*');
            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
        }

        [Test]
        public void Chain_SubExpression_CorrectlyEvaluate()
        {
            var subbuilder = new ExpressionBuilder();
            subbuilder.As<FirstChars>(5).Chain<PadRight>(7, '*');
            
            var builder = new ExpressionBuilder();
            builder.As<Lower>().Chain(subbuilder).Chain<Upper>();

            var expression = builder.Build();
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("NIKOL**"));
        }
    }   
}
    