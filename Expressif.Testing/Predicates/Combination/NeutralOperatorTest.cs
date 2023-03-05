using Expressif.Predicates;
using Expressif.Predicates.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Combination
{
    public class NeutralOperatorTest
    {
        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void Evaluate_Value_Success(bool value, bool expected)
        {
            var factory = new Mock<PredicationFactory>();
            var predication = new Mock<Predication>(new object[] { "any-code", new Context(), factory.Object });
            predication.Setup(x => x.Evaluate(It.IsAny<object>())).Returns(value);
            factory.Setup(x => x.Instantiate(It.IsAny<string>(), It.IsAny<Context>())).Returns(predication.Object);
            var @operator = new NeutralOperator(predication.Object);

            Assert.That(@operator.Evaluate(true, "my value"), Is.EqualTo(expected));
            predication.VerifyAll();
        }
    }
}
