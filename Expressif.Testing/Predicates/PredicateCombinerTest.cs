using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Text;
using Expressif.Predicates;

namespace Expressif.Testing.Predicates;
public class PredicateCombinerTest
{
    [Test]
    [TestCase("Nikola Tesla", true)]
    [TestCase("Albert Einstein", false)]
    public void Build_Combination_Success(string value, bool expected)
    {
        var combiner = new PredicateCombiner();
        var combination = combiner.With(new StartsWith(() => "Alb"))
                            .Or(new EndsWith(() => "sla"))
                            .And(new StartsWith(() => "Nik"));
        var combined = combination.Build();
        Assert.That(combined.Evaluate(value), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("Nikola Tesla", true)]
    [TestCase("Albert Einstein", false)]
    public void Build_CombinationWithNot_Success(string value, bool expected)
    {
        var combiner = new PredicateCombiner();
        var combination = combiner.WithNot(new StartsWith(() => "Alb"))
                            .Or(new EndsWith(() => "ring"))
                            .And(new StartsWith(() => "Nik"));
        var combined = combination.Build();
        Assert.That(combined.Evaluate(value), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("Nikola Tesla", true)]
    [TestCase("Albert Einstein", false)]
    public void Build_CombinationUsingNot_Success(string value, bool expected)
    {
        var combiner = new PredicateCombiner();
        var combination = combiner.With(new StartsWith(() => "Nik"))
                            .Or(new EndsWith(() => "stein"))
                            .AndNot(new StartsWith(() => "Alb"));
        var combined = combination.Build();
        Assert.That(combined.Evaluate(value), Is.EqualTo(expected));
    }
}
