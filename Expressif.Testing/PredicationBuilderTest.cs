using Expressif.Parsers;
using Expressif.Predicates.Text;
using Expressif.Serializers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing;

public class PredicationBuilderTest
{
    [Test]
    public void Build_NoPredicate_ThrowException()
        => Assert.Throws<InvalidOperationException>(() => new PredicationBuilder().Build());

    [Test]
    public void Chain_WithoutParameter_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder().Create<LowerCase>();
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.False);
    }

    [Test]
    public void Chain_WithParameter_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder().Create<StartsWith>("Nik");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Not_WithParameters_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder().Not<StartsWith>("Nik");
        var expression = builder.Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.False);
    }

    [Test]
    public void Chain_CombinationAnd_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder()
            .Create<StartsWith>("Nik")
            .And<EndsWith>("sla");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Chain_CombinationOr_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder()
            .Create<StartsWith>("ola")
            .Or<EndsWith>("sla");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void AndOrXor_Generic_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder()
            .Create<StartsWith>("ola")
            .Or<EndsWith>("sla")
            .And<SortedAfter>("Alan Turing")
            .Xor<SortedBefore>("Marie Curie");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Chain_NegateGenericFluent_CorrectlyEvaluate()
    {
        var builder = new PredicationBuilder()
            .Create<StartsWith>("ola")
            .OrNot<EndsWith>("Tes");
        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Chain_SubPredication_CorrectlyEvaluate()
    {
        var subPredicate = new PredicationBuilder()
            .Create<StartsWith>("Nik")
            .And<EndsWith>("sla");

        var builder = new PredicationBuilder()
            .Create<LowerCase>()
            .Or(subPredicate)
            .Or<UpperCase>();

        var predicate = builder.Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Serialize_SubPredication_CorrectlySerialized()
    {
        var subPredicate = new PredicationBuilder()
            .Create<StartsWith>("Nik")
            .And<EndsWith>("sla");

        var builder = new PredicationBuilder()
            .Create<LowerCase>()
            .Or(subPredicate)
            .Or<UpperCase>();

        var str = builder.Serialize();
        Assert.That(str, Is.EqualTo("{{lower-case |OR {starts-with(Nik) |AND ends-with(sla)}} |OR upper-case}"));
    }

    [Test]
    public void Serialize_SerializerCalledOnce()
    {
        var serializer = new Mock<PredicationSerializer>();
        serializer.Setup(x => x.Serialize(It.IsAny<IPredication>())).Returns("serialization");
        var builder = new PredicationBuilder(serializer: serializer.Object)
            .Create<StartsWith>("ola")
            .Or<EndsWith>("sla");
        var str = builder.Serialize();
        serializer.Verify(x => x.Serialize(It.IsAny<IPredication>()), Times.Once);
    }

    [Test]
    public void Serialize_Not_CorrectlySerialized()
    {
        var builder = new PredicationBuilder().Not<StartsWith>("Nik");
        var str = builder.Serialize();
        Assert.That(str, Is.EqualTo("!{starts-with(Nik)}"));
    }

    [Test]
    public void Serialize_Negate_CorrectlySerialized()
    {
        var builder = new PredicationBuilder()
            .Create<StartsWith>("ola")
            .OrNot<EndsWith>("sla");
        var str = builder.Serialize();
        Assert.That(str, Is.EqualTo("{starts-with(ola) |OR !{ends-with(sla)}}"));
    }

    [Test]
    public void Serialize_NoPredicate_ThrowException()
        => Assert.Throws<InvalidOperationException>(() => new PredicationBuilder().Serialize());
}
