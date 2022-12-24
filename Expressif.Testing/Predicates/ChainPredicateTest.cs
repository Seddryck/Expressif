using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates
{
    public class ChainPredicateTest
    {
        [Test]
        public void Evaluate_Predicate_Valid()
            => Assert.That(new ChainPredicate(new IPredicate[] { new LowerCase(), new UpperCase() }).Evaluate("foo"), Is.False);

        [Test]
        public void Evaluate_Function_Valid()
        {
            var chain = new ChainPredicate(new IPredicate[] { new LowerCase(), new UpperCase() });
            Assert.That((chain as IFunction).Evaluate("foo"), Is.False);
        }
    }
}
