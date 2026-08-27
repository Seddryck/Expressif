using System.Data;
using System.Diagnostics;
using Expressif;
using Expressif.Values;
using Expressif.Values.Special;

namespace Expressif.Testing;

public class PredicationTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    public void Create_ReturnsStronglyTypedPredication()
    {
        Predication predication = Predication.Create("lower-case");

        bool result = predication.Evaluate("Nikola Tesla");

        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_SinglePredicateWithoutParameter_Valid()
    {
        var predication = Predication.Create("lower-case");
        var result = predication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_SinglePredicateWithOneParameter_Valid()
    {
        var predication = Predication.Create("starts-with(\"Nik\")");
        var result = predication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_IntervalAsParameter_Valid()
    {
        var predication = Predication.Create("within-interval(I[0, 20[)");
        var result = predication.Evaluate(15);
        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_CultureAsParameter_Valid()
    {
        var predication = Predication.Create("matches-date(\"fr-fr\")");
        var result = predication.Evaluate("28/12/1978");
        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_Negation_Valid()
    {
        var predication = Predication.Create("!starts-with(\"Nik\")");
        var result = predication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_Negation_CheckParam()
    {
        var predication = Predication.Create("!starts-with(\"True\")");
        var result = predication.Evaluate("Truesla");
        Assert.That(result, Is.False);
    }

    [Test]
    public void Evaluate_CombinationAnd_Valid()
    {
        var predication = Predication.Create("starts-with(\"Nik\") |AND ends-with(\"sla\")");
        var result = predication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.True);
    }

    [Test]
    public void Evaluate_CombinationOr_Valid()
    {
        var predication = Predication.Create("starts-with(\"ola\") |OR ends-with(\"sla\")");
        var result = predication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.True);
    }

    [TestCase("!greater-than(2)", 3, false)]
    [TestCase("greater-than(2) |AND less-than(5)", 3, true)]
    [TestCase("greater-than(2) |OR less-than(0)", 3, true)]
    [TestCase("greater-than(2) |XOR less-than(5)", 3, false)]
    public void Evaluate_BooleanShorthand_Valid(string code, object value, bool expected)
        => Assert.That(Predication.Create(code).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_CombinationsGroup_Valid()
    {
        var predication = Predication.Create("(starts-with(\"Nik\") |AND ends-with(\"sla\")) |OR (starts-with(\"ola\") |AND ends-with(\"Tes\"))");
        var result = predication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.True);

        var withoutGroupsPredication = Predication.Create("starts-with(\"Nik\") |AND ends-with(\"sla\") |OR starts-with(\"ola\") |AND ends-with(\"Tes\")");
        var secondResult = withoutGroupsPredication.Evaluate("Nikola Tesla");
        Assert.That(result, Is.Not.EqualTo(secondResult));
    }
}
