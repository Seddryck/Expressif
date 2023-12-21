using Expressif.Functions;
using Expressif.Predicates.Boolean;
using Expressif.Predicates.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Boolean;

public class NegateTest
{
    [Test]
    public void Evaluate_Predicate_Valid()
        => Assert.Multiple(() =>
        {
            Assert.That(new Negate(new LowerCase()).Evaluate("foo"), Is.False);
            Assert.That(new Negate(new UpperCase()).Evaluate("foo"), Is.True);
        });

    [Test]
    public void Evaluate_Function_Valid()
        => Assert.Multiple(() =>
        {
            Assert.That((new Negate(new LowerCase()) as IFunction).Evaluate("foo"), Is.False);
            Assert.That((new Negate(new UpperCase()) as IFunction).Evaluate("foo"), Is.True);
        });
}
