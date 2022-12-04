using Expressif.Predicates;
using Expressif.Predicates.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Combination
{
    public class OrOperatorTest
    {
        [Test]
        [TestCase(true, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, true)]
        public void Evaluate_Value_Success(bool state, bool value, bool expected)
        {
            var factory = new Mock<PredicationFactory>();
            var predication = new Mock<Predication>(new object[] { "any-code", new Context(), factory.Object });
            predication.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(value);
            factory.Setup(x => x.Instantiate(It.IsAny<string>(), It.IsAny<Context>())).Returns(predication.Object);
            var @operator = new OrOperator(predication.Object);
            
            Assert.That(@operator.Evaluate(state, "my value"), Is.EqualTo(expected));
        }

        [Test]
        public void Evaluate_StateIsTrue_EvaluatePredicate()
        {
            var factory = new Mock<PredicationFactory>();
            var predication = new Mock<Predication>(new object[] { "any-code", new Context(), factory.Object });
            predication.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(true);
            factory.Setup(x => x.Instantiate(It.IsAny<string>(), It.IsAny<Context>())).Returns(predication.Object);
            var @operator = new OrOperator(predication.Object);
            @operator.Evaluate(true, "my value");

            predication.Verify(x => x.Evaluate(It.IsAny<object>()), Times.Never());
        }

        [Test]
        public void Evaluate_StateIsFalse_EvaluatePredicate()
        {
            var factory = new Mock<PredicationFactory>();
            var predication = new Mock<Predication>(new object[] { "any-code", new Context(), factory.Object });
            predication.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(true);
            factory.Setup(x => x.Instantiate(It.IsAny<string>(), It.IsAny<Context>())).Returns(predication.Object);
            var @operator = new OrOperator(predication.Object);
            @operator.Evaluate(false, "my value");

            predication.Verify(x => x.Evaluate(It.IsAny<object>()), Times.Once());
        }
    }
}
