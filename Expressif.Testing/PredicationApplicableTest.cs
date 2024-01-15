using Expressif;
using Expressif.Values;
using Expressif.Values.Special;
using System.Data;
using System.Diagnostics;

namespace Expressif.Testing;

public class PredicationApplicableTest
{

    [Test]
    public void Evaluate_SinglePredicateWithoutParameter_Valid()
    {
        var predication = new PredicationApplicable("`Nikola Tesla` |? lower-case");
        var result = predication.Evaluate();
        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_SinglePredicateWithOneParameter_Valid()
    {
        var predication = new PredicationApplicable("`Nikola Tesla` |? starts-with(Nik)");
        var result = predication.Evaluate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_CombinationAnd_Valid()
    {
        var predication = new PredicationApplicable("`Nikola Tesla` |? starts-with(Nik) |AND ends-with(sla)");
        var result = predication.Evaluate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_UsingContext_Valid()
    {
        var context = new Context();
        var predication = new PredicationApplicable("[Name] |? starts-with([Start]) |AND ends-with(@End)", context);

        context.Variables.Add<string>("End", "sla");
        context.CurrentObject.Set(new { Name = "Nikola Tesla", Start = "Nik" });
        Assert.That(predication.Evaluate(), Is.True);

        context.CurrentObject.Set(new { Name = "Albert Einstein", Start = "Alb" });
        Assert.That(predication.Evaluate(), Is.False);
    }
}
