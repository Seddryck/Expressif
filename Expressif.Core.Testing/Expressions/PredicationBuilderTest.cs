using Expressif.Hosting;
using Expressif.Library.Text;

namespace Expressif.Testing.Expressions;

public class PredicationBuilderTest
{
    private static PredicationBuilder CreateBuilder()
        => ExpressifEnvironment.Default.CreatePredicationBuilder();

    [Test]
    public void Create_BuildsSingleRule()
    {
        var predicate = CreateBuilder().Create<StartsWith>("Nik").Build();
        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void AndOrXor_CombineRules()
    {
        var predicate = CreateBuilder()
            .Create<StartsWith>("ola")
            .Or<EndsWith>("sla")
            .And<SortedAfter>("Alan Turing")
            .Xor<SortedBefore>("Marie Curie")
            .Build();

        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Not_NegatesWholeRule()
    {
        var predicate = CreateBuilder()
            .Create<StartsWith>("Nik")
            .And<EndsWith>("sla")
            .Not()
            .Build();

        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.False);
    }

    [Test]
    public void Combination_TypeOverloadsBuildRule()
    {
        var predicate = CreateBuilder()
            .Create(typeof(StartsWith), "Nik")
            .And(typeof(EndsWith), "sla")
            .Build();

        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void Combination_RuleOverloadsPreserveGrouping()
    {
        var builder = CreateBuilder();
        var name = builder.Create<StartsWith>("Nik").And<EndsWith>("sla");
        var casing = builder.Create<LowerCase>().Or<UpperCase>();
        var rule = name.Or(casing);

        Assert.Multiple(() =>
        {
            Assert.That(rule.Build().Evaluate("Nikola Tesla"), Is.True);
            Assert.That(rule.ToSource(), Is.EqualTo("{{starts-with(Nik) |AND ends-with(sla)} |OR {lower-case |OR upper-case}}"));
        });
    }

    [Test]
    public void Combination_DoesNotMutateEarlierRule()
    {
        var startsWith = CreateBuilder().Create<StartsWith>("Nik");
        var fullName = startsWith.And<EndsWith>("sla");

        Assert.Multiple(() =>
        {
            Assert.That(startsWith.ToSource(), Is.EqualTo("starts-with(Nik)"));
            Assert.That(fullName.ToSource(), Is.EqualTo("{starts-with(Nik) |AND ends-with(sla)}"));
        });
    }

    [Test]
    public void Build_CanBeRepeated()
    {
        var rule = CreateBuilder().Create<StartsWith>("Nik");
        var first = rule.Build();
        var second = rule.Build();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.Evaluate("Nikola"), Is.True);
            Assert.That(second.Evaluate("Ada"), Is.False);
        });
    }

    [Test]
    public void ArgumentProvider_UsesEvaluationContext()
    {
        var rule = CreateBuilder().Create<StartsWith>(
            Argument.From<string>(scope => scope.GetVariable<string>("prefix")));
        var context = EvaluationContext.CreateBuilder().AddValue("prefix", "Nik").Build();

        Assert.That(rule.Build().WithContext(context).Evaluate("Nikola Tesla"), Is.True);
    }

    [Test]
    public void ToSource_SerializesNegation()
    {
        var source = CreateBuilder().Create<StartsWith>("Nik").Not().ToSource();
        Assert.That(source, Is.EqualTo("!{starts-with(Nik)}"));
    }
}
