using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Predicates.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Combination
{
    public class AndOperatorTest
    {
        [Test]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void Evaluate_Value_Success(bool state, bool value, bool expected)
        {
            var left = new Mock<IPredicate>();
            left.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(state);
            var right = new Mock<IPredicate>();
            right.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(value);

            var @operator = new AndOperator();
            
            Assert.That(@operator.Evaluate(left.Object, right.Object, "my value"), Is.EqualTo(expected));
        }

        [Test]
        public void Evaluate_LeftIsTrue_EvaluateRightPredicate()
        {
            var left = new Mock<IPredicate>();
            left.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(true);
            var right = new Mock<IPredicate>();
            right.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(false);

            var @operator = new AndOperator();
            @operator.Evaluate(left.Object, right.Object, "my value");

            left.Verify(x => x.Evaluate("my value"), Times.Once());
            right.Verify(x => x.Evaluate("my value"), Times.Once());
        }

        [Test]
        public void Evaluate_LeftIsFalse_DontEvaluateRightPredicate()
        {
            var left = new Mock<IPredicate>();
            left.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(false);
            var right = new Mock<IPredicate>();
            right.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(true);

            var @operator = new AndOperator();
            @operator.Evaluate(left.Object, right.Object, "my value");

            left.Verify(x => x.Evaluate("my value"), Times.Once());
            right.Verify(x => x.Evaluate(It.IsAny<object>()), Times.Never());
        }
    }
}
