using Expressif.Functions.Text;
using Expressif.Parsers;
using Expressif.Predicates;
using Expressif.Predicates.Text;
using Expressif.Serializers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing;

public class PredicationApplicableBuilderTest
{
    [Test]
    public void Build_NoPredicate_ThrowException()
        => Assert.Throws<InvalidOperationException>(() => new PredicationApplicableBuilder().Build());

    [Test]
    [TestCase("Nikola Tesla", false)]
    [TestCase("nikola tesla", true)]
    public void Build_SinglePredicate_CorrectlyEvaluate(string argument, bool expected)
    {
        var builder = new PredicationApplicableBuilder()
                            .WithArgument(argument)
                            .Is<LowerCase>();

        var predicate = builder.Build();
        Assert.That(predicate.Evaluate(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("Nikola Tesla", false)]
    [TestCase("nikola tesla", true)]
    public void Build_ManyPredicate_CorrectlyEvaluate(string argument, bool expected)
    {
        var builder = new PredicationApplicableBuilder()
                            .WithArgument(argument)
                            .Is<LowerCase>()
                            .And<EndsWith>("sla");

        var predicate = builder.Build();
        Assert.That(predicate.Evaluate(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("rie", false)]
    [TestCase("sla", true)]
    public void Build_AndOrXorGeneric_CorrectlyEvaluate(string endsWith, bool expected)
    {
        var builder = new PredicationApplicableBuilder()
            .WithArgument("Nikola Tesla")
            .Is<StartsWith>(endsWith)
            .Or<EndsWith>("sla")
            .And<SortedAfter>("Alan Turing")
            .Xor<SortedBefore>("Marie Curie");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate(), Is.True);
    }

    [Test]
    public void Build_NegateGenericFluent_CorrectlyEvaluate()
    {
        var builder = new PredicationApplicableBuilder()
            .WithArgument("Nikola Tesla")
            .Is<StartsWith>("ola")
            .OrNot<EndsWith>("Tes");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate(), Is.True);
    }

    [Test]
    public void Build_SubPredication_CorrectlyEvaluate()
    {
        var subPredicate = new PredicationBuilder()
            .Is<StartsWith>("Nik")
            .And<EndsWith>("sla");

        var builder = new PredicationApplicableBuilder()
            .WithArgument("Nikola Tesla")
            .Is<LowerCase>()
            .Or(subPredicate)
            .Or<UpperCase>();

        var predicate = builder.Build();
        Assert.That(predicate.Evaluate(), Is.True);
    }

    [Test]
    public void Serialize_SubPredication_CorrectlySerialized()
    {
        var subPredicate = new PredicationBuilder()
           .Is<StartsWith>("Nik")
           .And<EndsWith>("sla");

        var builder = new PredicationApplicableBuilder()
            .WithArgument("Nikola Tesla")
            .Is<LowerCase>()
            .Or(subPredicate)
            .Or<UpperCase>();

        var str = builder.Serialize();
        Assert.That(str, Is.EqualTo("`Nikola Tesla` |? {{lower-case |OR {starts-with(Nik) |AND ends-with(sla)}} |OR upper-case}"));
    }

    [Test]
    public void Serialize_NoPredicate_ThrowException()
        => Assert.Throws<InvalidOperationException>(() => new PredicationApplicableBuilder().Serialize());
}
